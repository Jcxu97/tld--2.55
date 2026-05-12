using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MehToolBox;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shotgun;

public class Main : MelonMod
{
	[HarmonyPatch(typeof(vp_FPSCamera), "LoadAllFPSWeapons")]
	internal static class LateInitializationPatch
	{
		internal static void Postfix()
		{
			Instance?.LateInitialize();
		}
	}

	private const string AudioResourcePrefix = "Shotgun.AprilFools.Audio.";

	private bool Register = false;

	internal static Main? Instance { get; private set; }

	internal Configuration Configuration { get; private set; } = new Configuration();


	internal Settings? Settings { get; private set; }

	internal bool LateInitialized { get; private set; } = false;


	public override void OnEarlyInitializeMelon()
	{
		EarlyInitialize();
	}

	internal void EarlyInitialize()
	{
		try
		{
			MelonLogger.Msg("EarlyInitialize");
			Instance = this;
			string modsDirectory = MelonEnvironment.ModsDirectory;
			string text = Path.Combine(modsDirectory, "Shotgun");
			Directory.CreateDirectory(text);
			Settings = new Settings();
			bool flag = Versioning.IsVersionCorrect(text, "0.9.8");
			EmbeddedResourceExtractor.RefreshEmbeddedData(modsDirectory, "Shotgun.modcomponent", flag);
			if (Settings.Instance != null && Settings.Instance.EnableAprilFools)
				ExtractAprilFoolsAudio(text);
			MelonLogger.Msg("EarlyInitialize Complete");
		}
		catch (Exception value)
		{
			MelonLogger.Error($"EarlyInitialize Failed: {value}");
		}
	}

	internal static void ExtractAprilFoolsAudio(string basePath)
	{
		string text = Path.Combine(basePath, "Audio");
		if (Directory.Exists(text))
		{
			Directory.Delete(text, recursive: true);
		}
		Directory.CreateDirectory(text);
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
		int num = 0;
		string[] array = manifestResourceNames;
		foreach (string text2 in array)
		{
			if (!text2.StartsWith("Shotgun.AprilFools.Audio."))
			{
				continue;
			}
			string path = text2.Substring("Shotgun.AprilFools.Audio.".Length);
			string path2 = Path.Combine(text, path);
			using Stream stream = executingAssembly.GetManifestResourceStream(text2);
			if (stream != null)
			{
				using FileStream destination = File.Create(path2);
				stream.CopyTo(destination);
				num++;
			}
		}
		MelonLogger.Msg($"Extracted {num} april fools audio clips");
	}

	public override void OnInitializeMelon()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		RegisterTypeOptions val = new RegisterTypeOptions
		{
			Interfaces = new Il2CppInterfaceCollection((IEnumerable<Type>)new Type[1] { typeof(IResourceLocator) })
		};
		ClassInjector.RegisterTypeInIl2Cpp<ModdedResourceLocator>(val);
		ClassInjector.RegisterTypeInIl2Cpp<AnimationEventProxy>();
		ClassInjector.RegisterTypeInIl2Cpp<WildlifeSpinner>();
		ShotgunSkill.Init();
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!Register)
		{
			Addressables.AddResourceLocator(((Il2CppObjectBase)new ModdedResourceLocator()).Cast<IResourceLocator>(), (string)null, (IResourceLocation)null);
			Register = true;
		}
	}

	internal void LateInitialize()
	{
		try
		{
			string activeScene = GameManager.m_ActiveScene;
			if (activeScene.Contains("MainMenu") || activeScene.Contains("Boot"))
			{
				MelonLogger.Msg("Delaying late initialize until non-boot/main scene load");
				LateInitialized = false;
				return;
			}
			if (LateInitialized)
			{
				MelonLogger.Msg("LateInitialization already performed, skipping until next boot/mainmenu scene load");
				return;
			}
			MelonLogger.Msg("LateInitializing");
			LateInitialized = Configuration.PrepareShotgun();
			if (LateInitialized)
			{
				ShotgunAudio.Init();
				AprilFools.InitAudio();
			}
			MelonLogger.Msg(LateInitialized ? "LateInitialize Complete" : "LateInitialize Failed!");
			MelonLogger.Msg("");
		}
		catch (Exception value)
		{
			MelonLogger.Error($"LateInitialize Failed: {value}");
		}
	}
}
