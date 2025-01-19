using System;
using System.Collections.Generic;
using Challenges;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeGroupEntry : XUiController
{
	public ChallengeGroupEntry Entry
	{
		get
		{
			return this.entry;
		}
		set
		{
			base.ViewComponent.Enabled = (value != null);
			this.entry = value;
			this.group = ((this.entry != null) ? this.entry.ChallengeGroup : null);
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.ChallengeList = base.GetChildByType<XUiC_ChallengeEntryList>();
		this.IsDirty = true;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		bool flag = this.group != null;
		if (bindingName == "groupname")
		{
			value = "";
			if (flag)
			{
				value = this.group.Title;
			}
			return true;
		}
		if (bindingName == "groupreward")
		{
			value = (flag ? this.group.RewardText : "");
			return true;
		}
		if (bindingName == "resetday")
		{
			if (flag)
			{
				value = ((this.group.DayReset == -1) ? "" : this.entry.LastUpdateDay.ToString());
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (bindingName == "hasreset")
		{
			value = (flag ? (this.group.DayReset != -1).ToString() : "false");
			return true;
		}
		if (!(bindingName == "hasentry"))
		{
			return false;
		}
		value = (flag ? "true" : "false");
		return true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.group != null && this.group.UIDirty)
		{
			this.IsDirty = true;
			this.group.UIDirty = false;
		}
		if (this.IsDirty)
		{
			this.currentItems = (from item in this.player.challengeJournal.Challenges
			where item.ChallengeGroup == this.@group
			select item).ToList<Challenge>();
			if (this.ChallengeList != null)
			{
				this.ChallengeList.Owner = this;
				this.ChallengeList.SetChallengeEntryList(this.currentItems);
			}
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
	}

	public void Select()
	{
		this.Owner.SelectedGroup = this;
	}

	public void UnSelect()
	{
		this.ChallengeList.UnSelect();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = base.xui.playerUI.entityPlayer;
		this.IsDirty = true;
	}

	public void Refresh()
	{
		this.IsDirty = true;
	}

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	public XUiC_ChallengeEntryList ChallengeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Challenge> currentItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroup group;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroupEntry entry;

	public XUiC_ChallengeGroupList Owner;
}
