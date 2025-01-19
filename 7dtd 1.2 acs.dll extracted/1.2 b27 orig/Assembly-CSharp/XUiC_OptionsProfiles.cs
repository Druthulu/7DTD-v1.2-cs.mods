using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsProfiles : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsProfiles.ID = base.WindowGroup.ID;
		this.profiles = base.GetChildByType<XUiC_ProfilesList>();
		this.profiles.SelectionChanged += this.Profiles_OnSelectionChanged;
		this.btnOk = base.GetChildById("btnOk").GetChildByType<XUiC_SimpleButton>();
		this.btnOk.OnPressed += this.BtnOk_OnPressed;
		this.btnProfileCreate = base.GetChildById("btnProfileCreate").GetChildByType<XUiC_SimpleButton>();
		this.btnProfileCreate.OnPressed += this.BtnProfileCreate_OnPressed;
		this.btnProfileDelete = base.GetChildById("btnProfileDelete").GetChildByType<XUiC_SimpleButton>();
		this.btnProfileDelete.OnPressed += this.BtnProfileDelete_OnPressed;
		this.btnProfileEdit = base.GetChildById("btnProfileEdit").GetChildByType<XUiC_SimpleButton>();
		this.btnProfileEdit.OnPressed += this.BtnProfileEdit_OnPressed;
		this.deleteProfilePanel = (XUiV_Panel)base.GetChildById("deleteProfilePanel").ViewComponent;
		((XUiC_SimpleButton)this.deleteProfilePanel.Controller.GetChildById("btnCancel")).OnPressed += this.BtnCancelDelete_OnPressed;
		((XUiC_SimpleButton)this.deleteProfilePanel.Controller.GetChildById("btnConfirm")).OnPressed += this.BtnConfirmDelete_OnPressed;
		this.deleteProfileText = (XUiV_Label)this.deleteProfilePanel.Controller.GetChildById("deleteText").ViewComponent;
		this.createProfilePanel = (XUiV_Panel)base.GetChildById("createProfilePanel").ViewComponent;
		((XUiC_SimpleButton)this.createProfilePanel.Controller.GetChildById("btnCancel")).OnPressed += this.BtnCancelCreate_OnPressed;
		this.createProfileConfirm = (XUiC_SimpleButton)this.createProfilePanel.Controller.GetChildById("btnConfirm");
		this.createProfileConfirm.OnPressed += this.BtnConfirmCreate_OnPressed;
		this.createProfileName = (XUiC_TextInput)this.createProfilePanel.Controller.GetChildById("createProfileName");
		this.createProfileName.OnSubmitHandler += this.CreateProfileName_OnSubmitHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileEdit_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenCustomCharacterWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteProfilePanel.IsVisible = true;
		this.deleteProfileText.Text = string.Format(Localization.Get("xuiProfilesDeleteConfirmation", false), Utils.EscapeBbCodes(this.profiles.SelectedEntry.GetEntry().name, false, false));
		base.xui.playerUI.CursorController.SetNavigationLockView(this.deleteProfilePanel, this.deleteProfilePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.createProfilePanel.IsVisible = true;
		this.createProfileName.Text = "";
		base.xui.playerUI.CursorController.SetNavigationLockView(this.createProfilePanel, null);
		this.createProfileName.SelectOrVirtualKeyboard(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Profiles_OnSelectionChanged(XUiC_ListEntry<XUiC_ProfilesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_ProfilesList.ListEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			ProfileSDF.SetSelectedProfile(_newEntry.GetEntry().name);
			ProfileSDF.Save();
		}
		this.updateButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteProfilePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.btnProfileDelete.SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteProfilePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.btnProfileDelete.SelectCursorElement(false, false);
		ProfileSDF.DeleteProfile(this.profiles.SelectedEntry.GetEntry().name);
		this.playerProfileCount--;
		string selectedProfile = "";
		string[] array = ProfileSDF.GetProfiles();
		if (array.Length != 0)
		{
			selectedProfile = array[0];
		}
		ProfileSDF.SetSelectedProfile(selectedProfile);
		this.profiles.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.createProfilePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.btnProfileCreate.SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.createProfileConfirm.Enabled)
		{
			this.createProfilePanel.IsVisible = false;
			base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
			this.btnProfileCreate.SelectCursorElement(false, false);
			string text = this.createProfileName.Text.Trim();
			ProfileSDF.SaveProfile(text, "", true, "White", 1, "Blue01", "", "", "", "", "");
			ProfileSDF.SetSelectedProfile(text);
			ProfileSDF.Save();
			this.playerProfileCount++;
			this.profiles.RebuildList(false);
			this.OpenCustomCharacterWindow();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenCustomCharacterWindow()
	{
		Action onCloseAction = this.OnCloseAction;
		this.OnCloseAction = null;
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		this.OnCloseAction = onCloseAction;
		base.xui.playerUI.windowManager.Open(XUiC_CustomCharacterWindowGroup.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnInputAbortedHandler(XUiController _sender)
	{
		this.BtnCancelCreate_OnPressed(this, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnSubmitHandler(XUiController _sender, string _text)
	{
		ThreadManager.AddSingleTaskMainThread("OpenProfileEditorWindow", delegate(object _func)
		{
			this.BtnConfirmCreate_OnPressed(this, -1);
		}, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateButtonStates()
	{
		bool flag = ProfileSDF.CurrentProfileName().Length != 0;
		bool flag2 = this.profiles.SelectedEntry != null;
		this.btnOk.Enabled = (flag && flag2);
		bool flag3 = flag && flag2;
		PlayerProfile playerProfile = PlayerProfile.LoadLocalProfile();
		Archetype archetype = Archetype.GetArchetype(playerProfile.ProfileArchetype);
		if (archetype == null)
		{
			archetype = Archetype.GetArchetype(playerProfile.IsMale ? "BaseMale" : "BaseFemale");
		}
		if (archetype != null)
		{
			flag3 &= archetype.CanCustomize;
		}
		this.btnProfileEdit.Enabled = flag3;
		this.btnProfileDelete.Enabled = flag3;
		this.btnProfileCreate.Enabled = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		string text = ProfileSDF.CurrentProfileName();
		this.playerProfileCount = 0;
		foreach (XUiC_ProfilesList.ListEntry listEntry in this.profiles.AllEntries())
		{
			ProfileSDF.SetSelectedProfile(listEntry.name);
			if (string.IsNullOrEmpty(text))
			{
				text = listEntry.name;
				this.profiles.SelectByName(text);
			}
			if (Archetype.GetArchetype(PlayerProfile.LoadLocalProfile().ProfileArchetype).CanCustomize)
			{
				this.playerProfileCount++;
			}
		}
		ProfileSDF.SetSelectedProfile(text);
		this.deleteProfilePanel.IsVisible = false;
		this.createProfilePanel.IsVisible = false;
		this.updateButtonStates();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.OnCloseAction != null)
		{
			this.OnCloseAction();
			this.OnCloseAction = null;
			return;
		}
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.createProfilePanel.IsVisible)
		{
			string text = this.createProfileName.Text.Trim();
			bool flag = text.Length > 0 && text.IndexOf('.') < 0 && !ProfileSDF.ProfileExists(text);
			this.createProfileConfirm.Enabled = flag;
			this.createProfileName.ActiveTextColor = (flag ? Color.white : Color.red);
		}
	}

	public static void Open(XUi _xuiInstance, Action _onCloseAction = null)
	{
		_xuiInstance.FindWindowGroupByName(XUiC_OptionsProfiles.ID).GetChildByType<XUiC_OptionsProfiles>().OnCloseAction = _onCloseAction;
		_xuiInstance.playerUI.windowManager.Open(XUiC_OptionsProfiles.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_USER_PROFILES = -1;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ProfilesList profiles;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerProfileCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProfileCreate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProfileDelete;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProfileEdit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel deleteProfilePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteProfileText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel createProfilePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput createProfileName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton createProfileConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action OnCloseAction;
}
