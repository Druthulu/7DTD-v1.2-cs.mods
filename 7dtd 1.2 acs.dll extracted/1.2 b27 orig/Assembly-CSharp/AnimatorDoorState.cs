using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AnimatorDoorState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.matchDoorState(animator, stateInfo))
		{
			this.matchedEnterState = true;
			this.colliders = animator.gameObject.GetComponentsInChildren<Collider>(true);
			this.rules = new EntityCollisionRules[this.colliders.Length];
			for (int i = this.colliders.Length - 1; i >= 0; i--)
			{
				Collider collider = this.colliders[i];
				this.rules[i] = collider.GetComponent<EntityCollisionRules>();
				if (!this.isOpenAnim)
				{
					EntityCollisionRules entityCollisionRules = this.rules[i];
					if (entityCollisionRules && entityCollisionRules.IsAnimPush)
					{
						collider.enabled = false;
					}
				}
			}
			this.colliderState = -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool matchDoorState(Animator animator, AnimatorStateInfo stateInfo)
	{
		bool @bool = animator.GetBool(AnimatorDoorState.isOpenHash);
		this.isOpenAnim = (stateInfo.shortNameHash == AnimatorDoorState.openHash);
		return @bool == this.isOpenAnim;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.matchedEnterState && this.matchDoorState(animator, stateInfo))
		{
			if (this.stopNext)
			{
				this.stopNext = false;
				this.matchedEnterState = false;
				this.colliderState = 1;
				this.EnableColliders(true);
				animator.enabled = false;
				return;
			}
			if (animator.IsInTransition(layerIndex))
			{
				return;
			}
			float normalizedTime = stateInfo.normalizedTime;
			if (normalizedTime >= this.collideOnPercent)
			{
				if (this.colliderState != 1)
				{
					if (this.CheckForObstacles())
					{
						return;
					}
					this.colliderState = 1;
					this.EnableColliders(true);
				}
				else if (this.disableColliderOnObstacleDetection && this.CheckForObstacles())
				{
					this.colliderState = 0;
					this.EnableColliders(false);
				}
				if (normalizedTime >= 0.99f)
				{
					this.stopNext = true;
					return;
				}
			}
			else if (normalizedTime < 1f && normalizedTime >= this.collideOffPercent)
			{
				if (this.colliderState != 0)
				{
					this.colliderState = 0;
					this.EnableColliders(false);
				}
				if (!this.isOpenAnim)
				{
					this.PushPlayers(normalizedTime);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnableColliders(bool _on)
	{
		if (this.colliders != null)
		{
			int i = this.colliders.Length - 1;
			while (i >= 0)
			{
				EntityCollisionRules entityCollisionRules = this.rules[i];
				if (!entityCollisionRules)
				{
					goto IL_49;
				}
				if (!entityCollisionRules.IsStatic)
				{
					if (!entityCollisionRules.IsAnimPush)
					{
						goto IL_49;
					}
					this.colliders[i].enabled = !_on;
				}
				IL_57:
				i--;
				continue;
				IL_49:
				this.colliders[i].enabled = _on;
				goto IL_57;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForObstacles()
	{
		if (this.colliders == null)
		{
			return false;
		}
		Ray ray = new Ray(Vector3.zero, Vector3.up);
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			Vector3 origin = list[i].position - Origin.position;
			origin.y += 0.35f;
			ray.origin = origin;
			for (int j = this.colliders.Length - 1; j >= 0; j--)
			{
				EntityCollisionRules entityCollisionRules = this.rules[j];
				if (!entityCollisionRules || (!entityCollisionRules.IsStatic && !entityCollisionRules.IsAnimPush))
				{
					Collider collider = this.colliders[j];
					bool enabled = collider.enabled;
					collider.enabled = true;
					RaycastHit raycastHit;
					bool flag = collider.Raycast(ray, out raycastHit, 0.9f);
					collider.enabled = enabled;
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PushPlayers(float _normalizedTime)
	{
		if (this.colliders == null)
		{
			return;
		}
		if (_normalizedTime < 0.5f)
		{
			return;
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = list[i];
			Vector3 vector = entityPlayer.position - Origin.position;
			vector.y += 0.1f;
			float num = entityPlayer.m_characterController.GetRadius();
			num *= num;
			for (int j = this.colliders.Length - 1; j >= 0; j--)
			{
				EntityCollisionRules entityCollisionRules = this.rules[j];
				if (entityCollisionRules && entityCollisionRules.IsAnimPush)
				{
					Collider collider = this.colliders[j];
					Vector3 b = collider.ClosestPoint(vector);
					Vector3 forceVec = vector - b;
					forceVec.y = 0f;
					float sqrMagnitude = forceVec.sqrMagnitude;
					if (sqrMagnitude < num)
					{
						float num2 = 0.002f;
						if (sqrMagnitude == 0f)
						{
							forceVec = collider.transform.forward * -1f;
							if (_normalizedTime >= 0.94f)
							{
								num2 *= 7f;
							}
						}
						forceVec = forceVec.normalized * num2;
						entityPlayer.PhysicsPush(forceVec, Vector3.zero, true);
					}
				}
			}
		}
	}

	[Conditional("DEBUG_DOOR")]
	public void LogDoor(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Door {1}", GameManager.frameCount, _format);
		Log.Warning(_format, _args);
	}

	[Range(0f, 1f)]
	public float collideOffPercent;

	[Range(0f, 1f)]
	public float collideOnPercent = 0.99f;

	public bool disableColliderOnObstacleDetection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int openHash = Animator.StringToHash("Open");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int isOpenHash = Animator.StringToHash("IsOpen");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityCollisionRules[] rules;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Collider[] colliders;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int colliderState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isOpenAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool matchedEnterState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool stopNext;
}
