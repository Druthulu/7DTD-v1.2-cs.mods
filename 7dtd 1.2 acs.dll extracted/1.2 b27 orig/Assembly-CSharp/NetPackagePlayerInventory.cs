using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerInventory : NetPackage
{
	public NetPackagePlayerInventory Setup(EntityPlayerLocal _player, bool _changedToolbelt, bool _changedBag, bool _changedEquipment, bool _changedDragAndDropItem)
	{
		if (_changedToolbelt)
		{
			this.toolbelt = ((_player.AttachedToEntity != null && _player.saveInventory != null) ? _player.saveInventory.CloneItemStack() : _player.inventory.CloneItemStack());
		}
		if (_changedBag)
		{
			this.bag = _player.bag.GetSlots();
		}
		if (_changedEquipment)
		{
			this.equipment = _player.equipment.Clone();
		}
		if (_changedDragAndDropItem)
		{
			this.dragAndDropItem = _player.DragAndDropItem.Clone();
		}
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		if (_reader.ReadBoolean())
		{
			this.toolbelt = GameUtils.ReadItemStack(_reader);
		}
		if (_reader.ReadBoolean())
		{
			this.bag = GameUtils.ReadItemStack(_reader);
		}
		if (_reader.ReadBoolean())
		{
			ItemValue[] array = GameUtils.ReadItemValueArray(_reader);
			this.equipment = new Equipment();
			int num = Utils.FastMin(array.Length, this.equipment.GetSlotCount());
			for (int i = 0; i < num; i++)
			{
				this.equipment.SetSlotItemRaw(i, array[i]);
			}
		}
		if (_reader.ReadBoolean())
		{
			ItemStack[] array2 = GameUtils.ReadItemStack(_reader);
			if (array2 != null && array2.Length != 0)
			{
				this.dragAndDropItem = array2[0];
			}
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.toolbelt != null);
		if (this.toolbelt != null)
		{
			GameUtils.WriteItemStack(_writer, this.toolbelt);
		}
		_writer.Write(this.bag != null);
		if (this.bag != null)
		{
			GameUtils.WriteItemStack(_writer, this.bag);
		}
		_writer.Write(this.equipment != null);
		if (this.equipment != null)
		{
			GameUtils.WriteItemValueArray(_writer, this.equipment.GetItems());
		}
		_writer.Write(this.dragAndDropItem != null);
		if (this.dragAndDropItem != null)
		{
			GameUtils.WriteItemStack(_writer, new ItemStack[]
			{
				this.dragAndDropItem
			});
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		PlayerDataFile latestPlayerData = base.Sender.latestPlayerData;
		if (this.toolbelt != null)
		{
			latestPlayerData.inventory = this.toolbelt;
		}
		if (this.bag != null)
		{
			latestPlayerData.bag = this.bag;
		}
		if (this.equipment != null)
		{
			latestPlayerData.equipment = this.equipment;
		}
		if (this.dragAndDropItem != null)
		{
			latestPlayerData.dragAndDropItem = this.dragAndDropItem;
		}
		latestPlayerData.bModifiedSinceLastSave = true;
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] bag;

	[PublicizedFrom(EAccessModifier.Private)]
	public Equipment equipment;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack dragAndDropItem;
}
