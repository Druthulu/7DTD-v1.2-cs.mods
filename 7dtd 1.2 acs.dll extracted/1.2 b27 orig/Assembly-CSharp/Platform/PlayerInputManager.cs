using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InControl;
using UnityEngine;

namespace Platform
{
	public class PlayerInputManager
	{
		public PlayerInputManager()
		{
			Log.Out("Starting PlayerInputManager...");
			MouseBindingSource.ScaleX = (MouseBindingSource.ScaleY = (MouseBindingSource.ScaleZ = 0.2f));
			GameObject gameObject = GameObject.Find("Input");
			if (gameObject == null)
			{
				gameObject = new GameObject("Input");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			InControlManager inControlManager = gameObject.GetComponent<InControlManager>();
			if (inControlManager != null)
			{
				Log.Error("InControl already instantiated");
				return;
			}
			bool flag = GameUtils.GetLaunchArgument("noxinput") == null;
			bool enableNativeInput = GameUtils.GetLaunchArgument("disablenativeinput") == null;
			if (GameManager.IsDedicatedServer)
			{
				flag = false;
				enableNativeInput = false;
			}
			gameObject.SetActive(false);
			inControlManager = gameObject.AddComponent<InControlManager>();
			inControlManager.logDebugInfo = false;
			inControlManager.suspendInBackground = true;
			inControlManager.nativeInputPreventSleep = true;
			inControlManager.enableNativeInput = enableNativeInput;
			inControlManager.enableXInput = flag;
			inControlManager.nativeInputEnableXInput = flag;
			InputManager.AddCustomDeviceManagers += delegate(ref bool enableUnityInput)
			{
			};
			InControl.Logger.OnLogMessage += delegate(LogMessage _message)
			{
				switch (_message.Type)
				{
				case LogMessageType.Info:
					Log.Out(_message.Text);
					return;
				case LogMessageType.Warning:
					Log.Warning(_message.Text);
					return;
				case LogMessageType.Error:
					Log.Error(_message.Text);
					return;
				default:
					return;
				}
			};
			InControl.Logger.LogInfo(string.Concat(new string[]
			{
				"InControl (version ",
				InputManager.Version.ToString(),
				", native module = ",
				inControlManager.enableNativeInput.ToString(),
				", XInput = ",
				inControlManager.enableXInput.ToString(),
				")"
			}));
			gameObject.SetActive(true);
			PlayerActionsGlobal.Init();
			if (!Submission.Enabled)
			{
				this.actionSets.Add(PlayerActionsGlobal.Instance);
			}
			this.PrimaryPlayer = new PlayerActionsLocal();
			this.actionSets.Add(this.PrimaryPlayer);
			this.actionSets.Add(this.PrimaryPlayer.VehicleActions);
			this.actionSets.Add(this.PrimaryPlayer.GUIActions);
			this.actionSets.Add(this.PrimaryPlayer.PermanentActions);
			this.ActionSets = new ReadOnlyCollection<PlayerActionsBase>(this.actionSets);
			for (int i = 0; i < this.actionSets.Count; i++)
			{
				PlayerActionSet actionSet = this.actionSets[i];
				actionSet.OnLastInputTypeChanged += delegate(BindingSourceType _type)
				{
					if (_type == BindingSourceType.DeviceBindingSource)
					{
						this.newInputDevice = (actionSet.Device ?? InputManager.ActiveDevice);
						return;
					}
					this.newInputDevice = InputDevice.Null;
				};
			}
			if (!GameManager.IsDedicatedServer)
			{
				this.ActionSetManager.Push(this.PrimaryPlayer);
			}
			this.CurrentInputStyle = this.defaultInputStyle;
		}

		public void Update()
		{
			BindingSourceType bindingSourceType;
			this.newInputDevice = this.LastActiveInputDevice(out bindingSourceType);
			if (!this.firstInputDetected && bindingSourceType == BindingSourceType.None)
			{
				return;
			}
			if (this.lastInputDevice == this.newInputDevice)
			{
				if (bindingSourceType == this.lastBindingSource)
				{
					return;
				}
				if (bindingSourceType == BindingSourceType.KeyBindingSource && this.lastBindingSource == BindingSourceType.MouseBindingSource)
				{
					return;
				}
				if (bindingSourceType == BindingSourceType.MouseBindingSource && this.lastBindingSource == BindingSourceType.KeyBindingSource)
				{
					return;
				}
			}
			if (!this.firstInputDetected)
			{
				this.firstInputDetected = true;
			}
			this.lastInputDevice = this.newInputDevice;
			this.lastBindingSource = bindingSourceType;
			PlayerInputManager.InputStyle currentInputStyle = this.CurrentInputStyle;
			if (bindingSourceType == BindingSourceType.KeyBindingSource || bindingSourceType == BindingSourceType.MouseBindingSource)
			{
				this.lastInputDeviceName = null;
				this.CurrentInputStyle = PlayerInputManager.InputStyle.Keyboard;
			}
			else if (this.lastInputDevice.Name == "None" || bindingSourceType == BindingSourceType.None)
			{
				this.lastInputDeviceName = null;
				this.CurrentInputStyle = this.defaultInputStyle;
			}
			else
			{
				string name = this.lastInputDevice.Name;
				if (name != this.lastInputDeviceName)
				{
					this.lastInputDeviceName = name;
					this.CurrentInputStyle = ((this.lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation2 || this.lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation3 || this.lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation4 || this.lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation5) ? PlayerInputManager.InputStyle.PS4 : PlayerInputManager.InputStyle.XB1);
				}
			}
			if (currentInputStyle != this.CurrentInputStyle && currentInputStyle != PlayerInputManager.InputStyle.Undefined)
			{
				float unscaledTime = Time.unscaledTime;
				this.inputStylesUsedMinutes[(int)currentInputStyle] += (unscaledTime - this.lastInputStyleSwitchTime) / 60f;
				this.lastInputStyleSwitchTime = unscaledTime;
			}
			Action<PlayerInputManager.InputStyle> onLastInputStyleChanged = this.OnLastInputStyleChanged;
			if (onLastInputStyleChanged == null)
			{
				return;
			}
			onLastInputStyleChanged(this.CurrentInputStyle);
		}

		public void ForceInputStyleChange()
		{
			Action<PlayerInputManager.InputStyle> onLastInputStyleChanged = this.OnLastInputStyleChanged;
			if (onLastInputStyleChanged == null)
			{
				return;
			}
			onLastInputStyleChanged(this.CurrentInputStyle);
		}

		public event Action<PlayerInputManager.InputStyle> OnLastInputStyleChanged;

		public PlayerInputManager.InputStyle CurrentInputStyle
		{
			get
			{
				return this._currentInputStyle;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				if (value == PlayerInputManager.InputStyle.PS4 || value == PlayerInputManager.InputStyle.XB1)
				{
					this.CurrentControllerInputStyle = value;
				}
				this._currentInputStyle = value;
			}
		}

		public PlayerInputManager.InputStyle CurrentControllerInputStyle
		{
			get
			{
				if (DeviceFlag.PS5.IsCurrent())
				{
					return PlayerInputManager.InputStyle.PS4;
				}
				if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
				{
					return PlayerInputManager.InputStyle.XB1;
				}
				return this._currentControllerInputStyle;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				this._currentControllerInputStyle = value;
			}
		}

		public ReadOnlyCollection<PlayerActionsBase> ActionSets { get; }

		[PublicizedFrom(EAccessModifier.Private)]
		public InputDevice LastActiveInputDevice(out BindingSourceType lastBindingSource)
		{
			ulong num = 0UL;
			PlayerActionSet playerActionSet = null;
			lastBindingSource = BindingSourceType.None;
			for (int i = 0; i < this.ActionSets.Count; i++)
			{
				PlayerActionSet playerActionSet2 = this.ActionSets[i];
				if (playerActionSet2.Enabled && playerActionSet2.LastInputTypeChangedTick > num)
				{
					playerActionSet = playerActionSet2;
					num = playerActionSet2.LastInputTypeChangedTick;
				}
			}
			if (playerActionSet != null)
			{
				lastBindingSource = playerActionSet.LastInputType;
				if (playerActionSet.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					return playerActionSet.Device ?? InputManager.ActiveDevice;
				}
			}
			return InputDevice.Null;
		}

		public void ResetInputStyleUsage()
		{
			for (int i = 0; i < this.inputStylesUsedMinutes.Length; i++)
			{
				this.inputStylesUsedMinutes[i] = 0f;
				this.lastInputStyleSwitchTime = Time.unscaledTime;
			}
		}

		public PlayerInputManager.InputStyle MostUsedInputStyle()
		{
			if (this.CurrentInputStyle != PlayerInputManager.InputStyle.Undefined)
			{
				float unscaledTime = Time.unscaledTime;
				this.inputStylesUsedMinutes[(int)this.CurrentInputStyle] += (unscaledTime - this.lastInputStyleSwitchTime) / 60f;
				this.lastInputStyleSwitchTime = unscaledTime;
			}
			PlayerInputManager.InputStyle result = PlayerInputManager.InputStyle.Count;
			float num = -1f;
			for (int i = 0; i < this.inputStylesUsedMinutes.Length; i++)
			{
				if (this.inputStylesUsedMinutes[i] > num)
				{
					num = this.inputStylesUsedMinutes[i];
					result = (PlayerInputManager.InputStyle)i;
				}
			}
			return result;
		}

		public PlayerActionsBase GetActionSetForName(string _name)
		{
			foreach (PlayerActionsBase playerActionsBase in this.ActionSets)
			{
				if (playerActionsBase.Name.EqualsCaseInsensitive(_name))
				{
					return playerActionsBase;
				}
			}
			return null;
		}

		public PlayerActionsLocal PrimaryPlayer { get; }

		public void LoadActionSetsFromStrings(IList<string> actionSets)
		{
			if (this.ActionSets.Count != actionSets.Count)
			{
				Log.Warning(string.Format("Loading ActionSets from string array with incorrect length. Expected: {0}. Actual: {1}.", this.ActionSets.Count, (actionSets != null) ? new int?(actionSets.Count) : null));
				return;
			}
			for (int i = 0; i < this.ActionSets.Count; i++)
			{
				this.ActionSets[i].Load(actionSets[i]);
			}
		}

		public static PlayerInputManager.InputStyle InputStyleFromSelectedIconStyle()
		{
			PlayerInputManager.ControllerIconStyle @int = (PlayerInputManager.ControllerIconStyle)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerIconStyle);
			if (@int == PlayerInputManager.ControllerIconStyle.Xbox)
			{
				return PlayerInputManager.InputStyle.XB1;
			}
			if (@int != PlayerInputManager.ControllerIconStyle.Playstation)
			{
				IPlatform nativePlatform = PlatformManager.NativePlatform;
				PlayerInputManager.InputStyle? inputStyle;
				if (nativePlatform == null)
				{
					inputStyle = null;
				}
				else
				{
					PlayerInputManager input = nativePlatform.Input;
					inputStyle = ((input != null) ? new PlayerInputManager.InputStyle?(input.CurrentControllerInputStyle) : null);
				}
				PlayerInputManager.InputStyle? inputStyle2 = inputStyle;
				return inputStyle2.GetValueOrDefault();
			}
			return PlayerInputManager.InputStyle.PS4;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerInputManager.InputStyle defaultInputStyle = PlayerInputManager.InputStyle.Keyboard;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool firstInputDetected;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly float[] inputStylesUsedMinutes = new float[4];

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastInputStyleSwitchTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public string lastInputDeviceName;

		[PublicizedFrom(EAccessModifier.Private)]
		public InputDevice lastInputDevice;

		[PublicizedFrom(EAccessModifier.Private)]
		public InputDevice newInputDevice;

		[PublicizedFrom(EAccessModifier.Private)]
		public BindingSourceType lastBindingSource;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerInputManager.InputStyle _currentInputStyle;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerInputManager.InputStyle _currentControllerInputStyle = PlayerInputManager.InputStyle.XB1;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<PlayerActionsBase> actionSets = new List<PlayerActionsBase>();

		public readonly ActionSetManager ActionSetManager = new ActionSetManager();

		public enum InputStyle
		{
			Undefined,
			Keyboard,
			PS4,
			XB1,
			Count
		}

		public enum ControllerIconStyle
		{
			Automatic,
			Xbox,
			Playstation
		}
	}
}
