using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BaseAi), "OnDisable")]
public class DeathPatch2
{
	public static void Postfix(ref BaseAi __instance)
	{
		Component component = ((Component)__instance).gameObject.GetComponent(Il2CppType.Of<PingComponent>());
		PingComponent.ManualDelete((component != null) ? ((Il2CppObjectBase)component).TryCast<PingComponent>() : null);
	}
}
