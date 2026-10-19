# PRD — Cappy Cleaner

Tanggal: 19 Oktober 2026  
Status: Draft diperluas dari pemeriksaan laptop dan rancangan integrasi AI  
Platform: Windows 11; dukungan versi Windows lain ditentukan kemudian  
Lokasi repo: `C:\Users\raffi\Downloads\Project\Cappy Cleaner`

## 1. Ringkasan produk

Cappy Cleaner adalah aplikasi Windows untuk memahami penggunaan penyimpanan: melakukan scanning, mengelompokkan file/folder, menemukan item besar, dan menyusun kandidat pembersihan dengan alasan serta konsekuensi yang jelas. Pengguna memilih sendiri tindakan pembersihan setelah meninjau hasil scan.

Masalah utama: pengguna mengetahui drive hampir penuh, tetapi kesulitan membedakan sampah, cache, aplikasi, proyek aktif, dan file pribadi yang masih bernilai. Produk harus membantu mengambil keputusan tanpa mengorbankan data atau mengklaim bahwa seluruh folder besar aman dihapus.

Dokumen ini merupakan spesifikasi produk, bukan implementasi. Framework, bahasa, paket, installer, rancangan visual, provider AI, serta protokol integrasi AI belum dipilih. Contoh perintah pada bagian 16 adalah catatan operasi yang sudah dilakukan, bukan kode aplikasi atau izin untuk menjalankannya kembali.

## 2. Pengguna dan tujuan

Pengguna awal adalah pemilik laptop Windows dengan campuran file pribadi, proyek pengembangan, aplikasi kreatif, game, dan cache tooling.

Tujuan:

- Menunjukkan lokasi penyimpanan yang paling berdampak untuk ditinjau.
- Membedakan penyimpanan disk dari penggunaan RAM.
- Memberi alasan dan tingkat keyakinan untuk setiap kandidat pembersihan.
- Memulihkan ruang dengan tindakan yang dipilih secara spesifik oleh pengguna.
- Memperlihatkan keberhasilan, kegagalan, item yang dilewati, dan ruang kosong aktual sesudah tindakan.

Ukuran keberhasilan:

- Tidak ada mutasi pada perangkat saat scan atau preview.
- Tidak ada penghapusan di luar item dan tindakan yang dikonfirmasi.
- Semua tindakan memiliki hasil yang dapat diperiksa kembali.
- Semua direktori yang gagal dibaca terlihat sebagai cakupan tidak lengkap.
- Tidak ada penghapusan folder terlindungi dalam skenario uji keselamatan.

## 3. Ruang lingkup MVP

Termasuk:

1. Ringkasan kapasitas, ruang terpakai, dan ruang kosong drive lokal.
2. Scan drive atau folder pilihan, termasuk hasil parsial selama pemindaian.
3. Pengelompokan berdasarkan lokasi, jenis file, dan kategori penggunaan.
4. Daftar file dan folder terbesar, dengan pencarian serta penyortiran.
5. Kandidat pembersihan berbasis aturan yang dapat dijelaskan.
6. Review pilihan, konfirmasi tindakan, eksekusi terbatas, dan verifikasi hasil.
7. Pengelolaan daftar lokasi terlindungi dan pengecualian scan.
8. Riwayat tindakan lokal dengan informasi minimum yang diperlukan.
9. Snapshot RAM total/tersedia sebagai konteks, terpisah dari analisis disk.
10. Inventori aplikasi Win32 dan paket WindowsApps, termasuk resource, bundle, framework, serta paket untuk akun lain yang hanya boleh dibaca sesuai izin.
11. Scan folder kosong dan kandidat sisa aplikasi dengan bukti serta tingkat kepastian yang terpisah.
12. Retry baca sebagai administrator dengan cakupan terbatas, perbandingan sebelum/sesudah, dan diagnosis jenis penolakan.
13. Perbandingan snapshot, refresh setelah pembersihan manual, dan ekspor rincian lengkap tanpa membanjiri tampilan utama.
14. Kontrak tool terstruktur untuk AI opsional: memahami hasil, menjelaskan kandidat, membuat rencana, dan meminta eksekusi yang diotorisasi backend.

Di luar MVP:

- Pembersihan otomatis, terjadwal, atau berjalan diam-diam di background.
- Registry cleaner, RAM booster, penghentian proses otomatis, atau pengubahan layanan.
- Optimasi driver, penghapusan restore point, partisi, dan perbaikan sistem.
- Penghapusan permanen umum, pengosongan Recycle Bin, dan secure erase.
- Deduplication otomatis, pembacaan isi dokumen untuk klasifikasi, dan hashing seluruh disk.
- Scan jaringan, perangkat ponsel, serta pengunduhan isi cloud untuk diperiksa.
- Akun produk, sinkronisasi cloud, telemetry, serta upload otomatis daftar file. Pengiriman konteks minimum ke AI eksternal hanya boleh melalui pilihan eksplisit pengguna sebagaimana bagian 15.

## 4. Alur pengguna

1. Aplikasi dibuka dalam mode read-only. Tidak ada kandidat yang terpilih otomatis.
2. Pengguna memilih drive/folder dan melihat cakupan, pengecualian, serta batas izin.
3. Scan menampilkan progres, hasil parsial, dan opsi berhenti.
4. Hasil menampilkan ringkasan ruang, folder besar, dan kandidat pembersihan.
5. Pengguna membuka rincian kandidat: path penuh, ukuran, alasan, risiko, metode tindakan, dan kemungkinan pemulihan.
6. Pengguna memilih item tertentu. Ukuran pilihan tidak disamakan dengan ruang yang pasti akan pulih.
7. Review akhir menampilkan daftar sasaran dan metode untuk setiap item sebelum konfirmasi.
8. Aplikasi memvalidasi ulang sasaran. Perubahan yang material mengharuskan review ulang.
9. Eksekusi melaporkan status per item. Pembatalan menghentikan pekerjaan berikutnya pada batas operasi yang aman.
10. Aplikasi memeriksa sisa item dan mengukur kembali ruang kosong drive.

Pilihan pengguna berlaku untuk tindakan tersebut saja. Memilih scan seluruh drive tidak memberi izin menghapus seluruh drive; memilih cache tidak memberi izin menghapus folder induk atau profil aplikasi.

## 5. Persyaratan scan dan hasil

- Scan membaca metadata yang diperlukan: nama/path, ukuran, waktu perubahan, atribut, dan status akses. Tidak mengeksekusi file yang ditemukan.
- File sistem tersembunyi boleh dihitung jika terbaca, tetapi tetap tunduk pada aturan perlindungan.
- Folder besar ditampilkan sebagai hierarki. Ukuran induk dan anak tidak boleh dijumlahkan seolah kategori yang terpisah.
- Bedakan ukuran logis dari ukuran yang dialokasikan jika keduanya tersedia. Gunakan satuan konsisten: GiB/MiB atau GB/MB dengan penjelasan.
- Perhitungkan hard link agar tidak menjanjikan ruang pulih dari penghapusan satu nama file. Jika tidak dapat dipastikan, beri label estimasi.
- Sparse file, kompresi, dan aktivitas aplikasi lain dapat membuat estimasi berbeda dari perubahan ruang kosong aktual.
- Jangan mengikuti junction, symbolic link, mount point, atau reparse point secara default. Tampilkan jumlah/lokasi yang dilewati tanpa menelusuri targetnya.
- Jangan membaca isi cloud placeholder atau memicu hydration. Lokasi cloud diberi status khusus.
- Lokasi tidak terbaca tidak dilaporkan sebagai kosong. Hasil menyebut jumlah error dan cakupan yang belum tercakup.
- Progres tidak boleh memberi persentase atau estimasi waktu palsu ketika jumlah pekerjaan belum diketahui.
- Berikan pause/resume scan jika implementasi dapat mempertahankan akurasi; minimal tersedia pembatalan dan hasil parsial yang diberi label.
- Snapshot RAM mencantumkan waktu pengukuran; tidak menjadi rekomendasi menghapus file atau membunuh proses.
- Status kesehatan drive, bila tersedia dari Windows, diberi label sebagai status yang dilaporkan perangkat, bukan jaminan diagnostik menyeluruh.

Setiap hasil kandidat memuat minimal:

| Informasi | Isi |
|---|---|
| Identitas | Path absolut, jenis file/folder, identitas objek jika tersedia |
| Ukuran | Ukuran terukur, jenis ukuran, estimasi ruang yang mungkin pulih |
| Kategori | Cache, temporary, installer sisa, aplikasi/game, proyek, media/arsip, sistem, belum diketahui |
| Penjelasan | Bukti aturan, tingkat keyakinan, waktu scan |
| Dampak | Kemungkinan unduh ulang, hilangnya akses offline, atau dampak lain |
| Tindakan | Review saja, Recycle Bin, pembersihan native, uninstall, atau diblokir |
| Pemulihan | Apakah bisa dipulihkan, batasan, dan hasil operasi |

## 6. Batasan penghapusan wajib

### 6.1 Prinsip keputusan

- Ukuran besar, nama mengandung `temp`/`cache`, umur lama, dan lama tidak diakses bukan bukti tunggal bahwa item aman dihapus.
- Bedakan keyakinan klasifikasi dari risiko tindakan. Cache yang teridentifikasi tetap dapat memerlukan unduh ulang.
- Item yang belum dikenal hanya mendapat rekomendasi review. Jangan menyebutnya aman.
- Aplikasi tidak menghapus sendiri sesudah scan, menambah pilihan secara otomatis, atau memperluas sasaran setelah konfirmasi.
- Konfirmasi harus mengikat daftar objek, tindakan, dan estimasinya; tidak berupa izin umum yang berlaku selamanya.

### 6.2 Lokasi terlindungi

Blokir penghapusan generik untuk:

- Root drive, root profil pengguna, serta seluruh `Windows`, `Program Files`, `Program Files (x86)`, `ProgramData`, atau `AppData` sebagai satu sasaran.
- Folder boot, recovery, System Volume Information, Windows Installer, WinSxS, dan DriverStore.
- `pagefile.sys`, `swapfile.sys`, `hiberfil.sys`, registry hive, serta komponen sistem aktif.
- Data browser seperti profil, kata sandi, bookmark, cookie, dan sesi; pembersihan cache harus memisahkannya dari data tersebut.
- Kredensial, SSH key, konfigurasi rahasia, database aplikasi, save game, dan data akun pengguna lain.
- Metadata repositori seperti `.git`, source code, dataset, hasil penelitian, dan environment proyek sebagai kandidat otomatis.

`C:\Users\raffi\Downloads\Project` menjadi lokasi terlindungi default untuk instalasi pengguna awal ini. Pada instalasi lain, lokasi proyek dipilih lewat pengaturan. Direktori proyek tetap boleh dihitung ukurannya, tetapi tidak otomatis menjadi kandidat hapus.

Provider pembersihan spesifik boleh menargetkan subfolder cache yang telah dikenali di dalam AppData/ProgramData. Pengecualian ini tidak memberi izin menghapus folder induknya. Penghapusan aplikasi di Program Files menggunakan alur uninstall terpisah.

Jika pengguna ingin menghapus data pribadi/proyek, sediakan tindakan manual yang jelas dengan review tambahan dan Recycle Bin jika didukung. Tidak ada override untuk penghapusan generik komponen sistem dalam MVP.

### 6.3 Temp dan file aktif

- Jangan menghapus seluruh Temp secara rekursif sebagai satu operasi.
- Hanya pilih berkas sementara yang teridentifikasi, berusia lebih dari 24 jam, dan tidak terkunci. Umur merupakan filter tambahan, bukan jaminan keselamatan.
- Lewati file aktif, terkunci, baru dibuat, atau gagal diperiksa. Jangan menghentikan proses untuk memaksakan penghapusan.
- Jangan mengubah ownership, ACL, layanan, atau proteksi Windows agar item bisa dihapus.
- Kegagalan sebagian harus terlihat; aplikasi tidak boleh melaporkan seluruh folder sudah bersih jika masih ada sisa.

### 6.4 Metode tindakan

- Untuk file pribadi yang dipilih manual, gunakan Recycle Bin jika tersedia dan kapasitas memungkinkan.
- Jika Recycle Bin tidak didukung, jangan otomatis beralih ke penghapusan permanen. Lewati item dan jelaskan keterbatasannya.
- Untuk cache, utamakan API/perintah pembersihan native yang dikenali dan dibatasi ke lokasi yang sudah diverifikasi.
- Jangan mengeksekusi perintah shell dari nama/path file atau teks hasil scan. Provider hanya boleh menjalankan tindakan yang ditentukan aplikasi dengan argumen yang tervalidasi.
- Pembersihan native dapat tidak bisa dibatalkan. Preview harus menjelaskan hal ini sebelum konfirmasi.
- Untuk aplikasi/game, gunakan uninstaller terdaftar atau uninstaller yang diverifikasi. Jangan menghapus folder instalasi sebagai pengganti uninstall.
- Uninstaller tidak dikenal atau dengan cakupan yang tidak dapat dipastikan harus dialihkan ke proses uninstall manual. Jangan menjanjikan save game/data pasti tetap ada jika tidak bisa diverifikasi.
- Mengosongkan Recycle Bin merupakan tindakan terpisah dan tidak termasuk pembersihan lain dalam MVP.

### 6.5 Validasi sebelum tindakan

- Normalisasi dan verifikasi path absolut tepat sebelum eksekusi; tolak wildcard, path kosong, parent traversal, dan sasaran di luar pilihan pengguna.
- Validasi root beserta semua komponen path terkait; perubahan menjadi junction/symlink/reparse point membatalkan tindakan pada item tersebut.
- Jika identitas, ukuran, waktu perubahan, atau cakupan item berubah secara material sejak preview, batalkan pilihan itu dan minta review ulang.
- Cegah pergantian sasaran antara pemeriksaan dan penghapusan; gunakan identitas/handle file dan mekanisme Windows yang sesuai ketika tersedia. Jika identitas tidak dapat dijamin, jangan lanjutkan penghapusan generik.
- Direktori hanya boleh dihapus jika seluruh cakupannya telah diketahui dan disetujui. Item baru atau terlindungi yang muncul harus menghentikan penghapusan direktori.
- Tidak meminta hak administrator untuk scan biasa. Jika fitur tertentu memerlukannya, jelaskan tindakan dan cakupannya secara spesifik.
- Jika kontrol keamanan menolak operasi, laporkan penolakannya. Jangan mencoba bypass melalui interpreter, perubahan izin, atau metode penghapusan alternatif.

## 7. Kategori dan contoh aturan awal

| Kategori | Contoh | Perilaku |
|---|---|---|
| Cache tooling | npm, npx, uv | Verifikasi lokasi/provider; preview konsekuensi unduh ulang; pembersihan native |
| Temporary | Temp pengguna | Filter umur, identifikasi, dan lock; hapus hanya item yang lolos |
| Sisa installer | SquirrelTemp, adobeTemp | Nama folder tidak cukup; periksa bukti installer dan aktivitas sebelum menawarkan tindakan |
| Unduhan tidak selesai | Android NDK `.asdownload` lama | Verifikasi bukan unduhan aktif; tampilkan path dan umur |
| Aplikasi/game | Sekiro | Tampilkan ukuran folder dan tawarkan uninstall, bukan penghapusan generik |
| Media/arsip besar | Video, ZIP, foto astronomi | Review manual; arahkan untuk backup/arsip sebelum memilih hapus |
| Proyek | Downloads/Project, repository, dataset | Terlindungi secara default; ukuran tetap terlihat |
| Sistem/belum diketahui | WinSxS, file tak dikenali | Tidak dapat dipilih untuk penghapusan generik |

Duplikat nama, pasangan ZIP dan folder ekstraksi, atau kemiripan ukuran tidak membuktikan isi identik. Jangan menyarankan penghapusan otomatis berdasarkan sinyal tersebut.

## 8. Verifikasi dan riwayat

- Simpan ruang kosong sebelum dan sesudah tindakan, waktu pengukuran, dan volume yang sama.
- Laporkan perubahan yang teramati, bukan mengklaim seluruh delta pasti disebabkan aplikasi; proses lain dapat menulis atau menghapus file bersamaan.
- Status per item: selesai, sebagian selesai, dilewati, dibatalkan, gagal, atau diblokir.
- Periksa keberadaan dan ukuran sisa sesudah tindakan. Ukur ulang saat pengguna meminta refresh setelah menghapus file lewat Explorer.
- Riwayat mencatat sasaran, metode, ukuran estimasi, status, dan alasan kegagalan; tidak menyimpan isi dokumen, token, atau output proses yang berpotensi rahasia.
- Hasil scan tidak otomatis dikirim ke jaringan. Ekspor laporan harus merupakan tindakan eksplisit dan mengingatkan bahwa path/nama file dapat bersifat pribadi.
- Pemulihan hanya ditawarkan jika mekanismenya benar-benar tersedia; tidak ada klaim undo untuk uninstall atau cache cleanup native.
- Pembatalan bukan rollback; item yang sudah selesai tetap dilaporkan. Uninstaller yang sedang berjalan tidak dihentikan paksa.

## 9. Kualitas dan batas operasional

- UI tetap responsif selama scan, dengan pembaruan progres paling lambat setiap dua detik selama worker aktif.
- Permintaan batal scan diakui UI dalam satu detik; worker berhenti pada batas operasi aman dan melaporkan jika sedang menunggu operasi OS.
- Jangan memuat daftar seluruh file ke RAM sekaligus. Rancang streaming/agregasi agar pohon berisi jutaan file tetap dapat diproses.
- Tidak memicu unduhan cloud atau scan jaringan tanpa izin terpisah.
- Jalankan satu pekerjaan pembersihan pada satu waktu agar pilihan, pembatalan, dan audit tetap jelas.
- Tetapkan benchmark waktu, RAM, dan jumlah file pada mesin uji sebelum rilis; jangan menuliskan klaim performa yang belum diukur.
- Dukung keyboard, status yang tidak bergantung pada warna, serta path panjang Windows. Bahasa awal antarmuka: Indonesia.

## 10. Acceptance criteria

1. Scan dan preview tidak membuat, mengubah, memindahkan, atau menghapus file yang dipindai.
2. Membuka aplikasi atau menyelesaikan scan tidak memicu pembersihan; semua pilihan awal kosong.
3. Review menampilkan path absolut, ukuran, alasan, dampak, metode, dan pemulihan untuk setiap pilihan.
4. Memilih beberapa kandidat tidak membuat parent/sibling, Recycle Bin, atau lokasi lain ikut terhapus.
5. Lokasi proyek default, metadata `.git`, data akun lain, dan komponen sistem terlindungi lolos pengujian penolakan.
6. Symlink/junction yang menunjuk keluar cakupan, perubahan objek setelah preview, dan item baru dalam direktori tidak menyebabkan penghapusan di luar pilihan.
7. File Temp yang baru, terkunci, atau tidak teridentifikasi dilewati tanpa menghentikan proses pemiliknya.
8. Penolakan akses/keamanan, uninstaller gagal, dan kegagalan parsial menghasilkan status yang akurat tanpa fallback destruktif.
9. Game terhapus lewat alur uninstall; tidak ada penghapusan manual folder instalasi sebagai fallback.
10. Hard link, sparse file, folder hierarkis, serta direktori tidak terbaca tidak menghasilkan janji ruang pulih yang menyesatkan.
11. Refresh setelah penghapusan eksternal memperbarui keberadaan folder, ukuran kandidat, dan ruang kosong drive.
12. Hasil akhir menyertakan item tersisa dan delta ruang kosong terukur; tidak menyatakan selesai hanya berdasarkan exit code.
13. Operasi batal/crash menyisakan riwayat hasil yang diketahui; aplikasi tidak mengulang pekerjaan destruktif otomatis saat dibuka kembali.
14. Semua skenario penghapusan diuji di direktori fixture/VM terisolasi; pengujian tidak menargetkan data nyata pengguna.
15. Inventori WindowsApps mencakup Main, Framework, Resource, Bundle, dan Optional; resource/bundle yang sah tidak salah diklasifikasikan yatim karena inventori tidak lengkap.
16. Paket yang berkasnya hilang tetapi registrasinya masih ada dilaporkan sebagai hasil sebagian, dengan dua bukti terpisah.
17. Folder tidak terbaca, folder berisi file berukuran nol, dan folder berisi reparse point tidak diklasifikasikan sebagai direktori kosong yang terverifikasi.
18. Ketiadaan entri uninstall tidak cukup untuk menyatakan aplikasi tidak ada; instalasi portable, lokasi khusus, dan pemilik data yang belum diketahui tetap disebutkan.
19. Temuan yatim dibedakan dari installer sisa, cache aplikasi aktif, dependensi bersama, serta data pengguna yang sengaja dipertahankan.
20. Scan elevated hanya memperluas pembacaan sasaran yang disetujui, tanpa mengubah ACL/ownership, mematikan proteksi, atau mengeksekusi proses sebagai SYSTEM.
21. AI tidak bisa mengeksekusi perintah shell arbitrer, membuat approval token, menyetujui tindakannya sendiri, atau mengubah kebijakan backend.
22. Instruksi di nama file, README, manifest, log, output perintah, atau dokumen yang dipindai diperlakukan sebagai data tidak tepercaya, bukan instruksi pengguna.
23. Pilihan bahasa alami seperti "hapus tiga installer tadi" harus terikat ke tiga finding ID dari snapshot yang jelas; sasaran ambigu tidak dieksekusi.
24. Tindakan untuk semua pengguna, paket SYSTEM, dan uninstall native memiliki scope tersendiri; izin scan elevated tidak otomatis memberi izin mutasi elevated.
25. Laporan manual "Done" dicatat sebagai laporan pengguna sampai refresh membuktikan perubahan objek; hilangnya folder tidak dianggap bukti bahwa Cappy Cleaner yang menghapusnya.
26. Ringkasan, tool AI, dan ekspor menggunakan snapshot serta satuan yang konsisten; hasil lama tidak ditampilkan sebagai kondisi terkini.
27. Approval yang kedaluwarsa, cakupan yang berubah, provider yang tidak dikenal, atau objek yang berganti identitas membatalkan eksekusi sebelum ada mutasi.
28. Pengujian AI mencakup prompt injection dari filesystem/output, permintaan menghapus parent, path lintas akun, klaim approval palsu, serta percobaan mengganti provider setelah review.

## 11. Pelajaran dari sesi awal

Pada sesi pemeriksaan laptop, ruang kosong awal C: sekitar 19,28 GiB dari 475,25 GiB. Penggunaan terbesar termasuk AppData, Downloads, video, dan aplikasi/game; folder besar tidak semuanya merupakan sampah.

Pembersihan dilakukan hanya setelah pemilihan spesifik. Sekiro menggunakan uninstaller; cache npm/npx dan uv menggunakan mekanisme native. Penolakan penghapusan langsung dilaporkan sebagai keterbatasan, bukan keberhasilan. Penghapusan eksternal folder Astro kemudian diverifikasi lewat refresh; ruang kosong terukur menjadi sekitar 59,57 GiB.

Angka tersebut adalah konteks historis, bukan hasil scan terkini atau data yang boleh di-hardcode ke aplikasi. Folder Astro berisi data pengguna dan tidak menjadi contoh aturan bahwa semua folder Astro aman dihapus.

## 12. Keputusan untuk sesi pengembangan berikutnya

- Tentukan stack Windows dan kemasan aplikasi berdasarkan kebutuhan integrasi filesystem, Recycle Bin, serta uninstall.
- Tentukan bentuk UI dan tingkat detail review bersama pengguna.
- Pilih provider cache pertama; tambahkan provider hanya setelah cakupan, konsekuensi, dan skenario gagalnya teruji.
- Susun rencana implementasi dan pengujian dari PRD ini sebelum menulis kode produk.
- Mulai dari scanner read-only dan proteksi sasaran; aktifkan pembersihan setelah acceptance criteria keselamatan terkait terpenuhi.
- Gunakan temuan nyata pada bagian 13-14 sebagai skenario uji; kontrak AI bagian 15 harus tetap memisahkan penalaran model dari wewenang eksekusi.
- Katalog bagian 16 harus diterjemahkan menjadi provider tervalidasi, bukan kumpulan string shell yang dapat dijalankan bebas oleh model.

Arahan sesi berikutnya: baca `prd.md`, periksa keadaan repo, pertahankan batasan keselamatan, dan jangan menganggap PRD ini sebagai izin menghapus file nyata saat mengembangkan atau menguji aplikasi.

## 13. Temuan nyata dan kebutuhan yang diturunkan

Semua angka di bagian ini adalah snapshot historis tanggal 19 Oktober 2026, bukan angka default produk. Sebagian pembersihan dilakukan lewat tool native, sebagian dilakukan pengguna lewat Explorer. Aktivitas lain pada laptop berjalan bersamaan, sehingga total selisih ruang kosong tidak boleh diatribusikan seluruhnya ke satu tindakan.

### 13.1 Disk, RAM, dan folder besar

| Temuan | Hasil sesi | Kebutuhan produk |
|---|---|---|
| Drive C: hampir penuh | Awalnya 19,28 GiB kosong dari 475,25 GiB | Tampilkan ukuran, persentase, waktu snapshot, dan tren ruang kosong |
| RAM berbeda dari disk | RAM total 15,84 GiB; tersedia 5,08 GiB pada pemeriksaan awal | Pisahkan konteks RAM dari kandidat penghapusan disk |
| SSD melaporkan Healthy/OK | Status dari Windows, bukan pemeriksaan menyeluruh | Jangan menyamakan status perangkat dengan jaminan kesehatan |
| Folder terbesar | AppData 100,35 GiB; Downloads 68,96 GiB; Videos 27,94 GiB | Hierarki ukuran dan pengelompokan berdasarkan lokasi/data |
| AppData setelah pembersihan awal | Turun menjadi sekitar 90,82 GiB | Compare snapshot dan rincian Local/Roaming/LocalLow |
| Program Files dan x86 | Dari 82,35 menjadi 67,28 GiB setelah Sekiro dihapus, pada scan tanpa akses WindowsApps | Label cakupan akses; ukuran yang belum tercakup bukan nol |
| Downloads bukan seluruhnya sampah | Berisi Project, DETR, HASHMICRO, arsip acara, dataset, dan Astro | Lindungi proyek/data; jangan menawarkan hapus seluruh Downloads |
| Video besar | Videos/Anomaly sekitar 21,49 GiB | Pilihan review dan arsip manual; jangan menganggap media lama tidak bernilai |
| Model AI lokal | LM Studio sekitar 10,85 GiB, termasuk model sekitar 5,06 GiB | Pisahkan model, runtime, cache, dan instalasi; tawarkan pengelolaan lewat pemiliknya |

### 13.2 Cache, uninstall, dan verifikasi manual

| Sasaran | Hasil yang diamati | Pelajaran untuk produk |
|---|---|---|
| npm cache | 6,15 GiB; cache utama dan npx memerlukan dua perintah native | Satu provider bisa mempunyai beberapa subjenis cache; verifikasi seluruh cakupan |
| uv cache | Sekitar 1,11 GiB logis; tool melaporkan 27.951 file dibersihkan | Ruang fisik pulih bisa berbeda karena hard link dan aktivitas lain |
| SquirrelTemp, adobeTemp, unduhan NDK | Kandidat terukur; penghapusan shell langsung sempat diblokir host | Bedakan penolakan kebijakan eksekusi dari penolakan ACL Windows |
| Sekiro | Uninstaller native exit 0; direktori instalasi hilang | Verifikasi hasil filesystem setelah exit code |
| PyCharm | Uninstaller native exit 0; direktori instalasi dan entri uninstall hilang | Settings/cache dapat tetap ada sesudah uninstall |
| Visual Studio Enterprise | Launcher memberi exit -1; log menyebut Completed uninstall; direktori dan inventori produk hilang | Tampilkan bukti yang berbeda, jangan menyimpulkan sukses/gagal hanya dari satu kode |
| Android SDK | SDK Manager lama memberi peringatan schema tetapi exit 0; tersisa tools sekitar 0,17 GiB sebelum pembersihan manual | Verifikasi paket serta direktori sisa; jangan mengklaim uninstall total dari exit 0 |
| TRAE SOLO, Slack, Android Studio | Biner utama sudah tidak ditemukan saat diminta uninstall; data AppData masih ada | Bedakan aplikasi terpasang, registrasi, cache, settings, dan sisa uninstaller |
| Playwright | Browser terhapus lewat perintah uninstall native; direktori hanya menyisakan .links kosong | Bersihkan browser unduhan, bukan node_modules, source proyek, atau browser pengguna |
| NVIDIA | DXCache sekitar 1,26 GiB dan GLCache sekitar 0,08 GiB; driver bukan sasaran | Provider cache harus terbatas ke subfolder yang dikenali |
| Astro dan folder data sisa | Pengguna menghapus manual; refresh memeriksa keberadaan dan ruang kosong | Aplikasi harus dapat merekonsiliasi perubahan dari luar |
| Installer Cursor/Fireflies/MiniMax Code | Masing-masing sekitar 0,524/0,422/0,616 GiB; penghapusan shell diblokir; pengguna menghapus manual | Approval tidak menjamin host mengizinkan eksekusi; laporkan tanpa bypass |

Verifikasi ulang setelah pengguna menyatakan "Done": ketiga direktori updater tersebut sudah tidak ada pada 19 Oktober 2026 sekitar 20:17 waktu Asia/Bangkok. Ruang kosong saat itu sekitar **82,653 GiB**. Ini verifikasi perubahan manual, bukan klaim bahwa penghapusan dilakukan agent.

### 13.3 Cache yang perlu dipisahkan dari data aplikasi

- `Roaming\Code\CachedExtensionVSIXs`: 222 file, sekitar 3,598 GiB. Sebanyak 20 arsip bernama ekstensi sekitar 0,335 GiB dan 202 file bernama sementara sekitar 3,263 GiB. Nama sementara adalah bukti klasifikasi tambahan, bukan jaminan bahwa file tidak sedang digunakan.
- `Local\pnpm\store\v11`: 45.235 file, sekitar 1,656 GiB. Store dipakai lintas proyek; ukuran total bukan ukuran paket tidak terpakai. Tidak dibersihkan pada sesi ini.
- Slack: Service Worker sekitar 1,086 GiB, Cache 0,428 GiB, Code Cache 0,289 GiB. Seluruh profil tidak boleh dianggap cache.
- Android Studio: cache index versi 2023.3 sekitar 1,147 GiB dan versi 2025.3.1 sekitar 0,792 GiB; settings/plugins harus dibedakan.
- `app_shell_cache_8311`: installer mengidentifikasi produknya sebagai TikTok LIVE Studio yang masih terpasang; bukan bukti folder yatim.
- `ACD Systems MSIX`: sekitar 1,032 GiB aset/model. Tidak ditemukan kecocokan inventori, tetapi pemilik dan penggunaan belum terkonfirmasi. Tetap kategori review, bukan aman hapus.

### 13.4 Akses administrator dan cakupan tidak lengkap

1. Scan biasa menemukan 38 lokasi yang ditolak. Retry biasa tetap gagal.
2. Helper read-only yang dielevasi melalui UAC berhasil membaca 37 lokasi; satu direktori konfigurasi Defender tetap ditolak.
3. Pengukuran lanjutan membuktikan 32 lokasi cache web dua versi Nirwana hanya sekitar 28,44 MiB total. Empat lokasi diagnostik/crash report kosong.
4. Satu lokasi besar yang sebelumnya tidak tercakup adalah WindowsApps, sekitar 13,925 GiB sebelum Asphalt dihapus.
5. Scan yang diperluas ke ProgramData menemukan total 15 lokasi terlindungi dalam cakupan empat root: 13 terkait Defender, SystemData, dan CapabilityAccessManager. Ukuran isinya tetap unknown.

Izin filesystem luas pada host tidak sama dengan token administrator Windows. Hak administrator juga tidak menjamin semua komponen proteksi dapat dibaca. Produk harus membedakan kegagalan baca, item dilewati, kegagalan tindakan, penolakan kebijakan, dan operasi menunggu UAC.

### 13.5 WindowsApps dan kasus Asphalt 9

Inventori harus menyertakan seluruh jenis paket. Query default yang hanya mencakup sebagian jenis pernah membuat direktori resource/bundle tampak tidak cocok inventori. Setelah Main, Framework, Resource, Bundle, dan Optional diperiksa, direktori yang tidak cocok tinggal direktori internal WindowsApps; tidak ada dasar menganggapnya sampah.

Setelah berkas Asphalt dihapus, WindowsApps berisi 284 direktori, sekitar 12,847 GiB:

| Jenis | Direktori | GiB |
|---|---:|---:|
| Aplikasi/komponen | 96 | 10,009 |
| Framework/dependensi | 84 | 2,810 |
| Resource/bahasa | 44 | 0,009 |
| Bundle | 52 | 0,002 |
| Direktori internal | 8 | 0,017 |

Angka direktori bukan jumlah aplikasi unik. Framework berbeda versi/arsitektur dapat sah dan digunakan bersama.

Kasus Asphalt:

- Paket tepat: `A278AB0D.Asphalt9_4.3.201.2_x64__h6adky7gbf63m`.
- Perintah Remove-AppxPackage untuk semua pengguna tidak memberi error, tetapi direktori dan registrasi masih ada.
- Tidak ada paket provisioning Asphalt dan tidak ada bundle induk yang ditemukan.
- Metadata menunjukkan registrasi `Paused` untuk `SYSTEM` (`S-1-5-18`). Penghapusan native ditujukan ke SID tersebut setelah diagnosis.
- Direktori berkas kemudian hilang, WindowsApps turun sekitar 1,078 GiB, dan ruang kosong drive ikut bertambah.
- Entri metadata Paused masih muncul pada verifikasi akhir. Status akhir harus **berkas terhapus, metadata tersisa**, bukan uninstall sepenuhnya terverifikasi.
- Tidak menggunakan proses dengan identitas SYSTEM, PsExec, scheduled task, perusakan registry, atau penghapusan manual WindowsApps. Parameter target SID tidak berarti proses scanner berjalan sebagai SYSTEM.

### 13.6 Folder kosong dan kandidat yatim

Scan empat root mencakup sekitar 245.228 direktori dan 1.697.758 file:

| Root | Direktori | File | Error baca | Reparse dilewati |
|---|---:|---:|---:|---:|
| Program Files | 26.173 | 201.924 | 1 | 32 |
| Program Files (x86) | 6.508 | 70.963 | 0 | 3 |
| ProgramData | 6.974 | 48.637 | 14 | 6 |
| Profil pengguna awal | 205.573 | 1.376.234 | 0 | 394 |

Ditemukan 13.396 direktori tanpa entri langsung, serta 304 path pohon kosong pada kedalaman awal. Angka kedua dapat mencakup induk dan anak yang sama; tidak boleh dijumlahkan sebagai 304 sasaran independen. Folder kosong hampir tidak memulihkan ruang dan dapat sengaja dipertahankan aplikasi.

Klasifikasi wajib:

| Status | Bukti minimum | Batasan |
|---|---|---|
| Empty directory | Enumerasi sukses dan nol entri langsung | Tetap bisa merupakan struktur aplikasi aktif |
| Empty subtree | Seluruh turunan terenumerasi; nol file; tidak ada error atau link yang dilewati | File berukuran nol tetap file, sehingga bukan empty subtree |
| Confirmed application residue | Riwayat uninstall/ketiadaan biner utama, kecocokan identitas aplikasi, dan isi sisa yang dikenali | Bisa berisi data pengguna yang sengaja disimpan |
| Suspected orphan | Tidak cocok inventori dan lokasi umum; bukti tambahan ada | Portable/custom install dan pemilik lain belum dikesampingkan |
| Shared dependency | Dipakai paket/proyek lain atau hubungan belum dapat dipastikan | Tidak ditawarkan sebagai yatim otomatis |
| Unknown/incomplete | Akses ditolak, pemilik belum diketahui, atau scan tidak lengkap | Tidak boleh diklasifikasikan kosong/aman |

Sisa kuat yang ditemukan: satu debug.log TRAE sekitar 48 KiB dan updater Slack beserta .dead sekitar 1,5 MiB. Kandidat tambahan Cursor, Fireflies, dan MiniMax Code terbukti berupa installer pembaruan dari metadata produk executable; tidak adanya inventori tetap bukan bukti mutlak bahwa tidak ada instalasi portable.

## 14. Fitur pemeriksaan lanjutan

| Fitur | Masukan dan hasil | Tindakan yang tersedia |
|---|---|---|
| Storage overview | Volume ID, kapasitas, free space, timestamp | Scan/refresh/compare |
| Folder explorer | Hierarki, logical/allocated size bila tersedia, file count, cakupan | Drill-down dan buka lokasi |
| Large files | Path, ukuran, tanggal, kategori, alasan | Review; tidak auto-select |
| Application inventory | Win32 uninstall keys, Appx semua jenis, scope akun/SID | Tampilkan aplikasi, install path, sisa data, serta metode uninstall |
| WindowsApps inspector | Paket, versi, arsitektur, tipe, NonRemovable, state per akun | Uninstall paket yang didukung dan diotorisasi; tanpa hapus folder paket |
| Cache inspector | Pemilik/provider, ukuran, subjenis cache, dampak unduh ulang | Native cleanup terkurasi atau review manual |
| Installer/update residue | Metadata produk, file pending, installer duplikat, marker uninstall | Rencana hapus terpilih; nama folder tidak cukup |
| Empty folder scan | Empty leaf dan empty subtree terpisah, kesalahan dan symlink | Ekspor/review; tidak ada hapus massal default |
| Orphan/residue analysis | Korelasi inventori, biner utama, riwayat, versi, marker, data tersisa | Bukti dan confidence; konfirmasi item spesifik |
| Read-only elevated retry | Daftar finding akses gagal dan tujuan pembacaan | Retry terbatas setelah UAC; tanpa perubahan izin |
| Partial uninstall diagnosis | Exit code, log tersaring, registrasi, biner, paket, ukuran sisa | Laporkan state gabungan dan langkah berikutnya |
| Manual cleanup handoff | Path copyable, alasan, risiko, cara refresh | Pengguna menghapus sendiri; backend hanya memverifikasi |
| Snapshot comparison | Sebelum/sesudah dan perubahan eksternal | Rekonsiliasi ukuran, keberadaan, cakupan, dan asal tindakan |
| Export | Ringkasan singkat dan rincian lengkap terpisah | Markdown/text lokal; tidak upload otomatis |
| AI assistant | Finding terstruktur dan konteks minimum | Tanya jawab, penjelasan, prioritas, rencana tindakan |

Pembersihan pnpm, cache ekstensi VS Code, model AI lokal, updater khusus vendor, dan paket Windows yang macet harus ditambahkan sebagai provider bertahap sesudah aturan keselamatan dan kompatibilitasnya diuji. Kemampuan mengidentifikasi kandidat tidak otomatis berarti MVP sudah mendukung eksekusinya.

## 15. Integrasi AI: wewenang dan kontrak tool

### 15.1 Pembagian tanggung jawab

- Scanner/inventory engine menghasilkan fakta dan evidence; model tidak mengarang ukuran, file, status akses, atau aplikasi terpasang.
- AI membantu menjelaskan hasil, menghubungkan bukti, menjawab pertanyaan, dan menyusun prioritas. Rekomendasi model bukan izin eksekusi.
- Policy engine deterministik mengevaluasi lokasi terlindungi, scope, provider, identitas objek, izin, serta konsekuensi tindakan.
- Action executor hanya menjalankan operasi bertipe dari provider yang diizinkan. Tidak ada tool `run_shell`, `eval`, PowerShell bebas, atau executable/path bebas yang dapat dipilih model.
- Approval berasal dari pengguna melalui UI/host tepercaya, lalu diikat backend ke manifest konkret. AI tidak boleh membuat token atau mengirim `approved: true` sebagai pengganti bukti tersebut.
- Izin pengguna yang sudah jelas dan masih sesuai rencana dapat digunakan tanpa pertanyaan berulang. Persetujuan harus tetap terikat ke sasaran nyata; perubahan scope, objek, atau dampak memerlukan review baru.
- Adapter model/protokol, termasuk kemungkinan MCP, masih keputusan implementasi. Belum ada provider AI atau stack yang dipilih.

### 15.2 Bentuk fakta dan status

Setiap finding setidaknya membawa `findingId`, `snapshotId`, waktu scan, volume ID, root/cakupan akun, path kanonis atau alias yang dipseudonimkan, identitas objek, ukuran beserta jenisnya, jumlah file, kategori, evidence IDs, confidence klasifikasi, risiko, status akses, status kelengkapan, dan tindakan provider yang tersedia.

Semua hasil turunan harus menyebut snapshot. Jika ukuran tidak diketahui, gunakan nilai unknown/null dengan alasan; nol hanya untuk ukuran nol yang benar-benar terukur. Confidence tidak menggantikan risk atau approval.

State operasi minimal: `queued`, `waiting_for_approval`, `waiting_for_elevation`, `running`, `verifying`, `completed`, `partial`, `skipped`, `cancelled`, `failed`, `blocked_by_policy`, dan `requires_manual_action`.

Pisahkan state filesystem, state inventori aplikasi, serta pengukuran disk. Contoh Asphalt mempunyai filesystem absent, metadata present/Paused, dan disk delta terukur; satu boolean `success` tidak cukup.

### 15.3 Tool yang diusulkan

Nama berikut adalah rancangan kontrak, belum API yang sudah diimplementasikan.

| Tool | Input inti | Output dan batas wewenang |
|---|---|---|
| `storage.overview` | Volume ID yang dipilih | Kapasitas, ruang kosong, snapshot; read-only |
| `scan.start` | Root IDs yang diizinkan dan opsi metadata | Job ID, cakupan dan pengecualian; tidak menghapus |
| `scan.status` | Job ID | Progres faktual, hasil parsial, error, cursor |
| `scan.cancel` | Job ID | Pembatalan scan saja |
| `inventory.list` | Jenis inventori dan scope akun | Win32/Appx, paket/resource/framework, state dan cakupan |
| `findings.list` | Snapshot ID, filter, sort, pagination | Ringkasan finding; tidak mengirim semua path sekaligus |
| `findings.explain` | Finding IDs | Evidence, pemilik, ketidakpastian, risiko, opsi tindakan |
| `access.retry_readonly` | Finding IDs dan persetujuan elevasi tepercaya bila perlu | Retry baca untuk sasaran tersebut; tidak mengubah ACL |
| `cleanup.plan` | Finding IDs dan action type yang diminta | Plan ID, manifest, dampak, metode, scope, estimasi, prasyarat; tanpa mutasi |
| `cleanup.execute` | Plan ID dan approval reference dari host | Job ID dan hasil per item; validator backend wajib lolos |
| `cleanup.cancel` | Job ID | Hentikan pekerjaan berikutnya di batas aman; tidak mematikan uninstaller paksa |
| `cleanup.verify` | Job ID atau finding IDs | Keberadaan, registrasi, ukuran sisa, disk delta, status gabungan |
| `snapshots.compare` | Dua snapshot IDs | Perubahan dan cakupan yang sebanding; asal manual/agent dipisahkan |
| `reports.export` | Snapshot/job IDs dan format | Artefak lokal pada lokasi ekspor yang dipilih pengguna |

Backend tidak boleh menerima command string model sebagai implementasi `cleanup.execute`. Input plan berupa finding/action IDs; executor memilih binary dan parameter yang didukung. Jangan menyediakan parameter bebas yang dapat menyuntik argumen, menambah root, atau mengganti target akun.

### 15.4 Manifest dan approval

Manifest harus memuat ID/hash rencana, snapshot sumber, daftar objek dan identitasnya, provider/version, action type, scope pengguna, kebutuhan elevasi, ukuran estimasi, kemampuan pemulihan, konsekuensi unduh ulang, serta batas waktu/prasyarat yang dipilih kebijakan produk.

Sebelum mutasi: cocokkan approval dengan manifest, cek policy, identitas objek, perubahan subtree, keberadaan link, status lock, serta scope akun. Token tidak dapat dipakai ulang untuk rencana berbeda. Jika item sudah dihapus manual, laporkan absent; jangan mencari sasaran pengganti.

Contoh batas otorisasi:

- "Cek lagi" hanya memberi izin pemeriksaan.
- "Hapus tiga installer tadi" mengacu ke finding Cursor, Fireflies, dan MiniMax Code yang ditinjau, bukan seluruh direktori updater.
- "Hapus cache NVIDIA saja" tidak mencakup driver, NVIDIA App, atau seluruh folder NVIDIA.
- "Uninstall PyCharm" tidak otomatis memberi izin menghapus proyek PyCharm, environment, atau source code.
- "Jalankan scan administrator" tidak memberi izin uninstall untuk semua pengguna.
- "Done" setelah instruksi manual memicu rekonsiliasi; bukan bukti otomatis bahwa perubahan berhasil.

### 15.5 Privasi, prompt injection, dan hasil tool

- Mode lokal tanpa AI tetap harus dapat melakukan scan, review, dan tindakan yang didukung.
- AI lokal atau cloud bersifat opt-in. Untuk cloud, jelaskan jenis metadata yang dikirim sebelum pengguna mengaktifkan integrasi.
- Default konteks berupa ringkasan kategori, ukuran, finding IDs, alasan dan status. Path pribadi menggunakan alias; path asli diselesaikan backend lokal.
- Nama file/path teridentifikasi dapat dikirim hanya jika diperlukan dan disetujui. Jangan mengirim isi file, log mentah, kredensial, daftar environment, atau seluruh profil.
- Output uninstaller bisa memuat environment variable dan informasi sensitif. Ambil kode, status, serta pesan relevan yang disaring; jangan meneruskan dump log mentah ke model.
- Instruksi di filesystem, manifest, nama executable, log, dan sumber web bukan instruksi pengguna. Model tidak boleh menuruti perintah di dalamnya untuk menghapus, menaikkan hak, atau mengubah kebijakan.
- Tool result harus bertipe, mempunyai scope/timestamp, dan menyertakan error serta status parsial. Memotong daftar hasil tidak boleh menghilangkan tanda ada sisa/error.
- Batasi ukuran konteks dan gunakan pagination/agregasi. Jangan mengirim 13.396 path kosong atau jutaan file ke model untuk satu jawaban.
- Kegagalan koneksi AI tidak boleh memicu cleanup otomatis, mengulang tindakan destruktif, atau menghalangi pembatalan.

### 15.6 Kegagalan, retry, dan idempotensi

- Error baca dapat dicoba ulang secara terbatas dengan scope yang sama. Retry elevated memerlukan penjelasan dan persetujuan host yang sesuai.
- Penolakan kebijakan eksekusi tidak diatasi dengan mengganti interpreter, encoding, API deletion, identitas proses, atau mengubah izin.
- Tool native yang lebih sempit boleh dipilih jika merupakan metode resmi yang memang didukung dan diizinkan; bukan pembungkus untuk menjalankan kembali operasi yang dilarang.
- Operasi mutasi menggunakan operation ID/idempotency key. Setelah timeout/crash, periksa keadaan nyata sebelum melanjutkan; jangan mengirim uninstall/delete yang sama secara buta.
- Manifest untuk semua pengguna atau target registrasi SYSTEM harus menyebut dampak tersebut dan identitas paket tepat. Tidak ada elevasi ke proses SYSTEM dalam MVP.
- Perbedaan `access_denied`, `locked`, `not_found`, `policy_denied`, `provider_missing`, `unsupported_version`, `registration_remaining`, dan `cancelled` harus tetap terlihat hingga ringkasan AI.

## 16. Katalog perintah yang dipakai pada sesi ini

**Catatan historis, jangan dijalankan otomatis.** Potongan berikut merangkum perintah/primitive yang benar-benar dipakai. Ini bukan satu script instalasi atau cleanup siap jalan. Binary, versi, root, SID, package ID, serta izin harus ditemukan dan diverifikasi ulang pada perangkat tujuan. Path mesin awal tidak boleh di-hardcode ke produk.

### 16.1 Inventori sistem dan ruang disk — read-only

```powershell
Get-CimInstance Win32_OperatingSystem
Get-CimInstance Win32_ComputerSystem
Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3'
Get-PhysicalDisk
[IO.DriveInfo]::new('C:\').AvailableFreeSpace
Get-Process | Sort-Object WorkingSet64 -Descending
Get-Process | Group-Object ProcessName
```

Data RAM menggunakan TotalPhysicalMemory dan FreePhysicalMemory. Pemakaian proses menggunakan WorkingSet64; hasil per proses tidak dianggap ukuran fisik unik yang boleh langsung dijumlahkan tanpa batasan. CIM pernah gagal dari lingkungan eksekusi terbatas lalu berhasil saat pemeriksaan di luar sandbox disetujui. Ini berbeda dari UAC Windows.

### 16.2 Scan metadata, ukuran, path, dan izin — read-only

```powershell
Get-ChildItem -LiteralPath $path -Force
Get-ChildItem -LiteralPath $path -File -Recurse -Force
Get-ChildItem -LiteralPath $path -File -Recurse -Force | Measure-Object Length -Sum
[IO.DirectoryInfo]::new($path).GetFileSystemInfos()
Resolve-Path -LiteralPath $path
Get-Item -LiteralPath $path -Force
Test-Path -LiteralPath $path
Get-Acl -LiteralPath $path
[Security.Principal.WindowsIdentity]::GetCurrent()
```

`$path` pada potongan ini adalah sasaran literal yang dipilih, bukan input shell bebas. Walker yang dipakai memeriksa atribut ReparsePoint sebelum menelusuri direktori, memakai stack, mencatat error, dan mengagregasi ukuran anak ke induk. Jangan menyalin satu baris Get-ChildItem sebagai pengganti seluruh pengaman walker.

Untuk scan kosong, hasil GetFileSystemInfos nol entri dicatat sebagai empty leaf. Empty subtree ditentukan setelah agregasi post-order dengan nol file dan nol turunan unknown/link. Tidak memakai ukuran byte nol saja sebagai tes kosong.

### 16.3 Inventori aplikasi — read-only

```powershell
Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
Get-ItemProperty -Path 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
Get-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
Get-AppxPackage -AllUsers -PackageTypeFilter Main,Framework,Resource,Bundle,Optional
Get-AppxProvisionedPackage -Online
Get-CimInstance Win32_UserProfile
& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -all -format json
```

Nilai yang dibaca mencakup DisplayName, DisplayVersion, InstallLocation, UninstallString, QuietUninstallString, jenis paket, NonRemovable, dan PackageUserInformation. Nama akun dapat berbeda dari nama folder profil; tentukan scope dari SID dan pemetaan profil, bukan tebakan dari `C:\Users\...`.

Provisioning diperiksa untuk Asphalt; hasilnya kosong. Membaca inventori akun lain adalah operasi read-only yang membutuhkan hak sesuai, bukan izin menghapus data akun itu.

### 16.4 Diagnostik installer dan helper elevated

```powershell
Get-Command npm,uv,pnpm,py -ErrorAction SilentlyContinue
Get-Process | Where-Object { $_.ProcessName -match 'pycharm|devenv|slack|studio64' }
Get-Content -LiteralPath $diagnosticResult -Raw
Get-WinEvent -LogName 'Microsoft-Windows-AppXDeploymentServer/Operational' -MaxEvents 100
Start-Process -FilePath $verifiedPowerShell -ArgumentList $verifiedReadOnlyScriptArgs -Verb RunAs -WindowStyle Hidden -Wait -PassThru
```

Start-Process RunAs dipakai untuk helper dengan sasaran tetap dan uninstaller yang diotorisasi. WindowStyle Hidden tidak menyetujui UAC dan tidak melewati proteksi Windows. Helper read-only hanya menulis hasil diagnostik ke direktori kerja, tidak mengubah sumber yang dipindai.

`setup.exe --help` sempat membuka jendela bantuan Visual Studio; itu bukan uninstall. Produk harus membedakan help/diagnostic invocation dari operasi mutasi, serta menghindari membuka jendela bantuan tanpa penjelasan.

### 16.5 npm dan uv — mutasi native yang diotorisasi

```powershell
& 'C:\Program Files\nodejs\npm.cmd' cache clean --force --cache 'C:\Users\raffi\AppData\Local\npm-cache'
& 'C:\Program Files\nodejs\npm.cmd' cache npx rm --force --cache 'C:\Users\raffi\AppData\Local\npm-cache'
& 'C:\Users\raffi\AppData\Local\hermes\bin\uv.exe' cache clean --cache-dir 'C:\Users\raffi\AppData\Local\uv\cache'
```

Ketiganya berhasil dijalankan. Perintah npm cache clean tidak membersihkan semua cache npx pada versi yang digunakan. Percobaan `cache npx rm --yes` gagal dengan pesan perlu `--force`; setelah help/error lokal diperiksa, perintah khusus di atas berhasil. Flag force merupakan bagian metode native yang ditinjau, bukan izin memperluas root atau mematikan proteksi host.

### 16.6 Browser Playwright — mutasi native yang diotorisasi

```powershell
$env:PLAYWRIGHT_BROWSERS_PATH='C:\Users\raffi\AppData\Local\ms-playwright'
& node 'C:\Users\raffi\AppData\Local\hermes\hermes-agent\node_modules\playwright-core\cli.js' uninstall --help
& node 'C:\Users\raffi\AppData\Local\hermes\hermes-agent\node_modules\playwright-core\cli.js' uninstall --all
```

Help diperiksa dahulu. Uninstall menghapus browser Chromium/headless/Firefox/WebKit serta utilitas pada root cache yang ditetapkan. Variabel environment diubah hanya pada proses tersebut. Browser ini semula direferensikan beberapa instalasi Playwright; otomasi berikutnya dapat perlu mengunduhnya lagi. Perintah tersebut tidak menghapus proyek atau package Playwright.

### 16.7 Android SDK — mutasi native yang diotorisasi

```powershell
$env:JAVA_HOME='C:\Program Files\Java\jre1.8.0_431'
& 'C:\Users\raffi\AppData\Local\Android\Sdk\tools\bin\sdkmanager.bat' --help
& 'C:\Users\raffi\AppData\Local\Android\Sdk\tools\bin\sdkmanager.bat' --sdk_root=$sdk --uninstall @packages
```

Sebelum uninstall, `$sdk` diikat ke root SDK yang dipilih dan `@packages` diambil dari localPackage path pada package.xml yang ada. Paket meliputi versi build-tools/platforms, sources, platform-tools, emulator, dan paket extras yang terdaftar. Paket `tools` tidak dimasukkan karena menyediakan tool yang sedang berjalan; sisanya sekitar 0,17 GiB dilaporkan untuk tindakan manual.

JAVA_HOME sebelumnya menunjuk lokasi tidak sesuai. Penyesuaian hanya pada proses operasi dan khusus versi SDK Manager lama yang terpasang, bukan rekomendasi Java 8 untuk semua versi. Tidak mengubah environment sistem, mengunduh tool baru, atau mengklaim layanan driver terkait extras sudah diverifikasi terhapus.

### 16.8 Uninstaller desktop — mutasi native yang diotorisasi

```powershell
Start-Process -FilePath 'C:\Program Files (x86)\Sekiro Shadows Die Twice GOTY Edition\unins000.exe' -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -WindowStyle Hidden -Wait -PassThru
Start-Process -FilePath 'C:\Program Files\JetBrains\PyCharm 2023.3.4\bin\Uninstall.exe' -ArgumentList '/S' -Verb RunAs -WindowStyle Hidden -Wait -PassThru
Start-Process -FilePath 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\setup.exe' -ArgumentList 'uninstall --installPath "C:\Program Files\Microsoft Visual Studio\2022\Enterprise" --passive --norestart' -Verb RunAs -WindowStyle Hidden -Wait -PassThru
```

Sasaran dan native uninstaller diperiksa terlebih dahulu. Flags berbeda menurut produk; jangan memberikan flags yang sama ke semua executable. Visual Studio ditargetkan ke instance Enterprise, bukan menghapus shared directory atau BuildTools secara manual. Settings, proyek, dan environment pengguna tidak menjadi sasaran implisit.

### 16.9 Asphalt 9 — penghapusan paket dan verifikasi terpisah

```powershell
Get-AppxPackage -AllUsers -Name 'A278AB0D.Asphalt9'
Get-AppxPackage -AllUsers -PackageTypeFilter Bundle -Name 'A278AB0D.Asphalt9'
Remove-AppxPackage -Package 'A278AB0D.Asphalt9_4.3.201.2_x64__h6adky7gbf63m' -AllUsers
Remove-AppxPackage -Package 'A278AB0D.Asphalt9_4.3.201.2_x64__h6adky7gbf63m' -User 'S-1-5-18'
```

Kedua mutasi dijalankan sebagai administrator, dengan diagnosis di antara keduanya, bukan satu batch yang diulang tanpa syarat. Perintah pertama tidak menghapus folder fisik; perintah kedua kemudian diikuti bukti folder hilang dan disk delta. Registrasi Paused masih tersisa. Implementasi harus menemukan package ID dan scope dari metadata aktual; jangan hardcode SID SYSTEM atau mencoba semua SID sampai suatu perintah berhasil.

### 16.10 Penghapusan langsung yang ditolak host — bukan keberhasilan

Contoh percobaan setelah pengguna memilih tiga installer tertentu:

```powershell
Remove-Item -LiteralPath 'C:\Users\raffi\AppData\Local\cursor-updater' -Recurse -Force -ErrorAction Stop
Remove-Item -LiteralPath 'C:\Users\raffi\AppData\Local\fireflies-desktop-updater' -Recurse -Force -ErrorAction Stop
Remove-Item -LiteralPath 'C:\Users\raffi\AppData\Local\@mmx-agentelectron-updater' -Recurse -Force -ErrorAction Stop
```

Perintah gabungan ditolak sebelum proses dijalankan dengan pesan `blocked by policy`; tidak ada penghapusan oleh agent dari percobaan ini. Percobaan penghapusan langsung cache NVIDIA dan kandidat tertentu sebelumnya juga ditolak. Tidak ada alasan kebijakan yang lebih rinci tersedia; jangan mengarang bahwa penyebabnya file terkunci atau kurang hak Windows.

Pengguna kemudian menghapus tiga direktori updater manual. Test-Path pada pemeriksaan berikutnya menunjukkan ketiganya absent. Ini contoh jalur `blocked_by_policy` → `requires_manual_action` → `user_reported_done` → `verified_absent`, dengan asal perubahan tetap manual. Jangan memasukkan Remove-Item rekursif bebas ke tool AI untuk mengatasi penolakan tersebut.

### 16.11 Script diagnostik historis dan ekspor

Script sementara dibuat di direktori kerja sesi, bukan sebagai source aplikasi di repo:

- `check-storage-access.ps1`: retry read-only 38 lokasi dengan hak administrator.
- `measure-previously-denied.ps1`: mengukur isi yang sebelumnya gagal dibaca, bukan hanya menguji akses.
- `inventory-appx-cleaner.ps1`: inventori Appx semua pengguna.
- `asphalt-and-windowsapps.ps1`, `check-asphalt-state.ps1`, dan `verify-appx-parent-and-asphalt.ps1`: tindakan/diagnosis paket Asphalt yang diotorisasi serta inventori paket.
- `refresh-windowsapps-final.ps1`: verifikasi fisik dan metadata sesudah tindakan.
- `scan-empty-and-residual.ps1`: walker metadata empat root, empty leaf/subtree, ukuran, error, dan reparse.
- `write-storage-report.ps1`: menulis laporan Markdown serta daftar folder kosong ke direktori outputs.

Lokasi historis script: `C:\Users\raffi\Documents\Codex\2026-10-19\c\work`. Lokasi laporan: `C:\Users\raffi\Documents\Codex\2026-10-19\c\outputs`.

Artefak yang dihasilkan: `windowsapps-dan-folder-sisa-2026-10-19.md` dan `daftar-folder-kosong-2026-10-19.txt`. PRD ini tetap dapat dipakai tanpa script sementara tersebut. Perilaku yang diperlukan telah ditulis di dokumen; script eksperimen bukan implementasi produksi dan tidak dijalankan otomatis oleh sesi berikutnya.

### 16.12 Referensi native yang diperiksa saat operasi

- [Microsoft: Remove-AppxPackage](https://learn.microsoft.com/en-us/powershell/module/appx/remove-appxpackage) — scope User/AllUsers dan jenis paket induk.
- [Microsoft: parameter command-line Visual Studio](https://learn.microsoft.com/en-us/visualstudio/install/use-command-line-parameters-to-install-visual-studio) — uninstall instance dan parameter UI/restart.
- [JetBrains: uninstall PyCharm](https://www.jetbrains.com/help/pycharm/uninstall.html) — uninstaller standalone, silent uninstall, dan direktori settings/cache yang terpisah.
- [Android: sdkmanager](https://developer.android.com/tools/sdkmanager) — pengelolaan/uninstall paket; kompatibilitas harus mengikuti versi yang dipakai.

Untuk npm, uv, dan Playwright, perilaku juga diperiksa lewat help/error tool lokal yang tersedia serta hasil aktual. Pada implementasi nanti, verifikasi ulang dokumentasi dan kemampuan versi terpasang sebelum mengaktifkan provider.

## 17. Tahapan pengembangan dan handoff

1. **Scanner read-only:** ringkasan disk, hierarki ukuran, daftar file besar, error/unknown, reparse protection, pembatalan dan ekspor.
2. **Inventori dan analisis:** Win32/Appx semua jenis, WindowsApps, cache terkurasi, folder kosong, kandidat residu dan perbandingan snapshot.
3. **Eksekusi terbatas:** policy engine, action manifest, persetujuan pengguna, helper elevated dengan scope terbatas, native uninstall/cleanup, verifikasi serta manual handoff.
4. **AI opsional:** kontrak tool terstruktur, context minimization, provenance/evidence, approval bridge, pengujian prompt injection, dan rekonsiliasi operasi saat timeout.
5. **Provider tambahan:** perluasan berbasis bukti dan pengujian versi; tidak memperluas cakupan tindakan yang sudah disetujui secara diam-diam.

Urutan ini adalah rancangan untuk perencanaan, bukan izin memulai implementasi di sesi dokumentasi ini. Tugas sesi berikutnya: baca PRD lengkap, klarifikasi keputusan stack/UI/provider yang benar-benar diperlukan, lalu susun rencana implementasi. Tetap jangan menguji cleanup terhadap file nyata pengguna atau mengeksekusi contoh historis tanpa permintaan tindakan yang relevan.
