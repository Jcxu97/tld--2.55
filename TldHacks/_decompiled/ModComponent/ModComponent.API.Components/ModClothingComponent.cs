using System;
using System.Reflection;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModClothingComponent : ModBaseComponent
{
	public enum BodyRegion
	{
		Head,
		Hands,
		Chest,
		Legs,
		Feet,
		Accessory
	}

	public enum MovementSounds
	{
		None,
		HeavyNylon,
		LeatherHide,
		LightCotton,
		LightNylon,
		SoftCloth,
		Wool
	}

	public BodyRegion Region;

	public ClothingLayer MinLayer;

	public ClothingLayer MaxLayer;

	public MovementSounds MovementSound;

	public float DaysToDecayWornOutside;

	public float DaysToDecayWornInside;

	public float Warmth;

	public float WarmthWhenWet;

	public float Windproof;

	public float Toughness;

	public float SprintBarReduction;

	public float Waterproofness;

	public int DecreaseAttackChance;

	public int IncreaseFleeChance;

	public float HoursToDryNearFire;

	public float HoursToDryWithoutFire;

	public float HoursToFreeze;

	public string MainTexture = "";

	public string BlendTexture = "";

	public int DrawLayer;

	public string ImplementationType = "";

	public object Implementation = "";

	public Action? OnPutOn;

	public Action? OnTakeOff;

	public string? FirstPersonPrefabMale;

	public string? FirstPersonPrefabFemale;

	public DamageReason PreventAllDamageFromSource;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModClothingComponent>(this);
		if (!string.IsNullOrEmpty(ImplementationType))
		{
			object obj = Activator.CreateInstance(TypeResolver.Resolve(ImplementationType, throwErrorOnFailure: true));
			if (obj != null)
			{
				Implementation = obj;
				OnPutOn = CreateImplementationActionDelegate("OnPutOn");
				OnTakeOff = CreateImplementationActionDelegate("OnTakeOff");
			}
		}
	}

	[HideFromIl2Cpp]
	private Action? CreateImplementationActionDelegate(string methodName)
	{
		MethodInfo method = Implementation.GetType().GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (!(method != null))
		{
			return null;
		}
		return (Action)Delegate.CreateDelegate(typeof(Action), Implementation, method);
	}

	public ModClothingComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModClothingComponent")
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		Region = entry.GetEnum<BodyRegion>("Region");
		MinLayer = entry.GetEnum<ClothingLayer>("MinLayer");
		MaxLayer = entry.GetEnum<ClothingLayer>("MaxLayer");
		MovementSound = entry.GetEnum<MovementSounds>("MovementSound");
		DaysToDecayWornOutside = entry.GetFloat("DaysToDecayWornOutside");
		DaysToDecayWornInside = entry.GetFloat("DaysToDecayWornInside");
		Warmth = entry.GetFloat("Warmth");
		WarmthWhenWet = entry.GetFloat("WarmthWhenWet");
		Windproof = entry.GetFloat("Windproof");
		Waterproofness = entry.GetFloat("Waterproofness");
		Toughness = entry.GetFloat("Toughness");
		SprintBarReduction = entry.GetFloat("SprintBarReduction");
		DecreaseAttackChance = entry.GetInt("DecreaseAttackChance");
		IncreaseFleeChance = entry.GetInt("IncreaseFleeChance");
		HoursToDryNearFire = entry.GetFloat("HoursToDryNearFire");
		HoursToDryWithoutFire = entry.GetFloat("HoursToDryWithoutFire");
		HoursToFreeze = entry.GetFloat("HoursToFreeze");
		MainTexture = entry.GetString("MainTexture");
		BlendTexture = entry.GetString("BlendTexture");
		DrawLayer = entry.GetInt("DrawLayer");
		ImplementationType = entry.GetString("ImplementationType");
		FirstPersonPrefabMale = entry.GetString("FirstPersonPrefabMale");
		FirstPersonPrefabFemale = entry.GetString("FirstPersonPrefabFemale");
		PreventAllDamageFromSource = entry.GetEnum<DamageReason>("PreventAllDamageFromSource");
	}
}
