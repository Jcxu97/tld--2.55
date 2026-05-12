using AudioMgr;
using Il2Cpp;
using UnityEngine;

namespace Shotgun;

internal class ConfigurationReferences
{
	internal vp_FPSPlayer? FPSPlayer { get; set; }

	internal vp_FPSCamera? FPSCamera { get; set; }

	internal Animator? VanillaAnimator { get; set; }

	internal Animator? ModdedAnimator { get; set; }

	internal ShotgunRefs? Shotgun { get; set; } = new ShotgunRefs();


	internal GunRefs? Rifle { get; set; } = new GunRefs();


	internal ClipManager? ClipManager { get; set; }

	internal Shot? AudioShot { get; set; }
}
