using System;
using UnityEngine;

public class DetachedDismembermentPart
{
	public Transform detachT { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public Transform pivotT { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool ReadyForCleanup { get; set; }

	public void SetDetached(Transform _t)
	{
		this.detachT = _t;
	}

	public void SetPivot(Transform _t)
	{
		this.pivotT = _t;
	}

	public void CleanupDetached()
	{
		if (this.detachT)
		{
			UnityEngine.Object.Destroy(this.detachT.gameObject);
			this.detachT = null;
		}
	}

	public void Update()
	{
		this.elapsedTime += Time.deltaTime;
		if (this.pivotT && this.overrideHeadSize != 1f)
		{
			if (!this.startValuesSet)
			{
				this.startingHeadSize = this.overrideHeadSize;
				this.startingScale = this.pivotT.localScale;
				this.startValuesSet = true;
			}
			float t = this.elapsedTime / this.overrideHeadDismemberScaleTime;
			this.overrideHeadSize = Mathf.Lerp(this.startingHeadSize, 1f, t);
			this.pivotT.localScale = Vector3.Lerp(this.startingScale, Vector3.one, t);
		}
		if (this.elapsedTime >= this.lifeTime)
		{
			this.ReadyForCleanup = true;
		}
	}

	public float lifeTime = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float elapsedTime;

	public float overrideHeadSize = 1f;

	public float overrideHeadDismemberScaleTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startValuesSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public float startingHeadSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startingScale;
}
