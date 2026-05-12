using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace GearSpawner;

public readonly record struct GearSpawnInfo(string Tag, Vector3 Position, string PrefabName, Quaternion Rotation, float SpawnChance)
{
	internal GearSpawnInfo NormalizePrefabName()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!PrefabName.StartsWith("GEAR_", StringComparison.Ordinal))
		{
			return new GearSpawnInfo(Tag, Position, "GEAR_" + PrefabName, Rotation, SpawnChance);
		}
		return this;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		builder.Append("Tag = ");
		builder.Append((object?)Tag);
		builder.Append(", Position = ");
		Vector3 position = Position;
		builder.Append(((object)(Vector3)(ref position)).ToString());
		builder.Append(", PrefabName = ");
		builder.Append((object?)PrefabName);
		builder.Append(", Rotation = ");
		Quaternion rotation = Rotation;
		builder.Append(((object)(Quaternion)(ref rotation)).ToString());
		builder.Append(", SpawnChance = ");
		builder.Append(SpawnChance.ToString());
		return true;
	}
}
