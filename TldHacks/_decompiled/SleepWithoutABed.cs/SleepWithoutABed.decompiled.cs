using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using SleepWithoutABed;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("SleepWithoutABed")]
[assembly: AssemblyCopyright("Created by Ezinw")]
[assembly: AssemblyFileVersion("2.3.1")]
[assembly: MelonInfo(typeof(Implementation), "SleepWithoutABed", "2.3.1", "Ezinw", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("2.3.1.0")]
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
internal static class BuildInfo
{
	internal const string ModName = "SleepWithoutABed";

	internal const string ModAuthor = "Ezinw";

	internal const string ModVersion = "2.3.1";
}
namespace SleepWithoutABed
{
	internal sealed class Implementation : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
		}
	}
	internal class Settings : JsonModSettings
	{
		[Section(" ")]
		[Name("疲劳恢复惩罚")]
		[Description("调整在没有床或睡袋的情况下睡觉时的疲劳恢复惩罚 \n\n 1=游戏默认值，模组默认值 = 0.75")]
		[Slider(0.15f, 1f, NumberFormat = "{0:0.00}")]
		public float fatigueRecoveryPenalty = 0.75f;

		[Name("无床时的生命恢复比例")]
		[Description("调整在没有床或睡袋的情况下睡觉时的生命恢复速度，模组默认 = 0.50")]
		[Slider(0.05f, 1f, NumberFormat = "{0:0.00}")]
		public float cloneBedConditionGainPerHour = 0.5f;

		[Name("冻伤速率")]
		[Description("每低于阈值1度，每小时冻伤速率增加量，模组默认 = 1.75")]
		[Slider(1f, 3f, NumberFormat = "{0:0.00}")]
		public float freezingScale = 1.75f;

		[Name("冻伤生命值损失")]
		[Description("冻伤时因寒冷暴露造成的额外生命值损失 \n\n在没有床铺或睡袋的情况下睡觉或消磨时间，此项数值越高，生命值损失越大，模组默认 = 1.20")]
		[Slider(1f, 2f, NumberFormat = "{0:0.00}")]
		public float freezingHealthLoss = 1.2f;

		[Name("低温症生命值损失")]
		[Description("患有低温症时因寒冷暴露造成的额外生命值损失 \n\n在无床铺或睡袋的情况下睡觉或消磨时间，此项数值越高，生命值损失越大，模组默认 = 1.20")]
		[Slider(1f, 2f, NumberFormat = "{0:0.00}")]
		public float hypothermicHealthLoss = 1.4f;

		[Name("消磨时间时的暴露惩罚")]
		[Description("降低消磨时间时的寒冷暴露效果，或将此数值调高，使其与睡觉时的寒冷暴露效果一致。模组默认 = 0.75")]
		[Slider(0.25f, 2f, NumberFormat = "{0:0.00}")]
		public float passTimeExposurePenalty = 0.75f;

		[Section(" ")]
		[Name("是否启用低生命值强制中断睡眠？")]
		[Description("在没有床铺或睡袋的情况下睡觉，启用或禁用低生命值强制中断睡眠")]
		[Choice(new string[] { "No", "Yes" })]
		public bool lowHealthSleepInterruption = true;

		[Name("         - 低生命值睡眠中断")]
		[Description("当生命值低于设定阈值时，中断睡眠或消磨时间，以此提高生存几率 \n\n设定为0.10 时，玩家生命值降至10% 就会被唤醒。模组默认 = 0.10")]
		[Slider(0.05f, 0.2f, NumberFormat = "{0:0.00}")]
		public float sleepInterruptionThreshold = 0.1f;

		[Name("         - 打断冷却时间")]
		[Description("控制睡眠/消磨时间时发生打断的频率 \n\n只有在经过这段时间（以秒为单位）后，才会再次发生打断。模组默认 = 15")]
		[Slider(1f, 60f)]
		public int interruptionCooldown = 15;

		[Name("         - 是否在界面上显示提示信息？")]
		[Description("当睡眠被中断时，向玩家显示或隐藏界面提示信息")]
		[Choice(new string[] { "No", "Yes" })]
		public bool hudMessage = true;

		[Name("         - 对所有床铺都启用睡眠中断吗？")]
		[Description("对所有床铺和睡袋都启用低生命值睡眠中断功能。当生命值低于阈值时，会唤醒玩家")]
		[Choice(new string[] { "No", "Yes" })]
		public bool applyInterruptToBeds = true;

		[Section(" ")]
		[Name("额外选项")]
		[Description("显示或隐藏额外选项")]
		[Choice(new string[] { "+", "-" })]
		public bool extraOptions;

		[Name("显示寒冷暴露相关设置项？")]
		[Description("显示或隐藏额外的寒冷暴露相关设置 \n\n您可以微调温度如何影响暴露惩罚 \n\n想调整暴露效果的强弱时，修改这些数值")]
		[Choice(new string[] { "+", "-" })]
		public bool exposureSettings;

		[Name("         - 灵敏度系数")]
		[Description("决定当温度降至冰点以下时，寒冷暴露的惩罚力度 \n\n数值越高，惩罚力度越强，生存难度更大。模组默认 = 0.20")]
		[Slider(0.01f, 1f, NumberFormat = "{0:0.00}")]
		public float sensitivityScale = 0.2f;

		[Name("         - 修正后的灵敏度")]
		[Description("控制寒冷暴露的惩罚力度。数值越高，惩罚力度越强；数值越低，惩罚力度越弱 \n模组默认 = 0.75")]
		[Slider(0.01f, 2f, NumberFormat = "{0:0.00}")]
		public float adjustedSensitivity = 0.75f;

		internal static Settings settings;

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			if (field.Name == "lowHealthSleepInterruption" || field.Name == "sleepInterruptionThreshold" || field.Name == "interruptionCooldown" || field.Name == "hudMessage" || field.Name == "applyInterruptToBeds" || field.Name == "extraOptions" || field.Name == "exposureSettings" || field.Name == "sensitivityScale" || field.Name == "adjustedSensitivity")
			{
				Refresh();
			}
		}

		internal void Refresh()
		{
			SetFieldVisible("sleepInterruptionThreshold", lowHealthSleepInterruption);
			SetFieldVisible("interruptionCooldown", lowHealthSleepInterruption);
			SetFieldVisible("hudMessage", lowHealthSleepInterruption);
			SetFieldVisible("applyInterruptToBeds", lowHealthSleepInterruption);
			SetFieldVisible("exposureSettings", extraOptions);
			SetFieldVisible("sensitivityScale", exposureSettings && extraOptions);
			SetFieldVisible("adjustedSensitivity", exposureSettings && extraOptions);
		}

		internal static void OnLoad()
		{
			settings = new Settings();
			settings.AddToModSettings("裸睡模组v2.3.1");
			settings.Refresh();
		}
	}
	[HarmonyPatch(typeof(Rest), "UpdateWhenSleeping")]
	public static class SWaB_SleepInterruption
	{
		private static float lastInterruptionTime = -1f;

		private static bool lastInterruptionWasConditionBased = false;

		private static float interruptionCooldown = Settings.settings.interruptionCooldown;

		private static void Postfix(Rest __instance)
		{
			if ((Object)(object)__instance == (Object)null || GameManager.GetPlayerManagerComponent().PlayerIsDead())
			{
				return;
			}
			bool applyInterruptToBeds = Settings.settings.applyInterruptToBeds;
			bool flag = (Object)(object)SWaB._tempBedroll != (Object)null;
			if ((Object)(object)__instance.m_Bed != (Object)null && !flag && !applyInterruptToBeds)
			{
				return;
			}
			Condition conditionComponent = GameManager.GetConditionComponent();
			Freezing freezingComponent = GameManager.GetFreezingComponent();
			if ((Object)(object)conditionComponent == (Object)null || (Object)(object)freezingComponent == (Object)null)
			{
				return;
			}
			float num = GetEffectiveMaxCondition.CalculateMaxCondition();
			float num2 = conditionComponent.m_CurrentHP / num;
			float sleepInterruptionThreshold = Settings.settings.sleepInterruptionThreshold;
			bool flag2 = freezingComponent.IsFreezing();
			float time = Time.time;
			if (lastInterruptionWasConditionBased && lastInterruptionTime >= 0f && time - lastInterruptionTime < interruptionCooldown)
			{
				return;
			}
			if (num2 < sleepInterruptionThreshold && flag2)
			{
				__instance.EndSleeping(true);
				lastInterruptionTime = time;
				lastInterruptionWasConditionBased = true;
				if (Settings.settings.hudMessage)
				{
					HUDMessage.AddMessage(Localization.Get("You are about to fade into the long dark. Seek shelter and warmth!"), 5f, false, false);
				}
				CameraFade.FadeIn(0.5f, 0f, (Action)null);
			}
			else
			{
				lastInterruptionWasConditionBased = false;
			}
		}
	}
	[HarmonyPatch(typeof(PassTime), "UpdatePassingTime")]
	public static class SWaB_PassTimeInterruption
	{
		private static float lastInterruptionTime = -1f;

		private static bool lastInterruptionWasConditionBased = false;

		private static float interruptionCooldown = Settings.settings.interruptionCooldown;

		private static void Postfix(PassTime __instance)
		{
			if ((Object)(object)__instance == (Object)null || GameManager.GetPlayerManagerComponent().PlayerIsDead())
			{
				return;
			}
			bool applyInterruptToBeds = Settings.settings.applyInterruptToBeds;
			bool flag = (Object)(object)SWaB._tempBedroll != (Object)null;
			if ((Object)(object)__instance.m_Bed != (Object)null && !flag && !applyInterruptToBeds)
			{
				return;
			}
			Condition conditionComponent = GameManager.GetConditionComponent();
			Freezing freezingComponent = GameManager.GetFreezingComponent();
			if ((Object)(object)conditionComponent == (Object)null || (Object)(object)freezingComponent == (Object)null)
			{
				return;
			}
			float num = GetEffectiveMaxCondition.CalculateMaxCondition();
			float num2 = conditionComponent.m_CurrentHP / num;
			float sleepInterruptionThreshold = Settings.settings.sleepInterruptionThreshold;
			bool flag2 = freezingComponent.IsFreezing();
			float time = Time.time;
			if (lastInterruptionWasConditionBased && lastInterruptionTime >= 0f && time - lastInterruptionTime < interruptionCooldown)
			{
				return;
			}
			if (num2 < sleepInterruptionThreshold && flag2)
			{
				__instance.End();
				lastInterruptionTime = time;
				lastInterruptionWasConditionBased = true;
				if (Settings.settings.hudMessage)
				{
					HUDMessage.AddMessage(Localization.Get("You are about to fade into the long dark. Seek shelter and warmth!"), 5f, false, false);
				}
			}
			else
			{
				lastInterruptionWasConditionBased = false;
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Rest), "Enable", new Type[]
	{
		typeof(bool),
		typeof(bool)
	})]
	public class SWaB
	{
		public static GearItem? _tempBedroll;

		private static void Prefix(Panel_Rest __instance, ref bool enable, ref bool passTimeOnly)
		{
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			passTimeOnly = false;
			if (enable)
			{
				if ((Object)(object)__instance.m_Bed != (Object)null)
				{
					Bed bed = __instance.m_Bed;
					GearItem? tempBedroll = _tempBedroll;
					if ((Object)(object)bed != (Object)(object)((tempBedroll != null) ? ((Component)tempBedroll).GetComponent<Bed>() : null))
					{
						return;
					}
				}
				if (!((Object)(object)__instance.m_Bed == (Object)null) || !((Object)(object)_tempBedroll == (Object)null))
				{
					return;
				}
				GearItem val = GearItem.LoadGearItemPrefab("GEAR_BedRoll");
				if (!((Object)(object)val != (Object)null))
				{
					return;
				}
				GearItem component = GearItem.InstantiateDepletedGearPrefab(((Component)val).gameObject).GetComponent<GearItem>();
				if ((Object)(object)component != (Object)null)
				{
					component.m_CurrentHP = Mathf.Max(2f, component.m_CurrentHP);
					component.m_WornOut = false;
					component.m_InPlayerInventory = false;
					((Component)component).gameObject.transform.position = GameManager.GetPlayerTransform().position;
					((Component)component).gameObject.SetActive(true);
					Bed component2 = ((Component)component).GetComponent<Bed>();
					if ((Object)(object)component2 != (Object)null)
					{
						__instance.m_Bed = component2;
						_tempBedroll = component;
						component2.m_OpenAudio = null;
						component2.m_CloseAudio = null;
						component2.SetState((BedRollState)1);
					}
				}
			}
			else
			{
				if (enable)
				{
					return;
				}
				if ((Object)(object)__instance.m_Bed != (Object)null)
				{
					Bed bed2 = __instance.m_Bed;
					GearItem? tempBedroll2 = _tempBedroll;
					if ((Object)(object)bed2 != (Object)(object)((tempBedroll2 != null) ? ((Component)tempBedroll2).GetComponent<Bed>() : null) && !GameManager.GetRestComponent().IsSleeping())
					{
						__instance.m_Bed = null;
					}
				}
				if ((Object)(object)__instance.m_Bed != (Object)null)
				{
					Bed bed3 = __instance.m_Bed;
					GearItem? tempBedroll3 = _tempBedroll;
					if ((Object)(object)bed3 == (Object)(object)((tempBedroll3 != null) ? ((Component)tempBedroll3).GetComponent<Bed>() : null) && !GameManager.GetRestComponent().IsSleeping() && (Object)(object)_tempBedroll != (Object)null)
					{
						GearManager.DestroyGearObject(((Component)_tempBedroll).gameObject);
						_tempBedroll = null;
						__instance.m_Bed = null;
					}
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_ActionsRadial), "DoPassTime")]
	public static class SWaB_Radial_DoPassTime
	{
		public static void Postfix()
		{
			Panel_Rest panel = InterfaceManager.GetPanel<Panel_Rest>();
			if ((Object)(object)panel != (Object)null)
			{
				panel.m_ShowPassTime = true;
				panel.m_ShowPassTimeOnly = false;
				panel.m_RestOnlyObject.SetActive(false);
				panel.m_PassTimeOnlyObject.SetActive(true);
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Rest), "UpdateButtonLegend")]
	public class SWaB_UpdateButtonLegend
	{
		private static void Postfix(Panel_Rest __instance)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)SWaB._tempBedroll != (Object)null)
			{
				Bed bed = __instance.m_Bed;
				GearItem? tempBedroll = SWaB._tempBedroll;
				if ((Object)(object)bed == (Object)(object)((tempBedroll != null) ? ((Component)tempBedroll).GetComponent<Bed>() : null) && __instance.m_PickUpButton.activeSelf)
				{
					Utils.SetActive(__instance.m_PickUpButton, false);
					__instance.m_SleepButton.transform.localPosition = __instance.m_SleepButtonCenteredPos;
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Rest), "OnRest")]
	public class SWaB_SleepExposure
	{
		private static void Postfix(Panel_Rest __instance)
		{
			if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
			{
				return;
			}
			Rest restComponent = GameManager.GetRestComponent();
			Freezing freezingComponent = GameManager.GetFreezingComponent();
			Condition conditionComponent = GameManager.GetConditionComponent();
			Hypothermia hypothermiaComponent = GameManager.GetHypothermiaComponent();
			if ((Object)(object)__instance == (Object)null || (Object)(object)restComponent == (Object)null || (Object)(object)freezingComponent == (Object)null || (Object)(object)conditionComponent == (Object)null || (Object)(object)hypothermiaComponent == (Object)null)
			{
				return;
			}
			if ((Object)(object)__instance.m_Bed != (Object)null)
			{
				Bed bed = __instance.m_Bed;
				GearItem? tempBedroll = SWaB._tempBedroll;
				if ((Object)(object)bed == (Object)(object)((tempBedroll != null) ? ((Component)tempBedroll).GetComponent<Bed>() : null))
				{
					goto IL_008f;
				}
			}
			if ((Object)(object)__instance.m_Bed == (Object)null)
			{
				goto IL_008f;
			}
			goto IL_00a6;
			IL_00a6:
			restComponent.m_ReduceFatiguePerHourRest = 8.33f;
			freezingComponent.m_FreezingIncreasePerHourPerDegreeCelsius = 6f;
			conditionComponent.m_HPDecreasePerDayFromFreezing = 450f;
			hypothermiaComponent.m_HPDrainPerHour = 40f;
			if ((Object)(object)SWaB._tempBedroll != (Object)null)
			{
				Bed bed2 = __instance.m_Bed;
				GearItem? tempBedroll2 = SWaB._tempBedroll;
				if ((Object)(object)bed2 == (Object)(object)((tempBedroll2 != null) ? ((Component)tempBedroll2).GetComponent<Bed>() : null))
				{
					float num = ExposurePenalty.ApplyExposurePenalty();
					float freezingIncreasePerHourPerDegreeCelsius = 6f * Settings.settings.freezingScale;
					float hPDecreasePerDayFromFreezing = 450f * Settings.settings.freezingHealthLoss + num;
					float hPDrainPerHour = 40f * Settings.settings.hypothermicHealthLoss + num;
					restComponent.m_ReduceFatiguePerHourRest = 8.33f * Settings.settings.fatigueRecoveryPenalty;
					freezingComponent.m_FreezingIncreasePerHourPerDegreeCelsius = freezingIncreasePerHourPerDegreeCelsius;
					conditionComponent.m_HPDecreasePerDayFromFreezing = hPDecreasePerDayFromFreezing;
					hypothermiaComponent.m_HPDrainPerHour = hPDrainPerHour;
				}
			}
			return;
			IL_008f:
			GearItem? tempBedroll3 = SWaB._tempBedroll;
			__instance.m_Bed = ((tempBedroll3 != null) ? ((Component)tempBedroll3).GetComponent<Bed>() : null);
			goto IL_00a6;
		}
	}
	[HarmonyPatch(typeof(Rest), "UpdateWhenSleeping")]
	public class SWaB_ConditionRecovery
	{
		private static void Postfix(Rest __instance)
		{
			if ((Object)(object)__instance == (Object)null || (Object)(object)__instance.m_Bed == (Object)null || GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
			{
				return;
			}
			__instance.m_Bed.m_ConditionPercentGainPerHour = 1f;
			__instance.m_Bed.m_UinterruptedRestPercentGainPerHour = 1f;
			if ((Object)(object)__instance.m_Bed != (Object)null)
			{
				Bed bed = __instance.m_Bed;
				GearItem? tempBedroll = SWaB._tempBedroll;
				if ((Object)(object)bed == (Object)(object)((tempBedroll != null) ? ((Component)tempBedroll).GetComponent<Bed>() : null))
				{
					__instance.m_Bed.m_ConditionPercentGainPerHour = 1f * Settings.settings.cloneBedConditionGainPerHour;
					__instance.m_Bed.m_UinterruptedRestPercentGainPerHour = 1f * Settings.settings.cloneBedConditionGainPerHour;
				}
			}
		}
	}
	[HarmonyPatch(typeof(Rest), "EndSleeping", new Type[] { typeof(bool) })]
	public class SWaB_EndSleeping
	{
		private static void Postfix(Rest __instance, ref bool interrupted)
		{
			if (!((Object)(object)__instance == (Object)null))
			{
				if (interrupted && (Object)(object)SWaB._tempBedroll != (Object)null)
				{
					GearManager.DestroyGearObject(((Component)SWaB._tempBedroll).gameObject);
					SWaB._tempBedroll = null;
				}
				__instance.m_Bed = null;
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Rest), "OnPassTime")]
	public class SWaB_PassTimeExposure
	{
		private static void Postfix(Panel_Rest __instance)
		{
			if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
			{
				return;
			}
			Freezing freezingComponent = GameManager.GetFreezingComponent();
			Condition conditionComponent = GameManager.GetConditionComponent();
			Hypothermia hypothermiaComponent = GameManager.GetHypothermiaComponent();
			if ((Object)(object)__instance == (Object)null || (Object)(object)freezingComponent == (Object)null || (Object)(object)conditionComponent == (Object)null || (Object)(object)hypothermiaComponent == (Object)null)
			{
				return;
			}
			if ((Object)(object)__instance.m_Bed != (Object)null)
			{
				Bed bed = __instance.m_Bed;
				GearItem? tempBedroll = SWaB._tempBedroll;
				if ((Object)(object)bed == (Object)(object)((tempBedroll != null) ? ((Component)tempBedroll).GetComponent<Bed>() : null))
				{
					goto IL_0080;
				}
			}
			if ((Object)(object)__instance.m_Bed == (Object)null)
			{
				goto IL_0080;
			}
			goto IL_0097;
			IL_0080:
			GearItem? tempBedroll2 = SWaB._tempBedroll;
			__instance.m_Bed = ((tempBedroll2 != null) ? ((Component)tempBedroll2).GetComponent<Bed>() : null);
			goto IL_0097;
			IL_0097:
			freezingComponent.m_FreezingIncreasePerHourPerDegreeCelsius = 6f;
			conditionComponent.m_HPDecreasePerDayFromFreezing = 450f;
			hypothermiaComponent.m_HPDrainPerHour = 40f;
			if ((Object)(object)SWaB._tempBedroll != (Object)null)
			{
				Bed bed2 = __instance.m_Bed;
				GearItem? tempBedroll3 = SWaB._tempBedroll;
				if ((Object)(object)bed2 == (Object)(object)((tempBedroll3 != null) ? ((Component)tempBedroll3).GetComponent<Bed>() : null))
				{
					float num = ExposurePenalty.ApplyExposurePenalty();
					float passTimeExposurePenalty = Settings.settings.passTimeExposurePenalty;
					float num2 = 6f * Settings.settings.freezingScale;
					float num3 = 450f * Settings.settings.freezingHealthLoss + num;
					float num4 = 40f * Settings.settings.hypothermicHealthLoss + num;
					freezingComponent.m_FreezingIncreasePerHourPerDegreeCelsius = num2 * passTimeExposurePenalty;
					conditionComponent.m_HPDecreasePerDayFromFreezing = num3 * passTimeExposurePenalty;
					hypothermiaComponent.m_HPDrainPerHour = num4 * passTimeExposurePenalty;
				}
			}
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "PlayerIsSleeping")]
	public class SWaB_RestPanelX
	{
		private static void Postfix(PlayerManager __instance, ref bool __result)
		{
			if (!((Object)(object)__instance == (Object)null) && !GameManager.m_IsPaused && !GameManager.s_IsGameplaySuspended && __result)
			{
				Panel_Rest panel = InterfaceManager.GetPanel<Panel_Rest>();
				if ((Object)(object)panel != (Object)null && ((Panel_Base)panel).IsEnabled())
				{
					((Panel_Base)panel).Enable(false);
				}
			}
		}
	}
	public static class ExposurePenalty
	{
		public static float ApplyExposurePenalty()
		{
			float num = AmbientTemperature.CalculateAmbientTemperature();
			float num2 = WarmthFromClothing.CalculateWarmth();
			float num3 = 0f;
			float num4 = num + num2;
			if (num4 >= num3)
			{
				return 0f;
			}
			float num5 = num3 - num4;
			float num6 = Settings.settings.sensitivityScale * num5;
			float num7 = Settings.settings.adjustedSensitivity + num6;
			return num5 * num7;
		}
	}
	public static class WarmthFromClothing
	{
		public static float CalculateWarmth()
		{
			PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
			if ((Object)(object)playerManagerComponent == (Object)null)
			{
				return 0f;
			}
			float warmthBonusFromClothing = playerManagerComponent.m_WarmthBonusFromClothing;
			float windproofBonusFromClothing = playerManagerComponent.m_WindproofBonusFromClothing;
			return warmthBonusFromClothing + windproofBonusFromClothing;
		}
	}
	public static class AmbientTemperature
	{
		public static float CalculateAmbientTemperature()
		{
			Weather weatherComponent = GameManager.GetWeatherComponent();
			if ((Object)(object)weatherComponent == (Object)null)
			{
				return 0f;
			}
			return weatherComponent.GetCurrentTemperatureWithWindchill();
		}
	}
	public static class GetEffectiveMaxCondition
	{
		public static float CalculateMaxCondition()
		{
			Condition conditionComponent = GameManager.GetConditionComponent();
			if (!((Object)(object)conditionComponent != (Object)null))
			{
				return 100f;
			}
			return conditionComponent.m_MaxHP;
		}
	}
}
