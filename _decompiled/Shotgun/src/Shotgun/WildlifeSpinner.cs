using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Shotgun;

[RegisterTypeInIl2Cpp(false)]
internal class WildlifeSpinner : MonoBehaviour
{
	public Camera TargetCamera;

	private Renderer mCachedRenderer;

	private float mYawSpeed;

	private float mPitchSpeed;

	private float mRollSpeed;

	private float mWobbleTimer = 0f;

	private float mWobbleInterval;

	private float mOffScreenTime = 0f;

	private const float OffScreenGracePeriod = 2f;

	public WildlifeSpinner()
	{
	}

	public WildlifeSpinner(IntPtr pointer)
		: base(pointer)
	{
	}

	private void Start()
	{
		mYawSpeed = Random.Range(900f, 2000f) * ((Random.value > 0.5f) ? 1f : (-1f));
		mPitchSpeed = Random.Range(0f, 400f) * ((Random.value > 0.5f) ? 1f : (-1f));
		mRollSpeed = Random.Range(0f, 300f) * ((Random.value > 0.5f) ? 1f : (-1f));
		mWobbleInterval = Random.Range(0.3f, 0.8f);
		mCachedRenderer = ((Component)this).GetComponentInChildren<Renderer>();
	}

	private void Update()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		mWobbleTimer += Time.deltaTime;
		if (mWobbleTimer >= mWobbleInterval)
		{
			mWobbleTimer = 0f;
			mPitchSpeed = Random.Range(-500f, 500f);
			mRollSpeed = Random.Range(-400f, 400f);
		}
		float deltaTime = Time.deltaTime;
		((Component)this).transform.Rotate(mPitchSpeed * deltaTime, mYawSpeed * deltaTime, mRollSpeed * deltaTime, (Space)0);
		if ((Object)(object)TargetCamera == (Object)null)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		if ((Object)(object)mCachedRenderer == (Object)null)
		{
			return;
		}
		Plane[] array = (Plane[])(Il2CppArrayBase<Plane>)(object)GeometryUtility.CalculateFrustumPlanes(TargetCamera);
		if (!GeometryUtility.TestPlanesAABB((Il2CppStructArray<Plane>)array, mCachedRenderer.bounds))
		{
			mOffScreenTime += Time.deltaTime;
			if (mOffScreenTime >= 2f)
			{
				Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
				((Component)this).transform.eulerAngles = new Vector3(0f, eulerAngles.y, 0f);
				Object.Destroy((Object)(object)this);
			}
		}
		else
		{
			mOffScreenTime = 0f;
		}
	}
}
