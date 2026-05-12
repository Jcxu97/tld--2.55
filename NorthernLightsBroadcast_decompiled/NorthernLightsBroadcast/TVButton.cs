using System;
using System.Collections;
using AudioMgr;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using UnityEngine;

namespace NorthernLightsBroadcast;

[RegisterTypeInIl2Cpp]
public class TVButton : MonoBehaviour
{
	public TVManager manager;

	public Shot tvClickShot;

	public MeshRenderer meshRenderer;

	public Color32 emissionColorOn = new Color32((byte)106, (byte)7, (byte)7, byte.MaxValue);

	public Color32 emissionColorOff = new Color32((byte)0, (byte)0, (byte)0, (byte)0);

	public bool isMoving;

	public bool isGlowing;

	public Vector3 outPosition;

	public Vector3 inPosition;

	public bool isSetup;

	public TVButton(IntPtr intPtr)
		: base(intPtr)
	{
	}//IL_000a: Unknown result type (might be due to invalid IL or missing references)
	//IL_000f: Unknown result type (might be due to invalid IL or missing references)
	//IL_0019: Unknown result type (might be due to invalid IL or missing references)
	//IL_001e: Unknown result type (might be due to invalid IL or missing references)


	public void Awake()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (!isSetup)
		{
			tvClickShot = AudioMaster.CreateShot(((Component)this).gameObject, (SourceType)0);
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
			((Renderer)meshRenderer).material.DisableKeyword("_EMISSION");
			((Renderer)meshRenderer).material.SetColor("_EmissionColor", Color32.op_Implicit(emissionColorOff));
			outPosition = ((Component)this).transform.localPosition;
			inPosition = new Vector3(outPosition.x, outPosition.y, outPosition.z - 0.006f);
			isSetup = true;
		}
	}

	[HideFromIl2Cpp]
	public IEnumerator PressButtonAnimation(float speed)
	{
		isMoving = true;
		Action<ITween<Vector3>> progress = delegate(ITween<Vector3> t)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			((Component)manager.redbutton).transform.localPosition = t.CurrentValue;
		};
		Action<ITween<Vector3>> completion = delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			((Component)manager.redbutton).transform.localPosition = inPosition;
			tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			Glow(!isGlowing);
			if (manager.currentState == TVManager.TVState.Off)
			{
				manager.SwitchState(TVManager.TVState.Static);
			}
			else
			{
				manager.SwitchState(TVManager.TVState.Off);
			}
		};
		Action<ITween<Vector3>> progress2 = delegate(ITween<Vector3> t)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			((Component)manager.redbutton).transform.localPosition = t.CurrentValue;
		};
		Action<ITween<Vector3>> completion2 = delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			((Component)manager.redbutton).transform.localPosition = outPosition;
			isMoving = false;
		};
		((Tween<Vector3>)((Component)manager.redbutton).gameObject.Tween(((Component)manager.redbutton).gameObject, outPosition, inPosition, speed, TweenScaleFunctions.SineEaseInOut, progress, completion)).ContinueWith<Vector3>(new Vector3Tween().Setup(inPosition, outPosition, speed, TweenScaleFunctions.SineEaseInOut, progress2, completion2));
		yield return null;
	}

	[HideFromIl2Cpp]
	public void Glow(bool enabled)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (enabled)
		{
			((Renderer)meshRenderer).material.EnableKeyword("_EMISSION");
			((Renderer)meshRenderer).material.SetColor("_EmissionColor", Color32.op_Implicit(emissionColorOn));
			isGlowing = true;
		}
		else
		{
			((Renderer)meshRenderer).material.DisableKeyword("_EMISSION");
			((Renderer)meshRenderer).material.SetColor("_EmissionColor", Color32.op_Implicit(emissionColorOff));
			isGlowing = false;
		}
	}

	[HideFromIl2Cpp]
	public void TogglePower()
	{
		if (!isMoving)
		{
			MelonCoroutines.Start(PressButtonAnimation(0.5f));
		}
	}
}
