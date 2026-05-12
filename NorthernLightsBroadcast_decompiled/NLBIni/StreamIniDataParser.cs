using System;
using System.IO;

namespace NLBIni;

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
