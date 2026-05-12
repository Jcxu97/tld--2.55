#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Common;
using Common.Configuration;
using Common.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Reflection;
using Il2CppTLD.Placement;
using MelonLoader;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PlaceFromInventory;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: ComVisible(false)]
[assembly: MelonInfo(typeof(Main), "PlaceFromInventory", "1.1.3", "zorgesho", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyCompany("PlaceFromInventory")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("© 2021 zorgesho")]
[assembly: AssemblyFileVersion("1.1.3")]
[assembly: AssemblyInformationalVersion("1.1.3+58b370ffbf1130cbecba71a7c3ba69bf2dd3ae66")]
[assembly: AssemblyProduct("PlaceFromInventory")]
[assembly: AssemblyTitle("PlaceFromInventory")]
[assembly: AssemblyVersion("1.1.3.0")]
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
	internal static class IsExternalInit
	{
	}
}
namespace Common
{
	internal static class StringExtensions
	{
		public static void log(this string s)
		{
			Log.msg(s, Log.MsgType.INFO);
		}

		public static void logWarning(this string s)
		{
			Log.msg(s, Log.MsgType.WARNING);
		}

		public static void logError(this string s)
		{
			Log.msg(s, Log.MsgType.ERROR);
		}

		[Conditional("TRACE")]
		public static void logDbg(this string s)
		{
			Log.msg(s, Log.MsgType.DBG);
		}

		[Conditional("TRACE")]
		public static void logDbg(this string s, bool condition)
		{
			if (condition)
			{
				s.logDbg();
			}
		}

		[Conditional("TRACE")]
		public static void logDbgError(this string s, bool condition)
		{
			if (condition)
			{
				s.logError();
			}
		}

		public static bool isNullOrEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}

		private static string formatFileName(string filename)
		{
			if (!filename.isNullOrEmpty())
			{
				return Paths.makeRootPath(Paths.ensureExtension(filename, "txt"));
			}
			return filename;
		}

		public static bool startsWith(this string s, string str)
		{
			return s.StartsWith(str, StringComparison.Ordinal);
		}

		public static void saveToFile(this string s, string localPath)
		{
			string text = formatFileName(localPath);
			Paths.ensurePath(text);
			try
			{
				File.WriteAllText(text, s);
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}

		public static string onScreen(this string str, bool highPriority = false)
		{
			HUDMessage.AddMessage(str, highPriority, false);
			return str;
		}
	}
	internal static class Log
	{
		public enum MsgType
		{
			DBG,
			INFO,
			WARNING,
			ERROR,
			EXCEPTION
		}

		public static void msg(string str, MsgType msgType)
		{
			string value = $" [{Time.frameCount}]";
			MelonLogger.Msg($"{DateTime.Now:HH:mm:ss.fff}{value}  {msgType}: {str}");
		}

		public static void msg(Exception e, string str = "", bool verbose = true)
		{
			msg(str + ((str == "") ? "" : ": ") + (verbose ? formatException(e) : e.Message), MsgType.EXCEPTION);
		}

		private static string formatException(Exception e)
		{
			if (e != null)
			{
				return $"\r\n{e.GetType()}: {e.Message}\r\nSTACKTRACE:\r\n{e.StackTrace}\r\n" + formatException(e.InnerException);
			}
			return "";
		}
	}
	internal static class Debug
	{
		public class Profiler : IDisposable
		{
			public void Dispose()
			{
			}
		}

		[Conditional("DEBUG")]
		public static void assert(bool condition, string message = null, [CallerFilePath] string __filename = "", [CallerLineNumber] int __line = 0)
		{
			if (condition)
			{
				return;
			}
			string obj = $"Assertion failed{((message != null) ? (": " + message) : "")} ({__filename}:{__line})";
			(obj ?? "").logError();
			throw new Exception(obj);
		}

		public static Profiler profiler(string message = null)
		{
			return null;
		}
	}
	internal static class MiscExtensions
	{
		public static void forEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence != null)
			{
				IEnumerator<T> enumerator = sequence.GetEnumerator();
				while (enumerator.MoveNext())
				{
					action(enumerator.Current);
				}
			}
		}

		public static int findIndex<T>(this List<T> list, Predicate<T> predicate)
		{
			return list.FindIndex(Predicate<T>.op_Implicit((Func<T, bool>)predicate.Invoke));
		}
	}
	internal static class ArrayExtensions
	{
		public static bool isNullOrEmpty(this Array array)
		{
			if (array != null)
			{
				return array.Length == 0;
			}
			return true;
		}
	}
	internal static class InputHelper
	{
		public static bool isKeyDown(KeyCode key)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if ((int)key != 0)
			{
				return InputManager.GetKeyDown(InputManager.m_CurrentContext, key);
			}
			return false;
		}

		public static string getLabelForKey(KeyCode key)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if ((int)key != 0)
			{
				return InputManager.ConvertKeycodeToLabel(((object)(KeyCode)(ref key)).ToString());
			}
			return "";
		}
	}
	internal static class GameUtils
	{
		public static PlayerManager PlayerManager => GameManager.GetPlayerManagerComponent();

		public static Inventory Inventory => GameManager.GetInventoryComponent();

		public static GearItem addItem(string name, int count = 1)
		{
			return PlayerManager.AddItemCONSOLE(name, count, 1f);
		}

		public static bool isMainMenu()
		{
			return GameManager.m_ActiveScene == "MainMenu";
		}

		public static void showErrorMessage(string message)
		{
			message?.onScreen();
			GameAudioManager.PlayGUIError();
		}
	}
	public abstract class Mod : MelonMod
	{
		public static readonly string id = Assembly.GetExecutingAssembly().GetName().Name;

		protected virtual void init()
		{
		}

		public override void OnApplicationStart()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			init();
		}
	}
	internal static class Paths
	{
		public static readonly string modRootPath = Path.Combine(Path.GetDirectoryName(typeof(MelonMod).Assembly.Location), "..\\Mods\\");

		public static string makeRootPath(string filename)
		{
			if (!filename.isNullOrEmpty())
			{
				return (Path.IsPathRooted(filename) ? "" : modRootPath) + filename;
			}
			return filename;
		}

		public static string ensureExtension(string filename, string ext)
		{
			if (!filename.isNullOrEmpty())
			{
				if (!Path.HasExtension(filename))
				{
					return filename + (ext.startsWith(".") ? "" : ".") + ext;
				}
				return filename;
			}
			return filename;
		}

		public static void ensurePath(string filename)
		{
			if (!filename.isNullOrEmpty())
			{
				string path = makeRootPath(Path.GetDirectoryName(filename));
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
		}
	}
	internal static class ObjectAndComponentExtensions
	{
		public static C ensureComponent<C>(this GameObject go) where C : Component
		{
			return go.GetComponent<C>() ?? go.AddComponent<C>();
		}

		public static void setParent(this GameObject go, GameObject parent)
		{
			go.transform.SetParent(parent.transform, false);
		}

		public static GameObject getParent(this GameObject go)
		{
			Transform parent = go.transform.parent;
			if (parent == null)
			{
				return null;
			}
			return ((Component)parent).gameObject;
		}

		public static GameObject getChild(this GameObject go, string name)
		{
			Transform obj = go.transform.Find(name);
			if (obj == null)
			{
				return null;
			}
			return ((Component)obj).gameObject;
		}

		public static GameObject createChild(this GameObject go, string name, Vector3? pos = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(name);
			val.setParent(go);
			if (pos.HasValue)
			{
				val.transform.position = pos.Value;
			}
			return val;
		}

		public static GameObject createChild(this GameObject go, GameObject prefab, string name, Vector3? pos = null, Vector3? localPos = null, Vector3? localScale = null)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = Object.Instantiate<GameObject>(prefab, go.transform);
			if (name != null)
			{
				((Object)val).name = name;
			}
			if (pos.HasValue)
			{
				val.transform.position = pos.Value;
			}
			if (localPos.HasValue)
			{
				val.transform.localPosition = localPos.Value;
			}
			if (localScale.HasValue)
			{
				val.transform.localScale = localScale.Value;
			}
			return val;
		}

		private static void _destroy(this Object obj, bool immediate)
		{
			if (immediate)
			{
				Object.DestroyImmediate(obj);
			}
			else
			{
				Object.Destroy(obj);
			}
		}

		public static void destroyComponent<T>(this GameObject go, bool immediate = true) where T : Component
		{
			object obj = go.GetComponent<T>();
			if (obj != null)
			{
				((Object)(object)(T)obj)._destroy(immediate);
			}
		}

		public static string baseName(this GameObject go)
		{
			return ((Object)go).name.Replace("(Clone)", "");
		}
	}
	internal static class StructsExtensions
	{
		public static string toStringRGB(this Color color)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return ColorUtility.ToHtmlStringRGB(color);
		}
	}
}
namespace Common.Configuration
{
	internal abstract class Config
	{
		private class ConfigContractResolver : DefaultContractResolver
		{
			protected override List<MemberInfo> GetSerializableMembers(Type objectType)
			{
				return (from field in objectType.fields()
					where !field.IsStatic
					select field).Cast<MemberInfo>().ToList();
			}

			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				JsonProperty obj = ((DefaultContractResolver)this).CreateProperty(member, memberSerialization);
				bool writable = (obj.Readable = true);
				obj.Writable = writable;
				return obj;
			}
		}

		[Flags]
		public enum LoadOptions
		{
			None = 0,
			MainConfig = 2,
			ForcedLoad = 4,
			ReadOnly = 8,
			Default = 2
		}

		private JsonSerializerSettings srzSettings;

		public static readonly string defaultName = Mod.id + ".json";

		private const bool ignoreExistingFile = false;

		public static Config main { get; private set; }

		public static string lastError { get; private set; }

		public string configPath { get; private set; }

		private static JsonSerializerSettings _initSerializer(Type _)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_002f: Expected O, but got Unknown
			return new JsonSerializerSettings
			{
				Formatting = (Formatting)1,
				ContractResolver = (IContractResolver)(object)new ConfigContractResolver(),
				ObjectCreationHandling = (ObjectCreationHandling)2,
				Converters = { (JsonConverter)new StringEnumConverter() }
			};
		}

		private string serialize()
		{
			return JsonConvert.SerializeObject((object)this, srzSettings ?? (srzSettings = _initSerializer(GetType())));
		}

		private static Config deserialize(string text, Type configType)
		{
			return JsonConvert.DeserializeObject(text, configType, _initSerializer(configType)) as Config;
		}

		protected virtual void onLoad()
		{
		}

		public static C tryLoad<C>(LoadOptions loadOptions = LoadOptions.MainConfig) where C : Config
		{
			return tryLoad(typeof(C), defaultName, loadOptions) as C;
		}

		public static C tryLoad<C>(string loadPath, LoadOptions loadOptions = LoadOptions.MainConfig) where C : Config
		{
			return tryLoad(typeof(C), loadPath, loadOptions) as C;
		}

		public static Config tryLoad(Type configType, string loadPath, LoadOptions loadOptions = LoadOptions.MainConfig)
		{
			string filename = (loadPath.isNullOrEmpty() ? null : Paths.makeRootPath(loadPath));
			filename = Paths.ensureExtension(filename, "json");
			Paths.ensurePath(filename);
			Config config;
			try
			{
				bool num = !File.Exists(filename);
				if (num && filename != null)
				{
					("Creating default config (" + loadPath + ")").log();
				}
				config = (num ? (Activator.CreateInstance(configType) as Config) : deserialize(File.ReadAllText(filename), configType));
				config.onLoad();
				if (num || !loadOptions.HasFlag(LoadOptions.ReadOnly))
				{
					config.save(filename);
				}
				if (!loadOptions.HasFlag(LoadOptions.ReadOnly))
				{
					config.configPath = filename;
				}
				if (loadOptions.HasFlag(LoadOptions.MainConfig))
				{
					"Config.main is already set!".logDbgError(main != null);
					if (main == null)
					{
						main = config;
					}
				}
			}
			catch (Exception ex)
			{
				Log.msg(ex, "Exception while loading '" + loadPath + "'");
				lastError = ex.Message;
				config = null;
			}
			return config;
		}

		public void save(string savePath = null)
		{
			string text = Paths.ensureExtension(savePath, "json") ?? configPath;
			if (text == null)
			{
				return;
			}
			try
			{
				File.WriteAllText(text, serialize());
			}
			catch (Exception e)
			{
				Log.msg(e, "Exception while saving '" + text + "'");
			}
		}
	}
}
namespace Common.Reflection
{
	internal static class TypeExtensions
	{
		public static FieldInfo field(this Type type, string name, BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
		{
			return type.GetField(name, bf);
		}

		public static FieldInfo[] fields(this Type type, BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
		{
			return type.GetFields(bf);
		}
	}
	internal static class Il2CppTypeExtensions
	{
		public static FieldInfo[] fields(this Type type, BindingFlags bf = 60)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return Il2CppArrayBase<FieldInfo>.op_Implicit((Il2CppArrayBase<FieldInfo>)(object)type.GetFields(bf));
		}

		public static PropertyInfo[] properties(this Type type, BindingFlags bf = 60)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return Il2CppArrayBase<PropertyInfo>.op_Implicit((Il2CppArrayBase<PropertyInfo>)(object)type.GetProperties(bf));
		}
	}
	internal static class ReflectionHelper
	{
		public const BindingFlags bfAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public const BindingFlags bfAll_Il2Cpp = 60;
	}
}
namespace PlaceFromInventory
{
	internal class ModConfig : Config
	{
		public readonly bool showItemsQuantity = true;

		public readonly bool allowToPlaceItemsTooClose = true;
	}
	public class Main : Mod
	{
		internal const string version = "1.1.3";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();
	}
	[HarmonyPatch]
	internal static class InventoryPatches
	{
		private static GearItem clothItemPlaceAfterDrop;

		private const int delayAfterCancel = 10;

		private static int lastFrameCancelled;

		private static bool shouldSkipClick => Time.frameCount - lastFrameCancelled < 10;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(InventoryGridItem), "OnClick")]
		private static void InventoryGridItem_OnClick_Postfix(InventoryGridItem __instance)
		{
			if (!shouldSkipClick && Input.GetMouseButtonUp(1) && !((Panel_Base)InterfaceManager.GetPanel<Panel_Container>()).IsEnabled() && UIHelper.invPanel.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem) && !Object.op_Implicit((Object)(object)__instance.m_GearItem.m_WaterSupply))
			{
				UIHelper.startPlaceObject(((Component)__instance.m_GearItem).gameObject, (PlaceMeshFlags)2);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ClothingSlot), "DoClickAction")]
		private static void ClothingSlot_DoClickAction_Postfix(ClothingSlot __instance)
		{
			if (!shouldSkipClick && Input.GetMouseButtonUp(1) && Object.op_Implicit((Object)(object)__instance.m_GearItem) && UIHelper.clothPanel.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem))
			{
				clothItemPlaceAfterDrop = __instance.m_GearItem;
				UIHelper.clothPanel.OnDropItem();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_Clothing), "OnDropItem")]
		private static void PanelClothing_OnDropItem_Postfix()
		{
			if (Object.op_Implicit((Object)(object)clothItemPlaceAfterDrop))
			{
				UIHelper.startPlaceObject(((Component)clothItemPlaceAfterDrop).gameObject, (PlaceMeshFlags)0);
				clothItemPlaceAfterDrop = null;
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
		private static void PlayerManager_ExitMeshPlacement_Postfix(PlayerManager __instance)
		{
			if (!__instance.m_SkipCancel)
			{
				lastFrameCancelled = Time.frameCount;
			}
			UIHelper.restorePreviousPanel();
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PlayerManager), "DoPositionCheck")]
		private static void PlayerManager_DoPositionCheck_Postfix(ref MeshLocationCategory __result)
		{
			if (Main.config.allowToPlaceItemsTooClose && (int)__result == 11)
			{
				__result = (MeshLocationCategory)0;
			}
		}
	}
	[HarmonyPatch]
	internal static class PanelPickPatches
	{
		private static bool dropAsStack;

		private static GearItem droppedItem;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_PickUnits), "Update")]
		private static void PanelPickUnits_Update_Postfix(Panel_PickUnits __instance)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (((Panel_Base)__instance).IsEnabled() && (int)__instance.m_ExecuteAction == 0)
			{
				dropAsStack = Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305);
				string text = (dropAsStack ? "Drop as stack" : Localization.Get("GAMEPLAY_Drop"));
				__instance.m_Execute_Button.GetComponentInChildren<UILabel>().text = text;
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_PickUnits), "Refresh")]
		private static void PanelPickUnits_Refresh_Postfix(Panel_PickUnits __instance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if ((int)__instance.m_ExecuteAction == 0)
			{
				UILabel label_Description = __instance.m_Label_Description;
				label_Description.text += "\n(hold Control to drop as stack)";
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_PickUnits), "ExitInterface")]
		private static void PanelPickUnits_ExitInterface_Postfix()
		{
			if (Object.op_Implicit((Object)(object)droppedItem))
			{
				UIHelper.startPlaceObject(((Component)droppedItem).gameObject, (PlaceMeshFlags)0);
				droppedItem = null;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Panel_PickUnits), "DropGear")]
		private static bool PanelPickUnits_DropGear_Prefix(Panel_PickUnits __instance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if ((int)__instance.m_ExecuteAction != 0 || !dropAsStack)
			{
				return true;
			}
			droppedItem = __instance.m_GearItem.Drop(__instance.m_numUnits, true, true, false);
			return false;
		}
	}
	internal static class UIHelper
	{
		private static bool backToInvPanel;

		private static bool backToClothPanel;

		public static Panel_Inventory invPanel => InterfaceManager.GetPanel<Panel_Inventory>();

		public static Panel_Clothing clothPanel => InterfaceManager.GetPanel<Panel_Clothing>();

		public static void hideCurrentPanel()
		{
			if (backToInvPanel = ((Panel_Base)invPanel).IsEnabled())
			{
				invPanel.Enable(false, true);
			}
			if (backToClothPanel = ((Panel_Base)clothPanel).IsEnabled())
			{
				((Panel_Base)clothPanel).Enable(false);
			}
		}

		public static void restorePreviousPanel()
		{
			if (backToInvPanel)
			{
				invPanel.Enable(true, true);
			}
			if (backToClothPanel)
			{
				((Panel_Base)clothPanel).Enable(true);
			}
			backToInvPanel = (backToClothPanel = false);
		}

		public static void startPlaceObject(GameObject go, PlaceMeshFlags flags = 0)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			hideCurrentPanel();
			GameUtils.PlayerManager.StartPlaceMesh(go, flags, (PlaceMeshRules)1);
		}
	}
}
