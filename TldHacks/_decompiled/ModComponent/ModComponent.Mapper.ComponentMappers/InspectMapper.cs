using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class InspectMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (modComponent.InspectOnPickup)
		{
			Inspect orCreateComponent = ((Component?)(object)modComponent).GetOrCreateComponent<Inspect>();
			orCreateComponent.m_DistanceFromCamera = modComponent.InspectDistance;
			orCreateComponent.m_Scale = modComponent.InspectScale;
			orCreateComponent.m_Angles = modComponent.InspectAngles;
			orCreateComponent.m_Offset = modComponent.InspectOffset;
			if ((Object)(object)modComponent.InspectModel != (Object)null && (Object)(object)modComponent.NormalModel != (Object)null)
			{
				orCreateComponent.m_NormalMesh = modComponent.NormalModel;
				orCreateComponent.m_NormalMesh.SetActive(true);
				orCreateComponent.m_InspectModeMesh = modComponent.InspectModel;
				orCreateComponent.m_InspectModeMesh.SetActive(false);
			}
		}
	}
}
