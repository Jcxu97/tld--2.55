using System;
using System.Runtime.CompilerServices;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MotionTracker;

[RegisterTypeInIl2Cpp]
public class PingComponent : MonoBehaviour
{
	public enum PingCategory
	{
		None,
		Animal,
		Spraypaint
	}

	public GameObject attachedGameObject;

	public GearItem attachedGearItem;

	public PingManager.AnimalType animalType;

	public ProjectileType spraypaintType;

	public PingCategory assignedCategory;

	public CanvasGroup canvasGroup;

	public GameObject iconObject;

	public bool isInitialized;

	public Image iconImage;

	public bool isVisible;

	private float timer;

	private float triggerTime = 5f;

	public RectTransform rectTransform;

	public bool clampOnRadar;

	public static GameObject playerObject;

	public PingComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
	{
	}

	[HideFromIl2Cpp]
	public static bool IsRawFish(GearItem gi)
	{
		if ((Object)(object)gi != (Object)null)
		{
			FoodItem component = ((Component)gi).GetComponent<FoodItem>();
			if ((Object)(object)component != (Object)null && component.m_IsFish && component.m_IsRawMeat)
			{
				return true;
			}
		}
		return false;
	}

	[HideFromIl2Cpp]
	public void CreateIcon()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (assignedCategory == PingCategory.Animal)
		{
			iconObject = Object.Instantiate<GameObject>(MotionTrackerMain.GetAnimalPrefab(animalType));
			iconImage = iconObject.GetComponent<Image>();
			((Graphic)iconImage).color = Settings.animalColor;
		}
		else if (assignedCategory == PingCategory.Spraypaint)
		{
			iconObject = Object.Instantiate<GameObject>(MotionTrackerMain.GetSpraypaintPrefab(spraypaintType));
			iconImage = iconObject.GetComponent<Image>();
			((Graphic)iconImage).color = Settings.spraypaintColor;
		}
		iconObject.transform.SetParent(((Component)PingManager.instance.iconContainer).transform, false);
		iconObject.active = true;
		canvasGroup = iconObject.GetComponent<CanvasGroup>();
		rectTransform = iconObject.GetComponent<RectTransform>();
	}

	[HideFromIl2Cpp]
	public void DeleteIcon()
	{
		if (Object.op_Implicit((Object)(object)iconObject))
		{
			Object.Destroy((Object)(object)iconObject);
		}
	}

	[HideFromIl2Cpp]
	public bool AllowedToShow()
	{
		if (assignedCategory == PingCategory.Animal)
		{
			if (animalType == PingManager.AnimalType.Crow && Settings.options.showCrows)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Rabbit && Settings.options.showRabbits)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Stag && Settings.options.showStags)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Doe && Settings.options.showDoes)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Wolf && Settings.options.showWolves)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Timberwolf && Settings.options.showTimberwolves)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Bear && Settings.options.showBears)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Cougar && Settings.options.showCougars)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Moose && Settings.options.showMoose)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.PuffyBird && Settings.options.showPuffyBirds)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Arrow && Settings.options.showArrows)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.Coal && Settings.options.showCoal)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.RawFish && Settings.options.showRawFish)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.LostAndFoundBox && Settings.options.showLostAndFoundBox)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.SaltDeposit && Settings.options.showSaltDeposit)
			{
				return true;
			}
			if (animalType == PingManager.AnimalType.BeachLoot && Settings.options.showBeachLoot)
			{
				return true;
			}
			return false;
		}
		if (assignedCategory == PingCategory.Spraypaint && Settings.options.showSpraypaint)
		{
			return true;
		}
		return false;
	}

	[HideFromIl2Cpp]
	public static void ManualDelete(PingComponent pingComponent)
	{
		if ((Object)(object)pingComponent != (Object)null)
		{
			pingComponent.DeleteIcon();
			Object.Destroy((Object)(object)pingComponent);
		}
	}

	[HideFromIl2Cpp]
	public void SetVisible(bool visibility)
	{
		if (!Object.op_Implicit((Object)(object)canvasGroup))
		{
			return;
		}
		if (AllowedToShow() && visibility)
		{
			try
			{
				canvasGroup.alpha = 1f;
				return;
			}
			catch (Exception ex)
			{
				LogMessage("Exception thrown (" + ex.Message + ") when setting canvasGroup.alpha = 1f for pingComponent.name = (" + ((Object)this).name + ":" + ((Object)this).GetInstanceID() + ")", 277, "SetVisible", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingComponent.cs");
				return;
			}
		}
		try
		{
			canvasGroup.alpha = 0f;
		}
		catch (Exception ex2)
		{
			LogMessage("Exception thrown (" + ex2.Message + ") when setting canvasGroup.alpha = 0f for pingComponent.name = (" + ((Object)this).name + ":" + ((Object)this).GetInstanceID() + ")", 304, "SetVisible", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Ping\\PingComponent.cs");
		}
	}

	[HideFromIl2Cpp]
	public void Initialize(PingManager.AnimalType type)
	{
		if (((Component)this).gameObject.activeSelf)
		{
			attachedGameObject = ((Component)this).gameObject;
			animalType = type;
			assignedCategory = PingCategory.Animal;
			CreateIcon();
			isInitialized = true;
			isVisible = true;
		}
	}

	[HideFromIl2Cpp]
	public void Initialize(ProjectileType type)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		attachedGameObject = ((Component)this).gameObject;
		spraypaintType = type;
		assignedCategory = PingCategory.Spraypaint;
		CreateIcon();
		isInitialized = true;
		isVisible = true;
	}

	[HideFromIl2Cpp]
	private void OnDisable()
	{
		DeleteIcon();
	}

	public void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Invalid comparison between Unknown and I4
		if (!Settings.options.enableMotionTracker || !PingManager.isVisible || (int)SaveGameSystem.m_CurrentGameMode != 3 || !((Object)(object)GameManager.GetVpFPSPlayer() != (Object)null))
		{
			return;
		}
		timer += Time.deltaTime;
		if (((Object)this).name.Contains("GEAR_RawCohoSalmon", StringComparison.CurrentCultureIgnoreCase))
		{
			GearItem val = attachedGearItem;
			if ((Object)(object)val != (Object)null && !((Behaviour)val).isActiveAndEnabled)
			{
				ManualDelete(this);
				return;
			}
		}
		BaseAi component = ((Component)this).gameObject.GetComponent<BaseAi>();
		if ((Object)(object)component != (Object)null && (int)component.m_CurrentMode == 2)
		{
			ManualDelete(this);
			return;
		}
		UpdateLocatableIcons();
		if (timer > triggerTime)
		{
			timer = 0f;
		}
	}

	private void UpdateLocatableIcons()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetIconLocation(out var iconLocation))
		{
			SetVisible(visibility: true);
			if (!Object.op_Implicit((Object)(object)rectTransform))
			{
				ManualDelete(this);
				return;
			}
			rectTransform.anchoredPosition = iconLocation;
			if (assignedCategory == PingCategory.Spraypaint)
			{
				if (((Graphic)iconImage).color != Settings.spraypaintColor || ((Transform)rectTransform).localScale != Settings.spraypaintScale)
				{
					((Transform)rectTransform).localScale = Settings.spraypaintScale;
					((Graphic)iconImage).color = Settings.spraypaintColor;
				}
			}
			else
			{
				if (assignedCategory != PingCategory.Animal)
				{
					return;
				}
				if (animalType == PingManager.AnimalType.Arrow || animalType == PingManager.AnimalType.Coal || animalType == PingManager.AnimalType.LostAndFoundBox || animalType == PingManager.AnimalType.SaltDeposit || animalType == PingManager.AnimalType.BeachLoot || animalType == PingManager.AnimalType.RawFish)
				{
					if (((Graphic)iconImage).color != Settings.gearColor || ((Transform)rectTransform).localScale != Settings.gearScale)
					{
						((Transform)rectTransform).localScale = Settings.gearScale;
						((Graphic)iconImage).color = Settings.gearColor;
					}
				}
				else if (((Graphic)iconImage).color != Settings.animalColor || ((Transform)rectTransform).localScale != Settings.animalScale)
				{
					((Transform)rectTransform).localScale = Settings.animalScale;
					((Graphic)iconImage).color = Settings.animalColor;
				}
				if (((Object)this).name.Contains("Arrow"))
				{
					((Graphic)iconImage).color = Color.yellow;
				}
			}
		}
		else
		{
			SetVisible(visibility: false);
		}
	}

	private bool TryGetIconLocation(out Vector2 iconLocation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		iconLocation = GetDistanceToPlayer(this);
		float radarUISize = GetRadarUISize();
		float num = radarUISize / (float)Settings.options.detectionRange;
		iconLocation *= num;
		if (PingManager.instance.applyRotation)
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(0f, 0f, 0f);
			if (Object.op_Implicit((Object)(object)GameManager.GetVpFPSPlayer()))
			{
				val = Vector3.ProjectOnPlane(((Component)GameManager.GetVpFPSPlayer()).gameObject.transform.forward, Vector3.up);
			}
			Quaternion val2 = Quaternion.LookRotation(val);
			Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
			eulerAngles.y = 0f - eulerAngles.y;
			((Quaternion)(ref val2)).eulerAngles = eulerAngles;
			Vector3 val3 = val2 * new Vector3(iconLocation.x, 0f, iconLocation.y);
			iconLocation = new Vector2(val3.x, val3.z);
		}
		if (((Vector2)(ref iconLocation)).sqrMagnitude < radarUISize * radarUISize || clampOnRadar)
		{
			iconLocation = Vector2.ClampMagnitude(iconLocation, radarUISize);
			return true;
		}
		return false;
	}

	private float GetRadarUISize()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = PingManager.instance.iconContainer.rect;
		return ((Rect)(ref rect)).width / 2f;
	}

	private Vector2 GetDistanceToPlayer(PingComponent locatable)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)GameManager.GetVpFPSPlayer()) && Object.op_Implicit((Object)(object)locatable))
		{
			Vector3 val = ((Component)locatable).transform.position - ((Component)GameManager.GetVpFPSPlayer()).gameObject.transform.position;
			return new Vector2(val.x, val.z);
		}
		return new Vector2(0f, 0f);
	}
}
