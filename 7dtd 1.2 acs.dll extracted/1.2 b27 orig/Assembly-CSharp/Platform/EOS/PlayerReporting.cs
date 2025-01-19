using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

namespace Platform.EOS
{
	public class PlayerReporting : IPlayerReporting
	{
		public ReportsInterface reportsInterface
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((Api)this.owner.Api).PlatformInterface.GetReportsInterface();
			}
		}

		public ProductUserId localProductUserId
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		public IList<IPlayerReporting.PlayerReportCategory> ReportCategories()
		{
			if (this.reportCategories != null)
			{
				return this.reportCategories.list;
			}
			this.reportCategories = new DictionaryList<PlayerReportsCategory, IPlayerReporting.PlayerReportCategory>();
			foreach (PlayerReportsCategory playerReportsCategory in EnumUtils.Values<PlayerReportsCategory>())
			{
				if (playerReportsCategory != PlayerReportsCategory.Invalid)
				{
					this.reportCategories.Add(playerReportsCategory, new PlayerReporting.PlayerReportCategoryEos(playerReportsCategory, Localization.Get("xuiCategoryPlayerReport" + playerReportsCategory.ToStringCached<PlayerReportsCategory>(), false)));
				}
			}
			return this.reportCategories.list;
		}

		public void ReportPlayer(PlatformUserIdentifierAbs _reportedUserCross, IPlayerReporting.PlayerReportCategory _reportCategory, string _message, Action<bool> _reportCompleteCallback)
		{
			if (_message != null && _message.Length > 256)
			{
				Log.Out("[EOS-Report] Long message, might get truncated");
			}
			EosHelpers.AssertMainThread("PRep.Send");
			SendPlayerBehaviorReportOptions sendPlayerBehaviorReportOptions = new SendPlayerBehaviorReportOptions
			{
				ReporterUserId = this.localProductUserId,
				ReportedUserId = ((UserIdentifierEos)_reportedUserCross).ProductUserId,
				Category = ((PlayerReporting.PlayerReportCategoryEos)_reportCategory).Category,
				Message = _message
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.reportsInterface.SendPlayerBehaviorReport(ref sendPlayerBehaviorReportOptions, null, delegate(ref SendPlayerBehaviorReportCompleteCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != Result.Success)
					{
						Log.Error("[EOS-Report] Reporting player failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						_reportCompleteCallback(false);
						return;
					}
					Log.Out("[EOS-Report] Sent player report");
					_reportCompleteCallback(true);
				});
			}
		}

		public IPlayerReporting.PlayerReportCategory GetPlayerReportCategoryMapping(EnumReportCategory _reportCategory)
		{
			PlayerReportsCategory playerReportsCategory;
			if (_reportCategory != EnumReportCategory.Cheating)
			{
				if (_reportCategory != EnumReportCategory.VerbalAbuse)
				{
					playerReportsCategory = PlayerReportsCategory.Other;
				}
				else
				{
					playerReportsCategory = PlayerReportsCategory.VerbalAbuse;
				}
			}
			else
			{
				playerReportsCategory = PlayerReportsCategory.Cheating;
			}
			PlayerReportsCategory key = playerReportsCategory;
			IPlayerReporting.PlayerReportCategory result;
			if (!this.reportCategories.dict.TryGetValue(key, out result))
			{
				return null;
			}
			return result;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public DictionaryList<PlayerReportsCategory, IPlayerReporting.PlayerReportCategory> reportCategories;

		[PublicizedFrom(EAccessModifier.Private)]
		public class PlayerReportCategoryEos : IPlayerReporting.PlayerReportCategory
		{
			public PlayerReportCategoryEos(PlayerReportsCategory _category, string _displayString)
			{
				this.Category = _category;
				this.displayString = _displayString;
			}

			public override string ToString()
			{
				return this.displayString;
			}

			public readonly PlayerReportsCategory Category;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly string displayString;
		}
	}
}
