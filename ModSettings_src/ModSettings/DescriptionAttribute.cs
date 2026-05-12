using System;

namespace ModSettings;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class DescriptionAttribute : Attribute
{
	private string description;

	private bool localize;

	public string Description => description;

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

	public DescriptionAttribute(string description)
	{
		this.description = description;
	}
}
