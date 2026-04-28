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

// ——— 隐身:AI 完全看不到 / 不扫描玩家 ———
// 多层拦截(单一 patch 不够彻底,游戏内 AI 逻辑有多个入口)
[HarmonyPatch(typeof(BaseAi), "CanSeeTarget")]
internal static class Patch_BaseAi_CanSeeTarget
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.Stealth) __result = false;
    }
}

// AI 不主动扫新目标 —— 最关键的一条,阻止 AI 把玩家登记为 target
[HarmonyPatch(typeof(BaseAi), "ScanForNewTarget")]
internal static class Patch_BaseAi_ScanForNewTarget
{
    private static bool Prefix() => !CheatState.Stealth;
}

[HarmonyPatch(typeof(BaseAi), "ScanForSmells")]
internal static class Patch_BaseAi_ScanForSmells
{
    private static bool Prefix() => !CheatState.Stealth;
}

[HarmonyPatch(typeof(BaseAi), "IsPlayerAThreat")]
internal static class Patch_BaseAi_IsPlayerAThreat
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.Stealth) __result = false;
    }
}

[HarmonyPatch(typeof(BaseAi), "DoOnDetection")]
internal static class Patch_BaseAi_DoOnDetection
{
    private static bool Prefix() => !CheatState.Stealth;
}

// ——— 忽略上锁:3 层拦截,任何查锁的地方都让它看起来没锁/玩家已有工具 ———
[HarmonyPatch(typeof(Lock), "IsLocked")]
internal static class Patch_Lock_IsLocked
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.IgnoreLock) __result = false;
    }
}

[HarmonyPatch(typeof(Lock), "RequiresToolToUnlock")]
internal static class Patch_Lock_RequiresTool
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.IgnoreLock) __result = false;
    }
}

[HarmonyPatch(typeof(Lock), "PlayerHasRequiredToolToUnlock")]
internal static class Patch_Lock_PlayerHasTool
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.IgnoreLock) __result = true;
    }
}

// ——— 无限体力(InfiniteStamina)—— Breath.GetBreathTimePercent 返回 1 + tick 保持字段满 ———
// 原 Patch_Breath_Update 挂的是 MonoBehaviour.Update,Il2Cpp 下 Harmony 钩不到 —— 所以之前无效
[HarmonyPatch(typeof(Breath), "GetBreathTimePercent")]
internal static class Patch_Breath_GetPercent
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.InfiniteStamina) __result = 1f;
    }
}

// ——— 无限负重:拦截 Encumber 的各层判定 ———
[HarmonyPatch(typeof(Encumber), "IsEncumbered")]
internal static class Patch_Encumber_IsEncumbered
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.InfiniteCarry) __result = false;
    }
}

[HarmonyPatch(typeof(Encumber), "GetEncumbranceSlowdownMultiplier")]
internal static class Patch_Encumber_Slowdown
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.InfiniteCarry) __result = 1f; // 不减速
    }
}

[HarmonyPatch(typeof(Encumber), "GetEffectiveCarryCapacityKG")]
internal static class Patch_Encumber_CapacityKG
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.InfiniteCarry) __result = 999f;
    }
}

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
[HarmonyPatch(typeof(Panel_Container), "Enable")]
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
    // 游戏内建 bool 开关 m_DisableAim* / m_DisableAmbientSway 双向同步(toggle 关也能恢复);
    // 浮点字段(ShakeAmplitude / BobAmplitude 等)一旦归零无法从运行时拿回原值 ——
    // 这些字段只在 toggle ON 时写入,关闭后需要场景重载才能完全复原。
    public static void TickCamera()
    {
        try
        {
            var cam = GameManager.GetVpFPSCamera();
            if (cam == null) return;

            // —— bool 开关始终同步(支持 toggle 关后恢复)——
            try { vp_FPSCamera.m_DisableAmbientSway = CheatState.NoAimSway; } catch { }

            try
            {
                var wField = typeof(vp_FPSCamera).GetField("m_CurrentWeapon", BindingFlags.Instance | BindingFlags.Public);
                var weapon = wField?.GetValue(cam);
                if (weapon != null)
                {
                    var wt = weapon.GetType();
                    TrySetFieldBool(wt, weapon, "m_DisableAimSway",      CheatState.NoAimSway);
                    TrySetFieldBool(wt, weapon, "m_DisableAimShake",     CheatState.NoAimShake);
                    TrySetFieldBool(wt, weapon, "m_DisableAimBreathing", CheatState.NoBreathSway);
                    TrySetFieldBool(wt, weapon, "m_DisableAimStamina",   CheatState.NoAimStamina);
                }
            }
            catch { }

            // —— 以下只在 toggle 开时写入(归零不可逆,关掉后等场景重载)——
            if (!CheatState.NoRecoil && !CheatState.NoAimSway && !CheatState.NoAimShake && !CheatState.NoBreathSway)
                return;

            if (CheatState.NoAimSway)
            {
                TrySetFieldFloat(cam, "m_MaxAmbientSwayAngleDegreesA", 0f);
                TrySetFieldFloat(cam, "m_MaxAmbientAimingSwayAngleDegreesA", 0f);
                TrySetFieldFloat(cam, "m_AmbientSwaySpeedA", 0f);
                TrySetFieldFloat(cam, "m_AmbientAimingSwaySpeedA", 0f);
                TrySetFieldFloat(cam, "m_CurrentMaxAmbientSwayAngle", 0f);
                TrySetFieldFloat(cam, "m_CurrentAmbientSwaySpeed", 0f);
            }
            if (CheatState.NoAimShake)
            {
                TrySetFieldFloat(cam, "ShakeAmplitude", 0f);
                TrySetFieldFloat(cam, "ShakeSpeed", 0f);
            }
            if (CheatState.NoBreathSway)
            {
                TrySetFieldFloat(cam, "BobAmplitude", 0f);
            }

            // RecoilSpring 是 struct,反射归零 m_Current/m_Target/m_Velocity
            if (CheatState.NoRecoil)
            {
                try
                {
                    var rsField = typeof(vp_FPSCamera).GetField("m_RecoilSpring", BindingFlags.Instance | BindingFlags.Public);
                    if (rsField != null)
                    {
                        var rs = rsField.GetValue(cam);
                        if (rs != null)
                        {
                            SetStructFieldFloat(rs, "m_Current", 0f);
                            SetStructFieldFloat(rs, "m_Target",  0f);
                            SetStructFieldFloat(rs, "m_Velocity",0f);
                            rsField.SetValue(cam, rs); // struct 复制回去
                        }
                    }
                }
                catch { }
            }

            // 武器实例侧的浮点归零
            try
            {
                var wField2 = typeof(vp_FPSCamera).GetField("m_CurrentWeapon", BindingFlags.Instance | BindingFlags.Public);
                var weapon2 = wField2?.GetValue(cam);
                if (weapon2 != null)
                {
                    if (CheatState.NoAimShake)
                    {
                        TrySetFieldFloat(weapon2, "ShakeAmplitude", 0f);
                        TrySetFieldFloat(weapon2, "m_ColdShakeAngle", 0f);
                        TrySetFieldFloat(weapon2, "m_RandomShakeAngle", 0f);
                    }
                    if (CheatState.NoBreathSway)
                    {
                        TrySetFieldFloat(weapon2, "BobAmplitude", 0f);
                    }
                    if (CheatState.NoAimSway)
                    {
                        TrySetFieldFloat(weapon2, "SwayLimits", 0f);
                        TrySetFieldFloat(weapon2, "SwayMaxFatigue", 0f);
                        TrySetFieldFloat(weapon2, "SwayStartFatigue", 0f);
                    }
                }
            }
            catch { }
        }
        catch { }
    }

    private static void TrySetFieldFloat(object inst, string name, float value)
    {
        try
        {
            var f = inst.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (f != null && f.FieldType == typeof(float)) f.SetValue(inst, value);
        }
        catch { }
    }

    private static void TrySetFieldBool(Type t, object inst, string name, bool value)
    {
        try
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (f != null && f.FieldType == typeof(bool)) f.SetValue(inst, value);
        }
        catch { }
    }

    private static void SetStructFieldFloat(object boxedStruct, string name, float value)
    {
        try
        {
            var f = boxedStruct.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (f != null && f.FieldType == typeof(float)) f.SetValue(boxedStruct, value);
        }
        catch { }
    }

    // ——— 动物不能动 / 隐身补丁 ———
    public static void TickAnimals()
    {
        if (!CheatState.FreezeAnimals && !CheatState.Stealth) return;
        try
        {
            var ais = UnityEngine.Object.FindObjectsOfType<BaseAi>();
            if (ais == null) return;
            foreach (var ai in ais)
            {
                if (ai == null) continue;
                try
                {
                    if (CheatState.FreezeAnimals)
                    {
                        ai.m_SpeedForPathfindingOverride = true;
                        ai.m_OverrideSpeed = 0f;
                    }
                    else
                    {
                        ai.m_SpeedForPathfindingOverride = false;
                    }

                    if (CheatState.Stealth)
                    {
                        foreach (var fn in new[] { "m_CurrentTarget", "m_Target", "m_DetectedTarget", "m_Attacker" })
                        {
                            try
                            {
                                var f = typeof(BaseAi).GetField(fn, BindingFlags.Instance | BindingFlags.Public);
                                f?.SetValue(ai, null);
                            }
                            catch { }
                        }
                    }
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

    // ——— 无限体力 / 无饥饿 / 无口渴 / 无疲劳 / 始终温暖 ———
    // Harmony 挂 MonoBehaviour.Update 在 Il2Cpp 下不生效,改 tick 直接设字段
    public static void TickStatus()
    {
        if (!CheatState.InfiniteStamina && !CheatState.NoFatigue && !CheatState.NoHunger
            && !CheatState.NoThirst && !CheatState.AlwaysWarm && !CheatState.GodMode) return;

        try
        {
            if (CheatState.InfiniteStamina)
            {
                var breath = GameManager.GetBreathComponent();
                if (breath != null)
                {
                    try { breath.m_BreathTime = 1f; } catch { }
                    try { breath.m_BreathTimePercent = 1f; } catch { }
                }
            }
            if (CheatState.InfiniteStamina || CheatState.NoFatigue || CheatState.GodMode)
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
