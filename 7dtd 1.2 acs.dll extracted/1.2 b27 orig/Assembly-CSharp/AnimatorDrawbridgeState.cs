using System;
using UnityEngine;

public class AnimatorDrawbridgeState : StateMachineBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.mask = LayerMask.GetMask(new string[]
		{
			"Physics",
			"CC Physics",
			"CC Local Physics"
		});
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.colliders = animator.gameObject.GetComponentsInChildren<Collider>();
		this.rules = new EntityCollisionRules[this.colliders.Length];
		for (int i = this.colliders.Length - 1; i >= 0; i--)
		{
			Collider collider = this.colliders[i];
			this.rules[i] = collider.GetComponent<EntityCollisionRules>();
		}
		this.SetCollidersEnabled(true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.IsInTransition(layerIndex))
		{
			return;
		}
		if (stateInfo.normalizedTime >= 0.99f)
		{
			if (!this.collidersEnabled)
			{
				this.SetCollidersEnabled(true);
			}
			animator.enabled = false;
			return;
		}
		if (!this.collidersEnabled)
		{
			this.SetCollidersEnabled(!this.CheckForObstacles());
			return;
		}
		if (this.disableColliderOnObstacleDetection && this.collidersEnabled && this.CheckForObstacles())
		{
			this.SetCollidersEnabled(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCollidersEnabled(bool enabled)
	{
		if (enabled == this.collidersEnabled)
		{
			return;
		}
		if (this.colliders != null)
		{
			for (int i = this.colliders.Length - 1; i >= 0; i--)
			{
				EntityCollisionRules entityCollisionRules = this.rules[i];
				if (!entityCollisionRules || !entityCollisionRules.IsStatic)
				{
					this.colliders[i].enabled = enabled;
				}
			}
		}
		this.collidersEnabled = enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForObstacles()
	{
		if (this.colliders == null)
		{
			return false;
		}
		for (int i = 0; i < this.colliders.Length; i++)
		{
			EntityCollisionRules entityCollisionRules = this.rules[i];
			if (!entityCollisionRules || !entityCollisionRules.IsStatic)
			{
				Vector3 halfExtents = this.colliders[i].bounds.extents;
				if (this.colliders[i] is MeshCollider)
				{
					halfExtents = Vector3.Scale(((MeshCollider)this.colliders[i]).sharedMesh.bounds.extents, this.colliders[i].transform.localScale);
				}
				if (Physics.OverlapBoxNonAlloc(this.colliders[i].bounds.center, halfExtents, AnimatorDrawbridgeState.overlapBoxHits, this.colliders[i].transform.rotation, this.mask) > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool disableColliderOnObstacleDetection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityCollisionRules[] rules;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Collider[] colliders;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool collidersEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Collider[] overlapBoxHits = new Collider[20];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LayerMask mask;
}
