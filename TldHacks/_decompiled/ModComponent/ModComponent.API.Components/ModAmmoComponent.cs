using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModAmmoComponent : ModBaseComponent
{
	public GunType AmmoForGunType;

	public ModAmmoComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModAmmoComponent>(this);
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModAmmoComponent")
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		AmmoForGunType = entry.GetEnum<GunType>("AmmoForGunType");
	}
}
