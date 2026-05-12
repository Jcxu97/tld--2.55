using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace ModComponent.Utils;

internal static class ActionPickerUtilities
{
	public struct ActionPickerData
	{
		private string SpriteName;

		private string LocID;

		private Action Callback;

		public ActionPickerData(string spriteName, string locId, Action callback)
		{
			SpriteName = spriteName;
			LocID = locId;
			Callback = callback;
		}

		public static implicit operator ActionPickerItemData(ActionPickerData data)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			return new ActionPickerItemData(data.SpriteName, data.LocID, Action.op_Implicit(data.Callback));
		}
	}

	public static void ShowCustomActionPicker(GameObject objectInteractedWith, List<ActionPickerData> actionList)
	{
		Panel_ActionPicker panel = InterfaceManager.GetPanel<Panel_ActionPicker>();
		if ((Object)(object)panel == (Object)null || InterfaceManager.IsOverlayActiveCached())
		{
			return;
		}
		if (panel.m_ActionPickerItemDataList == null)
		{
			panel.m_ActionPickerItemDataList = new List<ActionPickerItemData>();
		}
		else
		{
			panel.m_ActionPickerItemDataList.Clear();
		}
		foreach (ActionPickerData action in actionList)
		{
			panel.m_ActionPickerItemDataList.Add((ActionPickerItemData)action);
		}
		InterfaceManager.GetPanel<Panel_ActionPicker>().m_ObjectInteractedWith = objectInteractedWith;
		InterfaceManager.GetPanel<Panel_ActionPicker>().EnableWithCurrentList();
	}
}
