using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Look At Target")]
public class LookAtTarget : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.target != null)
		{
			Vector3 forward = this.target.position - this.mTrans.position;
			if (forward.magnitude > 0.001f)
			{
				Quaternion b = Quaternion.LookRotation(forward);
				this.mTrans.rotation = Quaternion.Slerp(this.mTrans.rotation, b, Mathf.Clamp01(this.speed * Time.deltaTime));
			}
		}
	}

	public int level;

	public Transform target;

	public float speed = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;
}
