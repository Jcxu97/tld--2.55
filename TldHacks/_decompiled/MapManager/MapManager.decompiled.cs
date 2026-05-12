using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using ComplexLogger;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using MapManager;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("MapManager")]
[assembly: AssemblyDescription("Adds settings to better manage the map")]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("MapManager")]
[assembly: AssemblyCopyright("2023-2024")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.1.7")]
[assembly: MelonInfo(typeof(MapManager.Main), "MapManager", "1.1.7", "The Illusion", null)]
[assembly: VerifyLoaderVersion("0.6.1", true)]
[assembly: MelonPriority(0)]
[assembly: MelonIncompatibleAssemblies(new string[] { "MapTweaks" })]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.1.7.0")]
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
namespace MapManager
{
	public static class BuildInfo
	{
		public const string Name = "MapManager";

		public const string Author = "The Illusion";

		public const string Version = "1.1.7";

		public const string GUIName = "Map Manager";

		public const string MelonLoaderVersion = "0.6.1";

		public const string Description = "Adds settings to better manage the map";

		public const string Company = null;

		public const string DownloadLink = null;

		public const string Copyright = "2023-2024";

		public const string Trademark = null;

		public const string Product = "MapManager";

		public const string Culture = null;

		public const int Priority = 0;
	}
	internal class Main : MelonMod
	{
		public static ComplexLogger<Main> Logger = new ComplexLogger<Main>();

		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
		}

		public override void OnUpdate()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			if (GameManager.IsMainMenuActive() || !((Object)(object)InputManager.instance != (Object)null))
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.Instance.RevealMap))
			{
				InterfaceManager.GetPanel<Panel_Map>().RevealCurrentScene();
			}
			if ((int)Settings.Instance.ShowBunkerKey == 0 || !InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.Instance.ShowBunkerKey))
			{
				return;
			}
			Panel_Map panel = InterfaceManager.GetPanel<Panel_Map>();
			if (!((Object)(object)panel != (Object)null))
			{
				return;
			}
			foreach (Vector3 item in BunkerData.GetBunkersForScene(panel.GetMapNameOfCurrentScene()))
			{
				panel.DoNearbyDetailsCheck(15f, true, true, item, false);
				Logger.Log($"Force revealed bunker at {item}", FlaggedLoggingLevel.Debug, "OnUpdate");
			}
		}

		public static AssetBundle LoadAssetBundle(string name)
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
			MemoryStream memoryStream = new MemoryStream((int)stream.Length);
			stream.CopyTo(memoryStream);
			return AssetBundle.LoadFromMemory(Il2CppStructArray<byte>.op_Implicit(memoryStream.ToArray()));
		}
	}
	public static class Extensions
	{
		public static T DontUnload<T>(this T obj) where T : Object
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			((Object)obj).hideFlags = (HideFlags)(((Object)obj).hideFlags | 0x20);
			return obj;
		}
	}
	[HarmonyPatch(typeof(CharcoalItem), "Awake")]
	internal class CharcoalItem_Awake
	{
		private static void Postfix(CharcoalItem __instance)
		{
			__instance.m_SurveyGameMinutes *= Settings.Instance.MapSurveyMultTime;
		}
	}
	[HarmonyPatch(typeof(Panel_Map), "DoNearbyDetailsCheck", new Type[]
	{
		typeof(float),
		typeof(bool),
		typeof(bool),
		typeof(Vector3),
		typeof(bool)
	})]
	internal class Panel_Map_DoNearbyDetailsCheck
	{
		private static void Prefix(Panel_Map __instance, ref float radius, ref bool forceAddSurveyPosition, ref bool useOverridePosition, ref Vector3 overridePostion, ref bool shouldAllowVistaReveals)
		{
			if (__instance.SceneCanBeMapped(__instance.GetMapNameOfCurrentScene()) && !((Panel_Base)InterfaceManager.GetPanel<Panel_Loading>()).IsEnabled())
			{
				radius *= Settings.Instance.MapSurveyMult;
				shouldAllowVistaReveals = Settings.Instance.RevealVista;
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Map), "Enable", new Type[]
	{
		typeof(bool),
		typeof(bool)
	})]
	internal class Panel_Map_Initialize
	{
		internal static ResetOpts resetOpts = (ResetOpts)1;

		private static void Postfix(Panel_Map __instance, ref bool enable, ref bool cameFromDetailSurvey)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (Settings.Instance.EnableArrow)
			{
				resetOpts = (ResetOpts)(resetOpts | 4);
			}
			if (Settings.Instance.CenterOnPlayer)
			{
				resetOpts = (ResetOpts)(resetOpts | 2);
			}
			__instance.ResetToNormal(resetOpts);
			if (enable && Settings.Instance.EnableArrow)
			{
				MelonCoroutines.Start(DelayedSetColor(__instance));
			}
		}

		private static IEnumerator DelayedSetColor(Panel_Map mapPanel)
		{
			yield return null;
			Transform playerIcon = mapPanel.m_PlayerIcon;
			if ((Object)(object)playerIcon == (Object)null)
			{
				Main.Logger.Log("m_PlayerIcon is null.", FlaggedLoggingLevel.Warning, "DelayedSetColor");
			}
			else
			{
				SetColor(playerIcon);
			}
		}

		private static void SetColor(Transform playerIcon)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			bool flag = false;
			Image component = ((Component)playerIcon).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				((Graphic)component).color = Color.red;
				Main.Logger.Log("Color set via Image", FlaggedLoggingLevel.Debug, "SetColor");
				flag = true;
			}
			SpriteRenderer component2 = ((Component)playerIcon).GetComponent<SpriteRenderer>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.color = Color.red;
				Main.Logger.Log("Color set via SpriteRenderer", FlaggedLoggingLevel.Debug, "SetColor");
				flag = true;
			}
			UISprite component3 = ((Component)playerIcon).GetComponent<UISprite>();
			if ((Object)(object)component3 != (Object)null)
			{
				((UIWidget)component3).color = Color.red;
				Main.Logger.Log("Color set via UISprite", FlaggedLoggingLevel.Debug, "SetColor");
				flag = true;
			}
			UITexture component4 = ((Component)playerIcon).GetComponent<UITexture>();
			if ((Object)(object)component4 != (Object)null)
			{
				((UIWidget)component4).color = Color.red;
				Main.Logger.Log("Color set via UITexture", FlaggedLoggingLevel.Debug, "SetColor");
				flag = true;
			}
			if (!flag)
			{
				Main.Logger.Log("No known colorizable component found.", FlaggedLoggingLevel.Warning, "SetColor");
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Map), "RevealOnPolaroidDiscovery", new Type[]
	{
		typeof(string),
		typeof(bool)
	})]
	internal class Panel_Map_RevealOnPolaroidDiscovery
	{
		private static bool Prefix(ref string polaroidGearItemName, ref bool showOnMap)
		{
			Main.Logger.Log($"polaroidGearItemName: {polaroidGearItemName}. showOnMap: {showOnMap}", FlaggedLoggingLevel.Debug, "Prefix");
			return !Settings.Instance.MapWithPolariods;
		}
	}
	[HarmonyPatch(typeof(VistaLocation), "HasRequiredGearItem")]
	internal class VistaLocation_HasRequiredGearItem
	{
		private static void Postfix(VistaLocation __instance, ref bool __result)
		{
			if (Settings.Instance.MapWithPolariods && !__result)
			{
				Main.Logger.Log("Add: " + __instance.m_RequiredGearItem.GetDisplayNameWithoutConditionForInventoryInterfaces(), FlaggedLoggingLevel.Debug, "Postfix");
				GameManager.GetPlayerManagerComponent().AddItemToPlayerInventory(__instance.m_RequiredGearItem, true, true);
				__result = true;
			}
			if (!__result)
			{
				Main.Logger.Log(__instance.m_LocationName.Text() + ": Result is false", FlaggedLoggingLevel.Verbose, "Postfix");
			}
		}

		private static void AddPolaroid(VistaLocation vistaLocation)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)vistaLocation))
			{
				MapDetail component = ((Component)vistaLocation).GetComponent<MapDetail>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.m_RequiresInteraction = false;
				}
				Vector3 position = ((Component)vistaLocation).transform.position;
				float detailSurvayPolaroidRadiusMeters = InterfaceManager.GetPanel<Panel_Map>().m_DetailSurvayPolaroidRadiusMeters;
				InterfaceManager.GetPanel<Panel_Map>().DoNearbyDetailsCheck(detailSurvayPolaroidRadiusMeters, false, true, position, true);
				InterfaceManager.GetPanel<Panel_Map>().Enable(true, true);
				InterfaceManager.GetPanel<Panel_Map>().CenterOnPoint(position);
			}
		}
	}
	internal class Settings : JsonModSettings
	{
		[Section("主要功能")]
		[Name("启用玩家箭头图标")]
		[Description("如果启用，将用红色箭头显示玩家在地图上的位置")]
		public bool EnableArrow;

		[Name("以玩家为中心")]
		[Description("如果启用，将以玩家在地图上的位置为中心。按M看地图时你都会第一时间看到自己的箭头图标")]
		public bool CenterOnPlayer;

		[Name("地图全开")]
		[Description("设置热键即可实现一键地图全开，注意：全开后无法恢复默认，需配合控制台代码survey_clear")]
		public KeyCode RevealMap = (KeyCode)267;

		[Section("绘制地图")]
		[Name("调查范围倍数")]
		[Slider(-0.99f, 25f, NumberFormat = "{0:F2}")]
		public float MapSurveyMult = 1f;

		[Name("绘制地图时间")]
		[Description("调查所需时间")]
		[Slider(0.1f, 5f, NumberFormat = "{0:F2}")]
		public float MapSurveyMultTime = 1f;

		[Name("宝丽来照片调查功能(与显示远景点必须同时开启)")]
		[Description("在没有宝丽来照片的情况下，远景调查点使用木炭开图和宝丽来照片远景调查效果相同。调查后宝丽来照片会自动记录在收藏品栏，该宝丽来照片刷点从此不在刷新")]
		public bool MapWithPolariods;

		[Name("显示远景点位置(与宝丽来照片功能必须同时开启)")]
		[Description("显示范围内的所有视景点位置，设置后必须重新加载或切换场景才能生效")]
		public bool RevealVista;

		[Name("解锁调查限制")]
		[Description("可在任意时间调查地图，比如夜幕降临时依旧可以调查。设置后必须重新加载或切换场景才能生效")]
		public bool UnlockSurvey;

		[Name("猎杀动物的死尸会显示在地图上(包括流血而死)")]
		[Description("设置后必须重新加载或切换场景才能生效")]
		public bool AddCorpseToMap;

		[Section("地堡显示-by:zhaochun8787")]
		[Name("显示地堡热键")]
		[Description("在任何难度模式按下此热键，都会在该区域的指定坐标下显示地堡图标。支持自定义模式")]
		public KeyCode ShowBunkerKey = (KeyCode)289;

		[Section("清除植物地图标记-by:hzb1130")]
		[Name("自动清除已采集的植物图标")]
		[Description("采集完植物后，地图上的对应图标会自动消失")]
		public bool AutoRemoveHarvestedMarkers = true;

		[Section("入侵/厄难难度地堡修复-by:zhaochun8787")]
		[Name("启用地堡修复")]
		[Description("在入侵者和厄难难度下，强制让每个地图至少生成一个地堡，但绝大多数地堡为空地堡")]
		public bool EnableHighDifficultyBunkerFix = true;

		[Name("入侵/厄难难度地堡数量")]
		[Description("在入侵者和厄难难度下，每个地图生成的地堡数量（仅当上方启用时生效）。注意：每个地图最多只能有1个地堡，因此此选项最大值为1。")]
		[Slider(0f, 1f, NumberFormat = "{0:F0}")]
		public int HighDifficultyBunkerCount = 1;

		internal static Settings Instance { get; }

		internal static void OnLoad()
		{
			Instance.AddToModSettings("地图管理v1.1.7");
			Instance.RefreshGUI();
		}

		static Settings()
		{
			Instance = new Settings();
		}
	}
}
namespace MapManager.Patches
{
	[HarmonyPatch(typeof(BaseAi), "EnterDead")]
	public class BaseAI_EnterDead
	{
		public static void Postfix(BaseAi __instance)
		{
			if (!((Object)(object)__instance == (Object)null) && Settings.Instance.AddCorpseToMap)
			{
				MapDetail component = ((Component)__instance).gameObject.GetComponent<MapDetail>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.ShowOnMap(true);
				}
			}
		}
	}
	[HarmonyPatch(typeof(CharcoalItem), "HasSurveyVisibility", new Type[] { typeof(float) })]
	public class CharcoalItem_HasSurveyVisibility
	{
		public static bool Prefix(ref bool __result)
		{
			__result = Settings.Instance.UnlockSurvey;
			return !Settings.Instance.UnlockSurvey;
		}
	}
}
namespace MapManager
{
	public class BunkerLocation
	{
		public string RegionName { get; set; }

		public Vector3 Position { get; set; }

		public bool IsFixedBunker { get; set; }
	}
	public static class BunkerData
	{
		public static readonly List<BunkerLocation__0> FixedBunkers;

		public static readonly List<Vector3> MysteryLakeBunkerSpots;

		public static List<Vector3> GetBunkersForScene(string sceneName)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			List<Vector3> list = new List<Vector3>();
			foreach (BunkerLocation__0 fixedBunker in FixedBunkers)
			{
				if (fixedBunker.RegionName == sceneName)
				{
					list.Add(fixedBunker.Position);
				}
			}
			return list;
		}

		static BunkerData()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			FixedBunkers = new List<BunkerLocation__0>
			{
				new BunkerLocation__0
				{
					RegionName = "MarshRegion",
					Position = new Vector3(592.06f, -84.87f, -106.82f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "BlackrockRegion",
					Position = new Vector3(706.7f, 373.11f, 816.39f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "CanneryRegion",
					Position = new Vector3(328.35f, 343.25f, 835.17f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "AshCanyon",
					Position = new Vector3(-42.28f, 171.5f, -794.88f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "RiverValleyRegion",
					Position = new Vector3(362.06f, 237.53f, 376.08f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "TimberwolfMountain",
					Position = new Vector3(1673.5f, 205.75f, 967.92f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "MountainTownRegion",
					Position = new Vector3(1829.84f, 443.03f, 1770.76f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "LakeRegion",
					Position = new Vector3(1029.65f, 90.51f, -50.71f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "RuralRegion",
					Position = new Vector3(335.83f, 185.33f, 2257.27f),
					IsFixedBunker = true
				},
				new BunkerLocation__0
				{
					RegionName = "RuralRegion",
					Position = new Vector3(422.32f, 176.3f, 1457.14f),
					IsFixedBunker = true
				}
			};
			MysteryLakeBunkerSpots = new List<Vector3>();
		}
	}
	public class BunkerLocation__0
	{
		public string RegionName { get; set; }

		public Vector3 Position { get; set; }

		public bool IsFixedBunker { get; set; }
	}
	[HarmonyPatch(typeof(MapDetail), "Surveyed")]
	internal static class MapDetail_Surveyed_Patch
	{
		private static void Postfix(MapDetail __instance)
		{
			if (!((Object)(object)__instance == (Object)null) && Settings.Instance.AutoRemoveHarvestedMarkers && __instance.m_IsSurveyed && AreAllHarvestablesHarvested(__instance))
			{
				Panel_Map panel = InterfaceManager.GetPanel<Panel_Map>();
				if ((Object)(object)panel != (Object)null)
				{
					panel.RemoveMapDetailFromMap(__instance, 0.5f);
					panel.RefreshIconVisibility();
				}
				else
				{
					__instance.ShowOnMap(false);
				}
			}
		}

		private static bool AreAllHarvestablesHarvested(MapDetail mapDetail)
		{
			Il2CppReferenceArray<Harvestable> harvestablesForMapVisibility = mapDetail.m_HarvestablesForMapVisibility;
			if (harvestablesForMapVisibility != null && ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility).Length > 0)
			{
				bool flag = true;
				bool flag2 = false;
				for (int i = 0; i < ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility).Length; i++)
				{
					Harvestable val = ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility)[i];
					if ((Object)(object)val != (Object)null)
					{
						flag2 = true;
						if (!val.m_Harvested && (Object)(object)((Component)val).gameObject != (Object)null && ((Component)val).gameObject.activeInHierarchy)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag2 && flag)
				{
					return true;
				}
			}
			List<Harvestable> harvestablesSharingIcon = mapDetail.m_HarvestablesSharingIcon;
			if (harvestablesSharingIcon != null && harvestablesSharingIcon.Count > 0)
			{
				bool flag3 = true;
				bool flag4 = false;
				for (int j = 0; j < harvestablesSharingIcon.Count; j++)
				{
					Harvestable val2 = harvestablesSharingIcon[j];
					if ((Object)(object)val2 != (Object)null)
					{
						flag4 = true;
						if (!val2.m_Harvested && (Object)(object)((Component)val2).gameObject != (Object)null && ((Component)val2).gameObject.activeInHierarchy)
						{
							flag3 = false;
							break;
						}
					}
				}
				if (flag4 && flag3)
				{
					return true;
				}
			}
			return false;
		}
	}
	[HarmonyPatch(typeof(MapDetail), "Unlock", new Type[] { typeof(bool) })]
	internal static class MapDetail_Unlock_Patch
	{
		private static void Postfix(MapDetail __instance, bool ignoreLogged)
		{
			if (!((Object)(object)__instance == (Object)null) && Settings.Instance.AutoRemoveHarvestedMarkers && __instance.m_IsUnlocked && AreAllHarvestablesHarvested(__instance))
			{
				Panel_Map panel = InterfaceManager.GetPanel<Panel_Map>();
				if ((Object)(object)panel != (Object)null)
				{
					panel.RemoveMapDetailFromMap(__instance, 0.5f);
					panel.RefreshIconVisibility();
				}
			}
		}

		private static bool AreAllHarvestablesHarvested(MapDetail mapDetail)
		{
			Il2CppReferenceArray<Harvestable> harvestablesForMapVisibility = mapDetail.m_HarvestablesForMapVisibility;
			if (harvestablesForMapVisibility != null && ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility).Length > 0)
			{
				bool flag = true;
				bool flag2 = false;
				for (int i = 0; i < ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility).Length; i++)
				{
					Harvestable val = ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility)[i];
					if ((Object)(object)val != (Object)null)
					{
						flag2 = true;
						if (!val.m_Harvested && (Object)(object)((Component)val).gameObject != (Object)null && ((Component)val).gameObject.activeInHierarchy)
						{
							flag = false;
							break;
						}
					}
				}
				return flag2 && flag;
			}
			return false;
		}
	}
	[HarmonyPatch(typeof(Panel_Map), "Update")]
	internal static class Panel_Map_Update_Cleanup_Patch
	{
		private static float lastCleanup;

		private const float CLEANUP_INTERVAL = 60f;

		private static void Postfix(Panel_Map __instance)
		{
			if (!GameManager.IsMainMenuActive() && Settings.Instance.AutoRemoveHarvestedMarkers)
			{
				float time = Time.time;
				if (time - lastCleanup >= 60f)
				{
					lastCleanup = time;
					CleanupStaleMarkers(__instance);
				}
			}
		}

		private static void CleanupStaleMarkers(Panel_Map mapPanel)
		{
			List<MapDetail> s_MapDetails = MapDetailManager.s_MapDetails;
			if (s_MapDetails == null)
			{
				return;
			}
			List<MapDetail> val = new List<MapDetail>();
			Enumerator<MapDetail> enumerator = s_MapDetails.GetEnumerator();
			while (enumerator.MoveNext())
			{
				MapDetail current = enumerator.Current;
				if ((Object)(object)current != (Object)null && current.m_IsSurveyed && AreAllHarvestablesHarvested(current))
				{
					val.Add(current);
				}
			}
			enumerator = val.GetEnumerator();
			while (enumerator.MoveNext())
			{
				MapDetail current2 = enumerator.Current;
				mapPanel.RemoveMapDetailFromMap(current2, 0f);
			}
			if (val.Count > 0)
			{
				mapPanel.RefreshIconVisibility();
			}
		}

		private static bool AreAllHarvestablesHarvested(MapDetail mapDetail)
		{
			Il2CppReferenceArray<Harvestable> harvestablesForMapVisibility = mapDetail.m_HarvestablesForMapVisibility;
			if (harvestablesForMapVisibility != null && ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility).Length > 0)
			{
				bool flag = true;
				bool flag2 = false;
				for (int i = 0; i < ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility).Length; i++)
				{
					Harvestable val = ((Il2CppArrayBase<Harvestable>)(object)harvestablesForMapVisibility)[i];
					if ((Object)(object)val != (Object)null)
					{
						flag2 = true;
						if (!val.m_Harvested && (Object)(object)((Component)val).gameObject != (Object)null && ((Component)val).gameObject.activeInHierarchy)
						{
							flag = false;
							break;
						}
					}
				}
				return flag2 && flag;
			}
			return false;
		}
	}
}
namespace MapManager.Patches
{
	[HarmonyPatch(typeof(SandboxConfig), "StartGame")]
	internal static class SandboxConfig_StartGame_Patch
	{
		private static void Postfix(SandboxConfig __instance)
		{
			if (!Settings.Instance.EnableHighDifficultyBunkerFix)
			{
				return;
			}
			Main.Logger.Log("Applying bunker interior fix (all difficulties)", FlaggedLoggingLevel.Debug, "BunkerInteriorFix");
			BunkerDistributor bunkerSetup = __instance.m_BunkerSetup;
			if ((Object)(object)bunkerSetup == (Object)null)
			{
				Main.Logger.Log("BunkerDistributor is null", FlaggedLoggingLevel.Warning, "BunkerInteriorFix");
				return;
			}
			Il2CppReferenceArray<BunkerInteriorSpecification> bunkerInteriors = bunkerSetup.m_BunkerInteriors;
			if (bunkerInteriors == null || ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors).Length == 0)
			{
				Main.Logger.Log("No bunker interiors found", FlaggedLoggingLevel.Warning, "BunkerInteriorFix");
				return;
			}
			List<BunkerInteriorSpecification> val = new List<BunkerInteriorSpecification>();
			for (int i = 0; i < ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors).Length; i++)
			{
				BunkerInteriorSpecification val2 = ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors)[i];
				if (!((Object)(object)val2 == (Object)null))
				{
					string name = ((Object)val2).name;
					if (!name.Contains("Empty") && !name.Contains("Interloper"))
					{
						val.Add(val2);
					}
				}
			}
			if (val.Count == 0)
			{
				Main.Logger.Log("No valid non-empty interiors found, using all interiors", FlaggedLoggingLevel.Warning, "BunkerInteriorFix");
				for (int j = 0; j < ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors).Length; j++)
				{
					val.Add(((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors)[j]);
				}
			}
			List<BunkerLocationInteriorPair> replacements = bunkerSetup.m_Replacements;
			if (replacements == null || replacements.Count == 0)
			{
				Main.Logger.Log("No replacements to modify", FlaggedLoggingLevel.Debug, "BunkerInteriorFix");
				return;
			}
			Random random = new Random();
			int num = 0;
			Enumerator<BunkerLocationInteriorPair> enumerator = replacements.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BunkerLocationInteriorPair current = enumerator.Current;
				if (current != null)
				{
					int num2 = random.Next(val.Count);
					BunkerInteriorSpecification val3 = val[num2];
					if ((Object)(object)current.m_ReplacementInterior != (Object)(object)val3)
					{
						current.m_ReplacementInterior = val3;
						num++;
					}
				}
			}
			Main.Logger.Log($"Replaced {num} bunker interiors", FlaggedLoggingLevel.Debug, "BunkerInteriorFix");
		}
	}
	[HarmonyPatch(typeof(RandomSpawnObject), "ActivateRandomObject")]
	internal static class RandomSpawnObject_ActivateRandomObject_Patch
	{
		private static void Prefix(RandomSpawnObject __instance)
		{
			if (Settings.Instance.EnableHighDifficultyBunkerFix && !((Object)(object)((Component)__instance).gameObject == (Object)null) && !(((Object)((Component)__instance).gameObject).name != "PrepperHatch"))
			{
				int num = Mathf.Clamp(Settings.Instance.HighDifficultyBunkerCount, 0, 1);
				if (__instance.m_NumObjectsToEnableInterloper != num)
				{
					__instance.m_NumObjectsToEnableInterloper = num;
					Main.Logger.Log($"RandomSpawnObject: Set bunker count to {num}", FlaggedLoggingLevel.Debug, "RandomSpawnObject_Patch");
				}
			}
		}
	}
	[HarmonyPatch(typeof(BunkerDistributor), "DistributeBunkerInteriors")]
	internal static class BunkerDistributor_Distribute_Patch
	{
		private static void Postfix(BunkerDistributor __instance)
		{
			if (!Settings.Instance.EnableHighDifficultyBunkerFix)
			{
				return;
			}
			Main.Logger.Log("BunkerDistributor.DistributeBunkerInteriors - replacing interiors", FlaggedLoggingLevel.Debug, "Postfix");
			Il2CppReferenceArray<BunkerInteriorSpecification> bunkerInteriors = __instance.m_BunkerInteriors;
			if (bunkerInteriors == null || ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors).Length == 0)
			{
				Main.Logger.Log("No bunker interiors found", FlaggedLoggingLevel.Warning, "Postfix");
				return;
			}
			List<BunkerInteriorSpecification> val = new List<BunkerInteriorSpecification>();
			for (int i = 0; i < ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors).Length; i++)
			{
				BunkerInteriorSpecification val2 = ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors)[i];
				if (!((Object)(object)val2 == (Object)null))
				{
					string name = ((Object)val2).name;
					if (!name.Contains("Empty") && !name.Contains("Interloper"))
					{
						val.Add(val2);
					}
				}
			}
			if (val.Count == 0)
			{
				Main.Logger.Log("No valid non-empty interiors found, using all interiors", FlaggedLoggingLevel.Warning, "Postfix");
				for (int j = 0; j < ((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors).Length; j++)
				{
					val.Add(((Il2CppArrayBase<BunkerInteriorSpecification>)(object)bunkerInteriors)[j]);
				}
			}
			List<BunkerLocationInteriorPair> replacements = __instance.m_Replacements;
			if (replacements == null || replacements.Count == 0)
			{
				Main.Logger.Log("No replacements to modify", FlaggedLoggingLevel.Debug, "Postfix");
				return;
			}
			Random random = new Random();
			int num = 0;
			Enumerator<BunkerLocationInteriorPair> enumerator = replacements.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BunkerLocationInteriorPair current = enumerator.Current;
				if (current != null)
				{
					int num2 = random.Next(val.Count);
					BunkerInteriorSpecification val3 = val[num2];
					if ((Object)(object)current.m_ReplacementInterior != (Object)(object)val3)
					{
						current.m_ReplacementInterior = val3;
						num++;
					}
				}
			}
			Main.Logger.Log($"Replaced {num} bunker interiors", FlaggedLoggingLevel.Debug, "Postfix");
		}
	}
}
