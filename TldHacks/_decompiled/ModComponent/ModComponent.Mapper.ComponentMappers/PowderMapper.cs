using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ModComponent.Mapper.ComponentMappers;

internal static class PowderMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		ModPowderComponent modPowderComponent = ((Il2CppObjectBase)modComponent).TryCast<ModPowderComponent>();
		if (!((Object)(object)modPowderComponent == (Object)null))
		{
			PowderItem? orCreateComponent = ((Component?)(object)modComponent).GetOrCreateComponent<PowderItem>();
			orCreateComponent.m_Type = Addressables.LoadAssetAsync<PowderType>(Object.op_Implicit("POWDER_Gunpowder")).WaitForCompletion();
			orCreateComponent.m_WeightLimit = ItemWeight.FromKilograms(modPowderComponent.CapacityKG);
			orCreateComponent.m_Weight = ItemWeight.FromKilograms(modPowderComponent.CapacityKG);
		}
	}
}
