using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModSettings;

public class ModSettingsMain : MelonMod
{
	public override void OnInitializeMelon()
	{
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		if (!sceneName.Contains("MainMenu"))
		{
			return;
		}
		Camera mainCamera = GameManager.GetMainCamera();
		if ((Object)(object)mainCamera != (Object)null)
		{
			if (!Object.op_Implicit((Object)(object)((Component)mainCamera).gameObject.GetComponent<EventSystem>()))
			{
				((Component)mainCamera).gameObject.AddComponent<EventSystem>();
			}
			if (!Object.op_Implicit((Object)(object)((Component)mainCamera).gameObject.GetComponent<StandaloneInputModule>()))
			{
				((Component)mainCamera).gameObject.AddComponent<StandaloneInputModule>();
			}
		}
	}
}
