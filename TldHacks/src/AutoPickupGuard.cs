using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace TldHacks;

// v2.7.58 / v2.7.59 —— 拦截 ItemPicker mod 自动 W 键拾取玩家刚丢的物品
//   思路:Drop 时记 Pointer → Time;ItemPicker.OnUpdate 跑时置 InAutoPickup=true;
//        PlayerManager.ProcessPickupItemInteraction Prefix 在 InAutoPickup + dict 命中时 skip
internal static class AutoPickupGuard
{
    private const string HARMONY_ID = "TldHacks.AutoPickupGuard.Dynamic";
    private static HarmonyLib.Harmony _h;
    private static bool _itemPickerPatched;

    public static readonly Dictionary<IntPtr, float> DroppedAt = new Dictionary<IntPtr, float>();
    public static bool InAutoPickup;

    // 诊断用:每个 patch 触发 & hook 成功 只打一次 log
    internal static bool _logDrop;
    internal static bool _logDropPM;
    internal static bool _logOnUpdate;
    internal static bool _logSkip;

    private static HarmonyLib.Harmony H { get { if (_h == null) _h = new HarmonyLib.Harmony(HARMONY_ID); return _h; } }

    public static void ReconcileItemPickerPatch()
    {
        try
        {
            bool wanted = CheatState.BlockAutoPickupOwnDrops;
            if (wanted == _itemPickerPatched) return;

            var target = Patch_ItemPicker_OnUpdate_Guard.ResolveTargetMethod();
            if (target == null) return;

            if (wanted)
            {
                H.Patch(target,
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ItemPicker_OnUpdate_Guard), "Prefix")),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ItemPicker_OnUpdate_Guard), "Postfix")));
                _itemPickerPatched = true;
                ModMain.Log?.Msg("[AutoPickupGuard] ON ItemPickerMain.OnUpdate");
            }
            else
            {
                H.Unpatch(target, HarmonyPatchType.All, HARMONY_ID);
                _itemPickerPatched = false;
                InAutoPickup = false;
                DroppedAt.Clear();
                ModMain.Log?.Msg("[AutoPickupGuard] OFF ItemPickerMain.OnUpdate");
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[AutoPickupGuard.Dynamic] {ex.Message}"); }
    }
}

// ——— 1) GearItem.Drop(int, bool, bool, bool) → GearItem ———
[HarmonyPatch(typeof(GearItem), "Drop", new System.Type[] { typeof(int), typeof(bool), typeof(bool), typeof(bool) })]
internal static class Patch_GearItem_Drop_Track
{
    private static void Postfix(GearItem __instance, GearItem __result)
    {
        try
        {
            if (!CheatState.BlockAutoPickupOwnDrops) return;
            float t = Time.time;
            if (__instance != null) AutoPickupGuard.DroppedAt[__instance.Pointer] = t;
            if (__result != null)   AutoPickupGuard.DroppedAt[__result.Pointer]   = t;
            if (!AutoPickupGuard._logDrop)
            {
                AutoPickupGuard._logDrop = true;
                ModMain.Log?.Msg($"[AutoPickupGuard.GIDrop] hook 生效,记录第一个 drop (ptrs={AutoPickupGuard.DroppedAt.Count})");
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[AutoPickupGuard.GIDrop] {ex.Message}"); }
    }
}

// ——— 2) PlayerManager.Drop(GameObject, bool) 第二条 drop 路径 ———
[HarmonyPatch(typeof(PlayerManager), "Drop", new System.Type[] { typeof(GameObject), typeof(bool) })]
internal static class Patch_PlayerManager_Drop_Track
{
    private static void Prefix(GameObject go)
    {
        try
        {
            if (!CheatState.BlockAutoPickupOwnDrops) return;
            if (go == null) return;
            var gi = go.GetComponent<GearItem>();
            if (gi == null) return;
            AutoPickupGuard.DroppedAt[gi.Pointer] = Time.time;
            if (!AutoPickupGuard._logDropPM)
            {
                AutoPickupGuard._logDropPM = true;
                ModMain.Log?.Msg("[AutoPickupGuard.PMDrop] hook 生效");
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[AutoPickupGuard.PMDrop] {ex.Message}"); }
    }
}

// ——— 3) 动态解析 ItemPickerMain.OnUpdate,置 InAutoPickup flag ———
internal static class Patch_ItemPicker_OnUpdate_Guard
{
    private static bool _resolved;
    private static MethodBase _target;

    internal static MethodBase ResolveTargetMethod()
    {
        if (_resolved) return _target;
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "ItemPicker");
            if (asm == null) return null;

            var t = asm.GetType("ItemPicker.ItemPickerMain")
                 ?? asm.GetType("ItemPickerMain")
                 ?? asm.GetTypes().FirstOrDefault(x => x.Name == "ItemPickerMain");
            if (t == null)
            {
                ModMain.Log?.Warning("[AutoPickupGuard] ItemPickerMain type 没找到");
                _resolved = true;
                return null;
            }
            _target = t.GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _resolved = true;
            ModMain.Log?.Msg(_target != null
                ? $"[AutoPickupGuard] hooked ItemPickerMain.OnUpdate (declaringType={t.FullName})"
                : "[AutoPickupGuard] ItemPickerMain.OnUpdate 方法没找到");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[AutoPickupGuard.Target] {ex.Message}"); }
        return _target;
    }

    private static void Prefix()
    {
        AutoPickupGuard.InAutoPickup = true;
        if (!AutoPickupGuard._logOnUpdate)
        {
            AutoPickupGuard._logOnUpdate = true;
            ModMain.Log?.Msg($"[AutoPickupGuard.IPUpdate] 第一次进 OnUpdate,dict 大小={AutoPickupGuard.DroppedAt.Count}");
        }
    }
    private static void Postfix() { AutoPickupGuard.InAutoPickup = false; }
}

// ——— 4) 拦截 pickup:InAutoPickup + 命中 dict 时 skip ———
[HarmonyPatch(typeof(PlayerManager), "ProcessPickupItemInteraction",
    new System.Type[] { typeof(GearItem), typeof(bool), typeof(bool), typeof(bool) })]
internal static class Patch_PlayerManager_ProcessPickup_BlockOwnDrops
{
    private static bool Prefix(GearItem item)
    {
        try
        {
            if (!CheatState.BlockAutoPickupOwnDrops) return true;
            if (!AutoPickupGuard.InAutoPickup)      return true;
            if (item == null)                       return true;
            if (AutoPickupGuard.DroppedAt.ContainsKey(item.Pointer))
            {
                if (!AutoPickupGuard._logSkip)
                {
                    AutoPickupGuard._logSkip = true;
                    ModMain.Log?.Msg($"[AutoPickupGuard.Skip] 第一次阻止 pickup (ptr命中,dict 大小={AutoPickupGuard.DroppedAt.Count})");
                }
                return false;
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[AutoPickupGuard.Skip] {ex.Message}"); }
        return true;
    }
}
