using System;

public readonly struct IntRange
{
	public IntRange(int _min, int _max)
	{
		this.min = _min;
		this.max = _max;
	}

	public bool IsSet()
	{
		return this.min != 0 || this.max != 0;
	}

	public float Random(GameRandom _rnd)
	{
		return (float)_rnd.RandomRange(this.min, this.max);
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			"(",
			this.min.ToString(),
			"-",
			this.max.ToString(),
			")"
		});
	}

	public readonly int min;

	public readonly int max;
}
