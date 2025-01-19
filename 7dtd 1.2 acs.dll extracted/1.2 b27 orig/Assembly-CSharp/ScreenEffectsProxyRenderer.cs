using System;
using UnityEngine.Rendering.PostProcessing;

public sealed class ScreenEffectsProxyRenderer : PostProcessEffectRenderer<ScreenEffectsProxy>
{
	public override void Render(PostProcessRenderContext context)
	{
		ScreenEffects instance = ScreenEffects.Instance;
		if (instance == null)
		{
			return;
		}
		instance.RenderScreenEffects(context);
	}
}
