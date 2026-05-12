using System.Collections.Generic;
using Il2CppTLD.Gear;

namespace Shotgun;

internal class ShotgunRefs
{
	internal GunPrefabRefs Gun { get; set; } = new GunPrefabRefs();


	internal Dictionary<string, ShotgunAmmoTypeRefs> AmmoTypes { get; set; } = new Dictionary<string, ShotgunAmmoTypeRefs>();


	internal CasingPrefabRefs Casing { get; set; } = new CasingPrefabRefs();


	internal WeaponViewRefs WeaponView { get; set; } = new WeaponViewRefs();


	internal FPHRefs FPH { get; set; } = new FPHRefs();


	internal FloatingRefs Floating { get; set; } = new FloatingRefs();


	internal List<GearItemData> ValidAmmoGearItemDatas { get; set; } = new List<GearItemData>();

}
