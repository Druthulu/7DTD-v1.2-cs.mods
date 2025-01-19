using System;
using Pathfinding;

namespace GamePath
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class TraversalProviderNoBreak : ITraversalProvider
	{
		public bool CanTraverse(Path path, GraphNode node)
		{
			return node.Walkable && (path.enabledTags >> (int)node.Tag & 1) != 0 && node.Penalty < 1000U;
		}

		public uint GetTraversalCost(Path path, GraphNode node)
		{
			return node.Penalty;
		}
	}
}
