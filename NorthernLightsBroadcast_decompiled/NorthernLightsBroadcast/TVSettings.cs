using ModSettings;
using UnityEngine;

namespace NorthernLightsBroadcast;

internal class TVSettings : JsonModSettings
{
	[Section("Buttons")]
	[Name("Interact button")]
	[Description("Button to witch TV on/off")]
	public KeyCode interactButton = (KeyCode)325;

	[Section("Playback")]
	[Name("Play folder")]
	[Description("Continues playing all files after the last played file")]
	public bool playFolder = true;

	[Name("Loop folder")]
	[Description("Loops through all files inside the current folder")]
	public bool loopFolder;

	[Section("Debug")]
	[Name("Debug")]
	[Description("Debug mode. Default: Off")]
	public bool disableStronks;

	protected override void OnConfirm()
	{
		((JsonModSettings)this).OnConfirm();
	}
}
