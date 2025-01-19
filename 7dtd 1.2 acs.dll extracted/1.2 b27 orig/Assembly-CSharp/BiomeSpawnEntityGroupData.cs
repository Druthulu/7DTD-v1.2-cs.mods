using System;

public class BiomeSpawnEntityGroupData
{
	public BiomeSpawnEntityGroupData(int _idHash, int _maxCount, int _respawndelay, EDaytime _daytime)
	{
		this.idHash = _idHash;
		this.maxCount = _maxCount;
		this.daytime = _daytime;
		this.respawnDelayInWorldTime = _respawndelay;
	}

	public int idHash;

	public string entityGroupName;

	public int maxCount;

	public int respawnDelayInWorldTime;

	public EDaytime daytime;

	public FastTags<TagGroup.Poi> POITags;

	public FastTags<TagGroup.Poi> noPOITags;
}
