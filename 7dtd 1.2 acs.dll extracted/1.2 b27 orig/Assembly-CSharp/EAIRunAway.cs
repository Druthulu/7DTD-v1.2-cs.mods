using System;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EAIRunAway : EAIBase
{
	public EAIRunAway()
	{
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "fleeDistance", ref this.fleeDistance);
	}

	public override bool CanExecute()
	{
		return this.FindFleePos(this.GetFleeFromPos());
	}

	public override void Start()
	{
		this.timeoutTicks = 800;
		this.fleeTicks = 0;
		this.pathTicks = 0;
		PathFinderThread.Instance.RemovePathsFor(this.theEntity.entityId);
	}

	public override bool Continue()
	{
		return this.timeoutTicks > 0;
	}

	public override void Update()
	{
		this.timeoutTicks--;
		PathEntity path = this.theEntity.navigator.getPath();
		if (this.checkedPath && path != null && path.getCurrentPathLength() >= 2 && path.NodeCountRemaining() <= 2)
		{
			this.fleeTicks = 0;
		}
		int num = this.fleeTicks - 1;
		this.fleeTicks = num;
		if (num <= 0)
		{
			Vector3 fleeFromPos = this.GetFleeFromPos();
			this.FindFleePos(fleeFromPos);
		}
		num = this.pathTicks - 1;
		this.pathTicks = num;
		if (num <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
		{
			this.pathTicks = 60;
			this.theEntity.FindPath(this.targetPos, this.theEntity.GetMoveSpeed(), false, this);
			this.checkedPath = false;
		}
		if (!this.checkedPath && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
		{
			this.checkedPath = true;
			if (path != null)
			{
				Vector3 rawEndPos = path.rawEndPos;
				if (this.theEntity.GetDistanceSq(rawEndPos) < 1.21f && !this.FindRandomPos())
				{
					this.checkedPath = false;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindFleePos(Vector3 fleeFromPos)
	{
		Vector3 dirV = this.theEntity.position - fleeFromPos;
		Vector3 vector = RandomPositionGenerator.CalcPositionInDirection(this.theEntity, this.theEntity.position, dirV, (float)this.fleeDistance, 80f);
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		this.targetPos = vector;
		this.fleeTicks = 60;
		this.pathTicks = 0;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindRandomPos()
	{
		Vector3 vector = RandomPositionGenerator.Calc(this.theEntity, this.fleeDistance, 0);
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		this.targetPos = vector;
		this.fleeTicks = 60;
		this.pathTicks = 0;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract Vector3 GetFleeFromPos();

	public override string ToString()
	{
		return string.Format("{0}, flee {1}, timeout {2}", base.ToString(), this.fleeTicks, this.timeoutTicks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int timeoutTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkedPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeDistance = 12;
}
