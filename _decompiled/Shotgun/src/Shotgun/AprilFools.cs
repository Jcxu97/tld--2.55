using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using AudioMgr;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Shotgun;

internal static class AprilFools
{
	[HarmonyPatch(typeof(vp_Bullet), "Start")]
	internal static class vp_Bullet_ReplaceWithSparkles
	{
		internal static bool Prefix(vp_Bullet __instance)
		{
			if (!IsAprilFoolsEnabled()) return true;
			PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
			GearItem val = ((playerManagerComponent != null) ? playerManagerComponent.m_ItemInHands : null);
			if ((Object)(object)val == (Object)null)
			{
				return true;
			}
			if ((Object)(object)val.m_GunItem == (Object)null)
			{
				return true;
			}
			if ((int)val.m_GunItem.m_GunType != 4)
			{
				return true;
			}
			if (Time.time - mLastSparkleTime >= 0.2f)
			{
				mLastSparkleTime = Time.time;
				SpawnPinkSparkles(((Component)__instance).transform.position, ((Component)__instance).transform.forward);
				PlayRandomFireSound();
			}
			Object.Destroy((Object)(object)((Component)__instance).gameObject);
			return false;
		}
	}

	[HarmonyPatch(typeof(GunItem), "Fired")]
	internal static class GunItem_PanicAllWildlife
	{
		internal static void Postfix(GunItem __instance)
		{
			if (!IsAprilFoolsEnabled()) return;
			if ((int)__instance.m_GunType != 4 || Time.time - mLastPanicTime < 0.2f)
			{
				return;
			}
			mLastPanicTime = Time.time;
			vp_FPSPlayer vpFPSPlayer = GameManager.GetVpFPSPlayer();
			object obj;
			if (vpFPSPlayer == null)
			{
				obj = null;
			}
			else
			{
				vp_FPSCamera fPSCamera = vpFPSPlayer.FPSCamera;
				obj = ((fPSCamera != null) ? ((Component)fPSCamera).GetComponentInChildren<Camera>() : null);
			}
			Camera val = (Camera)obj;
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Il2CppArrayBase<BaseAi> val2 = Object.FindObjectsOfType<BaseAi>();
			Plane[] array = (Plane[])(Il2CppArrayBase<Plane>)(object)GeometryUtility.CalculateFrustumPlanes(val);
			int num = 0;
			foreach (BaseAi item in val2)
			{
				if ((Object)(object)item == (Object)null || item.m_CurrentHP <= 0f)
				{
					continue;
				}
				Renderer componentInChildren = ((Component)item).GetComponentInChildren<Renderer>();
				if (!((Object)(object)componentInChildren == (Object)null) && GeometryUtility.TestPlanesAABB((Il2CppStructArray<Plane>)array, componentInChildren.bounds))
				{
					item.SetAiMode((AiMode)4);
					if (!((Object)(object)((Component)item).GetComponentInChildren<WildlifeSpinner>() != (Object)null))
					{
						WildlifeSpinner wildlifeSpinner = ((Component)item).gameObject.AddComponent<WildlifeSpinner>();
						wildlifeSpinner.TargetCamera = val;
						num++;
					}
				}
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CStopEmissionAfter_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ParticleSystem ps;

		public float delay;

		private ParticleSystem.EmissionModule _003Cemission_003E5__1;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStopEmissionAfter_003Ed__11(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cemission_003E5__1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = (object)new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if ((Object)(object)ps != (Object)null)
				{
					_003Cemission_003E5__1 = ps.emission;
					_003Cemission_003E5__1.rateOverTime = new ParticleSystem.MinMaxCurve(0f);
					_003Cemission_003E5__1 = null;
				}
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	internal static bool IsAprilFoolsEnabled()
	{
		return Settings.Instance != null && Settings.Instance.EnableAprilFools;
	}

	private static float mLastSparkleTime = -1f;

	private static float mLastPanicTime = -1f;

	private const float ShotCooldown = 0.2f;

	private static ClipManager mClipManager;

	private static Shot mShotPlayer;

	private static bool mAudioReady = false;

	internal static ParticleSystem.MinMaxCurve RandomBetween(float min, float max)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ParticleSystem.MinMaxCurve val = new ParticleSystem.MinMaxCurve(max);
		val.m_Mode = (ParticleSystemCurveMode)3;
		val.m_ConstantMin = min;
		val.m_ConstantMax = max;
		return val;
	}

	internal static void InitAudio()
	{
		if (!mAudioReady)
		{
			string text = Path.Combine(MelonEnvironment.ModsDirectory, "Shotgun", "Audio", "aprilfools_audio");
			AssetBundle val = AssetBundle.LoadFromFile(text);
			if (!((Object)(object)val == (Object)null))
			{
				mClipManager = AudioMaster.NewClipManager();
				mClipManager.LoadAllClipsFromBundle(val);
				mShotPlayer = AudioMaster.CreatePlayerShot((AudioMaster.SourceType)2);
				mAudioReady = mClipManager.clipCount > 0;
			}
		}
	}

	internal static void PlayRandomFireSound()
	{
		if (mAudioReady)
		{
			int num = Random.Range(0, mClipManager.clipCount);
			Clip clipAtIndex = mClipManager.GetClipAtIndex(num);
			if (clipAtIndex != null)
			{
				mShotPlayer.PlayOneshot(clipAtIndex);
			}
		}
	}

	internal static void SpawnPinkSparkles(Vector3 position, Vector3 direction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("ShotgunSparkles");
		val.transform.position = position;
		val.transform.rotation = Quaternion.LookRotation(direction);
		ParticleSystem val2 = val.AddComponent<ParticleSystem>();
		ParticleSystemRenderer component = val.GetComponent<ParticleSystemRenderer>();
		ParticleSystem.MainModule main = val2.main;
		main.startLifetime = RandomBetween(1.5f, 3.5f);
		main.startSpeed = RandomBetween(8f, 20f);
		main.startSize = RandomBetween(0.02f, 0.1f);
		main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.4f, 0.9f, 1f));
		main.maxParticles = 2000;
		main.simulationSpace = (ParticleSystemSimulationSpace)1;
		main.gravityModifier = new ParticleSystem.MinMaxCurve(0.3f);
		main.loop = false;
		ParticleSystem.EmissionModule emission = val2.emission;
		emission.enabled = true;
		emission.rateOverTime = new ParticleSystem.MinMaxCurve(15000f);
		ParticleSystem.ShapeModule shape = val2.shape;
		shape.shapeType = (ParticleSystemShapeType)4;
		shape.angle = 30f;
		shape.radius = 0.02f;
		Shader val3 = Shader.Find("Legacy Shaders/Particles/Additive");
		if ((Object)(object)val3 != (Object)null)
		{
			((Renderer)component).material = new Material(val3);
			((Renderer)component).material.SetColor("_TintColor", new Color(1f, 0.3f, 0.8f, 0.6f));
		}
		component.renderMode = (ParticleSystemRenderMode)0;
		val2.Play();
		MelonCoroutines.Start(StopEmissionAfter(val2, 0.125f));
		Object.Destroy((Object)(object)val, 5f);
	}

	[IteratorStateMachine(typeof(_003CStopEmissionAfter_003Ed__11))]
	internal static IEnumerator StopEmissionAfter(ParticleSystem ps, float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStopEmissionAfter_003Ed__11(0)
		{
			ps = ps,
			delay = delay
		};
	}
}
