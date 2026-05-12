using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using FiligranisSkills;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModData;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: ComVisible(false)]
[assembly: Guid("5c0dfb41-a109-48e8-9b78-a05c0b4be5c6")]
[assembly: AssemblyTitle("FiligranisSkills")]
[assembly: AssemblyDescription("Custom skill API")]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("FiligranisSkills")]
[assembly: AssemblyCopyright("Created by BA")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.0.3")]
[assembly: MelonInfo(typeof(global::FiligranisSkills.FiligranisSkills), "FiligranisSkills", "1.0.3", "BA", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonIncompatibleAssemblies(new string[] { "Sky Co-op LTS" })]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.0.3.0")]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace FiligranisSkills
{
	public static class BuildInfo
	{
		public const string Name = "FiligranisSkills";

		public const string Description = "Custom skill API";

		public const string Author = "BA";

		public const string Company = null;

		public const string Version = "1.0.3";

		public const string DownloadLink = null;
	}
	public class FiligranisSkills : MelonMod
	{
		internal static Dictionary<string, int> skills = new Dictionary<string, int>();

		internal static List<SkillDefinition> defs = new List<SkillDefinition>();

		internal static int customSkillOffset;

		public static FiligranisSkills Instance { get; private set; }

		internal ModDataManager ModSave { get; private set; } = new ModDataManager("FiligranisSkills");


		public override void OnEarlyInitializeMelon()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected I4, but got Unknown
			Instance = this;
			customSkillOffset = -1;
			SkillType[] values = Enum.GetValues<SkillType>();
			foreach (SkillType val in values)
			{
				if ((int)val > customSkillOffset)
				{
					customSkillOffset = (int)val;
				}
			}
		}

		public override void OnInitializeMelon()
		{
			uConsole.RegisterCommand("fskills_saved", DebugCommand.op_Implicit((Action)delegate
			{
				for (int i = customSkillOffset; i < GameManager.GetSkillsManager().m_Skills.Count; i++)
				{
					string text = ((Object)GameManager.GetSkillsManager().m_Skills[i]).name.Substring("Skill_".Length);
					Instance loggerInstance = ((MelonBase)Instance).LoggerInstance;
					if (loggerInstance != null)
					{
						loggerInstance.Msg("FSkill: " + text + " " + Instance.ModSave.Load("skillpoint_" + text));
					}
				}
			}));
			uConsole.RegisterCommand("fskills_add_point", DebugCommand.op_Implicit((Action)delegate
			{
				List<string> allParameters2 = uConsole.GetAllParameters();
				if (allParameters2 != null)
				{
					if (allParameters2.Count < 2)
					{
						uConsole.Log("[skillname, points]");
					}
					if (int.TryParse(allParameters2[1], out var result2))
					{
						IncreaseSkillPoints(allParameters2[0], result2);
					}
					else
					{
						uConsole.Log("points must be a number");
					}
				}
			}));
			uConsole.RegisterCommand("fskills_set_point", DebugCommand.op_Implicit((Action)delegate
			{
				List<string> allParameters = uConsole.GetAllParameters();
				if (allParameters != null)
				{
					if (allParameters.Count < 2)
					{
						uConsole.Log("[skillname, points]");
					}
					Skill val = null;
					if (skills.TryGetValue(allParameters[0], out var value))
					{
						val = GameManager.GetSkillsManager().m_Skills[customSkillOffset + value];
						if (int.TryParse(allParameters[1], out var result))
						{
							val.SetPoints(result, (PointAssignmentMode)1);
						}
						else
						{
							uConsole.Log("points must be a number");
						}
					}
					else
					{
						uConsole.Log("skill not found");
					}
				}
			}));
		}

		public static bool IsRegistered(string skillName)
		{
			string skillName2 = skillName;
			if (!skills.ContainsKey(skillName2))
			{
				return defs.Any((SkillDefinition d) => d.Name == skillName2);
			}
			return true;
		}

		public static (int points, int tier) GetPointsAndTiers(string skillName)
		{
			Skill val = GameManager.GetSkillsManager().m_Skills[customSkillOffset + skills[skillName]];
			return (points: val.GetPoints(), tier: val.GetCurrentTierNumber());
		}

		public static void IncreaseSkillPoints(string skillName, int points)
		{
			SkillsManager skillsManager = GameManager.GetSkillsManager();
			Skill val = skillsManager.m_Skills[customSkillOffset + skills[skillName]];
			int currentTierNumber = val.GetCurrentTierNumber();
			int points2 = val.GetPoints();
			val.IncrementPoints(points, (PointAssignmentMode)0);
			int currentTierNumber2 = val.GetCurrentTierNumber();
			if (currentTierNumber != currentTierNumber2)
			{
				GameManager.GetSkillNotify().MaybeShowLevelUp(val.m_SkillIcon, val.m_DisplayName, skillsManager.GetTierName(currentTierNumber2), currentTierNumber2 + 1);
			}
			else
			{
				GameManager.GetSkillNotify().MaybeShowPointIncrease(val.m_SkillIcon);
			}
			Instance loggerInstance = ((MelonBase)Instance).LoggerInstance;
			if (loggerInstance != null)
			{
				loggerInstance.Msg($"{skillName} | T{currentTierNumber} ({points2}+{points}) -> T{currentTierNumber2 + 1} ({val.GetPoints()})");
			}
		}

		public static void RegisterSkill(SkillDefinition def)
		{
			if (IsRegistered(def.Name))
			{
				Instance loggerInstance = ((MelonBase)Instance).LoggerInstance;
				if (loggerInstance != null)
				{
					loggerInstance.Error("Some mod is trying to regsiter a skill " + def.Name + " which the name is already used, it won't be registered, please report to the mod authors.");
				}
			}
			else
			{
				defs.Add(def);
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Log), "RefreshSelectedSkillDescriptionView")]
	public static class Panel_Log_Initialize
	{
		public static void Postfix(Panel_Log __instance)
		{
			SkillsManager skillsManager = GameManager.GetSkillsManager();
			for (int i = 0; i < skillsManager.m_Skills.Count; i++)
			{
				_ = skillsManager.m_Skills[i];
			}
			if (!((Object)(object)((UIWidget)__instance.m_SkillImageLarge).mainTexture == (Object)null))
			{
				return;
			}
			Skill skill = __instance.m_SkillsDisplayList[__instance.m_SkillListSelectedIndex].m_Skill;
			if (!((Object)(object)skill == (Object)null))
			{
				string key = ((Object)skill).name.Substring("Skill_".Length);
				Texture2D image = FiligranisSkills.defs[FiligranisSkills.skills[key]].Image;
				if ((Object)(object)image != (Object)null)
				{
					((UIWidget)__instance.m_SkillImageLarge).mainTexture = (Texture)(object)image;
				}
			}
		}
	}
	[HarmonyPatch(typeof(SkillsManager), "Awake")]
	public static class SkillsManagerInitialize
	{
		public static void Postfix(SkillsManager __instance)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			FiligranisSkills.skills.Clear();
			for (int i = 0; i < FiligranisSkills.defs.Count; i++)
			{
				SkillDefinition skillDefinition = FiligranisSkills.defs[i];
				Skill_IceFishing val = new GameObject
				{
					name = "Skill_" + skillDefinition.Name
				}.AddComponent<Skill_IceFishing>();
				((Skill)val).m_LocalizedDisplayName = skillDefinition.DisplayNameLocalized;
				((Skill)val).m_SkillIcon = skillDefinition.IconId;
				((Skill)val).m_SkillIconBackground = skillDefinition.IconBackgroundId;
				((Skill)val).m_SkillImage = skillDefinition.ImageId;
				((Skill)val).m_SkillType = (SkillType)(-1);
				((Skill)val).m_TierLocalizedDescriptions = Il2CppReferenceArray<LocalizedString>.op_Implicit(skillDefinition.TiersDescriptionLocalized);
				((Skill)val).m_TierLocalizedBenefits = Il2CppReferenceArray<LocalizedString>.op_Implicit(skillDefinition.TiersBenefitsLocalized);
				((Skill)val).m_TierPoints = Il2CppStructArray<int>.op_Implicit(skillDefinition.TierThresholds);
				FiligranisSkills.skills[skillDefinition.Name] = i;
				__instance.m_Skills.Add((Skill)(object)val);
			}
		}
	}
	[HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData")]
	internal static class RestoreGlobalData
	{
		[HarmonyPriority(700)]
		internal static void Postfix(string name)
		{
			SkillsManager skillsManager = GameManager.GetSkillsManager();
			for (int i = FiligranisSkills.customSkillOffset; i < skillsManager.m_Skills.Count; i++)
			{
				Skill val = skillsManager.m_Skills[i];
				string text = ((Object)val).name.Substring("Skill_".Length);
				string text2 = FiligranisSkills.Instance.ModSave.Load("skillpoint_" + text);
				if (text2 != null && int.TryParse(text2, out var result))
				{
					val.SetPoints(result, (PointAssignmentMode)1);
				}
			}
			for (int j = 0; j < skillsManager.m_Skills.Count; j++)
			{
				_ = skillsManager.m_Skills[j];
			}
		}
	}
	[HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData")]
	internal static class SaveGlobalData
	{
		[HarmonyPriority(700)]
		internal static void Postfix(SlotData slot)
		{
			SkillsManager skillsManager = GameManager.GetSkillsManager();
			for (int i = FiligranisSkills.customSkillOffset; i < skillsManager.m_Skills.Count; i++)
			{
				Skill val = skillsManager.m_Skills[i];
				string text = ((Object)val).name.Substring("Skill_".Length);
				Instance loggerInstance = ((MelonBase)FiligranisSkills.Instance).LoggerInstance;
				if (loggerInstance != null)
				{
					loggerInstance.Msg($"FSkill: Saving skill#{i} {text} {val.GetPoints()}p");
				}
				FiligranisSkills.Instance.ModSave.Save(val.GetPoints().ToString(), "skillpoint_" + text);
			}
		}
	}
	[HarmonyPatch(typeof(GameManager), "HandlePlayerDeath")]
	internal static class HandlePlayerDeath
	{
		internal static void Postfix()
		{
			SkillsManager skillsManager = GameManager.GetSkillsManager();
			for (int i = FiligranisSkills.customSkillOffset; i < skillsManager.m_Skills.Count; i++)
			{
				string text = ((Object)skillsManager.m_Skills[i]).name.Substring("Skill_".Length);
				FiligranisSkills.Instance.ModSave.Save("", "skillpoint_" + text);
			}
		}
	}
	public readonly record struct ReadOnlySkillHandle(Skill Skill)
	{
		public int Tier => Skill.GetCurrentTierNumber();

		public int Points => Skill.GetPoints();
	}
	public class SkillDefinition
	{
		public string Name { get; set; }

		public LocalizedString DisplayNameLocalized { get; set; }

		public string IconId { get; set; }

		public string IconBackgroundId { get; set; }

		public string ImageId { get; set; }

		public LocalizedString[] TiersDescriptionLocalized { get; set; }

		public LocalizedString[] TiersBenefitsLocalized { get; set; }

		public int[] TierThresholds { get; set; }

		public Texture2D? Image { get; set; }

		public SkillDefinition(string name = null, LocalizedString displayNameLocalized = null, string iconId = null, string iconBackgroundId = null, string imageId = null, LocalizedString[] tiersDescriptionLocalized = null, LocalizedString[] tiersBenefitsLocalized = null, int[] tierThresholds = null, Texture2D? image = null)
		{
			Name = name;
			DisplayNameLocalized = displayNameLocalized;
			IconId = iconId;
			IconBackgroundId = iconBackgroundId;
			ImageId = imageId;
			TiersDescriptionLocalized = tiersDescriptionLocalized;
			TiersBenefitsLocalized = tiersBenefitsLocalized;
			TierThresholds = tierThresholds;
			Image = image;
		}
	}
}
