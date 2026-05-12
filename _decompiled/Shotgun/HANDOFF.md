# HANDOFF — Shotgun Mod (TLD 2.55)
> Last updated: 2026-05-12 21:10
> 状态：从 Mods 目录已撤掉，源码留在 `_decompiled/Shotgun/src/`

## 当前目标
散弹枪 mod (monsieurmeh/MehToolBox 反编译) 在 TLD 2.55 上能用，但仍有动作问题未实战验证。用户决定暂搁置。

## 已完成（本轮可工作的修复）
- **纹理粉色** ✓ — Body 材质 `_Color=(1,0,0.9)` 是品红，强制 `mat.color = Color.white` 修复（Configuration.cs `FixNullTextures`）
- **音频自定义+去掉vanilla 猎枪声** ✓ — `AnimationEventProxy.OnFireFX` 调 vanilla 之前 null 掉 `gun.m_FireAudio` 时序对了；自定义音频走 `AudioMaster.CreateShot()._audioSource.PlayOneShot(clip)`，clip 用 byte 读 WAV + AudioClip.Create+SetData
- **三种弹药分化** ✓ — `GunItem.Fired` Postfix 调 `SpawnExtraProjectilesFromGunItem`：Buckshot 额外 spawn 8 颗(总9)、Birdshot 29 颗(总30)、Slug 不加（vanilla 1 颗）
- **FPSWeapon enabled = true** ✓ — Configuration 末尾加这一行让 Update 能跑（Unity 不对 disabled MB 调 Update）

## 下一步（重新启用时验证）
- 走路 Bob 是否生效（`vp_FPSWeapon.Update` Postfix `vp_FPSWeapon_ShotgunBob`）— 这次加了 `enabled = true` 但未实测
- 拿出/收回/跑步动画是否正常
- 三种弹药音效是否区分

## 未决问题
- **拿出/收回/跑步动画**：modded animator (`FPH_Shotgun_Controller_v2`) 可能缺这些状态，或者 vanilla animator 同时启用导致冲突。日志里 ModComponent 报 `Catalog Test Failed (FPH_Shotgun_Controller_v2.controller) Unknown asset extension` 但后续又 `loaded OK`，可能 controller 不完整
- `UnityWebRequestMultimedia` 在此 IL2Cpp 环境抛 unhandled exception 不可用，已废弃用 byte 读

## 部署步骤
```bash
cd "D:/Github/tld--2.55/_decompiled/Shotgun/src" && dotnet build -c Release
# DLL: bin/Release/Shotgun.dll → D:/Steam/steamapps/common/TheLongDark/Mods/Shotgun.dll
# 还需要: Mods/Shotgun.modcomponent (资源包) + Mods/Shotgun/ 目录 (slug.wav/buckshot.wav/birdshot.wav/localization.json)
```

WAV 来源：freesound.org CC0 — slug=405504, buckshot=515223, birdshot=473846。源 mp3 在 `audio/`，wav 转换用 Python miniaudio：
```python
import miniaudio, wave
d = miniaudio.decode_file('x.mp3', miniaudio.SampleFormat.SIGNED16, 1, 44100)
with wave.open('x.wav','wb') as w: w.setnchannels(1); w.setsampwidth(2); w.setframerate(44100); w.writeframes(d.samples)
```

## 关键文件
- `_decompiled/Shotgun/src/Shotgun/ShotgunAudio.cs` — AudioMaster Shot + WAV byte loader
- `_decompiled/Shotgun/src/Shotgun/AnimationEventProxy.cs` — OnFireFX 里 null m_FireAudio
- `_decompiled/Shotgun/src/Shotgun/Configuration.cs` — FixNullTextures 强制白色 + FPSWeapon.enabled=true
- `_decompiled/Shotgun/src/Shotgun/Patches.cs` — vp_FPSWeapon_ShotgunBob (Update Postfix), GunItem_RefreshCustomHUDOnFired (Prefix m_FireAudio=null + Postfix PlayFire+SpawnExtra), vp_FPSCamera_ShotgunSetWeapon
- `_decompiled/Shotgun/audio/` — 源 mp3 + 转换的 wav
