using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class CollectibleMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		ModCollectibleComponent modCollectibleComponent = ((Il2CppObjectBase)modComponent).TryCast<ModCollectibleComponent>();
		if (!((Object)(object)modCollectibleComponent == (Object)null))
		{
			NarrativeCollectibleItem? orCreateComponent = ((Component?)(object)modCollectibleComponent).GetOrCreateComponent<NarrativeCollectibleItem>();
			orCreateComponent.m_HudMessageOnPickup = new LocalizedString
			{
				m_LocalizationID = modCollectibleComponent.HudMessageLocalizationId
			};
			orCreateComponent.m_JournalEntryNumber = 0;
			orCreateComponent.m_NarrativeTextLocID = modCollectibleComponent.NarrativeTextLocalizationId;
			orCreateComponent.m_ShowDuringInspect = true;
			orCreateComponent.m_TextAlignment = modCollectibleComponent.TextAlignment;
			orCreateComponent.m_Type = (CollectibleType)1;
		}
	}
}
