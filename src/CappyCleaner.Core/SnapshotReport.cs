namespace CappyCleaner.Core;
public static class SnapshotReport
{
    public static Task ExportAsync(string database, string destination) => Task.Run(async () =>
    {
        if (string.Equals(Path.GetFullPath(database), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) throw new IOException("Laporan tidak boleh menimpa database snapshot.");
        using var store = new SnapshotStore(database);
        var summary = store.GetSummary() ?? throw new IOException("Snapshot tidak memiliki ringkasan.");
        using var writer = new StreamWriter(destination, false, new System.Text.UTF8Encoding(false));
        await writer.WriteLineAsync($"# Cappy Cleaner — laporan metadata\n\nSnapshot: {summary.Id}\nWaktu: {summary.Started:O}\nStatus: {summary.State}\nRoot: {Escape(summary.Root)}\nUkuran logis: {ByteSize.Format(summary.LogicalBytes)}\nFile: {summary.Files}\nError: {summary.Errors}\nDilewati: {summary.Skipped}\n\nUkuran induk mencakup anak. Ukuran logis bukan janji ruang fisik pulih. Tidak ada isi file yang dibaca.\n\n| Path | Jenis | Byte logis | Status | Keterangan |\n|---|---|---:|---|---|");
        foreach (var entry in store.EnumerateEntries())
            await writer.WriteLineAsync($"| {Escape(entry.Path)} | {entry.Kind} | {(entry.LogicalBytes?.ToString() ?? "unknown")} | {entry.Status} | {Escape(entry.Detail ?? "")} |");
    });
    private static string Escape(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("|", "&#124;").Replace("\r", " ").Replace("\n", " ");
}
