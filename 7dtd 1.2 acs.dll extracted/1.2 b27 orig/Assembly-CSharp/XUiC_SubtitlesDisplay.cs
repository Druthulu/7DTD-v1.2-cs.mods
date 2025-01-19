using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SubtitlesDisplay : XUiController
{
	public static bool IsDisplaying { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override void Init()
	{
		base.Init();
		XUiC_SubtitlesDisplay.ID = this.windowGroup.ID;
		this.subtitlesLabel = (XUiV_Label)base.GetChildById("lblSubtitle").ViewComponent;
		this.background = (XUiV_Panel)base.GetChildById("bgPanel").ViewComponent;
		this.bgSprite = this.background.UiTransform.Find("_background").GetComponentInChildren<UISprite>();
		this.subtitlesLabel.MaxLineCount = 2;
		this.subtitlesLabel.Overflow = UILabel.Overflow.ShrinkContent;
	}

	public static void DisplaySubtitle(LocalPlayerUI ui, string text, float duration = 3f, bool centerAlign = false)
	{
		XUiC_SubtitlesDisplay instance = XUiC_SubtitlesDisplay.GetInstance(ui.xui);
		ui.windowManager.OpenIfNotOpen("SubtitlesDisplay", false, false, false);
		instance.SetSubtitle(text, Mathf.Max(duration, 3f) + 1f, centerAlign);
	}

	public static XUiC_SubtitlesDisplay GetInstance(XUi _xui)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_SubtitlesDisplay.ID);
		if (xuiWindowGroup == null)
		{
			return null;
		}
		XUiController controller = xuiWindowGroup.Controller;
		if (controller == null)
		{
			return null;
		}
		return controller.GetChildByType<XUiC_SubtitlesDisplay>();
	}

	public void SetSubtitle(string text, float duration, bool centerAlign)
	{
		this.subtitlesLabel.Text = text;
		this.alignment = (centerAlign ? NGUIText.Alignment.Center : NGUIText.Alignment.Left);
		this.subtitlesLabel.Label.alignment = this.alignment;
		this.subtitlesLabel.Label.ProcessText(false, true);
		this.openTime = Time.time;
		this.duration = duration;
		XUiC_SubtitlesDisplay.IsDisplaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.IsOpen)
		{
			if (this.subtitlesLabel.Label.alignment != this.alignment)
			{
				this.subtitlesLabel.Label.alignment = this.alignment;
			}
			if (this.subtitlesLabel.Label.overflowMethod != UILabel.Overflow.ShrinkContent)
			{
				this.subtitlesLabel.Label.overflowMethod = UILabel.Overflow.ShrinkContent;
			}
			if (this.subtitlesLabel.Label.width != 1152)
			{
				this.subtitlesLabel.Label.width = 1152;
			}
			if (Time.time - this.openTime >= this.duration)
			{
				base.xui.playerUI.windowManager.CloseIfOpen("SubtitlesDisplay");
				XUiC_SubtitlesDisplay.IsDisplaying = false;
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_SubtitlesDisplay.IsDisplaying = false;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label subtitlesLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel background;

	[PublicizedFrom(EAccessModifier.Private)]
	public UISprite bgSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float minDuration = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float durationAdd = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int labelPadding = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public int targetHeight = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIText.Alignment alignment = NGUIText.Alignment.Left;
}
