using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionFillSafeZone : BaseAction
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
			string[] array = this.fillTypeNames.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				ActionFillSafeZone.FillTypes item = (ActionFillSafeZone.FillTypes)Enum.Parse(typeof(ActionFillSafeZone.FillTypes), array[i]);
				this.FillTypeList.Add(item);
			}
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			switch (this.currentState)
			{
			case ActionFillSafeZone.FillSafeZoneStates.FindClaim:
			{
				if (this.FillTypeList.Count == 0)
				{
					return BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				this.FillType = this.FillTypeList.RandomObject<ActionFillSafeZone.FillTypes>();
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
				if (this.destroyClaim)
				{
					world.SetBlockRPC(this.claimPos, BlockValue.Air);
				}
				this.currentState = ActionFillSafeZone.FillSafeZoneStates.HandleChunks;
				break;
			}
			case ActionFillSafeZone.FillSafeZoneStates.HandleChunks:
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
						}
					}
				}
				List<Entity> entitiesInBounds = world.GetEntitiesInBounds(null, new Bounds(this.claimPos, Vector3.one * (float)num), true);
				for (int k = 0; k < entitiesInBounds.Count; k++)
				{
					Entity entity = entitiesInBounds[k];
					bool flag = entity is EntityPlayer;
					Vector3i vector3i = new Vector3i(Utils.Fastfloor(entity.position.x), Utils.Fastfloor(entity.position.y), Utils.Fastfloor(entity.position.z));
					if (flag)
					{
						EntityPlayer key = entity as EntityPlayer;
						Vector3 value = vector3i + new Vector3(0.5f, 0.5f, 0.5f);
						this.playerDictionary.Add(key, value);
					}
					this.blockedPositions.Add(vector3i);
					this.blockedPositions.Add(vector3i + Vector3i.up);
					if (flag)
					{
						this.blockedPositions.Add(vector3i + Vector3i.up * 2);
					}
					vector3i += Vector3i.right;
					this.blockedPositions.Add(vector3i);
					this.blockedPositions.Add(vector3i + Vector3i.up);
					if (flag)
					{
						this.blockedPositions.Add(vector3i + Vector3i.up * 2);
					}
				}
				this.currentState = ActionFillSafeZone.FillSafeZoneStates.SetupChanges;
				break;
			}
			case ActionFillSafeZone.FillSafeZoneStates.SetupChanges:
			{
				this.blockChanges.Clear();
				IChunk chunk2 = null;
				int num5 = this.halfClaimSize;
				BlockValue blockValue = Block.GetBlockValue(this.blockName, false);
				this.TeleportPlayers();
				FastTags<TagGroup.Global> other = (this.blockTags != null) ? FastTags<TagGroup.Global>.Parse(this.blockTags) : FastTags<TagGroup.Global>.none;
				switch (this.FillType)
				{
				case ActionFillSafeZone.FillTypes.Cube:
					for (int l = -num5; l <= num5; l++)
					{
						for (int m = -num5; m <= num5; m++)
						{
							for (int n = -num5; n <= num5; n++)
							{
								Vector3i vector3i2 = this.claimPos + new Vector3i(n, l, m);
								if (world.GetChunkFromWorldPos(vector3i2, ref chunk2))
								{
									int x2 = World.toBlockXZ(vector3i2.x);
									int y = World.toBlockY(vector3i2.y);
									int z2 = World.toBlockXZ(vector3i2.z);
									BlockValue blockNoDamage = chunk2.GetBlockNoDamage(x2, y, z2);
									if ((blockNoDamage.isair || blockNoDamage.Block.Tags.Test_AnySet(other)) && !this.blockedPositions.Contains(vector3i2))
									{
										Chunk chunk3 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk3))
										{
											this.chunkList.Add(chunk3);
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i2, blockValue));
									}
								}
							}
						}
					}
					break;
				case ActionFillSafeZone.FillTypes.Sphere:
					for (int num6 = -num5; num6 <= num5; num6++)
					{
						for (int num7 = -num5; num7 <= num5; num7++)
						{
							for (int num8 = -num5; num8 <= num5; num8++)
							{
								Vector3i vector3i3 = this.claimPos + new Vector3i(num8, num6, num7);
								if (Vector3.SqrMagnitude(this.claimPos - vector3i3) < (float)this.distanceSq && world.GetChunkFromWorldPos(vector3i3, ref chunk2))
								{
									int x3 = World.toBlockXZ(vector3i3.x);
									int y2 = World.toBlockY(vector3i3.y);
									int z3 = World.toBlockXZ(vector3i3.z);
									BlockValue blockNoDamage2 = chunk2.GetBlockNoDamage(x3, y2, z3);
									if ((blockNoDamage2.isair || blockNoDamage2.Block.Tags.Test_AnySet(other)) && !this.blockedPositions.Contains(vector3i3))
									{
										Chunk chunk4 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk4))
										{
											this.chunkList.Add(chunk4);
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i3, blockValue));
									}
								}
							}
						}
					}
					break;
				case ActionFillSafeZone.FillTypes.Cylinder:
					for (int num9 = -num5; num9 <= num5; num9++)
					{
						for (int num10 = -num5; num10 <= num5; num10++)
						{
							for (int num11 = -num5; num11 <= num5; num11++)
							{
								Vector3i vector3i4 = this.claimPos + new Vector3i(num11, num9, num10);
								if (Vector3.SqrMagnitude(new Vector3i(this.claimPos.x, vector3i4.y, this.claimPos.z) - vector3i4) < (float)this.distanceSq && world.GetChunkFromWorldPos(vector3i4, ref chunk2))
								{
									int x4 = World.toBlockXZ(vector3i4.x);
									int y3 = World.toBlockY(vector3i4.y);
									int z4 = World.toBlockXZ(vector3i4.z);
									BlockValue blockNoDamage3 = chunk2.GetBlockNoDamage(x4, y3, z4);
									if ((blockNoDamage3.isair || blockNoDamage3.Block.Tags.Test_AnySet(other)) && !this.blockedPositions.Contains(vector3i4))
									{
										Chunk chunk5 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk5))
										{
											this.chunkList.Add(chunk5);
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk5.ClrIdx, vector3i4, blockValue));
									}
								}
							}
						}
					}
					break;
				case ActionFillSafeZone.FillTypes.Pyramid:
				{
					int num12 = 0;
					for (int num13 = -num5; num13 <= num5; num13++)
					{
						int num14 = num5 - num12 / 2;
						for (int num15 = -num14; num15 <= num14; num15++)
						{
							for (int num16 = -num14; num16 <= num14; num16++)
							{
								Vector3i vector3i5 = this.claimPos + new Vector3i(num16, num13, num15);
								if (world.GetChunkFromWorldPos(vector3i5, ref chunk2))
								{
									int x5 = World.toBlockXZ(vector3i5.x);
									int y4 = World.toBlockY(vector3i5.y);
									int z5 = World.toBlockXZ(vector3i5.z);
									BlockValue blockNoDamage4 = chunk2.GetBlockNoDamage(x5, y4, z5);
									if ((blockNoDamage4.isair || blockNoDamage4.Block.Tags.Test_AnySet(other)) && !this.blockedPositions.Contains(vector3i5))
									{
										Chunk chunk6 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk6))
										{
											this.chunkList.Add(chunk6);
										}
										this.blockChanges.Add(new BlockChangeInfo(chunk6.ClrIdx, vector3i5, blockValue));
									}
								}
							}
						}
						num12++;
					}
					break;
				}
				}
				this.currentState = ActionFillSafeZone.FillSafeZoneStates.AddSigns;
				break;
			}
			case ActionFillSafeZone.FillSafeZoneStates.AddSigns:
			{
				int num17 = this.halfClaimSize + 1;
				BlockValue blockValue2 = Block.GetBlockValue("playerSignWood1x3", false);
				if (this.signPositions == null)
				{
					this.signPositions = new List<Vector3i>();
				}
				Vector3i vector3i6 = this.claimPos + new Vector3i(0, 0, num17);
				vector3i6.y = (int)(world.GetHeight(vector3i6.x, vector3i6.z) + 1);
				blockValue2.rotation = 0;
				this.signPositions.Add(vector3i6);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i6, blockValue2));
				vector3i6 = this.claimPos + new Vector3i(0, 0, -num17);
				vector3i6.y = (int)(world.GetHeight(vector3i6.x, vector3i6.z) + 1);
				blockValue2.rotation = 2;
				this.signPositions.Add(vector3i6);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i6, blockValue2));
				vector3i6 = this.claimPos + new Vector3i(num17, 0, 0);
				vector3i6.y = (int)(world.GetHeight(vector3i6.x, vector3i6.z) + 1);
				blockValue2.rotation = 1;
				this.signPositions.Add(vector3i6);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i6, blockValue2));
				vector3i6 = this.claimPos + new Vector3i(-num17, 0, 0);
				vector3i6.y = (int)(world.GetHeight(vector3i6.x, vector3i6.z) + 1);
				blockValue2.rotation = 3;
				this.signPositions.Add(vector3i6);
				this.blockChanges.Add(new BlockChangeInfo(0, vector3i6, blockValue2));
				this.currentState = ActionFillSafeZone.FillSafeZoneStates.Action;
				break;
			}
			case ActionFillSafeZone.FillSafeZoneStates.Action:
				GameManager.Instance.ChangeBlocks(null, this.blockChanges);
				this.currentState = ActionFillSafeZone.FillSafeZoneStates.ResetChunks;
				break;
			case ActionFillSafeZone.FillSafeZoneStates.ResetChunks:
				for (int num18 = 0; num18 < this.chunkList.Count; num18++)
				{
					Chunk chunk7 = this.chunkList[num18];
					long item = WorldChunkCache.MakeChunkKey(chunk7.X, chunk7.Z);
					this.chunkHash.Add(item);
				}
				if (base.Owner.Target != null)
				{
					PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
					for (int num19 = 0; num19 < this.signPositions.Count; num19++)
					{
						TileEntitySign tileEntitySign = world.GetTileEntity(0, this.signPositions[num19]) as TileEntitySign;
						if (tileEntitySign != null)
						{
							tileEntitySign.SetText(this.ModifiedName, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
						}
					}
				}
				world.m_ChunkManager.ResendChunksToClients(this.chunkHash);
				this.TeleportPlayers();
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void TeleportPlayers()
		{
			foreach (EntityPlayer entityPlayer in this.playerDictionary.Keys)
			{
				GameManager.Instance.StartCoroutine(base.TeleportEntity(entityPlayer, this.playerDictionary[entityPlayer], 0f));
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionFillSafeZone.PropFillType, ref this.fillTypeNames);
			properties.ParseBool(ActionFillSafeZone.PropDestroyClaim, ref this.destroyClaim);
			properties.ParseString(ActionFillSafeZone.PropBlock, ref this.blockName);
			properties.ParseString(ActionFillSafeZone.PropNewName, ref this.newName);
			properties.ParseString(ActionFillSafeZone.PropBlockTags, ref this.blockTags);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionFillSafeZone
			{
				fillTypeNames = this.fillTypeNames,
				FillTypeList = this.FillTypeList,
				blockName = this.blockName,
				destroyClaim = this.destroyClaim,
				newName = this.newName,
				blockTags = this.blockTags
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionFillSafeZone.FillSafeZoneStates currentState;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionFillSafeZone.FillTypes FillType = ActionFillSafeZone.FillTypes.Sphere;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string fillTypeNames = "Sphere";

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<ActionFillSafeZone.FillTypes> FillTypeList = new List<ActionFillSafeZone.FillTypes>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool destroyClaim;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string newName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blockTags;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropFillType = "fill_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlock = "block";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDestroyClaim = "destroy_claim";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlockTags = "block_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropNewName = "new_name";

		[PublicizedFrom(EAccessModifier.Private)]
		public string blockName = "terrDirtTwitch";

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
		public HashSet<Vector3i> blockedPositions = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSetLong chunkHash = new HashSetLong();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<EntityPlayer, Vector3> playerDictionary = new Dictionary<EntityPlayer, Vector3>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector3i> signPositions;

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum FillSafeZoneStates
		{
			FindClaim,
			HandleChunks,
			SetupChanges,
			AddSigns,
			Action,
			ResetChunks
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum FillTypes
		{
			Cube,
			Sphere,
			Cylinder,
			Pyramid
		}
	}
}
