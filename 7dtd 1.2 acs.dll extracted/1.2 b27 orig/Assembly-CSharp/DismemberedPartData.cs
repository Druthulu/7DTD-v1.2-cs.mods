﻿using System;
using UnityEngine;

public class DismemberedPartData
{
	public Vector3 rot { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool hasRotOffset { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public void SetRot(Vector3 _rot)
	{
		this.hasRotOffset = true;
		this.rot = _rot;
	}

	public bool Invalid { get; set; }

	public string Log()
	{
		return string.Format("property: {0} prefabPath: {1} target: {2} damageTag {3}", new object[]
		{
			this.propertyKey,
			this.prefabPath,
			this.targetBone,
			this.damageTypeKey
		});
	}

	public string propertyKey;

	public string prefabPath;

	public string targetBone;

	public string damageTypeKey;

	public bool isDetachable;

	public Vector3 scale;

	public Vector3 offset;

	public bool attachToParent;

	public bool alignToBone;

	public bool snapToChild;

	public string[] particlePaths;

	public bool overrideAnimationState;

	public bool useMask;

	public bool maskOverride;

	public Vector3 tscale;

	public bool isLinked;

	public bool scaleOutLimb;

	public string solTarget;

	public Vector3 solScale;

	public bool hasSolScale;

	public string childTargetObj;
}
