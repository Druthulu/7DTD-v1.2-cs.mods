using System;
using System.Collections.Generic;
using Challenges;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryList : XUiController
{
	public XUiC_ChallengeEntry SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = true;
			}
			this.journalWindowGroup.SetEntry(this.selectedEntry);
		}
	}

	public override void Init()
	{
		base.Init();
		this.journalWindowGroup = (XUiC_ChallengeWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_ChallengeEntry)
			{
				XUiC_ChallengeEntry xuiC_ChallengeEntry = (XUiC_ChallengeEntry)this.children[i];
				xuiC_ChallengeEntry.Owner = this;
				xuiC_ChallengeEntry.JournalUIHandler = this.journalWindowGroup;
				this.entryList.Add(xuiC_ChallengeEntry);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.isDirty)
		{
			ChallengeObjectiveChallengeComplete challengeObjectiveChallengeComplete = null;
			string b = "";
			string b2 = "";
			if (base.xui.QuestTracker.TrackedChallenge != null)
			{
				challengeObjectiveChallengeComplete = base.xui.QuestTracker.TrackedChallenge.GetChallengeCompleteObjective();
				if (challengeObjectiveChallengeComplete != null && challengeObjectiveChallengeComplete.IsRedeemed)
				{
					if (challengeObjectiveChallengeComplete.IsGroup)
					{
						b2 = challengeObjectiveChallengeComplete.ChallengeName;
					}
					else
					{
						b = challengeObjectiveChallengeComplete.ChallengeName;
					}
				}
				else
				{
					challengeObjectiveChallengeComplete = null;
				}
			}
			for (int i = 0; i < this.entryList.Count; i++)
			{
				XUiC_ChallengeEntry xuiC_ChallengeEntry = this.entryList[i];
				if (xuiC_ChallengeEntry != null)
				{
					xuiC_ChallengeEntry.OnPress -= this.OnPressEntry;
					xuiC_ChallengeEntry.Selected = (this.selectedEntry == xuiC_ChallengeEntry || (this.selectedEntry == null && xuiC_ChallengeEntry.Tracked));
					if (i < this.challengeList.Count)
					{
						xuiC_ChallengeEntry.Entry = this.challengeList[i];
						if (xuiC_ChallengeEntry.IsChallengeVisible)
						{
							xuiC_ChallengeEntry.OnPress += this.OnPressEntry;
							xuiC_ChallengeEntry.ViewComponent.SoundPlayOnClick = true;
							xuiC_ChallengeEntry.ViewComponent.SoundPlayOnHover = true;
						}
						else
						{
							xuiC_ChallengeEntry.ViewComponent.SoundPlayOnClick = false;
							xuiC_ChallengeEntry.ViewComponent.SoundPlayOnHover = false;
						}
						xuiC_ChallengeEntry.IsRedeemBlinking = false;
						if (xuiC_ChallengeEntry.Entry.ChallengeState == Challenge.ChallengeStates.Completed && challengeObjectiveChallengeComplete != null && (xuiC_ChallengeEntry.Entry.ChallengeClass.Name.EqualsCaseInsensitive(b) || xuiC_ChallengeEntry.Entry.ChallengeGroup.Name.EqualsCaseInsensitive(b2)))
						{
							xuiC_ChallengeEntry.IsRedeemBlinking = true;
						}
						if (xuiC_ChallengeEntry.Entry.IsTracked && this.selectedEntry == null)
						{
							this.Owner.Select();
							this.SelectedEntry = xuiC_ChallengeEntry;
							this.journalWindowGroup.SetEntry(this.selectedEntry);
							xuiC_ChallengeEntry.SelectCursorElement(true, false);
						}
					}
					else
					{
						xuiC_ChallengeEntry.Entry = null;
						xuiC_ChallengeEntry.ViewComponent.SoundPlayOnClick = false;
					}
				}
			}
			this.isDirty = false;
		}
	}

	public void MarkDirty()
	{
		this.isDirty = true;
	}

	public void UnSelect()
	{
		this.SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		XUiC_ChallengeEntry xuiC_ChallengeEntry = _sender as XUiC_ChallengeEntry;
		if (xuiC_ChallengeEntry != null)
		{
			this.Owner.Select();
			this.SelectedEntry = xuiC_ChallengeEntry;
			this.SelectedEntry.JournalUIHandler.SetEntry(this.SelectedEntry);
			if (InputUtils.ShiftKeyPressed)
			{
				Challenge entry = xuiC_ChallengeEntry.Entry;
				if (entry.IsActive && !entry.IsTracked)
				{
					entry.IsTracked = true;
					base.xui.QuestTracker.TrackedChallenge = entry;
				}
			}
			this.isDirty = true;
		}
	}

	public void SetChallengeEntryList(List<Challenge> newChallengeList)
	{
		this.challengeList = newChallengeList;
		this.isDirty = true;
	}

	public void SetEntryByChallenge(Challenge newChallenge)
	{
		for (int i = 0; i < this.entryList.Count; i++)
		{
			XUiC_ChallengeEntry xuiC_ChallengeEntry = this.entryList[i];
			if (xuiC_ChallengeEntry != null && xuiC_ChallengeEntry.Entry == newChallenge)
			{
				this.SelectedEntry = xuiC_ChallengeEntry;
				return;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = base.xui.playerUI.entityPlayer;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeEntry> entryList = new List<XUiC_ChallengeEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Challenge> challengeList;

	public XUiC_ChallengeEntryListWindow ChallengeEntryListWindow;

	public XUiC_ChallengeGroupEntry Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeWindowGroup journalWindowGroup;
}
