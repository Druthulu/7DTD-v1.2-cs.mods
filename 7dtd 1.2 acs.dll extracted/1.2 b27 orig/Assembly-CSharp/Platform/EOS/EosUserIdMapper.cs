using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS
{
	public class EosUserIdMapper : IUserIdentifierMappingService
	{
		public EosUserIdMapper(Api _eosApi, User _eosUser)
		{
			this.api = _eosApi;
			this.user = _eosUser;
		}

		public bool CanQuery(PlatformUserIdentifierAbs _id)
		{
			return _id is UserIdentifierEos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool TryValidateUser(out ProductUserId loggedInUser)
		{
			UserIdentifierEos userIdentifierEos = this.user.PlatformUserId as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				Log.Error(string.Format("[EOS] Cannot query mapped account details. EosUserIdMapper has wrong id type {0}", this.user.PlatformUserId));
				loggedInUser = null;
				return false;
			}
			loggedInUser = userIdentifierEos.ProductUserId;
			if (loggedInUser == null)
			{
				Log.Error(string.Format("[EOS] Cannot query mapped account details. {0} is not logged in", userIdentifierEos));
				return false;
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool TryValidateRequest(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, out ProductUserId _puid)
		{
			UserIdentifierEos userIdentifierEos = _id as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				Log.Error(string.Format("[EOS] Cannot retrieve mapped account details, {0} is not an eos product user id", _id));
				_puid = null;
				return false;
			}
			_puid = userIdentifierEos.ProductUserId;
			if (!EosHelpers.PlatformIdentifierMappings.ContainsKey(_platform))
			{
				Log.Error(string.Format("[EOS] Cannot retrieve mapped acount details, target platform {0} does not map to a known external account type", _platform));
				return false;
			}
			return true;
		}

		public void QueryMappedAccountDetails(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback)
		{
			ProductUserId loggedInUser;
			if (!this.TryValidateUser(out loggedInUser))
			{
				return;
			}
			ProductUserId puid;
			if (!EosUserIdMapper.TryValidateRequest(_id, _platform, out puid))
			{
				return;
			}
			this.QueryMappedExternalAccount(loggedInUser, puid, _platform, _callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueryMappedExternalAccount(ProductUserId _loggedInUser, ProductUserId _puid, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback)
		{
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("QueryEosMappedAccount", delegate(object _)
				{
					this.QueryMappedExternalAccount(_loggedInUser, _puid, _platform, _callback);
				}, null);
				return;
			}
			if (this.api.ConnectInterface == null)
			{
				Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
				_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				return;
			}
			ExternalAccountType externalAccountType;
			if (!EosHelpers.PlatformIdentifierMappings.TryGetValue(_platform, out externalAccountType))
			{
				Log.Out(string.Format("[EOS] Unknown external account type for {0}", _platform));
				_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				return;
			}
			QueryProductUserIdMappingsOptions queryProductUserIdMappingsOptions = default(QueryProductUserIdMappingsOptions);
			queryProductUserIdMappingsOptions.LocalUserId = _loggedInUser;
			queryProductUserIdMappingsOptions.ProductUserIds = new ProductUserId[]
			{
				_puid
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.api.ConnectInterface.QueryProductUserIdMappings(ref queryProductUserIdMappingsOptions, queryProductUserIdMappingsOptions.ProductUserIds, delegate(ref QueryProductUserIdMappingsCallbackInfo _response)
				{
					if (_response.ResultCode != Result.Success)
					{
						Log.Out(string.Format("[EOS] QueryProductUserIdMappings failed {0}", _response.ResultCode));
						_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
						return;
					}
					if (this.api.ConnectInterface == null)
					{
						Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
						_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
						return;
					}
					ProductUserId targetUserId = ((ProductUserId[])_response.ClientData)[0];
					CopyProductUserExternalAccountByAccountTypeOptions copyOptions = default(CopyProductUserExternalAccountByAccountTypeOptions);
					copyOptions.TargetUserId = targetUserId;
					copyOptions.AccountIdType = externalAccountType;
					ExternalAccountInfo externalAccountInfo;
					if (!this.TryCopyResult(copyOptions, out externalAccountInfo))
					{
						_callback(MappedAccountQueryResult.MappingNotFound, _platform, null, null);
						return;
					}
					Log.Out(string.Format("[EOS] found external account for {0}: Type: {1}, Id: {2}", _puid, externalAccountType, externalAccountInfo.AccountId));
					_callback(MappedAccountQueryResult.Success, _platform, externalAccountInfo.AccountId, externalAccountInfo.DisplayName);
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool TryCopyResult(CopyProductUserExternalAccountByAccountTypeOptions _copyOptions, out ExternalAccountInfo _externalAccountInfo)
		{
			object lockObject = AntiCheatCommon.LockObject;
			ExternalAccountInfo? externalAccountInfo;
			Result result;
			lock (lockObject)
			{
				result = this.api.ConnectInterface.CopyProductUserExternalAccountByAccountType(ref _copyOptions, out externalAccountInfo);
			}
			if (result != Result.Success)
			{
				Log.Out(string.Format("[EOS] {0} copy failed. Result: {1}", _copyOptions.TargetUserId, result));
				_externalAccountInfo = default(ExternalAccountInfo);
				return false;
			}
			if (externalAccountInfo == null)
			{
				Log.Out(string.Format("[EOS] {0} copy failed, null info", _copyOptions.TargetUserId));
				_externalAccountInfo = default(ExternalAccountInfo);
				return false;
			}
			_externalAccountInfo = externalAccountInfo.Value;
			return true;
		}

		public void QueryMappedAccountsDetails(IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback)
		{
			if (_requests.Count == 0)
			{
				_callback(_requests);
				return;
			}
			ProductUserId loggedInUser;
			if (!this.TryValidateUser(out loggedInUser))
			{
				return;
			}
			this.QueryMappedExternalAccounts(loggedInUser, _requests, _callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueryMappedExternalAccounts(ProductUserId _loggedInUser, IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback)
		{
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("QueryEosMappedAccounts", delegate(object _)
				{
					this.QueryMappedExternalAccounts(_loggedInUser, _requests, _callback);
				}, null);
				return;
			}
			if (this.api.ConnectInterface == null)
			{
				Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
				foreach (MappedAccountRequest mappedAccountRequest in _requests)
				{
					mappedAccountRequest.Result = MappedAccountQueryResult.QueryFailed;
				}
				_callback(_requests);
			}
			List<ProductUserId> puids = null;
			List<int> requestIndices = null;
			for (int i = 0; i < _requests.Count; i++)
			{
				MappedAccountRequest mappedAccountRequest2 = _requests[i];
				ProductUserId item;
				if (!EosUserIdMapper.TryValidateRequest(mappedAccountRequest2.Id, mappedAccountRequest2.Platform, out item))
				{
					mappedAccountRequest2.Result = MappedAccountQueryResult.QueryFailed;
				}
				else
				{
					if (puids == null)
					{
						puids = new List<ProductUserId>();
					}
					if (requestIndices == null)
					{
						requestIndices = new List<int>();
					}
					puids.Add(item);
					requestIndices.Add(i);
				}
			}
			if (puids == null)
			{
				_callback(_requests);
				return;
			}
			QueryProductUserIdMappingsOptions queryProductUserIdMappingsOptions = default(QueryProductUserIdMappingsOptions);
			queryProductUserIdMappingsOptions.LocalUserId = _loggedInUser;
			queryProductUserIdMappingsOptions.ProductUserIds = puids.ToArray();
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.api.ConnectInterface.QueryProductUserIdMappings(ref queryProductUserIdMappingsOptions, null, delegate(ref QueryProductUserIdMappingsCallbackInfo _response)
				{
					if (_response.ResultCode != Result.Success)
					{
						Log.Out(string.Format("[EOS] QueryProductUserIdMappings failed {0}", _response.ResultCode));
						foreach (MappedAccountRequest mappedAccountRequest3 in _requests)
						{
							mappedAccountRequest3.Result = MappedAccountQueryResult.QueryFailed;
						}
						_callback(_requests);
						return;
					}
					if (this.api.ConnectInterface == null)
					{
						Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
						foreach (MappedAccountRequest mappedAccountRequest4 in _requests)
						{
							mappedAccountRequest4.Result = MappedAccountQueryResult.QueryFailed;
						}
						_callback(_requests);
						return;
					}
					CopyProductUserExternalAccountByAccountTypeOptions copyOptions = default(CopyProductUserExternalAccountByAccountTypeOptions);
					for (int j = 0; j < puids.Count; j++)
					{
						int index = requestIndices[j];
						MappedAccountRequest mappedAccountRequest5 = _requests[index];
						ProductUserId productUserId = puids[j];
						copyOptions.TargetUserId = productUserId;
						copyOptions.AccountIdType = EosHelpers.PlatformIdentifierMappings[mappedAccountRequest5.Platform];
						ExternalAccountInfo externalAccountInfo;
						if (!this.TryCopyResult(copyOptions, out externalAccountInfo))
						{
							mappedAccountRequest5.Result = MappedAccountQueryResult.MappingNotFound;
						}
						else
						{
							mappedAccountRequest5.MappedAccountId = externalAccountInfo.AccountId;
							mappedAccountRequest5.DisplayName = externalAccountInfo.DisplayName;
							mappedAccountRequest5.Result = MappedAccountQueryResult.Success;
							Log.Out(string.Format("[EOS] found external account for {0}: Type: {1}, Id: {2}", productUserId, externalAccountInfo.AccountIdType, externalAccountInfo.AccountId));
						}
					}
					_callback(_requests);
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Api api;

		[PublicizedFrom(EAccessModifier.Private)]
		public User user;
	}
}
