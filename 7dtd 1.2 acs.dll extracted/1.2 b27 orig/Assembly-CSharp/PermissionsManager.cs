﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Platform;

public class PermissionsManager
{
	public static EUserPerms GetPermissions(PermissionsManager.PermissionSources _sources = PermissionsManager.PermissionSources.All)
	{
		EUserPerms euserPerms = EUserPerms.All;
		if (_sources.HasFlag(PermissionsManager.PermissionSources.Platform))
		{
			euserPerms &= PlatformManager.MultiPlatform.User.Permissions;
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.GamePrefs))
		{
			if (euserPerms.HasFlag(EUserPerms.Communication) && !GamePrefs.GetBool(EnumGamePrefs.OptionsChatCommunication))
			{
				euserPerms &= ~EUserPerms.Communication;
			}
			if (euserPerms.HasFlag(EUserPerms.Crossplay) && !GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay))
			{
				euserPerms &= ~EUserPerms.Crossplay;
			}
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.LaunchPrefs) && euserPerms.HasFlag(EUserPerms.Crossplay) && !LaunchPrefs.AllowCrossplay.Value)
		{
			euserPerms &= ~EUserPerms.Crossplay;
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.DebugMask))
		{
			euserPerms &= PermissionsManager.DebugPermissionsMask;
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.TitleStorage) && euserPerms.HasFlag(EUserPerms.Crossplay) && !PermissionsManager.tsOverrides.Crossplay)
		{
			euserPerms &= ~EUserPerms.Crossplay;
		}
		return euserPerms;
	}

	public static IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		yield return null;
		if (_cancellationToken != null && _cancellationToken.IsCancelled())
		{
			yield break;
		}
		bool needsWait = PermissionsManager.resolvingPermissions;
		if (needsWait)
		{
			Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4}) Waiting on existing resolve...", new object[]
			{
				"ResolvePermissions",
				"_perms",
				_perms,
				"_canPrompt",
				_canPrompt
			}));
			while (PermissionsManager.resolvingPermissions)
			{
				yield return null;
				if (_cancellationToken != null && _cancellationToken.IsCancelled())
				{
					yield break;
				}
			}
		}
		try
		{
			PermissionsManager.<>c__DisplayClass5_0 CS$<>8__locals1 = new PermissionsManager.<>c__DisplayClass5_0();
			PermissionsManager.resolvingPermissions = true;
			if (needsWait)
			{
				Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4}) Finished waiting. Executing resolve.", new object[]
				{
					"ResolvePermissions",
					"_perms",
					_perms,
					"_canPrompt",
					_canPrompt
				}));
			}
			CS$<>8__locals1.tsFetchComplete = false;
			if (_perms.HasCrossplay())
			{
				TitleStorageOverridesManager.Instance.FetchFromSource(new Action<TitleStorageOverridesManager.TSOverrides>(CS$<>8__locals1.<ResolvePermissions>g__FetchComplete|0));
			}
			else
			{
				CS$<>8__locals1.tsFetchComplete = true;
			}
			yield return PlatformManager.MultiPlatform.User.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
			if (_cancellationToken != null && _cancellationToken.IsCancelled())
			{
				PermissionsManager.resolvingPermissions = false;
				yield break;
			}
			while (!CS$<>8__locals1.tsFetchComplete)
			{
				if (_cancellationToken != null && _cancellationToken.IsCancelled())
				{
					yield break;
				}
				yield return null;
			}
			CS$<>8__locals1 = null;
		}
		finally
		{
			PermissionsManager.resolvingPermissions = false;
		}
		yield break;
		yield break;
	}

	public static string GetPermissionDenyReason(EUserPerms _perms, PermissionsManager.PermissionSources _sources = PermissionsManager.PermissionSources.All)
	{
		if (_sources.HasFlag(PermissionsManager.PermissionSources.GamePrefs))
		{
			if (_perms.HasFlag(EUserPerms.Communication) && !GamePrefs.GetBool(EnumGamePrefs.OptionsChatCommunication))
			{
				return Localization.Get("permissionsMissing_communication", false);
			}
			if (_perms.HasFlag(EUserPerms.Crossplay) && !GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay))
			{
				return Localization.Get("permissionsMissing_crossplay", false);
			}
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.LaunchPrefs) && _perms.HasFlag(EUserPerms.Crossplay) && !LaunchPrefs.AllowCrossplay.Value)
		{
			return Localization.Get("auth_noCrossplay", false);
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.TitleStorage) && _perms.HasFlag(EUserPerms.Crossplay) && !PermissionsManager.tsOverrides.Crossplay)
		{
			return Localization.Get("auth_noCrossplayOverridden", false);
		}
		if (_sources.HasFlag(PermissionsManager.PermissionSources.Platform))
		{
			string permissionDenyReason = PlatformManager.MultiPlatform.User.GetPermissionDenyReason(_perms);
			if (!string.IsNullOrEmpty(permissionDenyReason))
			{
				return permissionDenyReason;
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAllowed(EUserPerms _checkPerms)
	{
		return PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All).HasFlag(_checkPerms);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsMultiplayerAllowed()
	{
		return PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All).HasMultiplayer();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCommunicationAllowed()
	{
		return PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All).HasCommunication();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCrossplayAllowed()
	{
		return PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All).HasCrossplay();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanHostMultiplayer()
	{
		EUserPerms permissions = PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All);
		return permissions.HasMultiplayer() && permissions.HasHostMultiplayer();
	}

	public static EUserPerms DebugPermissionsMask = EUserPerms.All;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TitleStorageOverridesManager.TSOverrides tsOverrides = default(TitleStorageOverridesManager.TSOverrides);

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool resolvingPermissions;

	[Flags]
	public enum PermissionSources
	{
		Platform = 1,
		GamePrefs = 2,
		LaunchPrefs = 4,
		DebugMask = 8,
		TitleStorage = 16,
		All = 31
	}
}
