using System;
using System.Reflection;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public abstract class ModBaseEquippableComponent : ModBaseComponent
{
	public GameObject? EquippedModel;

	public object Implementation = "";

	public Action? OnEquipped;

	public Action? OnUnequipped;

	public Action? OnPrimaryAction;

	public Action? OnSecondaryAction;

	public Action? OnControlModeChangedWhileEquipped;

	public Action? OnAwake;

	public Action? OnEnabled;

	public Action? OnStart;

	public Action? OnUpdate;

	public Action? OnLateUpdate;

	public Action? OnDisabled;

	public string? EquippedModelPrefabName { get; set; }

	public string? ImplementationType { get; set; }

	public string? EquippingAudio { get; set; }

	public ModBaseEquippableComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	protected Action? CreateImplementationActionDelegate(string methodName)
	{
		MethodInfo method = Implementation.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			return null;
		}
		Logger.LogDebug("CreateImplementationActionDelegate " + method.Name);
		return (Action)Delegate.CreateDelegate(typeof(Action), Implementation, method);
	}

	protected virtual void Awake()
	{
		if (string.IsNullOrEmpty(ImplementationType))
		{
			return;
		}
		Logger.LogDebug("Awake ImplementationType " + ImplementationType);
		Type type = TypeResolver.Resolve(ImplementationType, throwErrorOnFailure: true);
		Implementation = Activator.CreateInstance(type);
		if (Implementation != null)
		{
			Logger.LogDebug("Awake Implementation " + Implementation.ToString());
			OnEquipped = CreateImplementationActionDelegate("OnEquipped");
			OnUnequipped = CreateImplementationActionDelegate("OnUnequipped");
			OnPrimaryAction = CreateImplementationActionDelegate("OnPrimaryAction");
			OnSecondaryAction = CreateImplementationActionDelegate("OnSecondaryAction");
			OnControlModeChangedWhileEquipped = CreateImplementationActionDelegate("OnControlModeChangedWhileEquipped");
			OnAwake = CreateImplementationActionDelegate("OnAwake");
			OnEnabled = CreateImplementationActionDelegate("OnEnabled");
			OnStart = CreateImplementationActionDelegate("OnStart");
			OnUpdate = CreateImplementationActionDelegate("OnUpdate");
			OnLateUpdate = CreateImplementationActionDelegate("OnLateUpdate");
			OnDisabled = CreateImplementationActionDelegate("OnDisabled");
			FieldInfo fieldInfo = type?.GetField("EquippableModComponent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fieldInfo != null && fieldInfo.FieldType == typeof(ModBaseEquippableComponent))
			{
				fieldInfo.SetValue(Implementation, this);
			}
			if (OnAwake != null)
			{
				Logger.LogDebug("Awake OnAwake.Invoke");
				OnAwake();
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (OnEnabled != null)
		{
			OnEnabled();
		}
	}

	protected virtual void Start()
	{
		if (OnStart != null)
		{
			OnStart();
		}
	}

	protected virtual void Update()
	{
		if (OnUpdate != null)
		{
			OnUpdate();
		}
	}

	protected virtual void LateUpdate()
	{
		if (OnLateUpdate != null)
		{
			OnLateUpdate();
		}
	}

	protected virtual void OnDisable()
	{
		if (OnDisabled != null)
		{
			OnDisabled();
		}
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string inheritanceName)
	{
		base.InitializeComponent(jsonDict, inheritanceName);
		JsonDictEntry entry = jsonDict.GetEntry(inheritanceName);
		EquippedModelPrefabName = entry.GetString("EquippedModelPrefab");
		ImplementationType = entry.GetString("ImplementationType");
		EquippingAudio = entry.GetString("EquippingAudio");
	}
}
