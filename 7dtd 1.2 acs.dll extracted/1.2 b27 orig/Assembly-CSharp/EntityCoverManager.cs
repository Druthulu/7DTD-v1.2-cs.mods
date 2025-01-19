using System;
using System.Collections.Generic;
using ExtUtilsForEnt;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityCoverManager
{
	public static EntityCoverManager Instance
	{
		get
		{
			return EntityCoverManager.instance;
		}
	}

	public static void Init()
	{
		EntityCoverManager.instance = new EntityCoverManager();
		EntityCoverManager.instance.Load();
	}

	public void Clear()
	{
		this.CoverDic.Clear();
		this.CoverPoints.Clear();
	}

	public void Clear(EntityAlive entity, float dist)
	{
		foreach (KeyValuePair<int, EntityCoverManager.CoverPos> keyValuePair in this.CoverDic)
		{
			if (Vector3.Distance(keyValuePair.Value.BlockPos, entity.position) > dist)
			{
				this.CoverDic.Remove(keyValuePair.Key);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load()
	{
	}

	public void Update()
	{
		this.DrawCoverPoints();
	}

	public bool HasCover(int entityId)
	{
		EntityCoverManager.CoverPos coverPos = null;
		return this.CoverDic.TryGetValue(entityId, out coverPos) && coverPos.InUse;
	}

	public bool HasCoverReserved(int entityId)
	{
		EntityCoverManager.CoverPos coverPos = null;
		return this.CoverDic.TryGetValue(entityId, out coverPos) && (coverPos.Reserved || coverPos.InUse);
	}

	public bool IsFree(Vector3 coverPos)
	{
		foreach (KeyValuePair<int, EntityCoverManager.CoverPos> keyValuePair in this.CoverDic)
		{
			if (keyValuePair.Value.BlockPos == coverPos)
			{
				return false;
			}
		}
		return true;
	}

	public EntityCoverManager.CoverPos AddCover(Vector3 pos, Vector3 dir)
	{
		if (this.CoverPoints.Find((EntityCoverManager.CoverPos c) => c.BlockPos == pos) == null)
		{
			EntityCoverManager.CoverPos coverPos = new EntityCoverManager.CoverPos(pos, dir, Time.time);
			this.CoverPoints.Add(coverPos);
			return coverPos;
		}
		return null;
	}

	public EntityCoverManager.CoverPos GetCoverPos(int entityId)
	{
		EntityCoverManager.CoverPos result = null;
		this.CoverDic.TryGetValue(entityId, out result);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager.CoverPos GetCover(Vector3 pos)
	{
		return this.CoverPoints.Find((EntityCoverManager.CoverPos c) => c.BlockPos == pos);
	}

	public bool MarkReserved(int entityId, Vector3 pos)
	{
		if (!this.CoverDic.ContainsKey(entityId))
		{
			EntityCoverManager.CoverPos cover = this.GetCover(pos);
			if (cover != null)
			{
				cover.Reserved = true;
				this.CoverDic.Add(entityId, cover);
				return true;
			}
		}
		return false;
	}

	public bool UseCover(int entityId, Vector3 pos)
	{
		EntityCoverManager.CoverPos cover = this.GetCover(pos);
		if (!this.CoverDic.ContainsKey(entityId))
		{
			if (cover != null)
			{
				cover.InUse = true;
				this.CoverDic.Add(entityId, cover);
				return true;
			}
		}
		else if (this.CoverDic.TryGetValue(entityId, out cover))
		{
			cover.InUse = true;
			return true;
		}
		return false;
	}

	public void FreeCover(int entityId)
	{
		EntityCoverManager.CoverPos coverPos = null;
		if (this.CoverDic.TryGetValue(entityId, out coverPos))
		{
			this.CoverDic.Remove(entityId);
		}
	}

	public void DrawCoverPoints()
	{
		for (int i = 0; i < this.CoverPoints.Count; i++)
		{
			EntityCoverManager.CoverPos coverPos = this.CoverPoints[i];
			EUtils.DrawBounds(new Vector3i(coverPos.BlockPos), Color.yellow, 1f, 1f);
			EUtils.DrawLine(coverPos.BlockPos, coverPos.BlockPos + coverPos.CoverDir, Color.blue, 1f);
		}
	}

	public static bool DebugModeEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityCoverManager instance;

	public Dictionary<int, EntityCoverManager.CoverPos> CoverDic = new Dictionary<int, EntityCoverManager.CoverPos>();

	public List<EntityCoverManager.CoverPos> CoverPoints = new List<EntityCoverManager.CoverPos>();

	[Preserve]
	public class CoverPos
	{
		public CoverPos(Vector3 _pos, Vector3 _coverDir, float _timeCreated)
		{
			this.BlockPos = _pos;
			this.CoverDir = _coverDir;
			this.TimeCreated = _timeCreated;
		}

		public Vector3 BlockPos;

		public Vector3 CoverDir;

		public float TimeCreated;

		public bool Reserved;

		public bool InUse;
	}
}
