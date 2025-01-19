using System;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionConnectPower : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionConnectPower.ConnectPowerData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("WireOffset"))
		{
			this.wireOffset = StringParsers.ParseVector3(_props.Values["WireOffset"], 0, -1);
		}
		if (_props.Values.ContainsKey("MaxWireLength"))
		{
			this.maxWireLength = StringParsers.ParseSInt32(_props.Values["MaxWireLength"], 0, -1, NumberStyles.Integer);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		ItemActionConnectPower.ConnectPowerData connectPowerData = (ItemActionConnectPower.ConnectPowerData)_data;
		connectPowerData.HasStartPoint = false;
		if (connectPowerData.wireNode != null)
		{
			WireManager.Instance.RemoveActiveWire(connectPowerData.wireNode);
			UnityEngine.Object.Destroy(connectPowerData.wireNode.gameObject);
			connectPowerData.wireNode = null;
		}
		if (connectPowerData.invData.world.GetTileEntity(0, connectPowerData.startPoint) is TileEntityPowered)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, connectPowerData.invData.holdingEntity.entityId), false, -1, -1, -1, null, 192);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, connectPowerData.invData.holdingEntity.entityId), false);
			}
		}
		if (!(_data.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		((ItemActionConnectPower.ConnectPowerData)_data).playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null, true);
		WireManager.Instance.ToggleAllWirePulse(false);
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		if (!(_data.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		((ItemActionConnectPower.ConnectPowerData)_data).playerUI = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
		WireManager.Instance.ToggleAllWirePulse(true);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		_actionData.lastUseTime = Time.time;
		((ItemActionConnectPower.ConnectPowerData)_actionData).StartLink = true;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionConnectPower.ConnectPowerData connectPowerData = (ItemActionConnectPower.ConnectPowerData)_actionData;
		return connectPowerData.StartLink && Time.time - connectPowerData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[connectPowerData.invData.item.HoldType.Value].RayCast;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionConnectPower.ConnectPowerData connectPowerData = (ItemActionConnectPower.ConnectPowerData)_actionData;
		Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
		bool flag = true;
		if (connectPowerData.invData.holdingEntity is EntityPlayerLocal && connectPowerData.playerUI == null)
		{
			connectPowerData.playerUI = LocalPlayerUI.GetUIForPlayer(connectPowerData.invData.holdingEntity as EntityPlayerLocal);
		}
		if (connectPowerData.playerUI != null && !connectPowerData.invData.world.CanPlaceBlockAt(blockPos, connectPowerData.invData.world.gameManager.GetPersistentLocalPlayer(), false))
		{
			connectPowerData.isFriendly = false;
			connectPowerData.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null, true);
			return;
		}
		connectPowerData.isFriendly = true;
		if (_actionData.invData.hitInfo.bHitValid)
		{
			int num = (int)(Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance);
			if (_actionData.invData.hitInfo.hit.distanceSq <= (float)num)
			{
				BlockValue block = _actionData.invData.world.GetBlock(blockPos);
				BlockPowered blockPowered = block.Block as BlockPowered;
				if (blockPowered != null)
				{
					if (connectPowerData.playerUI != null)
					{
						Color value = Color.grey;
						int num2 = blockPowered.RequiredPower;
						if (blockPowered.isMultiBlock && block.ischild)
						{
							connectPowerData.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null, true);
							return;
						}
						Vector3i vector3i = blockPos;
						ChunkCluster chunkCluster = _actionData.invData.world.ChunkClusters[_actionData.invData.hitInfo.hit.clrIdx];
						if (chunkCluster != null)
						{
							Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(vector3i.x), vector3i.y, World.toChunkXZ(vector3i.z));
							if (chunk != null)
							{
								TileEntityPowered tileEntityPowered = chunk.GetTileEntity(World.toBlock(vector3i)) as TileEntityPowered;
								if (tileEntityPowered != null)
								{
									value = (tileEntityPowered.IsPowered ? Color.yellow : Color.grey);
									num2 = tileEntityPowered.PowerUsed;
								}
								else
								{
									value = Color.grey;
								}
							}
						}
						connectPowerData.playerUI.nguiWindowManager.SetLabel(EnumNGUIWindow.PowerInfo, string.Format("{0}W", num2), new Color?(value), true);
					}
					flag = false;
				}
			}
		}
		if (flag && connectPowerData.playerUI != null)
		{
			connectPowerData.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null, true);
		}
		if (connectPowerData.HasStartPoint)
		{
			if (connectPowerData.wireNode == null)
			{
				return;
			}
			float num3 = Vector3.Distance(connectPowerData.startPoint.ToVector3(), _actionData.invData.holdingEntity.position);
			if (num3 < (float)(this.maxWireLength - 5))
			{
				connectPowerData.inRange = true;
				connectPowerData.wireNode.wireColor = new Color(0f, 0f, 0f, 0f);
			}
			if (num3 > (float)(this.maxWireLength - 5))
			{
				connectPowerData.inRange = false;
				connectPowerData.wireNode.wireColor = Color.red;
			}
			if (num3 > (float)this.maxWireLength)
			{
				connectPowerData.HasStartPoint = false;
				if (connectPowerData.wireNode != null)
				{
					WireManager.Instance.RemoveActiveWire(connectPowerData.wireNode);
					UnityEngine.Object.Destroy(connectPowerData.wireNode.gameObject);
					connectPowerData.wireNode = null;
				}
				Chunk chunk2 = connectPowerData.invData.world.GetChunkFromWorldPos(connectPowerData.startPoint) as Chunk;
				if (chunk2 == null)
				{
					return;
				}
				if (connectPowerData.invData.world.GetTileEntity(chunk2.ClrIdx, connectPowerData.startPoint) is TileEntityPowered)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId), false, -1, -1, -1, null, 192);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId), false);
					}
				}
				_actionData.invData.holdingEntity.RightArmAnimationUse = true;
				connectPowerData.invData.holdingEntity.PlayOneShot("ui_denied", false, false, false);
			}
		}
		if (!connectPowerData.StartLink || Time.time - connectPowerData.lastUseTime < AnimationDelayData.AnimationDelay[connectPowerData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		connectPowerData.StartLink = false;
		ItemActionConnectPower.ConnectPowerData connectPowerData2 = (ItemActionConnectPower.ConnectPowerData)_actionData;
		ItemInventoryData invData = _actionData.invData;
		Vector3i lastBlockPos = invData.hitInfo.lastBlockPos;
		if (!invData.hitInfo.bHitValid || invData.hitInfo.tag.StartsWith("E_"))
		{
			connectPowerData2.HasStartPoint = false;
			return;
		}
		if (connectPowerData.invData.itemValue.MaxUseTimes > 0 && connectPowerData.invData.itemValue.UseTimes >= (float)connectPowerData.invData.itemValue.MaxUseTimes)
		{
			EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
			if (this.item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(this.item.Properties.Values[ItemClass.PropSoundJammed], -1, 0f, false, false);
			}
			GameManager.ShowTooltip(player, "ttItemNeedsRepair", false);
			return;
		}
		if (connectPowerData.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, invData.holdingEntity, null, (_actionData.invData.itemValue.ItemClass != null) ? _actionData.invData.itemValue.ItemClass.ItemTags : FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
			base.HandleItemBreak(_actionData);
		}
		if (connectPowerData2.HasStartPoint)
		{
			if (connectPowerData2.startPoint == invData.hitInfo.hit.blockPos || !connectPowerData2.inRange)
			{
				return;
			}
			if (Vector3.Distance(connectPowerData.startPoint.ToVector3(), invData.hitInfo.hit.blockPos.ToVector3()) > (float)this.maxWireLength)
			{
				return;
			}
			TileEntityPowered poweredBlock = this.GetPoweredBlock(invData);
			if (poweredBlock != null)
			{
				TileEntityPowered poweredBlock2 = this.GetPoweredBlock(connectPowerData2.startPoint);
				if (poweredBlock2 != null)
				{
					if (!poweredBlock.CanHaveParent(poweredBlock2))
					{
						GameManager.ShowTooltip(_actionData.invData.holdingEntity as EntityPlayerLocal, Localization.Get("ttCantHaveParent", false), false);
						invData.holdingEntity.PlayOneShot("ui_denied", false, false, false);
						return;
					}
					if (poweredBlock2.ChildCount > 8)
					{
						GameManager.ShowTooltip(_actionData.invData.holdingEntity as EntityPlayerLocal, Localization.Get("ttWireLimit", false), false);
						invData.holdingEntity.PlayOneShot("ui_denied", false, false, false);
						return;
					}
					poweredBlock.SetParentWithWireTool(poweredBlock2, invData.holdingEntity.entityId);
					_actionData.invData.holdingEntity.RightArmAnimationUse = true;
					connectPowerData2.HasStartPoint = false;
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId), false, -1, -1, -1, null, 192);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId), false);
					}
					EntityAlive holdingEntity = _actionData.invData.holdingEntity;
					string name = "wire_tool_" + (poweredBlock2.IsPowered ? "sparks" : "dust");
					Transform handTransform = this.GetHandTransform(holdingEntity);
					GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(name, handTransform.position + Origin.position, handTransform.rotation, holdingEntity.GetLightBrightness(), Color.white), invData.holdingEntity.entityId, false, false);
					if (connectPowerData.wireNode != null)
					{
						WireManager.Instance.RemoveActiveWire(connectPowerData.wireNode);
						UnityEngine.Object.Destroy(connectPowerData.wireNode.gameObject);
						connectPowerData.wireNode = null;
					}
					this.DecreaseDurability(connectPowerData);
					return;
				}
			}
		}
		else
		{
			TileEntityPowered poweredBlock3 = this.GetPoweredBlock(invData);
			if (poweredBlock3 != null)
			{
				_actionData.invData.holdingEntity.RightArmAnimationUse = true;
				connectPowerData2.startPoint = invData.hitInfo.hit.blockPos;
				connectPowerData2.HasStartPoint = true;
				EntityAlive holdingEntity2 = _actionData.invData.holdingEntity;
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.AddWire, connectPowerData2.startPoint, holdingEntity2.entityId), false, -1, -1, -1, null, 192);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.AddWire, connectPowerData2.startPoint, holdingEntity2.entityId), false);
				}
				Manager.BroadcastPlay(poweredBlock3.ToWorldPos().ToVector3(), poweredBlock3.IsPowered ? "wire_live_connect" : "wire_dead_connect", 0f);
				Transform handTransform2 = this.GetHandTransform(holdingEntity2);
				if (handTransform2 != null)
				{
					Transform transform = handTransform2.FindInChilds("wire_mesh", false);
					if (transform == null)
					{
						return;
					}
					if (connectPowerData2.wireNode != null)
					{
						WireManager.Instance.RemoveActiveWire(connectPowerData2.wireNode);
						UnityEngine.Object.Destroy(connectPowerData2.wireNode.gameObject);
						connectPowerData2.wireNode = null;
					}
					WireNode component = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/WireNode"))).GetComponent<WireNode>();
					component.LocalPosition = invData.hitInfo.hit.blockPos.ToVector3() - Origin.position;
					component.localOffset = poweredBlock3.GetWireOffset();
					WireNode wireNode = component;
					wireNode.localOffset.x = wireNode.localOffset.x + 0.5f;
					WireNode wireNode2 = component;
					wireNode2.localOffset.y = wireNode2.localOffset.y + 0.5f;
					WireNode wireNode3 = component;
					wireNode3.localOffset.z = wireNode3.localOffset.z + 0.5f;
					component.Source = transform.gameObject;
					component.sourceOffset = this.wireOffset;
					component.TogglePulse(false);
					component.SetPulseSpeed(360f);
					connectPowerData2.wireNode = component;
					WireManager.Instance.AddActiveWire(component);
					string name2 = "wire_tool_" + (poweredBlock3.IsPowered ? "sparks" : "dust");
					GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(name2, handTransform2.position + Origin.position, handTransform2.rotation, holdingEntity2.GetLightBrightness(), Color.white), invData.holdingEntity.entityId, false, false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetHandTransform(EntityAlive holdingEntity)
	{
		Transform transform = holdingEntity.RootTransform.Find("Graphics").FindInChilds(holdingEntity.GetRightHandTransformName(), true);
		Transform result;
		if (transform != null && transform.childCount > 0)
		{
			result = transform;
		}
		else
		{
			Transform transform2 = holdingEntity.RootTransform.Find("Camera").FindInChilds(holdingEntity.GetRightHandTransformName(), true);
			if (transform2 != null && transform2.childCount > 0)
			{
				result = transform2;
			}
			else
			{
				result = holdingEntity.emodel.GetRightHandTransform();
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void CheckForWireRemoveNeeded(EntityAlive _player, Vector3i _blockPos)
	{
		ItemActionConnectPower.ConnectPowerData connectPowerData = (ItemActionConnectPower.ConnectPowerData)_player.inventory.holdingItemData.actionData[1];
		if (connectPowerData.HasStartPoint && connectPowerData.startPoint == _blockPos)
		{
			this.DisconnectWire(connectPowerData);
		}
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (_actionData.invData.hitInfo.bHitValid && (_actionData as ItemActionConnectPower.ConnectPowerData).isFriendly)
		{
			int num = (int)(Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance);
			if (_actionData.invData.hitInfo.hit.distanceSq <= (float)num)
			{
				Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
				Block block = _actionData.invData.world.GetBlock(blockPos).Block;
				if (block is BlockPowered)
				{
					return ItemClass.EnumCrosshairType.PowerItem;
				}
				if (block is BlockPowerSource)
				{
					return ItemClass.EnumCrosshairType.PowerSource;
				}
			}
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered GetPoweredBlock(ItemInventoryData data)
	{
		Block block = data.world.GetBlock(data.hitInfo.hit.blockPos).Block;
		if (!(block is BlockPowered) && !(block is BlockPowerSource))
		{
			return null;
		}
		Vector3i blockPos = data.hitInfo.hit.blockPos;
		ChunkCluster chunkCluster = data.world.ChunkClusters[data.hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
		if (chunk == null)
		{
			return null;
		}
		TileEntity tileEntity = chunk.GetTileEntity(World.toBlock(blockPos));
		if (tileEntity == null)
		{
			if (block is BlockPowered)
			{
				tileEntity = (block as BlockPowered).CreateTileEntity(chunk);
			}
			else if (block is BlockPowerSource)
			{
				tileEntity = (block as BlockPowerSource).CreateTileEntity(chunk);
			}
			tileEntity.localChunkPos = World.toBlock(blockPos);
			BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
			if (blockEntity != null)
			{
				((TileEntityPowered)tileEntity).BlockTransform = blockEntity.transform;
			}
			((TileEntityPowered)tileEntity).InitializePowerData();
			chunk.AddTileEntity(tileEntity);
		}
		return tileEntity as TileEntityPowered;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered GetPoweredBlock(Vector3i tileEntityPos)
	{
		World world = GameManager.Instance.World;
		Block block = world.GetBlock(tileEntityPos).Block;
		if (!(block is BlockPowered) && !(block is BlockPowerSource))
		{
			return null;
		}
		Chunk chunk = world.GetChunkFromWorldPos(tileEntityPos.x, tileEntityPos.y, tileEntityPos.z) as Chunk;
		if (chunk == null)
		{
			return null;
		}
		TileEntity tileEntity = chunk.GetTileEntity(World.toBlock(tileEntityPos));
		if (tileEntity == null)
		{
			if (block is BlockPowered)
			{
				tileEntity = (block as BlockPowered).CreateTileEntity(chunk);
			}
			else if (block is BlockPowerSource)
			{
				tileEntity = (block as BlockPowerSource).CreateTileEntity(chunk);
			}
			tileEntity.localChunkPos = World.toBlock(tileEntityPos);
			BlockEntityData blockEntity = chunk.GetBlockEntity(tileEntityPos);
			if (blockEntity != null)
			{
				((TileEntityPowered)tileEntity).BlockTransform = blockEntity.transform;
			}
			((TileEntityPowered)tileEntity).InitializePowerData();
			chunk.AddTileEntity(tileEntity);
		}
		return tileEntity as TileEntityPowered;
	}

	public bool DisconnectWire(ItemActionConnectPower.ConnectPowerData _actionData)
	{
		if (!_actionData.HasStartPoint)
		{
			return false;
		}
		_actionData.HasStartPoint = false;
		if (_actionData.wireNode != null)
		{
			WireManager.Instance.RemoveActiveWire(_actionData.wireNode);
			UnityEngine.Object.Destroy(_actionData.wireNode.gameObject);
			_actionData.wireNode = null;
		}
		Chunk chunk = _actionData.invData.world.GetChunkFromWorldPos(_actionData.startPoint) as Chunk;
		if (chunk == null)
		{
			return false;
		}
		TileEntityPowered tileEntityPowered = _actionData.invData.world.GetTileEntity(chunk.ClrIdx, _actionData.startPoint) as TileEntityPowered;
		if (tileEntityPowered != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId), false, -1, -1, -1, null, 192);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId), false);
			}
			Manager.BroadcastPlay(tileEntityPowered.ToWorldPos().ToVector3(), tileEntityPowered.IsPowered ? "wire_live_break" : "wire_dead_break", 0f);
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			string name = "wire_tool_" + (tileEntityPowered.IsPowered ? "sparks" : "dust");
			Transform handTransform = this.GetHandTransform(holdingEntity);
			GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(name, handTransform.position + Origin.position, handTransform.rotation, holdingEntity.GetLightBrightness(), Color.white), holdingEntity.entityId, false, false);
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
		_actionData.invData.holdingEntity.PlayOneShot("ui_denied", false, false, false);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DecreaseDurability(ItemActionConnectPower.ConnectPowerData _actionData)
	{
		if (_actionData.invData.itemValue.MaxUseTimes > 0)
		{
			if (_actionData.invData.itemValue.UseTimes + 1f < (float)_actionData.invData.itemValue.MaxUseTimes)
			{
				_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, _actionData.invData.holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
				base.HandleItemBreak(_actionData);
				return;
			}
			_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 wireOffset = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxWireLength = 15;

	public class ConnectPowerData : ItemActionAttackData
	{
		public ConnectPowerData(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public bool StartLink;

		public bool HasStartPoint;

		public LocalPlayerUI playerUI;

		public Vector3i startPoint;

		public bool inRange;

		public bool isFriendly;

		public WireNode wireNode;
	}
}
