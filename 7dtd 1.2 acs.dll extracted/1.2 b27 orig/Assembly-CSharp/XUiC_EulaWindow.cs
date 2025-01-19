using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EulaWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_EulaWindow.ID = base.WindowGroup.ID;
		this.btnAccept = (XUiC_SimpleButton)base.GetChildById("btnAccept");
		this.btnDecline = (XUiC_SimpleButton)base.GetChildById("btnDecline");
		this.lblContent = (base.GetChildById("lblContent").ViewComponent as XUiV_Label);
		this.footerContainer = (base.GetChildById("footer").ViewComponent as XUiV_Rect);
		this.background = base.GetChildById("background");
		this.btnPageUp = base.GetChildById("btnPageUp");
		this.btnPageDown = base.GetChildById("btnPageDown");
		this.btnAccept.OnPressed += this.btnAccept_OnPressed;
		this.btnDecline.OnPressed += this.btnDecline_OnPressed;
		this.btnPageDown.OnPress += this.btnPageDown_OnPressed;
		this.btnPageUp.OnPress += this.btnPageUp_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnDone")).OnPressed += this.btnDecline_OnPressed;
		TextAsset textAsset = Resources.Load<TextAsset>(string.Format("Data/EULA/eula_{0}", Localization.language.ToLower()));
		if (textAsset != null)
		{
			this.LoadDefaultXML(textAsset.bytes);
			return;
		}
		Log.Error("Could not load default EULA text asset");
	}

	public static void Open(XUi _xui, bool _viewMode = false)
	{
		XUiC_EulaWindow.viewMode = _viewMode;
		_xui.playerUI.windowManager.Open(XUiC_EulaWindow.ID, true, true, true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.pageFormatted = false;
		this.currentPage = 0;
		this.SetVisibility(false);
		this.btnAccept.Enabled = false;
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.lblContent.Label.ResetAndUpdateAnchors();
		base.RefreshBindings(true);
		if (XUiC_EulaWindow.viewMode)
		{
			base.GetChildById("btnDone").SelectCursorElement(true, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close()
	{
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.SetVisibility(false);
		base.xui.playerUI.windowManager.Close(XUiC_EulaWindow.ID);
		if (XUiC_EulaWindow.viewMode)
		{
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, true, false, true);
			return;
		}
		XUiC_MainMenu.Open(base.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadDefaultXML(byte[] _data)
	{
		XmlFile xmlFile;
		try
		{
			xmlFile = new XmlFile(_data, true);
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading default EULA XML: {0}", new object[]
			{
				ex.Message
			});
			return;
		}
		XElement root = xmlFile.XmlDoc.Root;
		if (root == null)
		{
			return;
		}
		this.defaultEulaVersion = int.Parse(root.GetAttribute("version").Trim());
		this.defaultEula = root.Value;
		if (this.defaultEulaVersion > GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion))
		{
			GamePrefs.Set(EnumGamePrefs.EulaLatestVersion, this.defaultEulaVersion);
		}
		Log.Out("Loaded default EULA");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisplayLocalEulaOrClose()
	{
		if (GamePrefs.GetInt(EnumGamePrefs.EulaVersionAccepted) < this.defaultEulaVersion || XUiC_EulaWindow.viewMode)
		{
			this.SetVisibility(true);
			this.FormatPages(this.defaultEula);
			this.ShowGamepadCallouts();
			return;
		}
		this.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FormatPages(string content)
	{
		content = content.Replace("\t", "  ");
		this.pages.Clear();
		string[] array = content.Split('\n', StringSplitOptions.None);
		string language = Localization.language;
		int num;
		if (language == "japanese" || language == "koreana" || language == "schinese" || language == "tchinese")
		{
			num = 1000;
		}
		else
		{
			num = 2000;
		}
		int i = 0;
		while (i < array.Length)
		{
			string text = array[i];
			if (string.IsNullOrWhiteSpace(text))
			{
				i++;
			}
			else
			{
				i++;
				while (text.Length < num && i < array.Length)
				{
					text += "\n\n";
					text += array[i];
					i++;
				}
				this.pages.Add(text);
			}
		}
		this.SetPage(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPage(int page)
	{
		if (page < 0 || page >= this.pages.Count)
		{
			return;
		}
		this.currentPage = page;
		this.lblContent.Text = this.pages[page];
		this.UpdatePageButtonVisibility();
		if (!XUiC_EulaWindow.viewMode && this.currentPage == this.pages.Count - 1 && !this.btnAccept.Enabled)
		{
			base.xui.playerUI.CursorController.SetNavigationTarget(this.btnDecline.ViewComponent);
			this.btnAccept.Enabled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowGamepadCallouts()
	{
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", false, false, true);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (XUiC_EulaWindow.viewMode)
		{
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightStickUpDown, "igcoScroll", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.pageFormatted)
		{
			if (GameManager.UpdatingRemoteResources || !GameManager.RemoteResourcesLoaded)
			{
				return;
			}
			this.pageFormatted = true;
			if (string.IsNullOrEmpty(XUiC_EulaWindow.retrievedEula))
			{
				this.DisplayLocalEulaOrClose();
			}
			else if (GameManager.HasAcceptedLatestEula() && !XUiC_EulaWindow.viewMode)
			{
				this.Close();
			}
			else
			{
				this.SetVisibility(true);
				this.FormatPages(XUiC_EulaWindow.retrievedEula);
				this.ShowGamepadCallouts();
			}
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			float value = base.xui.playerUI.playerInput.GUIActions.scroll.Value;
			if (value == 0f)
			{
				return;
			}
			if (value > 0f)
			{
				this.btnPageUp_OnPressed(null, 0);
			}
			else if (value < 0f)
			{
				this.btnPageDown_OnPressed(null, 0);
			}
		}
		else
		{
			XUi.HandlePaging(base.xui, new Action(this.PageUpAction), new Action(this.PageDownAction), true);
		}
		if (XUiC_EulaWindow.viewMode && base.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			this.Close();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePageButtonVisibility()
	{
		bool flag;
		bool flag2;
		if (!this.background.ViewComponent.IsVisible || !this.pageFormatted || GameManager.UpdatingRemoteResources || !GameManager.RemoteResourcesLoaded || this.pages == null)
		{
			flag = false;
			flag2 = false;
		}
		else
		{
			flag = (this.currentPage > 0);
			flag2 = (this.currentPage < this.pages.Count - 1);
		}
		if (this.btnPageUp.ViewComponent.IsVisible != flag)
		{
			this.btnPageUp.ViewComponent.IsVisible = flag;
		}
		if (this.btnPageDown.ViewComponent.IsVisible != flag2)
		{
			this.btnPageDown.ViewComponent.IsVisible = flag2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.EulaVersionAccepted, GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion));
		GamePrefs.Instance.Save();
		this.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.Close();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageUpAction()
	{
		this.btnPageUp_OnPressed(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageDownAction()
	{
		this.btnPageDown_OnPressed(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnPageUp_OnPressed(XUiController _sender, int _mouseButton)
	{
		int num = this.currentPage - 1;
		if (num < 0)
		{
			return;
		}
		this.SetPage(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnPageDown_OnPressed(XUiController _sender, int _mouseButton)
	{
		int num = this.currentPage + 1;
		if (num >= this.pages.Count)
		{
			return;
		}
		this.SetPage(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisibility(bool _visible)
	{
		this.background.ViewComponent.IsVisible = _visible;
		this.footerContainer.IsVisible = _visible;
		this.UpdatePageButtonVisibility();
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "viewmode")
		{
			_value = XUiC_EulaWindow.viewMode.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDecline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect footerContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public int defaultEulaVersion = -1;

	public static string retrievedEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> pages = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageFormatted;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPage;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool viewMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAlphanumericPageCharacterLimit = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cScriptPageCharacterLimit = 1000;
}
