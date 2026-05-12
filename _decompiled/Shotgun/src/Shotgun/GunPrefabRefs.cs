using Il2Cpp;
using ModComponent.API.Components;
using UnityEngine;

namespace Shotgun;

internal class GunPrefabRefs
{
	internal GameObject? Prefab { get; set; }

	internal GearItem? GearItem { get; set; }

	internal GunItem? GunItem { get; set; }

	internal FirstPersonItem? FirstPersonItem { get; set; }

	internal StruggleBonus? StruggleBonus { get; set; }

	internal ModGenericComponent? ModComponent { get; set; }
}
