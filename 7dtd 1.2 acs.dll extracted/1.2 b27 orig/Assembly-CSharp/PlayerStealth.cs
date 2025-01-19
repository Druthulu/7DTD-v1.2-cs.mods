using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct PlayerStealth
{
	public void Init(EntityPlayer _player)
	{
		this.player = _player;
		this.noises = new List<PlayerStealth.NoiseData>();
		this.barColorUI = new Color32(0, 0, 0, byte.MaxValue);
	}

	public void Tick()
	{
		float num = Utils.FastAbs(this.player.speedForward) + Utils.FastAbs(this.player.speedStrafe);
		if (num > 0.1f)
		{
			this.speedAverage = Utils.FastLerpUnclamped(this.speedAverage, num, 0.2f);
		}
		else
		{
			this.speedAverage *= 0.5f;
		}
		float num3;
		float num2 = LightManager.GetStealthLightLevel(this.player, out num3);
		float num4 = num3 / (num2 + 0.05f);
		num4 = Utils.FastClamp(num4, 0.5f, 3.2f);
		num2 += num3 * num4;
		if (this.player.IsCrouching)
		{
			num2 *= 0.6f;
		}
		this.player.Buffs.SetCustomVar("_lightlevel", num2 * 100f, true);
		num2 *= 1f + this.speedAverage * 0.15f;
		float num5 = EffectManager.GetValue(PassiveEffects.LightMultiplier, null, 1f, this.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.lightAttackPercent = ((num3 < 0.1f) ? num5 : 1f);
		num5 = 0.32f + 0.68f * num5;
		float v = num2 * num5 * 100f;
		this.lightLevel = Utils.FastClamp(v, 0f, 200f);
		this.ProcNoiseCleanup();
		float num6 = this.CalcVolume();
		this.player.Buffs.SetCustomVar("_noiselevel", this.noiseVolume, true);
		int num7 = this.sleeperNoiseWaitTicks - 1;
		this.sleeperNoiseWaitTicks = num7;
		if (num7 <= 0)
		{
			this.sleeperNoiseVolume -= 2.5f;
			if (this.sleeperNoiseVolume < 0f)
			{
				this.sleeperNoiseVolume = 0f;
			}
		}
		if (num6 > 0f)
		{
			float num8 = num6 * 1.2f;
			float num9 = EAIManager.CalcSenseScale();
			num8 *= 1f + num9 * 1.6f;
			float num10 = 75f + 25f * num9;
			if (num8 > num10)
			{
				num8 = num10;
			}
			Bounds bb = new Bounds(this.player.position, new Vector3(num8, num8, num8));
			this.player.world.GetEntitiesInBounds(typeof(EntityEnemy), bb, PlayerStealth.entityTempList);
			for (int i = 0; i < PlayerStealth.entityTempList.Count; i++)
			{
				EntityAlive entityAlive = (EntityAlive)PlayerStealth.entityTempList[i];
				float distance = this.player.GetDistance(entityAlive);
				float num11 = this.noiseVolume * (1f + num9 * entityAlive.aiManager.feralSense);
				num11 /= distance * 0.6f + 0.4f;
				num11 *= this.player.DetectUsScale(entityAlive);
				if (num11 >= 1f)
				{
					bool flag = true;
					if (entityAlive.noisePlayer)
					{
						flag = (num11 > entityAlive.noisePlayerVolume);
					}
					if (flag)
					{
						entityAlive.noisePlayer = this.player;
						entityAlive.noisePlayerDistance = distance;
						entityAlive.noisePlayerVolume = num11;
					}
				}
			}
			PlayerStealth.entityTempList.Clear();
		}
		num7 = this.alertEnemiesTicks - 1;
		this.alertEnemiesTicks = num7;
		if (num7 <= 0)
		{
			this.alertEnemiesTicks = 20;
			this.alertEnemy = false;
			this.player.world.GetEntitiesAround(EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit, this.player.position, 12f, PlayerStealth.entityTempList);
			for (int j = 0; j < PlayerStealth.entityTempList.Count; j++)
			{
				if (((EntityAlive)PlayerStealth.entityTempList[j]).IsAlert)
				{
					this.alertEnemy = true;
					break;
				}
			}
			PlayerStealth.entityTempList.Clear();
			this.SetBarColor(this.alertEnemy);
		}
		if (this.player.isEntityRemote)
		{
			if (this.sendTickDelay > 0)
			{
				this.sendTickDelay--;
			}
			if ((this.player.IsCrouching && this.sendTickDelay == 0 && (this.lightLevelSent != (int)this.lightLevel || this.noiseVolumeSent != (int)this.noiseVolume)) || this.alertEnemySent != this.alertEnemy)
			{
				this.sendTickDelay = 16;
				this.lightLevelSent = (int)this.lightLevel;
				this.noiseVolumeSent = (int)this.noiseVolume;
				this.alertEnemySent = this.alertEnemy;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStealth>().Setup(this.player, this.lightLevelSent, this.noiseVolumeSent, this.alertEnemySent), false, this.player.entityId, -1, -1, null, 192);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBarColor(bool _isAlert)
	{
		this.barColorUI.r = 50;
		this.barColorUI.g = 135;
		if (_isAlert)
		{
			this.barColorUI.r = 180;
			this.barColorUI.g = 180;
		}
	}

	public void ProcNoiseCleanup()
	{
		for (int i = 0; i < this.noises.Count; i++)
		{
			PlayerStealth.NoiseData noiseData = this.noises[i];
			if (noiseData.ticks > 1)
			{
				noiseData.ticks--;
				this.noises[i] = noiseData;
			}
			else
			{
				this.noises.RemoveAt(i);
				i--;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcVolume()
	{
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < this.noises.Count; i++)
		{
			num += this.noises[i].volume * num2;
			num2 *= 0.6f;
		}
		this.noiseVolume = Mathf.Pow(num * 2.35f, 0.86f);
		this.noiseVolume *= 1.5f;
		this.noiseVolume *= EffectManager.GetValue(PassiveEffects.NoiseMultiplier, null, 1f, this.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		return num;
	}

	public bool CanSleeperAttackDetect(EntityAlive _e)
	{
		if (this.player.IsCrouching)
		{
			float num = Utils.FastLerp(3f, 15f, this.lightAttackPercent);
			if (_e.GetDistance(this.player) > num)
			{
				return false;
			}
		}
		return true;
	}

	public void SetClientLevels(float _lightLevel, float _noiseVolume, bool _isAlert)
	{
		this.lightLevel = _lightLevel;
		this.noiseVolume = _noiseVolume;
		this.alertEnemy = _isAlert;
		this.SetBarColor(_isAlert);
	}

	public bool NotifyNoise(float volume, float duration)
	{
		if (volume <= 0f)
		{
			return false;
		}
		this.AddNoise(this.noises, volume, (int)(duration * 20f));
		if (volume >= 11f)
		{
			this.sleeperNoiseWaitTicks = 20;
		}
		float num = volume;
		if (volume > 60f)
		{
			num = 60f + Mathf.Pow(volume - 60f, 1.4f);
		}
		num *= EffectManager.GetValue(PassiveEffects.NoiseMultiplier, null, 1f, this.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.sleeperNoiseVolume += num;
		if (this.sleeperNoiseVolume >= 360f)
		{
			this.sleeperNoiseVolume = 360f;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddNoise(List<PlayerStealth.NoiseData> list, float volume, int ticks)
	{
		PlayerStealth.NoiseData item = new PlayerStealth.NoiseData(volume, ticks);
		for (int i = 0; i < list.Count; i++)
		{
			PlayerStealth.NoiseData noiseData = this.noises[i];
			if (volume >= noiseData.volume)
			{
				list.Insert(i, item);
				return;
			}
		}
		list.Insert(list.Count, item);
	}

	public static PlayerStealth Read(EntityPlayer _player, BinaryReader br)
	{
		int num = br.ReadInt32();
		PlayerStealth playerStealth = default(PlayerStealth);
		playerStealth.Init(_player);
		playerStealth.lightLevel = (float)br.ReadInt32();
		int num2 = br.ReadInt32();
		if (num2 > 0)
		{
			if (num >= 3)
			{
				for (int i = 0; i < num2; i++)
				{
					br.ReadSingle();
					float volume = br.ReadSingle();
					int ticks = br.ReadInt32();
					playerStealth.AddNoise(playerStealth.noises, volume, ticks);
				}
			}
			else if (num >= 2)
			{
				for (int j = 0; j < num2; j++)
				{
					br.ReadSingle();
					br.ReadSingle();
					br.ReadInt32();
				}
			}
			else
			{
				for (int k = 0; k < num2; k++)
				{
					br.ReadInt32();
					br.ReadInt32();
				}
			}
		}
		return playerStealth;
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(3);
		bw.Write(this.lightLevel);
		bw.Write((this.noises != null) ? this.noises.Count : 0);
		if (this.noises != null)
		{
			for (int i = 0; i < this.noises.Count; i++)
			{
				PlayerStealth.NoiseData noiseData = this.noises[i];
				bw.Write(0f);
				bw.Write(noiseData.volume);
				bw.Write(noiseData.ticks);
			}
		}
	}

	public Color32 ValueColorUI
	{
		get
		{
			return this.barColorUI;
		}
	}

	public float ValuePercentUI
	{
		get
		{
			return Utils.FastClamp01((this.lightLevel + this.noiseVolume * 0.5f + (float)(this.alertEnemy ? 5 : 0)) * 0.01f + 0.005f);
		}
	}

	public const float cLightLevelMax = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLightMpyBase = 0.32f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cNextSoundPercent = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSleeperNoiseDecay = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSleeperNoiseHear = 360f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSleeperNoiseWaitTicks = 20;

	public float lightLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lightAttackPercent;

	public float noiseVolume;

	public int smell;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedAverage;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sendTickDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lightLevelSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int noiseVolumeSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alertEnemySent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sleeperNoiseWaitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float sleeperNoiseVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlayerStealth.NoiseData> noises;

	[PublicizedFrom(EAccessModifier.Private)]
	public int alertEnemiesTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alertEnemy;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 barColorUI;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> entityTempList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct NoiseData
	{
		public NoiseData(float _volume, int _ticks)
		{
			this.volume = _volume;
			this.ticks = _ticks;
		}

		public float volume;

		public int ticks;
	}
}
