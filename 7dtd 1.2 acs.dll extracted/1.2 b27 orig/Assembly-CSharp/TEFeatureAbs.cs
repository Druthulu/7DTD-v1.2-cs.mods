using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class TEFeatureAbs : ITileEntityFeature, ITileEntity
{
	public TileEntityFeatureData FeatureData { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public TileEntityComposite Parent { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public virtual void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		this.Parent = _parent;
		this.FeatureData = _featureData;
	}

	public virtual void CopyFrom(TileEntityComposite _other)
	{
		throw new NotImplementedException();
	}

	public void OnRemove(World _world)
	{
	}

	public virtual void OnUnload(World _world)
	{
	}

	public virtual void OnDestroy()
	{
	}

	public virtual void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _placingEntity)
	{
	}

	public virtual void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
	}

	public virtual void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
	}

	public virtual void Reset(FastTags<TagGroup.Global> _questTags)
	{
	}

	public virtual void UpdateTick(World _world)
	{
	}

	public virtual string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		return null;
	}

	public virtual void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
	}

	public virtual void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
	}

	public virtual bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		return false;
	}

	public virtual void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
	}

	public virtual void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
	}

	public event XUiEvent_TileEntityDestroyed Destroyed
	{
		add
		{
			this.Parent.Destroyed += value;
		}
		remove
		{
			this.Parent.Destroyed -= value;
		}
	}

	public List<ITileEntityChangedListener> listeners
	{
		get
		{
			return this.Parent.listeners;
		}
	}

	public void SetUserAccessing(bool _bUserAccessing)
	{
		this.Parent.SetUserAccessing(_bUserAccessing);
	}

	public bool IsUserAccessing()
	{
		return this.Parent.IsUserAccessing();
	}

	public void SetModified()
	{
		this.Parent.SetModified();
	}

	public Chunk GetChunk()
	{
		return this.Parent.GetChunk();
	}

	public Vector3i ToWorldPos()
	{
		return this.Parent.ToWorldPos();
	}

	public Vector3 ToWorldCenterPos()
	{
		return this.Parent.ToWorldCenterPos();
	}

	public BlockValue blockValue
	{
		get
		{
			return this.Parent.blockValue;
		}
	}

	public virtual int EntityId
	{
		get
		{
			return this.Parent.EntityId;
		}
		set
		{
		}
	}

	public int GetClrIdx()
	{
		return this.Parent.GetClrIdx();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CommandIs(ReadOnlySpan<char> _givenCommand, string _compareCommand)
	{
		return _givenCommand.Equals(_compareCommand, StringComparison.Ordinal);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TEFeatureAbs()
	{
	}
}
