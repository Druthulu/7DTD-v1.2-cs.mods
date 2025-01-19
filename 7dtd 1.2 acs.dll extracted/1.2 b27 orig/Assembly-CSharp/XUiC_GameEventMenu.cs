using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GameEventMenu : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_GameEventMenu.ID = base.WindowGroup.ID;
		this.categoryList = (XUiC_CategoryList)base.GetChildById("categories");
		this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		this.gameEventsList = (XUiC_GameEventsList)base.GetChildById("gameevents");
		this.gameEventsList.SelectionChanged += this.EntitiesList_SelectionChanged;
		this.cbxTarget = (XUiC_ComboBoxList<string>)base.GetChildById("cbxTarget");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.gameEventsList.Category = _categoryEntry.CategoryName;
		this.categoryDisplay = _categoryEntry.CategoryDisplayName;
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntitiesList_SelectionChanged(XUiC_ListEntry<XUiC_GameEventsList.GameEventEntry> _previousEntry, XUiC_ListEntry<XUiC_GameEventsList.GameEventEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			this.gameEventsList.ClearSelection();
			if (_newEntry.GetEntry() != null)
			{
				XUiC_GameEventsList.GameEventEntry entry = _newEntry.GetEntry();
				this.BtnSpawns_OnPress(entry.name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawns_OnPress(string _name)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		EntityPlayer entityPlayer2 = this.PlayerList[this.cbxTarget.Value];
		if (entityPlayer2 == entityPlayer || !entityPlayer2.IsAdmin)
		{
			GameEventManager.Current.HandleAction(_name, entityPlayer, entityPlayer2, false, "", "", false, true, "", null);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.categoryList.SetupCategoriesBasedOnGameEventCategories(GameEventManager.Current.CategoryList);
		this.categoryList.SetCategoryToFirst();
		this.cbxTarget.Elements.Clear();
		this.PlayerList.Clear();
		int selectedIndex = 0;
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = GameManager.Instance.World.Players.list[i];
			this.cbxTarget.Elements.Add(entityPlayer.EntityName);
			this.PlayerList.Add(entityPlayer.EntityName, entityPlayer);
			if (entityPlayer is EntityPlayerLocal)
			{
				selectedIndex = i;
			}
		}
		this.cbxTarget.SelectedIndex = selectedIndex;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "headertitle")
		{
			if (this.categoryDisplay == "")
			{
				value = "Game Events";
			}
			else
			{
				value = string.Format("Game Events - {0}", this.categoryDisplay);
			}
			return true;
		}
		return false;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GameEventsList gameEventsList;

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryDisplay = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, EntityPlayer> PlayerList = new Dictionary<string, EntityPlayer>();
}
