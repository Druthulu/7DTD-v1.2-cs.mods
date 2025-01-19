using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class ScreenEffects : MonoBehaviour
{
	public static ScreenEffects Instance { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		Material[] array = Resources.LoadAll<Material>("ScreenEffects/");
		for (int i = 0; i < array.Length; i++)
		{
			string name = array[i].name;
			this.loadedEffects.Add(new ScreenEffects.ScreenEffect
			{
				Name = name,
				Material = UnityEngine.Object.Instantiate<Material>(array[i]),
				TargetIntensity = 0f,
				Intensity = 0f
			});
		}
		this.SortEffects();
		if (ScreenEffects.Instance != null)
		{
			Debug.LogWarning("Detected multiple ScreenEffects instances when only one is expected.");
			return;
		}
		ScreenEffects.Instance = this;
	}

	public void ResetEffects()
	{
		Material[] array = Resources.LoadAll<Material>("ScreenEffects/");
		for (int i = 0; i < array.Length; i++)
		{
			string name = array[i].name;
			for (int j = 0; j < this.loadedEffects.Count; j++)
			{
				ScreenEffects.ScreenEffect screenEffect = this.loadedEffects[j];
				if (screenEffect.Name == name)
				{
					if (screenEffect.Material != null)
					{
						UnityEngine.Object.Destroy(screenEffect.Material);
					}
					screenEffect.Material = UnityEngine.Object.Instantiate<Material>(array[i]);
				}
			}
		}
		this.SortEffects();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortEffects()
	{
		this.loadedEffects = (from se in this.loadedEffects
		orderby se.Material.renderQueue
		select se).ToList<ScreenEffects.ScreenEffect>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < this.loadedEffects.Count; i++)
		{
			UnityEngine.Object.Destroy(this.loadedEffects[i].Material);
		}
		if (ScreenEffects.Instance == this)
		{
			ScreenEffects.Instance = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		int i = 0;
		while (i < this.activeEffects.Count)
		{
			ScreenEffects.ScreenEffect screenEffect = this.activeEffects[i];
			if (screenEffect == null)
			{
				this.activeEffects.RemoveAt(i);
			}
			else
			{
				if (screenEffect.FadeTime <= 0f)
				{
					screenEffect.Intensity = screenEffect.TargetIntensity;
				}
				else if (screenEffect.TargetIntensity > screenEffect.Intensity)
				{
					screenEffect.Intensity = Mathf.Min(screenEffect.Intensity + Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
				}
				else if (screenEffect.TargetIntensity < screenEffect.Intensity)
				{
					screenEffect.Intensity = Mathf.Max(screenEffect.Intensity - Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
				}
				if (screenEffect.Name == "NightVision")
				{
					world.m_WorldEnvironment.SetNightVision(screenEffect.Intensity);
				}
				if (screenEffect.Intensity == screenEffect.TargetIntensity && screenEffect.Intensity <= 0f)
				{
					this.activeEffects.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}
	}

	public void SetScreenEffect(string _name, float _intensity = 1f, float _fadeTime = 4f)
	{
		ScreenEffects.ScreenEffect screenEffect = this.Find(_name, this.activeEffects);
		if (screenEffect == null)
		{
			if (_intensity <= 0f)
			{
				return;
			}
			screenEffect = this.Find(_name, this.loadedEffects);
			if (screenEffect == null)
			{
				return;
			}
			int renderQueue = screenEffect.Material.renderQueue;
			int index = this.activeEffects.Count;
			for (int i = 0; i < this.activeEffects.Count; i++)
			{
				ScreenEffects.ScreenEffect screenEffect2 = this.activeEffects[i];
				if (screenEffect2 != null && renderQueue <= screenEffect2.Material.renderQueue)
				{
					index = i;
					break;
				}
			}
			this.activeEffects.Insert(index, screenEffect);
		}
		screenEffect.TargetIntensity = _intensity;
		screenEffect.FadeTime = _fadeTime;
	}

	public void DisableScreenEffects()
	{
		for (int i = 0; i < this.activeEffects.Count; i++)
		{
			this.DisableScreenEffect(this.activeEffects[i].Name);
		}
	}

	public void DisableScreenEffect(string _name)
	{
		this.SetScreenEffect(_name, 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ScreenEffects.ScreenEffect Find(string _name, List<ScreenEffects.ScreenEffect> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			ScreenEffects.ScreenEffect screenEffect = _list[i];
			if (screenEffect != null && screenEffect.Name == _name)
			{
				return screenEffect;
			}
		}
		return null;
	}

	public void RenderScreenEffects(PostProcessRenderContext context)
	{
		int count = this.activeEffects.Count;
		if (count == 0)
		{
			Debug.LogWarning("RenderEffect called when no effects are active, incurring a redundant blit. The check in ScreenEffectsProxy.IsEnabledAndSupported is supposed to avoid cases like this.");
			context.command.Blit(context.source, context.destination);
			return;
		}
		int num = -1;
		int num2 = -1;
		if (count > 1)
		{
			num = Shader.PropertyToID("_TempRT1");
			context.command.GetTemporaryRT(num, context.width, context.height);
			if (count > 2)
			{
				num2 = Shader.PropertyToID("_TempRT2");
				context.command.GetTemporaryRT(num2, context.width, context.height);
			}
		}
		RenderTargetIdentifier renderTargetIdentifier = context.source;
		for (int i = 0; i < count; i++)
		{
			ScreenEffects.ScreenEffect screenEffect = this.activeEffects[i];
			screenEffect.Material.SetFloat("Intensity", Mathf.Clamp01(screenEffect.Intensity));
			if (i >= count - 1)
			{
				context.command.Blit(renderTargetIdentifier, context.destination, screenEffect.Material);
			}
			else if (i == 0)
			{
				context.command.Blit(context.source, num, screenEffect.Material);
				renderTargetIdentifier = num;
			}
			else
			{
				RenderTargetIdentifier renderTargetIdentifier2 = (renderTargetIdentifier == num) ? num2 : num;
				context.command.Blit(renderTargetIdentifier, renderTargetIdentifier2, screenEffect.Material);
				renderTargetIdentifier = renderTargetIdentifier2;
			}
		}
		if (count > 1)
		{
			context.command.ReleaseTemporaryRT(num);
			if (count > 2)
			{
				context.command.ReleaseTemporaryRT(num2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDefaultFadeTime = 4f;

	public List<ScreenEffects.ScreenEffect> loadedEffects = new List<ScreenEffects.ScreenEffect>();

	public List<ScreenEffects.ScreenEffect> activeEffects = new List<ScreenEffects.ScreenEffect>();

	public class ScreenEffect
	{
		public string Name;

		public Material Material;

		public float Intensity;

		public float TargetIntensity;

		public float FadeTime;
	}
}
