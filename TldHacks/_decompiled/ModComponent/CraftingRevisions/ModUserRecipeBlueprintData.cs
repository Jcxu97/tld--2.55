using System.Collections.Generic;

namespace CraftingRevisions;

internal sealed class ModUserRecipeBlueprintData
{
	public string? Name { get; set; }

	public List<ModRequiredGearItem> RequiredGear { get; set; } = new List<ModRequiredGearItem>();


	public List<ModRequiredPowder> RequiredPowder { get; set; } = new List<ModRequiredPowder>();


	public List<ModRequiredLiquid> RequiredLiquid { get; set; } = new List<ModRequiredLiquid>();


	public string? CraftedResult { get; set; }

	public int CraftedResultCount { get; set; }

	public int DurationMinutes { get; set; }

	public string? CraftingAudio { get; set; }
}
