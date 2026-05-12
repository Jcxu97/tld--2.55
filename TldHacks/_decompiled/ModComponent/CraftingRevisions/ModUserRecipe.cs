using System;
using System.Collections.Generic;
using System.Text;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppTLD.Cooking;
using Il2CppTLD.Gear;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CraftingRevisions;

internal sealed class ModUserRecipe
{
	public string? RecipeName { get; set; }

	public string? RecipeShortName { get; set; }

	public string? RecipeDescription { get; set; }

	public int RequiredSkillLevel { get; set; } = 1;


	public List<string> AllowedCookingPots { get; set; } = new List<string>();


	public ModUserRecipeBlueprintData BlueprintData { get; set; } = new ModUserRecipeBlueprintData();


	private GearItemData[] GetAllowedCookingPots()
	{
		List<GearItemData> list = new List<GearItemData>();
		foreach (string allowedCookingPot in AllowedCookingPots)
		{
			if (allowedCookingPot == "GEAR_RecycledCan" || allowedCookingPot == null)
			{
				list.Add(null);
				continue;
			}
			GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(allowedCookingPot)).WaitForCompletion();
			if ((Object)(object)val != (Object)null)
			{
				GearItem component = val.GetComponent<GearItem>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.GearItemData != (Object)null)
				{
					list.Add(component.GearItemData);
				}
			}
		}
		Il2CppReferenceArray<GearItemData> val2 = new Il2CppReferenceArray<GearItemData>((long)list.Count);
		int num = 0;
		foreach (GearItemData item in list)
		{
			((Il2CppArrayBase<GearItemData>)(object)val2)[num] = item;
			num++;
		}
		return Il2CppArrayBase<GearItemData>.op_Implicit((Il2CppArrayBase<GearItemData>)(object)val2);
	}

	public RecipeData GetRecipeData()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		RecipeData obj = ScriptableObject.CreateInstance<RecipeData>();
		((Object)obj).name = "MODRECIPE_" + RecipeName;
		obj.m_RecipeName = new LocalizedString
		{
			m_LocalizationID = RecipeName
		};
		obj.m_RecipeShortName = new LocalizedString
		{
			m_LocalizationID = RecipeShortName
		};
		obj.m_RecipeDescription = new LocalizedString
		{
			m_LocalizationID = RecipeDescription
		};
		obj.m_UnlockRule = (UnlockType)0;
		obj.m_RequiredSkillLevel = RequiredSkillLevel;
		obj.m_AllowedCookingPots = Il2CppReferenceArray<GearItemData>.op_Implicit(GetAllowedCookingPots());
		obj.m_DishBlueprint = GetBlueprintData();
		obj.m_RecipeIcon = obj.m_DishBlueprint.m_CraftingIcon;
		return obj;
	}

	private BlueprintData GetBlueprintData()
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		ModUserRecipeBlueprintData blueprintData = BlueprintData;
		BlueprintData val = ScriptableObject.CreateInstance<BlueprintData>();
		GameObject obj = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(blueprintData.CraftedResult)).WaitForCompletion();
		GearItem component = obj.GetComponent<GearItem>();
		Cookable component2 = obj.GetComponent<Cookable>();
		((Object)val).name = "MOD_BLUEPRINT_" + RecipeName;
		val.m_RequiredGear = Il2CppReferenceArray<RequiredGearItem>.op_Implicit(Utils.GetRequiredGearItems(BlueprintData.RequiredGear));
		val.m_RequiredPowder = Il2CppReferenceArray<RequiredPowder>.op_Implicit(Utils.GetRequiredPowder(BlueprintData.RequiredPowder));
		val.m_RequiredLiquid = Il2CppReferenceArray<RequiredLiquid>.op_Implicit(Utils.GetRequiredLiquid(BlueprintData.RequiredLiquid));
		val.m_CraftingResultType = (CraftingResult)0;
		val.m_CraftedResultGear = component;
		val.m_CraftedResultCount = blueprintData.CraftedResultCount;
		val.m_DurationMinutes = blueprintData.DurationMinutes;
		val.m_CraftingAudio = Utils.MakeAudioEvent(blueprintData.CraftingAudio);
		val.m_RequiresLight = false;
		val.m_RequiresLitFire = true;
		val.m_RequiredCraftingLocation = (CraftingLocation)0;
		val.m_AppliedSkill = (SkillType)3;
		val.m_ImprovedSkill = (SkillType)3;
		if (Object.op_Implicit((Object)(object)component2))
		{
			val.m_CraftingIcon = new AssetReferenceTexture2D(((Object)component2.m_CookedPrefab).name.Replace("GEAR_", "ico_GearItem__"));
		}
		val.m_CanIncreaseRepairSkill = false;
		return val;
	}

	public static ModUserRecipe ParseFromJson(string jsonText)
	{
		ModUserRecipe obj = JsonConvert.DeserializeObject<ModUserRecipe>(jsonText) ?? throw new ArgumentException("Could not parse recipe data from the text.", "jsonText");
		obj.RecipeShortName = obj.RecipeName;
		return obj;
	}

	internal bool Validate()
	{
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		if (string.IsNullOrWhiteSpace(RecipeName))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder2);
			handler.AppendLiteral("RecipeName must be set on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder3.AppendLine(ref handler);
		}
		if (RequiredSkillLevel < 1)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(46, 1, stringBuilder2);
			handler.AppendLiteral("RequiredSkillLevel cannot be less than 1 on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder4.AppendLine(ref handler);
		}
		if (string.IsNullOrWhiteSpace(BlueprintData.CraftedResult))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(31, 1, stringBuilder2);
			handler.AppendLiteral("CraftedResult must be set on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder5.AppendLine(ref handler);
		}
		if (BlueprintData.CraftedResult != null && (Object)(object)Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(BlueprintData.CraftedResult)).WaitForCompletion().GetComponent<GearItem>() == (Object)null)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(46, 2, stringBuilder2);
			handler.AppendLiteral("CraftedResult (");
			handler.AppendFormatted(BlueprintData.CraftedResult);
			handler.AppendLiteral(") is not a valid GearItem on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder6.AppendLine(ref handler);
		}
		if (BlueprintData.CraftedResultCount < 1)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(46, 1, stringBuilder2);
			handler.AppendLiteral("CraftedResultCount cannot be less than 1 on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder7.AppendLine(ref handler);
		}
		if (BlueprintData.DurationMinutes < 0)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(40, 1, stringBuilder2);
			handler.AppendLiteral("DurationMinutes cannot be negative on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder8.AppendLine(ref handler);
		}
		if ((BlueprintData.RequiredGear == null && BlueprintData.RequiredLiquid == null && BlueprintData.RequiredPowder == null) || (BlueprintData.RequiredGear != null && BlueprintData.RequiredGear.Count == 0 && BlueprintData.RequiredLiquid != null && BlueprintData.RequiredLiquid.Count == 0 && BlueprintData.RequiredPowder != null && BlueprintData.RequiredPowder.Count == 0))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(71, 1, stringBuilder2);
			handler.AppendLiteral("One of RequiredGear/RequiredLiquid/RequiredPowder must be defined on '");
			handler.AppendFormatted(RecipeName);
			handler.AppendLiteral("'");
			stringBuilder9.AppendLine(ref handler);
		}
		if (BlueprintData.RequiredGear != null)
		{
			int num = 0;
			foreach (ModRequiredGearItem item in BlueprintData.RequiredGear)
			{
				if (item.Item != null && (Object)(object)Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(item.Item)).WaitForCompletion().GetComponent<GearItem>() == (Object)null)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder10 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(56, 3, stringBuilder2);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Item (");
					handler.AppendFormatted(item.Item);
					handler.AppendLiteral(") is not a valid GearItem on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder10.AppendLine(ref handler);
				}
				if (item.Item == null)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder11 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(41, 2, stringBuilder2);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Item must be set on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder11.AppendLine(ref handler);
				}
				if (item.Count == 0 && item.Quantity == 0f)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder12 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(77, 3, stringBuilder2);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Count or RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Quantity must be defined on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder12.AppendLine(ref handler);
				}
				if (item.Count > 0 && item.Quantity > 0f)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder13 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(98, 3, stringBuilder2);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Count and RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Quantity are both > 0, only one must be used on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder13.AppendLine(ref handler);
				}
				if (item.Quantity > 0f && (int)item.Units == 0)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder14 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(105, 3, stringBuilder2);
					handler.AppendLiteral("RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Quantity is > 0  but RequiredGearItem[");
					handler.AppendFormatted(num);
					handler.AppendLiteral("].Units is set to Count, is this intended? on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder14.AppendLine(ref handler);
				}
				num++;
			}
		}
		if (BlueprintData.RequiredLiquid != null && BlueprintData.RequiredLiquid.Count > 0)
		{
			int num2 = 0;
			foreach (ModRequiredLiquid item2 in BlueprintData.RequiredLiquid)
			{
				if (item2.Liquid != null && (Object)(object)Addressables.LoadAssetAsync<LiquidType>(Object.op_Implicit(item2.Liquid)).WaitForCompletion() == (Object)null)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder15 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(62, 3, stringBuilder2);
					handler.AppendLiteral("RequiredLiquidItem[");
					handler.AppendFormatted(num2);
					handler.AppendLiteral("].Liquid (");
					handler.AppendFormatted(item2.Liquid);
					handler.AppendLiteral(") is not a valid LiquidType on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder15.AppendLine(ref handler);
				}
				if (item2.Liquid == null)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder16 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(45, 2, stringBuilder2);
					handler.AppendLiteral("RequiredLiquidItem[");
					handler.AppendFormatted(num2);
					handler.AppendLiteral("].Liquid must be set on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder16.AppendLine(ref handler);
				}
				if (item2.VolumeInLitres < 0f)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder17 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(63, 2, stringBuilder2);
					handler.AppendLiteral("RequiredLiquidItem[");
					handler.AppendFormatted(num2);
					handler.AppendLiteral("].VolumeInLitres cannot be less than 0 on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder17.AppendLine(ref handler);
				}
				num2++;
			}
		}
		if (BlueprintData.RequiredPowder != null && BlueprintData.RequiredPowder.Count > 0)
		{
			int num3 = 0;
			foreach (ModRequiredPowder item3 in BlueprintData.RequiredPowder)
			{
				if (item3.Powder != null && (Object)(object)Addressables.LoadAssetAsync<PowderType>(Object.op_Implicit(item3.Powder)).WaitForCompletion() == (Object)null)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder18 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(62, 3, stringBuilder2);
					handler.AppendLiteral("RequiredPowderItem[");
					handler.AppendFormatted(num3);
					handler.AppendLiteral("].Powder (");
					handler.AppendFormatted(item3.Powder);
					handler.AppendLiteral(") is not a valid PowderType on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder18.AppendLine(ref handler);
				}
				if (item3.Powder == null)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder19 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(45, 2, stringBuilder2);
					handler.AppendLiteral("RequiredPowderItem[");
					handler.AppendFormatted(num3);
					handler.AppendLiteral("].Powder must be set on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder19.AppendLine(ref handler);
				}
				if (item3.QuantityInKilograms < 0f)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder20 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(68, 2, stringBuilder2);
					handler.AppendLiteral("RequiredPowderItem[");
					handler.AppendFormatted(num3);
					handler.AppendLiteral("].QuantityInKilograms cannot be less than 0 on '");
					handler.AppendFormatted(RecipeName);
					handler.AppendLiteral("'");
					stringBuilder20.AppendLine(ref handler);
				}
				num3++;
			}
		}
		if (stringBuilder.Length > 0)
		{
			Logger.LogError("\n" + stringBuilder.ToString().Trim());
			return false;
		}
		return true;
	}
}
