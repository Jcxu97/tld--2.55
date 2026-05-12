using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModFoodComponent : ModCookableComponent
{
	public int DaysToDecayOutdoors;

	public int DaysToDecayIndoors;

	public int Calories;

	public int EatingTime = 1;

	public string EatingAudio = "";

	public string EatingPackagedAudio = "";

	public int ThirstEffect;

	public int FoodPoisoning;

	public int FoodPoisoningLowCondition;

	public float[] ParasiteRiskIncrements = Array.Empty<float>();

	public bool Natural;

	public bool Raw;

	public bool Drink;

	public bool Meat;

	public bool Fish;

	public bool Canned;

	public bool Opening;

	public bool OpeningWithCanOpener;

	public bool OpeningWithKnife;

	public bool OpeningWithHatchet;

	public bool OpeningWithSmashing;

	public bool AffectCondition;

	public float ConditionRestBonus = 2f;

	public float ConditionRestMinutes = 360f;

	public bool AffectRest;

	public float InstantRestChange;

	public int RestFactorMinutes = 60;

	public bool AffectCold;

	public float InstantColdChange = 20f;

	public int ColdFactorMinutes = 60;

	public bool ContainsAlcohol;

	public float AlcoholPercentage;

	public float AlcoholUptakeMinutes = 45f;

	public int VitaminC;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModFoodComponent>(this);
	}

	public ModFoodComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModFoodComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		DaysToDecayOutdoors = entry.GetInt("DaysToDecayOutdoors");
		DaysToDecayIndoors = entry.GetInt("DaysToDecayIndoors");
		Calories = entry.GetInt("Calories");
		EatingTime = entry.GetInt("EatingTime");
		EatingAudio = entry.GetString("EatingAudio");
		EatingPackagedAudio = entry.GetString("EatingPackagedAudio");
		ThirstEffect = entry.GetInt("ThirstEffect");
		FoodPoisoning = entry.GetInt("FoodPoisoning");
		FoodPoisoningLowCondition = entry.GetInt("FoodPoisoningLowCondition");
		ParasiteRiskIncrements = entry.GetArray<float>("ParasiteRiskIncrements");
		Natural = entry.GetBool("Natural");
		Raw = entry.GetBool("Raw");
		Drink = entry.GetBool("Drink");
		Meat = entry.GetBool("Meat");
		Fish = entry.GetBool("Fish");
		Canned = entry.GetBool("Canned");
		Opening = entry.GetBool("Opening");
		OpeningWithCanOpener = entry.GetBool("OpeningWithCanOpener");
		OpeningWithKnife = entry.GetBool("OpeningWithKnife");
		OpeningWithHatchet = entry.GetBool("OpeningWithHatchet");
		OpeningWithSmashing = entry.GetBool("OpeningWithSmashing");
		AffectCondition = entry.GetBool("AffectCondition");
		ConditionRestBonus = entry.GetFloat("ConditionRestBonus");
		ConditionRestMinutes = entry.GetFloat("ConditionRestMinutes");
		AffectRest = entry.GetBool("AffectRest");
		InstantRestChange = entry.GetFloat("InstantRestChange");
		RestFactorMinutes = entry.GetInt("RestFactorMinutes");
		AffectCold = entry.GetBool("AffectCold");
		InstantColdChange = entry.GetFloat("InstantColdChange");
		ColdFactorMinutes = entry.GetInt("ColdFactorMinutes");
		ContainsAlcohol = entry.GetBool("ContainsAlcohol");
		AlcoholPercentage = entry.GetFloat("AlcoholPercentage");
		AlcoholUptakeMinutes = entry.GetFloat("AlcoholUptakeMinutes");
		VitaminC = entry.GetInt("VitaminC");
	}
}
