using ModSettings;

namespace GearSpawner;

public sealed class Settings : JsonModSettings
{
	[Section("MOD物品散落生成概率")]
	[Name("朝圣者/极高概率")]
	[Description("该模式下，MOD物品散落生成概率。0是禁用MOD物品散落生成，但不影响容器内MOD物品的生成概率")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float pilgramSpawnProbabilityMultiplier = 1f;

	[Name("航行者/高概率")]
	[Description("该模式下，MOD物品散落生成概率。0是禁用MOD物品散落生成，但不影响容器内MOD物品的生成概率")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float voyagerSpawnProbabilityMultiplier = 0.6f;

	[Name("潜行者/中等概率")]
	[Description("该模式下，MOD物品散落生成概率。0是禁用MOD物品散落生成，但不影响容器内MOD物品的生成概率")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float stalkerSpawnProbabilityMultiplier = 0.4f;

	[Name("入侵者/低概率")]
	[Description("该模式下，MOD物品散落生成概率。0是禁用MOD物品散落生成，但不影响容器内MOD物品的生成概率")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float interloperSpawnProbabilityMultiplier = 0.2f;

	[Name("挑战模式")]
	[Description("该模式下，MOD物品散落生成概率。0是禁用MOD物品散落生成，但不影响容器内MOD物品的生成概率")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float challengeSpawnProbabilityMultiplier = 1f;

	[Section("进阶功能")]
	[Name("MOD物品生成100%刷出")]
	[Description("用于测试生成新的刷点，所有散落刷点成为必刷点，对现有存档不一定有效")]
	public bool alwaysSpawnItems;

	public static Settings instance { get; }

	static Settings()
	{
		instance = new Settings();
	}
}
