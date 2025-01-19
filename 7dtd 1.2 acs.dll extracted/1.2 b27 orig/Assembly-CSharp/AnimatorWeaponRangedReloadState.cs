using System;
using UnityEngine;

public class AnimatorWeaponRangedReloadState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("Reload", false);
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		this.actionData = ((componentInParent != null) ? (componentInParent.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged) : null);
		this.actionData.isWeaponReloading = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.actionData != null && this.actionData.isWeaponReloadCancelled)
		{
			animator.Play(0, -1, 1f);
			animator.Update(1f);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.actionData.isWeaponReloading = false;
		this.actionData.isWeaponReloadCancelled = false;
		animator.speed = 1f;
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemActionRanged.ItemActionDataRanged actionData;
}
