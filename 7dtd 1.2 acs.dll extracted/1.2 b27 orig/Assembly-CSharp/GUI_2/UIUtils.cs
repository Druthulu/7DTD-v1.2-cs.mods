using System;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;

namespace GUI_2
{
	public static class UIUtils
	{
		public static UIAtlas IconAtlas
		{
			get
			{
				return UIUtils.symbolAtlas;
			}
		}

		public static string GetSpriteName(UIUtils.ButtonIcon _icon)
		{
			if (PlayerInputManager.InputStyleFromSelectedIconStyle() == PlayerInputManager.InputStyle.PS4)
			{
				return "PS5_" + UIUtils.buttonIconMap[_icon];
			}
			return "XB_" + UIUtils.buttonIconMap[_icon];
		}

		public static UIUtils.ButtonIcon GetButtonIconForAction(PlayerAction _action)
		{
			if (_action == null)
			{
				return UIUtils.ButtonIcon.None;
			}
			DeviceBindingSource deviceBindingSource = _action.GetBindingOfType(true) as DeviceBindingSource;
			if (deviceBindingSource == null)
			{
				Log.Warning("UIUtils: No device binding source could be found for PlayerAction {0}", new object[]
				{
					_action.Name
				});
				return UIUtils.ButtonIcon.None;
			}
			UIUtils.ButtonIcon result;
			if (UIUtils.iconControlMap.TryGetValue(deviceBindingSource.Control, out result))
			{
				return result;
			}
			Log.Warning("UIUtils: Could not assign a ButtonIcon for device control {0}", new object[]
			{
				deviceBindingSource.Control.ToString()
			});
			return UIUtils.ButtonIcon.None;
		}

		public static void LoadAtlas()
		{
			UIUtils.symbolAtlas = Resources.Load<UIAtlas>("GUI/Prefabs/SymbolAtlas");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static UIAtlas symbolAtlas;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string sprite_PS = "PS5_";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string sprite_XB = "XB_";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<UIUtils.ButtonIcon, string> buttonIconMap = new EnumDictionary<UIUtils.ButtonIcon, string>
		{
			{
				UIUtils.ButtonIcon.FaceButtonSouth,
				"Button_Action1"
			},
			{
				UIUtils.ButtonIcon.FaceButtonNorth,
				"Button_Action4"
			},
			{
				UIUtils.ButtonIcon.FaceButtonEast,
				"Button_Action2"
			},
			{
				UIUtils.ButtonIcon.FaceButtonWest,
				"Button_Action3"
			},
			{
				UIUtils.ButtonIcon.ConfirmButton,
				"Button_Action1"
			},
			{
				UIUtils.ButtonIcon.CancelButton,
				"Button_Action2"
			},
			{
				UIUtils.ButtonIcon.LeftBumper,
				"Button_LeftBumper"
			},
			{
				UIUtils.ButtonIcon.LeftTrigger,
				"Button_LeftTrigger"
			},
			{
				UIUtils.ButtonIcon.RightBumper,
				"Button_RightBumper"
			},
			{
				UIUtils.ButtonIcon.RightTrigger,
				"Button_RightTrigger"
			},
			{
				UIUtils.ButtonIcon.LeftStick,
				"Button_LeftStick"
			},
			{
				UIUtils.ButtonIcon.LeftStickUpDown,
				"Button_LeftStickUpDown"
			},
			{
				UIUtils.ButtonIcon.LeftStickLeftRight,
				"Button_LeftStickLeftRight"
			},
			{
				UIUtils.ButtonIcon.LeftStickButton,
				"Button_LeftStickButton"
			},
			{
				UIUtils.ButtonIcon.RightStick,
				"Button_RightStick"
			},
			{
				UIUtils.ButtonIcon.RightStickUpDown,
				"Button_RightStickUpDown"
			},
			{
				UIUtils.ButtonIcon.RightStickLeftRight,
				"Button_RightStickLeftRight"
			},
			{
				UIUtils.ButtonIcon.RightStickButton,
				"Button_RightStickButton"
			},
			{
				UIUtils.ButtonIcon.DPadLeft,
				"Button_DPadLeft"
			},
			{
				UIUtils.ButtonIcon.DPadRight,
				"Button_DPadRight"
			},
			{
				UIUtils.ButtonIcon.DPadUp,
				"Button_DPadUp"
			},
			{
				UIUtils.ButtonIcon.DPadDown,
				"Button_DPadDown"
			},
			{
				UIUtils.ButtonIcon.StartButton,
				"Button_Start"
			},
			{
				UIUtils.ButtonIcon.BackButton,
				"Button_Back"
			},
			{
				UIUtils.ButtonIcon.None,
				""
			}
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<InputControlType, UIUtils.ButtonIcon> iconControlMap = new EnumDictionary<InputControlType, UIUtils.ButtonIcon>
		{
			{
				InputControlType.Action1,
				UIUtils.ButtonIcon.FaceButtonSouth
			},
			{
				InputControlType.Action2,
				UIUtils.ButtonIcon.FaceButtonEast
			},
			{
				InputControlType.Action3,
				UIUtils.ButtonIcon.FaceButtonWest
			},
			{
				InputControlType.Action4,
				UIUtils.ButtonIcon.FaceButtonNorth
			},
			{
				InputControlType.LeftBumper,
				UIUtils.ButtonIcon.LeftBumper
			},
			{
				InputControlType.RightBumper,
				UIUtils.ButtonIcon.RightBumper
			},
			{
				InputControlType.LeftTrigger,
				UIUtils.ButtonIcon.LeftTrigger
			},
			{
				InputControlType.RightTrigger,
				UIUtils.ButtonIcon.RightTrigger
			},
			{
				InputControlType.LeftStickButton,
				UIUtils.ButtonIcon.LeftStickButton
			},
			{
				InputControlType.RightStickButton,
				UIUtils.ButtonIcon.RightStickButton
			},
			{
				InputControlType.DPadUp,
				UIUtils.ButtonIcon.DPadUp
			},
			{
				InputControlType.DPadDown,
				UIUtils.ButtonIcon.DPadDown
			},
			{
				InputControlType.DPadLeft,
				UIUtils.ButtonIcon.DPadLeft
			},
			{
				InputControlType.DPadRight,
				UIUtils.ButtonIcon.DPadRight
			},
			{
				InputControlType.Start,
				UIUtils.ButtonIcon.StartButton
			},
			{
				InputControlType.Menu,
				UIUtils.ButtonIcon.StartButton
			},
			{
				InputControlType.Options,
				UIUtils.ButtonIcon.StartButton
			},
			{
				InputControlType.Plus,
				UIUtils.ButtonIcon.StartButton
			},
			{
				InputControlType.Select,
				UIUtils.ButtonIcon.BackButton
			},
			{
				InputControlType.View,
				UIUtils.ButtonIcon.BackButton
			},
			{
				InputControlType.TouchPadButton,
				UIUtils.ButtonIcon.BackButton
			},
			{
				InputControlType.Minus,
				UIUtils.ButtonIcon.BackButton
			},
			{
				InputControlType.None,
				UIUtils.ButtonIcon.None
			}
		};

		public enum ButtonIcon
		{
			FaceButtonSouth,
			FaceButtonNorth,
			FaceButtonEast,
			FaceButtonWest,
			ConfirmButton,
			CancelButton,
			LeftBumper,
			RightBumper,
			LeftTrigger,
			RightTrigger,
			LeftStick,
			LeftStickUpDown,
			LeftStickLeftRight,
			LeftStickButton,
			RightStick,
			RightStickUpDown,
			RightStickLeftRight,
			RightStickButton,
			DPadLeft,
			DPadRight,
			DPadUp,
			DPadDown,
			StartButton,
			BackButton,
			None,
			Count
		}
	}
}
