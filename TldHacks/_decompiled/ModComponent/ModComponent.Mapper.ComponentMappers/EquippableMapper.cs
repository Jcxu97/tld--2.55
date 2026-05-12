using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class EquippableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		ModBaseEquippableComponent modBaseEquippableComponent = ((Il2CppObjectBase)modComponent).TryCast<ModBaseEquippableComponent>();
		if (!((Object)(object)modBaseEquippableComponent == (Object)null) && string.IsNullOrEmpty(modBaseEquippableComponent.InventoryActionLocalizationId) && !string.IsNullOrEmpty(modBaseEquippableComponent.ImplementationType) && !string.IsNullOrEmpty(modBaseEquippableComponent.EquippedModelPrefabName))
		{
			modBaseEquippableComponent.InventoryActionLocalizationId = "GAMEPLAY_Equip";
		}
	}
}
