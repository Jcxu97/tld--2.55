using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using MoreCookingSlots;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("MoreCookingSlots")]
[assembly: AssemblyCopyright("Marcy")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: MelonInfo(typeof(MoreCookingSlotsMelon), "MoreCookingSlots", "1.0.0", "Marcy", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.0.0.0")]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace MoreCookingSlots
{
	public class MoreCookingSlotsMelon : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Settings.instance.AddToModSettings("更多的烹饪槽位v1.0");
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
		}

		public override void OnUpdate()
		{
		}
	}
	internal static class Patches
	{
		[HarmonyPatch(typeof(WoodStove), "Awake")]
		public class AddCookingSlots
		{
			public static void Postfix(ref WoodStove __instance)
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
				//IL_0107: Unknown result type (might be due to invalid IL or missing references)
				//IL_010c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0134: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_018f: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_0401: Unknown result type (might be due to invalid IL or missing references)
				//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
				//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
				//IL_0309: Unknown result type (might be due to invalid IL or missing references)
				//IL_031d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0331: Unknown result type (might be due to invalid IL or missing references)
				//IL_0336: Unknown result type (might be due to invalid IL or missing references)
				//IL_0348: Unknown result type (might be due to invalid IL or missing references)
				//IL_035c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0361: Unknown result type (might be due to invalid IL or missing references)
				//IL_0442: Unknown result type (might be due to invalid IL or missing references)
				//IL_0466: Unknown result type (might be due to invalid IL or missing references)
				//IL_0485: Unknown result type (might be due to invalid IL or missing references)
				//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
				//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
				//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
				//IL_0501: Unknown result type (might be due to invalid IL or missing references)
				//IL_053c: Unknown result type (might be due to invalid IL or missing references)
				GameObject gameObject = ((Component)__instance).gameObject;
				if (((Object)gameObject).name.Contains("INTERACTIVE_FirePlace") && Settings.instance.fireplaces)
				{
					Scene scene = gameObject.scene;
					if (((Scene)(ref scene)).name == "HubRegion")
					{
						gameObject.transform.eulerAngles = new Vector3(0f, 15.5f, 0f);
					}
					GameObject gameObject2 = ((Component)StoveUtils.GetPlacePoints(gameObject).transform.FindChild("Cylinder")).gameObject;
					GameObject gameObject3 = ((Component)gameObject.transform.FindChild("GearPlacePoint")).gameObject;
					if ((Object)(object)gameObject2 != (Object)null && (Object)(object)gameObject3 != (Object)null)
					{
						Transform transform = gameObject2.transform;
						transform.localPosition -= new Vector3(0.2f, 0f, -0.015f);
						Transform transform2 = gameObject3.transform;
						transform2.localPosition -= new Vector3(0.2f, 0f, 0f);
						GameObject placePoints = StoveUtils.GetPlacePoints(gameObject);
						GameObject placePointObject = StoveUtils.InstantiatePlacePoint(gameObject3, placePoints.transform, new Vector3(0.4f, 0f, 0f));
						GameObject val = StoveUtils.InstantiateCookingSpot(gameObject2, placePointObject, placePoints.transform, new Vector3(0.4f, 0f, 0f), gameObject);
						StoveUtils.RecreateArrays(gameObject, placePoints);
						gameObject2 = ((Component)StoveUtils.GetPlacePoints(gameObject).transform.FindChild("Cylinder (1)")).gameObject;
						Transform transform3 = gameObject2.transform;
						transform3.localPosition += new Vector3(0.115f, 0f, 0.1f);
						gameObject3 = ((Component)gameObject.transform.FindChild("GearPlacePoint (1)")).gameObject;
						Transform transform4 = gameObject3.transform;
						transform4.localPosition += new Vector3(0.115f, 0f, 0.1f);
					}
				}
				if (((Object)gameObject).name.Contains("INTERACTIVE_RimGrill") && Settings.instance.grill)
				{
					GameObject gameObject2 = ((Component)StoveUtils.GetPlacePoints(gameObject).transform.FindChild("Cylinder (1)")).gameObject;
					GameObject gameObject3 = ((Component)gameObject.transform.FindChild("OBJ_RimGrillRack").FindChild("GearPlacePoint (1)")).gameObject;
					Transform parent = gameObject.transform.Find("OBJ_RimGrillRack");
					if ((Object)(object)gameObject2 != (Object)null && (Object)(object)gameObject3 != (Object)null)
					{
						GameObject placePoints = StoveUtils.GetPlacePoints(gameObject);
						GameObject placePointObject = StoveUtils.InstantiatePlacePoint(gameObject3, parent, new Vector3(-0.07f, 0f, -0.15f));
						GameObject val = StoveUtils.InstantiateCookingSpot(gameObject2, placePointObject, placePoints.transform, new Vector3(-0.07f, 0f, -0.15f), gameObject);
						placePointObject = StoveUtils.InstantiatePlacePoint(gameObject3, parent, new Vector3(-0.2f, 0f, 0.1f));
						val = StoveUtils.InstantiateCookingSpot(gameObject2, placePointObject, placePoints.transform, new Vector3(-0.2f, 0f, 0.1f), gameObject);
						Transform transform5 = gameObject2.transform;
						transform5.localPosition += new Vector3(0f, 0f, 0.03f);
						Transform transform6 = gameObject3.transform;
						transform6.localPosition += new Vector3(0f, 0f, 0.03f);
						StoveUtils.RecreateArrays(gameObject, placePoints);
					}
				}
				if (((Object)gameObject).name.Contains("INTERACTIVE_FireBarrel") && Settings.instance.barrel)
				{
					GameObject gameObject2 = ((Component)StoveUtils.GetPlacePoints(gameObject).transform.FindChild("Cylinder (1)")).gameObject;
					GameObject gameObject3 = ((Component)gameObject.transform.FindChild("OBJ_RimGrillRack").FindChild("GearPlacePoint (1)")).gameObject;
					Transform parent2 = gameObject.transform.Find("OBJ_RimGrillRack");
					gameObject2.transform.localScale = new Vector3(0.15f, 0.005f, 0.15f);
					if ((Object)(object)gameObject2 != (Object)null && (Object)(object)gameObject3 != (Object)null)
					{
						GameObject placePoints = StoveUtils.GetPlacePoints(gameObject);
						GameObject placePointObject = StoveUtils.InstantiatePlacePoint(gameObject3, parent2, new Vector3(0.16f, 0f, 0.1f));
						GameObject val = StoveUtils.InstantiateCookingSpot(gameObject2, placePointObject, placePoints.transform, new Vector3(0.16f, 0f, 0.1f), gameObject);
						placePointObject = StoveUtils.InstantiatePlacePoint(gameObject3, parent2, new Vector3(0f, 0f, -0.13f));
						val = StoveUtils.InstantiateCookingSpot(gameObject2, placePointObject, placePoints.transform, new Vector3(0f, 0f, -0.13f), gameObject);
						Transform transform7 = gameObject2.transform;
						transform7.localPosition += new Vector3(-0.05f, 0f, 0.05f);
						Transform transform8 = gameObject3.transform;
						transform8.localPosition += new Vector3(-0.05f, 0f, 0.05f);
						gameObject2 = ((Component)StoveUtils.GetPlacePoints(gameObject).transform.FindChild("Cylinder")).gameObject;
						gameObject2.transform.localScale = new Vector3(0.15f, 0.005f, 0.15f);
						StoveUtils.RecreateArrays(gameObject, placePoints);
					}
				}
			}
		}
	}
	internal class Settings : JsonModSettings
	{
		internal static Settings instance;

		[Section("微调")]
		[Name("启用壁炉")]
		[Description("是否给壁炉额外增加一个烹饪槽位(2 -> 3),必须重新加载场景或切换地图才能生效")]
		public bool fireplaces = true;

		[Name("启用火桶")]
		[Description("是否给火桶额外增加一个烹饪槽位(2 -> 4),必须重新加载场景或切换地图才能生效")]
		public bool barrel = true;

		[Name("启用边缘烧烤")]
		[Description("是否给边缘烧烤额外增加一个烹饪槽位(2 -> 4),必须重新加载场景或切换地图才能生效")]
		public bool grill = true;

		[Section("重置设置")]
		[Name("重置为默认")]
		[Description("将所有设置重置为默认值，必须重新加载场景或切换地图才能生效")]
		public bool ResetSettings;

		protected override void OnConfirm()
		{
			ApplyReset();
			instance.ResetSettings = false;
			base.OnConfirm();
			RefreshGUI();
		}

		public static void ApplyReset()
		{
			if (instance.ResetSettings)
			{
				instance.fireplaces = true;
				instance.barrel = true;
				instance.grill = true;
				instance.ResetSettings = false;
			}
		}

		static Settings()
		{
			instance = new Settings();
		}
	}
	internal static class StoveUtils
	{
		public static GameObject InstantiateCookingSpot(GameObject original, GameObject placePointObject, Transform placePoints, Vector3 offset, GameObject fireplace)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)original) || !Object.op_Implicit((Object)(object)placePointObject) || !Object.op_Implicit((Object)(object)placePoints) || !Object.op_Implicit((Object)(object)fireplace))
			{
				MelonLogger.Error("Cannot instantiate cooking spot, not all variables are met.");
				return null;
			}
			GameObject val = Object.Instantiate<GameObject>(original, placePoints);
			Transform transform = val.transform;
			transform.localPosition += offset;
			val.GetComponent<CookingSlot>().m_GearPlacePoint = placePointObject.GetComponent<GearPlacePoint>();
			val.GetComponent<CookingSlot>().m_FireplaceHost = (FireplaceInteraction)(object)fireplace.GetComponent<WoodStove>();
			return val;
		}

		public static GameObject InstantiatePlacePoint(GameObject original, Transform parent, Vector3 offset)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)original) || !Object.op_Implicit((Object)(object)parent))
			{
				MelonLogger.Error("Cannot instantiate gear place point, not all variables are met.");
				return null;
			}
			GameObject val = Object.Instantiate<GameObject>(original, parent);
			Transform transform = val.transform;
			transform.localPosition += offset;
			return val;
		}

		public static GameObject GetPlacePoints(GameObject stove)
		{
			if (!Object.op_Implicit((Object)(object)stove))
			{
				MelonLogger.Error("Cannot get Place Points Transform, stove is null.");
				return null;
			}
			GameObject gameObject = ((Component)stove.transform.FindChild("PlacePoints")).gameObject;
			if ((Object)(object)gameObject != (Object)null)
			{
				return gameObject;
			}
			MelonLogger.Error("Place Points Transform cannot be found.");
			return null;
		}

		public static void RecreateArrays(GameObject stove, GameObject placePointsObject)
		{
			CookingSlot[] array = Il2CppArrayBase<CookingSlot>.op_Implicit(stove.GetComponentsInChildren<CookingSlot>());
			MeshRenderer[] array2 = Il2CppArrayBase<MeshRenderer>.op_Implicit(placePointsObject.GetComponentsInChildren<MeshRenderer>());
			((FireplaceInteraction)stove.GetComponent<WoodStove>()).m_CookingSlots = Il2CppReferenceArray<CookingSlot>.op_Implicit(array);
			PlacePoints component = placePointsObject.GetComponent<PlacePoints>();
			Renderer[] array3 = (Renderer[])(object)array2;
			component.m_PlacePoints = Il2CppReferenceArray<Renderer>.op_Implicit(array3);
		}
	}
}
