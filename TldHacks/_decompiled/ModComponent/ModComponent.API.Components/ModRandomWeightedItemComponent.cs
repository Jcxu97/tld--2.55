using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModRandomWeightedItemComponent : ModBaseComponent
{
	public string[] ItemNames = Array.Empty<string>();

	public int[] ItemWeights = Array.Empty<int>();

	public ModRandomWeightedItemComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModRandomWeightedItemComponent>(this);
	}

	private void Update()
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		if (Settings.instance.disableRandomItemSpawns)
		{
			return;
		}
		if (ItemNames == null || ItemNames.Length == 0)
		{
			Logger.LogWarning("'" + ((Object)this).name + "' had an invalid list of potential spawn items.");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		if (ItemWeights == null || ItemWeights.Length == 0)
		{
			Logger.LogWarning("'" + ((Object)this).name + "' had an invalid list of item spawn weights.");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		if (ItemWeights.Length != ItemNames.Length)
		{
			Logger.LogWarning("The lists of item names and spawn weights for '" + ((Object)this).name + "' had unequal length.");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		int index = GetIndex();
		GameObject val = AssetBundleUtils.LoadAsset<GameObject>(ItemNames[index]);
		if ((Object)(object)val == (Object)null)
		{
			Logger.LogWarning($"Could not use '{((Object)this).name}' to spawn random item '{ItemNames[index]}'");
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			GameObject obj = Object.Instantiate<GameObject>(val, ((Component)this).transform.position, ((Component)this).transform.rotation);
			((Object)obj).name = ((Object)val).name;
			DisableObjectForXPMode val2 = ((obj != null) ? obj.GetComponent<DisableObjectForXPMode>() : null);
			if ((Object)(object)val2 != (Object)null)
			{
				Object.Destroy((Object)(object)val2);
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	[HideFromIl2Cpp]
	private int GetIndex()
	{
		if (ItemNames.Length == 1)
		{
			return 0;
		}
		int num = RandomUtils.Range(0, GetTotalWeight());
		int num2 = 0;
		int num3 = 0;
		int[] itemWeights = ItemWeights;
		foreach (int num4 in itemWeights)
		{
			num2 += num4;
			if (num2 > num)
			{
				return num3;
			}
			num3++;
		}
		Logger.LogError("Bug found while running 'GetIndex' for '" + ((Object)this).name + "'. For loop did not return a value.");
		return ItemNames.Length - 1;
	}

	[HideFromIl2Cpp]
	private int GetTotalWeight()
	{
		if (ItemWeights == null)
		{
			return 0;
		}
		int num = 0;
		int[] itemWeights = ItemWeights;
		foreach (int num2 in itemWeights)
		{
			num += num2;
		}
		return num;
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModRandomWeightedItemComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		ItemNames = entry.GetArray<string>("ItemNames");
		ItemWeights = entry.GetArray<int>("ItemWeights");
	}
}
