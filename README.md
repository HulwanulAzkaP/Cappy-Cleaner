# Cappy Cleaner

Aplikasi desktop native Windows 11 x64, C# / WPF / .NET 10. Meminta administrator melalui UAC saat diluncurkan. Installer memasang program ke Program Files; runtime .NET ikut dibundel.

## Status aktual

Versi 0.1 adalah **scanner preview**, belum MVP penuh PRD. Tersedia scan metadata streaming ke SQLite, hasil parsial, pembatalan, penelusuran folder, file/folder terbesar, pencarian per halaman, detail akses/link yang dilewati, ringkasan disk dan RAM terpisah, pengaturan perlindungan/pengecualian, riwayat snapshot dan ekspor lengkap Markdown/text.

Belum tersedia eksekusi pembersihan, inventori aplikasi/Appx, analisis cache/residu lengkap, perbandingan snapshot, ukuran alokasi/file identity dan AI. Policy eksekusi saat ini menolak semua penghapusan. Build ini tidak menghapus file.

## Build

Prasyarat developer: Windows, .NET SDK 10.0.113 (patch kompatibel lewat global.json), Inno Setup 6 untuk installer.

```powershell
& .\scripts\build.ps1
```

Jika kebijakan mesin tidak mengizinkan script, jangan mengubah proteksi: jalankan perintah dotnet/test/publish dan ISCC secara langsung. Opsi `-SkipInstaller` membangun folder aplikasi saja. Tidak perlu SDK/runtime di komputer pengguna untuk paket self-contained.

Artefak lokal:

- `artifacts/publish/win-x64/CappyCleaner.exe` bersama seluruh DLL/runtime dalam folder tersebut.
- `artifacts/installer/CappyCleaner-Setup-0.1.1-x64.exe`.

Jangan menyalin EXE aplikasi sendirian; dependensinya harus ikut. Paket belum ditandatangani dengan sertifikat publisher. UAC/install/upgrade/uninstall pada VM bersih perlu pengujian rilis tersendiri.

## Penggunaan

Pasang installer, buka shortcut, setujui UAC, pilih folder atau isi path absolut drive lokal lalu Mulai scan. Tidak ada scan otomatis. Klik dua kali folder pada hasil untuk menelusuri metadata snapshot. Riwayat adalah kondisi lama; scan ulang untuk keadaan terbaru.

Administrator tidak menjamin semua lokasi terbaca. Junction/symlink/reparse dan placeholder cloud dilewati; sumber tidak diubah. Ukuran adalah logis, bukan janji ruang pulih. Induk mencakup anak; jangan menjumlahkan ukuran tabel folder terbesar. Scanner tidak membaca konten dokumen.

Data lokal: `%LocalAppData%/CappyCleaner` milik **akun proses**. Jika UAC memakai kredensial akun admin lain, identitas yang tampil dan data lokal mengikuti akun tersebut; pilih root yang dimaksud secara eksplisit. Data ini dikecualikan dari scan dan dipertahankan saat uninstall. Retensi otomatis belum diterapkan; snapshot dapat membesar. Ekspor memuat nama/path pribadi dan memerlukan persetujuan pengguna.

## Tes dan kelanjutan

```powershell
dotnet test tests/CappyCleaner.Core.Tests -v quiet
dotnet build CappyCleaner.slnx -c Release -v quiet
```

Tes membuat fixture temporary sendiri, termasuk junction, dan tidak menjalankan cleanup pada data nyata. Rencana detail, keputusan, bukti pengujian dan langkah lanjut ada di `todo.md`; spesifikasi sumber di `prd.md` dan dokumen rancangan di `docs/superpowers/specs/`.
