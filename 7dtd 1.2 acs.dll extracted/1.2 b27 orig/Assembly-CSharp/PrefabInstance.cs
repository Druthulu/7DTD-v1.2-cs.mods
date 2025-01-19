using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstance
{
	public PrefabInstance(int _id, PathAbstractions.AbstractedLocation _location, Vector3i _position, byte _rotation, Prefab _bad, int _standaloneBlockSize)
	{
		this.id = _id;
		if (_bad != null)
		{
			_bad.location = _location;
			this.boundingBoxSize = _bad.size;
		}
		this.name = _location.Name + "." + _id.ToString();
		this.location = _location;
		this.boundingBoxPosition = _position;
		this.lastCopiedPrefabPosition = Vector3i.zero;
		this.bPrefabCopiedIntoWorld = false;
		this.lastCopiedRotation = _rotation;
		this.rotation = _rotation;
		this.prefab = _bad;
		this.standaloneBlockSize = _standaloneBlockSize;
	}

	public Bounds GetAABB()
	{
		return BoundsUtils.BoundsForMinMax((float)this.boundingBoxPosition.x, (float)this.boundingBoxPosition.y, (float)this.boundingBoxPosition.z, (float)(this.boundingBoxPosition.x + this.boundingBoxSize.x), (float)(this.boundingBoxPosition.y + this.boundingBoxSize.y), (float)(this.boundingBoxPosition.z + this.boundingBoxSize.z));
	}

	public Vector2 GetCenterXZ()
	{
		return new Vector2((float)this.boundingBoxPosition.x + (float)this.boundingBoxSize.x * 0.5f, (float)this.boundingBoxPosition.z + (float)this.boundingBoxSize.z * 0.5f);
	}

	public bool IsBBInSyncWithPrefab()
	{
		return this.bPrefabCopiedIntoWorld && this.lastCopiedPrefabPosition.Equals(this.boundingBoxPosition) && this.prefab.size.Equals(this.boundingBoxSize) && this.lastCopiedRotation == this.rotation;
	}

	public void CopyIntoWorld(World _world, bool _CopyEntities, bool _bOverwriteExistingBlocks, FastTags<TagGroup.Global> _tags)
	{
		if (this.lastCopiedRotation != this.rotation)
		{
			if (this.lastCopiedRotation < this.rotation)
			{
				int rotCount = (int)(this.rotation - this.lastCopiedRotation);
				this.prefab.RotateY(false, rotCount);
			}
			else
			{
				int rotCount2 = (int)(this.lastCopiedRotation - this.rotation);
				this.prefab.RotateY(true, rotCount2);
			}
			this.lastCopiedRotation = this.rotation;
			this.UpdateBoundingBoxPosAndScale(this.boundingBoxPosition, this.prefab.size, true);
		}
		this.prefab.CopyIntoLocal(_world.ChunkClusters[0], this.boundingBoxPosition, _bOverwriteExistingBlocks, true, _tags);
		if (_CopyEntities)
		{
			bool bSpawnEnemies = _world.IsEditor() || GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
			this.entityInstanceIds.Clear();
			this.prefab.CopyEntitiesIntoWorld(_world, this.boundingBoxPosition, this.entityInstanceIds, bSpawnEnemies);
		}
		this.lastCopiedPrefabPosition = this.boundingBoxPosition;
		this.bPrefabCopiedIntoWorld = true;
	}

	public static void RefreshSwitchesInContainingPoi(Quest q)
	{
		Vector3 position;
		if (!GameManager.Instance.World.IsEditor() && q.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
		{
			World world = GameManager.Instance.World;
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(position, true);
			if (prefabAtPosition != null)
			{
				for (int i = 0; i < prefabAtPosition.boundingBoxSize.z; i++)
				{
					for (int j = 0; j < prefabAtPosition.boundingBoxSize.x; j++)
					{
						for (int k = 0; k < prefabAtPosition.boundingBoxSize.y; k++)
						{
							Vector3i vector3i = World.worldToBlockPos(prefabAtPosition.boundingBoxPosition + new Vector3i(j, k, i));
							BlockValue block = world.GetBlock(vector3i);
							if (block.Block is BlockActivateSwitch || block.Block is BlockActivateSingle)
							{
								block.Block.Refresh(world, null, 0, vector3i, block);
							}
						}
					}
				}
			}
		}
	}

	public static void RefreshTriggersInContainingPoi(Vector3 v)
	{
		if (!GameManager.Instance.World.IsEditor())
		{
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(v, true);
			if (prefabAtPosition != null)
			{
				GameManager.Instance.World.triggerManager.RefreshTriggers(prefabAtPosition, prefabAtPosition.LastRefreshType);
			}
		}
	}

	public void CleanFromWorld(World _world, bool _bRemoveEntities)
	{
		if (!this.bPrefabCopiedIntoWorld)
		{
			return;
		}
		BlockTools.ClearRPC(_world, 0, this.lastCopiedPrefabPosition, this.prefab.size.x, this.prefab.size.y, this.prefab.size.z, true);
		if (_bRemoveEntities)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < this.entityInstanceIds.Count; i++)
			{
				int num = this.entityInstanceIds[i];
				Entity entity = _world.GetEntity(num);
				if (entity != null && !entity.IsDead())
				{
					_world.RemoveEntity(num, EnumRemoveEntityReason.Unloaded);
				}
				else
				{
					list.Add(num);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				int item = list[j];
				this.entityInstanceIds.Remove(item);
			}
		}
		this.lastCopiedPrefabPosition = Vector3i.zero;
		this.bPrefabCopiedIntoWorld = false;
	}

	public void AddSleeperVolume(SleeperVolume _volume)
	{
		if (!this.sleeperVolumes.Contains(_volume))
		{
			this.sleeperVolumes.Add(_volume);
		}
	}

	public void AddTriggerVolume(TriggerVolume _volume)
	{
		if (!this.triggerVolumes.Contains(_volume))
		{
			this.triggerVolumes.Add(_volume);
		}
	}

	public void AddWallVolume(WallVolume _volume)
	{
		if (!this.wallVolumes.Contains(_volume))
		{
			this.wallVolumes.Add(_volume);
		}
	}

	public void ResizeBoundingBox(Vector3i _deltaVec)
	{
		Vector3i vector3i = this.boundingBoxSize + _deltaVec;
		if (vector3i.x <= 1)
		{
			vector3i.x = 1;
		}
		if (vector3i.y <= 1)
		{
			vector3i.y = 1;
		}
		if (vector3i.z <= 1)
		{
			vector3i.z = 1;
		}
		this.UpdateBoundingBoxPosAndScale(this.boundingBoxPosition, vector3i, true);
	}

	public void MoveBoundingBox(Vector3i _deltaVec)
	{
		this.UpdateBoundingBoxPosAndScale(this.boundingBoxPosition + _deltaVec, this.boundingBoxSize, true);
	}

	public void SetBoundingBoxPosition(Vector3i _position)
	{
		this.UpdateBoundingBoxPosAndScale(_position, this.boundingBoxSize, true);
	}

	public void SetBoundingBoxSize(World _world, Vector3i _size)
	{
		this.UpdateBoundingBoxPosAndScale(this.boundingBoxPosition, _size, true);
	}

	public void CreateBoundingBox(bool _alsoCreateOtherBoxes = true)
	{
		SelectionBox selectionBox = SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").AddBox(this.name, this.boundingBoxPosition, this.boundingBoxSize, true, true);
		selectionBox.facingDirection = (float)(-(float)((this.prefab.rotationToFaceNorth + (int)this.rotation) % 4) * 90);
		selectionBox.UserData = this;
		selectionBox.SetCaption(this.prefab.PrefabName);
		if (_alsoCreateOtherBoxes)
		{
			if (this.prefab.bTraderArea)
			{
				for (int i = 0; i < this.prefab.TeleportVolumes.Count; i++)
				{
					Prefab.PrefabTeleportVolume prefabTeleportVolume = this.prefab.TeleportVolumes[i];
					this.prefab.AddTeleportVolumeSelectionBox(prefabTeleportVolume, this.name + "_" + i.ToString(), this.boundingBoxPosition + prefabTeleportVolume.startPos);
				}
			}
			if (this.prefab.bSleeperVolumes)
			{
				for (int j = 0; j < this.prefab.SleeperVolumes.Count; j++)
				{
					Prefab.PrefabSleeperVolume prefabSleeperVolume = this.prefab.SleeperVolumes[j];
					this.prefab.AddSleeperVolumeSelectionBox(prefabSleeperVolume, this.name + "_" + j.ToString(), this.boundingBoxPosition + prefabSleeperVolume.startPos);
				}
			}
			if (this.prefab.bInfoVolumes)
			{
				for (int k = 0; k < this.prefab.InfoVolumes.Count; k++)
				{
					Prefab.PrefabInfoVolume prefabInfoVolume = this.prefab.InfoVolumes[k];
					this.prefab.AddInfoVolumeSelectionBox(prefabInfoVolume, this.name + "_" + k.ToString(), this.boundingBoxPosition + prefabInfoVolume.startPos);
				}
			}
			if (this.prefab.bWallVolumes)
			{
				for (int l = 0; l < this.prefab.WallVolumes.Count; l++)
				{
					Prefab.PrefabWallVolume prefabWallVolume = this.prefab.WallVolumes[l];
					this.prefab.AddWallVolumeSelectionBox(prefabWallVolume, this.name + "_" + l.ToString(), this.boundingBoxPosition + prefabWallVolume.startPos);
				}
			}
			if (this.prefab.bTriggerVolumes)
			{
				for (int m = 0; m < this.prefab.TriggerVolumes.Count; m++)
				{
					Prefab.PrefabTriggerVolume prefabTriggerVolume = this.prefab.TriggerVolumes[m];
					this.prefab.AddTriggerVolumeSelectionBox(prefabTriggerVolume, this.name + "_" + m.ToString(), this.boundingBoxPosition + prefabTriggerVolume.startPos);
				}
			}
			if (this.prefab.bPOIMarkers)
			{
				for (int n = 0; n < this.prefab.POIMarkers.Count; n++)
				{
					Prefab.Marker marker = this.prefab.POIMarkers[n];
					this.prefab.AddPOIMarker(marker.GroupName + "_" + n.ToString(), this.boundingBoxPosition, marker.Start, marker.Size, marker.GroupName, marker.Tags, marker.MarkerType, n, false);
				}
			}
		}
	}

	public void UpdateBoundingBoxPosAndScale(Vector3i _pos, Vector3i _size, bool _moveSleepers = true)
	{
		if (_moveSleepers)
		{
			this.prefab.MoveVolumes(this.boundingBoxPosition - _pos);
		}
		this.boundingBoxPosition = _pos;
		this.boundingBoxSize = _size;
		SelectionBox box = SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").GetBox(this.name);
		box.SetPositionAndSize(this.boundingBoxPosition, this.boundingBoxSize);
		box.facingDirection = (float)((this.prefab.rotationToFaceNorth + (int)this.rotation) % 4 * 90);
		if (box.facingDirection == 90f)
		{
			box.facingDirection = 270f;
		}
		else if (box.facingDirection == 270f)
		{
			box.facingDirection = 90f;
		}
		if (this.prefab.bSleeperVolumes)
		{
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("SleeperVolume");
			bool visible = category.IsVisible();
			for (int i = 0; i < this.prefab.SleeperVolumes.Count; i++)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = this.prefab.SleeperVolumes[i];
				if (prefabSleeperVolume.used)
				{
					string text = this.name + "_" + i.ToString();
					SelectionBox box2 = category.GetBox(text);
					if (box2 != null)
					{
						box2.SetPositionAndSize(this.boundingBoxPosition + prefabSleeperVolume.startPos, prefabSleeperVolume.size);
						box2.SetVisible(visible);
					}
				}
			}
		}
		if (this.prefab.bTraderArea)
		{
			SelectionCategory category2 = SelectionBoxManager.Instance.GetCategory("TraderTeleport");
			for (int j = 0; j < this.prefab.TeleportVolumes.Count; j++)
			{
				if (this.prefab.TeleportVolumes[j].used)
				{
					SelectionBox box3 = category2.GetBox(this.name + "_" + j.ToString());
					if (box3 != null)
					{
						box3.SetPositionAndSize(this.boundingBoxPosition + this.prefab.TeleportVolumes[j].startPos, this.prefab.TeleportVolumes[j].size);
						box3.SetVisible(category2.IsVisible());
					}
				}
			}
		}
		if (this.prefab.bTriggerVolumes)
		{
			SelectionCategory category3 = SelectionBoxManager.Instance.GetCategory("TriggerVolume");
			for (int k = 0; k < this.prefab.TriggerVolumes.Count; k++)
			{
				if (this.prefab.TriggerVolumes[k].used)
				{
					SelectionBox box4 = category3.GetBox(this.name + "_" + k.ToString());
					if (box4 != null)
					{
						box4.SetPositionAndSize(this.boundingBoxPosition + this.prefab.TriggerVolumes[k].startPos, this.prefab.TriggerVolumes[k].size);
						box4.SetVisible(category3.IsVisible());
					}
				}
			}
		}
		if (this.prefab.bInfoVolumes)
		{
			SelectionCategory category4 = SelectionBoxManager.Instance.GetCategory("InfoVolume");
			for (int l = 0; l < this.prefab.InfoVolumes.Count; l++)
			{
				if (this.prefab.InfoVolumes[l].used)
				{
					SelectionBox box5 = category4.GetBox(this.name + "_" + l.ToString());
					if (box5 != null)
					{
						box5.SetPositionAndSize(this.boundingBoxPosition + this.prefab.InfoVolumes[l].startPos, this.prefab.InfoVolumes[l].size);
						box5.SetVisible(category4.IsVisible());
					}
				}
			}
		}
		if (this.prefab.bWallVolumes)
		{
			SelectionCategory category5 = SelectionBoxManager.Instance.GetCategory("WallVolume");
			for (int m = 0; m < this.prefab.WallVolumes.Count; m++)
			{
				SelectionBox box6 = category5.GetBox(this.name + "_" + m.ToString());
				if (box6 != null)
				{
					box6.SetPositionAndSize(this.boundingBoxPosition + this.prefab.WallVolumes[m].startPos, this.prefab.WallVolumes[m].size);
					box6.SetVisible(category5.IsVisible());
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				dynamicPrefabDecorator.CallPrefabChangedEvent(this);
			}
		}
	}

	public SelectionBox GetBox()
	{
		return SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").GetBox(this.name);
	}

	public void RotateAroundY()
	{
		this.rotation = (this.rotation + 1) % 4;
		MathUtils.Swap(ref this.boundingBoxSize.x, ref this.boundingBoxSize.z);
		this.UpdateBoundingBoxPosAndScale(this.boundingBoxPosition, this.boundingBoxSize, true);
	}

	public void SetRotation(byte _rotation)
	{
		while (this.rotation != (_rotation & 3))
		{
			this.rotation = (this.rotation + 1 & 3);
			this.prefab.RotateY(false, 1);
			this.boundingBoxSize = this.prefab.size;
		}
	}

	public bool Overlaps(Chunk _chunk)
	{
		Vector3i worldPosIMax = _chunk.worldPosIMax;
		if (worldPosIMax.x >= this.boundingBoxPosition.x && worldPosIMax.y >= this.boundingBoxPosition.y && worldPosIMax.z >= this.boundingBoxPosition.z)
		{
			Vector3i worldPosIMin = _chunk.worldPosIMin;
			Vector3i vector3i = this.boundingBoxPosition + this.boundingBoxSize;
			return worldPosIMin.x < vector3i.x && worldPosIMin.y < vector3i.y && worldPosIMin.z < vector3i.z;
		}
		return false;
	}

	public bool Overlaps(Vector3 _pos, float _expandBounds = 0f)
	{
		Bounds aabb = this.GetAABB();
		aabb.Expand(_expandBounds);
		Vector3 max = aabb.max;
		Vector3 min = aabb.min;
		return _pos.x <= max.x && _pos.x >= min.x && _pos.y <= max.y && _pos.y >= min.y && _pos.z <= max.z && _pos.z >= min.z;
	}

	public bool IsWithinInfoArea(Vector3 _pos)
	{
		if (this.prefab.InfoVolumes.Count == 0)
		{
			return true;
		}
		foreach (Prefab.PrefabInfoVolume prefabInfoVolume in this.prefab.InfoVolumes)
		{
			Vector3i vector3i = this.boundingBoxPosition + prefabInfoVolume.startPos;
			if ((float)(vector3i.x - 1) <= _pos.x && _pos.x <= (float)(vector3i.x + prefabInfoVolume.size.x + 1) && (float)(vector3i.y - 1) <= _pos.y && _pos.y <= (float)(vector3i.y + prefabInfoVolume.size.y + 1) && (float)(vector3i.z - 1) <= _pos.z && _pos.z <= (float)(vector3i.z + prefabInfoVolume.size.z + 1))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyIntoChunk(World _world, Chunk _chunk, bool _bForceOverwriteBlocks = false)
	{
		this.prefab.CopyBlocksIntoChunkNoEntities(_world, _chunk, this.boundingBoxPosition, _bForceOverwriteBlocks);
		bool bSpawnEnemies = _world.IsEditor() || GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
		this.prefab.CopyEntitiesIntoChunkStub(_chunk, this.boundingBoxPosition, this.entityInstanceIds, bSpawnEnemies);
		this.lastCopiedPrefabPosition = this.boundingBoxPosition;
		this.bPrefabCopiedIntoWorld = true;
	}

	public HashSetLong GetOccupiedChunks()
	{
		if (this.occupiedChunks != null)
		{
			return this.occupiedChunks;
		}
		this.occupiedChunks = new HashSetLong();
		int num = World.toChunkXZ(this.boundingBoxPosition.x);
		int num2 = World.toChunkXZ(this.boundingBoxPosition.x + this.boundingBoxSize.x);
		int num3 = World.toChunkXZ(this.boundingBoxPosition.z);
		int num4 = World.toChunkXZ(this.boundingBoxPosition.z + this.boundingBoxSize.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				this.occupiedChunks.Add(WorldChunkCache.MakeChunkKey(i, j));
			}
		}
		return this.occupiedChunks;
	}

	public IEnumerator ResetTerrain(World _world)
	{
		HashSetLong chunks = this.GetOccupiedChunks();
		yield return GameManager.Instance.ResetWindowsAndLocks(chunks);
		ChunkCluster chunkCluster = _world.ChunkClusters[0];
		foreach (long key in chunks)
		{
			Chunk chunkSync = chunkCluster.GetChunkSync(key);
			if (chunkSync != null)
			{
				chunkSync.ResetWaterDebugHandle();
				chunkSync.ResetWaterSimHandle();
			}
		}
		_world.RebuildTerrain(chunks, this.boundingBoxPosition, this.boundingBoxSize, true, false, false, true);
		yield break;
	}

	public void ResetBlocksAndRebuild(World _world, FastTags<TagGroup.Global> questTags)
	{
		this.LastRefreshType = questTags;
		ChunkCluster chunkCluster = _world.ChunkClusters[0];
		chunkCluster.ChunkPosNeedsRegeneration_DelayedStart();
		HashSetLong hashSetLong = this.GetOccupiedChunks();
		foreach (long key in hashSetLong)
		{
			Chunk chunkSync = chunkCluster.GetChunkSync(key);
			if (chunkSync != null)
			{
				chunkSync.StopStabilityCalculation = true;
				chunkSync.ResetWaterDebugHandle();
				chunkSync.ResetWaterSimHandle();
			}
		}
		this.CopyIntoWorld(_world, false, true, questTags);
		foreach (long key2 in hashSetLong)
		{
			Chunk chunkSync2 = chunkCluster.GetChunkSync(key2);
			if (chunkSync2 != null)
			{
				chunkSync2.NeedsDecoration = true;
				chunkSync2.NeedsLightDecoration = true;
				chunkSync2.NeedsLightCalculation = true;
			}
		}
		List<TileEntity> list = new List<TileEntity>(10);
		foreach (long key3 in hashSetLong)
		{
			Chunk chunkSync3 = chunkCluster.GetChunkSync(key3);
			if (chunkSync3 != null)
			{
				list.Clear();
				List<TileEntity> list2 = chunkSync3.GetTileEntities().list;
				for (int i = list2.Count - 1; i >= 0; i--)
				{
					if (!chunkSync3.GetBlock(list2[i].localChunkPos).Block.HasTileEntity)
					{
						list.Add(list2[i]);
					}
					else
					{
						Vector3i vector3i = list2[i].ToWorldPos();
						if (this.boundingBoxPosition.x <= vector3i.x && this.boundingBoxPosition.y <= vector3i.y && this.boundingBoxPosition.z <= vector3i.z && this.boundingBoxPosition.x + this.boundingBoxSize.x > vector3i.x && this.boundingBoxPosition.y + this.boundingBoxSize.y > vector3i.y && this.boundingBoxPosition.z + this.boundingBoxSize.z > vector3i.z)
						{
							list2[i].Reset(questTags);
						}
					}
				}
				foreach (TileEntity te in list)
				{
					chunkSync3.RemoveTileEntity(_world, te);
				}
			}
		}
		chunkCluster.ChunkPosNeedsRegeneration_DelayedStop();
		_world.m_ChunkManager.ResendChunksToClients(hashSetLong);
	}

	public GameUtils.EPlayerHomeType CheckForAnyPlayerHome(World _world)
	{
		return GameUtils.CheckForAnyPlayerHome(_world, this.boundingBoxPosition, this.boundingBoxPosition + this.boundingBoxSize);
	}

	public bool AddChunksToUncull(World _world, HashSetList<Chunk> _chunksToUncull)
	{
		bool result = false;
		foreach (long key in this.GetOccupiedChunks())
		{
			Chunk chunkSync = _world.ChunkCache.GetChunkSync(key);
			if (chunkSync != null && chunkSync.IsInternalBlocksCulled && !_chunksToUncull.hashSet.Contains(chunkSync))
			{
				_chunksToUncull.Add(chunkSync);
				result = true;
			}
		}
		return result;
	}

	public PathAbstractions.AbstractedLocation GetImposterLocation()
	{
		if (this.imposterLookupDone)
		{
			return this.imposterLocation;
		}
		Prefab prefab = this.prefab;
		string text = ((prefab != null) ? prefab.distantPOIOverride : null) ?? this.location.Name;
		this.imposterLocation = PathAbstractions.PrefabImpostersSearchPaths.GetLocation(text, null, null);
		this.imposterLookupDone = true;
		return this.imposterLocation;
	}

	public bool Contains(int _entityId)
	{
		return this.entityInstanceIds.Contains(_entityId);
	}

	public override bool Equals(object obj)
	{
		return obj is PrefabInstance && this.boundingBoxPosition == ((PrefabInstance)obj).boundingBoxPosition;
	}

	public override int GetHashCode()
	{
		return this.boundingBoxPosition.GetHashCode();
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			"[DynamicPrefabDecorator ",
			this.id.ToString(),
			" ",
			(this.prefab != null) ? this.prefab.PrefabName : string.Empty,
			"]"
		});
	}

	public void UpdateImposterView()
	{
		if (!GameManager.Instance.IsEditMode() || PrefabEditModeManager.Instance.IsActive())
		{
			return;
		}
		SelectionBox box = this.GetBox();
		if (box == null)
		{
			Log.Error("PrefabInstance has not SelectionBox! (UIV)");
			return;
		}
		Transform transform = box.transform.Find("PrefabImposter");
		if (transform == null)
		{
			ThreadManager.RunCoroutineSync(this.prefab.ToTransform(true, true, true, false, box.transform, "PrefabImposter", new Vector3((float)(-(float)this.boundingBoxSize.x) / 2f, 0.15f, (float)(-(float)this.boundingBoxSize.z) / 2f), DynamicPrefabDecorator.PrefabPreviewLimit));
			transform = box.transform.Find("PrefabImposter");
			this.imposterBaseRotation = this.lastCopiedRotation;
		}
		int num = MathUtils.Mod((int)(this.rotation - this.imposterBaseRotation), 4);
		Vector3 localEulerAngles = transform.localEulerAngles;
		localEulerAngles.y = -90f * (float)num;
		transform.localEulerAngles = localEulerAngles;
		Vector3 localPosition = transform.localPosition;
		localPosition.x = (float)this.boundingBoxSize.x / 2f * (float)((num % 3 == 0) ? -1 : 1);
		localPosition.z = (float)this.boundingBoxSize.z / 2f * (float)((num < 2) ? -1 : 1);
		transform.localPosition = localPosition;
		transform.gameObject.SetActive(!this.IsBBInSyncWithPrefab());
	}

	public void DestroyImposterView()
	{
		SelectionBox box = this.GetBox();
		if (box == null)
		{
			Log.Error("PrefabInstance has not SelectionBox! (DIV)");
			return;
		}
		Transform transform = box.transform.Find("PrefabImposter");
		if (transform != null)
		{
			UnityEngine.Object.DestroyImmediate(transform.gameObject);
		}
	}

	public Vector3i GetPositionRelativeToPoi(Vector3i _pos)
	{
		Vector3i vector3i = _pos - this.boundingBoxPosition;
		if ((this.rotation & 1) != 0)
		{
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		}
		Vector3i result;
		switch (this.rotation & 3)
		{
		case 0:
			result = vector3i;
			break;
		case 1:
			result = new Vector3i(vector3i.x, vector3i.y, this.boundingBoxSize.z - 1 - vector3i.z);
			break;
		case 2:
			result = new Vector3i(this.boundingBoxSize.x - 1 - vector3i.x, vector3i.y, this.boundingBoxSize.z - 1 - vector3i.z);
			break;
		case 3:
			result = new Vector3i(this.boundingBoxSize.x - 1 - vector3i.x, vector3i.y, vector3i.z);
			break;
		default:
			result = vector3i;
			break;
		}
		return result;
	}

	public Vector3i GetWorldPositionOfPoiOffset(Vector3i _offset)
	{
		Vector3i vector3i = this.boundingBoxSize;
		Vector3i vector3i2;
		switch (this.rotation & 3)
		{
		case 1:
			vector3i2 = new Vector3i(_offset.x, _offset.y, vector3i.z - 1 - _offset.z);
			break;
		case 2:
			vector3i2 = new Vector3i(vector3i.x - 1 - _offset.x, _offset.y, vector3i.z - 1 - _offset.z);
			break;
		case 3:
			vector3i2 = new Vector3i(vector3i.x - 1 - _offset.x, _offset.y, _offset.z);
			break;
		default:
			vector3i2 = _offset;
			break;
		}
		_offset = vector3i2;
		if ((this.rotation & 1) != 0)
		{
			MathUtils.Swap(ref _offset.x, ref _offset.z);
		}
		_offset += this.boundingBoxPosition;
		return _offset;
	}

	public int id;

	public byte rotation;

	public byte imposterBaseRotation;

	public Prefab prefab;

	public byte lastCopiedRotation;

	public Vector3i lastCopiedPrefabPosition;

	public bool bPrefabCopiedIntoWorld;

	public Vector3i boundingBoxPosition;

	public Vector3i boundingBoxSize;

	public string name;

	public PathAbstractions.AbstractedLocation location;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool imposterLookupDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathAbstractions.AbstractedLocation imposterLocation = PathAbstractions.AbstractedLocation.None;

	public int standaloneBlockSize;

	public float yOffsetOfPrefab;

	public QuestLockInstance lockInstance;

	public List<SleeperVolume> sleeperVolumes = new List<SleeperVolume>();

	public List<TriggerVolume> triggerVolumes = new List<TriggerVolume>();

	public List<WallVolume> wallVolumes = new List<WallVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> entityInstanceIds = new List<int>();

	public FastTags<TagGroup.Global> LastRefreshType = FastTags<TagGroup.Global>.none;

	public QuestClass LastQuestClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong occupiedChunks;
}
