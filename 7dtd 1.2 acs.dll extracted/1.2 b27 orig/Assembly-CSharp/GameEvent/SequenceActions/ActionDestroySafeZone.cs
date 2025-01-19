using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionDestroySafeZone : BaseAction
	{
		public string ModifiedName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return base.GetTextWithElements(this.newName);
			}
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

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			base.OnInit();
			string[] array = this.destroyTypeNames.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				ActionDestroySafeZone.DestructionTypes item = (ActionDestroySafeZone.DestructionTypes)Enum.Parse(typeof(ActionDestroySafeZone.DestructionTypes), array[i]);
				this.DestroyTypeList.Add(item);
			}
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			switch (this.currentState)
			{
			case ActionDestroySafeZone.DestroySafeZoneStates.FindClaim:
			{
				if (this.DestroyTypeList.Count == 0)
				{
					return BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				this.DestructionType = this.DestroyTypeList.RandomObject<ActionDestroySafeZone.DestructionTypes>();
				if (base.Owner.TargetPosition == Vector3.zero)
				{
					return BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				this.claimSize = Mathf.Min(GameStats.GetInt(EnumGameStats.LandClaimSize), 41);
				this.halfClaimSize = (this.claimSize - 1) / 2;
				this.distanceSq = this.halfClaimSize * this.halfClaimSize;
				this.claimPos = new Vector3i(base.Owner.TargetPosition);
				BlockValue block = world.GetBlock(this.claimPos);
				if (!(block.Block is BlockLandClaim) || !BlockLandClaim.IsPrimary(block))
				{
					return BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				world.SetBlockRPC(this.claimPos, BlockValue.Air);
				this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.HandleChunks;
				break;
			}
			case ActionDestroySafeZone.DestroySafeZoneStates.HandleChunks:
			{
				this.chunkList.Clear();
				int num = GameStats.GetInt(EnumGameStats.LandClaimSize) - 1;
				int num2 = GameStats.GetInt(EnumGameStats.LandClaimDeadZone) + num;
				int num3 = num2 / 16 + 1;
				int num4 = num2 / 16 + 1;
				for (int i = -num3; i <= num3; i++)
				{
					int x = this.claimPos.x + i * 16;
					for (int j = -num4; j <= num4; j++)
					{
						int z = this.claimPos.z + j * 16;
						Chunk chunk = (Chunk)world.GetChunkFromWorldPos(new Vector3i(x, this.claimPos.y, z));
						if (chunk != null && !this.chunkList.Contains(chunk))
						{
							this.chunkList.Add(chunk);
							chunk.StopStabilityCalculation = true;
						}
					}
				}
				List<Entity> entitiesInBounds = world.GetEntitiesInBounds(null, new Bounds(this.claimPos, Vector3.one * (float)num), true);
				for (int k = 0; k < entitiesInBounds.Count; k++)
				{
					EntityAlive entityAlive = entitiesInBounds[k] as EntityAlive;
					if (entityAlive != null)
					{
						entityAlive.Buffs.AddBuff("buffTwitchDontBreakLeg", -1, true, false, -1f);
					}
				}
				this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.SetupChanges;
				break;
			}
			case ActionDestroySafeZone.DestroySafeZoneStates.SetupChanges:
			{
				this.blockChanges.Clear();
				IChunk chunk2 = null;
				int num5 = this.halfClaimSize;
				switch (this.DestructionType)
				{
				case ActionDestroySafeZone.DestructionTypes.Cube:
					for (int l = -num5; l <= num5; l++)
					{
						for (int m = -num5; m <= num5; m++)
						{
							for (int n = -num5; n <= num5; n++)
							{
								Vector3i vector3i = this.claimPos + new Vector3i(n, l, m);
								if (world.GetChunkFromWorldPos(vector3i, ref chunk2))
								{
									int x2 = World.toBlockXZ(vector3i.x);
									int y = World.toBlockY(vector3i.y);
									int z2 = World.toBlockXZ(vector3i.z);
									if (!chunk2.IsAir(x2, y, z2) && chunk2.GetBlock(x2, y, z2).Block.blockMaterial.CanDestroy)
									{
										Chunk chunk3 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk3))
										{
											this.chunkList.Add(chunk3);
											chunk3.StopStabilityCalculation = true;
										}
										ITileEntityLootable tileEntityLootable;
										if (chunk3.GetTileEntity(World.toBlock(vector3i)).TryGetSelfOrFeature(out tileEntityLootable))
										{
											tileEntityLootable.SetEmpty();
											tileEntityLootable.SetModified();
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i, BlockValue.Air));
									}
								}
							}
						}
					}
					break;
				case ActionDestroySafeZone.DestructionTypes.Sphere:
					for (int num6 = -num5; num6 <= num5; num6++)
					{
						for (int num7 = -num5; num7 <= num5; num7++)
						{
							for (int num8 = -num5; num8 <= num5; num8++)
							{
								Vector3i vector3i2 = this.claimPos + new Vector3i(num8, num6, num7);
								if (Vector3.SqrMagnitude(this.claimPos - vector3i2) < (float)this.distanceSq && world.GetChunkFromWorldPos(vector3i2, ref chunk2))
								{
									int x3 = World.toBlockXZ(vector3i2.x);
									int y2 = World.toBlockY(vector3i2.y);
									int z3 = World.toBlockXZ(vector3i2.z);
									if (!chunk2.IsAir(x3, y2, z3) && chunk2.GetBlock(x3, y2, z3).Block.blockMaterial.CanDestroy)
									{
										Chunk chunk4 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk4))
										{
											this.chunkList.Add(chunk4);
											chunk4.StopStabilityCalculation = true;
										}
										ITileEntityLootable tileEntityLootable2;
										if (chunk4.GetTileEntity(World.toBlock(vector3i2)).TryGetSelfOrFeature(out tileEntityLootable2))
										{
											tileEntityLootable2.SetEmpty();
											tileEntityLootable2.SetModified();
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i2, BlockValue.Air));
									}
								}
							}
						}
					}
					break;
				case ActionDestroySafeZone.DestructionTypes.Cylinder:
					for (int num9 = -num5; num9 <= num5; num9++)
					{
						for (int num10 = -num5; num10 <= num5; num10++)
						{
							for (int num11 = -num5; num11 <= num5; num11++)
							{
								Vector3i vector3i3 = this.claimPos + new Vector3i(num11, num9, num10);
								if (Vector3.SqrMagnitude(new Vector3i(this.claimPos.x, vector3i3.y, this.claimPos.z) - vector3i3) < (float)this.distanceSq && world.GetChunkFromWorldPos(vector3i3, ref chunk2))
								{
									int x4 = World.toBlockXZ(vector3i3.x);
									int y3 = World.toBlockY(vector3i3.y);
									int z4 = World.toBlockXZ(vector3i3.z);
									if (!chunk2.IsAir(x4, y3, z4) && chunk2.GetBlock(x4, y3, z4).Block.blockMaterial.CanDestroy)
									{
										Chunk chunk5 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk5))
										{
											this.chunkList.Add(chunk5);
											chunk5.StopStabilityCalculation = true;
										}
										ITileEntityLootable tileEntityLootable3;
										if (chunk5.GetTileEntity(World.toBlock(vector3i3)).TryGetSelfOrFeature(out tileEntityLootable3))
										{
											tileEntityLootable3.SetEmpty();
											tileEntityLootable3.SetModified();
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk5.ClrIdx, vector3i3, BlockValue.Air));
									}
								}
							}
						}
					}
					break;
				case ActionDestroySafeZone.DestructionTypes.LandClaimOnly:
					this.blockChanges.Add(new BlockChangeInfo(0, this.claimPos, BlockValue.Air));
					break;
				}
				if (this.DestructionType == ActionDestroySafeZone.DestructionTypes.LandClaimOnly)
				{
					this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.Action;
				}
				else
				{
					this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.AddSigns;
				}
				break;
			}
			case ActionDestroySafeZone.DestroySafeZoneStates.AddSigns:
			{
				int num12 = this.halfClaimSize + 1;
				BlockValue blockValue = Block.GetBlockValue("playerSignWood1x3", false);
				if (this.signPositions == null)
				{
					this.signPositions = new List<Vector3i>();
				}
				Vector3i vector3i4 = this.claimPos + new Vector3i(0, 0, num12);
				vector3i4.y = (int)(world.GetHeight(vector3i4.x, vector3i4.z) + 1);
				blockValue.rotation = 0;
				this.signPositions.Add(vector3i4);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
				vector3i4 = this.claimPos + new Vector3i(0, 0, -num12);
				vector3i4.y = (int)(world.GetHeight(vector3i4.x, vector3i4.z) + 1);
				blockValue.rotation = 2;
				this.signPositions.Add(vector3i4);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
				vector3i4 = this.claimPos + new Vector3i(num12, 0, 0);
				vector3i4.y = (int)(world.GetHeight(vector3i4.x, vector3i4.z) + 1);
				blockValue.rotation = 1;
				this.signPositions.Add(vector3i4);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
				vector3i4 = this.claimPos + new Vector3i(-num12, 0, 0);
				vector3i4.y = (int)(world.GetHeight(vector3i4.x, vector3i4.z) + 1);
				blockValue.rotation = 3;
				this.signPositions.Add(vector3i4);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
				this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.Action;
				break;
			}
			case ActionDestroySafeZone.DestroySafeZoneStates.Action:
				GameManager.Instance.ChangeBlocks(null, this.blockChanges);
				this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.ResetChunks;
				break;
			case ActionDestroySafeZone.DestroySafeZoneStates.ResetChunks:
				for (int num13 = 0; num13 < this.chunkList.Count; num13++)
				{
					Chunk chunk6 = this.chunkList[num13];
					long item = WorldChunkCache.MakeChunkKey(chunk6.X, chunk6.Z);
					chunk6.StopStabilityCalculation = false;
					this.chunkHash.Add(item);
				}
				if (this.signPositions != null && base.Owner.Target != null)
				{
					PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
					for (int num14 = 0; num14 < this.signPositions.Count; num14++)
					{
						TileEntitySign tileEntitySign = world.GetTileEntity(0, this.signPositions[num14]) as TileEntitySign;
						if (tileEntitySign != null)
						{
							tileEntitySign.SetText(this.ModifiedName, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
						}
					}
				}
				world.m_ChunkManager.ResendChunksToClients(this.chunkHash);
				this.currentState = ActionDestroySafeZone.DestroySafeZoneStates.ClientResets;
				break;
			case ActionDestroySafeZone.DestroySafeZoneStates.ClientResets:
				if (this.delay <= 0f)
				{
					world.m_ChunkManager.ResendChunksToClients(this.chunkHash);
					return BaseAction.ActionCompleteStates.Complete;
				}
				this.delay -= Time.deltaTime;
				break;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionDestroySafeZone.PropDestructionType, ref this.destroyTypeNames);
			properties.ParseString(ActionDestroySafeZone.PropNewName, ref this.newName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionDestroySafeZone
			{
				destroyTypeNames = this.destroyTypeNames,
				DestroyTypeList = this.DestroyTypeList,
				newName = this.newName
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionDestroySafeZone.DestroySafeZoneStates currentState;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionDestroySafeZone.DestructionTypes DestructionType = ActionDestroySafeZone.DestructionTypes.Sphere;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string destroyTypeNames = "Sphere";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string newName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<ActionDestroySafeZone.DestructionTypes> DestroyTypeList = new List<ActionDestroySafeZone.DestructionTypes>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDestructionType = "destruction_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropNewName = "new_name";

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i claimPos = Vector3i.zero;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Chunk> chunkList = new List<Chunk>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int claimSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public int halfClaimSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public int distanceSq;

		[PublicizedFrom(EAccessModifier.Private)]
		public float delay = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSetLong chunkHash = new HashSetLong();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector3i> signPositions;

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum DestroySafeZoneStates
		{
			FindClaim,
			HandleChunks,
			SetupChanges,
			AddSigns,
			Action,
			ResetChunks,
			ClientResets
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum DestructionTypes
		{
			Cube,
			Sphere,
			Cylinder,
			LandClaimOnly
		}
	}
}
