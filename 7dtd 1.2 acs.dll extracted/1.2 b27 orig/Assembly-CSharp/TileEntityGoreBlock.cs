using System;
using System.Collections;
using UnityEngine;

public class TileEntityGoreBlock : TileEntityLootContainer
{
	public TileEntityGoreBlock(Chunk _chunk) : base(_chunk)
	{
		this.tickTimeToRemove = GameTimer.Instance.ticks + 60000UL;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.GoreBlock;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (GameTimer.Instance.ticks > this.tickTimeToRemove)
		{
			ThreadManager.StartCoroutine(this.destroyBlockLater(world));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator destroyBlockLater(World world)
	{
		yield return new WaitForEndOfFrame();
		world.SetBlockRPC(base.ToWorldPos(), BlockValue.Air);
		yield break;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		this.tickTimeToRemove = _br.ReadUInt64();
		if (this.readVersion < 4)
		{
			this.tickTimeToRemove += 60000UL;
		}
	}

	public override void write(PooledBinaryWriter stream, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		stream.Write(this.tickTimeToRemove);
	}

	public ulong tickTimeToRemove;
}
