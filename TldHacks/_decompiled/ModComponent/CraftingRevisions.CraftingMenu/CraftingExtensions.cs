using Il2Cpp;
using UnityEngine;

namespace CraftingRevisions.CraftingMenu;

internal static class CraftingExtensions
{
	public static void Move(this UIButton btn, float xOffset, float yOffset, float zOffset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)btn).transform.localPosition;
		((Component)btn).transform.localPosition = new Vector3(localPosition.x + xOffset, localPosition.y + yOffset, localPosition.z + zOffset);
	}

	public static void SetSpriteName(this UIButton btn, string newSpriteName)
	{
		UISprite component = ((Component)btn).GetComponent<UISprite>();
		component.spriteName = newSpriteName;
		((UIRect)component).OnInit();
	}

	public static UIButton Instantiate(this UIButton original)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		UIButton component = Object.Instantiate<GameObject>(((Component)original).gameObject).GetComponent<UIButton>();
		((Component)component).transform.parent = ((Component)original).transform.parent;
		((Component)component).transform.localPosition = ((Component)original).transform.localPosition;
		((Component)component).transform.localScale = Vector3.one;
		return component;
	}
}
