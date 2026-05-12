using ModSettings;
using UnityEngine;

namespace NorthernLightsBroadcast;

internal class TVSettings : JsonModSettings
{
	[Section("按键")]
	[Name("互动键")]
	[Description("用于开关电视的按键。默认鼠标中键(Mouse2)")]
	public KeyCode interactButton = (KeyCode)325;

	[Section("播放")]
	[Name("连续播放文件夹")]
	[Description("当前文件播完后继续播放同文件夹的下一个文件")]
	public bool playFolder = true;

	[Name("循环播放文件夹")]
	[Description("当前文件夹的所有文件循环播放")]
	public bool loopFolder;

	[Section("调试")]
	[Name("调试模式")]
	[Description("调试输出。默认：关闭")]
	public bool disableStronks;

	protected override void OnConfirm()
	{
		base.OnConfirm();
	}
}
