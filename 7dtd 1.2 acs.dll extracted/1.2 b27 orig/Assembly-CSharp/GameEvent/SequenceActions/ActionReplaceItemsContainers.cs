using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionReplaceItemsContainers : ActionBaseContainersAction
	{
		public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
		{
			isEmpty = true;
			TileEntityType tileEntityType = te.GetTileEntityType();
			if (tileEntityType <= TileEntityType.SecureLoot)
			{
				if (tileEntityType != TileEntityType.Loot && tileEntityType != TileEntityType.SecureLoot)
				{
					return false;
				}
			}
			else if (tileEntityType != TileEntityType.Workstation)
			{
				if (tileEntityType != TileEntityType.SecureLootSigned && tileEntityType != TileEntityType.Composite)
				{
					return false;
				}
			}
			else
			{
				if (!this.includeOutputs)
				{
					return false;
				}
				TileEntityWorkstation tileEntityWorkstation = te as TileEntityWorkstation;
				if (tileEntityWorkstation != null)
				{
					foreach (ItemStack itemStack in tileEntityWorkstation.Output)
					{
						if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
						{
							isEmpty = false;
						}
					}
					return true;
				}
				return false;
			}
			ITileEntityLootable tileEntityLootable;
			if (te.TryGetSelfOrFeature(out tileEntityLootable))
			{
				for (int j = 0; j < tileEntityLootable.items.Length; j++)
				{
					ItemStack itemStack2 = tileEntityLootable.items[j];
					if (!itemStack2.IsEmpty() && itemStack2.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack2.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
					{
						isEmpty = false;
					}
				}
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleContainerAction(List<TileEntity> tileEntityList)
		{
			new List<ItemStack>();
			List<TileEntity> list = new List<TileEntity>();
			bool flag = false;
			int i = 0;
			while (i < tileEntityList.Count)
			{
				bool flag2 = false;
				TileEntityType tileEntityType = tileEntityList[i].GetTileEntityType();
				if (tileEntityType <= TileEntityType.SecureLoot)
				{
					if (tileEntityType == TileEntityType.Loot || tileEntityType == TileEntityType.SecureLoot)
					{
						goto IL_53;
					}
				}
				else if (tileEntityType != TileEntityType.Workstation)
				{
					if (tileEntityType == TileEntityType.SecureLootSigned || tileEntityType == TileEntityType.Composite)
					{
						goto IL_53;
					}
				}
				else if (this.includeOutputs)
				{
					TileEntityWorkstation tileEntityWorkstation = tileEntityList[i] as TileEntityWorkstation;
					if (tileEntityWorkstation != null && tileEntityWorkstation.EntityId == -1)
					{
						ItemStack[] output = tileEntityWorkstation.Output;
						for (int j = 0; j < output.Length; j++)
						{
							ItemStack itemStack = output[j];
							if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
							{
								output[j] = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), itemStack.count);
								flag = true;
								flag2 = true;
							}
						}
						if (flag2)
						{
							tileEntityWorkstation.Output = output;
							list.Add(tileEntityWorkstation);
						}
					}
				}
				IL_1CA:
				if (flag2)
				{
					tileEntityList[i].SetModified();
				}
				i++;
				continue;
				IL_53:
				ITileEntityLootable tileEntityLootable;
				if (tileEntityList[i].TryGetSelfOrFeature(out tileEntityLootable) && tileEntityLootable.EntityId == -1)
				{
					for (int k = 0; k < tileEntityLootable.items.Length; k++)
					{
						ItemStack itemStack2 = tileEntityLootable.items[k];
						if (!itemStack2.IsEmpty() && itemStack2.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack2.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
						{
							tileEntityLootable.items[k] = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), itemStack2.count);
							flag = true;
							flag2 = true;
						}
					}
					goto IL_1CA;
				}
				goto IL_1CA;
			}
			if (flag && this.changeName && base.Owner.Target != null)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
				for (int l = 0; l < tileEntityList.Count; l++)
				{
					ITileEntitySignable tileEntitySignable;
					if ((tileEntityList[l].GetTileEntityType() == TileEntityType.SecureLootSigned || tileEntityList[l].GetTileEntityType() == TileEntityType.Composite) && tileEntityList[l].TryGetSelfOrFeature(out tileEntitySignable) && tileEntitySignable.EntityId == -1)
					{
						tileEntitySignable.SetText(base.ModifiedName, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
					}
				}
			}
			return flag;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseBool(ActionReplaceItemsContainers.PropIncludeOutputs, ref this.includeOutputs);
			properties.ParseString(ActionReplaceItemsContainers.PropReplacedByItem, ref this.ReplacedByItem);
			properties.ParseString(ActionReplaceItemsContainers.PropItemTag, ref this.itemTags);
			this.fastItemTags = FastTags<TagGroup.Global>.Parse(this.itemTags);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionReplaceItemsContainers
			{
				TargetingType = this.TargetingType,
				maxDistance = this.maxDistance,
				newName = this.newName,
				changeName = this.changeName,
				includeOutputs = this.includeOutputs,
				tileEntityList = this.tileEntityList,
				fastItemTags = this.fastItemTags,
				ReplacedByItem = this.ReplacedByItem
			};
		}

		public string ReplacedByItem = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool includeOutputs;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string itemTags = "";

		public static string PropReplacedByItem = "replaced_by_item";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIncludeOutputs = "include_outputs";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropItemTag = "items_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public FastTags<TagGroup.Global> fastItemTags = FastTags<TagGroup.Global>.none;
	}
}
