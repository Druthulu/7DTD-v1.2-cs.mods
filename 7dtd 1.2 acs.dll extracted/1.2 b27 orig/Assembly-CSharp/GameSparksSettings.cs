using System;
using UnityEngine;

public class GameSparksSettings : ScriptableObject
{
	public static void SetInstance(GameSparksSettings settings)
	{
		GameSparksSettings.instance = settings;
	}

	public static GameSparksSettings Instance
	{
		get
		{
			if (GameSparksSettings.instance == null)
			{
				GameSparksSettings.instance = (Resources.Load("GameSparksSettings") as GameSparksSettings);
				if (GameSparksSettings.instance == null)
				{
					GameSparksSettings.instance = ScriptableObject.CreateInstance<GameSparksSettings>();
				}
			}
			return GameSparksSettings.instance;
		}
	}

	public static bool PreviewBuild
	{
		get
		{
			return GameSparksSettings.Instance.previewBuild;
		}
		set
		{
			GameSparksSettings.Instance.previewBuild = value;
		}
	}

	public static string SdkVersion
	{
		get
		{
			return GameSparksSettings.Instance.sdkVersion;
		}
		set
		{
			GameSparksSettings.Instance.sdkVersion = value;
		}
	}

	public static string ApiSecret
	{
		get
		{
			return GameSparksSettings.Instance.apiSecret;
		}
		set
		{
			GameSparksSettings.Instance.apiSecret = value;
		}
	}

	public static string ApiKey
	{
		get
		{
			return GameSparksSettings.Instance.apiKey;
		}
		set
		{
			GameSparksSettings.Instance.apiKey = value;
		}
	}

	public static string Credential
	{
		get
		{
			if (GameSparksSettings.Instance.credential != null && GameSparksSettings.Instance.credential.Length != 0)
			{
				return GameSparksSettings.Instance.credential;
			}
			return "device";
		}
		set
		{
			GameSparksSettings.Instance.credential = value;
		}
	}

	public static bool DebugBuild
	{
		get
		{
			return GameSparksSettings.Instance.debugBuild;
		}
		set
		{
			GameSparksSettings.Instance.debugBuild = value;
		}
	}

	public static string ServiceUrl
	{
		get
		{
			string text = GameSparksSettings.Instance.apiKey;
			if (GameSparksSettings.Instance.apiSecret.Contains(":"))
			{
				text = GameSparksSettings.Instance.apiSecret.Substring(0, GameSparksSettings.Instance.apiSecret.IndexOf(":")) + "/" + text;
			}
			if (GameSparksSettings.Instance.previewBuild)
			{
				return string.Format(GameSparksSettings.previewServiceUrlBase, text, GameSparksSettings.Instance.credential);
			}
			return string.Format(GameSparksSettings.liveServiceUrlBase, text, GameSparksSettings.Instance.credential);
		}
	}

	public const string gamesparksSettingsAssetName = "GameSparksSettings";

	public const string gamesparksSettingsPath = "GameSparks/Resources";

	public const string gamesparksSettingsAssetExtension = ".asset";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string liveServiceUrlBase = "wss://live-{0}.ws.gamesparks.net/ws/{1}/{0}";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string previewServiceUrlBase = "wss://preview-{0}.ws.gamesparks.net/ws/{1}/{0}";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameSparksSettings instance;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string sdkVersion;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string apiKey = "";

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string credential = "device";

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string apiSecret = "";

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool previewBuild = true;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool debugBuild;
}
