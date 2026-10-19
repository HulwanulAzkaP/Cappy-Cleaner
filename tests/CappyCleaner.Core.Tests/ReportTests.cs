using CappyCleaner.Core;
namespace CappyCleaner.Core.Tests;
public class ReportTests
{
    [Fact]
    public async Task ExportsFullMetadataWithoutFileContents()
    {
        var sandbox = Path.Combine(Path.GetTempPath(), "CappyReport-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(sandbox, "source"));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(sandbox, "source", "hello.txt"), "PRIVATE CONTENT DO NOT EXPORT");
            var db = Path.Combine(sandbox, "scan.db"); var report = Path.Combine(sandbox, "report.md");
            var summary = await new DiskScanner().ScanAsync(Path.Combine(sandbox, "source"), db, [], null, default);
            await SnapshotReport.ExportAsync(db, report);
            var text = await File.ReadAllTextAsync(report);
            Assert.Contains("hello.txt", text); Assert.Contains(summary.Id, text);
            Assert.DoesNotContain("PRIVATE CONTENT", text);
        }
        finally { Directory.Delete(sandbox, true); }
    }
}
