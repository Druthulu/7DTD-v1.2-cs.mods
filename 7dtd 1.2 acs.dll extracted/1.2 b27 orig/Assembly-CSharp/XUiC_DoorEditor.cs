using System;
using UnityEngine.Scripting;
using XMLData;

[Preserve]
public class XUiC_DoorEditor : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_DoorEditor.ID = base.WindowGroup.ID;
		this.btnDowngrade = base.GetChildById("btnDowngrade").GetChildByType<XUiC_SimpleButton>();
		this.btnDowngrade.OnPressed += this.BtnDowngrade_OnPressed;
		this.btnUpgrade = base.GetChildById("btnUpgrade").GetChildByType<XUiC_SimpleButton>();
		this.btnUpgrade.OnPressed += this.BtnUpgrade_OnPressed;
		this.cbxColorPresetList = (XUiC_ComboBoxList<string>)base.GetChildById("cbxPresets");
		this.cbxColorPresetList.OnValueChanged += this.CbxColorPresetList_OnValueChanged;
		foreach (string item in ColorMappingData.Instance.IDFromName.Keys)
		{
			this.cbxColorPresetList.Elements.Add(item);
		}
		this.btnOpenClose = base.GetChildById("btnOpenClose").GetChildByType<XUiC_SimpleButton>();
		this.btnOpenClose.OnPressed += this.BtnOpenClose_OnPressed;
		this.btnCancel = base.GetChildById("btnCancel").GetChildByType<XUiC_SimpleButton>();
		this.btnCancel.OnPressed += this.BtnCancel_OnPressed;
		this.btnOk = base.GetChildById("btnOk").GetChildByType<XUiC_SimpleButton>();
		this.btnOk.OnPressed += this.BtnOk_OnPressed;
	}

	public static void Open(LocalPlayerUI _playerUi, TileEntitySecureDoor _te, Vector3i _blockPos, World _world, int _cIdx)
	{
		XUiC_DoorEditor childByType = _playerUi.xui.FindWindowGroupByName(XUiC_DoorEditor.ID).GetChildByType<XUiC_DoorEditor>();
		childByType.world = _world;
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		childByType.chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
		BlockEntityData blockEntity = childByType.chunk.GetBlockEntity(_blockPos);
		childByType.blockPos = _blockPos;
		childByType.initialColorIdx = blockEntity.blockValue.meta2;
		childByType.initialDamage = blockEntity.blockValue.damage;
		childByType.cbxColorPresetList.Value = ColorMappingData.Instance.NameFromID[(int)blockEntity.blockValue.meta2];
		childByType.bAcceptChanges = false;
		_playerUi.windowManager.Open(XUiC_DoorEditor.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxColorPresetList_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		int num;
		if (ColorMappingData.Instance.IDFromName.TryGetValue(_newValue, out num) && ColorMappingData.Instance.ColorFromID.ContainsKey(num))
		{
			this.UpdateDoorColor(num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDowngrade_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.UpdateDoorHealth(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpgrade_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.UpdateDoorHealth(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDoorHealth(bool _upgrade)
	{
		BlockEntityData blockEntity = this.chunk.GetBlockEntity(this.blockPos);
		Block block = blockEntity.blockValue.Block;
		BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
		if (blockShapeModelEntity == null)
		{
			Log.Warning(string.Format("block {0} does not have shape field. Cannot change damage state.", block));
			return;
		}
		int num = _upgrade ? ((int)blockShapeModelEntity.GetNextDamageStateUpHealth(blockEntity.blockValue)) : ((int)blockShapeModelEntity.GetNextDamageStateDownHealth(blockEntity.blockValue));
		blockEntity.blockValue.damage = block.MaxDamage - num;
		blockShapeModelEntity.UpdateDamageState(blockEntity.blockValue, blockEntity.blockValue, blockEntity, false);
		this.UpdateDoorColor((int)blockEntity.blockValue.meta2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDoorColor(int _colorIdx)
	{
		this.chunk.GetBlockEntity(this.blockPos).blockValue.meta2 = (byte)_colorIdx;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetDoorDamage()
	{
		BlockEntityData blockEntity = this.chunk.GetBlockEntity(this.blockPos);
		Block block = blockEntity.blockValue.Block;
		BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
		if (blockShapeModelEntity == null)
		{
			Log.Warning(string.Format("block {0} does not have shape field. Cannot change damage state.", block));
			return;
		}
		blockEntity.blockValue.damage = this.initialDamage;
		blockShapeModelEntity.UpdateDamageState(blockEntity.blockValue, blockEntity.blockValue, blockEntity, false);
		this.UpdateDoorColor((int)blockEntity.blockValue.meta2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenClose_OnPressed(XUiController _sender, int _mouseButton)
	{
		BlockEntityData blockEntity = this.chunk.GetBlockEntity(this.blockPos);
		blockEntity.blockValue.Block.OnBlockActivated("close", this.world, 0, this.blockPos, blockEntity.blockValue, this.world.GetPrimaryPlayer());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.bAcceptChanges = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.bAcceptChanges)
		{
			BlockEntityData blockEntity = this.chunk.GetBlockEntity(this.blockPos);
			this.world.SetBlockRPC(this.blockPos, blockEntity.blockValue);
			return;
		}
		this.ResetDoorDamage();
		this.UpdateDoorColor((int)this.initialColorIdx);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDowngrade;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpgrade;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxColorPresetList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOpenClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte initialColorIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int initialDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAcceptChanges;
}
