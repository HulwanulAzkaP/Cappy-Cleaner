# Renders the actual XAML without launching the privileged app or scanning data.
# This validates static layout only, not event handlers or UAC behavior.
param([string]$OutputFile = '')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework,PresentationCore,WindowsBase,System.Xml.Linq
$projectRoot = Split-Path -Parent $PSScriptRoot
if (!$OutputFile) { $OutputFile = Join-Path $projectRoot 'artifacts/ui-preview.png' }
$application = [System.Windows.Application]::new()
[xml]$appXml = Get-Content -LiteralPath (Join-Path $projectRoot 'src/CappyCleaner.App/App.xaml') -Raw -Encoding UTF8
$dictionary = '<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">' + $appXml.Application.'Application.Resources'.InnerXml + '</ResourceDictionary>'
$application.Resources = [System.Windows.Markup.XamlReader]::Parse($dictionary)
[xml]$windowXml = Get-Content -LiteralPath (Join-Path $projectRoot 'src/CappyCleaner.App/MainWindow.xaml') -Raw -Encoding UTF8
$events = @('Closing','Click','SelectionChanged','KeyDown','MouseDoubleClick')
foreach ($node in $windowXml.SelectNodes('//*')) {
    foreach ($attribute in @($node.Attributes)) {
        if (($attribute.LocalName -eq 'Class') -or ($events -contains $attribute.LocalName)) { [void]$node.RemoveAttributeNode($attribute) }
    }
}
$window = [System.Windows.Markup.XamlReader]::Parse($windowXml.OuterXml)
$window.FindName('AccountText').Text = 'Pratinjau tata letak'
$window.FindName('DiskText').Text = 'Belum diukur - pilih drive pada aplikasi'
$window.FindName('RamText').Text = 'Belum diukur'
$content = $window.Content
$window.Content = $null
$content.Background = $window.Background
[System.Windows.Documents.TextElement]::SetForeground($content,$window.Foreground)
[System.Windows.Documents.TextElement]::SetFontFamily($content,$window.FontFamily)
[System.Windows.Documents.TextElement]::SetFontSize($content,$window.FontSize)
$content.Width = 1480; $content.Height = 900
$content.Measure([System.Windows.Size]::new(1480,900))
$content.Arrange([System.Windows.Rect]::new(0,0,1480,900))
$content.UpdateLayout()
$bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(1480,900,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
$bitmap.Render($content)
$encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
$encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
$stream = [IO.File]::Create($OutputFile)
try { $encoder.Save($stream) } finally { $stream.Dispose() }
$application.Shutdown()
Write-Output $OutputFile

