﻿using System;
using System.Collections;
using System.Collections.Generic;
using RaycastPathing;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class FloodFillEntityPathGenerator : RaycastEntityPathGenerator
{
	public FloodFillEntityPathGenerator(World _world, EntityAlive _entity) : base(_world, _entity)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitPath(Vector3 start, Vector3 end)
	{
		this.Path = new FloodFillPath(start, end);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator BuildPathProc()
	{
		FloodFillPath path = this.Path as FloodFillPath;
		FloodFillNode item = new FloodFillNode(path.Start, 1f, 0);
		path.open.Add(item);
		while (path.open.Count > 0)
		{
			FloodFillNode lowestScore = path.getLowestScore();
			path.open.Remove(lowestScore);
			path.closed.Add(lowestScore);
			if (lowestScore.BlockPos == this.Path.TargetBlockPos)
			{
				break;
			}
			List<FloodFillNode> currentNeighbors = this.AddNeighborNodes(base.GameWorld, path, lowestScore);
			this.ProcNeighborNodes(currentNeighbors, lowestScore);
			FloodFillNode floodFillNode = this.IsNeighborEnd(currentNeighbors);
			if (floodFillNode != null)
			{
				path.closed.Add(floodFillNode);
				break;
			}
			if (path.closed.Count > 1536)
			{
				Log.Warning("Search Exausted.");
				break;
			}
			yield return new WaitForSeconds(this.debugTick);
		}
		RaycastNode raycastNode = path.closed[path.closed.Count - 1];
		while (raycastNode.Parent != null)
		{
			this.Path.AddNode(raycastNode);
			raycastNode = raycastNode.Parent;
		}
		yield return this.<>n__0();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidNeighbor(World world, FloodFillPath path, FloodFillNode current, FloodFillNode adjacent)
	{
		return (adjacent.nodeType != cPathNodeType.Solid || adjacent.BlockPos == path.TargetBlockPos) && (adjacent.nodeType != cPathNodeType.Door || !RaycastPathUtils.IsPositionBlocked(current.Center, adjacent.Center, 65536, false) || BlockDoor.IsDoorOpen(world.GetBlock(adjacent.BlockPos).meta));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<FloodFillNode> AddNeighborNodes(World world, FloodFillPath path, FloodFillNode current)
	{
		List<FloodFillNode> list = RaycastPathWorldUtils.ScanNeighborNodes(base.GameWorld, path, current, false);
		for (int i = 0; i < list.Count; i++)
		{
			FloodFillNode floodFillNode = list[i];
			if (this.IsValidNeighbor(world, path, current, floodFillNode) && !path.IsPosOpen(floodFillNode.Position) && !path.IsPosClosed(floodFillNode.Position))
			{
				path.open.Add(floodFillNode);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FloodFillNode IsNeighborEnd(List<FloodFillNode> currentNeighbors)
	{
		for (int i = 0; i < currentNeighbors.Count; i++)
		{
			FloodFillNode floodFillNode = currentNeighbors[i];
			if (floodFillNode.BlockPos == this.Path.TargetBlockPos)
			{
				return floodFillNode;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FloodFillNode ProcNeighborNodes(List<FloodFillNode> currentNeighbors, FloodFillNode current)
	{
		List<FloodFillNode> list = new List<FloodFillNode>();
		for (int i = 0; i < currentNeighbors.Count; i++)
		{
			FloodFillNode floodFillNode = currentNeighbors[i];
			if (floodFillNode.ChildAirBlocks.Count > 0)
			{
				for (int j = 0; j < floodFillNode.ChildAirBlocks.Count; j++)
				{
					FloodFillNode item = floodFillNode.Children[j] as FloodFillNode;
					list.Add(item);
				}
			}
		}
		List<FloodFillNode> list2 = new List<FloodFillNode>();
		for (int k = 0; k < current.ChildAirBlocks.Count; k++)
		{
			FloodFillNode floodFillNode2 = current.ChildAirBlocks[k] as FloodFillNode;
			list2.Add(floodFillNode2);
			for (int l = 0; l < list.Count; l++)
			{
				FloodFillNode floodFillNode3 = list[l];
				if (floodFillNode3 != null && !RaycastPathUtils.IsPointBlocked(floodFillNode2.Position, floodFillNode3.Position, 65536, true, this.debugTick))
				{
					list2.Add(floodFillNode3);
				}
			}
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		for (int m = 0; m < list2.Count - 1; m++)
		{
			FloodFillNode floodFillNode4 = list2[m];
			if (floodFillNode4 != null)
			{
				FloodFillNode floodFillNode5 = list2[m + 1];
				if (m < 1)
				{
					vector = Vector3.Min(floodFillNode4.Min, floodFillNode5.Min);
					vector2 = Vector3.Max(floodFillNode4.Max, floodFillNode5.Max);
				}
				else
				{
					vector = Vector3.Min(vector, floodFillNode5.Min);
					vector2 = Vector3.Max(vector2, floodFillNode5.Max);
				}
			}
		}
		FloodFillNode floodFillNode6 = new FloodFillNode(vector, vector2, 1f, 0);
		RaycastPathUtils.DrawNode(floodFillNode6, Color.green, 5f);
		current.SetWaypoint(floodFillNode6);
		return floodFillNode6;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float debugTick;
}
