namespace NLBIni;

public interface IIniDataFormatter
{
	IniParserConfiguration Configuration { get; set; }

	string IniDataToString(IniData iniData);
}
