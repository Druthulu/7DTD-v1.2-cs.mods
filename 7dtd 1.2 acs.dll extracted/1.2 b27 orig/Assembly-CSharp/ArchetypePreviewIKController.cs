﻿using System;
using UnityEngine;

public class ArchetypePreviewIKController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.animator = base.GetComponent<Animator>();
		Transform transform = base.transform.FindInChilds("LeftFoot", false);
		Transform transform2 = base.transform.FindInChilds("RightFoot", false);
		this.leftFootPos = transform.position;
		this.leftFootRot = transform.rotation;
		this.rightFootPos = transform2.position;
		this.rightFootRot = transform2.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAnimatorIK()
	{
		if (this.animator)
		{
			this.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
			this.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
			this.animator.SetIKPosition(AvatarIKGoal.LeftFoot, this.leftFootPos);
			this.animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.Euler(this.leftFootRot.eulerAngles.x + this.FootRotationModifier.x, this.leftFootRot.eulerAngles.y - this.FootRotationModifier.y, this.leftFootRot.eulerAngles.z + this.FootRotationModifier.z));
			this.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
			this.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
			this.animator.SetIKPosition(AvatarIKGoal.RightFoot, this.rightFootPos);
			this.animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(this.rightFootRot.eulerAngles.x + this.FootRotationModifier.x, this.rightFootRot.eulerAngles.y + this.FootRotationModifier.y, this.rightFootRot.eulerAngles.z + this.FootRotationModifier.z));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Animator animator;

	public bool ikActive;

	public Transform rightHandObj;

	public Transform lookObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 leftFootPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 rightFootPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion leftFootRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion rightFootRot;

	public Vector3 FootRotationModifier = new Vector3(-62f, -198f, -93.5f);
}
