using System;
using System.Collections.Generic;
using System.Text;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CraftingRevisions;

internal sealed class ModUserBlueprintData
{
	public string? Name { get; set; }

	public List<ModRequiredGearItem> RequiredGear { get; set; } = new List<ModRequiredGearItem>();


	public List<ModRequiredPowder>? RequiredPowder { get; set; } = new List<ModRequiredPowder>();


	public List<ModRequiredLiquid>? RequiredLiquid { get; set; } = new List<ModRequiredLiquid>();


	public string? RequiredTool { get; set; }

	public List<string>? OptionalTools { get; set; } = new List<string>();


	public string? CraftedResult { get; set; }

	public int CraftedResultCount { get; set; }

	public string CraftingIcon => CraftedResult.Replace("GEAR_", "ico_CraftItem__");

	public int DurationMinutes { get; set; }

	public string? CraftingAudio { get; set; }

	public float? KeroseneLitersRequired { get; set; }

	public float? GunpowderKGRequired { get; set; }

	public bool RequiresLight { get; set; }

	public bool Locked { get; set; }

	public bool IgnoreLockInSurvival { get; set; } = true;


	public bool AppearsInStoryOnly { get; set; }

	public bool AppearsInSurvivalOnly { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public SkillType AppliedSkill { get; set; } = (SkillType)(-1);


	[JsonConverter(typeof(StringEnumConverter))]
	public SkillType ImprovedSkill { get; set; } = (SkillType)(-1);


	[JsonConverter(typeof(StringEnumConverter))]
	public CraftingLocation RequiredCraftingLocation { get; set; }

	public bool RequiresLitFire { get; set; }

	public bool CanIncreaseRepairSkill { get; set; }

	public static ModUserBlueprintData ParseFromJson(string jsonText)
	{
		return JsonConvert.DeserializeObject<ModUserBlueprintData>(jsonText) ?? throw new ArgumentException("Could not parse blueprint data from the text.", "jsonText");
	}

	internal bool Validate()
	{
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		if (string.IsNullOrWhiteSpace(CraftedResult))
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(31, 1, stringBuilder3);
			handler.AppendLiteral("CraftedResult must be set on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder4.AppendLine(ref handler);
		}
		if (CraftedResult != null && (Object)(object)Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(CraftedResult)).WaitForCompletion().GetComponent<GearItem>() == (Object)null)
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(46, 2, stringBuilder3);
			handler.AppendLiteral("CraftedResult (");
			handler.AppendFormatted(CraftedResult);
			handler.AppendLiteral(") is not a valid GearItem on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder5.AppendLine(ref handler);
		}
		if (CraftedResultCount < 1)
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(46, 1, stringBuilder3);
			handler.AppendLiteral("CraftedResultCount cannot be less than 1 on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder6.AppendLine(ref handler);
		}
		if (DurationMinutes < 0)
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(40, 1, stringBuilder3);
			handler.AppendLiteral("DurationMinutes cannot be negative on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder7.AppendLine(ref handler);
		}
		if (!Utils.EnumValues<CraftingLocation>.Contains(RequiredCraftingLocation))
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(53, 2, stringBuilder3);
			handler.AppendLiteral("Unsupported value ");
			handler.AppendFormatted<CraftingLocation>(RequiredCraftingLocation);
			handler.AppendLiteral(" for RequiredCraftingLocation on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder8.AppendLine(ref handler);
		}
		if (!Utils.EnumValues<SkillType>.Contains(AppliedSkill))
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(41, 2, stringBuilder3);
			handler.AppendLiteral("Unsupported value ");
			handler.AppendFormatted<SkillType>(AppliedSkill);
			handler.AppendLiteral(" for AppliedSkill on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder9.AppendLine(ref handler);
		}
		if (!Utils.EnumValues<SkillType>.Contains(ImprovedSkill))
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(53, 2, stringBuilder3);
			handler.AppendLiteral("Unsupported value ");
			handler.AppendFormatted<SkillType>(ImprovedSkill);
			handler.AppendLiteral(" for RequiredCraftingLocation on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder10.AppendLine(ref handler);
		}
		if ((RequiredGear == null && RequiredLiquid == null && RequiredPowder == null) || (RequiredGear != null && RequiredGear.Count == 0 && RequiredLiquid != null && RequiredLiquid.Count == 0 && RequiredPowder != null && RequiredPowder.Count == 0))
		{
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(56, 1, stringBuilder3);
			handler.AppendLiteral("One of RequiredGear/RequiredPowder must be defined on '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder11.AppendLine(ref handler);
		}
		if (RequiredGear != null && RequiredGear.Count > 0)
		{
			int num = 0;
			foreach (ModRequiredGearItem item in RequiredGear)
			{
				if (item.Item != null)
				{
					GameObject obj = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(item.Item)).WaitForCompletion();
					if ((Object)(object)obj == (Object)null)
					{
						StringBuilder stringBuilder3 = stringBuilder;
						StringBuilder stringBuilder12 = stringBuilder3;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(51, 3, stringBuilder3);
						handler.AppendLiteral("RequiredGearItem[");
						handler.AppendFormatted(num);
						handler.AppendLiteral("].Item (");
						handler.AppendFormatted(item.Item);
						handler.AppendLiteral(") unknown GameObject on '");
						handler.AppendFormatted(Name);
						handler.AppendLiteral("'");
						stringBuilder12.AppendLine(ref handler);
					}
					if ((Object)(object)obj.GetComponent<GearItem>() == (Object)null)
					{
						StringBuilder stringBuilder3 = stringBuilder;
						StringBuilder stringBuilder13 = stringBuilder3;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(69, 3, stringBuilder3);
						handler.AppendLiteral("RequiredGearItem[");
						handler.AppendFormatted(num);
						handler.AppendLiteral("].Item (");
						handler.AppendFormatted(item.Item);
						handler.AppendLiteral(") GameObject has no GearItem component on '");
						handler.AppendFormatted(Name);
						handler.AppendLiteral("'");
						stringBuilder13.AppendLine(ref handler);
					}
				}
				if (item.Item == null)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder14 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(41, 2, stringBuilder3);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Item must be set on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder14.AppendLine(ref handler);
				}
				if (item.Count < 1)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder15 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(52, 2, stringBuilder3);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Count cannot be less than 1 on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder15.AppendLine(ref handler);
				}
				num++;
			}
		}
		if (RequiredLiquid != null && RequiredLiquid.Count > 0)
		{
			int num2 = 0;
			foreach (ModRequiredLiquid item2 in RequiredLiquid)
			{
				if (item2.Liquid != null && (Object)(object)Addressables.LoadAssetAsync<LiquidType>(Object.op_Implicit(item2.Liquid)).WaitForCompletion() == (Object)null)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder16 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(55, 3, stringBuilder3);
					handler.AppendLiteral("RequiredLiquidItem[");
					handler.AppendFormatted(num2);
					handler.AppendLiteral("].Liquid (");
					handler.AppendFormatted(item2.Liquid);
					handler.AppendLiteral(") unknown LiquidType on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder16.AppendLine(ref handler);
				}
				if (item2.Liquid == null)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder17 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(45, 2, stringBuilder3);
					handler.AppendLiteral("RequiredLiquidItem[");
					handler.AppendFormatted(num2);
					handler.AppendLiteral("].Liquid must be set on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder17.AppendLine(ref handler);
				}
				if (item2.VolumeInLitres < 0f)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder18 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(63, 2, stringBuilder3);
					handler.AppendLiteral("RequiredLiquidItem[");
					handler.AppendFormatted(num2);
					handler.AppendLiteral("].VolumeInLitres cannot be less than 0 on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder18.AppendLine(ref handler);
				}
				num2++;
			}
		}
		if (RequiredPowder != null && RequiredPowder.Count > 0)
		{
			int num3 = 0;
			foreach (ModRequiredPowder item3 in RequiredPowder)
			{
				if (item3.Powder != null && (Object)(object)Addressables.LoadAssetAsync<PowderType>(Object.op_Implicit(item3.Powder)).WaitForCompletion() == (Object)null)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder19 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(55, 3, stringBuilder3);
					handler.AppendLiteral("RequiredPowderItem[");
					handler.AppendFormatted(num3);
					handler.AppendLiteral("].Powder (");
					handler.AppendFormatted(item3.Powder);
					handler.AppendLiteral(") unknown PowderType on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder19.AppendLine(ref handler);
				}
				if (item3.Powder == null)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder20 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(45, 2, stringBuilder3);
					handler.AppendLiteral("RequiredPowderItem[");
					handler.AppendFormatted(num3);
					handler.AppendLiteral("].Powder must be set on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder20.AppendLine(ref handler);
				}
				if (item3.QuantityInKilograms < 0f)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder stringBuilder21 = stringBuilder3;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(68, 2, stringBuilder3);
					handler.AppendLiteral("RequiredPowderItem[");
					handler.AppendFormatted(num3);
					handler.AppendLiteral("].QuantityInKilograms cannot be less than 0 on '");
					handler.AppendFormatted(Name);
					handler.AppendLiteral("'");
					stringBuilder21.AppendLine(ref handler);
				}
				num3++;
			}
		}
		if (KeroseneLitersRequired.HasValue && KeroseneLitersRequired > 0f)
		{
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder stringBuilder22 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(80, 1, stringBuilder3);
			handler.AppendLiteral("KeroseneLitersRequired IS deprecated, please use LiquidRequired in blueprints '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder22.AppendLine(ref handler);
		}
		if (GunpowderKGRequired.HasValue && GunpowderKGRequired > 0f)
		{
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder stringBuilder23 = stringBuilder3;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(77, 1, stringBuilder3);
			handler.AppendLiteral("GunpowderKGRequired IS deprecated, please use PowderRequired in blueprints '");
			handler.AppendFormatted(Name);
			handler.AppendLiteral("'");
			stringBuilder23.AppendLine(ref handler);
		}
		if (stringBuilder2.Length > 0)
		{
			MelonLogger.Warning("\n" + stringBuilder2.ToString().Trim());
		}
		if (stringBuilder.Length > 0)
		{
			MelonLogger.Error("\n" + stringBuilder.ToString().Trim());
			return false;
		}
		return true;
	}

	internal BlueprintData GetBlueprintData()
	{
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		if (RequiredPowder.Count == 0 && GunpowderKGRequired.HasValue && GunpowderKGRequired > 0f)
		{
			RequiredPowder.Add(new ModRequiredPowder
			{
				Powder = "POWDER_Gunpowder",
				QuantityInKilograms = GunpowderKGRequired.GetValueOrDefault()
			});
		}
		if (RequiredLiquid.Count == 0 && KeroseneLitersRequired.HasValue && KeroseneLitersRequired > 0f)
		{
			RequiredLiquid.Add(new ModRequiredLiquid
			{
				Liquid = "LIQUID_Kerosene",
				VolumeInLitres = KeroseneLitersRequired.GetValueOrDefault()
			});
		}
		BlueprintData val = ScriptableObject.CreateInstance<BlueprintData>();
		((Object)val).name = "BP_" + Name;
		val.m_XPModesToDisable = new List<ExperienceModeType>();
		val.m_UsesPhoto = false;
		val.m_RequiredGear = Il2CppReferenceArray<RequiredGearItem>.op_Implicit(Utils.GetRequiredGearItems(RequiredGear));
		val.m_RequiredPowder = Il2CppReferenceArray<RequiredPowder>.op_Implicit(Utils.GetRequiredPowder(RequiredPowder));
		val.m_RequiredLiquid = Il2CppReferenceArray<RequiredLiquid>.op_Implicit(Utils.GetRequiredLiquid(RequiredLiquid));
		val.m_CraftingResultType = (CraftingResult)0;
		val.m_CraftedResultGear = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(CraftedResult)).WaitForCompletion().GetComponent<GearItem>();
		val.m_CraftedResultCount = CraftedResultCount;
		val.m_DurationMinutes = DurationMinutes;
		val.m_CraftingAudio = Utils.MakeAudioEvent(CraftingAudio);
		val.m_CraftingIcon = new AssetReferenceTexture2D(CraftingIcon);
		val.m_RequiresLight = RequiresLight;
		val.m_RequiresLitFire = RequiresLitFire;
		val.m_RequiredCraftingLocation = Utils.TranslateEnumValue<CraftingLocation, CraftingLocation>(RequiredCraftingLocation);
		val.m_AppliedSkill = Utils.TranslateEnumValue<SkillType, SkillType>(AppliedSkill);
		val.m_ImprovedSkill = Utils.TranslateEnumValue<SkillType, SkillType>(ImprovedSkill);
		if (!string.IsNullOrEmpty(RequiredTool))
		{
			val.m_RequiredTool = Utils.GetToolsItem(RequiredTool);
		}
		val.m_OptionalTools = Il2CppReferenceArray<ToolsItem>.op_Implicit(Utils.GetToolsItems(OptionalTools));
		val.m_Locked = false;
		val.m_AppearsInStoryOnly = false;
		val.m_CanIncreaseRepairSkill = false;
		return val;
	}
}
