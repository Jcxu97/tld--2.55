using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NLBIni;

public class IniParserConfiguration : ICloneable
{
	private char _sectionStartChar;

	private char _sectionEndChar;

	private string _commentString;

	protected const string _strCommentRegex = "^{0}(.*)";

	protected const string _strSectionRegexStart = "^(\\s*?)";

	protected const string _strSectionRegexMiddle = "{1}\\s*[\\p{L}\\p{P}\\p{M}_\\\"\\'\\{\\}\\#\\+\\;\\*\\%\\(\\)\\=\\?\\&\\$\\,\\:\\/\\.\\-\\w\\d\\s\\\\\\~]+\\s*";

	protected const string _strSectionRegexEnd = "(\\s*?)$";

	protected const string _strKeyRegex = "^(\\s*[_\\.\\d\\w]*\\s*)";

	protected const string _strValueRegex = "([\\s\\d\\w\\W\\.]*)$";

	protected const string _strSpecialRegexChars = "[]\\^$.|?*+()";

	public Regex CommentRegex { get; set; }

	public Regex SectionRegex { get; set; }

	public char SectionStartChar
	{
		get
		{
			return _sectionStartChar;
		}
		set
		{
			_sectionStartChar = value;
			RecreateSectionRegex(_sectionStartChar);
		}
	}

	public char SectionEndChar
	{
		get
		{
			return _sectionEndChar;
		}
		set
		{
			_sectionEndChar = value;
			RecreateSectionRegex(_sectionEndChar);
		}
	}

	public bool CaseInsensitive { get; set; }

	[Obsolete("Please use the CommentString property")]
	public char CommentChar
	{
		get
		{
			return CommentString[0];
		}
		set
		{
			CommentString = value.ToString();
		}
	}

	public string CommentString
	{
		get
		{
			return _commentString ?? string.Empty;
		}
		set
		{
			string text = "[]\\^$.|?*+()";
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				value = value.Replace(new string(c, 1), "\\" + c);
			}
			CommentRegex = new Regex($"^{value}(.*)");
			_commentString = value;
		}
	}

	public string NewLineStr { get; set; }

	public char KeyValueAssigmentChar { get; set; }

	public string AssigmentSpacer { get; set; }

	public bool AllowKeysWithoutSection { get; set; }

	public bool AllowDuplicateKeys { get; set; }

	public bool OverrideDuplicateKeys { get; set; }

	public bool ConcatenateDuplicateKeys { get; set; }

	public bool ThrowExceptionsOnError { get; set; }

	public bool AllowDuplicateSections { get; set; }

	public bool AllowCreateSectionsOnFly { get; set; }

	public bool SkipInvalidLines { get; set; }

	public IniParserConfiguration()
	{
		CommentString = ";";
		SectionStartChar = '[';
		SectionEndChar = ']';
		KeyValueAssigmentChar = '=';
		AssigmentSpacer = " ";
		NewLineStr = Environment.NewLine;
		ConcatenateDuplicateKeys = false;
		AllowKeysWithoutSection = true;
		AllowDuplicateKeys = true;
		AllowDuplicateSections = false;
		AllowCreateSectionsOnFly = true;
		ThrowExceptionsOnError = true;
		SkipInvalidLines = true;
	}

	public IniParserConfiguration(IniParserConfiguration ori)
	{
		AllowDuplicateKeys = ori.AllowDuplicateKeys;
		OverrideDuplicateKeys = ori.OverrideDuplicateKeys;
		AllowDuplicateSections = ori.AllowDuplicateSections;
		AllowKeysWithoutSection = ori.AllowKeysWithoutSection;
		AllowCreateSectionsOnFly = ori.AllowCreateSectionsOnFly;
		SectionStartChar = ori.SectionStartChar;
		SectionEndChar = ori.SectionEndChar;
		CommentString = ori.CommentString;
		ThrowExceptionsOnError = ori.ThrowExceptionsOnError;
	}

	private void RecreateSectionRegex(char value)
	{
		if (char.IsControl(value) || char.IsWhiteSpace(value) || CommentString.Contains(new string(new char[1] { value })) || value == KeyValueAssigmentChar)
		{
			throw new Exception($"Invalid character for section delimiter: '{value}");
		}
		string text = "^(\\s*?)";
		text = ((!"[]\\^$.|?*+()".Contains(new string(_sectionStartChar, 1))) ? (text + _sectionStartChar) : (text + "\\" + _sectionStartChar));
		text += "{1}\\s*[\\p{L}\\p{P}\\p{M}_\\\"\\'\\{\\}\\#\\+\\;\\*\\%\\(\\)\\=\\?\\&\\$\\,\\:\\/\\.\\-\\w\\d\\s\\\\\\~]+\\s*";
		text = ((!"[]\\^$.|?*+()".Contains(new string(_sectionEndChar, 1))) ? (text + _sectionEndChar) : (text + "\\" + _sectionEndChar));
		text += "(\\s*?)$";
		SectionRegex = new Regex(text);
	}

	public override int GetHashCode()
	{
		int num = 27;
		PropertyInfo[] properties = GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			num = num * 7 + propertyInfo.GetValue(this, null).GetHashCode();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IniParserConfiguration obj2))
		{
			return false;
		}
		Type type = GetType();
		try
		{
			PropertyInfo[] properties = type.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetValue(obj2, null).Equals(propertyInfo.GetValue(this, null)))
				{
					return false;
				}
			}
		}
		catch
		{
			return false;
		}
		return true;
	}

	public IniParserConfiguration Clone()
	{
		return MemberwiseClone() as IniParserConfiguration;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}
}
