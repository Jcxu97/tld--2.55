using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using Il2Cpp;
using Il2CppSystem;
using Il2CppTLD.Scenes;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModData;
using ModSettings;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.FastTravel;
using Pathoschild.TheLongDarkMods.FastTravel.Framework;
using Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("Fast Travel")]
[assembly: AssemblyDescription("Lets you save up to 9 places (like your home base), and fast travel to them anytime at the press of a button.")]
[assembly: AssemblyFileVersion("0.2.0")]
[assembly: MelonInfo(typeof(ModEntry), "Fast Travel", "0.2.0", "Pathoschild", "https://www.nexusmods.com/thelongdark/mods/54")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("0.2.0.0")]
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
namespace Pathoschild.TheLongDarkMods.Common
{
	internal class InteractionHelper
	{
		private readonly Instance Log;

		public InteractionHelper(Instance log)
		{
			Log = log;
		}

		public bool IsKeyDown(KeyCode key)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return Input.GetKey(key);
		}

		public bool IsKeyJustPressed(KeyCode key)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return InputManager.GetKeyDown(InputManager.m_CurrentContext, key);
		}

		public void ShowMessageBox(string message, Action? onConfirm = null)
		{
			if (TryGetUnusedConfirmationPanel(out Panel_Confirmation panel))
			{
				panel.ShowErrorMessage(message, CallbackDelegate.op_Implicit(onConfirm));
			}
		}

		public void ShowConfirmDialogue(string question, Action onConfirm)
		{
			if (TryGetUnusedConfirmationPanel(out Panel_Confirmation panel))
			{
				panel.ShowConfirmPanel(question, "Yes", "No", CallbackDelegate.op_Implicit(onConfirm), (EnableDelegate)null);
			}
		}

		private bool TryGetUnusedConfirmationPanel([NotNullWhen(true)] out Panel_Confirmation? panel)
		{
			panel = InterfaceManager.GetPanel<Panel_Confirmation>();
			if (panel == null)
			{
				Log.Warning("Can't show confirmation dialogue: Panel_Confirmation not found.");
				return false;
			}
			if (((Behaviour)panel).isActiveAndEnabled)
			{
				panel = null;
				return false;
			}
			return true;
		}
	}
	internal static class SceneHelper
	{
		public static bool IsSaveLoaded()
		{
			bool flag = GameManager.m_Instance != null && !GameManager.IsMainMenuActive();
			bool flag2 = flag;
			if (flag2)
			{
				string sceneName = GetSceneName();
				bool flag3 = ((sceneName == null || (sceneName != null && sceneName.Length == 0) || sceneName == "MainMenu") ? true : false);
				flag2 = !flag3;
			}
			return flag2;
		}

		public static Scene GetScene()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			return SceneManager.GetActiveScene();
		}

		public static RegionSpecification? TryGetRegion()
		{
			return GameManager.TryGetCurrentRegion();
		}

		public static string GetSceneName()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Scene scene = GetScene();
			return ((Scene)(ref scene)).name;
		}

		public static string GetDisplayName(string name)
		{
			return InterfaceManager.GetNameForScene(name);
		}

		public static bool IsOutdoors(string name)
		{
			return GameManager.IsOutDoorsScene(name);
		}

		public static bool IsCustomizableSafehouse()
		{
			return GameManager.GetSafehouseManager().InCustomizableSafehouse();
		}
	}
}
namespace Pathoschild.TheLongDarkMods.FastTravel
{
	public class ModEntry : MelonMod
	{
		private const int MaxDestinations = 9;

		private readonly ModConfig Config = new ModConfig();

		private Instance Log;

		private DestinationManager DestinationManager;

		private InteractionHelper InteractionHelper;

		private FastTravelTransition FastTravel;

		private DestinationListOverlay DestinationListOverlay;

		public override void OnInitializeMelon()
		{
			Log = Melon<ModEntry>.Logger;
			DestinationManager = new DestinationManager(Log);
			InteractionHelper = new InteractionHelper(Log);
			DestinationListOverlay = DestinationListOverlay.Create();
			Config.AddToModSettings("快速旅行v0.2");
		}

		public override void OnUpdate()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			if (DestinationListOverlay.IsVisible && !SceneHelper.IsSaveLoaded())
			{
				DestinationListOverlay.Hide();
			}
			if (!InputManager.HasPressedKey() || !SceneHelper.IsSaveLoaded())
			{
				return;
			}
			if (InteractionHelper.IsKeyJustPressed(Config.ShowListKey))
			{
				if (DestinationListOverlay.IsVisible)
				{
					DestinationListOverlay.Hide();
				}
				else
				{
					ShowDestinationList(DestinationManager.GetData());
				}
				return;
			}
			if (InteractionHelper.IsKeyJustPressed(Config.ReturnPointKey))
			{
				InteractivelyReturn();
				return;
			}
			for (int i = 0; i < 9; i++)
			{
				KeyCode keyForSlot = GetKeyForSlot(i);
				if (InteractionHelper.IsKeyJustPressed(keyForSlot))
				{
					if (InteractionHelper.IsKeyDown(Config.SaveModifierKey))
					{
						InteractivelySave(i);
					}
					else if (InteractionHelper.IsKeyDown(Config.DeleteModifierKey))
					{
						InteractivelyDelete(i);
					}
					else
					{
						InteractivelyFastTravel(i);
					}
					break;
				}
			}
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			if (sceneName == "Empty")
			{
				return;
			}
			if (Config.LogDebugInfo)
			{
				Destination currentLocation = DestinationManager.GetCurrentLocation();
				Log.Msg($"Scene initialized:\n    buildIndex: {buildIndex}\n    sceneName: '{sceneName}'\n    save name: '{SaveGameSystem.GetCurrentSaveName()}'\n\n    location: {currentLocation}\n    is outside: {SceneHelper.IsOutdoors(currentLocation.Scene.Name)}\n    is safehouse: {SceneHelper.IsCustomizableSafehouse()}\n    was restored: {GameManager.m_SceneWasRestored}\n\n    Unity scene:\n        name: {currentLocation.Scene.Name}\n        guid: {currentLocation.Scene.Guid}\n        path: {currentLocation.Scene.Path}\n        isSubScene: {currentLocation.Scene.IsSubScene}\n\n    Fast travel:\n        from: {FastTravel?.From.ToString() ?? "null"}\n        to:   {FastTravel?.To.ToString() ?? "null"}\n\n{GetTransitionDebugSummary("transition", currentLocation.LastTransition)}");
			}
			if (FastTravel != null)
			{
				Destination to = FastTravel.To;
				if (sceneName != to.Scene.Name)
				{
					Log.Warning($"Failed setting position after warp back: arrived in scene '{sceneName}' instead of the expected '{to.Scene.Name}'.");
				}
				else
				{
					SnapPlayerTo(to.Position.ToVector3(), to.CameraPitch, to.CameraYaw);
				}
				FastTravel = null;
			}
		}

		private void InteractivelyDelete(int slotIndex)
		{
			if (!Config.CanEditDestinations)
			{
				Log.Warning("Can't edit fast travel destinations (per your mod settings).");
				return;
			}
			SaveModel data = DestinationManager.GetData();
			Destination destination = data.Get(slotIndex);
			if (destination != null)
			{
				InteractionHelper.ShowConfirmDialogue($"是否删除快速传送点位 {slotIndex + 1} ({destination.GetDisplayName(false)})?", delegate
				{
					data.Set(slotIndex, null);
					DestinationManager.SaveData(data);
					UpdateDestinationListIfVisible(data);
				});
			}
		}

		private void InteractivelySave(int slotIndex)
		{
			if (!Config.CanEditDestinations)
			{
				Log.Warning("Can't edit fast travel destinations (per your mod settings).");
				return;
			}
			SaveModel data = DestinationManager.GetData();
			Destination here = DestinationManager.GetCurrentLocation();
			Destination destination = data.Get(slotIndex);
			string text = $"将 {here.GetDisplayName(false)} 保存为传送点位 {slotIndex + 1}?";
			if (destination != null)
			{
				string text2 = ((destination.Scene.Name == here.Scene.Name) ? "in this location" : ("(" + destination.GetDisplayName(false) + ")"));
				text = text + "\n\n此操作将覆盖原有传送点 " + text2 + ".";
			}
			InteractionHelper.ShowConfirmDialogue(text, delegate
			{
				data.Set(slotIndex, here);
				DestinationManager.SaveData(data);
				UpdateDestinationListIfVisible(data);
			});
		}

		private void InteractivelyFastTravel(int slotIndex)
		{
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			SaveModel data = DestinationManager.GetData();
			Destination here = DestinationManager.GetCurrentLocation();
			Destination destination = data.Get(slotIndex);
			Destination returnPoint = data.ReturnPoint;
			if (HasFastTravelRestrictions(here, destination, data, out var restrictionPhrase))
			{
				Log.Warning("Can't fast travel " + restrictionPhrase + " (per your mod settings).");
				return;
			}
			if (destination == null)
			{
				string text = $"你尚未设置第 {slotIndex + 1} 号快速传送点";
				if (Config.ShowUsageHints)
				{
					text += $"\n\n按住 {Config.SaveModifierKey} + {GetKeyForSlot(slotIndex)} 即可保存当前位置到此点位";
				}
				InteractionHelper.ShowMessageBox(text);
				return;
			}
			string text2 = "是否传送至 " + destination.GetDisplayName(false) + "?";
			if ((int)Config.ReturnPointKey != 0 && Config.ShowUsageHints)
			{
				if (returnPoint != null && returnPoint.Scene.Name != here.Scene.Name)
				{
					text2 = text2 + "\n\n此操作将覆盖上一个回溯点位 (" + returnPoint.GetDisplayName(false) + ").";
				}
				text2 += $"\n\n稍后按下 {Config.ReturnPointKey}即可返回此处";
			}
			InteractionHelper.ShowConfirmDialogue(text2, delegate
			{
				data.ReturnPoint = here;
				DestinationManager.SaveData(data);
				UpdateDestinationListIfVisible(data);
				FastTravelTo(destination);
			});
		}

		private void InteractivelyReturn()
		{
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			SaveModel data = DestinationManager.GetData();
			Destination here = DestinationManager.GetCurrentLocation();
			Destination returnPoint = data.ReturnPoint;
			if (HasFastTravelRestrictions(here, returnPoint, data, out var restrictionPhrase))
			{
				Log.Warning("Can't fast travel " + restrictionPhrase + " (per your mod settings).");
				return;
			}
			if (returnPoint == null)
			{
				string text = "You haven't fast traveled anywhere yet.";
				if (Config.ShowUsageHints)
				{
					text += $"\n\n完成一次传送后，按下 {Config.ReturnPointKey}即可回到出发位置";
				}
				InteractionHelper.ShowMessageBox(text);
				return;
			}
			string text2 = "是否传送回 " + returnPoint.GetDisplayName(false) + "?";
			if (here.Scene.Name != returnPoint.Scene.Name)
			{
				text2 = text2 + "\n\n将把 " + here.GetDisplayName(false) + " 设置为新的回溯点位";
			}
			InteractionHelper.ShowConfirmDialogue(text2, delegate
			{
				data.ReturnPoint = here;
				DestinationManager.SaveData(data);
				UpdateDestinationListIfVisible(data);
				FastTravelTo(returnPoint);
			});
		}

		private void FastTravelTo(Destination destination)
		{
			string currentSceneName = SceneHelper.GetSceneName();
			SaveGameSystem.SaveGame("autosave", currentSceneName);
			CameraFade.FadeOut(GameManager.m_SceneTransitionFadeOutTime, 0f, Action.op_Implicit((Action)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0079: Unknown result type (might be due to invalid IL or missing references)
				//IL_0085: Expected O, but got Unknown
				TransitionModel lastTransition = destination.LastTransition;
				GameManager.m_SceneTransitionData = new SceneTransitionData
				{
					m_SceneSaveFilenameCurrent = lastTransition.FromSceneId,
					m_SceneSaveFilenameNextLoad = lastTransition.ToSceneId,
					m_ForceNextSceneLoadTriggerScene = lastTransition.ForceNextSceneLoadTriggerScene,
					m_SceneLocationLocIDOverride = lastTransition.SceneLocationLocIdOverride,
					m_GameRandomSeed = lastTransition.GameRandomSeed,
					m_Location = lastTransition.Location,
					m_LastOutdoorScene = lastTransition.LastOutdoorScene,
					m_PosBeforeInteriorLoad = lastTransition.LastOutdoorPosition.ToVector3(),
					m_TeleportPlayerSaveGamePosition = true
				};
				if (Config.LogDebugInfo)
				{
					Log.Msg($"Starting fast travel:\n    save name: '{SaveGameSystem.GetCurrentSaveName()}'\n\n    from location: '{currentSceneName}'\n    from outside: {SceneHelper.IsOutdoors(currentSceneName)}\n    from safehouse: {SceneHelper.IsCustomizableSafehouse()}\n\n    destination: {destination}\n    destination is outside: {SceneHelper.IsOutdoors(destination.Scene.Name)}\n\n{GetTransitionDebugSummary("transition", new TransitionModel(GameManager.m_SceneTransitionData))}");
				}
				FastTravel = new FastTravelTransition(DestinationManager.GetCurrentLocation(), destination);
				GameManager.LoadScene(destination.Scene.Name, SaveGameSystem.GetCurrentSaveName());
			}));
		}

		private void SnapPlayerTo(Vector3 position, float cameraPitch, float cameraYaw)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			GameObject playerObject = GameManager.GetPlayerObject();
			CharacterController component = playerObject.GetComponent<CharacterController>();
			vp_FPSCamera vpFPSCamera = GameManager.GetVpFPSCamera();
			if ((Object)(object)component != (Object)null)
			{
				((Collider)component).enabled = false;
			}
			try
			{
				playerObject.transform.position = position;
			}
			finally
			{
				if ((Object)(object)component != (Object)null)
				{
					((Collider)component).enabled = true;
				}
			}
			vpFPSCamera.m_Pitch = cameraPitch;
			vpFPSCamera.m_TargetPitch = cameraPitch;
			vpFPSCamera.m_CurrentPitch = cameraPitch;
			vpFPSCamera.m_Yaw = cameraYaw;
			vpFPSCamera.m_TargetYaw = cameraYaw;
			vpFPSCamera.m_CurrentYaw = cameraYaw;
		}

		private void ShowDestinationList(SaveModel data)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			string[] array = (from p in data.Destinations
				orderby p.Key
				select $"[{GetKeyForSlot(p.Key)}] {p.Value.GetDisplayName(true)}").ToArray();
			string text = ((data.ReturnPoint != null) ? $"[{Config.ReturnPointKey}] {data.ReturnPoint.GetDisplayName(true)}" : "None set.");
			string text2 = "返程点位:\n   " + text + "\n\n已保存传送点位:\n   " + ((array.Length != 0) ? string.Join("\n   ", array) : "None set.");
			DestinationListOverlay.Show(text2);
		}

		private void UpdateDestinationListIfVisible(SaveModel data)
		{
			if (DestinationListOverlay.IsVisible)
			{
				ShowDestinationList(data);
			}
		}

		private KeyCode GetKeyForSlot(int slotIndex)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			return (KeyCode)(slotIndex switch
			{
				0 => Config.Destination1, 
				1 => Config.Destination2, 
				2 => Config.Destination3, 
				3 => Config.Destination4, 
				4 => Config.Destination5, 
				5 => Config.Destination6, 
				6 => Config.Destination7, 
				7 => Config.Destination8, 
				8 => Config.Destination9, 
				_ => throw new InvalidOperationException($"Unsupported destination slot {slotIndex}."), 
			});
		}

		private bool HasFastTravelRestrictions(Destination from, Destination to, SaveModel data, [NotNullWhen(true)] out string restrictionPhrase)
		{
			if (!Config.CanTravel)
			{
				restrictionPhrase = "at all";
				return true;
			}
			if (Config.OnlyBetweenDestinations && to != null)
			{
				bool flag = false;
				bool flag2 = false;
				foreach (KeyValuePair<int, Destination> destination in data.Destinations)
				{
					string name = destination.Value.Scene.Name;
					if (name == from.Scene.Name)
					{
						flag = true;
					}
					if (name == to.Scene.Name)
					{
						flag2 = true;
					}
					if (flag && flag2)
					{
						break;
					}
				}
				if (!flag || !flag2)
				{
					restrictionPhrase = "because you can only travel between saved fast travel points";
					return true;
				}
			}
			if (!Config.CanTravelFromOutside || !Config.CanTravelFromNonSafehouseInterior)
			{
				bool flag3 = SceneHelper.IsOutdoors(from.Scene.Name);
				if (!Config.CanTravelFromOutside && flag3)
				{
					restrictionPhrase = "from outside";
					return true;
				}
				if (!Config.CanTravelFromNonSafehouseInterior && !flag3 && !SceneHelper.IsCustomizableSafehouse())
				{
					restrictionPhrase = "from non-safehouse interior";
					return true;
				}
			}
			if (!Config.CanTravelToSameScene && from.Scene.Name == to?.Scene.Name)
			{
				restrictionPhrase = "to the same location";
				return true;
			}
			restrictionPhrase = null;
			return false;
		}

		private string GetTransitionDebugSummary(string label, TransitionModel transition, string indent = "    ")
		{
			return $"{indent}{label}:\n{indent}    FromSceneId: {transition.FromSceneId ?? "<null>"}\n{indent}    ToSceneId: {transition.ToSceneId ?? "<null>"}\n{indent}    ToSpawnPoint: {transition.ToSpawnPoint ?? "<null>"}\n{indent}    ToSpawnPointAudio: {transition.ToSpawnPointAudio ?? "<null>"}\n{indent}    RestorePlayerPosition: {transition.RestorePlayerPosition}\n{indent}    LastOutdoorScene: {transition.LastOutdoorScene ?? "<null>"}\n{indent}    LastOutdoorPosition: {transition.LastOutdoorPosition}\n{indent}    GameRandomSeed: {transition.GameRandomSeed}\n{indent}    ForceNextSceneLoadTriggerScene: {transition.ForceNextSceneLoadTriggerScene ?? "<null>"}\n{indent}    SceneLocationLocIdOverride: {transition.SceneLocationLocIdOverride ?? "<null>"}\n{indent}    Location: {transition.Location ?? "<null>"}";
		}
	}
	internal class ModInfo
	{
		public const string DisplayName = "Fast Travel";

		public const string Version = "0.2.0";

		public const string AssemblyVersion = "0.2.0";

		public const string Author = "Pathoschild";

		public const string Description = "Lets you save up to 9 places (like your home base), and fast travel to them anytime at the press of a button.";

		public const string? DownloadLink = "https://www.nexusmods.com/thelongdark/mods/54";
	}
}
namespace Pathoschild.TheLongDarkMods.FastTravel.Framework
{
	internal class Destination
	{
		public RegionModel? Region { get; }

		public SceneModel Scene { get; }

		public Vector3Model Position { get; }

		public float CameraPitch { get; }

		public float CameraYaw { get; }

		public TransitionModel LastTransition { get; }

		[JsonConstructor]
		public Destination(RegionModel? region, SceneModel scene, Vector3Model position, float cameraPitch, float cameraYaw, TransitionModel lastTransition)
		{
			Region = region;
			Scene = scene;
			Position = position;
			CameraPitch = cameraPitch;
			CameraYaw = cameraYaw;
			LastTransition = lastTransition;
		}

		public Destination(RegionSpecification? region, Scene scene, Vector3 position, float cameraPitch, float cameraYaw, SceneTransitionData lastTransition)
			: this(((Object)(object)region != (Object)null) ? new RegionModel(region) : null, new SceneModel(scene), new Vector3Model(position), cameraPitch, cameraYaw, new TransitionModel(lastTransition))
		{
		}//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)


		public string GetDisplayName(bool? showRegion = false)
		{
			string text = SceneHelper.GetDisplayName(Scene.Name);
			if (Region != null)
			{
				bool valueOrDefault = showRegion.GetValueOrDefault();
				if (!showRegion.HasValue)
				{
					int num;
					if (!SceneHelper.IsOutdoors(Scene.Name))
					{
						string id = Region.Id;
						RegionSpecification? obj = SceneHelper.TryGetRegion();
						num = ((id != ((obj != null) ? ((Object)obj).GetName() : null)) ? 1 : 0);
					}
					else
					{
						num = 0;
					}
					valueOrDefault = (byte)num != 0;
					showRegion = valueOrDefault;
				}
				if (showRegion.Value)
				{
					text = text + " in " + Localization.Get(Region.NameLocalizationId);
				}
			}
			return text;
		}

		public override string ToString()
		{
			return $"(scene: '{Scene.Name}' in '{Region?.Name ?? "<unknown>"}', position: {Position}, camera: ({CameraPitch}, {CameraYaw}), lastTransition: from '{LastTransition.FromSceneId}' to '{LastTransition.ToSceneId}')";
		}
	}
	[RegisterTypeInIl2Cpp]
	internal class DestinationListOverlay : MonoBehaviour
	{
		private const float Padding = 8f;

		private const float Margin = 10f;

		private string Text = "";

		private GUIStyle? LabelStyle;

		private Vector2 TextSize;

		public bool IsVisible { get; private set; }

		public DestinationListOverlay(IntPtr pointer)
			: base(pointer)
		{
		}

		public static DestinationListOverlay Create()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			GameObject val = new GameObject("FastTravel_DestinationListOverlay");
			Object.DontDestroyOnLoad((Object)(object)val);
			return val.AddComponent<DestinationListOverlay>();
		}

		public void Show(string text)
		{
			Reset(text);
		}

		public void Hide()
		{
			Reset(null);
		}

		public void OnGUI()
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_0053: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			if (IsVisible)
			{
				if (LabelStyle == null)
				{
					LabelStyle = new GUIStyle(GUI.skin.label)
					{
						fontSize = 13,
						padding = new RectOffset(0, 0, 0, 0)
					};
					TextSize = LabelStyle.CalcSize(new GUIContent(Text));
				}
				Vector2 textSize = TextSize;
				float num = textSize.x + 16f;
				float num2 = textSize.y + 16f;
				float num3 = (float)Screen.width - num - 10f;
				float num4 = (float)Screen.height - num2 - 10f;
				float num5 = textSize.x + 8f;
				GUI.Box(new Rect(num3, num4, num, num2), GUIContent.none);
				GUI.Label(new Rect(num3 + 8f, num4 + 8f, num5, textSize.y), Text, LabelStyle);
			}
		}

		private void Reset(string? text)
		{
			Text = text ?? "";
			LabelStyle = null;
			IsVisible = text != null;
		}
	}
	internal class DestinationManager
	{
		private readonly Instance Log;

		public DestinationManager(Instance log)
		{
			Log = log;
		}

		public SaveModel GetData()
		{
			ModDataManager modDataManager = CreateDataManager();
			SaveModel saveModel = DeserializeRaw(modDataManager.Load());
			return saveModel ?? new SaveModel();
		}

		public void SaveData(SaveModel data)
		{
			data.Version = "0.2.0";
			CreateDataManager().Save(Serialize(data));
		}

		public Destination GetCurrentLocation()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			vp_FPSCamera vpFPSCamera = GameManager.GetVpFPSCamera();
			Transform transform = GameManager.GetPlayerObject().transform;
			return new Destination(SceneHelper.TryGetRegion(), SceneHelper.GetScene(), transform.position, vpFPSCamera.m_Pitch, vpFPSCamera.m_Yaw, GameManager.m_SceneTransitionData);
		}

		private ModDataManager CreateDataManager()
		{
			return new ModDataManager("FastTravel", debug: false);
		}

		private SaveModel? DeserializeRaw(string? rawData)
		{
			if (rawData != null)
			{
				try
				{
					SaveModel saveModel = JsonSerializer.Deserialize<SaveModel>(rawData);
					if (saveModel?.Destinations != null)
					{
						return saveModel;
					}
				}
				catch (JsonException ex)
				{
					Log.Error("Can't load saved destinations; the data will be reset.", (Exception)ex);
				}
			}
			return null;
		}

		private string Serialize(SaveModel data)
		{
			return JsonSerializer.Serialize(data);
		}
	}
	internal class FastTravelTransition
	{
		public Destination From { get; }

		public Destination To { get; }

		public FastTravelTransition(Destination from, Destination to)
		{
			From = from;
			To = to;
		}
	}
	internal class ModConfig : JsonModSettings
	{
		[Section("限制条件")]
		[Name("允许传送")]
		[Description("是否完全启用快速传送功能\n\n如果开启（仅在已存点位间传送）的功能，会自动关闭该选项。")]
		public bool CanTravel = true;

		[Name("允许在室外传送")]
		[Description("是否允许在室外环境使用快速传送\n\n关闭此选项可避免你利用传送逃离危险处境，更贴合生存难度")]
		public bool CanTravelFromOutside;

		[Name("允许在非安全屋室内传送")]
		[Description("是否允许从不可自定义的室内场景（如洞穴）使用快速传送")]
		public bool CanTravelFromNonSafehouseInterior;

		[Name("允许传送到同区域点位")]
		[Description("是否允许在同一地图或区域内的传送点之间传送\n\n非预期功能，可能有问题！慎用！")]
		public bool CanTravelToSameScene;

		[Name("仅在已存点位间传送")]
		[Description("是否仅允许在你保存的传送点之间传送（例如仅在主据点之间移动）")]
		public bool OnlyBetweenDestinations;

		[Name("允许编辑传送点")]
		[Description("是否允许修改，新增，删除快速传送点位\n\n设置完成后关闭此选项，可防止误操作修改传送点")]
		public bool CanEditDestinations = true;

		[Section("组合快捷键")]
		[Name("保存传送点快捷键")]
		[Description("按住此键，再按下对应的数字键，即可将当前位置保存为快速传送点")]
		public KeyCode SaveModifierKey = (KeyCode)270;

		[Name("删除传送点快捷键")]
		[Description("按住此键，再按下对应的数字键，即可删除该传送点")]
		public KeyCode DeleteModifierKey = (KeyCode)269;

		[Section("快速传送按键")]
		[Name("显示传送点列表")]
		[Description("按下快捷键，查看已记录的存位点")]
		public KeyCode ShowListKey = (KeyCode)266;

		[Name("快速传送点 1")]
		[Description("按下此键，传送到第1个保存的传送点。可通过上方组合键修改该点位")]
		public KeyCode Destination1 = (KeyCode)257;

		[Name("快速传送点 2")]
		[Description("按下此键，传送到第2个保存的传送点")]
		public KeyCode Destination2 = (KeyCode)258;

		[Name("快速传送点 3")]
		[Description("按下此键，传送到第3个保存的传送点")]
		public KeyCode Destination3 = (KeyCode)259;

		[Name("快速传送点 4")]
		[Description("按下此键，传送到第4个保存的传送点")]
		public KeyCode Destination4 = (KeyCode)260;

		[Name("快速传送点 5")]
		[Description("按下此键，传送到第5个保存的传送点")]
		public KeyCode Destination5 = (KeyCode)261;

		[Name("快速传送点 6")]
		[Description("按下此键，传送到第6个保存的传送点")]
		public KeyCode Destination6 = (KeyCode)262;

		[Name("快速传送点 7")]
		[Description("按下此键，传送到第7个保存的传送点")]
		public KeyCode Destination7 = (KeyCode)263;

		[Name("快速传送点 8")]
		[Description("按下此键，传送到第8个保存的传送点")]
		public KeyCode Destination8 = (KeyCode)264;

		[Name("快速传送点 9")]
		[Description("按下此键，传送到第9个保存的传送点")]
		public KeyCode Destination9 = (KeyCode)265;

		[Name("返回上一位置")]
		[Description("按下此键，回到上一次传送之前的位置")]
		public KeyCode ReturnPointKey = (KeyCode)256;

		[Section("其他设置")]
		[Name("显示使用提示")]
		[Description("是否在游戏内显示操作提示，例如按什么<键>可返回此处\n\n熟悉模组后可关闭，减少破坏游戏沉浸感的提示")]
		public bool ShowUsageHints = true;

		[Name("记录调试信息")]
		[Description("是否记录场景切换和传送的调试日志。仅用于排查故障，不影响游戏内任何功能。")]
		public bool LogDebugInfo;
	}
}
namespace Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels
{
	internal class RegionModel
	{
		public string Id { get; }

		public string NameLocalizationId { get; }

		public string Name { get; }

		[JsonConstructor]
		public RegionModel(string id, string nameLocalizationId, string name)
		{
			Id = id;
			NameLocalizationId = nameLocalizationId;
			Name = name;
		}

		public RegionModel(RegionSpecification region)
			: this(((Object)region).GetName(), ((ZoneSpecification)region).ZoneNameId, ((ZoneSpecification)region).ZoneName)
		{
		}
	}
	internal class SaveModel
	{
		public string? Version { get; set; }

		public Destination? ReturnPoint { get; set; }

		public Dictionary<int, Destination> Destinations { get; set; } = new Dictionary<int, Destination>();


		public Destination? Get(int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", $"Invalid fast travel slot index {index}.");
			}
			return Destinations.GetValueOrDefault(index);
		}

		public void Set(int index, Destination? destination)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", $"Invalid fast travel slot index {index}.");
			}
			if (destination == null)
			{
				Destinations.Remove(index);
			}
			else
			{
				Destinations[index] = destination;
			}
		}
	}
	internal class SceneModel
	{
		public string Name { get; }

		public string Guid { get; }

		public string Path { get; }

		public bool IsSubScene { get; }

		[JsonConstructor]
		public SceneModel(string name, string guid, string path, bool isSubScene)
		{
			Name = name;
			Guid = guid;
			Path = path;
			IsSubScene = isSubScene;
		}

		public SceneModel(Scene scene)
			: this(((Scene)(ref scene)).name, ((Scene)(ref scene)).guid, ((Scene)(ref scene)).path, ((Scene)(ref scene)).isSubScene)
		{
		}
	}
	internal class TransitionModel
	{
		public string FromSceneId { get; }

		public string ToSceneId { get; }

		public string? ToSpawnPoint { get; }

		public string? ToSpawnPointAudio { get; }

		public bool RestorePlayerPosition { get; }

		public string? LastOutdoorScene { get; }

		public Vector3Model LastOutdoorPosition { get; }

		public int GameRandomSeed { get; }

		public string? ForceNextSceneLoadTriggerScene { get; }

		public string? SceneLocationLocIdOverride { get; }

		public string? Location { get; }

		[JsonConstructor]
		public TransitionModel(string fromSceneId, string toSceneId, string? toSpawnPoint, string? toSpawnPointAudio, bool restorePlayerPosition, string? lastOutdoorScene, Vector3Model lastOutdoorPosition, int gameRandomSeed, string? forceNextSceneLoadTriggerScene, string? sceneLocationLocIdOverride, string? location)
		{
			FromSceneId = fromSceneId;
			ToSceneId = toSceneId;
			ToSpawnPoint = toSpawnPoint;
			ToSpawnPointAudio = toSpawnPointAudio;
			RestorePlayerPosition = restorePlayerPosition;
			LastOutdoorScene = lastOutdoorScene;
			LastOutdoorPosition = lastOutdoorPosition;
			GameRandomSeed = gameRandomSeed;
			ForceNextSceneLoadTriggerScene = forceNextSceneLoadTriggerScene;
			SceneLocationLocIdOverride = sceneLocationLocIdOverride;
			Location = location;
		}

		public TransitionModel(SceneTransitionData transition)
			: this(transition.m_SceneSaveFilenameCurrent, transition.m_SceneSaveFilenameNextLoad, transition.m_SpawnPointName, transition.m_SpawnPointAudio, transition.m_TeleportPlayerSaveGamePosition, transition.m_LastOutdoorScene, new Vector3Model(transition.m_PosBeforeInteriorLoad), transition.m_GameRandomSeed, transition.m_ForceNextSceneLoadTriggerScene, transition.m_SceneLocationLocIDOverride, transition.m_Location)
		{
		}//IL_0026: Unknown result type (might be due to invalid IL or missing references)

	}
	internal struct Vector3Model
	{
		public float X { get; }

		public float Y { get; }

		public float Z { get; }

		[JsonConstructor]
		public Vector3Model(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3Model(Vector3 vector)
			: this(vector.x, vector.y, vector.z)
		{
		}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)


		public Vector3 ToVector3()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(X, Y, Z);
		}

		public override string ToString()
		{
			return $"({X}, {Y}, {Z})";
		}
	}
}
