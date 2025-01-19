using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using WorldGenerationEngineFinal;

public class MainMenuMono : MonoBehaviour
{
	public bool IsQuickContinue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameUtils.GetLaunchArgument("quick-continue") != null || (ToggleCapsLock.GetScrollLock() && Application.isEditor);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.windowManager = base.GetComponent<GUIWindowManager>();
		this.nguiWindowManager = base.GetComponent<NGUIWindowManager>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (!GameManager.IsDedicatedServer)
		{
			this.nguiWindowManager.SetLabelText(EnumNGUIWindow.Version, Constants.cVersionInformation.LongString, false);
			Cursor.visible = true;
			Cursor.lockState = SoftCursor.DefaultCursorLockState;
			this.nguiWindowManager.Show(EnumNGUIWindow.Loading, true);
			return;
		}
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "RWG")
		{
			base.StartCoroutine(this.startGeneration(new Action(this.startServer)));
			return;
		}
		this.startServer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGeneration(Action onGenerationComplete)
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.WorldGenSeed);
		int @int = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);
		string worldName = WorldBuilder.GetGeneratedWorldName(@string, @int);
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(worldName, worldName, GamePrefs.GetString(EnumGamePrefs.GameName));
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			WorldBuilder worldBuilder = new WorldBuilder(@string, @int);
			yield return worldBuilder.GenerateFromServer();
		}
		else
		{
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(location);
			if (worldInfo == null)
			{
				Log.Error("====================================================================================================");
				Log.Error(string.Concat(new string[]
				{
					"Error starting dedicated server: Folder for requested RWG world \"",
					worldName,
					"\" to be generated from seed \"",
					@string,
					"\" already exists."
				}));
				Log.Error("It does not contain a map_info.xml, so the world likely was never successfully generated!");
				Log.Error("Either delete the folder or change the WorldGenSeed and/or WorldGenSize settings!");
				Log.Error("(Path of the world: " + location.FullPath + ")");
				Log.Error("====================================================================================================");
				Application.Quit();
				yield break;
			}
			if (worldInfo.GameVersionCreated.IsValid && !worldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation))
			{
				Log.Error("====================================================================================================");
				Log.Error(string.Concat(new string[]
				{
					"Error starting dedicated server: Requested RWG world \"",
					worldName,
					"\" to be generated from seed \"",
					@string,
					"\" already exists."
				}));
				Log.Error("It was created with a different major version of the game!");
				Log.Error("Either delete the world or change the WorldGenSeed and/or WorldGenSize settings!");
				Log.Error("(Path of the world: " + location.FullPath + ")");
				Log.Error("====================================================================================================");
				Application.Quit();
				yield break;
			}
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
		if (onGenerationComplete != null)
		{
			yield return new WaitForSeconds(2f);
			onGenerationComplete();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startServer()
	{
		base.StartCoroutine(this.startServerCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startServerCo()
	{
		yield return new WaitForSeconds(2.5f);
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (PathAbstractions.WorldsSearchPaths.GetLocation(@string, @string, GamePrefs.GetString(EnumGamePrefs.GameName)).Type == PathAbstractions.EAbstractedLocationType.None)
		{
			Log.Error("====================================================================================================");
			Log.Error("Error starting dedicated server: GameWorld \"" + @string + "\" not found!");
			Log.Error("====================================================================================================");
			Application.Quit();
			yield break;
		}
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			Log.Error("====================================================================================================");
			Log.Error("Error starting dedicated server: " + networkConnectionError.ToStringCached<NetworkConnectionError>());
			Log.Out("Make sure all required ports are unused: " + SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString());
			Log.Error("====================================================================================================");
			Application.Quit();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckLogin()
	{
		this.loginCheckDone = true;
		this.bOpenMainMenu = false;
		if (GameManager.IsDedicatedServer)
		{
			this.bOpenMainMenu = true;
		}
		else if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			XUiC_LoginStandalone.Login(this.windowManager.playerUI.xui, new Action(this.OnLoginComplete));
		}
		else if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
		{
			XUiC_LoginXBOX.Login(this.windowManager.playerUI.xui, new Action(this.OnLoginComplete));
		}
		else
		{
			if (!DeviceFlag.PS5.IsCurrent())
			{
				throw new Exception(string.Format("Could not find Login window for platform: {0}", DeviceFlags.Current));
			}
			XUiC_LoginPS5.Login(this.windowManager.playerUI.xui, new Action(this.OnLoginComplete));
		}
		return this.bOpenMainMenu;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLoginComplete()
	{
		XUiC_MainMenuPlayerName.OpenIfNotOpen(this.windowManager.playerUI.xui);
		if (!GameManager.RemoteResourcesLoaded)
		{
			GameManager.LoadRemoteResources(null);
		}
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			this.bOpenMainMenu = true;
			return;
		}
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn)
		{
			EUserPerms euserPerms = EUserPerms.All;
			if (!GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
			{
				euserPerms &= ~EUserPerms.HostMultiplayer;
			}
			base.StartCoroutine(this.<OnLoginComplete>g__ResolveInitialPermissions|13_0(euserPerms));
			return;
		}
		string format = "Login complete but user is not in valid state. Native platform user status: {0}, Crossplatform user status: {1}";
		object arg = PlatformManager.NativePlatform.User.UserStatus;
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		object obj;
		if (crossplatformPlatform == null)
		{
			obj = null;
		}
		else
		{
			IUserClient user = crossplatformPlatform.User;
			obj = ((user != null) ? user.UserStatus.ToString() : null);
		}
		Log.Error(string.Format(format, arg, obj ?? "N/A"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (!GameStartupHelper.Instance.OpenMainMenuAfterAwake)
		{
			return;
		}
		if (!GameManager.Instance.bStaticDataLoaded)
		{
			if (this.loadingText == "")
			{
				this.loadingText = Localization.Get("loadActionLoading", false);
			}
			this.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, this.loadingText + " " + GameManager.Instance.CurrentLoadAction + "...", false);
			return;
		}
		if (!GameManager.Instance.gameStateManager.IsGameStarted() && this.nguiWindowManager.IsShowing(EnumNGUIWindow.Loading))
		{
			this.nguiWindowManager.Show(EnumNGUIWindow.Loading, false);
		}
		if (this.windowManager.playerUI == null || this.windowManager.playerUI.xui == null || !this.windowManager.playerUI.xui.isReady)
		{
			return;
		}
		if (!this.loginCheckDone && !this.CheckLogin())
		{
			return;
		}
		if (!this.bOpenMainMenu)
		{
			return;
		}
		if (this.IsQuickContinue)
		{
			GamePrefs.Instance.Load(GameIO.GetSaveGameDir() + "/gameOptions.sdf");
			NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
			if (networkConnectionError != NetworkConnectionError.NoError)
			{
				((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)this.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowNetworkError(networkConnectionError);
			}
		}
		else
		{
			if (ModManager.GetFailedMods(new Mod.EModLoadState?(Mod.EModLoadState.NotAntiCheatCompatible)).Count > 0)
			{
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if (antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled())
				{
					XUiC_MessageBoxWindowGroup.ShowMessageBox(this.windowManager.playerUI.xui, Localization.Get("xuiModsAntiCheatModWithCodeTitle", false), Localization.Get("xuiModsAntiCheatModWithCodeText", false), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, delegate()
					{
						Utils.RestartGame(Utils.ERestartAntiCheatMode.ForceOff);
					}, delegate()
					{
						XUiC_EulaWindow.Open(this.windowManager.playerUI.xui, false);
					}, false, true);
					goto IL_1F2;
				}
			}
			XUiC_EulaWindow.Open(this.windowManager.playerUI.xui, false);
		}
		IL_1F2:
		this.bOpenMainMenu = false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator <OnLoginComplete>g__ResolveInitialPermissions|13_0(EUserPerms _perms)
	{
		yield return PermissionsManager.ResolvePermissions(_perms, false, null);
		this.bOpenMainMenu = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float ServerStartDelaySec = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bOpenMainMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool loginCheckDone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string loadingText = "";
}
