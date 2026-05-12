using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using Il2CppTLD.BigCarry;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.Interactions;
using Il2CppTLD.OptionalContent;
using Il2CppTLD.Placement;
using LocalizationUtilities;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;
using UniversalTweaks;
using UniversalTweaks.Properties;
using UniversalTweaks.Tweaks;
using UniversalTweaks.Utilities;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(Mod), "Universal Tweaks", "1.4.8", "Deadman", "https://github.com/Deaadman/UniversalTweaks/releases/latest/download/UniversalTweaks.dll")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonPriority(0)]
[assembly: MelonIncompatibleAssemblies(new string[] { "DisableBreathEffect", "NonPotableToiletWater", "UnlimitedRockCaches", "ContainerTweaks" })]
[assembly: VerifyLoaderVersion("0.7.2-ci.2388", true)]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyCompany("Deadman")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Deadman")]
[assembly: AssemblyFileVersion("1.4.8.0")]
[assembly: AssemblyInformationalVersion("1.4.8")]
[assembly: AssemblyProduct("Universal Tweaks")]
[assembly: AssemblyTitle("UniversalTweaks")]
[assembly: AssemblyVersion("1.4.8.0")]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace UniversalTweaks
{
	internal sealed class Mod : MelonMod
	{
		public override void OnInitializeMelon()
		{
			LoadLocalizations();
			Settings.OnLoad();
		}

		private static void LoadLocalizations()
		{
			try
			{
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UniversalTweaks.Resources.Localization.json");
				using StreamReader streamReader = new StreamReader(stream);
				LocalizationManager.LoadJsonLocalization(streamReader.ReadToEnd());
			}
			catch (Exception ex)
			{
				Logging.LogError(ex.Message);
			}
		}
	}
}
namespace UniversalTweaks.Utilities
{
	internal static class AssetBundleLoader
	{
		internal static AssetBundle LoadBundle(string path)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			MemoryStream memoryStream = new MemoryStream((int)stream.Length);
			stream.CopyTo(memoryStream);
			return AssetBundle.LoadFromStream((Stream)new MemoryStream(Il2CppStructArray<byte>.op_Implicit(memoryStream.ToArray())));
		}
	}
	internal static class ComponentUtilities
	{
		private static readonly Dictionary<string, Dictionary<Type, Component>> StoredComponents = new Dictionary<string, Dictionary<Type, Component>>();

		internal static void RemoveComponent<T>(params string[] itemNames) where T : Component
		{
			foreach (string text in itemNames)
			{
				GearItem val = GearItem.LoadGearItemPrefab(text);
				if ((Object)(object)val == (Object)null)
				{
					continue;
				}
				T component = ((Component)val).gameObject.GetComponent<T>();
				if (!((Object)(object)component == (Object)null))
				{
					if (!StoredComponents.ContainsKey(text))
					{
						StoredComponents[text] = new Dictionary<Type, Component>();
					}
					StoredComponents[text][typeof(T)] = (Component)(object)component;
					Object.Destroy((Object)(object)component);
				}
			}
		}

		internal static void RestoreComponent<T>(params string[] itemNames) where T : Component
		{
			foreach (string text in itemNames)
			{
				if (StoredComponents.TryGetValue(text, out Dictionary<Type, Component> value) && value.TryGetValue(typeof(T), out var value2))
				{
					GearItem val = GearItem.LoadGearItemPrefab(text);
					if ((Object)(object)val != (Object)null && (Object)(object)((Component)val).gameObject.GetComponent<T>() == (Object)null)
					{
						((Component)val).gameObject.AddComponent<T>().CopyFrom(value2);
					}
				}
			}
		}

		private static void CopyFrom<T>(this T destination, Component source) where T : Component
		{
			Type type = ((object)destination).GetType();
			if (!(((object)source).GetType() != type))
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
				foreach (FieldInfo fieldInfo in fields)
				{
					fieldInfo.SetValue(destination, fieldInfo.GetValue(source));
				}
			}
		}
	}
	internal enum FlashlightBeamColor
	{
		Default,
		Blue,
		Green,
		Orange,
		Purple,
		Red,
		White,
		Yellow,
		Custom
	}
	internal static class Logging
	{
		internal static void Log(string message, params object[] parameters)
		{
			Melon<Mod>.Logger.Msg(message, parameters);
		}

		internal static void LogDebug(string message, params object[] parameters)
		{
			Melon<Mod>.Logger.Msg("[DEBUG] " + message, parameters);
		}

		internal static void LogWarning(string message, params object[] parameters)
		{
			Melon<Mod>.Logger.Warning(message, parameters);
		}

		internal static void LogError(string message, params object[] parameters)
		{
			Melon<Mod>.Logger.Error(message, parameters);
		}

		internal static void LogException(string message, Exception exception, params object[] parameters)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[EXCEPTION]");
			stringBuilder.Append(message);
			stringBuilder.AppendLine(exception.Message);
			Melon<Mod>.Logger.Error(stringBuilder.ToString(), new object[2]
			{
				Color.red,
				parameters
			});
		}
	}
	internal static class TextureSwapper
	{
		[HarmonyPatch(typeof(Utils), "GetInventoryIconTexture", new Type[] { typeof(GearItem) })]
		private static class GenericIconTextureSwap
		{
			private static bool Prefix(GearItem gi, ref Texture2D __result)
			{
				if ((Object)(object)gi == (Object)null)
				{
					return true;
				}
				string textureNameForGearItem = TextureSwap.GetTextureNameForGearItem(gi);
				if (string.IsNullOrEmpty(textureNameForGearItem))
				{
					return true;
				}
				Dictionary<string, Texture2D> dictionary = LoadTexturesFromAssetBundle();
				if (dictionary.Count == 0)
				{
					return true;
				}
				if (!dictionary.TryGetValue(textureNameForGearItem, out var value))
				{
					return true;
				}
				__result = value;
				return false;
			}
		}

		private static readonly AssetBundle? UniversalTweaksAssetBundle = AssetBundleLoader.LoadBundle("UniversalTweaks.Resources.UniversalTweaksAssetBundle");

		private static readonly Dictionary<string, Texture2D> Textures = LoadTexturesFromAssetBundle();

		private static Dictionary<string, Texture2D> LoadTexturesFromAssetBundle()
		{
			Dictionary<string, Texture2D> dictionary = new Dictionary<string, Texture2D>();
			if ((Object)(object)UniversalTweaksAssetBundle == (Object)null)
			{
				return dictionary;
			}
			foreach (Texture2D item in UniversalTweaksAssetBundle.LoadAllAssets<Texture2D>())
			{
				dictionary[((Object)item).name] = item;
			}
			return dictionary;
		}

		internal static void SwapGearItemTexture(string gearItemName, string gameObjectName, string newTextureName)
		{
			if (!Textures.TryGetValue(newTextureName, out Texture2D value))
			{
				return;
			}
			GearItem val = GearItem.LoadGearItemPrefab(gearItemName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			foreach (Renderer componentsInChild in ((Component)val).GetComponentsInChildren<Renderer>(true))
			{
				if (((Object)((Component)componentsInChild).gameObject).name != gameObjectName)
				{
					continue;
				}
				foreach (Material item in (Il2CppArrayBase<Material>)(object)componentsInChild.materials)
				{
					item.mainTexture = (Texture)(object)value;
				}
			}
		}
	}
}
namespace UniversalTweaks.Tweaks
{
	internal static class Breath
	{
		[HarmonyPatch(typeof(Breath), "PlayBreathEffect")]
		private static class BreathVisibility
		{
			private static void Postfix(Breath __instance)
			{
				__instance.m_SuppressEffects = !Settings.Instance.BreathVisibility;
			}
		}
	}
	internal static class Container
	{
		[HarmonyPatch(typeof(Container), "Awake")]
		private class AdjustContainerCapacityAmount
		{
			private static void Postfix(Container __instance)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00db: Unknown result type (might be due to invalid IL or missing references)
				//IL_0103: Unknown result type (might be due to invalid IL or missing references)
				//IL_012b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0153: Unknown result type (might be due to invalid IL or missing references)
				//IL_017b: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
				//IL_021b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0243: Unknown result type (might be due to invalid IL or missing references)
				//IL_026b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0293: Unknown result type (might be due to invalid IL or missing references)
				//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_030b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0333: Unknown result type (might be due to invalid IL or missing references)
				//IL_035b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0383: Unknown result type (might be due to invalid IL or missing references)
				//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0423: Unknown result type (might be due to invalid IL or missing references)
				//IL_044b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0473: Unknown result type (might be due to invalid IL or missing references)
				//IL_049b: Unknown result type (might be due to invalid IL or missing references)
				//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
				//IL_0525: Unknown result type (might be due to invalid IL or missing references)
				//IL_055f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0587: Unknown result type (might be due to invalid IL or missing references)
				//IL_05af: Unknown result type (might be due to invalid IL or missing references)
				//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
				//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0627: Unknown result type (might be due to invalid IL or missing references)
				//IL_064f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0677: Unknown result type (might be due to invalid IL or missing references)
				//IL_069f: Unknown result type (might be due to invalid IL or missing references)
				//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
				//IL_06ef: Unknown result type (might be due to invalid IL or missing references)
				//IL_0729: Unknown result type (might be due to invalid IL or missing references)
				//IL_0763: Unknown result type (might be due to invalid IL or missing references)
				//IL_078b: Unknown result type (might be due to invalid IL or missing references)
				//IL_07b3: Unknown result type (might be due to invalid IL or missing references)
				//IL_07db: Unknown result type (might be due to invalid IL or missing references)
				//IL_0803: Unknown result type (might be due to invalid IL or missing references)
				//IL_082b: Unknown result type (might be due to invalid IL or missing references)
				//IL_08a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_087b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0865: Unknown result type (might be due to invalid IL or missing references)
				//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
				if (Settings.Instance.InfiniteContainerWeight)
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(10000f);
					return;
				}
				if (((Object)__instance).name.Contains("CarSedanGloveBox_Prefab"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerGloveBoxCapacity);
				}
				else if (((Object)__instance).name.Contains("CarSedanTrunkDoor_Prefab"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCarTrunkCapacity);
				}
				else if (((Object)__instance).name.Contains("CarTruckGloveBox_Prefab"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerGloveBoxCapacity);
				}
				if (((Object)__instance).name.Contains("CONTAINER_BackPack"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerBackpackCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_BathroomCabinet"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCabinetLgeCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_Briefcase"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerBriefcaseCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_CacheStoreCommon"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerHiddenCacheCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_CacheStoreRare"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerPlasticContainerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_CoalBin"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCoalBinCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_Cooler"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCoolerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_Dryer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerDryerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_FirewoodBin"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerFirewoodBinCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_FirstAidKit"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerFirstAidCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_StorageGunLocker"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerGunLockerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_ForestryCrate"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerSupplyBinCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_LargeCabinet"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCabinetLgeCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_LilysChest"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerTrunkCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_LockBoxB"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerLockBoxCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_LockerA"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerLockerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_MedicineShelf"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerMedicineShelfCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_MetalBox"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerMetalContainerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_MetalLocker"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerLockerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_PlasticBox"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerPlasticContainerCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_Safe"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerSafeCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_SmallCabinet"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCabinetSmlCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_SteamerTrunk"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerTrunkCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_StoneCabinATrapDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerHatchCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_TrashCanister"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerTrashCanCapacity);
				}
				else if (((Object)__instance).name.Contains("CONTAINER_Washer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerWasherCapacity);
				}
				else if (((Object)__instance).name.Contains("GEAR_RockCache_Prefab"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerRockCacheCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_CargoCrateBottomDoor") || ((Object)__instance).name.Contains("OBJ_CargoCrateTopDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCargoContainerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_CashRegisterDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCashRegisterCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_DresserDrawer") || ((Object)__instance).name.Contains("OBJ_DresserTallDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerDresserDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_EndTableDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerEndTableDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_CupboardDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCupboardCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_FishingCabinCupboardDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCupboardCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_FishingCabinDresserDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerFishingHutDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_FridgeBottomDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerFridgeCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_FridgeTopDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerFreezerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_InfirmaryDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerInfirmaryDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_GasOvenDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerOvenCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_KitchenCabinetDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerKitchenCabinetCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_KitchenDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerKitchenDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_MetalDeskDrawer1") || ((Object)__instance).name.Contains("OBJ_MetalDeskDrawer4"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerDeskDrawerLgeCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_MetalDeskDrawer2") || ((Object)__instance).name.Contains("OBJ_MetalDeskDrawer3"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerDeskDrawerSmlCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_MetalFileCabinetDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerFileCabinetCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_WorkBenchDrawer"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerWorkbenchDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_MetalLockerDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerLockerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_SmallCabinetDoor"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerCabinetSmlCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_Suitcase"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerSuitcaseCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_ToolCabinetDrawer"))
				{
					if (((Object)__instance).name.Contains("OBJ_ToolCabinetDrawerE"))
					{
						__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerToolCabinetDrawerLgeCapacity);
					}
					else
					{
						__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerToolCabinetDrawerSmlCapacity);
					}
				}
				else if (((Object)__instance).name.Contains("OBJ_WardenDesk"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerWardenDeskDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("OBJ_TrailerInteriorDeskDrawerLg_Prefab"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerWoodDeskDrawerCapacity);
				}
				else if (((Object)__instance).name.Contains("STR_BankAVaultDepositBox"))
				{
					__instance.m_Capacity = ItemWeight.FromKilograms(Settings.Instance.ContainerSafetyDepositBoxCapacity);
				}
			}
		}
	}
	internal static class Decals
	{
		[HarmonyPatch(typeof(DynamicDecalsManager), "RenderDynamicDecal")]
		private static class GlowingDecals
		{
			private static bool Prefix(DynamicDecalsManager __instance)
			{
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				if (!Settings.Instance.GlowingDecals && (Object)(object)__instance.m_GlowMaterial == (Object)null)
				{
					return true;
				}
				__instance.m_GlowMaterial.SetColor("_GlowColor", new Color(1f, 0.4489248f, 0f, 0f));
				__instance.m_GlowMaterial.SetFloat("_GlowMult", Settings.Instance.GlowingDecalMultiplier);
				__instance.m_AnimatedRevealMaterial = __instance.m_GlowMaterial;
				return true;
			}
		}

		[HarmonyPatch(typeof(Panel_SprayPaint), "Enable", new Type[] { typeof(bool) })]
		private static class DecalRestrictions
		{
			private static void Postfix()
			{
				GameManager.GetDynamicDecalsManager().m_DecalOverlapLeniencyPercent = Settings.Instance.DecalOverlapLeniency;
			}
		}
	}
	internal static class Encumber
	{
		[HarmonyPatch(typeof(Encumber), "Start")]
		private class StartEncumberTweaks
		{
			private static void Postfix(Encumber __instance)
			{
				EncumberUpdate(__instance);
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "CalculateModifiedCalorieBurnRate", new Type[] { typeof(float) })]
		private class CalorieBurnRateFix
		{
			private static void Postfix(PlayerManager __instance, float baseBurnRate, ref float __result)
			{
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_0079: Unknown result type (might be due to invalid IL or missing references)
				float num = baseBurnRate;
				if (__instance.PlayerIsSprinting() || __instance.PlayerIsWalking() || __instance.PlayerIsClimbing())
				{
					num += GameManager.GetEncumberComponent().GetHourlyCalorieBurnFromWeight() * (30f / (float)GameManager.GetEncumberComponent().m_MaxCarryCapacity.m_Units);
				}
				if (GameManager.GetFreezingComponent().IsFreezing())
				{
					num *= GameManager.GetFreezingComponent().m_CalorieBurnMultiplier;
				}
				if (__instance.PlayerIsSprinting() || __instance.PlayerIsWalking())
				{
					Vector3 velocity = GameManager.GetVpFPSPlayer().Controller.Velocity;
					float y = ((Vector3)(ref velocity)).normalized.y;
					if (y > 0.1f)
					{
						float num2 = (y - 0.1f) / 0.5f;
						num2 = Mathf.Clamp(num2, 0f, 1f);
						float num3 = Mathf.Lerp(1f, 1.5f, num2);
						num *= num3;
					}
				}
				num *= GameManager.GetExperienceModeManagerComponent().GetCalorieBurnScale();
				float num4 = num * GameManager.GetFeatEfficientMachine().ReduceCaloriesScale();
				__result = num4;
			}
		}

		[HarmonyPatch(typeof(Encumber), "Update")]
		private static class EncumberUpdateTweaks
		{
			private static void Postfix(Encumber __instance)
			{
				EncumberUpdate(__instance);
			}
		}

		internal static void EncumberUpdate(Encumber encumber)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			if (Settings.Instance.AdditionalEncumbermentWeight > 0 || Settings.Instance.InfiniteEncumberWeight)
			{
				int num = Settings.Instance.AdditionalEncumbermentWeight;
				if (Settings.Instance.InfiniteEncumberWeight)
				{
					num = 9970;
				}
				encumber.m_MaxCarryCapacity = ItemWeight.FromKilograms(30f + (float)num);
				encumber.m_MaxCarryCapacityWhenExhausted = ItemWeight.FromKilograms(15f + (float)num);
				encumber.m_NoSprintCarryCapacity = ItemWeight.FromKilograms(40f + (float)num);
				encumber.m_NoWalkCarryCapacity = ItemWeight.FromKilograms(60f + (float)num);
				encumber.m_EncumberLowThreshold = ItemWeight.FromKilograms(31f + (float)num);
				encumber.m_EncumberMedThreshold = ItemWeight.FromKilograms(40f + (float)num);
				encumber.m_EncumberHighThreshold = ItemWeight.FromKilograms(60f + (float)num);
			}
			else
			{
				encumber.m_MaxCarryCapacity = ItemWeight.FromKilograms(30f);
				encumber.m_MaxCarryCapacityWhenExhausted = ItemWeight.FromKilograms(15f);
				encumber.m_NoSprintCarryCapacity = ItemWeight.FromKilograms(40f);
				encumber.m_NoWalkCarryCapacity = ItemWeight.FromKilograms(60f);
				encumber.m_EncumberLowThreshold = ItemWeight.FromKilograms(31f);
				encumber.m_EncumberMedThreshold = ItemWeight.FromKilograms(40f);
				encumber.m_EncumberHighThreshold = ItemWeight.FromKilograms(60f);
			}
		}
	}
	internal static class FeatProgress
	{
		[HarmonyPatch(typeof(Feat), "ShouldBlockIncrement")]
		private static class FeatProgressInCustomMode
		{
			private static void Postfix(ref bool __result)
			{
				if (Settings.Instance.FeatProgressInCustomMode)
				{
					__result = false;
				}
			}
		}
	}
	internal static class Flashlight
	{
		[HarmonyPatch(typeof(FlashlightItem), "Awake")]
		private static class FlashlightCustomization
		{
			private static void Prefix(FlashlightItem __instance)
			{
				if (Settings.Instance.BatteryRandomization)
				{
					__instance.m_CurrentBatteryCharge = Random.Range(0f, 1f);
				}
			}
		}

		[HarmonyPatch(typeof(FlashlightItem), "EnableLights")]
		private static class FlashlightBeamDropped
		{
			private static bool Prefix(FlashlightItem __instance, State state)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Invalid comparison between Unknown and I4
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Invalid comparison between Unknown and I4
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_004e: Invalid comparison between Unknown and I4
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Invalid comparison between Unknown and I4
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_008f: Invalid comparison between Unknown and I4
				//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Invalid comparison between Unknown and I4
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Invalid comparison between Unknown and I4
				if (!Settings.Instance.ExtendedFunctionality)
				{
					return true;
				}
				__instance.m_AuroraField.Enable((int)state == 2);
				if ((Object)(object)__instance.m_GearItem == (Object)(object)GameManager.GetPlayerManagerComponent().m_ItemInHands)
				{
					state = (State)0;
				}
				__instance.m_FxObjectLow.SetActive((int)state == 1);
				__instance.m_FxObjectHigh.SetActive((int)state == 2);
				bool flag = GameManager.GetWeatherComponent().UseOutdoorLightingForLightSources();
				Light lightIndoor = __instance.m_LightIndoor;
				bool flag2 = !flag;
				if (flag2)
				{
					bool flag3 = state - 1 <= 1;
					flag2 = flag3;
				}
				((Behaviour)lightIndoor).enabled = flag2;
				((Behaviour)__instance.m_LightIndoorHigh).enabled = !flag && (int)state == 2;
				lightIndoor = __instance.m_LightOutdoor;
				flag2 = flag;
				if (flag2)
				{
					bool flag3 = state - 1 <= 1;
					flag2 = flag3;
				}
				((Behaviour)lightIndoor).enabled = flag2;
				((Behaviour)__instance.m_LightOutdoorHigh).enabled = flag && (int)state == 2;
				return false;
			}
		}

		[HarmonyPatch(typeof(FlashlightItem), "GetNormalizedCharge")]
		private static class FlashlightKeepBatteryCharge
		{
			private static bool Prefix(FlashlightItem __instance, ref float __result)
			{
				if (!Settings.Instance.ExtendedFunctionality)
				{
					return true;
				}
				__result = __instance.m_CurrentBatteryCharge;
				return false;
			}
		}

		[HarmonyPatch(typeof(FlashlightItem), "IsLit")]
		private static class FlashlightFunctionality
		{
			private static bool Prefix(FlashlightItem __instance, ref bool __result)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Invalid comparison between Unknown and I4
				if (Settings.Instance.HighBeamRestrictions && (int)__instance.m_State == 2 && !GameManager.GetAuroraManager().AuroraIsActive())
				{
					GameAudioManager.PlayGUIError();
					HUDMessage.AddMessage(Localization.Get("GAMEPLAY_StateHighFail"), false, false);
					__result = __instance.IsOn();
					return true;
				}
				__result = __instance.IsOn();
				return false;
			}
		}

		[HarmonyPatch(typeof(FlashlightItem), "Update")]
		private static class FlashlightBatteryDrain
		{
			private static void Postfix(FlashlightItem __instance)
			{
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Invalid comparison between Unknown and I4
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Invalid comparison between Unknown and I4
				float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
				if (!GameManager.GetAuroraManager().AuroraIsActive())
				{
					if ((int)__instance.m_State == 1)
					{
						__instance.m_CurrentBatteryCharge -= tODHours / __instance.m_LowBeamDuration;
					}
					else if (!Settings.Instance.HighBeamRestrictions && (int)__instance.m_State == 2)
					{
						__instance.m_CurrentBatteryCharge -= tODHours / __instance.m_HighBeamDuration;
					}
					if (__instance.m_CurrentBatteryCharge <= 0f)
					{
						__instance.m_CurrentBatteryCharge = 0f;
						__instance.m_State = (State)0;
					}
				}
				if (Settings.Instance.InfiniteBattery)
				{
					__instance.m_CurrentBatteryCharge = 1f;
				}
				bool flag = (Object)(object)__instance.m_GearItem != (Object)null && ((Object)__instance.m_GearItem).name == "GEAR_Flashlight_LongLasting";
				__instance.m_LowBeamDuration = ((!Settings.Instance.CheatingTweaks) ? (flag ? 1.5f : 1f) : (flag ? Settings.Instance.MinersFlashlightLowBeamDuration : Settings.Instance.FlashlightLowBeamDuration));
				__instance.m_HighBeamDuration = ((!Settings.Instance.CheatingTweaks) ? 0.08333334f : (flag ? Settings.Instance.MinersFlashlightHighBeamDuration : Settings.Instance.FlashlightHighBeamDuration));
				__instance.m_RechargeTime = ((!Settings.Instance.CheatingTweaks) ? (flag ? 1.75f : 2f) : (flag ? Settings.Instance.MinersFlashlightRechargeTime : Settings.Instance.FlashlightRechargeTime));
				UpdateFlashlightBeamColor(__instance, __instance.m_FxObjectLow, __instance.m_FxObjectHigh);
			}
		}

		[HarmonyPatch(typeof(FirstPersonFlashlight), "Update")]
		private static class ModifyFlashFlashlightBeamColor
		{
			private static void Postfix(FirstPersonFlashlight __instance)
			{
				GearItem itemInHands = GameManager.GetPlayerManagerComponent().m_ItemInHands;
				if (!((Object)(object)itemInHands == (Object)null))
				{
					UpdateFlashlightBeamColor(itemInHands.m_FlashlightItem, __instance.m_LowFxGameObject, __instance.m_HighFxGameObject);
				}
			}
		}

		[HarmonyPatch(typeof(LightRandomIntensity), "Update")]
		private static class FlashlightFlicker
		{
			private static bool Prefix(LightRandomIntensity __instance)
			{
				if (!Settings.Instance.AuroraFlickering)
				{
					return true;
				}
				bool flag;
				switch (((Object)((Component)__instance).gameObject).name)
				{
				case "LightIndoors":
				case "LightOutdoors":
				case "LightExtend":
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag && !GameManager.GetAuroraManager().AuroraIsActive() && Settings.Instance.ExtendedFunctionality)
				{
					return false;
				}
				return true;
			}
		}

		private static Color GetColorFromFlashlight(FlashlightItem flashlightItem)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			GearItem gearItem = flashlightItem.m_GearItem;
			bool flag = ((gearItem != null) ? ((Object)gearItem).name : null) == "GEAR_Flashlight_LongLasting";
			return GetColorFromSettings(flag ? Settings.Instance.MinersFlashlightBeamColor : Settings.Instance.FlashlightBeamColor, flag);
		}

		private static Color GetColorFromSettings(FlashlightBeamColor beamColor, bool isMinersFlashlight)
		{
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			return (Color)(beamColor switch
			{
				FlashlightBeamColor.Custom => new Color((float)(isMinersFlashlight ? Settings.Instance.MinersFlashlightRedValue : Settings.Instance.FlashlightRedValue) / 255f, (float)(isMinersFlashlight ? Settings.Instance.MinersFlashlightGreenValue : Settings.Instance.FlashlightGreenValue) / 255f, (float)(isMinersFlashlight ? Settings.Instance.MinersFlashlightBlueValue : Settings.Instance.FlashlightBlueValue) / 255f), 
				FlashlightBeamColor.Default => new Color(0.7215686f, 69f / 85f, 0.9176471f), 
				FlashlightBeamColor.Red => Color.red, 
				FlashlightBeamColor.Green => Color.green, 
				FlashlightBeamColor.Blue => Color.blue, 
				FlashlightBeamColor.Yellow => Color.yellow, 
				FlashlightBeamColor.Purple => new Color(0.5f, 0f, 0.5f), 
				FlashlightBeamColor.Orange => new Color(1f, 0.6470588f, 0f), 
				FlashlightBeamColor.White => Color.white, 
				_ => new Color(0.7215686f, 69f / 85f, 0.9176471f), 
			});
		}

		private static void UpdateFlashlightBeamColor(FlashlightItem flashlightItem, GameObject lowFxGameObject, GameObject highFxGameObject)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)flashlightItem == (Object)null))
			{
				Color colorFromFlashlight = GetColorFromFlashlight(flashlightItem);
				if ((Object)(object)lowFxGameObject != (Object)null)
				{
					UpdateLightColorRecursive(lowFxGameObject.transform, colorFromFlashlight);
				}
				if ((Object)(object)highFxGameObject != (Object)null)
				{
					UpdateLightColorRecursive(highFxGameObject.transform, colorFromFlashlight);
				}
			}
		}

		private static void UpdateLightColorRecursive(Transform transform, Color color)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)transform == (Object)null)
			{
				return;
			}
			Light component = ((Component)transform).GetComponent<Light>();
			if ((Object)(object)component == (Object)null)
			{
				return;
			}
			component.color = color;
			for (int i = 0; i < transform.childCount; i++)
			{
				if ((Object)(object)transform.GetChild(i) != (Object)null)
				{
					UpdateLightColorRecursive(transform.GetChild(i), color);
				}
			}
		}
	}
	internal static class Food
	{
		[HarmonyPatch(typeof(GearItem), "Deserialize")]
		private static class RemoveHeadacheComponents
		{
			private static void Postfix(GearItem __instance)
			{
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				string[] itemNames = new string[4] { "GEAR_CookedPiePeach", "GEAR_CookedPieRoseHip", "GEAR_CookedPorridgeFruit", "GEAR_CookedPancakePeach" };
				if (Settings.Instance.RemoveHeadacheDebuffFromFoods)
				{
					ComponentUtilities.RemoveComponent<CausesHeadacheDebuff>(itemNames);
				}
				else
				{
					ComponentUtilities.RestoreComponent<CausesHeadacheDebuff>(itemNames);
				}
				string name = ((Object)((Component)__instance).gameObject).name;
				if (name == "GEAR_CookedStewMeat" || name == "GEAR_CookedStewVegetables")
				{
					((Component)__instance).gameObject.GetComponentInParent<FoodStatEffect>().m_Effect = Settings.Instance.ReduceStewFatigueLossAmount;
				}
				if (Settings.Instance.ConsistantDressingWeight && ((Object)((Component)__instance).gameObject).name == "GEAR_OldMansBeardDressing")
				{
					__instance.m_GearItemData.m_BaseWeight = ItemWeight.FromKilograms(0.03f);
				}
			}
		}
	}
	internal static class Guns
	{
		[HarmonyPatch(typeof(vp_FPSPlayer), "Update")]
		private static class RevolverMovementUnblocked
		{
			private static void Postfix(vp_FPSPlayer __instance)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Invalid comparison between Unknown and I4
				if (Settings.Instance.RevolverImprovements && (int)GameManager.GetPlayerManagerComponent().GetControlMode() == 18 && GameManager.IsMoveInputUnblocked())
				{
					__instance.InputWalk();
				}
			}
		}

		[HarmonyPatch(typeof(Panel_HUD), "Update")]
		private static class RevolverLimitedMobilityUiDisable
		{
			private static void Postfix(Panel_HUD __instance)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Invalid comparison between Unknown and I4
				if (Settings.Instance.RevolverImprovements && (int)GameManager.GetPlayerManagerComponent().GetControlMode() == 18)
				{
					((Component)__instance.m_AimingLimitedMobility).gameObject.SetActive(false);
				}
			}
		}
	}
	internal static class Miscellaneous
	{
		[HarmonyPatch(typeof(GearItem), "Drop")]
		public class RandomizedItemRotation
		{
			private static void Postfix(GearItem __instance)
			{
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)__instance == (Object)null) && Settings.Instance.RandomizedItemRotationDrops)
				{
					Transform transform = ((Component)__instance).transform;
					float num = Random.Range(0f, 360f);
					transform.eulerAngles = (((Object)__instance).name.Contains("GEAR_Rifle") ? new Vector3(transform.eulerAngles.x, num, 90f) : new Vector3(0f, num, 0f));
				}
			}
		}
	}
	internal static class Noisemaker
	{
		[HarmonyPatch(typeof(GearItem), "Deserialize")]
		private static class AdjustNoisemakerValues
		{
			private static void Postfix(GearItem __instance)
			{
				if (((Object)((Component)__instance).gameObject).name == "GEAR_NoiseMaker")
				{
					__instance.m_NoiseMakerItem.m_BurnLifetimeMinutes = Settings.Instance.NoisemakerBurnLength;
					__instance.m_NoiseMakerItem.m_ThrowForce = Settings.Instance.NoisemakerThrowForce;
				}
			}
		}
	}
	internal static class Respirator
	{
		[HarmonyPatch(typeof(Respirator), "AttachCanister")]
		private static class ModifyCanisterDuration
		{
			private static void Postfix(Respirator __instance, RespiratorCanister canister)
			{
				canister.m_ProtectionDurationRTSeconds = Settings.Instance.RespiratorCanisterDuration;
			}
		}
	}
	internal static class RockCache
	{
		[HarmonyPatch(typeof(RockCacheManager), "CanAttemptToPlaceRockCache")]
		public static class RockCacheRadialMenuIndoors
		{
			private static bool Prefix(ref bool __result)
			{
				if (!Settings.Instance.AllowedIndoorsRockCaches)
				{
					return true;
				}
				__result = true;
				return false;
			}

			private static void Postfix(RockCacheManager __instance)
			{
				__instance.m_MaxRockCachesPerRegion = Settings.Instance.MaximumPerRegionRockCaches;
				__instance.m_MinDistanceBetweenRockCaches = Settings.Instance.MinimumDistanceBetweenRockCaches;
			}
		}

		[HarmonyPatch(typeof(Panel_Actions), "OnPlaceRockCache")]
		public static class RockCachePlacementIndoors
		{
			private static bool Prefix()
			{
				if (!Settings.Instance.AllowedIndoorsRockCaches)
				{
					return true;
				}
				if (!GameManager.GetRockCacheManager().CanAttemptToPlaceRockCache())
				{
					GameAudioManager.PlayGUIError();
					return false;
				}
				string missingMaterialsString = GameManager.GetRockCacheManager().GetMissingMaterialsString();
				if (missingMaterialsString != null)
				{
					HUDMessage.AddMessage(missingMaterialsString, false, false);
					return false;
				}
				GameAudioManager.PlayGUIButtonClick();
				GameObject val = Object.Instantiate<GameObject>(((Component)GameManager.GetRockCacheManager().m_RockCachePrefab).gameObject);
				if ((Object)(object)val == (Object)null)
				{
					return false;
				}
				((Object)val).name = ((Object)GameManager.GetRockCacheManager().m_RockCachePrefab).name;
				val.SetActive(false);
				GameManager.GetPlayerManagerComponent().StartPlaceMesh(val, GameManager.GetRockCacheManager().m_BuildRangeMax, (PlaceMeshFlags)1, (PlaceMeshRules)1);
				return false;
			}
		}
	}
	internal static class SnowShelter
	{
		[HarmonyPatch(typeof(SnowShelterManager), "InstantiateSnowShelter")]
		private static class SnowShelterDecayRate
		{
			private static void Postfix(ref SnowShelter __result)
			{
				__result.m_DailyDecayHP = (Settings.Instance.CheatingTweaks ? Settings.Instance.SnowShelterDailyDecayRate : 100);
			}
		}
	}
	internal static class TextureSwap
	{
		[HarmonyPatch(typeof(GearItem), "Deserialize")]
		private static class SwapGearItemTextures
		{
			private static void Postfix()
			{
				if (Settings.Instance.MRETextureVariant)
				{
					TextureSwapper.SwapGearItemTexture("GEAR_MRE", "Obj_FoodMRE_LOD0", "GEAR_FoodBrownMRE_Dif");
				}
			}
		}

		internal static string GetTextureNameForGearItem(GearItem gi)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string> { { "GEAR_MRE", "ico_GearItem__BrownMRE" } };
			if (((Object)gi).name == "GEAR_MRE" && !Settings.Instance.MRETextureVariant)
			{
				return string.Empty;
			}
			if (!dictionary.TryGetValue(((Object)gi).name, out var value))
			{
				return string.Empty;
			}
			return value;
		}
	}
	internal static class Travois
	{
		[HarmonyPatch(typeof(TravoisBigCarryItem), "CanPerformInteractionWhileCarrying")]
		private static class OverrideInteractionRestrictionsWhileCarrying
		{
			private static void Postfix(ref bool __result, IInteraction interaction)
			{
				if (Settings.Instance.OverrideTravoisInteractionRestrictions)
				{
					if (GameManager.GetPlayerInVehicle().IsEntering() || GameManager.GetSnowShelterManager().PlayerEnteringShelter())
					{
						__result = false;
					}
					else
					{
						__result = true;
					}
				}
			}
		}

		[HarmonyPatch(typeof(TravoisMovement), "CheckMovementRestriction")]
		private static class OverrideMovementRestrictions
		{
			private static void Postfix(ref CarryDisplayError __result)
			{
				if (Settings.Instance.OverrideTravoisMovementRestrictions)
				{
					__result = (CarryDisplayError)(-1);
				}
			}
		}

		[HarmonyPatch(typeof(TravoisBigCarryItem), "OnCarried")]
		private static class AllTravoisTweaks
		{
			private static void Postfix(TravoisBigCarryItem __instance)
			{
				__instance.m_TravoisMovement.m_TurnSpeed = Settings.Instance.TurnSpeedTravois;
				__instance.m_TravoisMovement.m_MaxSlopeClimbAngle = Settings.Instance.MaximumSlopeAngleTravois;
				__instance.m_TravoisMovement.m_MaxSlopeDownhillAngle = Settings.Instance.MaximumSlopeAngleTravois;
				__instance.m_BlizzardDecayPerHour = (Settings.Instance.CheatingTweaks ? Settings.Instance.DecayBlizzardTravois : 3);
				__instance.m_DecayHPPerHour = (Settings.Instance.CheatingTweaks ? ((float)Settings.Instance.DecayHPPerHourTravois / 1000f) : 0.01f);
				__instance.m_MovementDecayPerUnit = (Settings.Instance.CheatingTweaks ? ((float)Settings.Instance.DecayMovementPerUnitTravois / 100f) : 0.05f);
			}
		}
	}
	internal static class UserInterface
	{
		[HarmonyPatch(typeof(HUDManager), "UpdateCrosshair")]
		private static class PermanentCrosshair
		{
			private static bool Prefix(HUDManager __instance)
			{
				if (Settings.Instance.PermanentCrosshair)
				{
					__instance.m_CrosshairAlpha = 1f;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Panel_ActionsRadial), "GetShouldGreyOut")]
		private static class GreyOutSprayPaintRadial
		{
			private static bool Prefix(RadialType radialType, ref bool __result)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0003: Invalid comparison between Unknown and I4
				if ((int)radialType != 26)
				{
					return true;
				}
				__result = (Object)(object)GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_SprayPaintCan") == (Object)null;
				return false;
			}
		}

		[HarmonyPatch(typeof(Panel_FeedFire), "Initialize")]
		private static class FireSpriteFix
		{
			private static void Postfix(Panel_FeedFire __instance)
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)__instance.m_Sprite_FireFill == (Object)null))
				{
					((Component)__instance.m_Sprite_FireFill).gameObject.transform.localPosition = new Vector3(159.1f, -31.6f, 0f);
					((Component)__instance.m_Sprite_FireFill).gameObject.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_MainMenu), "Initialize")]
		private class RemoveOptionalContentMenus
		{
			private static void Postfix(Panel_MainMenu __instance)
			{
				if (Settings.Instance.RemoveMainMenuItems)
				{
					bool num = OptionalContentManager.Instance.IsContentOwned(__instance.m_WintermuteConfig);
					RemoveMainMenuItem((MainMenuItemType)2, __instance);
					if (!num)
					{
						RemoveMainMenuItem((MainMenuItemType)1, __instance);
					}
				}
			}

			private static void RemoveMainMenuItem(MainMenuItemType removeType, Panel_MainMenu __instance)
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				for (int num = __instance.m_MenuItems.Count - 1; num >= 0; num--)
				{
					if (__instance.m_MenuItems[num].m_Type == removeType)
					{
						__instance.m_MenuItems.RemoveAt(num);
					}
				}
			}
		}
	}
	internal static class Water
	{
		[HarmonyPatch(typeof(WaterSource), "Update")]
		private static class NonPotableToiletWater
		{
			private static void Postfix(WaterSource __instance)
			{
				__instance.m_CurrentLiquidQuality = (LiquidQuality)(Settings.Instance.ToiletWaterQuality == 1);
			}
		}
	}
}
namespace UniversalTweaks.Properties
{
	internal static class BuildInfo
	{
		public const string Name = "Universal Tweaks";

		public const string Version = "1.4.8";

		public const string Author = "Deadman";

		public const string DownloadLink = "https://github.com/Deaadman/UniversalTweaks/releases/latest/download/UniversalTweaks.dll";

		public const int Priority = 0;

		public const string MelonLoaderVersion = "0.7.2-ci.2388";
	}
	internal class Settings : JsonModSettings
	{
		[Section("常用修改")]
		[Name("额外负重能力")]
		[Description("在玩家现有负重的基础上额外增加的负重")]
		[Slider(0f, 120f, NumberFormat = "{0:0.##} KG")]
		public int AdditionalEncumbermentWeight;

		[Name("启用或禁用哈气")]
		[Description("开启或关闭呼吸的视觉特效")]
		public bool BreathVisibility = true;

		[Name("胡须地衣伤口敷料重量调整")]
		[Description("等同于胡须地衣的重量")]
		public bool ConsistantDressingWeight;

		[Name("自定义模式解锁徽章进度")]
		[Description("自定义模式下可解锁全徽章进度")]
		public bool FeatProgressInCustomMode;

		[Name("无限负重")]
		[Description("给予玩家几乎不受限制的负重能力")]
		public bool InfiniteEncumberWeight;

		[Name("MRE纹理替换")]
		[Description("将MRE切换成棕色纹理，需重新加载场景才可生效")]
		public bool MRETextureVariant;

		[Name("制噪器引信燃烧持续时间")]
		[Description("调整制噪器引信燃烧速度，默认=0.7分钟")]
		[Slider(0.7f, 2.7f, NumberFormat = "{0:0.##} MIN")]
		public float NoisemakerBurnLength = 0.7f;

		[Name("制噪器投掷力度")]
		[Description("调整制噪器投掷力度可以让玩家投掷的更远，默认=9")]
		[Slider(1f, 20f)]
		public int NoisemakerThrowForce = 9;

		[Name("物品掉落时随机旋转")]
		[Description("丢掉的物品随机旋转方向")]
		public bool RandomizedItemRotationDrops;

		[Name("滤罐的持续时间")]
		[Description("滤罐的持续时间以现实中的秒为单位，默认=45秒")]
		[Slider(45f, 90f, 4, NumberFormat = "{0:0.##} SEC")]
		public int RespiratorCanisterDuration = 45;

		[Name("左轮手枪操作改良")]
		[Description("左轮手枪在瞄准状态下可以自由移动")]
		public bool RevolverImprovements;

		[Name("雪屋耐久损耗率")]
		[Description("调整雪屋耐久的日损耗率，范围在50(损耗率慢2倍)到100(正常)之间。需要重新加载场景才可生效")]
		[Slider(50f, 100f)]
		public int SnowShelterDailyDecayRate = 100;

		[Name("马桶水能否饮用")]
		[Description("从马桶收集的水能否直接饮用")]
		[Choice(new string[] { "纯净水", "脏水" })]
		public int ToiletWaterQuality;

		[Section("手电筒修改")]
		[Name("极光闪烁")]
		[Description("如果启用，仅在极光出现时闪烁")]
		public bool AuroraFlickering;

		[Name("随机电池电量")]
		[Description("设置手电筒以随机的电量开启")]
		public bool BatteryRandomization;

		[Name("扩展功能")]
		[Description("使手电筒不仅可以在极光天气使用，还可以在任意时间使用")]
		public bool ExtendedFunctionality;

		[Name("远光限制")]
		[Description("手电筒的远光功能只能在极光天气使用")]
		public bool HighBeamRestrictions;

		[Name("手电筒光束颜色")]
		[Description("更改手电筒光束颜色")]
		public FlashlightBeamColor FlashlightBeamColor;

		[Name("红色数值")]
		[Slider(0f, 255f)]
		public int FlashlightRedValue;

		[Name("绿色数值")]
		[Slider(0f, 255f)]
		public int FlashlightGreenValue;

		[Name("蓝色数值")]
		[Slider(0f, 255f)]
		public int FlashlightBlueValue;

		[Name("手电筒近光持续时间")]
		[Description("修改手电筒近光持续时间，默认=1")]
		[Slider(0.08333334f, 2f, 20)]
		public float FlashlightLowBeamDuration = 1f;

		[Name("手电筒远光持续时间")]
		[Description("修改手电筒远光持续时间，范围从0.1(短)到2(长)")]
		[Slider(0.08333334f, 2f, 20)]
		public float FlashlightHighBeamDuration = 0.08333334f;

		[Name("手电筒充电时间")]
		[Description("设置手电筒电池的充电时间，范围从0(块)到2(慢)")]
		[Slider(0f, 2f, 20)]
		public float FlashlightRechargeTime = 2f;

		[Name("无限电量")]
		[Description("核能手电筒，无需充电无限使用")]
		public bool InfiniteBattery;

		[Name("矿工手电筒光束颜色")]
		[Description("更改矿工手电筒光束颜色")]
		public FlashlightBeamColor MinersFlashlightBeamColor;

		[Name("红色数值")]
		[Slider(0f, 255f)]
		public int MinersFlashlightRedValue;

		[Name("绿色数值")]
		[Slider(0f, 255f)]
		public int MinersFlashlightGreenValue;

		[Name("蓝色数值")]
		[Slider(0f, 255f)]
		public int MinersFlashlightBlueValue;

		[Name("矿工手电筒近光持续时间")]
		[Description("修改手电筒的近光持续时间，范围从0.1(短)到2(长)")]
		[Slider(0.08333334f, 2f, 20)]
		public float MinersFlashlightLowBeamDuration = 1.5f;

		[Name("矿工手电筒远光持续时间")]
		[Description("修改手电筒的远光持续时间，范围从0.1(短)到2(长)")]
		[Slider(0.08333334f, 2f, 20)]
		public float MinersFlashlightHighBeamDuration = 0.08333334f;

		[Name("矿工手电筒充电时间")]
		[Description("设置手电筒电池的充电时间，范围从0(块)到2(慢)")]
		[Slider(0f, 2f, 20)]
		public float MinersFlashlightRechargeTime = 1.75f;

		[Section("食物修改")]
		[Name("移除食物中的头疼DEBUFF")]
		[Description("移除某些食物的头疼负面效果，需要重新加载场景才能生效")]
		public bool RemoveHeadacheDebuffFromFoods;

		[Name("减少炖汤类食品的疲劳DEBUFF")]
		[Description("减少食用后的疲劳值损耗，默认=15，需要重新加载场景才能生效")]
		[Slider(0f, 15f, 15)]
		public int ReduceStewFatigueLossAmount = 15;

		[Section("岩石贮藏处修改")]
		[Name("允许室内")]
		[Description("启用的话可在室内放置岩石贮藏处")]
		public bool AllowedIndoorsRockCaches;

		[Name("每个区域的最大值")]
		[Description("设置每个区域允许堆放的最大值，默认=5")]
		[Slider(1f, 100f)]
		public int MaximumPerRegionRockCaches = 5;

		[Name("间隔最小距离")]
		[Description("设置每个岩石贮藏处间隔的最小距离，默认=10")]
		[Slider(0.3f, 10f, 97)]
		public float MinimumDistanceBetweenRockCaches = 10f;

		[Section("油漆喷灌修改")]
		[Name("喷漆贴图重叠率")]
		[Description("设置两个贴图可重叠的限度，默认=0.2")]
		[Slider(0f, 1f, 11)]
		public float DecalOverlapLeniency = 0.2f;

		[Name("高亮喷漆贴图")]
		[Description("启用喷漆贴图的高亮特效，需要重新加载场景才能生效")]
		public bool GlowingDecals;

		[Name("喷漆贴图高亮倍数")]
		[Description("调整喷漆贴图的亮度，默认=1")]
		[Slider(0.5f, 1.5f, 11)]
		public float GlowingDecalMultiplier = 1f;

		[Section("雪橇修改")]
		[Name("暴风雪中的耐久损耗")]
		[Description("在暴风雪中每小时的耐久损耗率，范围从1(速率最慢)到10(速率最快)")]
		[Slider(1f, 10f)]
		public int DecayBlizzardTravois = 3;

		[Name("常态下的耐久损耗")]
		[Description("在常态下每小时的耐久损耗率，范围从1(速率最慢)到10(速率最快)")]
		[Slider(1f, 10f, 10)]
		public int DecayHPPerHourTravois = 10;

		[Name("承载单件物品的移动速度")]
		[Description("设置雪橇每承载一件物品时对移动速度的影响，默认=5")]
		[Slider(1f, 5f, 5)]
		public int DecayMovementPerUnitTravois = 5;

		[Name("最大坡度角")]
		[Description("设置雪橇所能承受的最大坡度角，范围从35(陡峭)到75(非常陡峭)")]
		[Slider(35f, 75f)]
		public int MaximumSlopeAngleTravois = 35;

		[Name("转向速度")]
		[Description("调整雪橇的转向速度，范围从0.5(慢)到5(快)")]
		[Slider(0.5f, 5f, 45)]
		public float TurnSpeedTravois = 0.5f;

		[Name("解除雪橇的移动区域限制")]
		[Description("可在室内室外随意穿梭")]
		public bool OverrideTravoisMovementRestrictions;

		[Name("解除雪橇的互动限制")]
		[Description("拉雪橇时可与某些物体(如门)等进行互动")]
		public bool OverrideTravoisInteractionRestrictions;

		[Section("UI修改")]
		[Name("主菜单推广项目")]
		[Description("移除主菜单中的剧情模式及DLC扩展内容的推广选项")]
		public bool RemoveMainMenuItems;

		[Name("永久准星")]
		[Description("准星始终开启，永远不会隐藏")]
		public bool PermanentCrosshair;

		[Section("容器容量修改")]
		[Name("无限容量")]
		[Description("将所有容器的容量设置成几乎无限大的数值")]
		public bool InfiniteContainerWeight;

		[Name("背包")]
		[Description("修改背包容量，默认=15KG")]
		[Slider(0f, 30f, 31, NumberFormat = "{0:0.##} KG")]
		public float ContainerBackpackCapacity = 15f;

		[Name("公文包")]
		[Description("修改公文包容量，默认=10KG")]
		[Slider(0f, 30f, 31, NumberFormat = "{0:0.##} KG")]
		public float ContainerBriefcaseCapacity = 10f;

		[Name("小橱柜")]
		[Description("修改小橱柜容量(参考家用储物矮柜)，默认=20KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerCabinetSmlCapacity = 20f;

		[Name("大橱柜")]
		[Description("修改大橱柜容量(参考家用衣柜)，默认=40KG")]
		[Slider(0f, 200f, 201, NumberFormat = "{0:0.##} KG")]
		public float ContainerCabinetLgeCapacity = 40f;

		[Name("集装箱")]
		[Description("修改集装箱容量(林狼雪岭)，默认=30KG")]
		[Slider(0f, 250f, 251, NumberFormat = "{0:0.##} KG")]
		public float ContainerCargoContainerCapacity = 30f;

		[Name("收银台")]
		[Description("修改收银台容量，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerCashRegisterCapacity = 5f;

		[Name("煤箱")]
		[Description("修改煤箱出纳机容量，默认=60KG")]
		[Slider(0f, 250f, 251, NumberFormat = "{0:0.##} KG")]
		public float ContainerCoalBinCapacity = 60f;

		[Name("冷藏柜")]
		[Description("修改冷藏柜容量，默认=20KG")]
		[Slider(0f, 150f, 151, NumberFormat = "{0:0.##} KG")]
		public float ContainerCoolerCapacity = 20f;

		[Name("钓鱼小屋橱柜")]
		[Description("修改钓鱼小屋橱柜容量，默认=15KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerCupboardCapacity = 15f;

		[Name("梳妆台抽屉")]
		[Description("修改梳妆台抽屉容量，默认=5KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerDresserDrawerCapacity = 5f;

		[Name("烘干机")]
		[Description("修改烘干机容量，默认=30KG")]
		[Slider(0f, 500f, 501, NumberFormat = "{0:0.##} KG")]
		public float ContainerDryerCapacity = 30f;

		[Name("文件柜")]
		[Description("修改文件柜容量，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerFileCabinetCapacity = 10f;

		[Name("医药箱")]
		[Description("修改医药箱容量，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerFirstAidCapacity = 5f;

		[Name("柴火箱")]
		[Description("修改柴火箱容量，默认=30KG")]
		[Slider(0f, 500f, 501, NumberFormat = "{0:0.##} KG")]
		public float ContainerFirewoodBinCapacity = 30f;

		[Name("钓鱼小屋抽屉")]
		[Description("修改钓鱼小屋抽屉容量，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerFishingHutDrawerCapacity = 10f;

		[Name("冰柜")]
		[Description("修改冰柜容量，默认=20KG")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0.##} KG")]
		public float ContainerFreezerCapacity = 20f;

		[Name("冰箱")]
		[Description("修改冰箱容量，默认=40KG")]
		[Slider(0f, 200f, 201, NumberFormat = "{0:0.##} KG")]
		public float ContainerFridgeCapacity = 40f;

		[Name("副驾驶杂物盒")]
		[Description("修改副驾驶杂物盒容量，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerGloveBoxCapacity = 5f;

		[Name("枪柜")]
		[Description("修改枪柜容量，默认=30KG")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0.##} KG")]
		public float ContainerGunLockerCapacity = 30f;

		[Name("舱口")]
		[Description("修改舱口容量(参考登山者小屋那个地窖)，默认=40KG")]
		[Slider(0f, 250f, 251, NumberFormat = "{0:0.##} KG")]
		public float ContainerHatchCapacity = 40f;

		[Name("隐藏储物盒")]
		[Description("修改隐藏储物盒容量，默认=15KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerHiddenCacheCapacity = 15f;

		[Name("医务室抽屉")]
		[Description("修改医务室抽屉容量(参考黑岩监狱)，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerInfirmaryDrawerCapacity = 10f;

		[Name("厨房橱柜")]
		[Description("修改厨房橱柜容量，默认=15KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerKitchenCabinetCapacity = 15f;

		[Name("厨房抽屉")]
		[Description("修改厨房抽屉容量，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerKitchenDrawerCapacity = 10f;

		[Name("锁箱")]
		[Description("修改锁箱容量(黑铁盒子)，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerLockBoxCapacity = 10f;

		[Name("储物柜")]
		[Description("修改储物柜容量(参考那种黄色的要用撬棍打开的柜子)，默认=30KG")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0.##} KG")]
		public float ContainerLockerCapacity = 30f;

		[Name("药架")]
		[Description("修改药架容量(挂在墙壁上那种)，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerMedicineShelfCapacity = 5f;

		[Name("金属桌抽屉(小)")]
		[Description("修改小金属桌抽屉容量，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerDeskDrawerSmlCapacity = 5f;

		[Name("金属桌抽屉(大)")]
		[Description("修改大金属桌抽屉容量，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerDeskDrawerLgeCapacity = 10f;

		[Name("金属收纳盒")]
		[Description("修改金属收纳盒容量，默认=15KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerMetalContainerCapacity = 15f;

		[Name("烤箱")]
		[Description("修改烤箱容量，默认=40KG")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0.##} KG")]
		public float ContainerOvenCapacity = 40f;

		[Name("塑料盒")]
		[Description("修改塑料盒容量，默认=15KG")]
		[Slider(0f, 30f, 31, NumberFormat = "{0:0.##} KG")]
		public float ContainerPlasticContainerCapacity = 15f;

		[Name("岩石贮藏处")]
		[Description("修改岩石贮藏处容量，默认=30KG")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0.##} KG")]
		public float ContainerRockCacheCapacity = 30f;

		[Name("保险箱")]
		[Description("修改保险箱容量，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerSafeCapacity = 10f;

		[Name("保险盒")]
		[Description("修改保险盒容量(参考弥尔顿银行)，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerSafetyDepositBoxCapacity = 5f;

		[Name("床头柜抽屉")]
		[Description("修改床头柜抽屉容量，默认=5KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerEndTableDrawerCapacity = 5f;

		[Name("补给箱")]
		[Description("修改补给箱容量(参考神秘湖瞭望塔上面的补给箱)，默认=30KG")]
		[Slider(0f, 250f, 251, NumberFormat = "{0:0.##} KG")]
		public float ContainerSupplyBinCapacity = 30f;

		[Name("手提箱")]
		[Description("修改手提箱容量(参考飞机残骸手提箱)，默认=20KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerSuitcaseCapacity = 20f;

		[Name("工具柜抽屉(小)")]
		[Description("修改小型工具柜抽屉容量，默认=5KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerToolCabinetDrawerSmlCapacity = 5f;

		[Name("工具柜抽屉(大)")]
		[Description("修改大型工具柜抽屉容量，默认=10KG")]
		[Slider(0f, 100f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerToolCabinetDrawerLgeCapacity = 10f;

		[Name("垃圾桶")]
		[Description("修改垃圾桶容量，默认=15KG")]
		[Slider(0f, 500f, 501, NumberFormat = "{0:0.##} KG")]
		public float ContainerTrashCanCapacity = 15f;

		[Name("汽车后备箱")]
		[Description("修改汽车后备箱容量，默认=40KG")]
		[Slider(0f, 500f, 501, NumberFormat = "{0:0.##} KG")]
		public float ContainerCarTrunkCapacity = 40f;

		[Name("木质行李箱")]
		[Description("修改木质行李箱容量(参考灰色妈妈小屋莉莉房间的绿色行李箱)，默认=40KG")]
		[Slider(0f, 500f, 501, NumberFormat = "{0:0.##} KG")]
		public float ContainerTrunkCapacity = 40f;

		[Name("典狱长书桌抽屉")]
		[Description("修改典狱长书桌抽屉容量，默认=10KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerWardenDeskDrawerCapacity = 10f;

		[Name("洗衣机")]
		[Description("修改洗衣机容量，默认=30KG")]
		[Slider(0f, 500f, 501, NumberFormat = "{0:0.##} KG")]
		public float ContainerWasherCapacity = 30f;

		[Name("木桌抽屉")]
		[Description("修改木桌容量，默认=5KG")]
		[Slider(0f, 50f, 51, NumberFormat = "{0:0.##} KG")]
		public float ContainerWoodDeskDrawerCapacity = 5f;

		[Name("工作台抽屉")]
		[Description("修改工作台抽屉容量，默认=5KG")]
		[Slider(0f, 25f, 26, NumberFormat = "{0:0.##} KG")]
		public float ContainerWorkbenchDrawerCapacity = 5f;

		[Section("其他")]
		[Name("'作弊'项")]
		[Description("允许玩家对物体属性进行修改，开启该功能可将上面所有隐藏选项打开")]
		public bool CheatingTweaks;

		[Name("容器容量修改")]
		[Description("启用全容器修改，所有修改必须切换场景才能生效")]
		public bool ContainerWeightTweaks;

		internal static Settings Instance { get; }

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			RefreshFields();
		}

		protected override void OnConfirm()
		{
			base.OnConfirm();
			Encumber encumberComponent = GameManager.GetEncumberComponent();
			if (!((Object)(object)encumberComponent == (Object)null))
			{
				Encumber.EncumberUpdate(encumberComponent);
			}
		}

		private void RefreshFields()
		{
			if (ExtendedFunctionality)
			{
				SetFieldVisible("HighBeamRestrictions", visible: true);
			}
			else
			{
				SetFieldVisible("HighBeamRestrictions", visible: false);
			}
			if (FlashlightBeamColor == FlashlightBeamColor.Custom)
			{
				SetFieldVisible("FlashlightRedValue", visible: true);
				SetFieldVisible("FlashlightGreenValue", visible: true);
				SetFieldVisible("FlashlightBlueValue", visible: true);
			}
			else
			{
				SetFieldVisible("FlashlightRedValue", visible: false);
				SetFieldVisible("FlashlightGreenValue", visible: false);
				SetFieldVisible("FlashlightBlueValue", visible: false);
			}
			if (MinersFlashlightBeamColor == FlashlightBeamColor.Custom)
			{
				SetFieldVisible("MinersFlashlightRedValue", visible: true);
				SetFieldVisible("MinersFlashlightGreenValue", visible: true);
				SetFieldVisible("MinersFlashlightBlueValue", visible: true);
			}
			else
			{
				SetFieldVisible("MinersFlashlightRedValue", visible: false);
				SetFieldVisible("MinersFlashlightGreenValue", visible: false);
				SetFieldVisible("MinersFlashlightBlueValue", visible: false);
			}
			if (CheatingTweaks)
			{
				SetFieldVisible("SnowShelterDailyDecayRate", visible: true);
				SetFieldVisible("DecayBlizzardTravois", visible: true);
				SetFieldVisible("DecayHPPerHourTravois", visible: true);
				SetFieldVisible("DecayMovementPerUnitTravois", visible: true);
				SetFieldVisible("InfiniteBattery", visible: true);
				SetFieldVisible("FlashlightLowBeamDuration", visible: true);
				SetFieldVisible("FlashlightHighBeamDuration", visible: true);
				SetFieldVisible("FlashlightRechargeTime", visible: true);
				SetFieldVisible("MinersFlashlightLowBeamDuration", visible: true);
				SetFieldVisible("MinersFlashlightHighBeamDuration", visible: true);
				SetFieldVisible("MinersFlashlightRechargeTime", visible: true);
				SetFieldVisible("OverrideTravoisMovementRestrictions", visible: true);
				SetFieldVisible("OverrideTravoisInteractionRestrictions", visible: true);
				SetFieldVisible("InfiniteEncumberWeight", visible: true);
			}
			else
			{
				SetFieldVisible("SnowShelterDailyDecayRate", visible: false);
				SetFieldVisible("DecayBlizzardTravois", visible: false);
				SetFieldVisible("DecayHPPerHourTravois", visible: false);
				SetFieldVisible("DecayMovementPerUnitTravois", visible: false);
				SetFieldVisible("InfiniteBattery", visible: false);
				SetFieldVisible("FlashlightLowBeamDuration", visible: false);
				SetFieldVisible("FlashlightHighBeamDuration", visible: false);
				SetFieldVisible("FlashlightRechargeTime", visible: false);
				SetFieldVisible("MinersFlashlightLowBeamDuration", visible: false);
				SetFieldVisible("MinersFlashlightHighBeamDuration", visible: false);
				SetFieldVisible("MinersFlashlightRechargeTime", visible: false);
				SetFieldVisible("OverrideTravoisMovementRestrictions", visible: false);
				SetFieldVisible("OverrideTravoisInteractionRestrictions", visible: false);
				SetFieldVisible("InfiniteEncumberWeight", visible: false);
			}
			if (ContainerWeightTweaks && CheatingTweaks)
			{
				SetFieldVisible("InfiniteContainerWeight", visible: true);
			}
			if (ContainerWeightTweaks)
			{
				SetFieldVisible("ContainerBackpackCapacity", visible: true);
				SetFieldVisible("ContainerBriefcaseCapacity", visible: true);
				SetFieldVisible("ContainerCabinetSmlCapacity", visible: true);
				SetFieldVisible("ContainerCabinetLgeCapacity", visible: true);
				SetFieldVisible("ContainerCargoContainerCapacity", visible: true);
				SetFieldVisible("ContainerCashRegisterCapacity", visible: true);
				SetFieldVisible("ContainerCoalBinCapacity", visible: true);
				SetFieldVisible("ContainerCoolerCapacity", visible: true);
				SetFieldVisible("ContainerCupboardCapacity", visible: true);
				SetFieldVisible("ContainerDresserDrawerCapacity", visible: true);
				SetFieldVisible("ContainerDryerCapacity", visible: true);
				SetFieldVisible("ContainerFileCabinetCapacity", visible: true);
				SetFieldVisible("ContainerFirstAidCapacity", visible: true);
				SetFieldVisible("ContainerFirewoodBinCapacity", visible: true);
				SetFieldVisible("ContainerFishingHutDrawerCapacity", visible: true);
				SetFieldVisible("ContainerFreezerCapacity", visible: true);
				SetFieldVisible("ContainerFridgeCapacity", visible: true);
				SetFieldVisible("ContainerGloveBoxCapacity", visible: true);
				SetFieldVisible("ContainerGunLockerCapacity", visible: true);
				SetFieldVisible("ContainerHatchCapacity", visible: true);
				SetFieldVisible("ContainerHiddenCacheCapacity", visible: true);
				SetFieldVisible("ContainerInfirmaryDrawerCapacity", visible: true);
				SetFieldVisible("ContainerKitchenCabinetCapacity", visible: true);
				SetFieldVisible("ContainerKitchenDrawerCapacity", visible: true);
				SetFieldVisible("ContainerLockBoxCapacity", visible: true);
				SetFieldVisible("ContainerLockerCapacity", visible: true);
				SetFieldVisible("ContainerMedicineShelfCapacity", visible: true);
				SetFieldVisible("ContainerDeskDrawerSmlCapacity", visible: true);
				SetFieldVisible("ContainerDeskDrawerLgeCapacity", visible: true);
				SetFieldVisible("ContainerMetalContainerCapacity", visible: true);
				SetFieldVisible("ContainerOvenCapacity", visible: true);
				SetFieldVisible("ContainerPlasticContainerCapacity", visible: true);
				SetFieldVisible("ContainerRockCacheCapacity", visible: true);
				SetFieldVisible("ContainerSafeCapacity", visible: true);
				SetFieldVisible("ContainerSafetyDepositBoxCapacity", visible: true);
				SetFieldVisible("ContainerEndTableDrawerCapacity", visible: true);
				SetFieldVisible("ContainerSupplyBinCapacity", visible: true);
				SetFieldVisible("ContainerSuitcaseCapacity", visible: true);
				SetFieldVisible("ContainerToolCabinetDrawerSmlCapacity", visible: true);
				SetFieldVisible("ContainerToolCabinetDrawerLgeCapacity", visible: true);
				SetFieldVisible("ContainerTrashCanCapacity", visible: true);
				SetFieldVisible("ContainerCarTrunkCapacity", visible: true);
				SetFieldVisible("ContainerTrunkCapacity", visible: true);
				SetFieldVisible("ContainerWardenDeskDrawerCapacity", visible: true);
				SetFieldVisible("ContainerWasherCapacity", visible: true);
				SetFieldVisible("ContainerWoodDeskDrawerCapacity", visible: true);
				SetFieldVisible("ContainerWorkbenchDrawerCapacity", visible: true);
			}
			else
			{
				SetFieldVisible("ContainerBackpackCapacity", visible: false);
				SetFieldVisible("ContainerBriefcaseCapacity", visible: false);
				SetFieldVisible("ContainerCabinetSmlCapacity", visible: false);
				SetFieldVisible("ContainerCabinetLgeCapacity", visible: false);
				SetFieldVisible("ContainerCargoContainerCapacity", visible: false);
				SetFieldVisible("ContainerCashRegisterCapacity", visible: false);
				SetFieldVisible("ContainerCoalBinCapacity", visible: false);
				SetFieldVisible("ContainerCoolerCapacity", visible: false);
				SetFieldVisible("ContainerCupboardCapacity", visible: false);
				SetFieldVisible("ContainerDresserDrawerCapacity", visible: false);
				SetFieldVisible("ContainerDryerCapacity", visible: false);
				SetFieldVisible("ContainerFileCabinetCapacity", visible: false);
				SetFieldVisible("ContainerFirstAidCapacity", visible: false);
				SetFieldVisible("ContainerFirewoodBinCapacity", visible: false);
				SetFieldVisible("ContainerFishingHutDrawerCapacity", visible: false);
				SetFieldVisible("ContainerFreezerCapacity", visible: false);
				SetFieldVisible("ContainerFridgeCapacity", visible: false);
				SetFieldVisible("ContainerGloveBoxCapacity", visible: false);
				SetFieldVisible("ContainerGunLockerCapacity", visible: false);
				SetFieldVisible("ContainerHatchCapacity", visible: false);
				SetFieldVisible("ContainerHiddenCacheCapacity", visible: false);
				SetFieldVisible("ContainerInfirmaryDrawerCapacity", visible: false);
				SetFieldVisible("ContainerKitchenCabinetCapacity", visible: false);
				SetFieldVisible("ContainerKitchenDrawerCapacity", visible: false);
				SetFieldVisible("ContainerLockBoxCapacity", visible: false);
				SetFieldVisible("ContainerLockerCapacity", visible: false);
				SetFieldVisible("ContainerMedicineShelfCapacity", visible: false);
				SetFieldVisible("ContainerDeskDrawerSmlCapacity", visible: false);
				SetFieldVisible("ContainerDeskDrawerLgeCapacity", visible: false);
				SetFieldVisible("ContainerMetalContainerCapacity", visible: false);
				SetFieldVisible("ContainerOvenCapacity", visible: false);
				SetFieldVisible("ContainerPlasticContainerCapacity", visible: false);
				SetFieldVisible("ContainerRockCacheCapacity", visible: false);
				SetFieldVisible("ContainerSafeCapacity", visible: false);
				SetFieldVisible("ContainerSafetyDepositBoxCapacity", visible: false);
				SetFieldVisible("ContainerEndTableDrawerCapacity", visible: false);
				SetFieldVisible("ContainerSupplyBinCapacity", visible: false);
				SetFieldVisible("ContainerSuitcaseCapacity", visible: false);
				SetFieldVisible("ContainerToolCabinetDrawerSmlCapacity", visible: false);
				SetFieldVisible("ContainerToolCabinetDrawerLgeCapacity", visible: false);
				SetFieldVisible("ContainerTrashCanCapacity", visible: false);
				SetFieldVisible("ContainerCarTrunkCapacity", visible: false);
				SetFieldVisible("ContainerTrunkCapacity", visible: false);
				SetFieldVisible("ContainerWardenDeskDrawerCapacity", visible: false);
				SetFieldVisible("ContainerWasherCapacity", visible: false);
				SetFieldVisible("ContainerWoodDeskDrawerCapacity", visible: false);
				SetFieldVisible("ContainerWorkbenchDrawerCapacity", visible: false);
				SetFieldVisible("InfiniteContainerWeight", visible: false);
			}
		}

		internal static void OnLoad()
		{
			Instance.AddToModSettings("多功能实用模组v1.4.8");
			Instance.RefreshFields();
			Instance.RefreshGUI();
		}

		static Settings()
		{
			Instance = new Settings();
		}
	}
}
