using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppAK;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.Interactions;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;
using UnityEngine.SceneManagement;
using ttrRnStripped;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: ComVisible(false)]
[assembly: Guid("daf59c7b-b568-4a66-beca-0d8b47971845")]
[assembly: AssemblyTitle("RnStripped")]
[assembly: AssemblyDescription("Some functionality ported out of Relentless Nighs.")]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("RnStripped")]
[assembly: AssemblyCopyright("Created by ttr")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("0.3.0")]
[assembly: MelonInfo(typeof(global::ttrRnStripped.ttrRnStripped), "RnStripped", "0.3.0", "ttr", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("0.3.0.0")]
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
namespace ttrRnStripped
{
	internal class CarcassMoving : MonoBehaviour
	{
		[HarmonyPatch(typeof(Panel_BodyHarvest), "CanEnable", new Type[] { typeof(BodyHarvest) })]
		internal class Panel_BodyHarvest_CanEnable
		{
			private static void Postfix(BodyHarvest bodyHarvest, ref bool __result)
			{
				if (Settings.options.carcassMovingEnabled && !(bodyHarvest.GetCondition() < 0.5f))
				{
					__result = true;
				}
			}
		}

		[HarmonyPatch(typeof(Panel_BodyHarvest), "Enable", new Type[]
		{
			typeof(bool),
			typeof(BodyHarvest),
			typeof(bool),
			typeof(ComingFromScreenCategory)
		})]
		internal class Panel_BodyHarvest_Enable
		{
			private static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
			{
				if (!enable || !__instance.CanEnable(bh))
				{
					return;
				}
				if (isCarryingCarcass)
				{
					DropCarcass();
				}
				if (Settings.options.carcassMovingEnabled)
				{
					if (IsMovableCarcass(bh) && !isCarryingCarcass)
					{
						bodyHarvest = bh;
						carcassObj = ((Component)bh).gameObject;
						if ((Object)(object)moveCarcassBtnObj == (Object)null)
						{
							AddCarcassMoveButton(__instance);
						}
					}
					else if ((Object)(object)moveCarcassBtnObj != (Object)null)
					{
						RemoveCarcassMoveButton(__instance);
					}
				}
				else if ((Object)(object)moveCarcassBtnObj != (Object)null)
				{
					RemoveCarcassMoveButton(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(GameManager), "SetAudioModeForLoadedScene")]
		internal class GameManager_SetAudioModeForLoadedScene
		{
			private static void Postfix()
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					if ((Object)(object)carcassObj == (Object)null)
					{
						isCarryingCarcass = false;
					}
					if ((Object)(object)bodyHarvest != (Object)null)
					{
						((Behaviour)bodyHarvest).enabled = true;
					}
					saveTrigger = true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "Update")]
		internal class PlayerManager_Update_Post
		{
			private static void Postfix(PlayerManager __instance)
			{
				if (saveTrigger)
				{
					saveTrigger = false;
					GearManager.UpdateAll();
					GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
				}
			}
		}

		[HarmonyPatch(typeof(LoadScene), "Activate", new Type[] { typeof(bool) })]
		internal class LoadScene_Activate
		{
			private static void Postfix()
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass && !((Object)(object)carcassObj == (Object)null))
				{
					Object.DontDestroyOnLoad((Object)(object)((Component)carcassObj.transform.root).gameObject);
					((Behaviour)bodyHarvest).enabled = false;
				}
			}
		}

		[HarmonyPatch(typeof(GameManager), "TriggerSurvivalSaveAndDisplayHUDMessage")]
		internal class GameManager_TriggerSurvivalSaveAndDisplayHUDMessage
		{
			private static void Prefix()
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass && (Object)(object)bodyHarvest != (Object)null)
				{
					((Behaviour)bodyHarvest).enabled = true;
					MoveCarcassToPlayerPosition();
					AddCarcassToSceneSaveData();
				}
			}
		}

		[HarmonyPatch(typeof(GameAudioManager), "PlayGUIError")]
		internal class GameAudioManager_PlayGUIError_Pre
		{
			private static bool Prefix()
			{
				if (!Settings.options.carcassMovingEnabled)
				{
					return true;
				}
				Panel_BodyHarvest panel = InterfaceManager.GetPanel<Panel_BodyHarvest>();
				return StopErrorDueToCarcassBeingFrozen(panel);
			}
		}

		[HarmonyPatch(typeof(RopeClimbPoint), "OnRopeTransition", new Type[] { typeof(bool) })]
		internal static class RopeClimbPoint_OnRopeTransition
		{
			private static void Postfix()
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					DropCarcass();
				}
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "EquipItem", new Type[]
		{
			typeof(GearItem),
			typeof(bool)
		})]
		internal class PlayerManager_EquipItem
		{
			private static bool Prefix()
			{
				if (!Settings.options.carcassMovingEnabled || !isCarryingCarcass)
				{
					return true;
				}
				Utilities.DisallowActionWithModMessage("搬运猎物时无法装备物品");
				return false;
			}
		}

		[HarmonyPatch(typeof(InputManager), "ExecuteAltFire")]
		internal class InputManager_ExecuteAltFire
		{
			private static bool Prefix()
			{
				if (!Settings.options.carcassMovingEnabled || !isCarryingCarcass)
				{
					return true;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(EquipItemPopup), "ShouldHideEquipPopup")]
		internal class EquipItemPopup_ShouldHideEquipPopup
		{
			private static void Postfix(ref bool __result)
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "PlayerCanSprint")]
		internal static class PlayerManager_PlayerCanSprint
		{
			private static void Postfix(ref bool __result)
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(Panel_HUD), "Update")]
		internal class Panel_HUD_Update
		{
			private static void Postfix(Panel_HUD __instance)
			{
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					((UIWidget)__instance.m_Sprite_SprintCenter).color = __instance.m_SprintBarNoSprintColor;
					((UIWidget)__instance.m_Sprite_SprintBar).color = __instance.m_SprintBarNoSprintColor;
				}
			}
		}

		[HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease", new Type[] { typeof(float) })]
		internal class Fatigue_CalculateFatigueIncrease
		{
			private static void Postfix(ref float __result)
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					__result *= 1f + carcassWeight * 0.05f;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "CalculateModifiedCalorieBurnRate", new Type[] { typeof(float) })]
		internal static class PlayerManager_CalculateModifiedCalorieBurnRate
		{
			private static void Postfix(ref float __result)
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					__result += carcassWeight * 15f;
				}
			}
		}

		[HarmonyPatch(typeof(Encumber), "GetEncumbranceSlowdownMultiplier")]
		internal class Encumber_GetEncumbranceSlowdownMultiplier
		{
			private static void Postfix(Encumber __instance, ref float __result)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					float num = __result;
					float carcassWeight = CarcassMoving.carcassWeight;
					ItemWeight val = __instance.GetGearWeightKG();
					float num2 = carcassWeight + ((ItemWeight)(ref val)).ToQuantity(1f);
					val = __instance.GetMaxCarryCapacityKG();
					__result = num * Mathf.Clamp(1f - (num2 - ((ItemWeight)(ref val)).ToQuantity(1f)) * 0.05f, 0.1f, 0.8f);
				}
			}
		}

		[HarmonyPatch(typeof(Inventory), "GetExtraScentIntensity")]
		internal class Inventory_GetExtraScentIntensity
		{
			private static void Postfix(ref float __result)
			{
				if (Settings.options.carcassMovingEnabled && isCarryingCarcass)
				{
					__result += 33f;
				}
			}
		}

		[HarmonyPatch(typeof(BaseAi), "SetDamageImpactParameter", new Type[]
		{
			typeof(DamageSide),
			typeof(int),
			typeof(SetupDamageParamsOptions)
		})]
		internal class BaseAi_SetDamageImpactParameter
		{
			private static void Prefix(BaseAi __instance, ref DamageSide side)
			{
				if (Settings.options.carcassMovingEnabled && ((Object)(object)__instance.BaseDeer != (Object)null || (Object)(object)__instance.BaseWolf != (Object)null))
				{
					side = (DamageSide)0;
				}
			}
		}

		internal const float carryAddedFatiguePerKilo = 0.05f;

		internal const float carryAddedSlowDownPerKilo = 0.05f;

		internal const float carryAddedCaloryBurnPerKilo = 15f;

		internal static GameObject moveCarcassBtnObj;

		internal static GameObject carcassObj;

		internal static BodyHarvest bodyHarvest;

		internal static string carcassOriginalScene;

		internal static float carcassWeight;

		internal static bool isCarryingCarcass;

		internal static bool saveTrigger;

		static CarcassMoving()
		{
			ClassInjector.RegisterTypeInIl2Cpp<CarcassMoving>();
		}

		public CarcassMoving(IntPtr ptr)
			: base(ptr)
		{
		}

		internal void Update()
		{
			if (GameManager.m_IsPaused || !isCarryingCarcass)
			{
				return;
			}
			if (!Settings.options.carcassMovingEnabled)
			{
				DropCarcass();
				return;
			}
			DisplayDropCarcassPopUp();
			if (InputManager.GetAltFirePressed((MonoBehaviour)(object)this) || HasInjuryPreventingCarry() || GameManager.GetPlayerStruggleComponent().InStruggle())
			{
				DropCarcass();
			}
		}

		internal static void AddCarcassMoveButton(Panel_BodyHarvest panelBodyHarvest)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			moveCarcassBtnObj = Object.Instantiate<GameObject>(panelBodyHarvest.m_Mouse_Button_Harvest, panelBodyHarvest.m_Mouse_Button_Harvest.transform);
			moveCarcassBtnObj.GetComponentInChildren<UILocalize>().key = "搬运猎物";
			Transform transform = panelBodyHarvest.m_Mouse_Button_Harvest.transform;
			transform.localPosition += new Vector3(-100f, 0f, 0f);
			moveCarcassBtnObj.transform.localPosition = new Vector3(200f, 0f, 0f);
			UIButton componentInChildren = moveCarcassBtnObj.GetComponentInChildren<UIButton>();
			componentInChildren.onClick.Clear();
			componentInChildren.onClick.Add(new EventDelegate(Callback.op_Implicit((Action)OnMoveCarcass)));
		}

		internal static void RemoveCarcassMoveButton(Panel_BodyHarvest panelBodyHarvest)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			Object.DestroyImmediate((Object)(object)moveCarcassBtnObj);
			Transform transform = panelBodyHarvest.m_Mouse_Button_Harvest.transform;
			transform.localPosition += new Vector3(100f, 0f, 0f);
		}

		internal static bool IsMovableCarcass(BodyHarvest bodyHarvest)
		{
			return (((Object)bodyHarvest).name.Contains("Doe") || ((Object)bodyHarvest).name.Contains("Stag") || ((Object)bodyHarvest).name.Contains("Deer") || ((Object)bodyHarvest).name.Contains("Wolf")) && !((Object)bodyHarvest).name.Contains("Quarter");
		}

		internal static void OnMoveCarcass()
		{
			if (HasInjuryPreventingCarry())
			{
				Utilities.DisallowActionWithModMessage("受伤时无法搬运猎物");
				return;
			}
			PickUpCarcass();
			InterfaceManager.GetPanel<Panel_BodyHarvest>().OnBack();
		}

		internal static void PickUpCarcass()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			isCarryingCarcass = true;
			ItemWeight val = bodyHarvest.m_MeatAvailableKG;
			float num = ((ItemWeight)(ref val)).ToQuantity(1f);
			val = bodyHarvest.GetGutsAvailableWeightKg();
			float num2 = num + ((ItemWeight)(ref val)).ToQuantity(1f);
			val = bodyHarvest.GetHideAvailableWeightKg();
			carcassWeight = num2 + ((ItemWeight)(ref val)).ToQuantity(1f);
			carcassOriginalScene = GameManager.m_ActiveScene;
			CarcassMoving component = carcassObj.GetComponent<CarcassMoving>();
			if ((Object)(object)component == (Object)null)
			{
				component = carcassObj.AddComponent<CarcassMoving>();
			}
			GameManager.GetPlayerManagerComponent().UnequipItemInHands();
			HideCarcassFromView();
			PlayCarcassPickUpAudio();
		}

		internal static void DropCarcass()
		{
			isCarryingCarcass = false;
			MoveCarcassToPlayerPosition();
			BringCarcassBackIntoView();
			if (GameManager.m_ActiveScene != carcassOriginalScene)
			{
				AddCarcassToSceneSaveData();
			}
			PlayCarcassDropAudio();
			EnableCarcassMeshes();
			bodyHarvest = null;
			carcassObj = null;
		}

		internal static void EnableCarcassMeshes()
		{
			SkinnedMeshRenderer[] array = Il2CppArrayBase<SkinnedMeshRenderer>.op_Implicit(carcassObj.GetComponentsInChildren<SkinnedMeshRenderer>());
			SkinnedMeshRenderer[] array2 = array;
			foreach (SkinnedMeshRenderer val in array2)
			{
				((Renderer)val).enabled = true;
			}
		}

		internal static void HideCarcassFromView()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			carcassObj.transform.localScale = new Vector3(0f, 0f, 0f);
		}

		internal static void BringCarcassBackIntoView()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			carcassObj.transform.localScale = new Vector3(1f, 1f, 1f);
		}

		internal static void DisplayDropCarcassPopUp()
		{
			InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup.ShowGenericPopupWithDefaultActions(string.Empty, "放下猎物");
		}

		internal static bool HasInjuryPreventingCarry()
		{
			return GameManager.GetSprainedAnkleComponent().HasSprainedAnkle() || GameManager.GetSprainedWristComponent().HasSprainedWrist() || GameManager.GetSprainedWristComponent().HasSprainedWrist() || GameManager.GetBrokenRibComponent().HasBrokenRib();
		}

		internal static void MoveCarcassToPlayerPosition()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			carcassObj.transform.position = GameManager.GetPlayerTransform().position;
			carcassObj.transform.rotation = GameManager.GetPlayerTransform().rotation * Quaternion.Euler(0f, 90f, 0f);
		}

		internal static void AddCarcassToSceneSaveData()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			BodyHarvestManager.AddBodyHarvest(bodyHarvest);
			SceneManager.MoveGameObjectToScene(((Component)carcassObj.transform.root).gameObject, SceneManager.GetActiveScene());
		}

		internal static void PlayCarcassPickUpAudio()
		{
			GameAudioManager.PlaySound("Play_RopeGetOn", InterfaceManager.GetSoundEmitter());
			GameAudioManager.PlaySound(EVENTS.PLAY_EXERTIONLOW, InterfaceManager.GetSoundEmitter());
		}

		internal static void PlayCarcassDropAudio()
		{
			GameAudioManager.PlaySound(EVENTS.PLAY_BODYFALLLARGE, InterfaceManager.GetSoundEmitter());
		}

		internal static bool HarvestAmmountsAreSelected(Panel_BodyHarvest __instance)
		{
			return (float)__instance.m_MenuItem_Meat.HarvestUnitsAvailable > 0f || (float)__instance.m_MenuItem_Hide.HarvestUnitsAvailable > 0f || (float)__instance.m_MenuItem_Gut.HarvestUnitsAvailable > 0f;
		}

		internal static bool StopErrorDueToCarcassBeingFrozen(Panel_BodyHarvest panelBodyHarvest)
		{
			if ((Object)(object)panelBodyHarvest != (Object)null && !HarvestAmmountsAreSelected(panelBodyHarvest))
			{
				return false;
			}
			return true;
		}

		internal static void ResetCarcassMoving()
		{
			Utilities.ModLog("ResetCarcassMoving");
			Panel_BodyHarvest panel = InterfaceManager.GetPanel<Panel_BodyHarvest>();
			if ((Object)(object)moveCarcassBtnObj != (Object)null)
			{
				RemoveCarcassMoveButton(panel);
			}
			isCarryingCarcass = false;
			bodyHarvest = null;
			carcassObj = null;
			Utilities.ModLog("..Done");
		}
	}
	internal class ElectricTorchLighting
	{
		[HarmonyPatch(typeof(MissionServicesManager), "RegisterAnyMissionObjects")]
		internal class MissionServicesManager_RegisterAnyMissionObjects
		{
			private static void Postfix()
			{
				if (Settings.options.electricTorchLightingEnabled)
				{
					MakeTorchLightingItemsInteractible();
				}
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
		internal static class PlayerManager_InteractiveObjectsProcessInteraction
		{
			private static void Postfix(PlayerManager __instance)
			{
				if (Settings.options.electricTorchLightingEnabled && GameManager.GetAuroraManager().AuroraIsActive() && PlayerInteractingWithElectricLightSource(__instance) && __instance.PlayerHoldingTorchThatCanBeLit() && (Object)(object)InterfaceManager.GetPanel<Panel_TorchLight>() != (Object)null)
				{
					InterfaceManager.GetPanel<Panel_TorchLight>().StartTorchIgnite(2f, string.Empty, true);
				}
			}
		}

		[HarmonyPatch(typeof(DamageTrigger), "ApplyOneTimeDamage", new Type[]
		{
			typeof(GameObject),
			typeof(float)
		})]
		internal class DamageTrigger_ApplyOneTimeDamage
		{
			private static bool Prefix(DamageTrigger __instance)
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Invalid comparison between Unknown and I4
				if (!Settings.options.electricTorchLightingEnabled)
				{
					return true;
				}
				if ((int)__instance.m_DamageSource != 7)
				{
					return true;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(DamageTrigger), "ApplyContinuousDamage", new Type[]
		{
			typeof(GameObject),
			typeof(float)
		})]
		internal class DamageTrigger_ApplyContinousDamage
		{
			private static bool Prefix(DamageTrigger __instance)
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Invalid comparison between Unknown and I4
				if (!Settings.options.electricTorchLightingEnabled || (int)__instance.m_DamageSource != 7)
				{
					return true;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(DamageTrigger), "OnTriggerExit")]
		internal class DamageTrigger_OnTriggerExit
		{
			private static bool Prefix(DamageTrigger __instance)
			{
				if (!Settings.options.electricTorchLightingEnabled)
				{
					return true;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "GetInteractiveObjectUnderCrosshairs", new Type[] { typeof(float) })]
		internal class GetInteractiveObjectUnderCrosshairs
		{
			public static void Postfix(PlayerManager __instance, ref GameObject? __result)
			{
				if (((Object)(object)__result == (Object)null || (Object)(object)__result != (Object)(object)lookingAt) && (Object)(object)lookingAt != (Object)null)
				{
					SimpleInteraction component = lookingAt.GetComponent<SimpleInteraction>();
					if ((Object)(object)component != (Object)null)
					{
						((Behaviour)component).enabled = false;
						lookingAt = null;
					}
				}
				if ((Object)(object)__result != (Object)null && itemsCanLightTorch.Any(((Object)__result).name.ToLowerInvariant().Contains) && GameManager.GetAuroraManager().AuroraIsActive() && __instance.PlayerHoldingTorchThatCanBeLit())
				{
					SimpleInteraction component2 = __result.GetComponent<SimpleInteraction>();
					if ((Object)(object)component2 != (Object)null && (Object)(object)__result != (Object)(object)lookingAt)
					{
						((Behaviour)component2).enabled = true;
						lookingAt = __result;
					}
				}
			}
		}

		private static string[] itemsCanLightTorch = new string[4] { "socket", "outlet", "cableset", "electricdamage_temp" };

		private static GameObject? lookingAt = null;

		private static bool PlayerInteractingWithElectricLightSource(PlayerManager __instance)
		{
			float maxPickupRange = GameManager.GetGlobalParameters().m_MaxPickupRange;
			float num = __instance.ComputeModifiedPickupRange(maxPickupRange);
			GameObject interactiveObjectUnderCrosshairs = __instance.GetInteractiveObjectUnderCrosshairs(num);
			if ((Object)(object)interactiveObjectUnderCrosshairs != (Object)null && itemsCanLightTorch.Any(((Object)interactiveObjectUnderCrosshairs).name.ToLowerInvariant().Contains))
			{
				return true;
			}
			return false;
		}

		internal static void MakeTorchLightingItemsInteractible()
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			List<GameObject> rootObjects = Utilities.GetRootObjects();
			Dictionary<int, GameObject> dictionary = new Dictionary<int, GameObject>();
			foreach (GameObject item in rootObjects)
			{
				dictionary.Clear();
				Utilities.GetChildrenWithNameArray(item, itemsCanLightTorch, dictionary);
				if (dictionary.Count <= 0)
				{
					continue;
				}
				foreach (KeyValuePair<int, GameObject> item2 in dictionary)
				{
					item2.Value.layer = vp_Layer.InteractivePropNoCollideGear;
					SimpleInteraction val = item2.Value.gameObject.AddComponent<SimpleInteraction>();
					LocalizedString val2 = new LocalizedString();
					val2.m_LocalizationID = "GAMEPLAY_Light";
					((BaseInteraction)val).m_DefaultHoverText = val2;
					((Behaviour)val).enabled = false;
				}
			}
		}
	}
	public static class BuildInfo
	{
		public const string Name = "RnStripped";

		public const string Description = "Some functionality ported out of Relentless Nighs.";

		public const string Author = "ttr";

		public const string Company = null;

		public const string Version = "0.3.0";

		public const string DownloadLink = null;
	}
	internal class ttrRnStripped : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Debug.Log(Object.op_Implicit($"[{((MelonBase)this).Info.Name}] Version {((MelonBase)this).Info.Version} loaded!"));
			Settings.OnLoad();
		}
	}
	internal class Utilities
	{
		internal static void PlayGameErrorAudio()
		{
			GameAudioManager.PlaySound(GameAudioManager.Instance.m_ErrorAudio, ((Component)GameAudioManager.Instance).gameObject);
		}

		internal static void DisallowActionWithModMessage(string message)
		{
			PlayGameErrorAudio();
			HUDMessage.AddMessage(message, false, false);
		}

		internal static void ModLog(string message)
		{
			MelonLogger.Msg(ConsoleColor.Cyan, "RnStrip > " + message);
		}

		internal static List<GameObject> GetRootObjects()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			List<GameObject> list = new List<GameObject>();
			Scene activeScene = SceneManager.GetActiveScene();
			GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit((Il2CppArrayBase<GameObject>)(object)((Scene)(ref activeScene)).GetRootGameObjects());
			GameObject[] array2 = array;
			foreach (GameObject item in array2)
			{
				list.Add(item);
			}
			return list;
		}

		internal static void GetChildrenWithNameArray(GameObject obj, string[] lookup, Dictionary<int, GameObject> found)
		{
			if (obj.transform.childCount <= 0)
			{
				return;
			}
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				GameObject gameObject = ((Component)obj.transform.GetChild(i)).gameObject;
				if (lookup.Any(((Object)gameObject).name.ToLower().Contains) && !found.ContainsKey(((Object)gameObject).GetInstanceID()))
				{
					found.Add(((Object)gameObject).GetInstanceID(), gameObject);
				}
				GetChildrenWithNameArray(gameObject, lookup, found);
			}
		}
	}
	internal class RnSettings : JsonModSettings
	{
		[Section(" ")]
		[Name("可搬运的猎物尸骸")]
		[Description("添加了可搬运中等大小猎物的能力，如鹿和狼。搬运猎物可穿梭于任意地图，包括室内\n\n 注意：该功能仅适用一次切换场景，二次切换场景会导致放下猎物后无法识别，切记！")]
		public bool carcassMovingEnabled = true;

		[Name("极光电路引火")]
		[Description("添加了在极光期间使用电线和家用插座点燃火把的能力")]
		public bool electricTorchLightingEnabled = true;
	}
	internal static class Settings
	{
		public static RnSettings options;

		public static void OnLoad()
		{
			options = new RnSettings();
			options.AddToModSettings("可搬运的猎物v0.3");
		}
	}
}
