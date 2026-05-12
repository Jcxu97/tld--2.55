using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MotionTracker;

internal class SpawnUtils
{
	internal static List<GameObject> GetRootObjects()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			MyLogger.LogMessage("SpawnUtils GetRootObjects: Scene (" + ((Scene)(ref sceneAt)).name + ").", 83, "GetRootObjects", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit((Il2CppArrayBase<GameObject>)(object)((Scene)(ref sceneAt)).GetRootGameObjects());
			foreach (GameObject val in array)
			{
				list.Add(val);
				string[] obj = new string[7]
				{
					"SpawnUtils GetRootObjects: (",
					((Object)val).name,
					":",
					((Object)val).GetInstanceID().ToString(),
					") at [",
					null,
					null
				};
				Vector3 position = val.transform.position;
				obj[5] = ((object)(Vector3)(ref position)).ToString();
				obj[6] = "].";
				MyLogger.LogMessage(string.Concat(obj), 91, "GetRootObjects", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			}
		}
		return list;
	}

	internal static void GetChildren(GameObject obj, List<GameObject> result)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (obj.transform.childCount > 0)
		{
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				GameObject gameObject = ((Component)obj.transform.GetChild(i)).gameObject;
				result.Add(gameObject);
				string[] obj2 = new string[9]
				{
					"SpawnUtils GetChildren: (",
					((Object)gameObject).name,
					":",
					((Object)gameObject).GetInstanceID().ToString(),
					") at [",
					null,
					null,
					null,
					null
				};
				Vector3 position = gameObject.transform.position;
				obj2[5] = ((object)(Vector3)(ref position)).ToString();
				obj2[6] = "] activeSelf=";
				obj2[7] = gameObject.activeSelf.ToString();
				obj2[8] = ".";
				MyLogger.LogMessage(string.Concat(obj2), 108, "GetChildren", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
				GetChildren(gameObject, result);
			}
		}
	}
}
