using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Platform;
using UnityEngine;

public class GameEntrypoint : MonoBehaviour
{
	public static bool EntrypointSuccess { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (!GameEntrypoint.s_entrypointEntered)
		{
			Log.Error("[GameEntrypoint] Blocking initialization in Awake!");
		}
		ThreadManager.RunCoroutineSync(GameEntrypoint.EntrypointCoroutine());
	}

	public static IEnumerator EntrypointCoroutine()
	{
		if (GameEntrypoint.s_entrypointEntered)
		{
			while (!GameEntrypoint.s_entrypointFinished)
			{
				yield return null;
			}
			yield break;
		}
		GameEntrypoint.s_entrypointEntered = true;
		try
		{
			yield return GameEntrypoint.EntrypointCoroutineInternal();
		}
		finally
		{
			GameEntrypoint.s_entrypointFinished = true;
			if (!GameEntrypoint.EntrypointSuccess)
			{
				Log.Error("[GameEntrypoint] Failed initializing core systems, shutting down");
				Application.Quit();
			}
		}
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator EntrypointCoroutineInternal()
	{
		BacktraceUtils.InitializeBacktrace();
		Cursor.visible = false;
		ThreadManager.SetMainThreadRef(Thread.CurrentThread);
		DeviceFlag deviceFlag = DeviceFlags.Current;
		PlatformOptimizations.Init();
		if (GameEntrypoint.HasPrefCollisions())
		{
			yield break;
		}
		GamePrefs.InitPropertyDeclarations();
		yield return null;
		if (!GameStartupHelper.Instance.InitCommandLine())
		{
			yield break;
		}
		if (!PlatformApplicationManager.Init())
		{
			yield break;
		}
		yield return null;
		if (!PlatformManager.Init())
		{
			yield break;
		}
		yield return null;
		yield return SaveDataUtils.InitStaticCoroutine();
		yield return null;
		GamePrefs.InitPrefs();
		if (!GameStartupHelper.Instance.InitGamePrefs())
		{
			yield break;
		}
		yield return null;
		try
		{
			Localization.Init();
		}
		catch (Exception ex)
		{
			Log.Error(string.Format("[GameEntrypoint] Failed initializing localization: {0}", ex.GetType()));
			Log.Exception(ex);
			yield break;
		}
		GameEntrypoint.EntrypointSuccess = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool HasPrefCollisions()
	{
		foreach (string text in EnumUtils.Names<EnumGamePrefs>())
		{
			ILaunchPref launchPref;
			if (LaunchPrefs.All.TryGetValue(text, out launchPref))
			{
				Log.Error(string.Concat(new string[]
				{
					"Name collision between LaunchPref '",
					launchPref.Name,
					"' and GamePref '",
					text,
					"'."
				}));
				return true;
			}
		}
		return false;
	}

	[Conditional("NEVER_DEFINED")]
	public static void ProfileSection(string identifier)
	{
		if (GameEntrypoint.s_profileTotal == null)
		{
			GameEntrypoint.s_profileTotal = new MicroStopwatch(true);
		}
		if (GameEntrypoint.s_profileSection == null)
		{
			GameEntrypoint.s_profileSection = new MicroStopwatch(true);
		}
		GameEntrypoint.s_profileIdentifier = identifier;
	}

	[Conditional("PROFILE_GAME_ENTRYPOINT")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ProfileSectionEnd()
	{
		if (GameEntrypoint.s_profileIdentifier == null)
		{
			return;
		}
		Log.Out(string.Format("[GameEntrypoint: Profile] Section {0} {1:F3} ms", GameEntrypoint.s_profileIdentifier, GameEntrypoint.s_profileSection.Elapsed.TotalMilliseconds));
		GameEntrypoint.s_profileSection.Restart();
		GameEntrypoint.s_profileIdentifier = null;
	}

	[Conditional("PROFILE_GAME_ENTRYPOINT")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ProfileEnd()
	{
		if (GameEntrypoint.s_profileTotal == null)
		{
			return;
		}
		Log.Out(string.Format("[GameEntrypoint: Profile] TOTAL {0:F3} ms", GameEntrypoint.s_profileTotal.Elapsed.TotalMilliseconds));
		GameEntrypoint.s_profileTotal = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool s_entrypointEntered;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool s_entrypointFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static MicroStopwatch s_profileTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static MicroStopwatch s_profileSection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string s_profileIdentifier;
}
