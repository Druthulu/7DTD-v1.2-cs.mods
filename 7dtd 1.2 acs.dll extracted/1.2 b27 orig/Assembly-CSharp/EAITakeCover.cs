using System;
using System.Collections.Generic;
using ExtUtilsForEnt;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAITakeCover : EAIBase
{
	public EAITakeCover()
	{
		this.MutexBits = 1;
		World world = GameManager.Instance.World;
		if (world != null)
		{
			this.world = world;
		}
		PathFinderThread instance = PathFinderThread.Instance;
		if (instance != null)
		{
			this.pathFinder = instance;
		}
		this.ecm = EntityCoverManager.Instance;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		if (!EntityCoverManager.DebugModeEnabled)
		{
			return false;
		}
		if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (this.theEntity.Jumping && !this.theEntity.isSwimming))
		{
			return false;
		}
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();
		if (attackTarget)
		{
			this.threatTarget = attackTarget;
		}
		return !(this.threatTarget == null) && !this.stopSeekingCover && (this.theEntity.Health < this.theEntity.GetMaxHealth() && Vector3.Distance(this.theEntity.position, this.threatTarget.position) > 5f);
	}

	public override void Start()
	{
		this.timeoutTicks = 800;
		this.retryPathTicks = 60f;
		this.fleeTicks = 0;
		PathFinderThread.Instance.RemovePathsFor(this.theEntity.entityId);
		this.stopSeekingCover = false;
	}

	public override bool Continue()
	{
		return (this.theEntity.Health >= this.theEntity.GetMaxHealth() || Vector3.Distance(this.theEntity.position, this.threatTarget.position) >= 3f) && !this.stopSeekingCover;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setState(EAITakeCover.State _state)
	{
		this.state = _state;
	}

	public override void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (this.world == null)
		{
			return;
		}
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();
		if (attackTarget)
		{
			this.threatTarget = attackTarget;
		}
		if (this.threatTarget == null)
		{
			return;
		}
		if (this.updateCover())
		{
			return;
		}
		switch (this.state)
		{
		case EAITakeCover.State.Idle:
			if (!this.findingPath)
			{
				this.findingPath = true;
				this.setState(EAITakeCover.State.FindPath);
				return;
			}
			break;
		case EAITakeCover.State.FindPath:
			if (!this.pathFinder.IsCalculatingPath(this.theEntity.entityId))
			{
				Vector3 a = this.findCoverDir(false);
				this.pathFinder.RemovePathsFor(this.theEntity.entityId);
				this.pathFinder.FindPath(this.theEntity, this.threatTarget.getHipPosition() + a * 10f * 2f, this.theEntity.moveSpeedAggro, false, this);
				this.setState(EAITakeCover.State.PreProcessPath);
				return;
			}
			break;
		case EAITakeCover.State.PreProcessPath:
			this.pathInfo = this.pathFinder.GetPath(this.theEntity.entityId);
			if (!this.pathFinder.IsCalculatingPath(this.theEntity.entityId))
			{
				PathInfo pathInfo = this.pathInfo;
				if (((pathInfo != null) ? pathInfo.path : null) != null)
				{
					this.currentPath.Clear();
					bool flag = false;
					int num = 0;
					List<Vector3> list = new List<Vector3>();
					for (int i = 0; i < this.pathInfo.path.points.Length; i++)
					{
						Vector3 projectedLocation = this.pathInfo.path.points[i].projectedLocation;
						this.currentPath.Add(projectedLocation);
						Vector3 vector = this.matchHipHeight(projectedLocation);
						List<EAITakeCover.CoverCastInfo> bestCoverDirection = this.getBestCoverDirection(vector, this.threatTarget.getHipPosition(), 10f, false);
						Vector3 dir = Vector3.zero;
						Vector3 v = Vector3.zero;
						if (bestCoverDirection.Count > 0)
						{
							dir = bestCoverDirection[0].Dir;
							v = bestCoverDirection[0].HitPoint;
						}
						Vector3 vector2 = new Vector3i(v).ToVector3CenterXZ();
						if (EUtils.isPositionBlocked(vector, this.threatTarget.getChestPosition(), 65536, false) && vector2 != this.pathEnd)
						{
							list.Add(vector2);
							if (num > 3 || i >= this.pathInfo.path.points.Length - 1)
							{
								int index = 0;
								float num2 = float.MaxValue;
								for (int j = 0; j < list.Count; j++)
								{
									EUtils.DrawBounds(new Vector3i(list[j]), Color.red * Color.yellow * 0.5f, 10f, 1f);
									float num3 = Vector3.Distance(list[j], this.theEntity.position);
									if (num3 < num2 && EUtils.isPositionBlocked(list[j], this.threatTarget.getChestPosition(), 65536, false) && this.ecm.IsFree(list[j]))
									{
										index = j;
										num2 = num3;
									}
								}
								Vector3 vector3 = list[index];
								this.pathEnd = new Vector3i(vector3).ToVector3CenterXZ();
								this.ecm.AddCover(this.pathEnd, dir);
								this.ecm.MarkReserved(this.theEntity.entityId, this.pathEnd);
								EUtils.DrawLine(vector, vector3, Color.red, 10f);
								EUtils.DrawBounds(new Vector3i(vector3), Color.green, 10f, 1f);
								this.pathFinder.FindPath(this.theEntity, this.theEntity.position, this.pathEnd, this.theEntity.moveSpeedAggro, false, this);
								flag = true;
								break;
							}
							num++;
						}
					}
					if (flag && this.currentPath.Count > 0)
					{
						EUtils.DrawPath(this.currentPath, Color.white, Color.yellow);
						this.setState(EAITakeCover.State.ProcessPath);
						return;
					}
					this.freeCover();
					this.retryPathTicks = 60f;
					this.setState(EAITakeCover.State.FindPath);
					return;
				}
			}
			break;
		case EAITakeCover.State.ProcessPath:
			if (this.retryPathTicks > 0f)
			{
				this.retryPathTicks -= 1f;
				if (this.retryPathTicks <= 0f)
				{
					this.freeCover();
					this.retryPathTicks = 60f;
					this.setState(EAITakeCover.State.FindPath);
					return;
				}
			}
			if (this.currentPath.Count > 0)
			{
				if (Vector3.Distance(this.theEntity.position, this.pathEnd) < 0.5f)
				{
					this.pathFinder.RemovePathsFor(this.theEntity.entityId);
					this.theEntity.SetLookPosition(this.threatTarget.getHeadPosition());
					this.ecm.UseCover(this.theEntity.entityId, this.pathEnd);
					this.theEntity.navigator.clearPath();
					this.theEntity.moveHelper.Stop();
					this.coverTicks = 20 * base.Random.RandomRange(4);
					this.findingPath = false;
					this.setState(EAITakeCover.State.Idle);
					return;
				}
			}
			else
			{
				this.freeCover();
				this.retryPathTicks = 60f;
				this.setState(EAITakeCover.State.FindPath);
			}
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateCover()
	{
		if (!this.ecm.HasCover(this.theEntity.entityId))
		{
			return false;
		}
		if (this.ecm.GetCoverPos(this.theEntity.entityId) == null)
		{
			return false;
		}
		if (this.coverTicks > 0)
		{
			this.coverTicks--;
			if (this.coverTicks <= 0)
			{
				if (base.Random.RandomRange(2) < 1)
				{
					this.freeCover();
					if (base.Random.RandomRange(2) < 1)
					{
						this.stopSeekingCover = true;
					}
				}
				else
				{
					this.coverTicks = 60;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeCover()
	{
		this.ecm.FreeCover(this.theEntity.entityId);
		this.coverTicks = 60;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addCover(Vector3 pos, Vector3 dir, bool debugDraw = false)
	{
		if (debugDraw)
		{
			EUtils.DrawBounds(new Vector3i(pos), Color.cyan, 10f, 1f);
		}
		this.ecm.AddCover(pos, dir);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 matchHipHeight(Vector3 point)
	{
		Vector3 vector = this.theEntity.getHipPosition();
		float y = vector.y;
		vector = point;
		vector.y = y;
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCrouching(bool value)
	{
		this.theEntity.Crouching = value;
		this.theEntity.GetComponentInChildren<Animator>().SetBool("IsCrouching", value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToDir(Vector3 dir)
	{
		return Quaternion.Lerp(this.theEntity.transform.rotation, Quaternion.LookRotation(dir), (1f - Vector3.Angle(this.theEntity.transform.forward, dir) / 180f) * 7f * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findCoverDir(bool debugDraw = false)
	{
		RaycastHit raycastHit;
		Vector3 vector = this.getBestCoverDirection(this.threatTarget.getHipPosition(), 10f, out raycastHit, debugDraw);
		if (vector == Vector3.zero)
		{
			vector = (this.theEntity.position - this.threatTarget.position).normalized;
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 getBestCoverDirection(Vector3 point, float dist, out RaycastHit hit, bool debugDraw = false)
	{
		List<EAITakeCover.PosData> list = new List<EAITakeCover.PosData>();
		hit = default(RaycastHit);
		for (int i = 0; i < EAITakeCover.mainBlockAxis.Length; i++)
		{
			if (EUtils.isPositionBlocked(point, point + EAITakeCover.mainBlockAxis[i] * dist, out hit, 65536, debugDraw))
			{
				Vector3 b = new Vector3(0f, 0.5f, 0f);
				RaycastHit raycastHit = default(RaycastHit);
				float cover = 0.5f;
				if (EUtils.isPositionBlocked(point + b, point + b + EAITakeCover.mainBlockAxis[i] * dist, out raycastHit, 65536, debugDraw))
				{
					cover = 1f;
				}
				list.Add(new EAITakeCover.PosData(EAITakeCover.mainBlockAxis[i], hit.distance, cover));
			}
		}
		float num = float.MaxValue;
		int index = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Dist < num)
			{
				num = list[j].Dist;
				index = j;
			}
		}
		if (list.Count > 0)
		{
			if (debugDraw)
			{
				EUtils.DrawLine(point, point + list[0].Dir * dist, Color.green, 10f);
			}
			return list[index].Dir;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITakeCover.CoverCastInfo> getBestCoverDirection(Vector3 point, Vector3 target, float dist, bool debugDraw = false)
	{
		List<EAITakeCover.CoverCastInfo> list = new List<EAITakeCover.CoverCastInfo>();
		Vector3 vector = new Vector3i(point).ToVector3Center();
		vector.y += 0.15f;
		for (int i = 0; i < EAITakeCover.mainBlockAxis.Length; i++)
		{
			Vector3 b = new Vector3i(EAITakeCover.mainBlockAxis[i] * dist) - EAITakeCover.halfBlockOffset;
			RaycastHit raycastHit;
			if (EUtils.isPositionBlocked(vector, vector + b, out raycastHit, 65536, debugDraw))
			{
				list.Add(new EAITakeCover.CoverCastInfo(vector, EAITakeCover.mainBlockAxis[i], raycastHit.point + Origin.position + raycastHit.normal * 0.1f, Vector3.Distance(raycastHit.point + Origin.position, target)));
			}
		}
		list.Sort((EAITakeCover.CoverCastInfo x, EAITakeCover.CoverCastInfo y) => x.ThreatDistance.CompareTo(y.ThreatDistance));
		return list;
	}

	public override string ToString()
	{
		return string.Format("{0}, state {1}, coverTicks {2}", base.ToString(), this.state, this.coverTicks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int timeoutTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int coverTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeDistance = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 halfBlockOffset = Vector3.one * 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathFinderThread pathFinder;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathInfo pathInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPathing;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> currentPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pathEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager ecm;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool targetViewBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive threatTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float retryPathTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAITakeCover.State state;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCoverDist = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool findingPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopSeekingCover;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] mainBlockAxis = new Vector3[]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, 0.5f)
	};

	public enum State
	{
		Idle,
		FindPath,
		PreProcessPath,
		ProcessPath,
		Empty
	}

	[Preserve]
	public class CoverNode
	{
		public Vector3i BlockPos { get; }

		public CoverNode(Vector3 _pos)
		{
			this.BlockPos = new Vector3i(_pos);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<EAITakeCover.CoverNode> neighbors = new List<EAITakeCover.CoverNode>();
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class PosData
	{
		public Vector3 Dir { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public float Dist { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public float Cover { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public PosData(Vector3 _dir, float _dist, float _cover = 0.5f)
		{
			this.Dir = _dir;
			this.Dist = _dist;
			this.Cover = _cover;
		}
	}

	[Preserve]
	public class CoverCastInfo
	{
		public CoverCastInfo(Vector3 _pos, Vector3 _dir, Vector3 _hitPoint, float _threatDist)
		{
			this.Set(_pos, _dir, _hitPoint, _threatDist);
		}

		public void Set(Vector3 _pos, Vector3 _dir, Vector3 _hitPoint, float _threatDist)
		{
			this.Pos = _pos;
			this.Dir = _dir;
			this.HitPoint = _hitPoint;
			this.ThreatDistance = _threatDist;
		}

		public Vector3 Pos;

		public Vector3 Dir;

		public Vector3 HitPoint;

		public float ThreatDistance;
	}
}
