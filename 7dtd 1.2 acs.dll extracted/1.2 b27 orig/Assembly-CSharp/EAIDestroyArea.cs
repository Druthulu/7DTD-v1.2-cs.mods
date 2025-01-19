using System;
using System.Diagnostics;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIDestroyArea : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 3;
		this.executeDelay = 1f + base.RandomFloat * 0.9f;
	}

	public override bool CanExecute()
	{
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		if (!moveHelper.CanBreakBlocks)
		{
			return false;
		}
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();
		if (!attackTarget)
		{
			return false;
		}
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		bool flag = this.isLookFar;
		if (moveHelper.IsDestroyAreaTryUnreachable)
		{
			moveHelper.IsDestroyAreaTryUnreachable = false;
			float num = moveHelper.UnreachablePercent;
			if (num > 0f)
			{
				if (base.RandomFloat < num)
				{
					flag = true;
					num = 0f;
				}
				moveHelper.UnreachablePercent = num * 0.5f;
			}
		}
		if (this.manager.pathCostScale < 0.65f)
		{
			float num2 = (1f - this.manager.pathCostScale * 1.53846157f) * 0.6f;
			if (base.RandomFloat < num2)
			{
				PathEntity path = this.theEntity.navigator.getPath();
				if (path != null && path.NodeCountRemaining() > 18 && (attackTarget.position - this.theEntity.position).sqrMagnitude <= 81f)
				{
					flag = true;
				}
			}
		}
		if (!flag && !moveHelper.IsUnreachableAbove)
		{
			return false;
		}
		Vector3 vector = this.theEntity.position;
		Vector3 vector2 = moveHelper.IsUnreachableSide ? moveHelper.UnreachablePos : attackTarget.position;
		Vector3 a = vector - vector2;
		float sqrMagnitude = a.sqrMagnitude;
		if (sqrMagnitude > 25f)
		{
			vector = vector2 + a * (5f / Mathf.Sqrt(sqrMagnitude));
		}
		vector.x += -3f + base.RandomFloat * 6f;
		vector.z += -3f + base.RandomFloat * 6f;
		if (!moveHelper.FindDestroyPos(ref vector, this.isLookFar))
		{
			return false;
		}
		this.seekPos = vector;
		this.seekBlockPos = World.worldToBlockPos(vector);
		this.isLookFar = false;
		this.state = EAIDestroyArea.eState.FindPath;
		this.theEntity.navigator.clearPath();
		this.theEntity.FindPath(vector, this.theEntity.GetMoveSpeedAggro(), true, this);
		moveHelper.IsDestroyArea = true;
		return true;
	}

	public override void Start()
	{
		this.isAtPathEnd = false;
		this.delayTime = 3f;
		this.attackTimeout = 0;
	}

	public void Stop()
	{
		this.delayTime = 0f;
	}

	public override bool Continue()
	{
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		if (this.delayTime <= 0f)
		{
			return false;
		}
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		if (this.state == EAIDestroyArea.eState.FindPath && this.theEntity.navigator.HasPath())
		{
			moveHelper.CalcIfUnreachablePos();
			if (moveHelper.IsUnreachableAbove || moveHelper.IsUnreachableSide)
			{
				this.isLookFar = true;
				return false;
			}
			moveHelper.IsUnreachableAbove = true;
			this.state = EAIDestroyArea.eState.HasPath;
			this.delayTime = 15f;
			this.theEntity.navigator.ShortenEnd(0.2f);
		}
		if (this.state == EAIDestroyArea.eState.HasPath)
		{
			PathEntity path = this.theEntity.navigator.getPath();
			if (path != null && path.NodeCountRemaining() <= 1)
			{
				this.state = EAIDestroyArea.eState.EndPath;
				this.delayTime = 5f + base.RandomFloat * 5f;
				this.isAtPathEnd = true;
			}
		}
		if (this.state == EAIDestroyArea.eState.EndPath && !moveHelper.IsBlocked)
		{
			if (!Voxel.BlockHit(this.hitInfo, this.seekBlockPos))
			{
				return false;
			}
			this.state = EAIDestroyArea.eState.Attack;
			this.theEntity.SeekYawToPos(this.seekPos, 10f);
		}
		return this.isAtPathEnd || !this.theEntity.navigator.noPathAndNotPlanningOne();
	}

	public override void Update()
	{
		this.delayTime -= 0.05f;
		if (this.state == EAIDestroyArea.eState.Attack)
		{
			int num = this.attackTimeout - 1;
			this.attackTimeout = num;
			if (num <= 0)
			{
				ItemActionAttackData itemActionAttackData = this.theEntity.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
				if (itemActionAttackData != null)
				{
					this.theEntity.SetLookPosition(Vector3.zero);
					if (this.theEntity.Attack(false))
					{
						this.attackTimeout = this.theEntity.GetAttackTimeoutTicks();
						itemActionAttackData.hitDelegate = new ItemActionAttackData.HitDelegate(this.GetHitInfo);
						this.theEntity.Attack(true);
						this.state = EAIDestroyArea.eState.EndPath;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo GetHitInfo(out float damageScale)
	{
		damageScale = 1f;
		return this.hitInfo;
	}

	public override void Reset()
	{
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		moveHelper.Stop();
		moveHelper.IsUnreachableAbove = false;
		moveHelper.IsDestroyArea = false;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, delayTime {2}", base.ToString(), this.state.ToStringCached<EAIDestroyArea.eState>(), this.delayTime.ToCultureInvariantString("0.00"));
	}

	[Conditional("DEBUG_AIDESTROY")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void LogDestroy(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} EAIDestroyArea {1} {2}, {3}", new object[]
		{
			GameManager.frameCount,
			this.theEntity.EntityName,
			this.theEntity.entityId,
			_format
		});
		Log.Warning(_format, _args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDumbDistance = 9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 seekPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i seekBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLookFar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAtPathEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAIDestroyArea.eState state;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo hitInfo = new WorldRayHitInfo();

	[PublicizedFrom(EAccessModifier.Private)]
	public enum eState
	{
		FindPath,
		HasPath,
		EndPath,
		Attack
	}
}
