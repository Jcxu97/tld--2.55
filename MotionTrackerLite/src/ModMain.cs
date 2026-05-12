using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(MotionTrackerLite.ModMain), "MotionTrackerLite", "1.0.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace MotionTrackerLite;

public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;
    internal static Settings ModSettings;
    private static bool _guiSubscribed;
    private static bool _updateSubscribed;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        TrackerConfig.Load();

        ModSettings = new Settings();
        ModSettings.LoadFromConfig();
        ModSettings.AddToModSettings("MotionTrackerLite");

        Log.Msg($"MotionTrackerLite v1.0.0 — press {TrackerConfig.ToggleKey} to activate radar");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName.Contains("MainMenu"))
        {
            Tracker.IsInGame = false;
            SetActive(false);
        }
        else
        {
            Tracker.IsInGame = true;
            Tracker.Clear();
            if (Tracker.Enabled)
                Tracker.ScanExisting();
        }
    }

    public override void OnUpdate()
    {
        if (TrackerConfig.ToggleKey != KeyCode.None && Input.GetKeyDown(TrackerConfig.ToggleKey))
        {
            Tracker.Enabled = !Tracker.Enabled;
            SetActive(Tracker.Enabled);
            Log.Msg(Tracker.Enabled ? "[Radar] ON" : "[Radar] OFF");
        }
    }

    private static void SetActive(bool active)
    {
        if (active && Tracker.IsInGame)
        {
            if (!_updateSubscribed)
            {
                MelonEvents.OnUpdate.Subscribe(Tracker.Tick, 200);
                _updateSubscribed = true;
            }
            if (!_guiSubscribed)
            {
                MelonEvents.OnGUI.Subscribe(Tracker.Draw, 200);
                _guiSubscribed = true;
            }
            TrackerPatches.PatchAll();
            Tracker.ScanExisting();
        }
        else
        {
            if (_updateSubscribed)
            {
                MelonEvents.OnUpdate.Unsubscribe(Tracker.Tick);
                _updateSubscribed = false;
            }
            if (_guiSubscribed)
            {
                MelonEvents.OnGUI.Unsubscribe(Tracker.Draw);
                _guiSubscribed = false;
            }
            TrackerPatches.UnpatchAll();
            Tracker.Clear();
        }
    }
}
