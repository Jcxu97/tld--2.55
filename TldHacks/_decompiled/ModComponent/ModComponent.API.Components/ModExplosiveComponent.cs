using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
internal class ModExplosiveComponent : ModBaseEquippableComponent
{
	public float killPlayerRange = 5f;

	public float explosionDelay = 5f;

	public string explosionAudio = "";

	protected override void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModExplosiveComponent>(this);
		base.Awake();
	}

	[HideFromIl2Cpp]
	internal void OnExplode()
	{
		GameAudioManager.Play3DSound(explosionAudio, ((Component)this).gameObject);
		GameManager.GetConditionComponent().AddHealth(-1f * GetDamageToPlayer(), (DamageSource)0);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	[HideFromIl2Cpp]
	private float GetDamageToPlayer()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (killPlayerRange == 0f)
		{
			return 0f;
		}
		float num = Vector3.Distance(((Component)GameManager.GetVpFPSPlayer()).transform.position, ((Component)this).transform.position);
		if (num <= 0f)
		{
			return 100f;
		}
		return 100f * killPlayerRange * killPlayerRange / (num * num);
	}

	public ModExplosiveComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}
}
