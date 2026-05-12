using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using CaffeinatedSodas;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("CaffeinatedSodas")]
[assembly: AssemblyCopyright("Marcy")]
[assembly: AssemblyFileVersion("1.0.1")]
[assembly: MelonInfo(typeof(CaffeinatedSodasMelon), "CaffeinatedSodas", "1.0.1", "Marcy", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.0.1.0")]
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
namespace CaffeinatedSodas
{
	public class CaffeinatedSodasMelon : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Settings.instance.AddToModSettings("劲爽苏打水v1.0.1");
			Settings.OnLoad();
		}
	}
	[HarmonyPatch(typeof(GearItem), "Awake")]
	internal class SodaPatches
	{
		private static void Postfix(ref GearItem __instance)
		{
			if (((Object)__instance).name.Contains("GEAR_SodaOrange") && Settings.instance.Orange)
			{
				FatigueBuff val = ((Component)__instance).gameObject.AddComponent<FatigueBuff>();
				__instance.m_FatigueBuff = val;
				val.m_InitialPercentDecrease = Settings.instance.OrangeInitial;
				val.m_RateOfIncreaseScale = 1f;
				switch (Settings.instance.OrangeDuration)
				{
				case 0:
					val.m_DurationHours = CaffeinatedSodasUtils.duration_small();
					break;
				case 1:
					val.m_DurationHours = CaffeinatedSodasUtils.duration_medium();
					break;
				case 2:
					val.m_DurationHours = CaffeinatedSodasUtils.duration_long();
					break;
				case 3:
					val.m_DurationHours = CaffeinatedSodasUtils.duration_verylong();
					break;
				}
			}
			if (((Object)__instance).name.Contains("GEAR_Soda") && !((Object)__instance).name.Contains("GEAR_SodaOrange") && !((Object)__instance).name.Contains("GEAR_SodaGrape") && Settings.instance.Summit)
			{
				FatigueBuff val2 = ((Component)__instance).gameObject.AddComponent<FatigueBuff>();
				__instance.m_FatigueBuff = val2;
				val2.m_InitialPercentDecrease = Settings.instance.SummitInitial;
				val2.m_RateOfIncreaseScale = 1f;
				switch (Settings.instance.SummitDuration)
				{
				case 0:
					val2.m_DurationHours = CaffeinatedSodasUtils.duration_small();
					break;
				case 1:
					val2.m_DurationHours = CaffeinatedSodasUtils.duration_medium();
					break;
				case 2:
					val2.m_DurationHours = CaffeinatedSodasUtils.duration_long();
					break;
				case 3:
					val2.m_DurationHours = CaffeinatedSodasUtils.duration_verylong();
					break;
				}
			}
			if (((Object)__instance).name.Contains("GEAR_SodaGrape") && Settings.instance.Grape)
			{
				FatigueBuff val3 = ((Component)__instance).gameObject.AddComponent<FatigueBuff>();
				__instance.m_FatigueBuff = val3;
				val3.m_InitialPercentDecrease = Settings.instance.GrapeInitial;
				val3.m_RateOfIncreaseScale = 1f;
				switch (Settings.instance.GrapeDuration)
				{
				case 0:
					val3.m_DurationHours = CaffeinatedSodasUtils.duration_small();
					break;
				case 1:
					val3.m_DurationHours = CaffeinatedSodasUtils.duration_medium();
					break;
				case 2:
					val3.m_DurationHours = CaffeinatedSodasUtils.duration_long();
					break;
				case 3:
					val3.m_DurationHours = CaffeinatedSodasUtils.duration_verylong();
					break;
				}
			}
		}
	}
	internal class Settings : JsonModSettings
	{
		internal static Settings instance;

		[Section("橘子味苏打水属性修改")]
		[Name("启用橘子味苏打水")]
		[Description("饮用橘子味苏打水可减少疲劳增益，需要重新加载场景才能生效")]
		public bool Orange = true;

		[Name("橘子味苏打水buff增益量")]
		[Description("饮用橙味苏打水后立即恢复的疲劳值，需要重新加载场景才能生效")]
		[Slider(1f, 10f)]
		public int OrangeInitial = 7;

		[Name("橘子味苏打水buff持续时间")]
		[Description("饮用橘子味苏打水后减少疲劳增益的持续时间，需要重新加载场景才能生效")]
		[Choice(new string[] { "5分钟", "10分钟", "15分钟", "30分钟" })]
		public int OrangeDuration = 1;

		[Section("尖峰苏打水属性修改")]
		[Name("启用尖峰苏打水")]
		[Description("饮用尖峰苏打水可减少疲劳增益，需要重新加载场景才能生效")]
		public bool Summit = true;

		[Name("尖峰苏打水buff增益量")]
		[Description("饮用尖峰苏打水后立即恢复的疲劳值，需要重新加载场景才能生效")]
		[Slider(1f, 10f)]
		public int SummitInitial = 3;

		[Name("尖峰苏打水buff持续时间")]
		[Description("饮用尖峰苏打水后减少疲劳增益的持续时间，需要重新加载场景才能生效")]
		[Choice(new string[] { "5分钟", "10分钟", "15分钟", "30分钟" })]
		public int SummitDuration;

		[Section("葡萄味苏打水属性修改")]
		[Name("启用葡萄味苏打水")]
		[Description("饮用葡萄味苏打水可减少疲劳增益，需要重新加载场景才能生效")]
		public bool Grape = true;

		[Name("葡萄味苏打水buff增益量")]
		[Description("饮用葡萄味苏打水后立即恢复的疲劳值，需要重新加载场景才能生效")]
		[Slider(1f, 10f)]
		public int GrapeInitial = 5;

		[Name("葡萄味苏打水buff持续时间")]
		[Description("饮用葡萄味苏打水后减少疲劳增益的持续时间，需要重新加载场景才能生效")]
		[Choice(new string[] { "5分钟", "10分钟", "15分钟", "30分钟" })]
		public int GrapeDuration = 1;

		[Section("重置")]
		[Name("重置为默认")]
		[Description("将所有设置重置为默认，点确认按钮！")]
		public bool ResetSettings;

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			RefreshFields();
		}

		protected override void OnConfirm()
		{
			ApplyReset();
			instance.ResetSettings = false;
			instance.RefreshGUI();
			base.OnConfirm();
		}

		internal static void OnLoad()
		{
			instance.RefreshFields();
		}

		internal void RefreshFields()
		{
			if (Orange)
			{
				SetFieldVisible("OrangeInitial", visible: true);
				SetFieldVisible("OrangeDuration", visible: true);
			}
			else
			{
				SetFieldVisible("OrangeInitial", visible: false);
				SetFieldVisible("OrangeDuration", visible: false);
			}
			if (Summit)
			{
				SetFieldVisible("SummitInitial", visible: true);
				SetFieldVisible("SummitDuration", visible: true);
			}
			else
			{
				SetFieldVisible("SummitInitial", visible: false);
				SetFieldVisible("SummitDuration", visible: false);
			}
			if (Grape)
			{
				SetFieldVisible("GrapeInitial", visible: true);
				SetFieldVisible("GrapeDuration", visible: true);
			}
			else
			{
				SetFieldVisible("GrapeInitial", visible: false);
				SetFieldVisible("GrapeDuration", visible: false);
			}
		}

		public static void ApplyReset()
		{
			if (instance.ResetSettings)
			{
				instance.Orange = true;
				instance.Summit = true;
				instance.Grape = true;
				instance.OrangeInitial = 7;
				instance.OrangeDuration = 1;
				instance.SummitInitial = 3;
				instance.SummitDuration = 0;
				instance.GrapeInitial = 5;
				instance.GrapeDuration = 1;
				instance.RefreshFields();
				instance.RefreshGUI();
			}
		}

		static Settings()
		{
			instance = new Settings();
		}
	}
	internal static class CaffeinatedSodasUtils
	{
		public static float duration_small()
		{
			return 0.085f;
		}

		public static float duration_medium()
		{
			return 0.167f;
		}

		public static float duration_long()
		{
			return 0.25f;
		}

		public static float duration_verylong()
		{
			return 0.5f;
		}
	}
}
