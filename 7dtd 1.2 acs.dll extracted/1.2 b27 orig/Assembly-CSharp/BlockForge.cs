using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockForge : BlockWorkstation
{
	public BlockForge()
	{
		this.CraftingParticleLightIntensity = 1.6f;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		this.MaterialUpdate(_world, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.checkParticles(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		this.MaterialUpdate(_world, _blockPos, _blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return Localization.Get("useForge", false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MaterialUpdate(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
		if (chunk != null)
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
			if (blockEntity != null && blockEntity.bHasTransform)
			{
				Renderer[] componentsInChildren = blockEntity.transform.GetComponentsInChildren<MeshRenderer>(true);
				Renderer[] array = componentsInChildren;
				if (array.Length != 0)
				{
					Material material = array[0].material;
					if (material)
					{
						float value = (float)((_blockValue.meta == 0) ? 0 : 20);
						material.SetFloat("_EmissionMultiply", value);
						for (int i = 1; i < array.Length; i++)
						{
							array[i].material = material;
						}
					}
				}
			}
		}
	}
}
