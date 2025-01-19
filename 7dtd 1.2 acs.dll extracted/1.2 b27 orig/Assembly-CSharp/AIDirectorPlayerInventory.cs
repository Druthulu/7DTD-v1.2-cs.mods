using System;
using System.Collections.Generic;
using System.IO;

public struct AIDirectorPlayerInventory : IEquatable<AIDirectorPlayerInventory>
{
	public static AIDirectorPlayerInventory FromEntity(EntityAlive entity)
	{
		AIDirectorPlayerInventory result;
		result.bag = AIDirectorPlayerInventory.TrackedItemsFromBag(entity.bag);
		result.belt = AIDirectorPlayerInventory.TrackedItemsFromInventory(entity.inventory);
		return result;
	}

	public bool Equals(AIDirectorPlayerInventory other)
	{
		return this.bag != null == (other.bag != null) && this.belt != null == (other.belt != null) && AIDirectorPlayerInventory.OrderIndependantEquals(this.bag, other.bag) && AIDirectorPlayerInventory.OrderIndependantEquals(this.belt, other.belt);
	}

	public static List<AIDirectorPlayerInventory.ItemId> TrackedItemsFromBag(Bag bag)
	{
		List<AIDirectorPlayerInventory.ItemId> list = null;
		foreach (ItemStack itemStack in bag.GetSlots())
		{
			if (!itemStack.IsEmpty())
			{
				ItemClass itemClass = itemStack.itemValue.ItemClass;
				if (itemClass != null && itemClass.Smell != null)
				{
					if (list == null)
					{
						list = new List<AIDirectorPlayerInventory.ItemId>();
					}
					AIDirectorPlayerInventory.AppendId(list, AIDirectorPlayerInventory.ItemId.FromStack(itemStack));
				}
			}
		}
		return list;
	}

	public static List<AIDirectorPlayerInventory.ItemId> TrackedItemsFromInventory(Inventory inv)
	{
		List<AIDirectorPlayerInventory.ItemId> list = null;
		for (int i = 0; i < inv.GetItemCount(); i++)
		{
			ItemStack item = inv.GetItem(i);
			if (!item.IsEmpty())
			{
				ItemClass itemClass = item.itemValue.ItemClass;
				if (itemClass != null && itemClass.Smell != null)
				{
					if (list == null)
					{
						list = new List<AIDirectorPlayerInventory.ItemId>();
					}
					AIDirectorPlayerInventory.AppendId(list, AIDirectorPlayerInventory.ItemId.FromStack(item));
				}
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AppendId(List<AIDirectorPlayerInventory.ItemId> list, AIDirectorPlayerInventory.ItemId id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			AIDirectorPlayerInventory.ItemId itemId = list[i];
			if (itemId.id == id.id)
			{
				itemId.count += id.count;
				list[i] = itemId;
				return;
			}
		}
		list.Add(id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool OrderIndependantEquals(List<AIDirectorPlayerInventory.ItemId> a, List<AIDirectorPlayerInventory.ItemId> b)
	{
		if (a == null && b == null)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			AIDirectorPlayerInventory.ItemId item = a[i];
			if (!b.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public List<AIDirectorPlayerInventory.ItemId> bag;

	public List<AIDirectorPlayerInventory.ItemId> belt;

	public struct ItemId : IEquatable<AIDirectorPlayerInventory.ItemId>
	{
		public static AIDirectorPlayerInventory.ItemId FromStack(ItemStack stack)
		{
			AIDirectorPlayerInventory.ItemId result;
			result.id = stack.itemValue.type;
			result.count = stack.count;
			return result;
		}

		public static AIDirectorPlayerInventory.ItemId Read(BinaryReader stream)
		{
			AIDirectorPlayerInventory.ItemId result;
			result.id = (int)stream.ReadInt16();
			result.count = (int)stream.ReadInt16();
			return result;
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write((short)this.id);
			stream.Write((short)this.count);
		}

		public bool Equals(AIDirectorPlayerInventory.ItemId other)
		{
			return this.id == other.id && this.count == other.count;
		}

		public const int kNetworkSize = 4;

		public int id;

		public int count;
	}
}
