# Re-writes the FastTravel entry inside sandbox2.moddata
# with 9 pre-populated bunker destinations.
# Preserves every other entry in the zip.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$mdPath = "D:\Steam\steamapps\common\TheLongDark\Mods\ModData\sandbox2.moddata"
$entryName = "FastTravel"

# Helper to build one destination JSON object as a PowerShell hashtable.
function New-Destination {
    param(
        [string]$sceneName,
        [string]$displayName,
        [float]$x, [float]$y, [float]$z
    )
    [ordered]@{
        Region = [ordered]@{
            Id                   = $sceneName
            NameLocalizationId   = "SCENENAME_$sceneName"
            Name                 = $displayName
        }
        Scene = [ordered]@{
            Name       = $sceneName
            Guid       = "00000000000000000000000000000000"
            Path       = "Assets/Scenes/_Zones/$sceneName/$sceneName.unity"
            IsSubScene = $true
        }
        Position       = [ordered]@{ X = $x; Y = $y; Z = $z }
        CameraPitch    = 0.0
        CameraYaw      = 0.0
        LastTransition = [ordered]@{
            FromSceneId                    = $sceneName
            ToSceneId                      = $sceneName
            ToSpawnPoint                   = $null
            ToSpawnPointAudio              = $null
            RestorePlayerPosition          = $false
            LastOutdoorScene               = $sceneName
            LastOutdoorPosition            = [ordered]@{ X = $x; Y = $y; Z = $z }
            GameRandomSeed                 = 0
            ForceNextSceneLoadTriggerScene = $null
            SceneLocationLocIdOverride     = "SCENENAME_$sceneName"
            Location                       = $null
        }
    }
}

$dests = [ordered]@{
    "0" = New-Destination "LakeRegion"         "神秘湖地堡"    1029.06 91.99   -52.52
    "1" = New-Destination "RuralRegion"        "怡人山谷地堡"   423.89 177.93  1458.51
    "2" = New-Destination "MountainPassRegion" "林狼雪岭地堡"  1675.41 207.32   968.21
    "3" = New-Destination "MarshRegion"        "孤寂沼泽地堡"   593.07 -83.38  -104.89
    "4" = New-Destination "MountainTownRegion" "山间小镇地堡"  1828.20 444.39  1771.27
    "5" = New-Destination "RiverValleyRegion"  "寂静河谷地堡"   363.44 238.61   375.49
    "6" = New-Destination "CanneryRegion"      "荒凉水湾地堡"   328.37 344.50   833.16
    "7" = New-Destination "AshCanyonRegion"    "灰烬峡谷地堡"   -42.12 172.95  -796.68
    "8" = New-Destination "BlackrockRegion"    "黑岩地区地堡"   705.04 373.98   816.38
}

$saveModel = [ordered]@{
    Version      = "0.2.0"
    ReturnPoint  = $null
    Destinations = $dests
}

$json = $saveModel | ConvertTo-Json -Depth 10 -Compress
Write-Output "FastTravel JSON length: $($json.Length) chars"

# Read the whole zip into memory, rewrite the FastTravel entry.
$bytes = [System.IO.File]::ReadAllBytes($mdPath)
$ms = New-Object System.IO.MemoryStream
$ms.Write($bytes, 0, $bytes.Length)
$ms.Position = 0

$zip = New-Object System.IO.Compression.ZipArchive($ms, [System.IO.Compression.ZipArchiveMode]::Update)

# Remove existing FastTravel entry if any.
$existing = $zip.GetEntry($entryName)
if ($existing) {
    Write-Output "Existing entry size: $($existing.Length) bytes -> removing"
    $existing.Delete()
}

# Create fresh entry.
$newEntry = $zip.CreateEntry($entryName, [System.IO.Compression.CompressionLevel]::Optimal)
$w = New-Object System.IO.StreamWriter($newEntry.Open(), (New-Object System.Text.UTF8Encoding $false))
$w.Write($json)
$w.Close()

$zip.Dispose()

# Write back to disk.
[System.IO.File]::WriteAllBytes($mdPath, $ms.ToArray())
$ms.Dispose()

$new = Get-Item $mdPath
Write-Output "Wrote back: $mdPath  size=$($new.Length) B"
