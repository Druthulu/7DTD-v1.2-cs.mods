using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsControls : XUiController
{
	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		XUiC_OptionsControls.ID = base.WindowGroup.ID;
		this.comboLookSensitivity = base.GetChildById("LookSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboZoomSensitivity = base.GetChildById("ZoomSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboZoomAccel = base.GetChildById("ZoomAccel").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboVehicleSensitivity = base.GetChildById("VehicleSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboWeaponAiming = base.GetChildById("WeaponAiming").GetChildByType<XUiC_ComboBoxBool>();
		this.comboInvertMouseLookY = base.GetChildById("InvertMouseLookY").GetChildByType<XUiC_ComboBoxBool>();
		this.comboSprintLock = base.GetChildById("SprintLock").GetChildByType<XUiC_ComboBoxBool>();
		this.comboLookSensitivity.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboZoomSensitivity.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboZoomAccel.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboVehicleSensitivity.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboWeaponAiming.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboInvertMouseLookY.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboSprintLock.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboLookSensitivity.Min = 0.05000000074505806;
		this.comboLookSensitivity.Max = 1.5;
		this.comboZoomSensitivity.Min = 0.05000000074505806;
		this.comboZoomSensitivity.Max = 1.0;
		this.comboZoomAccel.Min = 0.0;
		this.comboZoomAccel.Max = 3.0;
		this.comboVehicleSensitivity.Min = 0.05000000074505806;
		this.comboVehicleSensitivity.Max = 3.0;
		this.tabs = (base.GetChildById("tabs") as XUiC_TabSelector);
		this.btnBack = (base.GetChildById("btnBack") as XUiC_SimpleButton);
		this.btnDefaults = (base.GetChildById("btnDefaults") as XUiC_SimpleButton);
		this.btnApply = (base.GetChildById("btnApply") as XUiC_SimpleButton);
		this.btnBack.OnPressed += this.BtnBack_OnPressed;
		this.btnDefaults.OnPressed += this.BtnDefaults_OnOnPressed;
		this.btnApply.OnPressed += this.BtnApply_OnPressed;
		this.actionSetIngame = base.xui.playerUI.playerInput;
		this.actionSetVehicles = base.xui.playerUI.playerInput.VehicleActions;
		this.actionSetMenu = base.xui.playerUI.playerInput.GUIActions;
		this.RefreshApplyLabel();
		base.RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(this.btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Combo_OnValueChangedGeneric(XUiController _sender)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.applyChanges();
		this.btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		switch (this.tabs.SelectedTabIndex)
		{
		case 0:
			this.comboLookSensitivity.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsLookSensitivity));
			this.comboZoomSensitivity.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsZoomSensitivity));
			this.comboZoomAccel.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsZoomAccel));
			this.comboVehicleSensitivity.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsVehicleLookSensitivity));
			this.comboWeaponAiming.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsWeaponAiming);
			this.comboInvertMouseLookY.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsInvertMouse);
			this.comboSprintLock.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsSprintLock);
			goto IL_52F;
		case 1:
			using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PlayerActionsBase playerActionsBase = enumerator.Current;
					foreach (PlayerAction playerAction in playerActionsBase.Actions)
					{
						PlayerActionData.ActionUserData actionUserData = playerAction.UserData as PlayerActionData.ActionUserData;
						if (actionUserData != null && actionUserData.actionGroup.actionTab == PlayerActionData.TabMovement)
						{
							playerAction.ResetBindings();
						}
					}
				}
				goto IL_52F;
			}
			break;
		case 2:
			break;
		case 3:
			goto IL_1FC;
		case 4:
			goto IL_285;
		case 5:
			goto IL_30E;
		case 6:
			goto IL_397;
		case 7:
			goto IL_420;
		case 8:
			goto IL_4A9;
		default:
			return;
		}
		using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerActionsBase playerActionsBase2 = enumerator.Current;
				foreach (PlayerAction playerAction2 in playerActionsBase2.Actions)
				{
					PlayerActionData.ActionUserData actionUserData2 = playerAction2.UserData as PlayerActionData.ActionUserData;
					if (actionUserData2 != null && actionUserData2.actionGroup.actionTab == PlayerActionData.TabToolbelt)
					{
						playerAction2.ResetBindings();
					}
				}
			}
			goto IL_52F;
		}
		IL_1FC:
		using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerActionsBase playerActionsBase3 = enumerator.Current;
				foreach (PlayerAction playerAction3 in playerActionsBase3.Actions)
				{
					PlayerActionData.ActionUserData actionUserData3 = playerAction3.UserData as PlayerActionData.ActionUserData;
					if (actionUserData3 != null && actionUserData3.actionGroup.actionTab == PlayerActionData.TabVehicle)
					{
						playerAction3.ResetBindings();
					}
				}
			}
			goto IL_52F;
		}
		IL_285:
		using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerActionsBase playerActionsBase4 = enumerator.Current;
				foreach (PlayerAction playerAction4 in playerActionsBase4.Actions)
				{
					PlayerActionData.ActionUserData actionUserData4 = playerAction4.UserData as PlayerActionData.ActionUserData;
					if (actionUserData4 != null && actionUserData4.actionGroup.actionTab == PlayerActionData.TabMenus)
					{
						playerAction4.ResetBindings();
					}
				}
			}
			goto IL_52F;
		}
		IL_30E:
		using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerActionsBase playerActionsBase5 = enumerator.Current;
				foreach (PlayerAction playerAction5 in playerActionsBase5.Actions)
				{
					PlayerActionData.ActionUserData actionUserData5 = playerAction5.UserData as PlayerActionData.ActionUserData;
					if (actionUserData5 != null && actionUserData5.actionGroup.actionTab == PlayerActionData.TabUi)
					{
						playerAction5.ResetBindings();
					}
				}
			}
			goto IL_52F;
		}
		IL_397:
		using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerActionsBase playerActionsBase6 = enumerator.Current;
				foreach (PlayerAction playerAction6 in playerActionsBase6.Actions)
				{
					PlayerActionData.ActionUserData actionUserData6 = playerAction6.UserData as PlayerActionData.ActionUserData;
					if (actionUserData6 != null && actionUserData6.actionGroup.actionTab == PlayerActionData.TabOther)
					{
						playerAction6.ResetBindings();
					}
				}
			}
			goto IL_52F;
		}
		IL_420:
		using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerActionsBase playerActionsBase7 = enumerator.Current;
				foreach (PlayerAction playerAction7 in playerActionsBase7.Actions)
				{
					PlayerActionData.ActionUserData actionUserData7 = playerAction7.UserData as PlayerActionData.ActionUserData;
					if (actionUserData7 != null && actionUserData7.actionGroup.actionTab == PlayerActionData.TabEdit)
					{
						playerAction7.ResetBindings();
					}
				}
			}
			goto IL_52F;
		}
		IL_4A9:
		foreach (PlayerActionsBase playerActionsBase8 in PlatformManager.NativePlatform.Input.ActionSets)
		{
			foreach (PlayerAction playerAction8 in playerActionsBase8.Actions)
			{
				PlayerActionData.ActionUserData actionUserData8 = playerAction8.UserData as PlayerActionData.ActionUserData;
				if (actionUserData8 != null && actionUserData8.actionGroup.actionTab == PlayerActionData.TabGlobal)
				{
					playerAction8.ResetBindings();
				}
			}
		}
		IL_52F:
		this.updateActionBindingLabels();
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.closedForNewBinding = false;
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		this.comboLookSensitivity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsLookSensitivity);
		this.comboZoomSensitivity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomSensitivity);
		this.comboZoomAccel.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomAccel);
		this.comboVehicleSensitivity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsVehicleLookSensitivity);
		this.comboWeaponAiming.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsWeaponAiming);
		this.comboInvertMouseLookY.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsInvertMouse);
		this.comboSprintLock.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsControlsSprintLock);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createControlsEntries()
	{
		SortedDictionary<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>> sortedDictionary = new SortedDictionary<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>>();
		PlayerActionsBase[] array = new PlayerActionsBase[]
		{
			base.xui.playerUI.playerInput,
			base.xui.playerUI.playerInput.VehicleActions,
			base.xui.playerUI.playerInput.PermanentActions,
			base.xui.playerUI.playerInput.GUIActions,
			PlayerActionsGlobal.Instance
		};
		for (int i = 0; i < array.Length; i++)
		{
			foreach (PlayerAction playerAction in array[i].Actions)
			{
				PlayerActionData.ActionUserData actionUserData = playerAction.UserData as PlayerActionData.ActionUserData;
				if (actionUserData != null)
				{
					switch (actionUserData.appliesToInputType)
					{
					case PlayerActionData.EAppliesToInputType.None:
					case PlayerActionData.EAppliesToInputType.ControllerOnly:
						break;
					case PlayerActionData.EAppliesToInputType.KbdMouseOnly:
					case PlayerActionData.EAppliesToInputType.Both:
						if (!actionUserData.doNotDisplay)
						{
							SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>> sortedDictionary2;
							if (sortedDictionary.ContainsKey(actionUserData.actionGroup.actionTab))
							{
								sortedDictionary2 = sortedDictionary[actionUserData.actionGroup.actionTab];
							}
							else
							{
								sortedDictionary2 = new SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>();
								sortedDictionary.Add(actionUserData.actionGroup.actionTab, sortedDictionary2);
							}
							List<PlayerAction> list;
							if (sortedDictionary2.ContainsKey(actionUserData.actionGroup))
							{
								list = sortedDictionary2[actionUserData.actionGroup];
							}
							else
							{
								list = new List<PlayerAction>();
								sortedDictionary2.Add(actionUserData.actionGroup, list);
							}
							list.Add(playerAction);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
			}
		}
		int num = 1;
		foreach (KeyValuePair<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>> keyValuePair in sortedDictionary)
		{
			this.tabs.SetTabCaption(num, Localization.Get(keyValuePair.Key.tabNameKey, false));
			List<XUiC_KeyboardBindingEntry> list2 = new List<XUiC_KeyboardBindingEntry>((this.tabs.GetTabRect(num).Controller.GetChildById("controlsGrid").ViewComponent as XUiV_Grid).Controller.GetChildrenByType<XUiC_KeyboardBindingEntry>(null));
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<PlayerActionData.ActionGroup, List<PlayerAction>> keyValuePair2 in keyValuePair.Value)
			{
				if (num2 > 0)
				{
					list2[num3].Hide();
					num3++;
					list2[num3].Hide();
					num3++;
				}
				num2++;
				int num4 = 0;
				foreach (PlayerAction action in keyValuePair2.Value)
				{
					this.createControl(list2[num3], action, num4);
					num3++;
					num4++;
				}
				if (num4 % 2 != 0)
				{
					list2[num3].Hide();
					num3++;
				}
			}
			num++;
			foreach (XUiC_KeyboardBindingEntry xuiC_KeyboardBindingEntry in list2)
			{
				if (xuiC_KeyboardBindingEntry.action == null)
				{
					xuiC_KeyboardBindingEntry.Hide();
				}
			}
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createControl(XUiC_KeyboardBindingEntry _entry, PlayerAction _action, int _controlNum)
	{
		PlayerActionData.ActionUserData actionUserData = (PlayerActionData.ActionUserData)_action.UserData;
		_entry.SetAction(_action);
		this.actionToValueLabel.Add(_action, _entry.value.Label);
		if (actionUserData.allowRebind)
		{
			_entry.button.Controller.OnPress += this.newBindingClick;
			_entry.unbind.Controller.OnPress += this.unbindButtonClick;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator updateActionBindingLabelsLater()
	{
		yield return null;
		this.updateActionBindingLabels();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateActionBindingLabels()
	{
		foreach (KeyValuePair<PlayerAction, UILabel> keyValuePair in this.actionToValueLabel)
		{
			keyValuePair.Value.text = keyValuePair.Key.GetBindingString(false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newBindingClick(XUiController _sender, int _mouseButton)
	{
		XUiC_KeyboardBindingEntry parentByType = _sender.GetParentByType<XUiC_KeyboardBindingEntry>();
		this.closedForNewBinding = true;
		this.btnApply.Enabled = true;
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		XUiC_OptionsControlsNewBinding.GetNewBinding(base.xui, parentByType.action, this.windowGroup.ID, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unbindButtonClick(XUiController _sender, int _mouseButton)
	{
		_sender.GetParentByType<XUiC_KeyboardBindingEntry>().action.UnbindBindingsOfType(false);
		ThreadManager.StartCoroutine(this.updateActionBindingLabelsLater());
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		GamePrefs.Set(EnumGamePrefs.OptionsLookSensitivity, (float)this.comboLookSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsZoomSensitivity, (float)this.comboZoomSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsZoomAccel, (float)this.comboZoomAccel.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsVehicleLookSensitivity, (float)this.comboVehicleSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsWeaponAiming, this.comboWeaponAiming.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsInvertMouse, this.comboInvertMouseLookY.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControlsSprintLock, this.comboSprintLock.Value);
		GameOptionsManager.SaveControls();
		GamePrefs.Instance.Save();
		PlayerMoveController.UpdateControlsOptions();
		this.storeCurrentBindings();
		Action onSettingsChanged = XUiC_OptionsControls.OnSettingsChanged;
		if (onSettingsChanged == null)
		{
			return;
		}
		onSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void storeCurrentBindings()
	{
		this.actionBindingsOnOpen.Clear();
		foreach (PlayerActionSet playerActionSet in PlatformManager.NativePlatform.Input.ActionSets)
		{
			this.actionBindingsOnOpen.Add(playerActionSet.Save());
		}
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		if (!this.initialized)
		{
			this.createControlsEntries();
			this.initialized = true;
		}
		if (!this.closedForNewBinding)
		{
			this.updateOptions();
			this.storeCurrentBindings();
		}
		this.closedForNewBinding = false;
		this.updateActionBindingLabels();
		base.OnOpen();
		this.RefreshApplyLabel();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!this.closedForNewBinding)
		{
			PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(this.actionBindingsOnOpen);
			this.btnApply.Enabled = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
		if (this.btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			this.BtnApply_OnPressed(null, 0);
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLookSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboZoomAccel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboVehicleSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboWeaponAiming;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboInvertMouseLookY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSprintLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlayerAction, UILabel> actionToValueLabel = new Dictionary<PlayerAction, UILabel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closedForNewBinding;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> actionBindingsOnOpen = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_OptionsControls.ControllerLabelMapping> labelsForControllers = new List<XUiC_OptionsControls.ControllerLabelMapping>();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetIngame;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetVehicles;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ControllerLabelMapping
	{
		public ControllerLabelMapping(string[] _controlTypeNames, XUiV_Label[] _labels, InputControlType[] _controlTypes = null)
		{
			this.ControlTypeNames = _controlTypeNames;
			if (_controlTypes == null)
			{
				this.ControlTypes = new InputControlType[_controlTypeNames.Length];
				for (int i = 0; i < _controlTypeNames.Length; i++)
				{
					this.ControlTypes[i] = EnumUtils.Parse<InputControlType>(_controlTypeNames[i], true);
				}
			}
			else
			{
				this.ControlTypes = _controlTypes;
			}
			this.Labels = _labels;
		}

		public ControllerLabelMapping(string _controlTypeName, XUiV_Label[] _labels)
		{
			this.ControlTypeNames = new string[]
			{
				_controlTypeName
			};
			this.ControlTypes = new InputControlType[]
			{
				EnumUtils.Parse<InputControlType>(_controlTypeName, true)
			};
			this.Labels = _labels;
		}

		public readonly string[] ControlTypeNames;

		public readonly InputControlType[] ControlTypes;

		public readonly XUiV_Label[] Labels;
	}
}
