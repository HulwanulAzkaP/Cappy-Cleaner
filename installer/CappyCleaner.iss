#ifndef AppVersion
  #define AppVersion "0.1.1"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish\win-x64"
#endif

[Setup]
AppId={{98739124-63AF-485F-98DC-772327812AC2}
AppName=Cappy Cleaner
AppVersion={#AppVersion}
AppPublisher=Cappy Cleaner
DefaultDirName={autopf}\Cappy Cleaner
DefaultGroupName=Cappy Cleaner
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=CappyCleaner-Setup-{#AppVersion}-x64
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
PrivilegesRequired=admin
MinVersion=10.0.22000
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\CappyCleaner.exe
CloseApplications=yes
RestartApplications=no
SetupLogging=yes

[Tasks]
Name: "desktopicon"; Description: "Buat shortcut di Desktop"; GroupDescription: "Shortcut:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Cappy Cleaner"; Filename: "{app}\CappyCleaner.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Cappy Cleaner"; Filename: "{app}\CappyCleaner.exe"; Tasks: desktopicon; WorkingDir: "{app}"

; Tidak menjalankan aplikasi otomatis setelah setup agar tidak mewarisi
; akun administrator lain yang mungkin dipakai untuk memasang aplikasi.
; Data pengguna di LocalAppData dipertahankan saat uninstall.
