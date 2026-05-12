using System.IO;
using System.Reflection;
using AudioMgr;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		TweenFactory.SceneManagerSceneLoaded();
		if (!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu") && !Object.op_Implicit((Object)(object)eventSystem))
		{
			eventCam = GameManager.GetMainCamera();
			eventSystemObject = new GameObject("EventSystem");
			eventSystem = ((Component)eventCam).gameObject.AddComponent<EventSystem>();
			standaloneInputModule = ((Component)eventCam).gameObject.AddComponent<StandaloneInputModule>();
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton) && Physics.Raycast(((Component)GameManager.GetMainCamera()).transform.position, ((Component)GameManager.GetMainCamera()).transform.TransformDirection(Vector3.forward), ref hit, 2f, layerMask))
		{
			GameObject gameObject = ((Component)((RaycastHit)(ref hit)).collider).gameObject;
			if (((Object)gameObject).name == "PowerButton")
			{
				TVButton component = ((Component)gameObject.transform).GetComponent<TVButton>();
				if ((Object)(object)component != (Object)null)
				{
					component.TogglePower();
				}
			}
		}
		if (TVLock.lockedInTVView)
		{
			((Renderer)TVLock.currentManager.objectRenderer).enabled = true;
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)27) || InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
			{
				TVLock.ExitTVView();
			}
		}
		else
		{
			if (TVLock.lockedInTVView || !InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton) || !Physics.Raycast(((Component)GameManager.GetMainCamera()).transform.position, ((Component)GameManager.GetMainCamera()).transform.TransformDirection(Vector3.forward), ref hit, 2f, layerMask))
			{
				return;
			}
			GameObject gameObject2 = ((Component)((RaycastHit)(ref hit)).collider).gameObject;
			string name = ((Object)gameObject2).name;
			if (name.Contains("OBJ_TelevisionB_LOD0") || name.Contains("OBJ_Television_LOD0") || name.Contains("GEAR_TV_LCD") || name.Contains("GEAR_TV_CRT") || name.Contains("GEAR_TV_WALL"))
			{
				TVManager component2 = ((Component)gameObject2.transform).GetComponent<TVManager>();
				if ((Object)(object)component2 != (Object)null)
				{
					TVLock.EnterTVView(component2);
				}
			}
		}
	}

	public static void LoadEmbeddedAssetBundle()
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		Stream? manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthernLightsBroadcast.Resources.northernlightsbroadcastbundle");
		MemoryStream memoryStream = new MemoryStream((int)manifestResourceStream.Length);
		manifestResourceStream.CopyTo(memoryStream);
		assetBundle = AssetBundle.LoadFromMemory(Il2CppStructArray<byte>.op_Implicit(memoryStream.ToArray()));
		NLB_TV_CRT = assetBundle.LoadAsset<GameObject>("NLB_TV_CRT");
		((Object)NLB_TV_CRT).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)NLB_TV_CRT);
		NLB_TV_LCD = assetBundle.LoadAsset<GameObject>("NLB_TV_LCD");
		((Object)NLB_TV_LCD).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)NLB_TV_LCD);
		NLB_TV_WALL = assetBundle.LoadAsset<GameObject>("NLB_TV_WALL");
		((Object)NLB_TV_WALL).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)NLB_TV_WALL);
		TelevisionB_Material_Cutout = assetBundle.LoadAsset<Material>("MaterialTelevisionB");
		if ((Object)(object)TelevisionB_Material_Cutout == (Object)null)
		{
			MelonLogger.Msg("MaterialTelevisionB is null");
		}
		((Object)TelevisionB_Material_Cutout).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)TelevisionB_Material_Cutout);
		folderIcon = assetBundle.LoadAsset<Texture2D>("icon_folder");
		folderIconSprite = Sprite.Create(folderIcon, new Rect(0f, 0f, (float)((Texture)folderIcon).width, (float)((Texture)folderIcon).height), new Vector2(0.5f, 0.5f));
		((Object)folderIconSprite).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)folderIconSprite);
		audioIcon = assetBundle.LoadAsset<Texture2D>("icon_audio");
		audioIconSprite = Sprite.Create(audioIcon, new Rect(0f, 0f, (float)((Texture)audioIcon).width, (float)((Texture)audioIcon).height), new Vector2(0.5f, 0.5f));
		((Object)audioIconSprite).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)audioIconSprite);
		videoIcon = assetBundle.LoadAsset<Texture2D>("icon_video");
		videoIconSprite = Sprite.Create(videoIcon, new Rect(0f, 0f, (float)((Texture)videoIcon).width, (float)((Texture)videoIcon).height), new Vector2(0.5f, 0.5f));
		((Object)videoIconSprite).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)videoIconSprite);
		tvAudioManager = AudioMaster.NewClipManager();
		tvAudioManager.LoadClipFromBundle("click", "audiobuttonclick", assetBundle);
		tvAudioManager.LoadClipFromBundle("static", "audiotvstatic", assetBundle);
	}
}
