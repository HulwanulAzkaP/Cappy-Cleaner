using System.IO;
using System.Text.Json;
using System.Runtime.InteropServices;
using CappyCleaner.Core;

namespace CappyCleaner.App;

public sealed class AppSettings
{
    public string[] ProtectedRoots { get; set; } = [Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Project")];
    public string[] Exclusions { get; set; } = [];
    public static AppSettings Load(string directory)
    {
        var file = Path.Combine(directory, "settings.json");
        var settings = File.Exists(file) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(file)) ?? new() : new();
        settings.ProtectedRoots ??= []; settings.Exclusions ??= [];
        return settings;
    }
    public void Save(string directory)
    {
        var file = Path.Combine(directory, "settings.json");
        File.WriteAllText(file + ".new", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(file + ".new", file, true);
    }
}

internal static class WindowsMemory
{
    [StructLayout(LayoutKind.Sequential)] private struct MemoryStatus
    {
        public uint Length, Load;
        public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
    }
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
    public static string Describe()
    {
        var status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return GlobalMemoryStatusEx(ref status) ? $"{ByteSize.Format((long)status.AvailablePhysical)} tersedia\ndari {ByteSize.Format((long)status.TotalPhysical)}\n{DateTime.Now:HH:mm:ss}" : "Tidak tersedia";
    }
}
