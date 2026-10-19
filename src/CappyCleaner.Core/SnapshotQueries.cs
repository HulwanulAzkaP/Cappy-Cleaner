namespace CappyCleaner.Core;
public static class SnapshotQueries
{
    public static Task<IReadOnlyList<ScanEntry>> ReadAsync(string database, long? parent, string? kind, string search, int limit, int offset, CancellationToken cancellationToken)
        => Task.Run<IReadOnlyList<ScanEntry>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var store = new SnapshotStore(database);
            var rows = kind == "issues" ? store.GetIssues() : store.GetEntries(parent, kind, search, limit, offset);
            cancellationToken.ThrowIfCancellationRequested();
            return rows;
        }, cancellationToken);
}
