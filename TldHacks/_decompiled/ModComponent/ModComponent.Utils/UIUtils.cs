using Il2Cpp;
using UnityEngine;

namespace ModComponent.Utils;

internal static class UIUtils
{
	public static UITexture CreateOverlay(Texture2D texture)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		UIPanel val = NGUITools.AddChild<UIPanel>(((Component)UIRoot.list[0]).gameObject);
		UITexture obj = NGUITools.AddChild<UITexture>(((Component)val).gameObject);
		((UIWidget)obj).mainTexture = (Texture)(object)texture;
		Vector2 windowSize = val.GetWindowSize();
		((UIWidget)obj).width = (int)windowSize.x;
		((UIWidget)obj).height = (int)windowSize.y;
		return obj;
	}
}
