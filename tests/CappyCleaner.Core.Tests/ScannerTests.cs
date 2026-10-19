using CappyCleaner.Core;

namespace CappyCleaner.Core.Tests;

public sealed class ScannerTests : IDisposable
{
    private readonly string sandbox = Path.Combine(Path.GetTempPath(), "CappyFixture-" + Guid.NewGuid());
    private string Root => Path.Combine(sandbox, "source");
    private string Database => Path.Combine(sandbox, "snapshot.db");
    public ScannerTests() => Directory.CreateDirectory(Root);
    public void Dispose() { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(sandbox, true); }

    [Fact]
    public async Task RejectsOutputThroughJunctionBeforeWritingSource()
    {
        var link = Path.Combine(sandbox, "database-alias");
        var start = new System.Diagnostics.ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true };
        foreach (var arg in new[] { "/c", "mklink", "/J", link, Root }) start.ArgumentList.Add(arg);
        using var p = System.Diagnostics.Process.Start(start)!; await p.WaitForExitAsync(); Assert.Equal(0, p.ExitCode);
        try
        {
            await Assert.ThrowsAsync<ArgumentException>(() => new DiskScanner().ScanAsync(Root, Path.Combine(link, "output.db"), [], null, default));
            Assert.Empty(Directory.EnumerateFileSystemEntries(Root));
        }
        finally { Directory.Delete(link); }
    }

    [Theory]
    [InlineData(@"\\localhost\C$\CappyFixture")]
    [InlineData(@"relative\folder")]
    [InlineData(@"C:\wildcard\*")]
    public async Task RejectsNonLiteralOrNonLocalRootsBeforeAccess(string root)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new DiskScanner().ScanAsync(root, Database, [], null, default));
        Assert.False(File.Exists(Database));
    }

    [Fact]
    public async Task RejectsDatabaseInsideSourceBeforeCreatingIt()
    {
        var db = Path.Combine(Root, "output.db");
        await Assert.ThrowsAsync<ArgumentException>(() => new DiskScanner().ScanAsync(Root, db, [], null, default));
        Assert.False(File.Exists(db));
        Assert.Empty(Directory.EnumerateFileSystemEntries(Root));
    }

    [Fact]
    public async Task PageQueryCanBeCancelledBeforeOpeningDatabase()
    {
        using var cts = new CancellationTokenSource(); cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SnapshotQueries.ReadAsync(Database, null, null, "", 200, 0, cts.Token));
        Assert.False(File.Exists(Database));
    }

    [Fact]
    public async Task AggregatesChildrenOnceAndPreservesSource()
    {
        Directory.CreateDirectory(Path.Combine(Root, "child"));
        await File.WriteAllBytesAsync(Path.Combine(Root, "a.bin"), new byte[17]);
        await File.WriteAllBytesAsync(Path.Combine(Root, "child", "b.bin"), new byte[23]);
        var result = await new DiskScanner().ScanAsync(Root, Database, [], null, default);
        Assert.Equal(40, result.LogicalBytes);
        Assert.Equal(2, result.Files);
        Assert.Equal("completed", result.State);
        using var store = new SnapshotStore(Database);
        Assert.Equal(23, store.GetEntries(null, "folder").Single().LogicalBytes);
        Assert.Equal(17, new FileInfo(Path.Combine(Root, "a.bin")).Length);
    }

    [Fact]
    public async Task ZeroByteFileIsNotEmptyDirectory()
    {
        Directory.CreateDirectory(Path.Combine(Root, "zero"));
        Directory.CreateDirectory(Path.Combine(Root, "empty"));
        await File.WriteAllTextAsync(Path.Combine(Root, "zero", "zero.txt"), "");
        await new DiskScanner().ScanAsync(Root, Database, [], null, default);
        using var store = new SnapshotStore(Database);
        var folders = store.GetEntries(null, "folder");
        Assert.False(folders.Single(x => x.Name == "zero").IsEmpty);
        Assert.True(folders.Single(x => x.Name == "empty").IsEmpty);
    }

    [Fact]
    public async Task ExcludedSubtreeMakesCoveragePartial()
    {
        var excluded = Directory.CreateDirectory(Path.Combine(Root, "private")).FullName;
        await File.WriteAllBytesAsync(Path.Combine(excluded, "secret.bin"), new byte[200]);
        var result = await new DiskScanner().ScanAsync(Root, Database, [excluded], null, default);
        Assert.Equal(0, result.Files);
        Assert.Equal("partial", result.State);
        using var store = new SnapshotStore(Database);
        Assert.Contains(store.GetEntries(), x => x.Status == "excluded");
        Assert.DoesNotContain(store.GetEntries(), x => x.Name == "secret.bin");
    }

    [Fact]
    public async Task CancelledScanPersistsPartialState()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var result = await new DiskScanner().ScanAsync(Root, Database, [], null, cts.Token);
        Assert.Equal("cancelled", result.State);
        using var store = new SnapshotStore(Database);
        Assert.Equal("cancelled", store.GetSummary()!.State);
    }

    [Fact]
    public async Task MissingRootIsUnknownRatherThanEmpty()
    {
        var result = await new DiskScanner().ScanAsync(Path.Combine(Root, "missing"), Database, [], null, default);
        Assert.Equal("partial", result.State);
        Assert.True(result.Errors > 0);
        using var store = new SnapshotStore(Database);
        Assert.Contains(store.GetIssues(), x => x.Status == "access_error");
    }

    [Fact]
    public async Task DoesNotFollowDirectoryLink()
    {
        var outside = Directory.CreateDirectory(Path.Combine(sandbox, "outside")).FullName;
        await File.WriteAllBytesAsync(Path.Combine(outside, "outside.bin"), new byte[300]);
        // Junction creation works without requiring developer mode or symlink privileges.
        var link = Path.Combine(Root, "link");
        var start = new System.Diagnostics.ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add("/c"); start.ArgumentList.Add("mklink"); start.ArgumentList.Add("/J"); start.ArgumentList.Add(link); start.ArgumentList.Add(outside);
        using var p = System.Diagnostics.Process.Start(start)!;
        await p.WaitForExitAsync(); Assert.Equal(0, p.ExitCode);
        var result = await new DiskScanner().ScanAsync(Root, Database, [], null, default);
        Assert.Equal(0, result.Files);
        Assert.Equal(1, result.Skipped);
        using var store = new SnapshotStore(Database);
        Assert.Contains(store.GetIssues(), x => x.Status == "reparse_skipped");
        Directory.Delete(link);
    }

    [Fact]
    public void ProtectionChecksPathBoundariesAndMetadata()
    {
        var policy = new SafetyPolicy([Root]);
        Assert.False(policy.Evaluate(Path.Combine(Root, "code.cs")).CanDelete);
        Assert.False(policy.Evaluate(@"C:\anything\.git\config").CanDelete);
        Assert.False(policy.Evaluate(@"C:\Windows\system32\kernel32.dll").CanDelete);
        Assert.False(policy.Evaluate(@"C:\unknown\cache.bin").CanDelete);
        Assert.True(PathRules.IsWithin(Path.Combine(Root, "file"), Root));
        Assert.False(PathRules.IsWithin(Root + "-other", Root));
    }
}
