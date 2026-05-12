using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace Shotgun;

[RegisterTypeInIl2Cpp(false)]
public class CustomAmmoHUD : MonoBehaviour
{
	public class HUDPreset
	{
		public Sprite LoadedSprite;

		public Sprite UnloadedSprite;

		public float IconHeight;

		public float IconWidth;

		public float IconSpacing;

		public int IconsPerRow;
	}

	private static CustomAmmoHUD _Current;

	private Dictionary<string, HUDPreset> mHUDPresets = new Dictionary<string, HUDPreset>();

	private GameObject mCanvasObject;

	private Canvas mCanvas;

	private CanvasScaler mCanvasScaler;

	private GameObject mIconPanel;

	private GridLayoutGroup mGridLayoutGroup;

	private RectTransform mIconPanelRect;

	private List<Image> mIcons = new List<Image>();

	private string mCurrentHUDPreset;

	private int mLastAmmoCount;

	private float mSinceLastCheck = 0f;

	private GearItem mItemInHands;

	private GunItem mGunInHands;

	private GearItemData mLoadedAmmo;

	public static CustomAmmoHUD Current => GetOrCreteCustomAmmoHUD();

	public string CurrentHUDPreset => mCurrentHUDPreset;

	public GearItem ItemInHands => mItemInHands;

	public GunItem GunInHands => mGunInHands;

	public GearItemData LoadedAmmo => mLoadedAmmo;

	private static CustomAmmoHUD GetOrCreteCustomAmmoHUD()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((UObject)(object)_Current != (UObject)null)
		{
			return _Current;
		}
		GameObject val = new GameObject("CustomAmmoHUD");
		_Current = val.AddComponent<CustomAmmoHUD>();
		CustomAmmoHUD current = _Current;
		((UObject)current).hideFlags = ((UObject)current).hideFlags | (HideFlags)0x20;
		UObject.DontDestroyOnLoad((UObject)(object)val);
		return _Current;
	}

	public CustomAmmoHUD()
	{
	}

	public CustomAmmoHUD(IntPtr pointer)
		: base(pointer)
	{
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		mCanvasObject = new GameObject("Canvas");
		mCanvasObject.transform.SetParent(((Component)this).transform, false);
		RectTransform val = mCanvasObject.AddComponent<RectTransform>();
		val.anchorMin = Vector2.zero;
		val.anchorMax = Vector2.one;
		val.offsetMin = Vector2.zero;
		val.offsetMax = Vector2.zero;
		mCanvas = mCanvasObject.AddComponent<Canvas>();
		mCanvas.renderMode = (RenderMode)0;
		mCanvas.sortingOrder = 1000;
		mCanvasScaler = mCanvasObject.AddComponent<CanvasScaler>();
		mCanvasScaler.uiScaleMode = (CanvasScaler.ScaleMode)1;
		mCanvasScaler.referenceResolution = new Vector2(1920f, 1080f);
		mCanvasScaler.matchWidthOrHeight = 0.5f;
		mCanvasObject.AddComponent<GraphicRaycaster>();
		mIconPanel = new GameObject("IconPanel");
		mIconPanel.transform.SetParent(mCanvasObject.transform, false);
		mIconPanelRect = mIconPanel.AddComponent<RectTransform>();
		mIconPanelRect.anchorMin = new Vector2(1f, 0f);
		mIconPanelRect.anchorMax = new Vector2(1f, 0f);
		mIconPanelRect.pivot = new Vector2(1f, 0f);
		mGridLayoutGroup = mIconPanel.AddComponent<GridLayoutGroup>();
		mGridLayoutGroup.startCorner = (GridLayoutGroup.Corner)3;
		mGridLayoutGroup.startAxis = (GridLayoutGroup.Axis)0;
		((LayoutGroup)mGridLayoutGroup).childAlignment = (TextAnchor)8;
		mGridLayoutGroup.constraint = (GridLayoutGroup.Constraint)1;
		mLastAmmoCount = 0;
		UpdateIconPanelPosition();
	}

	public void UpdateIconPanelPosition()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (Settings.Instance == null)
		{
			return;
		}
		float num = (float)(-Screen.width) * ((float)Settings.Instance.CustomAmmoHUDXOffset / 1920f);
		float num2 = (float)Screen.height * ((float)Settings.Instance.CustomAmmoHUDYOffset / 1080f);
		mIconPanelRect.anchoredPosition = new Vector2(num, num2);
		foreach (HUDPreset value in mHUDPresets.Values)
		{
			value.IconHeight = Settings.Instance.CustomAmmoHUDIconHeight;
			value.IconWidth = value.IconHeight / 3f;
			value.IconSpacing = value.IconWidth * Settings.Instance.CustomAmmoHUDIconSpacingRatio;
		}
		Refresh();
	}

	private void OnRectTransformDimensionsChange()
	{
		UpdateIconPanelPosition();
	}

	public void Update()
	{
		mSinceLastCheck += Time.deltaTime;
		if (!(mSinceLastCheck <= 0.1f))
		{
			mSinceLastCheck = 0f;
			Refresh();
		}
	}

	public void Refresh()
	{
		if (GameManager.m_IsPaused)
		{
			Reset();
			return;
		}
		PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
		if ((UObject)(object)playerManagerComponent == (UObject)null)
		{
			Reset();
			return;
		}
		mItemInHands = playerManagerComponent.m_ItemInHands;
		if ((UObject)(object)mItemInHands == (UObject)null)
		{
			Reset();
			return;
		}
		mGunInHands = ((Component)mItemInHands).gameObject.GetComponent<GunItem>();
		if ((UObject)(object)mGunInHands == (UObject)null)
		{
			Reset();
			return;
		}
		if (!mGunInHands.TryGetSelectedAmmo(out mLoadedAmmo))
		{
			Reset();
			return;
		}
		string text = ((UObject)mItemInHands).name + "." + ((UObject)mLoadedAmmo).name;
		if (!(mCurrentHUDPreset == text) || mLastAmmoCount != mGunInHands.m_Clip.Count)
		{
			mCurrentHUDPreset = text;
			mLastAmmoCount = mGunInHands.m_Clip.Count;
			if (mHUDPresets.TryGetValue(text, out HUDPreset value))
			{
				ActivateHud(mGunInHands, value);
			}
			else
			{
				Reset();
			}
		}
	}

	private void Reset()
	{
		mCurrentHUDPreset = string.Empty;
		mLastAmmoCount = 0;
		DeactivateHUD();
	}

	public bool TryRegisterHUDPreset(string gunGearName, string ammoGearName, HUDPreset preset)
	{
		string text = gunGearName + "." + ammoGearName;
		if (mHUDPresets.Keys.Contains(text))
		{
			MelonLogger.Msg("Duplicate weapon.ammo pairing " + text + " while trying to register for custom ammo HUD, Skipping!");
			return true;
		}
		mHUDPresets.Add(text, preset);
		return true;
	}

	private static bool IsIconDead(Image icon)
	{
		if (icon == null)
		{
			return true;
		}
		try
		{
			_ = ((Component)icon).gameObject;
			return false;
		}
		catch
		{
			return true;
		}
	}

	private int PurgeDeadIcons()
	{
		int num = 0;
		for (int num2 = mIcons.Count - 1; num2 >= 0; num2--)
		{
			if (IsIconDead(mIcons[num2]))
			{
				MelonLogger.Warning($"[HUD] Purging dead icon at index {num2} (isNull={mIcons[num2] == null})");
				mIcons.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	private Image CreateIcon(int index, Sprite sprite)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject($"CustomAmmoIcon{index}");
		val.transform.SetParent(mIconPanel.transform);
		RectTransform val2 = val.AddComponent<RectTransform>();
		((Transform)val2).localScale = Vector3.one;
		val2.anchorMin = new Vector2(0.5f, 0.5f);
		val2.anchorMax = new Vector2(0.5f, 0.5f);
		val2.sizeDelta = Vector2.zero;
		Image val3 = val.AddComponent<Image>();
		val3.preserveAspect = false;
		val3.type = (Image.Type)0;
		val3.sprite = sprite;
		return val3;
	}

	private void ConfigureIcon(Image icon, Sprite sprite)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = ((Component)icon).gameObject.GetComponent<RectTransform>();
		((Transform)component).localScale = Vector3.one;
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.sizeDelta = Vector2.zero;
		icon.preserveAspect = false;
		icon.type = (Image.Type)0;
		icon.sprite = sprite;
	}

	private void EnsureIconCount(int targetCount, HUDPreset preset)
	{
		for (int i = 0; i < targetCount; i++)
		{
			Sprite sprite = ((i < mLastAmmoCount) ? preset.LoadedSprite : preset.UnloadedSprite);
			if (i < mIcons.Count)
			{
				if (IsIconDead(mIcons[i]))
				{
					mIcons[i] = CreateIcon(i, sprite);
				}
				else
				{
					ConfigureIcon(mIcons[i], sprite);
				}
			}
			else
			{
				mIcons.Add(CreateIcon(i, sprite));
			}
		}
	}

	private void TrimExcessIcons(int targetCount)
	{
		for (int num = mIcons.Count - 1; num >= targetCount; num--)
		{
			if (!IsIconDead(mIcons[num]))
			{
				UObject.Destroy((UObject)(object)((Component)mIcons[num]).gameObject);
			}
			mIcons.RemoveAt(num);
		}
	}

	private void ApplyGridLayout(HUDPreset preset)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		mGridLayoutGroup.cellSize = new Vector2(preset.IconWidth, preset.IconHeight);
		mGridLayoutGroup.spacing = new Vector2(preset.IconSpacing, preset.IconSpacing);
		mGridLayoutGroup.constraintCount = preset.IconsPerRow;
	}

	private bool IsUIAlive()
	{
		if ((UObject)(object)mIconPanel == (UObject)null)
		{
			return false;
		}
		if ((UObject)(object)mIconPanel.transform == (UObject)null)
		{
			return false;
		}
		if ((UObject)(object)mGridLayoutGroup == (UObject)null)
		{
			return false;
		}
		return true;
	}

	private void RebuildUI()
	{
		mIcons.Clear();
		if ((UObject)(object)mCanvasObject != (UObject)null)
			UObject.Destroy((UObject)(object)mCanvasObject);
		Awake();
	}

	private void ActivateHud(GunItem gun, HUDPreset preset)
	{
		if (!IsUIAlive())
		{
			RebuildUI();
		}
		PurgeDeadIcons();
		EnsureIconCount(gun.m_ClipSize, preset);
		TrimExcessIcons(gun.m_ClipSize);
		ApplyGridLayout(preset);
	}

	private void DeactivateHUD()
	{
		for (int num = mIcons.Count - 1; num >= 0; num--)
		{
			if (!IsIconDead(mIcons[num]))
			{
				UObject.Destroy((UObject)(object)((Component)mIcons[num]).gameObject);
			}
		}
		mIcons.Clear();
	}
}
