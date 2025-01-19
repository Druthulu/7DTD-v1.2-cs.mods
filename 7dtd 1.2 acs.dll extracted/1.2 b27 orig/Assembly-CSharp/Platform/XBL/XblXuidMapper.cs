using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Platform.XBL
{
	public static class XblXuidMapper
	{
		public static event XblXuidMapper.XuidMappedHandler XuidMapped;

		public static void Enable()
		{
			if (XblXuidMapper.s_enabled)
			{
				return;
			}
			XblXuidMapper.s_enabled = true;
			Log.Out("[XBL-XuidMapper] Enabled.");
		}

		public static ulong GetXuid(PlatformUserIdentifierAbs userId)
		{
			if (!XblXuidMapper.s_enabled)
			{
				return 0UL;
			}
			XblXuidMapper.XuidState xuidState = XblXuidMapper.GetXuidState(userId);
			XblXuidMapper.XuidState obj = xuidState;
			lock (obj)
			{
				ulong xuid = xuidState.Xuid;
				if (xuidState.Xuid != 0UL)
				{
					return xuid;
				}
			}
			if (XblXuidMapper.s_mappingService == null)
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				XblXuidMapper.s_mappingService = ((crossplatformPlatform != null) ? crossplatformPlatform.IdMappingService : null);
			}
			if (XblXuidMapper.s_mappingService == null)
			{
				Log.Error("[XBL-XuidMapper] ID mapping service required to identify Xbl users");
				return 0UL;
			}
			XblXuidMapper.ResolveXuid(xuidState);
			return 0UL;
		}

		public static void SetXuid(PlatformUserIdentifierAbs userId, ulong xuid)
		{
			if (xuid == 0UL)
			{
				return;
			}
			XblXuidMapper.XuidState xuidState = XblXuidMapper.GetXuidState(userId);
			XblXuidMapper.XuidState obj = xuidState;
			ulong xuid2;
			lock (obj)
			{
				xuid2 = xuidState.Xuid;
				if (xuid2 == xuid)
				{
					return;
				}
				xuidState.Xuid = xuid;
				xuidState.InProgress = false;
				xuidState.AttemptsCompleted++;
			}
			if (xuid2 != 0UL)
			{
				using (XblXuidMapper.s_xuidStateToUserIdLock.ReadLockScope())
				{
					IReadOnlyCollection<PlatformUserIdentifierAbs> values;
					XblXuidMapper.s_xuidStateToUserId.TryGetByKey(xuidState, out values);
					Log.Warning(string.Format("[XBL-XuidMapper] Unexpected mapping change Xuid changed from '{0}' to '{1}' for UserIds: {2}", xuid2, xuid, string.Join<PlatformUserIdentifierAbs>(", ", values)));
				}
			}
			object obj2 = XblXuidMapper.s_xuidMappedResultTempLock;
			lock (obj2)
			{
				XblXuidMapper.s_xuidMappedResultTemp.Clear();
				using (XblXuidMapper.s_xuidStateToUserIdLock.ReadLockScope())
				{
					XblXuidMapper.s_xuidStateToUserId.TryGetByKey(xuidState, XblXuidMapper.s_xuidMappedResultTemp);
				}
				XblXuidMapper.XuidMappedHandler xuidMapped = XblXuidMapper.XuidMapped;
				if (xuidMapped != null)
				{
					xuidMapped(XblXuidMapper.s_xuidMappedResultTemp, xuid);
				}
				XblXuidMapper.s_xuidMappedResultTemp.Clear();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ResolveXuid(XblXuidMapper.XuidState xuidState)
		{
			XblXuidMapper.<>c__DisplayClass16_0 CS$<>8__locals1 = new XblXuidMapper.<>c__DisplayClass16_0();
			CS$<>8__locals1.xuidState = xuidState;
			CS$<>8__locals1.userIds = null;
			XblXuidMapper.XuidState xuidState2 = CS$<>8__locals1.xuidState;
			lock (xuidState2)
			{
				if (CS$<>8__locals1.xuidState.InProgress)
				{
					return;
				}
				using (XblXuidMapper.s_xuidStateToUserIdLock.ReadLockScope())
				{
					IReadOnlyCollection<PlatformUserIdentifierAbs> readOnlyCollection;
					if (XblXuidMapper.s_xuidStateToUserId.TryGetByKey(CS$<>8__locals1.xuidState, out readOnlyCollection))
					{
						foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in readOnlyCollection)
						{
							if (XblXuidMapper.s_mappingService.CanQuery(platformUserIdentifierAbs))
							{
								using (XblXuidMapper.s_userIdsWithNoXblMappingLock.ReadLockScope())
								{
									if (XblXuidMapper.s_userIdsWithNoXblMapping.Contains(platformUserIdentifierAbs))
									{
										continue;
									}
								}
								if (CS$<>8__locals1.userIds == null)
								{
									CS$<>8__locals1.userIds = new List<PlatformUserIdentifierAbs>();
								}
								CS$<>8__locals1.userIds.Add(platformUserIdentifierAbs);
							}
						}
					}
				}
				if (CS$<>8__locals1.userIds == null || CS$<>8__locals1.userIds.Count <= 0)
				{
					return;
				}
				CS$<>8__locals1.attempt = CS$<>8__locals1.xuidState.AttemptsCompleted + 1;
				CS$<>8__locals1.xuidState.InProgress = true;
			}
			CS$<>8__locals1.userIdsIndex = -1;
			CS$<>8__locals1.<ResolveXuid>g__ProcessNextId|1();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static XblXuidMapper.XuidState GetXuidState(PlatformUserIdentifierAbs userId)
		{
			XblXuidMapper.XuidState result;
			using (XblXuidMapper.s_xuidStateToUserIdLock.UpgradableReadLockScope())
			{
				XblXuidMapper.XuidState xuidState;
				if (XblXuidMapper.s_xuidStateToUserId.TryGetByValue(userId, out xuidState))
				{
					result = xuidState;
				}
				else
				{
					using (XblXuidMapper.s_xuidStateToUserIdLock.WriteLockScope())
					{
						PlatformUserIdentifierAbs[] array = new PlatformUserIdentifierAbs[4];
						int num = PlatformUserManager.TryGetByNative(userId, array.AsSpan(0, array.Length - 1));
						array[num] = userId;
						int num2 = num + 1;
						int num3 = 0;
						while (num3 < num2 && !XblXuidMapper.s_xuidStateToUserId.TryGetByValue(array[num3], out xuidState))
						{
							num3++;
						}
						if (xuidState == null)
						{
							xuidState = new XblXuidMapper.XuidState();
						}
						for (int i = 0; i < num2; i++)
						{
							PlatformUserIdentifierAbs value = array[i];
							XblXuidMapper.XuidState xuidState2;
							if (!XblXuidMapper.s_xuidStateToUserId.TryGetByValue(value, out xuidState2))
							{
								XblXuidMapper.s_xuidStateToUserId.Add(xuidState, value);
							}
							else if (xuidState2 != xuidState)
							{
								XblXuidMapper.s_xuidStateToUserId.RemoveByValue(value);
								XblXuidMapper.s_xuidStateToUserId.Add(xuidState, value);
								Log.Error(string.Format("[XBL-XuidMapper] Unexpected state merge. UserId '{0}' already had state but has been merged with UserIds: '{1}'.", array[i], string.Join<PlatformUserIdentifierAbs>("', '", array.Take(num2))));
							}
						}
					}
					result = xuidState;
				}
			}
			return result;
		}

		public static void ResolveXuids(IReadOnlyList<XuidResolveRequest> _requests, Action<IReadOnlyList<XuidResolveRequest>> _onComplete)
		{
			XblXuidMapper.<>c__DisplayClass19_0 CS$<>8__locals1 = new XblXuidMapper.<>c__DisplayClass19_0();
			CS$<>8__locals1._requests = _requests;
			CS$<>8__locals1._onComplete = _onComplete;
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			IUserIdentifierMappingService userIdentifierMappingService = (crossplatformPlatform != null) ? crossplatformPlatform.IdMappingService : null;
			if (userIdentifierMappingService == null)
			{
				Log.Error("[XBL-XuidMapper] Cannot resolve xuids, no mapping service available");
				CS$<>8__locals1._onComplete(CS$<>8__locals1._requests);
				return;
			}
			List<MappedAccountRequest> list = null;
			CS$<>8__locals1.requestIndices = null;
			for (int i = 0; i < CS$<>8__locals1._requests.Count; i++)
			{
				XuidResolveRequest xuidResolveRequest = CS$<>8__locals1._requests[i];
				XblXuidMapper.XuidState xuidState = XblXuidMapper.GetXuidState(xuidResolveRequest.Id);
				XblXuidMapper.XuidState obj = xuidState;
				lock (obj)
				{
					if (xuidState.Xuid == 0UL)
					{
						using (XblXuidMapper.s_xuidStateToUserIdLock.ReadLockScope())
						{
							IReadOnlyCollection<PlatformUserIdentifierAbs> readOnlyCollection;
							if (XblXuidMapper.s_xuidStateToUserId.TryGetByKey(xuidState, out readOnlyCollection))
							{
								foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in readOnlyCollection)
								{
									if (userIdentifierMappingService.CanQuery(platformUserIdentifierAbs))
									{
										using (XblXuidMapper.s_userIdsWithNoXblMappingLock.ReadLockScope())
										{
											if (XblXuidMapper.s_userIdsWithNoXblMapping.Contains(platformUserIdentifierAbs))
											{
												continue;
											}
										}
										if (list == null)
										{
											list = new List<MappedAccountRequest>();
										}
										if (CS$<>8__locals1.requestIndices == null)
										{
											CS$<>8__locals1.requestIndices = new List<int>();
										}
										list.Add(new MappedAccountRequest(platformUserIdentifierAbs, EPlatformIdentifier.XBL));
										CS$<>8__locals1.requestIndices.Add(i);
										break;
									}
								}
							}
							goto IL_179;
						}
					}
					xuidResolveRequest.Xuid = xuidState.Xuid;
					xuidResolveRequest.IsSuccess = true;
				}
				IL_179:;
			}
			if (list == null)
			{
				CS$<>8__locals1._onComplete(CS$<>8__locals1._requests);
				return;
			}
			userIdentifierMappingService.QueryMappedAccountsDetails(list, new MappedAccountsQueryCallback(CS$<>8__locals1.<ResolveXuids>g__Callback|0));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool s_enabled;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IUserIdentifierMappingService s_mappingService;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly BiMultiDictionary<XblXuidMapper.XuidState, PlatformUserIdentifierAbs> s_xuidStateToUserId = new BiMultiDictionary<XblXuidMapper.XuidState, PlatformUserIdentifierAbs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ReaderWriterLockSlim s_xuidStateToUserIdLock = new ReaderWriterLockSlim();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly HashSet<PlatformUserIdentifierAbs> s_userIdsWithNoXblMapping = new HashSet<PlatformUserIdentifierAbs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ReaderWriterLockSlim s_userIdsWithNoXblMappingLock = new ReaderWriterLockSlim();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly List<PlatformUserIdentifierAbs> s_xuidMappedResultTemp = new List<PlatformUserIdentifierAbs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly object s_xuidMappedResultTempLock = new object();

		public delegate void XuidMappedHandler(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds, ulong xuid);

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class XuidState
		{
			public ulong Xuid;

			public int AttemptsCompleted;

			public bool InProgress;
		}
	}
}
