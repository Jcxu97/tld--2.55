using System;
using System.Collections.Generic;
using System.Text;

namespace NLBIni;

public class DefaultIniDataFormatter : IIniDataFormatter
{
	private IniParserConfiguration _configuration;

	public IniParserConfiguration Configuration
	{
		get
		{
			return _configuration;
		}
		set
		{
			_configuration = value.Clone();
		}
	}

	public DefaultIniDataFormatter()
		: this(new IniParserConfiguration())
	{
	}

	public DefaultIniDataFormatter(IniParserConfiguration configuration)
	{
		if (configuration == null)
		{
			throw new ArgumentNullException("configuration");
		}
		Configuration = configuration;
	}

	public virtual string IniDataToString(IniData iniData)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Configuration.AllowKeysWithoutSection)
		{
			WriteKeyValueData(iniData.Global, stringBuilder);
		}
		foreach (SectionData section in iniData.Sections)
		{
			WriteSection(section, stringBuilder);
		}
		return stringBuilder.ToString();
	}

	private void WriteSection(SectionData section, StringBuilder sb)
	{
		if (sb.Length > 0)
		{
			sb.Append(Configuration.NewLineStr);
		}
		WriteComments(section.LeadingComments, sb);
		sb.Append($"{Configuration.SectionStartChar}{section.SectionName}{Configuration.SectionEndChar}{Configuration.NewLineStr}");
		WriteKeyValueData(section.Keys, sb);
		WriteComments(section.TrailingComments, sb);
	}

	private void WriteKeyValueData(KeyDataCollection keyDataCollection, StringBuilder sb)
	{
		foreach (KeyData item in keyDataCollection)
		{
			if (item.Comments.Count > 0)
			{
				sb.Append(Configuration.NewLineStr);
			}
			WriteComments(item.Comments, sb);
			sb.Append(string.Format("{0}{3}{1}{3}{2}{4}", item.KeyName, Configuration.KeyValueAssigmentChar, item.Value, Configuration.AssigmentSpacer, Configuration.NewLineStr));
		}
	}

	private void WriteComments(List<string> comments, StringBuilder sb)
	{
		foreach (string comment in comments)
		{
			sb.Append($"{Configuration.CommentString}{comment}{Configuration.NewLineStr}");
		}
	}
}
