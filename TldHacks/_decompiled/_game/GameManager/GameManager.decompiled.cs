using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Cpp2ILInjected;
using InteractiveObjects;
using TLD.AI;
using TLD.Gameplay;
using TLD.Gameplay.Condition;
using TLD.Gear;
using TLD.Scenes;
using TLD.Trader;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

[Token(Token = "0x200074C")]
public class GameManager : MonoBehaviour
{
	[Token(Token = "0x200074D")]
	public enum GameplayComponent
	{
		[Token(Token = "0x4004E27")]
		Sprains,
		[Token(Token = "0x4004E28")]
		Wind,
		[Token(Token = "0x4004E29")]
		BodyHarvest
	}

	[Token(Token = "0x200074E")]
	private class DelayedGameplayComponentInfo
	{
		[Token(Token = "0x4004E2A")]
		[FieldOffset(Offset = "0x10")]
		public readonly GameplayComponent m_Component;

		[Token(Token = "0x4004E2B")]
		[FieldOffset(Offset = "0x14")]
		public readonly int m_Frequency;

		[Token(Token = "0x4004E2C")]
		[FieldOffset(Offset = "0x18")]
		public int m_ValidFrame;

		[Token(Token = "0x4004E2D")]
		[FieldOffset(Offset = "0x1C")]
		public float m_DeltaTime;

		[Token(Token = "0x4004E2E")]
		[FieldOffset(Offset = "0x20")]
		public float m_AccumulatedDelta;

		[Token(Token = "0x600434A")]
		[Address(RVA = "0xC537A0", Offset = "0xC525A0", Length = "0x8")]
		public DelayedGameplayComponentInfo(GameplayComponent component, int frequency)
		{
		}
	}

	[Token(Token = "0x4004D1B")]
	[FieldOffset(Offset = "0x20")]
	public GameObject m_AiDifficultySettingsPrefab;

	[Token(Token = "0x4004D1C")]
	[FieldOffset(Offset = "0x28")]
	public GameObject m_AiSubsystemsPrefab;

	[Token(Token = "0x4004D1D")]
	[FieldOffset(Offset = "0x30")]
	public GameObject m_ConditionSystemsPrefab;

	[Token(Token = "0x4004D1E")]
	[FieldOffset(Offset = "0x38")]
	public GameObject m_EffectPoolManagerPrefab;

	[Token(Token = "0x4004D1F")]
	[FieldOffset(Offset = "0x40")]
	public GameObject m_TravoisTrailManagerPrefab;

	[Token(Token = "0x4004D20")]
	[FieldOffset(Offset = "0x48")]
	public GameObject m_EngineSystemsPrefab;

	[Token(Token = "0x4004D21")]
	[FieldOffset(Offset = "0x50")]
	public GameObject m_EnvironmentSystemsPrefab;

	[Token(Token = "0x4004D22")]
	[FieldOffset(Offset = "0x58")]
	public GameObject m_ExperienceModesPrefab;

	[Token(Token = "0x4004D23")]
	[FieldOffset(Offset = "0x60")]
	public GameObject m_FirstAidSystemsPrefab;

	[Token(Token = "0x4004D24")]
	[FieldOffset(Offset = "0x68")]
	public GameObject m_GamePlaySystemsPrefab;

	[Token(Token = "0x4004D25")]
	[FieldOffset(Offset = "0x70")]
	public GameObject m_PlayerSystemsPrefab;

	[Token(Token = "0x4004D26")]
	[FieldOffset(Offset = "0x78")]
	public GameObject m_PlayerObjectPrefab;

	[Token(Token = "0x4004D27")]
	[FieldOffset(Offset = "0x80")]
	public GameObject m_RumbleEffectManagerPrefab;

	[Token(Token = "0x4004D28")]
	[FieldOffset(Offset = "0x88")]
	public ConditionTable m_ConditionTable;

	[Token(Token = "0x4004D29")]
	[FieldOffset(Offset = "0x90")]
	public DialogueStatesTable m_DialogueStatesTable;

	[Token(Token = "0x4004D2A")]
	[FieldOffset(Offset = "0x98")]
	public MissionObjectiveTable m_MissionObjectiveTable;

	[Token(Token = "0x4004D2B")]
	[FieldOffset(Offset = "0xA0")]
	public AfflictionDefinitionTable m_AfflictionDefinitionTable;

	[Header("Switch Variants")]
	[Token(Token = "0x4004D2C")]
	[FieldOffset(Offset = "0xA8")]
	public GameObject m_GamePlaySystemsPrefab_Switch;

	[Token(Token = "0x4004D2D")]
	public const int NUM_SANDBOX_SAVE_SLOTS = 5;

	[Token(Token = "0x4004D2E")]
	public const int NUM_CHALLENGE_SAVE_SLOTS = 5;

	[Token(Token = "0x4004D2F")]
	public const int NUM_STORY_SAVE_SLOTS = 6;

	[Token(Token = "0x4004D30")]
	public static string m_GameVersionString;

	[Token(Token = "0x4004D31")]
	public static int s_ActiveChangelist;

	[Token(Token = "0x4004D32")]
	public static bool m_IsPaused;

	[Token(Token = "0x4004D33")]
	public static bool m_SuppressLocationReveal;

	[Token(Token = "0x4004D34")]
	public static bool m_UnpauseOneFrame;

	[Token(Token = "0x4004D35")]
	public static bool m_PauseNextFrame;

	[Token(Token = "0x4004D36")]
	public static float m_SceneTransitionFadeOutTime;

	[Token(Token = "0x4004D37")]
	public static float m_SecondsSinceLevelLoad;

	[Token(Token = "0x4004D38")]
	public static SceneTransitionData m_SceneTransitionData;

	[Token(Token = "0x4004D39")]
	public static bool m_ForceForwardRendering;

	[Token(Token = "0x4004D3A")]
	public static bool m_ForceDeferredRendering;

	[Token(Token = "0x4004D3B")]
	public static bool m_SceneWasRestored;

	[Token(Token = "0x4004D3C")]
	public static bool m_CheckPointSaveRequested;

	[Token(Token = "0x4004D3D")]
	public static bool m_PendingSave;

	[Token(Token = "0x4004D3E")]
	public static float m_PendingSaveSecondsAccumulated;

	[Token(Token = "0x4004D3F")]
	public static float m_MaxPendingSaveTimeBeforeError;

	[Token(Token = "0x4004D40")]
	public static bool m_PendingSaveGeneratedError;

	[Token(Token = "0x4004D41")]
	public static bool m_DisableSaveLoad;

	[Token(Token = "0x4004D42")]
	public static bool m_BlockNonMovementInput;

	[Token(Token = "0x4004D43")]
	public static float m_LockControlsTimer;

	[Token(Token = "0x4004D44")]
	public static bool m_BlockMoveInputUntilReleased;

	[Token(Token = "0x4004D45")]
	private const float m_MoveInputMagnitudeReleaseThreshold = 0.1f;

	[Token(Token = "0x4004D46")]
	private static float m_MoveInputDotProdReleaseThreshold;

	[Token(Token = "0x4004D47")]
	private static Vector2 m_MoveInputWhenBlockedVector;

	[Token(Token = "0x4004D48")]
	public static bool m_BlockAbilityToRest;

	[Token(Token = "0x4004D49")]
	public static string m_BlockedRestLocID;

	[Token(Token = "0x4004D4A")]
	public static bool m_BlockSceneTransition;

	[Token(Token = "0x4004D4B")]
	public static string m_BlockedTransitionLocID;

	[Token(Token = "0x4004D4C")]
	public static string m_ActiveScene;

	[Token(Token = "0x4004D4D")]
	public static bool m_Borderless;

	[Token(Token = "0x4004D4E")]
	public static bool m_CurrentEpisodeCompleted;

	[Token(Token = "0x4004D4F")]
	public static bool m_SuppressWeaponAim;

	[Token(Token = "0x4004D50")]
	public static bool m_PauseAudioWithGame;

	[Token(Token = "0x4004D51")]
	public static bool m_SkipFadeOnNextLoad;

	[Token(Token = "0x4004D52")]
	public static bool m_AllowSaveWithoutGrounding;

	[Token(Token = "0x4004D53")]
	public static SceneSet m_ActiveSceneSet;

	[Token(Token = "0x4004D54")]
	public static SceneSet m_LastOutdoorSceneSet;

	[Token(Token = "0x4004D55")]
	public static RegionSpecification m_StartRegion;

	[Token(Token = "0x4004D56")]
	public static string m_FallbackRegion;

	[Token(Token = "0x4004D57")]
	public static bool s_ForcedGC;

	[Token(Token = "0x4004D58")]
	public static bool s_ForcedGCVerbose;

	[Token(Token = "0x4004D59")]
	public static bool s_IsGameplaySuspended;

	[Token(Token = "0x4004D5A")]
	public static bool s_IsAISuspended;

	[Token(Token = "0x4004D5C")]
	private static bool s_CurrentCultureSet;

	[Token(Token = "0x4004D5D")]
	private static float s_ForcedGCDelay;

	[Token(Token = "0x4004D5E")]
	private static float s_ForcedGCInterval;

	[Token(Token = "0x4004D5F")]
	public static bool s_VerboseAllowedToSave;

	[Token(Token = "0x4004D60")]
	private static float s_SaveGameDelayTime;

	[Token(Token = "0x4004D61")]
	private static Camera m_MainCamera;

	[Token(Token = "0x4004D62")]
	private static Camera m_WeaponCamera;

	[Token(Token = "0x4004D63")]
	private static Camera m_ImageEffectCamera;

	[Token(Token = "0x4004D64")]
	private static Camera m_InspectModeCamera;

	[Token(Token = "0x4004D65")]
	private static Camera m_GlobalCamera;

	[Token(Token = "0x4004D66")]
	private static CameraGlobalRT m_CameraGlobalRT;

	[Token(Token = "0x4004D67")]
	private static Light m_InspectModeLight;

	[Token(Token = "0x4004D68")]
	private static vp_FPSCamera m_vpFPSCamera;

	[Token(Token = "0x4004D69")]
	private static vp_FPSPlayer m_vpFPSPlayer;

	[Token(Token = "0x4004D6A")]
	private static NavMeshObstacle m_PlayerNavMeshObstacle;

	[Token(Token = "0x4004D6B")]
	private static CameraEffects m_CameraEffects;

	[Token(Token = "0x4004D6C")]
	private static CameraStatusEffects m_CameraStatusEffects;

	[Token(Token = "0x4004D6D")]
	private static GameObject m_PlayerObject;

	[Token(Token = "0x4004D6E")]
	private static PlayerCameraAnim m_NewPlayerCameraAnim;

	[Token(Token = "0x4004D6F")]
	private static PlayerAnimation m_NewPlayerAnimation;

	[Token(Token = "0x4004D70")]
	private static DialogueStatesTable m_GlobalDialogueStatesTable;

	[Token(Token = "0x4004D71")]
	private static MissionObjectiveTable m_GlobalMissionObjectiveTable;

	[Token(Token = "0x4004D72")]
	private static AfflictionDefinitionTable m_GlobalAfflictionDefinitionTable;

	[Token(Token = "0x4004D73")]
	private static GameObject m_WeaponViewObject;

	[Token(Token = "0x4004D74")]
	private static bool m_ForceTrialMode;

	[Token(Token = "0x4004D75")]
	private static bool m_PausedWhenFocusLost;

	[Token(Token = "0x4004D76")]
	public static float m_GlobalTimeScale;

	[Token(Token = "0x4004D77")]
	public static Vector3 m_DefaultSpawnPos;

	[Token(Token = "0x4004D78")]
	public static Quaternion m_DefaultSpawnRot;

	[Token(Token = "0x4004D79")]
	public static string m_OverridenCauseOfDeath;

	[Token(Token = "0x4004D7A")]
	private static TimeSpan m_ServerTimeOffset;

	[Token(Token = "0x4004D7B")]
	public static Dictionary<string, bool> m_AsyncSceneLoadsInProgress;

	[Token(Token = "0x4004D7C")]
	public static ThreadPriority m_LoadPriority;

	[Token(Token = "0x4004D7D")]
	private static AuroraManager m_AuroraManager;

	[Token(Token = "0x4004D7E")]
	private static AchievementManager m_AchievementManager;

	[Token(Token = "0x4004D7F")]
	private static AiDifficultySettings m_AiDifficultySettings;

	[Token(Token = "0x4004D80")]
	private static Burns m_Burns;

	[Token(Token = "0x4004D81")]
	private static BurnsElectric m_BurnsElectric;

	[Token(Token = "0x4004D82")]
	private static BloodLoss m_BloodLoss;

	[Token(Token = "0x4004D83")]
	private static BodyHarvestManager m_BodyHarvestManager;

	[Token(Token = "0x4004D84")]
	private static Breath m_Breath;

	[Token(Token = "0x4004D85")]
	private static BrokenRib m_BrokenRib;

	[Token(Token = "0x4004D86")]
	private static BrokenBody m_BrokenBody;

	[Token(Token = "0x4004D87")]
	private static CabinFever m_CabinFever;

	[Token(Token = "0x4004D88")]
	private static CheatDeathAffliction m_CheatDeathAffliction;

	[Token(Token = "0x4004D89")]
	private static Condition m_Condition;

	[Token(Token = "0x4004D8A")]
	private static DiminishedState m_DiminishedState;

	[Token(Token = "0x4004D8B")]
	private static DownsampleAurora m_DownsampleAurora;

	[Token(Token = "0x4004D8C")]
	private static DynamicDecalsManager m_DynamicDecalsManager;

	[Token(Token = "0x4004D8D")]
	private static Dysentery m_Dysentery;

	[Token(Token = "0x4004D8E")]
	private static EffectPoolManager m_EffectPoolManager;

	[Token(Token = "0x4004D8F")]
	private static EmergencyStim m_EmergencyStim;

	[Token(Token = "0x4004D90")]
	private static Encumber m_Encumber;

	[Token(Token = "0x4004D91")]
	private static EnergyBoost m_EnergyBoost;

	[Token(Token = "0x4004D92")]
	private static ExperienceModeManager m_ExperienceModeManager;

	[Token(Token = "0x4004D93")]
	private static Fatigue m_Fatigue;

	[Token(Token = "0x4004D94")]
	private static FireManager m_FireManager;

	[Token(Token = "0x4004D95")]
	private static FoodPoisoning m_FoodPoisoning;

	[Token(Token = "0x4004D96")]
	private static FootStepSounds m_FootStepSounds;

	[Token(Token = "0x4004D97")]
	private static Freezing m_Freezing;

	[Token(Token = "0x4004D98")]
	private static Frostbite m_Frostbite;

	[Token(Token = "0x4004D99")]
	private static Suffocating m_Suffocating;

	[Token(Token = "0x4004D9A")]
	private static HeatSourceManager m_HeatSourceManager;

	[Token(Token = "0x4004D9B")]
	private static Headache m_Headache;

	[Token(Token = "0x4004D9C")]
	private static Hunger m_Hunger;

	[Token(Token = "0x4004D9D")]
	private static Hypothermia m_Hypothermia;

	[Token(Token = "0x4004D9E")]
	private static Infection m_Infection;

	[Token(Token = "0x4004D9F")]
	private static InfectionRisk m_InfectionRisk;

	[Token(Token = "0x4004DA0")]
	private static IntestinalParasites m_IntestinalParasites;

	[Token(Token = "0x4004DA1")]
	private static FallDamage m_FallDamage;

	[Token(Token = "0x4004DA2")]
	private static FeatsManager m_FeatsManager;

	[Token(Token = "0x4004DA3")]
	private static FeatNotify m_FeatNotify;

	[Token(Token = "0x4004DA4")]
	private static FlareIntensityManager m_FlareIntensityManager;

	[Token(Token = "0x4004DA5")]
	private static FlyOver m_FlyOver;

	[Token(Token = "0x4004DA6")]
	private static FootstepTrailManager m_FootstepTrailManager;

	[Token(Token = "0x4004DA7")]
	private static GlobalParameters m_GlobalParameters;

	[Token(Token = "0x4004DA8")]
	private static HeatPadBuff m_HeatPadBuff;

	[Token(Token = "0x4004DA9")]
	private static IceCrackingManager m_IceCrackingManager;

	[Token(Token = "0x4004DAA")]
	private static InteractiveClothManager m_InteractiveClothManager;

	[Token(Token = "0x4004DAB")]
	private static Inventory m_Inventory;

	[Token(Token = "0x4004DAC")]
	private static LifeAfterDeathManager m_LifeAfterDeathManager;

	[Token(Token = "0x4004DAD")]
	private static Log m_Log;

	[Token(Token = "0x4004DAE")]
	private static MapDetailManager m_MapDetailManager;

	[Token(Token = "0x4004DAF")]
	private static MiseryManager m_MiseryManager;

	[Token(Token = "0x4004DB0")]
	private static MusicEventManager m_MusicEventManager;

	[Token(Token = "0x4004DB1")]
	private static PassTime m_PassTime;

	[Token(Token = "0x4004DB2")]
	private static PlayerClimbRope m_PlayerClimbRope;

	[Token(Token = "0x4004DB3")]
	private static PlayerGameStats m_PlayerGameStats;

	[Token(Token = "0x4004DB4")]
	private static PlayerInVehicle m_PlayerInVehicle;

	[Token(Token = "0x4004DB5")]
	private static PlayerInConstrainedCamera m_PlayerInConstrainedCamera;

	[Token(Token = "0x4004DB6")]
	private static PlayerKnowledge m_PlayerKnowledge;

	[Token(Token = "0x4004DB7")]
	private static PlayerManager m_PlayerManager;

	[Token(Token = "0x4004DB8")]
	private static PlayerMovement m_PlayerMovement;

	[Token(Token = "0x4004DB9")]
	private static PlayerSkills m_PlayerSkills;

	[Token(Token = "0x4004DBA")]
	private static PlayerStruggle m_PlayerStruggle;

	[Token(Token = "0x4004DBB")]
	private static PlayerStunned m_PlayerStunned;

	[Token(Token = "0x4004DBC")]
	private static PlayerSwing m_PlayerSwing;

	[Token(Token = "0x4004DBD")]
	private static PlayerVoice m_PlayerVoice;

	[Token(Token = "0x4004DBE")]
	private static PoorCirculation m_PoorCirculation;

	[Token(Token = "0x4004DBF")]
	private static QualitySettingsManager m_QualitySettingsManager;

	[Token(Token = "0x4004DC0")]
	private static RadialSpawnManager m_RadialSpawnManager;

	[Token(Token = "0x4004DC1")]
	private static RenderTextureCameraManager m_RenderTextureCameraManager;

	[Token(Token = "0x4004DC2")]
	private static Rest m_Rest;

	[Token(Token = "0x4004DC3")]
	private static RumbleEffectManager m_RumbleEffectManager;

	[Token(Token = "0x4004DC4")]
	private static ScentRanges m_ScentRanges;

	[Token(Token = "0x4004DC5")]
	private static SevereLacerations m_SevereLacerations;

	[Token(Token = "0x4004DC6")]
	private static SkillNotify m_SkillNotify;

	[Token(Token = "0x4004DC7")]
	private static SkillsManager m_SkillsManager;

	[Token(Token = "0x4004DC8")]
	private static SnowPatchManager m_SnowPatchManager;

	[Token(Token = "0x4004DC9")]
	private static SnowShelterManager m_SnowShelterManager;

	[Token(Token = "0x4004DCA")]
	private static RockCacheManager m_RockCacheManager;

	[Token(Token = "0x4004DCB")]
	private static SpawnRegionManager m_SpawnRegionManager;

	[Token(Token = "0x4004DCC")]
	private static SprainedAnkle m_SprainedAnkle;

	[Token(Token = "0x4004DCD")]
	private static SprainedWrist m_SprainedWrist;

	[Token(Token = "0x4004DCE")]
	private static SprainPain m_SprainPain;

	[Token(Token = "0x4004DCF")]
	private static Sprains m_Sprains;

	[Token(Token = "0x4004DD0")]
	private static SourStomach m_SourStomach;

	[Token(Token = "0x4004DD1")]
	private static Anxiety m_Anxiety;

	[Token(Token = "0x4004DD2")]
	private static Fear m_Fear;

	[Token(Token = "0x4004DD3")]
	private static ToxicFog m_ToxicFog;

	[Token(Token = "0x4004DD4")]
	private static StartSettings m_StartSettings;

	[Token(Token = "0x4004DD5")]
	private static StatsManager m_StatsManager;

	[Token(Token = "0x4004DD6")]
	private static TerrainGrassModifier m_TerrainGrassModifier;

	[Token(Token = "0x4004DD7")]
	private static TerrainRenderingManager m_TerrainRenderingManager;

	[Token(Token = "0x4004DD8")]
	private static Thirst m_Thirst;

	[Token(Token = "0x4004DD9")]
	private static TLD_TimelineDirector m_TimelineDirector;

	[Token(Token = "0x4004DDA")]
	private static TimeOfDay m_TimeOfDay;

	[Token(Token = "0x4004DDB")]
	private static TraderManager m_TraderManager;

	[Token(Token = "0x4004DDC")]
	private static TravoisTrailManager m_TravoisTrailManager;

	[Token(Token = "0x4004DDD")]
	private static TravoisStampingManager m_TravoisStampingManager;

	[Token(Token = "0x4004DDE")]
	private static UnsettledSleep m_UnsettledSleep;

	[Token(Token = "0x4004DDF")]
	private static WeakConstitution m_WeakConstitution;

	[Token(Token = "0x4004DE0")]
	private static WeakJoints m_WeakJoints;

	[Token(Token = "0x4004DE1")]
	private static Weather m_Weather;

	[Token(Token = "0x4004DE2")]
	private static WeatherTransition m_WeatherTransition;

	[Token(Token = "0x4004DE3")]
	private static WellFed m_WellFed;

	[Token(Token = "0x4004DE4")]
	private static Willpower m_Willpower;

	[Token(Token = "0x4004DE5")]
	private static Wind m_Wind;

	[Token(Token = "0x4004DE6")]
	private static WindZone m_WindZone;

	[Token(Token = "0x4004DE7")]
	private static PackManager m_PackManager;

	[Token(Token = "0x4004DE8")]
	private static AreaMarkupManager m_AreaMarkupManager;

	[Token(Token = "0x4004DE9")]
	private static FastClothManager m_FastClothManager;

	[Token(Token = "0x4004DEA")]
	private static DebugViewModeManager m_DebugViewModeManager;

	[Token(Token = "0x4004DEB")]
	private static InvisibleEntityManager m_InvisibleEntityManager;

	[Token(Token = "0x4004DEC")]
	private static ToxicFogManager m_ToxicFogManager;

	[Token(Token = "0x4004DED")]
	private static SprainProtection m_Sprainprotection;

	[Token(Token = "0x4004DEE")]
	private static SteamPipeEffectManager m_SteamPipeEffectManager;

	[Token(Token = "0x4004DEF")]
	private static SteamPipeValveManager m_SteamPipeValveManager;

	[Token(Token = "0x4004DF0")]
	private static SteamPipeManager m_SteamPipeManager;

	[Token(Token = "0x4004DF1")]
	private static PlayerCough m_PlayerCough;

	[Token(Token = "0x4004DF2")]
	private static HighResolutionTimerManager m_HighResTimerManager;

	[Token(Token = "0x4004DF3")]
	private static FontManager m_FontManager;

	[Token(Token = "0x4004DF4")]
	private static NotificationFlagManager m_NotificationFlagManager;

	[Token(Token = "0x4004DF5")]
	private static DamageProtection m_DamageProtection;

	[Token(Token = "0x4004DF6")]
	private static InsomniaManager m_InsomniaManager;

	[Token(Token = "0x4004DF7")]
	private static TrackableItemsManager m_TrackableItemsManager;

	[Token(Token = "0x4004DF8")]
	private static TransmitterManager m_TransmitterManager;

	[Token(Token = "0x4004DF9")]
	private static ChemicalPoisoning m_ChemicalPoisoning;

	[Token(Token = "0x4004DFA")]
	private static ScurvyManager m_ScurvyManager;

	[Token(Token = "0x4004DFB")]
	private static RespiratorManager m_RespiratorManager;

	[Token(Token = "0x4004DFC")]
	private static CougarManager m_CougarManager;

	[Token(Token = "0x4004DFD")]
	private static SafehouseManager m_SafehouseManager;

	[Token(Token = "0x4004DFE")]
	private static ParticleActivationManager m_ParticleActivationManager;

	[Token(Token = "0x4004DFF")]
	private static TerrainMaterialBlendingManager m_TerrainMaterialBlendingManager;

	[Token(Token = "0x4004E00")]
	private static PhotoManager m_PhotoManager;

	[Token(Token = "0x4004E01")]
	private static JunkManager m_JunkManager;

	[Token(Token = "0x4004E02")]
	private static MeshDistanceFieldManager m_MeshDistanceFieldManager;

	[Token(Token = "0x4004E03")]
	private static GameObject m_InputSystems;

	[Token(Token = "0x4004E04")]
	private static GameObject m_Console;

	[Token(Token = "0x4004E05")]
	[FieldOffset(Offset = "0xB0")]
	private GameObject m_ConditionSystems;

	[Token(Token = "0x4004E06")]
	[FieldOffset(Offset = "0xB8")]
	private GameObject m_EngineSystems;

	[Token(Token = "0x4004E07")]
	[FieldOffset(Offset = "0xC0")]
	private GameObject m_EnvironmentSystems;

	[Token(Token = "0x4004E08")]
	[FieldOffset(Offset = "0xC8")]
	private GameObject m_ExperienceModes;

	[Token(Token = "0x4004E09")]
	[FieldOffset(Offset = "0xD0")]
	private GameObject m_FirstAidSystems;

	[Token(Token = "0x4004E0A")]
	[FieldOffset(Offset = "0xD8")]
	private GameObject m_GamePlaySystems;

	[Token(Token = "0x4004E0B")]
	[FieldOffset(Offset = "0xE0")]
	private GameObject m_PlayerSystems;

	[Token(Token = "0x4004E0C")]
	[FieldOffset(Offset = "0xE8")]
	private bool m_DoBurntHouseCheckNextFrame;

	[Token(Token = "0x4004E0D")]
	[FieldOffset(Offset = "0xE9")]
	private bool m_SetQualitySettingsForLoadedScene;

	[Token(Token = "0x4004E0E")]
	[FieldOffset(Offset = "0xEA")]
	private bool m_SetAudioModeForLoadedScene;

	[Token(Token = "0x4004E0F")]
	[FieldOffset(Offset = "0xEC")]
	private float m_HeatmapTimer;

	[Token(Token = "0x4004E10")]
	private static GameObject m_SteamSystems;

	[Token(Token = "0x4004E11")]
	private static GameObject m_EOSSystems;

	[Token(Token = "0x4004E12")]
	private static GameObject m_EffectPoolManagerObject;

	[Token(Token = "0x4004E13")]
	private static GameObject m_TravoisTrailManagerObject;

	[Token(Token = "0x4004E14")]
	private static GameObject m_RumbleEffectManagerObject;

	[Token(Token = "0x4004E15")]
	private static bool m_FirstRun;

	[Token(Token = "0x4004E16")]
	private static bool m_HasCalledInitializeUser;

	[Token(Token = "0x4004E17")]
	private static GameManager m_Instance;

	[Token(Token = "0x4004E18")]
	private static int m_SaveFrameDelay;

	[Token(Token = "0x4004E19")]
	private static bool m_InitialEditorSceneCheckComplete;

	[Token(Token = "0x4004E1A")]
	private static bool s_AllowPhysicsSimulationControl;

	[Token(Token = "0x4004E1B")]
	private static bool m_TempResolutionChangeRequested;

	[Token(Token = "0x4004E1C")]
	private static SaveSlotInfo m_QuickLoadSlot;

	[Token(Token = "0x4004E1D")]
	private static EpisodeTransferData m_EpisodeTransferData;

	[Token(Token = "0x4004E1E")]
	private static bool s_PausedWhenFocusLost;

	[Token(Token = "0x4004E1F")]
	private static DelayedGameplayComponentInfo[] s_DelayedGameplayComponentInfos;

	[Token(Token = "0x4004E20")]
	public const string EMPTY = "Empty";

	[Token(Token = "0x4004E21")]
	public const string MAIN_MENU = "MainMenu";

	[Token(Token = "0x4004E22")]
	private static bool s_ActiveIsEmpty;

	[Token(Token = "0x4004E23")]
	private static bool s_ActiveIsMainMenu;

	[Token(Token = "0x4004E24")]
	private const StringComparison NAME_COMPARISON = StringComparison.OrdinalIgnoreCase;

	[Token(Token = "0x4004E25")]
	private const string DATA_SET_NAME = "MainMenuDataSet";

	[Token(Token = "0x17000320")]
	[field: Token(Token = "0x4004D5B")]
	public static CultureInfo s_CurrentCulture
	{
		[Token(Token = "0x6004201")]
		[Address(RVA = "0xC31AB0", Offset = "0xC308B0", Length = "0x57")]
		get;
		[Token(Token = "0x6004202")]
		[Address(RVA = "0xC31B10", Offset = "0xC30910", Length = "0xB7")]
		private set;
	}

	[Token(Token = "0x1400000D")]
	public static event Action OnGameQuitEvent
	{
		[CompilerGenerated]
		[Token(Token = "0x60041FF")]
		[Address(RVA = "0xC31810", Offset = "0xC30610", Length = "0x14F")]
		add
		{
		}
		[CompilerGenerated]
		[Token(Token = "0x6004200")]
		[Address(RVA = "0xC31960", Offset = "0xC30760", Length = "0x14F")]
		remove
		{
		}
	}

	[Token(Token = "0x6004203")]
	[Address(RVA = "0xC31BD0", Offset = "0xC309D0", Length = "0x655")]
	private void InstantiateSystems()
	{
	}

	[Token(Token = "0x6004204")]
	[Address(RVA = "0xC32230", Offset = "0xC31030", Length = "0x4C")]
	public static void InstantiateInterfaceSystem()
	{
	}

	[Token(Token = "0x6004205")]
	[Address(RVA = "0xC32280", Offset = "0xC31080", Length = "0x3A3")]
	public static void InstantiateInputSystem()
	{
	}

	[Token(Token = "0x6004206")]
	[Address(RVA = "0xC32630", Offset = "0xC31430", Length = "0x4B1")]
	public static void InstantiateOnlineSystems()
	{
	}

	[Token(Token = "0x6004207")]
	[Address(RVA = "0xC32AF0", Offset = "0xC318F0", Length = "0x17B")]
	public static void InstantiateMissionServicesManager()
	{
	}

	[Token(Token = "0x6004208")]
	[Address(RVA = "0xC32C70", Offset = "0xC31A70", Length = "0x58D")]
	public static void ResetGameState()
	{
	}

	[Token(Token = "0x6004209")]
	[Address(RVA = "0xC33200", Offset = "0xC32000", Length = "0x137")]
	public static void DestroyMissionServicesManager()
	{
	}

	[Token(Token = "0x600420A")]
	[Address(RVA = "0xC33340", Offset = "0xC32140", Length = "0x301")]
	public static void EarlyUpdate()
	{
	}

	[Token(Token = "0x600420B")]
	[Address(RVA = "0xC33650", Offset = "0xC32450", Length = "0xE9E")]
	public void Awake()
	{
	}

	[Token(Token = "0x600420C")]
	[Address(RVA = "0xC344F0", Offset = "0xC332F0", Length = "0x45")]
	private void OnOptionalContentLoaded(IResourceLocator obj)
	{
	}

	[Token(Token = "0x600420D")]
	[Address(RVA = "0xC34540", Offset = "0xC33340", Length = "0x25EF")]
	private static void ResetLists()
	{
	}

	[Token(Token = "0x600420E")]
	[Address(RVA = "0xC36B30", Offset = "0xC35930", Length = "0xE4E")]
	public void Start()
	{
	}

	[Token(Token = "0x600420F")]
	[Address(RVA = "0xC37980", Offset = "0xC36780", Length = "0x12DE")]
	public void Update()
	{
	}

	[Token(Token = "0x6004210")]
	[Address(RVA = "0xC38C60", Offset = "0xC37A60", Length = "0xC8")]
	public static void AllScenesLoaded()
	{
	}

	[Token(Token = "0x6004211")]
	[Address(RVA = "0xC38D30", Offset = "0xC37B30", Length = "0x2F9")]
	public void LateUpdate()
	{
	}

	[Token(Token = "0x6004212")]
	[Address(RVA = "0xC39030", Offset = "0xC37E30", Length = "0x4F7")]
	private void OnDestroy()
	{
	}

	[Token(Token = "0x6004213")]
	[Address(RVA = "0xC39530", Offset = "0xC38330", Length = "0xB")]
	private bool CanMigrateEnteringEpisode(Episode episode)
	{
		return false;
	}

	[Token(Token = "0x6004214")]
	[Address(RVA = "0xC39540", Offset = "0xC38340", Length = "0x120")]
	public static void ForceGC()
	{
	}

	[Token(Token = "0x6004215")]
	[Address(RVA = "0xC39660", Offset = "0xC38460", Length = "0x26E")]
	public static void MaybeForceGC()
	{
	}

	[Token(Token = "0x6004216")]
	[Address(RVA = "0xC398D0", Offset = "0xC386D0", Length = "0x57")]
	public static void RequestTempResolutionChange()
	{
	}

	[Token(Token = "0x6004217")]
	[Address(RVA = "0xC39930", Offset = "0xC38730", Length = "0x352")]
	public static string Serialize()
	{
		return null;
	}

	[Token(Token = "0x6004218")]
	[Address(RVA = "0xC39C90", Offset = "0xC38A90", Length = "0x52D")]
	public static void Deserialize(string serialized)
	{
	}

	[Token(Token = "0x6004219")]
	[Address(RVA = "0xC3A1C0", Offset = "0xC38FC0", Length = "0x57")]
	public static void ReInitDefaultOptions()
	{
	}

	[Token(Token = "0x600421A")]
	[Address(RVA = "0xC3A220", Offset = "0xC39020", Length = "0x6C9")]
	public void OnApplicationQuit()
	{
	}

	[Token(Token = "0x600421B")]
	[Address(RVA = "0xC3A8F0", Offset = "0xC396F0", Length = "0x5E")]
	public static bool SaveGameCanBeLoaded(string slotName)
	{
		return false;
	}

	[Token(Token = "0x600421C")]
	[Address(RVA = "0xC3A950", Offset = "0xC39750", Length = "0x2B0")]
	public static bool SaveGameCanBeLoaded(string slotName, out string saveErrorMessage)
	{
		saveErrorMessage = null;
		return false;
	}

	[Token(Token = "0x600421D")]
	[Address(RVA = "0xC3AC10", Offset = "0xC39A10", Length = "0x285")]
	private static bool SaveShouldBePending()
	{
		return false;
	}

	[Token(Token = "0x600421E")]
	[Address(RVA = "0xC3AEA0", Offset = "0xC39CA0", Length = "0x2BF")]
	private static void SaveGameAndDisplayHUDMessage()
	{
	}

	[Token(Token = "0x600421F")]
	[Address(RVA = "0xC3B160", Offset = "0xC39F60", Length = "0x103")]
	public static void SaveProfileAndDisplayHUDMessage()
	{
	}

	[Token(Token = "0x6004220")]
	[Address(RVA = "0xC3B270", Offset = "0xC3A070", Length = "0x26A")]
	public static void SaveProfileSettingsAndDisplayHUDMessage()
	{
	}

	[Token(Token = "0x6004221")]
	[Address(RVA = "0xC3B4E0", Offset = "0xC3A2E0", Length = "0xDC")]
	public static void TriggerAutosaveAndDisplayHUDMessage()
	{
	}

	[Token(Token = "0x6004222")]
	[Address(RVA = "0xC3B5C0", Offset = "0xC3A3C0", Length = "0xDC")]
	public static void TriggerQuicksaveAndDisplayHUDMessage()
	{
	}

	[Token(Token = "0x6004223")]
	[Address(RVA = "0xC3B6A0", Offset = "0xC3A4A0", Length = "0x65")]
	public static void TriggerManualSaveAndDisplayHUDMessage(string saveName, string displayName)
	{
	}

	[Token(Token = "0x6004224")]
	[Address(RVA = "0xC3B710", Offset = "0xC3A510", Length = "0x5A")]
	public static void TriggerSurvivalSaveAndDisplayHUDMessage()
	{
	}

	[Token(Token = "0x6004225")]
	[Address(RVA = "0xC3B770", Offset = "0xC3A570", Length = "0x45")]
	public static bool ShouldPauseMoviePlayer()
	{
		return false;
	}

	[Token(Token = "0x6004226")]
	[Address(RVA = "0xC3B7C0", Offset = "0xC3A5C0", Length = "0x944")]
	public static bool AllowedToSave(SaveState state)
	{
		return false;
	}

	[Token(Token = "0x6004227")]
	[Address(RVA = "0xC3C110", Offset = "0xC3AF10", Length = "0x253")]
	public static bool AllowedToLoadActiveGame()
	{
		return false;
	}

	[Token(Token = "0x6004228")]
	[Address(RVA = "0xC3C370", Offset = "0xC3B170", Length = "0xB2")]
	public static void DelaySaving(float delay)
	{
	}

	[Token(Token = "0x6004229")]
	[Address(RVA = "0xC3C430", Offset = "0xC3B230", Length = "0x6C")]
	public static void SaveGame()
	{
	}

	[Token(Token = "0x600422A")]
	[Address(RVA = "0xC3C4A0", Offset = "0xC3B2A0", Length = "0x217")]
	public static void ForceSaveGame()
	{
	}

	[Token(Token = "0x600422B")]
	[Address(RVA = "0xC3C6C0", Offset = "0xC3B4C0", Length = "0xE2")]
	public static bool LoadSaveGameSlot(SaveSlotInfo ssi)
	{
		return false;
	}

	[Token(Token = "0x600422C")]
	[Address(RVA = "0xC3C7B0", Offset = "0xC3B5B0", Length = "0x2E4")]
	public static bool LoadSaveGameSlot(string slotName, int saveChangelistVersion)
	{
		return false;
	}

	[Token(Token = "0x600422D")]
	[Address(RVA = "0xC3CAA0", Offset = "0xC3B8A0", Length = "0x47E")]
	public static bool LoadGame(SaveSlotInfo ssi)
	{
		return false;
	}

	[Token(Token = "0x600422E")]
	[Address(RVA = "0x407D80", Offset = "0x406B80", Length = "0x3")]
	public static void ContinueToNextEpisode()
	{
	}

	[Token(Token = "0x600422F")]
	[Address(RVA = "0xC3CF20", Offset = "0xC3BD20", Length = "0x24E")]
	private static void SetupTransferData()
	{
	}

	[Token(Token = "0x6004230")]
	[Address(RVA = "0xC3D170", Offset = "0xC3BF70", Length = "0x220")]
	private static void ApplyTransferData()
	{
	}

	[Token(Token = "0x6004231")]
	[Address(RVA = "0xC3D3A0", Offset = "0xC3C1A0", Length = "0x38B")]
	private static void OnLoadGameCallback()
	{
	}

	[Token(Token = "0x6004232")]
	[Address(RVA = "0xC3D730", Offset = "0xC3C530", Length = "0x36D")]
	private static void OnLoadStoryFromEmptyCallback()
	{
	}

	[Token(Token = "0x6004233")]
	[Address(RVA = "0xC3DAA0", Offset = "0xC3C8A0", Length = "0xDA")]
	public static void FadeOutSceneAudio()
	{
	}

	[Token(Token = "0x6004234")]
	[Address(RVA = "0xC3DB80", Offset = "0xC3C980", Length = "0x2EF")]
	public static void LoadScene(string sceneName, string sceneSaveFilenameCurrent)
	{
	}

	[Token(Token = "0x6004235")]
	[Address(RVA = "0xC3DE70", Offset = "0xC3CC70", Length = "0x2B6")]
	public static void LoadSceneWithLoadingScreen(string sceneName)
	{
	}

	[Token(Token = "0x6004236")]
	[Address(RVA = "0xC3E130", Offset = "0xC3CF30", Length = "0xCE3")]
	public static void LoadSceneAsynchronously(string sceneName)
	{
	}

	[Token(Token = "0x6004237")]
	[Address(RVA = "0xC3EE20", Offset = "0xC3DC20", Length = "0x1F7")]
	public static void AddAsyncLoadRequest(SceneSet sceneSet, List<AsyncOperationHandle<SceneInstance>> asyncOps)
	{
	}

	[Token(Token = "0x6004238")]
	[Address(RVA = "0xC3F020", Offset = "0xC3DE20", Length = "0x32E")]
	public static void AddAsyncLoadRequest(string sceneName, List<AsyncOperationHandle<SceneInstance>> asyncOps)
	{
	}

	[Token(Token = "0x6004239")]
	[Address(RVA = "0xC3F350", Offset = "0xC3E150", Length = "0x140")]
	public static void RefreshDynamicConditionalScenes(List<AsyncOperationHandle<SceneInstance>> asyncOperations)
	{
	}

	[Token(Token = "0x600423A")]
	[Address(RVA = "0xC3F4A0", Offset = "0xC3E2A0", Length = "0x294")]
	public static RegionSpecification TryGetCurrentRegion()
	{
		return null;
	}

	[Token(Token = "0x600423B")]
	[Address(RVA = "0xC3F740", Offset = "0xC3E540", Length = "0x54")]
	public static string GetVersionString()
	{
		return null;
	}

	[Token(Token = "0x600423C")]
	[Address(RVA = "0xC3F7A0", Offset = "0xC3E5A0", Length = "0x5A")]
	public static bool SaveIsBlockedDueToRestoreGame()
	{
		return false;
	}

	[Token(Token = "0x600423D")]
	[Address(RVA = "0xC3F800", Offset = "0xC3E600", Length = "0x282")]
	public static void LoadMainMenu()
	{
	}

	[Token(Token = "0x600423E")]
	[Address(RVA = "0xC3FA90", Offset = "0xC3E890", Length = "0x57D")]
	public static void OnGameQuit()
	{
	}

	[Token(Token = "0x600423F")]
	[Address(RVA = "0xC40010", Offset = "0xC3EE10", Length = "0x6D")]
	public static string StripOptFromSceneName(string sceneName)
	{
		return null;
	}

	[Token(Token = "0x6004240")]
	[Address(RVA = "0xC40080", Offset = "0xC3EE80", Length = "0xE7")]
	public static bool AreControlsLockedForIntro()
	{
		return false;
	}

	[Token(Token = "0x6004241")]
	[Address(RVA = "0xC40170", Offset = "0xC3EF70", Length = "0x1DA")]
	public static bool ControlsLocked()
	{
		return false;
	}

	[Token(Token = "0x6004242")]
	[Address(RVA = "0xC40350", Offset = "0xC3F150", Length = "0x252")]
	public static bool IsMovementLockedBecauseOfLantern()
	{
		return false;
	}

	[Token(Token = "0x6004243")]
	[Address(RVA = "0xC405B0", Offset = "0xC3F3B0", Length = "0x57")]
	public static bool IsMoveInputUnblocked()
	{
		return false;
	}

	[Token(Token = "0x6004244")]
	[Address(RVA = "0xC40610", Offset = "0xC3F410", Length = "0x12C")]
	public static void MaybeBlockMoveInputUntilReleased(MonoBehaviour context)
	{
	}

	[Token(Token = "0x6004245")]
	[Address(RVA = "0xC40740", Offset = "0xC3F540", Length = "0x57")]
	public static GameManager Instance()
	{
		return null;
	}

	[Token(Token = "0x6004246")]
	[Address(RVA = "0xC407A0", Offset = "0xC3F5A0", Length = "0x57")]
	public static Camera GetGlobalCamera()
	{
		return null;
	}

	[Token(Token = "0x6004247")]
	[Address(RVA = "0xC40800", Offset = "0xC3F600", Length = "0x57")]
	public static Camera GetMainCamera()
	{
		return null;
	}

	[Token(Token = "0x6004248")]
	[Address(RVA = "0xC40860", Offset = "0xC3F660", Length = "0x57")]
	public static Camera GetWeaponCamera()
	{
		return null;
	}

	[Token(Token = "0x6004249")]
	[Address(RVA = "0xC408C0", Offset = "0xC3F6C0", Length = "0x57")]
	public static Camera GetImageEffectCamera()
	{
		return null;
	}

	[Token(Token = "0x600424A")]
	[Address(RVA = "0xC40920", Offset = "0xC3F720", Length = "0x57")]
	public static Camera GetInspectModeCamera()
	{
		return null;
	}

	[Token(Token = "0x600424B")]
	[Address(RVA = "0xC40980", Offset = "0xC3F780", Length = "0x57")]
	public static Light GetInspectModeLight()
	{
		return null;
	}

	[Token(Token = "0x600424C")]
	[Address(RVA = "0xC409E0", Offset = "0xC3F7E0", Length = "0x57")]
	public static vp_FPSCamera GetVpFPSCamera()
	{
		return null;
	}

	[Token(Token = "0x600424D")]
	[Address(RVA = "0xC40A40", Offset = "0xC3F840", Length = "0x57")]
	public static vp_FPSPlayer GetVpFPSPlayer()
	{
		return null;
	}

	[Token(Token = "0x600424E")]
	[Address(RVA = "0xC40AA0", Offset = "0xC3F8A0", Length = "0x57")]
	public static NavMeshObstacle GetPlayerNavMeshObstacle()
	{
		return null;
	}

	[Token(Token = "0x600424F")]
	[Address(RVA = "0xC40B00", Offset = "0xC3F900", Length = "0x57")]
	public static CameraEffects GetCameraEffects()
	{
		return null;
	}

	[Token(Token = "0x6004250")]
	[Address(RVA = "0xC40B60", Offset = "0xC3F960", Length = "0x57")]
	public static CameraStatusEffects GetCameraStatusEffects()
	{
		return null;
	}

	[Token(Token = "0x6004251")]
	[Address(RVA = "0xC40BC0", Offset = "0xC3F9C0", Length = "0x137")]
	public static GameObject GetPlayerObject()
	{
		return null;
	}

	[Token(Token = "0x6004252")]
	[Address(RVA = "0xC40D00", Offset = "0xC3FB00", Length = "0x57")]
	public static PlayerCameraAnim GetNewPlayerCameraAnim()
	{
		return null;
	}

	[Token(Token = "0x6004253")]
	[Address(RVA = "0xC40D60", Offset = "0xC3FB60", Length = "0x57")]
	public static GameObject GetWeaponView()
	{
		return null;
	}

	[Token(Token = "0x6004254")]
	[Address(RVA = "0xC40DC0", Offset = "0xC3FBC0", Length = "0x57")]
	public static TravoisTrailManager GetTravoisTrailManager()
	{
		return null;
	}

	[Token(Token = "0x6004255")]
	[Address(RVA = "0xC40E20", Offset = "0xC3FC20", Length = "0x57")]
	public static TravoisStampingManager GetTravoisStampingManager()
	{
		return null;
	}

	[Token(Token = "0x6004256")]
	[Address(RVA = "0xC40E80", Offset = "0xC3FC80", Length = "0x137")]
	public static Transform GetPlayerTransform()
	{
		return null;
	}

	[Token(Token = "0x6004257")]
	[Address(RVA = "0xC40FC0", Offset = "0xC3FDC0", Length = "0x57")]
	public static GameObject GetTopLevelCharacterFpsPlayer()
	{
		return null;
	}

	[Token(Token = "0x6004258")]
	[Address(RVA = "0xC41020", Offset = "0xC3FE20", Length = "0x57")]
	public static AiDifficultySettings GetAiDifficultySettings()
	{
		return null;
	}

	[Token(Token = "0x6004259")]
	[Address(RVA = "0xC41080", Offset = "0xC3FE80", Length = "0x68")]
	public static UniStormWeatherSystem GetUniStorm()
	{
		return null;
	}

	[Token(Token = "0x600425A")]
	[Address(RVA = "0xC410F0", Offset = "0xC3FEF0", Length = "0x57")]
	public static AuroraManager GetAuroraManager()
	{
		return null;
	}

	[Token(Token = "0x600425B")]
	[Address(RVA = "0xC41150", Offset = "0xC3FF50", Length = "0x57")]
	public static AchievementManager GetAchievementManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x600425C")]
	[Address(RVA = "0xC411B0", Offset = "0xC3FFB0", Length = "0x57")]
	public static Burns GetBurnsComponent()
	{
		return null;
	}

	[Token(Token = "0x600425D")]
	[Address(RVA = "0xC41210", Offset = "0xC40010", Length = "0x57")]
	public static BurnsElectric GetBurnsElectricComponent()
	{
		return null;
	}

	[Token(Token = "0x600425E")]
	[Address(RVA = "0xC41270", Offset = "0xC40070", Length = "0x57")]
	public static BloodLoss GetBloodLossComponent()
	{
		return null;
	}

	[Token(Token = "0x600425F")]
	[Address(RVA = "0xC412D0", Offset = "0xC400D0", Length = "0x57")]
	public static BodyHarvestManager GetBodyHarvestManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x6004260")]
	[Address(RVA = "0xC41330", Offset = "0xC40130", Length = "0x57")]
	public static Breath GetBreathComponent()
	{
		return null;
	}

	[Token(Token = "0x6004261")]
	[Address(RVA = "0xC41390", Offset = "0xC40190", Length = "0x57")]
	public static BrokenBody GetBrokenBody()
	{
		return null;
	}

	[Token(Token = "0x6004262")]
	[Address(RVA = "0xC413F0", Offset = "0xC401F0", Length = "0x57")]
	public static BrokenRib GetBrokenRibComponent()
	{
		return null;
	}

	[Token(Token = "0x6004263")]
	[Address(RVA = "0xC41450", Offset = "0xC40250", Length = "0x57")]
	public static CabinFever GetCabinFeverComponent()
	{
		return null;
	}

	[Token(Token = "0x6004264")]
	[Address(RVA = "0xC414B0", Offset = "0xC402B0", Length = "0x57")]
	public static CheatDeathAffliction GetCheatDeathAffliction()
	{
		return null;
	}

	[Token(Token = "0x6004265")]
	[Address(RVA = "0xC41510", Offset = "0xC40310", Length = "0x57")]
	public static Condition GetConditionComponent()
	{
		return null;
	}

	[Token(Token = "0x6004266")]
	[Address(RVA = "0xC41570", Offset = "0xC40370", Length = "0x68")]
	public static ConditionTable GetConditionTable()
	{
		return null;
	}

	[Token(Token = "0x6004267")]
	[Address(RVA = "0xC415E0", Offset = "0xC403E0", Length = "0x57")]
	public static DialogueStatesTable GetDialogueStatesTable()
	{
		return null;
	}

	[Token(Token = "0x6004268")]
	[Address(RVA = "0xC41640", Offset = "0xC40440", Length = "0x57")]
	public static DiminishedState GetDiminishedState()
	{
		return null;
	}

	[Token(Token = "0x6004269")]
	[Address(RVA = "0xC416A0", Offset = "0xC404A0", Length = "0x57")]
	public static MissionObjectiveTable GetMissionObjectiveTable()
	{
		return null;
	}

	[Token(Token = "0x600426A")]
	[Address(RVA = "0xC41700", Offset = "0xC40500", Length = "0x57")]
	public static AfflictionDefinitionTable GetAfflictionDefinitionTable()
	{
		return null;
	}

	[Token(Token = "0x600426B")]
	[Address(RVA = "0xC41760", Offset = "0xC40560", Length = "0x57")]
	public static DownsampleAurora GetDownsampleAurora()
	{
		return null;
	}

	[Token(Token = "0x600426C")]
	[Address(RVA = "0xC417C0", Offset = "0xC405C0", Length = "0x57")]
	public static DynamicDecalsManager GetDynamicDecalsManager()
	{
		return null;
	}

	[Token(Token = "0x600426D")]
	[Address(RVA = "0xC41820", Offset = "0xC40620", Length = "0x57")]
	public static Dysentery GetDysenteryComponent()
	{
		return null;
	}

	[Token(Token = "0x600426E")]
	[Address(RVA = "0xC41880", Offset = "0xC40680", Length = "0x57")]
	public static EffectPoolManager GetEffectPoolManager()
	{
		return null;
	}

	[Token(Token = "0x600426F")]
	[Address(RVA = "0xC418E0", Offset = "0xC406E0", Length = "0x57")]
	public static EmergencyStim GetEmergencyStimComponent()
	{
		return null;
	}

	[Token(Token = "0x6004270")]
	[Address(RVA = "0xC41940", Offset = "0xC40740", Length = "0x57")]
	public static Encumber GetEncumberComponent()
	{
		return null;
	}

	[Token(Token = "0x6004271")]
	[Address(RVA = "0xC419A0", Offset = "0xC407A0", Length = "0x57")]
	public static EnergyBoost GetEnergyBoostComponent()
	{
		return null;
	}

	[Token(Token = "0x6004272")]
	[Address(RVA = "0xC41A00", Offset = "0xC40800", Length = "0x57")]
	public static ExperienceModeManager GetExperienceModeManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x6004273")]
	[Address(RVA = "0xC41A60", Offset = "0xC40860", Length = "0x57")]
	public static Fatigue GetFatigueComponent()
	{
		return null;
	}

	[Token(Token = "0x6004274")]
	[Address(RVA = "0xC41AC0", Offset = "0xC408C0", Length = "0x57")]
	public static FireManager GetFireManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x6004275")]
	[Address(RVA = "0xC41B20", Offset = "0xC40920", Length = "0x57")]
	public static FoodPoisoning GetFoodPoisoningComponent()
	{
		return null;
	}

	[Token(Token = "0x6004276")]
	[Address(RVA = "0xC41B80", Offset = "0xC40980", Length = "0x57")]
	public static FootStepSounds GetFootStepSoundsComponent()
	{
		return null;
	}

	[Token(Token = "0x6004277")]
	[Address(RVA = "0xC41BE0", Offset = "0xC409E0", Length = "0x57")]
	public static Freezing GetFreezingComponent()
	{
		return null;
	}

	[Token(Token = "0x6004278")]
	[Address(RVA = "0xC41C40", Offset = "0xC40A40", Length = "0x57")]
	public static Frostbite GetFrostbiteComponent()
	{
		return null;
	}

	[Token(Token = "0x6004279")]
	[Address(RVA = "0xC41CA0", Offset = "0xC40AA0", Length = "0x57")]
	public static Suffocating GetSuffocatingComponent()
	{
		return null;
	}

	[Token(Token = "0x600427A")]
	[Address(RVA = "0xC41D00", Offset = "0xC40B00", Length = "0x57")]
	public static HeatSourceManager GetHeatSourceManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x600427B")]
	[Address(RVA = "0xC41D60", Offset = "0xC40B60", Length = "0x57")]
	public static Headache GetHeadacheComponent()
	{
		return null;
	}

	[Token(Token = "0x600427C")]
	[Address(RVA = "0xC41DC0", Offset = "0xC40BC0", Length = "0x57")]
	public static Hunger GetHungerComponent()
	{
		return null;
	}

	[Token(Token = "0x600427D")]
	[Address(RVA = "0xC41E20", Offset = "0xC40C20", Length = "0x57")]
	public static Hypothermia GetHypothermiaComponent()
	{
		return null;
	}

	[Token(Token = "0x600427E")]
	[Address(RVA = "0xC41E80", Offset = "0xC40C80", Length = "0x57")]
	public static Infection GetInfectionComponent()
	{
		return null;
	}

	[Token(Token = "0x600427F")]
	[Address(RVA = "0xC41EE0", Offset = "0xC40CE0", Length = "0x57")]
	public static InfectionRisk GetInfectionRiskComponent()
	{
		return null;
	}

	[Token(Token = "0x6004280")]
	[Address(RVA = "0xC41F40", Offset = "0xC40D40", Length = "0x57")]
	public static IntestinalParasites GetIntestinalParasitesComponent()
	{
		return null;
	}

	[Token(Token = "0x6004281")]
	[Address(RVA = "0xC41FA0", Offset = "0xC40DA0", Length = "0x57")]
	public static FallDamage GetFallDamageComponent()
	{
		return null;
	}

	[Token(Token = "0x6004282")]
	[Address(RVA = "0xC42000", Offset = "0xC40E00", Length = "0x53")]
	public static Feat_BookSmarts GetFeatBookSmarts()
	{
		return null;
	}

	[Token(Token = "0x6004283")]
	[Address(RVA = "0xC42060", Offset = "0xC40E60", Length = "0x54")]
	public static Feat_ColdFusion GetFeatColdFusion()
	{
		return null;
	}

	[Token(Token = "0x6004284")]
	[Address(RVA = "0xC420C0", Offset = "0xC40EC0", Length = "0x54")]
	public static Feat_EfficientMachine GetFeatEfficientMachine()
	{
		return null;
	}

	[Token(Token = "0x6004285")]
	[Address(RVA = "0xC42120", Offset = "0xC40F20", Length = "0x54")]
	public static Feat_FireMaster GetFeatFireMaster()
	{
		return null;
	}

	[Token(Token = "0x6004286")]
	[Address(RVA = "0xC42180", Offset = "0xC40F80", Length = "0x54")]
	public static Feat_FreeRunner GetFeatFreeRunner()
	{
		return null;
	}

	[Token(Token = "0x6004287")]
	[Address(RVA = "0xC421E0", Offset = "0xC40FE0", Length = "0x54")]
	public static Feat_SnowWalker GetFeatSnowWalker()
	{
		return null;
	}

	[Token(Token = "0x6004288")]
	[Address(RVA = "0xC42240", Offset = "0xC41040", Length = "0x54")]
	public static Feat_ExpertTrapper GetFeatExpertTrapper()
	{
		return null;
	}

	[Token(Token = "0x6004289")]
	[Address(RVA = "0xC422A0", Offset = "0xC410A0", Length = "0x54")]
	public static Feat_StraightToHeart GetFeatStraightToHeart()
	{
		return null;
	}

	[Token(Token = "0x600428A")]
	[Address(RVA = "0xC42300", Offset = "0xC41100", Length = "0x54")]
	public static Feat_BlizzardWalker GetFeatBlizzardWalker()
	{
		return null;
	}

	[Token(Token = "0x600428B")]
	[Address(RVA = "0xC42360", Offset = "0xC41160", Length = "0x54")]
	public static Feat_NightWalker GetFeatNightWalker()
	{
		return null;
	}

	[Token(Token = "0x600428C")]
	[Address(RVA = "0xC423C0", Offset = "0xC411C0", Length = "0x54")]
	public static Feat_MasterHunter GetFeatMasterHunter()
	{
		return null;
	}

	[Token(Token = "0x600428D")]
	[Address(RVA = "0xC42420", Offset = "0xC41220", Length = "0x54")]
	public static Feat_SettledMind GetFeatSettledMind()
	{
		return null;
	}

	[Token(Token = "0x600428E")]
	[Address(RVA = "0xC42480", Offset = "0xC41280", Length = "0x54")]
	public static Feat_CelestialNavigator GetFeatCelestialNavigator()
	{
		return null;
	}

	[Token(Token = "0x600428F")]
	[Address(RVA = "0xC424E0", Offset = "0xC412E0", Length = "0x57")]
	public static FeatsManager GetFeatsManager()
	{
		return null;
	}

	[Token(Token = "0x6004290")]
	[Address(RVA = "0xC42540", Offset = "0xC41340", Length = "0x57")]
	public static FeatNotify GetFeatNotify()
	{
		return null;
	}

	[Token(Token = "0x6004291")]
	[Address(RVA = "0xC425A0", Offset = "0xC413A0", Length = "0x57")]
	public static FlyOver GetFlyOverComponent()
	{
		return null;
	}

	[Token(Token = "0x6004292")]
	[Address(RVA = "0xC42600", Offset = "0xC41400", Length = "0x57")]
	public static FootstepTrailManager GetFootstepTrailManager()
	{
		return null;
	}

	[Token(Token = "0x6004293")]
	[Address(RVA = "0xC42660", Offset = "0xC41460", Length = "0x57")]
	public static GlobalParameters GetGlobalParameters()
	{
		return null;
	}

	[Token(Token = "0x6004294")]
	[Address(RVA = "0xC426C0", Offset = "0xC414C0", Length = "0x57")]
	public static HeatPadBuff GetHeatPadBuff()
	{
		return null;
	}

	[Token(Token = "0x6004295")]
	[Address(RVA = "0xC42720", Offset = "0xC41520", Length = "0x57")]
	public static IceCrackingManager GetIceCrackingManager()
	{
		return null;
	}

	[Token(Token = "0x6004296")]
	[Address(RVA = "0xC42780", Offset = "0xC41580", Length = "0x57")]
	public static InteractiveClothManager GetInteractiveClothManager()
	{
		return null;
	}

	[Token(Token = "0x6004297")]
	[Address(RVA = "0xC427E0", Offset = "0xC415E0", Length = "0x57")]
	public static Inventory GetInventoryComponent()
	{
		return null;
	}

	[Token(Token = "0x6004298")]
	[Address(RVA = "0xC42840", Offset = "0xC41640", Length = "0x57")]
	public static Log GetLogComponent()
	{
		return null;
	}

	[Token(Token = "0x6004299")]
	[Address(RVA = "0xC428A0", Offset = "0xC416A0", Length = "0x57")]
	public static LifeAfterDeathManager GetLifeAfterDeathManager()
	{
		return null;
	}

	[Token(Token = "0x600429A")]
	[Address(RVA = "0xC42900", Offset = "0xC41700", Length = "0x13C")]
	public static MissionServicesManager GetMissionServicesManager()
	{
		return null;
	}

	[Token(Token = "0x600429B")]
	[Address(RVA = "0xC42A40", Offset = "0xC41840", Length = "0x57")]
	public static PassTime GetPassTime()
	{
		return null;
	}

	[Token(Token = "0x600429C")]
	[Address(RVA = "0xC42AA0", Offset = "0xC418A0", Length = "0x57")]
	public static MapDetailManager GetMapDetailManager()
	{
		return null;
	}

	[Token(Token = "0x600429D")]
	[Address(RVA = "0xC42B00", Offset = "0xC41900", Length = "0x57")]
	public static MiseryManager GetMiseryManager()
	{
		return null;
	}

	[Token(Token = "0x600429E")]
	[Address(RVA = "0xC42B60", Offset = "0xC41960", Length = "0x57")]
	public static MusicEventManager GetMusicEventManager()
	{
		return null;
	}

	[Token(Token = "0x600429F")]
	[Address(RVA = "0xC42BC0", Offset = "0xC419C0", Length = "0x57")]
	public static PlayerClimbRope GetPlayerClimbRopeComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A0")]
	[Address(RVA = "0xC42C20", Offset = "0xC41A20", Length = "0x57")]
	public static PlayerGameStats GetPlayerGameStatsComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A1")]
	[Address(RVA = "0xC42C80", Offset = "0xC41A80", Length = "0x57")]
	public static PlayerInVehicle GetPlayerInVehicle()
	{
		return null;
	}

	[Token(Token = "0x60042A2")]
	[Address(RVA = "0xC42CE0", Offset = "0xC41AE0", Length = "0x57")]
	public static PlayerInConstrainedCamera GetPlayerInConstrainedCamera()
	{
		return null;
	}

	[Token(Token = "0x60042A3")]
	[Address(RVA = "0xC42D40", Offset = "0xC41B40", Length = "0x57")]
	public static PlayerKnowledge GetPlayerKnowledgeComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A4")]
	[Address(RVA = "0xC42DA0", Offset = "0xC41BA0", Length = "0x57")]
	public static PlayerAnimation GetPlayerAnimationComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A5")]
	[Address(RVA = "0xC42E00", Offset = "0xC41C00", Length = "0x57")]
	public static PlayerManager GetPlayerManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A6")]
	[Address(RVA = "0xC42E60", Offset = "0xC41C60", Length = "0x57")]
	public static PlayerMovement GetPlayerMovementComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A7")]
	[Address(RVA = "0xC42EC0", Offset = "0xC41CC0", Length = "0x57")]
	public static PlayerSkills GetPlayerSkillsComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A8")]
	[Address(RVA = "0xC42F20", Offset = "0xC41D20", Length = "0x57")]
	public static PlayerStruggle GetPlayerStruggleComponent()
	{
		return null;
	}

	[Token(Token = "0x60042A9")]
	[Address(RVA = "0xC42F80", Offset = "0xC41D80", Length = "0x57")]
	public static PlayerStunned GetPlayerStunnedComponent()
	{
		return null;
	}

	[Token(Token = "0x60042AA")]
	[Address(RVA = "0xC42FE0", Offset = "0xC41DE0", Length = "0x57")]
	public static PlayerSwing GetPlayerSwingComponent()
	{
		return null;
	}

	[Token(Token = "0x60042AB")]
	[Address(RVA = "0xC43040", Offset = "0xC41E40", Length = "0x57")]
	public static PlayerVoice GetPlayerVoiceComponent()
	{
		return null;
	}

	[Token(Token = "0x60042AC")]
	[Address(RVA = "0xC430A0", Offset = "0xC41EA0", Length = "0x57")]
	public static PoorCirculation GetPoorCirculation()
	{
		return null;
	}

	[Token(Token = "0x60042AD")]
	[Address(RVA = "0xC43100", Offset = "0xC41F00", Length = "0x57")]
	public static QualitySettingsManager GetQualitySettingsManager()
	{
		return null;
	}

	[Token(Token = "0x60042AE")]
	[Address(RVA = "0xC43160", Offset = "0xC41F60", Length = "0x57")]
	public static RadialSpawnManager GetRadialSpawnManager()
	{
		return null;
	}

	[Token(Token = "0x60042AF")]
	[Address(RVA = "0xC431C0", Offset = "0xC41FC0", Length = "0x57")]
	public static RenderTextureCameraManager GetRenderTextureCameraManager()
	{
		return null;
	}

	[Token(Token = "0x60042B0")]
	[Address(RVA = "0xC43220", Offset = "0xC42020", Length = "0x57")]
	public static Rest GetRestComponent()
	{
		return null;
	}

	[Token(Token = "0x60042B1")]
	[Address(RVA = "0xC43280", Offset = "0xC42080", Length = "0x57")]
	public static RumbleEffectManager GetRumbleEffectManager()
	{
		return null;
	}

	[Token(Token = "0x60042B2")]
	[Address(RVA = "0xC432E0", Offset = "0xC420E0", Length = "0x57")]
	public static ScentRanges GetSceneRanges()
	{
		return null;
	}

	[Token(Token = "0x60042B3")]
	[Address(RVA = "0xC43340", Offset = "0xC42140", Length = "0x57")]
	public static SevereLacerations GetSevereLacerations()
	{
		return null;
	}

	[Token(Token = "0x60042B4")]
	[Address(RVA = "0xC433A0", Offset = "0xC421A0", Length = "0x68")]
	public static Skill_Archery GetSkillArchery()
	{
		return null;
	}

	[Token(Token = "0x60042B5")]
	[Address(RVA = "0xC43410", Offset = "0xC42210", Length = "0x65")]
	public static Skill_CarcassHarvesting GetSkillCarcassHarvesting()
	{
		return null;
	}

	[Token(Token = "0x60042B6")]
	[Address(RVA = "0xC43480", Offset = "0xC42280", Length = "0x68")]
	public static Skill_ClothingRepair GetSkillClothingRepair()
	{
		return null;
	}

	[Token(Token = "0x60042B7")]
	[Address(RVA = "0xC434F0", Offset = "0xC422F0", Length = "0x68")]
	public static Skill_Cooking GetSkillCooking()
	{
		return null;
	}

	[Token(Token = "0x60042B8")]
	[Address(RVA = "0xC43560", Offset = "0xC42360", Length = "0x65")]
	public static Skill_Firestarting GetSkillFireStarting()
	{
		return null;
	}

	[Token(Token = "0x60042B9")]
	[Address(RVA = "0xC435D0", Offset = "0xC423D0", Length = "0x68")]
	public static Skill_IceFishing GetSkillIceFishing()
	{
		return null;
	}

	[Token(Token = "0x60042BA")]
	[Address(RVA = "0xC43640", Offset = "0xC42440", Length = "0x68")]
	public static Skill_Rifle GetSkillRifle()
	{
		return null;
	}

	[Token(Token = "0x60042BB")]
	[Address(RVA = "0xC436B0", Offset = "0xC424B0", Length = "0x68")]
	public static Skill_Revolver GetSkillRevolver()
	{
		return null;
	}

	[Token(Token = "0x60042BC")]
	[Address(RVA = "0xC43720", Offset = "0xC42520", Length = "0x68")]
	public static Skill_Gunsmithing GetSkillGunsmithing()
	{
		return null;
	}

	[Token(Token = "0x60042BD")]
	[Address(RVA = "0xC43790", Offset = "0xC42590", Length = "0x57")]
	public static SkillNotify GetSkillNotify()
	{
		return null;
	}

	[Token(Token = "0x60042BE")]
	[Address(RVA = "0xC437F0", Offset = "0xC425F0", Length = "0x57")]
	public static SkillsManager GetSkillsManager()
	{
		return null;
	}

	[Token(Token = "0x60042BF")]
	[Address(RVA = "0xC43850", Offset = "0xC42650", Length = "0x57")]
	public static SnowPatchManager GetSnowPatchManager()
	{
		return null;
	}

	[Token(Token = "0x60042C0")]
	[Address(RVA = "0xC438B0", Offset = "0xC426B0", Length = "0x57")]
	public static SnowShelterManager GetSnowShelterManager()
	{
		return null;
	}

	[Token(Token = "0x60042C1")]
	[Address(RVA = "0xC43910", Offset = "0xC42710", Length = "0x57")]
	public static RockCacheManager GetRockCacheManager()
	{
		return null;
	}

	[Token(Token = "0x60042C2")]
	[Address(RVA = "0xC43970", Offset = "0xC42770", Length = "0x57")]
	public static SpawnRegionManager GetSpawnRegionManager()
	{
		return null;
	}

	[Token(Token = "0x60042C3")]
	[Address(RVA = "0xC439D0", Offset = "0xC427D0", Length = "0x57")]
	public static SprainedAnkle GetSprainedAnkleComponent()
	{
		return null;
	}

	[Token(Token = "0x60042C4")]
	[Address(RVA = "0xC43A30", Offset = "0xC42830", Length = "0x57")]
	public static SprainedWrist GetSprainedWristComponent()
	{
		return null;
	}

	[Token(Token = "0x60042C5")]
	[Address(RVA = "0xC43A90", Offset = "0xC42890", Length = "0x57")]
	public static SprainPain GetSprainPainComponent()
	{
		return null;
	}

	[Token(Token = "0x60042C6")]
	[Address(RVA = "0xC43AF0", Offset = "0xC428F0", Length = "0x57")]
	public static Sprains GetSprainsComponent()
	{
		return null;
	}

	[Token(Token = "0x60042C7")]
	[Address(RVA = "0xC43B50", Offset = "0xC42950", Length = "0x57")]
	public static SourStomach GetSourStomach()
	{
		return null;
	}

	[Token(Token = "0x60042C8")]
	[Address(RVA = "0xC43BB0", Offset = "0xC429B0", Length = "0x57")]
	public static Anxiety GetAnxietyComponent()
	{
		return null;
	}

	[Token(Token = "0x60042C9")]
	[Address(RVA = "0xC43C10", Offset = "0xC42A10", Length = "0x57")]
	public static Fear GetFearComponent()
	{
		return null;
	}

	[Token(Token = "0x60042CA")]
	[Address(RVA = "0xC43C70", Offset = "0xC42A70", Length = "0x57")]
	public static ToxicFog GetToxicFogComponent()
	{
		return null;
	}

	[Token(Token = "0x60042CB")]
	[Address(RVA = "0xC43CD0", Offset = "0xC42AD0", Length = "0x57")]
	public static StartSettings GetStartSettingsComponent()
	{
		return null;
	}

	[Token(Token = "0x60042CC")]
	[Address(RVA = "0xC43D30", Offset = "0xC42B30", Length = "0x57")]
	public static StatsManager GetStatsManagerComponent()
	{
		return null;
	}

	[Token(Token = "0x60042CD")]
	[Address(RVA = "0xC43D90", Offset = "0xC42B90", Length = "0x57")]
	public static TerrainGrassModifier GetTerrainGrassModifier()
	{
		return null;
	}

	[Token(Token = "0x60042CE")]
	[Address(RVA = "0xC43DF0", Offset = "0xC42BF0", Length = "0x57")]
	public static TerrainRenderingManager GetTerrainRenderingManager()
	{
		return null;
	}

	[Token(Token = "0x60042CF")]
	[Address(RVA = "0xC43E50", Offset = "0xC42C50", Length = "0x57")]
	public static Thirst GetThirstComponent()
	{
		return null;
	}

	[Token(Token = "0x60042D0")]
	[Address(RVA = "0xC43EB0", Offset = "0xC42CB0", Length = "0x57")]
	public static TLD_TimelineDirector GetPlayerTimelineDirector()
	{
		return null;
	}

	[Token(Token = "0x60042D1")]
	[Address(RVA = "0xC43F10", Offset = "0xC42D10", Length = "0x57")]
	public static TimeOfDay GetTimeOfDayComponent()
	{
		return null;
	}

	[Token(Token = "0x60042D2")]
	[Address(RVA = "0xC43F70", Offset = "0xC42D70", Length = "0x57")]
	public static TraderManager GetTraderManager()
	{
		return null;
	}

	[Token(Token = "0x60042D3")]
	[Address(RVA = "0xC43FD0", Offset = "0xC42DD0", Length = "0x57")]
	public static UnsettledSleep GetUnsettledSleep()
	{
		return null;
	}

	[Token(Token = "0x60042D4")]
	[Address(RVA = "0xC44030", Offset = "0xC42E30", Length = "0x57")]
	public static WeakConstitution GetWeakConstitution()
	{
		return null;
	}

	[Token(Token = "0x60042D5")]
	[Address(RVA = "0xC44090", Offset = "0xC42E90", Length = "0x57")]
	public static WeakJoints GetWeakJoints()
	{
		return null;
	}

	[Token(Token = "0x60042D6")]
	[Address(RVA = "0xC440F0", Offset = "0xC42EF0", Length = "0x57")]
	public static Weather GetWeatherComponent()
	{
		return null;
	}

	[Token(Token = "0x60042D7")]
	[Address(RVA = "0xC44150", Offset = "0xC42F50", Length = "0x57")]
	public static WeatherTransition GetWeatherTransitionComponent()
	{
		return null;
	}

	[Token(Token = "0x60042D8")]
	[Address(RVA = "0xC441B0", Offset = "0xC42FB0", Length = "0x57")]
	public static WellFed GetWellFedComponent()
	{
		return null;
	}

	[Token(Token = "0x60042D9")]
	[Address(RVA = "0xC44210", Offset = "0xC43010", Length = "0x57")]
	public static Willpower GetWillpowerComponent()
	{
		return null;
	}

	[Token(Token = "0x60042DA")]
	[Address(RVA = "0xC44270", Offset = "0xC43070", Length = "0x57")]
	public static Wind GetWindComponent()
	{
		return null;
	}

	[Token(Token = "0x60042DB")]
	[Address(RVA = "0xC442D0", Offset = "0xC430D0", Length = "0x57")]
	public static WindZone GetWindZone()
	{
		return null;
	}

	[Token(Token = "0x60042DC")]
	[Address(RVA = "0xC44330", Offset = "0xC43130", Length = "0x57")]
	public static PackManager GetPackManager()
	{
		return null;
	}

	[Token(Token = "0x60042DD")]
	[Address(RVA = "0xC44390", Offset = "0xC43190", Length = "0x57")]
	public static AreaMarkupManager GetAreaMarkupManager()
	{
		return null;
	}

	[Token(Token = "0x60042DE")]
	[Address(RVA = "0xC443F0", Offset = "0xC431F0", Length = "0x57")]
	public static FastClothManager GetFastClothManager()
	{
		return null;
	}

	[Token(Token = "0x60042DF")]
	[Address(RVA = "0xC44450", Offset = "0xC43250", Length = "0x57")]
	public static DebugViewModeManager GetDebugViewModeManager()
	{
		return null;
	}

	[Token(Token = "0x60042E0")]
	[Address(RVA = "0xC444B0", Offset = "0xC432B0", Length = "0x57")]
	public static InvisibleEntityManager GetInvisibleEntityManager()
	{
		return null;
	}

	[Token(Token = "0x60042E1")]
	[Address(RVA = "0xC44510", Offset = "0xC43310", Length = "0x57")]
	public static ToxicFogManager GetToxicFogManager()
	{
		return null;
	}

	[Token(Token = "0x60042E2")]
	[Address(RVA = "0xC44570", Offset = "0xC43370", Length = "0x57")]
	public static SprainProtection GetSprainProtectionComponent()
	{
		return null;
	}

	[Token(Token = "0x60042E3")]
	[Address(RVA = "0xC445D0", Offset = "0xC433D0", Length = "0x57")]
	public static SteamPipeEffectManager GetSteamPipeEffectManager()
	{
		return null;
	}

	[Token(Token = "0x60042E4")]
	[Address(RVA = "0xC44630", Offset = "0xC43430", Length = "0x57")]
	public static SteamPipeValveManager GetSteamPipeValveManager()
	{
		return null;
	}

	[Token(Token = "0x60042E5")]
	[Address(RVA = "0xC44690", Offset = "0xC43490", Length = "0x57")]
	public static SteamPipeManager GetSteamPipeManager()
	{
		return null;
	}

	[Token(Token = "0x60042E6")]
	[Address(RVA = "0xC446F0", Offset = "0xC434F0", Length = "0x57")]
	public static PlayerCough GetPlayerCough()
	{
		return null;
	}

	[Token(Token = "0x60042E7")]
	[Address(RVA = "0xC44750", Offset = "0xC43550", Length = "0x57")]
	public static HighResolutionTimerManager GetHighResolutionTimerManager()
	{
		return null;
	}

	[Token(Token = "0x60042E8")]
	[Address(RVA = "0xC447B0", Offset = "0xC435B0", Length = "0x57")]
	public static FontManager GetFontManager()
	{
		return null;
	}

	[Token(Token = "0x60042E9")]
	[Address(RVA = "0xC44810", Offset = "0xC43610", Length = "0x57")]
	public static FlareIntensityManager GetFlareIntensityManager()
	{
		return null;
	}

	[Token(Token = "0x60042EA")]
	[Address(RVA = "0xC44870", Offset = "0xC43670", Length = "0x57")]
	public static NotificationFlagManager GetNotificationFlagManager()
	{
		return null;
	}

	[Token(Token = "0x60042EB")]
	[Address(RVA = "0xC448D0", Offset = "0xC436D0", Length = "0x57")]
	public static DamageProtection GetDamageProtection()
	{
		return null;
	}

	[Token(Token = "0x60042EC")]
	[Address(RVA = "0xC44930", Offset = "0xC43730", Length = "0x57")]
	public static InsomniaManager GetInsomniaComponent()
	{
		return null;
	}

	[Token(Token = "0x60042ED")]
	[Address(RVA = "0xC44990", Offset = "0xC43790", Length = "0x57")]
	public static TrackableItemsManager GetTrackableItemsManager()
	{
		return null;
	}

	[Token(Token = "0x60042EE")]
	[Address(RVA = "0xC449F0", Offset = "0xC437F0", Length = "0x57")]
	public static TransmitterManager GetTransmitterManager()
	{
		return null;
	}

	[Token(Token = "0x60042EF")]
	[Address(RVA = "0xC44A50", Offset = "0xC43850", Length = "0x57")]
	public static ChemicalPoisoning GetChemicalPoisoningComponent()
	{
		return null;
	}

	[Token(Token = "0x60042F0")]
	[Address(RVA = "0xC44AB0", Offset = "0xC438B0", Length = "0x57")]
	public static ScurvyManager GetScurvyComponent()
	{
		return null;
	}

	[Token(Token = "0x60042F1")]
	[Address(RVA = "0xC44B10", Offset = "0xC43910", Length = "0x57")]
	public static RespiratorManager GetRespiratorManager()
	{
		return null;
	}

	[Token(Token = "0x60042F2")]
	[Address(RVA = "0xC44B70", Offset = "0xC43970", Length = "0x11B")]
	public static HeldItemInPlacementZone GetPlayerHeldItemPlacementZone()
	{
		return null;
	}

	[Token(Token = "0x60042F3")]
	[Address(RVA = "0xC44C90", Offset = "0xC43A90", Length = "0x57")]
	public static CougarManager GetCougarManager()
	{
		return null;
	}

	[Token(Token = "0x60042F4")]
	[Address(RVA = "0xC44CF0", Offset = "0xC43AF0", Length = "0x57")]
	public static SafehouseManager GetSafehouseManager()
	{
		return null;
	}

	[Token(Token = "0x60042F5")]
	[Address(RVA = "0xC44D50", Offset = "0xC43B50", Length = "0x57")]
	public static PhotoManager GetPhotoManager()
	{
		return null;
	}

	[Token(Token = "0x60042F6")]
	[Address(RVA = "0xC44DB0", Offset = "0xC43BB0", Length = "0x57")]
	public static JunkManager GetJunkManager()
	{
		return null;
	}

	[Token(Token = "0x60042F7")]
	[Address(RVA = "0xC44E10", Offset = "0xC43C10", Length = "0x57")]
	public static ParticleActivationManager GetParticleActivationManager()
	{
		return null;
	}

	[Token(Token = "0x60042F8")]
	[Address(RVA = "0xC44E70", Offset = "0xC43C70", Length = "0x57")]
	public static TerrainMaterialBlendingManager GetTerrainMaterialBlendingManager()
	{
		return null;
	}

	[Token(Token = "0x60042F9")]
	[Address(RVA = "0xC44ED0", Offset = "0xC43CD0", Length = "0x57")]
	public static MeshDistanceFieldManager GetMeshDistanceFieldManager()
	{
		return null;
	}

	[Token(Token = "0x60042FA")]
	[Address(RVA = "0x4A2140", Offset = "0x4A0F40", Length = "0x3")]
	public static bool IsStoryMode()
	{
		return false;
	}

	[Token(Token = "0x60042FB")]
	[Address(RVA = "0xC44F30", Offset = "0xC43D30", Length = "0x57")]
	public static CameraGlobalRT GetCameraGlobalRT()
	{
		return null;
	}

	[Token(Token = "0x60042FC")]
	[Address(RVA = "0xC44F90", Offset = "0xC43D90", Length = "0x57")]
	public static Camera GetCurrentCamera()
	{
		return null;
	}

	[Token(Token = "0x60042FD")]
	[Address(RVA = "0xC44FF0", Offset = "0xC43DF0", Length = "0x137")]
	public static CustomExperienceMode GetCustomMode()
	{
		return null;
	}

	[Token(Token = "0x60042FE")]
	[Address(RVA = "0xC45130", Offset = "0xC43F30", Length = "0x137")]
	public static bool InCustomMode()
	{
		return false;
	}

	[Token(Token = "0x60042FF")]
	[Address(RVA = "0xC45270", Offset = "0xC44070", Length = "0x45")]
	public static bool IsPlayingCustomXPGame()
	{
		return false;
	}

	[Token(Token = "0x6004300")]
	[Address(RVA = "0xC452C0", Offset = "0xC440C0", Length = "0x1F5")]
	private void UpdatePaused()
	{
	}

	[Token(Token = "0x6004301")]
	[Address(RVA = "0xC454C0", Offset = "0xC442C0", Length = "0x1184")]
	private void UpdateNotPaused()
	{
	}

	[Token(Token = "0x6004302")]
	[Address(RVA = "0xC46650", Offset = "0xC45450", Length = "0x370E")]
	private void CacheComponents()
	{
	}

	[Token(Token = "0x6004303")]
	[Address(RVA = "0xC49D60", Offset = "0xC48B60", Length = "0xB6E")]
	private void CachePlayerComponents()
	{
	}

	[Token(Token = "0x6004304")]
	[Address(RVA = "0xC4A8D0", Offset = "0xC496D0", Length = "0x1065")]
	private void InstantiatePlayerObject()
	{
	}

	[Token(Token = "0x6004305")]
	[Address(RVA = "0xC4B940", Offset = "0xC4A740", Length = "0xA8")]
	public static void RestorePlayerTransformParent()
	{
	}

	[Token(Token = "0x6004306")]
	[Address(RVA = "0xC4B9F0", Offset = "0xC4A7F0", Length = "0x135")]
	public static bool IsOutDoorsScene(string sceneName)
	{
		return false;
	}

	[Token(Token = "0x6004307")]
	[Address(RVA = "0xC4BB30", Offset = "0xC4A930", Length = "0x24F")]
	public static int GetResolutionOverrideForActiveScene()
	{
		return 0;
	}

	[Token(Token = "0x6004308")]
	[Address(RVA = "0xC4BD80", Offset = "0xC4AB80", Length = "0x15E")]
	private static void MaybeSwitchRenderingPath()
	{
	}

	[Token(Token = "0x6004309")]
	[Address(RVA = "0xC4BEE0", Offset = "0xC4ACE0", Length = "0x291")]
	private static void UpdateTerrainSettings()
	{
	}

	[Token(Token = "0x600430A")]
	[Address(RVA = "0x407D80", Offset = "0x406B80", Length = "0x3")]
	private static void InstantiateSwitchKeyboard()
	{
	}

	[Token(Token = "0x600430B")]
	[Address(RVA = "0xC4C180", Offset = "0xC4AF80", Length = "0x54")]
	public static void CancelPendingSave()
	{
	}

	[Token(Token = "0x600430C")]
	[Address(RVA = "0xC4C1E0", Offset = "0xC4AFE0", Length = "0x12E")]
	public static bool IsTrialMode()
	{
		return false;
	}

	[Token(Token = "0x600430D")]
	[Address(RVA = "0xC4C310", Offset = "0xC4B110", Length = "0x54")]
	public static void PauseForLoading()
	{
	}

	[Token(Token = "0x600430E")]
	[Address(RVA = "0xC4C370", Offset = "0xC4B170", Length = "0x72")]
	public static bool ToggleTrialMode()
	{
		return false;
	}

	[Token(Token = "0x600430F")]
	[Address(RVA = "0xC4C3F0", Offset = "0xC4B1F0", Length = "0xE1")]
	public static void OnBuyNow()
	{
	}

	[Token(Token = "0x6004310")]
	[Address(RVA = "0xC4C4E0", Offset = "0xC4B2E0", Length = "0x349")]
	public static string TrialModeTimeRemaining()
	{
		return null;
	}

	[Token(Token = "0x6004311")]
	[Address(RVA = "0xC4C830", Offset = "0xC4B630", Length = "0x83")]
	public static bool IsStorySaveActive()
	{
		return false;
	}

	[Token(Token = "0x6004312")]
	[Address(RVA = "0xC4C8C0", Offset = "0xC4B6C0", Length = "0x267")]
	public static void DoExitToMainMenu()
	{
	}

	[Token(Token = "0x6004313")]
	[Address(RVA = "0xC4CB30", Offset = "0xC4B930", Length = "0x1B2")]
	public static int GetRandomSeed(int seed)
	{
		return 0;
	}

	[Token(Token = "0x6004314")]
	[Address(RVA = "0xC4CCF0", Offset = "0xC4BAF0", Length = "0x2DE")]
	public static bool RollSpawnChance(GameObject go, float spawnChance)
	{
		return false;
	}

	[Token(Token = "0x6004315")]
	[Address(RVA = "0xC4CFD0", Offset = "0xC4BDD0", Length = "0x101")]
	public static bool HasPlayerObject()
	{
		return false;
	}

	[Token(Token = "0x6004316")]
	[Address(RVA = "0xC4D0E0", Offset = "0xC4BEE0", Length = "0x123")]
	private void OnApplicationFocus(bool focusStatus)
	{
	}

	[Token(Token = "0x6004317")]
	[Address(RVA = "0xC4D210", Offset = "0xC4C010", Length = "0x191")]
	private GameObject InstantiateSystem(GameObject prefab, Vector3 pos, Transform parent)
	{
		return null;
	}

	[Token(Token = "0x6004318")]
	[Address(RVA = "0xC4D3B0", Offset = "0xC4C1B0", Length = "0x15C")]
	private void InstantiateAiSystems(Vector3 pos, Transform parent)
	{
	}

	[Token(Token = "0x6004319")]
	[Address(RVA = "0x407D80", Offset = "0x406B80", Length = "0x3")]
	public static void InstantiateConsole()
	{
	}

	[Token(Token = "0x600431A")]
	[Address(RVA = "0xC4D510", Offset = "0xC4C310", Length = "0x9D")]
	private void InstantiateUniStorm()
	{
	}

	[Token(Token = "0x600431B")]
	[Address(RVA = "0xC4D5B0", Offset = "0xC4C3B0", Length = "0x2FE")]
	private void InstantiateEffectPoolManager()
	{
	}

	[Token(Token = "0x600431C")]
	[Address(RVA = "0xC4D8B0", Offset = "0xC4C6B0", Length = "0x39E")]
	private void InstantiateTravoisTrailManager()
	{
	}

	[Token(Token = "0x600431D")]
	[Address(RVA = "0xC4DC50", Offset = "0xC4CA50", Length = "0x3DE")]
	private void InstantiateRumbleEffectManager()
	{
	}

	[Token(Token = "0x600431E")]
	[Address(RVA = "0xC4E030", Offset = "0xC4CE30", Length = "0x244")]
	private void DestroyInventoryItems(GameObject parent)
	{
	}

	[Token(Token = "0x600431F")]
	[Address(RVA = "0xC4E280", Offset = "0xC4D080", Length = "0x297")]
	private static void SetGameVersionString(string versionString, string dlcVersionString)
	{
	}

	[Token(Token = "0x6004320")]
	[Address(RVA = "0xC4E520", Offset = "0xC4D320", Length = "0xE5")]
	public static int ExtractChangelistFromVersion(string versionString)
	{
		return 0;
	}

	[Token(Token = "0x6004321")]
	[Address(RVA = "0xC4E610", Offset = "0xC4D410", Length = "0x8D")]
	public static void UpdateGameVersionString()
	{
	}

	[Token(Token = "0x6004322")]
	[Address(RVA = "0xC4E6A0", Offset = "0xC4D4A0", Length = "0x1E7")]
	private static string GetDLCVersionInfo()
	{
		return null;
	}

	[Token(Token = "0x6004323")]
	[Address(RVA = "0xC4E890", Offset = "0xC4D690", Length = "0x1B5")]
	private static string ReadVersionFile()
	{
		return null;
	}

	[Token(Token = "0x6004324")]
	[Address(RVA = "0xC4EA50", Offset = "0xC4D850", Length = "0xB47")]
	private void OutputSystemInfo()
	{
	}

	[Token(Token = "0x6004325")]
	[Address(RVA = "0xC4F5A0", Offset = "0xC4E3A0", Length = "0x122")]
	private static bool SceneNotCompatibleWithMode(bool isStory, string sceneName)
	{
		return false;
	}

	[Token(Token = "0x6004326")]
	[Address(RVA = "0xC4F6D0", Offset = "0xC4E4D0", Length = "0x5D9")]
	private void LoadSlotOnStart()
	{
	}

	[Token(Token = "0x6004327")]
	[Address(RVA = "0xC4FCB0", Offset = "0xC4EAB0", Length = "0x106")]
	private void MigrateSlotOnStart()
	{
	}

	[Token(Token = "0x6004328")]
	[Address(RVA = "0xC4FDC0", Offset = "0xC4EBC0", Length = "0x62")]
	private void SetAudioModeForLoadedScene()
	{
	}

	[Token(Token = "0x6004329")]
	[Address(RVA = "0xC4FE30", Offset = "0xC4EC30", Length = "0x1BC")]
	private void PostMigrationDestoryLitFlareOrTorch()
	{
	}

	[Token(Token = "0x600432A")]
	[Address(RVA = "0xC4FFF0", Offset = "0xC4EDF0", Length = "0x8B")]
	private void HandleCheckpointSaveRequest()
	{
	}

	[Token(Token = "0x600432B")]
	[Address(RVA = "0xC50080", Offset = "0xC4EE80", Length = "0x54")]
	public static void RequestSaveCheckpoint()
	{
	}

	[Token(Token = "0x600432C")]
	[Address(RVA = "0xC500E0", Offset = "0xC4EEE0", Length = "0x57")]
	public static bool GetAllowPhysicsSimulationControl()
	{
		return false;
	}

	[Token(Token = "0x600432D")]
	[Address(RVA = "0xC50140", Offset = "0xC4EF40", Length = "0x7A")]
	public static void SetAllowPhysicsSimulationControl(bool allow)
	{
	}

	[Token(Token = "0x600432E")]
	[Address(RVA = "0xC501C0", Offset = "0xC4EFC0", Length = "0x128")]
	public static void SetPhysicsAutoSimulationEnabled(bool enabled)
	{
	}

	[Token(Token = "0x600432F")]
	[Address(RVA = "0xC502F0", Offset = "0xC4F0F0", Length = "0x877")]
	public static void HandlePlayerDeath(string overrideCauseOfDeath = null)
	{
	}

	[Token(Token = "0x6004330")]
	[Address(RVA = "0xC50B70", Offset = "0xC4F970", Length = "0x204")]
	public static void InitializeCulture()
	{
	}

	[Token(Token = "0x6004331")]
	[Address(RVA = "0xC50D80", Offset = "0xC4FB80", Length = "0x21E")]
	public static void DestroyPlayerObject()
	{
	}

	[Conditional("__DEBUG")]
	[Token(Token = "0x6004332")]
	[Address(RVA = "0xC50FA0", Offset = "0xC4FDA0", Length = "0x43")]
	private static void DebugNotAllowedToLoad(string message)
	{
	}

	[Conditional("__DEBUG")]
	[Token(Token = "0x6004333")]
	[Address(RVA = "0xC50FF0", Offset = "0xC4FDF0", Length = "0x43")]
	private static void DebugNotAllowedToSave(string message)
	{
	}

	[Token(Token = "0x6004334")]
	[Address(RVA = "0xC51040", Offset = "0xC4FE40", Length = "0x1E1")]
	public static void PauseWhenFocusLost(bool focusLost)
	{
	}

	[Token(Token = "0x6004335")]
	[Address(RVA = "0xC51230", Offset = "0xC50030", Length = "0xBF")]
	public static bool IsFrameValidToUpdate(GameplayComponent component)
	{
		return false;
	}

	[Token(Token = "0x6004336")]
	[Address(RVA = "0xC512F0", Offset = "0xC500F0", Length = "0x81")]
	public static float GetDeltaTime(GameplayComponent component)
	{
		return 0f;
	}

	[Token(Token = "0x6004337")]
	[Address(RVA = "0xC51380", Offset = "0xC50180", Length = "0x11C")]
	private static void RegisterDelayedComponent(GameplayComponent component, int frequency)
	{
	}

	[Token(Token = "0x6004338")]
	[Address(RVA = "0xC514A0", Offset = "0xC502A0", Length = "0x6A")]
	private static void RegisterDelayedComponents()
	{
	}

	[Token(Token = "0x6004339")]
	[Address(RVA = "0xC51510", Offset = "0xC50310", Length = "0x390")]
	private static void InitDelayedComponents()
	{
	}

	[Token(Token = "0x600433A")]
	[Address(RVA = "0xC518B0", Offset = "0xC506B0", Length = "0x77")]
	private static DelayedGameplayComponentInfo GetDelayedGameplayComponentInfo(GameplayComponent component)
	{
		return null;
	}

	[Token(Token = "0x600433B")]
	[Address(RVA = "0xC51930", Offset = "0xC50730", Length = "0x181")]
	private static void UpdateDelayedComponentsDeltaTime()
	{
	}

	[Token(Token = "0x600433C")]
	[Address(RVA = "0xC51AC0", Offset = "0xC508C0", Length = "0xB52")]
	public void LaunchSandbox()
	{
	}

	[Token(Token = "0x600433D")]
	[Address(RVA = "0xC52620", Offset = "0xC51420", Length = "0x134")]
	public static bool RegionLockedBySelectedMode()
	{
		return false;
	}

	[Token(Token = "0x600433E")]
	[Address(RVA = "0xC52760", Offset = "0xC51560", Length = "0xD1")]
	public static bool CompareSceneNames(string lhs, string rhs)
	{
		return false;
	}

	[Token(Token = "0x600433F")]
	[Address(RVA = "0xC52840", Offset = "0xC51640", Length = "0x157")]
	public static bool MatchesMainMenuSceneName(string name)
	{
		return false;
	}

	[Token(Token = "0x6004340")]
	[Address(RVA = "0xC529A0", Offset = "0xC517A0", Length = "0x4F")]
	public static void AsyncLoadMainMenu()
	{
	}

	[Token(Token = "0x6004341")]
	[Address(RVA = "0xC529F0", Offset = "0xC517F0", Length = "0xBB")]
	public static AsyncOperationHandle<SceneInstance> AsyncDirectLoadMainMenu(bool activateOnLoad = true)
	{
		return default(AsyncOperationHandle<SceneInstance>);
	}

	[Token(Token = "0x6004342")]
	[Address(RVA = "0xC52AB0", Offset = "0xC518B0", Length = "0x57")]
	public static bool IsEmptySceneActive()
	{
		return false;
	}

	[Token(Token = "0x6004343")]
	[Address(RVA = "0xC52B10", Offset = "0xC51910", Length = "0x57")]
	public static bool IsMainMenuActive()
	{
		return false;
	}

	[Token(Token = "0x6004344")]
	[Address(RVA = "0xC52B70", Offset = "0xC51970", Length = "0x64")]
	public static bool IsActiveScene(string sceneName)
	{
		return false;
	}

	[Token(Token = "0x6004345")]
	[Address(RVA = "0xC52BE0", Offset = "0xC519E0", Length = "0x1EF")]
	private static string GetTargetMainMenuSceneName()
	{
		return null;
	}

	[Token(Token = "0x6004346")]
	[Address(RVA = "0xC52DD0", Offset = "0xC51BD0", Length = "0x92")]
	public static bool IsWellKnownScene(string sceneName)
	{
		return false;
	}

	[Token(Token = "0x6004347")]
	[Address(RVA = "0xC52E70", Offset = "0xC51C70", Length = "0x20C")]
	private static void SetActiveScene(string sceneName)
	{
	}

	[Token(Token = "0x6004348")]
	[Address(RVA = "0x4AF5E0", Offset = "0x4AE3E0", Length = "0x43")]
	public GameManager()
	{
	}
}
