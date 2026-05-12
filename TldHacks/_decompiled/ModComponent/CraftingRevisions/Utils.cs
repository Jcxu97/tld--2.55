using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Il2Cpp;
using Il2CppAK;
using Il2CppAK.Wwise;
using Il2CppSystem;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CraftingRevisions;

internal class Utils
{
	internal static class EnumValues<T> where T : struct, Enum
	{
		private static readonly T[] values = Enum.GetValues<T>();

		public static bool Contains(T value)
		{
			return values.Contains(value);
		}
	}

	private static readonly Dictionary<string, uint> eventIds = new Dictionary<string, uint>();

	internal static Event? MakeAudioEvent(string? eventName)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		if (string.IsNullOrEmpty(eventName) || GetAKEventIdFromString(eventName) == 0)
		{
			Event val = new Event
			{
				WwiseObjectReference = ScriptableObject.CreateInstance<WwiseEventReference>()
			};
			((WwiseObjectReference)val.WwiseObjectReference).objectName = "NULL_WWISEEVENT";
			((WwiseObjectReference)val.WwiseObjectReference).id = GetAKEventIdFromString("NULL_WWISEEVENT");
			return val;
		}
		Event val2 = new Event
		{
			WwiseObjectReference = ScriptableObject.CreateInstance<WwiseEventReference>()
		};
		((WwiseObjectReference)val2.WwiseObjectReference).objectName = eventName;
		((WwiseObjectReference)val2.WwiseObjectReference).id = GetAKEventIdFromString(eventName);
		return val2;
	}

	private static uint GetAKEventIdFromString(string eventName)
	{
		if (eventIds.Count == 0)
		{
			PropertyInfo[] properties = typeof(EVENTS).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo obj in properties)
			{
				string key = obj.Name.ToLowerInvariant();
				uint value = (uint)obj.GetValue(null);
				eventIds.Add(key, value);
			}
		}
		eventIds.TryGetValue(eventName.ToLowerInvariant(), out var value2);
		return value2;
	}

	internal static ToolsItem[] GetToolsItems(List<string> items)
	{
		List<ToolsItem> list = new List<ToolsItem>();
		foreach (string item in items)
		{
			ToolsItem toolsItem = GetToolsItem(item);
			list.Add(toolsItem);
		}
		return list.ToArray();
	}

	internal static ToolsItem GetToolsItem(string item)
	{
		return Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(item)).WaitForCompletion().GetComponent<ToolsItem>();
	}

	internal static RequiredGearItem[] GetRequiredGearItems(List<ModRequiredGearItem> RequiredGear)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		List<RequiredGearItem> list = new List<RequiredGearItem>();
		foreach (ModRequiredGearItem item in RequiredGear)
		{
			RequiredGearItem val = new RequiredGearItem();
			val.m_Item = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(item.Item)).WaitForCompletion().GetComponent<GearItem>();
			if ((int)item.Units == 0)
			{
				val.m_Count = item.Count;
				val.m_Units = (Units)0;
			}
			if ((int)item.Units == 1)
			{
				val.m_Weight = ItemWeight.FromKilograms(item.Quantity);
				val.m_Units = (Units)1;
			}
			list.Add(val);
		}
		return list.ToArray();
	}

	internal static RequiredLiquid[] GetRequiredLiquid(List<ModRequiredLiquid> RequiredLiquid)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		List<RequiredLiquid> list = new List<RequiredLiquid>();
		foreach (ModRequiredLiquid item in RequiredLiquid)
		{
			RequiredLiquid val = new RequiredLiquid();
			val.m_Liquid = Addressables.LoadAssetAsync<LiquidType>(Object.op_Implicit(item.Liquid)).WaitForCompletion();
			val.m_Volume = ItemLiquidVolume.FromLiters(item.VolumeInLitres);
			list.Add(val);
		}
		return list.ToArray();
	}

	internal static RequiredPowder[] GetRequiredPowder(List<ModRequiredPowder> RequiredPowder)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		List<RequiredPowder> list = new List<RequiredPowder>();
		foreach (ModRequiredPowder item in RequiredPowder)
		{
			RequiredPowder val = new RequiredPowder();
			val.m_Powder = Addressables.LoadAssetAsync<PowderType>(Object.op_Implicit(item.Powder)).WaitForCompletion();
			val.m_Quantity = ItemWeight.FromKilograms(item.QuantityInKilograms);
			list.Add(val);
		}
		return list.ToArray();
	}

	internal static T TranslateEnumValue<T, E>(E value) where T : Enum where E : Enum
	{
		return (T)Enum.Parse(typeof(T), Enum.GetName(typeof(E), value));
	}
}
