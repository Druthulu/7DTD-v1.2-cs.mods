using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Fluctuating : LightState
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.LODThreshold = 0.2f;
		this.lightComp = this.lightLOD.GetLight();
		this.startIntensity = this.lightComp.intensity;
		this.fixedFrameRate = 1f / Time.fixedDeltaTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		if (base.GetDistSqrRatio() >= this.LODThreshold)
		{
			base.enabled = false;
		}
		if (this.canSwitchProcess)
		{
			this.lightLOD.SwitchLightByState(true);
			this.ChangeProcess();
			this.currentFrame = 0;
			int num = (int)(this.lightLOD.FluxDelay * this.fixedFrameRate);
			if (this.process == 0)
			{
				this.numOfFrames = UnityEngine.Random.Range(num / 2, num);
				this.t = 0f;
				this.preSlideIntenisty = this.lightComp.intensity;
				this.up = (UnityEngine.Random.Range(0f, 1f) > this.slideProbability(this.preSlideIntenisty));
				if (this.up)
				{
					this.slideTo = UnityEngine.Random.Range(this.preSlideIntenisty, this.hiRange);
				}
				else
				{
					this.slideTo = UnityEngine.Random.Range(this.loRange, this.preSlideIntenisty);
				}
				this.increment = (this.slideTo - this.preSlideIntenisty) / (float)this.numOfFrames;
			}
			else if (this.process == 1)
			{
				this.numOfFrames = UnityEngine.Random.Range(90, 181);
			}
			else
			{
				this.numOfFrames = UnityEngine.Random.Range(num / 2, num);
			}
		}
		if (this.process == 0)
		{
			this.Slide();
		}
		else if (this.process == 1)
		{
			this.Flutter();
		}
		else if (this.canSwitchProcess)
		{
			this.lightComp.intensity = this.startIntensity;
			base.UpdateEmissive(this.startIntensity / this.hiRange, this.lightLOD.EmissiveFromLightColorOn);
		}
		int num2 = this.currentFrame + 1;
		this.currentFrame = num2;
		this.canSwitchProcess = (num2 >= this.numOfFrames);
	}

	public override void Kill()
	{
		base.Kill();
		this.lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Slide()
	{
		if (this.up)
		{
			this.lightComp.intensity = Mathf.Lerp(this.preSlideIntenisty, this.slideTo, this.t);
			base.UpdateEmissive(this.lightComp.intensity / this.hiRange, this.lightLOD.EmissiveFromLightColorOn);
			this.t += this.increment;
			return;
		}
		this.lightComp.intensity = Mathf.Lerp(this.slideTo, this.preSlideIntenisty, this.t);
		base.UpdateEmissive(this.lightComp.intensity / this.hiRange, this.lightLOD.EmissiveFromLightColorOn);
		this.t -= this.increment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Flutter()
	{
		int num = UnityEngine.Random.Range(0, 3);
		if (num == 0)
		{
			this.lightComp.intensity = Mathf.Clamp(this.lightComp.intensity + 0.25f, this.loRange, this.hiRange);
			base.UpdateEmissive(this.lightComp.intensity / this.hiRange, this.lightLOD.EmissiveFromLightColorOn);
			return;
		}
		if (num != 1)
		{
			return;
		}
		this.lightComp.intensity = Mathf.Clamp(this.lightComp.intensity - 0.25f, this.loRange, this.hiRange);
		base.UpdateEmissive(this.lightComp.intensity / this.hiRange, this.lightLOD.EmissiveFromLightColorOn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Skip()
	{
		if (this.didSkip)
		{
			this.lightLOD.SwitchLightByState(true);
			this.didSkip = false;
		}
		if (this.currentFrame == UnityEngine.Random.Range(0, this.numOfFrames))
		{
			this.lightLOD.SwitchLightByState(!this.lightLOD.bSwitchedOn);
			this.didSkip = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float slideProbability(float intensity)
	{
		return 1f / (this.hiRange - this.loRange) * (intensity - this.loRange);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		LightLOD lightLOD = this.lightLOD;
		lightLOD.MaxIntensityChanged = (LightLOD.MaxIntensityEvent)Delegate.Combine(lightLOD.MaxIntensityChanged, new LightLOD.MaxIntensityEvent(this.ChangeMaxIntensity));
		this.hiRange = this.lightLOD.MaxIntensity;
		this.loRange = 0.2f * this.lightLOD.MaxIntensity;
		this.lightLOD.lightStateEnabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LightLOD lightLOD = this.lightLOD;
		lightLOD.MaxIntensityChanged = (LightLOD.MaxIntensityEvent)Delegate.Remove(lightLOD.MaxIntensityChanged, new LightLOD.MaxIntensityEvent(this.ChangeMaxIntensity));
		this.lightLOD.MaxIntensity = this.hiRange;
		this.lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeMaxIntensity()
	{
		this.loRange = 0.2f * this.lightLOD.MaxIntensity;
		this.hiRange = this.lightLOD.MaxIntensity;
		this.lightComp.intensity = (this.startIntensity = this.lightLOD.MaxIntensity);
		this.currentFrame = this.numOfFrames;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeProcess()
	{
		int num = UnityEngine.Random.Range(1, 3);
		if (num != this.process)
		{
			this.process = num;
			return;
		}
		if (this.process > 0)
		{
			this.process = (this.process + 1) % 3;
			return;
		}
		this.process++;
	}

	public float hiRange;

	public float loRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float increment;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light lightComp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fixedFrameRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canSwitchProcess = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int process;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numOfFrames;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float t;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float preSlideIntenisty;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float slideTo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool up;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool didSkip;
}
