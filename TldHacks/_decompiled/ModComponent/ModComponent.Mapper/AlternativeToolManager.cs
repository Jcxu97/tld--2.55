using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper;

internal static class AlternativeToolManager
{
	private static List<ModToolComponent> toolList = new List<ModToolComponent>();

	private static List<string> templateNameList = new List<string>();

	internal static void AddToList(ModToolComponent alternateTool, string templateName)
	{
		toolList.Add(alternateTool);
		templateNameList.Add(templateName);
	}

	private static void Clear()
	{
		toolList = new List<ModToolComponent>();
		templateNameList = new List<string>();
	}

	internal static void ProcessList()
	{
		for (int i = 0; i < toolList.Count; i++)
		{
			AddAlternativeTool(toolList[i], templateNameList[i]);
		}
		Clear();
	}

	private static void AddAlternativeTool(ModToolComponent modToolComponent, string templateName)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		GameObject val = AssetBundleUtils.LoadAsset<GameObject>(templateName);
		if (!((Object)(object)val == (Object)null))
		{
			AlternateTools orCreateComponent = val.GetOrCreateComponent<AlternateTools>();
			List<AssetReferenceGearItem> list = new List<AssetReferenceGearItem>();
			if (((IEnumerable<AssetReferenceGearItem>)orCreateComponent.m_AlternateTools).Count() > 0)
			{
				list.AddRange((IEnumerable<AssetReferenceGearItem>)orCreateComponent.m_AlternateTools);
			}
			list.Add(new AssetReferenceGearItem(templateName));
			orCreateComponent.m_AlternateTools = Il2CppReferenceArray<AssetReferenceGearItem>.op_Implicit(list.ToArray());
		}
	}
}
