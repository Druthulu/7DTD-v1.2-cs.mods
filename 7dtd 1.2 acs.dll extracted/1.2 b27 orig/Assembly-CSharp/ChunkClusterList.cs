using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ChunkClusterList
{
	public ChunkClusterList()
	{
		this.AddLayerMappingTable(0, new Dictionary<string, int>
		{
			{
				"terraincollision",
				16
			},
			{
				"nocollision",
				14
			},
			{
				"grass",
				18
			},
			{
				"Glass",
				30
			},
			{
				"water",
				4
			},
			{
				"terrain",
				28
			}
		});
	}

	public void AddFixed(ChunkCluster _cc, int _index)
	{
		this.Cluster0 = _cc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddLayerMappingTable(int _id, Dictionary<string, int> _table)
	{
		for (int i = this.LayerMappingTable.Count - 1; i < _id; i++)
		{
			this.LayerMappingTable.Add(null);
		}
		this.LayerMappingTable[_id] = _table;
	}

	public ChunkCluster this[int _idx]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.Cluster0;
		}
	}

	public int Count
	{
		get
		{
			if (this.Cluster0 == null)
			{
				return 0;
			}
			return 1;
		}
	}

	public void Cleanup()
	{
		if (this.Cluster0 != null)
		{
			this.Cluster0.Cleanup();
			this.Cluster0 = null;
		}
	}

	public ChunkCluster Cluster0;

	public List<Dictionary<string, int>> LayerMappingTable = new List<Dictionary<string, int>>();
}
