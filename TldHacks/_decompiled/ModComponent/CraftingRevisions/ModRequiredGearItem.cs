using System.Text.Json.Serialization;
using Il2CppTLD.Gear;

namespace CraftingRevisions;

internal sealed class ModRequiredGearItem
{
	public string? Item { get; set; }

	public int Count { get; set; }

	public float Quantity { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public Units Units { get; set; }
}
