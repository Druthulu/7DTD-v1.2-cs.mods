using System;
using Pathfinding;
using UnityEngine;

namespace GamePath
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class ASPPathFinder : PathFinder
	{
		public ASPPathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls) : base(_pathInfo, _bDrn, _canClimbLadders, _bCanClimbWalls)
		{
			this.entity = _pathInfo.entity;
			this.smoothPercent = 0.82f + this.entity.rand.RandomFloat * 0.07f;
		}

		public override void Calculate(Vector3 _fromPos, Vector3 _toPos)
		{
			if (AstarPath.active == null)
			{
				return;
			}
			Vector3 start = _fromPos - Origin.position;
			this.moveToPos = _toPos;
			this.moveToLocalPos = _toPos - Origin.position;
			this.path = ABPath.Construct(start, this.moveToLocalPos, new OnPathDelegate(this.OnPathFinished));
			this.path.calculatePartial = true;
			this.path.enabledTags = ((!this.pathInfo.canBreakBlocks) ? 257 : 267);
			if (this.pathInfo.entity.bCanClimbLadders)
			{
				this.path.enabledTags |= 16;
			}
			if (this.pathInfo.entity.height <= 1f)
			{
				this.path.enabledTags |= 4;
			}
			if (this.entity is EntityDrone && !this.pathInfo.canBreakBlocks)
			{
				this.path.enabledTags |= 8;
			}
			float pathCostScale = this.entity.aiManager.pathCostScale;
			if (pathCostScale <= 99f)
			{
				float num = this.entity.aiManager.partialPathHeightScale;
				this.path.traversalProvider = new TraversalProvider();
				this.path.CostScale = pathCostScale;
				this.path.PartialPathHeightScale = num * 0.3f;
				if (pathCostScale >= 0.28f)
				{
					num -= this.entity.rand.RandomFloat * 0.02f * pathCostScale;
					if (num < 0f)
					{
						num = 0f;
					}
					this.entity.aiManager.partialPathHeightScale = num;
				}
			}
			else
			{
				this.path.traversalProvider = new TraversalProviderNoBreak();
				this.path.CostScale = 1f;
				this.path.PartialPathHeightScale = 0f;
			}
			AstarPath.StartPath(this.path, false);
			this.pathInfo.state = PathInfo.State.Pathing;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPathFinished(Path p)
		{
			this.pathInfo.state = PathInfo.State.Done;
			EntityAlive entityAlive = this.pathInfo.entity;
			if (!entityAlive || entityAlive.navigator == null)
			{
				return;
			}
			if (this.pathInfo != entityAlive.navigator.pathInfo)
			{
				return;
			}
			this.path = (p as ABPath);
			int num = this.path.vectorPath.Count;
			if (num == 0)
			{
				return;
			}
			this.pathInfo.path = new PathEntity();
			this.pathInfo.path.toPos = this.moveToPos;
			for (int i = 0; i < num - 2; i++)
			{
				Vector3 vector = this.path.vectorPath[i];
				Vector3 vector2 = this.path.vectorPath[i + 2];
				float num2 = vector2.x - vector.x;
				float num3 = vector2.z - vector.z;
				if ((num2 < -0.1f || num2 > 0.1f) && (num3 < -0.1f || num3 > 0.1f))
				{
					Vector3 vector3 = this.path.vectorPath[i + 1];
					if (Mathf.Abs(vector.y - vector3.y) <= 0.5f && this.IsLineClear(vector, vector2, true))
					{
						this.path.vectorPath[i + 1] = (vector + vector2) * 0.475f + vector3 * 0.05f;
						i++;
					}
				}
			}
			Vector3 a = entityAlive.position - Origin.position;
			this.pathInfo.path.rawEndPos = this.path.vectorPath[num - 1] + Origin.position;
			if (num >= 2)
			{
				this.path.vectorPath[0] = a * 0.45f + this.path.vectorPath[0] * 0.55f;
			}
			if (this.path.CompleteState == PathCompleteState.Complete)
			{
				if (!this.pathInfo.canBreakBlocks)
				{
					this.moveToLocalPos.y = this.path.vectorPath[num - 1].y;
				}
				this.path.vectorPath[num - 1] = this.moveToLocalPos;
			}
			else if (this.path.CompleteState == PathCompleteState.Partial && num == 1)
			{
				this.path.vectorPath[0] = a * 0.3f + this.path.vectorPath[0] * 0.7f;
				Vector3 item = Vector3.MoveTowards(this.path.vectorPath[0], this.moveToLocalPos, 5f);
				this.path.vectorPath.Add(item);
				num++;
			}
			if (num >= 3)
			{
				float num4 = this.smoothPercent;
				float num5 = (1f - num4) * 0.5f;
				for (int j = 2; j > 0; j--)
				{
					for (int k = 0; k < num - 2; k++)
					{
						Vector3 vector4 = this.path.vectorPath[k];
						Vector3 vector5 = this.path.vectorPath[k + 1];
						if (Mathf.Abs(vector4.y - vector5.y) <= 0.5f)
						{
							Vector3 vector6 = this.path.vectorPath[k + 2];
							vector5.x = vector5.x * num4 + (vector4.x + vector6.x) * num5;
							vector5.z = vector5.z * num4 + (vector4.z + vector6.z) * num5;
							this.path.vectorPath[k + 1] = vector5;
						}
					}
				}
			}
			PathPoint[] array = new PathPoint[num];
			for (int l = 0; l < num; l++)
			{
				PathPoint pathPoint = PathPoint.Allocate(this.path.vectorPath[l] + Origin.position);
				array[l] = pathPoint;
			}
			this.pathInfo.path.SetPoints(array);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsLineClear(Vector3 pos1, Vector3 pos2, bool isTall)
		{
			if (Mathf.Abs(pos1.y - pos2.y) > 0.5f)
			{
				return false;
			}
			pos1.y += 0.5f;
			pos2.y += 0.5f;
			if (Physics.Linecast(pos1, pos2, 1082195968))
			{
				return false;
			}
			Vector3 direction = pos2 - pos1;
			Ray ray = new Ray(pos1, direction);
			if (Physics.SphereCast(ray, 0.25f, direction.magnitude, 1082195968))
			{
				return false;
			}
			if (isTall)
			{
				pos1.y += 1f;
				pos2.y += 1f;
				if (Physics.Linecast(pos1, pos2, 1082195968))
				{
					return false;
				}
				ray.origin = pos1;
				if (Physics.SphereCast(ray, 0.25f, direction.magnitude, 1082195968))
				{
					return false;
				}
			}
			return true;
		}

		public void Cancel()
		{
			if (this.path != null)
			{
				this.path.Error();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cCollisionMask = 1082195968;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;

		[PublicizedFrom(EAccessModifier.Private)]
		public float smoothPercent;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 moveToPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 moveToLocalPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public ABPath path;
	}
}
