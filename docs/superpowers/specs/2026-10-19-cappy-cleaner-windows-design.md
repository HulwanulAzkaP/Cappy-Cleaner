# Rancangan Cappy Cleaner untuk Windows

Tanggal: 19 Oktober 2026. Status: rancangan untuk ditinjau, belum izin implementasi.
Sumber kebutuhan: `prd.md` lengkap dan arahan pengguna di chat ini.

## 1. Arah produk dan keputusan

Cappy Cleaner adalah aplikasi desktop Windows native untuk memahami penggunaan disk, meninjau kandidat pembersihan, dan menjalankan tindakan yang dipilih pengguna dengan bukti hasil. Seluruh kebutuhan dan batas keselamatan PRD tetap berlaku.

Keputusan pengguna:
- Aplikasi desktop Windows, dengan executable aplikasi dan installer `.exe`.
- Aplikasi berjalan dengan hak administrator; akses diminta melalui UAC saat dibuka.
- Sesi ini menghasilkan rancangan terlebih dahulu.

Usulan teknis penyusun, belum keputusan pengguna:
- C# + WPF pada .NET LTS yang masih didukung, dengan patch didukung saat implementasi dimulai.
- Windows 11 x64 sebagai target rilis pertama. ARM64 dan Windows lain memerlukan validasi tersendiri.
- Inno Setup untuk installer `.exe`; publish self-contained agar runtime .NET ikut didistribusikan.
- SQLite untuk snapshot, finding, manifest, dan jurnal lokal; daftar besar dibaca dengan pagination.
- Portable ZIP merupakan keluaran opsional setelah installer stabil, bukan syarat rilis pertama.

WPF memakai UI desktop Windows dan runtime .NET; istilah native di sini berarti aplikasi desktop khusus Windows, bukan aplikasi web, dan bukan klaim bahwa seluruh kode dikompilasi menjadi mesin native tanpa runtime. Distribusi self-contained berisi EXE dan dependensi; tidak harus satu file EXE tunggal.

Alternatif yang dipertimbangkan: WinUI 3 cocok untuk tampilan Windows modern tetapi menambah keputusan deployment; Tauri cocok untuk UI berbasis teknologi web tetapi bukan pilihan utama untuk kebutuhan UI desktop native ini. WPF dipilih sebagai usulan karena fokus Windows, kontrol UI desktop, dan integrasi API Windows dari C#.

## 2. Bentuk distribusi

| Artefak | Fungsi | Status rancangan |
|---|---|---|
| `CappyCleaner-Setup-<version>-x64.exe` | Memasang aplikasi, shortcut, dan entri uninstall | Keluaran utama |
| `CappyCleaner.exe` | Program yang dijalankan setelah instalasi | Wajib |
| `CappyCleaner-Portable-<version>-x64.zip` | Folder aplikasi yang diekstrak dan dijalankan tanpa installer | Opsional |

Installer meminta UAC dan memasang binary di `%ProgramFiles%\Cappy Cleaner`, dengan izin standar Windows yang tidak memberi pengguna biasa akses tulis ke binary. Shortcut tidak menyertakan parameter perintah bebas. Tidak memasang service, scheduled task, autostart, atau scanner latar belakang.

Program memakai manifest `requireAdministrator`. Saat dibuka, Windows menangani UAC atau permintaan kredensial admin. Jika ditolak, aplikasi tidak mulai; tidak mencoba cara lain untuk elevasi. Installer tidak otomatis menjalankan aplikasi memakai identitas akun installer yang mungkin berbeda dari pengguna desktop.

Upgrade manual lewat installer baru mempertahankan pengaturan dan riwayat, memeriksa versi skema, dan tidak mengulang cleanup yang tertunda. Program harus ditutup secara normal sebelum penggantian binary. Migrasi data gagal harus menghentikan upgrade data dan mempertahankan salinan sebelumnya. Downgrade dengan skema tidak kompatibel ditolak.

Uninstall menghapus program dan shortcut. Penghapusan data lokal Cappy Cleaner menjadi pilihan tersendiri yang tidak terpilih otomatis; uninstall tidak menjalankan cleanup terhadap temuan scan. Rilis publik direncanakan menandatangani EXE aplikasi dan installer dengan Authenticode; sertifikat/publisher belum tersedia dan perlu ditentukan sebelum distribusi publik. Signing tidak menjamin peringatan reputasi Windows langsung hilang.

Untuk portable: tetap meminta UAC. Jika ditambahkan, data disimpan pada lokasi data akun Windows secara default, bukan di sebelah EXE. Lokasi paket harus dipercaya; jangan menjadikan folder unduhan yang dapat ditulis pengguna lain sebagai lokasi instalasi elevated permanen.

## 3. Identitas pengguna dan administrator

Arahan admin memperbarui bagian 6.5 PRD yang sebelumnya menyatakan scan biasa tidak meminta admin. Yang berubah hanya model peluncuran: tidak ada izin pembersihan otomatis atau tambahan cakupan akun.

Token administrator bukan jaminan akses seluruh filesystem. Tetap tidak ada perubahan ACL/ownership, pematian Defender, proses SYSTEM, atau bypass penolakan Windows. UAC memberi hak OS; manifest tindakan dan konfirmasi pengguna tetap menentukan tindakan yang boleh dilakukan aplikasi.

Tampilkan identitas proses dan cakupan akun di UI. Jika pengguna standar memasukkan kredensial akun admin lain, jangan menganggap `%USERPROFILE%` proses adalah profil pengguna yang dimaksud. Petakan akun/SID dan root secara eksplisit; pemilihan profil lain memerlukan pilihan cakupan yang jelas. Tidak otomatis memindai atau membersihkan profil admin yang baru dipakai untuk elevasi. Data aplikasi disimpan di LocalAppData milik akun proses dan ditandai identitasnya; data lintas akun tidak digabung otomatis.

Tidak menerima shell string, plugin dari folder pengguna, atau perintah startup dari berkas yang dipindai. Akses admin membuat pemisahan data tidak tepercaya dan tindakan bertipe semakin penting. Aturan blok sistem/proyek/rahasia dalam PRD tetap wajib.

## 4. Susunan aplikasi dan alur layar

Usulan visual: UI desktop yang tenang, font sistem Segoe UI, mengikuti tema Windows, Bahasa Indonesia, sidebar tetap, tabel yang dapat diurutkan, detail saat item dipilih, serta dukungan keyboard dan pembaca layar. Warna bukan satu-satunya pembeda status. Gambar mockup belum dibuat.

| Halaman | Isi dan interaksi utama |
|---|---|
| Ringkasan | Drive, ruang kosong, snapshot terakhir, cakupan akun, indikator Administrator, RAM terpisah, pilih drive/folder lalu Mulai scan |
| Penjelajah penyimpanan | Pohon folder dan tabel file terbesar, filter, pencarian, ukuran logis/alokasi bila diketahui, error dan lokasi dilewati |
| Kandidat pembersihan | Kategori, estimasi, keyakinan klasifikasi, risiko, metode tersedia; semua checkbox awal kosong |
| Aplikasi | Inventori Win32 dan Appx, jenis paket, scope akun, status registrasi/filesystem yang terpisah, uninstall yang didukung |
| Pemeriksaan lanjutan | WindowsApps, folder kosong, dugaan residu, akses gagal, dan bukti ketidakpastian |
| Review tindakan | Sasaran persis, metode per item, konsekuensi, pemulihan, estimasi, scope akun dan konfirmasi |
| Riwayat dan perbandingan | Hasil per item, sisa, waktu pengukuran, delta ruang kosong, refresh perubahan manual dan ekspor |
| Pengaturan | Lokasi terlindungi, pengecualian, retensi data, tema, AI opsional dan pengaturan privasi |

Alur utama: buka + UAC → pilih cakupan → scan → tinjau hasil → pilih item → review manifest → konfirmasi → validasi ulang → jalankan → verifikasi.

Selama scan tampilkan jumlah file/folder diproses, lokasi saat ini, waktu berjalan dan akses gagal. Jangan membuat persentase ketika total belum diketahui. Hasil parsial dan pembatalan tersedia; pause/resume bukan janji rilis pertama.

Folder kosong tidak dimasukkan ke rekomendasi pemulihan ruang besar. WindowsApps bukan kumpulan folder yang dapat dihapus manual. Data yang belum terbaca menggunakan unknown, bukan nol.

## 5. Arsitektur

Modul utama:
1. Desktop UI: navigasi, view model, tabel virtual, dialog review dan persetujuan tepercaya.
2. Application services: koordinasi scan, snapshot, inventori, compare, export dan satu antrean cleanup.
3. Scanner/inventory: enumerasi metadata streaming, agregasi hierarki, informasi volume, identitas file dan inventori aplikasi.
4. Analysis/policy: evidence, kategori, confidence dan risiko terpisah, proteksi root, akun, reparse point dan tindakan yang diizinkan.
5. Planner/executor: manifest tetap, otorisasi terikat manifest, provider tervalidasi, jurnal dan verifikasi.
6. Persistence: SQLite dan pengaturan lokal dengan akses akun proses; isi file, token AI dan log proses mentah tidak disimpan di jurnal.
7. AI adapter opsional: kontrak tool pada bagian 15 PRD, konteks minimum dan tanpa wewenang persetujuan.

UI bekerja terpisah dari pekerjaan scan. Worker memakai antrean berbatas kapasitas, concurrency terkendali, penulisan database per batch, dan tidak memuat seluruh file ke RAM. Query tabel memakai pagination/virtualisasi. Agregasi direktori menjaga agar induk dan anak tidak dijumlahkan dua kali. Metadata hard link dan ukuran fisik dipakai jika dapat diperoleh; jika tidak, estimasi diberi label.

Scan metadata tidak mengubah sumber yang dipindai. Penulisan snapshot/jurnal hanya ke direktori data aplikasi, yang dikecualikan dari scan. Pemeriksaan reparse/cloud dilakukan sebelum traversal atau akses yang dapat memicu hydration; jangan mengikuti link atau membaca konten placeholder.

Integrasi Windows menggunakan API bertipe bila tersedia: enumerasi filesystem dan file identity, API Shell untuk Recycle Bin dengan pengujian tanpa fallback permanen, registry untuk inventori Win32, serta API deployment paket untuk Appx. Provider native tertentu boleh memakai adapter PowerShell/tool terpasang dengan operasi dan argumen tetap, bukan script bebas dari UI/AI. Pemilihan API rinci dan kompatibilitasnya ditetapkan saat implementasi.

## 6. Data dan transaksi tindakan

Entitas inti: Volume, ScanJob, ScanRoot, Snapshot, FileEntry, Finding, Evidence, InstalledApplication, ProtectedLocation, CleanupPlan, PlanItem, ApprovalReference, Operation, OperationItem dan Verification.

Semua finding mengacu ke snapshot, volume, akun dan bukti. Ukuran menyertakan jenis dan status kelengkapan. Path asli diselesaikan lokal; AI menggunakan alias secara default. Detail kontrak field mengikuti bagian 15.2 PRD.

State scan dan cleanup mengikuti PRD. Persetujuan berasal dari dialog aplikasi; backend membuat referensi terikat hash manifest, sesi, scope dan masa berlaku usulan 10 menit. Angka ini keputusan rancangan penyusun, dapat disesuaikan. Referensi dikonsumsi untuk satu operasi dan tidak diterima dari teks AI. Perubahan objek/cakupan/provider yang material membatalkan item dan membutuhkan review baru.

Sebelum eksekusi, validasi path dan identitas seluruh komponen, reparse, lock, akun, proteksi serta perubahan subtree. Jika mekanisme aksi tidak dapat menjamin sasaran tetap sama antara validasi dan operasi, aksi generik dinonaktifkan untuk sasaran itu; sediakan review/manual handoff. Ketersediaan API bukan bukti bahwa kondisi balapan sudah teratasi: ini harus dibuktikan pada tahap eksekusi.

Jurnal ditulis sebelum mutasi, lalu setelah setiap hasil dan verifikasi. Setelah crash, operasi menjadi perlu rekonsiliasi; aplikasi memeriksa keadaan nyata dan tidak melanjutkan penghapusan otomatis. Pembatalan menghentikan item berikutnya, bukan rollback atau pematian paksa uninstaller. Hasil filesystem, registrasi aplikasi, dan delta disk dipertahankan terpisah.

## 7. Provider awal dan batas MVP

MVP penuh tetap mencakup bagian 3 PRD; urutan pengiriman berikut adalah tahapan menuju MVP, bukan pengurangan cakupan diam-diam.

- Identifikasi awal: temp pengguna, npm/npx, uv, inventori aplikasi, file besar, folder kosong dan dugaan residu berbasis bukti.
- Eksekusi pertama: Recycle Bin untuk item manual yang memenuhi jaminan sasaran; npm/npx dan uv hanya untuk versi/root/tool yang terverifikasi; uninstall yang memiliki kontrak didukung. Tool tidak tersedia/versi tidak cocok menghasilkan status tidak didukung atau instruksi manual.
- Kategori lain tetap dapat dilihat tanpa mengaktifkan tombol eksekusi. Dukungan pnpm, VS Code, NVIDIA, Playwright, Android SDK, updater vendor dan kasus Appx khusus diperluas setelah pengujian versi dan scope.
- Inventori semua tipe Appx tersedia sebelum analisis WindowsApps. Aksi paket memakai package identity dan scope yang diverifikasi; paket nonremovable diblokir. Tidak ada penghapusan generik WindowsApps.
- AI disiapkan lewat kontrak service; tidak perlu AI agar scan/review/cleanup bekerja. Pilihan provider, protokol dan pengelolaan credential ditunda sampai tahap AI. Mode lokal tidak melakukan koneksi jaringan otomatis.

## 8. Data lokal, privasi dan ekspor

Usulan retensi: simpan 5 snapshot metadata terbaru dan jurnal tindakan 90 hari; pengaturan bisa diubah pengguna. Snapshot yang sedang dirujuk operasi aktif tidak dipangkas. Ini batas jumlah awal, bukan jaminan ukuran disk; ukur dan beri informasi pemakaian database, serta sediakan hapus snapshot eksplisit. Backup/migrasi menghormati retensi yang sama.

Ekspor Markdown dan teks mengikuti PRD dan memerlukan lokasi pilihan pengguna. Ekspor tidak otomatis upload. Peringatan privasi path ditampilkan saat ekspor; konteks cloud AI hanya dikirim setelah opt-in terpisah. Credential AI kelak memakai penyimpanan kredensial Windows, tidak file konfigurasi plaintext.

## 9. Tahapan dan syarat rilis

1. Fondasi desktop + paket awal: jendela native, manifest UAC, identitas/scope akun, publish self-contained dan installer percobaan di VM bersih.
2. Scanner read-only: disk, hierarki, file besar, error, cancel, reparse/cloud protection dan ekspor. Build ini belum mempunyai cleanup aktif.
3. Inventori/analisis: Win32, seluruh tipe Appx, evidence, folder kosong/residu, snapshot dan compare.
4. Eksekusi terbatas: policy, manifest, approval, provider pertama, jurnal, crash reconciliation, pembatalan dan verifikasi.
5. AI opt-in dan provider lanjutan: kontrak tool, alias path, batas konteks, prompt injection dan kompatibilitas.
6. Pematangan distribusi: upgrade/uninstall, signing untuk publik, dokumentasi dan portable bila dipilih.

Pengujian berkas hanya pada fixture atau VM terisolasi. Syarat penerimaan mencakup seluruh 28 kriteria PRD, ditambah: UAC terima/tolak; peluncuran dari akun standar dengan kredensial berbeda; Windows 11 bersih tanpa runtime terpasang; install/upgrade/uninstall; path panjang/non-ASCII; pemulihan dari crash; dan data aplikasi tidak ikut menjadi kandidat cleanup.

Benchmark scan jutaan entri mengukur waktu, puncak RAM, pertumbuhan SQLite dan respons UI. Target PRD untuk update progres maksimum dua detik dan pengakuan batal satu detik diuji. Tidak mengklaim performa/ukuran installer sebelum build dan pengukuran.

## 10. Yang diserahkan berikutnya

Setelah rancangan disetujui, susun rencana implementasi terperinci sebelum menulis kode produk. Keputusan tampilan rinci, paket UI tambahan, nomor versi SDK/paket, API interop spesifik dan strategi migrasi skema diserahkan ke tahap rencana dengan verifikasi dokumentasi. Sertifikat signing dan provider AI belum dipilih; keduanya tidak menghalangi fondasi scanner lokal.

## Referensi teknis

- [Microsoft WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/): UI desktop Windows pada .NET.
- [Microsoft deployment .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/): self-contained membundel runtime; executable tidak berarti semua dependensi satu file.
- [Kebijakan dukungan .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core): pilih versi didukung dan perbarui runtime yang dibundel bersama rilis aplikasi.
- [Inno Setup PrivilegesRequired](https://jrsoftware.org/ishelp/topic_setup_privilegesrequired.htm): mode admin meminta elevasi saat setup.

Penelusuran proyek menggunakan pembacaan file terarah karena CodeGraph MCP/CLI tidak tersedia. Belum ada build, benchmark, pengujian installer, atau eksekusi cleanup pada sesi rancangan ini.
