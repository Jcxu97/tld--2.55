namespace ModComponent.API.Components;

internal static class ModLiquidComponentExtensions
{
	public static string GetLiquidTypeString(this ModLiquidComponent.LiquidKind lk)
	{
		return lk switch
		{
			ModLiquidComponent.LiquidKind.Water => "LIQUID_WaterPotable", 
			ModLiquidComponent.LiquidKind.Kerosene => "LIQUID_Kerosene", 
			_ => lk.ToString(), 
		};
	}
}
