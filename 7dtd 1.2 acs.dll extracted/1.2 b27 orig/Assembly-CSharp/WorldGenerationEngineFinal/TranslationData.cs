using System;

namespace WorldGenerationEngineFinal
{
	public class TranslationData
	{
		public TranslationData(int _x, int _y, float _randomScaleMin = 0.5f, float _randomScaleMax = 1.5f, int _rotation = -1)
		{
			this.x = _x;
			this.y = _y;
			this.scale = Rand.Instance.Range(_randomScaleMin, _randomScaleMax);
			this.rotation = _rotation;
			if (_rotation < 0)
			{
				this.rotation = Rand.Instance.Range(0, 360);
			}
		}

		public TranslationData(int _x, int _y, float _scale, int _rotation)
		{
			this.x = _x;
			this.y = _y;
			this.scale = _scale;
			this.rotation = _rotation;
		}

		public int x;

		public int y;

		public float scale;

		public int rotation;
	}
}
