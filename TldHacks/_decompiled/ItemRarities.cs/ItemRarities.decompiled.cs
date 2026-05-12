using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Cooking;
using ItemRarities;
using ItemRarities.Components;
using ItemRarities.Enums;
using ItemRarities.Managers;
using ItemRarities.Properties;
using ItemRarities.Utilities;
using LocalizationUtilities;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using Newtonsoft.Json.Linq;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(Mod), "Item Rarities", "2.0.5", "Deadman", "https://github.com/Deaadman/item-rarities/releases/latest/download/ItemRarities.dll")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonPriority(0)]
[assembly: MelonIncompatibleAssemblies(new string[] { "DisableBreathEffect", "NonPotableToiletWater", "UnlimitedRockCaches", "ContainerTweaks" })]
[assembly: VerifyLoaderVersion("0.7.2-ci.2388", true)]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyCompany("Deadman")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Deadman")]
[assembly: AssemblyDescription("Item Rarities is a modification that gives each item within The Long Dark a sense of exclusivity.")]
[assembly: AssemblyFileVersion("2.0.5.0")]
[assembly: AssemblyInformationalVersion("2.0.5")]
[assembly: AssemblyProduct("Item Rarities")]
[assembly: AssemblyTitle("ItemRarities")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/Deaadman/item-rarities")]
[assembly: AssemblyVersion("2.0.5.0")]
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
namespace ItemRarities
{
	internal sealed class Mod : MelonMod
	{
		public override void OnInitializeMelon()
		{
			LoadLocalizations();
			Settings.OnLoad();
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			if (!RarityManager.isInitialized && GameManager.IsMainMenuActive())
			{
				MelonCoroutines.Start(RarityManager.InitializeRarities());
			}
		}

		private static void LoadLocalizations()
		{
			ParsingUtilities.ReadJSON("ItemRarities.Resources.Localization.json", out string jsonText);
			LocalizationManager.LoadJsonLocalization(jsonText);
		}
	}
}
namespace ItemRarities.Utilities
{
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

		internal static void LogItemsWithoutRarities()
		{
			HashSet<string> hashSet = new HashSet<string>(new string[203]
			{
				"GEAR_WolfCarcass", "GEAR_NewsprintBootStuffing", "GEAR_NewsprintInsulation", "GEAR_TechnicalBackpack", "GEAR_BoltCutters", "GEAR_CookingPotDummy", "GEAR_SpearHead", "GEAR_BearSpear", "GEAR_BearSpearBroken", "GEAR_BearSpearBrokenStory",
				"GEAR_BearSpearStory", "GEAR_GoldNugget", "Gear_Travois_Dummy", "GEAR_AccelerantGunpowder", "GEAR_CattailPlant", "GEAR_MountainTownFarmKey", "GEAR_PostCard_AC_CentralSpire", "GEAR_PostCard_AC_TopShelf", "GEAR_PostCard_BI_EchoOne-RadioTower", "GEAR_PostCard_BR_Canyon",
				"GEAR_PostCard_BR_Prison", "GEAR_PostCard_CR_AbandonedLookout", "GEAR_PostCard_FM_MuskegOverlook", "GEAR_PostCard_FM_ShortwaveTower", "GEAR_PostCard_ML_ForestryLookout", "GEAR_PostCard_ML_LakeOverlook", "GEAR_PostCard_MT_RadioTower", "GEAR_PostCard_PV_SignalHill", "GEAR_PostCard_RV_Pensive", "GEAR_PostCard_TM_AndresPeak",
				"GEAR_PostCard_TM_TailSection", "GEAR_BackerNote1A", "GEAR_BackerNote1B", "GEAR_BackerNote1C", "GEAR_BackerNote2A", "GEAR_BackerNote2B", "GEAR_BackerNote2C", "GEAR_BackerNote3A", "GEAR_BackerNote3B", "GEAR_BackerNote3C",
				"GEAR_BackerNote4A", "GEAR_BackerNote4B", "GEAR_BackerNote4C", "GEAR_BlackrockAdminNote", "GEAR_BlackrockAmmoRoomNote", "GEAR_BlackrockCodeNote", "GEAR_BlackrockSecurityNote", "GEAR_BlackrockTowerNote", "GEAR_CanneryCodeNote", "GEAR_CanneryMemo",
				"GEAR_CannerySurvivalPath", "GEAR_CanyonClimbersCaveNote", "GEAR_CanyonDeadClimberNote", "GEAR_CanyonFishingHutJournal", "GEAR_CanyonMinersNote", "GEAR_ClimbersJournal", "GEAR_DarkwalkerDiary1", "GEAR_DarkwalkerDiary2", "GEAR_DarkwalkerDiary3", "GEAR_DarkwalkerDiary4",
				"GEAR_DarkwalkerDiary5", "GEAR_DarkwalkerDiary6", "GEAR_DarkwalkerDiary7", "GEAR_DarkwalkerDiary8", "GEAR_DarkwalkerDiary9", "GEAR_DarkwalkerDiary10", "GEAR_DarkwalkerDiary11", "GEAR_DarkwalkerID", "GEAR_DeadmanNote1", "GEAR_DeadmanNote2",
				"GEAR_DeadmanNote3", "GEAR_DeadmanNote4", "GEAR_DeadmanNote5", "GEAR_VisorNoteML1", "GEAR_VisorNoteFM1", "GEAR_VisorNoteFM2", "GEAR_VisorNoteFM3", "GEAR_VisorNoteBR1", "GEAR_VisorNoteBR2", "GEAR_VisorNoteBR3",
				"GEAR_BRKey1", "GEAR_BRKey2", "GEAR_BIKey1", "GEAR_BIKey2", "GEAR_VisorNoteBI1", "GEAR_VisorNoteBI2", "GEAR_VisorNoteBI3", "GEAR_VisorNoteMT1", "GEAR_VisorNoteMT2", "GEAR_VisorNoteMT3",
				"GEAR_VisorNoteMTKey1", "GEAR_VisorNoteHRV1", "GEAR_VisorNoteHRV2", "GEAR_VisorNoteHRV3", "GEAR_VisorNoteHRVKey1", "GEAR_VisorNoteBlackrock1", "GEAR_VisorNoteBlackrock2", "GEAR_VisorNoteBlackrock3", "GEAR_VisorNoteBlackrockKey3", "GEAR_VisorNoteAC1",
				"GEAR_VisorNoteAC2", "GEAR_VisorNoteAC3", "GEAR_VisorNoteACKey1", "GEAR_VisorNoteDP1", "GEAR_VisorNoteDP2", "GEAR_VisorNoteDP3", "GEAR_VisorNoteDPKey1", "GEAR_VisorNoteTWM1", "GEAR_VisorNoteTWM2", "GEAR_VisorNotePV1",
				"GEAR_VisorNotePV2", "GEAR_VisorNotePV3", "GEAR_VisorNoteML2", "GEAR_VisorNoteML3", "GEAR_VisorNoteMLKey2", "GEAR_VisorNoteMLKey3", "GEAR_VisorNoteCH1", "GEAR_VisorNoteCH2", "GEAR_AirfieldCargomasterNote", "GEAR_AirfieldHangarNote",
				"GEAR_AirfieldJunkerNote", "GEAR_AirfieldTowerNote", "GEAR_AirfieldCabinNote", "GEAR_AirfieldControlNote", "GEAR_AirfieldGeologistNote1", "GEAR_AirfieldSecChiefNote1", "GEAR_Tale1ChiefNote1", "GEAR_Tale1ChiefNote2", "GEAR_Tale1ChiefNote3", "GEAR_Tale1ChiefNote4",
				"GEAR_Tale1ChiefNote5", "GEAR_Tale1Transcript1", "GEAR_Tale1Transcript2", "GEAR_Tale1Transcript3", "GEAR_Tale1Transcript4", "GEAR_Logbook_A01", "GEAR_Logbook_A02", "GEAR_Logbook_A03", "GEAR_Logbook_A04", "GEAR_RecipeCardFishcakes",
				"GEAR_RecipeCardPancakePeach", "GEAR_RecipeCardPieFishermans", "GEAR_RecipeCardPieForagers", "GEAR_RecipeCardPieMeat", "GEAR_RecipeCardPiePredator", "GEAR_RecipeCardPorridgeFruit", "GEAR_RecipeCardStewMeat", "GEAR_RecipeCardStewVegetables", "GEAR_LogbookTale2_A01", "GEAR_LogbookTale2_A02",
				"GEAR_LogbookTale2_A03", "GEAR_LogbookTale2_A04", "GEAR_ClipBoardTale2_A", "GEAR_ClipBoardTale2_B", "GEAR_Tale2GeologistNote1", "GEAR_Tale2_CorpseKey", "GEAR_Tale2RudigerNote1", "GEAR_Tale2SecurityChiefBones", "GEAR_LangstonMineKey1", "GEAR_Tale2_LockerKey",
				"GEAR_LangstonMineLockboxKey1", "GEAR_ClipBoardTale2_C", "GEAR_ClipBoardTale2_D", "GEAR_MineRegionBunkhouseNote", "GEAR_MineRegionPumphouseNote", "GEAR_PostCard_Tale02", "GEAR_PostCard_MR_Settlement", "GEAR_PostCard_MR_Window", "GEAR_Tale3ForemanLog1", "GEAR_Tale3ForemanLog2",
				"GEAR_Tale3ForemanLog3", "GEAR_Tale3RudigerNote1", "GEAR_Tale3RudigerNote2", "GEAR_Tale3RudigerNote3", "GEAR_Tale3ForemanLog4", "GEAR_Tale3_ForemanRefugeKey", "GEAR_Tale_03_PostCard_01", "GEAR_Tale_03_PostCard_02", "GEAR_Tale_03_PostCard_03", "GEAR_Tale_03_PostCard_04",
				"GEAR_Tale_03_PostCard_05", "GEAR_SecurityChiefID", "GEAR_MountainPassAvalancheNote", "GEAR_MountainPassWeatherStationNote", "GEAR_Rudiger_Watch", "GEAR_PostCard_MP_OgresTeardrop", "GEAR_PostCard_MP_BrokenRoad", "GEAR_RecipeCardSoupPotato", "GEAR_RecipeCardSoupRabbit", "GEAR_RecipeCardBarPemmican",
				"GEAR_BunkerPapers1", "GEAR_BunkerPapers2", "GEAR_BunkerPapers3", "GEAR_BunkerPapers4", "GEAR_BunkerSchematic_A", "GEAR_BunkerSchematic_B", "GEAR_BunkerSchematic_C", "GEAR_CougarPoster1", "GEAR_CougarPoster2", "GEAR_NaturalistNote1",
				"GEAR_NaturalistNote2", "GEAR_TraderWyattBonesNote", "GEAR_Trader_Pamphlet"
			});
			Enumerator<string, string> enumerator = ConsoleManager.m_SearchStringToGearNames._values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (current.StartsWith("GEAR_") && !hashSet.Contains(current) && RarityManager.GetRarity(current) == Rarities.None)
				{
					Log("Item '" + current + "' does not have an assigned rarity.");
				}
			}
		}

		internal static void LogDecorationNames()
		{
			Enumerator<DecorationItem> enumerator = DecorationItem.s_DecorationItems.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string displayNameLocId = enumerator.Current.DisplayNameLocId;
				Log("Decoration Item Name: " + displayNameLocId);
			}
		}
	}
	internal static class ParsingUtilities
	{
		internal static void LoadRaritiesFromIR(StreamReader reader)
		{
			Rarities rarities = Rarities.None;
			while (true)
			{
				string text = reader.ReadLine();
				if (text == null)
				{
					break;
				}
				text = text.Trim();
				if (string.IsNullOrEmpty(text) || text.StartsWith("#"))
				{
					continue;
				}
				if (text.StartsWith("[") && text.EndsWith("]"))
				{
					if (Enum.TryParse<Rarities>(text.Trim('[', ']'), out var result))
					{
						rarities = result;
					}
				}
				else if (rarities != 0)
				{
					RarityManager.raritiesLookup[text] = rarities;
				}
			}
		}

		internal static void LoadRaritiesFromJson(string jsonText)
		{
			foreach (KeyValuePair<string, JToken> item in JObject.Parse(jsonText))
			{
				if (!Enum.TryParse<Rarities>(item.Key, out var result) || item.Value == null)
				{
					continue;
				}
				foreach (JToken item2 in (IEnumerable<JToken>)item.Value)
				{
					RarityManager.raritiesLookup[((object)item2).ToString()] = result;
				}
			}
		}

		internal static void ReadJSON(string jsonFilePath, out string jsonText)
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(jsonFilePath);
			if (stream == null)
			{
				jsonText = string.Empty;
				return;
			}
			using StreamReader streamReader = new StreamReader(stream);
			jsonText = streamReader.ReadToEnd();
		}
	}
	internal static class SettingsUtilities
	{
		internal static void UpdateFieldVisibilities(bool visible)
		{
			Settings.Instance.SetFieldVisible("commonRed", visible);
			Settings.Instance.SetFieldVisible("commonGreen", visible);
			Settings.Instance.SetFieldVisible("commonBlue", visible);
			Settings.Instance.SetFieldVisible("uncommonRed", visible);
			Settings.Instance.SetFieldVisible("uncommonGreen", visible);
			Settings.Instance.SetFieldVisible("uncommonBlue", visible);
			Settings.Instance.SetFieldVisible("rareRed", visible);
			Settings.Instance.SetFieldVisible("rareGreen", visible);
			Settings.Instance.SetFieldVisible("rareBlue", visible);
			Settings.Instance.SetFieldVisible("epicRed", visible);
			Settings.Instance.SetFieldVisible("epicGreen", visible);
			Settings.Instance.SetFieldVisible("epicBlue", visible);
			Settings.Instance.SetFieldVisible("legendaryRed", visible);
			Settings.Instance.SetFieldVisible("legendaryGreen", visible);
			Settings.Instance.SetFieldVisible("legendaryBlue", visible);
			Settings.Instance.SetFieldVisible("mythicRed", visible);
			Settings.Instance.SetFieldVisible("mythicGreen", visible);
			Settings.Instance.SetFieldVisible("mythicBlue", visible);
		}
	}
	internal static class UIButtonExtensions
	{
		private static UIButton Instantiate(this UIButton original)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			UIButton component = Object.Instantiate<GameObject>(((Component)original).gameObject).GetComponent<UIButton>();
			((Component)component).transform.parent = ((Component)original).transform.parent;
			((Component)component).transform.localPosition = ((Component)original).transform.localPosition;
			((Component)component).transform.localScale = Vector3.one;
			return component;
		}

		internal static void NewSortButton(Il2CppReferenceArray<UIButton> uiButtonsArray, string buttonName, string spriteName, Action<UIButton> uiButtonMethod, float posX = 0f, float posY = 0f, float posZ = 0f)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			Action<UIButton> uiButtonMethod2 = uiButtonMethod;
			UIButton original = new UIButton();
			foreach (UIButton item in (Il2CppArrayBase<UIButton>)(object)uiButtonsArray)
			{
				original = item;
			}
			UIButton newButton = original.Instantiate();
			((Object)newButton).name = buttonName;
			newButton.SetSpriteName(spriteName);
			EventDelegate.Set(newButton.onClick, Callback.op_Implicit((Action)delegate
			{
				uiButtonMethod2(newButton);
			}));
			((Component)newButton).transform.localPosition = new Vector3(posX, posY, posZ);
			CollectionExtensions.AddItem<UIButton>((IEnumerable<UIButton>)uiButtonsArray, newButton);
		}

		private static void SetSpriteName(this UIButton btn, string newSpriteName)
		{
			UISprite component = ((Component)btn).GetComponent<UISprite>();
			component.spriteName = newSpriteName;
			((UIRect)component).OnInit();
		}
	}
	internal static class UILabelExtensions
	{
		internal static UILabel SetupGameObjectWithUILabel(string gameObjectName, Transform parent, bool worldPositionStays, bool inspectLabel = false, float posX = 0f, float posY = 0f, float posZ = 0f)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(gameObjectName);
			UILabel val2 = val.AddComponent<UILabel>();
			val.transform.SetParent(parent, worldPositionStays);
			val.transform.localPosition = new Vector3(posX, posY, posZ);
			SetupUILabel(val2, string.Empty, (FontStyle)0, (Crispness)2, (Alignment)(inspectLabel ? 1 : 2), (Overflow)2, multiLine: true, inspectLabel ? 2 : 0, inspectLabel ? 14 : 18, Color.clear, capsLock: true);
			return val2;
		}

		private static void SetupUILabel(UILabel label, string text, FontStyle fontStyle, Crispness crispness, Alignment alignment, Overflow overflow, bool multiLine, int depth, int fontSize, Color color, bool capsLock)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			label.text = text;
			label.ambigiousFont = (Object)(object)GameManager.GetFontManager().GetUIFontForCharacterSet(FontManager.m_CurrentCharacterSet);
			label.bitmapFont = GameManager.GetFontManager().GetUIFontForCharacterSet(FontManager.m_CurrentCharacterSet);
			label.font = GameManager.GetFontManager().GetUIFontForCharacterSet(FontManager.m_CurrentCharacterSet);
			label.fontStyle = fontStyle;
			label.keepCrispWhenShrunk = crispness;
			label.alignment = alignment;
			label.overflowMethod = overflow;
			label.multiLine = multiLine;
			((UIWidget)label).depth = depth;
			label.fontSize = fontSize;
			((UIWidget)label).color = color;
			label.capsLock = capsLock;
		}
	}
}
namespace ItemRarities.Patches
{
	internal static class ClothingPatches
	{
		[HarmonyPatch(typeof(ClothingSlot), "ActivateMouseHoverHighlight")]
		private static class ClothingSlotHoverPatch
		{
			private static void Postfix(ClothingSlot __instance)
			{
				RarityUIManager.UpdateClothingSlotColors(__instance);
			}
		}

		[HarmonyPatch(typeof(ClothingSlot), "SetSelected")]
		private static class ClothingSlotSelectedPatch
		{
			private static void Postfix(ClothingSlot __instance, bool isSelected)
			{
				RarityUIManager.UpdateClothingSlotColors(__instance);
			}
		}

		[HarmonyPatch(typeof(Panel_Clothing), "Enable")]
		private static class ClothingSlotEnabledPatch
		{
			private static void Postfix(Panel_Clothing __instance)
			{
				Enumerator<ClothingSlot> enumerator = __instance.m_ClothingSlots.GetEnumerator();
				while (enumerator.MoveNext())
				{
					RarityUIManager.UpdateClothingSlotColors(enumerator.Current);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Clothing), "OnUseClothingItem")]
		private static class ClothingSlotUsePatch
		{
			private static void Postfix(Panel_Clothing __instance)
			{
				MelonCoroutines.Start(RarityUIManager.DelayedUpdateAllSlots(__instance));
			}
		}
	}
	internal static class ContainerPatches
	{
		[HarmonyPatch(typeof(InventoryGridItem), "OnClick")]
		private static class InventoryGridItemClickPatch
		{
			private static void Postfix(InventoryGridItem __instance)
			{
				Panel_Container panel = InterfaceManager.GetPanel<Panel_Container>();
				if (!((Object)(object)panel == (Object)null))
				{
					RarityUIManager.UpdateContainerColours(panel);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Container), "Initialize")]
		private static class PanelContainerInitializePatch
		{
			private static void Postfix(Panel_Container __instance)
			{
				UIButtonExtensions.NewSortButton(__instance.m_SortButtons, "Button_SortRarity", "ico_Star", (Action<UIButton>)__instance.OnSortInventoryChange, 100f, 1.8f, 0f);
				__instance.m_SortFlipDictionary.Add("GAMEPLAY_SortRarity", false);
			}
		}

		[HarmonyPatch(typeof(Panel_Container), "OnSortInventoryChange")]
		private static class PanelContainerInventorySortedPatch
		{
			private static void Prefix(Panel_Container __instance, UIButton sortButtonClicked)
			{
				if (((Object)sortButtonClicked).name == "Button_SortRarity" && __instance.m_InventorySortName == "GAMEPLAY_SortRarity")
				{
					RarityUIManager.ToggleRaritySort();
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Container), "UpdateFilteredContainerList")]
		private static class PanelContainerContainerListPatch
		{
			private static void Postfix(Panel_Container __instance)
			{
				if (__instance.m_InventorySortName == "GAMEPLAY_SortRarity")
				{
					RarityUIManager.CompareGearByRarity(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Container), "UpdateFilteredInventoryList")]
		private static class PanelContainerInventoryListPatch
		{
			private static void Postfix(Panel_Container __instance)
			{
				if (__instance.m_InventorySortName == "GAMEPLAY_SortRarity")
				{
					RarityUIManager.CompareGearByRarityInventory(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Container), "Update")]
		private static class PanelContainerUpdatePatch
		{
			private static void Postfix(Panel_Container __instance)
			{
				if (__instance.m_FilteredContainerList.Count != 0 && __instance.m_FilteredInventoryList.Count != 0)
				{
					RarityUIManager.UpdateContainerColours(__instance);
				}
			}
		}
	}
	internal static class InventoryPatches
	{
		[HarmonyPatch(typeof(InventoryGridItem), "OnHover")]
		private static class InventoryGridItemHoverPatch
		{
			private static void Postfix(InventoryGridItem __instance, bool isOver)
			{
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)__instance.m_Button == (Object)null))
				{
					((UIButtonColor)__instance.m_Button).hover = RarityUIManager.GetRarityAndColour(__instance.m_GearItem, 0.5f);
					((UIButtonColor)__instance.m_Button).pressed = RarityUIManager.GetRarityAndColour(__instance.m_GearItem, 0.5f, 0.5f);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory), "Initialize")]
		private static class PanelInventoryInitializePatch
		{
			private static void Postfix(Panel_Inventory __instance)
			{
				UIButtonExtensions.NewSortButton(__instance.m_SortButtons, "Button_SortRarity", "ico_Star", (Action<UIButton>)__instance.OnSortChange, 85f, 1.8f, 0f);
				__instance.m_SortFlipDictionary.Add("GAMEPLAY_SortRarity", false);
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory), "OnSortChange")]
		private static class PanelInventorySortPatch
		{
			private static void Prefix(Panel_Inventory __instance, UIButton sortButtonClicked)
			{
				if (((Object)sortButtonClicked).name == "Button_SortRarity" && __instance.m_SortName == "GAMEPLAY_SortRarity")
				{
					RarityUIManager.ToggleRaritySort();
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory), "UpdateFilteredInventoryList")]
		private static class PanelInventorySortRarityPatch
		{
			private static void Postfix(Panel_Inventory __instance)
			{
				if (__instance.m_SortName == "GAMEPLAY_SortRarity")
				{
					RarityUIManager.CompareGearByRarity(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory), "UpdateGearStatsBlock")]
		private static class PanelInventoryUpdateGearPatch
		{
			private static void Postfix(Panel_Inventory __instance)
			{
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0098: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)__instance.m_SelectedSpriteObj == (Object)null || (Object)(object)__instance.m_SelectedSpriteTweenScale == (Object)null || __instance.GetCurrentlySelectedItem() == null)
				{
					return;
				}
				if ((Object)(object)__instance.GetCurrentlySelectedItem().m_GearItem == (Object)null)
				{
					UILabel? rarityLabel = RarityUIManager.m_RarityLabel;
					if (rarityLabel != null)
					{
						((Component)rarityLabel).gameObject.SetActive(false);
					}
				}
				((UIWidget)__instance.m_SelectedSpriteObj.GetComponentInChildren<UISprite>()).color = RarityUIManager.GetRarityAndColour(__instance.GetCurrentlySelectedItem().m_GearItem, 1f, 0.5f);
				((UIWidget)((Component)__instance.m_SelectedSpriteTweenScale).GetComponent<UISprite>()).color = RarityUIManager.GetRarityAndColour(__instance.GetCurrentlySelectedItem().m_GearItem, 1f, 1f);
			}
		}
	}
	internal static class OtherPatches
	{
		[HarmonyPatch(typeof(ItemDescriptionPage), "UpdateGearItemDescription")]
		private static class UpdateItemDescriptionPageRarity
		{
			private static void Postfix(ItemDescriptionPage __instance, GearItem gi)
			{
				RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_ItemNameLabel).gameObject.transform, 0f, 35f, 0f);
				RarityUIManager.UpdateRarityLabelProperties(gi);
			}
		}

		[HarmonyPatch(typeof(Panel_ActionsRadial), "UpdateDisplayText")]
		private static class UpdatePanelActionsRadialRarity
		{
			private static void Postfix(Panel_ActionsRadial __instance)
			{
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)RarityUIManager.m_RarityLabel == (Object)null)
				{
					RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_SegmentLabel).gameObject.transform, 0f, 35f, 0f);
				}
				foreach (RadialMenuArm item in (Il2CppArrayBase<RadialMenuArm>)(object)__instance.m_RadialArms)
				{
					if (item.IsHoveredOver())
					{
						RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_SegmentLabel).gameObject.transform, 0f, 35f, 0f);
						RarityUIManager.UpdateRarityLabelProperties(item.m_GearItem);
						if (!((Object)(object)item.m_GearItem == (Object)null))
						{
							((UIWidget)item.m_BG).color = RarityUIManager.GetRarityAndColour(item.m_GearItem, 1f, 0.3f);
						}
						break;
					}
					UILabel? rarityLabel = RarityUIManager.m_RarityLabel;
					if (rarityLabel != null)
					{
						((Component)rarityLabel).gameObject.SetActive(false);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Cooking), "GetSelectedCookableItem")]
		private static class UpdatePanelCookingRarity
		{
			private static void Postfix(Panel_Cooking __instance, ref CookableItem __result)
			{
				RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_Label_CookedItemName).gameObject.transform, 0f, 35f, 0f);
				RarityUIManager.UpdateRarityLabelProperties(__result.m_GearItem);
			}
		}

		[HarmonyPatch(typeof(Panel_Crafting), "RefreshSelectedBlueprint")]
		private static class UpdatePanelCraftingRarity
		{
			private static void Postfix(Panel_Crafting __instance)
			{
				if ((Object)(object)__instance.SelectedBPI == (Object)null)
				{
					return;
				}
				if (Object.op_Implicit((Object)(object)__instance.SelectedBPI.m_CraftedResultDecoration))
				{
					UILabel? rarityLabel = RarityUIManager.m_RarityLabel;
					if (rarityLabel != null)
					{
						((Component)rarityLabel).gameObject.SetActive(false);
					}
				}
				else
				{
					RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_SelectedName).gameObject.transform, 0f, 35f, 0f);
					RarityUIManager.UpdateRarityLabelProperties(__instance.SelectedBPI.m_CraftedResultGear);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_HUD), "SetHoverText")]
		private static class UpdateHoverLabelRarity
		{
			private static void Postfix(Panel_HUD __instance, string hoverText, GameObject itemUnderCrosshairs, HoverTextState textState)
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)itemUnderCrosshairs == (Object)null) && (Object)(object)itemUnderCrosshairs.GetComponent<GearItem>() != (Object)null)
				{
					((UIWidget)__instance.m_Label_ObjectName).color = RarityUIManager.GetRarityAndColour(itemUnderCrosshairs.GetComponent<GearItem>(), 1f, 1f);
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory_Examine), "RefreshMainWindow")]
		private static class UpdatePanelInventoryExamineRarity
		{
			private static void Postfix(Panel_Inventory_Examine __instance)
			{
				RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_Item_Label).gameObject.transform, 0f, 35f, 0f);
				RarityUIManager.UpdateRarityLabelProperties(__instance.m_GearItem);
			}
		}

		[HarmonyPatch(typeof(Panel_Milling), "GetSelected")]
		private static class UpdatePanelMillingRarity
		{
			private static void Postfix(Panel_Milling __instance, ref GearItem __result)
			{
				RarityUIManager.InstantiateOrMoveRarityLabel(((Component)__instance.m_NameLabel).gameObject.transform, 0f, 35f, 0f);
				RarityUIManager.UpdateRarityLabelProperties(__result);
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "InitLabelsForGear")]
		private static class UpdatePlayerManagerInspectModeRarity
		{
			private static void Prefix(PlayerManager __instance)
			{
				Panel_HUD panel = InterfaceManager.GetPanel<Panel_HUD>();
				if ((Object)(object)RarityUIManager.m_RarityLabelInspect == (Object)null)
				{
					RarityUIManager.InstantiateInspectRarityLabel(((Component)panel.m_InspectModeDetailsGrid).gameObject.transform);
				}
				RarityUIManager.UpdateRarityLabelProperties(__instance.m_Gear, inspectLabel: true);
				InspectFade val = ((Il2CppArrayBase<InspectFade>)(object)panel.m_InspectFadeSequence)[1];
				UIWidget[] array = (UIWidget[])(object)new UIWidget[((Il2CppArrayBase<UIWidget>)(object)val.m_FadeElements).Length + 1];
				Array.Copy(Il2CppArrayBase<UIWidget>.op_Implicit((Il2CppArrayBase<UIWidget>)(object)val.m_FadeElements), array, ((Il2CppArrayBase<UIWidget>)(object)val.m_FadeElements).Length);
				if (RarityUIManager.m_RarityLabelInspect != null)
				{
					array[((Il2CppArrayBase<UIWidget>)(object)val.m_FadeElements).Length] = (UIWidget)(object)RarityUIManager.m_RarityLabelInspect;
					val.m_FadeElements = Il2CppReferenceArray<UIWidget>.op_Implicit(array);
					((Il2CppArrayBase<InspectFade>)(object)panel.m_InspectFadeSequence)[1] = val;
				}
			}
		}
	}
}
namespace ItemRarities.Managers
{
	public static class RarityManager
	{
		[CompilerGenerated]
		private sealed class <InitializeRarities>d__4 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private object <>2__current;

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
			public <InitializeRarities>d__4(int <>1__state)
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
				switch (<>1__state)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<>2__current = LoadRaritiesFromLocalFile();
					<>1__state = 1;
					return true;
				case 1:
					<>1__state = -1;
					LoadRaritiesFromModComponents();
					isInitialized = true;
					return false;
				}
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

		[CompilerGenerated]
		private sealed class <LoadRaritiesFromLocalFile>d__5 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private object <>2__current;

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
			public <LoadRaritiesFromLocalFile>d__5(int <>1__state)
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
				if (<>1__state != 0)
				{
					return false;
				}
				<>1__state = -1;
				ParsingUtilities.ReadJSON("ItemRarities.Resources.ItemRarities.json", out string jsonText);
				ParsingUtilities.LoadRaritiesFromJson(jsonText);
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

		internal static bool isInitialized;

		internal static Dictionary<string, Rarities> raritiesLookup = new Dictionary<string, Rarities>();

		public static void AddGearItemAndRarity(string gearItem, Rarities rarity)
		{
			raritiesLookup[gearItem] = rarity;
		}

		internal static Rarities GetRarity(string itemName)
		{
			return raritiesLookup.GetValueOrDefault(itemName, Rarities.None);
		}

		[IteratorStateMachine(typeof(<InitializeRarities>d__4))]
		internal static IEnumerator InitializeRarities()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <InitializeRarities>d__4(0);
		}

		[IteratorStateMachine(typeof(<LoadRaritiesFromLocalFile>d__5))]
		private static IEnumerator LoadRaritiesFromLocalFile()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <LoadRaritiesFromLocalFile>d__5(0);
		}

		private static void LoadRaritiesFromModComponents()
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (directoryName == null)
			{
				return;
			}
			string[] files = Directory.GetFiles(directoryName, "*.modcomponent");
			for (int i = 0; i < files.Length; i++)
			{
				using ZipArchive zipArchive = ZipFile.OpenRead(files[i]);
				ZipArchiveEntry entry = zipArchive.GetEntry("rarities.ir");
				if (entry != null)
				{
					using StreamReader reader = new StreamReader(entry.Open());
					ParsingUtilities.LoadRaritiesFromIR(reader);
				}
			}
		}
	}
	internal static class RarityUIManager
	{
		[CompilerGenerated]
		private sealed class <DelayedUpdateAllSlots>d__6 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private object <>2__current;

			public Panel_Clothing panel;

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
			public <DelayedUpdateAllSlots>d__6(int <>1__state)
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
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				switch (<>1__state)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<>2__current = (object)new WaitForEndOfFrame();
					<>1__state = 1;
					return true;
				case 1:
				{
					<>1__state = -1;
					Enumerator<ClothingSlot> enumerator = panel.m_ClothingSlots.GetEnumerator();
					while (enumerator.MoveNext())
					{
						UpdateClothingSlotColors(enumerator.Current);
					}
					return false;
				}
				}
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

		private static bool isRaritySortDescending = true;

		internal static UILabel? m_RarityLabel;

		internal static UILabel? m_RarityLabelInspect;

		internal static void CompareGearByRarity(Panel_Inventory panelInventory)
		{
			List<InventoryGridDataItem> list = new List<InventoryGridDataItem>();
			for (int i = 0; i < panelInventory.m_FilteredInventoryList.Count; i++)
			{
				list.Add(panelInventory.m_FilteredInventoryList[i]);
			}
			list.Sort(delegate(InventoryGridDataItem a, InventoryGridDataItem b)
			{
				Rarities rarities = (((Object)(object)((a != null) ? a.m_GearItem : null) != (Object)null) ? RarityManager.GetRarity(((Object)a.m_GearItem).name) : Rarities.None);
				Rarities rarities2 = (((Object)(object)((b != null) ? b.m_GearItem : null) != (Object)null) ? RarityManager.GetRarity(((Object)b.m_GearItem).name) : Rarities.None);
				return (!isRaritySortDescending) ? rarities.CompareTo(rarities2) : rarities2.CompareTo(rarities);
			});
			panelInventory.m_FilteredInventoryList.Clear();
			foreach (InventoryGridDataItem item in list)
			{
				panelInventory.m_FilteredInventoryList.Add(item);
			}
		}

		internal static void CompareGearByRarity(Panel_Container panelContainer)
		{
			List<InventoryGridDataItem> list = new List<InventoryGridDataItem>();
			for (int i = 0; i < panelContainer.m_FilteredContainerList.Count; i++)
			{
				list.Add(panelContainer.m_FilteredContainerList[i]);
			}
			list.Sort(delegate(InventoryGridDataItem a, InventoryGridDataItem b)
			{
				Rarities rarities = (((Object)(object)((a != null) ? a.m_GearItem : null) != (Object)null) ? RarityManager.GetRarity(((Object)a.m_GearItem).name) : Rarities.None);
				Rarities rarities2 = (((Object)(object)((b != null) ? b.m_GearItem : null) != (Object)null) ? RarityManager.GetRarity(((Object)b.m_GearItem).name) : Rarities.None);
				return (!isRaritySortDescending) ? rarities.CompareTo(rarities2) : rarities2.CompareTo(rarities);
			});
			panelContainer.m_FilteredContainerList.Clear();
			foreach (InventoryGridDataItem item in list)
			{
				panelContainer.m_FilteredContainerList.Add(item);
			}
		}

		internal static void CompareGearByRarityInventory(Panel_Container panelContainer)
		{
			List<InventoryGridDataItem> list = new List<InventoryGridDataItem>();
			for (int i = 0; i < panelContainer.m_FilteredInventoryList.Count; i++)
			{
				list.Add(panelContainer.m_FilteredInventoryList[i]);
			}
			list.Sort(delegate(InventoryGridDataItem a, InventoryGridDataItem b)
			{
				Rarities rarities = (((Object)(object)((a != null) ? a.m_GearItem : null) != (Object)null) ? RarityManager.GetRarity(((Object)a.m_GearItem).name) : Rarities.None);
				Rarities rarities2 = (((Object)(object)((b != null) ? b.m_GearItem : null) != (Object)null) ? RarityManager.GetRarity(((Object)b.m_GearItem).name) : Rarities.None);
				return (!isRaritySortDescending) ? rarities.CompareTo(rarities2) : rarities2.CompareTo(rarities);
			});
			panelContainer.m_FilteredInventoryList.Clear();
			foreach (InventoryGridDataItem item in list)
			{
				panelContainer.m_FilteredInventoryList.Add(item);
			}
		}

		[IteratorStateMachine(typeof(<DelayedUpdateAllSlots>d__6))]
		internal static IEnumerator DelayedUpdateAllSlots(Panel_Clothing panel)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <DelayedUpdateAllSlots>d__6(0)
			{
				panel = panel
			};
		}

		internal static Color GetRarityAndColour(GearItem gearItem, float alpha = 1f, float secondAlpha = 0f)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)gearItem == (Object)null))
			{
				return GetRarityColour(RarityManager.GetRarity(((Object)gearItem).name), alpha, secondAlpha);
			}
			return GetRarityColour(Rarities.None, alpha, secondAlpha);
		}

		private static Color GetRarityColour(Rarities rarity, float alpha = 1f, float secondAlpha = 0f)
		{
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			if (Settings.Instance.customColours)
			{
				return (Color)(rarity switch
				{
					Rarities.Common => new Color(Settings.Instance.commonRed, Settings.Instance.commonGreen, Settings.Instance.commonBlue, alpha), 
					Rarities.Uncommon => new Color(Settings.Instance.uncommonRed, Settings.Instance.uncommonGreen, Settings.Instance.uncommonBlue, alpha), 
					Rarities.Rare => new Color(Settings.Instance.rareRed, Settings.Instance.rareGreen, Settings.Instance.rareBlue, alpha), 
					Rarities.Epic => new Color(Settings.Instance.epicRed, Settings.Instance.epicGreen, Settings.Instance.epicBlue, alpha), 
					Rarities.Legendary => new Color(Settings.Instance.legendaryRed, Settings.Instance.legendaryGreen, Settings.Instance.legendaryBlue, alpha), 
					Rarities.Mythic => new Color(Settings.Instance.mythicRed, Settings.Instance.mythicGreen, Settings.Instance.mythicBlue, alpha), 
					_ => new Color(1f, 1f, 1f, secondAlpha), 
				});
			}
			return (Color)(rarity switch
			{
				Rarities.Common => new Color(0.6f, 0.6f, 0.6f, alpha), 
				Rarities.Uncommon => new Color(0.3f, 0.7f, 0f, alpha), 
				Rarities.Rare => new Color(0f, 0.6f, 0.9f, alpha), 
				Rarities.Epic => new Color(0.7f, 0.3f, 0.9f, alpha), 
				Rarities.Legendary => new Color(0.9f, 0.5f, 0.2f, alpha), 
				Rarities.Mythic => new Color(0.8f, 0.7f, 0.3f, alpha), 
				_ => new Color(1f, 1f, 1f, secondAlpha), 
			});
		}

		private static string GetRarityLocalizationKey(Rarities rarity)
		{
			return rarity switch
			{
				Rarities.Common => Localization.Get("GAMEPLAY_RarityCommon"), 
				Rarities.Uncommon => Localization.Get("GAMEPLAY_RarityUncommon"), 
				Rarities.Rare => Localization.Get("GAMEPLAY_RarityRare"), 
				Rarities.Epic => Localization.Get("GAMEPLAY_RarityEpic"), 
				Rarities.Legendary => Localization.Get("GAMEPLAY_RarityLegendary"), 
				Rarities.Mythic => Localization.Get("GAMEPLAY_RarityMythic"), 
				_ => string.Empty, 
			};
		}

		internal static void InstantiateInspectRarityLabel(Transform transform)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("RarityLabelInspectGameObject");
			val.transform.SetParent(transform, false);
			val.transform.localPosition = new Vector3(0f, 0f, 0f);
			m_RarityLabelInspect = UILabelExtensions.SetupGameObjectWithUILabel("RarityLabelInspect", val.transform, worldPositionStays: false, inspectLabel: true, 40f);
			((Component)m_RarityLabelInspect).transform.SetSiblingIndex(0);
		}

		internal static void InstantiateOrMoveRarityLabel(Transform transform, float posX, float posY, float posZ)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)m_RarityLabel == (Object)null)
			{
				m_RarityLabel = UILabelExtensions.SetupGameObjectWithUILabel("RarityLabel", transform, worldPositionStays: false, inspectLabel: false, posX, posY, posZ);
				return;
			}
			((Component)m_RarityLabel).transform.SetParent(transform, false);
			((Component)m_RarityLabel).transform.localPosition = new Vector3(posX, posY, posZ);
		}

		internal static void ToggleRaritySort()
		{
			isRaritySortDescending = !isRaritySortDescending;
		}

		internal static void UpdateClothingSlotColors(ClothingSlot slot)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			Color val = (Color)(((Object)(object)slot.m_GearItem != (Object)null) ? GetRarityAndColour(slot.m_GearItem, 1f, 0.5f) : new Color(1f, 1f, 1f, 0.5f));
			Color val2 = default(Color);
			((Color)(ref val2))..ctor(val.r, val.g, val.b, 0.5f);
			Color pressed = default(Color);
			((Color)(ref pressed))..ctor(val.r, val.g, val.b, 0.6f);
			if ((Object)(object)slot.m_Selected != (Object)null)
			{
				List<Transform> val3 = new List<Transform>();
				slot.m_Selected.GetComponentsInChildren<Transform>(true, val3);
				for (int i = 0; i < val3.Count; i++)
				{
					Transform val4 = val3[i];
					string name = ((Object)((Component)val4).gameObject).name;
					if ((name == "InnerGlow" || name == "TweenedContent") ? true : false)
					{
						((UIWidget)((Component)val4).GetComponent<UISprite>()).color = val;
					}
				}
			}
			if ((Object)(object)slot.m_SpriteBoxHover != (Object)null)
			{
				((UIWidget)slot.m_SpriteBoxHover.GetComponent<UISprite>()).color = val2;
			}
			List<Transform> val5 = new List<Transform>();
			((Component)slot).GetComponentsInChildren<Transform>(true, val5);
			for (int j = 0; j < val5.Count; j++)
			{
				Transform val6 = val5[j];
				if (!(((Object)((Component)val6).gameObject).name != "Button"))
				{
					UIWidget component = ((Component)val6).GetComponent<UIWidget>();
					if ((Object)(object)component != (Object)null)
					{
						component.color = val;
					}
					UIButton component2 = ((Component)val6).GetComponent<UIButton>();
					if (!((Object)(object)component2 == (Object)null))
					{
						((UIButtonColor)component2).hover = val2;
						((UIButtonColor)component2).pressed = pressed;
					}
				}
			}
		}

		internal static void UpdateContainerColours(Panel_Container panelContainer)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			InventoryGridDataItem currentlySelectedItem = panelContainer.GetCurrentlySelectedItem();
			if ((Object)(object)panelContainer.m_SelectedSpriteObj == (Object)null || currentlySelectedItem == null)
			{
				return;
			}
			((UIWidget)panelContainer.m_SelectedSpriteObj.GetComponentInChildren<UISprite>()).color = GetRarityAndColour(currentlySelectedItem.m_GearItem, 1f, 0.5f);
			List<Transform> val = new List<Transform>();
			panelContainer.m_SelectedSpriteObj.GetComponentsInChildren<Transform>(true, val);
			for (int i = 0; i < val.Count; i++)
			{
				Transform val2 = val[i];
				if (((Object)((Component)val2).gameObject).name == "TweenedContent")
				{
					((UIWidget)((Component)val2).GetComponent<UISprite>()).color = GetRarityAndColour(currentlySelectedItem.m_GearItem, 1f, 1f);
				}
			}
		}

		internal static void UpdateRarityLabelProperties(GearItem gearItem, bool inspectLabel = false)
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)gearItem == (Object)null)
			{
				return;
			}
			UILabel val = (inspectLabel ? m_RarityLabelInspect : m_RarityLabel);
			Rarities rarity = RarityManager.GetRarity(((Object)gearItem).name);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			if (rarity == Rarities.None)
			{
				val.text = GetRarityLocalizationKey(rarity);
				((Component)val).gameObject.SetActive(false);
				return;
			}
			val.text = GetRarityLocalizationKey(rarity);
			((Component)val).gameObject.SetActive(true);
			MythicGlowEffect component = ((Component)val).GetComponent<MythicGlowEffect>();
			if ((Object)(object)component != (Object)null)
			{
				Object.Destroy((Object)(object)component);
			}
			if (rarity == Rarities.Mythic)
			{
				((Component)val).gameObject.AddComponent<MythicGlowEffect>();
			}
			((UIWidget)val).color = GetRarityColour(rarity);
		}
	}
}
namespace ItemRarities.Enums
{
	public enum Rarities
	{
		None,
		Common,
		Uncommon,
		Rare,
		Epic,
		Legendary,
		Mythic
	}
}
namespace ItemRarities.Components
{
	[RegisterTypeInIl2Cpp(false)]
	internal class MythicGlowEffect : MonoBehaviour
	{
		private UILabel? labelMythic;

		private Color originalColor;

		private float time;

		private const float glowMin = 0.9f;

		private const float glowMax = 1.3f;

		private const float glowSpeed = 2f;

		private static readonly Color shiftColor = new Color(1.1f, 1.1f, 0.9f);

		private void OnDisable()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)labelMythic != (Object)null)
			{
				((UIWidget)labelMythic).color = originalColor;
			}
		}

		private void Start()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			labelMythic = ((Component)this).GetComponent<UILabel>();
			originalColor = ((UIWidget)labelMythic).color;
		}

		private void Update()
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			if (labelMythic != null)
			{
				time += Time.deltaTime * 2f;
				float num = Mathf.Lerp(0.9f, 1.3f, (Mathf.Sin(time) + 1f) * 0.5f);
				Color color = default(Color);
				((Color)(ref color))..ctor(originalColor.r * num * shiftColor.r, originalColor.g * num * shiftColor.g, originalColor.b * num * shiftColor.b, originalColor.a);
				((UIWidget)labelMythic).color = color;
			}
		}
	}
}
namespace ItemRarities.Properties
{
	internal static class BuildInfo
	{
		public const string Name = "Item Rarities";

		public const string Version = "2.0.5";

		public const string Author = "Deadman";

		public const string DownloadLink = "https://github.com/Deaadman/item-rarities/releases/latest/download/ItemRarities.dll";

		public const int Priority = 0;

		public const string MelonLoaderVersion = "0.7.2-ci.2388";
	}
	internal class Settings : JsonModSettings
	{
		[Name("自定义颜色")]
		[Description("启用该功能后，您可以自定义每种稀有度的颜色")]
		public bool customColours;

		[Section("Lv1")]
		[Name("红色")]
		public float commonRed = 0.6f;

		[Name("绿色")]
		public float commonGreen = 0.6f;

		[Name("蓝色")]
		public float commonBlue = 0.6f;

		[Section("Lv2")]
		[Name("红色")]
		public float uncommonRed = 0.3f;

		[Name("绿色")]
		public float uncommonGreen = 0.7f;

		[Name("蓝色")]
		public float uncommonBlue;

		[Section("Lv3")]
		[Name("红色")]
		public float rareRed;

		[Name("绿色")]
		public float rareGreen = 0.6f;

		[Name("蓝色")]
		public float rareBlue = 0.9f;

		[Section("Lv4")]
		[Name("红色")]
		public float epicRed = 0.7f;

		[Name("绿色")]
		public float epicGreen = 0.3f;

		[Name("蓝色")]
		public float epicBlue = 0.9f;

		[Section("Lv5")]
		[Name("红色")]
		public float legendaryRed = 0.9f;

		[Name("绿色")]
		public float legendaryGreen = 0.5f;

		[Name("蓝色")]
		public float legendaryBlue = 0.2f;

		[Section("Lv6")]
		[Name("红色")]
		public float mythicRed = 0.8f;

		[Name("绿色")]
		public float mythicGreen = 0.7f;

		[Name("蓝色")]
		public float mythicBlue = 0.3f;

		internal static Settings Instance { get; }

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			RefreshFields();
		}

		internal static void OnLoad()
		{
			Instance.AddToModSettings("物品品阶分类v2.0.5");
			Instance.RefreshFields();
			Instance.RefreshGUI();
		}

		private void RefreshFields()
		{
			SettingsUtilities.UpdateFieldVisibilities(customColours);
		}

		static Settings()
		{
			Instance = new Settings();
		}
	}
}
