using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveSpaceNeeded : XUiController
{
	public bool ShouldShowDataBar
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SaveInfoProvider.DataLimitEnabled;
		}
	}

	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !SaveInfoProvider.DataLimitEnabled || this.m_pendingBytes <= this.m_totalAvailableBytes;
		}
	}

	public XUiC_SaveSpaceNeeded.ConfirmationResult Result { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1709222174U)
		{
			if (num <= 33042494U)
			{
				if (num != 31044760U)
				{
					if (num == 33042494U)
					{
						if (_bindingName == "langKeyDiscard")
						{
							_value = this.m_langKeyDiscard;
							return true;
						}
					}
				}
				else if (_bindingName == "shouldShowDataBar")
				{
					_value = this.ShouldShowDataBar.ToString();
					return true;
				}
			}
			else if (num != 119933375U)
			{
				if (num != 953006081U)
				{
					if (num == 1709222174U)
					{
						if (_bindingName == "langKeyCancel")
						{
							_value = this.m_langKeyCancel;
							return true;
						}
					}
				}
				else if (_bindingName == "canDiscard")
				{
					_value = this.m_canDiscard.ToString();
					return true;
				}
			}
			else if (_bindingName == "hasSufficientSpace")
			{
				_value = this.HasSufficientSpace.ToString();
				return true;
			}
		}
		else if (num <= 2615038744U)
		{
			if (num != 2066570139U)
			{
				if (num == 2615038744U)
				{
					if (_bindingName == "langKeyBody")
					{
						_value = this.m_langKeyBody;
						return true;
					}
				}
			}
			else if (_bindingName == "canCancel")
			{
				_value = this.m_canCancel.ToString();
				return true;
			}
		}
		else if (num != 2723046083U)
		{
			if (num != 2742710468U)
			{
				if (num == 3429363738U)
				{
					if (_bindingName == "langKeyTitle")
					{
						_value = this.m_langKeyTitle;
						return true;
					}
				}
			}
			else if (_bindingName == "langKeyConfirm")
			{
				_value = this.m_langKeyConfirm;
				return true;
			}
		}
		else if (_bindingName == "langKeyManage")
		{
			_value = this.m_langKeyManage;
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	public override void Init()
	{
		base.Init();
		XUiC_SaveSpaceNeeded.ID = base.WindowGroup.ID;
		this.labelTitle = (XUiV_Label)base.GetChildById("titleText").ViewComponent;
		this.labelBody = (XUiV_Label)base.GetChildById("bodyText").ViewComponent;
		this.btnCancel = (XUiC_SimpleButton)base.GetChildById("btnCancel");
		this.btnDiscard = (XUiC_SimpleButton)base.GetChildById("btnDiscard");
		this.btnManage = (XUiC_SimpleButton)base.GetChildById("btnManage");
		this.btnConfirm = (XUiC_SimpleButton)base.GetChildById("btnConfirm");
		this.btnCancel.OnPressed += this.BtnCancel_OnPressed;
		this.btnDiscard.OnPressed += this.BtnDiscard_OnPressed;
		this.btnManage.OnPressed += this.BtnManage_OnPressed;
		this.btnConfirm.OnPressed += this.BtnConfirm_OnPressed;
		this.dataManagementBar = (base.GetChildById("data_bar_controller") as XUiC_DataManagementBar);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.m_canCancel)
		{
			Log.Error("[SaveSpaceNeeded] Cancel button was pressed even though cancel is hidden?");
			return;
		}
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Cancelled;
		base.xui.playerUI.windowManager.Close(XUiC_SaveSpaceNeeded.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDiscard_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.m_canDiscard)
		{
			Log.Error("[SaveSpaceNeeded] Discard button was pressed even though discard is hidden?");
			return;
		}
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Discarded;
		base.xui.playerUI.windowManager.Close(XUiC_SaveSpaceNeeded.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID);
		object obj;
		if (xuiWindowGroup == null)
		{
			obj = null;
		}
		else
		{
			XUiController controller = xuiWindowGroup.Controller;
			obj = ((controller != null) ? controller.GetChildByType<XUiC_LoadingScreen>() : null);
		}
		object obj2 = obj;
		if (obj2 != null)
		{
			obj2.SetTipsVisible(false);
		}
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.HasSufficientSpace)
		{
			Log.Error("[SaveSpaceNeeded] Confirm button was pressed even though there isn't enough free space?");
			return;
		}
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed;
		base.xui.playerUI.windowManager.Close(XUiC_SaveSpaceNeeded.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID);
		object obj;
		if (xuiWindowGroup == null)
		{
			obj = null;
		}
		else
		{
			XUiController controller = xuiWindowGroup.Controller;
			obj = ((controller != null) ? controller.GetChildByType<XUiC_LoadingScreen>() : null);
		}
		object obj2 = obj;
		if (obj2 != null)
		{
			obj2.SetTipsVisible(true);
		}
		this.UpdateBarValues();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!XUiC_SaveSpaceNeeded.m_openedProperly)
		{
			Log.Error("[SaveSpaceNeeded] XUiC_SaveSpaceNeeded should be opened with the static Open method so that InitInternal is executed.");
			base.xui.playerUI.windowManager.Close(XUiC_SaveSpaceNeeded.ID);
			return;
		}
		XUiC_SaveSpaceNeeded.m_openedProperly = false;
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Pending;
		this.m_wasCursorHidden = base.xui.playerUI.CursorController.GetCursorHidden();
		base.xui.playerUI.CursorController.SetCursorHidden(false);
		this.m_wasCursorLocked = base.xui.playerUI.CursorController.Locked;
		base.xui.playerUI.CursorController.Locked = false;
		this.m_previousLockView = base.xui.playerUI.CursorController.lockNavigationToView;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.CursorController.SetCursorHidden(this.m_wasCursorHidden);
		base.xui.playerUI.CursorController.SetNavigationLockView(this.m_previousLockView, null);
		base.xui.playerUI.CursorController.Locked = this.m_wasCursorLocked;
		if (this.m_protectedPaths != null)
		{
			foreach (string text in this.m_protectedPaths)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, false);
				}
			}
		}
		ParentControllerState parentControllerState = this.m_parentControllerState;
		if (parentControllerState != null)
		{
			parentControllerState.Restore();
		}
		this.m_pendingBytes = 0L;
		this.m_protectedPaths = null;
		this.m_canCancel = true;
		this.m_canDiscard = true;
		this.m_langKeyTitle = "xuiSave";
		this.m_langKeyBody = "xuiDmSavingBody";
		this.m_langKeyCancel = "xuiCancel";
		this.m_langKeyDiscard = "xuiDiscard";
		this.m_langKeyConfirm = "xuiConfirm";
		this.m_langKeyManage = "xuiDmManageSaves";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitInternal(long pendingBytes, string[] protectedPaths, XUiController parentController, bool autoConfirm, bool canCancel, bool canDiscard, string langKeyTitle, string langKeyBody, string langKeyCancel, string langKeyDiscard, string langKeyConfirm, string langKeyManage)
	{
		this.m_pendingBytes = pendingBytes;
		this.m_protectedPaths = protectedPaths;
		this.m_canCancel = canCancel;
		this.m_canDiscard = canDiscard;
		this.m_langKeyTitle = (string.IsNullOrWhiteSpace(langKeyTitle) ? "xuiSave" : langKeyTitle);
		this.m_langKeyBody = (string.IsNullOrWhiteSpace(langKeyBody) ? "xuiDmSavingBody" : langKeyBody);
		this.m_langKeyCancel = (string.IsNullOrWhiteSpace(langKeyCancel) ? "xuiCancel" : langKeyCancel);
		this.m_langKeyDiscard = (string.IsNullOrWhiteSpace(langKeyDiscard) ? "xuiDiscard" : langKeyDiscard);
		this.m_langKeyConfirm = (string.IsNullOrWhiteSpace(langKeyConfirm) ? "xuiConfirm" : langKeyConfirm);
		this.m_langKeyManage = (string.IsNullOrWhiteSpace(langKeyManage) ? "xuiDmManageSaves" : langKeyManage);
		this.m_parentControllerState = new ParentControllerState(parentController);
		this.m_parentControllerState.Hide();
		if (this.m_protectedPaths != null)
		{
			foreach (string text in this.m_protectedPaths)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, true);
				}
			}
		}
		this.UpdateBarValues();
		if (this.m_pendingBytes != 0L)
		{
			Log.Out("[SaveSpaceNeeded] Pending Bytes: " + this.m_pendingBytes.FormatSize(true) + ", Total Available Bytes: " + this.m_totalAvailableBytes.FormatSize(true));
		}
		if (!autoConfirm || !this.HasSufficientSpace)
		{
			return;
		}
		if (this.m_pendingBytes != 0L)
		{
			Log.Out("[SaveSpaceNeeded] Auto-Confirming.");
		}
		this.BtnConfirm_OnPressed(this.btnConfirm, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarValues()
	{
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		this.dataManagementBar.ViewComponent.IsVisible = SaveInfoProvider.DataLimitEnabled;
		this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		this.dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		this.dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		this.dataManagementBar.SetPendingBytes(this.m_pendingBytes);
		this.m_totalAvailableBytes = instance.TotalAvailableBytes;
		base.RefreshBindings(false);
		if (this.HasSufficientSpace)
		{
			this.btnConfirm.SelectCursorElement(true, false);
			return;
		}
		this.btnManage.SelectCursorElement(true, false);
	}

	public static XUiC_SaveSpaceNeeded Open(long pendingBytes, string protectedPath, XUiController parentController = null, bool autoConfirm = false, bool canCancel = true, bool canDiscard = true, string title = null, string body = null, string cancel = null, string discard = null, string confirm = null, string manage = null)
	{
		return XUiC_SaveSpaceNeeded.Open(pendingBytes, new string[]
		{
			protectedPath
		}, parentController, autoConfirm, canCancel, canDiscard, title, body, cancel, discard, confirm, manage);
	}

	public static XUiC_SaveSpaceNeeded Open(long pendingBytes, string[] protectedPaths, XUiController parentController = null, bool autoConfirm = false, bool canCancel = true, bool canDiscard = true, string title = null, string body = null, string cancel = null, string discard = null, string confirm = null, string manage = null)
	{
		GUIWindowManager windowManager = LocalPlayerUI.primaryUI.xui.playerUI.windowManager;
		XUiC_SaveSpaceNeeded.m_openedProperly = true;
		windowManager.Open(XUiC_SaveSpaceNeeded.ID, true, true, false);
		XUiC_SaveSpaceNeeded.m_openedProperly = false;
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)windowManager.GetWindow(XUiC_SaveSpaceNeeded.ID);
		XUiC_SaveSpaceNeeded xuiC_SaveSpaceNeeded;
		if (xuiWindowGroup == null)
		{
			xuiC_SaveSpaceNeeded = null;
		}
		else
		{
			XUiController controller = xuiWindowGroup.Controller;
			xuiC_SaveSpaceNeeded = ((controller != null) ? controller.GetChildByType<XUiC_SaveSpaceNeeded>() : null);
		}
		XUiC_SaveSpaceNeeded xuiC_SaveSpaceNeeded2 = xuiC_SaveSpaceNeeded;
		if (xuiC_SaveSpaceNeeded2 == null)
		{
			Log.Error("[SaveSpaceNeeded] Failed to retrieve reference to XUiC_SaveSpaceNeeded instance.");
		}
		else
		{
			xuiC_SaveSpaceNeeded2.InitInternal(pendingBytes, protectedPaths, parentController, autoConfirm, canCancel, canDiscard, title, body, cancel, discard, confirm, manage);
		}
		return xuiC_SaveSpaceNeeded2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultTitle = "xuiSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultBody = "xuiDmSavingBody";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultCancel = "xuiCancel";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultDiscard = "xuiDiscard";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultConfirm = "xuiConfirm";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultManage = "xuiDmManageSaves";

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool m_openedProperly;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDiscard;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnManage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView m_previousLockView;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] m_protectedPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_canCancel = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_canDiscard = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyTitle = "xuiSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyBody = "xuiDmSavingBody";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyCancel = "xuiCancel";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyDiscard = "xuiDiscard";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyConfirm = "xuiConfirm";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyManage = "xuiDmManageSaves";

	[PublicizedFrom(EAccessModifier.Private)]
	public ParentControllerState m_parentControllerState;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_totalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_wasCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_wasCursorLocked;

	public enum ConfirmationResult
	{
		Pending,
		Cancelled,
		Discarded,
		Confirmed
	}
}
