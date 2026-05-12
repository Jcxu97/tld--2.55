using System;

namespace ModSettings;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class NameAttribute : Attribute
{
	private string name;

	private bool localize;

	public string Name => name;

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

	public NameAttribute(string name)
	{
		this.name = name;
	}
}
