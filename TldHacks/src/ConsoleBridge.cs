using System;
using Il2Cpp;

namespace TldHacks;

// uConsole 是 TLD 内置的 debug console。RunCommand/RunCommandSilent 是 public static。
// Hinterland 留了一大堆 debug_/cheat_/force_/lock_/trader_ 命令,很多作弊功能直接调命令即可。
// 前提:控制台要 "on" 状态 —— 否则 RunCommand 可能 early return。
// DeveloperConsole.dll 会自动开启;我们这里也主动 TurnOn 一次保险。
internal static class ConsoleBridge
{
    private static bool _didInitialTurnOn = false;

    private static void EnsureOn()
    {
        try
        {
            if (_didInitialTurnOn) return;
            if (!uConsole.IsOn()) uConsole.TurnOn();
            _didInitialTurnOn = true;
        }
        catch { }
    }

    public static bool Run(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return false;
        try
        {
            EnsureOn();
            uConsole.RunCommandSilent(cmd);
            ModMain.Log?.Msg($"[Console] {cmd}");
            CheatState.LastActionLog = $"[Console] {cmd}";
            return true;
        }
        catch (Exception ex)
        {
            ModMain.Log?.Error($"[Console] {cmd}: {ex.Message}");
            CheatState.LastActionLog = $"[Console.Error] {cmd}: {ex.Message}";
            return false;
        }
    }

    public static void RunToggle(string cmdName, bool on)
        => Run($"{cmdName} {(on ? "true" : "false")}");
}
