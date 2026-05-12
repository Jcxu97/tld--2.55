using ModSettings;
using UnityEngine;

namespace BetterCamera;

internal class Settings : JsonModSettings
{
	internal static Settings instance;

	[Section("照相机")]
	[Name("动态缩放功能")]
	[Description("使用相机时，启用滚轮进行拉近/推远缩放。默认=开启")]
	public bool dynazoom = true;

	[Name("滚动音效")]
	[Description("是否开启动态缩放时的滚轮音效。默认=开启")]
	public bool scrollsound = true;

	[Name("瞄准动画速度")]
	[Description("调整瞄准动画的播放速度。默认=1，修改后需重新加载场景方可生效")]
	[Slider(1f, 2f, 1)]
	public int aimspeed = 1;

	[Name("修改'开火'提示")]
	[Description("将取出相机时的'开火'提示更名为'拍照'。默认=开启，修改后需重新加载场景方可生效")]
	public bool tooltip = true;

	[Name("相机胶卷容量")]
	[Description("可调整相机内最大胶卷数量，需重新加载场景才能生效，默认=6")]
	[Slider(6f, 10f, 1)]
	public int clipsize = 6;

	[Name("允许卸载胶卷")]
	[Description("允许在物品栏里取出相机胶卷，需重新加载场景才能生效，默认=开启")]
	public bool unloading = true;

	[Section("照片")]
	[Name("启用保存照片弹窗")]
	[Description("是否显示照片保存的相关弹窗，默认=启用")]
	public bool popups = true;

	[Name("启用西瓜加载器日志")]
	[Description("是否将照片保存的日志信息与潜在错误输出到西瓜加载器的控制台，默认=启用")]
	public bool melonlogs = true;

	[Section("快捷键")]
	[Name("保存照片")]
	[Description("点击设置按键绑定，默认=P")]
	public KeyCode keyCode = (KeyCode)112;

	[Name("备用缩放键")]
	[Description("开启动态缩放时，除滚轮外用于放大的备用按键。默认：+=")]
	public KeyCode zoomin = (KeyCode)61;

	[Name("备用缩放键")]
	[Description("开启动态缩放时，除滚轮外用于缩小的备用按键。默认：-")]
	public KeyCode zoomout = (KeyCode)45;

	[Section("重置设置")]
	[Name("重置为默认")]
	[Description("将所有设置重置为默认值，必须点确认并切换场景或重新加载存档才能生效")]
	public bool ResetSettings;

	protected override void OnConfirm()
	{
		ApplyReset();
		instance.ResetSettings = false;
		base.OnConfirm();
		RefreshGUI();
	}

	public static void ApplyReset()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (instance.ResetSettings)
		{
			instance.melonlogs = true;
			instance.popups = true;
			instance.clipsize = 6;
			instance.aimspeed = 1;
			instance.unloading = true;
			instance.keyCode = (KeyCode)112;
			instance.ResetSettings = false;
			instance.zoomout = (KeyCode)45;
			instance.zoomin = (KeyCode)43;
			instance.dynazoom = true;
			instance.scrollsound = true;
			instance.tooltip = true;
		}
	}

	static Settings()
	{
		instance = new Settings();
	}
}
