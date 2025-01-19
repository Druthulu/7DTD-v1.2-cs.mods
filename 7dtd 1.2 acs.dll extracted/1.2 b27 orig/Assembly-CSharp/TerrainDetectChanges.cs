using System;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainDetectChanges : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTerrainChanged(TerrainChangedFlags flags)
	{
		TerrainChangedFlags terrainChangedFlags = flags & TerrainChangedFlags.Heightmap;
		if ((flags & TerrainChangedFlags.DelayedHeightmapUpdate) != (TerrainChangedFlags)0)
		{
			this.bChanged = true;
		}
		TerrainChangedFlags terrainChangedFlags2 = flags & TerrainChangedFlags.TreeInstances;
	}

	public bool bChanged;
}
