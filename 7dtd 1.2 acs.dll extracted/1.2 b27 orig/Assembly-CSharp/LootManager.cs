using System;
using System.Collections.Generic;

public class LootManager
{
	public LootManager(WorldBase _world)
	{
		this.world = _world;
		this.Random = _world.GetGameRandom();
	}

	public void LootContainerOpened(ITileEntityLootable _tileEntity, int _entityIdThatOpenedIt, FastTags<TagGroup.Global> _containerTags)
	{
		if (this.world.IsEditor())
		{
			return;
		}
		if (_tileEntity.bTouched)
		{
			return;
		}
		_tileEntity.bTouched = true;
		_tileEntity.worldTimeTouched = this.world.GetWorldTime();
		LootContainer lootContainer = LootContainer.GetLootContainer(_tileEntity.lootListName, true);
		if (lootContainer == null)
		{
			return;
		}
		bool flag = _tileEntity.IsEmpty();
		_tileEntity.bTouched = true;
		_tileEntity.worldTimeTouched = this.world.GetWorldTime();
		if (!flag)
		{
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)this.world.GetEntity(_entityIdThatOpenedIt);
		if (entityPlayer == null)
		{
			return;
		}
		entityPlayer.MinEventContext.TileEntity = _tileEntity;
		entityPlayer.FireEvent(MinEventTypes.onSelfOpenLootContainer, true);
		float containerMod = 0f;
		float containerBonus = 0f;
		if (_tileEntity.EntityId == -1)
		{
			containerMod = _tileEntity.LootStageMod;
			containerBonus = _tileEntity.LootStageBonus;
		}
		int num = lootContainer.useUnmodifiedLootstage ? entityPlayer.unModifiedGameStage : entityPlayer.GetHighestPartyLootStage(containerMod, containerBonus);
		IList<ItemStack> list = lootContainer.Spawn(this.Random, _tileEntity.items.Length, (float)num, 0f, entityPlayer, _containerTags, lootContainer.UniqueItems, lootContainer.IgnoreLootProb);
		for (int i = 0; i < list.Count; i++)
		{
			_tileEntity.items[i] = list[i].Clone();
		}
		entityPlayer.FireEvent(MinEventTypes.onSelfLootContainer, true);
	}

	public GameRandom Random;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBase world;

	public static float[] POITierMod;

	public static float[] POITierBonus;
}
