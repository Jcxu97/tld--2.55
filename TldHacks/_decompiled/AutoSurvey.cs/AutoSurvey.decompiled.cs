using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using AutoSurvey;
using Il2Cpp;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(Main), "Auto Map Survey", "1.1.0", "SuperKlever", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyCompany("AutoSurvey")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+9d7cefd68b9e24f7c90c15be1fa4152b259fcb0f")]
[assembly: AssemblyProduct("AutoSurvey")]
[assembly: AssemblyTitle("AutoSurvey")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.0.0")]
[module: UnverifiableCode]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace AutoSurvey
{
	public class Main : MelonMod
	{
		public static float lastDrawTime;

		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
		}

		public override void OnUpdate()
		{
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			if (!GameManager.IsMainMenuActive() && Settings.Instance.autodrawEnabled && !InterfaceManager.IsPanelEnabled<Panel_Map>() && InterfaceManager.IsPanelLoaded<Panel_Map>() && (Settings.Instance.UnlockSurvey || CharcoalItem.HasSurveyVisibility(0f)))
			{
				lastDrawTime += Time.deltaTime;
				if (lastDrawTime >= Settings.Instance.autodrawDelay)
				{
					float num = Settings.Instance.drawingRange * 150f;
					InterfaceManager.GetPanel<Panel_Map>().DoNearbyDetailsCheck(num, true, false, GameManager.GetPlayerTransform().position, true);
					lastDrawTime = 0f;
				}
			}
		}
	}
	internal class Settings : JsonModSettings
	{
		[Section("设置")]
		[Name("自动勘测地图 (无需木炭)")]
		[Description("")]
		public bool autodrawEnabled;

		[Name("自动解锁地图延迟（秒）")]
		[Description("")]
		[Slider(1f, 120f, NumberFormat = "{0:F1}")]
		public float autodrawDelay = 10f;

		[Name("自动解锁地图范围倍率")]
		[Description("")]
		[Slider(0f, 10f, NumberFormat = "{0:F1}")]
		public float drawingRange = 1f;

		[Name("解锁勘测限制")]
		[Description("允许在任何天气下进行自动勘测")]
		public bool UnlockSurvey;

		internal static Settings Instance { get; }

		internal static void OnLoad()
		{
			Instance.AddToModSettings("自动开图v1.0");
			Instance.RefreshGUI();
		}

		static Settings()
		{
			Instance = new Settings();
		}
	}
}
