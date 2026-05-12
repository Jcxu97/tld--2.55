using System;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModGenericComponent : ModBaseComponent
{
	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModGenericComponent>(this);
	}

	public ModGenericComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}
}
