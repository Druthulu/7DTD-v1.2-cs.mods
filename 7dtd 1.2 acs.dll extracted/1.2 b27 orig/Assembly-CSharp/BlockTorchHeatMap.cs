using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockTorchHeatMap : BlockTorch
{
	public BlockTorchHeatMap()
	{
		this.IsRandomlyTick = true;
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
		if (this.HeatMapStrength > 0f)
		{
			AIDirector aidirector = _world.GetAIDirector();
			if (aidirector != null)
			{
				float num = 1f;
				num *= 0.4f;
				aidirector.NotifyActivity(EnumAIDirectorChunkEvent.Torch, _blockPos, this.HeatMapStrength * num, 720f);
			}
		}
		return true;
	}
}
