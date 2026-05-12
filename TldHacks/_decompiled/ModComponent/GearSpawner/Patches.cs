using HarmonyLib;
using Il2CppSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GearSpawner;

internal static class Patches
{
	[HarmonyPatch(typeof(AssetReference), "RuntimeKeyIsValid")]
	internal static class RuntimeKeyIsValid
	{
		private static void Postfix(AssetReference __instance, ref bool __result)
		{
			if (!__result && __instance.AssetGUID != null && __instance.AssetGUID != null && __instance.AssetGUID.StartsWith("GEAR_"))
			{
				GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(__instance.AssetGUID)).WaitForCompletion();
				if ((Object)(object)val != (Object)null && ((Object)val).name == __instance.AssetGUID)
				{
					__result = true;
				}
			}
		}
	}
}
