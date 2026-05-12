using System;
using System.Reflection;
using UnityEngine;

namespace ModSettings;

internal static class Attributes
{
	internal static void GetAttributes(FieldInfo field, out SectionAttribute section, out NameAttribute name, out DescriptionAttribute description, out SliderAttribute slider, out ChoiceAttribute choice)
	{
		section = null;
		name = null;
		description = null;
		slider = null;
		choice = null;
		object[] customAttributes = field.GetCustomAttributes(inherit: true);
		foreach (object obj in customAttributes)
		{
			if (obj is SectionAttribute sectionAttribute)
			{
				section = sectionAttribute;
			}
			else if (obj is NameAttribute nameAttribute)
			{
				name = nameAttribute;
			}
			else if (obj is DescriptionAttribute descriptionAttribute)
			{
				description = descriptionAttribute;
			}
			else if (obj is SliderAttribute sliderAttribute)
			{
				slider = sliderAttribute;
			}
			else if (obj is ChoiceAttribute choiceAttribute)
			{
				choice = choiceAttribute;
			}
		}
	}

	internal static void ValidateFields(ModSettingsBase modSettings)
	{
		FieldInfo[] fields = modSettings.GetFields();
		foreach (FieldInfo field in fields)
		{
			ValidateFieldAttributes(modSettings, field);
		}
	}

	private static void ValidateFieldAttributes(ModSettingsBase modSettings, FieldInfo field)
	{
		GetAttributes(field, out SectionAttribute section, out NameAttribute name, out DescriptionAttribute _, out SliderAttribute slider, out ChoiceAttribute choice);
		Type fieldType = field.FieldType;
		if (name == null)
		{
			throw new ArgumentException("[ModSettings] Mod settings contain field without a name attribute", field.Name);
		}
		if (string.IsNullOrEmpty(name.Name))
		{
			throw new ArgumentException("[ModSettings] Setting name attribute must have a non-empty value", field.Name);
		}
		if (section != null && string.IsNullOrEmpty(section.Title))
		{
			throw new ArgumentException("[ModSettings] Section title attribute must have a non-empty value", field.Name);
		}
		if (slider != null && choice != null)
		{
			throw new ArgumentException("[ModSettings] Field cannot be annotated with both 'Slider' and 'Choice' attributes", field.Name);
		}
		if (slider != null)
		{
			slider.ValidateFor(modSettings, field);
		}
		else if (choice != null)
		{
			choice.ValidateFor(modSettings, field);
		}
		else if (!AttributeFieldTypes.IsSupportedType(fieldType))
		{
			throw new ArgumentException("[ModSettings] Field type " + fieldType.Name + " is not supported", field.Name);
		}
		if (fieldType.IsEnum && fieldType != typeof(KeyCode))
		{
			ValidateEnum(field, fieldType);
		}
	}

	private static void ValidateEnum(FieldInfo field, Type enumType)
	{
		int length = Enum.GetValues(enumType).Length;
		Type underlyingType = Enum.GetUnderlyingType(enumType);
		for (int i = 0; i < length; i++)
		{
			object value = Convert.ChangeType(i, underlyingType);
			if (!Enum.IsDefined(enumType, value))
			{
				throw new ArgumentException("[ModSettings] Enum fields must have consecutive values starting at 0", field.Name);
			}
		}
	}
}
