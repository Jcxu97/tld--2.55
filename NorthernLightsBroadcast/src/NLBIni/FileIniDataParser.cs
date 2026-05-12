using System;
using System.IO;
using System.Text;

namespace NLBIni;

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
			throw new ParsingException($"Could not parse file {filePath}", innerException);
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
