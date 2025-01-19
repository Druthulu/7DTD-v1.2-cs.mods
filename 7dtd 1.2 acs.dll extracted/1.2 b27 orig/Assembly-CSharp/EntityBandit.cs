using System;
using UnityEngine.Scripting;

[Preserve]
public class EntityBandit : EntityHuman
{
	public override void PostInit()
	{
		ItemValue bareHandItemValue = this.inventory.GetBareHandItemValue();
		bareHandItemValue.Quality = (ushort)this.rand.RandomRange(1, 3);
		bareHandItemValue.UseTimes = (float)bareHandItemValue.MaxUseTimes * 0.7f - 1f;
		this.inventory.SetItem(0, bareHandItemValue, 1, true);
	}

	public override bool Attack(bool _bAttackReleased)
	{
		if (!_bAttackReleased)
		{
			ItemActionAttackData itemActionAttackData = this.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
			if (itemActionAttackData != null)
			{
				ItemValue itemValue = itemActionAttackData.invData.itemValue;
				itemValue.UseTimes = (float)itemValue.MaxUseTimes * 0.8f - 1f;
				if (itemActionAttackData is ItemActionRanged.ItemActionDataRanged)
				{
					itemValue.Meta = 2;
				}
			}
		}
		return base.Attack(_bAttackReleased);
	}
}
