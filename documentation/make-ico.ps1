Add-Type -AssemblyName System.Drawing

$src = [System.Drawing.Image]::FromFile("$PSScriptRoot\100words-logo.png")
$sizes = @(16, 24, 32, 48, 64, 128, 256)
$pngBytesList = New-Object System.Collections.ArrayList

foreach ($s in $sizes) {
    # Create square transparent canvas, draw source centred with aspect ratio preserved
    $bmp = New-Object System.Drawing.Bitmap($s, $s, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $scaleX = $s / $src.Width
    $scaleY = $s / $src.Height
    $scale  = [Math]::Min($scaleX, $scaleY)
    $drawW  = [int]($src.Width  * $scale)
    $drawH  = [int]($src.Height * $scale)
    $offX   = [int](($s - $drawW) / 2)
    $offY   = [int](($s - $drawH) / 2)

    $g.DrawImage($src, $offX, $offY, $drawW, $drawH)
    $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    [void]$pngBytesList.Add($ms.ToArray())
    $bmp.Dispose()
}
$src.Dispose()

$outMs = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($outMs)

$bw.Write([int16]0)
$bw.Write([int16]1)
$bw.Write([int16]$sizes.Count)

$offset = 6 + $sizes.Count * 16
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s   = $sizes[$i]
    $b   = $pngBytesList[$i]
    $dim = if ($s -ge 256) { 0 } else { $s }
    $bw.Write([byte]$dim)
    $bw.Write([byte]$dim)
    $bw.Write([byte]0)
    $bw.Write([byte]0)
    $bw.Write([int16]1)
    $bw.Write([int16]32)
    $bw.Write([int]$b.Length)
    $bw.Write([int]$offset)
    $offset += $b.Length
}

foreach ($b in $pngBytesList) { $bw.Write($b) }
$bw.Flush()

$outPath = "$PSScriptRoot\..\100words\Assets\app.ico"
$tmpPath = "$env:TEMP\app_new.ico"
[System.IO.File]::WriteAllBytes($tmpPath, $outMs.ToArray())
Copy-Item $tmpPath $outPath -Force
Write-Host "Done: $((Get-Item $outPath).Length) bytes -> $outPath"
