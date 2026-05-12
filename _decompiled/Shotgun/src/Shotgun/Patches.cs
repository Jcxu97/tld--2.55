using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shotgun;

internal class Patches
{
	[HarmonyPatch(typeof(EquipItemPopup), "AllowedToHideAmmoPopup")]
	[HarmonyPriority(-999999)]
	internal static class EquipItemPopup_AllowedToHideAmmoPopup_ModComponentCompatibilityFix
	{
		internal static void Postfix(ref bool __result)
		{
			if (__result)
			{
				PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
				if (!((Object)(object)playerManagerComponent == (Object)null) && !((Object)(object)playerManagerComponent.m_ItemInHands == (Object)null) && !((Object)(object)playerManagerComponent.m_ItemInHands.m_GunItem == (Object)null) && (int)playerManagerComponent.m_ItemInHands.m_GunItem.m_GunType == 4)
				{
					__result = false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "PressSwitchAmmo")]
	internal static class GunItem_PreventSwitchAmmoUnlessIntended
	{
		internal static bool Prefix(GunItem __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)__instance.m_GunType != 4)
			{
				return true;
			}
			if (!InputManager.HasContext(InputManager.m_CurrentContext))
			{
				return false;
			}
			if (!Input.GetKey((KeyCode)116))
			{
				return false;
			}
			return true;
		}

		internal static void Postfix(GunItem __instance)
		{
			try
			{
				if ((int)__instance.m_GunType != 4) return;
				GearItemData ammo = null;
				if (__instance.ValidAmmo != null && __instance.m_SelectedAmmoIndex >= 0)
				{
					try { ammo = __instance.ValidAmmo[__instance.m_SelectedAmmoIndex]; }
					catch { ammo = null; }
				}
				if ((Object)(object)ammo != (Object)null)
				{
					try { UpdateShotgunParams(__instance, ammo.DisplayName); }
					catch (System.Exception e) { MelonLogger.Error($"[散弹枪] UpdateShotgunParams (postfix) threw: {e}"); }
				}
				try { CustomAmmoHUD.Current.Refresh(); }
				catch (System.Exception e) { MelonLogger.Error($"[散弹枪] HUD.Refresh (postfix) threw: {e}"); }
			}
			catch (System.Exception e)
			{
				MelonLogger.Error($"[散弹枪] PressSwitchAmmo Postfix outer threw: {e}");
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "PressReloadAmmo")]
	internal static class GunItem_PreventReloadAmmoUnlessIntended
	{
		internal static bool Prefix(GunItem __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)__instance.m_GunType != 4)
			{
				return true;
			}
			if (!InputManager.HasContext(InputManager.m_CurrentContext))
			{
				return false;
			}
			if (!Input.GetKey((KeyCode)116) && !Input.GetKey((KeyCode)114))
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(GunItem), "Start")]
	internal static class GunItem_UpdateOnStart
	{
		internal static void Postfix(GunItem __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)__instance.m_GunType == 4)
			{
				ShotgunSkill.ApplyToGunItem(__instance);
				UpdateShotgunParams(__instance, __instance.ValidAmmo[__instance.m_SelectedAmmoIndex].DisplayName);
				CustomAmmoHUD.Current.Refresh();
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "Update")]
	internal static class GunItem_ShotgunUpdateInput
	{
		internal static void Postfix(GunItem __instance)
		{
			try
			{
				if ((int)__instance.m_GunType != 4)
					return;
				if (!InputManager.HasContext(InputManager.m_CurrentContext))
					return;

				if (Input.GetKeyDown((KeyCode)116))
				{
					try
					{
						if (__instance.ValidAmmo != null)
						{
							int nextIndex = __instance.m_SelectedAmmoIndex + 1;
							GearItemData nextAmmo = null;
							try { nextAmmo = __instance.ValidAmmo[nextIndex]; } catch { }
							if ((Object)(object)nextAmmo == (Object)null)
								nextIndex = 0;
							__instance.m_SelectedAmmoIndex = nextIndex;
							GearItemData ammo = __instance.ValidAmmo[nextIndex];
							if ((Object)(object)ammo != (Object)null)
								UpdateShotgunParams(__instance, ammo.DisplayName);
							CustomAmmoHUD.Current.Refresh();
							MelonLogger.Msg($"[散弹枪] Switched ammo to index {__instance.m_SelectedAmmoIndex}: {((Object)ammo).name}");
						}
					}
					catch (System.Exception e) { MelonLogger.Error($"[散弹枪] Manual ammo switch threw: {e}"); }
				}

				if (Input.GetKeyDown((KeyCode)114) && (__instance.m_Clip.Count != __instance.m_ClipSize || __instance.m_SpentCasingsInClip != 0))
				{
					try { CustomAmmoHUD.Current.Refresh(); }
					catch (System.Exception e) { MelonLogger.Error($"[散弹枪] HUD.Refresh threw: {e}"); }
				}
			}
			catch (System.Exception e)
			{
				MelonLogger.Error($"[散弹枪] ShotgunUpdateInput Postfix outer threw: {e}");
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "RemoveNextFromClip")]
	internal static class GunItem_AddSpentShellCasingOnRemoveNextFromClip
	{
		internal static void Prefix(GunItem __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)__instance.m_GunType == 4 && __instance.m_Clip.Count != 0)
			{
				int spentCasingsInClip = __instance.m_SpentCasingsInClip;
				__instance.m_SpentCasingsInClip = spentCasingsInClip + 1;
			}
		}
	}

	[HarmonyPatch(typeof(vp_FPSShooter), "Reload", new Type[] { typeof(int) })]
	internal static class vp_FPSShooter_EnsureCorrectReloadCount
	{
		internal static void Prefix(ref int ammoCount, vp_FPSShooter __instance)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Invalid comparison between Unknown and I4
			if (!((Object)(object)__instance == (Object)null) && !((Object)(object)__instance.m_Weapon == (Object)null) && !((Object)(object)__instance.m_Weapon.m_GunItem == (Object)null) && (int)__instance.m_Weapon.m_GunItem.m_GunType == 4)
			{
				GunItem gunItem = __instance.m_Weapon.m_GunItem;
				int val = ((gunItem.m_PreviousAmmoIndex < 0) ? (gunItem.m_ClipSize - gunItem.m_Clip.Count) : gunItem.m_ClipSize);
				int val2 = 0;
				PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
				GearItem val3 = ((playerManagerComponent != null) ? playerManagerComponent.m_ItemInHands : null);
				Inventory inventoryComponent = GameManager.GetInventoryComponent();
				if ((Object)(object)val3 != (Object)null && (Object)(object)inventoryComponent != (Object)null)
				{
					val2 = inventoryComponent.GetAmmoAvailableForWeapon(val3);
				}
				ammoCount = Math.Min(val, val2);
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "Fired")]
	internal static class GunItem_RefreshCustomHUDOnFired
	{
		internal static void Prefix(GunItem __instance)
		{
			if ((int)__instance.m_GunType == 4)
				__instance.m_FireAudio = null;
		}

		internal static void Postfix(GunItem __instance)
		{
			CustomAmmoHUD.Current.Refresh();
			if ((int)__instance.m_GunType == 4)
			{
				ShotgunSkill.MaybeIncreaseSkillOnFire();
				ShotgunAudio.PlayFire();
				SpawnExtraProjectilesFromGunItem(__instance);
			}
		}
	}

	private static void SpawnExtraProjectilesFromGunItem(GunItem gunItem)
	{
		try
		{
			var refs = Main.Instance?.Configuration?.Refs;
			if (refs == null || refs.Shotgun == null || refs.Shotgun.WeaponView == null) return;
			var fps = refs.Shotgun.WeaponView.FPSShooter;
			if ((Object)(object)fps == (Object)null || (Object)(object)fps.ProjectilePrefab == (Object)null) return;
			if ((Object)(object)fps.BulletEmissionLocator == (Object)null) return;

			string ammoName = "";
			if (gunItem.ValidAmmo != null && gunItem.m_SelectedAmmoIndex >= 0)
			{
				try { var a = gunItem.ValidAmmo[gunItem.m_SelectedAmmoIndex]; if (a != null) ammoName = ((Object)a).name ?? ""; } catch { }
			}

			int extraCount = 0;
			float spreadDeg = 0f;
			if (ammoName.Contains("Buckshot")) { extraCount = 8; spreadDeg = 1.5f; }
			else if (ammoName.Contains("Birdshot")) { extraCount = 29; spreadDeg = 3.5f; }

			if (extraCount <= 0) return;

			Transform emitter = fps.BulletEmissionLocator;
			Vector3 origin = emitter.position;
			float spreadRad = spreadDeg * Mathf.Deg2Rad;

			for (int i = 0; i < extraCount; i++)
			{
				float u = UnityEngine.Random.value;
				float v = UnityEngine.Random.value * 2f * Mathf.PI;
				float r = Mathf.Sqrt(u) * spreadRad;
				Vector3 localDir = new Vector3(Mathf.Cos(v) * r, Mathf.Sin(v) * r, 1f).normalized;
				Vector3 worldDir = emitter.TransformDirection(localDir);
				Object.Instantiate(fps.ProjectilePrefab, origin, Quaternion.LookRotation(worldDir));
			}
		}
		catch (System.Exception e)
		{
			MelonLogger.Error($"[散弹枪] 弹丸生成异常: {e.Message}");
		}
	}

	[HarmonyPatch(typeof(vp_FPSShooter), "OnBulletLoaded")]
	internal static class vp_FPSShooter_RefreshHUDOnBulletLoaded
	{
		internal static void Postfix()
		{
			CustomAmmoHUD.Current.Refresh();
		}
	}

	[HarmonyPatch(typeof(LocalizedDamage), "GetBleedOutMinutes", new Type[] { typeof(WeaponSource) })]
	internal static class LocalizedDamage_PreventUnfairBleeding
	{
		internal static void Postfix(LocalizedDamage __instance, WeaponSource weapon, ref float __result)
		{
			if ((int)weapon != 1)
				return;
			PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
			if ((Object)(object)playerManagerComponent == (Object)null || (Object)(object)playerManagerComponent.m_ItemInHands == (Object)null)
				return;
			GunItem gunItem = playerManagerComponent.m_ItemInHands.m_GunItem;
			if ((Object)(object)gunItem == (Object)null || (int)gunItem.m_GunType != 4)
				return;
			CustomAmmoHUD current = CustomAmmoHUD.Current;
			if ((Object)(object)current.LoadedAmmo == (Object)null)
				return;
			string name = ((Object)current.LoadedAmmo).name;
			if (name == "GEAR_ShotgunAmmoBoxBuckshot" || name == "GEAR_ShotgunAmmoBoxBirdshot")
			{
				__result = 0f;
			}
		}
	}

	[HarmonyPatch(typeof(LocalizedDamage), "RollChanceToKill", new Type[] { typeof(WeaponSource) })]
	internal static class LocalizedDamage_PreventUnfairCriticals
	{
		internal static void Postfix(LocalizedDamage __instance, WeaponSource weapon, ref bool __result)
		{
			if ((int)weapon != 1)
				return;
			PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
			if ((Object)(object)playerManagerComponent == (Object)null || (Object)(object)playerManagerComponent.m_ItemInHands == (Object)null)
				return;
			GunItem gunItem = playerManagerComponent.m_ItemInHands.m_GunItem;
			if ((Object)(object)gunItem == (Object)null || (int)gunItem.m_GunType != 4)
				return;
			CustomAmmoHUD current = CustomAmmoHUD.Current;
			if ((Object)(object)current.LoadedAmmo == (Object)null)
				return;
			string name = ((Object)current.LoadedAmmo).name;
			if (name == "GEAR_ShotgunAmmoBoxBirdshot" || name == "GEAR_ShotgunAmmoBoxBuckshot")
			{
				__result = false;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "Trigger_Generic_Equip")]
	internal static class PlayerAnimation_ShotgunEquip
	{
		internal static void Postfix(PlayerAnimation __instance)
		{
			if (MaybeLatchShotgunAnimator(__instance, "Equip"))
			{
				if ((Object)(object)__instance.m_EquippedFirstPersonWeaponShoulder != (Object)null)
				{
					((Object)((Component)__instance.m_EquippedFirstPersonWeaponShoulder).gameObject).name = "FPH_Shotgun";
					try { ReapplyShotgunShader(((Component)__instance.m_EquippedFirstPersonWeaponShoulder).gameObject); }
					catch (System.Exception e) { MelonLogger.Error($"[散弹枪] ReapplyShotgunShader threw: {e}"); }
				}
				Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
				moddedAnimator.Rebind();
				moddedAnimator.SetTrigger("Trigger_Generic_Equip");
				__instance.OnAnimationEvent_Generic_Equip_ShowItem();
				__instance.OnAnimationEvent_Generic_Equip_Complete();
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "Trigger_Generic_Unequip")]
	internal static class PlayerAnimation_ShotgunUnequip
	{
		internal static void Prefix(PlayerAnimation __instance)
		{
			if (!((Object)(object)__instance.m_EquippedFirstPersonWeaponShoulder == (Object)null) && ((Object)__instance.m_EquippedFirstPersonWeaponShoulder).name.Contains("Shotgun"))
			{
				Animator vanillaAnimator = Main.Instance.Configuration.Refs.VanillaAnimator;
				Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
				if ((Object)(object)vanillaAnimator != (Object)null)
				{
					((Behaviour)vanillaAnimator).enabled = true;
				}
				if ((Object)(object)moddedAnimator != (Object)null)
				{
					((Behaviour)moddedAnimator).enabled = false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "Trigger_Generic_Aim")]
	internal static class PlayerAnimation_ShotgunAim
	{
		internal static void Postfix(PlayerAnimation __instance)
		{
			if (MaybeLatchShotgunAnimator(__instance, "Aim"))
			{
				Main.Instance.Configuration.Refs.ModdedAnimator.SetTrigger("Trigger_Generic_Aim");
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "Trigger_Generic_Aim_Cancel")]
	internal static class PlayerAnimation_ShotgunAimCancel
	{
		internal static void Postfix(PlayerAnimation __instance)
		{
			if (MaybeLatchShotgunAnimator(__instance, "AimCancel"))
			{
				Main.Instance.Configuration.Refs.ModdedAnimator.SetTrigger("Trigger_Generic_Aim_Cancel");
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "Trigger_Generic_Fire")]
	internal static class PlayerAnimation_ShotgunFire
	{
		internal static void Postfix(PlayerAnimation __instance)
		{
			if (MaybeLatchShotgunAnimator(__instance, "Fire"))
			{
				Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
				moddedAnimator.SetInteger("ShotgunSkillLevel", ShotgunSkill.GetCurrentTier() + 1);
				moddedAnimator.SetTrigger("Trigger_Generic_Fire");
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "Trigger_Generic_Reload")]
	internal static class PlayerAnimation_ShotgunReload
	{
		internal static void Postfix(PlayerAnimation __instance, int bulletsToReload, int roundsLoaded, bool willJam)
		{
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Invalid comparison between Unknown and I4
			if (MaybeLatchShotgunAnimator(__instance, "Reload"))
			{
				PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
				GearItem val = ((playerManagerComponent != null) ? playerManagerComponent.m_ItemInHands : null);
				if ((Object)(object)val == (Object)null)
					return;
				GunItem gunItem = val.m_GunItem;
				int num = (((Object)(object)gunItem != (Object)null) ? gunItem.m_SpentCasingsInClip : 0);
				bool flag = (Object)(object)gunItem != (Object)null && gunItem.m_PreviousAmmoIndex >= 0;
				Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
				moddedAnimator.SetInteger("Reload_Count", bulletsToReload);
				moddedAnimator.SetInteger("Rounds_Loaded", roundsLoaded);
				moddedAnimator.SetInteger("SpentCasingsInClip", num);
				moddedAnimator.SetBool("Is_Loaded", roundsLoaded > 0);
				moddedAnimator.SetBool("IsAmmoSwitch", flag);
				moddedAnimator.SetBool("Is_Jammed", willJam);
				moddedAnimator.SetBool("IsAimHeld", (int)__instance.m_State == 2);
				moddedAnimator.SetInteger("ShotgunSkillLevel", ShotgunSkill.GetCurrentTier() + 1);
				moddedAnimator.SetTrigger("Trigger_Generic_Reload_Single");
			}
		}
	}

	[HarmonyPatch(typeof(vp_FPSWeapon), "OnRoundsUnloaded")]
	internal static class vp_FPSWeapon_ShotgunOnRoundsUnloaded
	{
		internal static void Prefix(vp_FPSWeapon __instance)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			if ((Object)(object)__instance.m_GunItem == (Object)null || (int)__instance.m_GunItem.m_GunType != 4)
			{
				return;
			}
			GunItem gunItem = __instance.m_GunItem;
			int spentCasingsInClip = gunItem.m_SpentCasingsInClip;
			if (spentCasingsInClip <= 0)
			{
				return;
			}
			PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
			if (!((Object)(object)playerManagerComponent == (Object)null))
			{
				for (int i = 0; i < spentCasingsInClip; i++)
				{
					playerManagerComponent.InstantiateItemAtPlayersFeet(gunItem.m_CasingData.m_PrefabReference, 1);
				}
				gunItem.PlayCasingAudio(spentCasingsInClip);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerAnimation), "IsAllowedToFire")]
	internal static class PlayerAnimation_IsAllowedToFire_ShotgunOverride
	{
		internal static bool Prefix(PlayerAnimation __instance, bool allowHipFire, ref bool __result)
		{
			if ((Object)(object)__instance.m_EquippedFirstPersonWeaponShoulder == (Object)null)
			{
				return true;
			}
			if (!((Object)__instance.m_EquippedFirstPersonWeaponShoulder).name.Contains("Shotgun"))
			{
				return true;
			}
			Animator moddedAnimator = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
			if ((Object)(object)moddedAnimator == (Object)null || (Object)(object)moddedAnimator.runtimeAnimatorController == (Object)null)
			{
				return true;
			}
			if (allowHipFire)
			{
				int num = (int)__instance.m_State;
				int num2 = 80;
				__result = ((1 << num) & num2) == 0;
			}
			else
			{
				__result = (int)__instance.m_State == 2;
			}
			return false;
		}
	}

	internal static void ReapplyShotgunShader(GameObject equippedShoulder)
	{
		if ((Object)(object)equippedShoulder == (Object)null) return;
		var refs = Main.Instance?.Configuration?.Refs;
		if (refs == null || refs.Rifle == null || refs.Rifle.Gun == null || (Object)(object)refs.Rifle.Gun.Prefab == (Object)null) return;
		Shader correctShader = null;
		Renderer[] rifleRenderers = refs.Rifle.Gun.Prefab.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in rifleRenderers)
		{
			if ((Object)(object)r != (Object)null && (Object)(object)r.sharedMaterial != (Object)null && (Object)(object)r.sharedMaterial.shader != (Object)null)
			{
				correctShader = r.sharedMaterial.shader;
				break;
			}
		}
		if ((Object)(object)correctShader == (Object)null) return;
		string[][] texPropMappings = new string[][] {
			new string[] { "_MainTex", "_BaseMap", "_BaseColorMap" },
			new string[] { "_BumpMap", "_NormalMap" },
			new string[] { "_MetallicGlossMap", "_MaskMap" },
			new string[] { "_EmissionMap" },
			new string[] { "_OcclusionMap" },
		};
		int fixedCount = 0;
		int totalMats = 0;
		Renderer[] renderers = equippedShoulder.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in renderers)
		{
			if ((Object)(object)r == (Object)null) continue;
			Material[] mats = r.materials;
			if (mats == null) continue;
			for (int i = 0; i < mats.Length; i++)
			{
				Material mat = mats[i];
				totalMats++;
				if ((Object)(object)mat == (Object)null) continue;
				if (!Configuration.IsNativeShader(mat.shader))
				{
					MelonLogger.Msg($"[散弹枪] 材质 '{mat.name}': shader '{mat.shader?.name}' → '{correctShader.name}'");
					var savedTextures = new System.Collections.Generic.Dictionary<string, Texture>();
					foreach (string[] group in texPropMappings)
					{
						foreach (string prop in group)
						{
							if (mat.HasProperty(prop))
							{
								Texture tex = mat.GetTexture(prop);
								if ((Object)(object)tex != (Object)null)
								{
									savedTextures[group[0]] = tex;
									break;
								}
							}
						}
					}
					mat.shader = correctShader;
					foreach (var kv in savedTextures)
					{
						foreach (string[] group in texPropMappings)
						{
							if (group[0] != kv.Key) continue;
							foreach (string prop in group)
							{
								if (mat.HasProperty(prop))
								{
									mat.SetTexture(prop, kv.Value);
									break;
								}
							}
							break;
						}
					}
					fixedCount++;
				}
			}
		}
		MelonLogger.Msg($"[散弹枪] 装备着色器修复: 总材质={totalMats}, 已修复={fixedCount}");
	}

	internal static bool MaybeLatchShotgunAnimator(PlayerAnimation instance, string callerName = "")
	{
		Animator vanillaAnimator = Main.Instance.Configuration.Refs.VanillaAnimator;
		if ((Object)(object)vanillaAnimator == (Object)null)
			return false;
		Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
		if ((Object)(object)moddedAnimator == (Object)null || (Object)(object)moddedAnimator.runtimeAnimatorController == (Object)null)
		{
			((Behaviour)vanillaAnimator).enabled = true;
			return false;
		}
		if ((Object)(object)instance.m_EquippedFirstPersonWeaponShoulder == (Object)null)
		{
			((Behaviour)moddedAnimator).enabled = false;
			((Behaviour)vanillaAnimator).enabled = true;
			return false;
		}
		Animator animator = instance.m_EquippedFirstPersonWeaponShoulder.m_Animator;
		if ((Object)(object)animator == (Object)null)
		{
			((Behaviour)moddedAnimator).enabled = false;
			((Behaviour)vanillaAnimator).enabled = true;
			return false;
		}
		string shoulderName = ((Object)instance.m_EquippedFirstPersonWeaponShoulder).name;
		bool isShotgun = shoulderName.Contains("Shotgun");
		((Behaviour)moddedAnimator).enabled = isShotgun;
		((Behaviour)vanillaAnimator).enabled = true;
		((Behaviour)animator).enabled = !isShotgun;
		return isShotgun;
	}

	internal static void UpdateShotgunParams(GunItem gun, string ammoName)
	{
		if ((Object)(object)gun == (Object)null) return;
		if (!((Object)gun).name.Contains("Shotgun")) return;
		if (ammoName == null) return;

		var refs = Main.Instance?.Configuration?.Refs;
		if (refs == null) { MelonLogger.Warning("[散弹枪] UpdateShotgunParams: Configuration.Refs is null"); return; }
		if (refs.Shotgun == null) { MelonLogger.Warning("[散弹枪] UpdateShotgunParams: Refs.Shotgun is null"); return; }
		if (refs.Shotgun.WeaponView == null) { MelonLogger.Warning("[散弹枪] UpdateShotgunParams: WeaponView is null"); return; }
		var fps = refs.Shotgun.WeaponView.FPSShooter;
		if ((Object)(object)fps == (Object)null) { MelonLogger.Warning("[散弹枪] UpdateShotgunParams: FPSShooter is null"); return; }
		if ((Object)(object)fps.ProjectilePrefab == (Object)null) { MelonLogger.Warning("[散弹枪] UpdateShotgunParams: ProjectilePrefab is null"); return; }

		vp_Bullet component = fps.ProjectilePrefab.GetComponent<vp_Bullet>();

		if (ammoName.Contains("Slug"))
		{
			gun.m_DamageHP = 120f;
			gun.m_AccuracyRange = 50f;
			fps.ProjectileCount = 1;
			fps.ProjectileSpread = 0f;
			if ((Object)(object)component != (Object)null)
			{
				component.MinimumDamageFalloffBeyondEffectiveRange = 25f;
				component.DamageFalloffPerMeterBeyondEffectiveRange = 5f;
			}
		}
		else if (ammoName.Contains("Buckshot"))
		{
			gun.m_DamageHP = 15f;
			gun.m_AccuracyRange = 40f;
			fps.ProjectileCount = 9;
			fps.ProjectileSpread = 1f;
			if ((Object)(object)component != (Object)null)
			{
				component.MinimumDamageFalloffBeyondEffectiveRange = 5f;
				component.DamageFalloffPerMeterBeyondEffectiveRange = 0.5f;
			}
		}
		else if (ammoName.Contains("Birdshot"))
		{
			gun.m_DamageHP = 3f;
			gun.m_AccuracyRange = 30f;
			fps.ProjectileCount = 30;
			fps.ProjectileSpread = 2f;
			if ((Object)(object)component != (Object)null)
			{
				component.MinimumDamageFalloffBeyondEffectiveRange = 1f;
				component.DamageFalloffPerMeterBeyondEffectiveRange = 0.2f;
			}
		}
	}

	[HarmonyPatch(typeof(vp_FPSCamera), "SetWeapon")]
	internal static class vp_FPSCamera_ShotgunSetWeapon
	{
		internal static void Postfix(vp_FPSCamera __instance, GearItem gearItem)
		{
			if ((Object)(object)gearItem == (Object)null) return;
			if ((Object)(object)gearItem.m_GunItem == (Object)null) return;
			if ((int)gearItem.m_GunItem.m_GunType != 4) return;
			var refs = Main.Instance?.Configuration?.Refs;
			if (refs == null || refs.Shotgun == null || refs.Shotgun.WeaponView == null) return;
			vp_FPSWeapon shotgunWeapon = refs.Shotgun.WeaponView.FPSWeapon;
			if ((Object)(object)__instance.m_CurrentWeapon != (Object)(object)shotgunWeapon)
				__instance.m_CurrentWeapon = shotgunWeapon;
		}
	}

	[HarmonyPatch(typeof(vp_FPSWeapon), "Update")]
	internal static class vp_FPSWeapon_ShotgunBob
	{
		private static float _bobTimer;

		internal static void Postfix(vp_FPSWeapon __instance)
		{
			var refs = Main.Instance?.Configuration?.Refs;
			if (refs == null || refs.Shotgun == null || refs.Shotgun.WeaponView == null) return;
			if ((Object)(object)__instance != (Object)(object)refs.Shotgun.WeaponView.FPSWeapon) return;

			Transform t = ((Component)__instance).transform;
			if ((Object)(object)t == (Object)null) return;

			var player = GameManager.GetVpFPSPlayer();
			if (player == null || player.Controller == null) return;
			Vector3 vel = player.Controller.Velocity;
			float speed = new Vector2(vel.x, vel.z).magnitude;

			if (speed < 0.1f) { _bobTimer = 0f; return; }

			float rate = speed > 3f ? 1.4f : 0.9f;
			float ampY = speed > 3f ? 0.002f : 0.0012f;
			float ampX = speed > 3f ? 0.001f : 0.0006f;

			_bobTimer += Time.deltaTime * rate * Mathf.PI * 2f;
			t.localPosition += new Vector3(
				Mathf.Cos(_bobTimer * 0.5f) * ampX * speed,
				Mathf.Sin(_bobTimer) * ampY * speed,
				0f);
		}
	}
}
