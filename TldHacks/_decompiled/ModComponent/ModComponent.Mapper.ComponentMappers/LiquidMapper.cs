using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ModComponent.Mapper.ComponentMappers;

internal static class LiquidMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		ModLiquidComponent modLiquidComponent = ((Il2CppObjectBase)modComponent).TryCast<ModLiquidComponent>();
		if ((Object)(object)modLiquidComponent == (Object)null)
		{
			return;
		}
		string liquidTypeString = modLiquidComponent.LiquidType.GetLiquidTypeString();
		LiquidType val = Addressables.LoadAssetAsync<LiquidType>(Object.op_Implicit(liquidTypeString)).WaitForCompletion();
		if ((Object)(object)val == (Object)null)
		{
			Logger.LogError("Invalid LiquidType " + liquidTypeString + " for " + ((Object)modComponent).name);
			return;
		}
		LiquidItem orCreateComponent = ((Component?)(object)modComponent).GetOrCreateComponent<LiquidItem>();
		orCreateComponent.m_LiquidCapacity = ItemLiquidVolume.FromLiters(modLiquidComponent.LiquidCapacityLiters);
		orCreateComponent.m_LiquidType = val;
		orCreateComponent.m_Liquid = ItemLiquidVolume.FromLiters(modLiquidComponent.LiquidLiters);
		orCreateComponent.m_Maximum = ItemLiquidVolume.FromLiters(modLiquidComponent.LiquidCapacityLiters);
		orCreateComponent.m_Minimum = ItemLiquidVolume.FromLiters(modLiquidComponent.LiquidLiters);
		if (modLiquidComponent.RandomizeQuantity)
		{
			float num = Math.Clamp(modLiquidComponent.LiquidCapacityLiters / 16f * Random.Range(1f, 16f), modLiquidComponent.LiquidLiters, modLiquidComponent.LiquidCapacityLiters);
			orCreateComponent.m_Liquid = ItemLiquidVolume.FromLiters(num);
		}
	}
}
