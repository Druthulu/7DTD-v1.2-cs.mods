using System;
using UnityEngine;

namespace GamePath
{
	public class PathInfo
	{
		public PathInfo(EntityAlive _entity, Vector3 _targetPos, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
		{
			this.entity = _entity;
			this.hasStart = false;
			this.targetPos = _targetPos;
			this.canBreakBlocks = _canBreakBlocks;
			this.speed = _speed;
			this.aiTask = _aiTask;
			this.path = null;
		}

		public void SetStartPos(Vector3 _startPos)
		{
			this.startPos = _startPos;
			this.hasStart = true;
		}

		public static PathInfo Empty = new PathInfo(null, Vector3.zero, false, 0f, null);

		public EntityAlive entity;

		public PathInfo.State state;

		public bool hasStart;

		public Vector3 startPos;

		public Vector3 targetPos;

		public bool canBreakBlocks;

		public float speed;

		public EAIBase aiTask;

		public ChunkCache chunkcache;

		public PathEntity path;

		public enum State
		{
			Queued,
			Pathing,
			Done
		}
	}
}
