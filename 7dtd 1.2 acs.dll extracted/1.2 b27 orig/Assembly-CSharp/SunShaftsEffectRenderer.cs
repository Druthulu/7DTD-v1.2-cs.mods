using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public sealed class SunShaftsEffectRenderer : PostProcessEffectRenderer<SunShaftsEffect>
{
	public Mesh fullscreenTriangle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_FullscreenTriangle != null)
			{
				return this.m_FullscreenTriangle;
			}
			this.m_FullscreenTriangle = new Mesh
			{
				name = "Fullscreen Triangle"
			};
			this.m_FullscreenTriangle.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f)
			});
			this.m_FullscreenTriangle.SetIndices(new int[]
			{
				0,
				1,
				2
			}, MeshTopology.Triangles, 0, false);
			this.m_FullscreenTriangle.UploadMeshData(false);
			return this.m_FullscreenTriangle;
		}
	}

	public override void Init()
	{
		base.Init();
		this.sunShaftsMaterial = new Material(base.settings.sunShaftsShader);
		this.sunShaftsMaterial.hideFlags = HideFlags.DontSave;
		this.simpleClearMaterial = new Material(base.settings.simpleClearShader);
		this.simpleClearMaterial.hideFlags = HideFlags.DontSave;
	}

	public override void Release()
	{
		base.Release();
		UnityEngine.Object.Destroy(this.sunShaftsMaterial);
		UnityEngine.Object.Destroy(this.simpleClearMaterial);
	}

	public void DrawBorder(CommandBuffer cmd, RenderTargetIdentifier dest, int width, int height, Material material, int borderWidth = 1)
	{
		cmd.SetRenderTarget(dest);
		Matrix4x4 proj = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, 0f, 1f);
		Matrix4x4 identity = Matrix4x4.identity;
		cmd.SetViewProjectionMatrices(identity, proj);
		cmd.SetViewport(new Rect(0f, 0f, (float)borderWidth, (float)height));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect((float)(width - borderWidth), 0f, (float)borderWidth, (float)height));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(0f, 0f, (float)width, (float)borderWidth));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(0f, (float)(height - borderWidth), (float)width, (float)borderWidth));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
	}

	public override void Render(PostProcessRenderContext context)
	{
		SunShaftsEffect.SunSettings sunSettings = base.settings.autoUpdateSun ? SkyManager.GetSunShaftSettings() : base.settings.GetSunSettings();
		int num = 4;
		if (base.settings.resolution == SunShaftsEffect.SunShaftsResolution.Normal)
		{
			num = 2;
		}
		else if (base.settings.resolution == SunShaftsEffect.SunShaftsResolution.High)
		{
			num = 1;
		}
		Vector3 vector = context.camera.WorldToViewportPoint(sunSettings.sunPosition);
		int width = context.width;
		int height = context.height;
		int width2 = width / num;
		int height2 = height / num;
		int nameID = Shader.PropertyToID("_Temp1");
		context.command.GetTemporaryRT(nameID, width2, height2, 0);
		context.command.SetGlobalVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * base.settings.sunShaftBlurRadius);
		context.command.SetGlobalVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, base.settings.maxRadius));
		context.command.SetGlobalVector("_SunThreshold", sunSettings.sunThreshold);
		context.command.Blit(context.source, nameID, this.sunShaftsMaterial, 2);
		this.DrawBorder(context.command, nameID, width2, height2, this.simpleClearMaterial, 1);
		int num2 = Mathf.Clamp(base.settings.radialBlurIterations, 1, 4);
		float num3 = base.settings.sunShaftBlurRadius * 0.00130208337f;
		context.command.SetGlobalVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
		context.command.SetGlobalVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, base.settings.maxRadius));
		for (int i = 0; i < num2; i++)
		{
			int nameID2 = Shader.PropertyToID("_Temp3");
			context.command.GetTemporaryRT(nameID2, width2, height2, 0);
			context.command.Blit(nameID, nameID2, this.sunShaftsMaterial, 1);
			context.command.ReleaseTemporaryRT(nameID);
			num3 = base.settings.sunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			context.command.SetGlobalVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
			nameID = Shader.PropertyToID("_Temp4");
			context.command.GetTemporaryRT(nameID, width2, height2, 0);
			context.command.Blit(nameID2, nameID, this.sunShaftsMaterial, 1);
			context.command.ReleaseTemporaryRT(nameID2);
			num3 = base.settings.sunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			context.command.SetGlobalVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
		}
		if (vector.z >= 0f)
		{
			context.command.SetGlobalVector("_SunColor", sunSettings.sunColor * sunSettings.sunShaftIntensity);
		}
		else
		{
			context.command.SetGlobalVector("_SunColor", Vector4.zero);
		}
		context.command.SetGlobalTexture("_ColorBuffer", nameID);
		context.command.Blit(context.source, context.destination, this.sunShaftsMaterial, (base.settings.screenBlendMode == SunShaftsEffect.ShaftsScreenBlendMode.Screen) ? 0 : 4);
		context.command.ReleaseTemporaryRT(nameID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material sunShaftsMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material simpleClearMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh m_FullscreenTriangle;
}
