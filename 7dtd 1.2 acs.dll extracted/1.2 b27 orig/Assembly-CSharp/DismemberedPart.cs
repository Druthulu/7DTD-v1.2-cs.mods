using System;
using UnityEngine;

public class DismemberedPart : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Vector3 zero = Vector3.zero;
		if (this.useRandomForce)
		{
			World world = GameManager.Instance.World;
			zero.x += world.RandomRange(-0.8f, 0.8f);
			zero.y += world.RandomRange(0f, 0.8f);
			zero.z += world.RandomRange(-0.8f, 0.8f);
		}
		this.rigidbodies = base.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < this.rigidbodies.Length; i++)
		{
			this.rigidbodies[i].AddForce(zero, ForceMode.Impulse);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.hangTime >= 0f)
		{
			this.hangTime -= Time.deltaTime;
			for (int i = 0; i < this.rigidbodies.Length; i++)
			{
				Rigidbody rigidbody = this.rigidbodies[i];
				rigidbody.mass = Mathf.Lerp(rigidbody.mass, 0.5f, this.hangTime / 0.1f);
			}
			if (DismembermentManager.DebugBulletTime)
			{
				Time.timeScale = Mathf.Lerp(0.25f, 1f, 1f - this.hangTime / 0.5f);
				if (this.hangTime <= 0f)
				{
					Time.timeScale = 1f;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody[] rigidbodies;

	public Vector3 initialForce;

	public bool useRandomForce;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float halfMass;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHangTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float hangTime = 0.5f;
}
