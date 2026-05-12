using System;

namespace NLBIni;

[Obsolete("Use class IniDataParser instead. See remarks comments in this class.")]
public class StringIniParser
{
	public IniDataParser Parser { get; protected set; }

	public StringIniParser()
		: this(new IniDataParser())
	{
	}

	public StringIniParser(IniDataParser parser)
	{
		Parser = parser;
	}

	public IniData ParseString(string dataStr)
	{
		return Parser.Parse(dataStr);
	}

	public string WriteString(IniData iniData)
	{
		return iniData.ToString();
	}
}
