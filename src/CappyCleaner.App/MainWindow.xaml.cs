using System.ComponentModel;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CappyCleaner.Core;
using Microsoft.Win32;
namespace CappyCleaner.App;

public partial class MainWindow : Window
{
    private readonly string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CappyCleaner");
    private CancellationTokenSource? scanCancellation;
    private string? currentDatabase;
    private ScanSummary? summary;
    private long currentParent = 1;
    private readonly Stack<long> parents = new();
    private int page;
    private bool ready;
    private bool queryRunning, queryAgain;
    private int queryVersion;
    private CancellationTokenSource? queryCancellation;
    private AppSettings settings = new();
    public MainWindow()
    {
        InitializeComponent();
        using var identity = WindowsIdentity.GetCurrent();
        AccountText.Text = identity.Name;
        SidText.Text = $"Identitas proses: {identity.Name}\nSID: {identity.User?.Value}\nJika UAC memakai akun lain, pilih root profil yang dimaksud secara eksplisit.";
        if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
        { MessageBox.Show("Aplikasi memerlukan administrator. Buka CappyCleaner.exe dan setujui UAC.", "Akses administrator"); Loaded += (_, _) => Close(); return; }
        Directory.CreateDirectory(dataDirectory);
        DataPathText.Text = $"Data lokal akun proses:\n{dataDirectory}\nRiwayat berisi nama/path pribadi. Ekspor hanya melalui pilihan Anda.";
        try { settings = AppSettings.Load(dataDirectory); }
        catch (Exception ex) { StatusText.Text = "Pengaturan gagal dibaca; default dipakai: " + ex.Message; }
        ProtectedBox.Text = string.Join(Environment.NewLine, settings.ProtectedRoots);
        ExclusionsBox.Text = string.Join(Environment.NewLine, settings.Exclusions);
        ready = true; RefreshDrives(); LoadHistory();
    }
    private void RefreshDrives()
    {
        var list = new List<DriveDisplay>();
        foreach (var d in DriveInfo.GetDrives())
            try { if (d.DriveType == DriveType.Fixed && d.IsReady) list.Add(new(d.Name, $"{d.Name}  {d.VolumeLabel}", d.TotalSize, d.AvailableFreeSpace)); }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        DrivePicker.ItemsSource = list;
        // Selecting a drive does not select a profile or start scanning.
        if (list.Count > 0) DrivePicker.SelectedIndex = 0;
        RamText.Text = WindowsMemory.Describe();
    }
    private void RefreshDrives_Click(object sender, RoutedEventArgs e) => RefreshDrives();
    private void DrivePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ready || DrivePicker.SelectedItem is not DriveDisplay d) return;
        DiskBar.Value = d.Total == 0 ? 0 : 100d * (d.Total - d.Free) / d.Total;
        DiskText.Text = $"{ByteSize.Format(d.Free)} kosong dari {ByteSize.Format(d.Total)} • {DateTime.Now:HH:mm:ss}";
    }
    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Pilih folder atau drive yang akan dipindai" };
        if (dialog.ShowDialog(this) == true) RootBox.Text = dialog.FolderName;
    }
    private void FullScan_Click(object sender, RoutedEventArgs e)
    {
        if (scanCancellation != null) return;
        RootBox.Text = @"C:\"; MainTabs.SelectedIndex = 0; Scan_Click(sender, e);
    }
    private async void Scan_Click(object sender, RoutedEventArgs e)
    {
        if (scanCancellation != null) return;
        try
        {
            var root = RootBox.Text.Trim();
            if (!Path.IsPathFullyQualified(root) || root.StartsWith(@"\\") || root.IndexOfAny(['*', '?']) >= 0) throw new IOException("Pilih path absolut pada drive lokal, tanpa wildcard atau jaringan.");
            if (new DriveInfo(Path.GetPathRoot(root)!).DriveType != DriveType.Fixed) throw new IOException("Build ini hanya memindai drive lokal tetap.");
            if (PathRules.IsWithin(root, dataDirectory)) throw new IOException("Folder data internal Cappy Cleaner dikecualikan dari scan.");
            currentDatabase = Path.Combine(dataDirectory, $"scan-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.db");
            currentParent = 1; parents.Clear(); page = 0; summary = null;
            scanCancellation = new(); SetBusy(true); ResultsGrid.ItemsSource = null;
            StatusText.Text = "Scan metadata berjalan. Tidak ada perubahan pada sumber.";
            var progress = new Progress<ScanProgress>(p =>
            {
                ProgressText.Text = $"{p.Files:N0} file • {p.Folders:N0} folder • {p.Errors:N0} gagal • {p.Skipped:N0} dilewati";
                ProgressText.ToolTip = p.Path;
                SummaryText.Text = $"Terukur {ByteSize.Format(p.LogicalBytes)} logis • hasil sementara";
            });
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (_, _) => SafeQuery();
            timer.Start();
            try { summary = await new DiskScanner().ScanAsync(root, currentDatabase, settings.Exclusions.Append(dataDirectory).ToArray(), progress, scanCancellation.Token); }
            finally { timer.Stop(); }
            SummaryText.Text = $"{StateLabel(summary.State)} • {ByteSize.Format(summary.LogicalBytes)} logis • {summary.Files:N0} file • {summary.Errors:N0} gagal • {summary.Skipped:N0} dilewati\n{summary.Started:dd MMM yyyy HH:mm:ss} • {summary.Root}";
            StatusText.Text = "Snapshot dicatat. Ukuran bukan janji ruang fisik yang dapat dipulihkan.";
            await QueryResultsAsync(); LoadHistory();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { scanCancellation?.Dispose(); scanCancellation = null; SetBusy(false); }
    }
    private void SetBusy(bool busy)
    {
        FullScanButton.IsEnabled = ScanButton.IsEnabled = BrowseButton.IsEnabled = RootBox.IsEnabled = DrivePicker.IsEnabled = !busy;
        CancelButton.IsEnabled = busy; ExportButton.IsEnabled = !busy && summary != null;
        RefreshScanButton.IsEnabled = !busy && summary != null;
        ScanBar.Visibility = busy ? Visibility.Visible : Visibility.Collapsed; ScanBar.IsIndeterminate = busy;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    { scanCancellation?.Cancel(); ProgressText.Text = "Pembatalan diminta; menunggu batas operasi OS yang aman…"; CancelButton.IsEnabled = false; }
    private async Task QueryResultsAsync()
    {
        if (!ready || currentDatabase == null || !File.Exists(currentDatabase)) return;
        var version = ++queryVersion;
        queryCancellation?.Cancel();
        if (queryRunning) { queryAgain = true; return; }
        queryRunning = true;
        queryCancellation = new CancellationTokenSource();
        var token = queryCancellation.Token;
        var database = currentDatabase;
        var view = ViewPicker.SelectedIndex;
        var parent = currentParent;
        var requestedPage = page;
        var search = SearchBox.Text;
        try
        {
            var rows = await SnapshotQueries.ReadAsync(database, view == 0 ? parent : null,
                view == 3 ? "issues" : view == 1 ? "file" : view == 2 ? "folder" : null,
                search, 200, requestedPage * 200, token);
            if (version != queryVersion || currentDatabase != database) return;
            ResultsGrid.ItemsSource = rows;
            PreviousButton.IsEnabled = view != 3 && requestedPage > 0;
            NextButton.IsEnabled = view != 3 && rows.Count == 200;
            PageText.Text = view == 3 ? "Maksimum 1.000 isu; ekspor memuat rincian lengkap" : $"Halaman {requestedPage + 1} • {rows.Count} item • urut ukuran";
        }
        catch (OperationCanceledException) { }
        catch (Microsoft.Data.Sqlite.SqliteException) when (scanCancellation != null) { }
        finally
        {
            queryCancellation.Dispose(); queryCancellation = null; queryRunning = false;
            if (queryAgain) { queryAgain = false; await QueryResultsAsync(); }
        }
    }
    private async void SafeQuery() { try { await QueryResultsAsync(); } catch (Exception ex) { ShowError(ex); } }
    private void Search_Click(object sender, RoutedEventArgs e) { page = 0; SafeQuery(); }
    private void ViewPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) { page = 0; SafeQuery(); }
    private void SearchBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { page = 0; SafeQuery(); } }
    private void Next_Click(object sender, RoutedEventArgs e) { page++; SafeQuery(); }
    private void Previous_Click(object sender, RoutedEventArgs e) { page = Math.Max(0, page - 1); SafeQuery(); }
    private void Up_Click(object sender, RoutedEventArgs e) { currentParent = parents.TryPop(out var p) ? p : 1; page = 0; ViewPicker.SelectedIndex = 0; SafeQuery(); }
    private void ResultsGrid_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ResultsGrid.SelectedItem is not ScanEntry { Kind: "folder" } entry) return;
        parents.Push(currentParent); currentParent = entry.Id; page = 0; ViewPicker.SelectedIndex = 0; SafeQuery();
    }
    private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultsGrid.SelectedItem is not ScanEntry entry) return;
        DetailText.Text = $"{entry.Path}\n{entry.SizeLabel} • {entry.StatusLabel} • {(entry.IsEmpty ? "Subtree kosong terverifikasi. " : "")}{entry.Detail}\n{new SafetyPolicy(settings.ProtectedRoots).Evaluate(entry.Path).Reason}";
    }
    private void CopyPath_Click(object sender, RoutedEventArgs e) { if (ResultsGrid.SelectedItem is ScanEntry entry) try { Clipboard.SetText(entry.Path); } catch (Exception ex) { ShowError(ex); } }
    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (currentDatabase == null || summary == null || scanCancellation != null) return;
        if (MessageBox.Show(this, "Laporan menyertakan nama dan path file pribadi. Simpan hanya di lokasi yang Anda percaya.", "Ekspor metadata", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK) return;
        var dialog = new SaveFileDialog { FileName = $"cappy-scan-{summary.Started:yyyyMMdd-HHmmss}.md", Filter = "Markdown (*.md)|*.md|Teks (*.txt)|*.txt" };
        if (dialog.ShowDialog(this) != true) return;
        try { await SnapshotReport.ExportAsync(currentDatabase, dialog.FileName); StatusText.Text = "Laporan lengkap disimpan: " + dialog.FileName; }
        catch (Exception ex) { ShowError(ex); }
    }
    private async void LoadHistory()
    {
      try
      {
        var rows = await Task.Run(() => {
        var records = new List<HistoryDisplay>();
        foreach (var file in Directory.EnumerateFiles(dataDirectory, "scan-*.db"))
            try { using var store = new SnapshotStore(file); if (store.GetSummary() is ScanSummary s) records.Add(new(file, s.Root, s.Started, s.State == "running" ? "Terputus / periksa" : StateLabel(s.State))); }
            catch (Microsoft.Data.Sqlite.SqliteException) { /* Corrupt files are retained, never auto-deleted. */ }
        return records.OrderByDescending(x => x.Started).ToArray(); });
        HistoryGrid.ItemsSource = rows;
      } catch (Exception ex) { ShowError(ex); }
    }
    private void OpenHistory_Click(object sender, RoutedEventArgs e) => OpenHistory();
    private void History_DoubleClick(object sender, MouseButtonEventArgs e) => OpenHistory();
    private async void OpenHistory()
    {
        if (scanCancellation != null || HistoryGrid.SelectedItem is not HistoryDisplay h) return;
        try
        {
            var loaded = await Task.Run(() => { using var store = new SnapshotStore(h.Database); return store.GetSummary(); });
            if (loaded == null || scanCancellation != null) return;
            summary = loaded; currentDatabase = h.Database;
            currentParent = 1; parents.Clear(); page = 0; RootBox.Text = summary.Root;
            SummaryText.Text = $"Snapshot {summary.Started:dd MMM yyyy HH:mm:ss} • {StateLabel(summary.State)} • {ByteSize.Format(summary.LogicalBytes)} logis\n{summary.Root} • Data historis; scan ulang untuk kondisi terbaru.";
            MainTabs.SelectedIndex = 0; ViewPicker.SelectedIndex = 0; SafeQuery(); SetBusy(false);
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        try { settings = new() { ProtectedRoots = ParsePaths(ProtectedBox.Text), Exclusions = ParsePaths(ExclusionsBox.Text) }; settings.Save(dataDirectory); StatusText.Text = "Pengaturan disimpan. Pengecualian berlaku pada scan berikutnya."; }
        catch (Exception ex) { ShowError(ex); }
    }
    private static string[] ParsePaths(string text) => text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => Path.IsPathFullyQualified(x) && x.IndexOfAny(['*', '?']) < 0 ? Path.GetFullPath(x) : throw new IOException("Gunakan path absolut tanpa wildcard: " + x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    private void ShowError(Exception ex) { StatusText.Text = ex.Message; MessageBox.Show(this, ex.Message, "Cappy Cleaner", MessageBoxButton.OK, MessageBoxImage.Warning); }
    private static string StateLabel(string state) => state switch { "completed" => "Selesai", "partial" => "Cakupan sebagian", "cancelled" => "Dibatalkan", "running" => "Belum selesai / periksa", _ => state };
    private void Window_Closing(object? sender, CancelEventArgs e)
    { if (scanCancellation != null) { e.Cancel = true; scanCancellation.Cancel(); StatusText.Text = "Menunggu scanner berhenti dan menyimpan snapshot. Tutup kembali setelah selesai."; } }
    private sealed record DriveDisplay(string Path, string Label, long Total, long Free);
    private sealed record HistoryDisplay(string Database, string Root, DateTimeOffset Started, string State) { public string StartedLabel => Started.ToString("dd MMM yyyy HH:mm:ss"); }
}
