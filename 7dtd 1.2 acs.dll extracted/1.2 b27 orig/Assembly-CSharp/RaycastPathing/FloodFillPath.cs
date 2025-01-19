using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class FloodFillPath : RaycastPath
	{
		public FloodFillPath(Vector3 start, Vector3 target) : base(start, target)
		{
		}

		public bool IsPosOpen(Vector3 pos)
		{
			return this.open.Find((FloodFillNode n) => n.Position == pos) != null;
		}

		public bool IsPosClosed(Vector3 pos)
		{
			return this.closed.Find((FloodFillNode n) => n.Position == pos) != null;
		}

		public FloodFillNode getLowestScore()
		{
			FloodFillNode result = null;
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			for (int i = 0; i < this.open.Count; i++)
			{
				FloodFillNode floodFillNode = this.open[i];
				if (floodFillNode.F <= num && floodFillNode.Heuristic < num2)
				{
					result = floodFillNode;
					num = floodFillNode.F;
					num2 = floodFillNode.Heuristic;
				}
			}
			return result;
		}

		public override void DebugDraw()
		{
			for (int i = 0; i < this.closed.Count; i++)
			{
				FloodFillNode floodFillNode = this.closed[i];
				if (floodFillNode.nodeType == cPathNodeType.Air)
				{
					RaycastPathUtils.DrawBounds(floodFillNode.BlockPos, Color.yellow, 0f, 1f);
				}
			}
			base.DebugDraw();
		}

		public List<FloodFillNode> open = new List<FloodFillNode>();

		public List<FloodFillNode> closed = new List<FloodFillNode>();
	}
}
