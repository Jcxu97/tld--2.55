using System.IO;
using System.Reflection;
using AudioMgr;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NorthernLightsBroadcast;

public class NorthernLightsBroadcastMain : MelonMod
{
	public static AssetBundle assetBundle;

	public static GameObject NLB_TV_CRT;

	public static GameObject NLB_TV_LCD;

	public static GameObject NLB_TV_WALL;

	public static Material TelevisionB_Material_Cutout;

	public static ClipManager tvAudioManager;

	public static RaycastHit hit;

	public static int layerMask;

	public static Sprite folderIconSprite;

	public static Sprite audioIconSprite;

	public static Sprite videoIconSprite;

	public static Texture2D folderIcon;

	public static Texture2D audioIcon;

	public static Texture2D videoIcon;

	public static GameObject eventSystemObject;

	public static EventSystem eventSystem;

	public static StandaloneInputModule standaloneInputModule;

	public static Camera eventCam;

	public override void OnInitializeMelon()
	{
		LoadEmbeddedAssetBundle();
		layerMask |= 1;
		layerMask |= 131072;
		layerMask |= 524288;
		FileStuff.OpenFrameFile();
		Settings.OnLoad();
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		TweenFactory.SceneManagerSceneLoaded();
		if (!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu") && eventSystem == null)
		{
			eventCam = GameManager.GetMainCamera();
			if (eventCam != null)
			{
				eventSystemObject = new GameObject("EventSystem");
				eventSystem = eventCam.gameObject.AddComponent<EventSystem>();
				standaloneInputModule = eventCam.gameObject.AddComponent<StandaloneInputModule>();
			}
		}
		if (sceneName.Contains("MainMenu"))
		{
			SaveLoad.reloadPending = true;
		}
		if (!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu"))
		{
			SaveLoad.LoadTheTVs();
		}
	}

	public override void OnUpdate()
	{
		if (Settings.options == null) return;
		Camera mainCam = GameManager.GetMainCamera();
		if (mainCam == null) return;

		Vector3 camPos = mainCam.transform.position;
		Vector3 camFwd = mainCam.transform.TransformDirection(Vector3.forward);

		if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton) && Physics.Raycast(camPos, camFwd, out hit, 2f, layerMask))
		{
			Collider col = hit.collider;
			if (col != null)
			{
				GameObject gameObject = col.gameObject;
				if (gameObject != null && gameObject.name == "PowerButton")
				{
					TVButton component = gameObject.GetComponent<TVButton>();
					if (component != null)
					{
						component.TogglePower();
					}
				}
			}
		}
		if (TVLock.lockedInTVView)
		{
			if (TVLock.currentManager != null && TVLock.currentManager.objectRenderer != null)
			{
				TVLock.currentManager.objectRenderer.enabled = true;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Escape) || InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
			{
				TVLock.ExitTVView();
			}
		}
		else
		{
			if (!InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton) || !Physics.Raycast(camPos, camFwd, out hit, 2f, layerMask))
			{
				return;
			}
			Collider col2 = hit.collider;
			if (col2 == null) return;
			GameObject gameObject2 = col2.gameObject;
			if (gameObject2 == null) return;
			string name = gameObject2.name;
			if (name.Contains("OBJ_TelevisionB_LOD0") || name.Contains("OBJ_Television_LOD0") || name.Contains("GEAR_TV_LCD") || name.Contains("GEAR_TV_CRT") || name.Contains("GEAR_TV_WALL"))
			{
				TVManager component2 = gameObject2.GetComponent<TVManager>();
				if (component2 != null)
				{
					TVLock.EnterTVView(component2);
				}
			}
		}
	}

	public static void LoadEmbeddedAssetBundle()
	{
		// v2.0.6.1: AssetBundle.LoadFromMemory 在 IL2CppInterop 2.x 上会因临时 Il2CppStructArray 被 GC 而崩,
		// 改用 LoadFromFile 走临时文件路径避免 GC 问题
		string tempPath = Path.Combine(Path.GetTempPath(), "NLB_bundle_" + System.Guid.NewGuid().ToString("N") + ".unity3d");
		using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthernLightsBroadcast.Resources.northernlightsbroadcastbundle"))
		{
			if (manifestResourceStream == null)
			{
				MelonLogger.Error("[NLB] Embedded asset bundle resource not found");
				return;
			}
			using FileStream fs = File.Create(tempPath);
			manifestResourceStream.CopyTo(fs);
		}

		assetBundle = AssetBundle.LoadFromFile(tempPath);
		try { File.Delete(tempPath); } catch { }
		if (assetBundle == null)
		{
			MelonLogger.Error("[NLB] AssetBundle.LoadFromFile returned null");
			return;
		}
		Object.DontDestroyOnLoad(assetBundle);

		NLB_TV_CRT = assetBundle.LoadAsset<GameObject>("NLB_TV_CRT");
		if (NLB_TV_CRT != null)
		{
			NLB_TV_CRT.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(NLB_TV_CRT);
		}
		NLB_TV_LCD = assetBundle.LoadAsset<GameObject>("NLB_TV_LCD");
		if (NLB_TV_LCD != null)
		{
			NLB_TV_LCD.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(NLB_TV_LCD);
		}
		NLB_TV_WALL = assetBundle.LoadAsset<GameObject>("NLB_TV_WALL");
		if (NLB_TV_WALL != null)
		{
			NLB_TV_WALL.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(NLB_TV_WALL);
		}
		TelevisionB_Material_Cutout = assetBundle.LoadAsset<Material>("MaterialTelevisionB");
		if (TelevisionB_Material_Cutout == null)
		{
			MelonLogger.Msg("MaterialTelevisionB is null");
		}
		else
		{
			TelevisionB_Material_Cutout.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(TelevisionB_Material_Cutout);
		}
		folderIcon = assetBundle.LoadAsset<Texture2D>("icon_folder");
		if (folderIcon != null)
		{
			folderIconSprite = Sprite.Create(folderIcon, new Rect(0f, 0f, folderIcon.width, folderIcon.height), new Vector2(0.5f, 0.5f));
			folderIconSprite.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(folderIconSprite);
		}
		audioIcon = assetBundle.LoadAsset<Texture2D>("icon_audio");
		if (audioIcon != null)
		{
			audioIconSprite = Sprite.Create(audioIcon, new Rect(0f, 0f, audioIcon.width, audioIcon.height), new Vector2(0.5f, 0.5f));
			audioIconSprite.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(audioIconSprite);
		}
		videoIcon = assetBundle.LoadAsset<Texture2D>("icon_video");
		if (videoIcon != null)
		{
			videoIconSprite = Sprite.Create(videoIcon, new Rect(0f, 0f, videoIcon.width, videoIcon.height), new Vector2(0.5f, 0.5f));
			videoIconSprite.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(videoIconSprite);
		}
		tvAudioManager = AudioMaster.NewClipManager();
		tvAudioManager.LoadClipFromBundle("click", "audiobuttonclick", assetBundle);
		tvAudioManager.LoadClipFromBundle("static", "audiotvstatic", assetBundle);
	}
}
