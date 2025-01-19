using System;
using UnityEngine;

public abstract class LightState : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.lightLOD = base.gameObject.GetComponent<LightLOD>();
	}

	public virtual void Kill()
	{
		UnityEngine.Object.Destroy(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float GetDistSqrRatio()
	{
		Vector3 b = GameLightManager.Instance.CameraPos();
		float num = (base.transform.position - b).sqrMagnitude * this.lightLOD.DistanceScale;
		float num2 = (LightLOD.DebugViewDistance > 0f) ? LightLOD.DebugViewDistance : this.lightLOD.MaxDistance;
		num2 *= num2;
		return num / num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateEmissive(float newV, bool useLightColor)
	{
		Color color = useLightColor ? this.lightLOD.GetLight().color : this.lightLOD.EmissiveColor;
		float h;
		float s;
		float num;
		Color.RGBToHSV(color, out h, out s, out num);
		color = Color.HSVToRGB(h, s, newV);
		this.lightLOD.SetEmissiveColorCurrent(color);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public LightState()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public LightLOD lightLOD;

	public float LODThreshold;
}
