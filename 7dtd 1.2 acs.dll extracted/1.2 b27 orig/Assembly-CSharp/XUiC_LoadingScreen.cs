using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoadingScreen : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_LoadingScreen.ID = base.WindowGroup.ID;
		XUiController childById = base.GetChildById("loading_image");
		if (childById != null)
		{
			this.backgroundTextureView = (childById.ViewComponent as XUiV_Texture);
		}
		base.GetChildById("pnlBlack").ViewComponent.IsSnappable = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.showTips && !XUiC_VideoPlayer.IsVideoPlaying)
		{
			if (base.xui.playerUI.playerInput.PermanentActions.PageTipsForward.WasPressed)
			{
				this.cycle(1);
			}
			else if (base.xui.playerUI.playerInput.PermanentActions.PageTipsBack.WasPressed)
			{
				this.cycle(-1);
			}
		}
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycle(int _increment)
	{
		this.currentTipIndex += _increment;
		if (this.currentTipIndex >= XUiC_LoadingScreen.tips.Count)
		{
			this.currentTipIndex = 0;
		}
		else if (this.currentTipIndex < 0)
		{
			this.currentTipIndex = XUiC_LoadingScreen.tips.Count - 1;
		}
		if (this.browseSound != null)
		{
			Manager.PlayXUiSound(this.browseSound, 1f);
		}
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
		if (XUiC_LoadingScreen.backgrounds.Count > 0)
		{
			UnityEngine.Random.InitState(Time.frameCount);
			this.currentBackground = XUiC_LoadingScreen.backgrounds[UnityEngine.Random.Range(0, XUiC_LoadingScreen.backgrounds.Count)];
		}
		this.showTips = true;
		this.currentTipIndex = GamePrefs.GetInt(EnumGamePrefs.LastLoadingTipRead) + 1;
		if (this.currentTipIndex >= XUiC_LoadingScreen.tips.Count)
		{
			this.currentTipIndex = 0;
		}
		this.currentTipIndex = Mathf.Clamp(this.currentTipIndex, 0, XUiC_LoadingScreen.tips.Count - 1);
		base.RefreshBindings(true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
	}

	public override void OnClose()
	{
		base.OnClose();
		GamePrefs.Set(EnumGamePrefs.LastLoadingTipRead, this.currentTipIndex);
		XUiV_Texture xuiV_Texture = this.backgroundTextureView;
		if (xuiV_Texture != null)
		{
			xuiV_Texture.UnloadTexture();
		}
		base.xui.playerUI.CursorController.SetCursorHidden(false);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "browse_sound")
		{
			base.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
			{
				this.browseSound = _o;
			});
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "background_texture")
		{
			_value = this.currentBackground;
			return true;
		}
		if (_bindingName == "index")
		{
			_value = (this.currentTipIndex + 1).ToString();
			return true;
		}
		if (_bindingName == "count")
		{
			_value = XUiC_LoadingScreen.tips.Count.ToString();
			return true;
		}
		if (_bindingName == "show_tips")
		{
			_value = this.showTips.ToString();
			return true;
		}
		if (_bindingName == "title")
		{
			_value = ((this.currentTipIndex < 0) ? "" : Localization.Get(XUiC_LoadingScreen.tips[this.currentTipIndex] + "_title", false));
			return true;
		}
		if (!(_bindingName == "text"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = ((this.currentTipIndex < 0) ? "" : Localization.Get(XUiC_LoadingScreen.tips[this.currentTipIndex], false));
		return true;
	}

	public void SetTipsVisible(bool visible)
	{
		if (this.showTips == visible)
		{
			return;
		}
		this.showTips = visible;
		this.IsDirty = true;
	}

	public static IEnumerator LoadXml(XmlFile _xmlFile)
	{
		XContainer root = _xmlFile.XmlDoc.Root;
		XUiC_LoadingScreen.backgrounds.Clear();
		XUiC_LoadingScreen.tips.Clear();
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (xelement.Name == "backgrounds")
				{
					using (IEnumerator<XElement> enumerator2 = xelement.Elements("tex").GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement element = enumerator2.Current;
							if (!element.HasAttribute("file"))
							{
								Log.Warning("Backgrounds entry is missing file attribute, skipping.");
							}
							else
							{
								XUiC_LoadingScreen.backgrounds.Add(element.GetAttribute("file"));
							}
						}
						continue;
					}
				}
				if (xelement.Name == "tips")
				{
					foreach (XElement element2 in xelement.Elements("tip"))
					{
						if (!element2.HasAttribute("key"))
						{
							Log.Warning("Loading tips entry is missing file attribute, skipping.");
						}
						else
						{
							XUiC_LoadingScreen.tips.Add(element2.GetAttribute("key"));
						}
					}
				}
			}
			yield break;
		}
		yield break;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentTipIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentBackground = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTips = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip browseSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture backgroundTextureView;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> backgrounds = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> tips = new List<string>();
}
