using ModSettings;

namespace ModComponent;

internal class Settings : JsonModSettings
{
	internal static Settings instance;

	[Name("随机塑料水瓶")]
	[Description("选是，搜刮到的塑料水瓶盛水容量是随机的")]
	public bool randomPlasticWaterBottles;

	[Name("禁用随机物品生成")]
	[Description("选否，物品生成将不在固定点刷出")]
	public bool disableRandomItemSpawns;

	[Section("附加功能")]
	[Name("报错调试")]
	[Description("西瓜加载器报错日志会显示额外的信息，有助于故障排除与调试")]
	public bool showDebugOutput;

	static Settings()
	{
		instance = new Settings();
	}
}
