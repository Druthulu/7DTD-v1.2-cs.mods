using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMPlayersList : XUiC_DMBaseList<XUiC_DMPlayersList.ListEntry>
{
	public bool HasBlockedPlayers
	{
		get
		{
			return this.BlockedPlayerCount > 0;
		}
	}

	public int BlockedPlayerCount
	{
		get
		{
			return this.blockedPlayers.Count;
		}
	}

	public IEnumerable<SaveInfoProvider.PlayerEntryInfo> BlockedPlayers
	{
		get
		{
			return this.blockedPlayers;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("lblPlayerLimit");
		this.lblPlayerLimit = (((childById != null) ? childById.ViewComponent : null) as XUiV_Label);
		this.loadingView = base.GetChildById("loadingOverlay").ViewComponent;
		this.loadingView.IsVisible = false;
		this.lblLoadingText = (base.GetChildById("lblLoadingText").ViewComponent as XUiV_Label);
		this.ellipsisAnimator = new TextEllipsisAnimator(this.lblLoadingText.Text, this.lblLoadingText);
		this.blockedPlayers = new List<SaveInfoProvider.PlayerEntryInfo>();
		this.profileButtons = new List<XUiController>();
		base.GetChildrenById("btnProfile", this.profileButtons);
		foreach (XUiController xuiController in this.profileButtons)
		{
			xuiController.OnPress += this.ProfileButtonOnPress;
			xuiController.OnHover += base.ChildElementHovered;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfileButtonOnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < this.profileButtons.Count; i++)
		{
			if (_sender == this.profileButtons[i])
			{
				this.ShowProfileForEntry(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowProfileForEntry(int _index)
	{
		if (_index < 0 || _index >= this.listEntryControllers.Length)
		{
			Log.Error(string.Format("ProfileButton index out of range. Index: {0}", _index));
			return;
		}
		XUiC_DMPlayersList.ListEntry entry = this.listEntryControllers[_index].GetEntry();
		if (entry == null)
		{
			Log.Error("ProfileButton pressed for empty entry");
			return;
		}
		if (entry.nativeUserId == null)
		{
			Log.Error("ProfileButton pressed for null user id");
			return;
		}
		PlatformManager.MultiPlatform.User.ShowProfile(entry.nativeUserId);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.PlayerEntryInfo> playerEntryInfos, bool _resetFilter = false)
	{
		this.ClearList(false);
		foreach (SaveInfoProvider.PlayerEntryInfo playerEntryInfo in playerEntryInfos)
		{
			IPlatformUserBlockedData platformUserBlockedData;
			if (playerEntryInfo.PlatformUserData != null && playerEntryInfo.PlatformUserData.Blocked.TryGetValue(EBlockType.Play, out platformUserBlockedData) && platformUserBlockedData.State != EUserBlockState.NotBlocked)
			{
				this.blockedPlayers.Add(playerEntryInfo);
			}
			else
			{
				this.allEntries.Add(new XUiC_DMPlayersList.ListEntry(playerEntryInfo));
			}
		}
		if (this.lblPlayerLimit != null)
		{
			this.lblPlayerLimit.Text = string.Format("{0}/{1}", this.allEntries.Count + this.BlockedPlayerCount, 100);
		}
		this.loadingView.IsVisible = false;
		base.RebuildList(_resetFilter);
	}

	public void ClearList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		if (this.lblPlayerLimit != null)
		{
			this.lblPlayerLimit.Text = string.Empty;
		}
		this.blockedPlayers.Clear();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		base.OnSearchInputChanged(_sender, _text, _changeFromCode);
	}

	public void ShowLoading()
	{
		this.loadingView.IsVisible = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.loadingView.IsVisible)
		{
			this.ellipsisAnimator.GetNextAnimatedString(_dt);
		}
	}

	public string filter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPlayerLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView loadingView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLoadingText;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> profileButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.PlayerEntryInfo> blockedPlayers;

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public ListEntry(SaveInfoProvider.PlayerEntryInfo playerEntryInfo)
		{
			this.playerEntryInfo = playerEntryInfo;
			this.id = playerEntryInfo.Id;
			this.cachedName = playerEntryInfo.CachedName;
			IPlatformUserData platformUserData = playerEntryInfo.PlatformUserData;
			this.playerName = ((platformUserData != null) ? platformUserData.Name : null);
			this.platform = playerEntryInfo.PlatformName;
			this.saveSize = playerEntryInfo.Size;
			this.lastPlayed = playerEntryInfo.LastPlayed;
			this.playerLevel = playerEntryInfo.PlayerLevel;
			this.distanceWalked = playerEntryInfo.DistanceWalked;
			this.nativeUserId = playerEntryInfo.NativeUserId;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CanShowProfile()
		{
			return this.nativeUserId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.nativeUserId);
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_DMPlayersList.ListEntry listEntry = _otherEntry as XUiC_DMPlayersList.ListEntry;
			if (listEntry != null)
			{
				return this.playerEntryInfo.CompareTo(listEntry.playerEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1566407741U)
			{
				if (num <= 709505714U)
				{
					if (num != 205488363U)
					{
						if (num == 709505714U)
						{
							if (_bindingName == "platform")
							{
								_value = this.platform;
								return true;
							}
						}
					}
					else if (_bindingName == "savename")
					{
						_value = (this.playerName ?? this.cachedName);
						return true;
					}
				}
				else if (num != 783488098U)
				{
					if (num == 1566407741U)
					{
						if (_bindingName == "hasentry")
						{
							_value = true.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "distance")
				{
					_value = ((this.playerLevel < 1) ? "-" : string.Format("{0} {1}", (int)(this.distanceWalked / 1000f), Localization.Get("xuiKMTravelled", false)));
					return true;
				}
			}
			else if (num <= 1823525230U)
			{
				if (num != 1800901934U)
				{
					if (num == 1823525230U)
					{
						if (_bindingName == "lastplayedinfo")
						{
							int num2 = (int)(DateTime.Now - this.lastPlayed).TotalDays;
							_value = string.Format("{0} {1}", num2, Localization.Get("xuiDmDaysAgo", false));
							return true;
						}
					}
				}
				else if (_bindingName == "lastplayed")
				{
					_value = this.lastPlayed.ToString("yyyy-MM-dd HH:mm");
					return true;
				}
			}
			else if (num != 2610554845U)
			{
				if (num == 3266695369U)
				{
					if (_bindingName == "canShowProfile")
					{
						_value = this.CanShowProfile().ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "level")
			{
				_value = ((this.playerLevel < 1) ? "-" : string.Format("{0} {1}", Localization.Get("xuiLevel", false), this.playerLevel));
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return (this.playerName ?? this.cachedName).ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1566407741U)
			{
				if (num <= 709505714U)
				{
					if (num != 205488363U)
					{
						if (num != 709505714U)
						{
							return false;
						}
						if (!(_bindingName == "platform"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "savename"))
					{
						return false;
					}
				}
				else if (num != 783488098U)
				{
					if (num != 1566407741U)
					{
						return false;
					}
					if (!(_bindingName == "hasentry"))
					{
						return false;
					}
					_value = false.ToString();
					return true;
				}
				else if (!(_bindingName == "distance"))
				{
					return false;
				}
			}
			else if (num <= 1823525230U)
			{
				if (num != 1800901934U)
				{
					if (num != 1823525230U)
					{
						return false;
					}
					if (!(_bindingName == "lastplayedinfo"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "lastplayed"))
				{
					return false;
				}
			}
			else if (num != 2610554845U)
			{
				if (num != 3266695369U)
				{
					return false;
				}
				if (!(_bindingName == "canShowProfile"))
				{
					return false;
				}
				_value = false.ToString();
				return true;
			}
			else if (!(_bindingName == "level"))
			{
				return false;
			}
			_value = "";
			return true;
		}

		public readonly string id;

		public readonly string cachedName;

		public readonly string playerName;

		public readonly string platform;

		public readonly DateTime lastPlayed;

		public readonly int playerLevel;

		public readonly float distanceWalked;

		public readonly long saveSize;

		public readonly PlatformUserIdentifierAbs nativeUserId;

		public readonly SaveInfoProvider.PlayerEntryInfo playerEntryInfo;
	}
}
