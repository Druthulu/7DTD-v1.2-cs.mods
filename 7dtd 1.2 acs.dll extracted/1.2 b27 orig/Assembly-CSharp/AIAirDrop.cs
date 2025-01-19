using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIAirDrop
{
	public AIAirDrop(AIDirectorAirDropComponent _airDropController, World _world, List<EntityPlayer> _players)
	{
		AIAirDrop.controller = _airDropController;
		this.world = _world;
		this.numPlayers = _players.Count;
		this.MakePlayerClusters(_players);
	}

	public bool Tick(float dt)
	{
		if (this.flightPaths == null)
		{
			this.CreateFlightPaths();
			Log.Out("AIAirDrop: Computed flight paths for " + this.flightPaths.Count.ToString() + " aircraft.");
			Log.Out("AIAirDrop: Waiting for supply crate chunk locations to load...");
		}
		if (!this.spawningCrates)
		{
			bool flag = true;
			for (int i = 0; i < this.flightPaths.Count; i++)
			{
				AIAirDrop.FlightPath flightPath = this.flightPaths[i];
				for (int j = 0; j < flightPath.Crates.Count; j++)
				{
					AIAirDrop.SupplyCrateSpawn supplyCrateSpawn = flightPath.Crates[j];
					if ((Chunk)this.world.GetChunkFromWorldPos(World.worldToBlockPos(supplyCrateSpawn.SpawnPos)) == null)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			this.spawningCrates = flag;
		}
		if (this.spawningCrates)
		{
			int k = 0;
			while (k < this.flightPaths.Count)
			{
				AIAirDrop.FlightPath flightPath2 = this.flightPaths[k];
				flightPath2.Delay -= dt;
				if (flightPath2.Delay <= 0f)
				{
					if (!flightPath2.Spawned)
					{
						this.SpawnPlane(flightPath2);
						flightPath2.Spawned = true;
					}
					int l = 0;
					while (l < flightPath2.Crates.Count)
					{
						AIAirDrop.SupplyCrateSpawn supplyCrateSpawn2 = flightPath2.Crates[l];
						supplyCrateSpawn2.Delay -= dt;
						if (supplyCrateSpawn2.Delay <= 0f)
						{
							EntitySupplyCrate entitySupplyCrate = AIAirDrop.controller.SpawnSupplyCrate(supplyCrateSpawn2.SpawnPos);
							if (entitySupplyCrate != null)
							{
								entitySupplyCrate.ChunkObserver = supplyCrateSpawn2.ChunkRef;
							}
							else if (supplyCrateSpawn2.ChunkRef != null)
							{
								this.world.GetGameManager().RemoveChunkObserver(supplyCrateSpawn2.ChunkRef);
								supplyCrateSpawn2.ChunkRef = null;
							}
							Log.Out("AIAirDrop: Spawned supply crate at " + supplyCrateSpawn2.SpawnPos.ToCultureInvariantString() + ", plane is at " + ((this.eSupplyPlane != null) ? this.eSupplyPlane.position : Vector3.zero).ToString());
							flightPath2.Crates.RemoveAt(l);
						}
						else
						{
							l++;
						}
					}
				}
				if (flightPath2.Crates.Count == 0)
				{
					this.flightPaths.RemoveAt(k);
				}
				else
				{
					k++;
				}
			}
			if (this.flightPaths.Count == 0)
			{
				this.flightPaths = null;
			}
		}
		return this.flightPaths == null;
	}

	public static float Angle(Vector2 p_vector2)
	{
		if (p_vector2.x < 0f)
		{
			return 360f - Mathf.Atan2(p_vector2.x, p_vector2.y) * 57.29578f * -1f;
		}
		return Mathf.Atan2(p_vector2.x, p_vector2.y) * 57.29578f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnPlane(AIAirDrop.FlightPath _fp)
	{
		Vector3 vector = _fp.End - _fp.Start;
		Vector3 normalized = vector.normalized;
		Vector2 vector2 = new Vector2(normalized.x, normalized.z);
		EntitySupplyPlane entitySupplyPlane = (EntitySupplyPlane)EntityFactory.CreateEntity(EntityClass.FromString("supplyPlane"), _fp.Start, new Vector3(0f, AIAirDrop.Angle(vector2), 0f));
		this.eSupplyPlane = entitySupplyPlane;
		entitySupplyPlane.SetDirectionToFly(normalized, (int)(20f * (vector.magnitude / 120f + 10f)));
		this.world.SpawnEntityInWorld(entitySupplyPlane);
		Log.Out(string.Concat(new string[]
		{
			"AIAirDrop: Spawned aircraft at (",
			_fp.Start.ToCultureInvariantString(),
			"), heading (",
			vector2.ToCultureInvariantString(),
			")"
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateFlightPaths()
	{
		this.flightPaths = new List<AIAirDrop.FlightPath>();
		int i;
		int num;
		this.CalcSupplyDropMetrics(this.numPlayers, this.clusters.Count, out i, out num);
		int num2 = Math.Max(1, i / num);
		int num3 = i - num * num2;
		HashSet<int> hashSet = new HashSet<int>();
		GameRandom random = AIAirDrop.controller.Random;
		while (i > 0)
		{
			int num4 = Mathf.Min(i, num2 + num3);
			i -= num4;
			num3 = 0;
			int num5;
			do
			{
				num5 = random.RandomRange(0, this.clusters.Count);
			}
			while (hashSet.Contains(num5));
			AIAirDrop.PlayerCluster playerCluster = this.clusters[num5];
			hashSet.Add(num5);
			if (hashSet.Count == this.clusters.Count)
			{
				hashSet.Clear();
			}
			float num6 = playerCluster.Players[random.RandomRange(0, playerCluster.Players.Count)].position.y + 180f;
			num6 = Utils.FastMin(num6, 276f);
			Vector2 vector = random.RandomOnUnitCircle;
			Vector2 a = playerCluster.XZCenter + vector * random.RandomRange(30f, 750f);
			float num7 = random.RandomRange(150f, 700f);
			float num8 = num7 / 2f;
			float x = vector.x;
			vector.x = -vector.y;
			vector.y = x;
			float num9 = random.RandomRange(1500f, 2000f) / 2f;
			Vector2 vector2 = a + -vector * (num8 + num9);
			Vector2 vector3 = a + vector * (num8 + num9);
			vector2 = this.FindSafePoint(vector2, -vector, 25f, 600f);
			vector3 = this.FindSafePoint(vector3, vector, 25f, 600f);
			float num10 = num7 / (float)num4;
			float num11 = -num10 * Math.Max(1f, ((float)num4 - 1f) / 2f);
			AIAirDrop.FlightPath flightPath = new AIAirDrop.FlightPath();
			flightPath.Start = new Vector3(vector2.x, num6, vector2.y);
			flightPath.End = new Vector3(vector3.x, num6, vector3.y);
			float magnitude = (flightPath.End - flightPath.Start).magnitude;
			for (int j = 0; j < num4; j++)
			{
				Vector2 vector4 = a + (num11 + (float)j * num10) * vector;
				AIAirDrop.SupplyCrateSpawn supplyCrateSpawn = new AIAirDrop.SupplyCrateSpawn();
				float num12 = num6 - 10f;
				if (GameManager.Instance != null && GameManager.Instance.World != null)
				{
					float num13 = (float)GameManager.Instance.World.GetHeight((int)vector4.x, (int)vector4.y);
					if (num12 <= num13 + 15f)
					{
						num12 = num13 + 15f;
					}
				}
				supplyCrateSpawn.SpawnPos = this.ClampToMapExtents(new Vector3(vector4.x, num12, vector4.y), vector, 25f);
				if (j == 0)
				{
					vector = new Vector2(supplyCrateSpawn.SpawnPos.x, supplyCrateSpawn.SpawnPos.z) - new Vector2(flightPath.Start.x, flightPath.Start.z);
					vector.Normalize();
					flightPath.End = flightPath.Start + new Vector3(vector.x, 0f, vector.y) * magnitude;
				}
				supplyCrateSpawn.Delay = (vector2 - vector4).magnitude / 120f;
				supplyCrateSpawn.ChunkRef = this.world.GetGameManager().AddChunkObserver(supplyCrateSpawn.SpawnPos, false, 3, -1);
				flightPath.Crates.Add(supplyCrateSpawn);
			}
			flightPath.Delay = playerCluster.Delay + random.RandomRange(0f, 15f);
			playerCluster.Delay += random.RandomRange(25f, 120f);
			this.flightPaths.Add(flightPath);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 FindSafePoint(Vector2 _point, Vector2 _dir, float _stepSize, float _range)
	{
		_range *= _range;
		for (;;)
		{
			bool flag = true;
			for (int i = 0; i < this.clusters.Count; i++)
			{
				AIAirDrop.PlayerCluster playerCluster = this.clusters[i];
				for (int j = 0; j < playerCluster.Players.Count; j++)
				{
					EntityPlayer entityPlayer = playerCluster.Players[j];
					if ((_point - new Vector2(entityPlayer.position.x, entityPlayer.position.z)).sqrMagnitude < _range)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
			_point += _dir * _stepSize;
		}
		return _point;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakePlayerClusters(List<EntityPlayer> _players)
	{
		this.clusters = new List<AIAirDrop.PlayerCluster>();
		foreach (EntityPlayer entityPlayer in _players)
		{
			bool flag = true;
			for (int i = 0; i < this.clusters.Count; i++)
			{
				AIAirDrop.PlayerCluster cluster = this.clusters[i];
				if (this.TryAddPlayerToCluster(entityPlayer, cluster))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				AIAirDrop.PlayerCluster playerCluster = new AIAirDrop.PlayerCluster();
				playerCluster.Radius = 30f;
				playerCluster.XZCenter = new Vector2(entityPlayer.position.x, entityPlayer.position.z);
				playerCluster.Players.Add(entityPlayer);
				this.clusters.Add(playerCluster);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryAddPlayerToCluster(EntityPlayer _player, AIAirDrop.PlayerCluster _cluster)
	{
		Vector2 vector = _cluster.XZCenter + new Vector2(_player.position.x, _player.position.z);
		vector.Scale(new Vector2(0.5f, 0.5f));
		float num = this.GetPlayerDistanceSq(_player, vector);
		if (num > 70f)
		{
			return false;
		}
		for (int i = 0; i < _cluster.Players.Count; i++)
		{
			EntityPlayer player = _cluster.Players[i];
			num = Mathf.Max(num, this.GetPlayerDistanceSq(player, vector));
			if (num > 70f)
			{
				return false;
			}
		}
		_cluster.XZCenter = vector;
		_cluster.Radius = Mathf.Max(num, 30f);
		_cluster.Players.Add(_player);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetPlayerDistanceSq(EntityPlayer _player, Vector2 _xzPos)
	{
		return (_xzPos - new Vector2(_player.position.x, _player.position.z)).sqrMagnitude;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SelectCrateCount(int _numPlayers, out int _min, out int _max)
	{
		if (_numPlayers < 6)
		{
			_min = 1;
			_max = 1;
			return;
		}
		if (_numPlayers < 11)
		{
			_min = 2;
			_max = 2;
			return;
		}
		_min = 2;
		_max = 3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcSupplyDropMetrics(int _numPlayers, int _numClusters, out int _numCrates, out int _numPlanes)
	{
		int min;
		int num;
		AIAirDrop.SelectCrateCount(_numPlayers, out min, out num);
		_numCrates = AIAirDrop.controller.Random.RandomRange(min, num + 1);
		_numPlanes = Math.Max(1, Math.Min(_numClusters + AIAirDrop.controller.Random.RandomRange(0, 2), 4));
		if (_numCrates / _numPlanes < 1 || _numCrates / _numPlanes > 3)
		{
			_numPlanes = Math.Min(1, Mathf.CeilToInt((float)_numCrates / 3f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ClampToMapExtents(Vector3 _pos, Vector2 _dir, float _step)
	{
		return this.world.ClampToValidWorldPos(_pos);
	}

	public static void RemoveSupplyCrate(int id)
	{
		if (AIAirDrop.controller == null)
		{
			return;
		}
		AIAirDrop.controller.RemoveSupplyCrate(id);
	}

	public const float cPlaneMetersPerSecond = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxPlayerClusterRadius = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinPlayerClusterRadius = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxPlaneTangentPointRadius = 750f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinPlaneTangentPointRadius = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinPlaneFlightVector = 1500f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxPlaneFlightVector = 2000f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinDropRange = 150f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxDropRange = 700f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kMaxDropsPerPlane = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kSpawnYUp = 180;

	public static AIDirectorAirDropComponent controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIAirDrop.PlayerCluster> clusters;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIAirDrop.FlightPath> flightPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawningCrates;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity eSupplyPlane;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class SupplyCrateSpawn
	{
		public float Delay;

		public Vector3 SpawnPos;

		public ChunkManager.ChunkObserver ChunkRef;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class PlayerCluster
	{
		public Vector2 XZCenter;

		public float Radius;

		public List<EntityPlayer> Players = new List<EntityPlayer>();

		public float Delay;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class FlightPath
	{
		public List<AIAirDrop.SupplyCrateSpawn> Crates = new List<AIAirDrop.SupplyCrateSpawn>();

		public Vector3 Start;

		public Vector3 End;

		public float Delay;

		public bool Spawned;
	}
}
