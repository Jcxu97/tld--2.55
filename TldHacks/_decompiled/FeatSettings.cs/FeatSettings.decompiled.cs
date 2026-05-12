using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using ExpandedAiFramework;
using FeatSettings;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gameplay;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using ModSettings;
using TLD.TinyJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("FeatSettings")]
[assembly: AssemblyCopyright("MonsiuerMeh")]
[assembly: AssemblyFileVersion("1.4.1")]
[assembly: MelonInfo(typeof(Main), "FeatSettings", "1.4.1", "MonsieurMeh", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.4.1.0")]
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
namespace ExpandedAiFramework
{
	public class SceneUtilities
	{
		public static string GetActiveSceneName()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Scene activeScene = SceneManager.GetActiveScene();
			return ((Scene)(ref activeScene)).name;
		}

		public static bool IsSceneModded(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName.StartsWith("mod");
		}

		public static bool IsSceneIndoor(bool scene)
		{
			if (!(GameManager.GetWeatherComponent().IsIndoorScene() && scene))
			{
				return GameManager.GetWeatherComponent().IsIndoorEnvironment();
			}
			return true;
		}

		public static bool IsSceneEmpty(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName?.Contains("Empty", StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public static bool IsSceneBoot(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName?.Contains("Boot", StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public static bool IsSceneMenu(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName?.StartsWith("MainMenu", StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public static bool IsScenePlayable(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			if (sceneName != null && !IsSceneEmpty(sceneName) && !IsSceneBoot(sceneName))
			{
				return !IsSceneMenu(sceneName);
			}
			return false;
		}

		public static bool IsSceneBase(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			if (sceneName != null)
			{
				if (!sceneName.Contains("Region", StringComparison.InvariantCultureIgnoreCase))
				{
					return sceneName.Contains("Zone", StringComparison.InvariantCultureIgnoreCase);
				}
				return true;
			}
			return false;
		}

		public static bool IsSceneSandbox(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName?.EndsWith("SANDBOX", StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public static bool IsSceneDLC01(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName?.EndsWith("DLC01", StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public static bool IsSceneDarkWalker(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			return sceneName?.Contains("DARKWALKER", StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public static bool IsSceneAdditive(string? sceneName = null)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			if (sceneName != null)
			{
				if (!IsSceneSandbox(sceneName) && !IsSceneDLC01(sceneName))
				{
					return IsSceneDarkWalker(sceneName);
				}
				return true;
			}
			return false;
		}

		public static bool IsValidSceneForWeather(string sceneName, bool IndoorOverride)
		{
			if (sceneName == null)
			{
				sceneName = GetActiveSceneName();
			}
			if (sceneName == null || !IsSceneBase(sceneName) || IsSceneAdditive(sceneName) || GameManager.GetWeatherComponent().IsIndoorScene())
			{
				return GameManager.GetWeatherComponent().IsIndoorScene() && IndoorOverride;
			}
			return true;
		}
	}
}
namespace FeatSettings
{
	public class BlizzardWalkerSettings : FeatSpecificSettings<Feat_BlizzardWalker>
	{
		[Section("暴雪行者")]
		[Name("所需户外天数")]
		[Slider(5f, 100f, 20)]
		[Description("设置需要在暴风雪期间于户外过度多少天才能解锁，默认=20")]
		public int DaysRequired = 20;

		[Name("风中行走速度降低效果")]
		[Slider(5f, 100f, 20)]
		[Description("降低强风所造成的移速惩罚百分比，默认=25%")]
		public int PenaltyReductionPercent = 25;

		public override string FeatName => "BlizzardWalker";

		public override bool Vanilla => true;

		public BlizzardWalkerSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_BlizzardHoursOutsideRequired = (float)DaysRequired * 24f;
				mFeat.m_WalkingSpeedInWindReductionPercent = PenaltyReductionPercent;
			}
		}

		public override Feat_BlizzardWalker GetFeat()
		{
			return FeatsManager.m_Feat_BlizzardWalker;
		}
	}
	public class BookSmartsSettings : FeatSpecificSettings<Feat_BookSmarts>
	{
		[Section("书本智慧")]
		[Name("所需研读时长")]
		[Slider(25f, 2500f, 100)]
		[Description("设置需要多少小时的研读才能解锁，默认=250")]
		public int HoursRequired = 1000;

		[Name("提升研读收益")]
		[Slider(5f, 100f, 20)]
		[Description("从每本研读完毕的书本中额外获取的收益，默认=10%")]
		public int PercentIncrease = 10;

		public override string FeatName => "BookSmarts";

		public override bool Vanilla => true;

		public BookSmartsSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_NumHoursRequired = HoursRequired;
				mFeat.m_PercentBenefit = PercentIncrease;
			}
		}

		public override Feat_BookSmarts GetFeat()
		{
			return FeatsManager.m_Feat_BookSmarts;
		}
	}
	public class CelestialNavigatorSettings : FeatSpecificSettings<Feat_CelestialNavigator>
	{
		[Section("天文导航仪")]
		[Name("提升移速")]
		[Slider(5f, 100f, 20)]
		[Description("在晴朗的夜晚或极光夜下行走速度额外提升的百分比，默认=10%")]
		public int WalkingSpeedBoostPercent = 10;

		public override string FeatName => "CelestialNavigator";

		public override bool Vanilla => true;

		public CelestialNavigatorSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_WalkingSpeedBoostPercent = WalkingSpeedBoostPercent;
			}
		}

		public override Feat_CelestialNavigator GetFeat()
		{
			return FeatsManager.m_Feat_CelestialNavigator;
		}
	}
	public class ColdFusionSettings : FeatSpecificSettings<Feat_ColdFusion>
	{
		[Section("冷聚变")]
		[Name("所需户外天数")]
		[Slider(10f, 1000f, 100)]
		[Description("设置需要在户外总共度过多少天才能解锁，默认=100")]
		public int NumDaysRequired = 100;

		[Name("温度增益")]
		[Slider(1f, 10f, 10)]
		[Description("玩家获取永久性气温奖励的数值，默认=2")]
		public int TemperatureCelsiusBenefit = 2;

		public override string FeatName => "ColdFusion";

		public override bool Vanilla => true;

		public ColdFusionSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_NumDaysRequired = NumDaysRequired;
				mFeat.m_TemperatureCelsiusBenefit = TemperatureCelsiusBenefit;
			}
		}

		public override Feat_ColdFusion GetFeat()
		{
			return FeatsManager.m_Feat_ColdFusion;
		}
	}
	public class EfficientMachineSettings : FeatSpecificSettings<Feat_EfficientMachine>
	{
		[Section("高效机械")]
		[Name("所需生存天数")]
		[Slider(10f, 1000f, 100)]
		[Description("设置需要生存多少天才能解锁，默认=500")]
		public int NumDaysRequired = 500;

		[Name("卡路里消耗减免")]
		[Slider(1f, 100f, 100)]
		[Description("耗费的卡路里减免百分比，默认=10%")]
		public int CalorieReductionBenefit = 10;

		public override string FeatName => "EfficientMachine";

		public override bool Vanilla => true;

		public EfficientMachineSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_CalorieReductionBenefit = CalorieReductionBenefit;
				mFeat.m_NumDaysRequired = NumDaysRequired;
			}
		}

		public override Feat_EfficientMachine GetFeat()
		{
			return FeatsManager.m_Feat_EfficientMachine;
		}
	}
	public class ExpertTrapperSettings : FeatSpecificSettings<Feat_ExpertTrapper>
	{
		[Section("捕兽专家")]
		[Name("捕获所需的兔子")]
		[Slider(5f, 100f, 20)]
		[Description("设置用陷阱捕捉到多少只兔子才能解锁，默认=20")]
		public int NumSnaredRabbitsRequired = 20;

		[Name("提升陷阱成功率")]
		[Slider(5f, 100f, 20)]
		[Description("提高陷阱成功率的百分比，默认=25%")]
		public int ChanceIncreaseToCatchRabbitsPercent = 25;

		public override string FeatName => "ExpertTrapper";

		public override bool Vanilla => true;

		public ExpertTrapperSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_NumSnaredRabbitsRequired = NumSnaredRabbitsRequired;
				mFeat.m_ChanceIncreaseToCatchRabbitsPercent = ChanceIncreaseToCatchRabbitsPercent;
			}
		}

		public override Feat_ExpertTrapper GetFeat()
		{
			return FeatsManager.m_Feat_ExpertTrapper;
		}
	}
	public abstract class FeatSpecificSettingsBase : JsonModSettings
	{
		protected FeatSettingsManager mManager;

		protected string mMenuName;

		public abstract string FeatName { get; }

		public abstract bool Vanilla { get; }

		public abstract Feat BaseFeat { get; }

		public FeatSpecificSettingsBase(FeatSettingsManager manager, string path, string menuName)
			: base(path)
		{
			mManager = manager;
			mMenuName = menuName;
			AddToModSettings(mMenuName);
			RefreshGUI();
		}

		protected override void OnConfirm()
		{
			try
			{
				ApplyAdjustedFeatSettings();
				base.OnConfirm();
			}
			catch (Exception value)
			{
				mManager.Log($"ERROR in FeatSpecificSettingsBase.onConfirm: {value}");
			}
		}

		public abstract void ApplyAdjustedFeatSettings();
	}
	public abstract class FeatSpecificSettings<T> : FeatSpecificSettingsBase where T : Feat
	{
		protected T? mFeat;

		public T Feat => mFeat ?? GetFeat();

		public override Feat BaseFeat => (Feat)(object)(mFeat ?? GetFeat());

		public FeatSpecificSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public virtual void Initialize(T tFeat)
		{
			mFeat = tFeat;
			ApplyAdjustedFeatSettings();
		}

		public abstract T GetFeat();
	}
	public class FireMasterSettings : FeatSpecificSettings<Feat_FireMaster>
	{
		[Section("生火大师")]
		[Name("所需生火次数")]
		[Slider(100f, 10000f, 100)]
		[Description("设置需要成功生火多少次才能解锁，默认=1000")]
		public int FiresRequired = 1000;

		[Name("提升生火技能等级")]
		[Slider(2f, 5f)]
		[Description("设定在新建游戏时的初始生火技能等级，默认=3")]
		public int StartingLevel = 3;

		public override string FeatName => "FireMaster";

		public override bool Vanilla => true;

		public FireMasterSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_NumFiresRequired = FiresRequired;
				mFeat.m_DefaultFireStartingSkillLevel = StartingLevel;
			}
		}

		public override Feat_FireMaster GetFeat()
		{
			return FeatsManager.m_Feat_FireMaster;
		}
	}
	public class FreeRunnerSettings : FeatSpecificSettings<Feat_FreeRunner>
	{
		[Section("跑酷者")]
		[Name("所需奔跑公里数")]
		[Slider(5f, 500f, 100)]
		[Description("设置需要奔跑多少公里才能解锁，默认=50")]
		public int NumKilometersRequired = 50;

		[Name("奔跑卡路里消耗减免")]
		[Slider(1f, 100f, 100)]
		[Description("降低奔跑时消耗卡路里的百分比，默认=25")]
		public int CalorieReductionBenefit = 25;

		public override string FeatName => "FreeRunner";

		public override bool Vanilla => true;

		public FreeRunnerSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_NumKilometersRequired = NumKilometersRequired;
				mFeat.m_CalorieReductionBenefit = CalorieReductionBenefit;
			}
		}

		public override Feat_FreeRunner GetFeat()
		{
			return FeatsManager.m_Feat_FreeRunner;
		}
	}
	public class MasterHunterSettings : FeatSpecificSettings<Feat_MasterHunter>
	{
		[Section("大型猫科动物杀手")]
		[Name("击杀美洲狮的数量")]
		[Slider(3f, 30f, 10)]
		[Description("设置需要在单场游戏中杀死多少头美洲狮才能解锁，模组默认=9，游戏原版默认=3")]
		public int KillCountRequirement = 9;

		[Name("听觉感知降低百分比")]
		[Slider(0f, 95f, 20)]
		[Description("AI野生动物听觉感知降低的百分比，默认=50%")]
		public int SoundDecreasePercent = 50;

		[Name("视觉感知降低百分比")]
		[Slider(0f, 95f, 20)]
		[Description("AI野生动物视觉感知降低的百分比，默认=50%")]
		public int SightDecreasePercent = 50;

		[Name("嗅觉感知降低百分比")]
		[Slider(0f, 95f, 20)]
		[Description("AI野生动物嗅觉感知降低的百分比，默认=50%")]
		public int SmellDecreasePercent = 50;

		public override string FeatName => "MasterHunter";

		public override bool Vanilla => true;

		public MasterHunterSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_AiSoundRangeScale = Math.Clamp(100 - SoundDecreasePercent, 5, 100);
				mFeat.m_AiSightRangeScale = Math.Clamp(100 - SightDecreasePercent, 5, 100);
				mFeat.m_AiSmellRangeScale = Math.Clamp(100 - SmellDecreasePercent, 5, 100);
				MaybeUnlock();
			}
		}

		public override Feat_MasterHunter GetFeat()
		{
			return FeatsManager.m_Feat_MasterHunter;
		}

		public void MaybeUnlock()
		{
			if (mManager.Data.CougarsKilled >= KillCountRequirement)
			{
				mFeat.Unlock();
			}
			else
			{
				mFeat.m_Unlocked = false;
			}
		}
	}
	public class NightWalkerSettings : FeatSpecificSettings<Feat_NightWalker>
	{
		[Section("暗夜行者")]
		[Name("日间疲劳倍率")]
		[Slider(10f, 1000f, 100)]
		[Description("设置白天的疲劳倍率，默认=200%")]
		public int DayFatigueScale = 250;

		[Name("夜间疲劳倍率")]
		[Slider(10f, 1000f, 100)]
		[Description("设置夜里的疲劳倍率，默认=50%")]
		public int NightFatigueScale = 50;

		public override string FeatName => "NightWalker";

		public override bool Vanilla => true;

		public NightWalkerSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_FatigueScaleDayMultiplier = (float)DayFatigueScale * 0.01f;
				mFeat.m_FatigueScaleNightMultiplier = (float)NightFatigueScale * 0.01f;
			}
		}

		public override Feat_NightWalker GetFeat()
		{
			return FeatsManager.m_Feat_NightWalker;
		}
	}
	public class SettledMindSettings : FeatSpecificSettings<Feat_SettledMind>
	{
		[Section("心平气和")]
		[Name("提升阅读速度")]
		[Slider(5f, 100f, 20)]
		[Description("设置提升阅读速度的百分比，默认=20")]
		public int PercentIncrease = 20;

		public override string FeatName => "SettledMind";

		public override bool Vanilla => true;

		public SettledMindSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_ReadSpeedBoostPercent = PercentIncrease;
			}
		}

		public override Feat_SettledMind GetFeat()
		{
			return FeatsManager.m_Feat_SettledMind;
		}
	}
	public class SnowWalkerSettings : FeatSpecificSettings<Feat_SnowWalker>
	{
		[Section("雪地行者")]
		[Name("所需行进的公里数")]
		[Slider(100f, 10000f, 100)]
		[Description("设置需要在游戏中行进多少公里才能解锁，默认=1000")]
		public int KilometersRequired = 1000;

		[Name("提升体力恢复速率")]
		[Slider(5f, 100f, 20)]
		[Description("加快体力恢复速度的百分比，默认=20% ")]
		public int StartingLevel = 20;

		public override string FeatName => "SnowWalker";

		public override bool Vanilla => true;

		public SnowWalkerSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_NumKilometersRequired = KilometersRequired;
				mFeat.m_StaminaRechargeFasterPercent = StartingLevel;
			}
		}

		public override Feat_SnowWalker GetFeat()
		{
			return FeatsManager.m_Feat_SnowWalker;
		}
	}
	public class StraightToTheHeartSettings : FeatSpecificSettings<Feat_StraightToHeart>
	{
		[Section("发自内心")]
		[Name("所需消耗的数量")]
		[Slider(25f, 2500f, 100)]
		[Description("需要消耗多少咖啡，能量饮料或强心针才能解锁，默认=250")]
		public int ConsumablesRequired = 250;

		[Name("延长持续时间")]
		[Slider(5f, 100f, 20)]
		[Description("延长咖啡，能量饮料和强心针效果持续时间的百分比，默认=25%")]
		public int DurationIncrease = 25;

		public override string FeatName => "StraightToHeart";

		public override bool Vanilla => true;

		public StraightToTheHeartSettings(FeatSettingsManager manager, string path, string menuName)
			: base(manager, path, menuName)
		{
		}

		public override void ApplyAdjustedFeatSettings()
		{
			if (!((Object)(object)mFeat == (Object)null))
			{
				mFeat.m_ItemConsumedCountRequired = ConsumablesRequired;
				mFeat.m_EffectiveLengthIncreasePercent = DurationIncrease;
			}
		}

		public override Feat_StraightToHeart GetFeat()
		{
			return GameManager.m_FeatsManager.m_Feat_StraightToHeart_Prefab.GetComponent<Feat_StraightToHeart>();
		}
	}
	public class FeatSettingsManager
	{
		private class Nested
		{
			internal static readonly FeatSettingsManager instance;

			static Nested()
			{
				instance = new FeatSettingsManager();
			}
		}

		[Serializable]
		public class FeatSettingsData
		{
			[Include]
			private int mCougarsKilled;

			public int CougarsKilled
			{
				get
				{
					return mCougarsKilled;
				}
				set
				{
					mCougarsKilled = value;
				}
			}
		}

		private const string ModName = "FeatSettings";

		private Settings mSettings;

		private Dictionary<Type, FeatSpecificSettingsBase> mFeatSettingsDict = new Dictionary<Type, FeatSpecificSettingsBase>();

		private Dictionary<string, FeatSpecificSettingsBase> mFeatSettingsByNameDict = new Dictionary<string, FeatSpecificSettingsBase>();

		private Instance mLogger;

		private FeatSettingsData mData = new FeatSettingsData();

		public static FeatSettingsManager Instance => Nested.instance;

		public FeatSettingsData Data => mData;

		public Settings Settings => mSettings;

		private FeatSettingsManager()
		{
		}

		public void Initialize(Instance logger)
		{
			mLogger = logger;
			InitializeSystems();
			InitializeCustomSettings();
		}

		private void InitializeCustomSettings()
		{
			AddFeatSettings(typeof(Feat_BlizzardWalker), new BlizzardWalkerSettings(this, Path.Combine("FeatSettings", "BlizzardWalkerSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_BookSmarts), new BookSmartsSettings(this, Path.Combine("FeatSettings", "BookSmartsSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_CelestialNavigator), new CelestialNavigatorSettings(this, Path.Combine("FeatSettings", "CelestialNavigatorSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_ColdFusion), new ColdFusionSettings(this, Path.Combine("FeatSettings", "ColdFusionSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_EfficientMachine), new EfficientMachineSettings(this, Path.Combine("FeatSettings", "EfficientMachineSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_ExpertTrapper), new ExpertTrapperSettings(this, Path.Combine("FeatSettings", "ExpertTrapperSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_FireMaster), new FireMasterSettings(this, Path.Combine("FeatSettings", "FireMasterSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_FreeRunner), new FreeRunnerSettings(this, Path.Combine("FeatSettings", "FreeRunnerSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_MasterHunter), new MasterHunterSettings(this, Path.Combine("FeatSettings", "MasterHunterSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_NightWalker), new NightWalkerSettings(this, Path.Combine("FeatSettings", "NightWalkerSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_SettledMind), new SettledMindSettings(this, Path.Combine("FeatSettings", "SettledMindSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_SnowWalker), new SnowWalkerSettings(this, Path.Combine("FeatSettings", "SnowWalkerSettings"), "可定制的徽章v1.4.1分页"));
			AddFeatSettings(typeof(Feat_StraightToHeart), new StraightToTheHeartSettings(this, Path.Combine("FeatSettings", "StraightToTheHeartSettings"), "可定制的徽章v1.4.1分页"));
		}

		private void AddFeatSettings(Type type, FeatSpecificSettingsBase settings)
		{
			mFeatSettingsDict.Add(type, settings);
			mFeatSettingsByNameDict.Add(settings.FeatName.ToLower(), settings);
		}

		private void InitializeSystems()
		{
			Directory.CreateDirectory(Path.Combine(MelonEnvironment.ModsDirectory, "FeatSettings"));
			mSettings = new Settings(this);
		}

		public void Deserialize(string data)
		{
			JSON.Populate(JSON.Load(data), mData);
		}

		public string Serialize()
		{
			return JSON.Dump(mData, EncodeOptions.NoTypeHints);
		}

		public void Log(string msg)
		{
			mLogger.Msg(msg);
		}

		public void RegisterFeat(GameObject featPrefab)
		{
			try
			{
				Feat val = default(Feat);
				if ((Object)(object)featPrefab == (Object)null)
				{
					Log("Null feat prefab, skipping.");
				}
				else if (!featPrefab.TryGetComponent<Feat>(ref val))
				{
					Log("No feat found, skipping.");
				}
				else if ((Object)(object)val == (Object)null)
				{
					Log("Somehow feat is null, skipping.");
				}
				else if (!TryRegisterFeat(val))
				{
					Log("Couldnt register feat " + ((Object)val).name + "!");
				}
			}
			catch (Exception value)
			{
				Log($"ERROR in FeatSettingsManager.RegisterFeat: {value}");
			}
		}

		private bool TryRegisterFeat(Feat feat)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected I4, but got Unknown
			FeatType featType = feat.m_FeatType;
			switch ((int)featType)
			{
			case 8:
				return TryRegisterFeat<Feat_BlizzardWalker>(feat);
			case 0:
				return TryRegisterFeat<Feat_BookSmarts>(feat);
			case 12:
				return TryRegisterFeat<Feat_CelestialNavigator>(feat);
			case 1:
				return TryRegisterFeat<Feat_ColdFusion>(feat);
			case 2:
				return TryRegisterFeat<Feat_EfficientMachine>(feat);
			case 6:
				return TryRegisterFeat<Feat_ExpertTrapper>(feat);
			case 3:
				return TryRegisterFeat<Feat_FireMaster>(feat);
			case 4:
				return TryRegisterFeat<Feat_FreeRunner>(feat);
			case 10:
				return TryRegisterFeat<Feat_MasterHunter>(feat);
			case 9:
				return TryRegisterFeat<Feat_NightWalker>(feat);
			case 11:
				return TryRegisterFeat<Feat_SettledMind>(feat);
			case 5:
				return TryRegisterFeat<Feat_SnowWalker>(feat);
			case 7:
				return TryRegisterFeat<Feat_StraightToHeart>(feat);
			default:
				Log($"Unknown feat {((Object)feat).name} of type {((object)feat).GetType()}!");
				return false;
			}
		}

		private bool TryRegisterFeat<T>(Feat feat) where T : Feat
		{
			T tFeat = default(T);
			if (!((Component)feat).gameObject.TryGetComponent<T>(ref tFeat))
			{
				Log("ERROR in FeatSettingsManager.TryRegisterFeat: could not get specific feat class from gameobject!");
				return false;
			}
			if (!mFeatSettingsDict.TryGetValue(typeof(T), out FeatSpecificSettingsBase value))
			{
				Log("ERROR during FeatSettingsManager.TryRegisterFeat: Could not fetch settings for type " + typeof(T).Name + "!");
				return false;
			}
			if (!(value is FeatSpecificSettings<T> featSpecificSettings))
			{
				Log("ERROR during FeatSettingsManager.TryRegisterFeat: Could not convert feat settings to correct class!");
				return false;
			}
			featSpecificSettings.Initialize(tFeat);
			return true;
		}

		public bool TryGetFeatSpecificSettings<T>(out FeatSpecificSettings<T>? settings) where T : Feat
		{
			settings = null;
			if (!mFeatSettingsDict.TryGetValue(typeof(T), out FeatSpecificSettingsBase value))
			{
				return false;
			}
			if (!(value is FeatSpecificSettings<T> featSpecificSettings))
			{
				return false;
			}
			settings = featSpecificSettings;
			return true;
		}

		public bool TryGetFeatSpecificSettingsByName(string featName, out FeatSpecificSettingsBase settings)
		{
			return mFeatSettingsByNameDict.TryGetValue(featName.ToLower(), out settings);
		}

		public void Console_SetCougarKills()
		{
			string @string = uConsole.GetString();
			if (@string == null || @string.Length == 0)
			{
				Log("Enter kill quantity!");
				return;
			}
			if (!int.TryParse(@string, out var result))
			{
				Log("Enter kill quantity as integer!");
				return;
			}
			mData.CougarsKilled = result;
			if (this.TryGetFeatSpecificSettings<Feat_MasterHunter>(out FeatSpecificSettings<Feat_MasterHunter> settings) && settings is MasterHunterSettings masterHunterSettings)
			{
				masterHunterSettings.MaybeUnlock();
			}
		}

		public void Console_EnableFeat()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			string @string = uConsole.GetString();
			FeatSpecificSettingsBase settings;
			if (@string == null || @string.Length == 0)
			{
				Log("Enter feat name!");
			}
			else if (!TryGetFeatSpecificSettingsByName(@string, out settings))
			{
				Log("Invalid feat name!");
			}
			else if (!settings.BaseFeat.IsUnlocked())
			{
				Log("You must unlock this feat first!");
			}
			else if (settings.Vanilla)
			{
				if (!FeatEnabledTracker.m_FeatsEnabledThisSandbox.Contains(settings.BaseFeat.m_FeatType))
				{
					FeatEnabledTracker.m_FeatsEnabledThisSandbox.Add(settings.BaseFeat.m_FeatType);
				}
			}
			else
			{
				Log("Non vanilla feats WIP!");
			}
		}

		public void Console_DisableFeat()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			string @string = uConsole.GetString();
			FeatSpecificSettingsBase settings;
			if (@string == null || @string.Length == 0)
			{
				Log("Enter feat name!");
			}
			else if (!TryGetFeatSpecificSettingsByName(@string, out settings))
			{
				Log("Invalid feat name!");
			}
			else if (!settings.BaseFeat.IsUnlocked())
			{
				Log("You must unlock this feat first!");
			}
			else if (settings.Vanilla)
			{
				if (FeatEnabledTracker.m_FeatsEnabledThisSandbox.Contains(settings.BaseFeat.m_FeatType))
				{
					FeatEnabledTracker.m_FeatsEnabledThisSandbox.Remove(settings.BaseFeat.m_FeatType);
				}
			}
			else
			{
				Log("Non vanilla feats WIP!");
			}
		}

		public bool TryRegisterFeatSettings(Type type, FeatSpecificSettingsBase featSettings)
		{
			if (!mFeatSettingsDict.TryAdd(type, featSettings))
			{
				Log($"Error in FeatSettingsManager.RegisterFeat: Could not add new feat type {type.Name}'s featsettings {featSettings.GetType()} as it is already added with custom settings!");
				return false;
			}
			Log($"FeatSettingsMaanger.RegisterFeat: Registered feat type {type.Name}'s featsettings {featSettings.GetType()}!");
			return true;
		}
	}
	public class Main : MelonMod
	{
		public override void OnInitializeMelon()
		{
			((MelonBase)this).LoggerInstance.Msg(Initialize() ? "Initialized Successfully!" : "Initialization Errors!");
		}

		public override void OnDeinitializeMelon()
		{
			((MelonBase)this).LoggerInstance.Msg(Shutdown() ? "Shutdown Successfully!" : "Shutdown Errors!");
		}

		protected bool Initialize()
		{
			FeatSettingsManager.Instance?.Initialize(((MelonBase)this).LoggerInstance);
			return FeatSettingsManager.Instance != null;
		}

		protected bool Shutdown()
		{
			return true;
		}
	}
	internal class Patches
	{
		[HarmonyPatch(typeof(FeatsManager), "Deserialize", new Type[] { typeof(string) })]
		private static class FeatsManagerPatches_Deserialize
		{
			private static void Prefix(string text, FeatsManager __instance)
			{
				if (Utils.TryGetField(text, "FeatSettingsData", out string value))
				{
					MelonLogger.Msg(value);
					FeatSettingsManager.Instance.Deserialize(value);
				}
			}
		}

		[HarmonyPatch(typeof(FeatsManager), "Serialize")]
		private static class FeatsManagerPatches_Serialize
		{
			private static void Postfix(ref string __result)
			{
				__result = Utils.AddOrUpdateField(__result, "FeatSettingsData", FeatSettingsManager.Instance.Serialize());
			}
		}

		[HarmonyPatch(typeof(FeatsManager), "InstantiateFeatPrefab", new Type[] { typeof(GameObject) })]
		private static class FeatsManagerPatches_InstantiateFeatPrefab
		{
			private static void Postfix(GameObject __result)
			{
				FeatSettingsManager.Instance.RegisterFeat(__result);
			}
		}

		[HarmonyPatch(typeof(BaseAi), "EnterDead")]
		private static class BaseAiPatches_EnterDead
		{
			private static void Prefix(BaseAi __instance)
			{
				if (!((Object)(object)__instance == (Object)null) && !((Object)(object)__instance.Cougar == (Object)null) && !__instance.m_ForceToCorpse && FeatSettingsManager.Instance.TryGetFeatSpecificSettings<Feat_MasterHunter>(out FeatSpecificSettings<Feat_MasterHunter> settings) && settings is MasterHunterSettings masterHunterSettings)
				{
					int cougarsKilled = FeatSettingsManager.Instance.Data.CougarsKilled;
					int killCountRequirement = masterHunterSettings.KillCountRequirement;
					FeatSettingsManager.Instance.Log($"Cougar killed, increasing Big Cat Killer progress! Current: {Math.Min(cougarsKilled, killCountRequirement)} of {killCountRequirement} | after: {Math.Min(cougarsKilled + 1, killCountRequirement)} of {killCountRequirement}");
					FeatSettingsManager.Instance.Data.CougarsKilled++;
					masterHunterSettings.MaybeUnlock();
					SaveGameSystem.SaveProfile();
				}
			}
		}

		[HarmonyPatch(typeof(Feat), "TryToDisplayKicker")]
		private static class FeatPatches_TryToDisplayKicker
		{
			private static bool Prefix()
			{
				FeatSettingsManager.Instance.Log("Attempting to stop kicker!");
				return !SceneUtilities.IsSceneMenu(SceneUtilities.GetActiveSceneName());
			}
		}

		[HarmonyPatch(typeof(FeatNotify), "ShowFeatUnlockedKicker", new Type[]
		{
			typeof(AssetReferenceTexture2D),
			typeof(string)
		})]
		private static class FeatNotifyPatches_ShowFeatUnlockedKicker
		{
			private static bool Prefix(AssetReferenceTexture2D textureReference, string footer)
			{
				FeatSettingsManager.Instance.Log("Attempting to stop kicker display!");
				return !SceneUtilities.IsSceneMenu(SceneUtilities.GetActiveSceneName());
			}
		}

		[HarmonyPatch(typeof(ConsoleManager), "Initialize")]
		internal class ConsoleManagerPatches_Initialize
		{
			private static void Postfix()
			{
				uConsole.RegisterCommand("SetCougarKills", DebugCommand.op_Implicit((Action)delegate
				{
					FeatSettingsManager.Instance.Console_SetCougarKills();
				}));
				uConsole.RegisterCommand("EnableFeat", DebugCommand.op_Implicit((Action)delegate
				{
					FeatSettingsManager.Instance.Console_EnableFeat();
				}));
				uConsole.RegisterCommand("DisableFeat", DebugCommand.op_Implicit((Action)delegate
				{
					FeatSettingsManager.Instance.Console_DisableFeat();
				}));
			}
		}
	}
	[HarmonyPatch(typeof(Panel_MainMenu), "GetNumFeatsForXPMode")]
	internal class Panel_MainMenuPatches_GetNumFeatsForXPMode
	{
		private static void Postfix(ref int __result, Panel_MainMenu __instance)
		{
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Invalid comparison between Unknown and I4
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Invalid comparison between Unknown and I4
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Expected I4, but got Unknown
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Invalid comparison between Unknown and I4
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Invalid comparison between Unknown and I4
			FeatSettingsManager instance = FeatSettingsManager.Instance;
			if (__instance.m_ActiveFeatObjects == null || ((Il2CppArrayBase<GameObject>)(object)__instance.m_ActiveFeatObjects).Count == 0)
			{
				return;
			}
			if (((Il2CppArrayBase<GameObject>)(object)__instance.m_ActiveFeatObjects).Count < 9)
			{
				instance.Log($"active feat count: {((Il2CppArrayBase<GameObject>)(object)__instance.m_ActiveFeatObjects).Count}");
				GameObject[] array = (GameObject[])(object)new GameObject[9];
				int i = 0;
				for (int count = ((Il2CppArrayBase<GameObject>)(object)__instance.m_ActiveFeatObjects).Count; i < count; i++)
				{
					instance.Log($"copying object {i}");
					array[i] = ((Il2CppArrayBase<GameObject>)(object)__instance.m_ActiveFeatObjects)[i];
					instance.Log($"copied object {i}");
				}
				int j = ((Il2CppArrayBase<GameObject>)(object)__instance.m_ActiveFeatObjects).Count;
				for (int num = 9; j < num; j++)
				{
					instance.Log($"instantiating object {j}");
					array[j] = Object.Instantiate<GameObject>(array[j - 1], array[j - 1].transform.parent);
					instance.Log($"instantiated object {j}");
				}
				__instance.m_ActiveFeatObjects = Il2CppReferenceArray<GameObject>.op_Implicit(array);
			}
			GameModeConfig s_CurrentGameMode = ExperienceModeManager.s_CurrentGameMode;
			if ((Object)(object)s_CurrentGameMode == (Object)null)
			{
				__result = 0;
				return;
			}
			ExperienceMode xPMode = s_CurrentGameMode.m_XPMode;
			if ((Object)(object)xPMode == (Object)null)
			{
				__result = 0;
				return;
			}
			ExperienceModeType modeType = xPMode.m_ModeType;
			if ((int)modeType <= 9)
			{
				switch ((int)modeType)
				{
				case 0:
					__result = instance.Settings.PilgrimFeatCount;
					return;
				case 1:
					__result = instance.Settings.VoyagerFeatCount;
					return;
				case 2:
					__result = instance.Settings.StalkerFeatCount;
					return;
				}
				if ((int)modeType == 9)
				{
					__result = instance.Settings.InterloperFeatCount;
					return;
				}
			}
			else
			{
				if ((int)modeType == 10)
				{
					__result = instance.Settings.CustomFeatCount;
					return;
				}
				if ((int)modeType == 18)
				{
					__result = instance.Settings.MiseryFeatCount;
					return;
				}
			}
			__result = 0;
		}
	}
	[HarmonyPatch(typeof(FeatEnabledTracker), "Deserialize")]
	internal class FeatEnabledTrackerPatches_Deserialize
	{
		private static void Prefix(string text)
		{
			FeatSettingsManager.Instance.Log(text);
		}
	}
	[HarmonyPatch(typeof(FeatEnabledTracker), "Serialize")]
	internal class FeatEnabledTrackerPatches_Serialize
	{
		private static void Postfix(ref string __result)
		{
			FeatSettingsManager.Instance.Log(__result);
		}
	}
	public class Settings : JsonModSettings
	{
		protected FeatSettingsManager mManager;

		[Section("编辑徽章数量")]
		[Name("朝圣者模式")]
		[Slider(1f, 6f, 6)]
		public int PilgrimFeatCount = 5;

		[Name("航行者模式")]
		[Slider(1f, 6f, 6)]
		public int VoyagerFeatCount = 4;

		[Name("潜行者模式")]
		[Slider(1f, 6f, 6)]
		public int StalkerFeatCount = 3;

		[Name("入侵者模式")]
		[Slider(1f, 6f, 6)]
		public int InterloperFeatCount = 2;

		[Name("厄难模式")]
		[Slider(0f, 6f, 7)]
		public int MiseryFeatCount;

		[Name("自定义模式")]
		[Slider(1f, 6f, 6)]
		public int CustomFeatCount = 5;

		public Settings(FeatSettingsManager manager)
			: base(Path.Combine("FeatSettings", "FeatSettings"))
		{
			mManager = manager;
			Initialize();
		}

		protected void Initialize()
		{
			AddToModSettings("可定制的徽章v1.4.1");
			RefreshGUI();
		}
	}
	public static class Utils
	{
		public static string AddOrUpdateField(string json, string key, string value, bool quoteValue = true)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				json = "{}";
			}
			json = json.Trim();
			if (json.EndsWith("}"))
			{
				json = json.Substring(0, json.Length - 1);
			}
			string value2 = "\"" + key + "\"";
			int num = json.IndexOf(value2, StringComparison.Ordinal);
			string text = (quoteValue ? ("\"" + EscapeJson(value) + "\"") : value);
			if (num >= 0)
			{
				int num2 = json.IndexOf(':', num);
				if (num2 >= 0)
				{
					int num3 = json.IndexOf(',', num2);
					if (num3 == -1)
					{
						num3 = json.Length;
					}
					string text2 = json.Substring(0, num2 + 1);
					string text3 = json.Substring(num3);
					json = text2 + " " + text + text3;
				}
			}
			else
			{
				if (json.Length > 1)
				{
					json += ",";
				}
				json = json + "\"" + key + "\":" + text;
			}
			return json + "}";
		}

		public static bool TryGetField(string json, string key, out string value)
		{
			value = "";
			if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
			{
				return false;
			}
			json = json.Trim();
			string value2 = "\"" + key + "\"";
			int num = json.IndexOf(value2, StringComparison.Ordinal);
			if (num == -1)
			{
				return false;
			}
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				if (json[i] == '"' && (i == 0 || json[i - 1] != '\\'))
				{
					num2++;
				}
			}
			if (num2 % 2 != 0)
			{
				return false;
			}
			int num3 = json.IndexOf(':', num);
			if (num3 == -1)
			{
				return false;
			}
			int num4 = num3 + 1;
			int num5 = FindValueEnd(json, num4);
			string text = json.Substring(num4, num5 - num4).Trim();
			if (text.StartsWith("\"") && text.EndsWith("\""))
			{
				text = text.Substring(1, text.Length - 2);
				text = text.Replace("\\\"", "\"").Replace("\\\\", "\\");
			}
			value = text;
			return true;
		}

		private static int FindValueEnd(string json, int startIdx)
		{
			bool flag = false;
			for (int i = startIdx; i < json.Length; i++)
			{
				char c = json[i];
				if (c == '"' && (i == 0 || json[i - 1] != '\\'))
				{
					flag = !flag;
				}
				if (!flag && (c == ',' || c == '}'))
				{
					return i;
				}
			}
			return json.Length;
		}

		private static string EscapeJson(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return s;
			}
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
	}
}
