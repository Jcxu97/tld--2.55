using Il2Cpp;
using UnityEngine;

namespace NorthernLightsBroadcast;

public static class TVLock
{
	public static float m_StartCameraFOV;

	public static Vector2 m_StartPitchLimit;

	public static Vector2 m_StartYawLimit;

	public static Vector3 m_StartPlayerPosition;

	public static float m_StartAngleX;

	public static float m_StartAngleY;

	public static TVManager currentManager;

	public static bool lockedInTVView;

	public static void ToggleTVView(TVManager tvManager)
	{
		if (lockedInTVView)
		{
			ExitTVView();
		}
		else
		{
			EnterTVView(tvManager);
		}
	}

	public static void EnterTVView(TVManager tvManager)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		if (tvManager.currentState != 0)
		{
			currentManager = tvManager;
			lockedInTVView = true;
			tvManager.ui.canvas.worldCamera = NorthernLightsBroadcastMain.eventCam;
			m_StartCameraFOV = GameManager.GetMainCamera().fieldOfView;
			m_StartPitchLimit = GameManager.GetVpFPSCamera().RotationPitchLimit;
			m_StartYawLimit = GameManager.GetVpFPSCamera().RotationYawLimit;
			m_StartPlayerPosition = ((Component)GameManager.GetVpFPSPlayer()).transform.position;
			Quaternion rotation = ((Component)GameManager.GetVpFPSPlayer()).transform.rotation;
			m_StartAngleX = ((Quaternion)(ref rotation)).eulerAngles.x;
			rotation = ((Component)GameManager.GetVpFPSPlayer()).transform.rotation;
			m_StartAngleY = ((Quaternion)(ref rotation)).eulerAngles.y;
			GameManager.GetPlayerManagerComponent().SetControlMode((PlayerControlMode)6);
			GameManager.GetPlayerManagerComponent().TeleportPlayer(tvManager.dummyCamera.transform.position - GameManager.GetVpFPSCamera().PositionOffset, tvManager.dummyCamera.transform.rotation);
			((Component)GameManager.GetVpFPSCamera()).transform.position = tvManager.dummyCamera.transform.position;
			((Component)GameManager.GetVpFPSCamera()).transform.localPosition = GameManager.GetVpFPSCamera().PositionOffset;
			vp_FPSCamera vpFPSCamera = GameManager.GetVpFPSCamera();
			rotation = tvManager.dummyCamera.transform.rotation;
			float y = ((Quaternion)(ref rotation)).eulerAngles.y;
			rotation = tvManager.dummyCamera.transform.rotation;
			vpFPSCamera.SetAngle(y, ((Quaternion)(ref rotation)).eulerAngles.x);
			GameManager.GetVpFPSCamera().SetPitchLimit(new Vector2(0f, 0f));
			GameManager.GetVpFPSCamera().SetFOVFromOptions(50f);
			GameManager.GetVpFPSCamera().SetNearPlaneOverride(0.001f);
			GameManager.GetVpFPSCamera().SetYawLimit(tvManager.dummyCamera.transform.rotation, new Vector2(0f, 0f));
			GameManager.GetVpFPSCamera().LockRotationLimit();
			((Renderer)tvManager.objectRenderer).enabled = true;
		}
	}

	public static void ExitTVView()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (lockedInTVView)
		{
			GameManager.GetVpFPSCamera().m_PanViewCamera.m_IsDetachedFromPlayer = false;
			GameManager.GetPlayerManagerComponent().SetControlMode((PlayerControlMode)0);
			GameManager.GetVpFPSCamera().UnlockRotationLimit();
			GameManager.GetVpFPSCamera().RotationPitchLimit = m_StartPitchLimit;
			GameManager.GetVpFPSCamera().RotationYawLimit = m_StartYawLimit;
			((Component)GameManager.GetVpFPSPlayer()).transform.position = m_StartPlayerPosition;
			((Component)GameManager.GetVpFPSCamera()).transform.localPosition = GameManager.GetVpFPSCamera().PositionOffset;
			GameManager.GetVpFPSCamera().SetAngle(m_StartAngleY, m_StartAngleX);
			GameManager.GetVpFPSCamera().SetFOVFromOptions(m_StartCameraFOV);
			GameManager.GetVpFPSCamera().UpdateCameraRotation();
			GameManager.GetPlayerManagerComponent().StickPlayerToGround();
			GameManager.GetVpFPSCamera().UnlockRotationLimit();
			currentManager.ui.ActivateOSD(value: false);
			currentManager = null;
			lockedInTVView = false;
		}
	}
}
