using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper;

internal static class GearEquipper
{
	public static void Equip(ModBaseEquippableComponent equippable)
	{
		if (!((Object)(object)equippable == (Object)null))
		{
			if (!string.IsNullOrEmpty(equippable.EquippedModelPrefabName))
			{
				GameObject val = AssetBundleUtils.LoadAsset<GameObject>(equippable.EquippedModelPrefabName);
				equippable.EquippedModel = Object.Instantiate<GameObject>(val, ((Component)GameManager.GetWeaponCamera()).transform);
				equippable.EquippedModel.layer = vp_Layer.Weapon;
			}
			equippable.OnEquipped?.Invoke();
			InterfaceManager.QuitCurrentScreens();
			ModUtils.PlayAudio(equippable.EquippingAudio);
		}
	}

	public static void Unequip(ModBaseEquippableComponent modComponent)
	{
		if (!((Object)(object)modComponent == (Object)null))
		{
			GameManager.GetPlayerManagerComponent().UnequipItemInHandsSkipAnimation();
		}
	}

	internal static void OnUnequipped(ModBaseEquippableComponent modComponent)
	{
		if (!((Object)(object)modComponent == (Object)null))
		{
			if ((Object)(object)modComponent.EquippedModel != (Object)null)
			{
				Object.Destroy((Object)(object)modComponent.EquippedModel);
				modComponent.EquippedModel = null;
			}
			modComponent.OnUnequipped?.Invoke();
			ModUtils.PlayAudio(modComponent.StowAudio);
		}
	}
}
