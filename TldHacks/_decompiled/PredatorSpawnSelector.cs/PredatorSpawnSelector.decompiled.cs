using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppTLD.AI;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using PredatorSpawnSelector;
using UnityEngine;
using UnityEngine.AddressableAssets;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("PredatorSpawnSelector")]
[assembly: AssemblyDescription("Custom timberwolf location settings")]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("PredatorSpawnSelector")]
[assembly: AssemblyCopyright("Created by moosemeat")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.3.1")]
[assembly: MelonInfo(typeof(Implementation), "PredatorSpawnSelector", "1.3.1", "moosemeat", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.3.1.0")]
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
namespace PredatorSpawnSelector
{
	internal static class BearManager
	{
		internal static void AdjustBearToRegionSetting(SpawnRegion spawnRegion)
		{
			string activeScene = GameManager.m_ActiveScene;
			if (activeScene == null)
			{
				return;
			}
			switch (activeScene.Length)
			{
			case 15:
				switch (activeScene[0])
				{
				case 'A':
					if (activeScene == "AshCanyonRegion")
					{
						SetBearType(spawnRegion, Settings.instance.ashCanyonBears);
					}
					break;
				case 'B':
					if (activeScene == "BlackrockRegion")
					{
						SetBearType(spawnRegion, Settings.instance.blackRockBears);
					}
					break;
				}
				break;
			case 19:
				switch (activeScene[0])
				{
				case 'B':
					if (activeScene == "BlackrockPrisonZone")
					{
						SetBearType(spawnRegion, Settings.instance.blackRockPrisonBears);
					}
					break;
				case 'C':
					if (activeScene == "CrashMountainRegion")
					{
						SetBearType(spawnRegion, Settings.instance.timberwolfMountainBears);
					}
					break;
				}
				break;
			case 13:
				switch (activeScene[1])
				{
				case 'a':
					if (activeScene == "CanneryRegion")
					{
						SetBearType(spawnRegion, Settings.instance.bleakInletBears);
					}
					break;
				case 'o':
					if (activeScene == "CoastalRegion")
					{
						SetBearType(spawnRegion, Settings.instance.coastalHighwayBears);
					}
					break;
				}
				break;
			case 12:
				switch (activeScene[0])
				{
				case 'T':
					if (activeScene == "TracksRegion")
					{
						SetBearType(spawnRegion, Settings.instance.brokenRailroadBears);
					}
					break;
				case 'M':
					if (activeScene == "MiningRegion")
					{
						SetBearType(spawnRegion, Settings.instance.miningRegionBears);
					}
					break;
				}
				break;
			case 11:
				switch (activeScene[0])
				{
				case 'M':
					if (activeScene == "MarshRegion")
					{
						SetBearType(spawnRegion, Settings.instance.forlornMuskegBears);
					}
					break;
				case 'R':
					if (activeScene == "RuralRegion")
					{
						SetBearType(spawnRegion, Settings.instance.pleasantValleyBears);
					}
					break;
				}
				break;
			case 18:
				switch (activeScene[8])
				{
				case 'P':
					if (activeScene == "MountainPassRegion")
					{
						SetBearType(spawnRegion, Settings.instance.mountainPassBears);
					}
					break;
				case 'T':
					if (activeScene == "MountainTownRegion")
					{
						SetBearType(spawnRegion, Settings.instance.mountainTownBears);
					}
					break;
				}
				break;
			case 20:
				if (activeScene == "WhalingStationRegion")
				{
					SetBearType(spawnRegion, Settings.instance.desolationPointBears);
				}
				break;
			case 17:
				if (activeScene == "RiverValleyRegion")
				{
					SetBearType(spawnRegion, Settings.instance.hushedRiverValleyBears);
				}
				break;
			case 10:
				if (activeScene == "LakeRegion")
				{
					SetBearType(spawnRegion, Settings.instance.mysteryLakeBears);
				}
				break;
			case 14:
				if (activeScene == "AirfieldRegion")
				{
					SetBearType(spawnRegion, Settings.instance.forsakenAirfieldBears);
				}
				break;
			case 16:
				break;
			}
		}

		internal static void HandleBearSpawnRegionsForCurrentScene()
		{
			if (Settings.instance.disableAllBears)
			{
				DisableAllBearSpawnRegions();
			}
		}

		private static int DisableAllBearSpawnRegions()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			SpawnRegion[] array = Il2CppArrayBase<SpawnRegion>.op_Implicit(Resources.FindObjectsOfTypeAll<SpawnRegion>());
			int num = 0;
			SpawnRegion[] array2 = array;
			foreach (SpawnRegion val in array2)
			{
				if ((int)val.m_AiSubTypeSpawned == 2)
				{
					((Component)val).gameObject.SetActive(false);
					num++;
				}
			}
			return num;
		}

		private static void SetBearType(SpawnRegion spawnRegion, BearSpawnType bearType)
		{
			switch (bearType)
			{
			case BearSpawnType.RegularBears:
				MakeRegularBears(spawnRegion);
				break;
			case BearSpawnType.ChallengeBears:
				MakeChallengeBears(spawnRegion);
				break;
			case BearSpawnType.AuroraBears:
				MakeAuroraBears(spawnRegion);
				break;
			case BearSpawnType.None:
				((Component)spawnRegion).gameObject.SetActive(false);
				break;
			}
		}

		private static void MakeRegularBears(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Bear")).WaitForCompletion();
			GameObject val2 = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Bear_aurora")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val2;
			}
		}

		private static void MakeChallengeBears(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_BearHuntedChallenge")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val;
				SetChallengeBearCriticalHits(val);
			}
		}

		private static void MakeAuroraBears(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Bear_aurora")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val;
			}
		}

		private static void SetChallengeBearCriticalHits(GameObject challengeBear)
		{
			try
			{
				AiBearChallengeHunted component = challengeBear.GetComponent<AiBearChallengeHunted>();
				if ((Object)(object)component != (Object)null)
				{
					((BaseAi)component).m_IgnoreCriticalHits = false;
				}
			}
			catch (Exception)
			{
			}
		}
	}
	internal static class BuildInfo
	{
		public const string Name = "PredatorSpawnSelector";

		public const string Description = "Custom timberwolf location settings";

		public const string Author = "moosemeat";

		public const string Company = null;

		public const string Version = "1.3.1";

		public const string DownloadLink = null;
	}
	internal static class CougarManager
	{
		internal static void HandleCougarTerritoriesForCurrentScene()
		{
			string activeScene = GameManager.m_ActiveScene;
			if (!string.IsNullOrEmpty(activeScene))
			{
				MelonLogger.Msg("Handling cougar territories for scene: " + activeScene);
				switch (activeScene)
				{
				case "AshCanyonRegion":
					DisableAshCanyonCougars();
					break;
				case "BlackrockRegion":
				case "BlackrockPrisonZone":
					DisableBlackrockCougars();
					break;
				case "CanneryRegion":
					DisableCanneryCougars();
					break;
				case "AirfieldRegion":
					DisableAirfieldCougars();
					break;
				case "RiverValleyRegion":
					DisableRiverValleyCougars();
					break;
				case "MountainTownRegion":
					DisableMountainTownCougars();
					break;
				case "LakeRegion":
					DisableLakeRegionCougars();
					break;
				case "RuralRegion":
					DisableRuralRegionCougars();
					break;
				case "MountainPassRegion":
					DisableMountainPassCougars();
					break;
				case "CrashMountainRegion":
					DisableTimberwolfMountainCougars();
					break;
				default:
					MelonLogger.Msg("No cougar handling implementation for scene: " + activeScene);
					break;
				}
			}
		}

		private static void DisableAshCanyonCougars()
		{
			bool num = Settings.instance.disableAshCanyon1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableAshCanyon2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableAshCanyon3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Root/Design/Cougar/AttackZoneArea/CougarTerritoryZone_a_T1", "Ash Canyon Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Root/Design/Cougar/AttackZoneArea/CougarTerritoryZone_a_T2", "Ash Canyon Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Root/Design/Cougar/AttackZoneArea/CougarTerritoryZone_a_T3", "Ash Canyon Territory 3");
			}
		}

		private static void DisableBlackrockCougars()
		{
			bool num = Settings.instance.disableBlackrock1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableBlackrock2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableBlackrock3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Blackrock Territory 1");
				Patches.SafeDisable("Design/Cougar/Cougar_RockScene_Prefab", "Blackrock Cougar Rock Scene");
			}
			if (flag)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Blackrock Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Blackrock Territory 3");
			}
		}

		private static void DisableCanneryCougars()
		{
			bool num = Settings.instance.disableBleak1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableBleak2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableBleak3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Root/Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Bleak Inlet Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Root/Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Bleak Inlet Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Root/Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Bleak Inlet Territory 3");
			}
		}

		private static void DisableAirfieldCougars()
		{
			bool num = Settings.instance.disableAirfield1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableAirfield2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableAirfield3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Cougar/AttackZones/CougarTerritoryZone_a_T1", "Forsaken Airfield Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Cougar/AttackZones/CougarTerritoryZone_a_T2", "Forsaken Airfield Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Cougar/AttackZones/CougarTerritoryZone_a_T3", "Forsaken Airfield Territory 3");
			}
		}

		private static void DisableRiverValleyCougars()
		{
			bool num = Settings.instance.disableRiverValley1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableRiverValley2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableRiverValley3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T1", "Hushed River Valley Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T2", "Hushed River Valley Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T3", "Hushed River Valley Territory 3");
			}
		}

		private static void DisableMountainTownCougars()
		{
			bool num = Settings.instance.disableMountainTown1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableMountainTown2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableMountainTown3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneAreas/CougarTerritoryZone_a_T1", "Mountain Town Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneAreas/CougarTerritoryZone_a_T2", "Mountain Town Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneAreas/CougarTerritoryZone_a_T3", "Mountain Town Territory 3");
			}
		}

		private static void DisableLakeRegionCougars()
		{
			bool num = Settings.instance.disableMysteryLake1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableMysteryLake2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableMysteryLake3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Mystery Lake Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Mystery Lake Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Mystery Lake Territory 3");
			}
		}

		private static void DisableRuralRegionCougars()
		{
			bool num = Settings.instance.disablePleasantValley1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disablePleasantValley2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disablePleasantValley3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Pleasant Valley Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Pleasant Valley Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Pleasant Valley Territory 3");
			}
		}

		private static void DisableMountainPassCougars()
		{
			bool num = Settings.instance.disableMountainPass1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableMountainPass2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableMountainPass3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Root/Wildlife_Spawns/Cougar/AttackZones/CougarTerritoryZone_a_T1", "Mountain Pass Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Root/Wildlife_Spawns/Cougar/AttackZones/CougarTerritoryZone_a_T2", "Mountain Pass Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Root/Wildlife_Spawns/Cougar/AttackZones/CougarTerritoryZone_a_T3", "Mountain Pass Territory 3");
			}
		}

		private static void DisableTimberwolfMountainCougars()
		{
			bool num = Settings.instance.disableTimberwolfMountain1 || Settings.instance.disableAllCougars;
			bool flag = Settings.instance.disableTimberwolfMountain2 || Settings.instance.disableAllCougars;
			bool flag2 = Settings.instance.disableTimberwolfMountain3 || Settings.instance.disableAllCougars;
			if (num)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T1", "Timberwolf Mountain Territory 1");
			}
			if (flag)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T2", "Timberwolf Mountain Territory 2");
			}
			if (flag2)
			{
				Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T3", "Timberwolf Mountain Territory 3");
			}
		}
	}
	internal sealed class Implementation : MelonMod
	{
		public const string settingDescription = "The type of wolf to spawn in this region. If Random, each pack is treated individually.";

		public override void OnInitializeMelon()
		{
			Settings.instance.AddToModSettings("可定制的掠食者领地v1.3.1");
		}
	}
	internal static class Patches
	{
		[HarmonyPatch(typeof(SpawnRegion), "Deserialize")]
		internal class OnlyTimberwolvesDeserialize
		{
			private static void Postfix(SpawnRegion __instance)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Invalid comparison between Unknown and I4
				if ((int)__instance.m_AiSubTypeSpawned == 1)
				{
					WolfManager.AdjustToRegionSetting(__instance);
				}
				else if ((int)__instance.m_AiSubTypeSpawned == 2)
				{
					BearManager.AdjustBearToRegionSetting(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(SpawnRegion), "Start")]
		internal class OnlyTimberwolvesStart
		{
			private static void Postfix(SpawnRegion __instance)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Invalid comparison between Unknown and I4
				if ((int)__instance.m_AiSubTypeSpawned == 1)
				{
					WolfManager.AdjustToRegionSetting(__instance);
				}
				else if ((int)__instance.m_AiSubTypeSpawned == 2)
				{
					BearManager.AdjustBearToRegionSetting(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(BaseAi), "Start")]
		internal class BaseAiStart
		{
			private static void Postfix(BaseAi __instance)
			{
				if ((Object)(object)((Component)__instance).GetComponent<AiBearChallengeHunted>() != (Object)null)
				{
					__instance.m_IgnoreCriticalHits = false;
				}
			}
		}

		[HarmonyPatch(typeof(GameManager), "Awake")]
		internal class GameManagerAwake
		{
			private static void Postfix()
			{
				CougarManager.HandleCougarTerritoriesForCurrentScene();
				BearManager.HandleBearSpawnRegionsForCurrentScene();
				WolfManager.HandleWolfSpawnRegionsForCurrentScene();
			}
		}

		[HarmonyPatch(typeof(GameManager), "Start")]
		internal class GameManagerStart
		{
			[CompilerGenerated]
			private sealed class <DelayedAnimalHandling>d__1 : IEnumerator<object>, IEnumerator, IDisposable
			{
				private int <>1__state;

				private object <>2__current;

				private int <i>5__2;

				object IEnumerator<object>.Current
				{
					[DebuggerHidden]
					get
					{
						return <>2__current;
					}
				}

				object IEnumerator.Current
				{
					[DebuggerHidden]
					get
					{
						return <>2__current;
					}
				}

				[DebuggerHidden]
				public <DelayedAnimalHandling>d__1(int <>1__state)
				{
					this.<>1__state = <>1__state;
				}

				[DebuggerHidden]
				void IDisposable.Dispose()
				{
					<>1__state = -2;
				}

				private bool MoveNext()
				{
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					//IL_0032: Expected O, but got Unknown
					//IL_0060: Unknown result type (might be due to invalid IL or missing references)
					//IL_006a: Expected O, but got Unknown
					switch (<>1__state)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>2__current = (object)new WaitForSeconds(0.5f);
						<>1__state = 1;
						return true;
					case 1:
						<>1__state = -1;
						CougarManager.HandleCougarTerritoriesForCurrentScene();
						BearManager.HandleBearSpawnRegionsForCurrentScene();
						WolfManager.HandleWolfSpawnRegionsForCurrentScene();
						<i>5__2 = 0;
						break;
					case 2:
						<>1__state = -1;
						CougarManager.HandleCougarTerritoriesForCurrentScene();
						BearManager.HandleBearSpawnRegionsForCurrentScene();
						WolfManager.HandleWolfSpawnRegionsForCurrentScene();
						<i>5__2++;
						break;
					}
					if (<i>5__2 < 5)
					{
						<>2__current = (object)new WaitForSeconds(1f);
						<>1__state = 2;
						return true;
					}
					return false;
				}

				bool IEnumerator.MoveNext()
				{
					//ILSpy generated this explicit interface implementation from .override directive in MoveNext
					return this.MoveNext();
				}

				[DebuggerHidden]
				void IEnumerator.Reset()
				{
					throw new NotSupportedException();
				}
			}

			private static void Postfix()
			{
				MelonCoroutines.Start(DelayedAnimalHandling());
			}

			[IteratorStateMachine(typeof(<DelayedAnimalHandling>d__1))]
			private static IEnumerator DelayedAnimalHandling()
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				return new <DelayedAnimalHandling>d__1(0);
			}
		}

		private const float INITIAL_DELAY = 0.5f;

		private const float RECHECK_DELAY = 1f;

		private const int MAX_RECHECKS = 5;

		internal static void SafeDisable(string objectPath, string territoryName)
		{
			GameObject val = GameObject.Find(objectPath);
			if ((Object)(object)val != (Object)null)
			{
				val.SetActive(false);
			}
		}
	}
	internal sealed class Settings : JsonModSettings
	{
		internal static Settings instance;

		[Section("可见性操控")]
		[Name("显示狼生成选项")]
		[Description("显示或隐藏所有狼的生成设置")]
		public bool showWolfSettings;

		[Name("显示熊生成选项")]
		[Description("显示或隐藏所有熊的生成设置")]
		public bool showBearSettings;

		[Name("显示美洲狮生成选项")]
		[Description("显示或隐藏美洲狮的生成设置")]
		public bool showCougarSettings;

		[Section("狼设置")]
		[Name("禁用所有普通狼的生成")]
		[Description("在所有区域禁用普通狼，该设置会覆盖下面的区域设置。注意：这不会影响到下面其他类型的狼的设置")]
		public bool disableAllRegularWolves;

		[Name("禁用所有森林狼的生成")]
		[Description("在所有区域禁用普通狼，该设置会覆盖下面的区域设置。注意：这不会影响到下面其他类型的狼的设置")]
		public bool disableAllTimberWolves;

		[Name("禁用所有毒狼的生成")]
		[Description("在所有区域禁用普通狼，该设置会覆盖下面的区域设置。注意：这不会影响到下面其他类型的狼的设置")]
		public bool disableAllStarvingWolves;

		[Name("普通狼和森林狼的占比")]
		[Description("功能是下面所有设置模组中选随机的话普通狼的占比，剩余的是森林狼占比")]
		[Slider(0f, 100f, 101)]
		public float regularWolfPercentage = 50f;

		[Name("灰烬峡谷")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType ashCanyonWolves;

		[Name("黑岩地区")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType blackRockWolves;

		[Name("黑岩监狱")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType blackRockPrisonWolves;

		[Name("荒凉水湾")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType bleakInletWolves;

		[Name("荒凉水湾罐头工厂车间")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType bleakInletWorkshopWolves;

		[Name("断开的铁路")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType brokenRailroadWolves;

		[Name("沿海公路")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType coastalHighwayWolves;

		[Name("公路废墟")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType crumblingHighwayWolves;

		[Name("荒芜据点")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType desolationPointWolves;

		[Name("荒芜据点洞穴")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType desolationPointCaveWolves;

		[Name("远境支路")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType farRangeBranchLineWolves;

		[Name("孤寂沼泽")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType forlornMuskegWolves;

		[Name("废弃机场")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType forsakenAirfieldWolves;

		[Name("寂静河谷")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType hushedRiverValleyWolves;

		[Name("守山人山隘")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType keepersPassWolves;

		[Name("矿区")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType miningRegionWolves;

		[Name("破碎山道")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType mountainPassWolves;

		[Name("山间小镇")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType mountainTownWolves;

		[Name("神秘湖")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType mysteryLakeWolves;

		[Name("怡人山谷")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType pleasantValleyWolves;

		[Name("林狼雪岭")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType timberwolfMountainWolves;

		[Name("蜿蜒河流")]
		[Description("定义该区域的狼生成类型。若设为随机，每种狼都将独立随机生成")]
		[Choice(new string[] { "默认", "森林狼", "普通狼", "毒狼", "极光森林狼", "极光普通狼", "极光毒狼", "随机", "不生成" })]
		public WolfSpawnType windingRiverWolves;

		[Section("熊设置")]
		[Name("禁用所有熊的生成")]
		[Description("在所有区域禁用熊，该设置会覆盖下面的区域设置")]
		public bool disableAllBears;

		[Name("灰烬峡谷")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType ashCanyonBears;

		[Name("黑岩地区")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType blackRockBears;

		[Name("黑岩监狱")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType blackRockPrisonBears;

		[Name("荒凉水湾")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType bleakInletBears;

		[Name("断开的铁路")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType brokenRailroadBears;

		[Name("沿海公路")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType coastalHighwayBears;

		[Name("荒芜据点")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType desolationPointBears;

		[Name("孤寂沼泽")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType forlornMuskegBears;

		[Name("废弃机场")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType forsakenAirfieldBears;

		[Name("寂静河谷")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType hushedRiverValleyBears;

		[Name("矿区")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType miningRegionBears;

		[Name("破碎山道")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType mountainPassBears;

		[Name("山间小镇")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType mountainTownBears;

		[Name("神秘湖")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType mysteryLakeBears;

		[Name("怡人山谷")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType pleasantValleyBears;

		[Name("林狼雪岭")]
		[Description("选择该区域生成的熊的类型")]
		[Choice(new string[] { "普通熊", "挑战熊", "极光熊", "不生成" })]
		public BearSpawnType timberwolfMountainBears;

		[Section("美洲狮设置")]
		[Name("禁用所有美洲狮领地")]
		[Description("在所有区域禁用美洲狮，该设置会覆盖下面的区域设置")]
		public bool disableAllCougars;

		[Name("灰烬峡谷 - 禁用领地1")]
		[Description("禁用岩棚洞穴附近的美洲狮领地")]
		public bool disableAshCanyon1;

		[Name("灰烬峡谷 - 禁用领地2")]
		[Description("禁用琵琶鱼巢穴附近的美洲狮领地")]
		public bool disableAshCanyon2;

		[Name("灰烬峡谷 - 禁用领地3")]
		[Description("禁用冰钓小屋附近的美洲狮领地")]
		public bool disableAshCanyon3;

		[Name("黑岩地区 - 禁用领地1")]
		[Description("禁用地堡附近的美洲狮领地")]
		public bool disableBlackrock1;

		[Name("黑岩地区 - 禁用领地2")]
		[Description("禁用搜寻者之迹附近的美洲狮领地")]
		public bool disableBlackrock2;

		[Name("黑岩地区 - 禁用领地3")]
		[Description("禁用工头活动场附近的美洲狮领地")]
		public bool disableBlackrock3;

		[Name("荒凉水湾 - 禁用领地1")]
		[Description("禁用阿尔法地堡附近的美洲狮领地")]
		public bool disableBleak1;

		[Name("荒凉水湾 - 禁用领地2")]
		[Description("禁用罐头厂工人居住区附近的美洲狮领地")]
		public bool disableBleak2;

		[Name("荒凉水湾 - 禁用领地3")]
		[Description("禁用被冲毁的活动房附近的美洲狮领地")]
		public bool disableBleak3;

		[Name("废弃机场 - 禁用领地1")]
		[Description("禁用冷淡洞穴附近的美洲狮领地")]
		public bool disableAirfield1;

		[Name("废弃机场 - 禁用领地2")]
		[Description("禁用正念木屋附近的美洲狮领地")]
		public bool disableAirfield2;

		[Name("废弃机场 - 禁用领地3")]
		[Description("禁用农闲憩所附近的美洲狮领地")]
		public bool disableAirfield3;

		[Name("寂静河谷 - 禁用领地1")]
		[Description("禁用地图西北角附近的美洲狮领地")]
		public bool disableRiverValley1;

		[Name("寂静河谷 - 禁用领地2")]
		[Description("禁用阶梯湖附近的美洲狮领地")]
		public bool disableRiverValley2;

		[Name("寂静河谷 - 禁用领地3")]
		[Description("禁用第三个美洲狮领地")]
		public bool disableRiverValley3;

		[Name("山间小镇 - 禁用领地1")]
		[Description("禁用天堂农场附近的美洲狮领地")]
		public bool disableMountainTown1;

		[Name("山间小镇 - 禁用领地2")]
		[Description("禁用教堂附近的美洲狮领地")]
		public bool disableMountainTown2;

		[Name("山间小镇 - 禁用领地3")]
		[Description("禁用殴卡加油站附近的美洲狮领地")]
		public bool disableMountainTown3;

		[Name("神秘湖 - 禁用领地1")]
		[Description("禁用伐木空地附近的美洲狮领地")]
		public bool disableMysteryLake1;

		[Name("神秘湖 - 禁用领地2")]
		[Description("禁用伐木场附近的美洲狮领地")]
		public bool disableMysteryLake2;

		[Name("神秘湖 - 禁用领地3")]
		[Description("禁用营地办公室附近的美洲狮领地")]
		public bool disableMysteryLake3;

		[Name("怡人山谷 - 禁用领地1")]
		[Description("禁用沉思池塘附近的美洲狮领地")]
		public bool disablePleasantValley1;

		[Name("怡人山谷 - 禁用领地2")]
		[Description("禁用农庄附近的美洲狮领地")]
		public bool disablePleasantValley2;

		[Name("怡人山谷 - 禁用领地3")]
		[Description("禁用索姆森岔路附近的美洲狮领地")]
		public bool disablePleasantValley3;

		[Name("破碎山道 - 禁用领地1")]
		[Description("禁用巨人指印附近的美洲狮领地")]
		public bool disableMountainPass1;

		[Name("破碎山道 - 禁用领地2")]
		[Description("禁用幼苗之泉附近的美洲狮领地")]
		public bool disableMountainPass2;

		[Name("破碎山道 - 禁用领地3")]
		[Description("禁用美洲狮第三个领地")]
		public bool disableMountainPass3;

		[Name("林狼雪岭 - 禁用领地1")]
		[Description("禁用机翼附近的美洲狮领地")]
		public bool disableTimberwolfMountain1;

		[Name("林狼雪岭 - 禁用领地2")]
		[Description("禁用登山者小屋附近的美洲狮领地")]
		public bool disableTimberwolfMountain2;

		[Name("林狼雪岭 - 禁用领地3")]
		[Description("禁用黑岩监狱过度口附近的美洲狮领地")]
		public bool disableTimberwolfMountain3;

		public Settings()
		{
			InitializeVisibility();
		}

		private void InitializeVisibility()
		{
			SetWolfSettingsVisible(showWolfSettings);
			SetBearSettingsVisible(showBearSettings);
			SetCougarSettingsVisible(showCougarSettings);
		}

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			if (field.Name == "showWolfSettings")
			{
				SetWolfSettingsVisible((bool)newValue);
			}
			else if (field.Name == "showBearSettings")
			{
				SetBearSettingsVisible((bool)newValue);
			}
			else if (field.Name == "showCougarSettings")
			{
				SetCougarSettingsVisible((bool)newValue);
			}
		}

		private void SetWolfSettingsVisible(bool visible)
		{
			string[] array = new string[26]
			{
				"regularWolfPercentage", "ashCanyonWolves", "blackRockWolves", "blackRockPrisonWolves", "bleakInletWolves", "bleakInletWorkshopWolves", "brokenRailroadWolves", "coastalHighwayWolves", "crumblingHighwayWolves", "desolationPointWolves",
				"desolationPointCaveWolves", "farRangeBranchLineWolves", "forlornMuskegWolves", "forsakenAirfieldWolves", "hushedRiverValleyWolves", "keepersPassWolves", "miningRegionWolves", "mountainPassWolves", "mountainTownWolves", "mysteryLakeWolves",
				"pleasantValleyWolves", "timberwolfMountainWolves", "windingRiverWolves", "disableAllRegularWolves", "disableAllTimberWolves", "disableAllStarvingWolves"
			};
			foreach (string name in array)
			{
				FieldInfo field = typeof(Settings).GetField(name);
				if (field != null)
				{
					SetFieldVisible(field, visible);
				}
			}
		}

		private void SetBearSettingsVisible(bool visible)
		{
			string[] array = new string[17]
			{
				"ashCanyonBears", "blackRockBears", "blackRockPrisonBears", "bleakInletBears", "brokenRailroadBears", "coastalHighwayBears", "desolationPointBears", "forlornMuskegBears", "forsakenAirfieldBears", "hushedRiverValleyBears",
				"disableAllBears", "miningRegionBears", "mountainPassBears", "mountainTownBears", "mysteryLakeBears", "pleasantValleyBears", "timberwolfMountainBears"
			};
			foreach (string name in array)
			{
				FieldInfo field = typeof(Settings).GetField(name);
				if (field != null)
				{
					SetFieldVisible(field, visible);
				}
			}
		}

		private void SetCougarSettingsVisible(bool visible)
		{
			string[] array = new string[31]
			{
				"disableAllCougars", "disableAshCanyon1", "disableAshCanyon2", "disableAshCanyon3", "disableBlackrock1", "disableBlackrock2", "disableBlackrock3", "disableBleak1", "disableBleak2", "disableBleak3",
				"disableAirfield1", "disableAirfield2", "disableAirfield3", "disableRiverValley1", "disableRiverValley2", "disableRiverValley3", "disableMountainTown1", "disableMountainTown2", "disableMountainTown3", "disableMysteryLake1",
				"disableMysteryLake2", "disableMysteryLake3", "disablePleasantValley1", "disablePleasantValley2", "disablePleasantValley3", "disableMountainPass1", "disableMountainPass2", "disableMountainPass3", "disableTimberwolfMountain1", "disableTimberwolfMountain2",
				"disableTimberwolfMountain3"
			};
			foreach (string name in array)
			{
				FieldInfo field = typeof(Settings).GetField(name);
				if (field != null)
				{
					SetFieldVisible(field, visible);
				}
			}
		}

		static Settings()
		{
			instance = new Settings();
		}
	}
	internal enum SpawnType
	{
		Default,
		Timberwolves,
		RegularWolves,
		PoisonWolves,
		Random,
		None
	}
	internal enum BearSpawnType
	{
		RegularBears,
		ChallengeBears,
		AuroraBears,
		None
	}
	internal enum WolfSpawnType
	{
		Default,
		Timberwolves,
		RegularWolves,
		StarvingWolves,
		AuroraTimberwolves,
		AuroraRegularWolves,
		AuroraStarvingWolves,
		Random,
		None
	}
	internal enum CougarSpawnType
	{
		Default,
		Enabled,
		Disabled
	}
	internal static class WolfManager
	{
		internal static void AdjustToRegionSetting(SpawnRegion spawnRegion)
		{
			string activeScene = GameManager.m_ActiveScene;
			if (activeScene == null)
			{
				return;
			}
			switch (activeScene.Length)
			{
			case 15:
				switch (activeScene[0])
				{
				case 'A':
					if (activeScene == "AshCanyonRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.ashCanyonWolves);
					}
					break;
				case 'B':
					if (activeScene == "BlackrockRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.blackRockWolves);
					}
					break;
				}
				break;
			case 19:
				switch (activeScene[0])
				{
				case 'B':
					if (activeScene == "BlackrockPrisonZone")
					{
						SetWolfType(spawnRegion, Settings.instance.blackRockPrisonWolves);
					}
					break;
				case 'C':
					if (activeScene == "CrashMountainRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.timberwolfMountainWolves);
					}
					break;
				}
				break;
			case 13:
				switch (activeScene[1])
				{
				case 'a':
					if (activeScene == "CanneryRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.bleakInletWolves);
					}
					break;
				case 'o':
					if (activeScene == "CoastalRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.coastalHighwayWolves);
					}
					break;
				}
				break;
			case 24:
				switch (activeScene[0])
				{
				case 'M':
					if (activeScene == "MaintenanceShedB_SANDBOX")
					{
						SetWolfType(spawnRegion, Settings.instance.bleakInletWorkshopWolves);
					}
					break;
				case 'C':
					if (activeScene == "CanyonRoadTransitionZone")
					{
						SetWolfType(spawnRegion, Settings.instance.keepersPassWolves);
					}
					break;
				}
				break;
			case 12:
				switch (activeScene[0])
				{
				case 'T':
					if (activeScene == "TracksRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.brokenRailroadWolves);
					}
					break;
				case 'M':
					if (activeScene == "MiningRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.miningRegionWolves);
					}
					break;
				}
				break;
			case 11:
				switch (activeScene[0])
				{
				case 'M':
					if (activeScene == "MarshRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.forlornMuskegWolves);
					}
					break;
				case 'R':
					if (activeScene == "RuralRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.pleasantValleyWolves);
					}
					break;
				}
				break;
			case 18:
				switch (activeScene[8])
				{
				case 'P':
					if (activeScene == "MountainPassRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.mountainPassWolves);
					}
					break;
				case 'T':
					if (activeScene == "MountainTownRegion")
					{
						SetWolfType(spawnRegion, Settings.instance.mountainTownWolves);
					}
					break;
				}
				break;
			case 21:
				if (activeScene == "HighwayTransitionZone")
				{
					SetWolfType(spawnRegion, Settings.instance.crumblingHighwayWolves);
				}
				break;
			case 20:
				if (activeScene == "WhalingStationRegion")
				{
					SetWolfType(spawnRegion, Settings.instance.desolationPointWolves);
				}
				break;
			case 5:
				if (activeScene == "CaveC")
				{
					SetWolfType(spawnRegion, Settings.instance.desolationPointCaveWolves);
				}
				break;
			case 17:
				if (activeScene == "RiverValleyRegion")
				{
					SetWolfType(spawnRegion, Settings.instance.hushedRiverValleyWolves);
				}
				break;
			case 10:
				if (activeScene == "LakeRegion")
				{
					SetWolfType(spawnRegion, Settings.instance.mysteryLakeWolves);
				}
				break;
			case 23:
				if (activeScene == "DamRiverTransitionZoneB")
				{
					SetWolfType(spawnRegion, Settings.instance.windingRiverWolves);
				}
				break;
			case 22:
				if (activeScene == "LongRailTransitionZone")
				{
					SetWolfType(spawnRegion, Settings.instance.farRangeBranchLineWolves);
				}
				break;
			case 14:
				if (activeScene == "AirfieldRegion")
				{
					SetWolfType(spawnRegion, Settings.instance.forsakenAirfieldWolves);
				}
				break;
			case 6:
			case 7:
			case 8:
			case 9:
			case 16:
				break;
			}
		}

		internal static void HandleWolfSpawnRegionsForCurrentScene()
		{
			if (Settings.instance.disableAllRegularWolves)
			{
				DisableRegularWolfSpawnRegions();
			}
			if (Settings.instance.disableAllTimberWolves)
			{
				DisableTimberWolfSpawnRegions();
			}
			if (Settings.instance.disableAllStarvingWolves)
			{
				DisableStarvingWolfSpawnRegions();
			}
		}

		private static int DisableRegularWolfSpawnRegions()
		{
			GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit(Resources.FindObjectsOfTypeAll<GameObject>());
			int num = 0;
			GameObject[] array2 = array;
			foreach (GameObject val in array2)
			{
				if (((Object)val).name.Contains("SPAWNREGION_Wolf") && !((Object)val).name.Contains("SPAWNREGION_Timberwolf") && !((Object)val).name.Contains("SPAWNREGION_WolfStarving"))
				{
					_ = val.activeSelf;
					val.SetActive(false);
					num++;
				}
			}
			return num;
		}

		private static int DisableTimberWolfSpawnRegions()
		{
			GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit(Resources.FindObjectsOfTypeAll<GameObject>());
			int num = 0;
			GameObject[] array2 = array;
			foreach (GameObject val in array2)
			{
				if (((Object)val).name.Contains("SPAWNREGION_Timberwolf"))
				{
					_ = val.activeSelf;
					val.SetActive(false);
					num++;
				}
			}
			return num;
		}

		private static int DisableStarvingWolfSpawnRegions()
		{
			GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit(Resources.FindObjectsOfTypeAll<GameObject>());
			int num = 0;
			GameObject[] array2 = array;
			foreach (GameObject val in array2)
			{
				if (((Object)val).name.Contains("SPAWNREGION_WolfStarving"))
				{
					_ = val.activeSelf;
					val.SetActive(false);
					num++;
				}
			}
			return num;
		}

		private static void SetWolfType(SpawnRegion spawnRegion, WolfSpawnType spawnType)
		{
			switch (spawnType)
			{
			case WolfSpawnType.RegularWolves:
				MakeRegularWolves(spawnRegion);
				break;
			case WolfSpawnType.Timberwolves:
				MakeTimberwolves(spawnRegion);
				break;
			case WolfSpawnType.StarvingWolves:
				MakeStarvingWolves(spawnRegion);
				break;
			case WolfSpawnType.AuroraRegularWolves:
				MakeAuroraRegularWolves(spawnRegion);
				break;
			case WolfSpawnType.AuroraTimberwolves:
				MakeAuroraTimberwolves(spawnRegion);
				break;
			case WolfSpawnType.AuroraStarvingWolves:
				MakeAuroraStarvingWolves(spawnRegion);
				break;
			case WolfSpawnType.Random:
				MakeRandomWolves(spawnRegion);
				break;
			case WolfSpawnType.None:
				((Component)spawnRegion).gameObject.SetActive(false);
				break;
			case WolfSpawnType.Default:
				break;
			}
		}

		private static void MakeTimberwolves(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_grey")).WaitForCompletion();
			GameObject val2 = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_grey_aurora")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val2;
				AdjustTimberwolfPackSize(spawnRegion);
			}
		}

		private static void MakeRegularWolves(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf")).WaitForCompletion();
			GameObject val2 = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_aurora")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val2;
			}
		}

		private static void MakeStarvingWolves(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_starving")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val;
			}
		}

		private static void MakeAuroraTimberwolves(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_grey_aurora")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val;
				AdjustTimberwolfPackSize(spawnRegion);
			}
		}

		private static void MakeAuroraRegularWolves(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_aurora")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val;
			}
		}

		private static void MakeAuroraStarvingWolves(SpawnRegion spawnRegion)
		{
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("WILDLIFE_Wolf_starving")).WaitForCompletion();
			if (Object.op_Implicit((Object)(object)val) && ((Object)spawnRegion.m_SpawnablePrefab).name != ((Object)val).name)
			{
				spawnRegion.m_SpawnablePrefab = val;
				spawnRegion.m_AuroraSpawnablePrefab = val;
			}
		}

		private static void MakeRandomWolves(SpawnRegion spawnRegion)
		{
			if (Utils.RollChance(Settings.instance.regularWolfPercentage))
			{
				MakeRegularWolves(spawnRegion);
			}
			else
			{
				MakeTimberwolves(spawnRegion);
			}
		}

		private static void AdjustTimberwolfPackSize(SpawnRegion spawnRegion)
		{
			spawnRegion.m_MaxSimultaneousSpawnsDayInterloper = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayInterloper);
			spawnRegion.m_MaxSimultaneousSpawnsDayPilgrim = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayPilgrim);
			spawnRegion.m_MaxSimultaneousSpawnsDayStalker = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayStalker);
			spawnRegion.m_MaxSimultaneousSpawnsDayVoyageur = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayVoyageur);
			spawnRegion.m_MaxSimultaneousSpawnsNightInterloper = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightInterloper);
			spawnRegion.m_MaxSimultaneousSpawnsNightPilgrim = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightPilgrim);
			spawnRegion.m_MaxSimultaneousSpawnsNightStalker = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightStalker);
			spawnRegion.m_MaxSimultaneousSpawnsNightVoyageur = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightVoyageur);
		}

		private static int NotOne(int num)
		{
			if (num != 1)
			{
				return num;
			}
			return 2;
		}
	}
}
