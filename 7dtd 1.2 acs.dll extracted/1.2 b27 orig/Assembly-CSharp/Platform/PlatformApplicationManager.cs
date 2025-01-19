﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platform
{
	public static class PlatformApplicationManager
	{
		public static IPlatformApplication Application { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public static bool IsRestartRequired
		{
			get
			{
				return PlatformApplicationManager.isRestartRequired;
			}
		}

		public static bool Init()
		{
			PlatformApplicationManager.Application = IPlatformApplication.Create();
			return true;
		}

		public static void SetRestartRequired()
		{
			PlatformApplicationManager.isRestartRequired = PlatformOptimizations.RestartProcessSupported;
			Log.Out(string.Format("[PlatformApplication] restart required = {0}", PlatformApplicationManager.isRestartRequired));
		}

		public static IEnumerator CheckRestartCoroutine(bool loadSaveGame = false)
		{
			if (PlatformApplicationManager.isRestarting || !PlatformApplicationManager.isRestartRequired)
			{
				yield break;
			}
			PlatformApplicationManager.isRestartRequired = false;
			PlatformApplicationManager.isRestarting = true;
			try
			{
				yield return GameManager.Instance.ShowExitingGameUICoroutine();
				PlatformApplicationManager.RestartProcess(loadSaveGame);
			}
			finally
			{
				Log.Error("[PlatformApplication] failed to restart process.");
				PlatformApplicationManager.isRestarting = false;
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string[] RemoveFirstRunArguments(string[] argv)
		{
			return (from arg in argv
			where !arg.StartsWith("-LoadSaveGame=", StringComparison.OrdinalIgnoreCase)
			select arg).ToArray<string>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void RestartProcess(bool loadSaveGame)
		{
			List<string> list = new List<string>();
			list.AddRange(PlatformApplicationManager.RemoveFirstRunArguments(GameStartupHelper.RemoveTemporaryArguments(GameStartupHelper.GetCommandLineArgs())));
			list.Add("[REMOVE_ON_RESTART]");
			list.Add("-skipintro");
			list.Add(LaunchPrefs.SkipNewsScreen.ToCommandLine(true));
			if (PlatformOptimizations.RestartAfterRwg && loadSaveGame)
			{
				Log.Out("[LoadSaveGame] After restart should load: worldName=" + GamePrefs.GetString(EnumGamePrefs.GameWorld) + " saveName=" + GamePrefs.GetString(EnumGamePrefs.GameName));
				list.Add(LaunchPrefs.LoadSaveGame.ToCommandLine(true));
			}
			list.AddRange(PlatformManager.NativePlatform.GetArgumentsForRelaunch());
			try
			{
				GamePrefs.Instance.Save();
				SaveDataUtils.Destroy();
				PlatformManager.Destroy();
			}
			catch (Exception e)
			{
				Log.Error("Exception thrown while preparing for process restart. This may cause errors in the next run");
				Log.Exception(e);
			}
			PlatformApplicationManager.Application.RestartProcess(list.ToArray());
		}

		public static EPlatformLoadSaveGameState GetLoadSaveGameState()
		{
			if (!LaunchPrefs.LoadSaveGame.Value)
			{
				return EPlatformLoadSaveGameState.Done;
			}
			if (PlatformApplicationManager.loadSaveGameState != EPlatformLoadSaveGameState.Init)
			{
				return PlatformApplicationManager.loadSaveGameState;
			}
			string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			if (!GameIO.DoesWorldExist(worldName))
			{
				Log.Warning("[LoadSaveGame] World does not exist: " + worldName);
				return PlatformApplicationManager.loadSaveGameState = EPlatformLoadSaveGameState.Done;
			}
			string gameName = GamePrefs.GetString(EnumGamePrefs.GameName);
			bool found = false;
			bool isArchived = false;
			GameIO.GetPlayerSaves(delegate(string foundSaveName, string foundWorldName, DateTime _, WorldState _, bool foundIsArchived)
			{
				if (foundSaveName.EqualsCaseInsensitive(gameName) && foundWorldName.EqualsCaseInsensitive(worldName))
				{
					found = true;
					isArchived = foundIsArchived;
				}
			}, true);
			if (!found)
			{
				Log.Out(string.Concat(new string[]
				{
					"[LoadSaveGame] Creating new save game '",
					gameName,
					"' from the world '",
					worldName,
					"'."
				}));
				return PlatformApplicationManager.loadSaveGameState = EPlatformLoadSaveGameState.NewGameOpen;
			}
			if (isArchived)
			{
				Log.Warning(string.Concat(new string[]
				{
					"[LoadSaveGame] Can not load archived save '",
					gameName,
					"' (world '",
					worldName,
					"')."
				}));
				return PlatformApplicationManager.loadSaveGameState = EPlatformLoadSaveGameState.Done;
			}
			Log.Out(string.Concat(new string[]
			{
				"[LoadSaveGame] Loading existing save game '",
				gameName,
				"' (world '",
				worldName,
				"')."
			}));
			return PlatformApplicationManager.loadSaveGameState = EPlatformLoadSaveGameState.ContinueGameOpen;
		}

		public static void AdvanceLoadSaveGameStateFrom(EPlatformLoadSaveGameState previousState)
		{
			if (PlatformApplicationManager.loadSaveGameState != previousState)
			{
				Log.Error(string.Format("[LoadSaveGame] Expected advance from {0} but was {1}", PlatformApplicationManager.loadSaveGameState, previousState));
				PlatformApplicationManager.loadSaveGameState = EPlatformLoadSaveGameState.Done;
			}
			EPlatformLoadSaveGameState eplatformLoadSaveGameState;
			switch (previousState)
			{
			case EPlatformLoadSaveGameState.Init:
				throw new NotSupportedException("Init state should be manually advanced from because it branches.");
			case EPlatformLoadSaveGameState.NewGameOpen:
				eplatformLoadSaveGameState = EPlatformLoadSaveGameState.NewGameSelect;
				break;
			case EPlatformLoadSaveGameState.NewGameSelect:
				eplatformLoadSaveGameState = EPlatformLoadSaveGameState.NewGamePlay;
				break;
			case EPlatformLoadSaveGameState.NewGamePlay:
				eplatformLoadSaveGameState = EPlatformLoadSaveGameState.Done;
				break;
			case EPlatformLoadSaveGameState.ContinueGameOpen:
				eplatformLoadSaveGameState = EPlatformLoadSaveGameState.ContinueGameSelect;
				break;
			case EPlatformLoadSaveGameState.ContinueGameSelect:
				eplatformLoadSaveGameState = EPlatformLoadSaveGameState.ContinueGamePlay;
				break;
			case EPlatformLoadSaveGameState.ContinueGamePlay:
				eplatformLoadSaveGameState = EPlatformLoadSaveGameState.Done;
				break;
			case EPlatformLoadSaveGameState.Done:
				throw new NotSupportedException("Can't advance from the final state.");
			default:
				throw new ArgumentOutOfRangeException();
			}
			PlatformApplicationManager.loadSaveGameState = eplatformLoadSaveGameState;
			Log.Out(string.Format("[LoadSaveGame] Advanced to state {0} (was {1})", PlatformApplicationManager.loadSaveGameState, previousState));
		}

		public static void SetFailedLoadSaveGame()
		{
			if (PlatformApplicationManager.loadSaveGameState == EPlatformLoadSaveGameState.Done)
			{
				return;
			}
			Log.Warning(string.Format("[LoadSaveGame] Failed to automate creating or loading the save game. State: {0}", PlatformApplicationManager.loadSaveGameState));
			PlatformApplicationManager.loadSaveGameState = EPlatformLoadSaveGameState.Done;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool isRestartRequired;

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool isRestarting;

		[PublicizedFrom(EAccessModifier.Private)]
		public static EPlatformLoadSaveGameState loadSaveGameState;
	}
}
