using System;
using System.Collections.Generic;
using InControl;
using Platform;
using TriggerEffects;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

[UnityEngine.Scripting.Preserve]
public class TriggerEffectManager : IDisposable
{
	public bool Enabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this._enabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this._enabled = value;
			this._stateChanged = true;
		}
	}

	public TriggerEffectManager()
	{
		this.PollSetting();
		this._stateChanged = false;
		try
		{
			TriggerEffectDualsensePC.InitTriggerEffectManager(ref this._controllersConnected);
		}
		catch (DllNotFoundException arg)
		{
			Log.Warning(string.Format("[TriggerEffectManager] Failed to load ControllerExt, disabling. Details: {0}", arg));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerTriggerEffects, false);
			return;
		}
		TriggerEffectDualsense.InitTriggerEffectManager();
		for (int i = 0; i < this.vibrationAudioSources.Length; i++)
		{
			this.vibrationAudioSources[i] = new AudioGamepadRumbleSource();
		}
		TriggerEffectManager.UpdateControllerVibrationStrength();
		this.EnableVibration();
		this.InitializeLightbarGradient();
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
	}

	public void EnableVibration()
	{
		TriggerEffectDualsensePC.EnableVibration();
		TriggerEffectDualsense.EnableVibration();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeLightbarGradient()
	{
		TriggerEffectManager.lightbarGradients = Resources.Load<LightbarGradients>("Data/LightBarGradients");
	}

	public static void SetEnabled(bool _enabled)
	{
		GameManager.Instance.triggerEffectManager.Enabled = _enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte FloatToByte(float value)
	{
		return (byte)(Mathf.Clamp01(value) * 255f);
	}

	public void Update()
	{
		if (GameManager.Instance.World != null && TriggerEffectManager.audioRumbleStrength > 0f && !GameManager.Instance.IsPaused() && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			int num = 0;
			int num2 = 0;
			this.targetLeftAudioStrength = 0f;
			this.targetRightAudioStrength = 0f;
			Transform transform;
			if (GameManager.Instance.World != null)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				transform = ((primaryPlayer != null) ? primaryPlayer.cameraTransform : null);
			}
			else
			{
				transform = LocalPlayerUI.primaryUI.uiCamera.transform;
			}
			foreach (AudioGamepadRumbleSource audioGamepadRumbleSource in this.vibrationAudioSources)
			{
				if (audioGamepadRumbleSource.audioSrc != null)
				{
					if (audioGamepadRumbleSource.audioSrc.isPlaying)
					{
						float num3 = audioGamepadRumbleSource.GetSample(0);
						if (audioGamepadRumbleSource.locationBased)
						{
							float num4 = 1f - Vector3.Distance(transform.position, audioGamepadRumbleSource.audioSrc.transform.position) / audioGamepadRumbleSource.audioSrc.maxDistance;
							if (num4 < 0.9f)
							{
								goto IL_184;
							}
							num3 *= num4;
						}
						num3 *= audioGamepadRumbleSource.strengthMultiplier * audioGamepadRumbleSource.audioSrc.pitch * TriggerEffectManager.audioRumbleStrength;
						if (num3 > 0f)
						{
							num2++;
							this.targetRightAudioStrength += num3;
						}
						else if (num3 < 0f)
						{
							num++;
							this.targetLeftAudioStrength += num3;
						}
					}
					else
					{
						audioGamepadRumbleSource.Clear();
					}
				}
				IL_184:;
			}
			if (num > 0 || num2 > 0)
			{
				if (num > 0)
				{
					this.targetLeftAudioStrength /= (float)num;
				}
				if (num2 > 0)
				{
					this.targetRightAudioStrength /= (float)num2;
				}
				this.leftAudioStrength = Mathf.Lerp(this.leftAudioStrength, this.targetLeftAudioStrength, Time.deltaTime * 15f);
				this.rightAudioStrength = Mathf.Lerp(this.rightAudioStrength, this.targetRightAudioStrength, Time.deltaTime * 15f);
				InputManager.ActiveDevice.Vibrate(-this.leftAudioStrength, this.rightAudioStrength);
				this.SetGamepadVibration(-this.leftAudioStrength + this.rightAudioStrength);
				this.SetDualSenseVibration(this.FloatToByte(-this.leftAudioStrength * 0.25f), this.FloatToByte(this.rightAudioStrength * 0.25f));
			}
			else
			{
				this.SetDualSenseVibration(0, 0);
				InputManager.ActiveDevice.StopVibration();
				this.SetGamepadVibration(0f);
			}
		}
		TriggerEffectDualsensePC.PCConnectedUpdate(ref this._controllersConnected, this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect);
		TriggerEffectDualsense.ConnectedUpdate();
		if (this.Enabled && !this.inUI)
		{
			TriggerEffectDualsensePC.PCTriggerUpdate(this._stateChanged, this._triggerSetLeft, this._triggerSetRight, this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect, this._controllersConnected);
			TriggerEffectDualsense.Update(this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect);
		}
		else if (this._stateChanged && (!this.Enabled || this.inUI))
		{
			this.SetGamepadTriggerEffectOff(TriggerEffectManager.GamepadTrigger.RightTrigger);
			this.SetGamepadTriggerEffectOff(TriggerEffectManager.GamepadTrigger.LeftTrigger);
			TriggerEffectDualsensePC.DualsensePCSetEffectToOff(true, true);
			this._stateChanged = true;
		}
		this._triggerSetLeft = false;
		this._triggerSetRight = false;
		this._stateChanged = false;
	}

	public void SetGamepadTriggerEffectOff(TriggerEffectManager.GamepadTrigger trigger)
	{
		if (trigger != TriggerEffectManager.GamepadTrigger.LeftTrigger)
		{
			if (trigger != TriggerEffectManager.GamepadTrigger.RightTrigger)
			{
				throw new ArgumentOutOfRangeException("trigger", trigger, null);
			}
			this._currentEffectRight = TriggerEffectManager.NoneEffect;
		}
		else
		{
			this._currentEffectLeft = TriggerEffectManager.NoneEffect;
		}
		this._stateChanged = true;
	}

	public void SetWeaponEffect(int userID, TriggerEffectManager.GamepadTrigger trigger, byte startPosition, byte endPosition, byte strength)
	{
		if (!this.Enabled)
		{
			return;
		}
		TriggerEffectDualsensePC.SetWeaponEffect(userID, trigger, startPosition, endPosition, strength);
		TriggerEffectDualsense.SetWeaponEffect(userID, trigger, startPosition, endPosition, strength);
		this._stateChanged = true;
	}

	public void ResetControllerIdentification()
	{
		TriggerEffectDualsensePC.ResetControllerIdentification();
		TriggerEffectDualsense.ResetControllerIdentification();
		this._stateChanged = true;
	}

	public void SetControllerIdentification()
	{
		TriggerEffectDualsensePC.SetControllerIdentification();
		TriggerEffectDualsense.SetControllerIdentification();
		this._stateChanged = true;
	}

	public void SetTriggerEffectVibration(int userID, TriggerEffectManager.GamepadTrigger trigger, byte position, byte amplitude, byte frequency)
	{
		if (!this.Enabled)
		{
			return;
		}
		TriggerEffectDualsensePC.SetTriggerEffectVibration(userID, trigger, position, amplitude, frequency);
		TriggerEffectDualsense.SetTriggerEffectVibration(userID, trigger, position, amplitude, frequency);
		this._stateChanged = true;
	}

	public void SetTriggerEffectVibrationMultiplePosition(int userID, TriggerEffectManager.GamepadTrigger trigger, byte[] amplitudes, byte frequency)
	{
		if (!this.Enabled)
		{
			return;
		}
		TriggerEffectDualsensePC.SetTriggerEffectVibrationMultiplePosition(userID, trigger, amplitudes, frequency);
		TriggerEffectDualsense.SetTriggerEffectVibrationMultiplePosition(userID, trigger, amplitudes, frequency);
		this._stateChanged = true;
	}

	public void Shutdown()
	{
		this.StopGamepadVibration();
		TriggerEffectManager.LibShutdown();
		TriggerEffectDualsense.ResetControllerIdentification();
		TriggerEffectDualsensePC.ResetControllerIdentification();
		for (int i = 1; i < 5; i++)
		{
			this._controllersConnected[i - 1] = false;
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		this.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LibShutdown()
	{
		TriggerEffectDualsensePC.LibShutdown();
	}

	public void Dispose()
	{
		this.Shutdown();
	}

	public void PollSetting()
	{
		this.Enabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerTriggerEffects);
	}

	public bool inUI
	{
		get
		{
			return this._inUI;
		}
		set
		{
			if (this._inUI != value)
			{
				this._inUI = value;
				this._stateChanged = true;
			}
		}
	}

	public void SetGamepadVibration(float strength)
	{
		this._currentControllerVibrationStrength = strength;
		this._stateChanged = true;
	}

	public void StopGamepadVibration()
	{
		this._currentControllerVibrationStrength = 0f;
		this._stateChanged = true;
		this.SetDualSenseVibration(0, 0);
		InputManager.ActiveDevice.StopVibration();
	}

	public void SetTriggerEffect(TriggerEffectManager.ControllerTriggerEffect effect)
	{
		if (!this.Enabled)
		{
			return;
		}
		this.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.LeftTrigger, effect, false);
		this.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, effect, false);
	}

	public void SetTriggerEffect(TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.ControllerTriggerEffect effect, bool asap = false)
	{
		if (!this.Enabled)
		{
			return;
		}
		this._stateChanged = true;
		if (trigger == TriggerEffectManager.GamepadTrigger.LeftTrigger)
		{
			this._currentEffectLeft.DualsenseEffect = effect.DualsenseEffect;
			if (asap)
			{
				TriggerEffectDualsensePC.ApplyImmediate(trigger, this._currentEffectLeft);
			}
			this._triggerSetLeft = true;
			return;
		}
		if (trigger != TriggerEffectManager.GamepadTrigger.RightTrigger)
		{
			return;
		}
		this._currentEffectRight.DualsenseEffect = effect.DualsenseEffect;
		if (asap)
		{
			TriggerEffectDualsensePC.ApplyImmediate(trigger, this._currentEffectRight);
		}
		this._triggerSetRight = true;
	}

	public static TriggerEffectManager.ControllerTriggerEffect GetTriggerEffect(ValueTuple<string, string> triggerEffectNames)
	{
		if (string.IsNullOrEmpty(triggerEffectNames.Item1) || string.IsNullOrEmpty(triggerEffectNames.Item2) || (triggerEffectNames.Item1.Contains("NoEffect") && triggerEffectNames.Item2.Contains("NoEffect")) || (triggerEffectNames.Item1.Contains("NoneEffect") && triggerEffectNames.Item2.Contains("NoneEffect")))
		{
			return TriggerEffectManager.NoneEffect;
		}
		TriggerEffectManager.ControllerTriggerEffect result = new TriggerEffectManager.ControllerTriggerEffect
		{
			DualsenseEffect = TriggerEffectManager.NoneEffectDs,
			XboxTriggerEffect = TriggerEffectManager.NoneEffectXb
		};
		TriggerEffectManager.TriggerEffectDS dualsenseEffect;
		if (TriggerEffectManager.ControllerTriggerEffectsDS.TryGetValue(triggerEffectNames.Item1, out dualsenseEffect))
		{
			result.DualsenseEffect = dualsenseEffect;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect DS: " + triggerEffectNames.Item1);
		}
		TriggerEffectManager.TriggerEffectXB xboxTriggerEffect;
		if (TriggerEffectManager.ControllerTriggerEffectsXb.TryGetValue(triggerEffectNames.Item2, out xboxTriggerEffect))
		{
			result.XboxTriggerEffect = xboxTriggerEffect;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect XB: " + triggerEffectNames.Item2);
		}
		return result;
	}

	public static TriggerEffectManager.ControllerTriggerEffect GetTriggerEffect(string dualsenseTrigger, string impulseTrigger)
	{
		if (string.IsNullOrEmpty(dualsenseTrigger) || string.IsNullOrEmpty(impulseTrigger) || (dualsenseTrigger.Contains("NoEffect") && impulseTrigger.Contains("NoEffect")) || (dualsenseTrigger.Contains("NoneEffect") && impulseTrigger.Contains("NoneEffect")))
		{
			return TriggerEffectManager.NoneEffect;
		}
		TriggerEffectManager.ControllerTriggerEffect result = new TriggerEffectManager.ControllerTriggerEffect
		{
			DualsenseEffect = TriggerEffectManager.NoneEffectDs,
			XboxTriggerEffect = TriggerEffectManager.NoneEffectXb
		};
		TriggerEffectManager.TriggerEffectDS dualsenseEffect;
		if (TriggerEffectManager.ControllerTriggerEffectsDS.TryGetValue(dualsenseTrigger, out dualsenseEffect))
		{
			result.DualsenseEffect = dualsenseEffect;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect DS: " + dualsenseTrigger);
		}
		TriggerEffectManager.TriggerEffectXB xboxTriggerEffect;
		if (TriggerEffectManager.ControllerTriggerEffectsXb.TryGetValue(impulseTrigger, out xboxTriggerEffect))
		{
			result.XboxTriggerEffect = xboxTriggerEffect;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect XB: " + impulseTrigger);
		}
		return result;
	}

	public static ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB> GetTriggerEffectAsTuple(ValueTuple<string, string> triggerEffectNames)
	{
		if (string.IsNullOrEmpty(triggerEffectNames.Item1) || string.IsNullOrEmpty(triggerEffectNames.Item2) || (triggerEffectNames.Item1.Contains("NoEffect") && triggerEffectNames.Item2.Contains("NoEffect")) || (triggerEffectNames.Item1.Contains("NoneEffect") && triggerEffectNames.Item2.Contains("NoneEffect")))
		{
			return new ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB>(TriggerEffectManager.NoneEffect.DualsenseEffect, TriggerEffectManager.NoneEffect.XboxTriggerEffect);
		}
		ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB> result = new ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB>(TriggerEffectManager.NoneEffect.DualsenseEffect, TriggerEffectManager.NoneEffect.XboxTriggerEffect);
		TriggerEffectManager.TriggerEffectDS item;
		if (TriggerEffectManager.ControllerTriggerEffectsDS.TryGetValue(triggerEffectNames.Item1, out item))
		{
			result.Item1 = item;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect DS: " + triggerEffectNames.Item1);
		}
		TriggerEffectManager.TriggerEffectXB item2;
		if (TriggerEffectManager.ControllerTriggerEffectsXb.TryGetValue(triggerEffectNames.Item2, out item2))
		{
			result.Item2 = item2;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect XB: " + triggerEffectNames.Item2);
		}
		return result;
	}

	public static bool SettingDefaultValue()
	{
		return Application.platform == RuntimePlatform.PS5 || Application.platform == RuntimePlatform.GameCoreXboxSeries || Application.platform == RuntimePlatform.WindowsEditor;
	}

	public void SetAudioRumbleSource(AudioSource _audioSource, float _strengthMultiplier, bool _locationBased)
	{
		AudioGamepadRumbleSource audioGamepadRumbleSource = null;
		float num = float.MaxValue;
		foreach (AudioGamepadRumbleSource audioGamepadRumbleSource2 in this.vibrationAudioSources)
		{
			if (!(audioGamepadRumbleSource2.audioSrc != null))
			{
				audioGamepadRumbleSource2.SetAudioSource(_audioSource, _strengthMultiplier, _locationBased);
				return;
			}
			if (audioGamepadRumbleSource2.timeAdded < num)
			{
				audioGamepadRumbleSource = audioGamepadRumbleSource2;
				num = audioGamepadRumbleSource2.timeAdded;
			}
		}
		if (audioGamepadRumbleSource != null)
		{
			audioGamepadRumbleSource.SetAudioSource(_audioSource, _strengthMultiplier, _locationBased);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDualSenseVibration(byte _smallMotor, byte _largeMotor)
	{
		TriggerEffectDualsense.SetDualSenseVibration(_smallMotor, _largeMotor);
		TriggerEffectDualsensePC.SetDualSenseVibration(_smallMotor, _largeMotor);
	}

	public static void UpdateControllerVibrationStrength()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsControllerVibrationStrength))
		{
		case 0:
			TriggerEffectManager.audioRumbleStrength = 0f;
			return;
		case 1:
			TriggerEffectManager.audioRumbleStrength = 0.5f;
			return;
		case 2:
			TriggerEffectManager.audioRumbleStrength = 1f;
			return;
		case 3:
			TriggerEffectManager.audioRumbleStrength = 2f;
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		if (_style == PlayerInputManager.InputStyle.Keyboard)
		{
			this.StopGamepadVibration();
		}
	}

	public static void UpdateDualSenseLightFromWeather(WeatherManager.BiomeWeather weather)
	{
		if (TriggerEffectManager.lightbarGradients == null || weather == null)
		{
			return;
		}
		float num = GameManager.Instance.World.GetWorldTime() % 24000UL;
		float num2 = SkyManager.GetDawnTime() * 1000f;
		float num3 = SkyManager.GetDuskTime() * 1000f;
		bool flag = num < num2 || num > num3;
		float time;
		if (flag)
		{
			float num4 = 24000f - num3;
			float num5 = num4 + num2;
			if (num >= num3 && num < 24000f)
			{
				time = (num - num3) / num5;
			}
			else
			{
				time = num4 / num5 + num / (num2 + num4);
			}
		}
		else
		{
			float num6 = num3 - num2;
			time = (num - num2) / num6;
		}
		Color dualSenseLightbarColor;
		if (SkyManager.BloodMoonVisiblePercent() == 1f)
		{
			float time2 = (1f + Mathf.Sin(Time.time)) / 2f;
			dualSenseLightbarColor = TriggerEffectManager.lightbarGradients.bloodmoonGradient.Evaluate(time2);
		}
		else if (weather.rainParam.value >= 0.5f || (weather.biomeDefinition != null && (weather.biomeDefinition.m_BiomeType == BiomeDefinition.BiomeType.Wasteland || weather.biomeDefinition.m_BiomeType == BiomeDefinition.BiomeType.burnt_forest)))
		{
			if (flag)
			{
				dualSenseLightbarColor = TriggerEffectManager.lightbarGradients.cloudNightGradient.Evaluate(time);
			}
			else
			{
				dualSenseLightbarColor = TriggerEffectManager.lightbarGradients.cloudDayGradient.Evaluate(time);
			}
		}
		else if (flag)
		{
			dualSenseLightbarColor = TriggerEffectManager.lightbarGradients.nightGradient.Evaluate(time);
		}
		else
		{
			dualSenseLightbarColor = TriggerEffectManager.lightbarGradients.dayGradient.Evaluate(time);
		}
		TriggerEffectManager.SetDualSenseLightbarColor(dualSenseLightbarColor);
	}

	public static void SetDualSenseLightbarColor(Color color)
	{
		for (int i = 1; i < 5; i++)
		{
			TriggerEffectDualsense.SetLightbar(i, (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
			TriggerEffectDualsensePC.SetLightbar(i, (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
		}
	}

	public static void SetMainMenuLightbarColor()
	{
		TriggerEffectManager.SetDualSenseLightbarColor(TriggerEffectManager.lightbarGradients.mainMenuColor);
	}

	public static readonly Dictionary<string, TriggerEffectManager.TriggerEffectDS> ControllerTriggerEffectsDS = new Dictionary<string, TriggerEffectManager.TriggerEffectDS>();

	public static readonly Dictionary<string, TriggerEffectManager.TriggerEffectXB> ControllerTriggerEffectsXb = new Dictionary<string, TriggerEffectManager.TriggerEffectXB>();

	[PublicizedFrom(EAccessModifier.Internal)]
	public static readonly TriggerEffectManager.TriggerEffectDS NoneEffectDs = new TriggerEffectManager.TriggerEffectDS
	{
		Effect = TriggerEffectManager.EffectDualsense.Off,
		AmplitudeEndStrength = 0,
		Frequency = 0,
		Position = 0,
		EndPosition = 0,
		Strength = 0,
		Strengths = Array.Empty<byte>()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TriggerEffectManager.TriggerEffectXB NoneEffectXb = new TriggerEffectManager.TriggerEffectXB
	{
		Effect = TriggerEffectManager.EffectXbox.Off,
		Strength = 0f
	};

	public static readonly TriggerEffectManager.ControllerTriggerEffect NoneEffect = new TriggerEffectManager.ControllerTriggerEffect
	{
		DualsenseEffect = TriggerEffectManager.NoneEffectDs,
		XboxTriggerEffect = TriggerEffectManager.NoneEffectXb
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] _controllersConnected = new bool[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public float _currentControllerVibrationStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _triggerSetLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _triggerSetRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _stateChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public TriggerEffectManager.ControllerTriggerEffect _currentEffectLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public TriggerEffectManager.ControllerTriggerEffect _currentEffectRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthSubtle = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthStandard = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthStrong = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDualSenseRumbleStrengthMultiplier = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float audioRumbleStrength = 1f;

	public AudioGamepadRumbleSource[] vibrationAudioSources = new AudioGamepadRumbleSource[5];

	[PublicizedFrom(EAccessModifier.Protected)]
	public static LightbarGradients lightbarGradients;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leftAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rightAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetLeftAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetRightAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _inUI;

	public enum GamepadTrigger
	{
		LeftTrigger,
		RightTrigger
	}

	public enum EffectDualsense
	{
		Off,
		WeaponSingle,
		WeaponMultipoint,
		FeedbackSingle,
		VibrationSingle,
		FeedbackSlope,
		VibrationSlope,
		FeedbackMultipoint,
		VibrationMultipoint
	}

	public struct TriggerEffectDS
	{
		public TriggerEffectManager.EffectDualsense Effect;

		public byte Position;

		public byte EndPosition;

		public byte Frequency;

		public byte AmplitudeEndStrength;

		public byte Strength;

		public byte[] Strengths;
	}

	public enum EffectXbox
	{
		Off,
		FeedbackSingle,
		VibrationSingle,
		FeedbackSlope,
		VibrationSlope
	}

	public struct TriggerEffectXB
	{
		public TriggerEffectManager.EffectXbox Effect;

		public float Strength;

		[FormerlySerializedAs("endStrength")]
		[FormerlySerializedAs("Amplitude")]
		public float EndStrength;

		public float StartPosition;

		public float EndPosition;
	}

	public struct ControllerTriggerEffect
	{
		public TriggerEffectManager.TriggerEffectDS DualsenseEffect;

		public TriggerEffectManager.TriggerEffectXB XboxTriggerEffect;
	}
}
