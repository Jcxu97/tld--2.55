using AudioMgr;
using Il2Cpp;
using UnityEngine;

namespace Shotgun;

internal class AnimationEventProxy : MonoBehaviour
{
	internal void OnEquipComplete()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Equip_Complete();
		}
	}

	internal void OnEquipShowItem()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Equip_ShowItem();
		}
	}

	internal void OnAimComplete()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Aim_Complete();
		}
	}

	internal void OnAimCancelComplete()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Aim_Cancel_Complete();
		}
	}

	internal void OnFireFX()
	{
		PlayerManager pm = GameManager.GetPlayerManagerComponent();
		GunItem gun = (pm != null && pm.m_ItemInHands != null) ? pm.m_ItemInHands.m_GunItem : null;
		if (gun != null) gun.m_FireAudio = null;

		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Fire_FX();
		}
	}

	internal void OnFireComplete()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Fire_Complete();
		}
	}

	internal void OnRoundLoaded()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Round_Loaded();
		}
	}

	internal void OnRoundsUnloaded()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Rounds_Unloaded();
		}
	}

	internal void OnReloadComplete()
	{
		PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
		if (!((Object)(object)playerAnimationComponent == (Object)null))
		{
			playerAnimationComponent.OnAnimationEvent_Generic_Reload_Complete();
		}
	}

	internal void OnHandRemoveComplete()
	{
		Animator val = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
		if (!((Object)(object)val == (Object)null))
		{
			val.SetInteger("Rounds_Loaded", 0);
		}
	}

	internal void OnPlaySound(string sound)
	{
		ConfigurationReferences refs = Main.Instance.Configuration.Refs;
		if (refs.ClipManager != null && !((Object)(object)refs.AudioShot == (Object)null))
		{
			Clip clip = refs.ClipManager.GetClip(sound);
			if (clip != null)
			{
				refs.AudioShot._audioSource.PlayOneShot(clip.audioClip);
			}
		}
	}
}
