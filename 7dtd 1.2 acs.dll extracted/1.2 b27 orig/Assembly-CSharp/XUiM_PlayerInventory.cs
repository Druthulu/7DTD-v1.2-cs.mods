using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;

public class XUiM_PlayerInventory : XUiModel, IInventory
{
	public event XUiEvent_BackpackItemsChanged OnBackpackItemsChanged;

	public event XUiEvent_ToolbeltItemsChanged OnToolbeltItemsChanged;

	public event XUiEvent_CurrencyChanged OnCurrencyChanged;

	public Bag Backpack
	{
		get
		{
			return this.backpack;
		}
	}

	public Inventory Toolbelt
	{
		get
		{
			return this.toolbelt;
		}
	}

	public int CurrencyAmount { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public int QuickSwapSlot
	{
		get
		{
			return this.toolbelt.GetBestQuickSwapSlot();
		}
	}

	public XUiM_PlayerInventory(XUi _xui, EntityPlayerLocal _player)
	{
		if (_player == null)
		{
			return;
		}
		this.xui = _xui;
		this.localPlayer = _player;
		this.backpack = this.localPlayer.bag;
		this.toolbelt = this.localPlayer.inventory;
		this.backpack.OnBackpackItemsChangedInternal += this.onBackpackItemsChanged;
		this.toolbelt.OnToolbeltItemsChangedInternal += this.onToolbeltItemsChanged;
		this.localPlayer.PlayerUI.OnUIShutdown += this.HandleUIShutdown;
		this.currencyItem = ItemClass.GetItem(TraderInfo.CurrencyItem, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBackpackItemsChanged()
	{
		if (this.OnBackpackItemsChanged != null)
		{
			this.OnBackpackItemsChanged();
		}
		this.RefreshCurrency();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onToolbeltItemsChanged()
	{
		if (this.OnToolbeltItemsChanged != null)
		{
			this.OnToolbeltItemsChanged();
		}
		this.RefreshCurrency();
	}

	public bool AddItemToPreferredToolbeltSlot(ItemStack _itemStack, int _slot)
	{
		return this.toolbelt.AddItemAtSlot(_itemStack, _slot);
	}

	public bool AddItemNoPartial(ItemStack _itemStack, bool _playCollectSound = true)
	{
		return (this.backpack.CanStack(_itemStack) || this.toolbelt.CanStack(_itemStack)) && this.AddItem(_itemStack, _playCollectSound);
	}

	public bool CanSwapItems(ItemStack _removedStack, ItemStack _addedStack, int _slotNumber = -1)
	{
		List<ItemStack> allItemStacks = this.GetAllItemStacks();
		int num = _removedStack.count;
		int num2 = _addedStack.count;
		int value = _removedStack.itemValue.ItemClass.Stacknumber.Value;
		int value2 = _addedStack.itemValue.ItemClass.Stacknumber.Value;
		for (int i = 0; i < allItemStacks.Count - 1; i++)
		{
			if (num > 0 && allItemStacks[i].itemValue.type == _removedStack.itemValue.type && (_slotNumber == -1 || _slotNumber == i))
			{
				int count = allItemStacks[i].count;
				if (count > num)
				{
					num = 0;
				}
				else
				{
					num -= count;
					num2 -= value2;
				}
			}
			else if (num2 > 0 && allItemStacks[i].itemValue.type == _addedStack.itemValue.type)
			{
				int num3 = value2 - allItemStacks[i].count;
				if (num3 >= num2)
				{
					num2 = 0;
				}
				else
				{
					num2 -= num3;
				}
			}
			else if (allItemStacks[i].IsEmpty())
			{
				num2 -= value2;
			}
			if (num <= 0 && num2 <= 0)
			{
				return true;
			}
		}
		return false;
	}

	public void SortStacks(int _ignoreSlots = 0, bool[] _ignoredSlots = null)
	{
		if (EffectManager.GetValue(PassiveEffects.ShuffledBackpack, null, 0f, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f)
		{
			ItemStack[] slots = StackSortUtil.CombineAndSortStacks(this.GetBackpackItemStacks(), _ignoreSlots, _ignoredSlots);
			this.backpack.SetSlots(slots);
		}
	}

	public bool AddItem(ItemStack _itemStack, bool playCollectSound = true)
	{
		bool flag = false;
		ItemStack itemStack = _itemStack.Clone();
		if (!flag)
		{
			if (this.backpack.CanStackNoEmpty(itemStack))
			{
				flag = this.backpack.TryStackItem(0, itemStack).Item2;
			}
			else if (this.toolbelt.CanStackNoEmpty(itemStack))
			{
				flag = this.toolbelt.TryStackItem(0, itemStack);
			}
		}
		if (!flag)
		{
			if (this.backpack.CanStack(itemStack))
			{
				flag = this.backpack.TryStackItem(0, itemStack).Item2;
			}
			else if (this.toolbelt.CanStack(itemStack))
			{
				flag = this.toolbelt.TryStackItem(0, itemStack);
			}
		}
		if (!flag)
		{
			ItemClass itemClass = itemStack.itemValue.ItemClass;
			int num = 1;
			if (itemClass != null && itemClass.Stacknumber != null)
			{
				num = itemClass.Stacknumber.Value;
			}
			if (itemStack.count > num)
			{
				for (int i = itemStack.count; i > 0; i -= num)
				{
					bool flag2 = false;
					int num2 = Math.Min(i, num);
					num2 = Math.Max(0, num2);
					ItemStack itemStack2 = itemStack.Clone();
					itemStack2.count = num2;
					ItemClassArmor itemClassArmor = itemStack.itemValue.ItemClass as ItemClassArmor;
					if (itemClassArmor != null && !this.xui.PlayerEquipment.IsEquipmentTypeWorn(itemClassArmor.EquipSlot) && this.localPlayer.equipment.ReturnItem(itemStack2, true))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
						this.xui.PlayerEquipment.RefreshEquipment();
					}
					else if (this.toolbelt.ReturnItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					else if (this.backpack.AddItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					else if (this.toolbelt.AddItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					if (!flag2)
					{
						if (_itemStack.count != itemStack.count)
						{
							XUiC_CollectedItemList collectedItemList = this.xui.CollectedItemList;
							if (collectedItemList != null)
							{
								collectedItemList.AddItemStack(new ItemStack(itemStack.itemValue, _itemStack.count - itemStack.count), false);
							}
							if (playCollectSound)
							{
								Manager.PlayInsidePlayerHead("item_pickup", -1, 0f, false, false);
							}
							_itemStack.count = itemStack.count;
						}
						return false;
					}
				}
			}
			else
			{
				ItemClassArmor itemClassArmor2 = itemStack.itemValue.ItemClass as ItemClassArmor;
				if (itemClassArmor2 != null && !this.xui.PlayerEquipment.IsEquipmentTypeWorn(itemClassArmor2.EquipSlot) && this.localPlayer.equipment.ReturnItem(itemStack, true))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.xui.PlayerEquipment.RefreshEquipment();
				}
				else if (this.toolbelt.ReturnItem(itemStack))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.onToolbeltItemsChanged();
				}
				else if (this.backpack.AddItem(itemStack))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.onBackpackItemsChanged();
				}
				else if (this.toolbelt.AddItem(itemStack))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.onToolbeltItemsChanged();
				}
			}
		}
		if (itemStack.count != _itemStack.count)
		{
			ItemStack itemStack3 = new ItemStack(itemStack.itemValue, _itemStack.count - itemStack.count);
			QuestEventManager.Current.ItemAdded(itemStack3);
			XUiC_CollectedItemList collectedItemList2 = this.xui.CollectedItemList;
			if (collectedItemList2 != null)
			{
				collectedItemList2.AddItemStack(itemStack3, false);
			}
			if (playCollectSound)
			{
				Manager.PlayInsidePlayerHead("item_pickup", -1, 0f, false, false);
			}
		}
		if (itemStack.count == 0)
		{
			itemStack = ItemStack.Empty.Clone();
		}
		_itemStack.count = itemStack.count;
		return flag;
	}

	public int CountAvailabileSpaceForItem(ItemValue itemValue)
	{
		ItemStack itemStack = new ItemStack(itemValue, 1);
		int value = itemValue.ItemClass.Stacknumber.Value;
		int num = 0;
		ItemStack[] slots = this.backpack.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty())
			{
				return value;
			}
			if (itemStack.CanStackWith(slots[i], false))
			{
				num += value - slots[i].count;
				if (num > value)
				{
					return value;
				}
			}
		}
		ItemStack[] slots2 = this.toolbelt.GetSlots();
		for (int j = 0; j < this.toolbelt.PUBLIC_SLOTS; j++)
		{
			if (slots2[j].IsEmpty())
			{
				return value;
			}
			if (itemStack.CanStackWith(slots2[j], false))
			{
				num += value - slots2[j].count;
				if (num > value)
				{
					return value;
				}
			}
		}
		return num;
	}

	public void DropItem(ItemStack stack)
	{
		GameManager instance = GameManager.Instance;
		if (instance)
		{
			instance.ItemDropServer(stack, this.localPlayer.GetDropPosition(), Vector3.zero, this.localPlayer.entityId, 60f, false);
			Manager.BroadcastPlay("itemdropped");
		}
		XUiC_CollectedItemList collectedItemList = this.xui.CollectedItemList;
		if (collectedItemList == null)
		{
			return;
		}
		collectedItemList.RemoveItemStack(stack);
	}

	public bool AddItems(ItemStack[] _itemStacks)
	{
		bool flag = true;
		for (int i = 0; i < _itemStacks.Length; i++)
		{
			flag &= this.AddItem(_itemStacks[i]);
		}
		return flag;
	}

	public bool AddItemsUsingPreferenceTracker(XUiC_ItemStackGrid _srcGrid, PreferenceTracker preferences)
	{
		if (!preferences.AnyPreferences)
		{
			return false;
		}
		if (this.localPlayer.entityId != preferences.PlayerID)
		{
			return false;
		}
		bool flag = false;
		XUiC_ItemStack[] itemStackControllers = _srcGrid.GetItemStackControllers();
		HashSet<int> hashSet = new HashSet<int>();
		for (int j = 0; j < itemStackControllers.Length; j++)
		{
			ValueTuple<bool, bool> valueTuple = this.TryStackItem(0, itemStackControllers[j].ItemStack);
			bool item = valueTuple.Item1;
			bool item2 = valueTuple.Item2;
			flag = (flag || item);
			if (item)
			{
				if (item2)
				{
					itemStackControllers[j].ItemStack = ItemStack.Empty;
				}
				else
				{
					hashSet.Add(itemStackControllers[j].ItemStack.itemValue.type);
				}
				_srcGrid.HandleSlotChangedEvent(j, itemStackControllers[j].ItemStack);
			}
		}
		if (preferences.toolbelt != null)
		{
			ItemStack[] slots = this.toolbelt.GetSlots();
			int i = 0;
			while (i < preferences.toolbelt.Length && i < slots.Length)
			{
				if (slots[i].IsEmpty())
				{
					int type = preferences.toolbelt[i].itemValue.type;
					XUiC_ItemStack xuiC_ItemStack;
					if (hashSet.Contains(type))
					{
						xuiC_ItemStack = itemStackControllers.FirstOrDefault((XUiC_ItemStack stack) => stack.ItemStack.itemValue.type == type);
					}
					else
					{
						xuiC_ItemStack = itemStackControllers.FirstOrDefault((XUiC_ItemStack stack) => stack.ItemStack.Equals(preferences.toolbelt[i]));
					}
					if (((xuiC_ItemStack != null) ? xuiC_ItemStack.ItemStack : null) != null && !xuiC_ItemStack.ItemStack.IsEmpty())
					{
						this.toolbelt.SetItem(i, xuiC_ItemStack.ItemStack);
						xuiC_ItemStack.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xuiC_ItemStack.SlotNumber, ItemStack.Empty);
						flag = true;
					}
				}
				int i2 = i;
				i = i2 + 1;
			}
		}
		if (preferences.equipment != null)
		{
			int num = Utils.FastMin(this.localPlayer.equipment.GetSlotCount(), preferences.equipment.Length);
			int i2;
			int i;
			for (i = 0; i < num; i = i2 + 1)
			{
				if (this.localPlayer.equipment.GetSlotItem(i) == null)
				{
					XUiC_ItemStack xuiC_ItemStack2 = itemStackControllers.FirstOrDefault((XUiC_ItemStack stack) => stack.ItemStack.itemValue.Equals(preferences.equipment[i]));
					if (((xuiC_ItemStack2 != null) ? xuiC_ItemStack2.ItemStack : null) != null && !xuiC_ItemStack2.ItemStack.IsEmpty())
					{
						this.localPlayer.equipment.SetSlotItem(i, xuiC_ItemStack2.ItemStack.itemValue, true);
						xuiC_ItemStack2.ItemStack.count--;
						if (xuiC_ItemStack2.ItemStack.IsEmpty())
						{
							xuiC_ItemStack2.ItemStack = new ItemStack();
							_srcGrid.HandleSlotChangedEvent(xuiC_ItemStack2.SlotNumber, ItemStack.Empty);
						}
						flag = true;
					}
				}
				i2 = i;
			}
		}
		if (preferences.bag != null)
		{
			ItemStack[] slots2 = this.backpack.GetSlots();
			int i = 0;
			while (i < preferences.bag.Length && i < slots2.Length)
			{
				if (slots2[i].IsEmpty())
				{
					int type = preferences.bag[i].itemValue.type;
					XUiC_ItemStack xuiC_ItemStack3;
					if (hashSet.Contains(type))
					{
						xuiC_ItemStack3 = itemStackControllers.FirstOrDefault((XUiC_ItemStack stack) => stack.ItemStack.itemValue.type == type);
					}
					else
					{
						xuiC_ItemStack3 = itemStackControllers.FirstOrDefault((XUiC_ItemStack stack) => stack.ItemStack.Equals(preferences.bag[i]));
					}
					if (((xuiC_ItemStack3 != null) ? xuiC_ItemStack3.ItemStack : null) != null && !xuiC_ItemStack3.ItemStack.IsEmpty())
					{
						this.backpack.SetSlot(i, xuiC_ItemStack3.ItemStack, true);
						xuiC_ItemStack3.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xuiC_ItemStack3.SlotNumber, ItemStack.Empty);
						flag = true;
					}
				}
				int i2 = i;
				i = i2 + 1;
			}
		}
		return flag;
	}

	public bool AddItemToBackpack(ItemStack _itemStack)
	{
		this.backpack.TryStackItem(0, _itemStack);
		return _itemStack.count > 0 && this.backpack.AddItem(_itemStack);
	}

	public bool AddItemToToolbelt(ItemStack _itemStack)
	{
		this.toolbelt.TryStackItem(0, _itemStack);
		return _itemStack.count > 0 && this.toolbelt.AddItem(_itemStack);
	}

	public bool HasItem(ItemStack _itemStack)
	{
		return this.HasItems(new ItemStack[]
		{
			_itemStack
		}, 1);
	}

	public bool HasItems(IList<ItemStack> _itemStacks, int _multiplier = 1)
	{
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= this.backpack.GetItemCount(_itemStacks[i].itemValue, -1, -1, true);
			if (num > 0)
			{
				num -= this.toolbelt.GetItemCount(_itemStacks[i].itemValue, false, -1, -1, true);
			}
			if (num > 0)
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveItem(ItemStack _itemStack)
	{
		this.RemoveItems(new ItemStack[]
		{
			_itemStack
		}, 1, null);
	}

	public void RemoveItems(IList<ItemStack> _itemStacks, int _multiplier = 1, IList<ItemStack> _removedItems = null)
	{
		if (!this.HasItems(_itemStacks, 1))
		{
			return;
		}
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= this.backpack.DecItem(_itemStacks[i].itemValue, num, true, _removedItems);
			if (num > 0)
			{
				this.toolbelt.DecItem(_itemStacks[i].itemValue, num, true, _removedItems);
			}
		}
		this.onBackpackItemsChanged();
		this.onToolbeltItemsChanged();
	}

	public int GetItemCount(ItemValue _itemValue)
	{
		return 0 + this.backpack.GetItemCount(_itemValue, -1, -1, true) + this.toolbelt.GetItemCount(_itemValue, false, -1, -1, true);
	}

	public int GetItemCountWithMods(ItemValue _itemValue)
	{
		return 0 + this.backpack.GetItemCount(_itemValue, -1, -1, false) + this.toolbelt.GetItemCount(_itemValue, false, -1, -1, false);
	}

	public int GetItemCount(int _itemId)
	{
		ItemValue itemValue = new ItemValue(_itemId, false);
		return 0 + this.backpack.GetItemCount(itemValue, -1, -1, true) + this.toolbelt.GetItemCount(itemValue, false, -1, -1, true);
	}

	public List<ItemStack> GetAllItemStacks()
	{
		List<ItemStack> list = new List<ItemStack>();
		list.AddRange(this.GetBackpackItemStacks());
		list.AddRange(this.GetToolbeltItemStacks());
		return list;
	}

	public ItemStack[] GetBackpackItemStacks()
	{
		return this.backpack.GetSlots();
	}

	public ItemStack[] GetToolbeltItemStacks()
	{
		return this.toolbelt.GetSlots();
	}

	public void SetBackpackItemStacks(ItemStack[] _itemStacks)
	{
		this.backpack.SetSlots(_itemStacks);
		this.onBackpackItemsChanged();
	}

	public void SetToolbeltItemStacks(ItemStack[] _itemStacks)
	{
		this.toolbelt.SetSlots(_itemStacks, false);
		this.onToolbeltItemsChanged();
	}

	public void RefreshCurrency()
	{
		int itemCount = this.GetItemCount(this.currencyItem);
		if (itemCount != this.CurrencyAmount)
		{
			this.CurrencyAmount = itemCount;
			if (this.OnCurrencyChanged != null)
			{
				this.OnCurrencyChanged();
			}
		}
	}

	public void HandleUIShutdown()
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.localPlayer);
		if (uiforPlayer != null)
		{
			uiforPlayer.OnUIShutdown -= this.HandleUIShutdown;
		}
		this.backpack.OnBackpackItemsChangedInternal -= this.onBackpackItemsChanged;
		this.toolbelt.OnToolbeltItemsChangedInternal -= this.onToolbeltItemsChanged;
	}

	public bool AddItem(ItemStack _itemStack)
	{
		return this.AddItem(_itemStack, true);
	}

	[return: TupleElementNames(new string[]
	{
		"anyMoved",
		"allMoved"
	})]
	public ValueTuple<bool, bool> TryStackItem(int _startIndex, ItemStack _itemStack)
	{
		bool flag = true;
		int count = _itemStack.count;
		bool flag2 = this.toolbelt.TryStackItem(_startIndex, _itemStack);
		if (!flag2)
		{
			flag2 = this.backpack.TryStackItem(_startIndex, _itemStack).Item2;
		}
		if (count != _itemStack.count)
		{
			ItemStack itemStack = new ItemStack(_itemStack.itemValue, count - _itemStack.count);
			QuestEventManager.Current.ItemAdded(itemStack);
			XUiC_CollectedItemList collectedItemList = this.xui.CollectedItemList;
			if (collectedItemList != null)
			{
				collectedItemList.AddItemStack(itemStack, false);
			}
			if (flag)
			{
				Manager.PlayInsidePlayerHead("item_pickup", -1, 0f, false, false);
			}
		}
		return new ValueTuple<bool, bool>(count != _itemStack.count, flag2);
	}

	public bool HasItem(ItemValue _item)
	{
		return this.GetItemCount(_item) > 0;
	}

	public static bool TryStackItem(int _startIndex, ItemStack _itemStack, ItemStack[] _items)
	{
		int num = 0;
		for (int i = _startIndex; i < _items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == _items[i].itemValue.type && _items[i].CanStackPartly(ref num))
			{
				_items[i].count += num;
				_itemStack.count -= num;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bag backpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public Inventory toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ItemValue currencyItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;
}
