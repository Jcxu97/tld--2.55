using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModMillableBehaviour : MonoBehaviour
{
	public bool CanRestoreFromWornOut;

	public int RecoveryDurationMinutes = 1;

	public string[] RestoreRequiredGear = Array.Empty<string>();

	public int[] RestoreRequiredGearUnits = Array.Empty<int>();

	public int RepairDurationMinutes = 1;

	public string[] RepairRequiredGear = Array.Empty<string>();

	public int[] RepairRequiredGearUnits = Array.Empty<int>();

	public SkillType Skill = (SkillType)(-1);

	public ModMillableBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}//IL_003c: Unknown result type (might be due to invalid IL or missing references)


	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModMillableBehaviour")
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		JsonDictEntry entry = jsonDict.GetEntry(className);
		RepairDurationMinutes = entry.GetInt("RepairDurationMinutes");
		RepairRequiredGear = entry.GetArray<string>("RepairRequiredGear");
		RepairRequiredGearUnits = entry.GetArray<int>("RepairRequiredGearUnits");
		CanRestoreFromWornOut = entry.GetBool("CanRestoreFromWornOut");
		RecoveryDurationMinutes = entry.GetInt("RecoveryDurationMinutes");
		RestoreRequiredGear = entry.GetArray<string>("RestoreRequiredGear");
		RestoreRequiredGearUnits = entry.GetArray<int>("RestoreRequiredGearUnits");
		Skill = entry.GetEnum<SkillType>("Skill");
	}
}
