using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModRandomItemComponent : ModBaseComponent
{
	public string[] ItemNames = Array.Empty<string>();

	public ModRandomItemComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModRandomItemComponent>(this);
	}

	private void Update()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
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
		int num = RandomUtils.Range(0, ItemNames.Length);
		GameObject val = AssetBundleUtils.LoadAsset<GameObject>(ItemNames[num]);
		if ((Object)(object)val == (Object)null)
		{
			Logger.LogWarning($"Could not use '{((Object)this).name}' to spawn random item '{ItemNames[num]}'");
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
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModRandomItemComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		ItemNames = entry.GetArray<string>("ItemNames");
	}
}
