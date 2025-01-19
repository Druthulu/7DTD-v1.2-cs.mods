using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDynamicClientArrive : NetPackage
{
	public void BuildData()
	{
		foreach (DynamicMeshItem i in DynamicMeshManager.Instance.ItemsDictionary.Values)
		{
			this.Items.Add(this.FromPool(i));
		}
		Log.Out("Client package items: " + this.Items.Count.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionItemData FromPool(DynamicMeshItem i)
	{
		return new RegionItemData(i.WorldPosition.x, i.WorldPosition.z, i.UpdateTime);
	}

	public override void read(PooledBinaryReader reader)
	{
		int num = reader.ReadInt32();
		this.Items = new List<RegionItemData>(num);
		for (int i = 0; i < num; i++)
		{
			int x = reader.ReadInt32();
			int z = reader.ReadInt32();
			int updateTime = reader.ReadInt32();
			this.Items.Add(new RegionItemData(x, z, updateTime));
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override void write(PooledBinaryWriter writer)
	{
		base.write(writer);
		writer.Write(this.Items.Count);
		for (int i = 0; i < this.Items.Count; i++)
		{
			writer.Write(this.Items[i].X);
			writer.Write(this.Items[i].Z);
			writer.Write(this.Items[i].UpdateTime);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (DynamicMeshManager.CONTENT_ENABLED)
		{
			DynamicMeshServer.ClientMessageRecieved(this);
		}
	}

	public override int GetLength()
	{
		return 4 + 12 * this.Items.Count;
	}

	public override int Channel
	{
		get
		{
			return 0;
		}
	}

	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public List<RegionItemData> Items = new List<RegionItemData>();
}
