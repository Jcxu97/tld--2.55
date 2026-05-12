using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using AudioMgr;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTLD.PDID;
using Il2CppTMPro;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModData;
using ModSettings;
using NLBIni;
using NorthernLightsBroadcast;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("NorthernLightsBroadcast")]
[assembly: AssemblyCopyright("Digitalzombie")]
[assembly: AssemblyFileVersion("2.0.6.1")]
[assembly: MelonInfo(typeof(NorthernLightsBroadcastMain), "NorthernLightsBroadcast", "2.0.6.1", "Digitalzombie", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("2.0.6.1")]
[module: UnverifiableCode]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace NLBIni
{
	internal static class Assert
	{
		internal static bool StringHasNoBlankSpaces(string s)
		{
			return !s.Contains(" ");
		}
	}
	public class ConcatenateDuplicatedKeysIniDataParser : IniDataParser
	{
		public new ConcatenateDuplicatedKeysIniParserConfiguration Configuration
		{
			get
			{
				return (ConcatenateDuplicatedKeysIniParserConfiguration)base.Configuration;
			}
			set
			{
				base.Configuration = value;
			}
		}

		public ConcatenateDuplicatedKeysIniDataParser()
			: this(new ConcatenateDuplicatedKeysIniParserConfiguration())
		{
		}

		public ConcatenateDuplicatedKeysIniDataParser(ConcatenateDuplicatedKeysIniParserConfiguration parserConfiguration)
			: base(parserConfiguration)
		{
		}

		protected override void HandleDuplicatedKeyInCollection(string key, string value, KeyDataCollection keyDataCollection, string sectionName)
		{
			keyDataCollection[key] = keyDataCollection[key] + Configuration.ConcatenateSeparator + value;
		}
	}
	public class ConcatenateDuplicatedKeysIniParserConfiguration : IniParserConfiguration
	{
		public new bool AllowDuplicateKeys => true;

		public string ConcatenateSeparator { get; set; }

		public ConcatenateDuplicatedKeysIniParserConfiguration()
		{
			ConcatenateSeparator = ";";
		}

		public ConcatenateDuplicatedKeysIniParserConfiguration(ConcatenateDuplicatedKeysIniParserConfiguration ori)
			: base(ori)
		{
			ConcatenateSeparator = ori.ConcatenateSeparator;
		}
	}
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
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 4, sb);
			handler.AppendFormatted(Configuration.SectionStartChar);
			handler.AppendFormatted(section.SectionName);
			handler.AppendFormatted(Configuration.SectionEndChar);
			handler.AppendFormatted(Configuration.NewLineStr);
			sb.Append(ref handler);
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
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 3, sb);
				handler.AppendFormatted(Configuration.CommentString);
				handler.AppendFormatted(comment);
				handler.AppendFormatted(Configuration.NewLineStr);
				sb.Append(ref handler);
			}
		}
	}
	[Obsolete("Kept for backward compatibility, just use IniParserConfiguration class")]
	public class DefaultIniParserConfiguration : ConcatenateDuplicatedKeysIniParserConfiguration
	{
	}
	public class FileIniDataParser : StreamIniDataParser
	{
		public FileIniDataParser()
		{
		}

		public FileIniDataParser(IniDataParser parser)
			: base(parser)
		{
			base.Parser = parser;
		}

		[Obsolete("Please use ReadFile method instead of this one as is more semantically accurate")]
		public IniData LoadFile(string filePath)
		{
			return ReadFile(filePath);
		}

		[Obsolete("Please use ReadFile method instead of this one as is more semantically accurate")]
		public IniData LoadFile(string filePath, Encoding fileEncoding)
		{
			return ReadFile(filePath, fileEncoding);
		}

		public IniData ReadFile(string filePath)
		{
			return ReadFile(filePath, Encoding.ASCII);
		}

		public IniData ReadFile(string filePath, Encoding fileEncoding)
		{
			if (filePath == string.Empty)
			{
				throw new ArgumentException("Bad filename.");
			}
			try
			{
				using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using StreamReader reader = new StreamReader(stream, fileEncoding);
				return ReadData(reader);
			}
			catch (IOException innerException)
			{
				throw new ParsingException("Could not parse file " + filePath, innerException);
			}
		}

		[Obsolete("Please use WriteFile method instead of this one as is more semantically accurate")]
		public void SaveFile(string filePath, IniData parsedData)
		{
			WriteFile(filePath, parsedData, Encoding.UTF8);
		}

		public void WriteFile(string filePath, IniData parsedData, Encoding fileEncoding = null)
		{
			if (fileEncoding == null)
			{
				fileEncoding = Encoding.UTF8;
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentException("Bad filename.");
			}
			if (parsedData == null)
			{
				throw new ArgumentNullException("parsedData");
			}
			using FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
			using StreamWriter writer = new StreamWriter(stream, fileEncoding);
			WriteData(writer, parsedData);
		}
	}
	public interface IIniDataFormatter
	{
		IniParserConfiguration Configuration { get; set; }

		string IniDataToString(IniData iniData);
	}
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
			if (!TryGetKey(key, out var value))
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
	public class IniDataCaseInsensitive : IniData
	{
		public IniDataCaseInsensitive()
			: base(new SectionDataCollection(StringComparer.OrdinalIgnoreCase))
		{
			base.Global = new KeyDataCollection(StringComparer.OrdinalIgnoreCase);
		}

		public IniDataCaseInsensitive(SectionDataCollection sdc)
			: base(new SectionDataCollection(sdc, StringComparer.OrdinalIgnoreCase))
		{
			base.Global = new KeyDataCollection(StringComparer.OrdinalIgnoreCase);
		}

		public IniDataCaseInsensitive(IniData ori)
			: this(new SectionDataCollection(ori.Sections, StringComparer.OrdinalIgnoreCase))
		{
			base.Global = (KeyDataCollection)ori.Global.Clone();
			base.Configuration = ori.Configuration.Clone();
		}
	}
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
			return s.Substring(0, s.IndexOf(Configuration.KeyValueAssigmentChar, 0)).Trim();
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
				throw new ParsingException("Duplicated key '" + key + "' found in section '" + sectionName);
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
				CommentRegex = new Regex("^" + value + "(.*)");
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
	public class KeyData : ICloneable
	{
		private List<string> _comments;

		private string _value;

		private string _keyName;

		public List<string> Comments
		{
			get
			{
				return _comments;
			}
			set
			{
				_comments = new List<string>(value);
			}
		}

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public string KeyName
		{
			get
			{
				return _keyName;
			}
			set
			{
				if (value != string.Empty)
				{
					_keyName = value;
				}
			}
		}

		public KeyData(string keyName)
		{
			if (string.IsNullOrEmpty(keyName))
			{
				throw new ArgumentException("key name can not be empty");
			}
			_comments = new List<string>();
			_value = string.Empty;
			_keyName = keyName;
		}

		public KeyData(KeyData ori)
		{
			_value = ori._value;
			_keyName = ori._keyName;
			_comments = new List<string>(ori._comments);
		}

		public object Clone()
		{
			return new KeyData(this);
		}
	}
	public class KeyDataCollection : ICloneable, IEnumerable<KeyData>, IEnumerable
	{
		private IEqualityComparer<string> _searchComparer;

		private readonly Dictionary<string, KeyData> _keyData;

		public string this[string keyName]
		{
			get
			{
				if (_keyData.ContainsKey(keyName))
				{
					return _keyData[keyName].Value;
				}
				return null;
			}
			set
			{
				if (!_keyData.ContainsKey(keyName))
				{
					AddKey(keyName);
				}
				_keyData[keyName].Value = value;
			}
		}

		public int Count => _keyData.Count;

		public KeyDataCollection()
			: this(EqualityComparer<string>.Default)
		{
		}

		public KeyDataCollection(IEqualityComparer<string> searchComparer)
		{
			_searchComparer = searchComparer;
			_keyData = new Dictionary<string, KeyData>(_searchComparer);
		}

		public KeyDataCollection(KeyDataCollection ori, IEqualityComparer<string> searchComparer)
			: this(searchComparer)
		{
			foreach (KeyData item in ori)
			{
				if (_keyData.ContainsKey(item.KeyName))
				{
					_keyData[item.KeyName] = (KeyData)item.Clone();
				}
				else
				{
					_keyData.Add(item.KeyName, (KeyData)item.Clone());
				}
			}
		}

		public bool AddKey(string keyName)
		{
			if (!_keyData.ContainsKey(keyName))
			{
				_keyData.Add(keyName, new KeyData(keyName));
				return true;
			}
			return false;
		}

		[Obsolete("Pottentially buggy method! Use AddKey(KeyData keyData) instead (See comments in code for an explanation of the bug)")]
		public bool AddKey(string keyName, KeyData keyData)
		{
			if (AddKey(keyName))
			{
				_keyData[keyName] = keyData;
				return true;
			}
			return false;
		}

		public bool AddKey(KeyData keyData)
		{
			if (AddKey(keyData.KeyName))
			{
				_keyData[keyData.KeyName] = keyData;
				return true;
			}
			return false;
		}

		public bool AddKey(string keyName, string keyValue)
		{
			if (AddKey(keyName))
			{
				_keyData[keyName].Value = keyValue;
				return true;
			}
			return false;
		}

		public void ClearComments()
		{
			using IEnumerator<KeyData> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Comments.Clear();
			}
		}

		public bool ContainsKey(string keyName)
		{
			return _keyData.ContainsKey(keyName);
		}

		public KeyData GetKeyData(string keyName)
		{
			if (_keyData.ContainsKey(keyName))
			{
				return _keyData[keyName];
			}
			return null;
		}

		public void Merge(KeyDataCollection keyDataToMerge)
		{
			foreach (KeyData item in keyDataToMerge)
			{
				AddKey(item.KeyName);
				GetKeyData(item.KeyName).Comments.AddRange(item.Comments);
				this[item.KeyName] = item.Value;
			}
		}

		public void RemoveAllKeys()
		{
			_keyData.Clear();
		}

		public bool RemoveKey(string keyName)
		{
			return _keyData.Remove(keyName);
		}

		public void SetKeyData(KeyData data)
		{
			if (data != null)
			{
				if (_keyData.ContainsKey(data.KeyName))
				{
					RemoveKey(data.KeyName);
				}
				AddKey(data);
			}
		}

		public IEnumerator<KeyData> GetEnumerator()
		{
			foreach (string key in _keyData.Keys)
			{
				yield return _keyData[key];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _keyData.GetEnumerator();
		}

		public object Clone()
		{
			return new KeyDataCollection(this, _searchComparer);
		}

		internal KeyData GetLast()
		{
			KeyData result = null;
			if (_keyData.Keys.Count <= 0)
			{
				return result;
			}
			foreach (string key in _keyData.Keys)
			{
				result = _keyData[key];
			}
			return result;
		}
	}
	public class ParsingException : Exception
	{
		public Version LibVersion { get; private set; }

		public int LineNumber { get; private set; }

		public string LineValue { get; private set; }

		public ParsingException(string msg)
			: this(msg, 0, string.Empty, null)
		{
		}

		public ParsingException(string msg, Exception innerException)
			: this(msg, 0, string.Empty, innerException)
		{
		}

		public ParsingException(string msg, int lineNumber, string lineValue)
			: this(msg, lineNumber, lineValue, null)
		{
		}

		public ParsingException(string msg, int lineNumber, string lineValue, Exception innerException)
			: base($"{msg} while parsing line number {lineNumber} with value '{lineValue}' - IniParser version: {Assembly.GetExecutingAssembly().GetName().Version}", innerException)
		{
			LibVersion = Assembly.GetExecutingAssembly().GetName().Version;
			LineNumber = lineNumber;
			LineValue = lineValue;
		}
	}
	public class SectionData : ICloneable
	{
		private IEqualityComparer<string> _searchComparer;

		private List<string> _leadingComments;

		private List<string> _trailingComments = new List<string>();

		private KeyDataCollection _keyDataCollection;

		private string _sectionName;

		public string SectionName
		{
			get
			{
				return _sectionName;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					_sectionName = value;
				}
			}
		}

		[Obsolete("Do not use this property, use property Comments instead")]
		public List<string> LeadingComments
		{
			get
			{
				return _leadingComments;
			}
			internal set
			{
				_leadingComments = new List<string>(value);
			}
		}

		public List<string> Comments => _leadingComments;

		[Obsolete("Do not use this property, use property Comments instead")]
		public List<string> TrailingComments
		{
			get
			{
				return _trailingComments;
			}
			internal set
			{
				_trailingComments = new List<string>(value);
			}
		}

		public KeyDataCollection Keys
		{
			get
			{
				return _keyDataCollection;
			}
			set
			{
				_keyDataCollection = value;
			}
		}

		public SectionData(string sectionName)
			: this(sectionName, EqualityComparer<string>.Default)
		{
		}

		public SectionData(string sectionName, IEqualityComparer<string> searchComparer)
		{
			_searchComparer = searchComparer;
			if (string.IsNullOrEmpty(sectionName))
			{
				throw new ArgumentException("section name can not be empty");
			}
			_leadingComments = new List<string>();
			_keyDataCollection = new KeyDataCollection(_searchComparer);
			SectionName = sectionName;
		}

		public SectionData(SectionData ori, IEqualityComparer<string> searchComparer = null)
		{
			SectionName = ori.SectionName;
			_searchComparer = searchComparer;
			_leadingComments = new List<string>(ori._leadingComments);
			_keyDataCollection = new KeyDataCollection(ori._keyDataCollection, searchComparer ?? ori._searchComparer);
		}

		public void ClearComments()
		{
			Comments.Clear();
			TrailingComments.Clear();
			Keys.ClearComments();
		}

		public void ClearKeyData()
		{
			Keys.RemoveAllKeys();
		}

		public void Merge(SectionData toMergeSection)
		{
			foreach (string leadingComment in toMergeSection.LeadingComments)
			{
				LeadingComments.Add(leadingComment);
			}
			Keys.Merge(toMergeSection.Keys);
			foreach (string trailingComment in toMergeSection.TrailingComments)
			{
				TrailingComments.Add(trailingComment);
			}
		}

		public object Clone()
		{
			return new SectionData(this);
		}
	}
	public class SectionDataCollection : ICloneable, IEnumerable<SectionData>, IEnumerable
	{
		private IEqualityComparer<string> _searchComparer;

		private readonly Dictionary<string, SectionData> _sectionData;

		public int Count => _sectionData.Count;

		public KeyDataCollection this[string sectionName]
		{
			get
			{
				if (_sectionData.ContainsKey(sectionName))
				{
					return _sectionData[sectionName].Keys;
				}
				return null;
			}
		}

		public SectionDataCollection()
			: this(EqualityComparer<string>.Default)
		{
		}

		public SectionDataCollection(IEqualityComparer<string> searchComparer)
		{
			_searchComparer = searchComparer;
			_sectionData = new Dictionary<string, SectionData>(_searchComparer);
		}

		public SectionDataCollection(SectionDataCollection ori, IEqualityComparer<string> searchComparer)
		{
			_searchComparer = searchComparer ?? EqualityComparer<string>.Default;
			_sectionData = new Dictionary<string, SectionData>(_searchComparer);
			foreach (SectionData item in ori)
			{
				_sectionData.Add(item.SectionName, (SectionData)item.Clone());
			}
		}

		public bool AddSection(string keyName)
		{
			if (!ContainsSection(keyName))
			{
				_sectionData.Add(keyName, new SectionData(keyName, _searchComparer));
				return true;
			}
			return false;
		}

		public void Add(SectionData data)
		{
			if (ContainsSection(data.SectionName))
			{
				SetSectionData(data.SectionName, new SectionData(data, _searchComparer));
			}
			else
			{
				_sectionData.Add(data.SectionName, new SectionData(data, _searchComparer));
			}
		}

		public void Clear()
		{
			_sectionData.Clear();
		}

		public bool ContainsSection(string keyName)
		{
			return _sectionData.ContainsKey(keyName);
		}

		public SectionData GetSectionData(string sectionName)
		{
			if (_sectionData.ContainsKey(sectionName))
			{
				return _sectionData[sectionName];
			}
			return null;
		}

		public void Merge(SectionDataCollection sectionsToMerge)
		{
			foreach (SectionData item in sectionsToMerge)
			{
				if (GetSectionData(item.SectionName) == null)
				{
					AddSection(item.SectionName);
				}
				this[item.SectionName].Merge(item.Keys);
			}
		}

		public void SetSectionData(string sectionName, SectionData data)
		{
			if (data != null)
			{
				_sectionData[sectionName] = data;
			}
		}

		public bool RemoveSection(string keyName)
		{
			return _sectionData.Remove(keyName);
		}

		public IEnumerator<SectionData> GetEnumerator()
		{
			foreach (string key in _sectionData.Keys)
			{
				yield return _sectionData[key];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public object Clone()
		{
			return new SectionDataCollection(this, _searchComparer);
		}
	}
	public class StreamIniDataParser
	{
		public IniDataParser Parser { get; protected set; }

		public StreamIniDataParser()
			: this(new IniDataParser())
		{
		}

		public StreamIniDataParser(IniDataParser parser)
		{
			Parser = parser;
		}

		public IniData ReadData(StreamReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			return Parser.Parse(reader.ReadToEnd());
		}

		public void WriteData(StreamWriter writer, IniData iniData)
		{
			if (iniData == null)
			{
				throw new ArgumentNullException("iniData");
			}
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			writer.Write(iniData.ToString());
		}

		public void WriteData(StreamWriter writer, IniData iniData, IIniDataFormatter formatter)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}
			if (iniData == null)
			{
				throw new ArgumentNullException("iniData");
			}
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			writer.Write(iniData.ToString(formatter));
		}
	}
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
}
namespace NorthernLightsBroadcast
{
	public class ColorTween : Tween<Color>
	{
		private static readonly Func<ITween<Color>, Color, Color, float, Color> LerpFunc = LerpColor;

		private static Color LerpColor(ITween<Color> t, Color start, Color end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Color.Lerp(start, end, progress);
		}

		public ColorTween()
			: base(LerpFunc)
		{
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "ShouldSuppressCrosshairs")]
	public class CrosshairPAtch
	{
		public static void Postfix(PlayerManager __instance, ref bool __result)
		{
			if (TVLock.lockedInTVView)
			{
				__result = true;
			}
		}
	}
	[HarmonyPatch(typeof(InterfaceManager), "ShouldEnableMousePointer")]
	public class CursorPatch
	{
		public static void Postfix(ref bool __result)
		{
			if (TVLock.lockedInTVView)
			{
				__result = true;
			}
		}
	}
	public static class FileStuff
	{
		public static string settingsFile = Application.dataPath + "/../Mods/NorthernLightsBroadcast_ResumeAtFrame.ini";

		public static Dictionary<string, double> clipFrames = new Dictionary<string, double>();

		public static FileStream frameFile;

		public static List<string> videoFileExtensions = new List<string>
		{
			".asf", ".avi", ".dv", ".m4v", ".mov", ".mp4", ".mpg", ".mpeg", ".ogv", ".vp8",
			".webm", ".wmv"
		};

		public static void OpenFrameFile()
		{
			if (!File.Exists(settingsFile))
			{
				frameFile = File.Create(settingsFile);
				frameFile.Close();
			}
			string[] array = File.ReadAllLines(settingsFile);
			foreach (string obj in array)
			{
				string[] array2 = new string[2];
				array2 = obj.Split("|");
				clipFrames.Add(array2[0], (long)float.Parse(array2[1]));
			}
		}

		public static void AddFrameValueToFile(string clipName, double frameValue)
		{
			double num = frameValue;
			if (num < 0.0)
			{
				num = 0.0;
			}
			if (clipFrames.ContainsKey(clipName))
			{
				clipFrames[clipName] = num;
			}
			else
			{
				clipFrames.Add(clipName, num);
			}
		}

		public static double GetFrameValueFromFile(string clipName)
		{
			if (clipFrames.ContainsKey(clipName))
			{
				return clipFrames[clipName];
			}
			AddFrameValueToFile(clipName, 0.0);
			return 0.0;
		}

		public static void SaveFrameFile()
		{
			using StreamWriter streamWriter = new StreamWriter(settingsFile);
			foreach (KeyValuePair<string, double> clipFrame in clipFrames)
			{
				double num = clipFrame.Value;
				if (num < 0.0)
				{
					num = 0.0;
				}
				streamWriter.WriteLine(clipFrame.Key + "|" + num);
			}
			streamWriter.Close();
		}

		public static string[] GetFilesInPath(string path)
		{
			List<string> list = new List<string>();
			try
			{
				string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
				foreach (string text in files)
				{
					if (videoFileExtensions.Contains(Path.GetExtension(text).ToLowerInvariant()))
					{
						list.Add(text);
					}
				}
			}
			catch
			{
			}
			return list.ToArray();
		}

		public static string[] GetFoldersInPath(string path)
		{
			List<string> list = new List<string>();
			try
			{
				foreach (string item in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
				{
					try
					{
						if ((new DirectoryInfo(item).Attributes & FileAttributes.ReparsePoint) == 0)
						{
							list.Add(item);
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			return list.ToArray();
		}
	}
	public class FloatTween : Tween<float>
	{
		private static readonly Func<ITween<float>, float, float, float, float> LerpFunc = LerpFloat;

		private static float LerpFloat(ITween<float> t, float start, float end, float progress)
		{
			return start + (end - start) * progress;
		}

		public FloatTween()
			: base(LerpFunc)
		{
		}
	}
	public static class GameObjectTweenExtensions
	{
		[HideFromIl2Cpp]
		public static FloatTween Tween(this GameObject obj, object key, float start, float end, float duration, Func<float, float> scaleFunc, Action<ITween<float>> progress, Action<ITween<float>> completion = null)
		{
			FloatTween floatTween = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
			floatTween.GameObject = obj;
			floatTween.Renderer = obj.GetComponent<Renderer>();
			return floatTween;
		}

		[HideFromIl2Cpp]
		public static Vector2Tween Tween(this GameObject obj, object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector2>> progress, Action<ITween<Vector2>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Vector2Tween vector2Tween = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
			vector2Tween.GameObject = obj;
			vector2Tween.Renderer = obj.GetComponent<Renderer>();
			return vector2Tween;
		}

		[HideFromIl2Cpp]
		public static Vector3Tween Tween(this GameObject obj, object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector3>> progress, Action<ITween<Vector3>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Vector3Tween vector3Tween = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
			vector3Tween.GameObject = obj;
			vector3Tween.Renderer = obj.GetComponent<Renderer>();
			return vector3Tween;
		}

		[HideFromIl2Cpp]
		public static Vector4Tween Tween(this GameObject obj, object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector4>> progress, Action<ITween<Vector4>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Vector4Tween vector4Tween = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
			vector4Tween.GameObject = obj;
			vector4Tween.Renderer = obj.GetComponent<Renderer>();
			return vector4Tween;
		}

		[HideFromIl2Cpp]
		public static ColorTween Tween(this GameObject obj, object key, Color start, Color end, float duration, Func<float, float> scaleFunc, Action<ITween<Color>> progress, Action<ITween<Color>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			ColorTween colorTween = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
			colorTween.GameObject = obj;
			colorTween.Renderer = obj.GetComponent<Renderer>();
			return colorTween;
		}

		[HideFromIl2Cpp]
		public static QuaternionTween Tween(this GameObject obj, object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, Action<ITween<Quaternion>> progress, Action<ITween<Quaternion>> completion = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			QuaternionTween quaternionTween = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
			quaternionTween.GameObject = obj;
			quaternionTween.Renderer = obj.GetComponent<Renderer>();
			return quaternionTween;
		}
	}
	public interface ITween
	{
		object Key { get; }

		TweenState State { get; }

		Func<float> TimeFunc { get; set; }

		void Start();

		void Pause();

		void Resume();

		void Stop(TweenStopBehavior stopBehavior);

		bool Update(float elapsedTime);
	}
	public interface ITween<T> : ITween where T : struct
	{
		T CurrentValue { get; }

		float CurrentProgress { get; }

		Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, Action<ITween<T>> progress, Action<ITween<T>> completion = null);
	}
	public class NorthernLightsBroadcastMain : MelonMod
	{
		public static AssetBundle assetBundle;

		public static GameObject NLB_TV_CRT;

		public static GameObject NLB_TV_LCD;

		public static GameObject NLB_TV_WALL;

		public static Material TelevisionB_Material_Cutout;

		public static ClipManager tvAudioManager;

		public static RaycastHit hit;

		public static int layerMask;

		public static Sprite folderIconSprite;

		public static Sprite audioIconSprite;

		public static Sprite videoIconSprite;

		public static Texture2D folderIcon;

		public static Texture2D audioIcon;

		public static Texture2D videoIcon;

		public static GameObject eventSystemObject;

		public static EventSystem eventSystem;

		public static StandaloneInputModule standaloneInputModule;

		public static Camera eventCam;

		public override void OnInitializeMelon()
		{
			LoadEmbeddedAssetBundle();
			layerMask |= 1;
			layerMask |= 131072;
			layerMask |= 524288;
			FileStuff.OpenFrameFile();
			Settings.OnLoad();
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			TweenFactory.SceneManagerSceneLoaded();
			if (!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu") && (Object)(object)eventSystem == (Object)null)
			{
				eventCam = GameManager.GetMainCamera();
				if ((Object)(object)eventCam != (Object)null)
				{
					eventSystemObject = new GameObject("EventSystem");
					eventSystem = ((Component)eventCam).gameObject.AddComponent<EventSystem>();
					standaloneInputModule = ((Component)eventCam).gameObject.AddComponent<StandaloneInputModule>();
				}
			}
			if (sceneName.Contains("MainMenu"))
			{
				SaveLoad.reloadPending = true;
			}
			if (!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu"))
			{
				SaveLoad.LoadTheTVs();
			}
		}

		public override void OnUpdate()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			if (Settings.options == null)
			{
				return;
			}
			Camera mainCamera = GameManager.GetMainCamera();
			if ((Object)(object)mainCamera == (Object)null)
			{
				return;
			}
			Vector3 position = ((Component)mainCamera).transform.position;
			Vector3 val = ((Component)mainCamera).transform.TransformDirection(Vector3.forward);
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton) && Physics.Raycast(position, val, ref hit, 2f, layerMask))
			{
				Collider collider = ((RaycastHit)(ref hit)).collider;
				if ((Object)(object)collider != (Object)null)
				{
					GameObject gameObject = ((Component)collider).gameObject;
					if ((Object)(object)gameObject != (Object)null && ((Object)gameObject).name == "PowerButton")
					{
						TVButton component = gameObject.GetComponent<TVButton>();
						if ((Object)(object)component != (Object)null)
						{
							component.TogglePower();
						}
					}
				}
			}
			if (TVLock.lockedInTVView)
			{
				if ((Object)(object)TVLock.currentManager != (Object)null && (Object)(object)TVLock.currentManager.objectRenderer != (Object)null)
				{
					((Renderer)TVLock.currentManager.objectRenderer).enabled = true;
				}
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)27) || InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
				{
					TVLock.ExitTVView();
				}
			}
			else
			{
				if (!InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton) || !Physics.Raycast(position, val, ref hit, 2f, layerMask))
				{
					return;
				}
				Collider collider2 = ((RaycastHit)(ref hit)).collider;
				if ((Object)(object)collider2 == (Object)null)
				{
					return;
				}
				GameObject gameObject2 = ((Component)collider2).gameObject;
				if ((Object)(object)gameObject2 == (Object)null)
				{
					return;
				}
				string name = ((Object)gameObject2).name;
				if (name.Contains("OBJ_TelevisionB_LOD0") || name.Contains("OBJ_Television_LOD0") || name.Contains("GEAR_TV_LCD") || name.Contains("GEAR_TV_CRT") || name.Contains("GEAR_TV_WALL"))
				{
					TVManager component2 = gameObject2.GetComponent<TVManager>();
					if ((Object)(object)component2 != (Object)null)
					{
						TVLock.EnterTVView(component2);
					}
				}
			}
		}

		public static void LoadEmbeddedAssetBundle()
		{
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02db: Unknown result type (might be due to invalid IL or missing references)
			string text = Path.Combine(Path.GetTempPath(), "NLB_bundle_" + Guid.NewGuid().ToString("N") + ".unity3d");
			using (System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthernLightsBroadcast.Resources.northernlightsbroadcastbundle"))
			{
				if (stream == null)
				{
					MelonLogger.Error("[NLB] Embedded asset bundle resource not found");
					return;
				}
				using FileStream destination = File.Create(text);
				stream.CopyTo(destination);
			}
			assetBundle = AssetBundle.LoadFromFile(text);
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
			if ((Object)(object)assetBundle == (Object)null)
			{
				MelonLogger.Error("[NLB] AssetBundle.LoadFromFile returned null");
				return;
			}
			Object.DontDestroyOnLoad((Object)(object)assetBundle);
			NLB_TV_CRT = assetBundle.LoadAsset<GameObject>("NLB_TV_CRT");
			if ((Object)(object)NLB_TV_CRT != (Object)null)
			{
				((Object)NLB_TV_CRT).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)NLB_TV_CRT);
			}
			NLB_TV_LCD = assetBundle.LoadAsset<GameObject>("NLB_TV_LCD");
			if ((Object)(object)NLB_TV_LCD != (Object)null)
			{
				((Object)NLB_TV_LCD).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)NLB_TV_LCD);
			}
			NLB_TV_WALL = assetBundle.LoadAsset<GameObject>("NLB_TV_WALL");
			if ((Object)(object)NLB_TV_WALL != (Object)null)
			{
				((Object)NLB_TV_WALL).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)NLB_TV_WALL);
			}
			TelevisionB_Material_Cutout = assetBundle.LoadAsset<Material>("MaterialTelevisionB");
			if ((Object)(object)TelevisionB_Material_Cutout == (Object)null)
			{
				MelonLogger.Msg("MaterialTelevisionB is null");
			}
			else
			{
				((Object)TelevisionB_Material_Cutout).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)TelevisionB_Material_Cutout);
			}
			folderIcon = assetBundle.LoadAsset<Texture2D>("icon_folder");
			if ((Object)(object)folderIcon != (Object)null)
			{
				folderIconSprite = Sprite.Create(folderIcon, new Rect(0f, 0f, (float)((Texture)folderIcon).width, (float)((Texture)folderIcon).height), new Vector2(0.5f, 0.5f));
				((Object)folderIconSprite).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)folderIconSprite);
			}
			audioIcon = assetBundle.LoadAsset<Texture2D>("icon_audio");
			if ((Object)(object)audioIcon != (Object)null)
			{
				audioIconSprite = Sprite.Create(audioIcon, new Rect(0f, 0f, (float)((Texture)audioIcon).width, (float)((Texture)audioIcon).height), new Vector2(0.5f, 0.5f));
				((Object)audioIconSprite).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)audioIconSprite);
			}
			videoIcon = assetBundle.LoadAsset<Texture2D>("icon_video");
			if ((Object)(object)videoIcon != (Object)null)
			{
				videoIconSprite = Sprite.Create(videoIcon, new Rect(0f, 0f, (float)((Texture)videoIcon).width, (float)((Texture)videoIcon).height), new Vector2(0.5f, 0.5f));
				((Object)videoIconSprite).hideFlags = (HideFlags)61;
				Object.DontDestroyOnLoad((Object)(object)videoIconSprite);
			}
			tvAudioManager = AudioMaster.NewClipManager();
			tvAudioManager.LoadClipFromBundle("click", "audiobuttonclick", assetBundle);
			tvAudioManager.LoadClipFromBundle("static", "audiotvstatic", assetBundle);
		}
	}
	public class QuaternionTween : Tween<Quaternion>
	{
		private static readonly Func<ITween<Quaternion>, Quaternion, Quaternion, float, Quaternion> LerpFunc = LerpQuaternion;

		private static Quaternion LerpQuaternion(ITween<Quaternion> t, Quaternion start, Quaternion end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Quaternion.Lerp(start, end, progress);
		}

		public QuaternionTween()
			: base(LerpFunc)
		{
		}
	}
	[HarmonyPatch(typeof(SaveGameSystem), "SaveSceneData")]
	public class SaveCandles
	{
		public static void Postfix(ref SlotData slot)
		{
			SaveLoad.SaveTheTVs();
		}
	}
	public static class SaveLoad
	{
		public static IniDataParser iniDataParser;

		public static IniData thisIniData;

		public static ModDataManager dataManager;

		public static string moddataString;

		public static bool reloadPending = true;

		public static void LoadTheTVs()
		{
			if (reloadPending)
			{
				MelonLogger.Msg("Loading TV data");
				iniDataParser = new IniDataParser();
				thisIniData = new IniData();
				dataManager = new ModDataManager("NorthernLightsBroadcast", debug: false);
				iniDataParser.Configuration.AllowCreateSectionsOnFly = true;
				iniDataParser.Configuration.SkipInvalidLines = true;
				iniDataParser.Configuration.OverrideDuplicateKeys = true;
				iniDataParser.Configuration.AllowDuplicateKeys = false;
				moddataString = dataManager.Load();
				thisIniData = iniDataParser.Parse(moddataString);
				reloadPending = false;
			}
		}

		public static void MaybeAddTV(string TVID)
		{
			if (!thisIniData.Sections.ContainsSection(TVID))
			{
				thisIniData.Sections.AddSection(TVID);
			}
		}

		public static string GetFolder(string TVID)
		{
			if (thisIniData == null)
			{
				return Application.dataPath + "/../Mods";
			}
			if (TVID == null || !thisIniData[TVID].ContainsKey("currentFolder"))
			{
				return Application.dataPath + "/../Mods";
			}
			if (!Directory.Exists(thisIniData[TVID]["currentFolder"]))
			{
				return Application.dataPath + "/../Mods";
			}
			return thisIniData[TVID]["currentFolder"];
		}

		public static void SetFolder(string TVID, string folder)
		{
			if (thisIniData != null && TVID != "")
			{
				thisIniData[TVID].AddKey("currentFolder");
				thisIniData[TVID]["currentFolder"] = folder;
			}
		}

		public static float GetVolume(string TVID)
		{
			if (TVID == null || !thisIniData[TVID].ContainsKey("volume"))
			{
				return 0.5f;
			}
			return float.Parse(thisIniData[TVID]["volume"]);
		}

		public static void SetVolume(string TVID, float volume)
		{
			if (thisIniData != null && TVID != "")
			{
				thisIniData[TVID].AddKey("volume");
				thisIniData[TVID]["volume"] = volume.ToString();
			}
		}

		public static string GetLastPlayed(string TVID)
		{
			if (TVID == null || !thisIniData[TVID].ContainsKey("lastPlayed"))
			{
				return "";
			}
			return thisIniData[TVID]["lastPlayed"];
		}

		public static void SetLastPlayed(string TVID, string lastPlayed)
		{
			if (thisIniData != null && TVID != "")
			{
				thisIniData[TVID].AddKey("lastPlayed");
				thisIniData[TVID]["lastPlayed"] = lastPlayed;
			}
		}

		public static TVManager.TVState GetState(string TVID)
		{
			if (TVID == null || !thisIniData[TVID].ContainsKey("state"))
			{
				return TVManager.TVState.Off;
			}
			return (TVManager.TVState)Enum.Parse(typeof(TVManager.TVState), thisIniData[TVID]["state"]);
		}

		public static void SetState(string TVID, TVManager.TVState state)
		{
			if (thisIniData != null && TVID != "")
			{
				thisIniData[TVID].AddKey("state");
				thisIniData[TVID]["state"] = state.ToString();
			}
		}

		public static void SaveTheTVs()
		{
			dataManager.Save(thisIniData.ToString());
		}
	}
	internal static class Settings
	{
		public enum LoopSetting
		{
			Off,
			LoopFile,
			LoopFolder
		}

		public static TVSettings options;

		public static void OnLoad()
		{
			options = new TVSettings();
			options.AddToModSettings("NorthernLightsBroadcast");
		}
	}
	[HarmonyPatch(typeof(StickToGround), "Awake")]
	public class StickToGroundPatch
	{
		public static void Postfix(StickToGround __instance)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			if (!((Object)((Component)__instance).gameObject).name.Contains("OBJ_TelevisionB_LOD0") && !((Object)((Component)__instance).gameObject).name.Contains("OBJ_Television_LOD0"))
			{
				return;
			}
			GameObject gameObject = ((Component)__instance).gameObject;
			SaveLoad.LoadTheTVs();
			if ((Object)gameObject != (Object)null && (Object)gameObject.GetComponent<TVManager>() == (Object)null)
			{
				gameObject.AddComponent<TVManager>();
				if (((Object)((Component)__instance).gameObject).name.Contains("OBJ_TelevisionB_LOD0"))
				{
					((Renderer)gameObject.GetComponent<MeshRenderer>()).sharedMaterial = NorthernLightsBroadcastMain.TelevisionB_Material_Cutout;
				}
			}
		}
	}
	public static class StreamStuff
	{
		public static bool gotList = false;

		public static string indexFileContent;

		public static int globalChance = 50;

		public static List<string> fileURL = new List<string>();

		public static Dictionary<string, int> playbackChance = new Dictionary<string, int>();

		public static Dictionary<string, int> playbackMaxCount = new Dictionary<string, int>();

		public static string indexFileURL = "https://digitalzombie.de/NorthernLightsBroadcast/index";

		private static string _fileURL = "https://digitalzombie.de/NorthernLightsBroadcast/lines.txt";

		public static void GetText()
		{
			string[] array = new WebClient().DownloadString(_fileURL).Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				MelonLogger.Msg(array[i]);
			}
		}

		public static void GetIndexList()
		{
			try
			{
				indexFileContent = new WebClient().DownloadString(indexFileURL);
				string[] array = indexFileContent.Split("#");
				foreach (string text in array)
				{
					if (!text.Contains("___") && text.Contains("|"))
					{
						string[] array2 = new string[3];
						array2 = text.Split("|");
						fileURL.Add(array2[0]);
						playbackChance.Add(array2[0], int.Parse(array2[1]));
						playbackMaxCount.Add(array2[0], int.Parse(array2[2]));
					}
				}
				indexFileContent = "";
				if (fileURL.Count > 0)
				{
					gotList = true;
				}
			}
			catch
			{
				gotList = false;
			}
		}
	}
	[RegisterTypeInIl2Cpp]
	public class TVButton : MonoBehaviour
	{
		public TVManager manager;

		public Shot tvClickShot;

		public MeshRenderer meshRenderer;

		public Color32 emissionColorOn = new Color32((byte)106, (byte)7, (byte)7, byte.MaxValue);

		public Color32 emissionColorOff = new Color32((byte)0, (byte)0, (byte)0, (byte)0);

		public bool isMoving;

		public bool isGlowing;

		public Vector3 outPosition;

		public Vector3 inPosition;

		public bool isSetup;

		public TVButton(IntPtr intPtr)
			: base(intPtr)
		{
		}//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)


		public void Awake()
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			if (!isSetup)
			{
				tvClickShot = AudioMaster.CreateShot(((Component)this).gameObject, AudioMaster.SourceType.SFX);
				meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
				((Renderer)meshRenderer).material.DisableKeyword("_EMISSION");
				((Renderer)meshRenderer).material.SetColor("_EmissionColor", Color32.op_Implicit(emissionColorOff));
				outPosition = ((Component)this).transform.localPosition;
				inPosition = new Vector3(outPosition.x, outPosition.y, outPosition.z - 0.006f);
				isSetup = true;
			}
		}

		[HideFromIl2Cpp]
		public IEnumerator PressButtonAnimation(float speed)
		{
			isMoving = true;
			Action<ITween<Vector3>> progress = delegate(ITween<Vector3> t)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				((Component)manager.redbutton).transform.localPosition = t.CurrentValue;
			};
			Action<ITween<Vector3>> completion = delegate
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				((Component)manager.redbutton).transform.localPosition = inPosition;
				tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				Glow(!isGlowing);
				if (manager.currentState == TVManager.TVState.Off)
				{
					manager.SwitchState(TVManager.TVState.Static);
				}
				else
				{
					manager.SwitchState(TVManager.TVState.Off);
				}
			};
			Action<ITween<Vector3>> progress2 = delegate(ITween<Vector3> t)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				((Component)manager.redbutton).transform.localPosition = t.CurrentValue;
			};
			Action<ITween<Vector3>> completion2 = delegate
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				((Component)manager.redbutton).transform.localPosition = outPosition;
				isMoving = false;
			};
			((Tween<Vector3>)((Component)manager.redbutton).gameObject.Tween(((Component)manager.redbutton).gameObject, outPosition, inPosition, speed, TweenScaleFunctions.SineEaseInOut, progress, completion)).ContinueWith<Vector3>(new Vector3Tween().Setup(inPosition, outPosition, speed, TweenScaleFunctions.SineEaseInOut, progress2, completion2));
			yield return null;
		}

		[HideFromIl2Cpp]
		public void Glow(bool enabled)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (enabled)
			{
				((Renderer)meshRenderer).material.EnableKeyword("_EMISSION");
				((Renderer)meshRenderer).material.SetColor("_EmissionColor", Color32.op_Implicit(emissionColorOn));
				isGlowing = true;
			}
			else
			{
				((Renderer)meshRenderer).material.DisableKeyword("_EMISSION");
				((Renderer)meshRenderer).material.SetColor("_EmissionColor", Color32.op_Implicit(emissionColorOff));
				isGlowing = false;
			}
		}

		[HideFromIl2Cpp]
		public void TogglePower()
		{
			if (!isMoving)
			{
				MelonCoroutines.Start(PressButtonAnimation(0.5f));
			}
		}
	}
	[HarmonyPatch(typeof(GearItem), "Deserialize")]
	public class tvComponentDeserializePatcher
	{
		public static void Postfix(ref GearItem __instance)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			if (!((Object)__instance).name.Contains("GEAR_TV_LCD") && !((Object)__instance).name.Contains("GEAR_TV_CRT") && !((Object)__instance).name.Contains("GEAR_TV_WALL"))
			{
				return;
			}
			TVManager component = ((Component)__instance).gameObject.GetComponent<TVManager>();
			if (!((Object)component != (Object)null))
			{
				return;
			}
			ObjectGuid component2 = ((Component)__instance).gameObject.GetComponent<ObjectGuid>();
			if ((Object)component2 == (Object)null)
			{
				component.objectGuid = ((Component)component).gameObject.AddComponent<ObjectGuid>();
				component.thisGuid = ((PdidObjectBase)component.objectGuid).GetPDID();
				if (component.thisGuid == null)
				{
					component.objectGuid.MaybeRuntimeRegister();
					component.thisGuid = ((PdidObjectBase)component.objectGuid).GetPDID();
				}
			}
			else
			{
				component.thisGuid = ((PdidObjectBase)component2).GetPDID();
			}
			if (SaveLoad.GetState(component.thisGuid) == TVManager.TVState.Playing)
			{
				string folder = SaveLoad.GetFolder(component.thisGuid);
				if (folder != null && Directory.Exists(folder))
				{
					component.ui.currentFolder = folder;
				}
				string lastPlayed = SaveLoad.GetLastPlayed(component.thisGuid);
				if (lastPlayed != null && File.Exists(lastPlayed))
				{
					component.ui.currentClip = lastPlayed;
				}
				component.ui.Prepare();
			}
			else
			{
				component.SwitchState(SaveLoad.GetState(component.thisGuid));
			}
		}
	}
	[HarmonyPatch(typeof(GearItem), "Awake")]
	public class tvComponentPatcher
	{
		public static void Postfix(ref GearItem __instance)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			if ((((Object)__instance).name.Contains("GEAR_TV_LCD") || ((Object)__instance).name.Contains("GEAR_TV_CRT") || ((Object)__instance).name.Contains("GEAR_TV_WALL")) && (Object)((Component)__instance).gameObject.GetComponent<TVManager>() == (Object)null)
			{
				((Component)__instance).gameObject.AddComponent<TVManager>();
			}
		}
	}
	public static class TVLock
	{
		public static float m_StartCameraFOV;

		public static Vector2 m_StartPitchLimit;

		public static Vector2 m_StartYawLimit;

		public static Vector3 m_StartPlayerPosition;

		public static float m_StartAngleX;

		public static float m_StartAngleY;

		public static TVManager currentManager;

		public static bool lockedInTVView;

		public static void ToggleTVView(TVManager tvManager)
		{
			if (lockedInTVView)
			{
				ExitTVView();
			}
			else
			{
				EnterTVView(tvManager);
			}
		}

		public static void EnterTVView(TVManager tvManager)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			if (tvManager.currentState != 0)
			{
				currentManager = tvManager;
				lockedInTVView = true;
				tvManager.ui.canvas.worldCamera = NorthernLightsBroadcastMain.eventCam;
				m_StartCameraFOV = GameManager.GetMainCamera().fieldOfView;
				m_StartPitchLimit = GameManager.GetVpFPSCamera().RotationPitchLimit;
				m_StartYawLimit = GameManager.GetVpFPSCamera().RotationYawLimit;
				m_StartPlayerPosition = ((Component)GameManager.GetVpFPSPlayer()).transform.position;
				Quaternion rotation = ((Component)GameManager.GetVpFPSPlayer()).transform.rotation;
				m_StartAngleX = ((Quaternion)(ref rotation)).eulerAngles.x;
				rotation = ((Component)GameManager.GetVpFPSPlayer()).transform.rotation;
				m_StartAngleY = ((Quaternion)(ref rotation)).eulerAngles.y;
				GameManager.GetPlayerManagerComponent().SetControlMode((PlayerControlMode)6);
				GameManager.GetPlayerManagerComponent().TeleportPlayer(tvManager.dummyCamera.transform.position - GameManager.GetVpFPSCamera().PositionOffset, tvManager.dummyCamera.transform.rotation);
				((Component)GameManager.GetVpFPSCamera()).transform.position = tvManager.dummyCamera.transform.position;
				((Component)GameManager.GetVpFPSCamera()).transform.localPosition = GameManager.GetVpFPSCamera().PositionOffset;
				vp_FPSCamera vpFPSCamera = GameManager.GetVpFPSCamera();
				rotation = tvManager.dummyCamera.transform.rotation;
				float y = ((Quaternion)(ref rotation)).eulerAngles.y;
				rotation = tvManager.dummyCamera.transform.rotation;
				vpFPSCamera.SetAngle(y, ((Quaternion)(ref rotation)).eulerAngles.x);
				GameManager.GetVpFPSCamera().SetPitchLimit(new Vector2(0f, 0f));
				GameManager.GetVpFPSCamera().SetFOVFromOptions(50f);
				GameManager.GetVpFPSCamera().SetNearPlaneOverride(0.001f);
				GameManager.GetVpFPSCamera().SetYawLimit(tvManager.dummyCamera.transform.rotation, new Vector2(0f, 0f));
				GameManager.GetVpFPSCamera().LockRotationLimit();
				((Renderer)tvManager.objectRenderer).enabled = true;
			}
		}

		public static void ExitTVView()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			if (lockedInTVView)
			{
				GameManager.GetVpFPSCamera().m_PanViewCamera.m_IsDetachedFromPlayer = false;
				GameManager.GetPlayerManagerComponent().SetControlMode((PlayerControlMode)0);
				GameManager.GetVpFPSCamera().UnlockRotationLimit();
				GameManager.GetVpFPSCamera().RotationPitchLimit = m_StartPitchLimit;
				GameManager.GetVpFPSCamera().RotationYawLimit = m_StartYawLimit;
				((Component)GameManager.GetVpFPSPlayer()).transform.position = m_StartPlayerPosition;
				((Component)GameManager.GetVpFPSCamera()).transform.localPosition = GameManager.GetVpFPSCamera().PositionOffset;
				GameManager.GetVpFPSCamera().SetAngle(m_StartAngleY, m_StartAngleX);
				GameManager.GetVpFPSCamera().SetFOVFromOptions(m_StartCameraFOV);
				GameManager.GetVpFPSCamera().UpdateCameraRotation();
				GameManager.GetPlayerManagerComponent().StickPlayerToGround();
				GameManager.GetVpFPSCamera().UnlockRotationLimit();
				currentManager.ui.ActivateOSD(value: false);
				currentManager = null;
				lockedInTVView = false;
			}
		}
	}
	[RegisterTypeInIl2Cpp]
	public class TVManager : MonoBehaviour
	{
		public enum TVState
		{
			Off,
			Static,
			Paused,
			Preparing,
			Error,
			Playing,
			Resume,
			Ended
		}

		public GearItem thisGearItem;

		public ObjectGuid objectGuid;

		public string thisGuid;

		public bool firstStartDone;

		public GameObject screenObject;

		public MeshRenderer objectRenderer;

		public TVPlayer tvplayer;

		public TVUI ui;

		public TVButton redbutton;

		public GameObject dummyCamera;

		public VideoPlayer videoPlayer;

		public bool isSetup;

		public bool isCRT;

		public string errorText;

		public Light ambilight;

		public Setting audioSetting;

		public double saveTime;

		public Shot playerAudio;

		public Shot staticAudio;

		public GraphicRaycaster graphicsRaycaster;

		public PointerEventData pointerEventData;

		public TVState currentState;

		public TVManager(IntPtr intPtr)
			: base(intPtr)
		{
		}

		public void Awake()
		{
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Expected O, but got Unknown
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Expected O, but got Unknown
			if (isSetup)
			{
				return;
			}
			if (((Object)((Component)this).gameObject).name.Contains("OBJ_TelevisionB_LOD0") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_CRT"))
			{
				screenObject = Object.Instantiate<GameObject>(NorthernLightsBroadcastMain.NLB_TV_CRT, ((Component)this).transform);
			}
			else if (((Object)((Component)this).gameObject).name.Contains("OBJ_Television_LOD0") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_LCD"))
			{
				screenObject = Object.Instantiate<GameObject>(NorthernLightsBroadcastMain.NLB_TV_LCD, ((Component)this).transform);
			}
			else if (((Object)((Component)this).gameObject).name.Contains("OBJ_Television") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_WALL"))
			{
				screenObject = Object.Instantiate<GameObject>(NorthernLightsBroadcastMain.NLB_TV_WALL, ((Component)this).transform);
			}
			((Object)screenObject).name = "NLB_TV";
			objectRenderer = ((Component)this).gameObject.GetComponent<MeshRenderer>();
			if (((Object)((Component)this).gameObject).name.Contains("GEAR_TV_CRT"))
			{
				isCRT = true;
				MelonLogger.Msg("CRT TV found");
				((Renderer)objectRenderer).sharedMaterial = NorthernLightsBroadcastMain.TelevisionB_Material_Cutout;
			}
			redbutton = ((Component)screenObject.transform.Find("PowerButton")).gameObject.AddComponent<TVButton>();
			redbutton.manager = this;
			ambilight = ((Component)screenObject.transform.Find("Ambilight")).gameObject.GetComponent<Light>();
			((Behaviour)ambilight).enabled = false;
			dummyCamera = ((Component)screenObject.transform.Find("CameraDummy")).gameObject;
			dummyCamera.transform.localPosition = dummyCamera.transform.localPosition + new Vector3(0f, 0f, 0.32f);
			CreateAudioSetting();
			staticAudio = AudioMaster.CreateShot(((Component)this).gameObject, AudioMaster.SourceType.Custom);
			staticAudio.AssignClip(NorthernLightsBroadcastMain.tvAudioManager.GetClip("static"));
			staticAudio._audioSource.loop = true;
			staticAudio.Stop();
			staticAudio.ApplySettings(audioSetting);
			playerAudio = AudioMaster.CreateShot(((Component)this).gameObject, AudioMaster.SourceType.Custom);
			playerAudio.ApplySettings(audioSetting);
			graphicsRaycaster = ((Component)screenObject.transform.Find("OSD")).gameObject.GetComponent<GraphicRaycaster>();
			videoPlayer = screenObject.GetComponent<VideoPlayer>();
			tvplayer = ((Component)this).gameObject.AddComponent<TVPlayer>();
			ui = ((Component)this).gameObject.AddComponent<TVUI>();
			thisGearItem = ((Component)this).gameObject.GetComponent<GearItem>();
			objectGuid = ((Component)this).gameObject.GetComponent<ObjectGuid>();
			if ((Object)objectGuid != (Object)null)
			{
				thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
			}
			string folder = SaveLoad.GetFolder(thisGuid);
			if (folder != null && Directory.Exists(folder))
			{
				ui.currentFolder = folder;
			}
			string lastPlayed = SaveLoad.GetLastPlayed(thisGuid);
			if (lastPlayed != null && File.Exists(lastPlayed))
			{
				ui.currentClip = lastPlayed;
			}
			if (((Object)((Component)this).gameObject).name.Contains("GEAR_TV_LCD") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_CRT") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_WALL"))
			{
				if ((Object)objectGuid == (Object)null)
				{
					objectGuid = ((Component)this).gameObject.AddComponent<ObjectGuid>();
					thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
					if (thisGuid == null)
					{
						objectGuid.MaybeRuntimeRegister();
						thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
					}
				}
				else
				{
					thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
				}
			}
			isSetup = true;
			if (SaveLoad.GetState(thisGuid) == TVState.Playing)
			{
				ui.Prepare();
			}
			else
			{
				SwitchState(SaveLoad.GetState(thisGuid));
			}
		}

		public void Update()
		{
			if (isSetup && currentState == TVState.Playing)
			{
				saveTime = videoPlayer.time;
				if (ui.OSDOpen && TVLock.lockedInTVView)
				{
					UpdateTimeText();
					UpdateTimeSlider();
				}
			}
		}

		public void UpdateTimeText()
		{
			string text = TimeSpan.FromSeconds(videoPlayer.time).ToString("hh\\:mm\\:ss");
			((TMP_Text)ui.timeText).text = text;
		}

		public void UpdateTimeSlider()
		{
			ui.progressBar.value = videoPlayer.frame;
			ui.progressBar.minValue = 1f;
			ui.progressBar.maxValue = videoPlayer.frameCount;
		}

		[HideFromIl2Cpp]
		public void SavePlaytime()
		{
			if (ui.currentClip != null)
			{
				FileStuff.AddFrameValueToFile(ui.currentClip, saveTime);
				FileStuff.SaveFrameFile();
			}
		}

		[HideFromIl2Cpp]
		public double GetPlayTime()
		{
			double num = FileStuff.GetFrameValueFromFile(ui.currentClip);
			if (num <= 0.0 || num + 1.0 >= videoPlayer.length)
			{
				num = 0.0;
			}
			return num;
		}

		[HideFromIl2Cpp]
		public void SwitchState(TVState newState)
		{
			currentState = newState;
			switch (newState)
			{
			case TVState.Off:
				ui.screenOff.SetActive(true);
				ui.screenPlayback.SetActive(false);
				ui.screenStatic.SetActive(false);
				ui.screenError.SetActive(false);
				ui.screenLoading.SetActive(false);
				ui.osdAudio.SetActive(false);
				ui.osdButtons.SetActive(false);
				ui.osdFileMenu.SetActive(false);
				ui.ActivateOSD(value: false);
				videoPlayer.Stop();
				staticAudio.Stop();
				((Behaviour)ambilight).enabled = false;
				redbutton.Glow(enabled: false);
				break;
			case TVState.Static:
				ui.screenOff.SetActive(false);
				ui.screenPlayback.SetActive(false);
				ui.screenStatic.SetActive(true);
				ui.screenError.SetActive(false);
				ui.screenLoading.SetActive(false);
				((Component)ui.playButton).gameObject.SetActive(true);
				((Component)ui.pauseButton).gameObject.SetActive(false);
				videoPlayer.Stop();
				staticAudio.Play();
				((TMP_Text)ui.playingNowText).text = "Stopped";
				redbutton.Glow(enabled: true);
				((Behaviour)ambilight).enabled = false;
				break;
			case TVState.Paused:
				ui.screenOff.SetActive(false);
				ui.screenStatic.SetActive(false);
				ui.screenError.SetActive(false);
				ui.screenLoading.SetActive(false);
				ui.screenPlayback.SetActive(true);
				((TMP_Text)ui.playingNowText).text = "Paused";
				((Component)ui.playButton).gameObject.SetActive(true);
				((Component)ui.pauseButton).gameObject.SetActive(false);
				videoPlayer.Pause();
				redbutton.Glow(enabled: true);
				staticAudio.Stop();
				((Behaviour)ambilight).enabled = false;
				break;
			case TVState.Playing:
				ui.screenOff.SetActive(false);
				ui.screenStatic.SetActive(false);
				ui.screenError.SetActive(false);
				ui.screenLoading.SetActive(false);
				ui.screenPlayback.SetActive(true);
				((Component)ui.playButton).gameObject.SetActive(false);
				((Component)ui.pauseButton).gameObject.SetActive(true);
				ui.ActivateOSD(value: false);
				staticAudio.Stop();
				redbutton.Glow(enabled: true);
				videoPlayer.time = FileStuff.GetFrameValueFromFile(ui.currentClip);
				videoPlayer.Play();
				((Behaviour)ambilight).enabled = false;
				SaveLoad.SetLastPlayed(thisGuid, ui.currentClip);
				break;
			case TVState.Error:
				((TMP_Text)ui.errorText).text = errorText;
				ui.screenOff.SetActive(false);
				ui.screenStatic.SetActive(false);
				ui.screenError.SetActive(true);
				ui.screenLoading.SetActive(false);
				ui.osdAudio.SetActive(false);
				ui.osdButtons.SetActive(false);
				ui.osdFileMenu.SetActive(false);
				redbutton.Glow(enabled: true);
				((TMP_Text)ui.playingNowText).text = "Error";
				staticAudio.Play();
				videoPlayer.Stop();
				((Behaviour)ambilight).enabled = false;
				break;
			case TVState.Preparing:
				ui.screenOff.SetActive(false);
				ui.screenStatic.SetActive(true);
				ui.screenError.SetActive(false);
				ui.screenLoading.SetActive(true);
				ui.screenPlayback.SetActive(true);
				redbutton.Glow(enabled: true);
				((TMP_Text)ui.playingNowText).text = "Loading";
				ui.osdAudio.SetActive(false);
				ui.osdButtons.SetActive(false);
				ui.osdFileMenu.SetActive(false);
				((Component)ui.playButton).gameObject.SetActive(true);
				((Component)ui.pauseButton).gameObject.SetActive(false);
				staticAudio.Stop();
				videoPlayer.Prepare();
				((Behaviour)ambilight).enabled = false;
				break;
			case TVState.Resume:
				videoPlayer.Play();
				redbutton.Glow(enabled: true);
				SwitchState(TVState.Playing);
				break;
			case TVState.Ended:
				videoPlayer.Stop();
				redbutton.Glow(enabled: true);
				saveTime = 0.0;
				SavePlaytime();
				if (Settings.options.playFolder)
				{
					ui.NextClip();
				}
				else
				{
					SwitchState(TVState.Static);
				}
				break;
			}
		}

		public void OnDestroy()
		{
			SavePlaytime();
			SaveLoad.SetState(thisGuid, currentState);
		}

		[HideFromIl2Cpp]
		private void CreateAudioSetting()
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			audioSetting = new Setting(AudioMaster.SourceType.Custom);
			audioSetting.spread = 20f;
			audioSetting.panStereo = 0f;
			audioSetting.dopplerLevel = 0.1f;
			audioSetting.maxDistance = 11f;
			audioSetting.minDistance = 0.01f;
			audioSetting.pitch = 1f;
			audioSetting.spatialBlend = 1f;
			audioSetting.rolloffFactor = 1.8f;
			audioSetting.spatialize = true;
			audioSetting.rolloffMode = (AudioRolloffMode)1;
			audioSetting.priority = 128;
			audioSetting.maxVolume = 0.5f;
			audioSetting.minVolume = 0.1f;
		}
	}
	[RegisterTypeInIl2Cpp]
	public class TVPlayer : MonoBehaviour
	{
		public TVManager manager;

		public bool isSetup;

		public TVPlayer(IntPtr intPtr)
			: base(intPtr)
		{
		}

		public void Awake()
		{
			if (!isSetup)
			{
				manager = ((Component)this).GetComponent<TVManager>();
				manager.videoPlayer.audioOutputMode = (VideoAudioOutputMode)1;
				manager.videoPlayer.SetTargetAudioSource((ushort)0, manager.playerAudio._audioSource);
				manager.videoPlayer.targetCameraAlpha = 1f;
				manager.videoPlayer.isLooping = false;
				manager.videoPlayer.aspectRatio = (VideoAspectRatio)3;
				VideoPlayer videoPlayer = manager.videoPlayer;
				videoPlayer.loopPointReached += EventHandler.op_Implicit((Action<VideoPlayer>)PlaybackEnd);
				VideoPlayer videoPlayer2 = manager.videoPlayer;
				videoPlayer2.started += EventHandler.op_Implicit((Action<VideoPlayer>)PlaybackStarted);
				VideoPlayer videoPlayer3 = manager.videoPlayer;
				videoPlayer3.prepareCompleted += EventHandler.op_Implicit((Action<VideoPlayer>)PrepareCompleted);
				VideoPlayer videoPlayer4 = manager.videoPlayer;
				videoPlayer4.errorReceived += ErrorEventHandler.op_Implicit((Action<VideoPlayer, string>)Error);
				isSetup = true;
			}
		}

		public void Error(VideoPlayer source, string message)
		{
			MelonLogger.Msg("Error on videoplayback -> " + message);
			manager.errorText = message;
			manager.SwitchState(TVManager.TVState.Error);
		}

		public void PlaybackStarted(VideoPlayer source)
		{
			manager.SwitchState(TVManager.TVState.Playing);
		}

		public void PrepareCompleted(VideoPlayer source)
		{
			manager.SwitchState(TVManager.TVState.Playing);
		}

		public void PlaybackEnd(VideoPlayer source)
		{
			manager.SwitchState(TVManager.TVState.Ended);
		}
	}
	internal class TVSettings : JsonModSettings
	{
		[Section("按键")]
		[Name("互动键")]
		[Description("用于开关电视的按键。默认鼠标中键(Mouse2)")]
		public KeyCode interactButton = (KeyCode)325;

		[Section("播放")]
		[Name("连续播放文件夹")]
		[Description("当前文件播完后继续播放同文件夹的下一个文件")]
		public bool playFolder = true;

		[Name("循环播放文件夹")]
		[Description("当前文件夹的所有文件循环播放")]
		public bool loopFolder;

		[Section("调试")]
		[Name("调试模式")]
		[Description("调试输出。默认：关闭")]
		public bool disableStronks;

		protected override void OnConfirm()
		{
			base.OnConfirm();
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "AddItemToPlayerInventory")]
	public class tvTurnOffOnStow
	{
		public static void Prefix(ref PlayerManager __instance, ref GearItem gi, ref bool trackItemLooted, ref bool enableNotificationFlag)
		{
			if (((Object)gi).name.Contains("GEAR_TV_LCD") || ((Object)gi).name.Contains("GEAR_TV_CRT") || ((Object)gi).name.Contains("GEAR_TV_WALL"))
			{
				TVManager component = ((Component)gi).gameObject.GetComponent<TVManager>();
				if (component.currentState != 0)
				{
					component.SavePlaytime();
					component.SwitchState(TVManager.TVState.Off);
					SaveLoad.SetState(component.thisGuid, component.currentState);
				}
			}
		}
	}
	[RegisterTypeInIl2Cpp]
	public class TVUI : MonoBehaviour
	{
		public Canvas canvas;

		public CanvasGroup canvasGroup;

		public TVManager manager;

		public bool isSetup;

		public GameObject screenPlayback;

		public GameObject screenOff;

		public GameObject screenStatic;

		public GameObject screenError;

		public GameObject screenLoading;

		public GameObject osdAudio;

		public GameObject osdButtons;

		public GameObject osdFileMenu;

		public GameObject OSD;

		public bool OSDOpen;

		public bool isFading;

		public bool fileBrowserOpen;

		public Slider audioSlider;

		public Button muteButton;

		public Button playButton;

		public Button pauseButton;

		public Button stopButton;

		public Button nextButton;

		public Button prevButton;

		public Button fileBrowserButton;

		public Button uiActivator;

		public Slider progressBar;

		public Button pageNext;

		public Button pagePrev;

		public TextMeshProUGUI playingNowText;

		public TextMeshProUGUI timeText;

		public TextMeshProUGUI currentDir;

		public TextMeshProUGUI errorText;

		public TextMeshProUGUI pageText;

		public Button fileBrowserUpButton;

		public Button[] listButtons = (Button[])(object)new Button[8];

		public TextMeshProUGUI[] listText = (TextMeshProUGUI[])(object)new TextMeshProUGUI[8];

		public TextMeshProUGUI[] listLength = (TextMeshProUGUI[])(object)new TextMeshProUGUI[8];

		public Image[] listSprites = (Image[])(object)new Image[8];

		public GameObject[] driveButtonObjects = (GameObject[])(object)new GameObject[8];

		public Button[] driveButtons = (Button[])(object)new Button[8];

		public string currentFolder = Application.dataPath + "/../Mods";

		public string currentClip;

		public int currentClipIndex;

		public Dictionary<string, bool> folderContents = new Dictionary<string, bool>();

		public int currentPage;

		public int currentPageCount = 1;

		private Color32 folderColor = new Color32((byte)90, (byte)187, (byte)248, byte.MaxValue);

		private Color32 audioColor = new Color32(byte.MaxValue, (byte)226, (byte)115, byte.MaxValue);

		private Color32 videoColor = new Color32((byte)112, (byte)217, (byte)81, byte.MaxValue);

		public TVUI(IntPtr intPtr)
			: base(intPtr)
		{
		}//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)


		public void Awake()
		{
			if (isSetup)
			{
				return;
			}
			manager = ((Component)this).gameObject.GetComponent<TVManager>();
			screenPlayback = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenPlaybackMesh")).gameObject;
			screenOff = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenOff")).gameObject;
			screenStatic = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenStatic")).gameObject;
			screenLoading = ((Component)((Component)this).transform.Find("NLB_TV/Screens/ScreenLoading")).gameObject;
			screenError = ((Component)((Component)this).gameObject.transform.Find("NLB_TV/Screens/ScreenError")).gameObject;
			errorText = ((Component)((Component)this).gameObject.transform.Find("NLB_TV/Screens/ScreenError/ErrorWindow/Message")).GetComponent<TextMeshProUGUI>();
			OSD = ((Component)((Component)this).gameObject.transform.Find("NLB_TV/OSD")).gameObject;
			canvas = OSD.GetComponent<Canvas>();
			canvasGroup = OSD.GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			osdAudio = ((Component)OSD.transform.Find("Audio")).gameObject;
			osdButtons = ((Component)OSD.transform.Find("Buttons")).gameObject;
			osdFileMenu = ((Component)OSD.transform.Find("FileMenu")).gameObject;
			osdFileMenu.SetActive(false);
			uiActivator = ((Component)OSD.transform.Find("UIActivator")).GetComponent<Button>();
			((UnityEvent)uiActivator.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				OSDActivationButton();
			}));
			pageNext = ((Component)osdFileMenu.transform.Find("Pagestuff/ButtonRight")).GetComponent<Button>();
			((UnityEvent)pageNext.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				NextPage();
			}));
			pagePrev = ((Component)osdFileMenu.transform.Find("Pagestuff/ButtonLeft")).GetComponent<Button>();
			((UnityEvent)pagePrev.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				PrevPage();
			}));
			pageText = ((Component)osdFileMenu.transform.Find("Pagestuff/Pagenumber")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)pageText).text = "1 / 1";
			currentDir = ((Component)osdFileMenu.transform.Find("CurrentDir")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)currentDir).text = currentFolder;
			fileBrowserUpButton = ((Component)osdFileMenu.transform.Find("DirUp")).GetComponent<Button>();
			((UnityEvent)fileBrowserUpButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				UpDir();
			}));
			audioSlider = ((Component)osdAudio.transform.Find("Slider")).GetComponent<Slider>();
			audioSlider.value = SaveLoad.GetVolume(manager.thisGuid);
			manager.playerAudio._audioSource.volume = audioSlider.value;
			manager.staticAudio._audioSource.volume = audioSlider.value;
			((UnityEvent<float>)(object)audioSlider.onValueChanged).AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>((Delegate)(Action<float>)delegate
			{
				VolumeSlider();
			}));
			muteButton = ((Component)osdAudio.transform.Find("Mute")).GetComponent<Button>();
			((UnityEvent)muteButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				Mute();
			}));
			playingNowText = ((Component)osdButtons.transform.Find("PlayingNow")).GetComponent<TextMeshProUGUI>();
			timeText = ((Component)osdButtons.transform.Find("Time")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)timeText).text = "00:00:00";
			((TMP_Text)playingNowText).text = "已停止";
			playButton = ((Component)osdButtons.transform.Find("PlayButtons/Play")).GetComponent<Button>();
			((UnityEvent)playButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				Prepare();
			}));
			pauseButton = ((Component)osdButtons.transform.Find("PlayButtons/Pause")).GetComponent<Button>();
			((UnityEvent)pauseButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				Pause();
			}));
			stopButton = ((Component)osdButtons.transform.Find("PlayButtons/Stop")).GetComponent<Button>();
			((UnityEvent)stopButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				Stop();
			}));
			nextButton = ((Component)osdButtons.transform.Find("PlayButtons/Next")).GetComponent<Button>();
			((UnityEvent)nextButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				NextClip();
			}));
			prevButton = ((Component)osdButtons.transform.Find("PlayButtons/Prev")).GetComponent<Button>();
			((UnityEvent)prevButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				PrevClip();
			}));
			fileBrowserButton = ((Component)osdButtons.transform.Find("PlayButtons/Browser")).GetComponent<Button>();
			((UnityEvent)fileBrowserButton.onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				FileMenu();
			}));
			progressBar = ((Component)osdButtons.transform.Find("ProgressBar/Slider")).GetComponent<Slider>();
			progressBar.value = 0f;
			((UnityEvent<float>)(object)progressBar.onValueChanged).AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>((Delegate)(Action<float>)delegate
			{
				ProgressBar();
			}));
			listButtons[0] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line1")).GetComponent<Button>();
			((UnityEvent)listButtons[0].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(0);
			}));
			listButtons[1] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line2")).GetComponent<Button>();
			((UnityEvent)listButtons[1].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(1);
			}));
			listButtons[2] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line3")).GetComponent<Button>();
			((UnityEvent)listButtons[2].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(2);
			}));
			listButtons[3] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line4")).GetComponent<Button>();
			((UnityEvent)listButtons[3].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(3);
			}));
			listButtons[4] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line5")).GetComponent<Button>();
			((UnityEvent)listButtons[4].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(4);
			}));
			listButtons[5] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line6")).GetComponent<Button>();
			((UnityEvent)listButtons[5].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(5);
			}));
			listButtons[6] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line7")).GetComponent<Button>();
			((UnityEvent)listButtons[6].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(6);
			}));
			listButtons[7] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line8")).GetComponent<Button>();
			((UnityEvent)listButtons[7].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
			{
				ItemButtom(7);
			}));
			listSprites[0] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line1/Icon")).GetComponent<Image>();
			listSprites[1] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line2/Icon")).GetComponent<Image>();
			listSprites[2] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line3/Icon")).GetComponent<Image>();
			listSprites[3] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line4/Icon")).GetComponent<Image>();
			listSprites[4] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line5/Icon")).GetComponent<Image>();
			listSprites[5] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line6/Icon")).GetComponent<Image>();
			listSprites[6] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line7/Icon")).GetComponent<Image>();
			listSprites[7] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line8/Icon")).GetComponent<Image>();
			listText[0] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line1/Text")).GetComponent<TextMeshProUGUI>();
			listText[1] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line2/Text")).GetComponent<TextMeshProUGUI>();
			listText[2] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line3/Text")).GetComponent<TextMeshProUGUI>();
			listText[3] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line4/Text")).GetComponent<TextMeshProUGUI>();
			listText[4] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line5/Text")).GetComponent<TextMeshProUGUI>();
			listText[5] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line6/Text")).GetComponent<TextMeshProUGUI>();
			listText[6] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line7/Text")).GetComponent<TextMeshProUGUI>();
			listText[7] = ((Component)osdFileMenu.transform.Find("ContentFilelist/Line8/Text")).GetComponent<TextMeshProUGUI>();
			listLength[0] = ((Component)osdFileMenu.transform.Find("ContentTime/Line1/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[0]).text = " ";
			listLength[1] = ((Component)osdFileMenu.transform.Find("ContentTime/Line2/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[1]).text = " ";
			listLength[2] = ((Component)osdFileMenu.transform.Find("ContentTime/Line3/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[2]).text = " ";
			listLength[3] = ((Component)osdFileMenu.transform.Find("ContentTime/Line4/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[3]).text = " ";
			listLength[4] = ((Component)osdFileMenu.transform.Find("ContentTime/Line5/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[4]).text = "   ";
			listLength[5] = ((Component)osdFileMenu.transform.Find("ContentTime/Line6/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[5]).text = "  ";
			listLength[6] = ((Component)osdFileMenu.transform.Find("ContentTime/Line7/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[6]).text = "  ";
			listLength[7] = ((Component)osdFileMenu.transform.Find("ContentTime/Line8/Text")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)listLength[7]).text = "  ";
			DriveInfo[] drives = DriveInfo.GetDrives();
			for (int i = 0; i < 8; i++)
			{
				driveButtonObjects[i] = ((Component)osdFileMenu.transform.Find("ContentDrivelist/Line" + (i + 1))).gameObject;
				if (i < drives.Length)
				{
					driveButtons[i] = driveButtonObjects[i].GetComponent<Button>();
					driveButtonObjects[i].SetActive(true);
					string driveletter = drives[i].Name.ToString();
					((UnityEvent)driveButtons[i].onClick).AddListener(DelegateSupport.ConvertDelegate<UnityAction>((Delegate)(Action)delegate
					{
						DriveButton(driveletter);
					}));
					((TMP_Text)((Component)driveButtons[i]).GetComponentInChildren<TextMeshProUGUI>()).text = driveletter;
				}
				else
				{
					driveButtonObjects[i].SetActive(false);
				}
			}
			isSetup = true;
		}

		[HideFromIl2Cpp]
		public void PopulateFiles()
		{
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			string[] filesInPath = FileStuff.GetFilesInPath(currentFolder);
			string[] foldersInPath = FileStuff.GetFoldersInPath(currentFolder);
			SaveLoad.SetFolder(manager.thisGuid, currentFolder);
			folderContents = new Dictionary<string, bool>();
			string[] array = foldersInPath;
			foreach (string key in array)
			{
				folderContents.Add(key, value: true);
			}
			array = filesInPath;
			foreach (string key2 in array)
			{
				folderContents.Add(key2, value: false);
			}
			currentPageCount = (int)Math.Ceiling((double)folderContents.Count / 8.0);
			for (int j = 0; j < 8; j++)
			{
				if (j + currentPage * 8 < folderContents.Count)
				{
					((Component)listButtons[j]).gameObject.active = true;
					((TMP_Text)listText[j]).text = Path.GetFileName(folderContents.ElementAt(j + currentPage * 8).Key);
					if (folderContents.ElementAt(j + currentPage * 8).Value)
					{
						((Graphic)listText[j]).color = Color32.op_Implicit(folderColor);
						listSprites[j].sprite = NorthernLightsBroadcastMain.folderIconSprite;
					}
					else
					{
						((Graphic)listText[j]).color = Color32.op_Implicit(videoColor);
						listSprites[j].sprite = NorthernLightsBroadcastMain.videoIconSprite;
					}
				}
				else
				{
					((TMP_Text)listText[j]).text = "";
					listSprites[j].sprite = null;
					((Component)listButtons[j]).gameObject.active = false;
				}
			}
		}

		[HideFromIl2Cpp]
		public IEnumerator FadeIn(float speed)
		{
			isFading = true;
			float start = 0f;
			float endAlpha = 1f;
			Action<ITween<float>> progress = delegate(ITween<float> t)
			{
				canvasGroup.alpha = t.CurrentValue;
			};
			Action<ITween<float>> completion = delegate
			{
				canvasGroup.alpha = endAlpha;
				isFading = false;
			};
			((Component)canvasGroup).gameObject.Tween(((Component)canvasGroup).gameObject, start, endAlpha, speed, TweenScaleFunctions.SineEaseInOut, progress, completion);
			yield return null;
		}

		[HideFromIl2Cpp]
		public IEnumerator FadeOut(float speed)
		{
			isFading = true;
			float start = 1f;
			float endAlpha = 0f;
			Action<ITween<float>> progress = delegate(ITween<float> t)
			{
				canvasGroup.alpha = t.CurrentValue;
			};
			Action<ITween<float>> completion = delegate
			{
				canvasGroup.alpha = endAlpha;
				isFading = false;
				osdAudio.SetActive(false);
				osdFileMenu.SetActive(false);
				fileBrowserOpen = false;
			};
			((Component)canvasGroup).gameObject.Tween(((Component)canvasGroup).gameObject, start, endAlpha, speed, TweenScaleFunctions.SineEaseInOut, progress, completion);
			yield return null;
		}

		[HideFromIl2Cpp]
		public void ActivateOSD(bool value)
		{
			if (value)
			{
				OSDOpen = true;
				osdAudio.SetActive(true);
				osdButtons.SetActive(true);
				osdFileMenu.SetActive(false);
				fileBrowserOpen = false;
				MelonCoroutines.Start(FadeIn(0.5f));
			}
			else
			{
				OSDOpen = false;
				MelonCoroutines.Start(FadeOut(0.5f));
			}
		}

		[HideFromIl2Cpp]
		public void OSDActivationButton()
		{
			if (manager.currentState != 0 && !isFading)
			{
				if (manager.currentState == TVManager.TVState.Error)
				{
					manager.SwitchState(TVManager.TVState.Static);
				}
				ActivateOSD(!OSDOpen);
			}
		}

		[HideFromIl2Cpp]
		public void UpdatePage()
		{
			PopulateFiles();
			((TMP_Text)currentDir).text = currentFolder;
			int num = currentPage + 1;
			((TMP_Text)pageText).text = num + " / " + currentPageCount;
		}

		[HideFromIl2Cpp]
		public void ItemButtom(int button)
		{
			if (folderContents.ElementAt(button + currentPage * 8).Value)
			{
				currentFolder = folderContents.ElementAt(button + currentPage * 8).Key;
				currentPage = 0;
				UpdatePage();
			}
			else
			{
				currentClip = folderContents.ElementAt(button + currentPage * 8).Key;
				currentClipIndex = button + currentPage * 8;
				Prepare();
			}
		}

		[HideFromIl2Cpp]
		public void DriveButton(string driveletter)
		{
			currentFolder = driveletter;
			currentPage = 0;
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			UpdatePage();
		}

		[HideFromIl2Cpp]
		public void Resume()
		{
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			((TMP_Text)playingNowText).text = Path.GetFileName(currentClip);
			manager.videoPlayer.url = currentClip;
			manager.SwitchState(TVManager.TVState.Resume);
			screenLoading.SetActive(true);
		}

		[HideFromIl2Cpp]
		public void Prepare()
		{
			if (currentClip != null)
			{
				if (manager.currentState == TVManager.TVState.Paused)
				{
					manager.SwitchState(TVManager.TVState.Resume);
					return;
				}
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				((TMP_Text)playingNowText).text = Path.GetFileName(currentClip);
				manager.videoPlayer.url = currentClip;
				manager.SwitchState(TVManager.TVState.Preparing);
				screenLoading.SetActive(true);
			}
		}

		[HideFromIl2Cpp]
		public void Stop()
		{
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			manager.SavePlaytime();
			manager.SwitchState(TVManager.TVState.Static);
		}

		[HideFromIl2Cpp]
		public void Pause()
		{
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			manager.SavePlaytime();
			manager.SwitchState(TVManager.TVState.Paused);
		}

		[HideFromIl2Cpp]
		public void NextClip()
		{
			if (currentClipIndex < folderContents.Count - 1 && !folderContents.ElementAt(currentClipIndex + 1).Value)
			{
				currentClipIndex++;
				currentClip = folderContents.ElementAt(currentClipIndex).Key;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				Prepare();
			}
			else if (Settings.options.loopFolder)
			{
				currentClipIndex = 0;
				currentClip = folderContents.ElementAt(currentClipIndex).Key;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				Prepare();
			}
			else
			{
				manager.SwitchState(TVManager.TVState.Static);
			}
		}

		[HideFromIl2Cpp]
		public void PrevClip()
		{
			if (manager.currentState == TVManager.TVState.Playing)
			{
				if (manager.videoPlayer.time > 5.0)
				{
					manager.videoPlayer.time = 0.0;
					manager.saveTime = 0.0;
					manager.SavePlaytime();
				}
				else if (currentClipIndex > 0 && !folderContents.ElementAt(currentClipIndex - 1).Value)
				{
					currentClipIndex--;
					currentClip = folderContents.ElementAt(currentClipIndex).Key;
					manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
					Prepare();
				}
			}
			if (currentClipIndex > 0 && !folderContents.ElementAt(currentClipIndex - 1).Value)
			{
				currentClipIndex--;
				currentClip = folderContents.ElementAt(currentClipIndex).Key;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				Prepare();
			}
		}

		[HideFromIl2Cpp]
		public void FileMenu()
		{
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			if (fileBrowserOpen)
			{
				osdFileMenu.SetActive(false);
				fileBrowserOpen = false;
			}
			else
			{
				osdFileMenu.SetActive(true);
				UpdatePage();
				fileBrowserOpen = true;
			}
		}

		[HideFromIl2Cpp]
		public void UpDir()
		{
			if (Directory.GetParent(currentFolder) != null)
			{
				currentFolder = Directory.GetParent(currentFolder).FullName;
				currentPage = 0;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				UpdatePage();
			}
		}

		[HideFromIl2Cpp]
		public void Mute()
		{
			manager.playerAudio._audioSource.mute = !manager.playerAudio._audioSource.mute;
			manager.staticAudio._audioSource.mute = !manager.staticAudio._audioSource.mute;
		}

		[HideFromIl2Cpp]
		public void NextPage()
		{
			if (currentPage < currentPageCount - 1)
			{
				currentPage++;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				UpdatePage();
			}
		}

		[HideFromIl2Cpp]
		public void PrevPage()
		{
			if (currentPage > 0)
			{
				currentPage--;
				manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
				UpdatePage();
			}
		}

		[HideFromIl2Cpp]
		public void VolumeSlider()
		{
			manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
			manager.playerAudio.SetVolume(audioSlider.value);
			manager.staticAudio.SetVolume(audioSlider.value);
			SaveLoad.SetVolume(manager.thisGuid, audioSlider.value);
		}

		[HideFromIl2Cpp]
		public void ProgressBar()
		{
		}

		public void Update()
		{
		}
	}
	public class Tween<T> : ITween<T>, ITween where T : struct
	{
		private readonly Func<ITween<T>, T, T, float, T> lerpFunc;

		private float currentTime;

		private float duration;

		private Func<float, float> scaleFunc;

		private Action<ITween<T>> progressCallback;

		private Action<ITween<T>> completionCallback;

		private TweenState state;

		private T start;

		private T end;

		private T value;

		private ITween continueWith;

		public object Key { get; set; }

		public float CurrentTime => currentTime;

		public float Duration => duration;

		public float Delay { get; set; }

		public TweenState State => state;

		public T StartValue => start;

		public T EndValue => end;

		public T CurrentValue => value;

		public Func<float> TimeFunc { get; set; }

		public GameObject GameObject { get; set; }

		public Renderer Renderer { get; set; }

		public bool ForceUpdate { get; set; }

		public float CurrentProgress { get; private set; }

		public Tween(Func<ITween<T>, T, T, float, T> lerpFunc)
		{
			this.lerpFunc = lerpFunc;
			state = TweenState.Stopped;
			TimeFunc = TweenFactory.DefaultTimeFunc;
		}

		public Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, Action<ITween<T>> progress, Action<ITween<T>> completion = null)
		{
			scaleFunc = scaleFunc ?? TweenScaleFunctions.Linear;
			currentTime = 0f;
			this.duration = duration;
			this.scaleFunc = scaleFunc;
			progressCallback = progress;
			completionCallback = completion;
			this.start = start;
			this.end = end;
			return this;
		}

		public void Start()
		{
			if (state == TweenState.Running)
			{
				return;
			}
			if (duration <= 0f && Delay <= 0f)
			{
				value = end;
				if (progressCallback != null)
				{
					progressCallback(this);
				}
				if (completionCallback != null)
				{
					completionCallback(this);
				}
			}
			else
			{
				state = TweenState.Running;
				UpdateValue();
			}
		}

		public void Pause()
		{
			if (state == TweenState.Running)
			{
				state = TweenState.Paused;
			}
		}

		public void Resume()
		{
			if (state == TweenState.Paused)
			{
				state = TweenState.Running;
			}
		}

		public void Stop(TweenStopBehavior stopBehavior)
		{
			if (state == TweenState.Stopped)
			{
				return;
			}
			state = TweenState.Stopped;
			if (stopBehavior == TweenStopBehavior.Complete)
			{
				currentTime = duration;
				UpdateValue();
				if (completionCallback != null)
				{
					completionCallback(this);
					completionCallback = null;
				}
				if (continueWith != null)
				{
					continueWith.Start();
					TweenFactory.AddTween(continueWith);
					continueWith = null;
				}
			}
		}

		public bool Update(float elapsedTime)
		{
			if (state == TweenState.Running)
			{
				if (Delay > 0f)
				{
					currentTime += elapsedTime;
					if (currentTime <= Delay)
					{
						return false;
					}
					currentTime -= Delay;
					Delay = 0f;
				}
				else
				{
					currentTime += elapsedTime;
				}
				if (currentTime >= duration)
				{
					Stop(TweenStopBehavior.Complete);
					return true;
				}
				UpdateValue();
				return false;
			}
			return state == TweenState.Stopped;
		}

		public Tween<TNewTween> ContinueWith<TNewTween>(Tween<TNewTween> tween) where TNewTween : struct
		{
			tween.Key = Key;
			tween.GameObject = GameObject;
			tween.Renderer = Renderer;
			tween.ForceUpdate = ForceUpdate;
			continueWith = tween;
			return tween;
		}

		private void UpdateValue()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			if ((Object)Renderer == (Object)null || Renderer.isVisible || ForceUpdate)
			{
				CurrentProgress = scaleFunc(currentTime / duration);
				value = lerpFunc(this, start, end, CurrentProgress);
				if (progressCallback != null)
				{
					progressCallback(this);
				}
			}
		}
	}
	[RegisterTypeInIl2Cpp]
	public class TweenFactory : MonoBehaviour
	{
		private static GameObject root;

		private static readonly List<ITween> tweens = new List<ITween>();

		private static GameObject toDestroy;

		public static TweenStopBehavior AddKeyStopBehavior = TweenStopBehavior.DoNotModify;

		public static Func<float> DefaultTimeFunc = TimeFuncDeltaTime;

		public static readonly Func<float> TimeFuncDeltaTimeFunc = TimeFuncDeltaTime;

		public static readonly Func<float> TimeFuncUnscaledDeltaTimeFunc = TimeFuncUnscaledDeltaTime;

		public static bool ClearTweensOnLevelLoad { get; set; }

		public TweenFactory(IntPtr intPtr)
			: base(intPtr)
		{
		}

		[HideFromIl2Cpp]
		private static void EnsureCreated()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Expected O, but got Unknown
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected O, but got Unknown
			if (!((Object)root == (Object)null) || !Application.isPlaying)
			{
				return;
			}
			root = GameObject.Find("DigitalRubyTween");
			if ((Object)root == (Object)null || (Object)root.GetComponent<TweenFactory>() == (Object)null)
			{
				if ((Object)root != (Object)null)
				{
					toDestroy = root;
				}
				root = new GameObject
				{
					name = "DigitalRubyTween",
					hideFlags = (HideFlags)61
				};
				((Object)root.AddComponent<TweenFactory>()).hideFlags = (HideFlags)61;
			}
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad((Object)root);
			}
		}

		private void Start()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			if ((Object)toDestroy != (Object)null)
			{
				Object.Destroy((Object)toDestroy);
				toDestroy = null;
			}
		}

		[HideFromIl2Cpp]
		public static void SceneManagerSceneLoaded()
		{
			if (ClearTweensOnLevelLoad)
			{
				tweens.Clear();
			}
		}

		private void Update()
		{
			for (int num = tweens.Count - 1; num >= 0; num--)
			{
				ITween tween = tweens[num];
				if (tween.Update(tween.TimeFunc()) && num < tweens.Count && tweens[num] == tween)
				{
					tweens.RemoveAt(num);
				}
			}
		}

		public static void OwnUpdate()
		{
			for (int num = tweens.Count - 1; num >= 0; num--)
			{
				ITween tween = tweens[num];
				if (tween.Update(tween.TimeFunc()) && num < tweens.Count && tweens[num] == tween)
				{
					tweens.RemoveAt(num);
				}
			}
		}

		[HideFromIl2Cpp]
		public static FloatTween Tween(object key, float start, float end, float duration, Func<float, float> scaleFunc, Action<ITween<float>> progress, Action<ITween<float>> completion = null)
		{
			FloatTween floatTween = new FloatTween();
			floatTween.Key = key;
			floatTween.Setup(start, end, duration, scaleFunc, progress, completion);
			floatTween.Start();
			AddTween(floatTween);
			return floatTween;
		}

		[HideFromIl2Cpp]
		public static Vector2Tween Tween(object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector2>> progress, Action<ITween<Vector2>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector2Tween vector2Tween = new Vector2Tween();
			vector2Tween.Key = key;
			vector2Tween.Setup(start, end, duration, scaleFunc, progress, completion);
			vector2Tween.Start();
			AddTween(vector2Tween);
			return vector2Tween;
		}

		[HideFromIl2Cpp]
		public static Vector3Tween Tween(object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector3>> progress, Action<ITween<Vector3>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector3Tween vector3Tween = new Vector3Tween();
			vector3Tween.Key = key;
			vector3Tween.Setup(start, end, duration, scaleFunc, progress, completion);
			vector3Tween.Start();
			AddTween(vector3Tween);
			return vector3Tween;
		}

		[HideFromIl2Cpp]
		public static Vector4Tween Tween(object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector4>> progress, Action<ITween<Vector4>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Vector4Tween vector4Tween = new Vector4Tween();
			vector4Tween.Key = key;
			vector4Tween.Setup(start, end, duration, scaleFunc, progress, completion);
			vector4Tween.Start();
			AddTween(vector4Tween);
			return vector4Tween;
		}

		[HideFromIl2Cpp]
		public static ColorTween Tween(object key, Color start, Color end, float duration, Func<float, float> scaleFunc, Action<ITween<Color>> progress, Action<ITween<Color>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			ColorTween colorTween = new ColorTween();
			colorTween.Key = key;
			colorTween.Setup(start, end, duration, scaleFunc, progress, completion);
			colorTween.Start();
			AddTween(colorTween);
			return colorTween;
		}

		[HideFromIl2Cpp]
		public static QuaternionTween Tween(object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, Action<ITween<Quaternion>> progress, Action<ITween<Quaternion>> completion = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			QuaternionTween quaternionTween = new QuaternionTween();
			quaternionTween.Key = key;
			quaternionTween.Setup(start, end, duration, scaleFunc, progress, completion);
			quaternionTween.Start();
			AddTween(quaternionTween);
			return quaternionTween;
		}

		[HideFromIl2Cpp]
		public static void AddTween(ITween tween)
		{
			EnsureCreated();
			if (tween.Key != null)
			{
				RemoveTweenKey(tween.Key, AddKeyStopBehavior);
			}
			tweens.Add(tween);
		}

		[HideFromIl2Cpp]
		public static bool RemoveTween(ITween tween, TweenStopBehavior stopBehavior)
		{
			tween.Stop(stopBehavior);
			return tweens.Remove(tween);
		}

		[HideFromIl2Cpp]
		public static bool RemoveTweenKey(object key, TweenStopBehavior stopBehavior)
		{
			if (key == null)
			{
				return false;
			}
			bool result = false;
			for (int num = tweens.Count - 1; num >= 0; num--)
			{
				ITween tween = tweens[num];
				if (key.Equals(tween.Key))
				{
					tween.Stop(stopBehavior);
					tweens.RemoveAt(num);
					result = true;
				}
			}
			return result;
		}

		[HideFromIl2Cpp]
		public static void Clear()
		{
			tweens.Clear();
		}

		private static float TimeFuncDeltaTime()
		{
			return Time.deltaTime;
		}

		private static float TimeFuncUnscaledDeltaTime()
		{
			return Time.unscaledDeltaTime;
		}
	}
	public static class TweenScaleFunctions
	{
		private const float halfPi = (float)Math.PI / 2f;

		public static readonly Func<float, float> Linear = LinearFunc;

		public static readonly Func<float, float> QuadraticEaseIn = QuadraticEaseInFunc;

		public static readonly Func<float, float> QuadraticEaseOut = QuadraticEaseOutFunc;

		public static readonly Func<float, float> QuadraticEaseInOut = QuadraticEaseInOutFunc;

		public static readonly Func<float, float> CubicEaseIn = CubicEaseInFunc;

		public static readonly Func<float, float> CubicEaseOut = CubicEaseOutFunc;

		public static readonly Func<float, float> CubicEaseInOut = CubicEaseInOutFunc;

		public static readonly Func<float, float> QuarticEaseIn = QuarticEaseInFunc;

		public static readonly Func<float, float> QuarticEaseOut = QuarticEaseOutFunc;

		public static readonly Func<float, float> QuarticEaseInOut = QuarticEaseInOutFunc;

		public static readonly Func<float, float> QuinticEaseIn = QuinticEaseInFunc;

		public static readonly Func<float, float> QuinticEaseOut = QuinticEaseOutFunc;

		public static readonly Func<float, float> QuinticEaseInOut = QuinticEaseInOutFunc;

		public static readonly Func<float, float> SineEaseIn = SineEaseInFunc;

		public static readonly Func<float, float> SineEaseOut = SineEaseOutFunc;

		public static readonly Func<float, float> SineEaseInOut = SineEaseInOutFunc;

		private static float LinearFunc(float progress)
		{
			return progress;
		}

		private static float QuadraticEaseInFunc(float progress)
		{
			return EaseInPower(progress, 2);
		}

		private static float QuadraticEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 2);
		}

		private static float QuadraticEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 2);
		}

		private static float CubicEaseInFunc(float progress)
		{
			return EaseInPower(progress, 3);
		}

		private static float CubicEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 3);
		}

		private static float CubicEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 3);
		}

		private static float QuarticEaseInFunc(float progress)
		{
			return EaseInPower(progress, 4);
		}

		private static float QuarticEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 4);
		}

		private static float QuarticEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 4);
		}

		private static float QuinticEaseInFunc(float progress)
		{
			return EaseInPower(progress, 5);
		}

		private static float QuinticEaseOutFunc(float progress)
		{
			return EaseOutPower(progress, 5);
		}

		private static float QuinticEaseInOutFunc(float progress)
		{
			return EaseInOutPower(progress, 5);
		}

		private static float SineEaseInFunc(float progress)
		{
			return Mathf.Sin(progress * ((float)Math.PI / 2f) - (float)Math.PI / 2f) + 1f;
		}

		private static float SineEaseOutFunc(float progress)
		{
			return Mathf.Sin(progress * ((float)Math.PI / 2f));
		}

		private static float SineEaseInOutFunc(float progress)
		{
			return (Mathf.Sin(progress * (float)Math.PI - (float)Math.PI / 2f) + 1f) / 2f;
		}

		private static float EaseInPower(float progress, int power)
		{
			return Mathf.Pow(progress, (float)power);
		}

		private static float EaseOutPower(float progress, int power)
		{
			int num = ((power % 2 != 0) ? 1 : (-1));
			return (float)num * (Mathf.Pow(progress - 1f, (float)power) + (float)num);
		}

		private static float EaseInOutPower(float progress, int power)
		{
			progress *= 2f;
			if (progress < 1f)
			{
				return Mathf.Pow(progress, (float)power) / 2f;
			}
			int num = ((power % 2 != 0) ? 1 : (-1));
			return (float)num / 2f * (Mathf.Pow(progress - 2f, (float)power) + (float)(num * 2));
		}
	}
	public enum TweenState
	{
		Running,
		Paused,
		Stopped
	}
	public enum TweenStopBehavior
	{
		DoNotModify,
		Complete
	}
	public class Vector2Tween : Tween<Vector2>
	{
		private static readonly Func<ITween<Vector2>, Vector2, Vector2, float, Vector2> LerpFunc = LerpVector2;

		private static Vector2 LerpVector2(ITween<Vector2> t, Vector2 start, Vector2 end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Vector2.Lerp(start, end, progress);
		}

		public Vector2Tween()
			: base(LerpFunc)
		{
		}
	}
	public class Vector3Tween : Tween<Vector3>
	{
		private static readonly Func<ITween<Vector3>, Vector3, Vector3, float, Vector3> LerpFunc = LerpVector3;

		private static Vector3 LerpVector3(ITween<Vector3> t, Vector3 start, Vector3 end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.Lerp(start, end, progress);
		}

		public Vector3Tween()
			: base(LerpFunc)
		{
		}
	}
	public class Vector4Tween : Tween<Vector4>
	{
		private static readonly Func<ITween<Vector4>, Vector4, Vector4, float, Vector4> LerpFunc = LerpVector4;

		private static Vector4 LerpVector4(ITween<Vector4> t, Vector4 start, Vector4 end, float progress)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return Vector4.Lerp(start, end, progress);
		}

		public Vector4Tween()
			: base(LerpFunc)
		{
		}
	}
}
