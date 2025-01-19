using System;

namespace SharpEXR.AttributeTypes
{
	public struct TileDesc
	{
		public TileDesc(uint xSize, uint ySize, byte mode)
		{
			this.XSize = xSize;
			this.YSize = ySize;
			int roundingMode = (mode & 240) >> 4;
			int levelMode = (int)(mode & 15);
			this.RoundingMode = (RoundingMode)roundingMode;
			this.LevelMode = (LevelMode)levelMode;
		}

		public override string ToString()
		{
			return string.Format("{0}: XSize={1}, YSize={2}", base.GetType().Name, this.XSize, this.YSize);
		}

		public readonly uint XSize;

		public readonly uint YSize;

		public readonly LevelMode LevelMode;

		public readonly RoundingMode RoundingMode;
	}
}
