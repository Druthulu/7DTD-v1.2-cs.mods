using System;
using UnityEngine;

public class DroneBeamParticle : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!this.drone)
		{
			this.drone = base.GetComponentInParent<EntityDrone>();
			Transform transform = base.transform.parent.FindInChilds("WristLeft", false);
			if (transform)
			{
				base.transform.SetParent(transform, false);
			}
			return;
		}
		this.displayTime -= Time.deltaTime;
		EntityAlive attackTargetLocal = this.drone.GetAttackTargetLocal();
		if (attackTargetLocal)
		{
			Vector3 chestPosition = attackTargetLocal.getChestPosition();
			this.root.transform.rotation = Quaternion.LookRotation(chestPosition - this.drone.HealArmPosition);
			if (this.displayTime <= 0f || (attackTargetLocal && attackTargetLocal.IsDead()))
			{
				UnityEngine.Object.Destroy(this.root);
			}
		}
	}

	public void SetDisplayTime(float time)
	{
		this.displayTime = time;
	}

	public GameObject root;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone drone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float displayTime;
}
