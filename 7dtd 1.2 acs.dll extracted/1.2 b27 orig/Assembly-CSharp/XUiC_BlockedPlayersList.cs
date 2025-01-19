using System;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BlockedPlayersList : XUiController
{
	public bool IsVisible
	{
		get
		{
			return this.mainRect.IsVisible;
		}
		set
		{
			this.mainRect.IsVisible = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.mainRect = (XUiV_Rect)base.GetChildById("blockedPlayersRect").ViewComponent;
		this.noClick = (XUiV_Panel)base.GetChildById("noClick").ViewComponent;
		this.blockedPlayerList = (XUiV_Grid)base.GetChildById("blockList").ViewComponent;
		this.blockedEntries = base.GetChildrenByType<XUiC_PlayersBlockedListEntry>(null);
		this.blockedPager = (XUiC_Paging)base.GetChildById("blockedPager");
		this.blockedPager.OnPageChanged += this.updateBlockedList;
		this.blockedCounter = (XUiV_Label)base.GetChildById("blockedCounter").ViewComponent;
		this.recentPlayerList = (XUiV_Grid)base.GetChildById("recentList").ViewComponent;
		this.recentEntries = base.GetChildrenByType<XUiC_PlayersRecentListEntry>(null);
		this.recentPager = (XUiC_Paging)base.GetChildById("recentPager");
		this.recentPager.OnPageChanged += this.updateRecentList;
		this.recentCounter = (XUiV_Label)base.GetChildById("recentCounter").ViewComponent;
		for (int i = 0; i < this.blockedEntries.Length; i++)
		{
			this.blockedEntries[i].BlockList = this;
			this.blockedEntries[i].IsAlternating = (i % 2 == 0);
		}
		for (int j = 0; j < this.recentEntries.Length; j++)
		{
			this.recentEntries[j].BlockList = this;
			this.recentEntries[j].IsAlternating = (j % 2 == 0);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (BlockedPlayerList.Instance == null)
		{
			return;
		}
		this.noClick.Enabled = false;
		this.IsVisible = true;
		this.blockedPager.Reset();
		this.recentPager.Reset();
		BlockedPlayerList.Instance.UpdatePlayersSeenInWorld(GameManager.Instance.World);
		ThreadManager.StartCoroutine(BlockedPlayerList.Instance.ResolveUserDetails());
		this.updateBlockedList();
		this.updateRecentList();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (BlockedPlayerList.Instance == null)
		{
			return;
		}
		if (!this.IsDirty)
		{
			return;
		}
		this.IsDirty = false;
		this.updateBlockedList();
		this.updateRecentList();
		base.RefreshBindings(false);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "blockedCount")
		{
			_value = string.Format("{0}/{1}", BlockedPlayerList.Instance.EntryCount(true, false), 500);
			return true;
		}
		if (!(_bindingName == "recentCount"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = string.Format("{0}/{1}", BlockedPlayerList.Instance.EntryCount(false, false), 100);
		return true;
	}

	public void DisplayMessage(string _header, string _message)
	{
		this.noClick.Enabled = true;
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, _header, _message, XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, new Action(this.<DisplayMessage>g__DisableNoClick|18_0), new Action(this.<DisplayMessage>g__DisableNoClick|18_0), false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockedList()
	{
		for (int i = 0; i < this.blockedPlayerList.Rows; i++)
		{
			this.blockedEntries[i].Clear();
		}
		if (BlockedPlayerList.Instance.PendingResolve())
		{
			this.blockedEntries[0].PlayerName.SetGenericName(Localization.Get("xuiFetchingData", false));
			this.IsDirty = true;
			return;
		}
		this.blockedPager.SetLastPageByElementsAndPageLength(BlockedPlayerList.Instance.EntryCount(true, true), this.blockedPlayerList.Rows);
		int num = this.blockedPlayerList.Rows * this.blockedPager.GetPage();
		int num2 = 0;
		foreach (BlockedPlayerList.ListEntry listEntry in BlockedPlayerList.Instance.GetEntriesOrdered(true, true))
		{
			if (num2 < num)
			{
				num2++;
			}
			else
			{
				int num3 = num2 - num;
				if (num3 >= this.blockedPlayerList.Rows)
				{
					break;
				}
				this.blockedEntries[num3].UpdateEntry(listEntry.PlayerData.PrimaryId);
				num2++;
			}
		}
		for (int j = num2 - num; j < this.blockedPlayerList.Rows; j++)
		{
			this.blockedEntries[j].Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRecentList()
	{
		for (int i = 0; i < this.recentPlayerList.Rows; i++)
		{
			this.recentEntries[i].Clear();
		}
		if (BlockedPlayerList.Instance.PendingResolve())
		{
			this.recentEntries[0].PlayerName.SetGenericName(Localization.Get("xuiFetchingData", false));
			this.IsDirty = true;
			return;
		}
		this.recentPager.SetLastPageByElementsAndPageLength(BlockedPlayerList.Instance.EntryCount(false, true), this.recentPlayerList.Rows);
		int num = this.recentPlayerList.Rows * this.recentPager.GetPage();
		int num2 = 0;
		foreach (BlockedPlayerList.ListEntry listEntry in BlockedPlayerList.Instance.GetEntriesOrdered(false, true))
		{
			if (num2 < num)
			{
				num2++;
			}
			else
			{
				int num3 = num2 - num;
				if (num3 >= this.recentPlayerList.Rows)
				{
					break;
				}
				this.recentEntries[num3].UpdateEntry(listEntry.PlayerData.PrimaryId);
				num2++;
			}
		}
		for (int j = num2 - num; j < this.recentPlayerList.Rows; j++)
		{
			this.recentEntries[j].Clear();
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <DisplayMessage>g__DisableNoClick|18_0()
	{
		this.noClick.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect mainRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel noClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid blockedPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersBlockedListEntry[] blockedEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging blockedPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label blockedCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid recentPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersRecentListEntry[] recentEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging recentPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label recentCounter;
}
