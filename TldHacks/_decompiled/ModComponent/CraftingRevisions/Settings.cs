using ModSettings;

namespace CraftingRevisions;

public class Settings : JsonModSettings
{
	public static Settings instance;

	[Name("工艺菜单默认快捷项")]
	[Description("前置MOD，必须安装，否则蓝图模组无效！工作台蓝图子菜单左列默认选取。默认=7")]
	[Slider(1f, 7f)]
	public int numCraftingSteps = 1;

	static Settings()
	{
		instance = new Settings();
	}
}
