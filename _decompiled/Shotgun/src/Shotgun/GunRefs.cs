using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;

namespace Shotgun;

internal class GunRefs
{
	internal GunPrefabRefs Gun { get; set; } = new GunPrefabRefs();


	internal AmmoPrefabRefs AmmoSingle { get; set; } = new AmmoPrefabRefs();


	internal AmmoPrefabRefs AmmoBox { get; set; } = new AmmoPrefabRefs();


	internal CasingPrefabRefs Casing { get; set; } = new CasingPrefabRefs();


	internal WeaponViewRefs WeaponView { get; set; } = new WeaponViewRefs();


	internal FPHRefs FPH { get; set; } = new FPHRefs();


	internal FloatingRefs Floating { get; set; } = new FloatingRefs();


	internal List<GearItemData> ValidAmmoGearItemDatas { get; set; } = new List<GearItemData>();

}
