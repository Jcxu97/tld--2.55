using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class PurificationMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		ModPurificationComponent modPurificationComponent = ((Il2CppObjectBase)modComponent).TryCast<ModPurificationComponent>();
		if (!((Object)(object)modPurificationComponent == (Object)null))
		{
			PurifyWater? orCreateComponent = ((Component?)(object)modPurificationComponent).GetOrCreateComponent<PurifyWater>();
			orCreateComponent.m_LocalizedProgressBarMessage = new LocalizedString
			{
				m_LocalizationID = modPurificationComponent.ProgressBarLocalizationID
			};
			orCreateComponent.m_ProgressBarDurationSeconds = modPurificationComponent.ProgressBarDurationSeconds;
			orCreateComponent.m_PurifyAudio = modPurificationComponent.PurifyAudio;
			orCreateComponent.m_LitersPurify = ItemLiquidVolume.FromLiters(modPurificationComponent.LitersPurify);
		}
	}
}
