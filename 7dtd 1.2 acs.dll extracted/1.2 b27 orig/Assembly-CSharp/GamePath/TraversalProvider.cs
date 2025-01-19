using System;
using Pathfinding;

namespace GamePath
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class TraversalProvider : ITraversalProvider
	{
		public bool CanTraverse(Path path, GraphNode node)
		{
			return node.Walkable && (path.enabledTags >> (int)node.Tag & 1) != 0;
		}

		public uint GetTraversalCost(Path path, GraphNode node)
		{
			return (uint)(node.Penalty * path.CostScale);
		}
	}
}
