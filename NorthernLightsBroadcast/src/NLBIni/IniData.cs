using System;

namespace NLBIni;

public class IniData : ICloneable
{
	private SectionDataCollection _sections;

	private IniParserConfiguration _configuration;

	public IniParserConfiguration Configuration
	{
		get
		{
			if (_configuration == null)
			{
				_configuration = new IniParserConfiguration();
			}
			return _configuration;
		}
		set
		{
			_configuration = value.Clone();
		}
	}

	public KeyDataCollection Global { get; protected set; }

	public KeyDataCollection this[string sectionName]
	{
		get
		{
			if (!_sections.ContainsSection(sectionName))
			{
				if (!Configuration.AllowCreateSectionsOnFly)
				{
					return null;
				}
				_sections.AddSection(sectionName);
			}
			return _sections[sectionName];
		}
	}

	public SectionDataCollection Sections
	{
		get
		{
			return _sections;
		}
		set
		{
			_sections = value;
		}
	}

	public char SectionKeySeparator { get; set; }

	public IniData()
		: this(new SectionDataCollection())
	{
	}

	public IniData(SectionDataCollection sdc)
	{
		_sections = (SectionDataCollection)sdc.Clone();
		Global = new KeyDataCollection();
		SectionKeySeparator = '|';
	}

	public IniData(IniData ori)
		: this(ori.Sections)
	{
		Global = (KeyDataCollection)ori.Global.Clone();
		Configuration = ori.Configuration.Clone();
	}

	public override string ToString()
	{
		return ToString(new DefaultIniDataFormatter(Configuration));
	}

	public virtual string ToString(IIniDataFormatter formatter)
	{
		return formatter.IniDataToString(this);
	}

	public object Clone()
	{
		return new IniData(this);
	}

	public void ClearAllComments()
	{
		Global.ClearComments();
		foreach (SectionData section in Sections)
		{
			section.ClearComments();
		}
	}

	public void Merge(IniData toMergeIniData)
	{
		if (toMergeIniData != null)
		{
			Global.Merge(toMergeIniData.Global);
			Sections.Merge(toMergeIniData.Sections);
		}
	}

	public bool TryGetKey(string key, out string value)
	{
		value = string.Empty;
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		string[] array = key.Split(SectionKeySeparator);
		int num = array.Length - 1;
		if (num > 1)
		{
			throw new ArgumentException("key contains multiple separators", "key");
		}
		if (num == 0)
		{
			if (!Global.ContainsKey(key))
			{
				return false;
			}
			value = Global[key];
			return true;
		}
		string text = array[0];
		key = array[1];
		if (!_sections.ContainsSection(text))
		{
			return false;
		}
		KeyDataCollection keyDataCollection = _sections[text];
		if (!keyDataCollection.ContainsKey(key))
		{
			return false;
		}
		value = keyDataCollection[key];
		return true;
	}

	public string GetKey(string key)
	{
		if (!TryGetKey(key, out string value))
		{
			return null;
		}
		return value;
	}

	private void MergeSection(SectionData otherSection)
	{
		if (!Sections.ContainsSection(otherSection.SectionName))
		{
			Sections.AddSection(otherSection.SectionName);
		}
		Sections.GetSectionData(otherSection.SectionName).Merge(otherSection);
	}

	private void MergeGlobal(KeyDataCollection globals)
	{
		foreach (KeyData global in globals)
		{
			Global[global.KeyName] = global.Value;
		}
	}
}
