using System;
using UnityEngine;

public class RandomPositionGenerator
{
	public static Vector3 Calc(EntityAlive _entity, int _maxXZ, int _maxY)
	{
		Vector3 result;
		if (!RandomPositionGenerator.calc(_entity, _maxXZ, _maxY, false, out result) && _entity.isSwimming)
		{
			RandomPositionGenerator.calc(_entity, _maxXZ, _maxY, true, out result);
		}
		return result;
	}

	public static Vector3 CalcTowards(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _position)
	{
		Vector3 dirV = _position - _entity.position;
		return RandomPositionGenerator.CalcInDir(_entity, _minXZ, _maxXZ, _maxY, dirV);
	}

	public static Vector3 CalcAway(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _position)
	{
		Vector3 dirV = _entity.position - _position;
		return RandomPositionGenerator.CalcInDir(_entity, _minXZ, _maxXZ, _maxY, dirV);
	}

	public static Vector3 CalcInDir(EntityAlive _entity, int _minXZ, int _maxXZ, int _maxY, Vector3 _dirV)
	{
		Vector3 result;
		if (!RandomPositionGenerator.calcDir(_entity, _minXZ, _maxXZ, _maxY, _dirV, false, out result) && _entity.isSwimming)
		{
			RandomPositionGenerator.calcDir(_entity, _minXZ, _maxXZ, _maxY, _dirV, true, out result);
		}
		return result;
	}

	public static Vector3 CalcNear(EntityAlive _entity, Vector3 target, int _xzDist, int _yDist)
	{
		GameRandom rand = _entity.rand;
		int num = rand.RandomRange(2 * _xzDist) - _xzDist;
		int num2 = rand.RandomRange(2 * _yDist) - _yDist;
		int num3 = rand.RandomRange(2 * _xzDist) - _xzDist;
		num += Utils.Fastfloor(target.x);
		num2 += Utils.Fastfloor(target.y);
		num3 += Utils.Fastfloor(target.z);
		return new Vector3((float)num, (float)num2, (float)num3);
	}

	public static Vector3 CalcPositionInDirection(Entity _entity, Vector3 _startPos, Vector3 _dirV, float _dist, float _randomAngle)
	{
		World world = _entity.world;
		_dirV.y = 0f;
		Vector3 vector = _dirV.normalized;
		Quaternion q = Quaternion.Euler(0f, _randomAngle * (_entity.rand.RandomFloat - 0.5f), 0f);
		vector = Matrix4x4.TRS(Vector3.zero, q, Vector3.one).MultiplyVector(vector);
		while (_dist > 0f && (Chunk)world.GetChunkFromWorldPos((int)(_startPos.x + vector.x * _dist), 0, (int)(_startPos.z + vector.z * _dist)) == null)
		{
			_dist -= 4f;
		}
		if (_dist < 1f)
		{
			return Vector3.zero;
		}
		Vector3 vector2 = _startPos + vector * _dist;
		Vector3i vector3i = World.worldToBlockPos(vector2);
		BlockValue block = world.GetBlock(vector3i);
		if (block.Block.IsMovementBlocked(world, vector3i, block, BlockFaceFlag.None))
		{
			while (vector3i.y < 255)
			{
				vector3i.y++;
				vector2.y = (float)vector3i.y;
				block = world.GetBlock(vector3i);
				if (!block.Block.IsMovementBlocked(world, vector3i, block, BlockFaceFlag.None))
				{
					break;
				}
			}
		}
		else
		{
			while (vector3i.y > 0)
			{
				vector3i.y--;
				block = world.GetBlock(vector3i);
				if (block.Block.IsMovementBlocked(world, vector3i, block, BlockFaceFlag.None))
				{
					break;
				}
				vector2.y = (float)vector3i.y;
			}
		}
		return vector2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool calc(EntityAlive _entity, int _xzDist, int _yDist, bool canSwim, out Vector3 destPos)
	{
		GameRandom rand = _entity.rand;
		World world = _entity.world;
		ChunkCluster chunkCache = world.ChunkCache;
		Vector3 worldPos = _entity.position;
		if (_entity.IsSleeper)
		{
			worldPos = _entity.SleeperSpawnPosition;
		}
		Vector3i vector3i = World.worldToBlockPos(worldPos);
		bool flag = false;
		if (_entity.hasHome())
		{
			flag = (_entity.getHomePosition().getDistance(vector3i.x, vector3i.y, vector3i.z) + 4f < (float)(_entity.getMaximumHomeDistance() + _xzDist));
		}
		for (int i = 0; i < 30; i++)
		{
			Vector3i vector3i2;
			vector3i2.x = rand.RandomRange(2 * _xzDist) - _xzDist;
			vector3i2.z = rand.RandomRange(2 * _xzDist) - _xzDist;
			vector3i2.y = rand.RandomRange(2 * _yDist) - _yDist;
			vector3i2.x += vector3i.x;
			vector3i2.y += vector3i.y;
			vector3i2.z += vector3i.z;
			if (chunkCache.GetBlock(vector3i2).isair && (canSwim || !world.IsWater(vector3i2)) && (!flag || _entity.isWithinHomeDistance(vector3i2.x, vector3i2.y, vector3i2.z)) && vector3i2.y >= 0)
			{
				if (!canSwim)
				{
					bool flag2 = false;
					Vector3i vector3i3 = vector3i2;
					for (int j = 0; j < 10; j++)
					{
						vector3i3.y--;
						BlockValue block = chunkCache.GetBlock(vector3i3);
						if (world.IsWater(vector3i3))
						{
							flag2 = true;
							break;
						}
						if (block.Block.IsMovementBlocked(world, vector3i3, block, BlockFaceFlag.None))
						{
							break;
						}
					}
					if (flag2)
					{
						goto IL_1C6;
					}
				}
				destPos = new Vector3((float)vector3i2.x + 0.5f, (float)vector3i2.y, (float)vector3i2.z + 0.5f);
				return true;
			}
			IL_1C6:;
		}
		destPos = Vector3.zero;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool calcDir(EntityAlive _entity, int _distMinXZ, int _distMaxXZ, int _distMaxY, Vector3 _directionVec, bool canSwim, out Vector3 destPos)
	{
		if (_directionVec == Vector3.zero)
		{
			return RandomPositionGenerator.calc(_entity, _distMaxXZ, _distMaxY, canSwim, out destPos);
		}
		GameRandom rand = _entity.rand;
		ChunkCluster chunkCache = _entity.world.ChunkCache;
		Vector3i vector3i = World.worldToBlockPos(_entity.position);
		if (_distMaxXZ < _distMinXZ)
		{
			_distMaxXZ = _distMinXZ;
		}
		bool flag = false;
		if (_entity.hasHome())
		{
			float num = _entity.getHomePosition().getDistance(vector3i.x, vector3i.y, vector3i.z) + 1f;
			if ((float)_distMinXZ > num)
			{
				_distMinXZ = (int)num;
			}
			if ((float)_distMaxXZ > num)
			{
				_distMaxXZ = (int)num;
			}
			flag = ((float)(_entity.getMaximumHomeDistance() + _distMaxXZ) - num >= 2f);
		}
		int maxExclusive = _distMaxXZ - _distMinXZ;
		Vector2 vector;
		vector.x = _directionVec.x;
		vector.y = _directionVec.z;
		vector.Normalize();
		for (int i = 0; i < 30; i++)
		{
			float f = (rand.RandomFloat * 80f - 40f) * 0.0174532924f;
			float num2 = (float)(_distMinXZ + rand.RandomRange(maxExclusive));
			float num3 = Mathf.Sin(f);
			float num4 = Mathf.Cos(f);
			Vector2 vector2;
			vector2.x = vector.x * num4 - vector.y * num3;
			vector2.y = vector.x * num3 + vector.y * num4;
			vector2.x *= num2;
			vector2.y *= num2;
			Vector3i vector3i2;
			vector3i2.x = Utils.Fastfloor(vector2.x);
			vector3i2.z = Utils.Fastfloor(vector2.y);
			vector3i2.y = rand.RandomRange(2 * _distMaxY) - _distMaxY;
			vector3i2.x += vector3i.x;
			vector3i2.y += vector3i.y;
			vector3i2.z += vector3i.z;
			if (chunkCache.GetBlock(vector3i2).isair && (canSwim || !_entity.world.IsWater(vector3i2)) && (!flag || _entity.isWithinHomeDistance(vector3i2.x, vector3i2.y, vector3i2.z)))
			{
				destPos = new Vector3((float)vector3i2.x + 0.5f, (float)vector3i2.y, (float)vector3i2.z + 0.5f);
				return true;
			}
		}
		destPos = Vector3.zero;
		return false;
	}
}
