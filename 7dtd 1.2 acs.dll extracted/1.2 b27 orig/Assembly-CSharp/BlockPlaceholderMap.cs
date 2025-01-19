using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlaceholderMap
{
	public static void InitStatic()
	{
		BlockPlaceholderMap.Instance = new BlockPlaceholderMap();
	}

	public static void Cleanup()
	{
		if (BlockPlaceholderMap.Instance != null)
		{
			BlockPlaceholderMap.Instance.Clear();
		}
	}

	public void AddPlaceholder(BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation)
	{
		this.addPlaceholderInternal(this.placeholders, _placeholderBlockValue, _targetValue, _targetProb, _biome, _randomRotation);
	}

	public void AddQuestResetPlaceholder(BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation, FastTags<TagGroup.Global> questTags)
	{
		if (!this.questResetPlaceholders.ContainsKey(_placeholderBlockValue))
		{
			this.questResetPlaceholders.Add(_placeholderBlockValue, new List<BlockPlaceholderMap.QuestPlaceholderEntry>());
		}
		for (int i = 0; i < this.questResetPlaceholders[_placeholderBlockValue].Count; i++)
		{
			BlockPlaceholderMap.QuestPlaceholderEntry questPlaceholderEntry = this.questResetPlaceholders[_placeholderBlockValue][i];
			if (questPlaceholderEntry.QuestTag.Test_AnySet(questTags))
			{
				questPlaceholderEntry.PlaceholderList.Add(new BlockPlaceholderMap.PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation));
				return;
			}
		}
		BlockPlaceholderMap.QuestPlaceholderEntry questPlaceholderEntry2 = default(BlockPlaceholderMap.QuestPlaceholderEntry);
		questPlaceholderEntry2.QuestTag = questTags;
		questPlaceholderEntry2.PlaceholderList = new List<BlockPlaceholderMap.PlaceholderTarget>();
		questPlaceholderEntry2.PlaceholderList.Add(new BlockPlaceholderMap.PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation));
		this.questResetPlaceholders[_placeholderBlockValue].Add(questPlaceholderEntry2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addPlaceholderInternal(Dictionary<BlockValue, List<BlockPlaceholderMap.PlaceholderTarget>> _map, BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation)
	{
		if (!_map.ContainsKey(_placeholderBlockValue))
		{
			_map.Add(_placeholderBlockValue, new List<BlockPlaceholderMap.PlaceholderTarget>());
		}
		_map[_placeholderBlockValue].Add(new BlockPlaceholderMap.PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation));
	}

	public bool IsReplaceableBlockType(BlockValue _blockValue)
	{
		return !_blockValue.isair && this.placeholders.ContainsKey(_blockValue);
	}

	public BlockValue Replace(BlockValue _blockValue, GameRandom _random, int _blockX, int _blockZ, bool _useAlternate = false)
	{
		Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_blockX, 0, _blockZ);
		return this.Replace(_blockValue, _random, chunk, _blockX, 0, _blockZ, FastTags<TagGroup.Global>.none, _useAlternate, true);
	}

	public BlockValue Replace(BlockValue _blockValue, GameRandom _random, Chunk _chunk, int _blockX, int _blockY, int _blockZ, FastTags<TagGroup.Global> questTags, bool useAlternate = false, bool allowRandomRotation = true)
	{
		if (!this.placeholders.ContainsKey(_blockValue))
		{
			return _blockValue;
		}
		List<BlockPlaceholderMap.PlaceholderTarget> list = this.placeholders[_blockValue];
		bool ischild = _blockValue.ischild;
		Vector3i parent = _blockValue.parent;
		BlockValue result = _blockValue;
		string text = null;
		GameRandom gameRandom = _random;
		if (gameRandom == null)
		{
			Vector3i vector3i = _chunk.GetWorldPos() + new Vector3i(_blockX, _blockY, _blockZ);
			if (ischild)
			{
				vector3i += parent;
			}
			gameRandom = Utils.RandomFromSeedOnPos(vector3i.x, vector3i.y, vector3i.z, GameManager.Instance.World.Seed);
		}
		if (useAlternate && this.questResetPlaceholders.ContainsKey(_blockValue))
		{
			List<BlockPlaceholderMap.QuestPlaceholderEntry> list2 = this.questResetPlaceholders[_blockValue];
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].QuestTag.Test_AnySet(questTags))
				{
					list = list2[i].PlaceholderList;
					break;
				}
			}
		}
		BlockPlaceholderMap.PlaceholderTarget placeholderTarget;
		do
		{
			int index = gameRandom.RandomRange(list.Count);
			placeholderTarget = list[index];
			if (placeholderTarget.biome != null)
			{
				if (text == null)
				{
					byte biomeId = _chunk.GetBiomeId(World.toBlockXZ(_blockX), World.toBlockXZ(_blockZ));
					text = GameManager.Instance.World.Biomes.GetBiome(biomeId).m_sBiomeName;
					bool flag = false;
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].biome == null || list[j].biome.EqualsCaseInsensitive(text))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						goto IL_22B;
					}
				}
				if (!placeholderTarget.biome.EqualsCaseInsensitive(text))
				{
					continue;
				}
			}
		}
		while (gameRandom.RandomFloat >= placeholderTarget.prob);
		result.type = placeholderTarget.block.type;
		if (allowRandomRotation && placeholderTarget.randomRotation)
		{
			byte b;
			if (result.Block.shape.Has45DegreeRotations)
			{
				b = (byte)gameRandom.RandomRange(8);
				if (b > 3)
				{
					b += 20;
				}
			}
			else
			{
				b = (byte)gameRandom.RandomRange(4);
			}
			result.rotation = b;
		}
		else
		{
			result.rotation = _blockValue.rotation;
		}
		IL_22B:
		if (result.Equals(_blockValue))
		{
			result = BlockValue.Air;
		}
		if (_random == null)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}
		if (ischild)
		{
			result.ischild = true;
			result.parent = parent;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Clear()
	{
		this.questResetPlaceholders.Clear();
		this.placeholders.Clear();
	}

	public static BlockPlaceholderMap Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<BlockValue, List<BlockPlaceholderMap.PlaceholderTarget>> placeholders = new Dictionary<BlockValue, List<BlockPlaceholderMap.PlaceholderTarget>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<BlockValue, List<BlockPlaceholderMap.QuestPlaceholderEntry>> questResetPlaceholders = new Dictionary<BlockValue, List<BlockPlaceholderMap.QuestPlaceholderEntry>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PlaceholderTarget
	{
		public PlaceholderTarget(BlockValue _block, float _prob, string _biome, bool _randomRotation)
		{
			this.block = _block;
			this.prob = _prob;
			this.biome = _biome;
			this.randomRotation = _randomRotation;
		}

		public readonly BlockValue block;

		public readonly float prob;

		public readonly string biome;

		public readonly bool randomRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct QuestPlaceholderEntry
	{
		public FastTags<TagGroup.Global> QuestTag;

		public List<BlockPlaceholderMap.PlaceholderTarget> PlaceholderList;
	}
}
