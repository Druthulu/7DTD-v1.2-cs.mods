﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionFillArea : BaseAction
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
				ActionFillArea.FillTypes item = (ActionFillArea.FillTypes)Enum.Parse(typeof(ActionFillArea.FillTypes), array[i]);
				this.FillTypeList.Add(item);
			}
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			switch (this.currentState)
			{
			case ActionFillArea.FillSafeZoneStates.HandleChunks:
			{
				this.FillType = this.FillTypeList.RandomObject<ActionFillArea.FillTypes>();
				if (base.Owner.TargetPosition != Vector3.zero)
				{
					this.claimPos = new Vector3i(base.Owner.TargetPosition);
				}
				else
				{
					if (!(base.Owner.Target.position != Vector3.zero))
					{
						return BaseAction.ActionCompleteStates.InCompleteRefund;
					}
					this.claimPos = new Vector3i(base.Owner.Target.position);
				}
				this.chunkList.Clear();
				int num = this.claimSize / 16 + 1;
				int num2 = this.claimSize / 16 + 1;
				for (int i = -num; i <= num; i++)
				{
					int x = this.claimPos.x + i * 16;
					for (int j = -num2; j <= num2; j++)
					{
						int z = this.claimPos.z + j * 16;
						Chunk chunk = (Chunk)world.GetChunkFromWorldPos(new Vector3i(x, this.claimPos.y, z));
						if (chunk != null && !this.chunkList.Contains(chunk))
						{
							this.chunkList.Add(chunk);
						}
					}
				}
				List<Entity> entitiesInBounds = world.GetEntitiesInBounds(null, new Bounds(this.claimPos, Vector3.one * (float)this.claimSize), true);
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
				this.currentState = ActionFillArea.FillSafeZoneStates.SetupChanges;
				break;
			}
			case ActionFillArea.FillSafeZoneStates.SetupChanges:
			{
				this.blockChanges.Clear();
				IChunk chunk2 = null;
				int num3 = this.halfClaimSize;
				BlockValue blockValue = Block.GetBlockValue(this.blockName, false);
				this.TeleportPlayers();
				FastTags<TagGroup.Global> other = (this.blockTags != null) ? FastTags<TagGroup.Global>.Parse(this.blockTags) : FastTags<TagGroup.Global>.none;
				FastTags<TagGroup.Global> other2 = (this.excludeBlockTags != null) ? FastTags<TagGroup.Global>.Parse(this.excludeBlockTags) : FastTags<TagGroup.Global>.none;
				switch (this.FillType)
				{
				case ActionFillArea.FillTypes.Cube:
					for (int l = -num3; l <= num3; l++)
					{
						for (int m = -num3; m <= num3; m++)
						{
							for (int n = -num3; n <= num3; n++)
							{
								Vector3i vector3i2 = this.claimPos + new Vector3i(n, l, m);
								if (world.GetChunkFromWorldPos(vector3i2, ref chunk2))
								{
									int x2 = World.toBlockXZ(vector3i2.x);
									int y = World.toBlockY(vector3i2.y);
									int z2 = World.toBlockXZ(vector3i2.z);
									BlockValue blockNoDamage = chunk2.GetBlockNoDamage(x2, y, z2);
									if ((blockNoDamage.isair || blockNoDamage.Block.Tags.Test_AnySet(other) || this.replaceAll) && !blockNoDamage.Block.Tags.Test_AnySet(other2) && world.GetTraderAreaAt(vector3i2) == null && !this.blockedPositions.Contains(vector3i2))
									{
										Chunk chunk3 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk3))
										{
											this.chunkList.Add(chunk3);
										}
										if (this.maxDensity)
										{
											this.blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i2, blockValue, sbyte.MaxValue));
										}
										else
										{
											this.blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i2, blockValue));
										}
									}
								}
							}
						}
					}
					break;
				case ActionFillArea.FillTypes.Sphere:
					for (int num4 = -num3; num4 <= num3; num4++)
					{
						for (int num5 = -num3; num5 <= num3; num5++)
						{
							for (int num6 = -num3; num6 <= num3; num6++)
							{
								Vector3i vector3i3 = this.claimPos + new Vector3i(num6, num4, num5);
								if (Vector3.SqrMagnitude(this.claimPos - vector3i3) < (float)this.distanceSq && world.GetChunkFromWorldPos(vector3i3, ref chunk2))
								{
									int x3 = World.toBlockXZ(vector3i3.x);
									int y2 = World.toBlockY(vector3i3.y);
									int z3 = World.toBlockXZ(vector3i3.z);
									BlockValue blockNoDamage2 = chunk2.GetBlockNoDamage(x3, y2, z3);
									if ((blockNoDamage2.isair || blockNoDamage2.Block.Tags.Test_AnySet(other) || this.replaceAll) && !blockNoDamage2.Block.Tags.Test_AnySet(other2) && world.GetTraderAreaAt(vector3i3) == null && !this.blockedPositions.Contains(vector3i3))
									{
										Chunk chunk4 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk4))
										{
											this.chunkList.Add(chunk4);
										}
										if (this.maxDensity)
										{
											this.blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i3, blockValue, sbyte.MaxValue));
										}
										else
										{
											this.blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i3, blockValue));
										}
									}
								}
							}
						}
					}
					break;
				case ActionFillArea.FillTypes.Cylinder:
					for (int num7 = -num3; num7 <= num3; num7++)
					{
						for (int num8 = -num3; num8 <= num3; num8++)
						{
							for (int num9 = -num3; num9 <= num3; num9++)
							{
								Vector3i vector3i4 = this.claimPos + new Vector3i(num9, num7, num8);
								if (Vector3.SqrMagnitude(new Vector3i(this.claimPos.x, vector3i4.y, this.claimPos.z) - vector3i4) < (float)this.distanceSq && world.GetChunkFromWorldPos(vector3i4, ref chunk2))
								{
									int x4 = World.toBlockXZ(vector3i4.x);
									int y3 = World.toBlockY(vector3i4.y);
									int z4 = World.toBlockXZ(vector3i4.z);
									BlockValue blockNoDamage3 = chunk2.GetBlockNoDamage(x4, y3, z4);
									if ((blockNoDamage3.isair || blockNoDamage3.Block.Tags.Test_AnySet(other) || this.replaceAll) && !blockNoDamage3.Block.Tags.Test_AnySet(other2) && world.GetTraderAreaAt(vector3i4) == null && !this.blockedPositions.Contains(vector3i4))
									{
										Chunk chunk5 = (Chunk)chunk2;
										if (!this.chunkList.Contains(chunk5))
										{
											this.chunkList.Add(chunk5);
										}
										if (this.maxDensity)
										{
											this.blockChanges.Add(new BlockChangeInfo(chunk5.ClrIdx, vector3i4, blockValue, sbyte.MaxValue));
										}
										else
										{
											this.blockChanges.Add(new BlockChangeInfo(chunk5.ClrIdx, vector3i4, blockValue));
										}
									}
								}
							}
						}
					}
					break;
				}
				this.currentState = ActionFillArea.FillSafeZoneStates.AddSigns;
				break;
			}
			case ActionFillArea.FillSafeZoneStates.AddSigns:
				if (this.newName == "")
				{
					this.currentState = ActionFillArea.FillSafeZoneStates.Action;
				}
				else
				{
					int num10 = this.halfClaimSize + 1;
					BlockValue blockValue2 = Block.GetBlockValue("playerSignWood1x3", false);
					if (this.signPositions == null)
					{
						this.signPositions = new List<Vector3i>();
					}
					Vector3i vector3i5 = this.claimPos + new Vector3i(0, 0, num10);
					vector3i5.y = (int)(world.GetHeight(vector3i5.x, vector3i5.z) + 1);
					blockValue2.rotation = 0;
					if (world.GetTraderAreaAt(vector3i5) == null)
					{
						this.signPositions.Add(vector3i5);
						this.blockChanges.Add(new BlockChangeInfo(0, vector3i5, blockValue2));
					}
					vector3i5 = this.claimPos + new Vector3i(0, 0, -num10);
					vector3i5.y = (int)(world.GetHeight(vector3i5.x, vector3i5.z) + 1);
					blockValue2.rotation = 2;
					if (world.GetTraderAreaAt(vector3i5) == null)
					{
						this.signPositions.Add(vector3i5);
						this.blockChanges.Add(new BlockChangeInfo(0, vector3i5, blockValue2));
					}
					vector3i5 = this.claimPos + new Vector3i(num10, 0, 0);
					vector3i5.y = (int)(world.GetHeight(vector3i5.x, vector3i5.z) + 1);
					blockValue2.rotation = 1;
					if (world.GetTraderAreaAt(vector3i5) == null)
					{
						this.signPositions.Add(vector3i5);
						this.blockChanges.Add(new BlockChangeInfo(0, vector3i5, blockValue2));
					}
					vector3i5 = this.claimPos + new Vector3i(-num10, 0, 0);
					vector3i5.y = (int)(world.GetHeight(vector3i5.x, vector3i5.z) + 1);
					blockValue2.rotation = 3;
					if (world.GetTraderAreaAt(vector3i5) == null)
					{
						this.signPositions.Add(vector3i5);
						this.blockChanges.Add(new BlockChangeInfo(0, vector3i5, blockValue2));
					}
					this.currentState = ActionFillArea.FillSafeZoneStates.Action;
				}
				break;
			case ActionFillArea.FillSafeZoneStates.Action:
				GameManager.Instance.ChangeBlocks(null, this.blockChanges);
				this.currentState = ActionFillArea.FillSafeZoneStates.ResetChunks;
				break;
			case ActionFillArea.FillSafeZoneStates.ResetChunks:
				for (int num11 = 0; num11 < this.chunkList.Count; num11++)
				{
					Chunk chunk6 = this.chunkList[num11];
					long item = WorldChunkCache.MakeChunkKey(chunk6.X, chunk6.Z);
					this.chunkHash.Add(item);
				}
				if (base.Owner.Target != null && this.newName != "")
				{
					PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
					for (int num12 = 0; num12 < this.signPositions.Count; num12++)
					{
						TileEntitySign tileEntitySign = world.GetTileEntity(0, this.signPositions[num12]) as TileEntitySign;
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
			properties.ParseString(ActionFillArea.PropFillType, ref this.fillTypeNames);
			properties.ParseBool(ActionFillArea.PropDestroyClaim, ref this.destroyClaim);
			properties.ParseString(ActionFillArea.PropBlock, ref this.blockName);
			properties.ParseString(ActionFillArea.PropNewName, ref this.newName);
			properties.ParseString(ActionFillArea.PropBlockTags, ref this.blockTags);
			properties.ParseString(ActionFillArea.PropExcludeBlockTags, ref this.excludeBlockTags);
			properties.ParseBool(ActionFillArea.PropReplaceAll, ref this.replaceAll);
			properties.ParseBool(ActionFillArea.PropMaxDensity, ref this.maxDensity);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionFillArea
			{
				fillTypeNames = this.fillTypeNames,
				FillTypeList = this.FillTypeList,
				blockName = this.blockName,
				destroyClaim = this.destroyClaim,
				newName = this.newName,
				blockTags = this.blockTags,
				excludeBlockTags = this.excludeBlockTags,
				replaceAll = this.replaceAll,
				maxDensity = this.maxDensity
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionFillArea.FillSafeZoneStates currentState;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionFillArea.FillTypes FillType = ActionFillArea.FillTypes.Sphere;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string fillTypeNames = "Sphere";

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<ActionFillArea.FillTypes> FillTypeList = new List<ActionFillArea.FillTypes>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool destroyClaim;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string newName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blockTags;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string excludeBlockTags;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool replaceAll;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool maxDensity;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropFillType = "fill_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlock = "block";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDestroyClaim = "destroy_claim";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlockTags = "block_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropExcludeBlockTags = "exclude_block_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropReplaceAll = "replace_all";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropNewName = "new_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxDensity = "max_density";

		[PublicizedFrom(EAccessModifier.Private)]
		public string blockName = "terrDirtTwitch";

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i claimPos = Vector3i.zero;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Chunk> chunkList = new List<Chunk>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int claimSize = 10;

		[PublicizedFrom(EAccessModifier.Private)]
		public int halfClaimSize = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public int distanceSq = 25;

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
			Cylinder
		}
	}
}
