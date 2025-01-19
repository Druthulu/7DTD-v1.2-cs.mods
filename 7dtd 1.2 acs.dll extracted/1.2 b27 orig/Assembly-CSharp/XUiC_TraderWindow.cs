using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderWindow : XUiController
{
	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			if (this.page != value)
			{
				this.page = value;
				this.itemListGrid.Page = this.page;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging == null)
				{
					return;
				}
				xuiC_Paging.SetPage(this.page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.lblGeneralStock = Localization.Get("xuiGeneralStock", false);
		this.lblSecretStash = Localization.Get("xuiSecretStash", false);
		this.windowicon = base.GetChildById("windowicon");
		this.playerName = (XUiC_PlayerName)base.GetChildById("playerName");
		this.categoryList = this.windowGroup.Controller.GetChildByType<XUiC_CategoryList>();
		this.categoryList.CategoryChanged += this.HandleCategoryChanged;
		this.pager = base.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate()
			{
				this.Page = this.pager.CurrentPageNumber;
				this.GetItemStackData(this.txtInput.Text);
			};
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnScroll += this.HandleOnScroll;
		}
		base.OnScroll += this.HandleOnScroll;
		this.itemListGrid = base.Parent.GetChildByType<XUiC_TraderItemList>();
		XUiController[] childrenByType = this.itemListGrid.GetChildrenByType<XUiC_TraderItemEntry>(null);
		XUiController[] array = childrenByType;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].OnScroll += this.HandleOnScroll;
			((XUiC_TraderItemEntry)array[j]).TraderWindow = this;
		}
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		XUiController childById = base.GetChildById("collect");
		if (childById != null)
		{
			childById.OnPress += this.Collect_OnPress;
		}
		childById = base.GetChildById("takeAll");
		if (childById != null)
		{
			childById.OnPress += this.TakeAll_OnPress;
		}
		childById = base.GetChildById("rent");
		if (childById != null)
		{
			childById.OnPress += this.Rent_OnPress;
			this.rentButton = (XUiV_Button)childById.ViewComponent;
		}
		childById = base.GetChildById("showAll");
		if (childById != null)
		{
			childById.OnPress += this.ShowAll_OnPress;
			this.showAllButton = (XUiV_Button)childById.ViewComponent;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowAll_OnPress(XUiController _sender, int _mouseButton)
	{
		this.showAll = !this.showAll;
		this.showAllButton.Selected = this.showAll;
		this.RefreshTraderItems();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rent_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if (this.serviceInfoWindow == null)
			{
				this.serviceInfoWindow = (XUiC_ServiceInfoWindow)this.windowGroup.Controller.GetChildById("serviceInfoPanel");
			}
			InGameService service = new InGameService
			{
				Name = Localization.Get("rentVendingMachine", false),
				Description = Localization.Get("rentVendingMachineDesc", false),
				Icon = "ui_game_symbol_vending",
				Price = tileEntityVendingMachine.TraderData.TraderInfo.RentCost,
				VisibleChangedHandler = delegate(bool visible)
				{
					if (this.rentButton != null)
					{
						this.rentButton.Selected = visible;
					}
				}
			};
			if (base.xui.currentSelectedEntry != null)
			{
				base.xui.currentSelectedEntry.Selected = false;
			}
			this.serviceInfoWindow.SetInfo(service, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Collect_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if ((this.playerOwned || this.isRentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem, false);
				int availableMoney = base.xui.Trader.Trader.AvailableMoney;
				XUiM_PlayerInventory playerInventory = base.xui.PlayerInventory;
				ItemStack itemStack = new ItemStack(item.Clone(), availableMoney);
				playerInventory.AddItem(itemStack);
				base.xui.Trader.Trader.AvailableMoney = itemStack.count;
				base.RefreshBindings(false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TakeAll_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if ((this.playerOwned || this.isRentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				TraderData trader = base.xui.Trader.Trader;
				XUiM_PlayerInventory playerInventory = base.xui.PlayerInventory;
				bool flag = false;
				for (int i = 0; i < trader.PrimaryInventory.Count; i++)
				{
					ItemStack itemStack = trader.PrimaryInventory[i];
					int num = itemStack.itemValue.ItemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicBundleSize : itemStack.itemValue.ItemClass.EconomicBundleSize;
					int num2 = Math.Min(itemStack.count, base.xui.PlayerInventory.CountAvailabileSpaceForItem(itemStack.itemValue)) / num * num;
					int num3 = itemStack.count - num2;
					itemStack.count = num2;
					if (playerInventory.AddItem(itemStack, false))
					{
						flag = true;
					}
					itemStack.count += num3;
					if (itemStack.count == 0)
					{
						trader.RemoveMarkup(i);
						trader.PrimaryInventory.RemoveAt(i--);
					}
				}
				if (flag && GameManager.Instance != null && GameManager.Instance.World != null)
				{
					Manager.Play(GameManager.Instance.World.GetPrimaryPlayer(), "UseActions/takeall1", 1f, false);
				}
				this.RefreshTraderItems();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		string a = _categoryEntry.CategoryName;
		if (a == "SECRET STASH")
		{
			a = "";
			this.isSecretStash = true;
		}
		else
		{
			this.isSecretStash = false;
		}
		this.RefreshHeader();
		this.Page = 0;
		this.SetCategory(a);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshHeader()
	{
		if (base.xui.Trader.TraderTileEntity != null && this.isVending)
		{
			TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			if (tileEntityVendingMachine.IsRentable || tileEntityVendingMachine.TraderData.TraderInfo.PlayerOwned)
			{
				if (tileEntityVendingMachine.GetOwner() != null)
				{
					string displayName = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntityVendingMachine.GetOwner()).PlayerName.DisplayName;
					this.playerName.SetGenericName(string.Format(Localization.Get("xuiVendingWithOwner", false), displayName));
				}
				else
				{
					this.playerName.SetGenericName(Localization.Get("xuiEmptyVendingMachine", false));
				}
			}
			else
			{
				this.playerName.SetGenericName(this.lblGeneralStock);
			}
			if (this.windowicon != null)
			{
				((XUiV_Sprite)this.windowicon.ViewComponent).SpriteName = "ui_game_symbol_vending";
				return;
			}
		}
		else
		{
			this.playerName.SetGenericName(this.isSecretStash ? this.lblSecretStash : this.lblGeneralStock);
			((XUiV_Sprite)this.windowicon.ViewComponent).SpriteName = "ui_game_symbol_map_trader";
		}
	}

	public void RefreshOwner()
	{
		if (this.isVending)
		{
			this.isOwner = (base.xui.Trader.TraderTileEntity as TileEntityVendingMachine).LocalPlayerIsOwner();
			return;
		}
		this.isOwner = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.Page = 0;
		this.FilterByName(_text);
		this.itemListGrid.SetItems(this.currentInventory.ToArray(), this.currentIndexList);
		if (this.currentInventory.Count == 0 || this.currentInventory[0].IsEmpty())
		{
			base.GetChildById("searchControls").SelectCursorElement(true, false);
			return;
		}
		this.itemListGrid.SelectFirstElement();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetItemStackData(string _name)
	{
		if (_name == null)
		{
			_name = "";
		}
		this.currentInventory.Clear();
		this.length = this.itemListGrid.Length;
		this.FilterByName(_name);
		this.itemListGrid.SetItems(this.currentInventory.ToArray(), this.currentIndexList);
		if (!this.isSecretStash)
		{
			this.categoryList.SetupCategoriesBasedOnItems(this.buyInventory, this.traderStage);
		}
		if (this.currentInventory.Count == 0 || this.currentInventory[0].IsEmpty())
		{
			base.GetChildById("searchControls").SelectCursorElement(true, false);
			return;
		}
		this.itemListGrid.SelectFirstElement();
	}

	public void FilterByName(string _name)
	{
		this.currentIndexList.Clear();
		this.currentInventory.Clear();
		for (int i = 0; i < this.buyInventory.Count; i++)
		{
			if (this.buyInventory[i] == null || this.buyInventory[i].count == 0)
			{
				this.buyInventory.RemoveAt(i);
				i--;
			}
			else
			{
				ItemClass itemClass = this.buyInventory[i].itemValue.ItemClass;
				string text = itemClass.GetLocalizedItemName();
				if (text == null)
				{
					text = Localization.Get(itemClass.Name, false);
				}
				if (this.category == "")
				{
					if (_name == "" || itemClass.Name.ContainsCaseInsensitive(_name) || text.ContainsCaseInsensitive(_name) || this.buyInventory[i].itemValue.GetItemOrBlockId().ToString() == _name.Trim())
					{
						TraderStageTemplateGroup traderStageTemplateGroup = null;
						if (itemClass.TraderStageTemplate != null)
						{
							if (!TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
							{
								throw new Exception(string.Concat(new string[]
								{
									"TraderStageTemplate ",
									itemClass.TraderStageTemplate,
									" for item: ",
									itemClass.GetLocalizedItemName(),
									" does not exist."
								}));
							}
							traderStageTemplateGroup = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
						}
						if (traderStageTemplateGroup == null || this.traderStage == -1 || traderStageTemplateGroup.IsWithin(this.traderStage, (int)this.buyInventory[i].itemValue.Quality) || this.showAll)
						{
							this.currentIndexList.Add(i);
							this.currentInventory.Add(this.buyInventory[i]);
						}
					}
				}
				else
				{
					string[] array = itemClass.Groups;
					if (itemClass.IsBlock())
					{
						array = Block.list[this.buyInventory[i].itemValue.type].GroupNames;
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null && array[j].EqualsCaseInsensitive(this.category) && (_name == "" || itemClass.Name.ContainsCaseInsensitive(_name) || text.ContainsCaseInsensitive(_name) || this.buyInventory[i].itemValue.GetItemOrBlockId().ToString() == _name.Trim()))
						{
							TraderStageTemplateGroup traderStageTemplateGroup2 = null;
							if (itemClass.TraderStageTemplate != null)
							{
								if (!TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
								{
									throw new Exception(string.Concat(new string[]
									{
										"TraderStageTemplate ",
										itemClass.TraderStageTemplate,
										" for item: ",
										itemClass.GetLocalizedItemName(),
										" does not exist."
									}));
								}
								traderStageTemplateGroup2 = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
							}
							if (traderStageTemplateGroup2 == null || this.traderStage == -1 || traderStageTemplateGroup2.IsWithin(this.traderStage, (int)this.buyInventory[i].itemValue.Quality) || this.showAll)
							{
								this.currentIndexList.Add(i);
								this.currentInventory.Add(this.buyInventory[i]);
							}
						}
					}
				}
			}
		}
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging == null)
		{
			return;
		}
		xuiC_Paging.SetLastPageByElementsAndPageLength(this.currentInventory.Count, this.length);
	}

	public void SetCategory(string _category)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.category = _category;
		this.RefreshTraderItems();
	}

	public string GetCategory()
	{
		return this.category;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.xui.Trader.TraderTileEntity != null)
		{
			this.isVending = (base.xui.Trader.TraderTileEntity is TileEntityVendingMachine);
			if (this.isVending)
			{
				Manager.PlayInsidePlayerHead("open_vending", -1, 0f, false, false);
			}
		}
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (base.xui.Trader.TraderEntity != null)
		{
			this.traderStage = entityPlayer.GetTraderStage(entityPlayer.QuestJournal.GetCurrentFactionTier(base.xui.Trader.TraderEntity.NPCInfo.QuestFaction, 0, false));
		}
		else
		{
			this.traderStage = -1;
		}
		this.CompletedTransaction = false;
		if (base.xui.Trader.Trader != null)
		{
			this.categoryList.SetCategoryToFirst();
		}
		this.playerOwned = base.xui.Trader.TraderTileEntity.TraderData.TraderInfo.PlayerOwned;
		this.isRentable = base.xui.Trader.TraderTileEntity.TraderData.TraderInfo.Rentable;
		if (this.isRentable && this.isOwner && ((TileEntityVendingMachine)base.xui.Trader.TraderTileEntity).TryAutoBuy(true))
		{
			this.RefreshTraderItems();
		}
		this.Refresh();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.xui.Trader.TraderEntity != null)
		{
			if (this.CompletedTransaction)
			{
				base.xui.Trader.TraderEntity.PlayVoiceSetEntry("sale_accepted", base.xui.playerUI.entityPlayer, true, true);
			}
			else
			{
				base.xui.Trader.TraderEntity.PlayVoiceSetEntry("sale_declined", base.xui.playerUI.entityPlayer, true, true);
			}
			base.xui.Trader.TraderEntity = null;
		}
		else
		{
			Manager.PlayInsidePlayerHead("close_vending", -1, 0f, false, false);
		}
		if (base.xui.Trader.TraderTileEntity != null)
		{
			TileEntityTrader traderTileEntity = base.xui.Trader.TraderTileEntity;
			Vector3i blockPos = traderTileEntity.ToWorldPos();
			traderTileEntity.SetModified();
			traderTileEntity.SetUserAccessing(false);
			GameManager.Instance.TEUnlockServer(traderTileEntity.GetClrIdx(), blockPos, traderTileEntity.entityId, true);
			base.xui.Trader.TraderTileEntity = null;
		}
		base.xui.Trader.Trader = null;
	}

	public void RefreshTraderItems()
	{
		this.buyInventory.Clear();
		ItemStack[] stashSlots = this.GetStashSlots();
		this.hasSecretStash = (stashSlots != null);
		ItemStack[] array = (this.isSecretStash && this.hasSecretStash) ? stashSlots : base.xui.Trader.Trader.PrimaryInventory.ToArray();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				this.buyInventory.Add(array[i]);
			}
		}
		this.GetItemStackData(this.txtInput.Text);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] GetStashSlots()
	{
		float value = EffectManager.GetValue(PassiveEffects.SecretStash, null, (float)base.xui.playerUI.entityPlayer.Progression.Level, base.xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		for (int i = 0; i < base.xui.Trader.Trader.TierItemGroups.Count; i++)
		{
			TraderInfo.TierItemGroup tierItemGroup = base.xui.Trader.Trader.TraderInfo.TierItemGroups[i];
			if ((value >= (float)tierItemGroup.minLevel || tierItemGroup.minLevel == -1) && (value <= (float)tierItemGroup.maxLevel || tierItemGroup.maxLevel == -1))
			{
				return base.xui.Trader.Trader.TierItemGroups[i];
			}
		}
		return null;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1533283582U)
		{
			if (num <= 543700434U)
			{
				if (num != 142312684U)
				{
					if (num != 391295749U)
					{
						if (num == 543700434U)
						{
							if (bindingName == "renttimeleft")
							{
								if (base.xui.Trader.TraderTileEntity != null && this.isVending)
								{
									TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
									if (this.isOwner && tileEntityVendingMachine.IsRentable)
									{
										int rentalEndDay = tileEntityVendingMachine.RentalEndDay;
										value = this.rentTimeLeftFormatter.Format(rentalEndDay);
									}
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "timeleft")
					{
						if (base.xui.Trader.Trader != null)
						{
							int v = GameUtils.WorldTimeToDays(base.xui.Trader.Trader.NextResetTime);
							value = this.timeLeftFormatter.Format(v);
						}
						return true;
					}
				}
				else if (bindingName == "availablemoney")
				{
					value = ((base.xui.Trader.Trader != null) ? this.availableMoneyFormatter.Format(base.xui.Trader.Trader.AvailableMoney) : "");
					return true;
				}
			}
			else if (num != 619389156U)
			{
				if (num != 916116330U)
				{
					if (num == 1533283582U)
					{
						if (bindingName == "isrentable")
						{
							if (this.isVending)
							{
								TileEntityVendingMachine tileEntityVendingMachine2 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
								value = tileEntityVendingMachine2.IsRentable.ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "tradername")
				{
					value = "";
					if (base.xui.Trader.Trader != null)
					{
						if (base.xui.Trader.TraderEntity != null)
						{
							value = base.xui.Trader.TraderEntity.EntityName;
						}
						else if (base.xui.Trader.TraderTileEntity != null)
						{
							value = Localization.Get("VendingMachine", false);
						}
					}
					return true;
				}
			}
			else if (bindingName == "restocklabel")
			{
				value = Localization.Get("xuiRestock", false);
				return true;
			}
		}
		else if (num <= 3170511085U)
		{
			if (num != 1549422821U)
			{
				if (num != 2585672714U)
				{
					if (num == 3170511085U)
					{
						if (bindingName == "isrenter")
						{
							if (this.isVending)
							{
								TileEntityVendingMachine tileEntityVendingMachine3 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
								value = (tileEntityVendingMachine3.IsRentable && this.isOwner).ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "isownerorrentable")
				{
					if (this.isVending)
					{
						TileEntityVendingMachine tileEntityVendingMachine4 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
						value = (tileEntityVendingMachine4.IsRentable || (this.playerOwned && (this.isOwner || tileEntityVendingMachine4.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)))).ToString();
					}
					else
					{
						value = "false";
					}
					return true;
				}
			}
			else if (bindingName == "isnotowner")
			{
				if (this.isVending)
				{
					TileEntityVendingMachine tileEntityVendingMachine5 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
					value = ((!this.playerOwned && !this.isRentable) || !tileEntityVendingMachine5.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
				}
				else
				{
					value = "false";
				}
				return true;
			}
		}
		else if (num != 3218764573U)
		{
			if (num != 3392636521U)
			{
				if (num == 4243210436U)
				{
					if (bindingName == "isowner")
					{
						if (this.isVending)
						{
							TileEntityVendingMachine tileEntityVendingMachine6 = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
							value = ((this.playerOwned || this.isRentable) && tileEntityVendingMachine6.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
			}
			else if (bindingName == "is_debug")
			{
				value = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !this.isVending).ToString();
				return true;
			}
		}
		else if (bindingName == "showrestock")
		{
			value = (base.xui.Trader.Trader != null && base.xui.Trader.Trader.TraderInfo.ResetInterval > 0).ToString();
			return true;
		}
		return false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			if (base.xui.Trader.Trader != null && (!base.xui.Trader.TraderTileEntity.syncNeeded || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer))
			{
				if (base.xui.Trader.TraderTileEntity != null && this.isVending)
				{
					TileEntityVendingMachine tileEntityVendingMachine = base.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
					if (this.isRentable && tileEntityVendingMachine.GetOwner() != null && tileEntityVendingMachine.RentTimeRemaining <= 0f)
					{
						tileEntityVendingMachine.ClearVendingMachine();
						this.Refresh();
						this.RefreshTraderItems();
					}
				}
				if (base.xui.Trader.Trader.CurrentTime <= 0f && !base.xui.Trader.Trader.TraderInfo.PlayerOwned && !base.xui.Trader.Trader.TraderInfo.Rentable && GameManager.Instance.traderManager.TraderInventoryRequested(base.xui.Trader.Trader, XUiM_Player.GetPlayer().entityId))
				{
					if (base.xui.Trader.TraderTileEntity != null)
					{
						base.xui.Trader.TraderTileEntity.SetModified();
					}
					XUiM_Player.GetPlayer().PlayOneShot("ui_trader_inv_reset", false, false, false);
					this.RefreshTraderItems();
					base.RefreshBindings(false);
				}
			}
		}
	}

	public void Refresh()
	{
		this.RefreshOwner();
		this.RefreshHeader();
		base.RefreshBindings(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServiceInfoWindow serviceInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemList itemListGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> buyInventory = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> currentIndexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> currentInventory = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowicon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button rentButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showAllButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayerName playerName;

	public bool CompletedTransaction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblGeneralStock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwner;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerOwned;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRentable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isVending;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAll;

	[PublicizedFrom(EAccessModifier.Private)]
	public int traderStage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> rentTimeLeftFormatter = new CachedStringFormatter<int>((int _i) => string.Format("{0}: {1} {2}", Localization.Get("xuiExpires", false), Localization.Get("xuiDay", false), _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> timeLeftFormatter = new CachedStringFormatter<int>((int _i) => string.Format("{0} {1}", Localization.Get("xuiDay", false), _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt availableMoneyFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public enum TraderActionTypes
	{
		Buy,
		Sell,
		BuyBack
	}
}
