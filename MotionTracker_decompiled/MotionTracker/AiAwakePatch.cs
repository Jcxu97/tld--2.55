using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BaseAi), "Start")]
public class AiAwakePatch
{
	public static void Postfix(ref BaseAi __instance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Invalid comparison between Unknown and I4
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Invalid comparison between Unknown and I4
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Invalid comparison between Unknown and I4
		if ((int)__instance.m_CurrentMode != 2 && (int)__instance.m_CurrentMode != 27 && (int)__instance.m_CurrentMode != 0)
		{
			if ((int)__instance.m_AiSubType == 5)
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Moose);
			}
			else if ((int)__instance.m_AiSubType == 2)
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Bear);
			}
			else if ((int)__instance.m_AiSubType == 6)
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Cougar);
			}
			else if ((int)__instance.m_AiSubType == 1 && ((Object)((Component)__instance).gameObject).name.ToLower().Contains("grey"))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Timberwolf);
			}
			else if ((int)__instance.m_AiSubType == 1)
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Wolf);
			}
			else if ((int)__instance.m_AiSubType == 3 && !((Object)((Component)__instance).gameObject).name.Contains("_Doe"))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Stag);
			}
			else if ((int)__instance.m_AiSubType == 3 && ((Object)((Component)__instance).gameObject).name.Contains("_Doe"))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Doe);
			}
			else if ((int)__instance.m_SnowImprintType == 8)
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.PuffyBird);
			}
			else if ((int)__instance.m_AiSubType == 4)
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Rabbit);
			}
		}
	}
}
