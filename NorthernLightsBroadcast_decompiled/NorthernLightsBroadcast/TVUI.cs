using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NorthernLightsBroadcast;

[RegisterTypeInIl2Cpp]
public class TVUI : MonoBehaviour
{
	public Canvas canvas;

	public CanvasGroup canvasGroup;

	public TVManager manager;

	public bool isSetup;

	public GameObject screenPlayback;

	public GameObject screenOff;

	public GameObject screenStatic;

	public GameObject screenError;

	public GameObject screenLoading;

	public GameObject osdAudio;

	public GameObject osdButtons;

	public GameObject osdFileMenu;

	public GameObject OSD;

	public bool OSDOpen;

	public bool isFading;

	public bool fileBrowserOpen;

	public Slider audioSlider;

	public Button muteButton;

	public Button playButton;

	public Button pauseButton;

	public Button stopButton;

	public Button nextButton;

	public Button prevButton;

	public Button fileBrowserButton;

	public Button uiActivator;

	public Slider progressBar;

	public Button pageNext;

	public Button pagePrev;

	public TextMeshProUGUI playingNowText;

	public TextMeshProUGUI timeText;

	public TextMeshProUGUI currentDir;

	public TextMeshProUGUI errorText;

	public TextMeshProUGUI pageText;

	public Button fileBrowserUpButton;

	public Button[] listButtons = (Button[])(object)new Button[8];

	public TextMeshProUGUI[] listText = (TextMeshProUGUI[])(object)new TextMeshProUGUI[8];

	public TextMeshProUGUI[] listLength = (TextMeshProUGUI[])(object)new TextMeshProUGUI[8];

	public Image[] listSprites = (Image[])(object)new Image[8];

	public GameObject[] driveButtonObjects = (GameObject[])(object)new GameObject[8];

	public Button[] driveButtons = (Button[])(object)new Button[8];

	public string currentFolder = Application.dataPath + "/../Mods";

	public string currentClip;

	public int currentClipIndex;

	public Dictionary<string, bool> folderContents = new Dictionary<string, bool>();

	public int currentPage;

	public int currentPageCount = 1;

	private Color32 folderColor = new Color32((byte)90, (byte)187, (byte)248, byte.MaxValue);

	private Color32 audioColor = new Color32(byte.MaxValue, (byte)226, (byte)115, byte.MaxValue);

	private Color32 videoColor = new Color32((byte)112, (byte)217, (byte)81, byte.MaxValue);

	public TVUI(IntPtr intPtr)
		: base(intPtr)
	{
	}//IL_0081: Unknown result type (might be due to invalid IL or missing references)
	//IL_0086: Unknown result type (might be due to invalid IL or missing references)
	//IL_009d: Unknown result type (might be due to invalid IL or missing references)
	//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
	//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
	//IL_00bb: Unknown result type (might be due to invalid IL or missing references)


	public void Awake()
	{
		if (isSetup)
		{
			return;
		}
		manager = ((Component)this).gameObject.GetComponent<TVManager>();
		screenPlayback = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenPlaybackMesh")).gameObject;
		screenOff = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenOff")).gameObject;
		screenStatic = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenStatic")).gameObject;
		screenLoading = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenLoading")).gameObject;
		screenError = ((Component)((Component)this).gameObject.transform.Find("NLB_TV/Screens/ScreenError")).gameObject;
		errorText = ((Component)((Component)this).gameObject.transform.Find("NLB_TV/Screens/ScreenError/ErrorWindow/Message")).GetComponent<TextMeshProUGUI>();
		OSD = ((Component)((Component)this).gameObject.transform.Find("NLB_TV/OSD")).gameObject;
		canvas = OSD.GetComponent<Canvas>();
		canvasGroup = OSD.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		osdAudio = ((Component)OSD.transform.Find("Audio")).gameObject;
		osdButtons = ((Component)OSD.transform.Find("Buttons")).gameObject;
		osdFileMenu = ((Component)OSD.transform.Find("FileMenu")).gameObject;
		osdFileMenu.SetActive(false);
		uiActivator = ((Component)OSD.transform.Find("UIActivator")).GetComponent<Button>();
		((UnityEvent)uiActivator.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			OSDActivationButton();
		}));
		pageNext = ((Component)osdFileMenu.transform.Find("Pagestuff/ButtonRight")).GetComponent<Button>();
		((UnityEvent)pageNext.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			NextPage();
		}));
		pagePrev = ((Component)osdFileMenu.transform.Find("Pagestuff/ButtonLeft")).GetComponent<Button>();
		((UnityEvent)pagePrev.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			PrevPage();
		}));
		pageText = ((Component)osdFileMenu.transform.Find("Pagestuff/Pagenumber")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)pageText).text = "1 / 1";
		currentDir = ((Component)osdFileMenu.transform.Find("CurrentDir")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)currentDir).text = currentFolder;
		fileBrowserUpButton = ((Component)osdFileMenu.transform.Find("DirUp")).GetComponent<Button>();
		((UnityEvent)fileBrowserUpButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			UpDir();
		}));
		audioSlider = ((Component)osdAudio.transform.Find("Slider")).GetComponent<Slider>();
		audioSlider.value = SaveLoad.GetVolume(manager.thisGuid);
		manager.playerAudio._audioSource.volume = audioSlider.value;
		manager.staticAudio._audioSource.volume = audioSlider.value;
		((UnityEvent<float>)(object)audioSlider.onValueChanged).AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>((Delegate)(Action<float>)delegate
		{
			VolumeSlider();
		}));
		muteButton = ((Component)osdAudio.transform.Find("Mute")).GetComponent<Button>();
		((UnityEvent)muteButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			Mute();
		}));
		playingNowText = ((Component)osdButtons.transform.Find("PlayingNow")).GetComponent<TextMeshProUGUI>();
		timeText = ((Component)osdButtons.transform.Find("Time")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)timeText).text = "00:00:00";
		((TMP_Text)playingNowText).text = "Stopped";
		playButton = ((Component)osdButtons.transform.Find("PlayButtons/Play")).GetComponent<Button>();
		((UnityEvent)playButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			Prepare();
		}));
		pauseButton = ((Component)osdButtons.transform.Find("PlayButtons/Pause")).GetComponent<Button>();
		((UnityEvent)pauseButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			Pause();
		}));
		stopButton = ((Component)osdButtons.transform.Find("PlayButtons/Stop")).GetComponent<Button>();
		((UnityEvent)stopButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			Stop();
		}));
		nextButton = ((Component)osdButtons.transform.Find("PlayButtons/Next")).GetComponent<Button>();
		((UnityEvent)nextButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			NextClip();
		}));
		prevButton = ((Component)osdButtons.transform.Find("PlayButtons/Prev")).GetComponent<Button>();
		((UnityEvent)prevButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			PrevClip();
		}));
		fileBrowserButton = ((Component)osdButtons.transform.Find("PlayButtons/Browser")).GetComponent<Button>();
		((UnityEvent)fileBrowserButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			FileMenu();
		}));
		progressBar = ((Component)osdButtons.transform.Find("ProgressBar/Slider")).GetComponent<Slider>();
		progressBar.value = 0f;
		((UnityEvent<float>)(object)progressBar.onValueChanged).AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>((Delegate)(Action<float>)delegate
		{
			ProgressBar();
		}));
		listButtons[0] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line1")).GetComponent<Button>();
		((UnityEvent)listButtons[0].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(0);
		}));
		listButtons[1] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line2")).GetComponent<Button>();
		((UnityEvent)listButtons[1].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(1);
		}));
		listButtons[2] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line3")).GetComponent<Button>();
		((UnityEvent)listButtons[2].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(2);
		}));
		listButtons[3] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line4")).GetComponent<Button>();
		((UnityEvent)listButtons[3].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(3);
		}));
		listButtons[4] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line5")).GetComponent<Button>();
		((UnityEvent)listButtons[4].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(4);
		}));
		listButtons[5] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line6")).GetComponent<Button>();
		((UnityEvent)listButtons[5].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(5);
		}));
		listButtons[6] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line7")).GetComponent<Button>();
		((UnityEvent)listButtons[6].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(6);
		}));
		listButtons[7] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line8")).GetComponent<Button>();
		((UnityEvent)listButtons[7].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
		{
			ItemButtom(7);
		}));
		listSprites[0] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line1/Icon")).GetComponent<Image>();
		listSprites[1] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line2/Icon")).GetComponent<Image>();
		listSprites[2] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line3/Icon")).GetComponent<Image>();
		listSprites[3] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line4/Icon")).GetComponent<Image>();
		listSprites[4] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line5/Icon")).GetComponent<Image>();
		listSprites[5] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line6/Icon")).GetComponent<Image>();
		listSprites[6] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line7/Icon")).GetComponent<Image>();
		listSprites[7] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line8/Icon")).GetComponent<Image>();
		listText[0] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line1/Text")).GetComponent<TextMeshProUGUI>();
		listText[1] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line2/Text")).GetComponent<TextMeshProUGUI>();
		listText[2] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line3/Text")).GetComponent<TextMeshProUGUI>();
		listText[3] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line4/Text")).GetComponent<TextMeshProUGUI>();
		listText[4] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line5/Text")).GetComponent<TextMeshProUGUI>();
		listText[5] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line6/Text")).GetComponent<TextMeshProUGUI>();
		listText[6] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line7/Text")).GetComponent<TextMeshProUGUI>();
		listText[7] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line8/Text")).GetComponent<TextMeshProUGUI>();
		listLength[0] = ((Component)osdFileMenu.transform.Find("ContentTime/Line1/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[0]).text = " ";
		listLength[1] = ((Component)osdFileMenu.transform.Find("ContentTime/Line2/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[1]).text = " ";
		listLength[2] = ((Component)osdFileMenu.transform.Find("ContentTime/Line3/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[2]).text = " ";
		listLength[3] = ((Component)osdFileMenu.transform.Find("ContentTime/Line4/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[3]).text = " ";
		listLength[4] = ((Component)osdFileMenu.transform.Find("ContentTime/Line5/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[4]).text = "   ";
		listLength[5] = ((Component)osdFileMenu.transform.Find("ContentTime/Line6/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[5]).text = "  ";
		listLength[6] = ((Component)osdFileMenu.transform.Find("ContentTime/Line7/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[6]).text = "  ";
		listLength[7] = ((Component)osdFileMenu.transform.Find("ContentTime/Line8/Text")).GetComponent<TextMeshProUGUI>();
		((TMP_Text)listLength[7]).text = "  ";
		DriveInfo[] drives = DriveInfo.GetDrives();
		for (int i = 0; i < 8; i++)
		{
			driveButtonObjects[i] = ((Component)osdFileMenu.transform.Find("ContentDrivelist/Line" + (i + 1))).gameObject;
			if (i < drives.Length)
			{
				driveButtons[i] = driveButtonObjects[i].GetComponent<Button>();
				driveButtonObjects[i].SetActive(true);
				string driveletter = drives[i].Name.ToString();
				((UnityEvent)driveButtons[i].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
				{
					DriveButton(driveletter);
				}));
				((TMP_Text)((Component)driveButtons[i]).GetComponentInChildren<TextMeshProUGUI>()).text = driveletter;
			}
			else
			{
				driveButtonObjects[i].SetActive(false);
			}
		}
		isSetup = true;
	}

	[HideFromIl2Cpp]
	public void PopulateFiles()
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		string[] filesInPath = FileStuff.GetFilesInPath(currentFolder);
		string[] foldersInPath = FileStuff.GetFoldersInPath(currentFolder);
		SaveLoad.SetFolder(manager.thisGuid, currentFolder);
		folderContents = new Dictionary<string, bool>();
		string[] array = foldersInPath;
		foreach (string key in array)
		{
			folderContents.Add(key, value: true);
		}
		array = filesInPath;
		foreach (string key2 in array)
		{
			folderContents.Add(key2, value: false);
		}
		currentPageCount = (int)Math.Ceiling((double)folderContents.Count / 8.0);
		for (int j = 0; j < 8; j++)
		{
			if (j + currentPage * 8 < folderContents.Count)
			{
				((Component)listButtons[j]).gameObject.active = true;
				((TMP_Text)listText[j]).text = Path.GetFileName(folderContents.ElementAt(j + currentPage * 8).Key);
				if (folderContents.ElementAt(j + currentPage * 8).Value)
				{
					((Graphic)listText[j]).color = Color32.op_Implicit(folderColor);
					listSprites[j].sprite = NorthernLightsBroadcastMain.folderIconSprite;
				}
				else
				{
					((Graphic)listText[j]).color = Color32.op_Implicit(videoColor);
					listSprites[j].sprite = NorthernLightsBroadcastMain.videoIconSprite;
				}
			}
			else
			{
				((TMP_Text)listText[j]).text = "";
				listSprites[j].sprite = null;
				((Component)listButtons[j]).gameObject.active = false;
			}
		}
	}

	[HideFromIl2Cpp]
	public IEnumerator FadeIn(float speed)
	{
		isFading = true;
		float start = 0f;
		float endAlpha = 1f;
		Action<ITween<float>> progress = delegate(ITween<float> t)
		{
			canvasGroup.alpha = t.CurrentValue;
		};
		Action<ITween<float>> completion = delegate
		{
			canvasGroup.alpha = endAlpha;
			isFading = false;
		};
		((Component)canvasGroup).gameObject.Tween(((Component)canvasGroup).gameObject, start, endAlpha, speed, TweenScaleFunctions.SineEaseInOut, progress, completion);
		yield return null;
	}

	[HideFromIl2Cpp]
	public IEnumerator FadeOut(float speed)
	{
		isFading = true;
		float start = 1f;
		float endAlpha = 0f;
		Action<ITween<float>> progress = delegate(ITween<float> t)
		{
			canvasGroup.alpha = t.CurrentValue;
		};
		Action<ITween<float>> completion = delegate
		{
			canvasGroup.alpha = endAlpha;
			isFading = false;
			osdAudio.SetActive(false);
			osdFileMenu.SetActive(false);
			fileBrowserOpen = false;
		};
		((Component)canvasGroup).gameObject.Tween(((Component)canvasGroup).gameObject, start, endAlpha, speed, TweenScaleFunctions.SineEaseInOut, progress, completion);
		yield return null;
	}

	[HideFromIl2Cpp]
	public void ActivateOSD(bool value)
	{
		if (value)
		{
			OSDOpen = true;
			osdAudio.SetActive(true);
			osdButtons.SetActive(true);
			osdFileMenu.SetActive(false);
			fileBrowserOpen = false;
			MelonCoroutines.Start(FadeIn(0.5f));
		}
		else
		{
			OSDOpen = false;
			MelonCoroutines.Start(FadeOut(0.5f));
		}
	}

	[HideFromIl2Cpp]
	public void OSDActivationButton()
	{
		if (manager.currentState != 0 && !isFading)
		{
			if (manager.currentState == TVManager.TVState.Error)
			{
				manager.SwitchState(TVManager.TVState.Static);
			}
			ActivateOSD(!OSDOpen);
		}
	}

	[HideFromIl2Cpp]
	public void UpdatePage()
	{
		PopulateFiles();
		((TMP_Text)currentDir).text = currentFolder;
		int num = currentPage + 1;
		((TMP_Text)pageText).text = num + " / " + currentPageCount;
	}

	[HideFromIl2Cpp]
	public void ItemButtom(int button)
	{
		if (folderContents.ElementAt(button + currentPage * 8).Value)
		{
			currentFolder = folderContents.ElementAt(button + currentPage * 8).Key;
			currentPage = 0;
			UpdatePage();
		}
		else
		{
			currentClip = folderContents.ElementAt(button + currentPage * 8).Key;
			currentClipIndex = button + currentPage * 8;
			Prepare();
		}
	}

	[HideFromIl2Cpp]
	public void DriveButton(string driveletter)
	{
		currentFolder = driveletter;
		currentPage = 0;
		manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
		UpdatePage();
	}

	[HideFromIl2Cpp]
	public void Resume()
	{
		manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
		((TMP_Text)playingNowText).text = Path.GetFileName(currentClip);
		manager.videoPlayer.url = currentClip;
		manager.SwitchState(TVManager.TVState.Resume);
		screenLoading.SetActive(true);
	}

	[HideFromIl2Cpp]
	public void Prepare()
	{
		if (currentClip != null)
		{
			if (manager.currentState == TVManager.TVState.Paused)
			{
				manager.SwitchState(TVManager.TVState.Resume);
				return;
			}
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			((TMP_Text)playingNowText).text = Path.GetFileName(currentClip);
			manager.videoPlayer.url = currentClip;
			manager.SwitchState(TVManager.TVState.Preparing);
			screenLoading.SetActive(true);
		}
	}

	[HideFromIl2Cpp]
	public void Stop()
	{
		manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
		manager.SavePlaytime();
		manager.SwitchState(TVManager.TVState.Static);
	}

	[HideFromIl2Cpp]
	public void Pause()
	{
		manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
		manager.SavePlaytime();
		manager.SwitchState(TVManager.TVState.Paused);
	}

	[HideFromIl2Cpp]
	public void NextClip()
	{
		if (currentClipIndex < folderContents.Count - 1 && !folderContents.ElementAt(currentClipIndex + 1).Value)
		{
			currentClipIndex++;
			currentClip = folderContents.ElementAt(currentClipIndex).Key;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			Prepare();
		}
		else if (Settings.options.loopFolder)
		{
			currentClipIndex = 0;
			currentClip = folderContents.ElementAt(currentClipIndex).Key;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			Prepare();
		}
		else
		{
			manager.SwitchState(TVManager.TVState.Static);
		}
	}

	[HideFromIl2Cpp]
	public void PrevClip()
	{
		if (manager.currentState == TVManager.TVState.Playing)
		{
			if (manager.videoPlayer.time > 5.0)
			{
				manager.videoPlayer.time = 0.0;
				manager.saveTime = 0.0;
				manager.SavePlaytime();
			}
			else if (currentClipIndex > 0 && !folderContents.ElementAt(currentClipIndex - 1).Value)
			{
				currentClipIndex--;
				currentClip = folderContents.ElementAt(currentClipIndex).Key;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				Prepare();
			}
		}
		if (currentClipIndex > 0 && !folderContents.ElementAt(currentClipIndex - 1).Value)
		{
			currentClipIndex--;
			currentClip = folderContents.ElementAt(currentClipIndex).Key;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			Prepare();
		}
	}

	[HideFromIl2Cpp]
	public void FileMenu()
	{
		manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
		if (fileBrowserOpen)
		{
			osdFileMenu.SetActive(false);
			fileBrowserOpen = false;
		}
		else
		{
			osdFileMenu.SetActive(true);
			UpdatePage();
			fileBrowserOpen = true;
		}
	}

	[HideFromIl2Cpp]
	public void UpDir()
	{
		if (Directory.GetParent(currentFolder) != null)
		{
			currentFolder = Directory.GetParent(currentFolder).FullName;
			currentPage = 0;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			UpdatePage();
		}
	}

	[HideFromIl2Cpp]
	public void Mute()
	{
		manager.playerAudio._audioSource.mute = !manager.playerAudio._audioSource.mute;
		manager.staticAudio._audioSource.mute = !manager.staticAudio._audioSource.mute;
	}

	[HideFromIl2Cpp]
	public void NextPage()
	{
		if (currentPage < currentPageCount - 1)
		{
			currentPage++;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			UpdatePage();
		}
	}

	[HideFromIl2Cpp]
	public void PrevPage()
	{
		if (currentPage > 0)
		{
			currentPage--;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			UpdatePage();
		}
	}

	[HideFromIl2Cpp]
	public void VolumeSlider()
	{
		manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
		manager.playerAudio.SetVolume(audioSlider.value);
		manager.staticAudio.SetVolume(audioSlider.value);
		SaveLoad.SetVolume(manager.thisGuid, audioSlider.value);
	}

	[HideFromIl2Cpp]
	public void ProgressBar()
	{
	}

	public void Update()
	{
	}
}
