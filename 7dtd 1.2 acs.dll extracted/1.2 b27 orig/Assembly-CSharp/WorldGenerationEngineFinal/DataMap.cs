using System;

namespace WorldGenerationEngineFinal
{
	public class DataMap<T>
	{
		public DataMap(int tileWidth, T defaultValue)
		{
			this.data = new T[tileWidth, tileWidth];
			for (int i = 0; i < this.data.GetLength(0); i++)
			{
				for (int j = 0; j < this.data.GetLength(1); j++)
				{
					this.data[i, j] = defaultValue;
				}
			}
		}

		public T[,] data;
	}
}
