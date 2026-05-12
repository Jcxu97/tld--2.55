using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace ModSettings;

internal class CustomModeGUIBuilder : GUIBuilder
{
	internal class HinterlandSettingVisibilityListener : MonoBehaviour
	{
		private bool visible;

		private Group header;

		private UIGrid uiGrid;

		static HinterlandSettingVisibilityListener()
		{
			ClassInjector.RegisterTypeInIl2Cpp<HinterlandSettingVisibilityListener>();
		}

		public HinterlandSettingVisibilityListener(IntPtr ptr)
			: base(ptr)
		{
		}

		[HideFromIl2Cpp]
		internal void Init(bool visible, Group header, UIGrid uiGrid)
		{
			this.visible = visible;
			this.header = header;
			this.uiGrid = uiGrid;
			header.NotifyChildAdded(visible);
		}

		private void OnEnable()
		{
			UpdateVisibility(((Component)this).gameObject.activeSelf);
		}

		private void OnDisable()
		{
			UpdateVisibility(((Component)this).gameObject.activeSelf);
		}

		[HideFromIl2Cpp]
		private void UpdateVisibility(bool newVisible)
		{
			if (visible != newVisible)
			{
				visible = newVisible;
				header.NotifyChildVisible(newVisible);
				uiGrid.repositionNow = true;
			}
		}
	}

	private readonly Queue<Transform> sections = new Queue<Transform>();

	private bool afterLast;

	internal CustomModeGUIBuilder(Panel_CustomXPSetup panel)
		: base(CreateUIGrid(panel), panel.m_CustomXPMenuItemOrder)
	{
		panel.m_CustomXPMenuItemOrder.RemoveRange(1, panel.m_CustomXPMenuItemOrder.Count - 1);
		Transform scrollPanelOffsetTransform = panel.m_ScrollPanelOffsetTransform;
		int num = scrollPanelOffsetTransform.childCount - 1;
		for (int i = 0; i < num; i++)
		{
			Transform child = scrollPanelOffsetTransform.GetChild(i);
			sections.Enqueue(child);
		}
	}

	private static UIGrid CreateUIGrid(Panel_CustomXPSetup panel)
	{
		Panel_CustomXPSetup panel2 = panel;
		GameObject val = NGUITools.AddChild(((Component)panel2.m_ScrollPanelOffsetTransform).gameObject);
		((Object)val).name = "Custom Mode Settings UIGrid";
		UIGrid uiGrid = val.AddComponent<UIGrid>();
		uiGrid.arrangement = (Arrangement)1;
		uiGrid.cellHeight = 33f;
		uiGrid.hideInactive = true;
		uiGrid.onReposition = OnReposition.op_Implicit((Action)delegate
		{
			ResizeScrollBar(panel2, uiGrid);
		});
		return uiGrid;
	}

	private static void ResizeScrollBar(Panel_CustomXPSetup panel, UIGrid uiGrid)
	{
		UISlider componentInChildren = panel.m_Scrollbar.GetComponentInChildren<UISlider>(true);
		float height = panel.m_ScrollPanel.height;
		float num = ((UIProgressBar)componentInChildren).value * (panel.m_ScrollPanelHeight - height);
		float num3 = (panel.m_ScrollPanelHeight = ((Component)uiGrid).transform.childCount * 33);
		((Component)componentInChildren).GetComponent<ScrollbarThumbResizer>().SetNumSteps((int)panel.m_ScrollPanel.height, (int)num3);
		((UIProgressBar)componentInChildren).value = Mathf.Clamp01(num / Mathf.Max(1f, panel.m_ScrollPanelHeight - height));
		panel.OnScrollbarChange();
	}

	internal void NextSection()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (sections.Count == 0)
		{
			if (afterLast)
			{
				MelonLogger.Warning("Exhausted all GUI sections, skipping NextSection!");
			}
			afterLast = true;
			return;
		}
		Transform val = sections.Dequeue();
		int childCount = val.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = val.GetChild(0);
			if (child.childCount == 0)
			{
				GameObject val2 = NGUITools.AddChild(((Component)uiGrid).gameObject);
				GameObject val3 = NGUITools.AddChild(((Component)uiGrid).gameObject);
				lastHeader = new Header(val3, val2);
				child.parent = val3.transform;
				child.localPosition = Vector2.op_Implicit(new Vector2(-70f, 0f));
			}
			else
			{
				menuItems.Add(((Component)child).gameObject);
				child.parent = ((Component)uiGrid).transform;
				((Component)child).gameObject.AddComponent<HinterlandSettingVisibilityListener>().Init(((Component)child).gameObject.activeSelf, lastHeader, uiGrid);
			}
		}
		Object.Destroy((Object)(object)((Component)val).gameObject);
	}

	internal void Finish()
	{
		if (sections.Count > 0)
		{
			MelonLogger.Warning("More GUI elements in queue!");
			while (sections.Count > 0)
			{
				NextSection();
			}
		}
	}
}
