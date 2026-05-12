using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using StretchArmstrong;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("StretchArmstrong")]
[assembly: AssemblyDescription("Change the range at which you can interact with things.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("StretchArmstrong")]
[assembly: AssemblyCopyright("Copyright © phaedrus 2023")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("f38004bd-0441-4b0f-809c-9af8a9ae16e7")]
[assembly: AssemblyFileVersion("2.1.0")]
[assembly: MelonInfo(typeof(Implementation), "StretchArmstrong", "2.1.0", "bushtail, phaedrus", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("2.1.0.0")]
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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
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
namespace StretchArmstrong
{
	public class Implementation : MelonMod
	{
		public override void OnApplicationStart()
		{
			Settings.onLoad();
			Debug.Log(Object.op_Implicit("[stretch_armstrong] loaded."));
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "ComputeModifiedPickupRange")]
	internal class PlayerManager_ComputeModifiedPickupRange
	{
		private static void Postfix(ref float __result)
		{
			__result *= Settings.option.modifier;
		}
	}
	internal class SASettings : JsonModSettings
	{
		[Name("互动距离倍数")]
		[Description("将调整后的拾取范围按倍数放大")]
		[Slider(0f, 5f)]
		public float modifier = 1f;
	}
	internal class Settings
	{
		public static SASettings option;

		public static void onLoad()
		{
			option = new SASettings();
			option.AddToModSettings("互动距离设置v2.1", MenuType.Both);
		}
	}
}
