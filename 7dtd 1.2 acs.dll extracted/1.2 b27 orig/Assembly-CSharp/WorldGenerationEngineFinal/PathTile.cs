using System;

namespace WorldGenerationEngineFinal
{
	public class PathTile
	{
		public PathTile.PathTileStates TileState;

		public byte PathRadius;

		public Path Path;

		public enum PathTileStates : byte
		{
			Free,
			Blocked,
			Highway,
			Country
		}
	}
}
