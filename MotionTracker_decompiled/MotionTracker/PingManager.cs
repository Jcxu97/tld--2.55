using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.UI;

namespace MotionTracker;

public class PingManager : MonoBehaviour
{
	public enum AnimalType
	{
		Crow,
		Rabbit,
		Stag,
		Doe,
		Wolf,
		Timberwolf,
		Bear,
		Moose,
		PuffyBird,
		Cougar,
		Arrow,
		Coal,
		RawFish,
		LostAndFoundBox,
		SaltDeposit,
		BeachLoot
	}

	public static bool isVisible;

	public static PingManager? instance;

	public RectTransform iconContainer;

	public RectTransform radarUI;

	public Image backgroundImage;

	public Canvas trackerCanvas;

	public bool applyRotation = true;

	public static bool inMenu;

	private float timer;

	private float triggerTime = 5f;

	public Vector3 lastTransformPosition = Vector3.zero;

	public int stuckPositionCounter;

	public Dictionary<int, Vector3> iconPosition = new Dictionary<int, Vector3>();

	public PingManager(IntPtr intPtr)
		: base(intPtr)
	{
	}//IL_0013: Unknown result type (might be due to invalid IL or missing references)
	//IL_0018: Unknown result type (might be due to invalid IL or missing references)


	public void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
	{
	}

	public void ClearIcons()
	{
		Image[] array = Il2CppArrayBase<Image>.op_Implicit(((Component)((Component)iconContainer).transform).GetComponentsInChildren<Image>());
		foreach (Image obj in array)
		{
			_ = (Object)(object)((Component)obj).gameObject == (Object)null;
			Object.Destroy((Object)(object)((Component)obj).gameObject);
		}
		iconPosition.Clear();
	}

	public void Update()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		if (timer >= triggerTime)
		{
			int num = 0;
			Image[] array = Il2CppArrayBase<Image>.op_Implicit(((Component)((Component)iconContainer).transform).GetComponentsInChildren<Image>());
			foreach (Image val in array)
			{
				if ((Object)(object)val != (Object)null && ((Object)val).name.Contains("crow", StringComparison.CurrentCultureIgnoreCase))
				{
					if (!iconPosition.TryGetValue(((Object)((Component)val).gameObject).GetInstanceID(), out lastTransformPosition))
					{
						lastTransformPosition = Vector3.zero;
					}
					if (lastTransformPosition == ((Component)val).gameObject.transform.position)
					{
						stuckPositionCounter++;
						string[] obj = new string[9]
						{
							"Stale icon position detected!  icon # ",
							num.ToString(),
							" GameObject:Position (",
							((Object)((Component)val).gameObject).name,
							":",
							null,
							null,
							null,
							null
						};
						Vector3 position = ((Component)val).gameObject.transform.position;
						obj[5] = ((object)(Vector3)(ref position)).ToString();
						obj[6] = ") is the same as last position (";
						position = lastTransformPosition;
						obj[7] = ((object)(Vector3)(ref position)).ToString();
						obj[8] = ") so deleting it.";
						LogMessage(string.Concat(obj), 146, "Update", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingManager.cs");
						if (iconPosition.Remove(((Object)((Component)val).gameObject).GetInstanceID()))
						{
							LogMessage("Removed key/value (" + ((Object)((Component)val).gameObject).name + ":" + ((Object)((Component)val).gameObject).GetInstanceID() + ") from iconPosition dictionary.", 151, "Update", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingManager.cs");
						}
						Object.Destroy((Object)(object)((Component)val).gameObject);
					}
					else
					{
						stuckPositionCounter = 0;
						if (iconPosition.ContainsKey(((Object)((Component)val).gameObject).GetInstanceID()))
						{
							iconPosition[((Object)((Component)val).gameObject).GetInstanceID()] = ((Component)val).gameObject.transform.position;
						}
						else
						{
							iconPosition[((Object)((Component)val).gameObject).GetInstanceID()] = ((Component)val).gameObject.transform.position;
						}
					}
				}
				num++;
			}
		}
		if (AllowedToBeVisible())
		{
			SetVisible(visible: true);
		}
		else
		{
			SetVisible(visible: false);
		}
		if (timer >= triggerTime)
		{
			timer = 0f;
		}
	}

	public bool AllowedToBeVisible()
	{
		if (!Settings.options.enableMotionTracker)
		{
			return false;
		}
		if (!Object.op_Implicit((Object)(object)MotionTrackerMain.modSettingPage))
		{
			MotionTrackerMain.modSettingPage = GameObject.Find("Mod settings grid (Motion Tracker)");
		}
		if (Object.op_Implicit((Object)(object)MotionTrackerMain.modSettingPage) && MotionTrackerMain.modSettingPage.active)
		{
			return true;
		}
		if (inMenu)
		{
			return false;
		}
		if (Settings.options.displayStyle == Settings.DisplayStyle.Toggle && !Settings.toggleBool)
		{
			return false;
		}
		if (!Object.op_Implicit((Object)(object)GameManager.GetVpFPSPlayer()))
		{
			return false;
		}
		if (!Object.op_Implicit((Object)(object)GameManager.GetWeatherComponent()))
		{
			return false;
		}
		if (Settings.options.onlyOutdoors && GameManager.GetWeatherComponent().IsIndoorEnvironment())
		{
			return false;
		}
		return true;
	}

	private void SetVisible(bool visible)
	{
		if (isVisible != visible)
		{
			if (visible)
			{
				((Behaviour)trackerCanvas).enabled = true;
				isVisible = true;
			}
			else
			{
				((Behaviour)trackerCanvas).enabled = false;
				isVisible = false;
			}
		}
	}

	public void Awake()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		instance = this;
		trackerCanvas = ((Component)MotionTrackerMain.trackerObject.transform.FindChild("Canvas")).GetComponent<Canvas>();
		radarUI = ((Component)((Component)trackerCanvas).transform.FindChild("RadarUI")).GetComponent<RectTransform>();
		((Transform)radarUI).localScale = new Vector3(Settings.options.scale, Settings.options.scale, Settings.options.scale);
		iconContainer = ((Component)((Component)radarUI).transform.FindChild("IconContainer")).GetComponent<RectTransform>();
		backgroundImage = ((Component)((Component)radarUI).transform.FindChild("Background")).GetComponent<Image>();
		((Graphic)backgroundImage).color = new Color(1f, 1f, 1f, Settings.options.opacity);
		SetOpacity(Settings.options.opacity);
		Scale(Settings.options.scale);
		((Behaviour)trackerCanvas).enabled = true;
		isVisible = true;
	}

	public void Scale(float scale)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)radarUI))
		{
			((Transform)radarUI).localScale = new Vector3(scale, scale, scale);
		}
	}

	public void SetOpacity(float opacity)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)backgroundImage))
		{
			((Graphic)backgroundImage).color = new Color(1f, 1f, 1f, opacity);
		}
	}
}
