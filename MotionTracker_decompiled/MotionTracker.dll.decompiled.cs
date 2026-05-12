using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using MotionTracker;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("MotionTracker")]
[assembly: AssemblyCopyright("okclm")]
[assembly: AssemblyFileVersion("1.3.0")]
[assembly: MelonInfo(typeof(MotionTrackerMain), "MotionTracker", "1.3.0", "okclm", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.3.0.0")]
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
namespace MotionTracker
{
	public class MyLogger
	{
		public static void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
		{
		}
	}
	internal class SpawnUtils
	{
		internal static List<GameObject> GetRootObjects()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(i);
				MyLogger.LogMessage("SpawnUtils GetRootObjects: Scene (" + ((Scene)(ref sceneAt)).name + ").", 83, "GetRootObjects", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit((Il2CppArrayBase<GameObject>)(object)((Scene)(ref sceneAt)).GetRootGameObjects());
				foreach (GameObject val in array)
				{
					list.Add(val);
					string[] obj = new string[7]
					{
						"SpawnUtils GetRootObjects: (",
						((Object)val).name,
						":",
						((Object)val).GetInstanceID().ToString(),
						") at [",
						null,
						null
					};
					Vector3 position = val.transform.position;
					obj[5] = ((object)(Vector3)(ref position)).ToString();
					obj[6] = "].";
					MyLogger.LogMessage(string.Concat(obj), 91, "GetRootObjects", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				}
			}
			return list;
		}

		internal static void GetChildren(GameObject obj, List<GameObject> result)
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if (obj.transform.childCount > 0)
			{
				for (int i = 0; i < obj.transform.childCount; i++)
				{
					GameObject gameObject = ((Component)obj.transform.GetChild(i)).gameObject;
					result.Add(gameObject);
					string[] obj2 = new string[9]
					{
						"SpawnUtils GetChildren: (",
						((Object)gameObject).name,
						":",
						((Object)gameObject).GetInstanceID().ToString(),
						") at [",
						null,
						null,
						null,
						null
					};
					Vector3 position = gameObject.transform.position;
					obj2[5] = ((object)(Vector3)(ref position)).ToString();
					obj2[6] = "] activeSelf=";
					obj2[7] = gameObject.activeSelf.ToString();
					obj2[8] = ".";
					MyLogger.LogMessage(string.Concat(obj2), 108, "GetChildren", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
					GetChildren(gameObject, result);
				}
			}
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "Awake")]
	public class BeachcombingSpawnerAwakePatch
	{
		public static void Postfix(ref BeachcombingSpawner __instance)
		{
			MyLogger.LogMessage("!!BeachcombingSpawner Awake event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with " + __instance.m_ChildSpawners.Count + " child spawners.", 130, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "Update")]
	public class BeachcombingSpawnerUpdatePatch
	{
		private static float timer = 0f;

		private static float triggerTime = 5f;

		private static bool doOnce = false;

		public static void Postfix(ref BeachcombingSpawner __instance)
		{
			timer += Time.deltaTime;
			if (!(timer > triggerTime))
			{
				return;
			}
			if (doOnce)
			{
				MyLogger.LogMessage("BeachcombingSpawner Update event: Begin root objects identification.", 152, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				foreach (GameObject rootObject in SpawnUtils.GetRootObjects())
				{
					List<GameObject> result = new List<GameObject>();
					SpawnUtils.GetChildren(rootObject, result);
				}
				MyLogger.LogMessage("BeachcombingSpawner Update event: End root objects identification.", 163, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				doOnce = false;
			}
			if (((Object)__instance).name.Contains("Tide"))
			{
				int num = 0;
				Enumerator<RadialObjectSpawner> enumerator2 = __instance.m_ChildSpawners.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					RadialObjectSpawner current2 = enumerator2.Current;
					int num2 = 0;
					Enumerator<GameObject> enumerator3 = current2.m_Spawns.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						GameObject current3 = enumerator3.Current;
						if (!Object.op_Implicit((Object)(object)current3.gameObject.GetComponent<PingComponent>()))
						{
							current3.gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.BeachLoot);
						}
						num2++;
					}
					num++;
				}
			}
			timer = 0f;
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "UpdateBeachcombing")]
	public class BeachcombingSpawnerUpdateBeachcombingPatch
	{
		public static void Postfix(ref BeachcombingSpawner __instance)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			MyLogger.LogMessage("!!BeachcombingSpawner UpdateBeachcombing event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") at [" + ((Component)__instance).transform.position.x + "," + ((Component)__instance).transform.position.y + "," + ((Component)__instance).transform.position.z + "].", 223, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "UpdateBigItems")]
	public class BeachcombingSpawnerUpdateBigItemsPatch
	{
		private static float timer = 0f;

		private static float triggerTime = 5f;

		public static void Postfix(ref BeachcombingSpawner __instance)
		{
			timer += Time.deltaTime;
			if (!(timer > triggerTime))
			{
				return;
			}
			int num = 0;
			Enumerator<BeachcombingBigItemLocation> enumerator = __instance.m_BigItemLocations.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BeachcombingBigItemLocation current = enumerator.Current;
				GearItem[] array = Il2CppArrayBase<GearItem>.op_Implicit(((Component)current).GetComponentsInChildren<GearItem>());
				int num2 = 0;
				GearItem[] array2 = array;
				foreach (GearItem val in array2)
				{
					PingComponent component = ((Component)val).gameObject.GetComponent<PingComponent>();
					if (Object.op_Implicit((Object)(object)component))
					{
						if (component.animalType != PingManager.AnimalType.BeachLoot)
						{
							PingComponent.ManualDelete(component);
							((Component)val).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.BeachLoot);
						}
					}
					else if (((Component)val).gameObject.activeInHierarchy && ((Component)current).gameObject.activeInHierarchy)
					{
						((Component)val).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.BeachLoot);
					}
					num2++;
				}
				num++;
			}
			timer = 0f;
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "OnValidate")]
	public class BeachcombingSpawnerOnValidatePatch
	{
		public static void Postfix(ref BeachcombingSpawner __instance)
		{
			MyLogger.LogMessage("!!BeachcombingSpawner OnValidate event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 334, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "OnDestroy")]
	public class BeachcombingSpawnerOnDestroyPatch
	{
		public static void Prefix(ref BeachcombingSpawner __instance)
		{
			MyLogger.LogMessage("!!BeachcombingSpawner OnDestroy event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 344, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "ExpireBigItems")]
	public class BeachcombingSpawnerExpireBigItemsPatch
	{
		public static void Prefix(ref BeachcombingSpawner __instance)
		{
			MyLogger.LogMessage("!!BeachcombingSpawner ExpireBigItems event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 367, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "DeserializeBigItems")]
	public class BeachcombingSpawnerDeserializeBigItemsPatch
	{
		public static void Prefix(ref BeachcombingSpawner __instance)
		{
			MyLogger.LogMessage("!!BeachcombingSpawner DeserializeBigItems event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 377, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "DrawAndPlaceBigItems")]
	public class BeachcombingSpawnerDrawAndPlaceBigItemsPatch
	{
		public static void Prefix(ref BeachcombingSpawner __instance)
		{
			MyLogger.LogMessage("!!BeachcombingSpawner DrawAndPlaceBigItems event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 387, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(BeachcombingSpawner), "CheckForNewBlizzard")]
	public class BeachcombingSpawnerCheckForNewBlizzardPatch
	{
		public static void Prefix(ref BeachcombingSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "OnDestroy")]
	public class RadialObjectSpawnerOnDestroyPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "Awake")]
	public class RadialObjectSpawnerAwakePatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "SpawnAttemptAllNoVisChecks")]
	public class RadialObjectSpawnerSpawnAttemptAllNoVisChecksPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "SpawnAttemptOnceWithVisCheck")]
	public class RadialObjectSpawnerSpawnAttemptOnceWithVisCheckPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "ReleaseSpawnedObjectsToPool")]
	public class RadialObjectSpawnerReleaseSpawnedObjectsToPoolPatch
	{
		private static float timer;

		private static float triggerTime;

		public static void Prefix(ref RadialObjectSpawner __instance)
		{
			timer += Time.deltaTime;
			if (!(timer > triggerTime))
			{
				return;
			}
			int num = 0;
			Enumerator<GameObject> enumerator = __instance.m_Spawns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				if (Object.op_Implicit((Object)(object)current.gameObject.GetComponent<PingComponent>()))
				{
					PingComponent.ManualDelete(current.gameObject.GetComponent<PingComponent>());
				}
				num++;
			}
			timer = 0f;
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "DisableSplineMeshUpdating")]
	public class RadialObjectSpawnerDisableSplineMeshUpdatingPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "SetObjectToSpawnBoundingRadius")]
	public class RadialObjectSpawnerSetObjectToSpawnBoundingRadiusPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "SetSplineBoundingRadius")]
	public class RadialObjectSpawnerSetSplineBoundingRadiusPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
			MyLogger.LogMessage("RadialObjectSpawner SetSplineBoundingRadius event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ").", 511, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "RollRandomNumToSpawn")]
	public class RadialObjectSpawnerRollRandomNumToSpawnPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(typeof(RadialObjectSpawner), "Start")]
	public class RadialObjectSpawnerStartPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance)
		{
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public class RadialObjectSpawnerRemoveFromSpawnsPatch
	{
		public static void Postfix(ref RadialObjectSpawner __instance, GameObject go)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			string[] obj = new string[11]
			{
				"RadialObjectSpawner RemoveFromSpawns event: Harvesting RadialObjectSpawner (",
				((Object)__instance).name,
				":",
				((Object)__instance).GetInstanceID().ToString(),
				" loot (",
				((Object)go).name,
				":",
				((Object)go).GetInstanceID().ToString(),
				") at [",
				null,
				null
			};
			Vector3 position = go.transform.position;
			obj[9] = ((object)(Vector3)(ref position)).ToString();
			obj[10] = "] during.";
			MyLogger.LogMessage(string.Concat(obj), 541, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			if (Object.op_Implicit((Object)(object)go.gameObject.GetComponent<PingComponent>()))
			{
				MyLogger.LogMessage("RadialObjectSpawner RemoveFromSpawns event: Harvested object (" + ((Object)go).name + ":" + ((Object)go).GetInstanceID() + ") PingComponent exists for beach loot.  Delete PingComponent to remove from radar.", 547, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				PingComponent.ManualDelete(go.gameObject.GetComponent<PingComponent>());
			}
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public class HarvestableDeserializePatch
	{
		public static void Postfix(ref Harvestable __instance, string text)
		{
			if (!((Object)__instance).name.Contains("SaltDeposit"))
			{
				return;
			}
			if (__instance.IsHarvested())
			{
				if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
				{
					PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
				}
			}
			else if (!Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.SaltDeposit);
			}
		}
	}
	[HarmonyPatch(typeof(Harvestable), "Start")]
	public class SaltStartPatch
	{
		public static void Postfix(ref Harvestable __instance)
		{
			if (((Object)__instance).name.Contains("SaltDeposit") && !__instance.IsHarvested() && !Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.SaltDeposit);
			}
		}
	}
	[HarmonyPatch(typeof(Harvestable), "OnDestroy")]
	public class SaltDestroyPatch
	{
		public static void Postfix(ref Harvestable __instance)
		{
			((Object)__instance).name.Contains("SaltDeposit");
		}
	}
	[HarmonyPatch(typeof(Harvestable), "Harvest")]
	public class SaltHarvestPatch
	{
		public static void Postfix(ref Harvestable __instance)
		{
			if (((Object)__instance).name.Contains("SaltDeposit") && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
		}
	}
	[HarmonyPatch(typeof(Container), "Awake")]
	public class ContainerAwakePatch
	{
		public static void Postfix(ref Container __instance)
		{
			((Object)__instance).name.Contains("CONTAINER_InaccessibleGear");
		}
	}
	[HarmonyPatch(typeof(Container), "Start")]
	public class ContainerStartPatch
	{
		public static void Postfix(ref Container __instance)
		{
			((Object)__instance).name.Contains("CONTAINER_InaccessibleGear");
		}
	}
	[HarmonyPatch(typeof(Container), "OnEnable")]
	public class ContainerOnEnablePatch
	{
		public static void Postfix(ref Container __instance)
		{
			((Object)__instance).name.Contains("CONTAINER_InaccessibleGear");
		}
	}
	[HarmonyPatch(typeof(Container), "OnDisable")]
	public class ContainerOnDisablePatch
	{
		public static void Postfix(ref Container __instance)
		{
			if (((Object)__instance).name.Contains("CONTAINER_InaccessibleGear") && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
		}
	}
	[HarmonyPatch(typeof(Container), "OnDestroy")]
	public class ContainerOnDestroyPatch
	{
		public static void Postfix(ref Container __instance)
		{
			if (((Object)__instance).name.Contains("CONTAINER_InaccessibleGear") && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
		}
	}
	[HarmonyPatch(typeof(Container), "UpdateContainer")]
	public class ContainerUpdateContainerPatch
	{
		public static void Postfix(ref Container __instance)
		{
			if (((Object)__instance).name.Contains("CONTAINER_InaccessibleGear") && !Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.LostAndFoundBox);
			}
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public class PlayerManagerTryAddToExistingStackable
	{
		public static bool Prefix(GearItem gearToAdd, float normalizedCondition, int numUnits, GearItem existingGearItem)
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			PingComponent component = ((Component)gearToAdd).gameObject.GetComponent<PingComponent>();
			if (Object.op_Implicit((Object)(object)component) && component.animalType == PingManager.AnimalType.BeachLoot)
			{
				string[] obj = new string[7]
				{
					"PlayerManager.TryAddToExistingStackable event: See Spawned Beach Loot (",
					((Object)gearToAdd).name,
					":",
					((Object)gearToAdd).GetInstanceID().ToString(),
					") at [",
					null,
					null
				};
				Vector3 position = ((Component)gearToAdd).transform.position;
				obj[5] = ((object)(Vector3)(ref position)).ToString();
				obj[6] = "] and existing BeachLoot PingComponent.";
				MyLogger.LogMessage(string.Concat(obj), 803, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				PingComponent.ManualDelete(component);
			}
			if (((Object)((Component)gearToAdd).gameObject).name.Contains("Arrow") || ((Object)((Component)gearToAdd).gameObject).name.Contains("Coal") || PingComponent.IsRawFish(gearToAdd))
			{
				if ((Object)(object)((Component)gearToAdd).gameObject.GetComponent<PingComponent>() != (Object)null)
				{
					PingComponent.ManualDelete(((Component)gearToAdd).gameObject.GetComponent<PingComponent>());
				}
				return true;
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(GearItem), "ManualUpdate")]
	public class GearItemManualUpdatePatch
	{
		public static void Postfix(ref GearItem __instance)
		{
			if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal") && !PingComponent.IsRawFish(__instance))
			{
				return;
			}
			if (__instance.m_InsideContainer)
			{
				if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject) && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
				{
					PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
				}
			}
			else if (__instance.m_InPlayerInventory)
			{
				if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject) && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
				{
					PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
				}
			}
			else if (!Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				if (((Object)((Component)__instance).gameObject).name.Contains("Arrow"))
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Arrow);
				}
				else if (((Object)((Component)__instance).gameObject).name.Contains("Coal"))
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Coal);
				}
				else if (PingComponent.IsRawFish(__instance))
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.RawFish);
				}
				((Component)__instance).gameObject.GetComponent<PingComponent>().attachedGearItem = __instance;
			}
		}
	}
	[HarmonyPatch(typeof(GearItem), "OnDestroy")]
	public class GearItemDestroyPatch
	{
		public static void Postfix(ref GearItem __instance)
		{
			if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal"))
			{
				PingComponent.IsRawFish(__instance);
			}
			if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal"))
				{
					PingComponent.IsRawFish(__instance);
				}
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
			else if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal"))
			{
				PingComponent.IsRawFish(__instance);
			}
		}
	}
	[HarmonyPatch(typeof(BaseAi), "Start")]
	public class AiAwakePatch
	{
		public static void Postfix(ref BaseAi __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Invalid comparison between Unknown and I4
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Invalid comparison between Unknown and I4
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Invalid comparison between Unknown and I4
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Invalid comparison between Unknown and I4
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Invalid comparison between Unknown and I4
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Invalid comparison between Unknown and I4
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Invalid comparison between Unknown and I4
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Invalid comparison between Unknown and I4
			if ((int)__instance.m_CurrentMode != 2 && (int)__instance.m_CurrentMode != 27 && (int)__instance.m_CurrentMode != 0)
			{
				if ((int)__instance.m_AiSubType == 5)
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Moose);
				}
				else if ((int)__instance.m_AiSubType == 2)
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Bear);
				}
				else if ((int)__instance.m_AiSubType == 6)
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Cougar);
				}
				else if ((int)__instance.m_AiSubType == 1 && ((Object)((Component)__instance).gameObject).name.ToLower().Contains("grey"))
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Timberwolf);
				}
				else if ((int)__instance.m_AiSubType == 1)
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Wolf);
				}
				else if ((int)__instance.m_AiSubType == 3 && !((Object)((Component)__instance).gameObject).name.Contains("_Doe"))
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Stag);
				}
				else if ((int)__instance.m_AiSubType == 3 && ((Object)((Component)__instance).gameObject).name.Contains("_Doe"))
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Doe);
				}
				else if ((int)__instance.m_SnowImprintType == 8)
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.PuffyBird);
				}
				else if ((int)__instance.m_AiSubType == 4)
				{
					((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Rabbit);
				}
			}
		}
	}
	[HarmonyPatch(typeof(FlockChild), "Start")]
	public class FlockPatch
	{
		public static void Postfix(ref FlockChild __instance)
		{
			((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Crow);
		}
	}
	[HarmonyPatch(typeof(FlockChild), "Update")]
	public class FlockUpdatePatch
	{
		public static void Postfix(ref FlockChild __instance)
		{
		}
	}
	[HarmonyPatch(typeof(FlockController), "Start")]
	public class FlockController_Start_Patch
	{
		public static void Postfix(ref FlockController __instance)
		{
		}
	}
	[HarmonyPatch(typeof(FlockController), "Update")]
	public class FlockController_Update_Patch
	{
		public static void Postfix(ref FlockController __instance)
		{
		}
	}
	[HarmonyPatch(typeof(FlockController), "destroyBirds")]
	public class FlockController_destroyBirds_Patch
	{
		public static void Postfix(ref FlockController __instance)
		{
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
	}
	[HarmonyPatch(typeof(BaseAi), "EnterDead")]
	public class DeathPatch
	{
		public static void Postfix(ref BaseAi __instance)
		{
			Component component = ((Component)__instance).gameObject.GetComponent(Il2CppType.Of<PingComponent>());
			PingComponent.ManualDelete((component != null) ? ((Il2CppObjectBase)component).TryCast<PingComponent>() : null);
		}
	}
	[HarmonyPatch(typeof(BaseAi), "OnDisable")]
	public class DeathPatch2
	{
		public static void Postfix(ref BaseAi __instance)
		{
			Component component = ((Component)__instance).gameObject.GetComponent(Il2CppType.Of<PingComponent>());
			PingComponent.ManualDelete((component != null) ? ((Il2CppObjectBase)component).TryCast<PingComponent>() : null);
		}
	}
	[HarmonyPatch(typeof(BaseAi), "Despawn")]
	public class DeathPatch3
	{
		public static void Postfix(ref BaseAi __instance)
		{
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
	}
	[HarmonyPatch(typeof(BaseAi), "ProcessDead")]
	public class ProcessDeadPatch
	{
		public static void Postfix(ref BaseAi __instance)
		{
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
	}
	[HarmonyPatch(typeof(BaseAi), "ExitDead")]
	public class ExitDeadPatch
	{
		public static void Postfix(ref BaseAi __instance)
		{
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
	}
	[HarmonyPatch(typeof(Panel_Base), "Enable", new Type[] { typeof(bool) })]
	public class PanelPatch
	{
		public static void Postfix(ref Panel_Base __instance, bool enable)
		{
			PingManager.inMenu = enable;
		}
	}
	[HarmonyPatch(typeof(DynamicDecalsManager), "TrySpawnDecalObject", new Type[] { typeof(DecalProjectorInstance) })]
	public class TrySpawnDecalObjectPatch
	{
		public static void Postfix(ref DynamicDecalsManager __instance, ref DecalProjectorInstance decalInstance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			if ((int)decalInstance.m_DecalProjectorType == 7)
			{
				Vector3 position = default(Vector3);
				Quaternion rotation = default(Quaternion);
				Vector3 val = default(Vector3);
				__instance.CalculateDecalTransform(decalInstance, (DecalProjectorMaskData)null, ref position, ref rotation, ref val);
				GameObject val2 = new GameObject("DecalContainer");
				val2.transform.position = position;
				val2.transform.rotation = rotation;
				val2.AddComponent<PingComponent>().Initialize(decalInstance.m_ProjectileType);
			}
		}
	}
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
	internal class MotionTrackerSettings : JsonModSettings
	{
		[Section("常用功能")]
		[Name("启用动物雷达追踪")]
		[Description("启用或禁用")]
		public bool enableMotionTracker = true;

		[Name("雷达UI可见性")]
		[Description("始终可见 / 热键切换可见")]
		public Settings.DisplayStyle displayStyle;

		[Name("热键")]
		[Description("使用热键切换来实现可见")]
		public KeyCode toggleKey = (KeyCode)256;

		[Name("仅室外生效")]
		[Description("只有在室外时才显示雷达追踪")]
		public bool onlyOutdoors = true;

		[Name("探测范围")]
		[Description("探测动物的范围")]
		[Slider(0f, 800f)]
		public int detectionRange = 100;

		[Name("雷达界面大小")]
		[Description("左上角雷达界面大小")]
		[Slider(0f, 4f)]
		public float scale = 1f;

		[Name("雷达界面的透明度")]
		[Description("雷达界面的透明程度")]
		[Slider(0f, 1f)]
		public float opacity = 0.7f;

		[Section("喷漆")]
		[Name("显示喷漆标记")]
		[Description("启用/禁用")]
		public bool showSpraypaint = true;

		[Name("喷漆图标大小")]
		[Description("雷达上的图标大小")]
		[Slider(0.2f, 5f)]
		public float spraypaintScale = 2f;

		[Name("喷漆图标透明度")]
		[Description("喷漆图标的透明程度")]
		[Slider(0f, 1f)]
		public float spraypaintOpacity = 0.8f;

		[Section("野生动物")]
		[Name("动物图标大小")]
		[Description("雷达上的图标大小")]
		[Slider(0f, 5f)]
		public float animalScale = 3.5f;

		[Name("动物图标透明度")]
		[Description("雷达上动物图标的透明度")]
		[Slider(0f, 1f)]
		public float animalOpacity = 0.8f;

		[Name("显示乌鸦")]
		[Description("追踪乌鸦")]
		public bool showCrows = true;

		[Name("显示兔子")]
		[Description("追踪兔子")]
		public bool showRabbits = true;

		[Name("显示雄鹿")]
		[Description("追踪公鹿")]
		public bool showStags = true;

		[Name("显示雌鹿")]
		[Description("追踪母鹿")]
		public bool showDoes = true;

		[Name("显示普通狼")]
		[Description("追踪普通狼")]
		public bool showWolves = true;

		[Name("显示森林狼")]
		[Description("追踪森林狼")]
		public bool showTimberwolves = true;

		[Name("显示熊")]
		[Description("追踪熊")]
		public bool showBears = true;

		[Name("显示美洲狮")]
		[Description("追踪美洲狮")]
		public bool showCougars = true;

		[Name("显示驼鹿")]
		[Description("追踪驼鹿")]
		public bool showMoose = true;

		[Name("显示松鸡")]
		[Description("追踪松鸡")]
		public bool showPuffyBirds = true;

		[Section("装备")]
		[Name("装备图标大小")]
		[Description("雷达上显示装备图标的大小")]
		[Slider(0f, 5f)]
		public float gearScale = 3.5f;

		[Name("装备图标透明度")]
		[Description("雷达上显示装备图标的透明度")]
		[Slider(0f, 1f)]
		public float gearOpacity = 0.8f;

		[Name("显示箭矢")]
		[Description("雷达上显示箭矢位置")]
		public bool showArrows = true;

		[Name("显示煤炭")]
		[Description("雷达上显示煤炭位置")]
		public bool showCoal = true;

		[Name("显示生鱼")]
		[Description("雷达上显示生鱼位置")]
		public bool showRawFish = true;

		[Name("显示失物招领箱")]
		[Description("雷达上显示失物招领箱位置")]
		public bool showLostAndFoundBox = true;

		[Name("显示盐矿点")]
		[Description("雷达上显示盐矿位置")]
		public bool showSaltDeposit = true;

		[Name("显示赶海物资")]
		[Description("雷达上显示赶海拾荒物资")]
		public bool showBeachLoot = true;

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
		}

		protected override void OnConfirm()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			base.OnConfirm();
			if (Object.op_Implicit((Object)(object)PingManager.instance))
			{
				PingManager.instance.SetOpacity(Settings.options.opacity);
				PingManager.instance.Scale(Settings.options.scale);
				Settings.animalScale = new Vector3(Settings.options.animalScale, Settings.options.animalScale, Settings.options.animalScale);
				Settings.spraypaintScale = new Vector3(Settings.options.spraypaintScale, Settings.options.spraypaintScale, Settings.options.spraypaintScale);
				Settings.gearScale = new Vector3(Settings.options.gearScale, Settings.options.gearScale, Settings.options.gearScale);
				Settings.animalColor = new Color(1f, 1f, 1f, Settings.options.animalOpacity);
				Settings.gearColor = new Color(1f, 1f, 1f, Settings.options.gearOpacity);
				Settings.spraypaintColor = new Color(0.62f, 0.29f, 0f, Settings.options.spraypaintOpacity);
			}
		}
	}
	internal static class Settings
	{
		public enum DisplayStyle
		{
			AlwaysOn,
			Toggle
		}

		public static MotionTrackerSettings options;

		public static Vector3 animalScale;

		public static Vector3 spraypaintScale;

		public static Vector3 gearScale;

		public static Color animalColor;

		public static Color spraypaintColor;

		public static Color gearColor;

		public static bool toggleBool;

		public static void OnLoad()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			options = new MotionTrackerSettings();
			options.AddToModSettings("野生动物雷达追踪v1.3");
			animalScale = new Vector3(options.animalScale, options.animalScale, options.animalScale);
			gearScale = new Vector3(options.gearScale, options.gearScale, options.gearScale);
			spraypaintScale = new Vector3(options.spraypaintScale, options.spraypaintScale, options.spraypaintScale);
			animalColor = new Color(1f, 1f, 1f, options.animalOpacity);
			gearColor = new Color(1f, 1f, 1f, options.gearOpacity);
			spraypaintColor = new Color(0.62f, 0.29f, 0f, options.spraypaintOpacity);
		}
	}
	[RegisterTypeInIl2Cpp]
	public class PingComponent : MonoBehaviour
	{
		public enum PingCategory
		{
			None,
			Animal,
			Spraypaint
		}

		public GameObject attachedGameObject;

		public GearItem attachedGearItem;

		public PingManager.AnimalType animalType;

		public ProjectileType spraypaintType;

		public PingCategory assignedCategory;

		public CanvasGroup canvasGroup;

		public GameObject iconObject;

		public bool isInitialized;

		public Image iconImage;

		public bool isVisible;

		private float timer;

		private float triggerTime = 5f;

		public RectTransform rectTransform;

		public bool clampOnRadar;

		public static GameObject playerObject;

		public PingComponent(IntPtr intPtr)
			: base(intPtr)
		{
		}

		public void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
		{
		}

		[HideFromIl2Cpp]
		public static bool IsRawFish(GearItem gi)
		{
			if ((Object)(object)gi != (Object)null)
			{
				FoodItem component = ((Component)gi).GetComponent<FoodItem>();
				if ((Object)(object)component != (Object)null && component.m_IsFish && component.m_IsRawMeat)
				{
					return true;
				}
			}
			return false;
		}

		[HideFromIl2Cpp]
		public void CreateIcon()
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			if (assignedCategory == PingCategory.Animal)
			{
				iconObject = Object.Instantiate<GameObject>(MotionTrackerMain.GetAnimalPrefab(animalType));
				iconImage = iconObject.GetComponent<Image>();
				((Graphic)iconImage).color = Settings.animalColor;
			}
			else if (assignedCategory == PingCategory.Spraypaint)
			{
				iconObject = Object.Instantiate<GameObject>(MotionTrackerMain.GetSpraypaintPrefab(spraypaintType));
				iconImage = iconObject.GetComponent<Image>();
				((Graphic)iconImage).color = Settings.spraypaintColor;
			}
			iconObject.transform.SetParent(((Component)PingManager.instance.iconContainer).transform, false);
			iconObject.active = true;
			canvasGroup = iconObject.GetComponent<CanvasGroup>();
			rectTransform = iconObject.GetComponent<RectTransform>();
		}

		[HideFromIl2Cpp]
		public void DeleteIcon()
		{
			if (Object.op_Implicit((Object)(object)iconObject))
			{
				Object.Destroy((Object)(object)iconObject);
			}
		}

		[HideFromIl2Cpp]
		public bool AllowedToShow()
		{
			if (assignedCategory == PingCategory.Animal)
			{
				if (animalType == PingManager.AnimalType.Crow && Settings.options.showCrows)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Rabbit && Settings.options.showRabbits)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Stag && Settings.options.showStags)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Doe && Settings.options.showDoes)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Wolf && Settings.options.showWolves)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Timberwolf && Settings.options.showTimberwolves)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Bear && Settings.options.showBears)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Cougar && Settings.options.showCougars)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Moose && Settings.options.showMoose)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.PuffyBird && Settings.options.showPuffyBirds)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Arrow && Settings.options.showArrows)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.Coal && Settings.options.showCoal)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.RawFish && Settings.options.showRawFish)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.LostAndFoundBox && Settings.options.showLostAndFoundBox)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.SaltDeposit && Settings.options.showSaltDeposit)
				{
					return true;
				}
				if (animalType == PingManager.AnimalType.BeachLoot && Settings.options.showBeachLoot)
				{
					return true;
				}
				return false;
			}
			if (assignedCategory == PingCategory.Spraypaint && Settings.options.showSpraypaint)
			{
				return true;
			}
			return false;
		}

		[HideFromIl2Cpp]
		public static void ManualDelete(PingComponent pingComponent)
		{
			if ((Object)(object)pingComponent != (Object)null)
			{
				pingComponent.DeleteIcon();
				Object.Destroy((Object)(object)pingComponent);
			}
		}

		[HideFromIl2Cpp]
		public void SetVisible(bool visibility)
		{
			if (!Object.op_Implicit((Object)(object)canvasGroup))
			{
				return;
			}
			if (AllowedToShow() && visibility)
			{
				try
				{
					canvasGroup.alpha = 1f;
					return;
				}
				catch (Exception ex)
				{
					LogMessage("Exception thrown (" + ex.Message + ") when setting canvasGroup.alpha = 1f for pingComponent.name = (" + ((Object)this).name + ":" + ((Object)this).GetInstanceID() + ")", 277, "SetVisible", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingComponent.cs");
					return;
				}
			}
			try
			{
				canvasGroup.alpha = 0f;
			}
			catch (Exception ex2)
			{
				LogMessage("Exception thrown (" + ex2.Message + ") when setting canvasGroup.alpha = 0f for pingComponent.name = (" + ((Object)this).name + ":" + ((Object)this).GetInstanceID() + ")", 304, "SetVisible", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingComponent.cs");
			}
		}

		[HideFromIl2Cpp]
		public void Initialize(PingManager.AnimalType type)
		{
			if (((Component)this).gameObject.activeSelf)
			{
				attachedGameObject = ((Component)this).gameObject;
				animalType = type;
				assignedCategory = PingCategory.Animal;
				CreateIcon();
				isInitialized = true;
				isVisible = true;
			}
		}

		[HideFromIl2Cpp]
		public void Initialize(ProjectileType type)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			attachedGameObject = ((Component)this).gameObject;
			spraypaintType = type;
			assignedCategory = PingCategory.Spraypaint;
			CreateIcon();
			isInitialized = true;
			isVisible = true;
		}

		[HideFromIl2Cpp]
		private void OnDisable()
		{
			DeleteIcon();
		}

		public void Update()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Invalid comparison between Unknown and I4
			if (!Settings.options.enableMotionTracker || !PingManager.isVisible || (int)SaveGameSystem.m_CurrentGameMode != 3 || !((Object)(object)GameManager.GetVpFPSPlayer() != (Object)null))
			{
				return;
			}
			timer += Time.deltaTime;
			if (((Object)this).name.Contains("GEAR_RawCohoSalmon", StringComparison.CurrentCultureIgnoreCase))
			{
				GearItem val = attachedGearItem;
				if ((Object)(object)val != (Object)null && !((Behaviour)val).isActiveAndEnabled)
				{
					ManualDelete(this);
					return;
				}
			}
			BaseAi component = ((Component)this).gameObject.GetComponent<BaseAi>();
			if ((Object)(object)component != (Object)null && (int)component.m_CurrentMode == 2)
			{
				ManualDelete(this);
				return;
			}
			UpdateLocatableIcons();
			if (timer > triggerTime)
			{
				timer = 0f;
			}
		}

		private void UpdateLocatableIcons()
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			if (TryGetIconLocation(out var iconLocation))
			{
				SetVisible(visibility: true);
				if (!Object.op_Implicit((Object)(object)rectTransform))
				{
					ManualDelete(this);
					return;
				}
				rectTransform.anchoredPosition = iconLocation;
				if (assignedCategory == PingCategory.Spraypaint)
				{
					if (((Graphic)iconImage).color != Settings.spraypaintColor || ((Transform)rectTransform).localScale != Settings.spraypaintScale)
					{
						((Transform)rectTransform).localScale = Settings.spraypaintScale;
						((Graphic)iconImage).color = Settings.spraypaintColor;
					}
				}
				else
				{
					if (assignedCategory != PingCategory.Animal)
					{
						return;
					}
					if (animalType == PingManager.AnimalType.Arrow || animalType == PingManager.AnimalType.Coal || animalType == PingManager.AnimalType.LostAndFoundBox || animalType == PingManager.AnimalType.SaltDeposit || animalType == PingManager.AnimalType.BeachLoot || animalType == PingManager.AnimalType.RawFish)
					{
						if (((Graphic)iconImage).color != Settings.gearColor || ((Transform)rectTransform).localScale != Settings.gearScale)
						{
							((Transform)rectTransform).localScale = Settings.gearScale;
							((Graphic)iconImage).color = Settings.gearColor;
						}
					}
					else if (((Graphic)iconImage).color != Settings.animalColor || ((Transform)rectTransform).localScale != Settings.animalScale)
					{
						((Transform)rectTransform).localScale = Settings.animalScale;
						((Graphic)iconImage).color = Settings.animalColor;
					}
					if (((Object)this).name.Contains("Arrow"))
					{
						((Graphic)iconImage).color = Color.yellow;
					}
				}
			}
			else
			{
				SetVisible(visibility: false);
			}
		}

		private bool TryGetIconLocation(out Vector2 iconLocation)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			iconLocation = GetDistanceToPlayer(this);
			float radarUISize = GetRadarUISize();
			float num = radarUISize / (float)Settings.options.detectionRange;
			iconLocation *= num;
			if (PingManager.instance.applyRotation)
			{
				Vector3 val = default(Vector3);
				((Vector3)(ref val))..ctor(0f, 0f, 0f);
				if (Object.op_Implicit((Object)(object)GameManager.GetVpFPSPlayer()))
				{
					val = Vector3.ProjectOnPlane(((Component)GameManager.GetVpFPSPlayer()).gameObject.transform.forward, Vector3.up);
				}
				Quaternion val2 = Quaternion.LookRotation(val);
				Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
				eulerAngles.y = 0f - eulerAngles.y;
				((Quaternion)(ref val2)).eulerAngles = eulerAngles;
				Vector3 val3 = val2 * new Vector3(iconLocation.x, 0f, iconLocation.y);
				iconLocation = new Vector2(val3.x, val3.z);
			}
			if (((Vector2)(ref iconLocation)).sqrMagnitude < radarUISize * radarUISize || clampOnRadar)
			{
				iconLocation = Vector2.ClampMagnitude(iconLocation, radarUISize);
				return true;
			}
			return false;
		}

		private float GetRadarUISize()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = PingManager.instance.iconContainer.rect;
			return ((Rect)(ref rect)).width / 2f;
		}

		private Vector2 GetDistanceToPlayer(PingComponent locatable)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)GameManager.GetVpFPSPlayer()) && Object.op_Implicit((Object)(object)locatable))
			{
				Vector3 val = ((Component)locatable).transform.position - ((Component)GameManager.GetVpFPSPlayer()).gameObject.transform.position;
				return new Vector2(val.x, val.z);
			}
			return new Vector2(0f, 0f);
		}
	}
	public class PingManager : MonoBehaviour
	{
		public enum AnimalType
		{
			Crow,
			Rabbit,
			Stag,
			Doe,
			Wolf,
			Timberwolf,
			Bear,
			Moose,
			PuffyBird,
			Cougar,
			Arrow,
			Coal,
			RawFish,
			LostAndFoundBox,
			SaltDeposit,
			BeachLoot
		}

		public static bool isVisible;

		public static PingManager? instance;

		public RectTransform iconContainer;

		public RectTransform radarUI;

		public Image backgroundImage;

		public Canvas trackerCanvas;

		public bool applyRotation = true;

		public static bool inMenu;

		private float timer;

		private float triggerTime = 5f;

		public Vector3 lastTransformPosition = Vector3.zero;

		public int stuckPositionCounter;

		public Dictionary<int, Vector3> iconPosition = new Dictionary<int, Vector3>();

		public PingManager(IntPtr intPtr)
			: base(intPtr)
		{
		}//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)


		public void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
		{
		}

		public void ClearIcons()
		{
			Image[] array = Il2CppArrayBase<Image>.op_Implicit(((Component)((Component)iconContainer).transform).GetComponentsInChildren<Image>());
			foreach (Image obj in array)
			{
				_ = (Object)(object)((Component)obj).gameObject == (Object)null;
				Object.Destroy((Object)(object)((Component)obj).gameObject);
			}
			iconPosition.Clear();
		}

		public void Update()
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			timer += Time.deltaTime;
			if (timer >= triggerTime)
			{
				int num = 0;
				Image[] array = Il2CppArrayBase<Image>.op_Implicit(((Component)((Component)iconContainer).transform).GetComponentsInChildren<Image>());
				foreach (Image val in array)
				{
					if ((Object)(object)val != (Object)null && ((Object)val).name.Contains("crow", StringComparison.CurrentCultureIgnoreCase))
					{
						if (!iconPosition.TryGetValue(((Object)((Component)val).gameObject).GetInstanceID(), out lastTransformPosition))
						{
							lastTransformPosition = Vector3.zero;
						}
						if (lastTransformPosition == ((Component)val).gameObject.transform.position)
						{
							stuckPositionCounter++;
							string[] obj = new string[9]
							{
								"Stale icon position detected!  icon # ",
								num.ToString(),
								" GameObject:Position (",
								((Object)((Component)val).gameObject).name,
								":",
								null,
								null,
								null,
								null
							};
							Vector3 position = ((Component)val).gameObject.transform.position;
							obj[5] = ((object)(Vector3)(ref position)).ToString();
							obj[6] = ") is the same as last position (";
							position = lastTransformPosition;
							obj[7] = ((object)(Vector3)(ref position)).ToString();
							obj[8] = ") so deleting it.";
							LogMessage(string.Concat(obj), 146, "Update", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingManager.cs");
							if (iconPosition.Remove(((Object)((Component)val).gameObject).GetInstanceID()))
							{
								LogMessage("Removed key/value (" + ((Object)((Component)val).gameObject).name + ":" + ((Object)((Component)val).gameObject).GetInstanceID() + ") from iconPosition dictionary.", 151, "Update", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingManager.cs");
							}
							Object.Destroy((Object)(object)((Component)val).gameObject);
						}
						else
						{
							stuckPositionCounter = 0;
							if (iconPosition.ContainsKey(((Object)((Component)val).gameObject).GetInstanceID()))
							{
								iconPosition[((Object)((Component)val).gameObject).GetInstanceID()] = ((Component)val).gameObject.transform.position;
							}
							else
							{
								iconPosition[((Object)((Component)val).gameObject).GetInstanceID()] = ((Component)val).gameObject.transform.position;
							}
						}
					}
					num++;
				}
			}
			if (AllowedToBeVisible())
			{
				SetVisible(visible: true);
			}
			else
			{
				SetVisible(visible: false);
			}
			if (timer >= triggerTime)
			{
				timer = 0f;
			}
		}

		public bool AllowedToBeVisible()
		{
			if (!Settings.options.enableMotionTracker)
			{
				return false;
			}
			if (!Object.op_Implicit((Object)(object)MotionTrackerMain.modSettingPage))
			{
				MotionTrackerMain.modSettingPage = GameObject.Find("Mod settings grid (Motion Tracker)");
			}
			if (Object.op_Implicit((Object)(object)MotionTrackerMain.modSettingPage) && MotionTrackerMain.modSettingPage.active)
			{
				return true;
			}
			if (inMenu)
			{
				return false;
			}
			if (Settings.options.displayStyle == Settings.DisplayStyle.Toggle && !Settings.toggleBool)
			{
				return false;
			}
			if (!Object.op_Implicit((Object)(object)GameManager.GetVpFPSPlayer()))
			{
				return false;
			}
			if (!Object.op_Implicit((Object)(object)GameManager.GetWeatherComponent()))
			{
				return false;
			}
			if (Settings.options.onlyOutdoors && GameManager.GetWeatherComponent().IsIndoorEnvironment())
			{
				return false;
			}
			return true;
		}

		private void SetVisible(bool visible)
		{
			if (isVisible != visible)
			{
				if (visible)
				{
					((Behaviour)trackerCanvas).enabled = true;
					isVisible = true;
				}
				else
				{
					((Behaviour)trackerCanvas).enabled = false;
					isVisible = false;
				}
			}
		}

		public void Awake()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			instance = this;
			trackerCanvas = ((Component)MotionTrackerMain.trackerObject.transform.FindChild("Canvas")).GetComponent<Canvas>();
			radarUI = ((Component)((Component)trackerCanvas).transform.FindChild("RadarUI")).GetComponent<RectTransform>();
			((Transform)radarUI).localScale = new Vector3(Settings.options.scale, Settings.options.scale, Settings.options.scale);
			iconContainer = ((Component)((Component)radarUI).transform.FindChild("IconContainer")).GetComponent<RectTransform>();
			backgroundImage = ((Component)((Component)radarUI).transform.FindChild("Background")).GetComponent<Image>();
			((Graphic)backgroundImage).color = new Color(1f, 1f, 1f, Settings.options.opacity);
			SetOpacity(Settings.options.opacity);
			Scale(Settings.options.scale);
			((Behaviour)trackerCanvas).enabled = true;
			isVisible = true;
		}

		public void Scale(float scale)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)radarUI))
			{
				((Transform)radarUI).localScale = new Vector3(scale, scale, scale);
			}
		}

		public void SetOpacity(float opacity)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)backgroundImage))
			{
				((Graphic)backgroundImage).color = new Color(1f, 1f, 1f, opacity);
			}
		}
	}
	public enum TweenState
	{
		Running,
		Paused,
		Stopped
	}
	public enum TweenStopBehavior
	{
		DoNotModify,
		Complete
	}
	public class TweenManager : MonoBehaviour
	{
		private static GameObject root;

		private static readonly List<ITween> tweens = new List<ITween>();

		private static GameObject toDestroy;

		public static TweenStopBehavior AddKeyStopBehavior = TweenStopBehavior.DoNotModify;

		public static Func<float> DefaultTimeFunc = TimeFuncDeltaTime;

		public static readonly Func<float> TimeFuncDeltaTimeFunc = TimeFuncDeltaTime;

		public static readonly Func<float> TimeFuncUnscaledDeltaTimeFunc = TimeFuncUnscaledDeltaTime;

		public static bool ClearTweensOnLevelLoad { get; set; }

		public TweenManager(IntPtr intPtr)
			: base(intPtr)
		{
		}

		[HideFromIl2Cpp]
		private static void EnsureCreated()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected O, but got Unknown
			if (!((Object)(object)root == (Object)null) || !Application.isPlaying)
			{
				return;
			}
			root = GameObject.Find("ModTemplate.UtilsTween");
			if ((Object)(object)root == (Object)null || (Object)(object)root.GetComponent<TweenManager>() == (Object)null)
			{
				if ((Object)(object)root != (Object)null)
				{
					toDestroy = root;
				}
				root = new GameObject
				{
					name = "ModTemplate.UtilsTween",
					hideFlags = (HideFlags)61
				};
				((Object)root.AddComponent<TweenManager>()).hideFlags = (HideFlags)61;
			}
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad((Object)(object)root);
			}
		}

		[HideFromIl2Cpp]
		private void Start()
		{
			if ((Object)(object)toDestroy != (Object)null)
			{
				Object.Destroy((Object)(object)toDestroy);
				toDestroy = null;
			}
		}

		[HideFromIl2Cpp]
		public static void SceneManagerSceneLoaded()
		{
			if (ClearTweensOnLevelLoad)
			{
				tweens.Clear();
			}
		}

		[HideFromIl2Cpp]
		private void Update()
		{
			for (int num = tweens.Count - 1; num >= 0; num--)
			{
				ITween tween = tweens[num];
				if (tween.Update(tween.TimeFunc()) && num < tweens.Count && tweens[num] == tween)
				{
					tweens.RemoveAt(num);
				}
			}
		}

		[HideFromIl2Cpp]
		public static void OwnUpdate()
		{
			for (int num = tweens.Count - 1; num >= 0; num--)
			{
				ITween tween = tweens[num];
				if (tween.Update(tween.TimeFunc()) && num < tweens.Count && tweens[num] == tween)
				{
					tweens.RemoveAt(num);
				}
			}
		}

		[HideFromIl2Cpp]
		public static FloatTween Tween(object key, float start, float end, float duration, Func<float, float> scaleFunc, Action<ITween<float>> progress, Action<ITween<float>> completion = null)
		{
			FloatTween floatTween = new FloatTween();
			floatTween.Key = key;
			floatTween.Setup(start, end, duration, scaleFunc, progress, completion);
			floatTween.Start();
			AddTween(floatTween);
			return floatTween;
		}

		[HideFromIl2Cpp]
		public static Vector2Tween Tween(object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector2>> progress, Action<ITween<Vector2>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector2Tween vector2Tween = new Vector2Tween();
			vector2Tween.Key = key;
			vector2Tween.Setup(start, end, duration, scaleFunc, progress, completion);
			vector2Tween.Start();
			AddTween(vector2Tween);
			return vector2Tween;
		}

		[HideFromIl2Cpp]
		public static Vector3Tween Tween(object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector3>> progress, Action<ITween<Vector3>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector3Tween vector3Tween = new Vector3Tween();
			vector3Tween.Key = key;
			vector3Tween.Setup(start, end, duration, scaleFunc, progress, completion);
			vector3Tween.Start();
			AddTween(vector3Tween);
			return vector3Tween;
		}

		[HideFromIl2Cpp]
		public static Vector4Tween Tween(object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector4>> progress, Action<ITween<Vector4>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector4Tween vector4Tween = new Vector4Tween();
			vector4Tween.Key = key;
			vector4Tween.Setup(start, end, duration, scaleFunc, progress, completion);
			vector4Tween.Start();
			AddTween(vector4Tween);
			return vector4Tween;
		}

		[HideFromIl2Cpp]
		public static ColorTween Tween(object key, Color start, Color end, float duration, Func<float, float> scaleFunc, Action<ITween<Color>> progress, Action<ITween<Color>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			ColorTween colorTween = new ColorTween();
			colorTween.Key = key;
			colorTween.Setup(start, end, duration, scaleFunc, progress, completion);
			colorTween.Start();
			AddTween(colorTween);
			return colorTween;
		}

		[HideFromIl2Cpp]
		public static QuaternionTween Tween(object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, Action<ITween<Quaternion>> progress, Action<ITween<Quaternion>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			QuaternionTween quaternionTween = new QuaternionTween();
			quaternionTween.Key = key;
			quaternionTween.Setup(start, end, duration, scaleFunc, progress, completion);
			quaternionTween.Start();
			AddTween(quaternionTween);
			return quaternionTween;
		}

		[HideFromIl2Cpp]
		public static void AddTween(ITween tween)
		{
			EnsureCreated();
			if (tween.Key != null)
			{
				RemoveTweenKey(tween.Key, AddKeyStopBehavior);
			}
			tweens.Add(tween);
		}

		[HideFromIl2Cpp]
		public static bool RemoveTween(ITween tween, TweenStopBehavior stopBehavior)
		{
			tween.Stop(stopBehavior);
			return tweens.Remove(tween);
		}

		[HideFromIl2Cpp]
		public static bool RemoveTweenKey(object key, TweenStopBehavior stopBehavior)
		{
			if (key == null)
			{
				return false;
			}
			bool result = false;
			for (int num = tweens.Count - 1; num >= 0; num--)
			{
				ITween tween = tweens[num];
				if (key.Equals(tween.Key))
				{
					tween.Stop(stopBehavior);
					tweens.RemoveAt(num);
					result = true;
				}
			}
			return result;
		}

		[HideFromIl2Cpp]
		public static void Clear()
		{
			tweens.Clear();
		}

		[HideFromIl2Cpp]
		private static float TimeFuncDeltaTime()
		{
			return Time.deltaTime;
		}

		[HideFromIl2Cpp]
		private static float TimeFuncUnscaledDeltaTime()
		{
			return Time.unscaledDeltaTime;
		}
	}
	public static class GameObjectTweenExtensions
	{
		[HideFromIl2Cpp]
		public static FloatTween Tween(this GameObject obj, object key, float start, float end, float duration, Func<float, float> scaleFunc, Action<ITween<float>> progress, Action<ITween<float>> completion = null)
		{
			FloatTween floatTween = TweenManager.Tween(key, start, end, duration, scaleFunc, progress, completion);
			floatTween.GameObject = obj;
			floatTween.Renderer = obj.GetComponent<Renderer>();
			return floatTween;
		}

		[HideFromIl2Cpp]
		public static Vector2Tween Tween(this GameObject obj, object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector2>> progress, Action<ITween<Vector2>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Vector2Tween vector2Tween = TweenManager.Tween(key, start, end, duration, scaleFunc, progress, completion);
			vector2Tween.GameObject = obj;
			vector2Tween.Renderer = obj.GetComponent<Renderer>();
			return vector2Tween;
		}

		[HideFromIl2Cpp]
		public static Vector3Tween Tween(this GameObject obj, object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector3>> progress, Action<ITween<Vector3>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Vector3Tween vector3Tween = TweenManager.Tween(key, start, end, duration, scaleFunc, progress, completion);
			vector3Tween.GameObject = obj;
			vector3Tween.Renderer = obj.GetComponent<Renderer>();
			return vector3Tween;
		}

		[HideFromIl2Cpp]
		public static Vector4Tween Tween(this GameObject obj, object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector4>> progress, Action<ITween<Vector4>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Vector4Tween vector4Tween = TweenManager.Tween(key, start, end, duration, scaleFunc, progress, completion);
			vector4Tween.GameObject = obj;
			vector4Tween.Renderer = obj.GetComponent<Renderer>();
			return vector4Tween;
		}

		[HideFromIl2Cpp]
		public static ColorTween Tween(this GameObject obj, object key, Color start, Color end, float duration, Func<float, float> scaleFunc, Action<ITween<Color>> progress, Action<ITween<Color>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			ColorTween colorTween = TweenManager.Tween(key, start, end, duration, scaleFunc, progress, completion);
			colorTween.GameObject = obj;
			colorTween.Renderer = obj.GetComponent<Renderer>();
			return colorTween;
		}

		[HideFromIl2Cpp]
		public static QuaternionTween Tween(this GameObject obj, object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, Action<ITween<Quaternion>> progress, Action<ITween<Quaternion>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			QuaternionTween quaternionTween = TweenManager.Tween(key, start, end, duration, scaleFunc, progress, completion);
			quaternionTween.GameObject = obj;
			quaternionTween.Renderer = obj.GetComponent<Renderer>();
			return quaternionTween;
		}
	}
	public interface ITween
	{
		object Key { get; }

		TweenState State { get; }

		Func<float> TimeFunc { get; set; }

		void Start();

		void Pause();

		void Resume();

		void Stop(TweenStopBehavior stopBehavior);

		bool Update(float elapsedTime);
	}
	public interface ITween<T> : ITween where T : struct
	{
		T CurrentValue { get; }

		float CurrentProgress { get; }

		Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, Action<ITween<T>> progress, Action<ITween<T>> completion = null);
	}
	public class Tween<T> : ITween<T>, ITween where T : struct
	{
		private readonly Func<ITween<T>, T, T, float, T> lerpFunc;

		private float currentTime;

		private float duration;

		private Func<float, float> scaleFunc;

		private Action<ITween<T>> progressCallback;

		private Action<ITween<T>> completionCallback;

		private TweenState state;

		private T start;

		private T end;

		private T value;

		private ITween continueWith;

		public object Key { get; set; }

		public float CurrentTime => currentTime;

		public float Duration => duration;

		public float Delay { get; set; }

		public TweenState State => state;

		public T StartValue => start;

		public T EndValue => end;

		public T CurrentValue => value;

		public Func<float> TimeFunc { get; set; }

		public GameObject GameObject { get; set; }

		public Renderer Renderer { get; set; }

		public bool ForceUpdate { get; set; }

		public float CurrentProgress { get; private set; }

		public Tween(Func<ITween<T>, T, T, float, T> lerpFunc)
		{
			this.lerpFunc = lerpFunc;
			state = TweenState.Stopped;
			TimeFunc = TweenManager.DefaultTimeFunc;
		}

		[HideFromIl2Cpp]
		public Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, Action<ITween<T>> progress, Action<ITween<T>> completion = null)
		{
			scaleFunc = scaleFunc ?? TweenScaleFunctions.Linear;
			currentTime = 0f;
			this.duration = duration;
			this.scaleFunc = scaleFunc;
			progressCallback = progress;
			completionCallback = completion;
			this.start = start;
			this.end = end;
			return this;
		}

		[HideFromIl2Cpp]
		public void Start()
		{
			if (state == TweenState.Running)
			{
				return;
			}
			if (duration <= 0f && Delay <= 0f)
			{
				value = end;
				if (progressCallback != null)
				{
					progressCallback(this);
				}
				if (completionCallback != null)
				{
					completionCallback(this);
				}
			}
			else
			{
				state = TweenState.Running;
				UpdateValue();
			}
		}

		[HideFromIl2Cpp]
		public void Pause()
		{
			if (state == TweenState.Running)
			{
				state = TweenState.Paused;
			}
		}

		[HideFromIl2Cpp]
		public void Resume()
		{
			if (state == TweenState.Paused)
			{
				state = TweenState.Running;
			}
		}

		[HideFromIl2Cpp]
		public void Stop(TweenStopBehavior stopBehavior)
		{
			if (state == TweenState.Stopped)
			{
				return;
			}
			state = TweenState.Stopped;
			if (stopBehavior == TweenStopBehavior.Complete)
			{
				currentTime = duration;
				UpdateValue();
				if (completionCallback != null)
				{
					completionCallback(this);
					completionCallback = null;
				}
				if (continueWith != null)
				{
					continueWith.Start();
					TweenManager.AddTween(continueWith);
					continueWith = null;
				}
			}
		}

		[HideFromIl2Cpp]
		public bool Update(float elapsedTime)
		{
			if (state == TweenState.Running)
			{
				if (Delay > 0f)
				{
					currentTime += elapsedTime;
					if (currentTime <= Delay)
					{
						return false;
					}
					currentTime -= Delay;
					Delay = 0f;
				}
				else
				{
					currentTime += elapsedTime;
				}
				if (currentTime >= duration)
				{
					Stop(TweenStopBehavior.Complete);
					return true;
				}
				UpdateValue();
				return false;
			}
			return state == TweenState.Stopped;
		}

		[HideFromIl2Cpp]
		public Tween<TNewTween> ContinueWith<TNewTween>(Tween<TNewTween> tween) where TNewTween : struct
		{
			tween.Key = Key;
			tween.GameObject = GameObject;
			tween.Renderer = Renderer;
			tween.ForceUpdate = ForceUpdate;
			continueWith = tween;
			return tween;
		}

		[HideFromIl2Cpp]
		private void UpdateValue()
		{
			if ((Object)(object)Renderer == (Object)null || Renderer.isVisible || ForceUpdate)
			{
				CurrentProgress = scaleFunc(currentTime / duration);
				value = lerpFunc(this, start, end, CurrentProgress);
				if (progressCallback != null)
				{
					progressCallback(this);
				}
			}
		}
	}
	public class FloatTween : Tween<float>
	{
		private static readonly Func<ITween<float>, float, float, float, float> LerpFunc = LerpFloat;

		private static float LerpFloat(ITween<float> t, float start, float end, float progress)
		{
			return start + (end - start) * progress;
		}

		public FloatTween()
			: base(LerpFunc)
		{
		}
	}
	public class Vector2Tween : Tween<Vector2>
	{
		private static readonly Func<ITween<Vector2>, Vector2, Vector2, float, Vector2> LerpFunc = LerpVector2;

		private static Vector2 LerpVector2(ITween<Vector2> t, Vector2 start, Vector2 end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Vector2.Lerp(start, end, progress);
		}

		public Vector2Tween()
			: base(LerpFunc)
		{
		}
	}
	public class Vector3Tween : Tween<Vector3>
	{
		private static readonly Func<ITween<Vector3>, Vector3, Vector3, float, Vector3> LerpFunc = LerpVector3;

		private static Vector3 LerpVector3(ITween<Vector3> t, Vector3 start, Vector3 end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.Lerp(start, end, progress);
		}

		public Vector3Tween()
			: base(LerpFunc)
		{
		}
	}
	public class Vector4Tween : Tween<Vector4>
	{
		private static readonly Func<ITween<Vector4>, Vector4, Vector4, float, Vector4> LerpFunc = LerpVector4;

		private static Vector4 LerpVector4(ITween<Vector4> t, Vector4 start, Vector4 end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Vector4.Lerp(start, end, progress);
		}

		public Vector4Tween()
			: base(LerpFunc)
		{
		}
	}
	public class ColorTween : Tween<Color>
	{
		private static readonly Func<ITween<Color>, Color, Color, float, Color> LerpFunc = LerpColor;

		private static Color LerpColor(ITween<Color> t, Color start, Color end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Color.Lerp(start, end, progress);
		}

		public ColorTween()
			: base(LerpFunc)
		{
		}
	}
	public class QuaternionTween : Tween<Quaternion>
	{
		private static readonly Func<ITween<Quaternion>, Quaternion, Quaternion, float, Quaternion> LerpFunc = LerpQuaternion;

		private static Quaternion LerpQuaternion(ITween<Quaternion> t, Quaternion start, Quaternion end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Quaternion.Lerp(start, end, progress);
		}

		public QuaternionTween()
			: base(LerpFunc)
		{
		}
	}
	public static class TweenScaleFunctions
	{
		private const float halfPi = MathF.PI / 2f;

		public static readonly Func<float, float> Linear = LinearFunc;

		public static readonly Func<float, float> QuadraticEaseIn = QuadraticEaseInFunc;

		public static readonly Func<float, float> QuadraticEaseOut = QuadraticEaseOutFunc;

		public static readonly Func<float, float> QuadraticEaseInOut = QuadraticEaseInOutFunc;

		public static readonly Func<float, float> CubicEaseIn = CubicEaseInFunc;

		public static readonly Func<float, float> CubicEaseOut = CubicEaseOutFunc;

		public static readonly Func<float, float> CubicEaseInOut = CubicEaseInOutFunc;

		public static readonly Func<float, float> QuarticEaseIn = QuarticEaseInFunc;

		public static readonly Func<float, float> QuarticEaseOut = QuarticEaseOutFunc;

		public static readonly Func<float, float> QuarticEaseInOut = QuarticEaseInOutFunc;

		public static readonly Func<float, float> QuinticEaseIn = QuinticEaseInFunc;

		public static readonly Func<float, float> QuinticEaseOut = QuinticEaseOutFunc;

		public static readonly Func<float, float> QuinticEaseInOut = QuinticEaseInOutFunc;

		public static readonly Func<float, float> SineEaseIn = SineEaseInFunc;

		public static readonly Func<float, float> SineEaseOut = SineEaseOutFunc;

		public static readonly Func<float, float> SineEaseInOut = SineEaseInOutFunc;

		private static float LinearFunc(float progress)
		{
			return progress;
		}

		private static float QuadraticEaseInFunc(float progress)
		{
			return EaseInPower(progress, 2);
		}

		private static float QuadraticEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 2);
		}

		private static float QuadraticEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 2);
		}

		private static float CubicEaseInFunc(float progress)
		{
			return EaseInPower(progress, 3);
		}

		private static float CubicEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 3);
		}

		private static float CubicEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 3);
		}

		private static float QuarticEaseInFunc(float progress)
		{
			return EaseInPower(progress, 4);
		}

		private static float QuarticEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 4);
		}

		private static float QuarticEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 4);
		}

		private static float QuinticEaseInFunc(float progress)
		{
			return EaseInPower(progress, 5);
		}

		private static float QuinticEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 5);
		}

		private static float QuinticEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 5);
		}

		private static float SineEaseInFunc(float progress)
		{
			return Mathf.Sin(progress * (MathF.PI / 2f) - MathF.PI / 2f) + 1f;
		}

		private static float SineEaseOutFunc(float progress)
		{
			return Mathf.Sin(progress * (MathF.PI / 2f));
		}

		private static float SineEaseInOutFunc(float progress)
		{
			return (Mathf.Sin(progress * MathF.PI - MathF.PI / 2f) + 1f) / 2f;
		}

		[HideFromIl2Cpp]
		private static float EaseInPower(float progress, int power)
		{
			return Mathf.Pow(progress, (float)power);
		}

		[HideFromIl2Cpp]
		private static float EaseOutPower(float progress, int power)
		{
			int num = ((power % 2 != 0) ? 1 : (-1));
			return (float)num * (Mathf.Pow(progress - 1f, (float)power) + (float)num);
		}

		[HideFromIl2Cpp]
		private static float EaseInOutPower(float progress, int power)
		{
			progress *= 2f;
			if (progress < 1f)
			{
				return Mathf.Pow(progress, (float)power) / 2f;
			}
			int num = ((power % 2 != 0) ? 1 : (-1));
			return (float)num / 2f * (Mathf.Pow(progress - 2f, (float)power) + (float)(num * 2));
		}
	}
}
namespace MotionTracker.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("MotionTracker.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal Resources()
		{
		}
	}
}
