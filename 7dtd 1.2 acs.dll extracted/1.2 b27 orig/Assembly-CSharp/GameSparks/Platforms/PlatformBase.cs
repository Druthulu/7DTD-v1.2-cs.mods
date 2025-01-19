using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameSparks.Core;
using UnityEngine;

namespace GameSparks.Platforms
{
	public abstract class PlatformBase : MonoBehaviour, IGSPlatform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Start()
		{
			this.DeviceName = SystemInfo.deviceName.ToString();
			this.DeviceType = SystemInfo.deviceType.ToString();
			if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne || "n/a" == SystemInfo.deviceUniqueIdentifier)
			{
				if ("n/a" == SystemInfo.deviceUniqueIdentifier)
				{
					this.DeviceId = Guid.NewGuid().ToString();
				}
				else
				{
					this.DeviceId = SystemInfo.deviceUniqueIdentifier.ToString();
				}
			}
			else
			{
				this.DeviceId = SystemInfo.deviceUniqueIdentifier.ToString();
			}
			char[] separator = new char[]
			{
				' ',
				',',
				'.',
				':',
				'-',
				'_',
				'(',
				')'
			};
			int processorCount = SystemInfo.processorCount;
			string text = "Unknown";
			string value = SystemInfo.deviceModel;
			string value2 = SystemInfo.systemMemorySize.ToString() + " MB";
			string text2 = SystemInfo.operatingSystem;
			string value3 = SystemInfo.operatingSystem;
			string text3 = SystemInfo.processorType;
			string value4 = Screen.width.ToString() + "x" + Screen.height.ToString();
			string version = GS.Version;
			string sdk = this.SDK;
			string unityVersion = Application.unityVersion;
			string deviceOS = this.DeviceOS;
			uint num = <PrivateImplementationDetails>.ComputeStringHash(deviceOS);
			string[] array;
			if (num <= 2697922028U)
			{
				if (num <= 920978609U)
				{
					if (num != 63313862U)
					{
						if (num != 650872197U)
						{
							if (num != 920978609U)
							{
								goto IL_87E;
							}
							if (!(deviceOS == "SWITCH"))
							{
								goto IL_87E;
							}
							text = "Nintendo";
							value = "Switch";
							value3 = "Unknown";
							goto IL_87E;
						}
						else
						{
							if (!(deviceOS == "XBOXSERIES"))
							{
								goto IL_87E;
							}
							text = "Microsoft";
							value = "Xbox Series";
							value2 = (SystemInfo.systemMemorySize / 1000).ToString() + " MB";
							value3 = "Unknown";
							text3 = text3 + " " + SystemInfo.processorFrequency.ToString() + "MHz";
							RegexOptions options = RegexOptions.None;
							text3 = new Regex("[ ]{2,}", options).Replace(text3, " ");
							goto IL_87E;
						}
					}
					else if (!(deviceOS == "IOS"))
					{
						goto IL_87E;
					}
				}
				else if (num <= 2062687802U)
				{
					if (num != 1874269580U)
					{
						if (num != 2062687802U)
						{
							goto IL_87E;
						}
						if (!(deviceOS == "WSA"))
						{
							goto IL_87E;
						}
						goto IL_439;
					}
					else
					{
						if (!(deviceOS == "ANDROID"))
						{
							goto IL_87E;
						}
						array = SystemInfo.deviceModel.Split(separator);
						text = array[0];
						value = SystemInfo.deviceModel.Replace(text, "").Substring(1);
						array = SystemInfo.operatingSystem.Split(separator);
						text2 = array[0] + " " + array[1];
						value3 = array[7];
						text3 = text3 + " " + SystemInfo.processorFrequency.ToString() + "MHz";
						goto IL_87E;
					}
				}
				else if (num != 2077565087U)
				{
					if (num != 2697922028U)
					{
						goto IL_87E;
					}
					if (!(deviceOS == "XBOXONE"))
					{
						goto IL_87E;
					}
					goto IL_439;
				}
				else
				{
					if (!(deviceOS == "TIZEN"))
					{
						goto IL_87E;
					}
					text = "Tizen";
					goto IL_87E;
				}
			}
			else if (num <= 3522446090U)
			{
				if (num <= 3313477467U)
				{
					if (num != 3221571746U)
					{
						if (num != 3313477467U)
						{
							goto IL_87E;
						}
						if (!(deviceOS == "WIIU"))
						{
							goto IL_87E;
						}
						text = "Nintendo";
						value = "WiiU";
						goto IL_87E;
					}
					else if (!(deviceOS == "MACOS"))
					{
						goto IL_87E;
					}
				}
				else if (num != 3466466665U)
				{
					if (num != 3522446090U)
					{
						goto IL_87E;
					}
					if (!(deviceOS == "WEBGL"))
					{
						goto IL_87E;
					}
					array = SystemInfo.deviceModel.Split(separator);
					value = array[0];
					array = SystemInfo.operatingSystem.Split(separator);
					text2 = array[0];
					if (text2.Equals("Mac"))
					{
						text2 = string.Concat(new string[]
						{
							text2,
							" ",
							array[1],
							" ",
							array[2]
						});
						value3 = string.Concat(new string[]
						{
							array[3],
							".",
							array[4],
							".",
							array[5]
						});
						goto IL_87E;
					}
					value3 = array[1];
					goto IL_87E;
				}
				else if (!(deviceOS == "TVOS"))
				{
					goto IL_87E;
				}
			}
			else if (num <= 4197334560U)
			{
				if (num != 3805831818U)
				{
					if (num != 4197334560U)
					{
						goto IL_87E;
					}
					if (!(deviceOS == "PS4"))
					{
						goto IL_87E;
					}
					text = "Sony";
					value = "PS4";
					value2 = (SystemInfo.systemMemorySize / 1000000).ToString() + " MB";
					array = SystemInfo.operatingSystem.Split(separator);
					text2 = array[0];
					value3 = string.Concat(new string[]
					{
						array[1],
						".",
						array[2],
						".",
						array[3]
					});
					text3 = text3 + " " + SystemInfo.processorFrequency.ToString() + "MHz";
					goto IL_87E;
				}
				else
				{
					if (!(deviceOS == "WINDOWS"))
					{
						goto IL_87E;
					}
					goto IL_439;
				}
			}
			else if (num != 4214112179U)
			{
				if (num != 4225300267U)
				{
					goto IL_87E;
				}
				if (!(deviceOS == "GC_XBOXONE"))
				{
					goto IL_87E;
				}
				text = "Microsoft";
				value = "Xbox One";
				value2 = (SystemInfo.systemMemorySize / 1000).ToString() + " MB";
				value3 = "Unknown";
				text3 = text3 + " " + SystemInfo.processorFrequency.ToString() + "MHz";
				RegexOptions options2 = RegexOptions.None;
				text3 = new Regex("[ ]{2,}", options2).Replace(text3, " ");
				goto IL_87E;
			}
			else
			{
				if (!(deviceOS == "PS5"))
				{
					goto IL_87E;
				}
				text = "Sony";
				value = "PS4";
				value2 = (SystemInfo.systemMemorySize / 1000000).ToString() + " MB";
				array = SystemInfo.operatingSystem.Split(separator);
				text2 = array[0];
				value3 = string.Concat(new string[]
				{
					array[1],
					".",
					array[2],
					".",
					array[3]
				});
				text3 = text3 + " " + SystemInfo.processorFrequency.ToString() + "MHz";
				goto IL_87E;
			}
			text = "Apple";
			array = SystemInfo.operatingSystem.Split(separator);
			if (this.DeviceOS.Equals("MACOS"))
			{
				text2 = string.Concat(new string[]
				{
					array[0],
					" ",
					array[1],
					" ",
					array[2]
				});
				value3 = string.Concat(new string[]
				{
					array[3],
					".",
					array[4],
					".",
					array[5]
				});
				goto IL_87E;
			}
			text2 = array[0];
			value3 = array[1] + "." + array[2];
			goto IL_87E;
			IL_439:
			text = "Microsoft";
			if (this.DeviceOS.Equals("XBOXONE"))
			{
				value = "Xbox One";
				value2 = (SystemInfo.systemMemorySize / 1000).ToString() + " MB";
				value3 = "Unknown";
			}
			else
			{
				value = "PC";
				array = SystemInfo.operatingSystem.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				text2 = array[0] + " " + array[1];
				value3 = string.Concat(new string[]
				{
					array[2],
					".",
					array[3],
					".",
					array[4]
				});
			}
			text3 = text3 + " " + SystemInfo.processorFrequency.ToString() + "MHz";
			RegexOptions options3 = RegexOptions.None;
			text3 = new Regex("[ ]{2,}", options3).Replace(text3, " ");
			IL_87E:
			this.DeviceStats = new GSData(new Dictionary<string, object>
			{
				{
					"manufacturer",
					text
				},
				{
					"model",
					value
				},
				{
					"memory",
					value2
				},
				{
					"os.name",
					text2
				},
				{
					"os.version",
					value3
				},
				{
					"cpu.cores",
					processorCount.ToString()
				},
				{
					"cpu.vendor",
					text3
				},
				{
					"resolution",
					value4
				},
				{
					"gssdk",
					version
				},
				{
					"engine",
					sdk
				},
				{
					"engine.version",
					unityVersion
				}
			});
			this.Platform = Application.platform.ToString();
			GameSparksSettings.SetInstance(base.GetComponent<GameSparksUnity>().settings);
			this.ExtraDebug = GameSparksSettings.DebugBuild;
			this.PersistentDataPath = Application.persistentDataPath;
			GS.Initialise(this);
			UnityEngine.Object.DontDestroyOnLoad(this);
		}

		public void ExecuteOnMainThread(Action action)
		{
			List<Action> actions = this._actions;
			lock (actions)
			{
				this._actions.Add(action);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Update()
		{
			List<Action> actions = this._actions;
			lock (actions)
			{
				if (this._actions.Count > 0)
				{
					this._currentActions.AddRange(this._actions);
					this._actions.Clear();
				}
			}
			int count = this._currentActions.Count;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					Action action = this._currentActions[i];
					if (action != null)
					{
						try
						{
							action();
						}
						catch (Exception ex)
						{
							if (this.ExceptionReporter != null)
							{
								this.ExceptionReporter(ex);
							}
							else
							{
								Debug.Log(ex);
							}
						}
					}
				}
				this._currentActions.Clear();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnApplicationPause(bool paused)
		{
			if (!paused)
			{
				try
				{
					GS.Reconnect();
				}
				catch (Exception obj)
				{
					if (this.ExceptionReporter != null)
					{
						this.ExceptionReporter(obj);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnApplicationQuit()
		{
			GS.ShutDown();
			base.StartCoroutine("DelayedQuit");
			if (!this._allowQuitting)
			{
				Application.CancelQuit();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator DelayedQuit()
		{
			yield return new WaitForSeconds(1f);
			while (GS.Available)
			{
				yield return new WaitForSeconds(0.1f);
			}
			this._allowQuitting = true;
			Application.Quit();
			yield break;
		}

		public string DeviceOS
		{
			get
			{
				RuntimePlatform platform = Application.platform;
				if (platform <= RuntimePlatform.PS4)
				{
					switch (platform)
					{
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						return "MACOS";
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						return "WINDOWS";
					case RuntimePlatform.OSXWebPlayer:
					case RuntimePlatform.OSXDashboardPlayer:
					case RuntimePlatform.WindowsWebPlayer:
					case (RuntimePlatform)6:
					case RuntimePlatform.PS3:
					case RuntimePlatform.XBOX360:
					case RuntimePlatform.NaCl:
					case (RuntimePlatform)14:
					case RuntimePlatform.FlashPlayer:
					case RuntimePlatform.LinuxEditor:
						break;
					case RuntimePlatform.IPhonePlayer:
						return "IOS";
					case RuntimePlatform.Android:
						return "ANDROID";
					case RuntimePlatform.LinuxPlayer:
						return "LINUX";
					case RuntimePlatform.WebGLPlayer:
						return "WEBGL";
					case RuntimePlatform.MetroPlayerX86:
					case RuntimePlatform.MetroPlayerX64:
					case RuntimePlatform.MetroPlayerARM:
						return "WSA";
					default:
						if (platform == RuntimePlatform.PS4)
						{
							return "PS4";
						}
						break;
					}
				}
				else
				{
					if (platform == RuntimePlatform.XboxOne)
					{
						return "XBOXONE";
					}
					if (platform == RuntimePlatform.tvOS)
					{
						return "TVOS";
					}
					switch (platform)
					{
					case RuntimePlatform.GameCoreXboxSeries:
						return "XBOXSERIES";
					case RuntimePlatform.GameCoreXboxOne:
						return "GC_XBOXONE";
					case RuntimePlatform.PS5:
						return "PS5";
					}
				}
				return "UNKNOWN";
			}
		}

		public string DeviceName { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public string DeviceType { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public GSData DeviceStats { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public virtual string DeviceId { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public string Platform { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public bool ExtraDebug { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public string ApiKey
		{
			get
			{
				return GameSparksSettings.ApiKey;
			}
		}

		public string ApiSecret
		{
			get
			{
				return GameSparksSettings.ApiSecret;
			}
		}

		public string ApiCredential
		{
			get
			{
				return GameSparksSettings.Credential;
			}
		}

		public string ApiStage
		{
			get
			{
				if (!GameSparksSettings.PreviewBuild)
				{
					return "live";
				}
				return "preview";
			}
		}

		public string ApiDomain
		{
			get
			{
				return null;
			}
		}

		public string PersistentDataPath { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public void DebugMsg(string message)
		{
			if (GameSparksSettings.DebugBuild)
			{
				if (message.Length < 1500)
				{
					Log.Out("GS: " + message);
					return;
				}
				Log.Out("GS: " + message.Substring(0, 1500) + "...");
			}
		}

		public string SDK
		{
			get
			{
				return "Unity";
			}
		}

		public string AuthToken
		{
			get
			{
				return this.m_authToken;
			}
			set
			{
				this.m_authToken = value;
			}
		}

		public string UserId
		{
			get
			{
				return this.m_userId;
			}
			set
			{
				this.m_userId = value;
			}
		}

		public Action<Exception> ExceptionReporter { get; set; }

		public abstract IGameSparksTimer GetTimer();

		public abstract string MakeHmac(string stringToHmac, string secret);

		public abstract IGameSparksWebSocket GetSocket(string url, Action<string> messageReceived, Action closed, Action opened, Action<string> error);

		public abstract IGameSparksWebSocket GetBinarySocket(string url, Action<byte[]> messageReceived, Action closed, Action opened, Action<string> error);

		[PublicizedFrom(EAccessModifier.Protected)]
		public PlatformBase()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public static string PLAYER_PREF_AUTHTOKEN_KEY = "gamesparks.authtoken";

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public static string PLAYER_PREF_USERID_KEY = "gamesparks.userid";

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public static string PLAYER_PREF_DEVICEID_KEY = "gamesparks.deviceid";

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public List<Action> _actions = new List<Action>();

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public List<Action> _currentActions = new List<Action>();

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public bool _allowQuitting;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public string m_authToken = "0";

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public string m_userId = "";
	}
}
