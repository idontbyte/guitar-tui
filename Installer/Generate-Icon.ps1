param(
    [string]$OutputPath = (Join-Path $PSScriptRoot "assets/GuitarTui.ico")
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$assetDir = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $assetDir | Out-Null

function New-IconBitmap([int]$Size) {
    $bitmap = New-Object System.Drawing.Bitmap $Size, $Size
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::FromArgb(20, 24, 32))

    $margin = [int]($Size * 0.08)
    $rect = New-Object System.Drawing.Rectangle $margin, $margin, ($Size - ($margin * 2)), ($Size - ($margin * 2))
    $terminalBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(35, 42, 54))
    $borderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(110, 231, 183)), ([Math]::Max(2, $Size * 0.025))
    $graphics.FillRectangle($terminalBrush, $rect)
    $graphics.DrawRectangle($borderPen, $rect)

    $promptPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(250, 204, 21)), ([Math]::Max(3, $Size * 0.035))
    $promptY = [int]($Size * 0.32)
    $graphics.DrawLine($promptPen, [int]($Size * 0.22), $promptY, [int]($Size * 0.32), [int]($Size * 0.40))
    $graphics.DrawLine($promptPen, [int]($Size * 0.22), [int]($Size * 0.48), [int]($Size * 0.32), [int]($Size * 0.40))
    $graphics.DrawLine($promptPen, [int]($Size * 0.38), [int]($Size * 0.49), [int]($Size * 0.55), [int]($Size * 0.49))

    $guitarPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(96, 165, 250)), ([Math]::Max(3, $Size * 0.035))
    $bodyRect = New-Object System.Drawing.Rectangle ([int]($Size * 0.36)), ([int]($Size * 0.56)), ([int]($Size * 0.22)), ([int]($Size * 0.22))
    $graphics.DrawEllipse($guitarPen, $bodyRect)
    $graphics.DrawLine($guitarPen, [int]($Size * 0.54), [int]($Size * 0.58), [int]($Size * 0.78), [int]($Size * 0.34))
    $graphics.DrawLine($guitarPen, [int]($Size * 0.60), [int]($Size * 0.64), [int]($Size * 0.84), [int]($Size * 0.40))
    $graphics.DrawEllipse($guitarPen, [int]($Size * 0.80), [int]($Size * 0.30), [int]($Size * 0.08), [int]($Size * 0.08))

    $graphics.Dispose()
    return $bitmap
}

$sizes = @(256, 64, 48, 32, 16)
$pngBuffers = @()
foreach ($size in $sizes) {
    $bitmap = New-IconBitmap $size
    $stream = New-Object System.IO.MemoryStream
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBuffers += ,$stream.ToArray()
    $stream.Dispose()
    $bitmap.Dispose()
}

$output = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter $output
$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$sizes.Count)

$imageOffset = 6 + (16 * $sizes.Count)
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $size = $sizes[$i]
    $bytes = $pngBuffers[$i]
    $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
    $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$bytes.Length)
    $writer.Write([UInt32]$imageOffset)
    $imageOffset += $bytes.Length
}

foreach ($bytes in $pngBuffers) {
    $writer.Write($bytes)
}

$writer.Flush()
[System.IO.File]::WriteAllBytes($OutputPath, $output.ToArray())
$writer.Dispose()
$output.Dispose()
