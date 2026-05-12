# DisableIntegratedMods.ps1
# 一键禁用已被 TldHacks 整合的 25 个独立 mod
# 用法: 右键 → 使用 PowerShell 运行,或在终端执行 .\DisableIntegratedMods.ps1

$ModsDir = "D:\Steam\steamapps\common\TheLongDark\Mods"

$IntegratedMods = @(
    "SonicMode"
    "StretchArmstrong"
    "SilentWalker"
    "Jump"
    "FullSwing"
    "TimeScaleHotkey"
    "GearDecayModifier"
    "ClothingTweaker2"
    "TorchTweaker"
    "KeroseneLampTweaks"
    "GfxBoost"
    "LightCull"
    "ExtraGraphicsSettings"
    "GunZoom"
    "PauseInJournal"
    "SkipIntroRedux"
    "CougarSoundBegone"
    "VehicleFov"
    "DroppableUndroppables"
    "RememberBreakDownItem"
    "AutoToggleLights"
    "DisableAutoEquipCharcoal"
    "TinyTweaks-RunWithLantern"
    "ModSettingsQuickNav"
    "MotionTracker"
)

$disabled = 0
$already = 0
$missing = 0

foreach ($mod in $IntegratedMods) {
    $dll = Join-Path $ModsDir "$mod.dll"
    $target = "$dll.disabled"

    if (Test-Path $dll) {
        Rename-Item $dll $target -Force
        Write-Host "[OK] $mod.dll -> .disabled" -ForegroundColor Green
        $disabled++
    }
    elseif (Test-Path $target) {
        Write-Host "[--] $mod.dll already disabled" -ForegroundColor DarkGray
        $already++
    }
    else {
        Write-Host "[??] $mod.dll not found" -ForegroundColor Yellow
        $missing++
    }
}

Write-Host ""
Write-Host "Done: $disabled disabled, $already already disabled, $missing not found" -ForegroundColor Cyan
Write-Host "TldHacks.dll covers all above functionality." -ForegroundColor Cyan
