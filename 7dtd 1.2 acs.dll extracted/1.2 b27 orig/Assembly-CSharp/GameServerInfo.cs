﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Platform;
using UnityEngine;

public class GameServerInfo
{
	public event Action<GameServerInfo> OnChangedAny;

	public event Action<GameServerInfo, GameInfoString> OnChangedString;

	public event Action<GameServerInfo, GameInfoInt> OnChangedInt;

	public event Action<GameServerInfo, GameInfoBool> OnChangedBool;

	public bool IsValid
	{
		get
		{
			return !this.isBroken;
		}
	}

	public bool IsDedicated
	{
		get
		{
			return this.GetValue(GameInfoBool.IsDedicated);
		}
	}

	public bool IsDedicatedStock
	{
		get
		{
			return this.GetValue(GameInfoBool.IsDedicated) && this.GetValue(GameInfoBool.StockSettings) && !this.GetValue(GameInfoBool.ModdedConfig);
		}
	}

	public bool IsDedicatedModded
	{
		get
		{
			return this.GetValue(GameInfoBool.IsDedicated) && this.GetValue(GameInfoBool.ModdedConfig);
		}
	}

	public bool IsPeerToPeer
	{
		get
		{
			return !this.GetValue(GameInfoBool.IsDedicated) && !this.isNoResponse;
		}
	}

	public bool AllowsCrossplay
	{
		get
		{
			return this.GetValue(GameInfoBool.AllowCrossplay);
		}
	}

	public bool EACEnabled
	{
		get
		{
			return this.GetValue(GameInfoBool.EACEnabled);
		}
	}

	public bool IgnoresSanctions
	{
		get
		{
			return this.GetValue(GameInfoBool.SanctionsIgnored);
		}
	}

	public EPlayGroup PlayGroup
	{
		get
		{
			EPlayGroup result;
			if (!EnumUtils.TryParse<EPlayGroup>(this.GetValue(GameInfoString.PlayGroup), out result, false))
			{
				return EPlayGroup.Unknown;
			}
			return result;
		}
	}

	public bool IsFriends
	{
		get
		{
			return this.isFriends;
		}
		set
		{
			if (value != this.isFriends)
			{
				this.isFriends = value;
				Action<GameServerInfo> onChangedAny = this.OnChangedAny;
				if (onChangedAny == null)
				{
					return;
				}
				onChangedAny(this);
			}
		}
	}

	public bool IsFavoriteHistory
	{
		get
		{
			return this.IsFavorite || this.IsHistory;
		}
	}

	public bool IsFavorite
	{
		get
		{
			return this.isFavorite;
		}
		set
		{
			if (value != this.isFavorite)
			{
				this.isFavorite = value;
				Action<GameServerInfo> onChangedAny = this.OnChangedAny;
				if (onChangedAny == null)
				{
					return;
				}
				onChangedAny(this);
			}
		}
	}

	public bool IsHistory
	{
		get
		{
			return this.lastPlayed > 0;
		}
	}

	public int LastPlayedLinux
	{
		get
		{
			return this.lastPlayed;
		}
		set
		{
			if (value != this.lastPlayed)
			{
				this.lastPlayed = value;
				Action<GameServerInfo> onChangedAny = this.OnChangedAny;
				if (onChangedAny == null)
				{
					return;
				}
				onChangedAny(this);
			}
		}
	}

	public bool IsLAN
	{
		get
		{
			return this.isLan;
		}
		set
		{
			if (value != this.isLan)
			{
				this.isLan = value;
				Action<GameServerInfo> onChangedAny = this.OnChangedAny;
				if (onChangedAny == null)
				{
					return;
				}
				onChangedAny(this);
			}
		}
	}

	public bool IsLobby
	{
		get
		{
			return this.isLobby;
		}
		set
		{
			if (value != this.isLobby)
			{
				this.isLobby = value;
				Action<GameServerInfo> onChangedAny = this.OnChangedAny;
				if (onChangedAny == null)
				{
					return;
				}
				onChangedAny(this);
			}
		}
	}

	public bool IsNoResponse
	{
		get
		{
			return this.isNoResponse;
		}
		set
		{
			if (value != this.isNoResponse)
			{
				this.isNoResponse = value;
				Action<GameServerInfo> onChangedAny = this.OnChangedAny;
				if (onChangedAny == null)
				{
					return;
				}
				onChangedAny(this);
			}
		}
	}

	public VersionInformation Version
	{
		get
		{
			return this.version;
		}
	}

	public bool IsCompatibleVersion
	{
		get
		{
			return this.version.Major < 0 || this.version.EqualsMinor(Constants.cVersionInformation);
		}
	}

	public GameServerInfo()
	{
		this.OnChangedAny += this.RefreshServerDisplayTexts;
		this.Strings = new ReadOnlyDictionary<GameInfoString, string>(this.tableStrings);
		this.Ints = new ReadOnlyDictionary<GameInfoInt, int>(this.tableInts);
		this.Bools = new ReadOnlyDictionary<GameInfoBool, bool>(this.tableBools);
	}

	public GameServerInfo(GameServerInfo _gsi)
	{
		this.OnChangedAny += this.RefreshServerDisplayTexts;
		foreach (KeyValuePair<GameInfoString, string> keyValuePair in _gsi.tableStrings)
		{
			this.SetValue(keyValuePair.Key, keyValuePair.Value);
		}
		foreach (KeyValuePair<GameInfoInt, int> keyValuePair2 in _gsi.tableInts)
		{
			this.SetValue(keyValuePair2.Key, keyValuePair2.Value);
		}
		foreach (KeyValuePair<GameInfoBool, bool> keyValuePair3 in _gsi.tableBools)
		{
			this.SetValue(keyValuePair3.Key, keyValuePair3.Value);
		}
		this.isBroken = _gsi.isBroken;
		this.isFriends = _gsi.isFriends;
		this.isFavorite = _gsi.isFavorite;
		this.lastPlayed = _gsi.lastPlayed;
		this.isLan = _gsi.isLan;
		this.isLobby = _gsi.isLobby;
		this.isNoResponse = _gsi.isNoResponse;
	}

	public GameServerInfo(string _serverInfoString)
	{
		if (_serverInfoString.Length == 0)
		{
			this.isBroken = true;
			return;
		}
		this.OnChangedAny += this.RefreshServerDisplayTexts;
		this.BuildInfoFromString(_serverInfoString);
	}

	public void Merge(GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		this.isFriends |= _gameServerInfo.IsFriends;
		this.isFavorite |= _gameServerInfo.IsFavorite;
		this.isLan |= _gameServerInfo.IsLAN;
		if (_source == EServerRelationType.History)
		{
			this.lastPlayed = _gameServerInfo.LastPlayedLinux;
		}
		foreach (KeyValuePair<GameInfoBool, bool> keyValuePair in _gameServerInfo.tableBools)
		{
			if (keyValuePair.Value)
			{
				this.SetValue(keyValuePair.Key, keyValuePair.Value);
			}
		}
		foreach (KeyValuePair<GameInfoString, string> keyValuePair2 in _gameServerInfo.tableStrings)
		{
			if (_source == EServerRelationType.LAN || keyValuePair2.Key != GameInfoString.IP)
			{
				this.SetValue(keyValuePair2.Key, keyValuePair2.Value);
			}
		}
		foreach (KeyValuePair<GameInfoInt, int> keyValuePair3 in _gameServerInfo.tableInts)
		{
			this.SetValue(keyValuePair3.Key, keyValuePair3.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildInfoFromString(string _serverInfoString)
	{
		if (_serverInfoString.Length == 0)
		{
			this.isBroken = true;
			return;
		}
		foreach (string text in _serverInfoString.Substring(0, _serverInfoString.Length - 1).Split(';', StringSplitOptions.None))
		{
			if (text.Length >= 2)
			{
				string[] array2 = text.Split(':', StringSplitOptions.None);
				if (array2.Length >= 2)
				{
					string key = array2[0].Trim(GameServerInfo.whiteSpaceChars);
					string value = array2[1];
					this.ParseAny(key, value);
				}
			}
		}
	}

	public bool ParseAny(string _key, string _value)
	{
		bool result;
		try
		{
			GameInfoString key;
			GameInfoInt key2;
			GameInfoBool key3;
			if (EnumUtils.TryParse<GameInfoString>(_key, out key, true))
			{
				this.SetValue(key, _value);
			}
			else if (EnumUtils.TryParse<GameInfoInt>(_key, out key2, true))
			{
				this.SetValue(key2, Convert.ToInt32(_value));
			}
			else if (EnumUtils.TryParse<GameInfoBool>(_key, out key3, true))
			{
				this.SetValue(key3, StringParsers.ParseBool(_value, 0, -1, true));
			}
			result = true;
		}
		catch (Exception)
		{
			string text = this.GetValue(GameInfoString.IP);
			if (string.IsNullOrEmpty(text))
			{
				text = "<unknown>";
			}
			int value = this.GetValue(GameInfoInt.Port);
			Log.Warning("GameServer {0}:{1} replied with invalid setting: {2}={3}", new object[]
			{
				text,
				value,
				_key,
				_value
			});
			this.isBroken = true;
			result = false;
		}
		return result;
	}

	public bool Parse(string _key, string _value)
	{
		GameInfoString key;
		if (EnumUtils.TryParse<GameInfoString>(_key, out key, true))
		{
			this.SetValue(key, _value);
			return true;
		}
		return false;
	}

	public bool Parse(string _key, int _value)
	{
		GameInfoInt key;
		if (EnumUtils.TryParse<GameInfoInt>(_key, out key, true))
		{
			this.SetValue(key, _value);
			return true;
		}
		return false;
	}

	public bool Parse(string _key, bool _value)
	{
		GameInfoBool key;
		if (EnumUtils.TryParse<GameInfoBool>(_key, out key, true))
		{
			this.SetValue(key, _value);
			return true;
		}
		return false;
	}

	public void SetValue(GameInfoString _key, string _value)
	{
		if (_value == null)
		{
			_value = "";
		}
		this.tableStrings[_key] = _value.Replace(':', '^').Replace(';', '*');
		if (_key == GameInfoString.IP || _key == GameInfoString.SteamID || _key == GameInfoString.UniqueId)
		{
			this.hashcode = long.MinValue;
		}
		if (_key == GameInfoString.ServerVersion)
		{
			VersionInformation versionInformation;
			if (VersionInformation.TryParseSerializedString(_value, out versionInformation))
			{
				this.version = versionInformation;
			}
			else
			{
				Log.Warning("Server browser: Could not parse version from received data (from entry: " + this.GetValue(GameInfoString.IP) + "): " + _value);
			}
		}
		this.cachedToString = null;
		this.cachedToStringLineBreaks = null;
		Action<GameServerInfo, GameInfoString> onChangedString = this.OnChangedString;
		if (onChangedString != null)
		{
			onChangedString(this, _key);
		}
		Action<GameServerInfo> onChangedAny = this.OnChangedAny;
		if (onChangedAny == null)
		{
			return;
		}
		onChangedAny(this);
	}

	public string GetValue(GameInfoString _key)
	{
		string text;
		if (!this.tableStrings.TryGetValue(_key, out text))
		{
			return "";
		}
		return text.Replace('^', ':').Replace('*', ';');
	}

	public void SetValue(GameInfoInt _key, int _value)
	{
		if (_key != GameInfoInt.Ping || !this.tableInts.ContainsKey(GameInfoInt.Ping) || _value >= 0)
		{
			this.tableInts[_key] = _value;
		}
		if (_key == GameInfoInt.Port)
		{
			this.hashcode = long.MinValue;
		}
		this.cachedToString = null;
		this.cachedToStringLineBreaks = null;
		Action<GameServerInfo, GameInfoInt> onChangedInt = this.OnChangedInt;
		if (onChangedInt != null)
		{
			onChangedInt(this, _key);
		}
		Action<GameServerInfo> onChangedAny = this.OnChangedAny;
		if (onChangedAny == null)
		{
			return;
		}
		onChangedAny(this);
	}

	public int GetValue(GameInfoInt _key)
	{
		int result;
		if (!this.tableInts.TryGetValue(_key, out result))
		{
			return -1;
		}
		return result;
	}

	public void SetValue(GameInfoBool _key, bool _value)
	{
		this.tableBools[_key] = _value;
		this.cachedToString = null;
		this.cachedToStringLineBreaks = null;
		Action<GameServerInfo, GameInfoBool> onChangedBool = this.OnChangedBool;
		if (onChangedBool != null)
		{
			onChangedBool(this, _key);
		}
		Action<GameServerInfo> onChangedAny = this.OnChangedAny;
		if (onChangedAny == null)
		{
			return;
		}
		onChangedAny(this);
	}

	public bool GetValue(GameInfoBool _key)
	{
		bool flag;
		return this.tableBools.TryGetValue(_key, out flag) && flag;
	}

	public override string ToString()
	{
		return this.ToString(false);
	}

	public string ToString(bool _lineBreaks)
	{
		if ((!_lineBreaks && this.cachedToString == null) || (_lineBreaks && this.cachedToStringLineBreaks == null))
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<GameInfoString, string> keyValuePair in this.tableStrings)
			{
				stringBuilder.Append(keyValuePair.Key.ToStringCached<GameInfoString>());
				stringBuilder.Append(':');
				stringBuilder.Append(keyValuePair.Value);
				stringBuilder.Append(';');
				if (_lineBreaks)
				{
					stringBuilder.Append('\r');
					stringBuilder.Append('\n');
				}
			}
			foreach (KeyValuePair<GameInfoInt, int> keyValuePair2 in this.tableInts)
			{
				stringBuilder.Append(keyValuePair2.Key.ToStringCached<GameInfoInt>());
				stringBuilder.Append(':');
				stringBuilder.Append(keyValuePair2.Value);
				stringBuilder.Append(';');
				if (_lineBreaks)
				{
					stringBuilder.Append('\r');
					stringBuilder.Append('\n');
				}
			}
			foreach (KeyValuePair<GameInfoBool, bool> keyValuePair3 in this.tableBools)
			{
				stringBuilder.Append(keyValuePair3.Key.ToStringCached<GameInfoBool>());
				stringBuilder.Append(':');
				stringBuilder.Append(keyValuePair3.Value);
				stringBuilder.Append(';');
				if (_lineBreaks)
				{
					stringBuilder.Append('\r');
					stringBuilder.Append('\n');
				}
			}
			stringBuilder.Append('\r');
			stringBuilder.Append('\n');
			if (_lineBreaks)
			{
				this.cachedToStringLineBreaks = stringBuilder.ToString();
			}
			else
			{
				this.cachedToString = stringBuilder.ToString();
			}
		}
		if (!_lineBreaks)
		{
			return this.cachedToString;
		}
		return this.cachedToStringLineBreaks;
	}

	public override int GetHashCode()
	{
		if (this.hashcode == -9223372036854775808L)
		{
			string text = this.GetValue(GameInfoString.IP) + this.GetValue(GameInfoInt.Port).ToString();
			this.hashcode = (long)text.GetHashCode();
		}
		return (int)this.hashcode;
	}

	public override bool Equals(object _obj)
	{
		if (_obj == null)
		{
			return false;
		}
		GameServerInfo p = _obj as GameServerInfo;
		return this.Equals(p);
	}

	public bool Equals(GameServerInfo _p)
	{
		return _p != null && this.GetValue(GameInfoString.IP) == _p.GetValue(GameInfoString.IP) && this.GetValue(GameInfoInt.Port) == _p.GetValue(GameInfoInt.Port);
	}

	public void UpdateGameTimePlayers(ulong _time, int _players)
	{
		float time = Time.time;
		if (time - this.timeLastWorldTimeUpdate > 20f || this.GetValue(GameInfoInt.CurrentPlayers) != _players)
		{
			this.timeLastWorldTimeUpdate = time;
			if (PrefabEditModeManager.Instance.IsActive())
			{
				this.SetValue(GameInfoString.LevelName, PrefabEditModeManager.Instance.LoadedPrefab.Name);
			}
			this.SetValue(GameInfoInt.CurrentServerTime, (int)_time);
			this.SetValue(GameInfoInt.CurrentPlayers, _players);
			this.SetValue(GameInfoInt.FreePlayerSlots, this.GetValue(GameInfoInt.MaxPlayers) - _players);
			Action<GameServerInfo> onChangedAny = this.OnChangedAny;
			if (onChangedAny == null)
			{
				return;
			}
			onChangedAny(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshServerDisplayTexts(GameServerInfo gsi)
	{
		bool value = gsi.GetValue(GameInfoBool.IsDedicated);
		string value2 = gsi.GetValue(GameInfoString.CombinedPrimaryId);
		if (!value && string.IsNullOrEmpty(value2))
		{
			return;
		}
		PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
		if (!string.IsNullOrEmpty(value2))
		{
			platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(value2, false);
			if (platformUserIdentifierAbs == null)
			{
				return;
			}
		}
		string value3 = gsi.GetValue(GameInfoString.GameHost);
		string value4 = gsi.GetValue(GameInfoString.LevelName);
		string value5 = gsi.GetValue(GameInfoString.ServerDescription);
		string value6 = gsi.GetValue(GameInfoString.ServerWebsiteURL);
		string value7 = gsi.GetValue(GameInfoString.ServerLoginConfirmationText);
		if (!string.IsNullOrEmpty(value3) && string.IsNullOrEmpty(this.ServerDisplayName.Text))
		{
			this.ServerDisplayName.Update(value3, platformUserIdentifierAbs);
		}
		if (!string.IsNullOrEmpty(value4) && string.IsNullOrEmpty(this.ServerWorldName.Text))
		{
			this.ServerWorldName.Update(value4, platformUserIdentifierAbs);
		}
		if (!string.IsNullOrEmpty(value5) && string.IsNullOrEmpty(this.ServerDescription.Text))
		{
			this.ServerDescription.Update(value5.Replace("\\n", "\n"), platformUserIdentifierAbs);
		}
		if (!string.IsNullOrEmpty(value6) && string.IsNullOrEmpty(this.ServerURL.Text))
		{
			this.ServerURL.Update(value6, platformUserIdentifierAbs);
		}
		if (!string.IsNullOrEmpty(value7) && string.IsNullOrEmpty(this.ServerLoginConfirmationText.Text))
		{
			this.ServerLoginConfirmationText.Update(value7, platformUserIdentifierAbs);
		}
	}

	public void ClearOnChanged()
	{
		this.OnChangedAny = null;
		this.OnChangedString = null;
		this.OnChangedInt = null;
		this.OnChangedBool = null;
	}

	public static bool IsSearchable(GameInfoString _gameInfoKey)
	{
		return GameServerInfo.SearchableStringInfosSet.Contains(_gameInfoKey);
	}

	public static bool IsSearchable(GameInfoInt _gameInfoKey)
	{
		return GameServerInfo.IntInfosInGameTagsSet.Contains(_gameInfoKey);
	}

	public static bool IsSearchable(GameInfoBool _gameInfoKey)
	{
		return GameServerInfo.BoolInfosInGameTagsSet.Contains(_gameInfoKey);
	}

	public static void PrepareLocalServerInfo()
	{
		GameServerInfo gameServerInfo = new GameServerInfo();
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		if (GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
		{
			bool flag = true;
			if (8 < GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount))
			{
				Log.Warning(string.Format("CROSSPLAY INCOMPATIBLE VALUE: PLAYER COUNT GREATER THAN MAX OF {0}", 8));
				flag = false;
			}
			if (GamePrefs.GetBool(EnumGamePrefs.IgnoreEOSSanctions))
			{
				Log.Warning("CROSSPLAY INCOMPATIBLE VALUE: EOS SANCTIONS IGNORED");
				flag = false;
			}
			if (!flag)
			{
				Log.Warning("CROSSPLAY DISABLED FOR SESSION, CORRECT VALUES TO BE CROSSPLAY COMPATIBLE");
				GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, false);
			}
		}
		gameServerInfo.SetValue(GameInfoString.GameType, "7DTD");
		gameServerInfo.SetValue(GameInfoString.GameName, GamePrefs.GetString(EnumGamePrefs.GameName));
		gameServerInfo.SetValue(GameInfoString.GameMode, GamePrefs.GetString(EnumGamePrefs.GameMode).Replace("GameMode", ""));
		gameServerInfo.SetValue(GameInfoString.GameHost, GameManager.IsDedicatedServer ? GamePrefs.GetString(EnumGamePrefs.ServerName) : GamePrefs.GetString(EnumGamePrefs.PlayerName));
		gameServerInfo.SetValue(GameInfoString.LevelName, PrefabEditModeManager.Instance.IsActive() ? PrefabEditModeManager.Instance.LoadedPrefab.Name : GamePrefs.GetString(EnumGamePrefs.GameWorld));
		gameServerInfo.SetValue(GameInfoString.ServerDescription, GamePrefs.GetString(EnumGamePrefs.ServerDescription));
		gameServerInfo.SetValue(GameInfoString.ServerWebsiteURL, GamePrefs.GetString(EnumGamePrefs.ServerWebsiteURL));
		gameServerInfo.SetValue(GameInfoString.ServerLoginConfirmationText, GamePrefs.GetString(EnumGamePrefs.ServerLoginConfirmationText));
		gameServerInfo.SetValue(GameInfoBool.IsDedicated, GameManager.IsDedicatedServer);
		gameServerInfo.SetValue(GameInfoBool.IsPasswordProtected, !string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.ServerPassword)));
		bool value = GameManager.IsDedicatedServer ? GamePrefs.GetBool(EnumGamePrefs.EACEnabled) : GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		gameServerInfo.SetValue(GameInfoBool.EACEnabled, value);
		gameServerInfo.SetValue(GameInfoBool.SanctionsIgnored, GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.IgnoreEOSSanctions));
		gameServerInfo.SetValue(GameInfoBool.AllowCrossplay, @bool && GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay) && PermissionsManager.IsCrossplayAllowed());
		gameServerInfo.SetValue(GameInfoString.PlayGroup, EPlayGroupExtensions.Current.ToStringCached<EPlayGroup>());
		gameServerInfo.SetValue(GameInfoInt.MaxPlayers, GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount));
		gameServerInfo.SetValue(GameInfoInt.FreePlayerSlots, GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount) - (GameManager.IsDedicatedServer ? 0 : 1));
		gameServerInfo.SetValue(GameInfoInt.CurrentPlayers, GameManager.IsDedicatedServer ? 0 : 1);
		gameServerInfo.SetValue(GameInfoInt.Port, GamePrefs.GetInt(EnumGamePrefs.ServerPort));
		gameServerInfo.SetValue(GameInfoString.ServerVersion, Constants.cVersionInformation.SerializableString);
		gameServerInfo.SetValue(GameInfoBool.Architecture64, !Constants.Is32BitOs);
		gameServerInfo.SetValue(GameInfoString.Platform, Application.platform.ToString());
		gameServerInfo.SetValue(GameInfoBool.IsPublic, GamePrefs.GetBool(EnumGamePrefs.ServerIsPublic));
		gameServerInfo.SetValue(GameInfoInt.ServerVisibility, PermissionsManager.IsMultiplayerAllowed() ? GamePrefs.GetInt(EnumGamePrefs.ServerVisibility) : 0);
		gameServerInfo.SetValue(GameInfoBool.StockSettings, GamePrefs.HasStockSettings());
		bool flag2 = StockFileHashes.HasStockXMLs();
		gameServerInfo.SetValue(GameInfoBool.StockFiles, flag2);
		gameServerInfo.SetValue(GameInfoBool.ModdedConfig, !flag2 || ModManager.AnyConfigModActive());
		gameServerInfo.SetValue(GameInfoString.Region, GamePrefs.GetString(EnumGamePrefs.Region));
		gameServerInfo.SetValue(GameInfoString.Language, GameManager.IsDedicatedServer ? GamePrefs.GetString(EnumGamePrefs.Language) : Localization.language);
		gameServerInfo.SetValue(GameInfoInt.GameDifficulty, GamePrefs.GetInt(EnumGamePrefs.GameDifficulty));
		gameServerInfo.SetValue(GameInfoInt.BlockDamagePlayer, GamePrefs.GetInt(EnumGamePrefs.BlockDamagePlayer));
		gameServerInfo.SetValue(GameInfoInt.BlockDamageAI, GamePrefs.GetInt(EnumGamePrefs.BlockDamageAI));
		gameServerInfo.SetValue(GameInfoInt.BlockDamageAIBM, GamePrefs.GetInt(EnumGamePrefs.BlockDamageAIBM));
		gameServerInfo.SetValue(GameInfoInt.XPMultiplier, GamePrefs.GetInt(EnumGamePrefs.XPMultiplier));
		gameServerInfo.SetValue(GameInfoBool.BuildCreate, GamePrefs.GetBool(EnumGamePrefs.BuildCreate));
		gameServerInfo.SetValue(GameInfoInt.DayNightLength, GamePrefs.GetInt(EnumGamePrefs.DayNightLength));
		gameServerInfo.SetValue(GameInfoInt.DayLightLength, GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
		gameServerInfo.SetValue(GameInfoInt.DeathPenalty, GamePrefs.GetInt(EnumGamePrefs.DeathPenalty));
		gameServerInfo.SetValue(GameInfoInt.DropOnDeath, GamePrefs.GetInt(EnumGamePrefs.DropOnDeath));
		gameServerInfo.SetValue(GameInfoInt.DropOnQuit, GamePrefs.GetInt(EnumGamePrefs.DropOnQuit));
		gameServerInfo.SetValue(GameInfoInt.BedrollDeadZoneSize, GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize));
		gameServerInfo.SetValue(GameInfoInt.BedrollExpiryTime, GamePrefs.GetInt(EnumGamePrefs.BedrollExpiryTime));
		gameServerInfo.SetValue(GameInfoInt.MaxSpawnedZombies, GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies));
		gameServerInfo.SetValue(GameInfoInt.MaxSpawnedAnimals, GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals));
		gameServerInfo.SetValue(GameInfoBool.EnemySpawnMode, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		gameServerInfo.SetValue(GameInfoInt.EnemyDifficulty, GamePrefs.GetInt(EnumGamePrefs.EnemyDifficulty));
		gameServerInfo.SetValue(GameInfoInt.ZombieFeralSense, GamePrefs.GetInt(EnumGamePrefs.ZombieFeralSense));
		gameServerInfo.SetValue(GameInfoInt.ZombieMove, GamePrefs.GetInt(EnumGamePrefs.ZombieMove));
		gameServerInfo.SetValue(GameInfoInt.ZombieMoveNight, GamePrefs.GetInt(EnumGamePrefs.ZombieMoveNight));
		gameServerInfo.SetValue(GameInfoInt.ZombieFeralMove, GamePrefs.GetInt(EnumGamePrefs.ZombieFeralMove));
		gameServerInfo.SetValue(GameInfoInt.ZombieBMMove, GamePrefs.GetInt(EnumGamePrefs.ZombieBMMove));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonFrequency, GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonRange, GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonWarning, GamePrefs.GetInt(EnumGamePrefs.BloodMoonWarning));
		gameServerInfo.SetValue(GameInfoInt.BloodMoonEnemyCount, GamePrefs.GetInt(EnumGamePrefs.BloodMoonEnemyCount));
		gameServerInfo.SetValue(GameInfoInt.LootAbundance, GamePrefs.GetInt(EnumGamePrefs.LootAbundance));
		int num = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
		if (num == 0)
		{
			num = -1;
		}
		gameServerInfo.SetValue(GameInfoInt.LootRespawnDays, num);
		gameServerInfo.SetValue(GameInfoInt.AirDropFrequency, GamePrefs.GetInt(EnumGamePrefs.AirDropFrequency));
		gameServerInfo.SetValue(GameInfoBool.AirDropMarker, GamePrefs.GetBool(EnumGamePrefs.AirDropMarker));
		gameServerInfo.SetValue(GameInfoInt.PartySharedKillRange, GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange));
		gameServerInfo.SetValue(GameInfoInt.PlayerKillingMode, GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode));
		gameServerInfo.SetValue(GameInfoInt.LandClaimCount, GamePrefs.GetInt(EnumGamePrefs.LandClaimCount));
		gameServerInfo.SetValue(GameInfoInt.LandClaimSize, GamePrefs.GetInt(EnumGamePrefs.LandClaimSize));
		gameServerInfo.SetValue(GameInfoInt.LandClaimDeadZone, GamePrefs.GetInt(EnumGamePrefs.LandClaimDeadZone));
		gameServerInfo.SetValue(GameInfoInt.LandClaimExpiryTime, GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime));
		gameServerInfo.SetValue(GameInfoInt.LandClaimDecayMode, GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode));
		gameServerInfo.SetValue(GameInfoInt.LandClaimOnlineDurabilityModifier, GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier));
		gameServerInfo.SetValue(GameInfoInt.LandClaimOfflineDurabilityModifier, GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDurabilityModifier));
		gameServerInfo.SetValue(GameInfoInt.LandClaimOfflineDelay, GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDelay));
		gameServerInfo.SetValue(GameInfoInt.MaxChunkAge, GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge));
		gameServerInfo.SetValue(GameInfoBool.ShowFriendPlayerOnMap, GamePrefs.GetBool(EnumGamePrefs.ShowFriendPlayerOnMap));
		gameServerInfo.SetValue(GameInfoInt.DayCount, GamePrefs.GetInt(EnumGamePrefs.DayCount));
		gameServerInfo.SetValue(GameInfoBool.AllowSpawnNearBackpack, GamePrefs.GetBool(EnumGamePrefs.AllowSpawnNearBackpack));
		gameServerInfo.SetValue(GameInfoInt.QuestProgressionDailyLimit, GamePrefs.GetInt(EnumGamePrefs.QuestProgressionDailyLimit));
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo = gameServerInfo;
	}

	public static void SetLocalServerWorldInfo()
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoInt.CurrentServerTime, (int)GameManager.Instance.World.worldTime);
		Vector3i other;
		Vector3i one;
		GameManager.Instance.World.GetWorldExtent(out other, out one);
		Vector3i vector3i = one - other;
		SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.SetValue(GameInfoInt.WorldSize, vector3i.x);
	}

	public bool IsUsingDefaultValueRanges()
	{
		foreach (KeyValuePair<GameInfoInt, int[]> keyValuePair in GameServerInfo.DefaultIntRanges)
		{
			int num;
			if (this.Ints.TryGetValue(keyValuePair.Key, out num))
			{
				bool flag = false;
				for (int i = 0; i < keyValuePair.Value.Length; i++)
				{
					if (keyValuePair.Value[i] == num)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Log.Warning("Using non default value {0} for int {1}", new object[]
					{
						num,
						keyValuePair.Key
					});
					return false;
				}
			}
		}
		return true;
	}

	public static int[] GetDefaultIntValues(EnumGamePrefs gamePref)
	{
		string text = gamePref.ToString();
		Log.Out("Getting Default Int Values for " + text);
		foreach (KeyValuePair<GameInfoInt, int[]> keyValuePair in GameServerInfo.DefaultIntRanges)
		{
			if (keyValuePair.Key.ToString().Equals(text))
			{
				return keyValuePair.Value;
			}
		}
		Log.Error("Could not match game pref to game option for " + text);
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GameInfoString, string> tableStrings = new EnumDictionary<GameInfoString, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GameInfoInt, int> tableInts = new EnumDictionary<GameInfoInt, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GameInfoBool, bool> tableBools = new EnumDictionary<GameInfoBool, bool>();

	public readonly ReadOnlyDictionary<GameInfoString, string> Strings;

	public readonly ReadOnlyDictionary<GameInfoInt, int> Ints;

	public readonly ReadOnlyDictionary<GameInfoBool, bool> Bools;

	public AuthoredText ServerDisplayName = new AuthoredText();

	public AuthoredText ServerWorldName = new AuthoredText();

	public AuthoredText ServerDescription = new AuthoredText();

	public AuthoredText ServerURL = new AuthoredText();

	public AuthoredText ServerLoginConfirmationText = new AuthoredText();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBroken;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFriends;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFavorite;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLan;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLobby;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isNoResponse;

	[PublicizedFrom(EAccessModifier.Private)]
	public long hashcode = long.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public VersionInformation version = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, -1, -1, -1);

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLastWorldTimeUpdate;

	public static readonly Dictionary<GameInfoInt, int[]> DefaultIntRanges = new Dictionary<GameInfoInt, int[]>
	{
		{
			GameInfoInt.GameDifficulty,
			new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5
			}
		},
		{
			GameInfoInt.XPMultiplier,
			new int[]
			{
				25,
				50,
				75,
				100,
				125,
				150,
				175,
				200,
				300
			}
		},
		{
			GameInfoInt.DayNightLength,
			new int[]
			{
				10,
				20,
				30,
				40,
				50,
				60,
				90,
				120
			}
		},
		{
			GameInfoInt.DayLightLength,
			new int[]
			{
				12,
				14,
				16,
				18
			}
		},
		{
			GameInfoInt.BloodMoonFrequency,
			new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9,
				10,
				14,
				20,
				30
			}
		},
		{
			GameInfoInt.BloodMoonRange,
			new int[]
			{
				0,
				1,
				2,
				3,
				4,
				7,
				10,
				14,
				20
			}
		},
		{
			GameInfoInt.BloodMoonWarning,
			new int[]
			{
				-1,
				8,
				18
			}
		},
		{
			GameInfoInt.BloodMoonEnemyCount,
			new int[]
			{
				4,
				6,
				8,
				10,
				12,
				16,
				24,
				32,
				64
			}
		},
		{
			GameInfoInt.ZombieMove,
			new int[]
			{
				0,
				1,
				2,
				3,
				4
			}
		},
		{
			GameInfoInt.ZombieMoveNight,
			new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5
			}
		},
		{
			GameInfoInt.ZombieFeralMove,
			new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5
			}
		},
		{
			GameInfoInt.ZombieBMMove,
			new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5
			}
		},
		{
			GameInfoInt.ZombieFeralSense,
			new int[]
			{
				0,
				1,
				2,
				3,
				4
			}
		},
		{
			GameInfoInt.AirDropFrequency,
			new int[]
			{
				0,
				24,
				72,
				168
			}
		},
		{
			GameInfoInt.BlockDamagePlayer,
			new int[]
			{
				25,
				50,
				75,
				100,
				125,
				150,
				175,
				200,
				300
			}
		},
		{
			GameInfoInt.BlockDamageAI,
			new int[]
			{
				25,
				33,
				50,
				67,
				75,
				100,
				125,
				150,
				175,
				200,
				300
			}
		},
		{
			GameInfoInt.BlockDamageAIBM,
			new int[]
			{
				25,
				33,
				50,
				67,
				75,
				100,
				125,
				150,
				175,
				200,
				300
			}
		},
		{
			GameInfoInt.LootAbundance,
			new int[]
			{
				25,
				33,
				50,
				67,
				75,
				100,
				150,
				200
			}
		},
		{
			GameInfoInt.LootRespawnDays,
			new int[]
			{
				-1,
				5,
				7,
				10,
				15,
				20,
				30,
				40,
				50
			}
		},
		{
			GameInfoInt.MaxChunkAge,
			new int[]
			{
				-1,
				10,
				20,
				30,
				40,
				50,
				75,
				100
			}
		},
		{
			GameInfoInt.DeathPenalty,
			new int[]
			{
				0,
				1,
				2,
				3
			}
		},
		{
			GameInfoInt.DropOnDeath,
			new int[]
			{
				0,
				1,
				2,
				3,
				4
			}
		},
		{
			GameInfoInt.DropOnQuit,
			new int[]
			{
				0,
				1,
				2,
				3
			}
		},
		{
			GameInfoInt.QuestProgressionDailyLimit,
			new int[]
			{
				-1,
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8
			}
		},
		{
			GameInfoInt.PlayerKillingMode,
			new int[]
			{
				0,
				1,
				2,
				3
			}
		},
		{
			GameInfoInt.LandClaimSize,
			new int[]
			{
				21,
				31,
				41,
				51,
				71
			}
		},
		{
			GameInfoInt.LandClaimDeadZone,
			new int[]
			{
				0,
				5,
				10,
				15,
				20,
				30,
				40,
				50
			}
		},
		{
			GameInfoInt.LandClaimExpiryTime,
			new int[]
			{
				1,
				2,
				3,
				4,
				5,
				7,
				10,
				30
			}
		},
		{
			GameInfoInt.LandClaimDecayMode,
			new int[]
			{
				0,
				1,
				2
			}
		},
		{
			GameInfoInt.LandClaimOnlineDurabilityModifier,
			new int[]
			{
				0,
				1,
				2,
				4,
				8,
				16,
				32,
				64,
				128,
				256
			}
		},
		{
			GameInfoInt.LandClaimOfflineDurabilityModifier,
			new int[]
			{
				0,
				1,
				2,
				4,
				8,
				16,
				32,
				64,
				128,
				256
			}
		},
		{
			GameInfoInt.LandClaimOfflineDelay,
			new int[]
			{
				0,
				1,
				5,
				10,
				20,
				30,
				60
			}
		},
		{
			GameInfoInt.BedrollDeadZoneSize,
			new int[]
			{
				0,
				5,
				15,
				30
			}
		},
		{
			GameInfoInt.BedrollExpiryTime,
			new int[]
			{
				3,
				7,
				15,
				30,
				45,
				60
			}
		},
		{
			GameInfoInt.PartySharedKillRange,
			new int[]
			{
				0,
				100,
				500,
				1000,
				5000,
				10000
			}
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] whiteSpaceChars = new char[]
	{
		' ',
		'\r',
		'\n',
		'\t'
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedToString;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedToStringLineBreaks;

	public static readonly GameInfoString[] SearchableStringInfos = new GameInfoString[]
	{
		GameInfoString.LevelName,
		GameInfoString.GameHost,
		GameInfoString.SteamID,
		GameInfoString.Region,
		GameInfoString.Language,
		GameInfoString.UniqueId,
		GameInfoString.CombinedNativeId,
		GameInfoString.ServerVersion,
		GameInfoString.PlayGroup
	};

	public static readonly GameInfoInt[] IntInfosInGameTags = new GameInfoInt[]
	{
		GameInfoInt.GameDifficulty,
		GameInfoInt.DayNightLength,
		GameInfoInt.DeathPenalty,
		GameInfoInt.DropOnDeath,
		GameInfoInt.DropOnQuit,
		GameInfoInt.BloodMoonEnemyCount,
		GameInfoInt.EnemyDifficulty,
		GameInfoInt.PlayerKillingMode,
		GameInfoInt.CurrentServerTime,
		GameInfoInt.DayLightLength,
		GameInfoInt.AirDropFrequency,
		GameInfoInt.LootAbundance,
		GameInfoInt.LootRespawnDays,
		GameInfoInt.MaxSpawnedZombies,
		GameInfoInt.LandClaimCount,
		GameInfoInt.LandClaimSize,
		GameInfoInt.LandClaimExpiryTime,
		GameInfoInt.LandClaimDecayMode,
		GameInfoInt.LandClaimOnlineDurabilityModifier,
		GameInfoInt.LandClaimOfflineDurabilityModifier,
		GameInfoInt.MaxSpawnedAnimals,
		GameInfoInt.PartySharedKillRange,
		GameInfoInt.ZombieFeralSense,
		GameInfoInt.ZombieMove,
		GameInfoInt.ZombieMoveNight,
		GameInfoInt.ZombieFeralMove,
		GameInfoInt.ZombieBMMove,
		GameInfoInt.XPMultiplier,
		GameInfoInt.BlockDamagePlayer,
		GameInfoInt.BlockDamageAI,
		GameInfoInt.BlockDamageAIBM,
		GameInfoInt.BloodMoonFrequency,
		GameInfoInt.BloodMoonRange,
		GameInfoInt.BloodMoonWarning,
		GameInfoInt.BedrollExpiryTime,
		GameInfoInt.LandClaimOfflineDelay,
		GameInfoInt.Port,
		GameInfoInt.FreePlayerSlots,
		GameInfoInt.CurrentPlayers,
		GameInfoInt.MaxPlayers,
		GameInfoInt.WorldSize,
		GameInfoInt.MaxChunkAge,
		GameInfoInt.QuestProgressionDailyLimit
	};

	public static readonly GameInfoBool[] BoolInfosInGameTags = new GameInfoBool[]
	{
		GameInfoBool.IsDedicated,
		GameInfoBool.ShowFriendPlayerOnMap,
		GameInfoBool.BuildCreate,
		GameInfoBool.StockSettings,
		GameInfoBool.ModdedConfig,
		GameInfoBool.RequiresMod,
		GameInfoBool.AirDropMarker,
		GameInfoBool.EnemySpawnMode,
		GameInfoBool.IsPasswordProtected,
		GameInfoBool.AllowCrossplay,
		GameInfoBool.EACEnabled,
		GameInfoBool.SanctionsIgnored
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<GameInfoString> SearchableStringInfosSet = new HashSet<GameInfoString>(GameServerInfo.SearchableStringInfos);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<GameInfoInt> IntInfosInGameTagsSet = new HashSet<GameInfoInt>(GameServerInfo.IntInfosInGameTags);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<GameInfoBool> BoolInfosInGameTagsSet = new HashSet<GameInfoBool>(GameServerInfo.BoolInfosInGameTags);

	public class UniqueIdEqualityComparer : IEqualityComparer<GameServerInfo>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public UniqueIdEqualityComparer()
		{
		}

		public bool Equals(GameServerInfo _x, GameServerInfo _y)
		{
			return _x == _y || (_x != null && _y != null && _x.GetValue(GameInfoString.UniqueId) == _y.GetValue(GameInfoString.UniqueId));
		}

		public int GetHashCode(GameServerInfo _obj)
		{
			return _obj.GetValue(GameInfoString.UniqueId).GetHashCode();
		}

		public static readonly GameServerInfo.UniqueIdEqualityComparer Instance = new GameServerInfo.UniqueIdEqualityComparer();
	}
}
