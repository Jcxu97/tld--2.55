using MelonLoader;

namespace GearSpawner;

internal sealed class GearSpawnerMod : MelonMod
{
	internal static Instance Logger { get; }

	public override void OnInitializeMelon()
	{
	}

	static GearSpawnerMod()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		Logger = new Instance("MOD物品散落生成(前置MOD)");
	}
}
