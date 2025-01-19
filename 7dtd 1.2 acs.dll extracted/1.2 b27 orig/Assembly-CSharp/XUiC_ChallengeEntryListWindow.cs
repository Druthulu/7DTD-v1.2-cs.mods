using System;
using Challenges;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryListWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.categoryList = base.GetChildByType<XUiC_CategoryList>();
		this.challengeList = base.GetChildByType<XUiC_ChallengeGroupList>();
		if (this.challengeList != null)
		{
			this.challengeList.ChallengeEntryListWindow = this;
			this.challengeList.CategoryList = this.categoryList;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.challengeList.SetChallengeGroupEntryList(this.player.challengeJournal.ChallengeGroups, this.categoryChange);
			this.IsDirty = false;
			this.categoryChange = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = base.xui.playerUI.entityPlayer;
		if (this.categoryList != null)
		{
			this.categoryList.SetupCategoriesBasedOnChallengeCategories(ChallengeCategory.s_ChallengeCategories.Values.ToList<ChallengeCategory>());
		}
		if (this.categoryList != null)
		{
			this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		}
		base.RefreshBindings(false);
		if (this.categoryList != null && (this.categoryList.CurrentCategory == null || this.categoryList.CurrentCategory.SpriteName == ""))
		{
			this.categoryList.SetCategoryToFirst();
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		if (this.categoryList != null)
		{
			this.categoryList.CategoryChanged -= this.CategoryList_CategoryChanged;
		}
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.IsDirty = true;
		this.categoryChange = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupList challengeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryChange;
}
