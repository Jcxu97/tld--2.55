using Il2Cpp;

namespace ModComponent.Mapper;

internal static class DefaultDrawLayers
{
	public static int GetDefaultDrawLayer(ClothingRegion clothingRegion, ClothingLayer clothingLayer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		return (clothingRegion - 2) switch
		{
			0 => GetDefaultHeadDrawLayer(clothingLayer), 
			5 => 40, 
			1 => 25, 
			2 => GetDefaultChestDrawLayer(clothingLayer), 
			3 => GetDefaultLegsDrawLayer(clothingLayer), 
			4 => GetDefaultFeetDrawLayer(clothingLayer), 
			_ => 60, 
		};
	}

	public static int GetDefaultHeadDrawLayer(ClothingLayer clothingLayer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		if ((int)clothingLayer != 0)
		{
			if ((int)clothingLayer == 1)
			{
				return 42;
			}
			return 60;
		}
		return 41;
	}

	public static int GetDefaultChestDrawLayer(ClothingLayer clothingLayer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		return (int)clothingLayer switch
		{
			0 => 21, 
			1 => 22, 
			2 => 26, 
			3 => 27, 
			_ => 60, 
		};
	}

	public static int GetDefaultLegsDrawLayer(ClothingLayer clothingLayer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		return (int)clothingLayer switch
		{
			0 => 1, 
			1 => 2, 
			2 => 15, 
			3 => 16, 
			_ => 60, 
		};
	}

	public static int GetDefaultFeetDrawLayer(ClothingLayer clothingLayer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		return (int)clothingLayer switch
		{
			0 => 5, 
			1 => 6, 
			2 => 13, 
			_ => 60, 
		};
	}

	public static int MaybeGetDefaultDrawLayer(int drawLayer, ClothingRegion clothingRegion, ClothingLayer clothingLayer)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (drawLayer <= 0)
		{
			return GetDefaultDrawLayer(clothingRegion, clothingLayer);
		}
		return drawLayer;
	}
}
