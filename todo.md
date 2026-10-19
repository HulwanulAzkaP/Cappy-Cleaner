# Cappy Cleaner — rencana implementasi dan checkpoint

Terakhir diperbarui: 19 Oktober 2026 (Asia/Bangkok).
Status: scanner preview 0.1.1: jendela lebih lebar/maksimal, tombol Full scan Disk C:, perbaikan database scan seluruh drive. MVP penuh belum selesai.

## Cara melanjutkan

1. Baca file ini, `prd.md`, lalu `docs/superpowers/specs/2026-10-19-cappy-cleaner-windows-design.md`.
2. Periksa `git status --short` dan file source sebelum mengubah apa pun. Jangan mengulang pekerjaan yang sudah tercentang tanpa alasan baru.
3. Gunakan CodeGraph lebih dahulu. MCP/CLI tidak tersedia pada awal sesi; fallback berupa `rg` dan baca source terarah. Jangan install/index CodeGraph tanpa izin setup.
4. Jalankan verifikasi yang tercatat, lanjut dari pekerjaan aktif, perbarui bagian checkpoint dan hasil tes.
5. Tidak ada pengujian cleanup pada data nyata. Gunakan fixture/VM; contoh historis PRD bukan perintah untuk dijalankan.

## Usage dan kebijakan checkpoint

- Baseline akun saat mulai: window 5 jam **79% terpakai / 21% tersisa**, mingguan **47% terpakai / 53% tersisa**. Ini usage akun bersama, bukan ukuran khusus chat.
- Tool tidak menyediakan hitungan persentase pemakaian chat ini. Tafsiran konservatif: checkpoint lengkap setiap kenaikan 5 poin dari baseline (berikutnya 84%), serta ketika sisa <=5%. Tidak menunggu ambang untuk menyimpan kemajuan.
- Saat ambang tercapai: prioritaskan update file ini dengan source terakhir, command/output, kegagalan, langkah berikutnya yang konkret, dan status artefak. Jangan menyatakan pekerjaan selesai jika masih ada syarat rilis.
- Checkpoint tersimpan: usage **85% terpakai / 15% tersisa**, mingguan 48%. Ambang +5 baseline terlewati; checkpoint berikutnya 90%, lalu 95% (5% tersisa).
- Checkpoint kedua: **91% terpakai / 9% tersisa**, mingguan 49%. Paket scanner preview sudah ada; dua perbaikan review sedang dalam RED (detail di bawah). Prioritas berikutnya checkpoint 95%.
- Checkpoint terakhir yang terbaca: **94% terpakai / 6% tersisa**, mingguan 50%. Semua perubahan tersimpan sebelum sisa 5%; tidak ada pekerjaan mutasi terhadap data pengguna. Angka usage dapat berubah karena akun bersama; periksa ulang saat resume.
- **CHECKPOINT AMBANG PENGGUNA: 95% terpakai / 5% tersisa**, mingguan 50%, dibaca setelah paket dan commit lokal selesai. Prioritas penyimpanan checkpoint sudah dilakukan. Mulai sesi berikutnya dari urutan implementasi nomor 1 (metadata identity/scope/schema version), bukan mengulang scaffolding atau scanner yang sudah lolos tes.

## Keputusan tetap

- Native desktop Windows 11 x64, C# + WPF, SDK .NET 10 tersedia (10.0.113; runtime 10.0.13).
- `CappyCleaner.exe` memakai manifest `requireAdministrator`; UAC ditolak berarti aplikasi tidak berjalan.
- Installer `.exe` ke Program Files; runtime dibundel. Portable ZIP opsional.
- Hak admin bukan approval cleanup, bukan jaminan akses semua lokasi, dan tidak membolehkan ubah ACL/ownership/proteksi.
- Source lokal, tanpa telemetry/autostart/service. AI opt-in, keputusan executor deterministik.
- Tidak ada auto-select, auto-clean, fallback permanent delete, atau shell bebas.
- Data snapshot/jurnal milik akun proses; identitas proses dan scope root eksplisit, terutama jika UAC memakai akun admin lain.

## Peta file yang direncanakan

- `src/CappyCleaner.Core/`: model, scanner, analisis/policy dan kontrak snapshot; tidak bergantung UI.
- `src/CappyCleaner.App/`: WPF, manifest admin, navigasi, scan/cancel, review dan riwayat.
- `tests/CappyCleaner.Core.Tests/`: tes fixture untuk scanner, proteksi dan verifikasi.
- `installer/CappyCleaner.iss`: installer admin dengan shortcut dan uninstall.
- `scripts/`: build, test, publish/package yang dapat dijalankan ulang.
- `artifacts/`: hasil build lokal (diabaikan Git), bukan source.
- `README.md`: cara build, run, keterbatasan aktual dan pengujian.

## Tahap A — fondasi, scanner dan distribusi

- [x] Baca PRD dan setujui rancangan native/admin melalui arahan pengguna.
- [x] Periksa tooling .NET dan baseline usage.
- [x] Buat solution, projects dan ignore output build.
- [x] Test fixture dulu: ukuran hierarkis tidak ganda, file nol bukan folder kosong, error tidak menjadi nol, link tidak diikuti, cancel mengembalikan parsial.
- [x] Streaming scanner metadata ke SQLite, stack enumerator sebesar kedalaman folder, commit batch <=1.000 entri, agregasi folder dan hasil paginasi; tidak memuat seluruh file ke RAM. Benchmark jutaan file masih pending.
- [x] Jangan hydrate cloud placeholder, jangan mengikuti reparse; catat skipped/error beserta cakupan (tes junction lolos; fixture cloud nyata masih perlu).
- [x] UI native Indonesia: overview drive, root picker eksplisit, progres faktual, batal, file/folder terbesar dan detail (build lolos; visual/runtime manual belum).
- [x] Informasi admin/SID, disk/RAM terpisah, satuan GiB/MiB konsisten dan snapshot timestamp (manifest/implementasi; UAC runtime belum).
- [x] Pengaturan lokasi terlindungi/pengecualian; proyek default awal terlindungi.
- [x] Snapshot persisten dan ekspor Markdown/text atas tindakan pengguna.
- [x] Build Release dan publish self-contained win-x64 (paket sebelum perbaikan review; rebuild wajib).
- [ ] Installer `.exe`, validasi layout paket, upgrade dan uninstall (kompilasi awal berhasil; acceptance instalasi belum).
- [ ] Uji UAC terima/tolak, akun standar/kredensial admin lain dan mesin tanpa runtime (VM; jangan klaim sudah diuji jika belum).

## Tahap B — inventori dan analisis

- [ ] Inventori Win32 HKLM 64/32 dan HKCU; parser tanpa menjalankan UninstallString sembarang.
- [ ] Appx Main/Framework/Resource/Bundle/Optional dan scope akun; unknown/partial jelas.
- [ ] Korelasi filesystem dan registrasi; jangan menganggap tidak terdaftar berarti yatim.
- [ ] Finding/evidence terpisah dari risiko; kandidat cache awal npm/npx/uv dan Temp terkurasi.
- [ ] Empty leaf/subtree memerlukan enumerasi lengkap, bukan ukuran nol.
- [ ] WindowsApps inspector read-only dan nonremovable protection.
- [ ] Compare snapshot dengan root/scope/cakupan sebanding; refresh perubahan manual dan provenance.

## Tahap C — eksekusi terkontrol

- [ ] TDD policy: root sistem, profil, akun lain, project/.git/rahasia diblokir; unknown review saja.
- [ ] Manifest berisi snapshot, objek/identity, ukuran, provider/version, metode, risiko/pemulihan dan scope akun.
- [ ] Dialog final tanpa preselection; approval host satu operasi, expiry awal 10 menit, terikat hash manifest.
- [ ] Validasi ulang identitas/link/subtree/lock sebelum eksekusi. Jika jaminan sasaran tidak tersedia, fail closed/manual handoff.
- [ ] Recycle Bin via API Windows; tidak fallback permanent deletion, tes terhadap fixture saja.
- [ ] npm/npx/uv native adapters versi/root tervalidasi, tanpa executable atau argumen bebas dari AI.
- [ ] Uninstall native Win32/Appx yang didukung, verifikasi filesystem dan registrasi terpisah.
- [ ] Jurnal sebelum mutasi, satu cleanup aktif, cancel pada batas aman, crash reconciliation tanpa replay otomatis.
- [ ] Hasil per item dan delta ruang terukur; exit code saja bukan bukti keberhasilan.

## Tahap D — AI, hardening dan rilis

- [ ] Adapter AI opt-in: pilihan provider/protokol, credentials Windows, alias path dan persetujuan metadata cloud.
- [ ] Kontrak tool PRD 15.3, pagination, evidence, scope/timestamp/error.
- [ ] Tes prompt injection, approval palsu, target ambigu, ganti provider/path/akun dan retry timeout.
- [ ] Uji seluruh 28 acceptance criteria PRD; petakan bukti tiap kriteria di file ini.
- [ ] Benchmark fixture jutaan entri: waktu, peak RAM, database, update UI <=2 detik dan cancel acknowledgement <=1 detik.
- [ ] Review aksesibilitas, path panjang/non-ASCII, upgrade/migrasi/retensi dan rollback data migrasi gagal.
- [ ] Signing Authenticode sebelum rilis publik (sertifikat belum dipilih); jangan mengarang klaim SmartScreen.
- [ ] Portable opsional dan provider tambahan hanya sesudah validasi scope/versi.

## Verifikasi yang sudah dijalankan

- `dotnet --info`: SDK 10.0.113 dan WindowsDesktop runtime 10.0.13 tersedia, win-x64.
- `Get-Command codegraph`: tidak tersedia; tidak ada CodeGraph MCP pada sesi.
- RED: 7 tes scanner/policy gagal karena stub NotImplementedException; 1 tes template lolos.
- GREEN: `dotnet test tests/CappyCleaner.Core.Tests --verbosity normal --no-restore`: 8/8 lolos (7 perilaku + 1 template), build 0 warning/error. Tes junction memakai mklink /J hanya di fixture temporary.
- Dependency awal SQLite native 2.1.11 memberi NU1903; diganti direct bundle 3.0.5 (SQLite 3.53.4), restore/build berikutnya tanpa warning. Perlu audit dependencies pada verifikasi akhir.
- Belum ada tes UI/installer; jangan menandai tahap selesai sebelum output nyata diperiksa.
- RED ekspor: `ReportTests.ExportsFullMetadataWithoutFileContents` gagal pada stub; GREEN `dotnet test tests/CappyCleaner.Core.Tests -v quiet --no-restore`: **8/8 tes perilaku lolos**, tes template sudah dihapus.
- `dotnet build src/CappyCleaner.App -c Release -v quiet`: **0 warning, 0 error**. Belum berarti aplikasi diluncurkan/UAC diuji.
- `scripts/build.ps1`: 8/8 tes Release, solution build 0 warning/error, publish self-contained, ISCC sukses. Installer awal 47.383.032 bytes; folder publish sekitar 140 MB. Source setelah review harus dipaket ulang.
- `mt.exe` membaca manifest dari EXE hasil publish: `requireAdministrator`, `longPathAware=true`, `PerMonitorV2` tertanam. DLL native `e_sqlite3.dll` disertakan.
- `dotnet list src/CappyCleaner.Core package --vulnerable --include-transitive`: tidak ada paket rentan menurut sumber audit saat ini.
- `powershell.exe -NoProfile -STA -File scripts/render-preview.ps1`: render statis XAML sukses, gambar `artifacts/ui-preview.png` diperiksa. Ini bukan peluncuran aplikasi/admin dan bukan tes handler UI.
- Review independen read-only: dua P2 (SQLite query di UI thread; core tidak menolak DB di dalam root), satu minor AppMutex belum dibuat aplikasi. Cleanup/inventory/AI pending secara eksplisit.
- Review fixed: dua tes regresi RED→GREEN; SQLite query pindah ke `SnapshotQueries.ReadAsync` worker, cancellation sebelum/sesudah query, maksimum satu refresh aktif + satu permintaan terbaru; hasil stale dibuang. Riwayat dan pembukaan summary juga dibaca di worker.
- Penguatan tambahan RED→GREEN: tolak database melalui junction, root UNC/relative/wildcard sebelum akses. Total `dotnet test tests/CappyCleaner.Core.Tests -v quiet --no-restore`: **14/14 lolos**. Bukan pembuktian bahwa path tidak bisa diganti proses berbahaya saat scan; cleanup tetap dinonaktifkan.
- **Verifikasi paket terakhir:** `scripts/build.ps1` selesai exit 0 setelah seluruh perbaikan. Tes Release **14/14**, solution build **0 warning/error**, self-contained publish dan ISCC sukses. Tidak ada proses build yang masih harus ditunggu.
- Manifest EXE terakhir diekstrak kembali dengan `mt.exe` dan diassert `requireAdministrator`; pemeriksaan lolos. Installer SHA256: `5174DE07D1BB4F8A8AD51AFE375484663C4C7659AF77153BEE2F9831927EB42B`.
- Minor review AppMutex diselesaikan dengan menghapus directive yang tidak didukung aplikasi; Restart Manager `CloseApplications` tetap dipakai installer.

## Checkpoint aktif

- Pembaruan permintaan terbaru: area hasil diperlebar (jendela maksimal, lebar awal 1480, sidebar 190, margin konten 18); tombol `Full scan Disk C:` memilih C:\ dan memulai scanner yang sama, nonaktif selama scan aktif.
- Akar warning database: database LocalAppData berada di bawah C:\, dan larangan database-di-root lama mengabaikan exclusions. Core kini mengizinkan database di dalam drive hanya jika SELURUH direktori output dikecualikan, sementara root sendiri bukan pengecualian. Junction output dan database di sumber tanpa exclusion tetap ditolak.
- Tes regresi `AllowsSnapshotInExplicitlyExcludedDataDirectory`: awalnya RED, sekarang GREEN; bytes/file database tidak masuk hasil. `scripts/build.ps1` v0.1.1: **15/15 tes Release lolos**, build 0 warning/error; status kompilasi installer diperiksa sebelum penyerahan.
- GitHub public dibuat atas permintaan eksplisit pengguna: **https://github.com/HulwanulAzkaP/Cappy-Cleaner**. Tidak ada `gh` CLI; pembuatan memakai GitHub REST dengan credential Git yang sudah tersimpan, credential tidak dicetak/disimpan di source. Push source dilakukan setelah commit pembaruan. Tidak ada force push.
- Verifikasi publik tanpa autentikasi GitHub API: `private=false`, default branch `codex/windows-app`. Commit source `3b1e0b6dd6a85f5fc9eeee24a937a1c4f6be27fb` sama dengan `git ls-remote origin refs/heads/codex/windows-app`; push berhasil. Checkpoint dokumentasi menyusul melalui commit terpisah.
- Paket v0.1.1 selesai exit 0: 15/15 tes Release, build tanpa warning/error, publish/ISCC sukses. Installer baru **47.382.521 bytes**. Preview XAML 1480×900 diperiksa: tabel lebih lebar dan tombol Full scan Disk C: tampil. Ini verifikasi statis, bukan tes full scan C: atau UAC runtime.
- Artefak versi pembaruan: `artifacts/installer/CappyCleaner-Setup-0.1.1-x64.exe` dan `artifacts/publish/win-x64/CappyCleaner.exe` bersama dependensinya. Artefak biner tidak masuk Git; repo berisi source + script build.
- Tetap belum menguji full scan C:\ pada data nyata/VM, UI event runtime dan UAC/install; tes otomatis memakai fixture. Penolakan akses Windows tetap dicatat; full scan tidak berarti semua folder pasti dapat dibaca.

- Pekerjaan aktif berikutnya: schema version/identitas volume-akun-file dan metadata coverage sebagai fondasi analisis/compare/cleanup. Paket scanner tahap A sudah siap untuk tes manual di VM.
- Repo awal hanya berisi `prd.md` dan dokumen rancangan, keduanya belum tracked. Jangan menimpa/menghapus file pengguna.
- Source terakhir: `src/CappyCleaner.App/MainWindow.xaml`, `MainWindow.xaml.cs`, `App.xaml`, `AppSettings.cs`, `app.manifest`; scanner/store/models/policy/report di Core. Test ada di ScannerTests.cs dan ReportTests.cs.
- Langkah berikutnya tepat: baca `ScanModels.cs`, `DiskScanner.cs`, `SnapshotStore.cs`, serta PRD §5/6/15.2; tulis tes metadata identitas/scope dan migrasi skema sebelum mengubah model SQLite. Setelah fondasi fakta siap lanjut inventori, analisis dan compare. Lihat urutan lanjutan di bawah; jangan mulai executor dari path+ukuran saja.
- Perintah lanjut: `dotnet test tests/CappyCleaner.Core.Tests -v quiet`; `dotnet build CappyCleaner.slnx -c Release -v quiet`.
- Artefak terbaru: `artifacts/publish/win-x64/CappyCleaner.exe` (seluruh folder dependensi wajib ikut), `artifacts/installer/CappyCleaner-Setup-0.1.0-x64.exe`, `artifacts/ui-preview.png` dan `artifacts/application.manifest`. Belum signed; jangan klaim install/UAC runtime teruji.
- Compiler Inno Setup ditemukan pada `%LocalAppData%/Programs/Inno Setup 6`; signing certificate belum tersedia dan VM acceptance UAC/install belum dipakai.
- Keterbatasan penting: belum ada eksekusi cleanup, inventori Appx/Win32, compare snapshots, allocated size/file identity, retensi otomatis dan AI. UI Tentang menyatakan kemampuan aktual. Tidak boleh mengklaim MVP penuh selesai.

## Log keputusan / perubahan

- 19 Oktober 2026: pengguna meminta implementasi dan todo detail yang terus diperbarui; ini memberi izin melanjutkan dari rancangan ke kode tanpa meminta persetujuan rencana lagi.
- Memakai `todo.md` ini sebagai rencana dan ledger utama sesuai permintaan pengguna; implementasi dilakukan di sesi ini dengan tes perilaku inti dan pencatatan bukti aktual.
- Repo belum mempunyai HEAD/commit, sehingga worktree belum dapat dibuat tanpa membuat commit awal. Dipakai branch unborn `codex/windows-app` di direktori yang diminta; dokumen pengguna tetap utuh. Tidak ada push/merge.
- Antarmuka inti: `DiskScanner.ScanAsync(root,database,exclusions,progress,cancellationToken) -> Task<ScanSummary>`; `SnapshotStore` query paginasi dan enumerasi ekspor; `SafetyPolicy.Evaluate(path)` saat ini selalu menolak eksekusi sampai provider aman tersedia. Jangan mengira policy sudah mengizinkan cleanup.
- Preflight: UI mengonsumsi ScanSummary/ScanEntry yang sama dengan scanner/store; koleksi tabel dibatasi 200 item per halaman, bukan seluruh scan.

## Pemetaan acceptance criteria PRD (bukan klaim MVP selesai)

| PRD §10 | Status bukti saat ini | Yang masih harus dibuktikan |
|---|---|---|
| 1 scan/preview tidak memutasi sumber | Tes sumber tidak berubah; output di sumber/junction ditolak | Perbandingan metadata penuh fixture sebelum/sesudah, race dan cloud |
| 2 tidak auto-clean/auto-select | Tidak ada executor atau pilihan cleanup; app tidak auto-scan | Tes UI runtime ketika executor ditambahkan |
| 3 review rincian | Detail path/ukuran/status/reason read-only ada | Manifest, metode/dampak/pemulihan dan dialog review |
| 4 sasaran tidak meluas | Semua delete ditolak policy | Parent/sibling overlap fixtures saat provider tersedia |
| 5 proyek/.git/akun/sistem terlindungi | Tes policy lokasi proyek/.git/Windows | Identitas akun dan tes seluruh daftar sistem/rahasia |
| 6 junction/object berubah/item baru | Tes junction scanner dan output lewat junction | Identitas handle/TOCTOU dan subtree manifest sebelum mutasi |
| 7 Temp baru/terkunci | Belum ada provider Temp | Fixture umur/lock/identifikasi dan proses tetap hidup |
| 8 penolakan dan partial | Scan missing root/exclusion ditandai partial | Status executor policy/UAC/uninstaller/lock dan tanpa fallback |
| 9 game native uninstall | Belum ada uninstall | Fixture adapter dan VM aplikasi uji |
| 10 ukuran tidak menyesatkan | Hierarki dites; logis diberi label; unknown terpisah | Hard link identity, allocated/sparse/compressed fixture |
| 11 refresh eksternal | Scan ulang menghasilkan snapshot baru | Compare dan bukti absent per finding |
| 12 verifikasi sisa/disk delta | Belum ada cleanup | Verifier filesystem/registration/volume before-after |
| 13 batal/crash | Tes cancel dan snapshot persisten; running ditampilkan terputus | Crash-injection/jurnal mutasi tanpa replay |
| 14 fixture/VM | Seluruh tes berkas memakai sandbox temporary | VM untuk install dan destructive providers |
| 15 seluruh tipe Appx | Pending | Main/Framework/Resource/Bundle/Optional + inventory completeness |
| 16 registrasi vs berkas | Pending | Test filesystem absent / registration present |
| 17 folder kosong | Tes file zero-byte bukan kosong; link/unknown tidak empty | Fixture denied child dan cloud subtree |
| 18 uninstall entry bukan bukti tidak ada | Belum ada inventori/residue analyzer | Portable/custom owner unknown fixtures |
| 19 jenis residue terpisah | Pending | Evidence dan confidence/risk model + shared dependencies |
| 20 scan admin terbatas | Manifest requireAdministrator; tidak ada ACL/SYSTEM actions | VM UAC ditolak/diterima dan root cakupan tepat |
| 21 AI tanpa shell/approval | AI belum tersedia, tidak ada arbitrary shell endpoint | Tool authority dan approval forgery tests |
| 22 prompt injection sebagai data | Scanner tidak membaca konten; ekspor metadata saja | Filename/README/log injection setelah AI tersedia |
| 23 natural language terikat IDs | Pending | Finding IDs+snapshot dan target ambiguity tests |
| 24 scope all-users/SYSTEM | Tidak tersedia | Native package mutation dengan scope manifest terpisah |
| 25 laporan Done manual | Pending | Provenance user_reported_done→verified_absent |
| 26 snapshot/satuan konsisten | Timestamp+ID di report; history diberi label historis | Compare scope dan tool AI memakai snapshot sama |
| 27 approval expiry/objek berubah | Pending | Manifest hash/session/expiry/provider/object revalidation |
| 28 pengujian serangan AI | Pending | Parent/path/akun/provider substitution dan approval palsu |

## Urutan implementasi setelah checkpoint ini

1. **Fondasi fakta sebelum cleanup.** Tambah schema version dan migrasi fail-safe; simpan volume identity, SID/scope, start/end timestamp, root error/coverage, file identity + change metadata. Tes file identity berubah dan unknown/null. Ini prasyarat compare serta validator; jangan memakai path+ukuran saja sebagai identitas.
2. **Scanner hardening.** Tes akar melalui junction, nested reparse, denied directory (fixture ACL VM), zero-byte child, deep/long/non-ASCII paths, cloud placeholders tanpa hydration, hard links/sparse/compression; allocated size opsional dengan unknown bila tidak didukung. Cancellation saat operasi OS tersangkut tidak boleh diklaim hard deadline.
3. **Inventori.** Buat `Inventory/Win32Inventory.cs`, `AppxInventory.cs`, model evidence dan scope; adapter read-only worker; semua package types dan partial errors. Jangan menjalankan UninstallString mentah atau menggunakan PowerShell string dari input.
4. **Analisis.** Buat Finding/Evidence IDs per snapshot; bedakan confidence/risk; identifikasi Temp/cache provider awal tanpa aksi; empty leaf vs subtree terpisah; unknown/residue/shared dependency tidak diberi label aman.
5. **Compare/retensi.** Bandingkan hanya root/volume/SID dan coverage sebanding; flag unknown jangan jadi absent; delta logis berbeda dari free disk. Implementasi retensi default 5 snapshot + jurnal 90 hari dengan pengecualian operasi aktif dan opsi hapus snapshot eksplisit. UI data usage lokal perlu terlihat.
6. **Planner dan approval.** DTO immutable manifest; provider/version/scope/identity/evidence/estimate/recovery; approval host single-use 10 menit; tes expiry/hash/snapshot/target/account substitution sebelum executor.
7. **Executor dan verifier.** Satu operasi, jurnal durable sebelum mutasi, guarded native adapters, fail-closed identity/link/lock, cancel batas aman, no replay pasca crash. Hanya Recycle Bin/provider native yang kompatibel; tidak ada permanent fallback. Semua tes destructive hanya fixture/VM.
8. **AI dan rilis.** Ikuti kontrak PRD §15.3, paginasi/alias path, opt-in external metadata, credential manager; uji injection. Lengkapi signing, VM clean install/UAC/kredensial akun lain/upgrade/uninstall, aksesibilitas, tema OS dan benchmark terukur sebelum klaim rilis MVP.

## Command dan lokasi penting untuk resume

```powershell
# Dari repo Cappy Cleaner, di PowerShell Windows
dotnet test tests/CappyCleaner.Core.Tests -c Release -v quiet
dotnet build CappyCleaner.slnx -c Release -v quiet
& .\scripts\build.ps1
powershell.exe -NoProfile -STA -File .\scripts\render-preview.ps1
dotnet list src/CappyCleaner.Core package --vulnerable --include-transitive
```

- `scripts/build.ps1` mencari ISCC di LocalAppData/Programs atau Program Files; tidak memasang compiler otomatis.
- `scripts/render-preview.ps1` menghapus event handlers hanya pada XAML dalam memori untuk render statis, bukan mengubah source atau melewati UAC aplikasi. Preview bukan tes event handler.
- Jangan jalankan installer diam-diam pada host untuk mengklaim acceptance; pakai VM disposable. Agen host saat ini bukan administrator.
- Tidak ada service/background daemon. Tidak ada automasi jadwal. Tidak ada credential AI.
- Branch kerja: `codex/windows-app`. Snapshot commit lokal akan dibuat untuk source dan dokumentasi; lihat `git log -1` untuk hash, jangan mengasumsikan hanya ada perubahan tanpa commit. Tidak ada push/merge/PR.
- Snapshot source dan dokumentasi: commit `5dac363` (`feat: add native Windows scanner preview and admin installer`); checkpoint usage 95% ini disimpan dalam commit dokumentasi berikutnya. Working tree sebelum update checkpoint bersih. Artefak diabaikan Git dan berada di `artifacts/`, jangan dihapus saat resume.
- Installer final: **47.385.340 bytes**, EXE launcher **162.816 bytes** (runtime/DLL terpisah wajib ikut), SHA256 di bagian verifikasi. Angka launcher kecil bukan ukuran seluruh aplikasi.
- `git diff --cached --check` pada initial commit melaporkan Markdown hard line breaks bawaan `prd.md` dan blank EOF generated XAML/App; bukan warning compiler. PRD asli dipertahankan. Tidak ada klaim whitespace-check bersih.
