using System;
using System.Reflection;

namespace ModSettings;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SliderAttribute : Attribute
{
	internal const string DefaultFloatFormat = "{0:F1}";

	internal const string DefaultIntFormat = "{0:D}";

	internal static readonly SliderAttribute DefaultFloatRange = new SliderAttribute(0f, 1f)
	{
		numberFormat = "{0:F2}"
	};

	internal static readonly SliderAttribute DefaultIntRange = new SliderAttribute(0f, 100f, 101)
	{
		numberFormat = "{0,3:D}%"
	};

	private readonly float from;

	private readonly float to;

	private readonly int numberOfSteps;

	private string? numberFormat;

	public float From => from;

	public float To => to;

	public int NumberOfSteps => numberOfSteps;

	public string? NumberFormat
	{
		get
		{
			return numberFormat;
		}
		set
		{
			numberFormat = value;
		}
	}

	public SliderAttribute(float from, float to)
		: this(from, to, -1)
	{
	}

	public SliderAttribute(float from, float to, int numberOfSteps)
	{
		this.from = from;
		this.to = to;
		this.numberOfSteps = numberOfSteps;
	}

	internal void ValidateFor(ModSettingsBase modSettings, FieldInfo field)
	{
		Type fieldType = field.FieldType;
		if (!AttributeFieldTypes.IsSliderType(fieldType))
		{
			throw new ArgumentException("[ModSettings] 'Slider' attribute doesn't support fields of type " + fieldType.Name, field.Name);
		}
		float num = Math.Max(from, to);
		float num2 = Math.Min(from, to);
		float num3 = Convert.ToSingle(field.GetValue(modSettings));
		if (num == num2)
		{
			throw new ArgumentException("[ModSettings] 'Slider' must have different 'From' and 'To' values", field.Name);
		}
		if (num3 < num2 || num3 > num)
		{
			throw new ArgumentException("[ModSettings] 'Slider' default value must be between 'From' and 'To'", field.Name);
		}
		if (AttributeFieldTypes.IsIntegerType(fieldType))
		{
			long num4 = (long)Math.Round(num2);
			long num5 = (long)Math.Round(num);
			if (num4 < AttributeFieldTypes.MinValue(fieldType))
			{
				throw new ArgumentException("[ModSettings] 'Slider' minimum value smaller than minimum value of " + fieldType.Name, field.Name);
			}
			if (num5 > AttributeFieldTypes.MaxValue(fieldType))
			{
				throw new ArgumentException("[ModSettings] 'Slider' maximum value larger than maximum value of " + fieldType.Name, field.Name);
			}
			if (numberOfSteps > num5 - num4 + 1)
			{
				throw new ArgumentException("[ModSettings] 'Slider' has too many steps to be able to support integer values", field.Name);
			}
		}
		if (string.IsNullOrEmpty(numberFormat))
		{
			return;
		}
		try
		{
			if (AttributeFieldTypes.IsFloatType(fieldType))
			{
				string.Format(numberFormat, 0f);
			}
			else
			{
				string.Format(numberFormat, 0);
			}
		}
		catch (FormatException innerException)
		{
			throw new ArgumentException("[ModSettings] Invalid 'Slider' number format", field.Name, innerException);
		}
	}
}
