using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Bag : IInventory
{
	public event XUiEvent_BackpackItemsChangedInternal OnBackpackItemsChangedInternal;

	public Bag(EntityAlive _entity)
	{
		this.entity = _entity;
	}

	public int SlotCount
	{
		get
		{
			return this.GetSlots().Length;
		}
	}

	public bool[] LockedSlots { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkBagAssigned(int slotCount = 45)
	{
		if (this.items == null)
		{
			this.items = ItemStack.CreateArray((int)EffectManager.GetValue(PassiveEffects.BagSize, null, (float)slotCount, this.entity, null, default(FastTags<TagGroup.Global>), false, false, true, true, true, 1, true, false));
		}
	}

	public void Clear()
	{
		ItemStack[] array = this.items;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
		}
		this.onBackpackChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBackpackChanged()
	{
		if (this.OnBackpackItemsChangedInternal != null)
		{
			this.OnBackpackItemsChangedInternal();
		}
	}

	public bool CanStackNoEmpty(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].CanStackPartlyWith(_itemStack))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEmpty()
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanStack(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty() || slots[i].CanStackWith(_itemStack, false))
			{
				return true;
			}
		}
		return false;
	}

	[return: TupleElementNames(new string[]
	{
		"anyMoved",
		"allMoved"
	})]
	public ValueTuple<bool, bool> TryStackItem(int startIndex, ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		bool item = false;
		for (int i = startIndex; i < slots.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == slots[i].itemValue.type && slots[i].CanStackPartly(ref num))
			{
				slots[i].count += num;
				_itemStack.count -= num;
				this.onBackpackChanged();
				item = true;
				if (_itemStack.count == 0)
				{
					return new ValueTuple<bool, bool>(true, true);
				}
			}
		}
		return new ValueTuple<bool, bool>(item, false);
	}

	public bool CanTakeItem(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].CanStackPartlyWith(_itemStack))
			{
				return true;
			}
			if (slots[i].IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public ItemStack[] GetSlots()
	{
		this.checkBagAssigned(45);
		return this.items;
	}

	public void SetupSlots(ItemStack[] _slots)
	{
		this.checkBagAssigned(_slots.Length);
		this.items = _slots;
		this.onBackpackChanged();
	}

	public void SetSlots(ItemStack[] _slots)
	{
		this.checkBagAssigned(45);
		this.items = _slots;
		this.onBackpackChanged();
	}

	public void SetSlot(int index, ItemStack _stack, bool callChangedEvent = true)
	{
		if (index >= this.items.Length)
		{
			return;
		}
		this.items[index] = _stack;
		if (callChangedEvent)
		{
			this.onBackpackChanged();
		}
	}

	public void OnUpdate()
	{
		if (this.entity is EntityPlayer)
		{
			this.MaxItemCount = (int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, this.entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			this.entity.Buffs.SetCustomVar("_carrycapacity", (float)this.MaxItemCount, true);
			this.entity.Buffs.SetCustomVar("_encumbrance", Mathf.Max((float)(this.GetUsedSlotCount() - this.MaxItemCount), 0f) / (float)(this.items.Length - this.MaxItemCount), true);
			this.entity.Buffs.SetCustomVar("_encumberedslots", Mathf.Max((float)(this.GetUsedSlotCount() - this.MaxItemCount), 0f), true);
		}
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (this.items == null)
		{
			this.items = ItemStack.CreateArray((int)EffectManager.GetValue(PassiveEffects.BagSize, null, 45f, this.entity, null, default(FastTags<TagGroup.Global>), false, false, true, true, true, 1, true, false));
		}
		bool flag = ItemStack.AddToItemStackArray(this.items, _itemStack, -1) >= 0;
		if (flag)
		{
			this.onBackpackChanged();
		}
		return flag;
	}

	public int DecItem(ItemValue _itemValue, int _count, bool _ignoreModdedItems = false, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		ItemStack[] slots = this.GetSlots();
		int num2 = 0;
		while (_count > 0 && num2 < this.GetSlots().Length)
		{
			if (slots[num2].itemValue.type == _itemValue.type && (!_ignoreModdedItems || !slots[num2].itemValue.HasModSlots || !slots[num2].itemValue.HasMods()))
			{
				if (ItemClass.GetForId(slots[num2].itemValue.type).CanStack())
				{
					int count = slots[num2].count;
					int num3 = (count >= _count) ? _count : count;
					if (_removedItems != null)
					{
						_removedItems.Add(new ItemStack(slots[num2].itemValue.Clone(), num3));
					}
					slots[num2].count -= num3;
					_count -= num3;
					if (slots[num2].count <= 0)
					{
						slots[num2].Clear();
					}
				}
				else
				{
					if (_removedItems != null)
					{
						_removedItems.Add(slots[num2].Clone());
					}
					slots[num2].Clear();
					_count--;
				}
			}
			num2++;
		}
		this.SetSlots(slots);
		return num - _count;
	}

	public int GetUsedSlotCount()
	{
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public int GetItemCount(ItemValue _itemValue, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && slots[i].itemValue.type == _itemValue.type && (_seed == -1 || _seed == (int)slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public bool HasItem(ItemValue _item)
	{
		return this.GetItemCount(_item, -1, -1, true) > 0;
	}

	public ItemStack[] CloneItemStack()
	{
		this.checkBagAssigned(45);
		ItemStack[] array = new ItemStack[this.items.Length];
		for (int i = 0; i < this.items.Length; i++)
		{
			array[i] = this.items[i].Clone();
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	public int MaxItemCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;
}
