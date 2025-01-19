using System;
using System.Collections.Generic;
using Challenges;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeGroupList : XUiController
{
	public XUiC_ChallengeGroupEntry SelectedGroup
	{
		get
		{
			return this.selectedGroup;
		}
		set
		{
			if (this.selectedGroup != null)
			{
				this.selectedGroup.UnSelect();
			}
			this.selectedGroup = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ChallengeWindowGroup xuiC_ChallengeWindowGroup = (XUiC_ChallengeWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_ChallengeGroupEntry)
			{
				XUiC_ChallengeGroupEntry xuiC_ChallengeGroupEntry = (XUiC_ChallengeGroupEntry)this.children[i];
				xuiC_ChallengeGroupEntry.Owner = this;
				this.entryList.Add(xuiC_ChallengeGroupEntry);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.CategoryList.CurrentCategory == null)
		{
			return;
		}
		string categoryName = this.CategoryList.CurrentCategory.CategoryName;
		if (this.isDirty)
		{
			int num = 0;
			for (int i = 0; i < this.challengeGroupList.Count; i++)
			{
				XUiC_ChallengeGroupEntry xuiC_ChallengeGroupEntry = this.entryList[num];
				if (xuiC_ChallengeGroupEntry != null && this.challengeGroupList[i].ChallengeGroup.Category.EqualsCaseInsensitive(categoryName))
				{
					xuiC_ChallengeGroupEntry.Entry = this.challengeGroupList[i];
					xuiC_ChallengeGroupEntry.ViewComponent.SoundPlayOnClick = true;
					if (this.categoryChange)
					{
						xuiC_ChallengeGroupEntry.ChallengeList.SelectedEntry = null;
					}
					num++;
				}
				if (num >= this.entryList.Count)
				{
					break;
				}
			}
			for (int j = num; j < this.entryList.Count; j++)
			{
				XUiC_ChallengeGroupEntry xuiC_ChallengeGroupEntry2 = this.entryList[j];
				xuiC_ChallengeGroupEntry2.Entry = null;
				xuiC_ChallengeGroupEntry2.ViewComponent.SoundPlayOnClick = false;
			}
			this.isDirty = false;
			this.categoryChange = false;
		}
	}

	public void SetChallengeGroupEntryList(List<ChallengeGroupEntry> newChallengeGroupList, bool newCategoryChange)
	{
		this.challengeGroupList = newChallengeGroupList;
		if (this.CategoryList != null && this.CategoryList.CurrentCategory == null)
		{
			this.CategoryList.SetCategoryToFirst();
		}
		this.categoryChange = newCategoryChange;
		if (base.xui.QuestTracker.TrackedChallenge != null)
		{
			ChallengeGroup challengeGroup = base.xui.QuestTracker.TrackedChallenge.ChallengeGroup;
			int num = 0;
			while (num < this.challengeGroupList.Count && this.challengeGroupList[num].ChallengeGroup != challengeGroup)
			{
				num++;
			}
		}
		this.isDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = base.xui.playerUI.entityPlayer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeGroupEntry> entryList = new List<XUiC_ChallengeGroupEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupEntry selectedGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryChange;

	public XUiC_CategoryList CategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChallengeGroupEntry> challengeGroupList;

	public XUiC_ChallengeEntryListWindow ChallengeEntryListWindow;
}
