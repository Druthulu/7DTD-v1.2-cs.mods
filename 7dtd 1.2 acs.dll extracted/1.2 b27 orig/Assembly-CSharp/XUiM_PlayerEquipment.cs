using System;

public class XUiM_PlayerEquipment : XUiModel
{
	public static event XUiEvent_RefreshEquipment HandleRefreshEquipment;

	public Equipment Equipment
	{
		get
		{
			return this.equipment;
		}
	}

	public XUiM_PlayerEquipment(XUi _xui, EntityPlayerLocal _player)
	{
		if (!_player)
		{
			return;
		}
		this.xui = _xui;
		this.equipment = _player.equipment;
	}

	public ItemStack EquipItem(ItemStack stack)
	{
		ItemClassArmor itemClassArmor = stack.itemValue.ItemClass as ItemClassArmor;
		if (itemClassArmor != null)
		{
			EquipmentSlots equipSlot = itemClassArmor.EquipSlot;
			ItemStack stackFromSlot = this.GetStackFromSlot(equipSlot);
			if (!stackFromSlot.IsEmpty())
			{
				ItemClassArmor itemClassArmor2 = stackFromSlot.itemValue.ItemClass as ItemClassArmor;
				if (itemClassArmor2 != null)
				{
					this.equipment.SetSlotItem((int)itemClassArmor2.EquipSlot, null, true);
				}
			}
			this.equipment.SetSlotItem((int)equipSlot, stack.itemValue.Clone(), true);
			QuestEventManager.Current.WoreItem(stack.itemValue);
			this.RefreshEquipment();
			return stackFromSlot;
		}
		this.RefreshEquipment();
		return stack;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool IsWearing(ItemValue itemValue)
	{
		ItemClassArmor itemClassArmor = itemValue.ItemClass as ItemClassArmor;
		if (itemClassArmor != null)
		{
			EquipmentSlots equipSlot = itemClassArmor.EquipSlot;
			return equipSlot != EquipmentSlots.Count && this.GetStackFromSlot(equipSlot).itemValue.type == itemValue.type;
		}
		return false;
	}

	public bool IsEquipmentTypeWorn(EquipmentSlots slot)
	{
		return slot != EquipmentSlots.Count && !this.GetStackFromSlot(slot).itemValue.IsEmpty();
	}

	public void RefreshEquipment()
	{
		this.equipment.FireEventsForSetSlots();
		if (XUiM_PlayerEquipment.HandleRefreshEquipment != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment(this);
		}
		this.equipment.FireEventsForChangedSlots();
	}

	public ItemStack GetStackFromSlot(EquipmentSlots slot)
	{
		ItemStack itemStack = ItemStack.Empty.Clone();
		ItemValue slotItem = this.Equipment.GetSlotItem((int)slot);
		if (slotItem != null)
		{
			itemStack.itemValue = slotItem;
			itemStack.count = 1;
			return itemStack;
		}
		return itemStack;
	}

	public bool IsOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public Equipment equipment;
}
