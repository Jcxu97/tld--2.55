using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class AmmoMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ModAmmoComponent modAmmoComponent = ((Il2CppObjectBase)modComponent).TryCast<ModAmmoComponent>();
		if (!((Object)(object)modAmmoComponent == (Object)null))
		{
			((Component?)(object)modAmmoComponent).GetOrCreateComponent<AmmoItem>().m_AmmoForGunType = modAmmoComponent.AmmoForGunType;
		}
	}
}
