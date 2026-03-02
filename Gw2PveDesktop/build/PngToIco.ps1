# Generates a single-resolution .ico from a PNG for use as the application icon.
param([string]$PngPath, [string]$IcoPath)

Add-Type -AssemblyName System.Drawing

$null = Add-Type -MemberDefinition @'
[DllImport("user32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
public static extern bool DestroyIcon(IntPtr hIcon);
'@ -Name NativeMethods -Namespace PngToIco

$bmp = $null
try {
    $bmp = [System.Drawing.Bitmap]::FromFile($PngPath)
    $hIcon = $bmp.GetHicon()
    try {
        $icon = [System.Drawing.Icon]::FromHandle($hIcon).Clone()
        try {
            $dir = [System.IO.Path]::GetDirectoryName($IcoPath)
            if (-not [string]::IsNullOrEmpty($dir) -and -not (Test-Path $dir)) {
                New-Item -ItemType Directory -Path $dir -Force | Out-Null
            }
            $fs = [System.IO.File]::Create($IcoPath)
            try {
                $icon.Save($fs)
            } finally {
                $fs.Close()
            }
        } finally {
            $icon.Dispose()
        }
    } finally {
        [PngToIco.NativeMethods]::DestroyIcon($hIcon) | Out-Null
    }
} finally {
    if ($bmp) { $bmp.Dispose() }
}
