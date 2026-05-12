using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using InfiniteFiresDLC;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("InfiniteFiresDLC")]
[assembly: AssemblyCopyright("Digitalzombie, ds5678, MooseMeat")]
[assembly: AssemblyFileVersion("1.5.0")]
[assembly: MelonInfo(typeof(InfiniteFiresDLCMain), "InfiniteFiresDLC", "1.5.0", "Digitalzombie, ds5678, MooseMeat", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.5.0.0")]
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
namespace InfiniteFiresDLC
{
	[RegisterTypeInIl2Cpp]
	public abstract class AlternativeAction : MonoBehaviour
	{
		public virtual void ExecutePrimary()
		{
		}

		public virtual void ExecuteSecondary()
		{
		}

		public virtual void ExecuteTertiary()
		{
		}

		public AlternativeAction(IntPtr intPtr)
			: base(intPtr)
		{
		}
	}
	public class AlternativeFireAction : AlternativeAction
	{
		public AlternativeFireAction(IntPtr intPtr)
			: base(intPtr)
		{
		}

		public override void ExecuteTertiary()
		{
			Fire component = ((Component)this).gameObject.GetComponent<Fire>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.m_IsPerpetual = !component.m_IsPerpetual;
				return;
			}
			component = ((Component)this).gameObject.GetComponentInChildren<Fire>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.m_IsPerpetual = !component.m_IsPerpetual;
				if (component.m_IsPerpetual)
				{
					int num;
					for (num = Random.Range(0, InfiniteFiresDLCMain.endlessTxt.Length - 1); num == InfiniteFiresDLCMain.lastMsg; num = Random.Range(0, InfiniteFiresDLCMain.endlessTxt.Length - 1))
					{
					}
					InfiniteFiresDLCMain.lastMsg = num;
					HUDMessage.AddMessage(InfiniteFiresDLCMain.endlessTxt[num], 5f, true, false);
				}
				else
				{
					HUDMessage.AddMessage("The fire will now consume fuel", 3f, true, false);
				}
			}
			else
			{
				MelonLogger.Error("Attached object doesn't have a fire component.");
			}
		}
	}
	public class InfiniteFiresDLCMain : MelonMod
	{
		public static int lastMsg;

		public static string[] endlessTxt;

		public override void OnInitializeMelon()
		{
			ClassInjector.RegisterTypeInIl2Cpp<AlternativeFireAction>();
			Settings.instance.AddToModSettings("核能之火v1.5");
		}

		static InfiniteFiresDLCMain()
		{
			lastMsg = 0;
			endlessTxt = new string[18]
			{
				"The only way to put out this endless fire is with an endless supply of water, or a giant marshmallow.", "Someone call the fire department! Oh wait, they already gave up on this one.", "This fire is like a never-ending party, except it's not as fun and nobody brought marshmallows.", "This fire will burn till the end of time.", "If this fire were a person, it would be the Energizer Bunny's pyromaniac cousin.", "I'm starting to think this fire has a secret stash of lighter fluid somewhere.", "This fire is so stubborn, it makes a mule look like a pushover.", "I heard this fire once took on a firefighter in arm-wrestling and won.", "This fire is like a bad habit, it just won't go away no matter how hard you try.", "You know it's a serious fire when even the sun gets jealous of its brightness.",
				"This fire is like a never-ending romance novel, but without the happy ending.", "This fire is so bright, it could probably light up a black hole.", "I heard this fire once went on vacation to Antarctica just to cool off, but even that didn't work.", "It's like this fire has a vendetta against the world and won't stop until everything is ash.", "This fire is so intense, it could probably melt steel, and your dreams.", "I bet this fire is what hell looks like, but just a preview, before things get even worse.", "This fire is like a monster that just keeps growing and getting stronger, feeding off the destruction it creates.", "I heard this fire once made a deal with the devil, and even he's scared of it now."
			};
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
	internal static class PlayerManager_InteractiveObjectsProcessInteraction
	{
		internal static void Prefix(PlayerManager __instance)
		{
			GameObject val = ((__instance != null) ? __instance.GetInteractiveObjectUnderCrosshairs(10f) : null);
			AlternativeAction alternativeAction = ((val != null) ? val.GetComponent<AlternativeAction>() : null) ?? ((val != null) ? val.GetComponentInChildren<AlternativeAction>() : null);
			if (!((Object)(object)alternativeAction == (Object)null))
			{
				alternativeAction.ExecutePrimary();
			}
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessAltFire")]
	internal static class PlayerManager_InteractiveObjectsProcessAltFire
	{
		internal static void Prefix(PlayerManager __instance)
		{
			GameObject val = ((__instance != null) ? __instance.GetInteractiveObjectUnderCrosshairs(10f) : null);
			AlternativeAction alternativeAction = ((val != null) ? val.GetComponent<AlternativeAction>() : null) ?? ((val != null) ? val.GetComponentInChildren<AlternativeAction>() : null);
			if (!((Object)(object)alternativeAction == (Object)null))
			{
				alternativeAction.ExecuteSecondary();
			}
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcess")]
	internal static class PlayerManager_InteractiveObjectsProcess
	{
		internal static void Postfix(PlayerManager __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.instance.tertiaryKeyCode))
			{
				GameObject val = ((__instance != null) ? __instance.GetInteractiveObjectUnderCrosshairs(10f) : null);
				AlternativeAction alternativeAction = ((val != null) ? val.GetComponent<AlternativeAction>() : null) ?? ((val != null) ? val.GetComponentInChildren<AlternativeAction>() : null);
				if (!((Object)(object)alternativeAction == (Object)null))
				{
					alternativeAction.ExecuteTertiary();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Fire), "Awake")]
	internal static class Fire_Awake
	{
		private static void Postfix(Fire __instance)
		{
			if (((Component)__instance).GetComponent<AlternativeFireAction>() == null)
			{
				((Component)__instance).gameObject.AddComponent<AlternativeFireAction>();
			}
		}
	}
	[HarmonyPatch(typeof(WoodStove), "Awake")]
	internal static class WoodStove_Awake
	{
		private static void Postfix(WoodStove __instance)
		{
			if (((Component)__instance).GetComponent<AlternativeFireAction>() == null)
			{
				((Component)__instance).gameObject.AddComponent<AlternativeFireAction>();
			}
		}
	}
	internal class Settings : JsonModSettings
	{
		internal static Settings instance;

		[Name("第三方执行键(核能之火)")]
		[Description("用于执行第三方操作的按键，在这里设置核能之火的快捷键")]
		public KeyCode tertiaryKeyCode = (KeyCode)325;

		static Settings()
		{
			instance = new Settings();
		}
	}
}
