namespace CappyCleaner.Core;

public sealed record ScanSummary(string Id, string Root, DateTimeOffset Started, string State, long LogicalBytes, long Files, long Folders, long Errors, long Skipped);
public sealed record ScanProgress(string Path, long Files, long Folders, long Errors, long Skipped, long LogicalBytes);
public sealed record ScanEntry(long Id, long? ParentId, string Path, string Kind, long? LogicalBytes, long Files, string Status, string? Detail, bool IsEmpty)
{
    public string Name => System.IO.Path.GetFileName(System.IO.Path.TrimEndingDirectorySeparator(Path));
    public string SizeLabel => LogicalBytes is long size ? ByteSize.Format(size) : "Tidak diketahui";
    public string TypeLabel => Kind == "folder" ? "Folder" : "File";
    public string StatusLabel => Status switch { "complete" => "Terbaca", "partial" => "Sebagian", "excluded" => "Dikecualikan", "reparse_skipped" => "Link dilewati", "cloud_skipped" => "Cloud dilewati", "access_error" => "Gagal dibaca", "cancelled" => "Dibatalkan", _ => Status };
}
public static class ByteSize
{
    public static string Format(long value) => value >= 1073741824 ? $"{value / 1073741824d:N2} GiB" : value >= 1048576 ? $"{value / 1048576d:N2} MiB" : value >= 1024 ? $"{value / 1024d:N2} KiB" : $"{value:N0} B";
}
