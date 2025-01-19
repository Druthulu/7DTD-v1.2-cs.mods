using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionExchangeBlock : ItemAction
{
	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Sourceblock"))
		{
			throw new Exception("Missing attribute 'sourceblock' in use_action 'ExchangeBlock'");
		}
		string text = _props.Values["Sourceblock"];
		this.sourceblock = ItemClass.GetItem(text, false).ToBlockValue();
		if (this.sourceblock.Equals(BlockValue.Air))
		{
			throw new Exception("Unknown block name '" + text + "' in use_action!");
		}
		if (!_props.Values.ContainsKey("Targetblock"))
		{
			throw new Exception("Missing attribute 'targetblock' in use_action 'ExchangeBlock'");
		}
		text = _props.Values["Targetblock"];
		this.targetBlock = ItemClass.GetItem(text, false).ToBlockValue();
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (_bReleased)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime > Constants.cBuildIntervall)
		{
			ItemInventoryData invData = _actionData.invData;
			if (invData.hitInfo.bHitValid && GameUtils.IsBlockOrTerrain(invData.hitInfo.tag))
			{
				Vector3i blockPos = invData.hitInfo.hit.blockPos;
				BlockValue block = invData.world.GetBlock(blockPos);
				if (block.Equals(this.sourceblock))
				{
					_actionData.lastUseTime = Time.time;
					if (block.type != this.targetBlock.type)
					{
						invData.world.SetBlockRPC(blockPos, this.targetBlock);
						invData.holdingEntity.PlayOneShot((this.soundStart != null) ? this.soundStart : "placeblock", false, false, false);
					}
				}
				invData.holdingEntity.RightArmAnimationAttack = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue sourceblock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue targetBlock;
}
