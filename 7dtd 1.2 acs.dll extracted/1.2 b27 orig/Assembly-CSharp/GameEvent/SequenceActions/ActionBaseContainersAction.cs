using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBaseContainersAction : BaseAction
	{
		public string ModifiedName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return base.GetTextWithElements(this.newName);
			}
		}

		public override bool CanPerform(Entity target)
		{
			return base.CanPerform(target) && this.GetTileEntityList(target);
		}

		public virtual bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
		{
			isEmpty = true;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GetTileEntityList(Entity target)
		{
			World world = GameManager.Instance.World;
			Vector3i blockPosition = target.GetBlockPosition();
			int num = World.toChunkXZ(blockPosition.x);
			int num2 = World.toChunkXZ(blockPosition.z);
			int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
			int num3 = @int / 16 + 1;
			int num4 = @int / 16 + 1;
			this.tileEntityList.Clear();
			bool result = false;
			for (int i = -num4; i <= num4; i++)
			{
				for (int j = -num3; j <= num3; j++)
				{
					Chunk chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
					if (chunk != null)
					{
						DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
						for (int k = 0; k < tileEntities.list.Count; k++)
						{
							TileEntity tileEntity = tileEntities.list[k];
							if (tileEntity != null && tileEntity.EntityId == -1)
							{
								bool flag = false;
								ActionBaseContainersAction.TargetingTypes targetingType = this.TargetingType;
								if (targetingType != ActionBaseContainersAction.TargetingTypes.SafeZone)
								{
									if (targetingType == ActionBaseContainersAction.TargetingTypes.Distance)
									{
										if (target.GetDistanceSq(tileEntity.ToWorldPos().ToVector3()) < this.maxDistance)
										{
											ITileEntityLootable tileEntityLootable;
											if (tileEntity.TryGetSelfOrFeature(out tileEntityLootable) && !tileEntityLootable.bPlayerStorage)
											{
												goto IL_1D9;
											}
											flag = true;
										}
									}
								}
								else
								{
									ITileEntityLootable tileEntityLootable2;
									if (tileEntity.TryGetSelfOrFeature(out tileEntityLootable2) && !tileEntityLootable2.bPlayerStorage)
									{
										goto IL_1D9;
									}
									flag = world.IsMyLandProtectedBlock(tileEntity.ToWorldPos(), world.gameManager.GetPersistentPlayerList().GetPlayerDataFromEntityID(target.entityId), false);
								}
								if (flag)
								{
									bool flag2 = false;
									if (this.CheckValidTileEntity(tileEntity, out flag2))
									{
										this.tileEntityList.Add(tileEntity);
										if (!flag2)
										{
											result = true;
										}
										int entityIDForLockedTileEntity = GameManager.Instance.GetEntityIDForLockedTileEntity(tileEntity);
										if (entityIDForLockedTileEntity != -1)
										{
											EntityPlayer entityPlayer = world.GetEntity(entityIDForLockedTileEntity) as EntityPlayer;
											if (entityPlayer.isEntityRemote)
											{
												SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityIDForLockedTileEntity), false, entityIDForLockedTileEntity, -1, -1, null, 192);
											}
											else
											{
												(entityPlayer as EntityPlayerLocal).PlayerUI.windowManager.CloseAllOpenWindows(null, false);
											}
										}
									}
								}
							}
							IL_1D9:;
						}
					}
				}
			}
			return result;
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			for (int i = 0; i < this.tileEntityList.Count; i++)
			{
				TileEntity te = this.tileEntityList[i];
				int entityIDForLockedTileEntity = GameManager.Instance.GetEntityIDForLockedTileEntity(te);
				if (entityIDForLockedTileEntity != -1)
				{
					EntityPlayer entityPlayer = world.GetEntity(entityIDForLockedTileEntity) as EntityPlayer;
					if (entityPlayer.isEntityRemote)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityIDForLockedTileEntity), false, entityIDForLockedTileEntity, -1, -1, null, 192);
					}
					else
					{
						(entityPlayer as EntityPlayerLocal).PlayerUI.windowManager.CloseAllOpenWindows(null, false);
					}
					return BaseAction.ActionCompleteStates.InComplete;
				}
			}
			if (!this.HandleContainerAction(this.tileEntityList))
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool HandleContainerAction(List<TileEntity> tileEntityList)
		{
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override string ParseTextElement(string element)
		{
			if (!(element == "viewer"))
			{
				return element;
			}
			if (base.Owner.ExtraData.Length <= 12)
			{
				return base.Owner.ExtraData;
			}
			return base.Owner.ExtraData.Insert(12, "\n");
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Contains(ActionBaseContainersAction.PropNewName))
			{
				this.changeName = true;
				properties.ParseString(ActionBaseContainersAction.PropNewName, ref this.newName);
			}
			properties.ParseEnum<ActionBaseContainersAction.TargetingTypes>(ActionBaseContainersAction.PropTargetingType, ref this.TargetingType);
			properties.ParseFloat(ActionBaseContainersAction.PropMaxDistance, ref this.maxDistance);
			this.maxDistance *= this.maxDistance;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float maxDistance = 5f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string newName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool changeName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<TileEntity> tileEntityList = new List<TileEntity>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionBaseContainersAction.ContainerActionStates ActionState;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionBaseContainersAction.TargetingTypes TargetingType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxDistance = "max_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropNewName = "new_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetingType = "targeting_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum ContainerActionStates
		{
			FindContainers,
			Action
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum TargetingTypes
		{
			SafeZone,
			Distance
		}
	}
}
