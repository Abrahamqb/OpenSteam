if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host " [!] ERROR: Please run PowerShell as ADMINISTRATOR." -ForegroundColor Red
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    exit
}

$steamRegistry = Get-ItemProperty -Path "HKCU:\Software\Valve\Steam" -ErrorAction SilentlyContinue
if ($null -ne $steamRegistry -and $null -ne $steamRegistry.SteamPath) {
    $steamPath = $steamRegistry.SteamPath.Replace("/", "\")
}
else {
    $steamPath = "C:\Program Files (x86)\Steam"
}

$asciiArt = @'
  ____                      ____  _                         ____ _     ___ 
 / __ \                    / ___|| |_ ___  __ _ _ __ ___   / ___| |   |_ _|
| |  | |_ __   ___ _ __   \___ \| __/ _ \/ _` | '_ ` _ \  | |   | |    | | 
| |__| | '_ \ / _ \ '_ \   ___) | ||  __/ (_| | | | | | | | |___| |___ | | 
 \____/| .__/ \___|_| |_| |____/ \__\___|\__,_|_| |_| |_|  \____|_____|___|
       |_|
    Github: Abrahamqb
    Twitter: TheJbrequi                                                                  
'@

Clear-Host
Write-Host $asciiArt -ForegroundColor Cyan
Write-Host " =====================================================================" -ForegroundColor Gray

$tempFolder = $env:TEMP
$desktopPath = [System.Environment]::GetFolderPath("Desktop")

if (Test-Path $steamPath) {
    Write-Host " Steam is installed in the default location: $steamPath." -ForegroundColor Blue
}
else {
    Write-Host " Steam is not installed in the default location: $steamPath" -ForegroundColor Red
}
Write-Host " =====================================================================" -ForegroundColor Gray

Start-Sleep -Seconds 2

#1
function PatchSteam {
    Clear-Host
    Write-Host " --- Patch Steam --- " -ForegroundColor Cyan
    Write-Host "Steam is installed in the default location: $steamPath." -ForegroundColor Blue
    Write-Host "Patching Steam, please wait a few seconds..." -ForegroundColor Gray
    Invoke-WebRequest -Uri "https://github.com/Abrahamqb/OpenSteam/raw/refs/heads/master/Resources/xinput1_4.dll" -OutFile "$steamPath\xinput1_4.dll"
    #Invoke-WebRequest -Uri "https://github.com/Abrahamqb/OpenSteam/raw/refs/heads/master/Resources/hid.dll" -OutFile "$steamPath\hid.dll"
    Write-Host "Steam patched successfully!" -ForegroundColor Green
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
}

#2
function DeletePatchSteam {
    Clear-Host
    Write-Host " --- Delete Patch Steam --- " -ForegroundColor Cyan
    Write-Host "Steam is installed in the default location: $steamPath." -ForegroundColor Blue
    Write-Host "Deleting Path Steam, please wait a few seconds..." -ForegroundColor Red
    Remove-Item "$steamPath\xinput1_4.dll" -Recurse -Force
    #Remove-Item "$steamPath\hid.dll" -Recurse -Force
    Write-Host "Path Steam deleted successfully!" -ForegroundColor Green
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
}

#3
function SearchGame {
    Clear-Host
    Write-Host " --- Search & Load Game Lua (KernelOS) --- " -ForegroundColor Cyan

    $ID = Read-Host " Enter the Game ID (e.g., 12345)"
    if ([string]::IsNullOrWhiteSpace($ID)) { return }

    $checkUrl = "https://kernelos.org/games/download.php?gen=1&id=$ID"
    $luaPathSteam = Join-Path $steamPath "config\stplug-in"
    $tempZip = Join-Path $env:TEMP "Lua_$ID.zip"
    $extractPath = Join-Path $env:TEMP "Extract_$ID"

    try {
        Write-Host " Connecting to KernelOS API..." -ForegroundColor Gray
        $response = Invoke-RestMethod -Uri $checkUrl -Headers @{"User-Agent"="OpenSteam-Manager/1.0"}
        if ($null -eq $response.url) {
            Write-Host " Error: Game ID not found or invalid response." -ForegroundColor Red
            pause
            return
        }
        $fullLink = "https://kernelos.org" + $response.url
        Write-Host " Downloading Lua" -ForegroundColor Blue
        Invoke-WebRequest -Uri $fullLink -OutFile $tempZip -ErrorAction Stop
        if (-not (Test-Path $luaPathSteam)) {
            New-Item -ItemType Directory -Path $luaPathSteam -Force | Out-Null
        }
        if (Test-Path $extractPath) { Remove-Item $extractPath -Recurse -Force }
        Write-Host " Extracting files..." -ForegroundColor Gray
        Expand-Archive -Path $tempZip -DestinationPath $extractPath -Force
        $luaFile = Get-ChildItem -Path $extractPath -Filter "*.lua" -Recurse | Select-Object -First 1
        if ($null -ne $luaFile) {
            $finalLuaFile = Join-Path $luaPathSteam "$ID.lua"
            Move-Item -Path $luaFile.FullName -Destination $finalLuaFile -Force
            Write-Host " Script $ID.lua successfully loaded!" -ForegroundColor Green
        }
        else {
            Write-Host " Error: No .lua file found inside the ZIP." -ForegroundColor Red
        }
    }
    catch {
        Write-Host " Something went wrong: $($_.Exception.Message)" -ForegroundColor Red
    }
    finally {
        if (Test-Path $tempZip) { Remove-Item $tempZip -Force }
        if (Test-Path $extractPath) { Remove-Item $extractPath -Recurse -Force }
        Write-Host " Press any key to return to menu..." -ForegroundColor Gray
        $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    }
}

#4
function InstallMillennium {
    Clear-Host
    write-Host " --- Install Millennium (Plugin Loader) --- " -ForegroundColor Cyan
    Write-Host "Installing Millennium..." -ForegroundColor Gray
    Invoke-WebRequest -Uri "https://github.com/SteamClientHomebrew/Installer/releases/latest/download/MillenniumInstaller-Windows.exe" -OutFile "$tempFolder\Millennium.exe"
    Start-Process "$tempFolder\Millennium.exe" -Wait
    if (Test-Path "$steamPath\millennium.dll") {
        Write-Host "Millennium installed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Installation finished. Please verify millennium.dll in Steam folder." -ForegroundColor Yellow
    }
    Remove-Item "$tempFolder\Millennium.exe" -ErrorAction SilentlyContinue
    pause
}

#5
function DownloadKernelLua {
    Clear-Host
    write-Host " --- Download KernelLua (Plugin / KernelOS.org) --- " -ForegroundColor Cyan
    if (-not (Test-Path "$steamPath\plugins")) { New-Item -ItemType Directory -Path "$steamPath\plugins" | Out-Null }
    Write-Host "Downloading KernelLua..." -ForegroundColor Gray
    Invoke-WebRequest -Uri "https://github.com/Abrahamqb/OpenSteam/raw/refs/heads/master/Resources/KernelLua.zip" -OutFile "$tempFolder\KernelLua.zip"
    Write-Host "Extracting to plugins folder..." -ForegroundColor Green
    Expand-Archive -Path "$tempFolder\KernelLua.zip" -DestinationPath "$steamPath\plugins\" -Force
    Write-Host "Done! Remember to activate it in Millennium settings." -ForegroundColor Yellow
    pause
}

#6
function InstallOpenSteamDesktop {
    Clear-Host
    Write-Host " --- Install OpenSteam.exe to Desktop --- " -ForegroundColor Cyan
    Write-Host "Downloading on OpenSteam to desktop..." -ForegroundColor Blue
    Invoke-WebRequest -Uri "https://github.com/Abrahamqb/OpenSteam/releases/latest/download/OpenSteam.exe" -OutFile "$desktopPath\OpenSteam.exe"
    Write-Host "Executable created successfully!" -ForegroundColor Green
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    Start-Sleep -Seconds 2
}

#7
function RestartSteam {
    Clear-Host
    Write-Host " --- Restart Steam --- " -ForegroundColor Cyan
    Write-Host "Restarting Steam..." -ForegroundColor Gray
    Get-Process -Name "Steam" -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process -Name "steamwebhelper" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 2
    Start-Process "$steamPath\Steam.exe"
    Write-Host "Steam restarted successfully!" -ForegroundColor Green
    Start-Sleep -Seconds 2
}

while ($true) {
    Clear-Host
    Write-Host $asciiArt -ForegroundColor Cyan

    Write-Host " =====================================================================" -ForegroundColor Gray
    $patchStatus = if (Test-Path "$steamPath\xinput1_4.dll") { "[PATCHED]" } else { "[NOT PATCHED]" }
    $statusColor = if ($patchStatus -eq "[PATCHED]") { "Green" } else { "Red" }
    Write-Host " 1. Patch Steam $patchStatus" -ForegroundColor $statusColor
    Write-Host " 2. Remove Patch" -ForegroundColor Red
    Write-Host " 3. Search Game" -ForegroundColor Blue
    Write-Host " 4. Install Millennium (Plugin Loader)" -ForegroundColor Yellow
    Write-Host " 5. Download KernelLua (Plugin)" -ForegroundColor Yellow
    Write-Host " 6. Download OpenSteam.exe to Desktop" -ForegroundColor Blue
    Write-Host " 7. Restart Steam" -ForegroundColor Magenta
    Write-Host " 8. Exit" -ForegroundColor Red
    Write-Host " =====================================================================" -ForegroundColor Gray
    
    $choice = Read-Host " Enter your choice"
    switch ($choice) {
        "1" { PatchSteam }
        "2" { DeletePatchSteam }
        "3" { SearchGame }
        "4" { InstallMillennium }
        "5" { DownloadKernelLua }
        "6" { InstallOpenSteamDesktop }
        "7" { RestartSteam }
        "8" { exit }
        default { 
            Write-Host " Invalid choice!" -ForegroundColor Red
            Start-Sleep -Seconds 1 
        }
    }
}
