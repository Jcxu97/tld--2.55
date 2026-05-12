using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppTLD.Gameplay.Tunable;

namespace ModSettings;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ChoiceAttribute : Attribute
{
	private static readonly Dictionary<Type, ChoiceAttribute> predefinedHinterlandEnums = new Dictionary<Type, ChoiceAttribute>
	{
		{
			typeof(CustomTunableDayNightMultiplier),
			Localized("GAMEPLAY_OneX", "GAMEPLAY_TwoX", "GAMEPLAY_ThreeX", "GAMEPLAY_FourX")
		},
		{
			typeof(CustomTunableDistance),
			Localized("GAMEPLAY_DistanceClose", "GAMEPLAY_DistanceMedium", "GAMEPLAY_DistanceFar")
		},
		{
			typeof(CustomTunableLMH),
			Localized("GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High")
		},
		{
			typeof(CustomTunableLMHV),
			Localized("GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High", "GAMEPLAY_VeryHigh")
		},
		{
			typeof(CustomTunableNLH),
			Localized("GAMEPLAY_None", "GAMEPLAY_Low", "GAMEPLAY_High")
		},
		{
			typeof(CustomTunableNLMH),
			Localized("GAMEPLAY_None", "GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High")
		},
		{
			typeof(CustomTunableNLMHV),
			Localized("GAMEPLAY_None", "GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High", "GAMEPLAY_VeryHigh")
		},
		{
			typeof(CustomTunableTimeOfDay),
			Localized("GAMEPLAY_Dawn", "GAMEPLAY_Noon", "GAMEPLAY_Dusk", "GAMEPLAY_Midnight", "GAMEPLAY_Random")
		},
		{
			typeof(CustomTunableWeather),
			Localized("GAMEPLAY_WeatherClear", "GAMEPLAY_WeatherLightSnow", "GAMEPLAY_WeatherHeavySnow", "GAMEPLAY_WeatherBlizzard", "GAMEPLAY_WeatherLightFog", "GAMEPLAY_WeatherHeavyFog", "GAMEPLAY_Random")
		}
	};

	internal static readonly ChoiceAttribute YesNoAttribute = Localized("GAMEPLAY_No", "GAMEPLAY_Yes");

	private readonly string[] names;

	private bool localize;

	public string[] Names => names;

	public bool Localize
	{
		get
		{
			return localize;
		}
		set
		{
			localize = value;
		}
	}

	private static ChoiceAttribute Localized(params string[] names)
	{
		return new ChoiceAttribute(names)
		{
			Localize = true
		};
	}

	internal static ChoiceAttribute ForEnumType(Type enumType)
	{
		if (predefinedHinterlandEnums.TryGetValue(enumType, out ChoiceAttribute value))
		{
			return value;
		}
		string[] array = Enum.GetNames(enumType);
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = PrettifyEnumName(array[i]);
		}
		return new ChoiceAttribute(array2);
	}

	private static string PrettifyEnumName(string enumName)
	{
		string text = enumName.Replace('_', ' ');
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (flag && char.IsUpper(c))
			{
				text = text.Insert(i, " ");
			}
			flag = char.IsLower(c);
		}
		return text;
	}

	public ChoiceAttribute(params string[] names)
	{
		this.names = names;
	}

	internal void ValidateFor(ModSettingsBase modSettings, FieldInfo field)
	{
		Type type = field.FieldType;
		Type type2 = null;
		if (type.IsEnum)
		{
			type2 = type;
			type = Enum.GetUnderlyingType(type);
		}
		if (!AttributeFieldTypes.IsChoiceType(type))
		{
			throw new ArgumentException("[ModSettings] 'Choice' attribute doesn't support fields of type " + type.Name, field.Name);
		}
		long num = AttributeFieldTypes.MaxValue(type);
		if (names == null || names.Length == 0)
		{
			throw new ArgumentException("[ModSettings] 'Choice' attribute must contain non-empty array of non-empty strings", field.Name);
		}
		if (names.Length == 1)
		{
			throw new ArgumentException("[ModSettings] 'Choice' attribute must contain array of at least two elements", field.Name);
		}
		if (names.Length - 1 > num)
		{
			throw new ArgumentException("[ModSettings] 'Choice' attribute contains more elements than " + type.Name + " can represent: " + names.Length + " > " + (num + 1), field.Name);
		}
		if (type2 != null && names.Length != Enum.GetValues(type2).Length)
		{
			throw new ArgumentException("[ModSettings] 'Choice' attribute array length doesn't match " + type2.Name + " enum value count: " + names.Length + " != " + Enum.GetValues(type2).Length, field.Name);
		}
		string[] array = names;
		for (int i = 0; i < array.Length; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				throw new ArgumentException("[ModSettings] 'Choice' attribute must contain non-empty array of non-empty strings", field.Name);
			}
		}
		int num2 = Convert.ToInt32(field.GetValue(modSettings));
		if (num2 < 0 || num2 >= names.Length)
		{
			throw new ArgumentException("[ModSettings] Default value out of range for 'Choice' attribute", field.Name);
		}
	}
}
