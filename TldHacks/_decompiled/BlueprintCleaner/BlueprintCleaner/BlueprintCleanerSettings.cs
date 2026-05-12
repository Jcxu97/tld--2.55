using ModSettings;
using UnityEngine;

namespace BlueprintCleaner;

internal class BlueprintCleanerSettings : JsonModSettings
{
	[Name("切换快捷键")]
	[Description("按H键可实现在原始工艺界面与优化工艺界面之间的自由切换")]
	public KeyCode viewKey = (KeyCode)104;

	[Name("长按热键以隐藏或显示蓝图")]
	[Description("左ALT+鼠标左键，可将目标蓝图从工艺菜单中隐藏或显示为默认。注意：隐藏蓝图后必须按H键才能看到被隐藏为红色高亮的蓝图")]
	public KeyCode HoldKey = (KeyCode)308;

	[Name("切换到下一个蓝图")]
	[Description("当蓝图为堆叠状态时，按右方向键可切换到下一个蓝图")]
	public KeyCode rightKey = (KeyCode)275;

	[Name("切换到上一个蓝图")]
	[Description("当蓝图为堆叠状态时，按左方向键可切换到上一个蓝图")]
	public KeyCode leftKey = (KeyCode)276;
}
