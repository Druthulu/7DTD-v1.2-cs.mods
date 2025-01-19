using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassBlock : ItemClass
{
	public ItemClassBlock()
	{
		this.HoldType = new DataItem<int>(7);
		AnimationDelayData.AnimationDelay[this.HoldType.Value] = new AnimationDelayData.AnimationDelays(0f, 0f, 0.31f, 0.31f, true);
		this.Stacknumber = new DataItem<int>(500);
	}

	public override void Init()
	{
		base.Init();
		Block block = Block.list[base.Id];
		this.DescriptionKey = block.DescriptionKey;
		this.MadeOfMaterial = block.blockMaterial;
		if (block.CustomIcon != null)
		{
			this.CustomIcon = new DataItem<string>(block.CustomIcon);
		}
		this.NoScrapping = block.NoScrapping;
		this.CustomIconTint = block.CustomIconTint;
		this.SortOrder = block.SortOrder;
		this.CreativeMode = block.CreativeMode;
		this.TraderStageTemplate = block.TraderStageTemplate;
		this.SoundPickup = block.SoundPickup;
		this.SoundPlace = block.SoundPlace;
	}

	public override bool IsActionRunning(ItemInventoryData _data)
	{
		return Time.time - (_data as ItemClassBlock.ItemBlockInventoryData).lastBuildTime < Constants.cBuildIntervall;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ItemInventoryData createItemInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
	{
		return new ItemClassBlock.ItemBlockInventoryData(this, _itemStack, _gameManager, _holdingEntity, _slotIdx);
	}

	public override void StopHolding(ItemInventoryData _data, Transform _modelTransform)
	{
	}

	public override bool IsBlock()
	{
		return true;
	}

	public override Block GetBlock()
	{
		return Block.list[base.Id];
	}

	public override string GetItemName()
	{
		return this.GetBlock().GetBlockName();
	}

	public override string GetLocalizedItemName()
	{
		return this.GetBlock().GetLocalizedBlockName();
	}

	public override bool HasAnyTags(FastTags<TagGroup.Global> _tags)
	{
		return this.GetBlock().Tags.Test_AnySet(_tags);
	}

	public override bool HasAllTags(FastTags<TagGroup.Global> _tags)
	{
		return this.GetBlock().Tags.Test_AllSet(_tags);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue GetBlockValueFromItemValue(ItemValue _itemValue)
	{
		Block block = this.GetBlock();
		if (block.SelectAlternates)
		{
			return block.GetAltBlockValue(_itemValue.Meta);
		}
		return _itemValue.ToBlockValue();
	}

	public override Transform CloneModel(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose, long _textureFull = 0L)
	{
		return ItemClassBlock.CreateMesh(_go, _world, _blockValue, _vertices, _position, _parent, _purpose, _textureFull);
	}

	public override Transform CloneModel(World _world, ItemValue _itemValue, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose, long _textureFull = 0L)
	{
		return ItemClassBlock.CreateMesh(null, _world, this.GetBlockValueFromItemValue(_itemValue), null, _position, _parent, _purpose, _textureFull);
	}

	public static Transform CreateMesh(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _worldPos, Transform _parent, BlockShape.MeshPurpose _purpose, long _textureFull = 0L)
	{
		if (_purpose == BlockShape.MeshPurpose.Drop)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(LoadManager.LoadAsset<GameObject>("@:Other/Items/Misc/sack_droppedPrefab.prefab", null, null, false, true).Asset);
			Transform transform = gameObject.transform;
			transform.SetParent(_parent, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			return gameObject.transform;
		}
		Block block = _blockValue.Block;
		BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
		if (blockShapeModelEntity != null)
		{
			if (_go == null)
			{
				_go = new GameObject();
				_go.transform.SetParent(_parent, false);
			}
			blockShapeModelEntity.CloneModel(_blockValue, _go.transform);
			return _go.transform;
		}
		int meshIndex = (int)block.MeshIndex;
		Transform transform2 = ItemClassBlock.CreateMeshOfType(_go, _world, _blockValue, _vertices, _worldPos, _parent, _purpose, _textureFull, meshIndex);
		if (meshIndex == 0 && (_purpose == BlockShape.MeshPurpose.Preview || _purpose == BlockShape.MeshPurpose.Local))
		{
			Transform transform3 = ItemClassBlock.CreateMeshOfType(_go, _world, _blockValue, _vertices, _worldPos, _parent, _purpose, _textureFull, 2);
			if (transform3)
			{
				if (transform2)
				{
					transform3.SetParent(transform2, false);
				}
				else
				{
					transform2 = transform3;
				}
			}
		}
		return transform2;
	}

	public static Transform CreateMeshOfType(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _worldPos, Transform _parent, BlockShape.MeshPurpose _purpose, long _textureFull, int _meshIndex)
	{
		Vector3i vector3i = World.worldToBlockPos(_worldPos);
		byte sun;
		byte block;
		_world.GetSunAndBlockColors(vector3i, out sun, out block);
		VoxelMesh voxelMesh = VoxelMesh.Create(_meshIndex, MeshDescription.meshes[_meshIndex].meshType, 1);
		VoxelMesh[] array = new VoxelMesh[MeshDescription.meshes.Length];
		array[_meshIndex] = voxelMesh;
		_blockValue.Block.shape.renderFull(vector3i, _blockValue, ItemClassBlock.renderOffsetV, _vertices, new LightingAround(sun, block, 0), _textureFull, array, _purpose);
		if (voxelMesh.m_Vertices.Count == 0)
		{
			return null;
		}
		if (_go == null)
		{
			_go = new GameObject();
			_go.transform.SetParent(_parent, false);
			_go.AddComponent<UpdateLightOnChunkMesh>();
		}
		_go.name = "Block_" + _blockValue.type.ToString();
		MeshFilter[] array2;
		MeshRenderer[] mr;
		VoxelMesh.CreateMeshFilter(_meshIndex, 0, _go, "Item", false, out array2, out mr);
		if (array2[0] != null)
		{
			voxelMesh.CopyToMesh(array2, mr, 0);
		}
		return _go.transform;
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemInventoryData _holdingData)
	{
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override RenderCubeType GetFocusType(ItemInventoryData _data)
	{
		return RenderCubeType.FullBlockBothSides;
	}

	public override float GetFocusRange()
	{
		return Constants.cDigAndBuildDistance;
	}

	public override void ExecuteAction(int _actionIdx, ItemInventoryData _data, bool _bReleased, PlayerActionsLocal _playerActions)
	{
		if (_actionIdx == 0)
		{
			GameManager.Instance.GetActiveBlockTool().ExecuteAttackAction(_data, _bReleased, _playerActions);
			return;
		}
		GameManager.Instance.GetActiveBlockTool().ExecuteUseAction(_data, _bReleased, _playerActions);
	}

	public override bool IsFocusBlockInside()
	{
		return false;
	}

	public override Vector3 GetDroppedCorrectionRotation()
	{
		return Vector3.zero;
	}

	public override Vector3 GetCorrectionRotation()
	{
		return new Vector3(90f, 0f, 0f);
	}

	public override Vector3 GetCorrectionPosition()
	{
		return Vector3.zero;
	}

	public override Vector3 GetCorrectionScale()
	{
		return new Vector3(0.1f, 0.1f, 0.1f);
	}

	public override bool CanHold()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cNoRotation = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 renderOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	public class ItemBlockInventoryData : ItemInventoryData
	{
		public ItemBlockInventoryData(ItemClass _item, ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx) : base(_item, _itemStack, _gameManager, _holdingEntity, _slotIdx)
		{
			this.lastBuildTime = 0f;
			this.rotation = 128;
			Block block = Block.list[_item.Id];
			if (block.HandleFace != BlockFace.None)
			{
				this.mode = BlockPlacement.EnumRotationMode.ToFace;
			}
			if (block.BlockPlacementHelper != BlockPlacement.None)
			{
				this.mode = BlockPlacement.EnumRotationMode.Auto;
			}
		}

		public float lastBuildTime;

		public byte rotation;

		public BlockPlacement.EnumRotationMode mode = BlockPlacement.EnumRotationMode.Simple;

		public int localRot;

		public int damage;
	}
}
