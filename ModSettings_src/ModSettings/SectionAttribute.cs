using System;

namespace ModSettings;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SectionAttribute : Attribute
{
	private string title;

	private bool localize;

	public string Title => title;

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

	public SectionAttribute(string title)
	{
		this.title = title;
	}
}
