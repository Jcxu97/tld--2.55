using System.Reflection;
using ModSettings;
using UnityEngine;

namespace MotionTracker;

internal class MotionTrackerSettings : JsonModSettings
{
	[Section("常用功能")]
	[Name("启用动物雷达追踪")]
	[Description("启用或禁用")]
	public bool enableMotionTracker = true;

	[Name("雷达UI可见性")]
	[Description("始终可见 / 热键切换可见")]
	public Settings.DisplayStyle displayStyle;

	[Name("热键")]
	[Description("使用热键切换来实现可见")]
	public KeyCode toggleKey = (KeyCode)256;

	[Name("仅室外生效")]
	[Description("只有在室外时才显示雷达追踪")]
	public bool onlyOutdoors = true;

	[Name("探测范围")]
	[Description("探测动物的范围")]
	[Slider(0f, 800f)]
	public int detectionRange = 100;

	[Name("雷达界面大小")]
	[Description("左上角雷达界面大小")]
	[Slider(0f, 4f)]
	public float scale = 1f;

	[Name("雷达界面的透明度")]
	[Description("雷达界面的透明程度")]
	[Slider(0f, 1f)]
	public float opacity = 0.7f;

	[Section("喷漆")]
	[Name("显示喷漆标记")]
	[Description("启用/禁用")]
	public bool showSpraypaint = true;

	[Name("喷漆图标大小")]
	[Description("雷达上的图标大小")]
	[Slider(0.2f, 5f)]
	public float spraypaintScale = 2f;

	[Name("喷漆图标透明度")]
	[Description("喷漆图标的透明程度")]
	[Slider(0f, 1f)]
	public float spraypaintOpacity = 0.8f;

	[Section("野生动物")]
	[Name("动物图标大小")]
	[Description("雷达上的图标大小")]
	[Slider(0f, 5f)]
	public float animalScale = 3.5f;

	[Name("动物图标透明度")]
	[Description("雷达上动物图标的透明度")]
	[Slider(0f, 1f)]
	public float animalOpacity = 0.8f;

	[Name("显示乌鸦")]
	[Description("追踪乌鸦")]
	public bool showCrows = true;

	[Name("显示兔子")]
	[Description("追踪兔子")]
	public bool showRabbits = true;

	[Name("显示雄鹿")]
	[Description("追踪公鹿")]
	public bool showStags = true;

	[Name("显示雌鹿")]
	[Description("追踪母鹿")]
	public bool showDoes = true;

	[Name("显示普通狼")]
	[Description("追踪普通狼")]
	public bool showWolves = true;

	[Name("显示森林狼")]
	[Description("追踪森林狼")]
	public bool showTimberwolves = true;

	[Name("显示熊")]
	[Description("追踪熊")]
	public bool showBears = true;

	[Name("显示美洲狮")]
	[Description("追踪美洲狮")]
	public bool showCougars = true;

	[Name("显示驼鹿")]
	[Description("追踪驼鹿")]
	public bool showMoose = true;

	[Name("显示松鸡")]
	[Description("追踪松鸡")]
	public bool showPuffyBirds = true;

	[Section("装备")]
	[Name("装备图标大小")]
	[Description("雷达上显示装备图标的大小")]
	[Slider(0f, 5f)]
	public float gearScale = 3.5f;

	[Name("装备图标透明度")]
	[Description("雷达上显示装备图标的透明度")]
	[Slider(0f, 1f)]
	public float gearOpacity = 0.8f;

	[Name("显示箭矢")]
	[Description("雷达上显示箭矢位置")]
	public bool showArrows = true;

	[Name("显示煤炭")]
	[Description("雷达上显示煤炭位置")]
	public bool showCoal = true;

	[Name("显示生鱼")]
	[Description("雷达上显示生鱼位置")]
	public bool showRawFish = true;

	[Name("显示失物招领箱")]
	[Description("雷达上显示失物招领箱位置")]
	public bool showLostAndFoundBox = true;

	[Name("显示盐矿点")]
	[Description("雷达上显示盐矿位置")]
	public bool showSaltDeposit = true;

	[Name("显示赶海物资")]
	[Description("雷达上显示赶海拾荒物资")]
	public bool showBeachLoot = true;

	protected override void OnChange(FieldInfo field, object oldValue, object newValue)
	{
	}

	protected override void OnConfirm()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		base.OnConfirm();
		if (Object.op_Implicit((Object)(object)PingManager.instance))
		{
			PingManager.instance.SetOpacity(Settings.options.opacity);
			PingManager.instance.Scale(Settings.options.scale);
			Settings.animalScale = new Vector3(Settings.options.animalScale, Settings.options.animalScale, Settings.options.animalScale);
			Settings.spraypaintScale = new Vector3(Settings.options.spraypaintScale, Settings.options.spraypaintScale, Settings.options.spraypaintScale);
			Settings.gearScale = new Vector3(Settings.options.gearScale, Settings.options.gearScale, Settings.options.gearScale);
			Settings.animalColor = new Color(1f, 1f, 1f, Settings.options.animalOpacity);
			Settings.gearColor = new Color(1f, 1f, 1f, Settings.options.gearOpacity);
			Settings.spraypaintColor = new Color(0.62f, 0.29f, 0f, Settings.options.spraypaintOpacity);
		}
	}
}
