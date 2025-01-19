using System;
using System.Collections.Generic;
using Challenges;
using GUI_2;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CategoryList : XUiController
{
	public event XUiEvent_CategoryChangedEventHandler CategoryChanged;

	public event XUiEvent_CategoryChangedEventHandler CategoryClickChanged;

	public XUiC_CategoryEntry CurrentCategory
	{
		get
		{
			return this.currentCategory;
		}
		set
		{
			if (this.currentCategory != null)
			{
				this.currentCategory.Selected = false;
			}
			this.currentCategory = value;
			if (this.currentCategory != null)
			{
				this.currentCategory.Selected = true;
				this.currentIndex = this.categoryButtons.IndexOf(this.currentCategory);
			}
		}
	}

	public int MaxCategories
	{
		get
		{
			return this.categoryButtons.Count;
		}
	}

	public override void Init()
	{
		base.Init();
		base.GetChildrenByType<XUiC_CategoryEntry>(this.categoryButtons);
		for (int i = 0; i < this.categoryButtons.Count; i++)
		{
			this.categoryButtons[i].CategoryList = this;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.AllowKeyPaging || !base.xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			return;
		}
		PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
		if (guiactions.PageUp.WasReleased)
		{
			this.IncrementCategory(1);
			return;
		}
		if (guiactions.PageDown.WasReleased)
		{
			this.IncrementCategory(-1);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void HandleCategoryChanged()
	{
		XUiEvent_CategoryChangedEventHandler categoryChanged = this.CategoryChanged;
		if (categoryChanged != null)
		{
			categoryChanged(this.CurrentCategory);
		}
		XUiEvent_CategoryChangedEventHandler categoryClickChanged = this.CategoryClickChanged;
		if (categoryClickChanged == null)
		{
			return;
		}
		categoryClickChanged(this.CurrentCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryEntry GetCategoryByName(string _category, out int _index)
	{
		_index = 0;
		for (int i = 0; i < this.categoryButtons.Count; i++)
		{
			if (this.categoryButtons[i].CategoryName == _category)
			{
				_index = i;
				return this.categoryButtons[i];
			}
		}
		return null;
	}

	public XUiC_CategoryEntry GetCategoryByIndex(int _index)
	{
		if (_index >= this.categoryButtons.Count)
		{
			return null;
		}
		return this.categoryButtons[_index];
	}

	public void SetCategoryToFirst()
	{
		this.CurrentCategory = this.categoryButtons[0];
		XUiEvent_CategoryChangedEventHandler categoryChanged = this.CategoryChanged;
		if (categoryChanged == null)
		{
			return;
		}
		categoryChanged(this.CurrentCategory);
	}

	public void SetCategory(string _category)
	{
		int num;
		XUiC_CategoryEntry categoryByName = this.GetCategoryByName(_category, out num);
		if (categoryByName == null && !this.AllowUnselect)
		{
			return;
		}
		this.CurrentCategory = categoryByName;
		XUiEvent_CategoryChangedEventHandler categoryChanged = this.CategoryChanged;
		if (categoryChanged == null)
		{
			return;
		}
		categoryChanged(this.CurrentCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncrementCategory(int _offset)
	{
		if (_offset == 0)
		{
			return;
		}
		int num = 0;
		int num2 = NGUIMath.RepeatIndex(this.currentIndex + _offset, this.categoryButtons.Count);
		XUiC_CategoryEntry xuiC_CategoryEntry = this.categoryButtons[num2];
		while (num < this.categoryButtons.Count && (xuiC_CategoryEntry == null || xuiC_CategoryEntry.SpriteName == ""))
		{
			num2 = NGUIMath.RepeatIndex((_offset > 0) ? (num2 + 1) : (num2 - 1), this.categoryButtons.Count);
			xuiC_CategoryEntry = this.categoryButtons[num2];
			num++;
		}
		if (xuiC_CategoryEntry != null && xuiC_CategoryEntry.SpriteName != "")
		{
			this.CurrentCategory = xuiC_CategoryEntry;
			this.HandleCategoryChanged();
		}
	}

	public void SetCategoryEmpty(int _index)
	{
		XUiC_CategoryEntry xuiC_CategoryEntry = this.categoryButtons[_index];
		xuiC_CategoryEntry.CategoryDisplayName = (xuiC_CategoryEntry.CategoryName = (xuiC_CategoryEntry.SpriteName = ""));
		xuiC_CategoryEntry.ViewComponent.IsVisible = false;
		xuiC_CategoryEntry.ViewComponent.IsNavigatable = false;
	}

	public void SetCategoryEntry(int _index, string _categoryName, string _spriteName, string _displayName = null)
	{
		XUiC_CategoryEntry xuiC_CategoryEntry = this.categoryButtons[_index];
		xuiC_CategoryEntry.CategoryDisplayName = (_displayName ?? _categoryName);
		xuiC_CategoryEntry.CategoryName = _categoryName;
		xuiC_CategoryEntry.SpriteName = (_spriteName ?? "");
		xuiC_CategoryEntry.ViewComponent.IsVisible = true;
		xuiC_CategoryEntry.ViewComponent.IsNavigatable = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "allow_unselect")
		{
			this.AllowUnselect = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		if (!(_name == "allow_key_paging"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.AllowKeyPaging = StringParsers.ParseBool(_value, 0, -1, true);
		return true;
	}

	public bool SetupCategoriesByWorkstation(string _workstation)
	{
		if (this.currentWorkstation != _workstation)
		{
			this.currentWorkstation = _workstation;
			if (_workstation == "skills")
			{
				int num = 0;
				using (Dictionary<string, ProgressionClass>.Enumerator enumerator = Progression.ProgressionClasses.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, ProgressionClass> keyValuePair = enumerator.Current;
						if (keyValuePair.Value.IsAttribute)
						{
							this.SetCategoryEntry(num, keyValuePair.Value.Name, keyValuePair.Value.Icon, Localization.Get(keyValuePair.Value.Name, false));
							num++;
						}
					}
					return true;
				}
			}
			List<CraftingCategoryDisplayEntry> craftingCategoryDisplayList = UIDisplayInfoManager.Current.GetCraftingCategoryDisplayList(_workstation);
			if (craftingCategoryDisplayList != null)
			{
				int num2 = 0;
				int num3 = 0;
				while (num3 < craftingCategoryDisplayList.Count && num3 < this.categoryButtons.Count)
				{
					this.SetCategoryEntry(num2, craftingCategoryDisplayList[num3].Name, craftingCategoryDisplayList[num3].Icon, craftingCategoryDisplayList[num3].DisplayName);
					num2++;
					num3++;
				}
				for (int i = num2; i < this.categoryButtons.Count; i++)
				{
					this.SetCategoryEmpty(num2++);
				}
			}
			return true;
		}
		return false;
	}

	public bool SetupCategoriesBasedOnItems(List<ItemStack> _items, int _traderStage)
	{
		List<string> list = new List<string>();
		this.SetCategoryEntry(0, "", "ui_game_symbol_shopping_cart", Localization.Get("lblAll", false));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			ItemClass itemClass = _items[i].itemValue.ItemClass;
			TraderStageTemplateGroup traderStageTemplateGroup = null;
			if (itemClass.TraderStageTemplate != null && TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
			{
				traderStageTemplateGroup = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
			}
			if (traderStageTemplateGroup == null || traderStageTemplateGroup.IsWithin(_traderStage, (int)_items[i].itemValue.Quality))
			{
				string[] array = itemClass.Groups;
				if (itemClass.IsBlock())
				{
					array = Block.list[_items[i].itemValue.type].GroupNames;
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (!list.Contains(array[j]))
					{
						CraftingCategoryDisplayEntry traderCategoryDisplay = UIDisplayInfoManager.Current.GetTraderCategoryDisplay(array[j]);
						if (traderCategoryDisplay != null)
						{
							int count = list.Count;
							this.SetCategoryEntry(count, traderCategoryDisplay.Name, traderCategoryDisplay.Icon, traderCategoryDisplay.DisplayName);
							list.Add(array[j]);
						}
					}
				}
			}
		}
		for (int k = list.Count; k < this.categoryButtons.Count; k++)
		{
			this.SetCategoryEmpty(k);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnTwitchCategories(List<TwitchActionManager.ActionCategory> _items)
	{
		List<string> list = new List<string>();
		this.SetCategoryEntry(0, "", "ui_game_symbol_twitch_actions", Localization.Get("lblAll", false));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			TwitchActionManager.ActionCategory actionCategory = _items[i];
			if (actionCategory.Icon != "")
			{
				this.SetCategoryEntry(list.Count, actionCategory.Name, actionCategory.Icon, actionCategory.Name);
				list.Add(actionCategory.Name);
			}
		}
		for (int j = list.Count; j < this.categoryButtons.Count; j++)
		{
			this.SetCategoryEmpty(j);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnTwitchActions(List<TwitchAction> _items)
	{
		List<string> list = new List<string>();
		this.SetCategoryEntry(0, "", "ui_game_symbol_twitch_actions", Localization.Get("lblAll", false));
		list.Add("");
		Dictionary<TwitchActionManager.ActionCategory, int> dictionary = new Dictionary<TwitchActionManager.ActionCategory, int>();
		List<TwitchActionManager.ActionCategory> categoryList = TwitchActionManager.Current.CategoryList;
		for (int i = 0; i < categoryList.Count; i++)
		{
			dictionary.Add(categoryList[i], 0);
		}
		for (int j = 0; j < _items.Count; j++)
		{
			TwitchAction twitchAction = _items[j];
			if (twitchAction.DisplayCategory != null)
			{
				Dictionary<TwitchActionManager.ActionCategory, int> dictionary2 = dictionary;
				TwitchActionManager.ActionCategory displayCategory = twitchAction.DisplayCategory;
				int num = dictionary2[displayCategory];
				dictionary2[displayCategory] = num + 1;
			}
		}
		foreach (TwitchActionManager.ActionCategory actionCategory in dictionary.Keys)
		{
			if (dictionary[actionCategory] > 0)
			{
				this.SetCategoryEntry(list.Count, actionCategory.Name, actionCategory.Icon, actionCategory.DisplayName);
				list.Add(actionCategory.Name);
			}
		}
		for (int k = list.Count; k < this.categoryButtons.Count; k++)
		{
			this.SetCategoryEmpty(k);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnTwitchVoteCategories(List<TwitchVoteType> _items)
	{
		List<string> list = new List<string>();
		this.SetCategoryEntry(0, "", "ui_game_symbol_twitch_vote", Localization.Get("lblAll", false));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			TwitchVoteType twitchVoteType = _items[i];
			if (twitchVoteType.Icon != "")
			{
				this.SetCategoryEntry(list.Count, twitchVoteType.Name, twitchVoteType.Icon, twitchVoteType.Title);
				list.Add(twitchVoteType.Name);
			}
		}
		for (int j = list.Count; j < this.categoryButtons.Count; j++)
		{
			this.SetCategoryEmpty(j);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnGameEventCategories(List<GameEventManager.Category> _items)
	{
		List<string> list = new List<string>();
		this.SetCategoryEntry(0, "", "ui_game_symbol_airdrop", Localization.Get("lblAll", false));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			GameEventManager.Category category = _items[i];
			if (category.Icon != "")
			{
				this.SetCategoryEntry(list.Count, category.Name, category.Icon, category.Name);
				list.Add(category.Name);
			}
		}
		for (int j = list.Count; j < this.categoryButtons.Count; j++)
		{
			this.SetCategoryEmpty(j);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnChallengeCategories(List<ChallengeCategory> _items)
	{
		List<string> list = new List<string>();
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		for (int i = 0; i < _items.Count; i++)
		{
			ChallengeCategory challengeCategory = _items[i];
			if (challengeCategory.Icon != "" && challengeCategory.CanShow(entityPlayer))
			{
				this.SetCategoryEntry(list.Count, challengeCategory.Name, challengeCategory.Icon, challengeCategory.Title);
				list.Add(challengeCategory.Name);
			}
		}
		for (int j = list.Count; j < this.categoryButtons.Count; j++)
		{
			this.SetCategoryEmpty(j);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentWorkstation = "*";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_CategoryEntry> categoryButtons = new List<XUiC_CategoryEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryEntry currentCategory;

	public bool AllowUnselect;

	public bool AllowKeyPaging = true;
}
