using HarmonyLib;
using Il2Cpp;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.SceneLoader;

public static class Preloader
{
	[HarmonyPatch(typeof(GameManager), "Awake")]
	internal static class GameManager_Awake
	{
		private static void Postfix(GameManager __instance)
		{
			if (GameManager.m_ActiveScene == "MainMenu")
			{
				Initialize(__instance);
			}
			if ((Object)(object)gameManagerObjectPrefab == (Object)null)
			{
				Logger.LogError("The GameManager prefab was destroyed!!!!!!!!!!!!");
			}
		}
	}

	private static bool initialized;

	internal static GameObject? gameManagerObjectPrefab;

	private static void Initialize(GameManager gameManager)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (!initialized && !((Object)(object)gameManager == (Object)null))
		{
			gameManagerObjectPrefab = new GameObject();
			Object.DontDestroyOnLoad((Object)(object)gameManagerObjectPrefab);
			gameManagerObjectPrefab.SetActive(false);
			((Object)gameManagerObjectPrefab).name = ((Object)gameManager).name;
			CopyFieldHandler.CopyFieldsIl2Cpp<GameManager>(gameManagerObjectPrefab.AddComponent<GameManager>(), gameManager);
			initialized = true;
		}
	}

	public static void InstantiateGameManager()
	{
		if ((Object)(object)gameManagerObjectPrefab == (Object)null)
		{
			Logger.LogError("gameManagerObjectPrefab == null!");
			return;
		}
		Logger.Log("instantiate");
		Object.Instantiate<GameObject>(gameManagerObjectPrefab).SetActive(true);
	}
}
