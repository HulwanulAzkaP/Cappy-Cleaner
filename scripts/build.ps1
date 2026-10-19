param([string]$Configuration = 'Release', [switch]$SkipInstaller)
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $projectRoot
try {
    dotnet test 'tests/CappyCleaner.Core.Tests' -c $Configuration -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'Tes gagal.' }
    dotnet build 'CappyCleaner.slnx' -c $Configuration -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'Build gagal.' }
    $publishDirectory = Join-Path $projectRoot 'artifacts/publish/win-x64'
    dotnet publish 'src/CappyCleaner.App' -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o $publishDirectory -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'Publish gagal.' }
    if (!$SkipInstaller) {
        $compilerCandidates = @(
            (Join-Path $env:LOCALAPPDATA 'Programs/Inno Setup 6/ISCC.exe'),
            (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe'),
            (Join-Path $env:ProgramFiles 'Inno Setup 6/ISCC.exe')
        )
        $compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
        if (!$compiler) { throw 'Inno Setup 6 tidak ditemukan. Instal compiler atau gunakan -SkipInstaller.' }
        & $compiler '/Qp' ('/DPublishDir=' + $publishDirectory) 'installer/CappyCleaner.iss'
        if ($LASTEXITCODE -ne 0) { throw 'Kompilasi installer gagal.' }
    }
    Write-Output "Publish: $publishDirectory"
    Write-Output 'Installer: artifacts/installer/CappyCleaner-Setup-0.1.0-x64.exe'
} finally { Pop-Location }
