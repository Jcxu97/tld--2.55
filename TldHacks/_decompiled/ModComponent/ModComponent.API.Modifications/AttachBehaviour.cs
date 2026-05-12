using System;
using Il2CppSystem;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Modifications;

[RegisterTypeInIl2Cpp(false)]
internal class AttachBehaviour : MonoBehaviour
{
	public string BehaviourName = "";

	public bool ThrowOnError = true;

	public void Start()
	{
		try
		{
			Type val = TypeResolver.ResolveIl2Cpp(BehaviourName, throwErrorOnFailure: true);
			((Component)this).gameObject.AddComponent(val);
		}
		catch (Exception ex)
		{
			Logger.LogError("Could not load behaviour '" + BehaviourName + "': " + ex.Message);
			if (ThrowOnError)
			{
				throw ex;
			}
		}
	}

	public AttachBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}
}
