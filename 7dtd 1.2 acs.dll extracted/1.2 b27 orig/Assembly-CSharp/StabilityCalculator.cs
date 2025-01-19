﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StabilityCalculator
{
	public void Init(WorldBase _world)
	{
		this.bRunning = true;
		StabilityCalculator.world = _world;
		this.channelCalculator = new ChannelCalculator(StabilityCalculator.world);
		this.queueStabilityAvail = new Queue<Vector3i>();
		this.queueStabilityEmpty = new Queue<Vector3i>();
		this.ienumCoroutine = new StabilityCalculator.UpdatePhysics(this);
		ThreadManager.StartCoroutine(this.ienumCoroutine);
	}

	public void Cleanup()
	{
		this.bRunning = false;
		if (this.ienumCoroutine != null)
		{
			ThreadManager.StopCoroutine(this.ienumCoroutine);
			this.ienumCoroutine = null;
		}
		this.channelCalculator = null;
	}

	public void BlockRemovedAt(Vector3i _pos)
	{
		if (_pos.y >= 255)
		{
			return;
		}
		this.stab0Positions.Clear();
		this.channelCalculator.BlockRemovedAt(_pos, this.stab0Positions);
		if (StabilityCalculator.world.IsRemote())
		{
			return;
		}
		IChunk chunk = null;
		foreach (Vector3i other in Vector3i.AllDirections)
		{
			Vector3i vector3i = _pos + other;
			if (StabilityCalculator.world.GetChunkFromWorldPos(vector3i, ref chunk))
			{
				int x = World.toBlockXZ(vector3i.x);
				int y = World.toBlockY(vector3i.y);
				int z = World.toBlockXZ(vector3i.z);
				BlockValue blockNoDamage;
				BlockValue blockValue = blockNoDamage = chunk.GetBlockNoDamage(x, y, z);
				if (!blockNoDamage.isair && !blockValue.Block.blockMaterial.IsLiquid)
				{
					int stability = (int)chunk.GetStability(x, y, z);
					if (stability == 0)
					{
						if (!this.stab0Positions.Contains(vector3i))
						{
							this.queueStabilityEmpty.Enqueue(vector3i);
						}
					}
					else if (stability < 15 && this.queueStabilityAvail.Count < 200)
					{
						this.queueStabilityAvail.Enqueue(vector3i);
					}
				}
			}
		}
		foreach (Vector3i item in this.stab0Positions)
		{
			this.queueStabilityEmpty.Enqueue(item);
		}
	}

	public void BlockPlacedAt(Vector3i _pos, bool _bForceFullStabe = false)
	{
		this.channelCalculator.BlockPlacedAt(_pos, _bForceFullStabe);
		if (StabilityCalculator.world.IsRemote())
		{
			return;
		}
		if (this.queueStabilityAvail.Count < 200)
		{
			this.queueStabilityAvail.Enqueue(_pos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void physicsIsolation(Vector3i _pos)
	{
		this.queueIsolation.Clear();
		this.queueIsolation.Enqueue(_pos);
		this.hashSetIsolation.Clear();
		this.hashSetIsolation.Add(_pos);
		this.hashSetProcessed.Clear();
		this.hashSetProcessed.Add(_pos);
		IChunk chunk = null;
		Vector3i vector3i = Vector3i.zero;
		while (this.queueIsolation.Count > 0)
		{
			Vector3i one = this.queueIsolation.Dequeue();
			Vector3i[] allDirections = Vector3i.AllDirections;
			for (int i = 0; i < allDirections.Length; i++)
			{
				vector3i = one + allDirections[i];
				if (StabilityCalculator.world.GetChunkFromWorldPos(vector3i, ref chunk))
				{
					Vector3i vector3i2 = World.toBlock(vector3i);
					BlockValue blockNoDamage = chunk.GetBlockNoDamage(vector3i2.x, vector3i2.y, vector3i2.z);
					if (!blockNoDamage.isair && !blockNoDamage.Block.blockMaterial.IsLiquid && !blockNoDamage.Block.StabilityIgnore && !this.hashSetProcessed.Contains(vector3i))
					{
						this.hashSetProcessed.Add(vector3i);
						if (chunk.GetStability(vector3i2.x, vector3i2.y, vector3i2.z) <= 0)
						{
							if (!blockNoDamage.ischild)
							{
								this.hashSetIsolation.Add(vector3i);
								if (this.hashSetIsolation.Count >= 1000)
								{
									return;
								}
							}
							this.queueIsolation.Enqueue(vector3i);
						}
					}
				}
			}
		}
	}

	public static float GetBlockStability(Vector3i _pos)
	{
		float blockStability = StabilityCalculator.GetBlockStability(_pos, BlockValue.Air);
		StabilityCalculator.posChecked.Clear();
		StabilityCalculator.posToCheck.Clear();
		StabilityCalculator.posToCheckNext.Clear();
		return blockStability;
	}

	public static float GetBlockStabilityIfPlaced(Vector3i _pos, BlockValue _bv)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		Block block = _bv.Block;
		int value = block.StabilitySupport ? 1 : 0;
		float num;
		if (block.isMultiBlock)
		{
			int type = _bv.type;
			int rotation = (int)_bv.rotation;
			Vector3i vector3i = Vector3i.max;
			Vector3i vector3i2 = Vector3i.min;
			for (int i = block.multiBlockPos.Length - 1; i >= 0; i--)
			{
				Vector3i vector3i3 = block.multiBlockPos.Get(i, type, rotation) + _pos;
				if (!StabilityCalculator.posPlaced.ContainsKey(vector3i3))
				{
					StabilityCalculator.posPlaced.Add(vector3i3, value);
					vector3i = Vector3i.Min(vector3i, vector3i3);
					vector3i2 = Vector3i.Max(vector3i2, vector3i3);
				}
			}
			IChunk chunk = null;
			for (int j = vector3i.z; j <= vector3i2.z; j++)
			{
				Vector3i key;
				key.z = j;
				for (int k = vector3i.x; k <= vector3i2.x; k++)
				{
					key.x = k;
					StabilityCalculator.world.GetChunkFromWorldPos(k, j, ref chunk);
					if (chunk != null && chunk.GetStability(k & 15, vector3i.y - 1, j & 15) == 15)
					{
						for (int l = vector3i.y; l <= vector3i2.y; l++)
						{
							key.y = l;
							StabilityCalculator.posPlaced[key] = 15;
						}
					}
				}
			}
			num = 1f;
			using (Dictionary<Vector3i, int>.KeyCollection.Enumerator enumerator = StabilityCalculator.posPlaced.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Vector3i vector3i4 = enumerator.Current;
					if (vector3i4.x == vector3i.x || vector3i4.x == vector3i2.x || vector3i4.y == vector3i.y || vector3i4.y == vector3i2.y || vector3i4.z == vector3i.z || vector3i4.z == vector3i2.z)
					{
						float blockStability = StabilityCalculator.GetBlockStability(vector3i4, _bv);
						if (blockStability < num)
						{
							num = blockStability;
							if (blockStability == 0f)
							{
								break;
							}
						}
						if (microStopwatch.ElapsedMicroseconds > 25000L)
						{
							break;
						}
					}
				}
				goto IL_246;
			}
		}
		StabilityCalculator.posPlaced.Add(_pos, value);
		num = StabilityCalculator.GetBlockStability(_pos, _bv);
		IL_246:
		StabilityCalculator.posPlaced.Clear();
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetBlockStability(Vector3i _pos, BlockValue _newBV)
	{
		StabilityCalculator.posChecked.Clear();
		StabilityCalculator.posToCheck.Clear();
		StabilityCalculator.posToCheckNext.Clear();
		if (!Block.BlocksLoaded)
		{
			return 1f;
		}
		if (!GameManager.bPhysicsActive)
		{
			return 1f;
		}
		BlockValue blockValue = StabilityCalculator.posPlaced.ContainsKey(_pos) ? _newBV : StabilityCalculator.world.GetBlock(_pos);
		int num = 0;
		int num2 = 0;
		StabilityCalculator.posChecked.Add(_pos);
		StabilityCalculator.posToCheck.Add(_pos);
		int num3 = 0;
		int num4 = 0;
		float num5 = 0f;
		new Vector3(0f, -1f, 0f);
		IChunk chunk = null;
		while (num4 < 25 && StabilityCalculator.posToCheck.Count > 0)
		{
			num4++;
			int num6 = num;
			int i = 0;
			while (i < StabilityCalculator.posToCheck.Count)
			{
				Vector3i vector3i = StabilityCalculator.posToCheck[i];
				int num7;
				BlockValue blockValue2;
				if (StabilityCalculator.posPlaced.TryGetValue(vector3i, out num7))
				{
					blockValue2 = _newBV;
					goto IL_10F;
				}
				StabilityCalculator.world.GetChunkFromWorldPos(vector3i.x, vector3i.z, ref chunk);
				if (chunk != null)
				{
					blockValue2 = chunk.GetBlockNoDamage(vector3i.x & 15, vector3i.y, vector3i.z & 15);
					goto IL_10F;
				}
				IL_284:
				i++;
				continue;
				IL_10F:
				num3 += 7;
				num2 += blockValue2.Block.blockMaterial.Mass.Value;
				Vector3i[] allDirectionsShuffled = Vector3i.AllDirectionsShuffled;
				for (int j = 0; j < allDirectionsShuffled.Length; j++)
				{
					Vector3i vector3i2 = vector3i + allDirectionsShuffled[j];
					if (vector3i2.y >= 0)
					{
						int stability;
						BlockValue other;
						if (StabilityCalculator.posPlaced.TryGetValue(vector3i2, out stability))
						{
							other = _newBV;
						}
						else
						{
							StabilityCalculator.world.GetChunkFromWorldPos(vector3i2.x, vector3i2.z, ref chunk);
							if (chunk == null)
							{
								goto IL_273;
							}
							int x = vector3i2.x & 15;
							int z = vector3i2.z & 15;
							other = chunk.GetBlockNoDamage(x, vector3i2.y, z);
							stability = (int)chunk.GetStability(x, vector3i2.y, z);
						}
						if (stability > 0)
						{
							if (stability == 15)
							{
								vector3i;
								int forceToOtherBlock = blockValue2.GetForceToOtherBlock(other);
								if (allDirectionsShuffled[j].y == -1)
								{
									num6 = 100000;
								}
								else
								{
									num6 += forceToOtherBlock;
								}
								num += forceToOtherBlock;
							}
							else if ((stability > 1 || other.Block.StabilitySupport) && StabilityCalculator.posChecked.Add(vector3i2))
							{
								StabilityCalculator.posToCheckNext.Add(vector3i2);
								if (allDirectionsShuffled[j].y == -1)
								{
									num6 = 100000;
								}
								else
								{
									num6 += blockValue2.GetForceToOtherBlock(other);
								}
							}
						}
					}
					IL_273:;
				}
				goto IL_284;
			}
			if (num2 > num6)
			{
				StabilityViewer.GetBlocks += num3;
				StabilityViewer.TotalIterations += num4;
				return 0f;
			}
			if (num6 > 0)
			{
				num5 = Mathf.Max(num5, (float)num2 / ((float)num6 * 1.01f));
			}
			List<Vector3i> list = StabilityCalculator.posToCheck;
			StabilityCalculator.posToCheck = StabilityCalculator.posToCheckNext;
			StabilityCalculator.posToCheckNext = list;
			StabilityCalculator.posToCheckNext.Clear();
		}
		StabilityViewer.GetBlocks += num3;
		StabilityViewer.TotalIterations += num4;
		return 1f - num5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> CalcPhysicsStabilityToFall(Vector3i _pos, int maxBlocksToCheck, out float calculatedStability)
	{
		List<Vector3i> list = null;
		calculatedStability = 0f;
		this.unstablePositions.Clear();
		this.unstablePositions.Add(_pos);
		this.positionsToCheck.Clear();
		this.positionsToCheck.Enqueue(_pos);
		this.uniqueUnstablePositions.Clear();
		int num = 0;
		int num2 = 0;
		IChunk chunk = null;
		int i = 0;
		while (i < maxBlocksToCheck)
		{
			int num3 = num;
			foreach (Vector3i vector3i in this.positionsToCheck)
			{
				StabilityCalculator.world.GetChunkFromWorldPos(vector3i, ref chunk);
				BlockValue blockValue = (chunk != null) ? chunk.GetBlockNoDamage(World.toBlockXZ(vector3i.x), vector3i.y, World.toBlockXZ(vector3i.z)) : BlockValue.Air;
				Block block = blockValue.Block;
				num2 += block.blockMaterial.Mass.Value;
				foreach (Vector3i vector3i2 in Vector3i.AllDirectionsShuffled)
				{
					Vector3i vector3i3 = vector3i + vector3i2;
					if (chunk == null || chunk.X != World.toChunkXZ(vector3i3.x) || chunk.Z != World.toChunkXZ(vector3i3.z))
					{
						chunk = StabilityCalculator.world.GetChunkFromWorldPos(vector3i3);
					}
					int x = World.toBlockXZ(vector3i3.x);
					int z = World.toBlockXZ(vector3i3.z);
					BlockValue other = (chunk != null) ? chunk.GetBlockNoDamage(x, vector3i3.y, z) : BlockValue.Air;
					int num4 = (int)((!other.isair && chunk != null) ? chunk.GetStability(x, vector3i3.y, z) : 0);
					if (num4 == 15)
					{
						int forceToOtherBlock = blockValue.GetForceToOtherBlock(other);
						if (vector3i2.y == -1)
						{
							num3 = 100000;
						}
						else
						{
							num3 += forceToOtherBlock;
						}
						num += forceToOtherBlock;
					}
					else if (((num4 > 0 && other.Block.StabilitySupport) || num4 > 1) && this.unstablePositions.Add(vector3i3))
					{
						this.uniqueUnstablePositions.Enqueue(vector3i3);
						if (vector3i2.y == -1)
						{
							num3 = 100000;
						}
						else
						{
							num3 += blockValue.GetForceToOtherBlock(other);
						}
					}
				}
			}
			if (num3 > 0)
			{
				calculatedStability = 1f - (float)num2 / (float)num3;
			}
			if (num2 > num3)
			{
				list = this.unstablePositions.Except(this.uniqueUnstablePositions).ToList<Vector3i>();
				if (list.Count == 0)
				{
					calculatedStability = 1f;
					break;
				}
				break;
			}
			else
			{
				if (this.uniqueUnstablePositions.Count == 0)
				{
					break;
				}
				this.positionsToCheck.Clear();
				Queue<Vector3i> queue = this.uniqueUnstablePositions;
				this.uniqueUnstablePositions = this.positionsToCheck;
				this.positionsToCheck = queue;
				this.uniqueUnstablePositions.Clear();
				i++;
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToFallingBlocks(IList<Vector3i> _list)
	{
		StabilityCalculator.world.AddFallingBlocks(_list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldBase world;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChannelCalculator channelCalculator;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ienumCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxIterations = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updatePeriod = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int isolatedBlockLimit = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int stabilityQueueLimit = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public int updateTimeLimit = 3000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> queueStabilityAvail;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> queueStabilityEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> stab0Positions = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> hashSetIsolation = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> hashSetProcessed = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> queueIsolation = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<Vector3i> posChecked = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3i> posToCheck = new List<Vector3i>(6);

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3i> posToCheckNext = new List<Vector3i>(6);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, int> posPlaced = new Dictionary<Vector3i, int>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInfiniteSupport = 100000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSupportScale = 1.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> unstablePositions = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> positionsToCheck = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> uniqueUnstablePositions = new Queue<Vector3i>();

	public class UpdatePhysics : IEnumerator<object>, IEnumerator, IDisposable
	{
		public UpdatePhysics(StabilityCalculator _sp)
		{
			this.physicsCalculator = _sp;
			this.sw = MicroStopwatch.a();
		}

		public object Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.myEnumerator;
			}
		}

		public object Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.myEnumerator;
			}
		}

		public bool MoveNext()
		{
			int num = this.state;
			this.state = -1;
			if (num > 1)
			{
				return false;
			}
			if (!GameStats.GetBool(EnumGameStats.ChunkStabilityEnabled))
			{
				this.myEnumerator = new WaitForSeconds(this.physicsCalculator.updatePeriod);
				this.state = 1;
				return true;
			}
			if (!this.physicsCalculator.bRunning)
			{
				this.state = -1;
				return false;
			}
			this.sw.ResetAndRestart();
			while (this.sw.ElapsedMicroseconds < (long)this.physicsCalculator.updateTimeLimit)
			{
				if (this.physicsCalculator.queueStabilityEmpty.Count <= 0)
				{
					break;
				}
				this.curPosition = this.physicsCalculator.queueStabilityEmpty.Dequeue();
				this.physicsCalculator.physicsIsolation(this.curPosition);
				foreach (Vector3i blockPos in this.physicsCalculator.hashSetIsolation)
				{
					((World)StabilityCalculator.world).AddFallingBlock(blockPos, false);
				}
			}
			while (this.sw.ElapsedMicroseconds < (long)this.physicsCalculator.updateTimeLimit && this.physicsCalculator.queueStabilityAvail.Count > 0)
			{
				Vector3i pos = this.physicsCalculator.queueStabilityAvail.Dequeue();
				float num2;
				List<Vector3i> list = this.physicsCalculator.CalcPhysicsStabilityToFall(pos, 20, out num2);
				if (list != null)
				{
					this.physicsCalculator.addToFallingBlocks(list);
				}
			}
			this.sw.Stop();
			this.state = 1;
			return true;
		}

		public void Dispose()
		{
			this.state = -1;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public MicroStopwatch sw;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i curPosition;

		[PublicizedFrom(EAccessModifier.Private)]
		public int state;

		[PublicizedFrom(EAccessModifier.Private)]
		public object myEnumerator;

		[PublicizedFrom(EAccessModifier.Private)]
		public StabilityCalculator physicsCalculator;
	}
}
