using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.Interactions;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using QoL;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("QoL")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("QoL")]
[assembly: AssemblyCopyright("Copyright ©  2025")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("c51fb8b6-3d53-4ed9-a340-cab2215de3b9")]
[assembly: AssemblyFileVersion("1.7.6")]
[assembly: MelonInfo(typeof(Implementation), "QoL", "1.7.6", "BA", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.7.6.0")]
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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
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
namespace QoL
{
	[HarmonyPatch(typeof(InputManager), "GetEscapePressed")]
	internal class AlternativeEscape
	{
		private static bool Postfix(bool __result)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (!__result && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.backKey))
			{
				__result = true;
			}
			return __result;
		}
	}
	[HarmonyPatch(typeof(InputManager), "GetInteractPressed")]
	internal class AlternativeInteract
	{
		private static bool Postfix(bool __result)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (!__result && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__result = true;
			}
			return __result;
		}
	}
	[HarmonyPatch(typeof(InputManager), "GetPickupPressed")]
	internal class AlternativePickup
	{
		private static bool Postfix(bool __result)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (!__result && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__result = true;
			}
			return __result;
		}
	}
	[HarmonyPatch(typeof(InputManager), "GetPutBackPressed")]
	internal class AlternativePutBack
	{
		private static bool Postfix(bool __result)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (!__result && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.backKey))
			{
				__result = true;
			}
			return __result;
		}
	}
	[HarmonyPatch(typeof(Container), "Awake")]
	internal class ContainerWeightLimitPatch
	{
		private static void Postfix(Container __instance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			ItemWeight val = __instance.m_Capacity * Settings.options.containerWeightLimitScale;
			float num = ((ItemWeight)(ref val)).ToQuantity(1f);
			num = (float)Math.Round(num, 2);
			__instance.m_Capacity = ItemWeight.FromKilograms(num);
		}
	}
	[HarmonyPatch(typeof(Container), "BeginContainerOpen")]
	internal static class DynamicSearchTime
	{
		private static void Prefix(Container __instance)
		{
			if (!Settings.options.containterTimeTweak)
			{
				return;
			}
			ContainerInteraction component = ((Component)__instance).GetComponent<ContainerInteraction>();
			if (!((Object)(object)component == (Object)null))
			{
				float holdTime = ((TimedHoldInteraction)component).HoldTime;
				if (__instance.IsInspected())
				{
					((TimedHoldInteraction)component).HoldTime = ((TimedHoldInteraction)component).HoldTime * Settings.options.containterOpenTimeScale;
					return;
				}
				((TimedHoldInteraction)component).HoldTime = Mathf.Clamp(holdTime * Settings.options.containterSearchTimeScalePerItem * (float)__instance.m_GearToInstantiate.Count, holdTime * Settings.options.containterSearchTimeScaleMin, holdTime * Settings.options.containterSearchTimeScaleMax);
				Random random = new Random();
				((TimedHoldInteraction)component).HoldTime = ((TimedHoldInteraction)component).HoldTime * (1f + (float)random.Next(-100, 100) * Settings.options.containterSearchTimeScaleVar / 100f);
			}
		}
	}
	internal class EAPISupport
	{
		internal delegate void VoidDelegate();

		internal static EAPISupport Instance { get; set; }

		public Type EapiType { get; }

		internal VoidDelegate OnPerformSelectedAction { get; }

		internal VoidDelegate OnNextSubAction { get; }

		internal VoidDelegate OnPreviousSubAction { get; }

		public EAPISupport(Type eapiType)
		{
			EapiType = eapiType;
			MethodInfo methodInfo = AccessTools.FirstMethod(EapiType, (Func<MethodInfo, bool>)((MethodInfo mi) => mi.Name == "OnPerformSelectedAction"));
			OnPerformSelectedAction = (VoidDelegate)methodInfo.CreateDelegate(typeof(VoidDelegate), AccessTools.FirstProperty(EapiType, (Func<PropertyInfo, bool>)((PropertyInfo pi) => pi.Name == "Instance")).GetValue(null));
			methodInfo = AccessTools.FirstMethod(EapiType, (Func<MethodInfo, bool>)((MethodInfo mi) => mi.Name == "OnNextSubAction"));
			OnNextSubAction = (VoidDelegate)methodInfo.CreateDelegate(typeof(VoidDelegate), AccessTools.FirstProperty(EapiType, (Func<PropertyInfo, bool>)((PropertyInfo pi) => pi.Name == "Instance")).GetValue(null));
			methodInfo = AccessTools.FirstMethod(EapiType, (Func<MethodInfo, bool>)((MethodInfo mi) => mi.Name == "OnPreviousSubAction"));
			OnPreviousSubAction = (VoidDelegate)methodInfo.CreateDelegate(typeof(VoidDelegate), AccessTools.FirstProperty(EapiType, (Func<PropertyInfo, bool>)((PropertyInfo pi) => pi.Name == "Instance")).GetValue(null));
		}
	}
	public class Implementation : MelonMod
	{
		public const string VERSION = "1.7.6";

		internal static LegacyInput IM { get; private set; }

		public override void OnInitializeMelon()
		{
			MelonLogger.Msg($"[{((MelonBase)this).Info.Name}] Version {((MelonBase)this).Info.Version} loaded!");
			Settings.OnLoad();
		}

		public override void OnLateInitializeMelon()
		{
			IM = new LegacyInput();
			Type type = Type.GetType("ExamineActionsAPI.ExamineActionsAPI, ExamineActionsAPI");
			if (type != null)
			{
				EAPISupport.Instance = new EAPISupport(type);
			}
		}
	}
	internal sealed class LegacyInput
	{
		private readonly PropertyInfo m_mousePositionProp;

		private readonly PropertyInfo m_mouseScrollDeltaProp;

		private readonly MethodInfo m_getKeyMethod;

		private readonly MethodInfo m_getKeyDownMethod;

		private readonly MethodInfo m_getKeyUpMethod;

		private readonly MethodInfo m_getMouseButtonMethod;

		private readonly MethodInfo m_getMouseButtonDownMethod;

		private readonly MethodInfo m_getMouseButtonUpMethod;

		public Type TInput { get; }

		public Vector2 MousePosition => Vector2.op_Implicit(((Vector3?)m_mousePositionProp.GetValue(null, null)) ?? throw new NullReferenceException());

		public Vector2 MouseScrollDelta => ((Vector2?)m_mouseScrollDeltaProp.GetValue(null, null)) ?? throw new NullReferenceException();

		public LegacyInput()
		{
			ReflectionHelpers.LoadModule("UnityEngine.InputLegacyModule");
			TInput = ReflectionHelpers.GetTypeByName("UnityEngine.Input") ?? throw new NullReferenceException("TInput");
			m_mousePositionProp = TInput.GetProperty("mousePosition") ?? throw new NullReferenceException("m_mousePositionProp");
			m_mouseScrollDeltaProp = TInput.GetProperty("mouseScrollDelta") ?? throw new NullReferenceException("m_mouseScrollDeltaProp");
			m_getKeyMethod = TInput.GetMethod("GetKey", new Type[1] { typeof(KeyCode) }) ?? throw new NullReferenceException("m_getKeyMethod");
			m_getKeyDownMethod = TInput.GetMethod("GetKeyDown", new Type[1] { typeof(KeyCode) }) ?? throw new NullReferenceException("m_getKeyDownMethod");
			m_getKeyUpMethod = TInput.GetMethod("GetKeyUp", new Type[1] { typeof(KeyCode) }) ?? throw new NullReferenceException("m_getKeyUpMethod");
			m_getMouseButtonMethod = TInput.GetMethod("GetMouseButton", new Type[1] { typeof(int) }) ?? throw new NullReferenceException("m_getMouseButtonMethod");
			m_getMouseButtonDownMethod = TInput.GetMethod("GetMouseButtonDown", new Type[1] { typeof(int) }) ?? throw new NullReferenceException("m_getMouseButtonDownMethod");
			m_getMouseButtonUpMethod = TInput.GetMethod("GetMouseButtonUp", new Type[1] { typeof(int) }) ?? throw new NullReferenceException("m_getMouseButtonUpMethod");
		}

		public bool GetKey(KeyCode key)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return ((bool?)m_getKeyMethod.Invoke(null, new object[1] { key })) ?? throw new NullReferenceException();
		}

		public bool GetKeyDown(KeyCode key)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return ((bool?)m_getKeyDownMethod.Invoke(null, new object[1] { key })) ?? throw new NullReferenceException();
		}

		public bool GetKeyUp(KeyCode key)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return ((bool?)m_getKeyUpMethod.Invoke(null, new object[1] { key })) ?? throw new NullReferenceException();
		}

		public bool GetMouseButton(int btn)
		{
			return ((bool?)m_getMouseButtonMethod.Invoke(null, new object[1] { btn })) ?? throw new NullReferenceException();
		}

		public bool GetMouseButtonDown(int btn)
		{
			return ((bool?)m_getMouseButtonDownMethod.Invoke(null, new object[1] { btn })) ?? throw new NullReferenceException();
		}

		public bool GetMouseButtonUp(int btn)
		{
			return ((bool?)m_getMouseButtonUpMethod.Invoke(null, new object[1] { btn })) ?? throw new NullReferenceException();
		}
	}
	internal sealed class InputSystem
	{
		private readonly PropertyInfo m_btnIsPressedProp;

		private readonly PropertyInfo m_btnWasPressedProp;

		private readonly PropertyInfo m_btnWasReleasedProp;

		private readonly PropertyInfo m_kbCurrentProp;

		private readonly PropertyInfo m_kbIndexer;

		private readonly PropertyInfo m_positionProp;

		private readonly MethodInfo m_readVector2InputMethod;

		internal Dictionary<string, string> enumNameFixes = new Dictionary<string, string>
		{
			{ "Control", "Ctrl" },
			{ "Return", "Enter" },
			{ "Alpha", "Digit" },
			{ "Keypad", "Numpad" },
			{ "Numlock", "NumLock" },
			{ "Print", "PrintScreen" },
			{ "BackQuote", "Backquote" }
		};

		public Type TKeyboard { get; }

		public Type TMouse { get; }

		public Type TKey { get; }

		private object CurrentKeyboard { get; }

		private object CurrentMouse { get; }

		private object LeftMouseButton { get; }

		private object RightMouseButton { get; }

		private object MiddleMouseButton { get; }

		private object MouseScrollInfo { get; }

		private object MousePositionInfo { get; }

		internal Dictionary<KeyCode, object> ActualKeyDict { get; } = new Dictionary<KeyCode, object>();


		public Vector2 MousePosition
		{
			get
			{
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					return ((Vector2?)m_readVector2InputMethod.Invoke(MousePositionInfo, Array.Empty<object>())) ?? throw new NullReferenceException();
				}
				catch
				{
					return Vector2.zero;
				}
			}
		}

		public Vector2 MouseScrollDelta
		{
			get
			{
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					return ((Vector2?)m_readVector2InputMethod.Invoke(MouseScrollInfo, Array.Empty<object>())) ?? throw new NullReferenceException();
				}
				catch
				{
					return Vector2.zero;
				}
			}
		}

		public InputSystem()
		{
			MelonLogger.Msg($"Loading Unity.InputSystem: {ReflectionHelpers.LoadModule("Unity.InputSystem")}");
			TKeyboard = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Keyboard") ?? throw new NullReferenceException("TKeyboard");
			TMouse = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Mouse") ?? throw new NullReferenceException("TMouse");
			TKey = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Key") ?? throw new NullReferenceException("TKey");
			m_kbCurrentProp = TKeyboard.GetProperty("current") ?? throw new NullReferenceException("m_kbCurrentProp");
			CurrentKeyboard = m_kbCurrentProp.GetValue(null, null) ?? throw new NullReferenceException("CurrentKeyboard");
			m_kbIndexer = TKeyboard.GetProperty("Item", new Type[1] { TKey }) ?? throw new NullReferenceException("m_kbIndexer");
			Type type = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Controls.ButtonControl") ?? throw new NullReferenceException("btnControl");
			m_btnIsPressedProp = type.GetProperty("isPressed") ?? throw new NullReferenceException("m_btnIsPressedProp");
			m_btnWasPressedProp = type.GetProperty("wasPressedThisFrame") ?? throw new NullReferenceException("m_btnWasPressedProp");
			m_btnWasReleasedProp = type.GetProperty("wasReleasedThisFrame") ?? throw new NullReferenceException("m_btnWasReleasedProp");
			PropertyInfo propertyInfo = TMouse.GetProperty("current") ?? throw new NullReferenceException("m_mouseCurrentProp");
			CurrentMouse = propertyInfo.GetValue(null, null) ?? throw new NullReferenceException("CurrentMouse");
			PropertyInfo propertyInfo2 = TMouse.GetProperty("leftButton") ?? throw new NullReferenceException("m_leftButtonProp");
			LeftMouseButton = propertyInfo2.GetValue(CurrentMouse, null) ?? throw new NullReferenceException("LeftMouseButton");
			PropertyInfo propertyInfo3 = TMouse.GetProperty("rightButton") ?? throw new NullReferenceException("m_rightButtonProp");
			RightMouseButton = propertyInfo3.GetValue(CurrentMouse, null) ?? throw new NullReferenceException("RightMouseButton");
			PropertyInfo propertyInfo4 = TMouse.GetProperty("middleButton") ?? throw new NullReferenceException("m_middleButtonProp");
			MiddleMouseButton = propertyInfo4.GetValue(CurrentMouse, null) ?? throw new NullReferenceException("MiddleMouseButton");
			PropertyInfo propertyInfo5 = TMouse.GetProperty("scroll") ?? throw new NullReferenceException("m_kbCurrentProp");
			MouseScrollInfo = propertyInfo5.GetValue(CurrentMouse, null) ?? throw new NullReferenceException("MouseScrollInfo");
			m_positionProp = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Pointer")?.GetProperty("position") ?? throw new NullReferenceException("m_kbCurrentProp");
			MousePositionInfo = m_positionProp.GetValue(CurrentMouse, null) ?? throw new NullReferenceException("MousePositionInfo");
			m_readVector2InputMethod = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.InputControl`1")?.MakeGenericType(typeof(Vector2)).GetMethod("ReadValue") ?? throw new NullReferenceException("m_kbCurrentProp");
		}

		internal object GetActualKey(KeyCode key)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			if (!ActualKeyDict.ContainsKey(key))
			{
				string s = ((object)(KeyCode)(ref key)).ToString();
				try
				{
					KeyValuePair<string, string> keyValuePair = enumNameFixes.First<KeyValuePair<string, string>>((KeyValuePair<string, string> it) => s.Contains(it.Key));
					s = s.Replace(keyValuePair.Key, keyValuePair.Value);
				}
				catch
				{
				}
				object obj2 = Enum.Parse(TKey, s);
				object value = m_kbIndexer.GetValue(CurrentKeyboard, new object[1] { obj2 }) ?? throw new NullReferenceException();
				ActualKeyDict.Add(key, value);
			}
			return ActualKeyDict[key];
		}

		public bool GetKeyDown(KeyCode key)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return ((bool?)m_btnWasPressedProp.GetValue(GetActualKey(key), null)) ?? throw new NullReferenceException();
		}

		public bool GetKey(KeyCode key)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return ((bool?)m_btnIsPressedProp.GetValue(GetActualKey(key), null)) ?? throw new NullReferenceException();
		}

		public bool GetKeyUp(KeyCode key)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return ((bool?)m_btnWasReleasedProp.GetValue(GetActualKey(key), null)) ?? throw new NullReferenceException();
		}

		public bool GetMouseButtonDown(int btn)
		{
			return (byte)((btn switch
			{
				0 => (bool?)m_btnWasPressedProp.GetValue(LeftMouseButton, null), 
				1 => (bool?)m_btnWasPressedProp.GetValue(RightMouseButton, null), 
				2 => (bool?)m_btnWasPressedProp.GetValue(MiddleMouseButton, null), 
				_ => throw new NotImplementedException(), 
			}) ?? throw new NullReferenceException()) != 0;
		}

		public bool GetMouseButton(int btn)
		{
			return (byte)((btn switch
			{
				0 => (bool?)m_btnIsPressedProp.GetValue(LeftMouseButton, null), 
				1 => (bool?)m_btnIsPressedProp.GetValue(RightMouseButton, null), 
				2 => (bool?)m_btnIsPressedProp.GetValue(MiddleMouseButton, null), 
				_ => throw new NotImplementedException(), 
			}) ?? throw new NullReferenceException()) != 0;
		}

		public bool GetMouseButtonUp(int btn)
		{
			return (byte)((btn switch
			{
				0 => (bool?)m_btnWasReleasedProp.GetValue(LeftMouseButton, null), 
				1 => (bool?)m_btnWasReleasedProp.GetValue(RightMouseButton, null), 
				2 => (bool?)m_btnWasReleasedProp.GetValue(MiddleMouseButton, null), 
				_ => throw new NotImplementedException(), 
			}) ?? throw new NullReferenceException()) != 0;
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "UpdateInspectGear")]
	internal class FastPickAndAlternativeTakeIt
	{
		public static int LastTriggerFrameConsumed { get; private set; }

		public static float LastFastPickTriggered { get; private set; }

		private static void Postfix(PlayerManager __instance)
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			GearItem val = ((__instance != null) ? __instance.GearItemBeingInspected() : null);
			if ((Object)(object)val == (Object)null || !__instance.IsInspectModeActive() || GearItemPreInspect.LastTriggerFrame <= LastTriggerFrameConsumed || GearItemPreInspect.LastInspectType != 0)
			{
				return;
			}
			float num = Time.unscaledTime - GearItemPreInspect.LastInspect;
			bool flag = false;
			if (!flag)
			{
				flag = (InputManager.GetFireReleased(InputManager.m_CurrentContext) || Implementation.IM.GetKeyUp(Settings.options.interactKey)) && num > Settings.options.fastPickMin && num < Settings.options.fastPickMax;
			}
			if (flag)
			{
				if ((Object)(object)((Component)val).GetComponent<TravoisItem>() != (Object)null)
				{
					__instance.OnPickupFromTravoisInspection();
					__instance.ExitInspectGearMode(false);
				}
				else
				{
					__instance.ProcessPickupItemInteraction(val, false, false, false);
					__instance.ExitInspectGearMode(false);
				}
				LastFastPickTriggered = Time.unscaledTime;
				LastTriggerFrameConsumed = GearItemPreInspect.LastTriggerFrame;
			}
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "EnterInspectGearMode", new Type[]
	{
		typeof(GearItem),
		typeof(Container),
		typeof(IceFishingHole),
		typeof(Harvestable),
		typeof(CookingPotItem)
	})]
	internal class GearItemPreInspect
	{
		public enum InspectType
		{
			None,
			Container,
			IceFishingHole,
			Harvestable,
			CookingPot
		}

		public static int LastTriggerFrame { get; private set; }

		public static float LastInspect { get; private set; }

		public static InspectType LastInspectType { get; private set; }

		private static void Postfix(GearItem __0, Container __1, IceFishingHole __2, Harvestable __3, CookingPotItem __4)
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			LastInspectType = InspectType.None;
			if ((Object)(object)__1 != (Object)null)
			{
				LastInspectType = InspectType.Container;
			}
			if ((Object)(object)__2 != (Object)null)
			{
				LastInspectType = InspectType.IceFishingHole;
			}
			if ((Object)(object)__3 != (Object)null)
			{
				LastInspectType = InspectType.Harvestable;
			}
			if ((Object)(object)__4 != (Object)null)
			{
				LastInspectType = InspectType.CookingPot;
			}
			LastInspect = Time.unscaledTime;
			if (Implementation.IM.GetKey(Settings.options.interactKey) || InputManager.GetFirePressed(InputManager.m_CurrentContext))
			{
				LastTriggerFrame = Time.frameCount;
			}
		}
	}
	internal static class ReflectionHelpers
	{
		public const BindingFlags CommonFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public static Type? GetTypeByName(string fullName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				foreach (Type item in assemblies[i].TryGetTypes())
				{
					if (item.FullName == fullName)
					{
						return item;
					}
				}
			}
			return null;
		}

		public static void SearchTypesAndDo(string keyworld, Action<Type> action)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				foreach (Type item in assemblies[i].TryGetTypes())
				{
					if (item.FullName.Contains(keyworld))
					{
						action(item);
					}
				}
			}
		}

		private static IEnumerable<Type> TryGetTypes(this Assembly asm)
		{
			try
			{
				return asm.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				try
				{
					return asm.GetExportedTypes();
				}
				catch
				{
					return ex.Types.Where((Type t) => t != null);
				}
			}
			catch
			{
				return Enumerable.Empty<Type>();
			}
		}

		internal static void TryLoadGameModules()
		{
			LoadModule("Il2CppAssembly-CSharp");
			LoadModule("Il2CppAssembly-CSharp-firstpass");
		}

		public static bool LoadModule(string module)
		{
			return LoadModuleInternal("MelonLoader\\Il2CppAssemblies\\" + module + ".dll");
		}

		internal static bool LoadModuleInternal(string fullPath)
		{
			if (!File.Exists(fullPath))
			{
				return false;
			}
			try
			{
				Assembly.Load(File.ReadAllBytes(fullPath));
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.GetType()?.ToString() + ", " + ex.Message);
			}
			return false;
		}
	}
	internal class QoLSettings : JsonModSettings
	{
		[Name("备用键(后退/取消)")]
		[Description("类似ESC功能，比如在游戏里你想取消某个互动操作时可以使用该按键。默认=鼠标中间")]
		public KeyCode backKey = (KeyCode)326;

		[Name("备用互动键")]
		[Description("大多数情况下可当作为互动键使用(游戏默认鼠标左键)，也可在诸多UI中作为确认按钮。默认=insert")]
		public KeyCode interactKey = (KeyCode)277;

		[Name("堆叠快捷键")]
		[Description("在库存与容器之间转移物品时，左Shift+备用互动键即可实现一键转移，也适用于快速丢弃。默认=左Shift")]
		public KeyCode bulkKey = (KeyCode)304;

		[Name("丢弃快捷键")]
		[Description("丢弃当前所选的库存物品(默认=Q),堆叠物品使用左Ctrl+Q即可实现一键丢弃")]
		public KeyCode dropKey = (KeyCode)113;

		[Name("组合键")]
		[Description("按住这个键来修改其他功能，在执行快速丢弃时按住组合键(左Shift)+Q可将堆叠物品一键放置到库存外。默认=LCtrl")]
		public KeyCode modifierKey = (KeyCode)306;

		[Name("快速拾取持续时间下限")]
		[Description("对某个物品按住备用互动键拾取的过程是查看物品界面然后放入库存，这个时间是可以设置的。默认保持在(0.3-1秒之间)，低于或高于这个时间段不会触发放入库存这个动作")]
		[Slider(0.25f, 0.6f)]
		public float fastPickMin = 0.3f;

		[Name("快速拾取持续时间上限")]
		[Description("对某个物品按住备用互动键拾取的过程是查看物品界面然后放入库存，这个时间是可以设置的。默认保持在(0.3-1秒之间)，低于或高于这个时间段不会触发放入库存这个动作")]
		[Slider(0.8f, 2f)]
		public float fastPickMax = 1f;

		[Name("垃圾桶回收站")]
		[Description("不需要的垃圾直接扔垃圾桶。类似删除功能。该功能已兼容自定义安全屋模组的垃圾桶功能")]
		public bool voidTrashCan;

		[Name("容器时间修改")]
		[Description("打开或搜索容器的时间修改功能")]
		public bool containterTimeTweak;

		[Name("打开已搜索容器的时间")]
		[Description("打开已搜索容器的时间修改")]
		public float containterOpenTimeScale = 0.5f;

		[Name("搜索容器内每项物品的时间")]
		[Description("缩短了搜索容器的时间。如默认情况下，存储有3个物品的容器将花费默认时间的60%来搜索")]
		public float containterSearchTimeScalePerItem = 0.2f;

		[Name("搜索容器的时间下限")]
		[Description("搜索容器的时间最小值")]
		[Slider(0.1f, 1f)]
		public float containterSearchTimeScaleMin = 0.1f;

		[Name("搜索容器的时间上限")]
		[Description("搜索容器的时间最大值")]
		[Slider(0.4f, 3f)]
		public float containterSearchTimeScaleMax = 1f;

		[Name("搜索容器的时间差")]
		[Description("搜索容器所花费的时间差具备随机性")]
		[Slider(0f, 1f)]
		public float containterSearchTimeScaleVar = 0.2f;

		[Name("容器重量上限")]
		[Description("容器重量上限倍率，为新增功能，可能与多功能实用模组的容器容量修改有冲突，最好别用！保持默认。其他冲突未知！")]
		[Slider(0.5f, 5f, 46)]
		public float containerWeightLimitScale = 1f;

		[Name("容器格子数量")]
		[Description("每公斤重量限制下，容器内可使用的格子数量。设置为0=禁用该功能。")]
		[Slider(0f, 10f, 101)]
		public float containerSlotLimitRatio;
	}
	internal static class Settings
	{
		internal static QoLSettings options;

		public static void OnLoad()
		{
			options = new QoLSettings();
			options.AddToModSettings("微操模组v1.7.6");
		}
	}
	[HarmonyPatch(typeof(Panel_ActionPicker), "Update")]
	internal class UIQoLActionPicker
	{
		private static void Postfix(Panel_ActionPicker __instance)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!__instance.m_GenericProgressBar.IsEnabled())
			{
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
				{
					__instance.OnSelect(__instance.m_SelectedIndex);
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) || InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
				{
					__instance.PreviousItem();
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
				{
					__instance.NextItem();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_ActionPicker), "EnableWithCurrentList")]
	internal class AcitonPickerResetOnEnable
	{
		private static void Postfix(Panel_ActionPicker __instance)
		{
			__instance.NextItem();
			__instance.PreviousItem();
		}
	}
	[HarmonyPatch(typeof(Panel_Affliction), "Update")]
	internal class UIQoLAffliction
	{
		private static void Postfix(Panel_Affliction __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.TreatWound();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) || InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
			{
				__instance.PreviousAffliction();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
			{
				__instance.NextAffliction();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_BodyHarvest), "Update")]
	internal class UIQoLBodyHarvest
	{
		private static void Postfix(ref Panel_BodyHarvest __instance)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.IsHarvestingOrQuartering())
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.bulkKey) && __instance.IsTabHarvestSelected() && __instance.m_SelectedButtonIndex == 0)
			{
				__instance.OnIncreaseMeatHarvest();
				__instance.OnIncreaseMeatHarvest();
				__instance.OnIncreaseMeatHarvest();
				__instance.OnIncreaseMeatHarvest();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.bulkKey) && __instance.IsTabHarvestSelected() && __instance.m_SelectedButtonIndex == 0)
			{
				__instance.OnDecreaseMeatHarvest();
				__instance.OnDecreaseMeatHarvest();
				__instance.OnDecreaseMeatHarvest();
				__instance.OnDecreaseMeatHarvest();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.bulkKey) && __instance.IsTabHarvestSelected() && __instance.m_SelectedButtonIndex == 2)
			{
				__instance.OnIncreaseGutHarvest();
				__instance.OnIncreaseGutHarvest();
				__instance.OnIncreaseGutHarvest();
				__instance.OnIncreaseGutHarvest();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.bulkKey) && __instance.IsTabHarvestSelected() && __instance.m_SelectedButtonIndex == 2)
			{
				__instance.OnDecreaseGutHarvest();
				__instance.OnDecreaseGutHarvest();
				__instance.OnDecreaseGutHarvest();
				__instance.OnDecreaseGutHarvest();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.modifierKey) && __instance.IsTabHarvestSelected() && __instance.m_HarvestTabButtonRight.active)
			{
				__instance.OnTabQuarterSelected();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.modifierKey) && __instance.IsTabQuarterSelected())
			{
				__instance.OnTabHarvestSelected();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				if (__instance.IsTabHarvestSelected())
				{
					__instance.OnHarvest();
				}
				else if (__instance.IsTabQuarterSelected())
				{
					__instance.OnQuarter();
				}
			}
			else if (InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
			{
				__instance.OnToolPrev();
			}
			else if (InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
			{
				__instance.OnToolNext();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_BreakDown), "Update")]
	internal class UIQoLBreakdown
	{
		private static void Postfix(ref Panel_BreakDown __instance)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!__instance.IsBreakingDown())
			{
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
				{
					__instance.OnBreakDown();
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) || InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
				{
					__instance.OnPrevTool();
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
				{
					__instance.OnNextTool();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Clothing), "Update")]
	internal class UIQoLClothing
	{
		private static int lastTriggerFrame;

		private static void Postfix(Panel_Clothing __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.dropKey))
			{
				__instance.OnDropItem();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey) && Time.frameCount - lastTriggerFrame > 15 && (Object)(object)__instance.GetCurrentlySelectedGearItem() != (Object)null)
			{
				if (Implementation.IM.GetKey(Settings.options.modifierKey))
				{
					__instance.OnActionsButton();
				}
				else
				{
					__instance.OnUseClothingItem();
				}
				lastTriggerFrame = Time.frameCount;
			}
			else if (InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
			{
				__instance.PrevTool();
			}
			else if (InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
			{
				__instance.NextTool();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Container), "Update")]
	internal class UIQoLContainer
	{
		private static void Postfix(ref Panel_Container __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I4
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				if (((int)__instance.m_SelectedTable == 1 && __instance.CanMoveItemToInventory(__instance.GetCurrentlySelectedItem())) | ((int)__instance.m_SelectedTable == 0 && __instance.CanMoveItemToContainer(__instance.GetCurrentlySelectedItem())))
				{
					__instance.OnMoveItem();
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				Panel_Container obj = __instance;
				Panel_Container obj2 = __instance;
				int value = (obj2.m_SelectedSortIndex += 1);
				obj.m_SelectedSortIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons).Length - 1);
				__instance.OnSortInventoryChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons)[__instance.m_SelectedSortIndex]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				Panel_Container obj3 = __instance;
				Panel_Container obj4 = __instance;
				int value = (obj4.m_SelectedSortIndex -= 1);
				obj3.m_SelectedSortIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons).Length - 1);
				__instance.OnSortInventoryChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons)[__instance.m_SelectedSortIndex]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)119) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				Panel_Inventory panel = __instance.m_Inventory.GetPanel();
				int value = (panel.m_SelectedFilterIndex -= 1);
				panel.m_SelectedFilterIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons).Length - 1);
				__instance.OnFilterInventoryChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons)[panel.m_SelectedFilterIndex]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)115) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				Panel_Inventory panel2 = __instance.m_Inventory.GetPanel();
				int value = (panel2.m_SelectedFilterIndex += 1);
				panel2.m_SelectedFilterIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons).Length - 1);
				__instance.OnFilterInventoryChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons)[panel2.m_SelectedFilterIndex]);
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Cooking), "Update")]
	internal class UIQoLCooking
	{
		private static void Postfix(Panel_Cooking __instance)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.m_RecipePrepOperation.InProgress)
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnCook();
			}
			else if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (InputManager.GetScroll(InputManager.m_CurrentContext) > 0f || InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)119))
				{
					__instance.m_ScrollBehaviour.SetSelectedIndex(Mathf.Max(0, __instance.m_ScrollBehaviour.SelectedIndex - 5), false, false);
				}
				else if ((InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)115) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f) && (float)__instance.m_ScrollBehaviour.m_TotalItems > 0f)
				{
					__instance.m_ScrollBehaviour.SetSelectedIndex(Mathf.Min(__instance.m_ScrollBehaviour.m_TotalItems - 1, __instance.m_ScrollBehaviour.SelectedIndex + 5), false, false);
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)119) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				__instance.m_CategoryNavigation.OnNavigateUp();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)115) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				__instance.m_CategoryNavigation.OnNavigateDown();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "Update")]
	internal class UIQoLCookWater
	{
		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				if (__instance.SelectedFoodIsSnow())
				{
					__instance.OnMeltSnow();
				}
				else if (__instance.SelectedFoodIsWater())
				{
					__instance.OnBoil();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "OnMeltSnowUp")]
	internal class BulkIncreaseMeltUnits
	{
		private static int count;

		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (count++ >= 4)
				{
					count = 0;
				}
				else
				{
					__instance.OnMeltSnowUp();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "OnMeltSnowDown")]
	internal class BulkDecreaseMeltUnits
	{
		private static int count;

		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (count++ >= 4)
				{
					count = 0;
				}
				else
				{
					__instance.OnMeltSnowDown();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "OnWaterUp")]
	internal class BulkIncreaseCookingWaterUnits
	{
		private static int count;

		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (count++ >= 4)
				{
					count = 0;
				}
				else
				{
					__instance.OnWaterUp();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "OnWaterDown")]
	internal class BulkDecreaseCookingWaterUnits
	{
		private static int count;

		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (count++ >= 4)
				{
					count = 0;
				}
				else
				{
					__instance.OnWaterDown();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "OnBoilUp")]
	internal class BulkIncreaseBoilUnits
	{
		private static int count;

		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (count++ >= 4)
				{
					count = 0;
				}
				else
				{
					__instance.OnBoilUp();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_CookWater), "OnBoilDown")]
	internal class BulkDecreaseBoilUnits
	{
		private static int count;

		private static void Postfix(Panel_CookWater __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				if (count++ >= 4)
				{
					count = 0;
				}
				else
				{
					__instance.OnBoilDown();
				}
			}
		}
	}
	[HarmonyPatch(typeof(CraftingRequirementContainer), "Enable")]
	internal class CraftingUILocator
	{
		internal static CraftingRequirementContainer UIComp { get; private set; }

		private static void Postfix(CraftingRequirementContainer __instance)
		{
			UIComp = __instance;
		}
	}
	[HarmonyPatch(typeof(Panel_Crafting), "Update")]
	internal class UIQoLCrafting
	{
		private static void Postfix(Panel_Crafting __instance)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected I4, but got Unknown
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Expected I4, but got Unknown
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Expected I4, but got Unknown
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Expected I4, but got Unknown
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_0315: Unknown result type (might be due to invalid IL or missing references)
			CraftingOperation craftingOperation = __instance.m_CraftingOperation;
			if (craftingOperation != null && craftingOperation.InProgress)
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnBeginCrafting();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)119) && Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				__instance.m_ScrollBehaviour.SetSelectedIndex(Mathf.Max(0, __instance.m_ScrollBehaviour.SelectedIndex - 4), false, false);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)115) && Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				__instance.m_ScrollBehaviour.SetSelectedIndex(Mathf.Min(__instance.m_ScrollBehaviour.m_TotalItems - 1, __instance.m_ScrollBehaviour.SelectedIndex + 4), false, false);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				__instance.m_CurrentFilter = (Filter)(__instance.m_CurrentFilter - 1);
				__instance.m_CurrentFilter = (Filter)Math.Clamp((int)__instance.m_CurrentFilter, 0, __instance.m_FilterButtons.Count - 1);
				__instance.OnFilterChange(__instance.m_FilterButtons[(int)__instance.m_CurrentFilter]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				__instance.m_CurrentFilter = (Filter)(__instance.m_CurrentFilter + 1);
				__instance.m_CurrentFilter = (Filter)Math.Clamp((int)__instance.m_CurrentFilter, 0, __instance.m_FilterButtons.Count - 1);
				__instance.OnFilterChange(__instance.m_FilterButtons[(int)__instance.m_CurrentFilter]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)119) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				__instance.m_CategoryNavigation.OnNavigateUp();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)115) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				__instance.m_CategoryNavigation.OnNavigateDown();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && ((Behaviour)CraftingUILocator.UIComp).enabled)
			{
				if (((Behaviour)CraftingUILocator.UIComp.m_SingleTool).enabled)
				{
					CraftingUILocator.UIComp.OnPrevious();
				}
				else if (((Behaviour)CraftingUILocator.UIComp.m_QuantitySelect).enabled)
				{
					CraftingUILocator.UIComp.m_QuantitySelect.OnDecrease();
					if (Implementation.IM.GetKey(Settings.options.bulkKey))
					{
						CraftingUILocator.UIComp.m_QuantitySelect.OnDecrease();
						CraftingUILocator.UIComp.m_QuantitySelect.OnDecrease();
						CraftingUILocator.UIComp.m_QuantitySelect.OnDecrease();
						CraftingUILocator.UIComp.m_QuantitySelect.OnDecrease();
					}
				}
			}
			else
			{
				if (!InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || !((Behaviour)CraftingUILocator.UIComp).enabled)
				{
					return;
				}
				if (((Behaviour)CraftingUILocator.UIComp.m_SingleTool).enabled)
				{
					CraftingUILocator.UIComp.OnNext();
				}
				else if (((Behaviour)CraftingUILocator.UIComp.m_QuantitySelect).enabled)
				{
					CraftingUILocator.UIComp.m_QuantitySelect.OnIncrease();
					if (Implementation.IM.GetKey(Settings.options.bulkKey))
					{
						CraftingUILocator.UIComp.m_QuantitySelect.OnIncrease();
						CraftingUILocator.UIComp.m_QuantitySelect.OnIncrease();
						CraftingUILocator.UIComp.m_QuantitySelect.OnIncrease();
						CraftingUILocator.UIComp.m_QuantitySelect.OnIncrease();
					}
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Inventory_Examine), "Update")]
	internal class UIQoLExamine
	{
		private static void Postfix(Panel_Inventory_Examine __instance)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.IsCleaning() || __instance.IsRepairing() || __instance.IsHarvesting() || __instance.IsReading() || __instance.IsSharpening() || __instance.m_ActionInProgressWindow.active || ((Behaviour)InterfaceManager.GetPanel<Panel_GenericProgressBar>()).isActiveAndEnabled)
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				if ((Object)(object)__instance.m_MenuItemHarvest != (Object)null && __instance.m_MenuItemHarvest.m_Selected && ((Behaviour)__instance.m_Button_Harvest).enabled && __instance.CanHarvest())
				{
					if (__instance.m_ToolWindowActive)
					{
						__instance.OnSelectActionTool();
					}
					else
					{
						__instance.OnHarvest();
					}
				}
				else if ((Object)(object)__instance.m_MenuItemSharpen != (Object)null && __instance.m_MenuItemSharpen.m_Selected && ((Behaviour)__instance.m_Button_Sharpen).enabled)
				{
					if (__instance.m_ToolWindowActive)
					{
						__instance.OnSelectActionTool();
					}
					else
					{
						__instance.OnSharpen();
					}
				}
				else if ((Object)(object)__instance.m_MenuItemRepair != (Object)null && __instance.m_MenuItemRepair.m_Selected && ((Behaviour)__instance.m_Button_Repair).enabled && __instance.CanRepair())
				{
					if (__instance.m_ToolWindowActive)
					{
						__instance.OnSelectActionTool();
					}
					else
					{
						__instance.OnRepair();
					}
				}
				else if ((Object)(object)__instance.m_MenuItemClean != (Object)null && __instance.m_MenuItemClean.m_Selected && ((Behaviour)__instance.m_Button_Clean).enabled)
				{
					if (__instance.m_ToolWindowActive)
					{
						__instance.OnSelectActionTool();
					}
					else
					{
						__instance.OnClean();
					}
				}
				else if ((Object)(object)__instance.m_MenuItemRefuel != (Object)null && __instance.m_MenuItemRefuel.m_Selected && ((Behaviour)__instance.m_Button_Refuel).enabled && __instance.CanRefuel() && __instance.m_RefuelPanel.active)
				{
					if (__instance.m_ToolWindowActive)
					{
						__instance.OnSelectActionTool();
					}
					else
					{
						__instance.OnRefuel();
					}
				}
				else if ((Object)(object)__instance.m_MenuItemSafehouseCustomizationRepair != (Object)null && __instance.m_MenuItemSafehouseCustomizationRepair.m_Selected && ((Behaviour)__instance.m_Button_SafehouseCustomizationRepair).enabled && __instance.CanRepair() && __instance.m_SafehouseCustomizationRepairPanel.active)
				{
					if (__instance.m_ToolWindowActive)
					{
						__instance.OnSelectActionTool();
					}
					else
					{
						__instance.OnRepair();
					}
				}
				else if ((Object)(object)__instance.m_MenuItemUnload != (Object)null && __instance.m_MenuItemUnload.m_Selected && ((Behaviour)__instance.m_Button_Unload).enabled && __instance.m_RifleUnloadPanel.active)
				{
					__instance.OnUnload();
				}
				else if (__instance.m_ReadPanel.active)
				{
					__instance.OnRead();
				}
				else
				{
					EAPISupport.Instance?.OnPerformSelectedAction();
				}
				return;
			}
			if (__instance.m_ReadPanel.active)
			{
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) || InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
				{
					__instance.OnReadHoursDecrease();
					if (Implementation.IM.GetKey(Settings.options.bulkKey))
					{
						__instance.OnReadHoursDecrease();
						__instance.OnReadHoursDecrease();
						__instance.OnReadHoursDecrease();
						__instance.OnReadHoursDecrease();
					}
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
				{
					__instance.OnReadHoursIncrease();
					if (Implementation.IM.GetKey(Settings.options.bulkKey))
					{
						__instance.OnReadHoursIncrease();
						__instance.OnReadHoursIncrease();
						__instance.OnReadHoursIncrease();
						__instance.OnReadHoursIncrease();
					}
				}
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97))
			{
				EAPISupport.Instance?.OnPreviousSubAction();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100))
			{
				EAPISupport.Instance?.OnNextSubAction();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_FeedFire), "Update")]
	internal class UIQoLFeedFire
	{
		private static void Postfix(Panel_FeedFire __instance)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (Time.frameCount - UIQoLFeedFireMonitor.lastEntered >= 30 && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnFeedFire();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_FeedFire), "Enable")]
	internal class UIQoLFeedFireMonitor
	{
		internal static int lastEntered;

		private static void Postfix(ref Panel_FeedFire __instance)
		{
			lastEntered = Time.frameCount;
		}
	}
	[HarmonyPatch(typeof(Panel_FireStart), "Update")]
	internal class UIQoLFireStart
	{
		private static void Postfix(Panel_FireStart __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnStartFire(false);
			}
		}
	}
	[HarmonyPatch(typeof(Panel_GearSelect), "Update")]
	internal class UIQoLGearSelect
	{
		private static void Postfix(Panel_GearSelect __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.SelectGear();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_IceFishing), "Update")]
	internal class UIQoLIceFishing
	{
		private static void Postfix(Panel_IceFishing __instance)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (!__instance.IsFishing())
			{
				if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
				{
					__instance.OnFish();
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) || InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
				{
					__instance.OnDecreaseHours();
				}
				else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
				{
					__instance.OnIncreaseHours();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Inventory), "Update")]
	internal class UIQoLInventory
	{
		private static void Postfix(Panel_Inventory __instance)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_033d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.m_PickUnits.IsEnabled() || __instance.m_ItemDescriptionPage.m_ProgressBar.IsEnabled())
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				if (__instance.m_ItemDescriptionPage.m_MouseButtonExamine.active && Implementation.IM.GetKey(Settings.options.modifierKey))
				{
					__instance.OnExamine();
				}
				else
				{
					__instance.OnEquip();
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.dropKey))
			{
				GearItem gearItem = __instance.GetCurrentlySelectedItem().m_GearItem;
				if ((Object)(object)gearItem == (Object)null || gearItem.m_CantDropItem)
				{
					return;
				}
				ItemDescriptionPage itemDescriptionPage = __instance.m_ItemDescriptionPage;
				if (itemDescriptionPage == null || !itemDescriptionPage.CanDrop(gearItem))
				{
					return;
				}
				if (Implementation.IM.GetKey(Settings.options.modifierKey) && !Implementation.IM.GetKey(Settings.options.bulkKey))
				{
					SafehouseManager safehouseManager = GameManager.GetSafehouseManager();
					if (safehouseManager != null && safehouseManager.IsCustomizing())
					{
						SafehouseManager safehouseManager2 = GameManager.GetSafehouseManager();
						if (safehouseManager2 != null)
						{
							safehouseManager2.StopCustomizing();
						}
					}
					if ((Object)(object)((Component)gearItem).GetComponent<TravoisItem>() != (Object)null)
					{
						__instance.OnDrop();
						((Panel_Base)__instance).CloseSelf();
					}
					else if ((Object)(object)((Component)gearItem).GetComponent<StackableItem>() != (Object)null)
					{
						StackableItem stackableItem = gearItem.m_StackableItem;
						int num = ((stackableItem == null) ? 1 : stackableItem.DefaultUnitsInItem);
						int num2 = num;
						int? obj;
						if (gearItem == null)
						{
							obj = null;
						}
						else
						{
							StackableItem stackableItem2 = gearItem.m_StackableItem;
							obj = ((stackableItem2 != null) ? new int?(stackableItem2.m_Units) : null);
						}
						int? num3 = obj;
						num = Mathf.Clamp(num2, 0, num3.GetValueOrDefault(1));
						GearItem obj2 = gearItem.Drop(num, true, true, false);
						((Panel_Base)__instance).CloseSelf();
						if (obj2 != null)
						{
							obj2.PerformAlternativeInteraction();
						}
					}
					else
					{
						__instance.OnDrop();
						((Panel_Base)__instance).CloseSelf();
						gearItem.PerformAlternativeInteraction();
					}
				}
				else
				{
					__instance.OnDrop();
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)279))
			{
				__instance.ScrollToBottom();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				int value = (__instance.m_SelectedSortIndex += 1);
				__instance.m_SelectedSortIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons).Length - 1);
				__instance.OnSortChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons)[__instance.m_SelectedSortIndex]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				int value = (__instance.m_SelectedSortIndex -= 1);
				__instance.m_SelectedSortIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons).Length - 1);
				__instance.OnSortChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_SortButtons)[__instance.m_SelectedSortIndex]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)119) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				if (__instance.m_SelectedFilterIndex <= 0)
				{
					__instance.m_SelectedFilterIndex += 10;
				}
				int value = (__instance.m_SelectedFilterIndex -= 1);
				__instance.m_SelectedFilterIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons).Length - 1);
				__instance.OnFilterChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons)[__instance.m_SelectedFilterIndex]);
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)115) && Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				if (__instance.m_SelectedFilterIndex >= ((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons).Length - 1)
				{
					__instance.m_SelectedFilterIndex -= 10;
				}
				int value = (__instance.m_SelectedFilterIndex += 1);
				__instance.m_SelectedFilterIndex = Math.Clamp(value, 0, ((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons).Length - 1);
				__instance.OnFilterChange(((Il2CppArrayBase<UIButton>)(object)__instance.m_FilterButtons)[__instance.m_SelectedFilterIndex]);
			}
		}
	}
	[HarmonyPatch(typeof(Panel_PickUnits), "Update")]
	internal class UIQoLPickUnits
	{
		internal static int lastOpened;

		internal static int lastOn;

		internal static int lastExecuted;

		private static void Postfix(Panel_PickUnits __instance)
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			if (lastOn != Time.frameCount - 1)
			{
				lastOpened = Time.frameCount;
			}
			lastOn = Time.frameCount;
			if (lastOpened == Time.frameCount && Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				lastExecuted = Time.frameCount;
				__instance.OnExecuteAll();
			}
			else if (Implementation.IM.GetKey(Settings.options.bulkKey) && InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97))
			{
				__instance.OnDecrease();
				__instance.OnDecrease();
				__instance.OnDecrease();
				__instance.OnDecrease();
			}
			else if (Implementation.IM.GetKey(Settings.options.bulkKey) && InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100))
			{
				__instance.OnIncrease();
				__instance.OnIncrease();
				__instance.OnIncrease();
				__instance.OnIncrease();
			}
			else if (Implementation.IM.GetKey(Settings.options.interactKey) && Implementation.IM.GetKey(Settings.options.bulkKey) && !Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				if (Time.frameCount - lastOpened > 1)
				{
					__instance.OnExecuteAll();
					lastExecuted = lastOpened;
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey) && Time.frameCount - lastOpened > 1)
			{
				__instance.OnExecute();
				lastExecuted = lastOpened;
			}
		}
	}
	[HarmonyPatch(typeof(Panel_PickWater), "Update")]
	internal class UIQoLPickWater
	{
		internal static int lastOpened;

		internal static int lastOn;

		internal static int lastExecuted;

		private static void Postfix(Panel_PickWater __instance)
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			if (lastOn != Time.frameCount - 1)
			{
				lastOpened = Time.frameCount;
			}
			lastOn = Time.frameCount;
			if (lastOpened == Time.frameCount && Implementation.IM.GetKey(Settings.options.bulkKey))
			{
				lastExecuted = Time.frameCount;
				__instance.OnExecuteAll();
			}
			else if (Implementation.IM.GetKey(Settings.options.bulkKey) && InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97))
			{
				__instance.OnDecrease();
				__instance.OnDecrease();
				__instance.OnDecrease();
				__instance.OnDecrease();
			}
			else if (Implementation.IM.GetKey(Settings.options.bulkKey) && InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100))
			{
				__instance.OnIncrease();
				__instance.OnIncrease();
				__instance.OnIncrease();
				__instance.OnIncrease();
			}
			else if (Implementation.IM.GetKey(Settings.options.interactKey) && Implementation.IM.GetKey(Settings.options.bulkKey) && !Implementation.IM.GetKey(Settings.options.modifierKey))
			{
				if (Time.frameCount - lastOpened > 1)
				{
					__instance.OnExecuteAll();
					lastExecuted = lastOpened;
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey) && Time.frameCount - lastOpened > 1)
			{
				__instance.OnExecute();
				lastExecuted = lastOpened;
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Repair), "Update")]
	internal class UIQoLRepair
	{
		private static void Postfix(Panel_Repair __instance)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (!__instance.RepairInProgress() && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnRepair();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Rest), "Update")]
	internal class UIQoLRestPanel
	{
		private static void Postfix(ref Panel_Rest __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				if (__instance.m_PassTimeButtonObject.active)
				{
					__instance.OnPassTime();
				}
				else if (__instance.m_SleepButton.active)
				{
					__instance.OnRest();
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) && Implementation.IM.GetKey(Settings.options.modifierKey) && __instance.m_PassTimeButtonObject.active)
			{
				__instance.OnSelectRest();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) && Implementation.IM.GetKey(Settings.options.modifierKey) && __instance.m_SleepButton.active)
			{
				__instance.OnSelectPassTime();
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)97) || InputManager.GetScroll(InputManager.m_CurrentContext) > 0f)
			{
				__instance.OnDecreaseHours();
				if (Implementation.IM.GetKey(Settings.options.bulkKey))
				{
					__instance.OnDecreaseHours();
					__instance.OnDecreaseHours();
					__instance.OnDecreaseHours();
					__instance.OnDecreaseHours();
				}
			}
			else if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)100) || InputManager.GetScroll(InputManager.m_CurrentContext) < 0f)
			{
				__instance.OnIncreaseHours();
				if (Implementation.IM.GetKey(Settings.options.bulkKey))
				{
					__instance.OnIncreaseHours();
					__instance.OnIncreaseHours();
					__instance.OnIncreaseHours();
					__instance.OnIncreaseHours();
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_SnowShelterBuild), "Update")]
	internal class UIQoLSnowShelterBuild
	{
		private static void Postfix(Panel_SnowShelterBuild __instance)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (!__instance.IsBuilding() && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnBuild();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_SnowShelterInteract), "Update")]
	internal class UIQoLSnowShelterInteract
	{
		private static void Postfix(Panel_SnowShelterInteract __instance)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.IsDismantling() || __instance.IsRepairing() || !InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				return;
			}
			switch (__instance.m_SelectedButtonIndex)
			{
			case 0:
				if (!__instance.m_SnowShelter.IsRuined())
				{
					__instance.OnUse();
				}
				break;
			case 1:
				if (__instance.m_SnowShelter.IsRuined())
				{
					__instance.OnRepair();
				}
				break;
			case 2:
				if (__instance.m_SnowShelter.m_AllowDismantle)
				{
					__instance.OnDismantle();
				}
				break;
			}
		}
	}
	[HarmonyPatch(typeof(Panel_SprayPaint), "Update")]
	internal class UIQoLSprayPaint
	{
		private static void Postfix(Panel_SprayPaint __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnButtonConfirm();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_TorchLight), "Update")]
	internal class UIQoLTorchLight
	{
		private static void Postfix(Panel_TorchLight __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactKey))
			{
				__instance.OnUseSelectedItem();
			}
		}
	}
	[HarmonyPatch(typeof(Panel_Container), "OnDone")]
	internal class TrashCanWarning
	{
		[CompilerGenerated]
		private sealed class <DelayedWarning>d__1 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private object <>2__current;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <DelayedWarning>d__1(int <>1__state)
			{
				this.<>1__state = <>1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Expected O, but got Unknown
				switch (<>1__state)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<>2__current = (object)new WaitForSeconds(0.5f);
					<>1__state = 1;
					return true;
				case 1:
					<>1__state = -1;
					InterfaceManager.GetPanel<Panel_HUD>().DisplayWarningMessage("VOID TRASH CAN");
					<>2__current = (object)new WaitForSeconds(4f);
					<>1__state = 2;
					return true;
				case 2:
					<>1__state = -1;
					InterfaceManager.GetPanel<Panel_HUD>().ClearWarningMessage();
					return false;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		private static void Postfix(Panel_Container __instance)
		{
			if (Settings.options.voidTrashCan && ((Object)__instance.m_Container).name.Contains("CONTAINER_TrashCanister") && !__instance.m_Container.IsEmpty())
			{
				MelonCoroutines.Start(DelayedWarning());
			}
		}

		[IteratorStateMachine(typeof(<DelayedWarning>d__1))]
		private static IEnumerator DelayedWarning()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <DelayedWarning>d__1(0);
		}
	}
	[HarmonyPatch(typeof(Container), "UpdateContainer")]
	internal class TrashCan
	{
		private static List<GearItem> cache = new List<GearItem>();

		private static void Postfix(Container __instance)
		{
			if (__instance.m_StartHasBeenCalled && Settings.options.voidTrashCan && __instance.IsInspected() && ((Object)__instance).name.Contains("CONTAINER_TrashCanister") && !__instance.IsEmpty())
			{
				cache.Clear();
				__instance.GetItems(cache);
				Enumerator<GearItem> enumerator = cache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					GearItem current = enumerator.Current;
					current.Degrade(current.GearItemData.MaxHP * 5E-06f);
				}
			}
		}
	}
}
