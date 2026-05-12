using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using CraftAnywhereRedux;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("CraftAnywhereRedux")]
[assembly: AssemblyCopyright("moosemeat")]
[assembly: AssemblyFileVersion("1.3.0")]
[assembly: MelonInfo(typeof(CraftAnywhereReduxMain), "CraftAnywhereRedux", "1.3.0", "moosemeat", null)]
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
namespace CraftAnywhereRedux
{
	public class CraftAnywhereReduxMain : MelonMod
	{
		public override void OnInitializeMelon()
		{
			MelonLogger.Msg("CraftAnywhere is online.");
			Settings.OnLoad();
		}
	}
	internal static class Patches
	{
		[HarmonyPatch(typeof(Panel_Crafting), "ItemPassesFilter")]
		private static class ChangeCraftingLocation
		{
			internal static void Postfix(BlueprintData bpi)
			{
				object obj;
				if (bpi == null)
				{
					obj = null;
				}
				else
				{
					GearItem craftedResultGear = bpi.m_CraftedResultGear;
					obj = ((craftedResultGear == null) ? null : ((Object)craftedResultGear).name?.Substring(5));
				}
				switch ((string)obj)
				{
				case "ArrowShaft":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.arrowshaftLocationIndex;
					break;
				case "ArrowHead":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.arrowheadLocationIndex;
					break;
				case "BearSkinBedRoll":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.bearskinbedrollLocationIndex;
					break;
				case "Bullet":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.bulletLocationIndex;
					break;
				case "Bow_Bushcraft":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.bushcraftbowLocationIndex;
					break;
				case "GunpowderCan":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.gunpowderLocationIndex;
					break;
				case "CougarClawKnife":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.cougarclawknifeLocationIndex;
					break;
				case "ArrowHardened":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.firehardenedarrowLocationIndex;
					break;
				case "FishingLureD":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.fishinglurewiresLocationIndex;
					break;
				case "FishingLureC":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.fishinglureacornsLocationIndex;
					break;
				case "FishingLureB":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.fishinglurecasingLocationIndex;
					break;
				case "TipUp":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.fishingtipupLocationIndex;
					break;
				case "Hook":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.hookLocationIndex;
					break;
				case "HatchetImprovised":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvisedhatchetLocationIndex;
					break;
				case "KnifeImprovised":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvisedknifeLocationIndex;
					break;
				case "Line":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.lineLocationIndex;
					break;
				case "NoiseMaker_A":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.noisemakerLocationIndex;
					break;
				case "RevolverAmmoSingle":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.revolvercartridgeLocationIndex;
					break;
				case "RifleAmmoSingle":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.riflecartridgeLocationIndex;
					break;
				case "Arrow":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.simplearrowLocationIndex;
					break;
				case "FishingLureA":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.simplefishinglureLocationIndex;
					break;
				case "Snare":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.snareLocationIndex;
					break;
				case "Bow":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.survivalbowLocationIndex;
					break;
				case "Travois":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.travoisLocationIndex;
					break;
				case "BearSkinCoat":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.bearskincoatLocationIndex;
					break;
				case "DeerSkinBoots":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.deerskinbootsLocationIndex;
					break;
				case "DeerSkinPants":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.deerskinpantsLocationIndex;
					break;
				case "ImprovisedCrampons":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvisedcramponsLocationIndex;
					break;
				case "MooseHideCloak":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.moosehidecloakLocationIndex;
					break;
				case "MooseHideBag":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.moosehidesatchelLocationIndex;
					break;
				case "RabbitskinHat":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.rabbitskinhatLocationIndex;
					break;
				case "RabbitSkinMittens":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.rabbitskinmittsLocationIndex;
					break;
				case "WolfSkinCape":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.wolfskincoatLocationIndex;
					break;
				case "WolfSkinHat":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.wolfskinhatLocationIndex;
					break;
				case "WolfSkinPant":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.wolfskinpantsLocationIndex;
					break;
				case "WolfScarf":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.wolfscarfLocationIndex;
					break;
				case "JacketLeatherFlightA":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.jacketleatherLocationIndex;
					break;
				case "ImprovedDownInsulation":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improveddownLocationIndex;
					break;
				case "ImprovedJacketLeather":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvedjacketleatherLocationIndex;
					break;
				case "ImprovedLongJohns":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvedlongjohnsLocationIndex;
					break;
				case "ImprovisedFlask":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvisedflaskLocationIndex;
					break;
				case "BearskinLeggings_MOD":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.bearskinleggingsLocationIndex;
					break;
				case "DeerskinCoat_MOD":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.deerskincoatLocationIndex;
					break;
				case "DeerskinGloves_MOD":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.deerskinglovesLocationIndex;
					break;
				case "WolfskinBoots_MOD":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.wolfskinbootsLocationIndex;
					break;
				case "WolfskinCap_MOD":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.wolfskincapLocationIndex;
					break;
				case "EmptyShellCasing":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.emptyshellcasingLocationIndex;
					break;
				case "FlareGunAmmoSingle":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.flaregunammosingleLocationIndex;
					break;
				case "Magnesium":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.magnesiumLocationIndex;
					break;
				case "RifleCleaningKit":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.riflecleaningkitLocationIndex;
					break;
				case "CookingPot":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.cookingpotLocationIndex;
					break;
				case "Firelog":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.firelogLocationIndex;
					break;
				case "Hatchet":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.hatchetLocationIndex;
					break;
				case "Knife":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.knifeLocationIndex;
					break;
				case "ScrapMetal":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.scrapmetalLocationIndex;
					break;
				case "ImprovisedFilter":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.improvisedfilterLocationIndex;
					break;
				case "WoodMatches":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.woodmatchesLocationIndex;
					break;
				case "SewingKit":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.sewingkitLocationIndex;
					break;
				case "Bow_Woodwrights":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.bowwoodwrightsLocationIndex;
					break;
				case "Rope":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.ropeLocationIndex;
					break;
				case "SaltBag":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.saltbagLocationIndex;
					break;
				case "SharpeningStone":
					bpi.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.sharpeningstoneLocationIndex;
					break;
				}
			}
		}
	}
	internal class CraftAnywhereReduxSettings : JsonModSettings
	{
		[Name("显示工具")]
		public bool tools_enabled;

		[Name("     箭杆")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int arrowshaftLocationIndex = 1;

		[Name("     箭头")]
		[Description("默认位置是锻造炉")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int arrowheadLocationIndex = 2;

		[Name("     熊皮睡袋")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int bearskinbedrollLocationIndex = 1;

		[Name("     弹头")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int bulletLocationIndex = 3;

		[Name("     丛林生存弓")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int bushcraftbowLocationIndex = 1;

		[Name("     火药罐")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台" })]
		public int gunpowderLocationIndex = 2;

		[Name("     美洲狮爪刀")]
		[Description("默认位置是锻造炉")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int cougarclawknifeLocationIndex = 2;

		[Name("     淬火箭")]
		[Description("默认位置是火")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int firehardenedarrowLocationIndex = 4;

		[Name("     渔具路亚 - 电线")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int fishinglurewiresLocationIndex = 1;

		[Name("     渔具路亚 - 橡实")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int fishinglureacornsLocationIndex = 1;

		[Name("     渔具路亚 - 左轮手枪弹壳")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int fishinglurecasingLocationIndex = 1;

		[Name("     上钩指示器")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int fishingtipupLocationIndex = 1;

		[Name("     钩子")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "Anywhere", "工作台" })]
		public int hookLocationIndex = 1;

		[Name("     简易小斧")]
		[Description("默认位置是锻造炉")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int improvisedhatchetLocationIndex = 2;

		[Name("     简易小刀")]
		[Description("默认位置是锻造炉")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int improvisedknifeLocationIndex = 2;

		[Name("     鱼线")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int lineLocationIndex = 1;

		[Name("     制噪器")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int noisemakerLocationIndex = 3;

		[Name("     左轮弹药")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int revolvercartridgeLocationIndex = 3;

		[Name("     步枪弹药")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int riflecartridgeLocationIndex = 3;

		[Name("     简单的箭")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int simplearrowLocationIndex = 1;

		[Name("     简单的路亚")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int simplefishinglureLocationIndex = 1;

		[Name("     陷阱")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int snareLocationIndex = 1;

		[Name("     生存弓")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int survivalbowLocationIndex = 1;

		[Name("     旧式雪橇")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int travoisLocationIndex = 1;

		[Name("显示衣物")]
		public bool clothing_enabled;

		[Name("     熊皮大衣")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int bearskincoatLocationIndex = 1;

		[Name("     鹿皮靴")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int deerskinbootsLocationIndex = 1;

		[Name("     鹿皮短裤")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int deerskinpantsLocationIndex = 1;

		[Name("     简易鞋底钉")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int improvisedcramponsLocationIndex = 1;

		[Name("     驼鹿皮斗篷")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int moosehidecloakLocationIndex = 1;

		[Name("     驼鹿皮背包")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int moosehidesatchelLocationIndex = 1;

		[Name("     兔皮帽")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int rabbitskinhatLocationIndex = 1;

		[Name("     兔皮连指手套")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int rabbitskinmittsLocationIndex = 1;

		[Name("     狼皮大衣")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int wolfskincoatLocationIndex = 1;

		[Name("     狼皮帽")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int wolfskinhatLocationIndex = 1;

		[Name("     狼皮裤")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int wolfskinpantsLocationIndex = 1;

		[Name("显示围巾")]
		public bool wolfscarf_enabled;

		[Name("     狼皮围巾")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int wolfscarfLocationIndex = 1;

		[Name("显示制皮工艺")]
		public bool leatherworks_enabled;

		[Name("     飞行夹克")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int jacketleatherLocationIndex = 1;

		[Name("     防水贴身衣物")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int improveddownLocationIndex = 1;

		[Name("     Improved Flight Jacket(不知道是哪个模组故不做翻译)")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int improvedjacketleatherLocationIndex = 1;

		[Name("     Improved Longjohns(不知道是哪个模组故不做翻译)")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int improvedlongjohnsLocationIndex = 1;

		[Name("     Improvised Flask(不知道是哪个模组故不做翻译)")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench", "Forge" })]
		public int improvisedflaskLocationIndex = 1;

		[Name("显示北方佬服装")]
		public bool northfolk_enabled;

		[Name("     熊皮紧身裤")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int bearskinleggingsLocationIndex = 1;

		[Name("     鹿皮大衣")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int deerskincoatLocationIndex = 1;

		[Name("     鹿皮手套")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int deerskinglovesLocationIndex = 1;

		[Name("     狼皮靴")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int wolfskinbootsLocationIndex = 1;

		[Name("     狼王皮帽")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "任意地点", "工作台" })]
		public int wolfskincapLocationIndex = 1;

		[Name("显示弹药工具")]
		public bool ammotools_enabled;

		[Name("     空弹壳")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int emptyshellcasingLocationIndex = 3;

		[Name("     信号枪弹药")]
		[Description("默认位置是弹药工作台")]
		[Choice(new string[] { "任意地点", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int flaregunammosingleLocationIndex = 3;

		[Name("     Magnesium(不知道是什么故不做翻译)")]
		[Description("The default location is at the Ammo Workbench.")]
		[Choice(new string[] { "任意地点", "Workbench", "Forge", "Ammo Workbench", "Fire" })]
		public int magnesiumLocationIndex = 3;

		[Name("     枪械清理套件")]
		[Description("默认位置是工作台")]
		[Choice(new string[] { "Anywhere", "工作台", "锻造炉", "弹药工作台", "火" })]
		public int riflecleaningkitLocationIndex = 1;

		[Name("显示罐头加工厂(已删除该模组不做翻译)")]
		public bool cannery_enabled;

		[Name("     Cooking Pot")]
		[Description("The default location is at the Forge.")]
		[Choice(new string[] { "Anywhere", "Workbench", "Forge", "Ammo Workbench", "Fire" })]
		public int cookingpotLocationIndex = 2;

		[Name("     Firelog")]
		[Description("The default location is at the Ammo Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int firelogLocationIndex = 1;

		[Name("     Hatchet")]
		[Description("The default location is at the Ammo Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench", "Forge", "Ammo Workbench", "Fire" })]
		public int hatchetLocationIndex = 3;

		[Name("     Knife")]
		[Description("The default location is at the Ammo Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench", "Forge", "Ammo Workbench", "Fire" })]
		public int knifeLocationIndex = 3;

		[Name("     Scrap Metal")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench", "Forge", "Ammo Workbench", "Fire" })]
		public int scrapmetalLocationIndex = 2;

		[Name("Show Dead Air")]
		public bool deadair_enabled;

		[Name("     Improvised Filter")]
		[Description("The default location is at the Forge.")]
		[Choice(new string[] { "Anywhere", "Workbench", "Forge", "Ammo Workbench", "Fire" })]
		public int improvisedfilterLocationIndex = 2;

		[Name("未安装该模组故不做翻译")]
		public bool usefulblueprints_enabled;

		[Name("     Wood Matches")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int woodmatchesLocationIndex = 1;

		[Name("     Sewing Kit")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int sewingkitLocationIndex = 1;

		[Name("     Woodwright's Bow")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int bowwoodwrightsLocationIndex = 1;

		[Name("     Rope")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int ropeLocationIndex = 1;

		[Name("     Salt Bag")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int saltbagLocationIndex = 1;

		[Name("     Whetstone")]
		[Description("The default location is at the Workbench.")]
		[Choice(new string[] { "Anywhere", "Workbench" })]
		public int sharpeningstoneLocationIndex = 1;

		protected void SetToolsVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("arrowshaftLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("arrowheadLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("bearskinbedrollLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("bulletLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("bushcraftbowLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("gunpowderLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("cougarclawknifeLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("firehardenedarrowLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("fishinglurewiresLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("fishinglureacornsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("fishinglurecasingLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("fishingtipupLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("hookLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improvisedhatchetLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improvisedknifeLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("lineLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("noisemakerLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("revolvercartridgeLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("riflecartridgeLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("simplearrowLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("simplefishinglureLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("snareLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("survivalbowLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("travoisLocationIndex"), visible);
		}

		protected void SetClothingVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("bearskincoatLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("deerskinbootsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("deerskinpantsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improvisedcramponsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("moosehidecloakLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("moosehidesatchelLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("rabbitskinhatLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("rabbitskinmittsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("wolfskincoatLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("wolfskinhatLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("wolfskinpantsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("wolfscarfLocationIndex"), visible);
		}

		protected void SetWolfscarfVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("wolfscarfLocationIndex"), visible);
		}

		protected void SetLeatherworksVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("jacketleatherLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improveddownLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improvedjacketleatherLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improvedlongjohnsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("improvisedflaskLocationIndex"), visible);
		}

		protected void SetNorthfolkVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("bearskinleggingsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("deerskincoatLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("deerskinglovesLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("wolfskinbootsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("wolfskincapLocationIndex"), visible);
		}

		protected void SetAmmoToolsVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("emptyshellcasingLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("flaregunammosingleLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("magnesiumLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("riflecleaningkitLocationIndex"), visible);
		}

		protected void SetCanneryVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("cookingpotLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("firelogLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("hatchetLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("knifeLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("scrapmetalLocationIndex"), visible);
		}

		protected void SetDeadAirVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("improvisedfilterLocationIndex"), visible);
		}

		protected void SetUsefulBlueprintsVisibility(bool visible)
		{
			SetFieldVisible(GetType().GetField("woodmatchesLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("sewingkitLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("bowwoodwrightsLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("ropeLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("saltbagLocationIndex"), visible);
			SetFieldVisible(GetType().GetField("sharpeningstoneLocationIndex"), visible);
		}

		internal void UpdateVisibility()
		{
			SetToolsVisibility(tools_enabled);
			SetClothingVisibility(clothing_enabled);
			SetWolfscarfVisibility(wolfscarf_enabled);
			SetLeatherworksVisibility(leatherworks_enabled);
			SetNorthfolkVisibility(northfolk_enabled);
			SetAmmoToolsVisibility(ammotools_enabled);
			SetCanneryVisibility(cannery_enabled);
			SetDeadAirVisibility(deadair_enabled);
			SetUsefulBlueprintsVisibility(usefulblueprints_enabled);
		}

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			RefreshGUI();
			base.OnChange(field, oldValue, newValue);
			if (field.Name == "tools_enabled")
			{
				SetToolsVisibility((bool)newValue);
			}
			else if (field.Name == "clothing_enabled")
			{
				SetClothingVisibility((bool)newValue);
			}
			else if (field.Name == "wolfscarf_enabled")
			{
				SetWolfscarfVisibility((bool)newValue);
			}
			else if (field.Name == "leatherworks_enabled")
			{
				SetLeatherworksVisibility((bool)newValue);
			}
			else if (field.Name == "northfolk_enabled")
			{
				SetNorthfolkVisibility((bool)newValue);
			}
			else if (field.Name == "ammotools_enabled")
			{
				SetAmmoToolsVisibility((bool)newValue);
			}
			else if (field.Name == "cannery_enabled")
			{
				SetCanneryVisibility((bool)newValue);
			}
			else if (field.Name == "deadair_enabled")
			{
				SetDeadAirVisibility((bool)newValue);
			}
			else if (field.Name == "usefulblueprints_enabled")
			{
				SetUsefulBlueprintsVisibility((bool)newValue);
			}
		}

		protected override void OnConfirm()
		{
			RefreshGUI();
			base.OnConfirm();
		}
	}
	internal static class Settings
	{
		public static CraftAnywhereReduxSettings options;

		public static void OnLoad()
		{
			options = new CraftAnywhereReduxSettings();
			options.AddToModSettings("随意制作重置版v1.3");
			options.UpdateVisibility();
		}

		public static CraftAnywhereReduxSettings Get()
		{
			return options;
		}
	}
}
