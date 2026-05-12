using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace MotionTracker;

public class MotionTrackerMain : MelonMod
{
	public static AssetBundle? assetBundle;

	public static AssetBundle? assetBundle2;

	public static GameObject? motionTrackerParent;

	public static PingManager? activePingManager;

	public static GameObject? trackerPrefab;

	public static GameObject? trackerObject;

	public static GameObject? modSettingPage;

	public static Dictionary<PingManager.AnimalType, GameObject> animalPingPrefabs = new Dictionary<PingManager.AnimalType, GameObject>();

	public static Dictionary<ProjectileType, GameObject> spraypaintPingPrefabs = new Dictionary<ProjectileType, GameObject>();

	public static void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
	{
	}

	public static void LogError(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
	{
		MelonLogger.Msg(Path.GetFileName(filepath) + ":" + caller + "." + lineNumber + ": " + message);
	}

	public override void OnInitializeMelon()
	{
		LogMessage("[MotionTracker] Version " + Assembly.GetExecutingAssembly().GetName().Version, 52, "OnInitializeMelon", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		ClassInjector.RegisterTypeInIl2Cpp<TweenManager>();
		ClassInjector.RegisterTypeInIl2Cpp<PingManager>();
		ClassInjector.RegisterTypeInIl2Cpp<PingComponent>();
		LoadEmbeddedAssetBundle();
		LoadEmbeddedAssetBundle2();
		Settings.OnLoad();
	}

	public static void LoadEmbeddedAssetBundle()
	{
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MotionTracker.Resources.motiontracker");
		if (manifestResourceStream == null)
		{
			LogError("stream==null!  Failed to load embedded asset bundle.  Return.", 71, "LoadEmbeddedAssetBundle", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			return;
		}
		string text = Path.Combine(Path.GetTempPath(), "MotionTracker.Resources.motiontracker");
		if (text == null)
		{
			LogError("tempPath==null!  Failed to create temp path for embedded asset bundle.  Return.", 77, "LoadEmbeddedAssetBundle", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			return;
		}
		LogMessage("tempPath: " + text, 80, "LoadEmbeddedAssetBundle", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		using (FileStream destination = File.Create(text))
		{
			manifestResourceStream.CopyTo(destination);
			LogMessage("Copied embedded asset bundle to temp path.", 85, "LoadEmbeddedAssetBundle", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		}
		assetBundle = AssetBundle.LoadFromFile(text);
		if ((Object)(object)assetBundle == (Object)null)
		{
			LogError("assetBundle==null!  Failed to load asset bundle from file.  Return.", 91, "LoadEmbeddedAssetBundle", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			return;
		}
		try
		{
			File.Delete(text);
		}
		catch (Exception value)
		{
			LogError($"Failed to delete temp asset bundle file: {value}", 101, "LoadEmbeddedAssetBundle", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		}
	}

	public static void LoadEmbeddedAssetBundle2()
	{
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MotionTracker.Resources.motiontrackerassetbundleprefab");
		if (manifestResourceStream == null)
		{
			LogError("stream==null!  Failed to load embedded asset bundle 2.  Return.", 110, "LoadEmbeddedAssetBundle2", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			return;
		}
		string text = Path.Combine(Path.GetTempPath(), "MotionTracker.Resources.motiontrackerassetbundleprefab");
		if (text == null)
		{
			LogError("tempPath==null!  Failed to create temp path for embedded asset bundle 2.  Return.", 116, "LoadEmbeddedAssetBundle2", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			return;
		}
		LogMessage("tempPath: " + text, 119, "LoadEmbeddedAssetBundle2", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		using (FileStream destination = File.Create(text))
		{
			manifestResourceStream.CopyTo(destination);
			LogMessage("Copied embedded asset bundle 2 to temp path.", 124, "LoadEmbeddedAssetBundle2", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		}
		assetBundle2 = AssetBundle.LoadFromFile(text);
		if ((Object)(object)assetBundle2 == (Object)null)
		{
			LogError("assetBundle2==null!  Failed to load asset bundle 2 from file.  Return.", 130, "LoadEmbeddedAssetBundle2", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			return;
		}
		try
		{
			File.Delete(text);
		}
		catch (Exception value)
		{
			LogError($"Failed to delete temp asset bundle 2 file.: {value}", 140, "LoadEmbeddedAssetBundle2", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
		}
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		if (sceneName.Contains("MainMenu"))
		{
			PingManager.inMenu = true;
			FirstTimeSetup();
		}
		else if (sceneName.Contains("SANDBOX") && Object.op_Implicit((Object)(object)motionTrackerParent))
		{
			LogMessage("Scene name containing SANDBOX " + sceneName + " was loaded.", 183, "OnSceneWasLoaded", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\MotionTracker.cs");
			if (Object.op_Implicit((Object)(object)PingManager.instance))
			{
				PingManager.instance.ClearIcons();
			}
			PingManager.inMenu = false;
		}
		else
		{
			if (Object.op_Implicit((Object)(object)PingManager.instance))
			{
				PingManager.instance.ClearIcons();
			}
			PingManager.inMenu = false;
		}
	}

	public void FirstTimeSetup()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)motionTrackerParent))
		{
			return;
		}
		motionTrackerParent = new GameObject("MotionTracker");
		trackerObject = Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("MotionTracker"), motionTrackerParent.transform);
		Object.DontDestroyOnLoad((Object)(object)motionTrackerParent);
		activePingManager = motionTrackerParent.AddComponent<PingManager>();
		GameObject val = new GameObject("PrefabSafe");
		val.transform.parent = motionTrackerParent.transform;
		animalPingPrefabs = new Dictionary<PingManager.AnimalType, GameObject>();
		animalPingPrefabs.Add(PingManager.AnimalType.Crow, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("crow"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Rabbit, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("rabbit"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Wolf, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("wolf"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Timberwolf, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("timberwolf"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Bear, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("bear"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Cougar, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("cougar"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Moose, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("moose"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Stag, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("stag"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Doe, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("doe"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.PuffyBird, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("ptarmigan"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Arrow, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("arrow"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.Coal, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("coal"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.RawFish, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("rawcohosalmon"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.LostAndFoundBox, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("lostandfound"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.SaltDeposit, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("saltdeposit"), val.transform));
		animalPingPrefabs.Add(PingManager.AnimalType.BeachLoot, Object.Instantiate<GameObject>(assetBundle2.LoadAsset<GameObject>("beachloot"), val.transform));
		spraypaintPingPrefabs = new Dictionary<ProjectileType, GameObject>();
		spraypaintPingPrefabs.Add((ProjectileType)3, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Direction"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)4, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Clothing"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)5, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Danger"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)6, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_DeadEnd"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)7, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Avoid"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)8, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_FirstAid"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)9, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_FoodDrink"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)10, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_FireStarting"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)11, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Hunting"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)12, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Materials"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)13, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Storage"), val.transform));
		spraypaintPingPrefabs.Add((ProjectileType)14, Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("SprayPaint_Tools"), val.transform));
		foreach (KeyValuePair<PingManager.AnimalType, GameObject> animalPingPrefab in animalPingPrefabs)
		{
			animalPingPrefab.Value.active = false;
		}
		foreach (KeyValuePair<ProjectileType, GameObject> spraypaintPingPrefab in spraypaintPingPrefabs)
		{
			spraypaintPingPrefab.Value.active = false;
		}
		Object.DontDestroyOnLoad((Object)(object)val);
	}

	public static GameObject GetAnimalPrefab(PingManager.AnimalType animalType)
	{
		return animalPingPrefabs[animalType];
	}

	public static GameObject GetSpraypaintPrefab(ProjectileType pingType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return spraypaintPingPrefabs[pingType];
	}

	public override void OnUpdate()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Settings.options != null && Settings.options.displayStyle == Settings.DisplayStyle.Toggle && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.toggleKey) && Object.op_Implicit((Object)(object)PingManager.instance))
		{
			Settings.toggleBool = !Settings.toggleBool;
		}
	}
}
