using System;
using System.Collections.Generic;
using InControl;
using Platform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIWindowManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.nguiWindowManager = base.GetComponent<NGUIWindowManager>();
		this.playerUI = base.GetComponent<LocalPlayerUI>();
		GameOptionsManager.ResolutionChanged += this.OnResolutionChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ResolutionChanged -= this.OnResolutionChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _width, int _height)
	{
		this.RecenterAllWindows(_width, _height);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		for (int i = 0; i < this.windowsToOpen.Count; i++)
		{
			GUIWindow guiwindow = this.windowsToOpen[i];
			guiwindow.isShowing = true;
			this.windows.Add(guiwindow);
			this.topmostWindow = guiwindow;
		}
		this.windowsToOpen.Clear();
		this.modalWindow = null;
		for (int j = 0; j < this.windows.Count; j++)
		{
			GUIWindow guiwindow2 = this.windows[j];
			if (guiwindow2.isModal)
			{
				this.modalWindow = guiwindow2;
				break;
			}
		}
		if (this.modalWindow != null)
		{
			bool isDimBackground = this.modalWindow.isDimBackground;
		}
		this.cursorWindowOpen = false;
		for (int k = 0; k < this.windows.Count; k++)
		{
			GUIWindow guiwindow3 = this.windows[k];
			if (guiwindow3.isShowing)
			{
				this.cursorWindowOpen |= guiwindow3.alwaysUsesMouseCursor;
				GUI.matrix = guiwindow3.matrix;
				guiwindow3.OnGUI(this.topmostWindow == guiwindow3);
			}
		}
		GUI.enabled = true;
		List<GUIWindow> list = this.windowsToRemove;
		list.Clear();
		for (int l = 0; l < this.windows.Count; l++)
		{
			GUIWindow guiwindow4 = this.windows[l];
			if (!guiwindow4.isShowing)
			{
				list.Add(guiwindow4);
				this.topmostWindow = ((this.windows.Count > 0) ? this.windows[this.windows.Count - 1] : null);
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			GUIWindow item = list[m];
			this.windows.Remove(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		for (int i = 0; i < this.windows.Count; i++)
		{
			this.windows[i].Update();
		}
		if (this.lastActionClicked.Count != 0)
		{
			for (int j = 0; j < this.lastActionClicked.Count; j++)
			{
				NGuiAction nguiAction = this.lastActionClicked[j];
				PlayerAction hotkey = nguiAction.GetHotkey();
				if (hotkey != null && hotkey.WasReleased)
				{
					nguiAction.OnRelease();
					this.actionsToClear.Add(nguiAction);
				}
			}
			for (int k = 0; k < this.actionsToClear.Count; k++)
			{
				NGuiAction item = this.actionsToClear[k];
				this.lastActionClicked.Remove(item);
			}
			this.actionsToClear.Clear();
		}
		if (this.IsInputActive())
		{
			if (this.playerUI.playerInput != null && this.playerUI.playerInput.PermanentActions.Cancel.WasPressed && UIInput.selection != null)
			{
				UIInput.selection.RemoveFocus();
			}
			return;
		}
		if (!this.IsInputLocked)
		{
			List<NGuiAction> list = this.actionsForGlobalHotkeys;
			list.Clear();
			for (int l = 0; l < this.globalActions.Count; l++)
			{
				PlayerAction hotkey2 = this.globalActions[l].GetHotkey();
				if (hotkey2 != null && (((this.globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnRelease) == NGuiAction.EnumKeyMode.FireOnRelease && hotkey2.WasReleased) || ((this.globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnPress) == NGuiAction.EnumKeyMode.FireOnPress && hotkey2.WasPressed) || ((this.globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnRepeat) == NGuiAction.EnumKeyMode.FireOnRepeat && hotkey2.WasRepeated)))
				{
					list.Add(this.globalActions[l]);
				}
			}
			if (list.Count > 0)
			{
				for (int m = 0; m < list.Count; m++)
				{
					NGuiAction nguiAction2 = list[m];
					nguiAction2.OnClick();
					this.lastActionClicked.Add(nguiAction2);
				}
				list.Clear();
			}
			if (this.playerUI.playerInput != null && this.playerUI.playerInput.PermanentActions.Cancel.WasPressed && !this.IsWindowOpen("popupGroup") && this.modalWindow != null && this.modalWindow.isEscClosable)
			{
				this.CloseAllOpenWindows(null, true);
			}
		}
	}

	public bool IsInputActive()
	{
		return (UIInput.selection != null && UIInput.selection.gameObject.activeInHierarchy) || (this.topmostWindow != null && this.topmostWindow.isInputActive) || this.IsUGUIInputActive() || GameManager.Instance.m_GUIConsole.isShowing;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsUGUIInputActive()
	{
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return false;
		}
		GameObject currentSelectedGameObject = current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		InputField inputField;
		if (currentSelectedGameObject.TryGetComponent<InputField>(out inputField))
		{
			return inputField.isFocused;
		}
		TMP_InputField tmp_InputField;
		return currentSelectedGameObject.TryGetComponent<TMP_InputField>(out tmp_InputField) && tmp_InputField.isFocused;
	}

	public bool IsKeyShortcutsAllowed()
	{
		return PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || (!this.IsInputLocked && !this.IsInputActive());
	}

	public void Add(string _windowName, GUIWindow _window)
	{
		this.nameToWindowMap.Add(_windowName, _window);
		_window.windowManager = this;
		if (this.nguiWindowManager == null)
		{
			this.nguiWindowManager = base.GetComponent<NGUIWindowManager>();
		}
		if (this.playerUI == null)
		{
			this.playerUI = base.GetComponent<LocalPlayerUI>();
		}
		_window.nguiWindowManager = this.nguiWindowManager;
		_window.playerUI = this.playerUI;
	}

	public void Remove(string _windowName)
	{
		GUIWindow guiwindow;
		if (!this.nameToWindowMap.TryGetValue(_windowName, out guiwindow))
		{
			Log.Warning("GUIWindowManager.Remove: Window \"{0}\" unknown!", new object[]
			{
				_windowName
			});
			return;
		}
		if (guiwindow.isShowing)
		{
			this.Close(guiwindow, false);
		}
		guiwindow.Cleanup();
		this.nameToWindowMap.Remove(_windowName);
	}

	public GUIWindow GetWindow(string _windowName)
	{
		GUIWindow result;
		if (!this.nameToWindowMap.TryGetValue(_windowName, out result))
		{
			Log.Warning("GUIWindowManager.Remove: Window \"{0}\" unknown!", new object[]
			{
				_windowName
			});
			return null;
		}
		return result;
	}

	public T GetWindow<T>(string _windowName) where T : GUIWindow
	{
		return (T)((object)this.nameToWindowMap[_windowName]);
	}

	public void SwitchVisible(string _windowName, bool _bIsNotEscClosable = false)
	{
		this.SwitchVisible(this.nameToWindowMap[_windowName], _bIsNotEscClosable);
	}

	public void SwitchVisible(GUIWindow _guiWindow, bool _bIsNotEscClosable = false)
	{
		if (_guiWindow.isModal && _guiWindow.isShowing)
		{
			this.Close(_guiWindow, false);
			return;
		}
		this.Open(_guiWindow, true, _bIsNotEscClosable, true);
	}

	public bool CloseAllOpenWindows(GUIWindow _exceptThis = null, bool _fromEsc = false)
	{
		bool result = false;
		for (int i = 0; i < this.windows.Count; i++)
		{
			GUIWindow guiwindow = this.windows[i];
			if (guiwindow.isModal && (_exceptThis == null || _exceptThis != guiwindow))
			{
				this.Close(guiwindow, _fromEsc);
				result = true;
			}
		}
		if (this.playerUI.CursorController != null)
		{
			if (this.playerUI.CursorController.navigationTarget != null)
			{
				this.playerUI.CursorController.navigationTarget.Controller.Hovered(false);
			}
			this.playerUI.CursorController.SetNavigationTarget(null);
			this.playerUI.CursorController.SetNavigationLockView(null, null);
		}
		return result;
	}

	public bool CloseAllOpenWindows(string _windowName)
	{
		GUIWindow exceptThis = this.nameToWindowMap[_windowName];
		return this.CloseAllOpenWindows(exceptThis, false);
	}

	public void Open(string _windowName, int _x, int _y, bool _bModal, bool _bIsNotEscClosable = false)
	{
		GUIWindow guiwindow = this.nameToWindowMap[_windowName];
		guiwindow.windowRect = new Rect((float)_x, (float)_y, guiwindow.windowRect.width, guiwindow.windowRect.height);
		this.Open(guiwindow, _bModal, _bIsNotEscClosable, true);
	}

	public void OpenIfNotOpen(string _windowName, bool _bModal, bool _bIsNotEscClosable = false, bool _bCloseAllOpenWindows = true)
	{
		if (this.IsWindowOpen(_windowName))
		{
			return;
		}
		this.Open(_windowName, _bModal, _bIsNotEscClosable, _bCloseAllOpenWindows);
	}

	public void CloseIfOpen(string _windowName)
	{
		if (!this.IsWindowOpen(_windowName))
		{
			return;
		}
		this.Close(_windowName);
	}

	public void Open(string _windowName, bool _bModal, bool _bIsNotEscClosable = false, bool _bCloseAllOpenWindows = true)
	{
		if (this.IsFullHUDDisabled())
		{
			return;
		}
		GUIWindow w;
		if (!this.nameToWindowMap.TryGetValue(_windowName, out w))
		{
			Log.Warning("GUIWindowManager.Open: Window \"{0}\" unknown!", new object[]
			{
				_windowName
			});
			Log.Out("Trace: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		QuestEventManager.Current.ChangedWindow(_windowName);
		this.Open(w, _bModal, _bIsNotEscClosable, _bCloseAllOpenWindows);
	}

	public void Open(GUIWindow _w, bool _bModal, bool _bIsNotEscClosable = false, bool _bCloseAllOpenWindows = true)
	{
		if (this.IsFullHUDDisabled())
		{
			return;
		}
		if (_bModal)
		{
			if (_bCloseAllOpenWindows)
			{
				this.CloseAllOpenWindows(null, false);
			}
			int i = 0;
			while (i < this.windowsToOpen.Count)
			{
				GUIWindow guiwindow = this.windowsToOpen[i];
				if (guiwindow.isModal && guiwindow != _w)
				{
					this.windowsToOpen.Remove(guiwindow);
					guiwindow.OnClose();
					if (guiwindow.HasActionSet())
					{
						this.DisableWindowActionSet(guiwindow);
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
		if (_w.isShowing)
		{
			_w.isModal = _bModal;
			return;
		}
		_w.windowManager = this;
		_w.isModal = _bModal;
		_w.isEscClosable = !_bIsNotEscClosable;
		bool flag = _w.isShowing || this.windowsToOpen.Contains(_w);
		if (!this.windows.Contains(_w))
		{
			this.windowsToOpen.Add(_w);
		}
		else
		{
			_w.isShowing = true;
		}
		if (!flag)
		{
			_w.OnOpen();
		}
		if (_w.HasActionSet() && !_w.bActionSetEnabled && (_w.isShowing || this.windowsToOpen.Contains(_w)))
		{
			this.EnableWindowActionSet(_w);
		}
	}

	public bool IsWindowOpen(string _wdwID)
	{
		GUIWindow guiwindow;
		return this.nameToWindowMap.TryGetValue(_wdwID, out guiwindow) && (guiwindow.isShowing || this.windowsToOpen.Contains(guiwindow));
	}

	public bool HasWindow(string _wdwID)
	{
		return this.nameToWindowMap.ContainsKey(_wdwID);
	}

	public bool IsModalWindowOpen()
	{
		return this.modalWindow != null;
	}

	public GUIWindow GetModalWindow()
	{
		return this.modalWindow;
	}

	public bool IsCursorWindowOpen()
	{
		return this.cursorWindowOpen;
	}

	public void Close(string _windowName)
	{
		if (this.HasWindow(_windowName))
		{
			this.Close(this.nameToWindowMap[_windowName], false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableWindowActionSet(GUIWindow _w)
	{
		if (_w.playerUI != null && _w.playerUI.ActionSetManager != null)
		{
			_w.playerUI.ActionSetManager.Pop(_w);
			_w.bActionSetEnabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnableWindowActionSet(GUIWindow _w)
	{
		if (_w.playerUI != null && _w.playerUI.ActionSetManager != null)
		{
			_w.playerUI.ActionSetManager.Push(_w);
			_w.bActionSetEnabled = true;
		}
	}

	public void Close(GUIWindow _w, bool _fromEsc = false)
	{
		if (_w.isShowing)
		{
			_w.isShowing = false;
			_w.OnClose();
			if (_fromEsc && !string.IsNullOrEmpty(_w.openWindowOnEsc))
			{
				this.Open(_w.openWindowOnEsc, _w.isModal, false, true);
			}
		}
		else if (this.windowsToOpen.Contains(_w))
		{
			this.windowsToOpen.Remove(_w);
			_w.OnClose();
		}
		if (_w.bActionSetEnabled)
		{
			this.DisableWindowActionSet(_w);
		}
	}

	public void RecenterAllWindows(int _w, int _h)
	{
		foreach (KeyValuePair<string, GUIWindow> keyValuePair in this.nameToWindowMap)
		{
			string text;
			GUIWindow guiwindow;
			keyValuePair.Deconstruct(out text, out guiwindow);
			GUIWindow guiwindow2 = guiwindow;
			if (guiwindow2.bCenterWindow)
			{
				guiwindow2.SetPosition(((float)_w - guiwindow2.windowRect.width) / 2f, ((float)_h - guiwindow2.windowRect.height) / 2f);
			}
		}
	}

	public void RemoveGlobalAction(NGuiAction _action)
	{
		this.globalActions.Remove(_action);
	}

	public void AddGlobalAction(NGuiAction _action)
	{
		this.globalActions.Add(_action);
	}

	public bool IsHUDEnabled()
	{
		return this.bHUDEnabled == GUIWindowManager.HudEnabledStates.Enabled;
	}

	public bool IsHUDPartialHidden()
	{
		return this.bHUDEnabled == GUIWindowManager.HudEnabledStates.PartialHide;
	}

	public bool IsFullHUDDisabled()
	{
		return this.bHUDEnabled == GUIWindowManager.HudEnabledStates.FullHide;
	}

	public void ToggleHUDEnabled()
	{
		if (this.bHUDEnabled == GUIWindowManager.HudEnabledStates.FullHide)
		{
			this.bHUDEnabled = GUIWindowManager.HudEnabledStates.Enabled;
		}
		else
		{
			this.bHUDEnabled++;
		}
		this.SetHUDEnabled(this.bHUDEnabled);
	}

	public void TempHUDDisable()
	{
		this.bTempEnabled = this.bHUDEnabled;
		this.bHUDEnabled = GUIWindowManager.HudEnabledStates.FullHide;
		this.SetHUDEnabled(this.bHUDEnabled);
	}

	public void ReEnableHUD()
	{
		this.bHUDEnabled = this.bTempEnabled;
		this.SetHUDEnabled(this.bHUDEnabled);
	}

	public void SetHUDEnabled(GUIWindowManager.HudEnabledStates _hudState)
	{
		this.bHUDEnabled = _hudState;
		if (_hudState <= GUIWindowManager.HudEnabledStates.PartialHide)
		{
			this.nguiWindowManager.ShowAll(true);
			this.playerUI.xui.transform.gameObject.SetActive(true);
			return;
		}
		if (_hudState != GUIWindowManager.HudEnabledStates.FullHide)
		{
			return;
		}
		this.nguiWindowManager.ShowAll(false);
		this.playerUI.xui.transform.gameObject.SetActive(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<GUIWindow> windowsToOpen = new List<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<GUIWindow> windowsToRemove = new List<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<GUIWindow> windows = new List<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly Dictionary<string, GUIWindow> nameToWindowMap = new CaseInsensitiveStringDictionary<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindow topmostWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindow modalWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool cursorWindowOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> globalActions = new List<NGuiAction>();

	public bool IsInputLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> lastActionClicked = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> actionsToClear = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> actionsForGlobalHotkeys = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager.HudEnabledStates bHUDEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager.HudEnabledStates bTempEnabled;

	public enum HudEnabledStates
	{
		Enabled,
		PartialHide,
		FullHide
	}
}
