﻿using System;
using System.Collections;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Sanctions;
using JetBrains.Annotations;
using Pathfinding.Util;

namespace Platform.EOS
{
	public class SanctionsCheck
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator WaitUntilSanctionsCheck(CoroutineCancellationToken _cancellationToken = null)
		{
			while (this.queryInProgress)
			{
				if (_cancellationToken != null && _cancellationToken.IsCancelled())
				{
					yield break;
				}
				yield return null;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public IEnumerator CheckSanctionsEnumerator(SanctionsInterface _sanctionsInterface, ProductUserId _productUserId, [CanBeNull] ProductUserId _localUser, Action<SanctionsCheckResult> callback, CoroutineCancellationToken _cancellationToken = null)
		{
			if (this.queryInProgress)
			{
				yield return this.WaitUntilSanctionsCheck(_cancellationToken);
			}
			CoroutineCancellationToken cancellationToken = _cancellationToken;
			if (cancellationToken != null && cancellationToken.IsCancelled())
			{
				yield break;
			}
			this.queryInProgress = true;
			this.sanctionsInterface = _sanctionsInterface;
			this.productUserId = _productUserId;
			QueryActivePlayerSanctionsOptions queryActivePlayerSanctionsOptions = new QueryActivePlayerSanctionsOptions
			{
				TargetUserId = this.productUserId,
				LocalUserId = _localUser
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sanctionsInterface.QueryActivePlayerSanctions(ref queryActivePlayerSanctionsOptions, null, delegate(ref QueryActivePlayerSanctionsCallbackInfo data)
				{
					CoroutineCancellationToken cancellationToken2 = _cancellationToken;
					if (cancellationToken2 != null && cancellationToken2.IsCancelled())
					{
						return;
					}
					this.OnSanctionsQueryResolveAndGatherSanctions(ref data, callback);
				});
			}
			yield return this.WaitUntilSanctionsCheck(_cancellationToken);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public void CheckSanctions(SanctionsInterface _sanctionsInterface, ProductUserId _productUserId, [CanBeNull] ProductUserId _localUser, Action<SanctionsCheckResult> callback)
		{
			if (this.queryInProgress)
			{
				ThreadManager.StartCoroutine(this.CheckSanctionsEnumerator(_sanctionsInterface, _productUserId, _localUser, callback, null));
				return;
			}
			this.queryInProgress = true;
			this.sanctionsInterface = _sanctionsInterface;
			this.productUserId = _productUserId;
			QueryActivePlayerSanctionsOptions queryActivePlayerSanctionsOptions = new QueryActivePlayerSanctionsOptions
			{
				TargetUserId = this.productUserId,
				LocalUserId = _localUser
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sanctionsInterface.QueryActivePlayerSanctions(ref queryActivePlayerSanctionsOptions, null, delegate(ref QueryActivePlayerSanctionsCallbackInfo data)
				{
					this.OnSanctionsQueryResolveAndGatherSanctions(ref data, callback);
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnSanctionsQueryResolveAndGatherSanctions(ref QueryActivePlayerSanctionsCallbackInfo data, Action<SanctionsCheckResult> callback)
		{
			if (data.ResultCode == Result.NotFound)
			{
				Log.Out("[EOS] Player has no sanctions");
				this.queryInProgress = false;
				List<EOSSanction> currentCheckSanctions = this.CurrentCheckSanctions;
				if (currentCheckSanctions != null)
				{
					currentCheckSanctions.ClearFast<EOSSanction>();
				}
				if (callback != null)
				{
					callback(new SanctionsCheckResult(null));
				}
				return;
			}
			if (data.ResultCode != Result.Success)
			{
				Log.Out(string.Format("[EOS] Sanctions query failed {0}", data.ResultCode));
				if (data.ResultCode == Result.OperationWillRetry)
				{
					return;
				}
				this.queryInProgress = false;
				List<EOSSanction> currentCheckSanctions2 = this.CurrentCheckSanctions;
				if (currentCheckSanctions2 != null)
				{
					currentCheckSanctions2.ClearFast<EOSSanction>();
				}
				if (callback != null)
				{
					callback(new SanctionsCheckResult(default(DateTime), GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 5, data.ResultCode.ToStringCached<Result>()));
				}
			}
			else
			{
				Log.Out("[EOS] Player may have active sanctions");
				GetPlayerSanctionCountOptions getPlayerSanctionCountOptions = new GetPlayerSanctionCountOptions
				{
					TargetUserId = this.productUserId
				};
				object lockObject = AntiCheatCommon.LockObject;
				uint playerSanctionCount;
				lock (lockObject)
				{
					playerSanctionCount = this.sanctionsInterface.GetPlayerSanctionCount(ref getPlayerSanctionCountOptions);
				}
				this.CurrentCheckSanctions = new List<EOSSanction>();
				for (uint num = 0U; num < playerSanctionCount; num += 1U)
				{
					CopyPlayerSanctionByIndexOptions copyPlayerSanctionByIndexOptions = new CopyPlayerSanctionByIndexOptions
					{
						SanctionIndex = num,
						TargetUserId = this.productUserId
					};
					lockObject = AntiCheatCommon.LockObject;
					lock (lockObject)
					{
						PlayerSanction? playerSanction;
						if (this.sanctionsInterface.CopyPlayerSanctionByIndex(ref copyPlayerSanctionByIndexOptions, out playerSanction) == Result.Success)
						{
							Log.Error("[EOS] Sanction found: " + (((playerSanction != null) ? playerSanction.GetValueOrDefault().Action : null) ?? "Empty Action").ToString());
							if (playerSanction != null && playerSanction.GetValueOrDefault().Action.ToString().Equals("RESTRICT_MATCHMAKING"))
							{
								Log.Error("[EOS] Sanction in place");
								if (playerSanction.Value.TimeExpires == 0L)
								{
									Log.Out("[EOS] Sanctioned Until: Forever");
									this.CurrentCheckSanctions.Add(new EOSSanction(new DateTime?(DateTime.MaxValue), playerSanction.Value.ReferenceId));
									break;
								}
								DateTime value = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
								value = value.AddSeconds((double)playerSanction.Value.TimeExpires).ToLocalTime();
								Log.Out("[EOS] Sanctioned Until: " + value.ToLongDateString());
								this.CurrentCheckSanctions.Add(new EOSSanction(new DateTime?(value), playerSanction.Value.ReferenceId));
							}
						}
					}
				}
			}
			string format = "[EOS] SanctionsQuery finished with: {0} sanctions";
			List<EOSSanction> currentCheckSanctions3 = this.CurrentCheckSanctions;
			Log.Out(string.Format(format, (currentCheckSanctions3 != null) ? currentCheckSanctions3.Count : 0));
			this.queryInProgress = false;
			if (callback != null)
			{
				callback(new SanctionsCheckResult(this.CurrentCheckSanctions));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ProductUserId productUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public SanctionsInterface sanctionsInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool queryInProgress;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<EOSSanction> CurrentCheckSanctions;
	}
}
