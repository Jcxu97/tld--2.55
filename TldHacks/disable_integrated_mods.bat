@echo off
chcp 65001 >nul
title TldHacks - 一键禁用冗余/冲突 MOD

:: ===================================================================
::   TldHacks 冗余/冲突 MOD 禁用脚本
:: -------------------------------------------------------------------
::   把本脚本复制到游戏 Mods 文件夹后双击运行,会把已被 TldHacks
::   整合的独立 MOD 改名为 *.dll.disabled,游戏不再加载,功能仍由
::   TldHacks 提供。已禁用的会跳过,未装的会显示"未找到"。
::
::   恢复方法: 把对应文件去掉 .disabled 后缀即可。
:: ===================================================================

set "MODS=%~dp0"
if "%MODS:~-1%"=="\" set "MODS=%MODS:~0,-1%"

set /a disabled=0
set /a already=0
set /a notfound=0

echo ================================================
echo   TldHacks 一键禁用冗余/冲突 MOD
echo ================================================
echo.
echo   [!] 请将此脚本放到游戏的 Mods 文件夹下再运行!
echo       例如: ...\TheLongDark\Mods\disable_integrated_mods.bat
echo.
echo   当前目录: %MODS%
echo ================================================
echo.

echo [整合冗余 - 移动 / 跳跃 / 视角]
call :check "SonicMode.dll"
call :check "Jump.dll"
call :check "GunZoom.dll"
call :check "VehicleFov.dll"
:: TldHacks 已有跨场景传送(双击地图图标 + Quick Teleport 列表)
call :check "FastTravel.dll"
:: TldHacks 已整合 StretchArmstrong 交互距离倍率
call :check "StretchArmstrong.dll"

echo.
echo [整合冗余 - 火光 / 油灯]
call :check "TorchTweaker.dll"
call :check "KeroseneLampTweaks.dll"
call :check "AutoToggleLights.dll"

echo.
echo [整合冗余 - 衰减 / 衣物 / 装备]
call :check "GearDecayModifier.dll"
call :check "BowRepair.dll"
call :check "DisableAutoEquipCharcoal.dll"
call :check "RememberBreakDownItem.dll"
call :check "DroppableUndroppables.dll"

echo.
echo [整合冗余 - 制作 / 烹饪 / 时间]
call :check "CraftAnywhereRedux.dll"
call :check "MoreCookingSlots.dll"
call :check "TimeScaleHotkey.dll"
call :check "FullSwing.dll"
call :check "SilentWalker.dll"

echo.
echo [整合冗余 - QoL / TinyTweaks 系列 / 启动]
call :check "QoL.dll"
call :check "AutoSurvey.dll"
call :check "TinyTweaks-MapTextOuline.dll"
call :check "TinyTweaks-NoSaveOnSprain.dll"
call :check "TinyTweaks-WakeUpCall.dll"
call :check "TinyTweaks-BuryHumanCorpses.dll"
call :check "SleepWithoutABed.dll"
call :check "SkipIntroRedux.dll"
:: MapManager 不整合, 用户保留独立 mod
:: PlaceFromInventory 已整合
call :check "PlaceFromInventory.dll"

echo.
echo [整合冗余 - 食物 / 扭伤 / 搬运]
call :check "CaffeinatedSodas.dll"
call :check "Sprainkle.dll"
call :check "RnStripped.dll"

echo.
echo [整合冗余 - 准星 / 图形]
call :check "ExtraGraphicsSettings.dll"

echo.
echo [整合冗余 - v3.0.4 大型多功能 mod]
:: TldHacks v3.0.4 整合 UT/StackManager 核心功能
:: UT: 哈气/制噪器/滤罐/雪屋/左轮/岩石贮藏/喷漆/雪橇/手电筒/食物/马桶水
:: StackManager: 物品堆叠组件 + 默认列表(咖啡/茶/胡萝卜/土豆等)
call :check "UniversalTweaks.dll"
call :check "StackManager.dll"

echo.
echo [已知冲突 / 有害 MOD]
:: GfxBoost / LightCull: v3.0.2 实测会破坏山洞/夜晚动态灯光,导致漆黑
:: DarkerNights: 弃坑 3 年,在 TLD 2.55 上死循环,直接卡启动
call :check "GfxBoost.dll"
call :check "LightCull.dll"
call :check "DarkerNights.dll"

echo.
echo ================================================
echo   完成: %disabled% 个已禁用, %already% 个之前已禁用, %notfound% 个未找到
echo ================================================
echo.
pause
exit /b

:check
if exist "%MODS%\%~1" (
    ren "%MODS%\%~1" "%~1.disabled"
    echo [已禁用] %~1
    set /a disabled+=1
) else if exist "%MODS%\%~1.disabled" (
    echo [已禁用] %~1 (之前)
    set /a already+=1
) else (
    echo [未找到] %~1
    set /a notfound+=1
)
exit /b
