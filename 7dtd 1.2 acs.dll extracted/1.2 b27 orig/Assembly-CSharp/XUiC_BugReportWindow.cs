using System;
using System.Collections;
using Backtrace.Unity.Model;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_BugReportWindow.ID = base.WindowGroup.ID;
		this.windowGroup.isEscClosable = false;
		this.btnSubmit = base.GetChildById("btnSubmit").GetChildByType<XUiC_SimpleButton>();
		this.btnSubmit.OnPressed += this.BtnSubmitOnPressed;
		base.GetChildById("btnCancel").GetChildByType<XUiC_SimpleButton>().OnPressed += this.BtnCancelOnPressed;
		this.txtDescription = (base.GetChildById("txtDescription") as XUiC_TextInput);
		this.txtDescription.OnChangeHandler += this.TxtDescriptionOnChanged;
		this.comboAttachScreenshot = (base.GetChildById("comboAttachScreenshot") as XUiC_ComboBoxBool);
		this.comboAttachScreenshot.OnValueChanged += this.ComboAttachScreenshot_OnValueChanged;
		this.comboAttachSave = (base.GetChildById("comboAttachSave") as XUiC_ComboBoxBool);
		this.comboAttachSave.OnValueChanged += this.ComboAttachSave_OnValueChanged;
		this.saveSelectWindow = this.windowGroup.Controller.GetChildByType<XUiC_BugReportSaveSelect>();
		if (this.saveSelectWindow != null)
		{
			this.saveSelectWindow.GetChildByType<XUiC_BugReportSavesList>().SelectionChanged += this.List_SelectionChanged;
		}
		this.lblAttachSaveDescInGame = base.GetChildById("lblAttachSaveDescInGame");
		this.lblAttachSaveDescMenu = base.GetChildById("lblAttachSaveDescMenu");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void List_SelectionChanged(XUiC_ListEntry<XUiC_BugReportSavesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_BugReportSavesList.ListEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			this.selectedSaveInfo = _newEntry.GetEntry().saveEntryInfo;
		}
		else
		{
			this.selectedSaveInfo = null;
		}
		this.CheckCanSubmit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAttachSave_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		this.attachSave = _newValue;
		if (!this.inGame)
		{
			this.saveSelectWindow.ViewComponent.IsVisible = this.attachSave;
		}
		if (!this.attachSave)
		{
			this.selectedSaveInfo = null;
			if (this.saveSelectWindow != null)
			{
				this.saveSelectWindow.list.SelectedEntry = null;
			}
		}
		this.CheckCanSubmit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAttachScreenshot_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		this.attachScreenshot = _newValue;
	}

	public static void Open(XUi _xui, bool _fromMainMenu)
	{
		_xui.playerUI.windowManager.Open(XUiC_BugReportWindow.ID, true, false, true);
		XUiC_BugReportWindow.fromMainMenu = _fromMainMenu;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.inGame = (GameManager.Instance.World != null);
		this.txtDescription.Text = "";
		this.attachSave = false;
		this.attachScreenshot = false;
		this.selectedSaveInfo = null;
		this.uploading = false;
		this.comboAttachSave.Value = false;
		this.comboAttachScreenshot.Value = false;
		this.lblAttachSaveDescMenu.ViewComponent.IsVisible = (BacktraceUtils.BugReportAttachSaveFeature && !this.inGame);
		this.lblAttachSaveDescInGame.ViewComponent.IsVisible = (BacktraceUtils.BugReportAttachSaveFeature && this.inGame);
		if (this.inGame)
		{
			GameManager.Instance.Pause(true);
		}
		base.RefreshBindings(false);
		this.CheckCanSubmit();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.uploading = false;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = true;
		if (XUiC_BugReportWindow.fromMainMenu)
		{
			base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, true, false, true);
		}
		if (this.inGame && !XUiC_BugReportWindow.fromMainMenu)
		{
			GameManager.Instance.Pause(false);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.inGame && this.saveSelectWindow != null && this.saveSelectWindow.ViewComponent.IsVisible != this.attachSave)
		{
			this.saveSelectWindow.ViewComponent.IsVisible = this.attachSave;
		}
		else if (this.inGame && this.saveSelectWindow != null && this.saveSelectWindow.ViewComponent.IsVisible)
		{
			this.saveSelectWindow.ViewComponent.IsVisible = false;
		}
		if (!this.uploading && (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased || base.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased))
		{
			base.xui.playerUI.windowManager.Close(XUiC_BugReportWindow.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtDescriptionOnChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.CheckCanSubmit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckCanSubmit()
	{
		if (this.inGame)
		{
			this.btnSubmit.Enabled = (this.canSubmit && this.txtDescription.Text.Length > 0);
			return;
		}
		this.btnSubmit.Enabled = (this.canSubmit && this.txtDescription.Text.Length > 0 && (!this.attachSave || (this.attachSave && this.selectedSaveInfo != null)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmitOnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.canSubmit && this.txtDescription.Text.Length > 0)
		{
			if (this.inGame && this.attachSave)
			{
				SaveInfoProvider.SaveEntryInfo saveEntryInfo2;
				if (GameManager.Instance.World.IsRemote())
				{
					string @string = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
					SaveInfoProvider.SaveEntryInfo saveEntryInfo;
					if (SaveInfoProvider.Instance.TryGetRemoteSaveEntry(@string, out saveEntryInfo))
					{
						this.selectedSaveInfo = saveEntryInfo;
					}
					else
					{
						Log.Error("Could not get save info entry for remote world");
					}
				}
				else if (SaveInfoProvider.Instance.TryGetLocalSaveEntry(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), out saveEntryInfo2))
				{
					this.selectedSaveInfo = saveEntryInfo2;
				}
				else
				{
					Log.Error("Could not get save info entry for local world");
				}
			}
			XUiC_BugReportWindow.lastSubmissionTime = Time.time;
			ThreadManager.StartCoroutine(this.SubmitRoutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SubmitRoutine()
	{
		this.uploading = true;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = false;
		string screenshotPath = null;
		if (this.attachScreenshot)
		{
			base.ViewComponent.UiTransform.gameObject.SetActive(false);
			yield return null;
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(GameUtils.TakeScreenshotEnum(GameUtils.EScreenshotMode.File, PlatformApplicationManager.Application.temporaryCachePath + "/" + Application.productName, 0f, false, 0, 0, false), delegate(Exception _exception)
			{
				Log.Exception(_exception);
			});
			base.ViewComponent.UiTransform.gameObject.SetActive(true);
			screenshotPath = GameUtils.lastSavedScreenshotFilename;
		}
		yield return null;
		XUiC_ProgressWindow.Open(base.xui.playerUI, Localization.Get("xuiBugReportUploading", false), null, true, false, false, true);
		yield return new WaitForSecondsRealtime(0.5f);
		BacktraceUtils.SendBugReport(this.txtDescription.Text, screenshotPath, this.selectedSaveInfo, new Action<BacktraceResult>(this.BugReportCallBack));
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BugReportCallBack(BacktraceResult _result)
	{
		Log.Out("Bug Report Send callback: {0}", new object[]
		{
			_result.message
		});
		XUiC_ProgressWindow.Close(base.xui.playerUI);
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiBugReportHeader", false), Localization.Get("xuiBugReportSubmitted", false), delegate()
		{
			if (XUiC_BugReportWindow.fromMainMenu)
			{
				base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, true, false, true);
			}
		}, XUiC_BugReportWindow.fromMainMenu);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "can_submit")
		{
			this.canSubmit = (Time.time - XUiC_BugReportWindow.lastSubmissionTime >= 600f || XUiC_BugReportWindow.lastSubmissionTime < 0f);
			_value = this.canSubmit.ToString();
			return true;
		}
		if (_bindingName == "in_game")
		{
			_value = this.inGame.ToString();
			return true;
		}
		if (!(_bindingName == "attach_saves_enabled"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = BacktraceUtils.BugReportAttachSaveFeature.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public static float lastSubmissionTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAttachScreenshot;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAttachSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BugReportSaveSelect saveSelectWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblAttachSaveDescInGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblAttachSaveDescMenu;

	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool fromMainMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attachScreenshot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attachSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveEntryInfo selectedSaveInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uploading;
}
