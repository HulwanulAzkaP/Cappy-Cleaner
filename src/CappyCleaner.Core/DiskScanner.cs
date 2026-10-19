namespace CappyCleaner.Core;

public sealed class DiskScanner
{
    public Task<ScanSummary> ScanAsync(string root, string database, IReadOnlyList<string> exclusions, IProgress<ScanProgress>? progress, CancellationToken cancellationToken)
        => Task.Run(() => Scan(root, database, exclusions, progress, cancellationToken));

    private static ScanSummary Scan(string root, string database, IReadOnlyList<string> exclusions, IProgress<ScanProgress>? progress, CancellationToken token)
    {
        if (!Path.IsPathFullyQualified(root) || root.StartsWith(@"\\") || root.IndexOfAny(['*', '?']) >= 0)
            throw new ArgumentException("Root harus path absolut literal pada drive lokal.", nameof(root));
        root = Path.GetFullPath(root);
        if (PathRules.IsWithin(database, root)) throw new ArgumentException("Database snapshot harus berada di luar lokasi yang dipindai.", nameof(database));
        var outputParent = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(database))!);
        while (outputParent != null)
        {
            if (outputParent.Exists && (outputParent.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new ArgumentException("Direktori database tidak boleh melalui link/junction.", nameof(database));
            outputParent = outputParent.Parent;
        }
        if (File.Exists(database)) throw new IOException("Snapshot baru tidak boleh menimpa database yang sudah ada.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(database))!);
        using var store = new SnapshotStore(database); store.Initialize();
        var id = Guid.NewGuid().ToString("N"); var started = DateTimeOffset.Now;
        long files = 0, folders = 0, errors = 0, skipped = 0, bytes = 0, nextId = 1;
        var frames = new Stack<Frame>();
        var rootFrame = new Frame(1, null, root);
        var clock = System.Diagnostics.Stopwatch.StartNew(); var lastUpdate = TimeSpan.Zero;
        ScanSummary Summary(string state) => new(id, root, started, state, bytes, files, folders, errors, skipped);
        store.SaveSummary(Summary("running"));

        bool Open(Frame frame)
        {
            try
            {
                if (exclusions.Any(x => PathRules.IsWithin(frame.Path, x)))
                { frame.Status = "excluded"; frame.Detail = "Dikecualikan dari cakupan."; skipped++; return false; }
                // Check every ancestor of the selected root as well as discovered entries.
                var attributes = File.GetAttributes(frame.Path);
                if (PathRules.HasLinkAncestor(frame.Path) || (attributes & FileAttributes.ReparsePoint) != 0)
                { frame.Status = "reparse_skipped"; frame.Detail = "Target link tidak ditelusuri."; skipped++; return false; }
                if (IsCloud(attributes))
                { frame.Status = "cloud_skipped"; frame.Detail = "Placeholder tidak dibaca atau diunduh."; skipped++; return false; }
                if ((attributes & FileAttributes.Directory) == 0) throw new IOException("Pilih folder atau root drive.");
                frame.Enumerator = new DirectoryInfo(frame.Path).EnumerateFileSystemInfos("*", new EnumerationOptions { AttributesToSkip = 0, IgnoreInaccessible = false, RecurseSubdirectories = false, ReturnSpecialDirectories = false }).GetEnumerator();
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            { frame.Status = "access_error"; frame.Detail = ex.Message; errors++; return false; }
        }

        void Save(Frame frame)
        {
            var known = frame.Status is "complete" or "partial";
            store.Write(new(frame.Id, frame.Parent, frame.Path, "folder", known ? frame.Bytes : null, frame.Files, frame.Status, frame.Detail, frame.Status == "complete" && frame.Files == 0));
            frame.Enumerator?.Dispose();
            if (frames.TryPeek(out var parent))
            {
                parent.Bytes += frame.Bytes; parent.Files += frame.Files;
                if (frame.Status != "complete" && parent.Status == "complete") parent.Status = "partial";
            }
        }

        if (Open(rootFrame)) frames.Push(rootFrame); else Save(rootFrame);
        while (frames.Count > 0 && !token.IsCancellationRequested)
        {
            var current = frames.Peek();
            if (clock.Elapsed - lastUpdate >= TimeSpan.FromMilliseconds(400))
            {
                progress?.Report(new(current.Path, files, folders, errors, skipped, bytes));
                store.SaveSummary(Summary("running")); lastUpdate = clock.Elapsed;
            }
            FileSystemInfo? item;
            try
            {
                if (!current.Enumerator!.MoveNext()) { frames.Pop(); Save(current); continue; }
                item = current.Enumerator.Current;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            { current.Status = "access_error"; current.Detail = ex.Message; errors++; frames.Pop(); Save(current); continue; }

            var itemId = ++nextId;
            try
            {
                var attr = item.Attributes;
                var isDirectory = (attr & FileAttributes.Directory) != 0;
                if (isDirectory) folders++;
                var status = exclusions.Any(x => PathRules.IsWithin(item.FullName, x)) ? "excluded"
                    : (attr & FileAttributes.ReparsePoint) != 0 ? "reparse_skipped" : IsCloud(attr) ? "cloud_skipped" : "complete";
                if (status != "complete")
                {
                    skipped++; current.Status = "partial";
                    store.Write(new(itemId, current.Id, item.FullName, isDirectory ? "folder" : "file", null, 0, status, "Tidak ditelusuri; ukuran tidak diketahui.", false)); continue;
                }
                if (isDirectory)
                {
                    var child = new Frame(itemId, current.Id, item.FullName);
                    if (Open(child)) frames.Push(child); else Save(child);
                }
                else
                {
                    var length = ((FileInfo)item).Length;
                    files++; bytes += length; current.Bytes += length; current.Files++;
                    store.Write(new(itemId, current.Id, item.FullName, "file", length, 1, "complete", "Ukuran logis; bukan janji ruang fisik yang akan pulih.", false));
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            {
                errors++; current.Status = "partial";
                store.Write(new(itemId, current.Id, item.FullName, "file", null, 0, "access_error", ex.Message, false));
            }
        }
        while (frames.Count > 0) { var frame = frames.Pop(); frame.Status = "cancelled"; Save(frame); }
        var result = Summary(token.IsCancellationRequested ? "cancelled" : errors > 0 || skipped > 0 ? "partial" : "completed");
        store.SaveSummary(result); progress?.Report(new(root, files, folders, errors, skipped, bytes)); return result;
    }

    private static bool IsCloud(FileAttributes a) => ((int)a & (0x1000 | 0x40000 | 0x400000)) != 0;
    private sealed class Frame(long id, long? parent, string path)
    {
        public long Id = id; public long? Parent = parent; public string Path = path;
        public long Bytes; public long Files; public string Status = "complete"; public string? Detail;
        public IEnumerator<FileSystemInfo>? Enumerator;
    }
}
