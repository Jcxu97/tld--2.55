using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(HarmonyCache.Plugin), "HarmonyCache", "1.2.0", "TldHacks")]
[assembly: MelonGame]
[assembly: MelonPriority(-10000)]

namespace HarmonyCache;

public class Plugin : MelonPlugin
{
    private static readonly ConcurrentDictionary<Assembly, Type[]> TypesCache = new();
    private static readonly ConcurrentDictionary<string, Type> TypeByNameCache = new();
    private static readonly object AssemblyLock = new();
    private static Assembly[] _assemblySnapshot;
    private static int _lastAssemblyCount;
    private static HarmonyLib.Harmony _harmony;

    private static MelonLogger.Instance _log;

    public override void OnPreInitialization()
    {
        _log = LoggerInstance;
        _harmony = new HarmonyLib.Harmony("com.tldhacks.harmonycache");
        int patched = 0;

        // 1. Cache GetTypesFromAssembly
        var getTypes = AccessTools.Method(typeof(AccessTools), nameof(AccessTools.GetTypesFromAssembly));
        if (getTypes != null)
        {
            _harmony.Patch(getTypes, prefix: new HarmonyMethod(
                AccessTools.Method(typeof(Plugin), nameof(Prefix_GetTypesFromAssembly))));
            patched++;
        }

        // 2. Cache TypeByName (avoids repeated AllTypes full scan for missing types)
        var typeByName = AccessTools.Method(typeof(AccessTools), nameof(AccessTools.TypeByName));
        if (typeByName != null)
        {
            _harmony.Patch(typeByName,
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(Prefix_TypeByName))),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(Postfix_TypeByName))));
            patched++;
        }

        // 3. Cache AllAssemblies (snapshot + invalidate when count changes)
        var allAsm = AccessTools.Method(typeof(AccessTools), nameof(AccessTools.AllAssemblies));
        if (allAsm != null)
        {
            _harmony.Patch(allAsm, prefix: new HarmonyMethod(
                AccessTools.Method(typeof(Plugin), nameof(Prefix_AllAssemblies))));
            patched++;
        }

        // 4. ModComponent DependencyChecker timeout fix — deferred until assembly loads
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;

        LoggerInstance.Msg($"HarmonyCache v1.2: {patched} methods patched.");
    }

    // --- GetTypesFromAssembly: per-assembly cache ---
    private static bool Prefix_GetTypesFromAssembly(Assembly assembly, ref Type[] __result)
    {
        if (assembly == null)
        {
            __result = Type.EmptyTypes;
            return false;
        }

        __result = TypesCache.GetOrAdd(assembly, asm =>
        {
            try
            {
                var types = asm.GetTypes();
                for (var i = 0; i < types.Length; i++)
                {
                    if (types[i] == null)
                        return Array.FindAll(types, t => t != null);
                }
                return types;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return Array.FindAll(ex.Types, t => t != null);
            }
        });

        return false;
    }

    // --- TypeByName: negative cache for missing types, positive cache for found ---
    private static bool Prefix_TypeByName(string name, ref Type __result)
    {
        if (string.IsNullOrEmpty(name))
        {
            __result = null;
            return false;
        }

        if (TypeByNameCache.TryGetValue(name, out var cached))
        {
            __result = cached; // null = negative cache (type doesn't exist)
            return false;
        }

        // Let original run, then cache the result
        return true;
    }

    // Postfix to store result in cache
    private static void Postfix_TypeByName(string name, Type __result)
    {
        if (!string.IsNullOrEmpty(name))
            TypeByNameCache[name] = __result; // null is valid (negative cache)
    }

    // Prevent stuck loading when MelonLoader console steals focus
    public override void OnApplicationStart()
    {
        try
        {
            var appType = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule");
            var prop = appType?.GetProperty("runInBackground", BindingFlags.Static | BindingFlags.Public);
            prop?.SetValue(null, true);
            LoggerInstance.Msg("runInBackground = true");
        }
        catch { }
    }

    // --- AllAssemblies: snapshot that invalidates when assembly count changes ---
    private static bool Prefix_AllAssemblies(ref IEnumerable<Assembly> __result)
    {
        var current = AppDomain.CurrentDomain.GetAssemblies();
        if (_assemblySnapshot == null || current.Length != _lastAssemblyCount)
        {
            lock (AssemblyLock)
            {
                current = AppDomain.CurrentDomain.GetAssemblies();
                if (_assemblySnapshot == null || current.Length != _lastAssemblyCount)
                {
                    _assemblySnapshot = current.Where(a =>
                        !a.FullName.StartsWith("Microsoft.VisualStudio")).ToArray();
                    _lastAssemblyCount = current.Length;
                }
            }
        }

        __result = _assemblySnapshot;
        return false;
    }

    // --- ModComponent DependencyChecker: 10s timeout to prevent black screen without VPN ---
    private static bool _modCompPatched;

    private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        if (_modCompPatched) return;
        if (!args.LoadedAssembly.GetName().Name.Equals("ModComponent", StringComparison.OrdinalIgnoreCase))
            return;

        _modCompPatched = true;
        try
        {
            var depType = args.LoadedAssembly.GetType("ModComponent.Utils.DependencyChecker");
            if (depType == null) return;

            var target = AccessTools.Method(depType, "ReadGlobalDepEntries");
            if (target == null) return;

            _harmony.Patch(target, prefix: new HarmonyMethod(
                AccessTools.Method(typeof(Plugin), nameof(Prefix_ReadGlobalDepEntries))));
            _log?.Msg("[HarmonyCache] ModComponent dep checker patched (10s timeout).");
        }
        catch (Exception ex)
        {
            _log?.Msg($"[HarmonyCache] ModComponent patch failed: {ex.Message}");
        }
    }

    private static readonly string DepFileUrl =
        "https://raw.githubusercontent.com/TLD-Mods/ModLists/master/dependency_files/modcomponent.json";

    private static bool Prefix_ReadGlobalDepEntries()
    {
        try
        {
            string text = null;
            var task = Task.Run(() =>
            {
                using var wc = new WebClient();
                wc.Headers["User-Agent"] = "ModComponent";
                return wc.DownloadString(DepFileUrl);
            });

            if (task.Wait(TimeSpan.FromSeconds(10)))
                text = task.Result;
            else
                _log?.Msg("[HarmonyCache] ModComponent dep check timed out (10s), skipping.");

            if (!string.IsNullOrEmpty(text))
            {
                var depType = AccessTools.TypeByName("ModComponent.Utils.DependencyChecker");
                var field = AccessTools.Field(depType, "GlobalDepEntries");
                if (field != null)
                {
                    var jsonConvert = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                    var deser = jsonConvert?.GetMethod("DeserializeObject",
                        new[] { typeof(string), typeof(Type) });
                    if (deser != null)
                    {
                        var entries = deser.Invoke(null, new object[] { text, field.FieldType });
                        field.SetValue(null, entries);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _log?.Msg($"[HarmonyCache] ModComponent dep check failed: {ex.Message}");
        }

        return false;
    }
}
