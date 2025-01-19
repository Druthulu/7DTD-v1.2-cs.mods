using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlantGrowing : BlockPlant
{
	public BlockPlantGrowing()
	{
		this.fertileLevel = 5;
	}

	public override void LateInit()
	{
		base.LateInit();
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingNextPlant))
		{
			this.nextPlant = ItemClass.GetItem(base.Properties.Values[BlockPlantGrowing.PropGrowingNextPlant], false).ToBlockValue();
			if (this.nextPlant.Equals(BlockValue.Air))
			{
				throw new Exception("Block with name '" + base.Properties.Values[BlockPlantGrowing.PropGrowingNextPlant] + "' not found!");
			}
		}
		this.growOnTop = BlockValue.Air;
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingIsGrowOnTopEnabled) && StringParsers.ParseBool(base.Properties.Values[BlockPlantGrowing.PropGrowingIsGrowOnTopEnabled], 0, -1, true))
		{
			this.bGrowOnTopEnabled = true;
			if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingGrowOnTop))
			{
				this.growOnTop = ItemClass.GetItem(base.Properties.Values[BlockPlantGrowing.PropGrowingGrowOnTop], false).ToBlockValue();
				if (this.growOnTop.Equals(BlockValue.Air))
				{
					throw new Exception("Block with name '" + base.Properties.Values[BlockPlantGrowing.PropGrowingGrowOnTop] + "' not found!");
				}
			}
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingGrowthRate))
		{
			this.growthRate = StringParsers.ParseFloat(base.Properties.Values[BlockPlantGrowing.PropGrowingGrowthRate], 0, -1, NumberStyles.Any);
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingGrowthDeviation))
		{
			this.growthDeviation = StringParsers.ParseFloat(base.Properties.Values[BlockPlantGrowing.PropGrowingGrowthDeviation], 0, -1, NumberStyles.Any);
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingFertileLevel))
		{
			this.fertileLevel = int.Parse(base.Properties.Values[BlockPlantGrowing.PropGrowingFertileLevel]);
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingLightLevelStay))
		{
			this.lightLevelStay = int.Parse(base.Properties.Values[BlockPlantGrowing.PropGrowingLightLevelStay]);
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingLightLevelGrow))
		{
			this.lightLevelGrow = int.Parse(base.Properties.Values[BlockPlantGrowing.PropGrowingLightLevelGrow]);
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingGrowIfAnythinOnTop))
		{
			this.isPlantGrowingIfAnythingOnTop = StringParsers.ParseBool(base.Properties.Values[BlockPlantGrowing.PropGrowingGrowIfAnythinOnTop], 0, -1, true);
		}
		if (base.Properties.Values.ContainsKey(BlockPlantGrowing.PropGrowingIsRandom))
		{
			this.isPlantGrowingRandom = StringParsers.ParseBool(base.Properties.Values[BlockPlantGrowing.PropGrowingIsRandom], 0, -1, true);
		}
		if (this.growthRate > 0f)
		{
			this.BlockTag = BlockTags.GrowablePlant;
			this.IsRandomlyTick = true;
			return;
		}
		this.IsRandomlyTick = false;
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			byte light = chunkCluster.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN);
			if ((int)light < this.lightLevelStay || (int)light < this.lightLevelGrow)
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanGrowOn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValueOfPlant)
	{
		return this.fertileLevel == 0 || _world.GetBlock(_clrIdx, _blockPos).Block.blockMaterial.FertileLevel >= this.fertileLevel;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_ea is EntityPlayerLocal)
		{
			_ea.Progression.AddLevelExp((int)_result.blockValue.Block.blockMaterial.Experience, "_xpOther", Progression.XPTypes.Other, true, true);
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (!_world.IsRemote())
		{
			this.addScheduledTick(_world, _chunk.ClrIdx, _blockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addScheduledTick(WorldBase _world, int _clrIdx, Vector3i _blockPos)
	{
		if (!this.isPlantGrowingRandom)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, this.GetTickRate());
			return;
		}
		int num = (int)this.GetTickRate();
		int num2 = (int)((float)num * this.growthDeviation);
		int num3 = num / 2;
		int max = num + num3;
		GameRandom gameRandom = _world.GetGameRandom();
		int num4;
		int num5;
		do
		{
			float randomGaussian = gameRandom.RandomGaussian;
			num4 = Mathf.RoundToInt((float)num + (float)num2 * randomGaussian);
			num5 = Mathf.Clamp(num4, num3, max);
		}
		while (num5 != num4);
		_world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, (ulong)((long)num5));
	}

	public override ulong GetTickRate()
	{
		return (ulong)(this.growthRate * 20f * 60f);
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (this.nextPlant.isair)
		{
			return false;
		}
		if (!this.CheckPlantAlive(_world, _clrIdx, _blockPos, _blockValue))
		{
			return true;
		}
		if (_bRandomTick)
		{
			this.addScheduledTick(_world, _clrIdx, _blockPos);
			return true;
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return true;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		if ((int)chunkCluster.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN) < this.lightLevelGrow)
		{
			this.addScheduledTick(_world, _clrIdx, _blockPos);
			return true;
		}
		BlockValue block = _world.GetBlock(_clrIdx, _blockPos + Vector3i.up);
		if (!this.isPlantGrowingIfAnythingOnTop && !block.isair)
		{
			return true;
		}
		BlockPlant blockPlant = this.nextPlant.Block as BlockPlant;
		if (blockPlant != null && !blockPlant.CanGrowOn(_world, _clrIdx, _blockPos + Vector3i.down, this.nextPlant))
		{
			return true;
		}
		_blockValue.type = this.nextPlant.type;
		BiomeDefinition biome = ((World)_world).GetBiome(_blockPos.x, _blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(_blockValue.type))
		{
			_blockValue.type = biome.Replacements[_blockValue.type];
		}
		BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(_blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
		blockValue.rotation = _blockValue.rotation;
		blockValue.meta = _blockValue.meta;
		blockValue.meta2 = 0;
		_blockValue = blockValue;
		if (this.bGrowOnTopEnabled)
		{
			_blockValue.meta = (_blockValue.meta + 1 & 15);
		}
		if (this.isPlantGrowingRandom || _ticksIfLoaded <= this.GetTickRate() || !_blockValue.Block.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, false, _ticksIfLoaded - this.GetTickRate(), _rnd))
		{
			_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
		}
		if (!this.growOnTop.isair && _blockPos.y + 1 < 255 && block.isair)
		{
			_blockValue.type = this.growOnTop.type;
			_blockValue = _blockValue.Block.OnBlockPlaced(_world, _clrIdx, _blockPos, _blockValue, _rnd);
			Block block2 = _blockValue.Block;
			if (_blockValue.damage >= block2.blockMaterial.MaxDamage)
			{
				_blockValue.damage = block2.blockMaterial.MaxDamage - 1;
			}
			if (this.isPlantGrowingRandom || _ticksIfLoaded <= this.GetTickRate() || !block2.UpdateTick(_world, _clrIdx, _blockPos + Vector3i.up, _blockValue, false, _ticksIfLoaded - this.GetTickRate(), _rnd))
			{
				_world.SetBlockRPC(_clrIdx, _blockPos + Vector3i.up, _blockValue);
			}
		}
		return true;
	}

	public BlockValue ForceNextGrowStage(World _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockValue block = _world.GetBlock(_clrIdx, _blockPos + Vector3i.up);
		if (!this.isPlantGrowingIfAnythingOnTop && !block.isair)
		{
			return _blockValue;
		}
		_blockValue.type = this.nextPlant.type;
		BiomeDefinition biome = _world.GetBiome(_blockPos.x, _blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(_blockValue.type))
		{
			_blockValue.type = biome.Replacements[_blockValue.type];
		}
		BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(_blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
		blockValue.rotation = _blockValue.rotation;
		blockValue.meta = _blockValue.meta;
		blockValue.meta2 = 0;
		_blockValue = blockValue;
		if (this.bGrowOnTopEnabled)
		{
			_blockValue.meta = (_blockValue.meta + 1 & 15);
		}
		if (!this.growOnTop.isair && _blockPos.y + 1 < 255 && block.isair)
		{
			_blockValue.type = this.growOnTop.type;
			_blockValue = _blockValue.Block.OnBlockPlaced(_world, _clrIdx, _blockPos, _blockValue, _world.GetGameRandom());
			Block block2 = _blockValue.Block;
			if (_blockValue.damage >= block2.blockMaterial.MaxDamage)
			{
				_blockValue.damage = block2.blockMaterial.MaxDamage - 1;
			}
			_world.SetBlockRPC(_clrIdx, _blockPos + Vector3i.up, _blockValue);
		}
		return _blockValue;
	}

	public virtual float GetGrowthRate()
	{
		return this.growthRate;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingNextPlant = "PlantGrowing.Next";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowthRate = "PlantGrowing.GrowthRate";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowthDeviation = "PlantGrowing.GrowthDeviation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingFertileLevel = "PlantGrowing.FertileLevel";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowOnTop = "PlantGrowing.GrowOnTop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingIsGrowOnTopEnabled = "PlantGrowing.IsGrowOnTopEnabled";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingLightLevelStay = "PlantGrowing.LightLevelStay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingLightLevelGrow = "PlantGrowing.LightLevelGrow";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingIsRandom = "PlantGrowing.IsRandom";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowIfAnythinOnTop = "PlantGrowing.GrowIfAnythinOnTop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue nextPlant;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue growOnTop;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bGrowOnTopEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float growthRate;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float growthDeviation = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int lightLevelGrow = 8;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPlantGrowingRandom = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPlantGrowingIfAnythingOnTop = true;
}
