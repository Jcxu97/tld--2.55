using System;
using System.Runtime.CompilerServices;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gear;
using MehToolBox;
using MelonLoader;
using ModComponent.API.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Shotgun;

internal class Configuration
{
	internal static readonly string[] ShotgunAmmoTypes = new string[3] { "Slug", "Buckshot", "Birdshot" };

	internal ConfigurationReferences Refs { get; set; }

	internal bool PrepareShotgun()
	{
		try
		{
			MelonLogger.Msg("Preparing shotgun...");
			ConfigurationReferences refs = (Refs = new ConfigurationReferences());
			if (!Get(refs))
			{
				return false;
			}
			if (!Instantiate(refs))
			{
				return false;
			}
			FixShotgunShaders(refs);
			if (!Configure(refs))
			{
				return false;
			}
			if (!RegisterShotgun(refs))
			{
				return false;
			}
			if (!SetupTest.Validate(refs))
			{
				return false;
			}
			ReportDifferences(refs);
			MelonLogger.Msg("Ready to roll!");
			return true;
		}
		catch (System.Exception value)
		{
			MelonLogger.Error($"Error during PrepareShotgun: {value}");
			return false;
		}
	}

	internal bool RegisterShotgun(ConfigurationReferences refs)
	{
		MelonLogger.Msg("RegisterShotgunFPSItemAndFPSWeapon");
		if (!refs.FPSCamera.m_Weapons.TryAdd(refs.Shotgun.Floating.FPSItem, refs.Shotgun.WeaponView.FPSWeapon))
		{
			MelonLogger.Error("Failed to add shotgun to vpFPSCamera.m_weapons dictionary");
			return false;
		}
		return true;
	}

	internal void ReportDifferences(ConfigurationReferences refs)
	{
	}

	internal void FixShotgunShaders(ConfigurationReferences refs)
	{
		MelonLogger.Msg("FixShotgunShaders");
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
		if ((Object)(object)correctShader == (Object)null)
		{
			MelonLogger.Warning("Could not find correct shader from rifle, skipping shader fix");
			return;
		}
		MelonLogger.Msg($"Using shader: {correctShader.name}");
		int count = 0;
		count += FixRenderersShader(refs.Shotgun.Gun.Prefab, correctShader);
		count += FixRenderersShader(refs.Shotgun.FPH.Root, correctShader);
		string[] ammoTypes = ShotgunAmmoTypes;
		foreach (string ammoType in ammoTypes)
		{
			count += FixRenderersShader(refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab, correctShader);
			count += FixRenderersShader(refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab, correctShader);
		}
		count += FixRenderersShader(refs.Shotgun.Casing.Prefab, correctShader);
		MelonLogger.Msg($"Fixed {count} materials");
		FixNullTextures(refs);
	}

	internal static bool IsNativeShader(Shader shader)
	{
		if ((Object)(object)shader == (Object)null) return false;
		string name = shader.name;
		return name != null && (name.StartsWith("Shader Graphs/") || name.StartsWith("TLD") || name.StartsWith("Shader Forge/"));
	}

	private static readonly string[][] TexPropMappings = new string[][] {
		new string[] { "_MainTex", "_BaseMap", "_BaseColorMap" },
		new string[] { "_BumpMap", "_NormalMap" },
		new string[] { "_MetallicGlossMap", "_MaskMap" },
		new string[] { "_EmissionMap" },
		new string[] { "_OcclusionMap" },
	};

	internal static int FixRenderersShader(GameObject obj, Shader correctShader)
	{
		if ((Object)(object)obj == (Object)null) return 0;
		int count = 0;
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in renderers)
		{
			if ((Object)(object)r == (Object)null) continue;
			Material[] mats = r.sharedMaterials;
			if (mats == null) continue;
			foreach (Material mat in mats)
			{
				if ((Object)(object)mat == (Object)null) continue;
				if (!IsNativeShader(mat.shader))
				{
					MelonLogger.Msg($"[散弹枪] 材质 '{mat.name}': shader '{mat.shader?.name}' → '{correctShader.name}'");
					var saved = new System.Collections.Generic.Dictionary<string, Texture>();
					foreach (string[] group in TexPropMappings)
					{
						foreach (string prop in group)
						{
							if (mat.HasProperty(prop))
							{
								Texture tex = mat.GetTexture(prop);
								if ((Object)(object)tex != (Object)null) { saved[group[0]] = tex; break; }
							}
						}
					}
					mat.shader = correctShader;
					foreach (var kv in saved)
					{
						foreach (string[] group in TexPropMappings)
						{
							if (group[0] != kv.Key) continue;
							foreach (string prop in group)
							{
								if (mat.HasProperty(prop)) { mat.SetTexture(prop, kv.Value); break; }
							}
							break;
						}
					}
					count++;
				}
			}
		}
		return count;
	}

	internal void FixNullTextures(ConfigurationReferences refs)
	{
		MelonLogger.Msg("FixNullTextures — 诊断+修复");
		Texture2D[] allTex = UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>();
		Texture2D bodyTex = null;
		foreach (var t in allTex)
		{
			if (t != null && t.name != null && t.name.Contains("Shotgun_lambert1"))
			{ bodyTex = t; break; }
		}
		MelonLogger.Msg($"[散弹枪] Body贴图搜索: {(bodyTex != null ? bodyTex.name + " " + bodyTex.width + "x" + bodyTex.height : "未找到")}");

		GameObject[] targets = new GameObject[] { refs.Shotgun.Gun.Prefab, refs.Shotgun.FPH.Root };
		foreach (var go in targets)
		{
			if ((Object)(object)go == (Object)null) continue;
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
			{
				if ((Object)(object)r == (Object)null) continue;
				foreach (Material mat in r.sharedMaterials)
				{
					if ((Object)(object)mat == (Object)null) continue;
					Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
					Color col = mat.HasProperty("_Color") ? mat.color : Color.magenta;
					MelonLogger.Msg($"[散弹枪] 材质诊断: '{mat.name}' shader='{mat.shader?.name}' _MainTex={(mainTex != null ? mainTex.name : "NULL")} _Color=({col.r:F2},{col.g:F2},{col.b:F2},{col.a:F2})");

					if (mat.HasProperty("_Color") && (col.r < 0.9f || col.g < 0.9f || col.b < 0.9f || col.a < 0.9f))
					{
						mat.color = Color.white;
						MelonLogger.Msg($"[散弹枪]   → _Color 异常({col.r:F2},{col.g:F2},{col.b:F2},{col.a:F2}), 强制白色");
					}
					if (mainTex == null && bodyTex != null && mat.HasProperty("_MainTex"))
					{
						mat.SetTexture("_MainTex", bodyTex);
						MelonLogger.Msg($"[散弹枪]   → 绑定 {bodyTex.name}");
					}
				}
			}
		}
	}

	internal static bool Attempt(System.Action action, System.Func<bool> test, string errorMessage, [CallerMemberName] string methodName = "")
	{
		MelonLogger.Msg(methodName);
		action();
		if (!test())
		{
			MelonLogger.Error("[CONFIG FAIL]: " + errorMessage);
			return false;
		}
		return true;
	}

	internal bool Get(ConfigurationReferences refs)
	{
		if (!GetTopLevel(refs))
		{
			return false;
		}
		if (!GetRifle(refs))
		{
			return false;
		}
		if (!GetShotgun(refs))
		{
			return false;
		}
		return true;
	}

	internal bool GetTopLevel(ConfigurationReferences refs)
	{
		if (!GetFPSPlayer(refs))
		{
			return false;
		}
		if (!GetFPSCamera(refs))
		{
			return false;
		}
		return true;
	}

	internal bool GetShotgun(ConfigurationReferences refs)
	{
		if (!GetShotgunPrefab(refs))
		{
			return false;
		}
		if (!GetShotgunGearItem(refs))
		{
			return false;
		}
		if (!GetShotgunModComponent(refs))
		{
			return false;
		}
		if (!GetShotgunCasing(refs))
		{
			return false;
		}
		string[] shotgunAmmoTypes = ShotgunAmmoTypes;
		foreach (string text in shotgunAmmoTypes)
		{
			refs.Shotgun.AmmoTypes[text] = new ShotgunAmmoTypeRefs();
			if (!GetShotgunAmmoSingle(refs, text))
			{
				return false;
			}
			if (!GetShotgunAmmoBox(refs, text))
			{
				return false;
			}
		}
		return true;
	}

	internal bool GetRifle(ConfigurationReferences refs)
	{
		if (!GetRifleAsset(refs))
		{
			return false;
		}
		if (!GetRifleFirstPersonItem(refs))
		{
			return false;
		}
		if (!GetRifleFPSItem(refs))
		{
			return false;
		}
		if (!GetRifleVPFPSRoot(refs))
		{
			return false;
		}
		if (!GetRifleVPFPSWeapon(refs))
		{
			return false;
		}
		if (!GetRifleVPFPShooter(refs))
		{
			return false;
		}
		if (!GetRifleFPH(refs))
		{
			return false;
		}
		if (!GetRifleFirstPersonWeapon(refs))
		{
			return false;
		}
		if (!GetRifleAnimator(refs))
		{
			return false;
		}
		if (!GetRifleGearItem(refs))
		{
			return false;
		}
		if (!GetRifleGunItem(refs))
		{
			return false;
		}
		if (!GetRifleAmmoSingle(refs))
		{
			return false;
		}
		if (!GetRifleAmmoBox(refs))
		{
			return false;
		}
		if (!GetRifleCasing(refs))
		{
			return false;
		}
		return true;
	}

	internal bool GetRifleAmmoSingle(ConfigurationReferences refs)
	{
		if (!GetRifleAmmoSingleAsset(refs))
		{
			return false;
		}
		if (!GetRifleAmmoSingleGearItem(refs))
		{
			return false;
		}
		if (!GetRifleAmmoSingleAmmoItem(refs))
		{
			return false;
		}
		if (!GetRifleAmmoSingleStackableItem(refs))
		{
			return false;
		}
		GetRifleAmmoSingleHarvestable(refs);
		return true;
	}

	internal bool GetRifleAmmoBox(ConfigurationReferences refs)
	{
		if (!GetRifleAmmoBoxAsset(refs))
		{
			return false;
		}
		if (!GetRifleAmmoBoxGearItem(refs))
		{
			return false;
		}
		if (!GetRifleAmmoBoxAmmoItem(refs))
		{
			return false;
		}
		if (!GetRifleAmmoBoxStackableItem(refs))
		{
			return false;
		}
		GetRifleAmmoBoxHarvestable(refs);
		return true;
	}

	internal bool GetRifleCasing(ConfigurationReferences refs)
	{
		if (!GetRifleCasingAsset(refs))
		{
			return false;
		}
		if (!GetRifleCasingGearItem(refs))
		{
			return false;
		}
		if (!GetRifleCasingStackableItem(refs))
		{
			return false;
		}
		return true;
	}

	internal bool GetShotgunAmmoSingle(ConfigurationReferences refs, string ammoType)
	{
		if (!GetShotgunAmmoSingleAsset(refs, ammoType))
		{
			return false;
		}
		if (!GetShotgunAmmoSingleGearItem(refs, ammoType))
		{
			return false;
		}
		if (!GetShotgunAmmoSingleAmmoItem(refs, ammoType))
		{
			return false;
		}
		if (!GetShotgunAmmoSingleStackableItem(refs, ammoType))
		{
			return false;
		}
		GetShotgunAmmoSingleHarvestable(refs, ammoType);
		return true;
	}

	internal bool GetShotgunAmmoBox(ConfigurationReferences refs, string ammoType)
	{
		if (!GetShotgunAmmoBoxAsset(refs, ammoType))
		{
			return false;
		}
		if (!GetShotgunAmmoBoxGearItem(refs, ammoType))
		{
			return false;
		}
		if (!GetShotgunAmmoBoxAmmoItem(refs, ammoType))
		{
			return false;
		}
		if (!GetShotgunAmmoBoxStackableItem(refs, ammoType))
		{
			return false;
		}
		GetShotgunAmmoBoxHarvestable(refs, ammoType);
		return true;
	}

	internal bool GetShotgunCasing(ConfigurationReferences refs)
	{
		if (!GetShotgunCasingAsset(refs))
		{
			return false;
		}
		if (!GetShotgunCasingGearItem(refs))
		{
			return false;
		}
		if (!GetShotgunCasingStackableItem(refs))
		{
			return false;
		}
		return true;
	}

	internal bool GetRifleAmmoSingleAsset(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoSingle.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_RifleAmmoSingle").WaitForCompletion();
		}, () => (Object)(object)refs2.Rifle.AmmoSingle.Prefab != (Object)null, "Null GEAR_RifleAmmoSingle", "GetRifleAmmoSingleAsset");
	}

	internal bool GetRifleAmmoSingleGearItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoSingle.GearItem = refs2.Rifle.AmmoSingle.Prefab.GetComponent<GearItem>();
		}, () => (Object)(object)refs2.Rifle.AmmoSingle.GearItem != (Object)null, "Null GEAR_RifleAmmoSingle.GearItem", "GetRifleAmmoSingleGearItem");
	}

	internal bool GetRifleAmmoSingleAmmoItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoSingle.AmmoItem = refs2.Rifle.AmmoSingle.Prefab.GetComponent<AmmoItem>();
		}, () => (Object)(object)refs2.Rifle.AmmoSingle.AmmoItem != (Object)null, "Null GEAR_RifleAmmoSingle.AmmoItem", "GetRifleAmmoSingleAmmoItem");
	}

	internal bool GetRifleAmmoSingleStackableItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoSingle.StackableItem = refs2.Rifle.AmmoSingle.Prefab.GetComponent<StackableItem>();
		}, () => (Object)(object)refs2.Rifle.AmmoSingle.StackableItem != (Object)null, "Null GEAR_RifleAmmoSingle.StackableItem", "GetRifleAmmoSingleStackableItem");
	}

	internal bool GetRifleAmmoSingleHarvestable(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoSingle.Harvestable = refs2.Rifle.AmmoSingle.Prefab.GetComponent<Harvestable>();
		}, () => (Object)(object)refs2.Rifle.AmmoSingle.Harvestable != (Object)null, "Null GEAR_RifleAmmoSingle.Harvestable", "GetRifleAmmoSingleHarvestable");
	}

	internal bool GetRifleAmmoBoxAsset(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoBox.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_RifleAmmoBox").WaitForCompletion();
		}, () => (Object)(object)refs2.Rifle.AmmoBox.Prefab != (Object)null, "Null GEAR_RifleAmmoBox", "GetRifleAmmoBoxAsset");
	}

	internal bool GetRifleAmmoBoxGearItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoBox.GearItem = refs2.Rifle.AmmoBox.Prefab.GetComponent<GearItem>();
		}, () => (Object)(object)refs2.Rifle.AmmoBox.GearItem != (Object)null, "Null GEAR_RifleAmmoBox.GearItem", "GetRifleAmmoBoxGearItem");
	}

	internal bool GetRifleAmmoBoxAmmoItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoBox.AmmoItem = refs2.Rifle.AmmoBox.Prefab.GetComponent<AmmoItem>();
		}, () => (Object)(object)refs2.Rifle.AmmoBox.AmmoItem != (Object)null, "Null GEAR_RifleAmmoBox.AmmoItem", "GetRifleAmmoBoxAmmoItem");
	}

	internal bool GetRifleAmmoBoxStackableItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoBox.StackableItem = refs2.Rifle.AmmoBox.Prefab.GetComponent<StackableItem>();
		}, () => (Object)(object)refs2.Rifle.AmmoBox.StackableItem != (Object)null, "Null GEAR_RifleAmmoBox.StackableItem", "GetRifleAmmoBoxStackableItem");
	}

	internal bool GetRifleAmmoBoxHarvestable(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.AmmoBox.Harvestable = refs2.Rifle.AmmoBox.Prefab.GetComponent<Harvestable>();
		}, () => (Object)(object)refs2.Rifle.AmmoBox.Harvestable != (Object)null, "Null GEAR_RifleAmmoBox.Harvestable", "GetRifleAmmoBoxHarvestable");
	}

	internal bool GetRifleCasingAsset(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.Casing.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_RifleAmmoCasing").WaitForCompletion();
		}, () => (Object)(object)refs2.Rifle.Casing.Prefab != (Object)null, "Null GEAR_RifleAmmoCasing", "GetRifleCasingAsset");
	}

	internal bool GetRifleCasingGearItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.Casing.GearItem = refs2.Rifle.Casing.Prefab.GetComponent<GearItem>();
		}, () => (Object)(object)refs2.Rifle.Casing.GearItem != (Object)null, "Null GEAR_RifleAmmoCasing.GearItem", "GetRifleCasingGearItem");
	}

	internal bool GetRifleCasingStackableItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Rifle.Casing.StackableItem = refs2.Rifle.Casing.Prefab.GetComponent<StackableItem>();
		}, () => (Object)(object)refs2.Rifle.Casing.StackableItem != (Object)null, "Null GEAR_RifleAmmoCasing.StackableItem", "GetRifleCasingStackableItem");
	}

	internal bool GetShotgunAmmoSingleAsset(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_ShotgunAmmoSingle" + ammoType2).WaitForCompletion();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Prefab != (Object)null, "Null GEAR_ShotgunAmmoSingle" + ammoType2, "GetShotgunAmmoSingleAsset");
	}

	internal bool GetShotgunAmmoSingleGearItem(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.GearItem = refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Prefab.GetComponent<GearItem>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.GearItem != (Object)null, "Null GEAR_ShotgunAmmoSingle" + ammoType2 + ".GearItem", "GetShotgunAmmoSingleGearItem");
	}

	internal bool GetShotgunAmmoSingleAmmoItem(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.AmmoItem = refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Prefab.GetComponent<AmmoItem>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.AmmoItem != (Object)null, "Null GEAR_ShotgunAmmoSingle" + ammoType2 + ".AmmoItem", "GetShotgunAmmoSingleAmmoItem");
	}

	internal bool GetShotgunAmmoSingleStackableItem(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.StackableItem = refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Prefab.GetComponent<StackableItem>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.StackableItem != (Object)null, "Null GEAR_ShotgunAmmoSingle" + ammoType2 + ".StackableItem", "GetShotgunAmmoSingleStackableItem");
	}

	internal bool GetShotgunAmmoSingleHarvestable(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Harvestable = refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Prefab.GetComponent<Harvestable>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoSingle.Harvestable != (Object)null, "Null GEAR_ShotgunAmmoSingle" + ammoType2 + ".Harvestable", "GetShotgunAmmoSingleHarvestable");
	}

	internal bool GetShotgunAmmoBoxAsset(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_ShotgunAmmoBox" + ammoType2).WaitForCompletion();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Prefab != (Object)null, "Null GEAR_ShotgunAmmoBox" + ammoType2, "GetShotgunAmmoBoxAsset");
	}

	internal bool GetShotgunAmmoBoxGearItem(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.GearItem = refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Prefab.GetComponent<GearItem>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.GearItem != (Object)null, "Null GEAR_ShotgunAmmoBox" + ammoType2 + ".GearItem", "GetShotgunAmmoBoxGearItem");
	}

	internal bool GetShotgunAmmoBoxAmmoItem(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.AmmoItem = refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Prefab.GetComponent<AmmoItem>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.AmmoItem != (Object)null, "Null GEAR_ShotgunAmmoBox" + ammoType2 + ".AmmoItem", "GetShotgunAmmoBoxAmmoItem");
	}

	internal bool GetShotgunAmmoBoxStackableItem(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.StackableItem = refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Prefab.GetComponent<StackableItem>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.StackableItem != (Object)null, "Null GEAR_ShotgunAmmoBox" + ammoType2 + ".StackableItem", "GetShotgunAmmoBoxStackableItem");
	}

	internal bool GetShotgunAmmoBoxHarvestable(ConfigurationReferences refs, string ammoType)
	{
		ConfigurationReferences refs2 = refs;
		string ammoType2 = ammoType;
		return Attempt(delegate
		{
			refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Harvestable = refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Prefab.GetComponent<Harvestable>();
		}, () => (Object)(object)refs2.Shotgun.AmmoTypes[ammoType2].AmmoBox.Harvestable != (Object)null, "Null GEAR_ShotgunAmmoBox" + ammoType2 + ".Harvestable", "GetShotgunAmmoBoxHarvestable");
	}

	internal bool GetShotgunCasingAsset(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Shotgun.Casing.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_ShotgunAmmoCasing").WaitForCompletion();
		}, () => (Object)(object)refs2.Shotgun.Casing.Prefab != (Object)null, "Null GEAR_ShotgunAmmoCasing", "GetShotgunCasingAsset");
	}

	internal bool GetShotgunCasingGearItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Shotgun.Casing.GearItem = refs2.Shotgun.Casing.Prefab.GetComponent<GearItem>();
		}, () => (Object)(object)refs2.Shotgun.Casing.GearItem != (Object)null, "Null GEAR_ShotgunAmmoCasing.GearItem", "GetShotgunCasingGearItem");
	}

	internal bool GetShotgunCasingStackableItem(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		return Attempt(delegate
		{
			refs2.Shotgun.Casing.StackableItem = refs2.Shotgun.Casing.Prefab.GetComponent<StackableItem>();
		}, () => (Object)(object)refs2.Shotgun.Casing.StackableItem != (Object)null, "Null GEAR_ShotgunAmmoCasing.StackableItem", "GetShotgunCasingStackableItem");
	}

	internal bool GetFPSPlayer(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetFPSPlayer");
		refs.FPSPlayer = GameManager.GetVpFPSPlayer();
		if ((Object)(object)refs.FPSPlayer == (Object)null)
		{
			MelonLogger.Error("Could not summon vp_FPSPlayer");
		}
		return (Object)(object)refs.FPSPlayer != (Object)null;
	}

	internal bool GetFPSCamera(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetFPSCamera");
		vp_FPSPlayer? fPSPlayer = refs.FPSPlayer;
		refs.FPSCamera = ((fPSPlayer != null) ? fPSPlayer.FPSCamera : null) ?? null;
		if ((Object)(object)refs.FPSCamera == (Object)null)
		{
			MelonLogger.Error("Could not get vp_FPSCamera from vp_FPSPlayer instance");
		}
		return (Object)(object)refs.FPSCamera != (Object)null;
	}

	internal bool GetRifleAsset(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleAsset");
		refs.Rifle.Gun.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Rifle").WaitForCompletion();
		if ((Object)(object)refs.Rifle.Gun.Prefab == (Object)null)
		{
			MelonLogger.Error("Could not summon rifle prefab");
			return false;
		}
		return true;
	}

	internal bool GetRifleFirstPersonItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleFirstPersonItem");
		refs.Rifle.Gun.FirstPersonItem = refs.Rifle.Gun.Prefab.GetComponent<FirstPersonItem>();
		if ((Object)(object)refs.Rifle.Gun.FirstPersonItem == (Object)null)
		{
			MelonLogger.Error("Could not find rifle first person item");
			return false;
		}
		return true;
	}

	internal bool GetRifleFPSItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleFPSItem");
		refs.Rifle.Floating.FPSItem = refs.Rifle.Gun.FirstPersonItem.m_ItemData;
		if ((Object)(object)refs.Rifle.Floating.FPSItem == (Object)null)
		{
			MelonLogger.Error("Could not find rifle FPSItem");
			return false;
		}
		return true;
	}

	internal bool GetRifleVPFPSRoot(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleVPFPSWeapon");
		Dictionary<FPSItem, vp_FPSWeapon>.Enumerator enumerator = refs.FPSCamera.m_Weapons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<FPSItem, vp_FPSWeapon> current = enumerator.Current;
			if (((Object)current.Value).name.Contains("Rifle"))
			{
				refs.Rifle.WeaponView.Root = ((Component)current.Value).gameObject;
				break;
			}
		}
		if ((Object)(object)refs.Rifle.WeaponView.Root == (Object)null)
		{
			MelonLogger.Error("Could not find rifle vp_FPSWeapon script amongst all of vp_FPSCamera.m_Weapons");
			return false;
		}
		return true;
	}

	internal bool GetRifleVPFPSWeapon(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleVPFPSWeapon");
		refs.Rifle.WeaponView.FPSWeapon = refs.Rifle.WeaponView.Root.GetComponent<vp_FPSWeapon>();
		if ((Object)(object)refs.Rifle.WeaponView.FPSWeapon == (Object)null)
		{
			MelonLogger.Error("Could not find rifle vp_FPSWeapon");
			return false;
		}
		return true;
	}

	internal bool GetRifleVPFPShooter(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleVPFPShooter");
		refs.Rifle.WeaponView.FPSShooter = refs.Rifle.WeaponView.Root.GetComponent<vp_FPSShooter>();
		if ((Object)(object)refs.Rifle.WeaponView.FPSShooter == (Object)null)
		{
			MelonLogger.Error("Could not find rifle vp_FPSShooter");
			return false;
		}
		return true;
	}

	internal bool GetRifleFPH(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleFPH");
		refs.Rifle.FPH.Root = ((Component)refs.Rifle.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulder).gameObject;
		if ((Object)(object)refs.Rifle.FPH.Root == (Object)null)
		{
			MelonLogger.Error("Could not find rifle FPH root from rifle vp_FPSWeapon");
			return false;
		}
		return true;
	}

	internal bool GetRifleFirstPersonWeapon(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleFirstPersonWeapon");
		refs.Rifle.FPH.FirstPersonWeapon = refs.Rifle.FPH.Root.GetComponent<FirstPersonWeapon>();
		if ((Object)(object)refs.Rifle.FPH.FirstPersonWeapon == (Object)null)
		{
			MelonLogger.Error("Could not find rifle first person weapon from rifle FPH root");
			return false;
		}
		return true;
	}

	internal bool GetRifleAnimator(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleAnimator");
		refs.Rifle.FPH.Animator = refs.Rifle.FPH.Root.GetComponent<Animator>();
		if ((Object)(object)refs.Rifle.FPH.Animator == (Object)null)
		{
			MelonLogger.Error("Could not find rifle animator from rifle FPH root");
			return false;
		}
		return true;
	}

	internal bool GetRifleGunItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleGunItem");
		refs.Rifle.Gun.GunItem = refs.Rifle.Gun.Prefab.GetComponent<GunItem>();
		if ((Object)(object)refs.Rifle.Gun.GunItem == (Object)null)
		{
			MelonLogger.Error("Could not find rifle GunItem from rifle prefab");
			return false;
		}
		return true;
	}

	internal bool GetRifleGearItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetRifleGearItem");
		refs.Rifle.Gun.GearItem = refs.Rifle.Gun.Prefab.GetComponent<GearItem>();
		if ((Object)(object)refs.Rifle.Gun.GearItem == (Object)null)
		{
			MelonLogger.Error("Could not find rifle gearitem from rifle prefab");
			return false;
		}
		return true;
	}

	internal bool GetShotgunPrefab(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetShotgunPrefab");
		refs.Shotgun.Gun.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Shotgun").WaitForCompletion();
		if ((Object)(object)refs.Shotgun.Gun.Prefab == (Object)null)
		{
			MelonLogger.Error("Could not summon GEAR_Shotgun");
		}
		return (Object)(object)refs.Shotgun.Gun.Prefab != (Object)null;
	}

	internal bool GetShotgunGearItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetShotgunGearItem");
		GunPrefabRefs gun = refs.Shotgun.Gun;
		GameObject? prefab = refs.Shotgun.Gun.Prefab;
		gun.GearItem = ((prefab != null) ? prefab.GetComponent<GearItem>() : null);
		if ((Object)(object)refs.Shotgun.Gun.GearItem == (Object)null)
		{
			MelonLogger.Error("Null Shotgun GearItem");
		}
		return (Object)(object)refs.Shotgun.Gun.GearItem != (Object)null;
	}

	internal bool GetShotgunModComponent(ConfigurationReferences refs)
	{
		MelonLogger.Msg("GetShotgunModComponent");
		GunPrefabRefs gun = refs.Shotgun.Gun;
		GameObject? prefab = refs.Shotgun.Gun.Prefab;
		gun.ModComponent = ((prefab != null) ? prefab.GetComponent<ModGenericComponent>() : null);
		if ((Object)(object)refs.Shotgun.Gun.ModComponent == (Object)null)
		{
			MelonLogger.Error("Null Shotgun ModComponent");
		}
		return (Object)(object)refs.Shotgun.Gun.ModComponent != (Object)null;
	}

	internal bool Instantiate(ConfigurationReferences refs)
	{
		if (!InstantiateTopLevel(refs))
		{
			return false;
		}
		if (!InstantiateShotgun(refs))
		{
			return false;
		}
		return true;
	}

	internal bool InstantiateTopLevel(ConfigurationReferences refs)
	{
		return true;
	}

	internal bool InstantiateShotgun(ConfigurationReferences refs)
	{
		if (!InstantiateShotgunFPH(refs))
		{
			return false;
		}
		if (!InstantiateShotgunFirstPersonWeapon(refs))
		{
			return false;
		}
		if (!InstantiateShotgunAnimator(refs))
		{
			return false;
		}
		if (!InstantiateModdedAnimator(refs))
		{
			return false;
		}
		if (!InstantiateShotgunWeaponGroup(refs))
		{
			return false;
		}
		if (!InstantiateShotgunVPFPSWeapon(refs))
		{
			return false;
		}
		if (!InstantiateShotgunVPFPSShooter(refs))
		{
			return false;
		}
		if (!InstantiateShotgunStruggleBonus(refs))
		{
			return false;
		}
		if (!InstantiateShotgunGunItem(refs))
		{
			return false;
		}
		if (!InstantiateShotgunFirstPersonItem(refs))
		{
			return false;
		}
		if (!InstantiateShotgunFPSItem(refs))
		{
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunStruggleBonus(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunStruggleBonus");
		GunPrefabRefs gun = refs.Shotgun.Gun;
		GameObject? prefab = refs.Shotgun.Gun.Prefab;
		gun.StruggleBonus = ((prefab != null) ? GameObjectExtensions.GetOrCreateComponent<StruggleBonus>(prefab) : null) ?? null;
		if ((Object)(object)refs.Shotgun.Gun.StruggleBonus == (Object)null)
		{
			MelonLogger.Error("Null StruggleBonus");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunGunItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunGunItem");
		GunPrefabRefs gun = refs.Shotgun.Gun;
		GameObject? prefab = refs.Shotgun.Gun.Prefab;
		gun.GunItem = ((prefab != null) ? GameObjectExtensions.GetOrCreateComponent<GunItem>(prefab) : null) ?? null;
		if ((Object)(object)refs.Shotgun.Gun.GunItem == (Object)null)
		{
			MelonLogger.Error("Null GunItem");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunFirstPersonItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunFirstPersonItem");
		GunPrefabRefs gun = refs.Shotgun.Gun;
		GameObject? prefab = refs.Shotgun.Gun.Prefab;
		gun.FirstPersonItem = ((prefab != null) ? GameObjectExtensions.GetOrCreateComponent<FirstPersonItem>(prefab) : null) ?? null;
		if ((Object)(object)refs.Shotgun.Gun.FirstPersonItem == (Object)null)
		{
			MelonLogger.Error("Null FirstPersonItem");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunWeaponGroup(ConfigurationReferences refs)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		MelonLogger.Msg("InstantiateShotgunWeaponGroup");
		refs.Shotgun.WeaponView.Root = new GameObject();
		refs.Shotgun.WeaponView.Root.transform.SetParent(((Component)refs.FPSCamera.m_WeaponParent).transform, false);
		return true;
	}

	internal bool InstantiateShotgunVPFPSWeapon(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunVPFPSWeapon");
		refs.Shotgun.WeaponView.FPSWeapon = GameObjectExtensions.GetOrCreateComponent<vp_FPSWeapon>(refs.Shotgun.WeaponView.Root);
		if ((Object)(object)refs.Shotgun.WeaponView.FPSWeapon == (Object)null)
		{
			MelonLogger.Error("Null vp_FPSWeapon");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunVPFPSShooter(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunVPFPSShooter");
		refs.Shotgun.WeaponView.FPSShooter = GameObjectExtensions.GetOrCreateComponent<vp_FPSShooter>(refs.Shotgun.WeaponView.Root);
		if ((Object)(object)refs.Shotgun.WeaponView.FPSShooter == (Object)null)
		{
			MelonLogger.Error("Null vp_FPSShooter");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunFPSItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunFPSItem");
		refs.Shotgun.Floating.FPSItem = ScriptableObject.CreateInstance<FPSItem>();
		if ((Object)(object)refs.Shotgun.Floating.FPSItem == (Object)null)
		{
			MelonLogger.Error("Null FPSItem");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunFPH(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunFPH");
		refs.Shotgun.FPH.Root = Addressables.LoadAssetAsync<GameObject>("FPH_Shotgun").WaitForCompletion();
		if ((Object)(object)refs.Shotgun.FPH.Root == (Object)null)
		{
			MelonLogger.Error("Could not summon FPH_Shotgun");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunFirstPersonWeapon(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunFirstPersonWeapon");
		refs.Shotgun.FPH.FirstPersonWeapon = GameObjectExtensions.GetOrCreateComponent<FirstPersonWeapon>(refs.Shotgun.FPH.Root);
		if ((Object)(object)refs.Shotgun.FPH.FirstPersonWeapon == (Object)null)
		{
			MelonLogger.Error("Could not attach FirstPersonWeapon to FPH_Shotgun");
			return false;
		}
		return true;
	}

	internal bool InstantiateShotgunAnimator(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateShotgunAnimator");
		refs.Shotgun.FPH.Animator = GameObjectExtensions.GetOrCreateComponent<Animator>(refs.Shotgun.FPH.Root);
		if ((Object)(object)refs.Shotgun.FPH.Animator == (Object)null)
		{
			MelonLogger.Error("Could not attach Animator to FPH_Shotgun");
			return false;
		}
		return true;
	}

	internal bool InstantiateModdedAnimator(ConfigurationReferences refs)
	{
		MelonLogger.Msg("InstantiateModdedAnimator");
		Animator animator = GameManager.GetPlayerAnimationComponent().m_Animator;
		if ((Object)(object)animator == (Object)null)
		{
			MelonLogger.Error("Could not find player animator for sub-animator placement");
			return false;
		}
		refs.VanillaAnimator = animator;
		Transform val = null;
		int i = 0;
		for (int childCount = ((Component)animator).gameObject.transform.childCount; i < childCount; i++)
		{
			Transform child = ((Component)animator).gameObject.transform.GetChild(i);
			if (((Object)((Component)child).gameObject).name.Contains("GAME_DATA"))
			{
				val = child;
				break;
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			MelonLogger.Error("Could not find GAME_DATA child of player animator");
			return false;
		}
		refs.ModdedAnimator = GameObjectExtensions.GetOrCreateComponent<Animator>(((Component)val).gameObject);
		return true;
	}

	private bool Configure(ConfigurationReferences refs)
	{
		if (!ConfigureTopLevel(refs))
		{
			return false;
		}
		if (!ConfigureShotgun(refs))
		{
			return false;
		}
		return true;
	}

	internal bool ConfigureTopLevel(ConfigurationReferences refs)
	{
		if (!ConfigureRadial(refs))
		{
			return false;
		}
		return true;
	}

	internal bool ConfigureShotgun(ConfigurationReferences refs)
	{
		try
		{
			string[] shotgunAmmoTypes = ShotgunAmmoTypes;
			foreach (string ammoType in shotgunAmmoTypes)
			{
				ConfigureShotgunAmmoSingle(refs, ammoType);
				ConfigureShotgunAmmoBox(refs, ammoType);
				ConfigureShotgunAmmoCasing(refs, ammoType);
			}
			ConfigureShotgunGearItem(refs);
			ConfigureShotgunGunItem(refs);
			ConfigureShotgunFirstPersonItem(refs);
			ConfigureShotgunStruggleBonus(refs);
			ConfigureShotgunFPSItem(refs);
			ConfigureShotgunFPHRoot(refs);
			ConfigureShotgunFirstPersonWeapon(refs);
			ConfigureShotgunAnimator(refs);
			ConfigureModdedAnimator(refs);
			ConfigureShotgunFPSShooter(refs);
			ConfigureShotgunFPSWeapon(refs);
			ConfigureShotgunFPSRoot(refs);
			return true;
		}
		catch (System.Exception value)
		{
			MelonLogger.Error($"Error during detailed configuration: {value}");
			return false;
		}
	}

	internal bool ConfigureRadial(ConfigurationReferences refs)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		MelonLogger.Msg("ConfigureRadial");
		Panel_ActionsRadial panel = InterfaceManager.GetPanel<Panel_ActionsRadial>();
		if ((Object)(object)panel == (Object)null)
		{
			MelonLogger.Error("Null Panel_ActionsRadial");
			return false;
		}
		if (((Il2CppArrayBase<string>)(object)panel.m_WeaponRadialOrder).Contains("GEAR_Shotgun"))
		{
			MelonLogger.Msg("Already configured radial for GEAR_Shotgun, skipping");
			return true;
		}
		Il2CppStringArray val = new Il2CppStringArray((long)(((Il2CppArrayBase<string>)(object)panel.m_WeaponRadialOrder).Length + 1));
		int i = 0;
		for (int length = ((Il2CppArrayBase<string>)(object)panel.m_WeaponRadialOrder).Length; i < length; i++)
		{
			((Il2CppArrayBase<string>)(object)val)[i] = ((Il2CppArrayBase<string>)(object)panel.m_WeaponRadialOrder)[i];
		}
		((Il2CppArrayBase<string>)(object)val)[((Il2CppArrayBase<string>)(object)val).Length - 1] = "GEAR_Shotgun";
		panel.m_WeaponRadialOrder = val;
		return true;
	}

	internal void ConfigureShotgunAmmoSingle(ConfigurationReferences refs, string ammoType)
	{
		MelonLogger.Msg("ConfigureShotgunAmmoSingle" + ammoType);
		ShotgunAmmoTypeRefs shotgunAmmoTypeRefs = refs.Shotgun.AmmoTypes[ammoType];
		shotgunAmmoTypeRefs.AmmoSingle.AmmoItem.m_AmmoForGunType = (GunType)4;
		shotgunAmmoTypeRefs.AmmoSingle.AmmoItem.m_LoadedHudIconSpriteName = "";
		shotgunAmmoTypeRefs.AmmoSingle.AmmoItem.m_EmptyHudIconSpriteName = "";
		((Object)shotgunAmmoTypeRefs.AmmoSingle.GearItem.m_GearItemData).name = "GEAR_ShotgunAmmoSingle" + ammoType;
		CustomAddressablesPatch.customAddressablePaths["ModdedGearItemData_ShotgunAmmoSingle" + ammoType] = (Object)(object)shotgunAmmoTypeRefs.AmmoSingle.GearItem.m_GearItemData;
		shotgunAmmoTypeRefs.AmmoSingle.GearItem.m_AmmoItem = shotgunAmmoTypeRefs.AmmoSingle.AmmoItem;
	}

	internal void ConfigureShotgunAmmoBox(ConfigurationReferences refs, string ammoType)
	{
		MelonLogger.Msg("ConfigureShotgunAmmoBox" + ammoType);
		ShotgunAmmoTypeRefs shotgunAmmoTypeRefs = refs.Shotgun.AmmoTypes[ammoType];
		shotgunAmmoTypeRefs.AmmoBox.AmmoItem.m_AmmoForGunType = (GunType)4;
		shotgunAmmoTypeRefs.AmmoBox.AmmoItem.m_LoadedHudIconSpriteName = "";
		shotgunAmmoTypeRefs.AmmoBox.AmmoItem.m_EmptyHudIconSpriteName = "";
		((Object)shotgunAmmoTypeRefs.AmmoBox.GearItem.m_GearItemData).name = "GEAR_ShotgunAmmoBox" + ammoType;
		CustomAddressablesPatch.customAddressablePaths["ModdedGearItemData_ShotgunAmmoBox" + ammoType] = (Object)(object)shotgunAmmoTypeRefs.AmmoBox.GearItem.m_GearItemData;
		shotgunAmmoTypeRefs.AmmoBox.GearItem.m_AmmoItem = shotgunAmmoTypeRefs.AmmoBox.AmmoItem;
		CustomAmmoHUD.HUDPreset hUDPreset = new CustomAmmoHUD.HUDPreset();
		hUDPreset.LoadedSprite = Addressables.LoadAssetAsync<Sprite>("ico_Shotgun" + ammoType + "HUD").WaitForCompletion();
		hUDPreset.UnloadedSprite = Addressables.LoadAssetAsync<Sprite>("ico_ShotgunEmptyHUD").WaitForCompletion();
		hUDPreset.IconHeight = Settings.Instance.CustomAmmoHUDIconHeight;
		hUDPreset.IconWidth = hUDPreset.IconHeight / 3f;
		hUDPreset.IconSpacing = hUDPreset.IconWidth * Settings.Instance.CustomAmmoHUDIconSpacingRatio;
		hUDPreset.IconsPerRow = 2;
		CustomAmmoHUD.Current.TryRegisterHUDPreset("GEAR_Shotgun", "GEAR_ShotgunAmmoBox" + ammoType, hUDPreset);
	}

	internal void ConfigureShotgunAmmoCasing(ConfigurationReferences refs, string ammoType)
	{
		MelonLogger.Msg("ConfigureShotgunAmmoCasing" + ammoType);
	}

	internal void ConfigureShotgunGearItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunGearItem");
		GearItem gearItem = refs.Shotgun.Gun.GearItem;
		gearItem.m_StruggleBonus = refs.Shotgun.Gun.StruggleBonus;
		gearItem.m_FirstPersonItem = refs.Shotgun.Gun.FirstPersonItem;
		gearItem.m_GunItem = refs.Shotgun.Gun.GunItem;
		gearItem.m_Inspect = refs.Shotgun.Gun.Prefab.GetComponent<Inspect>();
	}

	internal void ConfigureShotgunGunItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunGunItem");
		GunItem gunItem = refs.Shotgun.Gun.GunItem;
		GunItem gunItem2 = refs.Rifle.Gun.GunItem;
		gunItem.m_AmmoReferences = new List<AssetReferenceT<GearItemData>>();
		string[] shotgunAmmoTypes = ShotgunAmmoTypes;
		foreach (string text in shotgunAmmoTypes)
		{
			gunItem.m_AmmoReferences.Add(new AssetReferenceT<GearItemData>("ModdedGearItemData_ShotgunAmmoBox" + text));
		}
		gunItem.m_GearItem = refs.Shotgun.Gun.GearItem;
		gunItem.m_CasingData = refs.Shotgun.Casing.GearItem.m_GearItemData;
		gunItem.m_AmmoSpriteName = gunItem2.m_AmmoSpriteName;

		gunItem.m_CasingAudio = gunItem2.m_CasingAudio;
		gunItem.m_DryFireAudio = gunItem2.m_DryFireAudio;
		gunItem.m_FireAudio = null;
		gunItem.m_ImpactAudio = gunItem2.m_ImpactAudio;
		gunItem.m_UncockAudio = gunItem2.m_UncockAudio;
		gunItem.m_MisfireTable = gunItem2.m_MisfireTable;
		gunItem.m_MisfirePlayerVO = gunItem2.m_MisfirePlayerVO;
		gunItem.m_AimingStaminaExhaustedSound = gunItem2.m_AimingStaminaExhaustedSound;
		gunItem.m_GunType = (GunType)4;
		gunItem.m_ClipSize = 2;
		gunItem.m_RoundsToReloadPerClip = 2;
		gunItem.m_DamageHP = 125f;
		gunItem.m_AccuracyRange = 35f;
		gunItem.m_FiringRateSeconds = 0.35f;
		gunItem.m_MinDistanceForAimAssist = 15f;
		gunItem.m_MuzzleFlash_FlashDelay = 0.1f;
		gunItem.m_MuzzleFlash_SmokeDelay = 0.15f;
		ShotgunSkill.ApplyToGunItem(gunItem);
	}

	internal void ConfigureShotgunFirstPersonItem(ConfigurationReferences refs)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		MelonLogger.Msg("ConfigureShotgunFirstPersonItem");
		FirstPersonItem firstPersonItem = refs.Shotgun.Gun.FirstPersonItem;
		FirstPersonItem firstPersonItem2 = refs.Rifle.Gun.FirstPersonItem;
		firstPersonItem.m_FirstPersonObjectName = "Shotgun";
		firstPersonItem.m_PlayerStateTransitions = new PlayerStateTransitions();
		firstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions = new Il2CppStructArray<PlayerAnimation.StateFlags>((long)((Il2CppArrayBase<PlayerAnimation.StateFlags>)(object)firstPersonItem2.m_PlayerStateTransitions.m_InvalidTransitions).Length);
		firstPersonItem.m_PlayerStateTransitions.m_ValidTransitions = new Il2CppStructArray<PlayerAnimation.StateFlags>((long)((Il2CppArrayBase<PlayerAnimation.StateFlags>)(object)firstPersonItem2.m_PlayerStateTransitions.m_ValidTransitions).Length);
		int num = 0;
		foreach (PlayerAnimation.StateFlags item in (Il2CppArrayBase<PlayerAnimation.StateFlags>)(object)firstPersonItem2.m_PlayerStateTransitions.m_InvalidTransitions)
		{
			((Il2CppArrayBase<PlayerAnimation.StateFlags>)(object)firstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions)[num] = item;
			num++;
		}
		num = 0;
		foreach (PlayerAnimation.StateFlags item2 in (Il2CppArrayBase<PlayerAnimation.StateFlags>)(object)firstPersonItem2.m_PlayerStateTransitions.m_ValidTransitions)
		{
			((Il2CppArrayBase<PlayerAnimation.StateFlags>)(object)firstPersonItem.m_PlayerStateTransitions.m_ValidTransitions)[num] = item2;
			num++;
		}
		firstPersonItem.m_ItemData = refs.Shotgun.Floating.FPSItem;
		firstPersonItem.m_WieldAudioEvent = firstPersonItem2.m_WieldAudioEvent;
		firstPersonItem.m_UnwieldAudioEvent = firstPersonItem2.m_UnwieldAudioEvent;
	}

	internal void ConfigureShotgunStruggleBonus(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunStruggleBonus");
		refs.Shotgun.Gun.StruggleBonus.m_StruggleWeaponType = (StruggleBonus.StruggleWeaponType)8;
	}

	internal void ConfigureShotgunFPSItem(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunFPSItem");
		((Object)refs.Shotgun.Floating.FPSItem).name = "FPSItem_Shotgun";
		refs.Shotgun.Floating.FPSItem.m_FPSWeaponPrefab = refs.Rifle.Floating.FPSItem.m_FPSWeaponPrefab;
	}

	internal void ConfigureShotgunFPHRoot(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunFPHRoot");
		((Object)refs.Shotgun.WeaponView.Root).name = "Shotgun";
	}

	internal void ConfigureShotgunFirstPersonWeapon(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunFirstPersonWeapon");
		refs.Shotgun.FPH.FirstPersonWeapon.m_Renderable = ((Component)refs.Shotgun.FPH.Root.transform.GetChild(0)).gameObject;
		refs.Shotgun.FPH.FirstPersonWeapon.m_Animator = refs.Shotgun.FPH.Animator;
		foreach (Transform componentsInChild in refs.Shotgun.FPH.Root.GetComponentsInChildren<Transform>())
		{
			switch (((Object)((Component)componentsInChild).gameObject).name)
			{
			case "FrontSight":
				refs.Shotgun.FPH.FirstPersonWeapon.m_FrontSight = componentsInChild;
				break;
			case "RearSight":
				refs.Shotgun.FPH.FirstPersonWeapon.m_RearSight = componentsInChild;
				break;
			case "BulletEmissionPoint":
				refs.Shotgun.FPH.FirstPersonWeapon.m_BulletEmissionPoint = ((Component)componentsInChild).gameObject;
				break;
			}
		}
	}

	internal void ConfigureShotgunAnimator(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunAnimator");
		refs.Shotgun.FPH.Animator.cullingMode = (AnimatorCullingMode)1;
	}

	internal void ConfigureModdedAnimator(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureModdedAnimator");
		RuntimeAnimatorController runtimeAnimatorController = Addressables.LoadAssetAsync<RuntimeAnimatorController>("FPH_Shotgun_Controller").WaitForCompletion();
		if ((Object)(object)runtimeAnimatorController == (Object)null)
			MelonLogger.Warning("FPH_Shotgun_Controller loaded as NULL! Animations will not work.");
		else
			MelonLogger.Msg($"FPH_Shotgun_Controller loaded OK: {((Object)runtimeAnimatorController).name}");
		refs.ModdedAnimator.runtimeAnimatorController = runtimeAnimatorController;
		refs.ModdedAnimator.cullingMode = (AnimatorCullingMode)1;
		((Behaviour)refs.ModdedAnimator).enabled = false;
		GameObjectExtensions.GetOrCreateComponent<AnimationEventProxy>(((Component)refs.ModdedAnimator).gameObject);
	}

	internal void ConfigureShotgunFPSShooter(ConfigurationReferences refs)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		MelonLogger.Msg("ConfigureShotgunFPSShooter");
		vp_FPSShooter fPSShooter = refs.Shotgun.WeaponView.FPSShooter;
		vp_FPSShooter fPSShooter2 = refs.Rifle.WeaponView.FPSShooter;
		fPSShooter.m_Weapon = refs.Shotgun.WeaponView.FPSWeapon;

		fPSShooter.MuzzleFlashPrefab = fPSShooter2.MuzzleFlashPrefab;
		fPSShooter.ProjectilePrefab = fPSShooter2.ProjectilePrefab;
		fPSShooter.ShellPrefab = fPSShooter2.ShellPrefab;
		fPSShooter.BulletEmissionLocator = refs.Shotgun.FPH.FirstPersonWeapon.m_BulletEmissionPoint.transform;
		((vp_Component)fPSShooter).m_StateManager.m_States.Clear();
		List<vp_StateInfo>.Enumerator enumerator = ((vp_Component)fPSShooter2).m_StateManager.m_States.GetEnumerator();
		while (enumerator.MoveNext())
		{
			vp_StateInfo current = enumerator.Current;
			vp_StateInfo val = new vp_StateInfo(current.TypeName, current.Name, (string)null, (TextAsset)null);
			vp_ComponentPreset val2 = new vp_ComponentPreset();
			val2.m_ComponentType = current.Preset.m_ComponentType;
			val2.m_Fields = new List<vp_ComponentPreset.Field>();
			List<vp_ComponentPreset.Field>.Enumerator enumerator2 = current.Preset.m_Fields.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				vp_ComponentPreset.Field current2 = enumerator2.Current;
				val2.m_Fields.Add(new vp_ComponentPreset.Field(current2.m_FieldInfo, current2.Args));
			}
			val.Preset = val2;
			val.TextAsset = current.TextAsset;
			((vp_Component)fPSShooter).m_StateManager.m_States.Add(val);
			if (current.Name == "Default")
			{
				((vp_Component)fPSShooter).m_DefaultState = val;
			}
		}
		fPSShooter.ProjectileScale = 1f;
		fPSShooter.ShellScale = 1f;
		fPSShooter.ShellEjectSpin = 0f;
		fPSShooter.MotionPositionRecoil = fPSShooter2.MotionPositionRecoil;
		fPSShooter.MotionRotationRecoil = fPSShooter2.MotionRotationRecoil;
		fPSShooter.MotionPositionReset = fPSShooter2.MotionPositionReset;
		fPSShooter.MotionRotationReset = fPSShooter2.MotionRotationReset;
		fPSShooter.MotionPositionPause = fPSShooter2.MotionPositionPause;
		fPSShooter.MotionRotationPause = fPSShooter2.MotionRotationPause;
		fPSShooter.MotionDryFireRecoil = fPSShooter2.MotionDryFireRecoil;
		((Behaviour)fPSShooter).enabled = true;
	}

	internal void ConfigureShotgunFPSWeapon(ConfigurationReferences refs)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		MelonLogger.Msg("ConfigureShotgunFPSWeapon");
		refs.Shotgun.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulderPrefab = refs.Shotgun.FPH.Root;
		((vp_Component)refs.Shotgun.WeaponView.FPSWeapon).m_StateManager.m_States.Clear();
		List<vp_StateInfo>.Enumerator enumerator = ((vp_Component)refs.Rifle.WeaponView.FPSWeapon).m_StateManager.m_States.GetEnumerator();
		while (enumerator.MoveNext())
		{
			vp_StateInfo current = enumerator.Current;
			vp_StateInfo val = new vp_StateInfo(current.TypeName, current.Name, (string)null, (TextAsset)null);
			vp_ComponentPreset val2 = new vp_ComponentPreset();
			val2.m_ComponentType = current.Preset.m_ComponentType;
			val2.m_Fields = new List<vp_ComponentPreset.Field>();
			List<vp_ComponentPreset.Field>.Enumerator enumerator2 = current.Preset.m_Fields.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				vp_ComponentPreset.Field current2 = enumerator2.Current;
				val2.m_Fields.Add(new vp_ComponentPreset.Field(current2.m_FieldInfo, current2.Args));
			}
			val.Preset = val2;
			val.TextAsset = current.TextAsset;
			((vp_Component)refs.Shotgun.WeaponView.FPSWeapon).m_StateManager.m_States.Add(val);
			if (current.Name == "Default")
			{
				((vp_Component)refs.Shotgun.WeaponView.FPSWeapon).m_DefaultState = val;
			}
		}
		refs.Shotgun.WeaponView.FPSWeapon.m_GearItem = refs.Shotgun.Gun.Prefab.GetComponent<GearItem>();
		refs.Shotgun.WeaponView.FPSWeapon.m_GunItem = refs.Shotgun.Gun.Prefab.GetComponent<GunItem>();
		refs.Shotgun.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulderPrefab.layer = 23;
		refs.Shotgun.WeaponView.FPSWeapon.m_UseFirstPersonHands = true;
		refs.Shotgun.WeaponView.FPSWeapon.PositionSpring2Stiffness = refs.Rifle.WeaponView.FPSWeapon.PositionSpring2Stiffness;
		refs.Shotgun.WeaponView.FPSWeapon.PositionSpring2Damping = refs.Rifle.WeaponView.FPSWeapon.PositionSpring2Damping;
		refs.Shotgun.WeaponView.FPSWeapon.RotationSpring2Stiffness = refs.Rifle.WeaponView.FPSWeapon.RotationSpring2Stiffness;
		refs.Shotgun.WeaponView.FPSWeapon.RotationSpring2Damping = refs.Rifle.WeaponView.FPSWeapon.RotationSpring2Damping;
		refs.Shotgun.WeaponView.FPSWeapon.PositionSpringStiffness = refs.Rifle.WeaponView.FPSWeapon.PositionSpringStiffness;
		refs.Shotgun.WeaponView.FPSWeapon.PositionSpringDamping = refs.Rifle.WeaponView.FPSWeapon.PositionSpringDamping;
		refs.Shotgun.WeaponView.FPSWeapon.RotationSpringStiffness = refs.Rifle.WeaponView.FPSWeapon.RotationSpringStiffness;
		refs.Shotgun.WeaponView.FPSWeapon.RotationSpringDamping = refs.Rifle.WeaponView.FPSWeapon.RotationSpringDamping;
		refs.Shotgun.WeaponView.FPSWeapon.PositionWalkSlide = refs.Rifle.WeaponView.FPSWeapon.PositionWalkSlide;
		refs.Shotgun.WeaponView.FPSWeapon.PositionFallRetract = refs.Rifle.WeaponView.FPSWeapon.PositionFallRetract;
		refs.Shotgun.WeaponView.FPSWeapon.PositionInputVelocityScale = refs.Rifle.WeaponView.FPSWeapon.PositionInputVelocityScale;
		refs.Shotgun.WeaponView.FPSWeapon.PositionMaxInputVelocity = refs.Rifle.WeaponView.FPSWeapon.PositionMaxInputVelocity;
		refs.Shotgun.WeaponView.FPSWeapon.RotationLookSway = refs.Rifle.WeaponView.FPSWeapon.RotationLookSway;
		refs.Shotgun.WeaponView.FPSWeapon.RotationStrafeSway = refs.Rifle.WeaponView.FPSWeapon.RotationStrafeSway;
		refs.Shotgun.WeaponView.FPSWeapon.RotationFallSway = refs.Rifle.WeaponView.FPSWeapon.RotationFallSway;
		refs.Shotgun.WeaponView.FPSWeapon.RotationSlopeSway = refs.Rifle.WeaponView.FPSWeapon.RotationSlopeSway;
		refs.Shotgun.WeaponView.FPSWeapon.RotationInputVelocityScale = refs.Rifle.WeaponView.FPSWeapon.RotationInputVelocityScale;
		refs.Shotgun.WeaponView.FPSWeapon.RotationMaxInputVelocity = refs.Rifle.WeaponView.FPSWeapon.RotationMaxInputVelocity;
		refs.Shotgun.WeaponView.FPSWeapon.BobRate = refs.Rifle.WeaponView.FPSWeapon.BobRate;
		refs.Shotgun.WeaponView.FPSWeapon.BobAmplitude = refs.Rifle.WeaponView.FPSWeapon.BobAmplitude;
		refs.Shotgun.WeaponView.FPSWeapon.BobInputVelocityScale = refs.Rifle.WeaponView.FPSWeapon.BobInputVelocityScale;
		refs.Shotgun.WeaponView.FPSWeapon.BobMaxInputVelocity = refs.Rifle.WeaponView.FPSWeapon.BobMaxInputVelocity;
		refs.Shotgun.WeaponView.FPSWeapon.ShakeSpeed = refs.Rifle.WeaponView.FPSWeapon.ShakeSpeed;
		refs.Shotgun.WeaponView.FPSWeapon.ShakeAmplitude = refs.Rifle.WeaponView.FPSWeapon.ShakeAmplitude;
		refs.Shotgun.WeaponView.FPSWeapon.MaybeBuildRuntimeHierarchy();
		refs.Shotgun.WeaponView.FPSWeapon.MaybeSpawnFirstPersonWeapon();
		((Behaviour)refs.Shotgun.WeaponView.FPSWeapon).enabled = true;
	}

	internal void ConfigureShotgunFPSRoot(ConfigurationReferences refs)
	{
		MelonLogger.Msg("ConfigureShotgunFPSRoot");
		((Object)((Component)refs.Shotgun.WeaponView.Root.transform.parent).gameObject).name = "FPSWeapon_ShotgunTransform";
	}
}
