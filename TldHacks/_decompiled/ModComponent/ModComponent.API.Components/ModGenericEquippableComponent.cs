using System;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModGenericEquippableComponent : ModBaseEquippableComponent
{
	public ModGenericEquippableComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	protected override void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModGenericEquippableComponent>(this);
		base.Awake();
	}
}
