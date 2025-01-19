using System;

public class BiomeBlockDecoration
{
	public BiomeBlockDecoration(string _name, float _prob, float _clusprob, BlockValue _blockValue, int _randomRotateMax, int _checkResource = 2147483647)
	{
		this.blockName = _name;
		this.prob = _prob;
		this.clusterProb = _clusprob;
		this.blockValue = _blockValue;
		this.randomRotateMax = _randomRotateMax;
		this.checkResourceOffsetY = _checkResource;
	}

	public static byte GetRandomRotation(float _rnd, int _randomRotateMax)
	{
		byte b = (byte)(_rnd * (float)_randomRotateMax + 0.5f);
		if (b >= 4 && b <= 7)
		{
			b = b - 4 + 24;
		}
		return b;
	}

	public string blockName;

	public float prob;

	public float clusterProb;

	public BlockValue blockValue;

	public int randomRotateMax;

	public int checkResourceOffsetY;
}
