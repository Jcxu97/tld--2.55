using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace TldHacks;

// —— 武器相关的方法级 patch(比 tick 改字段更可靠)——

// 无限弹药:每次 RemoveNextFromClip 被调用,若 InfiniteAmmo 则 skip
[HarmonyPatch(typeof(GunItem), "RemoveNextFromClip")]
internal static class Patch_Gun_RemoveNextFromClip
{
    private static bool Prefix() => !CheatState.InfiniteAmmo;
}

// 瞄准晃动归零(sway) —— 带式 patch 作双保险。真正干活的是 TickCamera 的 m_DisableAimSway
[HarmonyPatch(typeof(GunItem), "GetSwayIncreasePerSecond")]
internal static class Patch_Gun_SwayIncrease
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoAimSway) __result = 0f;
    }
}

// 无后坐力(belt-n-suspenders):Getter patch + TickCamera 的 m_RecoilSpring 重置
[HarmonyPatch(typeof(GunItem), "GetRecoilPitch")]
internal static class Patch_Gun_RecoilPitch
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoRecoil) __result = 0f;
    }
}

[HarmonyPatch(typeof(GunItem), "GetRecoilYaw")]
internal static class Patch_Gun_RecoilYaw
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoRecoil) __result = 0f;
    }
}

// 关闭瞄准体力消耗:Stamina percent 返回 1(满)—— 配合 TickCamera 的 m_DisableAimStamina
[HarmonyPatch(typeof(GunItem), "GetCurrentStaminaPercent")]
internal static class Patch_Gun_StaminaPercent
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoAimStamina) __result = 1f;
    }
}

// 可以开瞄即使体力不足
[HarmonyPatch(typeof(GunItem), "CanStartAiming")]
internal static class Patch_Gun_CanStartAiming
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.NoAimStamina) __result = true;
    }
}

// ——— Condition.AddHealth 总 Prefix:无坠落伤害 + 免疫动物伤害 + 不会窒息 ———
[HarmonyPatch(typeof(Condition), "AddHealth", new System.Type[] { typeof(float), typeof(DamageSource) })]
internal static class Patch_Condition_AddHealth_Filter
{
    private static bool Prefix(float hp, DamageSource cause)
    {
        if (hp >= 0f) return true; // 只拦负值(伤害),治疗放行

        if (CheatState.NoFallDamage && cause == DamageSource.Falling) return false;
        if (CheatState.NoSuffocating && cause == DamageSource.Suffocating) return false;
        if (CheatState.ImmuneAnimalDamage)
        {
            if (cause == DamageSource.Wolf || cause == DamageSource.Bear || cause == DamageSource.Cougar)
                return false;
        }
        return true;
    }
}

// ——— 免费制作:强制返回 "能造",跳过材料检查 ———
[HarmonyPatch(typeof(Panel_Crafting), "CanCraftSelectedBlueprint")]
internal static class Patch_Craft_CanCraftSelected
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

[HarmonyPatch(typeof(Panel_Crafting), "CanCraftBlueprint")]
internal static class Patch_Craft_CanCraft
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

// ——— 快速制作:CraftingStart 后游戏内时间狂飙,让 crafting 瞬间完成 ———
[HarmonyPatch(typeof(Panel_Crafting), "CraftingStart")]
internal static class Patch_Craft_Start_TimeAccel
{
    private static void Postfix()
    {
        if (!CheatState.QuickCraft) return;
        try
        {
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) return;
            tod.Accelerate(1000f, 30f, false);
            ModMain.Log?.Msg("[QuickCraft] time accelerated 1000x for 30s");
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickCraft] {ex.Message}"); }
    }
}

// ——— 隐身:AI 完全看不到 / 不扫描玩家(v2.7.0 原有 2 层)———
[HarmonyPatch(typeof(BaseAi), "CanSeeTarget")]
internal static class Patch_BaseAi_CanSeeTarget
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.Stealth) __result = false;
    }
}

[HarmonyPatch(typeof(BaseAi), "ScanForSmells")]
internal static class Patch_BaseAi_ScanForSmells
{
    private static bool Prefix() => !CheatState.Stealth;
}

// v2.7.6 —— BaseAi 的 3 层 patch(ScanForNewTarget / IsPlayerAThreat / DoOnDetection)
// 实测是启动卡死的罪魁,切掉。保留 v2.7.0 原有的 2 层(CanSeeTarget + ScanForSmells)。
// Stealth 功能不会 100% 完美(可能狼仍能看见你),但游戏能启动优先。
// 未来修法:要么改 tick 每帧清 target 字段,要么 patch 具体的感知方法(不是泛用的)。

// ——— 忽略上锁(只保留 IsLocked,其余 patch 去掉 —— 2.7.1 启动卡死嫌疑)———
[HarmonyPatch(typeof(Lock), "IsLocked")]
internal static class Patch_Lock_IsLocked
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.IgnoreLock) __result = false;
    }
}
// Lock.RequiresToolToUnlock + PlayerHasRequiredToolToUnlock, Breath.GetBreathTimePercent,
// Encumber.IsEncumbered + GetEncumbranceSlowdownMultiplier 这 5 个 patch 去掉 ——
// 前者因启动卡死嫌疑,后者因为对应功能(IC/IS)已去除(交给 UniversalTweaks 等 mod)

// ——— 冰面不破:冰面破裂触发 / 落水 直接跳过 ———
[HarmonyPatch(typeof(IceCrackingTrigger), "BreakIce")]
internal static class Patch_IceBreak_BreakIce
{
    private static bool Prefix() => !CheatState.ThinIceNoBreak;
}

[HarmonyPatch(typeof(IceCrackingTrigger), "FallInWater")]
internal static class Patch_IceBreak_FallInWater
{
    private static bool Prefix() => !CheatState.ThinIceNoBreak;
}

// ——— 生火 100% 成功 ———
[HarmonyPatch(typeof(FireManager), "CalculateFireStartSuccess")]
internal static class Patch_FireMgr_Success
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.QuickFire) __result = 1f;
    }
}

// ——— 关闭瞄准景深(DOF)—— 拦截 EnableCameraWeaponPostEffects(true),强制传 false ———
[HarmonyPatch(typeof(CameraEffects), "EnableCameraWeaponPostEffects")]
internal static class Patch_CamEffects_WeaponPost
{
    private static void Prefix(ref bool isEnabled)
    {
        if (CheatState.NoAimDOF) isEnabled = false;
    }
}

// ——— 快速开容器:直接把 delay 0 / 立刻 elapsed ———
// Enable 有两个重载,Harmony 自动匹配会歧义,指定 3 参版本
[HarmonyPatch(typeof(Panel_Container), "Enable", new System.Type[] { typeof(bool), typeof(bool), typeof(Il2CppSystem.Action) })]
internal static class Patch_Container_Enable
{
    private static void Postfix(Panel_Container __instance)
    {
        if (!CheatState.QuickOpenContainer) return;
        try
        {
            __instance.m_EnableDelaySeconds = 0f;
            __instance.m_EnableDelayElapsed = 999f;
        }
        catch { }
    }
}


// ——————————— Tick-based 补丁 ———————————
// OnUpdate 每帧/每 N 帧扫一次实例,直接改字段。
internal static class CheatsTick
{
    // ——— 武器:无限弹药 / 永不卡壳 / 快速射击 / 无后坐(字段层 fallback)———
    public static void TickGuns()
    {
        if (!CheatState.InfiniteAmmo && !CheatState.NoJam && !CheatState.FastFire && !CheatState.NoRecoil) return;
        try
        {
            if (CheatState.NoJam)
            {
                try { GunItem.m_ForceNoJam = true; } catch { }
            }

            var guns = UnityEngine.Object.FindObjectsOfType<GunItem>();
            if (guns == null) return;
            foreach (var g in guns)
            {
                if (g == null) continue;
                try
                {
                    if (CheatState.NoJam)
                    {
                        try { g.m_IsJammed = false; } catch { }
                    }
                    if (CheatState.InfiniteAmmo && g.m_ClipSize > 0)
                    {
                        if (g.m_RoundsInClip < g.m_ClipSize)
                            g.m_RoundsInClip = g.m_ClipSize;
                    }
                    if (CheatState.FastFire)
                    {
                        try { if (g.m_FiringRateSeconds > 0.2f) g.m_FiringRateSeconds = 0.2f; } catch { }
                        try { if (g.m_FireDelayOnAim > 0.05f) g.m_FireDelayOnAim = 0.05f; } catch { }
                        try { if (g.m_FireDelayAfterReload > 0.1f) g.m_FireDelayAfterReload = 0.1f; } catch { }
                    }
                    if (CheatState.NoRecoil)
                    {
                        try { g.m_PitchRecoilMin = 0f; } catch { }
                        try { g.m_PitchRecoilMax = 0f; } catch { }
                        try { g.m_YawRecoilMin = 0f; } catch { }
                        try { g.m_YawRecoilMax = 0f; } catch { }
                        // 其 sway 也顺带归零,aim 稳定
                        try { g.m_SwayValueZeroFatigue = 0f; } catch { }
                        try { g.m_SwayValueMaxFatigue = 0f; } catch { }
                    }
                    if (CheatState.NoAimSway)
                    {
                        try { g.m_SwayValueZeroFatigue = 0f; } catch { }
                        try { g.m_SwayValueMaxFatigue = 0f; } catch { }
                        try { g.m_SwayValue = 0f; } catch { }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    // ——— 摄像机:NoRecoil / NoAimSway / NoAimShake / NoBreathSway / NoAimStamina ———
    // v2.7.2 优化:(1) 所有 FieldInfo lazy 缓存到 static,不再每次 GetField;
    //             (2) 有任一 toggle ON 才跑 full path,全关且上次也关 → 早退 0 开销;
    //             (3) 从每帧改成 ModMain 里调用 —— 调用频率由 ModMain 决定(30 帧 = 0.5s)
    private static bool _lastAnyAimToggle = false;
    // Cached reflection members (lazy init on first call)
    private static FieldInfo _fi_currentWeapon, _fi_recoilSpring;
    private static FieldInfo[] _fi_camFloats;   // 对应 ShakeAmplitude / BobAmplitude 等
    private static FieldInfo[] _fi_camSwayFloats; // sway 特有的几个
    private static FieldInfo _fi_rsCur, _fi_rsTgt, _fi_rsVel;
    private static FieldInfo _fi_weapShake, _fi_weapCold, _fi_weapRandom, _fi_weapBob;
    private static FieldInfo _fi_weapSwayLim, _fi_weapSwayMax, _fi_weapSwayStart;
    private static FieldInfo _fi_weapDisSway, _fi_weapDisShake, _fi_weapDisBreath, _fi_weapDisStam;
    private static bool _reflectionInited = false;

    private static void EnsureReflectionInited(object weapon)
    {
        if (_reflectionInited && weapon == null) return;
        var camT = typeof(vp_FPSCamera);
        _fi_currentWeapon = _fi_currentWeapon ?? camT.GetField("m_CurrentWeapon", BindingFlags.Instance | BindingFlags.Public);
        _fi_recoilSpring  = _fi_recoilSpring  ?? camT.GetField("m_RecoilSpring",  BindingFlags.Instance | BindingFlags.Public);
        _fi_camFloats ??= new[] {
            camT.GetField("ShakeAmplitude", BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("ShakeSpeed",     BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("BobAmplitude",   BindingFlags.Instance | BindingFlags.Public),
        };
        _fi_camSwayFloats ??= new[] {
            camT.GetField("m_MaxAmbientSwayAngleDegreesA",        BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_MaxAmbientAimingSwayAngleDegreesA",  BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_AmbientSwaySpeedA",                   BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_AmbientAimingSwaySpeedA",             BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_CurrentMaxAmbientSwayAngle",          BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_CurrentAmbientSwaySpeed",             BindingFlags.Instance | BindingFlags.Public),
        };
        // RecoilSpring struct 字段
        if (_fi_recoilSpring != null)
        {
            var rsT = _fi_recoilSpring.FieldType;
            _fi_rsCur = _fi_rsCur ?? rsT.GetField("m_Current",  BindingFlags.Instance | BindingFlags.Public);
            _fi_rsTgt = _fi_rsTgt ?? rsT.GetField("m_Target",   BindingFlags.Instance | BindingFlags.Public);
            _fi_rsVel = _fi_rsVel ?? rsT.GetField("m_Velocity", BindingFlags.Instance | BindingFlags.Public);
        }
        // 武器字段(需要一个 weapon 实例才能拿 Type)
        if (weapon != null && _fi_weapDisSway == null)
        {
            var wt = weapon.GetType();
            _fi_weapDisSway   = wt.GetField("m_DisableAimSway",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapDisShake  = wt.GetField("m_DisableAimShake",     BindingFlags.Instance | BindingFlags.Public);
            _fi_weapDisBreath = wt.GetField("m_DisableAimBreathing", BindingFlags.Instance | BindingFlags.Public);
            _fi_weapDisStam   = wt.GetField("m_DisableAimStamina",   BindingFlags.Instance | BindingFlags.Public);
            _fi_weapShake     = wt.GetField("ShakeAmplitude",        BindingFlags.Instance | BindingFlags.Public);
            _fi_weapCold      = wt.GetField("m_ColdShakeAngle",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapRandom    = wt.GetField("m_RandomShakeAngle",    BindingFlags.Instance | BindingFlags.Public);
            _fi_weapBob       = wt.GetField("BobAmplitude",          BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayLim   = wt.GetField("SwayLimits",            BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayMax   = wt.GetField("SwayMaxFatigue",        BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayStart = wt.GetField("SwayStartFatigue",      BindingFlags.Instance | BindingFlags.Public);
        }
        _reflectionInited = true;
    }

    private static void SetFloatIfNotNull(FieldInfo f, object inst, float v)
    {
        if (f == null || inst == null) return;
        try { f.SetValue(inst, v); } catch { }
    }
    private static void SetBoolIfNotNull(FieldInfo f, object inst, bool v)
    {
        if (f == null || inst == null) return;
        try { f.SetValue(inst, v); } catch { }
    }

    public static void TickCamera()
    {
        bool anyOn = CheatState.NoAimSway || CheatState.NoAimShake || CheatState.NoBreathSway
                  || CheatState.NoAimStamina || CheatState.NoRecoil;
        // 全关 AND 上次也全关 → 零开销早退
        if (!anyOn && !_lastAnyAimToggle) return;

        try
        {
            var cam = GameManager.GetVpFPSCamera();
            if (cam == null) return;

            object weapon = null;
            EnsureReflectionInited(null);
            if (_fi_currentWeapon != null) { try { weapon = _fi_currentWeapon.GetValue(cam); } catch { } }
            if (weapon != null) EnsureReflectionInited(weapon);

            // bool 开关双向同步(包括 toggle 关后恢复)
            try { vp_FPSCamera.m_DisableAmbientSway = CheatState.NoAimSway; } catch { }
            SetBoolIfNotNull(_fi_weapDisSway,   weapon, CheatState.NoAimSway);
            SetBoolIfNotNull(_fi_weapDisShake,  weapon, CheatState.NoAimShake);
            SetBoolIfNotNull(_fi_weapDisBreath, weapon, CheatState.NoBreathSway);
            SetBoolIfNotNull(_fi_weapDisStam,   weapon, CheatState.NoAimStamina);

            // 浮点字段:只在 toggle 开时归零(关掉后不可逆)
            if (!anyOn)
            {
                _lastAnyAimToggle = false;
                return;
            }

            if (CheatState.NoAimSway)
            {
                // m_Max* / m_Ambient* 6 个 sway 字段
                for (int i = 0; i < _fi_camSwayFloats.Length; i++)
                    SetFloatIfNotNull(_fi_camSwayFloats[i], cam, 0f);
                SetFloatIfNotNull(_fi_weapSwayLim,   weapon, 0f);
                SetFloatIfNotNull(_fi_weapSwayMax,   weapon, 0f);
                SetFloatIfNotNull(_fi_weapSwayStart, weapon, 0f);
            }
            if (CheatState.NoAimShake)
            {
                // ShakeAmplitude[0] / ShakeSpeed[1]
                SetFloatIfNotNull(_fi_camFloats[0], cam, 0f);
                SetFloatIfNotNull(_fi_camFloats[1], cam, 0f);
                SetFloatIfNotNull(_fi_weapShake,  weapon, 0f);
                SetFloatIfNotNull(_fi_weapCold,   weapon, 0f);
                SetFloatIfNotNull(_fi_weapRandom, weapon, 0f);
            }
            if (CheatState.NoBreathSway)
            {
                // BobAmplitude[2]
                SetFloatIfNotNull(_fi_camFloats[2], cam, 0f);
                SetFloatIfNotNull(_fi_weapBob, weapon, 0f);
            }

            if (CheatState.NoRecoil && _fi_recoilSpring != null)
            {
                try
                {
                    var rs = _fi_recoilSpring.GetValue(cam);
                    if (rs != null)
                    {
                        SetFloatIfNotNull(_fi_rsCur, rs, 0f);
                        SetFloatIfNotNull(_fi_rsTgt, rs, 0f);
                        SetFloatIfNotNull(_fi_rsVel, rs, 0f);
                        _fi_recoilSpring.SetValue(cam, rs); // struct 复制回去
                    }
                }
                catch { }
            }

            _lastAnyAimToggle = true;
        }
        catch { }
    }

    // (旧的 TrySetFieldFloat / TrySetFieldBool / SetStructFieldFloat helper 已被缓存版
    //  FieldInfo 替换,删除。)

    // ——— 动物不能动 / 隐身(v2.7.7 改造:用 m_DisableScanForTargets 游戏内建开关)———
    // v2.7.8:修 toggle 关后字段卡 true 不复位 —— 记 _lastTickedStealth/Freeze,
    // 上轮开过就再跑一次同步把字段还原
    private static System.Reflection.FieldInfo _fi_baseAi_disableScan;
    private static System.Reflection.FieldInfo _fi_baseAi_currentTarget;
    private static System.Reflection.FieldInfo _fi_baseAi_aiTarget;
    private static bool _lastTickedStealth = false;
    private static bool _lastTickedFreeze = false;

    public static void TickAnimals()
    {
        // 早退条件:本轮 + 上轮 都没开过任何相关 toggle
        bool runStealth = CheatState.Stealth || _lastTickedStealth;
        bool runFreeze  = CheatState.FreezeAnimals || _lastTickedFreeze;
        if (!runStealth && !runFreeze) return;

        try
        {
            var ais = UnityEngine.Object.FindObjectsOfType<BaseAi>();
            if (ais == null) { _lastTickedStealth = CheatState.Stealth; _lastTickedFreeze = CheatState.FreezeAnimals; return; }

            if (_fi_baseAi_disableScan == null)
            {
                var t = typeof(BaseAi);
                _fi_baseAi_disableScan   = t.GetField("m_DisableScanForTargets", BindingFlags.Instance | BindingFlags.Public);
                _fi_baseAi_currentTarget = t.GetField("m_CurrentTarget",          BindingFlags.Instance | BindingFlags.Public);
                _fi_baseAi_aiTarget      = t.GetField("m_AiTarget",               BindingFlags.Instance | BindingFlags.Public);
            }

            foreach (var ai in ais)
            {
                if (ai == null) continue;
                try
                {
                    if (runFreeze)
                    {
                        try { ai.m_SpeedForPathfindingOverride = CheatState.FreezeAnimals; } catch { }
                        if (CheatState.FreezeAnimals)
                            try { ai.m_OverrideSpeed = 0f; } catch { }
                    }

                    // Stealth:双向同步游戏内建 m_DisableScanForTargets
                    if (runStealth && _fi_baseAi_disableScan != null)
                    {
                        try { _fi_baseAi_disableScan.SetValue(ai, CheatState.Stealth); } catch { }
                    }
                    // 清当前目标,避免已锁上的玩家逃不掉
                    if (CheatState.Stealth)
                    {
                        try { _fi_baseAi_currentTarget?.SetValue(ai, null); } catch { }
                        try { _fi_baseAi_aiTarget?.SetValue(ai, null); } catch { }
                    }
                }
                catch { }
            }

            _lastTickedStealth = CheatState.Stealth;
            _lastTickedFreeze  = CheatState.FreezeAnimals;
        }
        catch { }
    }

    // ——— 忽略上锁:v2.7.8 修 —— 不能用 lk.IsLocked() 判(被 Patch_Lock_IsLocked 篡返回 false)
    // 改直接读 m_LockState 字段
    private static System.Reflection.MethodInfo _mi_lock_setLockState;
    private static System.Reflection.FieldInfo _fi_lock_state;
    public static void TickLocks()
    {
        if (!CheatState.IgnoreLock) return;
        try
        {
            var locks = UnityEngine.Object.FindObjectsOfType<Lock>();
            if (locks == null) return;

            if (_mi_lock_setLockState == null)
                _mi_lock_setLockState = typeof(Lock).GetMethod("SetLockState", BindingFlags.Instance | BindingFlags.Public);
            if (_fi_lock_state == null)
                _fi_lock_state = typeof(Lock).GetField("m_LockState", BindingFlags.Instance | BindingFlags.Public);
            if (_mi_lock_setLockState == null || _fi_lock_state == null) return;

            foreach (var lk in locks)
            {
                if (lk == null) continue;
                try
                {
                    // 直接读字段,绕过 Patch_Lock_IsLocked 的 Postfix 篡改
                    var state = _fi_lock_state.GetValue(lk);
                    if (state != null && (int)state == (int)LockState.Locked)
                        _mi_lock_setLockState.Invoke(lk, new object[] { LockState.Unlocked });
                }
                catch { }
            }
        }
        catch { }
    }

    // ——— 火焰无限时长:保留 scaffold(用户已用 H 键方案,保留不 break)———
    public static void TickFires()
    {
        if (!CheatState.InfiniteFireDurations) return;
        try
        {
            TryLockFieldOnAll("TorchItem",       "m_ElapsedLifetimeTODHours", 0f);
            TryLockFieldOnAll("FlareItem",       "m_ElapsedLifetimeTODHours", 0f);
            TryLockFieldOnAll("CampFireItem",    "m_TemperatureLossPerSecond", 0f);
            TryLockFieldOnAll("KeroseneLampItem","m_CurrentFuelLiters", 1f);
        }
        catch { }
    }

    // ——— 爬绳加速 ———
    // 直接设固定值 5.0(原值 0.3~1.5 左右),幂等:多次 tick 不会叠加。
    // 注意:无法在 toggle 关掉后还原原值(原值没记录),要等场景重载。
    public static void TickClimbRope()
    {
        if (!CheatState.QuickClimb) return;
        try
        {
            var all = UnityEngine.Object.FindObjectsOfType<PlayerClimbRope>();
            if (all == null) return;
            foreach (var p in all)
            {
                if (p == null) continue;
                try
                {
                    var t = typeof(PlayerClimbRope);
                    foreach (var fn in new[] { "m_ClimbSpeed", "m_ClimbSpeedUpFullyRested", "m_ClimbSpeedUpExhausted",
                                               "m_ClimbSpeedDownFullyRested", "m_ClimbSpeedDownExhausted" })
                    {
                        var f = t.GetField(fn, BindingFlags.Instance | BindingFlags.Public);
                        if (f != null && f.FieldType == typeof(float))
                        {
                            float cur = (float)f.GetValue(p);
                            if (cur > 0f && cur < 5f) f.SetValue(p, 5f); // 幂等:设到 5 就不动了
                        }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    // ——— 无饥饿 / 无口渴 / 无疲劳 / 始终温暖 / GodMode 兜底 ———
    // (InfiniteStamina / InfiniteCarry 已去除,交给 UniversalTweaks 等 mod 处理)
    public static void TickStatus()
    {
        if (!CheatState.NoFatigue && !CheatState.NoHunger
            && !CheatState.NoThirst && !CheatState.AlwaysWarm && !CheatState.GodMode) return;

        try
        {
            if (CheatState.NoFatigue || CheatState.GodMode)
            {
                var fat = GameManager.GetFatigueComponent();
                if (fat != null) { try { fat.m_CurrentFatigue = 0f; } catch { } }
            }
            if (CheatState.NoHunger || CheatState.GodMode)
            {
                var h = GameManager.GetHungerComponent();
                if (h != null) { try { h.m_CurrentReserveCalories = h.m_MaxReserveCalories; } catch { } }
            }
            if (CheatState.NoThirst || CheatState.GodMode)
            {
                var t = GameManager.GetThirstComponent();
                if (t != null) { try { t.m_CurrentThirst = 0f; } catch { } }
            }
            if (CheatState.AlwaysWarm || CheatState.GodMode)
            {
                var f = GameManager.GetFreezingComponent();
                if (f != null) { try { f.m_CurrentFreezing = 0f; } catch { } }
            }
            if (CheatState.GodMode)
            {
                var c = GameManager.GetConditionComponent();
                if (c != null) { try { c.m_CurrentHP = c.m_MaxHP; } catch { } }
            }
        }
        catch { }
    }

    // ——— 衣物不潮湿 ———
    public static void TickClothingWetness()
    {
        if (!CheatState.NoWetClothes) return;
        try
        {
            var clothes = UnityEngine.Object.FindObjectsOfType<ClothingItem>();
            if (clothes == null) return;
            foreach (var c in clothes)
            {
                if (c == null) continue;
                try
                {
                    var f = typeof(ClothingItem).GetField("m_PercentWet", BindingFlags.Instance | BindingFlags.Public);
                    if (f != null) f.SetValue(c, 0f);
                }
                catch { }
            }
        }
        catch { }
    }

    private static void TryLockFieldOnAll(string typeName, string fieldName, float value)
    {
        try
        {
            var asm = typeof(GameManager).Assembly;
            var t = asm.GetType("Il2Cpp." + typeName) ?? asm.GetType(typeName);
            if (t == null) return;
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (f == null) return;
            var il2t = Il2CppType.From(t);
            var arr = UnityEngine.Object.FindObjectsOfType(il2t);
            if (arr == null) return;
            foreach (var inst in arr)
            {
                try { f.SetValue(inst, value); } catch { }
            }
        }
        catch { }
    }
}
