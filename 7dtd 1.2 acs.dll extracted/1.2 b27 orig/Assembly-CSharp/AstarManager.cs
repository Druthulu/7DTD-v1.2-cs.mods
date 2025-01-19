﻿using System;
using System.Collections;
using System.Collections.Generic;
using GamePath;
using Pathfinding;
using UnityEngine;

public class AstarManager : MonoBehaviour
{
	public static void Init(GameObject obj)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty")
		{
			return;
		}
		Log.Out("AstarManager Init");
		obj.AddComponent<AstarManager>();
		new ASPPathFinderThread().StartWorkerThreads();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		AstarManager.Instance = this;
		if (!AstarPath.active)
		{
			UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/AStarPath"), Vector3.zero, Quaternion.identity).transform.SetParent(GameManager.Instance.transform, false);
		}
		this.astar = AstarPath.active;
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		chunkCache.OnBlockChangedDelegates += this.OnBlockChanged;
		chunkCache.OnBlockDamagedDelegates += this.OnBlockDamaged;
		this.OriginChanged();
	}

	public static PathNavigate CreateNavigator(EntityAlive _entity)
	{
		return new ASPPathNavigate(_entity);
	}

	public static void Cleanup()
	{
		if (!AstarManager.Instance)
		{
			return;
		}
		Log.Out("AstarManager Cleanup");
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		chunkCache.OnBlockChangedDelegates -= AstarManager.Instance.OnBlockChanged;
		chunkCache.OnBlockDamagedDelegates -= AstarManager.Instance.OnBlockDamaged;
		PathFinderThread.Instance.Cleanup();
		if (AstarPath.active)
		{
			AstarPath.active.enabled = false;
			UnityEngine.Object.Destroy(AstarPath.active.gameObject);
		}
		UnityEngine.Object.Destroy(AstarManager.Instance);
		AstarManager.Instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		float elapsedTime = 0f;
		while (this.astar != null)
		{
			if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
			{
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				yield return new WaitForSeconds(0.1f);
				if (!(GameManager.Instance == null) && GameManager.Instance.World != null)
				{
					elapsedTime += 0.1f;
					if (this.astar.IsAnyWorkItemInProgress)
					{
						this.lastWorkTime = Time.time;
					}
					else if (Time.time - this.lastWorkTime >= 0.4f && !this.astar.IsAnyGraphUpdateInProgress)
					{
						this.UpdateGraphs(elapsedTime);
						int num = this.areaList.Count;
						if (num > 0)
						{
							num = Mathf.Min(20, num);
							int num2 = 0;
							for (int i = 0; i < num; i++)
							{
								AstarManager.Area area = this.areaList[num2];
								area.updateDelay -= elapsedTime;
								if (area.updateDelay > 0f)
								{
									num2++;
								}
								else
								{
									if (area.next == null)
									{
										this.areaList.RemoveAt(num2);
									}
									else
									{
										this.areaList[num2] = area.next;
										num2++;
									}
									Bounds bounds;
									if (!area.isPartial)
									{
										bounds = default(Bounds);
										Vector3 vector = new Vector3((float)area.pos.x, 0f, (float)area.pos.y);
										Vector3 max = vector;
										max.x += 16f;
										max.z += 16f;
										bounds.SetMinMax(vector, max);
									}
									else
									{
										if (!area.hasBlocks)
										{
											goto IL_294;
										}
										bounds = area.bounds.ToBounds();
									}
									Vector3 vector2 = bounds.center;
									vector2.y = 128f;
									vector2 -= this.worldOrigin;
									bounds.center = vector2;
									Vector3 size = bounds.size;
									size.y = 320f;
									bounds.size = size;
									if (this.graphList.Count > 0)
									{
										LayerGridGraphUpdate layerGridGraphUpdate = new LayerGridGraphUpdate();
										layerGridGraphUpdate.bounds = bounds;
										layerGridGraphUpdate.recalculateNodes = true;
										this.astar.UpdateGraphs(layerGridGraphUpdate);
									}
								}
								IL_294:;
							}
						}
						elapsedTime = 0f;
					}
				}
			}
		}
		yield break;
	}

	public void AddLocation(Vector3 pos3d, int size)
	{
		Vector2 vector;
		vector.x = pos3d.x;
		vector.y = pos3d.z;
		AstarManager.Location location = this.FindLocation(vector, size);
		if (location == null)
		{
			location = new AstarManager.Location();
			location.pos = vector;
			location.size = size;
			this.locations.Add(location);
		}
		else
		{
			location.pos = (location.pos + vector) * 0.5f;
		}
		location.duration = 4f;
	}

	public void AddLocationLine(Vector3 startPos, Vector3 endPos, int size)
	{
		startPos.y = 0f;
		endPos.y = 0f;
		Vector3 normalized = (endPos - startPos).normalized;
		Vector3 pos3d = startPos + normalized * ((float)size * 0.4f);
		this.AddLocation(pos3d, size);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarManager.Location FindLocation(Vector2 pos, int size)
	{
		AstarManager.Location result = null;
		float num = (float)(size * size) * 0.0400000028f;
		for (int i = 0; i < this.locations.Count; i++)
		{
			AstarManager.Location location = this.locations[i];
			if (location.size >= size)
			{
				float sqrMagnitude = (location.pos - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = location;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGraphs(float deltaTime)
	{
		World world = GameManager.Instance.World;
		this.mergedLocations.Clear();
		List<EntityPlayer> list = world.Players.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			Vector2 pos;
			pos.x = entityPlayer.position.x;
			pos.y = entityPlayer.position.z;
			this.Merge(pos, 76);
		}
		for (int j = 0; j < this.locations.Count; j++)
		{
			AstarManager.Location location = this.locations[j];
			location.duration -= deltaTime;
			if (location.duration <= 0f)
			{
				this.locations.RemoveAt(j);
				j--;
			}
			else
			{
				this.Merge(location.pos, location.size);
			}
		}
		for (int k = 0; k < this.graphList.Count; k++)
		{
			this.graphList[k].IsUsed = false;
		}
		for (int l = 0; l < this.mergedLocations.Count; l++)
		{
			AstarManager.MergedLocations mergedLocations = this.mergedLocations[l];
			AstarVoxelGrid astarVoxelGrid = this.FindClosestGraph(mergedLocations.pos, mergedLocations.size);
			if (astarVoxelGrid == null)
			{
				astarVoxelGrid = this.AddGraph(mergedLocations.size);
				astarVoxelGrid.SetPos(this.LocalPosToGridPos(mergedLocations.pos - this.worldOriginXZ));
			}
			astarVoxelGrid.IsUsed = true;
			this.UpdateGraphPos(astarVoxelGrid, mergedLocations.pos);
		}
		this.UpdateMoveGraph();
		for (int m = 0; m < this.graphList.Count; m++)
		{
			AstarVoxelGrid astarVoxelGrid2 = this.graphList[m];
			if (!astarVoxelGrid2.IsUsed)
			{
				this.MoveGraphRemove(astarVoxelGrid2);
				this.astar.data.RemoveGraph(astarVoxelGrid2);
				this.graphList.RemoveAt(m);
				m--;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge(Vector2 pos, int size)
	{
		bool flag = false;
		for (int i = 0; i < this.mergedLocations.Count; i++)
		{
			AstarManager.MergedLocations mergedLocations = this.mergedLocations[i];
			if (size <= mergedLocations.size && (mergedLocations.pos - pos).sqrMagnitude <= 361f)
			{
				mergedLocations.pos = (mergedLocations.pos + pos) * 0.5f;
				this.mergedLocations[i] = mergedLocations;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AstarManager.MergedLocations item;
			item.pos = pos;
			item.size = size;
			this.mergedLocations.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindMoveIndex(AstarVoxelGrid graph)
	{
		for (int i = 0; i < this.moveList.Count; i++)
		{
			if (this.moveList[i] == graph)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGraphPos(AstarVoxelGrid graph, Vector2 pos)
	{
		if (graph.IsMoving())
		{
			return;
		}
		Vector2 vector = pos - this.worldOriginXZ;
		if (graph.IsFullUpdateNeeded)
		{
			Vector3 pos2 = this.LocalPosToGridPos(vector);
			graph.SetPos(pos2);
			return;
		}
		Vector2 a = vector;
		a.x -= graph.center.x;
		a.y -= graph.center.z;
		if (Vector2.SqrMagnitude(a) > 100f)
		{
			graph.GridMovePendingPos = pos;
			if (this.FindMoveIndex(graph) < 0)
			{
				this.moveList.Insert(this.moveList.Count, graph);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMoveGraph()
	{
		if (this.moveCurrent != null)
		{
			if (this.moveCurrent.IsMoving())
			{
				return;
			}
			this.moveCurrent = null;
		}
		if (this.moveList.Count > 0)
		{
			AstarVoxelGrid astarVoxelGrid = this.moveList[0];
			this.moveList.RemoveAt(0);
			this.MoveGraph(astarVoxelGrid, astarVoxelGrid.GridMovePendingPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveGraphRemove(AstarVoxelGrid graph)
	{
		if (this.moveCurrent == graph)
		{
			this.moveCurrent = null;
		}
		int num = this.FindMoveIndex(graph);
		if (num >= 0)
		{
			this.moveList.RemoveAt(num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveGraph(AstarVoxelGrid graph, Vector2 pos)
	{
		this.moveCurrent = graph;
		Vector2 pos2 = pos - this.worldOriginXZ;
		Vector3 targetPos = this.LocalPosToGridPos(pos2);
		graph.Move(targetPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 LocalPosToGridPos(Vector2 pos)
	{
		Vector3 result;
		result.x = Mathf.Round(pos.x);
		result.z = Mathf.Round(pos.y);
		result.y = -32f - this.worldOrigin.y;
		return result;
	}

	public void OriginChanged()
	{
		this.worldOrigin = Origin.position;
		this.worldOriginXZ.x = this.worldOrigin.x;
		this.worldOriginXZ.y = this.worldOrigin.z;
		for (int i = 0; i < this.graphList.Count; i++)
		{
			this.graphList[i].IsFullUpdateNeeded = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Scan()
	{
		if (!this.astar.isScanning)
		{
			this.astar.Scan(null);
		}
		yield return null;
		yield break;
	}

	public void OnBlockChanged(Vector3i pos, BlockValue bvOld, sbyte densOld, long texOld, BlockValue bvNew)
	{
		Block block = bvNew.Block;
		bool isSlowUpdate = block is BlockDoor;
		if (!block.isMultiBlock)
		{
			this.UpdateBlock(pos, isSlowUpdate);
			return;
		}
		int rotation = (int)bvNew.rotation;
		int length = block.multiBlockPos.Length;
		for (int i = 0; i < length; i++)
		{
			Vector3i vector3i = block.multiBlockPos.Get(i, bvNew.type, rotation);
			vector3i += pos;
			this.UpdateBlock(vector3i, isSlowUpdate);
		}
	}

	public void OnBlockDamaged(Vector3i _blockPos, BlockValue _blockValue, int _damage, int _attackerEntityId)
	{
		this.UpdateBlock(_blockPos, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBlock(Vector3i blockPos, bool isSlowUpdate)
	{
		Vector2i vector2i = new Vector2i(blockPos.x, blockPos.z);
		Vector2i vector2i2 = vector2i;
		vector2i2.x &= -16;
		vector2i2.y &= -16;
		AstarManager.Area area = this.AddAreaBlock(vector2i);
		area.hasBlocks = true;
		area.isSlowUpdate = isSlowUpdate;
		for (int i = 0; i < 4; i++)
		{
			Vector2i vector2i3 = vector2i;
			int num = i * 2;
			vector2i3.x += AstarManager.updateBlockOffsets[num];
			vector2i3.y += AstarManager.updateBlockOffsets[num + 1];
			Vector2i vector2i4 = vector2i3;
			vector2i4.x &= -16;
			vector2i4.y &= -16;
			if (vector2i4.x != vector2i2.x || vector2i4.y != vector2i2.y)
			{
				this.AddAreaBlock(vector2i3);
			}
		}
	}

	public static void AddBoundsToUpdate(Bounds _bounds)
	{
		if (AstarManager.Instance == null)
		{
			return;
		}
		Vector2i pos = new Vector2i(Mathf.FloorToInt(_bounds.min.x), Mathf.FloorToInt(_bounds.min.z));
		AstarManager.Area area = AstarManager.Instance.AddArea(pos, true);
		if (!area.isSlowUpdate)
		{
			area.updateDelay = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarManager.Area AddAreaBlock(Vector2i pos)
	{
		AstarManager.Area area = this.AddArea(pos, false);
		if (!area.isPartial)
		{
			area.isPartial = true;
			area.bounds.min = pos;
			area.bounds.max = pos;
		}
		else
		{
			area.bounds.Encapsulate(pos);
		}
		return area;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarManager.Area AddArea(Vector2i pos, bool noNext)
	{
		pos.x &= -16;
		pos.y &= -16;
		AstarManager.Area area = this.FindArea(pos);
		if (area == null)
		{
			area = new AstarManager.Area();
			area.pos = pos;
			area.updateDelay = 2f;
			this.areaList.Add(area);
			return area;
		}
		if (noNext)
		{
			return area;
		}
		if (area.next != null)
		{
			return area.next;
		}
		if (area.updateDelay < 1.5f)
		{
			AstarManager.Area area2 = new AstarManager.Area();
			area2.pos = pos;
			area2.updateDelay = 2f - area.updateDelay;
			area.next = area2;
			return area2;
		}
		return area;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarManager.Area FindArea(Vector2i pos)
	{
		for (int i = 0; i < this.areaList.Count; i++)
		{
			AstarManager.Area area = this.areaList[i];
			if (area.pos == pos)
			{
				return area;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarVoxelGrid AddGraph(int size)
	{
		AstarVoxelGrid astarVoxelGrid = this.astar.data.AddGraph(typeof(AstarVoxelGrid)) as AstarVoxelGrid;
		this.graphList.Add(astarVoxelGrid);
		astarVoxelGrid.Init();
		astarVoxelGrid.neighbours = NumNeighbours.Four;
		astarVoxelGrid.uniformEdgeCosts = false;
		astarVoxelGrid.inspectorGridMode = InspectorGridMode.Grid;
		astarVoxelGrid.characterHeight = 1.8f;
		astarVoxelGrid.SetDimensions(size, size, 1f);
		astarVoxelGrid.maxClimb = 1.3f;
		astarVoxelGrid.maxSlope = 60f;
		astarVoxelGrid.mergeSpanRange = 0.1f;
		GraphCollision collision = astarVoxelGrid.collision;
		collision.collisionCheck = true;
		collision.type = ColliderType.Capsule;
		collision.diameter = 0.3f;
		collision.height = 1.5f;
		collision.collisionOffset = 0.15f;
		collision.mask = 65536;
		return astarVoxelGrid;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarVoxelGrid FindClosestGraph(Vector2 pos, int size)
	{
		Vector2 vector = pos - this.worldOriginXZ;
		AstarVoxelGrid result = null;
		float num = float.MaxValue;
		for (int i = 0; i < this.graphList.Count; i++)
		{
			AstarVoxelGrid astarVoxelGrid = this.graphList[i];
			if (!astarVoxelGrid.IsUsed && astarVoxelGrid.size.x >= (float)size)
			{
				Vector2 a = vector;
				a.x -= astarVoxelGrid.center.x;
				a.y -= astarVoxelGrid.center.z;
				float num2 = Vector2.SqrMagnitude(a);
				if (num2 < num)
				{
					num = num2;
					result = astarVoxelGrid;
				}
			}
		}
		return result;
	}

	public static AstarManager Instance;

	public const float cGridHeight = 320f;

	public const float cGridY = -32f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cGridXZSize = 76;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMoveDist = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCharHeight = 1.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCharDiameter = 0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLocationFindPer = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLocationDuration = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cPlayerMergeDist = 19f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cPlayerMergeDistSq = 361f;

	public const float cUpdateDeltaTime = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AstarPath astar;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastWorkTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 worldOrigin;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 worldOriginXZ;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<AstarManager.Area> areaList = new List<AstarManager.Area>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<AstarVoxelGrid> graphList = new List<AstarVoxelGrid>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<AstarManager.Location> locations = new List<AstarManager.Location>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<AstarManager.MergedLocations> mergedLocations = new List<AstarManager.MergedLocations>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<AstarVoxelGrid> moveList = new List<AstarVoxelGrid>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AstarVoxelGrid moveCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int[] updateBlockOffsets = new int[]
	{
		-1,
		0,
		1,
		0,
		0,
		-1,
		0,
		1
	};

	public class Area
	{
		public AstarManager.Area next;

		public Vector2i pos;

		public AstarManager.Bounds2i bounds;

		public bool hasBlocks;

		public bool isPartial;

		public bool isSlowUpdate;

		public float updateDelay;
	}

	public struct Bounds2i
	{
		public bool Contains(Vector2i pos)
		{
			return pos.x >= this.min.x && pos.x <= this.max.x && pos.y >= this.min.y && pos.y <= this.max.y;
		}

		public void Encapsulate(Vector2i pos)
		{
			if (pos.x < this.min.x)
			{
				this.min.x = pos.x;
			}
			if (pos.x > this.max.x)
			{
				this.max.x = pos.x;
			}
			if (pos.y < this.min.y)
			{
				this.min.y = pos.y;
			}
			if (pos.y > this.max.y)
			{
				this.max.y = pos.y;
			}
		}

		public Bounds ToBounds()
		{
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3((float)this.min.x, 0f, (float)this.min.y), new Vector3((float)this.max.x + 0.999999f, 0f, (float)this.max.y + 0.999999f));
			return result;
		}

		public Vector2i min;

		public Vector2i max;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Location
	{
		public Vector2 pos;

		public int size;

		public float duration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct MergedLocations
	{
		public Vector2 pos;

		public int size;
	}
}
