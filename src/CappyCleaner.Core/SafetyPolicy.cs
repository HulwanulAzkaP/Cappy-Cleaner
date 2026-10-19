namespace CappyCleaner.Core;

public sealed record PolicyDecision(bool CanDelete, string Reason);
public sealed class SafetyPolicy(IReadOnlyList<string> protectedRoots)
{
    public PolicyDecision Evaluate(string path)
    {
        if (!Path.IsPathFullyQualified(path) || path.IndexOfAny(['*', '?']) >= 0 || path.Split('\\', '/').Contains("..")) return new(false, "Path harus absolut dan literal.");
        path = Path.GetFullPath(path);
        if (protectedRoots.Any(root => PathRules.IsWithin(path, root))) return new(false, "Lokasi terlindungi oleh pengguna.");
        var segments = path.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        string[] protectedNames = ["Windows", "Program Files", "Program Files (x86)", "ProgramData", "AppData", ".git", ".ssh", "System Volume Information", "Recovery", "Boot"];
        if (segments.Any(s => protectedNames.Contains(s, StringComparer.OrdinalIgnoreCase))) return new(false, "Lokasi sistem, aplikasi, metadata atau rahasia terlindungi.");
        return new(false, "Review manual. Provider eksekusi untuk item ini belum diaktifkan.");
    }
}
public static class PathRules
{
    public static bool IsWithin(string path, string root)
    {
        var p = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var r = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        return string.Equals(p, r, StringComparison.OrdinalIgnoreCase) || p.StartsWith(Path.EndsInDirectorySeparator(r) ? r : r + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
    public static bool HasLinkAncestor(string path)
    {
        var current = new DirectoryInfo(path);
        while (current != null)
        {
            if ((current.Attributes & FileAttributes.ReparsePoint) != 0) return true;
            current = current.Parent;
        }
        return false;
    }
}
