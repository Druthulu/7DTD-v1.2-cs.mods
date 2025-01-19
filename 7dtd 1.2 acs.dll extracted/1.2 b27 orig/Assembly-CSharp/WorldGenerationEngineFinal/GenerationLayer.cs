using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class GenerationLayer
	{
		public GenerationLayer(int _x, int _y, int _range)
		{
			this.x = _x;
			this.y = _y;
			this.Range = _range;
			this.children = new List<TranslationData>();
		}

		public int x;

		public int y;

		public int Range;

		public List<TranslationData> children;
	}
}
