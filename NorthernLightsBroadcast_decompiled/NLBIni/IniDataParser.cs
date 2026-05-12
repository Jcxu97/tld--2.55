using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NLBIni;

public class IniDataParser
{
	private List<Exception> _errorExceptions;

	private readonly List<string> _currentCommentListTemp = new List<string>();

	private string _currentSectionNameTemp;

	public virtual IniParserConfiguration Configuration { get; protected set; }

	public bool HasError => _errorExceptions.Count > 0;

	public ReadOnlyCollection<Exception> Errors => _errorExceptions.AsReadOnly();

	public IniDataParser()
		: this(new IniParserConfiguration())
	{
	}

	public IniDataParser(IniParserConfiguration parserConfiguration)
	{
		if (parserConfiguration == null)
		{
			throw new ArgumentNullException("parserConfiguration");
		}
		Configuration = parserConfiguration;
		_errorExceptions = new List<Exception>();
	}

	public IniData Parse(string iniDataString)
	{
		IniData iniData = (Configuration.CaseInsensitive ? new IniDataCaseInsensitive() : new IniData());
		iniData.Configuration = Configuration.Clone();
		if (string.IsNullOrEmpty(iniDataString))
		{
			return iniData;
		}
		_errorExceptions.Clear();
		_currentCommentListTemp.Clear();
		_currentSectionNameTemp = null;
		try
		{
			string[] array = iniDataString.Split(new string[2] { "\n", "\r\n" }, StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (text.Trim() == string.Empty)
				{
					continue;
				}
				try
				{
					ProcessLine(text, iniData);
				}
				catch (Exception ex)
				{
					ParsingException ex2 = new ParsingException(ex.Message, i + 1, text, ex);
					if (Configuration.ThrowExceptionsOnError)
					{
						throw ex2;
					}
					_errorExceptions.Add(ex2);
				}
			}
			if (_currentCommentListTemp.Count > 0)
			{
				if (iniData.Sections.Count > 0)
				{
					iniData.Sections.GetSectionData(_currentSectionNameTemp).TrailingComments.AddRange(_currentCommentListTemp);
				}
				else if (iniData.Global.Count > 0)
				{
					iniData.Global.GetLast().Comments.AddRange(_currentCommentListTemp);
				}
				_currentCommentListTemp.Clear();
			}
		}
		catch (Exception item)
		{
			_errorExceptions.Add(item);
			if (Configuration.ThrowExceptionsOnError)
			{
				throw;
			}
		}
		if (HasError)
		{
			return null;
		}
		return (IniData)iniData.Clone();
	}

	protected virtual bool LineContainsAComment(string line)
	{
		if (!string.IsNullOrEmpty(line))
		{
			return Configuration.CommentRegex.Match(line).Success;
		}
		return false;
	}

	protected virtual bool LineMatchesASection(string line)
	{
		if (!string.IsNullOrEmpty(line))
		{
			return Configuration.SectionRegex.Match(line).Success;
		}
		return false;
	}

	protected virtual bool LineMatchesAKeyValuePair(string line)
	{
		if (!string.IsNullOrEmpty(line))
		{
			return line.Contains(Configuration.KeyValueAssigmentChar.ToString());
		}
		return false;
	}

	protected virtual string ExtractComment(string line)
	{
		string text = Configuration.CommentRegex.Match(line).Value.Trim();
		_currentCommentListTemp.Add(text.Substring(1, text.Length - 1));
		return line.Replace(text, "").Trim();
	}

	protected virtual void ProcessLine(string currentLine, IniData currentIniData)
	{
		currentLine = currentLine.Trim();
		if (LineContainsAComment(currentLine))
		{
			currentLine = ExtractComment(currentLine);
		}
		if (!(currentLine == string.Empty))
		{
			if (LineMatchesASection(currentLine))
			{
				ProcessSection(currentLine, currentIniData);
			}
			else if (LineMatchesAKeyValuePair(currentLine))
			{
				ProcessKeyValuePair(currentLine, currentIniData);
			}
			else if (!Configuration.SkipInvalidLines)
			{
				throw new ParsingException("Unknown file format. Couldn't parse the line: '" + currentLine + "'.");
			}
		}
	}

	protected virtual void ProcessSection(string line, IniData currentIniData)
	{
		string text = Configuration.SectionRegex.Match(line).Value.Trim();
		text = text.Substring(1, text.Length - 2).Trim();
		if (text == string.Empty)
		{
			throw new ParsingException("Section name is empty");
		}
		_currentSectionNameTemp = text;
		if (currentIniData.Sections.ContainsSection(text))
		{
			if (!Configuration.AllowDuplicateSections)
			{
				throw new ParsingException($"Duplicate section with name '{text}' on line '{line}'");
			}
		}
		else
		{
			currentIniData.Sections.AddSection(text);
			currentIniData.Sections.GetSectionData(text).LeadingComments = _currentCommentListTemp;
			_currentCommentListTemp.Clear();
		}
	}

	protected virtual void ProcessKeyValuePair(string line, IniData currentIniData)
	{
		string text = ExtractKey(line);
		if (string.IsNullOrEmpty(text) && Configuration.SkipInvalidLines)
		{
			return;
		}
		string value = ExtractValue(line);
		if (string.IsNullOrEmpty(_currentSectionNameTemp))
		{
			if (!Configuration.AllowKeysWithoutSection)
			{
				throw new ParsingException("key value pairs must be enclosed in a section");
			}
			AddKeyToKeyValueCollection(text, value, currentIniData.Global, "global");
		}
		else
		{
			SectionData sectionData = currentIniData.Sections.GetSectionData(_currentSectionNameTemp);
			AddKeyToKeyValueCollection(text, value, sectionData.Keys, _currentSectionNameTemp);
		}
	}

	protected virtual string ExtractKey(string s)
	{
		return s[..s.IndexOf(Configuration.KeyValueAssigmentChar, 0)].Trim();
	}

	protected virtual string ExtractValue(string s)
	{
		int num = s.IndexOf(Configuration.KeyValueAssigmentChar, 0);
		return s.Substring(num + 1, s.Length - num - 1).Trim();
	}

	protected virtual void HandleDuplicatedKeyInCollection(string key, string value, KeyDataCollection keyDataCollection, string sectionName)
	{
		if (!Configuration.AllowDuplicateKeys)
		{
			throw new ParsingException($"Duplicated key '{key}' found in section '{sectionName}");
		}
		if (Configuration.OverrideDuplicateKeys)
		{
			keyDataCollection[key] = value;
		}
	}

	private void AddKeyToKeyValueCollection(string key, string value, KeyDataCollection keyDataCollection, string sectionName)
	{
		if (keyDataCollection.ContainsKey(key))
		{
			HandleDuplicatedKeyInCollection(key, value, keyDataCollection, sectionName);
		}
		else
		{
			keyDataCollection.AddKey(key, value);
		}
		keyDataCollection.GetKeyData(key).Comments = _currentCommentListTemp;
		_currentCommentListTemp.Clear();
	}
}
