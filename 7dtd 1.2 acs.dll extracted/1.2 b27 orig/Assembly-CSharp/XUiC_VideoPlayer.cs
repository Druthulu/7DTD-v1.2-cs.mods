using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Video;

[Preserve]
public class XUiC_VideoPlayer : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_VideoPlayer.ID = this.windowGroup.ID;
		this.videoTexture = base.GetChildById("videoTexture").ViewComponent.UiTransform.GetComponent<UITexture>();
		this.backgroundSprite = base.GetChildById("videoBackground").ViewComponent.UiTransform.GetComponent<UISprite>();
		this.skipPrompt = base.GetChildById("skipPrompt").ViewComponent.UiTransform.gameObject;
		this.skipLabel = (XUiV_Label)base.GetChildById("lblSkip").ViewComponent;
		this.videoPlayer = this.videoTexture.gameObject.AddComponent<VideoPlayer>();
		this.videoPlayer.playOnAwake = false;
		this.videoPlayer.isLooping = false;
		this.videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		this.videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
		this.videoPlayer.prepareCompleted += this.OnVideoPrepared;
		this.videoPlayer.loopPointReached += this.OnVideoFinished;
		this.videoPlayer.errorReceived += this.OnVideoErrorReceived;
		this.skipPrompt.SetActive(false);
	}

	public static void PlayVideo(XUi _xui, VideoData _videoData, bool _skippable, XUiC_VideoPlayer.DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		XUiC_VideoPlayer instance = XUiC_VideoPlayer.GetInstance(_xui);
		_xui.playerUI.windowManager.OpenIfNotOpen("VideoPlayer", true, false, false);
		instance.PlayVideo(_videoData, _skippable, _videoFinishedCallback);
	}

	public static void EndVideo(XUiC_VideoPlayer _videoPlayer)
	{
		_videoPlayer.FinishAndClose(true);
	}

	public static XUiC_VideoPlayer GetInstance(XUi _xui)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_VideoPlayer.ID);
		if (xuiWindowGroup == null)
		{
			return null;
		}
		XUiController controller = xuiWindowGroup.Controller;
		if (controller == null)
		{
			return null;
		}
		return controller.GetChildByType<XUiC_VideoPlayer>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayVideo(VideoData _videoData, bool _skippable, XUiC_VideoPlayer.DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		this.currentVideo = _videoData;
		this.subtitlesEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled);
		this.wasVideoSkipped = false;
		this.skippable = _skippable;
		this.skipPrompt.SetActive(false);
		if (_videoFinishedCallback != null)
		{
			this.onVideoFinished = (XUiC_VideoPlayer.DelegateOnVideoFinished)Delegate.Combine(this.onVideoFinished, _videoFinishedCallback);
		}
		if (this.rt == null || (this.rt != null && (Screen.width != this.rt.width || Screen.height != this.rt.height)))
		{
			Log.Out("Creating video render texture {0} / {1}", new object[]
			{
				Screen.width,
				Screen.height
			});
			this.rt = new RenderTexture(Screen.width, Screen.height, 16);
			this.rt.Create();
		}
		this.videoPlayer.targetTexture = this.rt;
		this.videoTexture.mainTexture = this.videoPlayer.targetTexture;
		string bindingXuiMarkupString = base.xui.playerUI.playerInput.GUIActions.Cancel.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		this.skipLabel.Text = string.Format(Localization.Get("ui_video_skip", false), bindingXuiMarkupString);
		this.previousTimestamp = 0.0;
		this.videoPlayer.url = Application.streamingAssetsPath + _videoData.url;
		this.videoPlayer.Prepare();
		XUiC_VideoPlayer.IsVideoPlaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.IsOpen)
		{
			if (this.subtitlesEnabled && this.videoPlayer.isPlaying)
			{
				double time = this.videoPlayer.time;
				foreach (VideoSubtitle videoSubtitle in this.currentVideo.subtitles)
				{
					if (videoSubtitle.timestamp >= this.previousTimestamp && videoSubtitle.timestamp <= time)
					{
						GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, Manager.GetFormattedSubtitle(videoSubtitle.subtitleId), videoSubtitle.duration, true);
						break;
					}
				}
				this.previousTimestamp = time;
			}
			if (this.videoTexture.mainTexture != this.rt)
			{
				this.videoTexture.mainTexture = this.rt;
			}
			this.backgroundSprite.color = Color.black;
			if (this.skippable && base.xui.playerUI.playerInput != null)
			{
				if (!this.skipPrompt.activeSelf && base.xui.playerUI.playerInput.AnyGUIActionPressed())
				{
					this.skipPrompt.SetActive(true);
					this.skipVisibleTime = Time.time;
				}
				else if (this.skipPrompt.activeSelf && base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed)
				{
					this.FinishAndClose(true);
				}
				if (this.skipPrompt.activeSelf && Time.time - this.skipVisibleTime >= 3f)
				{
					this.skipPrompt.SetActive(false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoPrepared(VideoPlayer _source)
	{
		this.videoPlayer.Play();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(VideoPlayer _source, string _message)
	{
		Log.Error("Video player encountered an error. Skipping video. Message: {0}", new object[]
		{
			_message
		});
		this.FinishAndClose(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoFinished(VideoPlayer _source)
	{
		this.FinishAndClose(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishAndClose(bool _skipped)
	{
		this.wasVideoSkipped = _skipped;
		base.xui.playerUI.windowManager.Close(XUiC_SubtitlesDisplay.ID);
		base.xui.playerUI.windowManager.Close("VideoPlayer");
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.rt != null)
		{
			this.videoTexture.mainTexture = (this.videoPlayer.targetTexture = null);
			this.rt.Release();
			UnityEngine.Object.Destroy(this.rt);
		}
		XUiC_VideoPlayer.IsVideoPlaying = false;
		if (this.onVideoFinished != null)
		{
			this.onVideoFinished(this.wasVideoSkipped);
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public VideoPlayer videoPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITexture videoTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Camera videoCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite backgroundSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject skipPrompt;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label skipLabel;

	public XUiC_VideoPlayer.DelegateOnVideoFinished onVideoFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoData currentVideo;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture rt;

	[PublicizedFrom(EAccessModifier.Private)]
	public double previousTimestamp;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skippable;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float skipVisibleDuration = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float skipVisibleTime;

	public static bool IsVideoPlaying = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasVideoSkipped;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool subtitlesEnabled;

	public delegate void DelegateOnVideoFinished(bool skipped);
}
