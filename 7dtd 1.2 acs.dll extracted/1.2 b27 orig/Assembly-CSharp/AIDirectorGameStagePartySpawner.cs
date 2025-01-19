using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorGameStagePartySpawner
{
	public AIDirectorGameStagePartySpawner(World _world, string _gameStageName)
	{
		this.world = _world;
		this.def = GameStageDefinition.GetGameStage(_gameStageName);
		this.partyMembers = new ReadOnlyCollection<EntityPlayer>(this.members);
		this.partyLevel = -1;
		this.gsScaling = 1f;
	}

	public void SetScaling(float _scaling)
	{
		this.gsScaling = Utils.FastLerp(1f, 2.8f, (_scaling - 1f) / 3f);
	}

	public void ResetPartyLevel(int mod = 0)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < this.members.Count; i++)
		{
			EntityPlayer entityPlayer = this.members[i];
			list.Add(entityPlayer.gameStage);
		}
		int num = GameStageDefinition.CalcPartyLevel(list);
		num = (int)((float)num * this.gsScaling);
		if (mod != 0)
		{
			num %= mod;
		}
		this.SetPartyLevel(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPartyLevel(int _partyLevel)
	{
		this.partyLevel = _partyLevel;
		this.stageSpawnMax = 0;
		this.groupIndex = 0;
		this.spawnCount = 0;
		if (this.def != null)
		{
			this.stage = this.def.GetStage(_partyLevel);
			if (this.stage != null)
			{
				this.stageSpawnMax = this.CalcStageSpawnMax();
				this.SetupGroup();
			}
		}
		this.bonusLootEvery = Utils.FastMax(this.stageSpawnMax / GameStageDefinition.LootBonusMaxCount, GameStageDefinition.LootBonusEvery);
		Log.Out("Party of {0}, game stage {1}, scaling {2}, enemy max {3}, bonus every {4}", new object[]
		{
			this.members.Count,
			this.partyLevel,
			this.gsScaling,
			this.stageSpawnMax,
			this.bonusLootEvery
		});
		Log.Out("Party members: ");
		for (int i = 0; i < this.members.Count; i++)
		{
			EntityPlayer entityPlayer = this.members[i];
			Log.Out("Player id {0}, gameStage {1}", new object[]
			{
				entityPlayer.entityId,
				entityPlayer.gameStage
			});
		}
	}

	public bool Tick(double _deltaTime)
	{
		if (this.spawnGroup != null)
		{
			bool flag = false;
			if (this.nextStageTime > 0UL && this.world.worldTime >= this.nextStageTime)
			{
				flag = true;
			}
			else if (this.spawnCount >= this.numToSpawn)
			{
				this.interval -= _deltaTime;
				flag = (this.interval <= 0.0);
			}
			if (flag)
			{
				this.groupIndex++;
				this.SetupGroup();
			}
		}
		return this.spawnGroup != null;
	}

	public void AddMember(EntityPlayer _player)
	{
		if (!this.memberIDs.Contains(_player.entityId))
		{
			this.memberIDs.Add(_player.entityId);
		}
		if (!this.members.Contains(_player))
		{
			this.members.Add(_player);
		}
	}

	public bool IsMemberOfParty(int _entityID)
	{
		return this.memberIDs.Contains(_entityID);
	}

	public void RemoveMember(EntityPlayer _player, bool removeID)
	{
		this.members.Remove(_player);
		if (removeID)
		{
			this.memberIDs.Remove(_player.entityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CalcStageSpawnMax()
	{
		int num = 0;
		int count = this.stage.Count;
		for (int i = 0; i < count; i++)
		{
			this.spawnGroup = this.stage.GetSpawnGroup(i);
			num += this.spawnGroup.spawnCount;
		}
		return num;
	}

	public void ClearMembers()
	{
		this.members.Clear();
		this.memberIDs.Clear();
	}

	public void IncSpawnCount()
	{
		this.spawnCount++;
	}

	public void DecSpawnCount(int dec)
	{
		if (dec > this.spawnCount)
		{
			this.spawnCount = 0;
			return;
		}
		this.spawnCount -= dec;
	}

	public bool IsDone
	{
		get
		{
			return this.groupIndex > 0 && this.spawnGroup == null;
		}
	}

	public bool canSpawn
	{
		get
		{
			return this.spawnGroup != null && this.spawnCount < this.numToSpawn;
		}
	}

	public int maxAlive
	{
		get
		{
			if (this.spawnGroup == null)
			{
				return 0;
			}
			return this.spawnGroup.maxAlive;
		}
	}

	public string spawnGroupName
	{
		get
		{
			if (this.spawnGroup == null)
			{
				return null;
			}
			return this.spawnGroup.groupName;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupGroup()
	{
		this.spawnGroup = this.stage.GetSpawnGroup(this.groupIndex);
		if (this.spawnGroup != null)
		{
			this.interval = (double)this.spawnGroup.interval;
			this.nextStageTime = ((this.spawnGroup.duration > 0UL) ? (this.world.worldTime + this.spawnGroup.duration * 1000UL) : 0UL);
			this.numToSpawn = EntitySpawner.ModifySpawnCountByGameDifficulty(this.spawnGroup.spawnCount);
			this.spawnCount = 0;
			return;
		}
		Log.Out("AIDirectorGameStagePartySpawner: groups done ({0})", new object[]
		{
			this.groupIndex
		});
	}

	public override string ToString()
	{
		return string.Format("{0} {1} (count {2}, numToSpawn {3}, maxAlive {4})", new object[]
		{
			this.groupIndex,
			this.spawnGroupName,
			this.spawnCount,
			this.numToSpawn,
			this.maxAlive
		});
	}

	public ReadOnlyCollection<EntityPlayer> partyMembers;

	public float gsScaling;

	public int groupIndex;

	public int partyLevel;

	public int stageSpawnMax;

	public int bonusLootEvery;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStageDefinition def;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStageDefinition.Stage stage;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStageDefinition.SpawnGroup spawnGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public double interval;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextStageTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> memberIDs = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> members = new List<EntityPlayer>();
}
