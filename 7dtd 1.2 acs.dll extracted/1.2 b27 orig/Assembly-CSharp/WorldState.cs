using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WorldState
{
	public string Guid { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public WorldState()
	{
		this.providerId = EnumChunkProviderId.Disc;
		this.saveDataLimit = -1L;
		this.playerSpawnPoints = new SpawnPointList();
		this.GenerateNewGuid();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SaveLoad(string _filename, bool _load, bool _warnOnDifferentVersion, bool _infOnDiferentVersion)
	{
		bool result;
		lock (this)
		{
			Stream stream = null;
			try
			{
				if (_load)
				{
					try
					{
						stream = SdFile.OpenRead(_filename);
						goto IL_54;
					}
					catch (Exception arg)
					{
						Log.Error(string.Format("Opening saved game: {0}", arg));
						goto IL_54;
					}
				}
				try
				{
					stream = new BufferedStream(SdFile.Open(_filename, FileMode.Create, FileAccess.Write, FileShare.Read));
				}
				catch (Exception arg2)
				{
					Log.Error(string.Format("Opening buffer to save game: {0}", arg2));
				}
				IL_54:
				result = (stream != null && this.SaveLoad(stream, _load, _warnOnDifferentVersion, _infOnDiferentVersion));
			}
			catch (Exception arg3)
			{
				Log.Error(string.Format("Exception reading world header at pos {0}: {1}", (stream != null) ? stream.Position : 0L, arg3));
				result = false;
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SaveLoad(Stream _stream, bool _load, bool _warnOnDifferentVersion, bool _infOnDiferentVersion)
	{
		bool result;
		lock (this)
		{
			PooledBinaryWriter pooledBinaryWriter = null;
			PooledBinaryReader pooledBinaryReader = null;
			try
			{
				IBinaryReaderOrWriter binaryReaderOrWriter;
				if (_load)
				{
					this.chunkSizeX = (this.chunkSizeY = (this.chunkSizeZ = (this.chunkCount = 0)));
					pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false);
					pooledBinaryReader.SetBaseStream(_stream);
					binaryReaderOrWriter = pooledBinaryReader;
					int num = (int)binaryReaderOrWriter.ReadWrite(' ');
					char c = binaryReaderOrWriter.ReadWrite(' ');
					char c2 = binaryReaderOrWriter.ReadWrite(' ');
					byte b = binaryReaderOrWriter.ReadWrite(1);
					if (num != 116 || c != 't' || c2 != 'w' || b != 0)
					{
						Log.Error("Invalid magic bytes in world header");
						return false;
					}
				}
				else
				{
					pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false);
					pooledBinaryWriter.SetBaseStream(_stream);
					binaryReaderOrWriter = pooledBinaryWriter;
					binaryReaderOrWriter.ReadWrite('t');
					binaryReaderOrWriter.ReadWrite('t');
					binaryReaderOrWriter.ReadWrite('w');
					binaryReaderOrWriter.ReadWrite(0);
				}
				this.version = binaryReaderOrWriter.ReadWrite((uint)WorldState.CurrentSaveVersion);
				if (_load)
				{
					if ((ulong)this.version > (ulong)((long)WorldState.CurrentSaveVersion))
					{
						return true;
					}
					if (this.version > 11U)
					{
						this.gameVersionString = binaryReaderOrWriter.ReadWrite("");
						if (this.gameVersionString != Constants.cVersionInformation.LongString)
						{
							if (_warnOnDifferentVersion)
							{
								Log.Warning("Loaded world file from different version: '{0}'", new object[]
								{
									this.gameVersionString
								});
							}
							else if (_infOnDiferentVersion)
							{
								Log.Out("Loaded world file from different version: '{0}'", new object[]
								{
									this.gameVersionString
								});
							}
						}
					}
				}
				else
				{
					binaryReaderOrWriter.ReadWrite(Constants.cVersionInformation.LongString);
				}
				if (_load)
				{
					if (this.version > 14U)
					{
						VersionInformation.EGameReleaseType releaseType = (VersionInformation.EGameReleaseType)binaryReaderOrWriter.ReadWrite(1);
						int major = binaryReaderOrWriter.ReadWrite(1);
						int minor = binaryReaderOrWriter.ReadWrite(2);
						int build = binaryReaderOrWriter.ReadWrite(27);
						this.gameVersion = new VersionInformation(releaseType, major, minor, build);
					}
					else
					{
						VersionInformation.TryParseLegacyString(this.gameVersionString, out this.gameVersion);
					}
				}
				else
				{
					binaryReaderOrWriter.ReadWrite(1);
					binaryReaderOrWriter.ReadWrite(1);
					binaryReaderOrWriter.ReadWrite(2);
					binaryReaderOrWriter.ReadWrite(27);
				}
				binaryReaderOrWriter.ReadWrite(0U);
				if (this.version > 6U)
				{
					this.activeGameMode = binaryReaderOrWriter.ReadWrite(this.activeGameMode);
				}
				binaryReaderOrWriter.ReadWrite(0U);
				this.waterLevel = binaryReaderOrWriter.ReadWrite(this.waterLevel);
				this.chunkSizeX = binaryReaderOrWriter.ReadWrite(this.chunkSizeX);
				this.chunkSizeZ = binaryReaderOrWriter.ReadWrite(this.chunkSizeY);
				this.chunkSizeY = binaryReaderOrWriter.ReadWrite(this.chunkSizeZ);
				this.chunkCount = binaryReaderOrWriter.ReadWrite(this.chunkCount);
				this.providerId = (EnumChunkProviderId)binaryReaderOrWriter.ReadWrite((int)this.providerId);
				this.seed = binaryReaderOrWriter.ReadWrite(this.seed);
				this.worldTime = binaryReaderOrWriter.ReadWrite(this.worldTime);
				if (this.version > 8U)
				{
					this.timeInTicks = binaryReaderOrWriter.ReadWrite(this.timeInTicks);
				}
				if (_load)
				{
					if (this.version == 10U)
					{
						binaryReaderOrWriter.ReadWrite(0UL);
					}
					if (this.version > 1U && this.version < 7U)
					{
						binaryReaderOrWriter.ReadWrite(false);
					}
					if (this.version > 4U && this.version < 7U)
					{
						binaryReaderOrWriter.ReadWrite(false);
						binaryReaderOrWriter.ReadWrite(false);
					}
					if (this.version > 5U)
					{
						this.playerSpawnPoints.Read(binaryReaderOrWriter);
					}
					else if (this.version > 2U)
					{
						this.playerSpawnPoints.Clear();
						int num2 = binaryReaderOrWriter.ReadWrite(0);
						for (int i = 0; i < num2; i++)
						{
							this.playerSpawnPoints.Add(new SpawnPoint(new Vector3i(binaryReaderOrWriter.ReadWrite(0), binaryReaderOrWriter.ReadWrite(0), binaryReaderOrWriter.ReadWrite(0))));
						}
					}
				}
				else
				{
					binaryReaderOrWriter.ReadWrite(0);
					binaryReaderOrWriter.ReadWrite(0);
				}
				if (this.version > 3U)
				{
					this.nextEntityID = binaryReaderOrWriter.ReadWrite(this.nextEntityID);
				}
				if (_load)
				{
					this.nextEntityID = Utils.FastMax(this.nextEntityID, 171);
				}
				if (this.version >= 21U)
				{
					this.saveDataLimit = binaryReaderOrWriter.ReadWrite(this.saveDataLimit);
				}
				else
				{
					this.saveDataLimit = -1L;
				}
				if (this.version > 7U)
				{
					int num3 = binaryReaderOrWriter.ReadWrite((int)((this.dynamicSpawnerState != null) ? this.dynamicSpawnerState.Length : 0L));
					if (_load)
					{
						if (num3 > 0)
						{
							this.dynamicSpawnerState = new MemoryStream(num3);
							this.dynamicSpawnerState.SetLength((long)num3);
							binaryReaderOrWriter.ReadWrite(this.dynamicSpawnerState.GetBuffer(), 0, num3);
							this.dynamicSpawnerState.Position = 0L;
						}
					}
					else if (this.dynamicSpawnerState != null)
					{
						this.dynamicSpawnerState.Position = 0L;
						StreamUtils.StreamCopy(this.dynamicSpawnerState, binaryReaderOrWriter.BaseStream, null, true);
					}
				}
				if (this.version > 10U)
				{
					int num4 = binaryReaderOrWriter.ReadWrite((int)((this.aiDirectorState != null) ? this.aiDirectorState.Length : 0L));
					if (_load)
					{
						if (num4 > 0)
						{
							this.aiDirectorState = new MemoryStream(num4);
							this.aiDirectorState.SetLength((long)num4);
							binaryReaderOrWriter.ReadWrite(this.aiDirectorState.GetBuffer(), 0, num4);
							this.aiDirectorState.Position = 0L;
						}
					}
					else if (this.aiDirectorState != null)
					{
						this.aiDirectorState.Position = 0L;
						StreamUtils.StreamCopy(this.aiDirectorState, binaryReaderOrWriter.BaseStream, null, true);
					}
				}
				if (this.version > 12U)
				{
					int num5 = binaryReaderOrWriter.ReadWrite((int)((this.sleeperVolumeState != null) ? this.sleeperVolumeState.Length : 0L));
					if (_load)
					{
						if (num5 > 0)
						{
							this.sleeperVolumeState = new MemoryStream(num5);
							this.sleeperVolumeState.SetLength((long)num5);
							binaryReaderOrWriter.ReadWrite(this.sleeperVolumeState.GetBuffer(), 0, num5);
							this.sleeperVolumeState.Position = 0L;
						}
					}
					else if (this.sleeperVolumeState != null)
					{
						this.sleeperVolumeState.Position = 0L;
						StreamUtils.StreamCopy(this.sleeperVolumeState, binaryReaderOrWriter.BaseStream, null, true);
					}
				}
				if (this.version >= 19U)
				{
					int num6 = binaryReaderOrWriter.ReadWrite((int)((this.triggerVolumeState != null) ? this.triggerVolumeState.Length : 0L));
					if (_load)
					{
						if (num6 > 0)
						{
							this.triggerVolumeState = new MemoryStream(num6);
							this.triggerVolumeState.SetLength((long)num6);
							binaryReaderOrWriter.ReadWrite(this.triggerVolumeState.GetBuffer(), 0, num6);
							this.triggerVolumeState.Position = 0L;
						}
					}
					else if (this.triggerVolumeState != null)
					{
						this.triggerVolumeState.Position = 0L;
						StreamUtils.StreamCopy(this.triggerVolumeState, binaryReaderOrWriter.BaseStream, null, true);
					}
				}
				if (this.version >= 20U)
				{
					int num7 = binaryReaderOrWriter.ReadWrite((int)((this.wallVolumeState != null) ? this.wallVolumeState.Length : 0L));
					if (_load)
					{
						if (num7 > 0)
						{
							this.wallVolumeState = new MemoryStream(num7);
							this.wallVolumeState.SetLength((long)num7);
							binaryReaderOrWriter.ReadWrite(this.wallVolumeState.GetBuffer(), 0, num7);
							this.wallVolumeState.Position = 0L;
						}
					}
					else if (this.wallVolumeState != null)
					{
						this.wallVolumeState.Position = 0L;
						StreamUtils.StreamCopy(this.wallVolumeState, binaryReaderOrWriter.BaseStream, null, true);
					}
				}
				bool flag2 = false;
				if (this.version > 11U)
				{
					long position = binaryReaderOrWriter.BaseStream.Position;
					int num8 = 0;
					if (this.version > 15U)
					{
						num8 = binaryReaderOrWriter.ReadWrite(0);
					}
					if (GameManager.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.World != null && GameManager.Instance.World.Biomes != null)
					{
						int biomeCount = GameManager.Instance.World.Biomes.GetBiomeCount();
						long num9 = (long)biomeCount * 37L + 20L;
						bool flag3 = _stream.Position + num9 <= _stream.Length;
						if (!_load || flag3)
						{
							if (_load)
							{
								if (WeatherManager.savedWeather != null)
								{
									WeatherManager.savedWeather.Clear();
									for (int j = 0; j < biomeCount; j++)
									{
										WeatherPackage weatherPackage = new WeatherPackage();
										WeatherManager.savedWeather.Add(weatherPackage);
										for (int k = 0; k < 5; k++)
										{
											weatherPackage.param[k] = binaryReaderOrWriter.ReadWrite(0f);
										}
										weatherPackage.particleRain = binaryReaderOrWriter.ReadWrite(0f);
										weatherPackage.particleSnow = binaryReaderOrWriter.ReadWrite(0f);
										weatherPackage.surfaceWet = binaryReaderOrWriter.ReadWrite(0f);
										weatherPackage.surfaceSnow = binaryReaderOrWriter.ReadWrite(0f);
										weatherPackage.biomeID = binaryReaderOrWriter.ReadWrite(0);
									}
									WeatherManager.sLightningWorldTime = binaryReaderOrWriter.ReadWrite(0UL);
									WeatherManager.sLightningPos = binaryReaderOrWriter.ReadWrite(Vector3.zero);
								}
							}
							else if (WeatherManager.Instance && WeatherManager.Instance.biomeWeather != null)
							{
								List<WeatherManager.BiomeWeather> biomeWeather = WeatherManager.Instance.biomeWeather;
								for (int l = 0; l < biomeWeather.Count; l++)
								{
									WeatherManager.BiomeWeather biomeWeather2 = biomeWeather[l];
									for (int m = 0; m < 5; m++)
									{
										binaryReaderOrWriter.ReadWrite(biomeWeather2.parameters[m].value);
									}
									binaryReaderOrWriter.ReadWrite(biomeWeather2.rainParam.value);
									binaryReaderOrWriter.ReadWrite(biomeWeather2.snowFallParam.value);
									binaryReaderOrWriter.ReadWrite(biomeWeather2.wetParam.value);
									binaryReaderOrWriter.ReadWrite(biomeWeather2.snowCoverParam.value);
									binaryReaderOrWriter.ReadWrite(biomeWeather2.biomeDefinition.m_Id);
								}
								binaryReaderOrWriter.ReadWrite(WeatherManager.sLightningWorldTime);
								binaryReaderOrWriter.ReadWrite(WeatherManager.sLightningPos);
							}
							WeatherManager.globalTemperatureOffset = binaryReaderOrWriter.ReadWrite(WeatherManager.globalTemperatureOffset);
							WeatherManager.globalRainPercent = binaryReaderOrWriter.ReadWrite(WeatherManager.globalRainPercent);
							WeatherManager.globalRainDayStart = binaryReaderOrWriter.ReadWrite(WeatherManager.globalRainDayStart);
							WeatherManager.globalRainOLD1 = binaryReaderOrWriter.ReadWrite(WeatherManager.globalRainOLD1);
							WeatherManager.globalRainDayPeak = binaryReaderOrWriter.ReadWrite(WeatherManager.globalRainDayPeak);
							WeatherManager.globalRainOLD2 = binaryReaderOrWriter.ReadWrite(WeatherManager.globalRainOLD2);
							if (this.version <= 17U)
							{
								WeatherManager.VersionReset();
							}
							flag2 = true;
						}
					}
					if (this.version > 15U)
					{
						if (_load)
						{
							if (binaryReaderOrWriter.BaseStream.Position != position + (long)num8)
							{
								if (flag2)
								{
									Log.Out("Failed reading weather data from world header");
								}
								binaryReaderOrWriter.BaseStream.Position = position + (long)num8;
							}
						}
						else
						{
							num8 = (int)(binaryReaderOrWriter.BaseStream.Position - position);
							binaryReaderOrWriter.BaseStream.Position = position;
							binaryReaderOrWriter.ReadWrite(num8);
							binaryReaderOrWriter.BaseStream.Seek(0L, SeekOrigin.End);
						}
					}
				}
				if (this.version > 13U && (flag2 || this.version > 15U))
				{
					this.Guid = binaryReaderOrWriter.ReadWrite(this.Guid);
				}
				if (_load && string.IsNullOrEmpty(this.Guid))
				{
					this.GenerateNewGuid();
				}
				result = true;
			}
			catch (Exception e)
			{
				Log.Error("Exception reading world header at pos {0}:", new object[]
				{
					_stream.Position
				});
				Log.Exception(e);
				result = false;
			}
			finally
			{
				if (pooledBinaryReader != null)
				{
					MemoryPools.poolBinaryReader.FreeSync(pooledBinaryReader);
				}
				if (pooledBinaryWriter != null)
				{
					MemoryPools.poolBinaryWriter.FreeSync(pooledBinaryWriter);
				}
			}
		}
		return result;
	}

	public bool Load(string _filename, bool _warnOnDifferentVersion = true, bool _infOnDiferentVersion = false, bool _makeExtraBackupOnSuccess = false)
	{
		WorldState.<>c__DisplayClass29_0 CS$<>8__locals1;
		CS$<>8__locals1._makeExtraBackupOnSuccess = _makeExtraBackupOnSuccess;
		CS$<>8__locals1._filename = _filename;
		if (this.SaveLoad(CS$<>8__locals1._filename, true, _warnOnDifferentVersion, _infOnDiferentVersion))
		{
			WorldState.<Load>g__DoExtraBackup|29_0(CS$<>8__locals1._filename, ref CS$<>8__locals1);
			return true;
		}
		Log.Warning("Failed loading world header file: " + CS$<>8__locals1._filename);
		SdFile.Copy(CS$<>8__locals1._filename, CS$<>8__locals1._filename + ".loadFailed", true);
		string text = CS$<>8__locals1._filename + ".bak";
		if (SdFile.Exists(text))
		{
			Log.Out("Trying backup header: " + text);
			if (this.SaveLoad(text, true, _warnOnDifferentVersion, _infOnDiferentVersion))
			{
				WorldState.<Load>g__DoExtraBackup|29_0(text, ref CS$<>8__locals1);
				return true;
			}
			SdFile.Copy(text, text + ".loadFailed", true);
			Log.Error("Failed loading backup header file!");
		}
		else
		{
			Log.Out("No backup header!");
		}
		string text2 = CS$<>8__locals1._filename + ".ext.bak";
		if (SdFile.Exists(text2))
		{
			Log.Out("Trying extra backup header (from last successful load): " + text2);
			if (this.SaveLoad(text2, true, _warnOnDifferentVersion, _infOnDiferentVersion))
			{
				return true;
			}
			SdFile.Copy(text2, text2 + ".loadFailed", true);
			Log.Error("Failed loading extra backup header file!");
		}
		else
		{
			Log.Out("No extra backup header!");
		}
		return false;
	}

	public bool Save(string _filename)
	{
		if (SdFile.Exists(_filename) && GameIO.FileSize(_filename) > 0L)
		{
			SdFile.Copy(_filename, _filename + ".bak", true);
		}
		return this.SaveLoad(_filename, false, false, false);
	}

	public bool Save(Stream _stream)
	{
		return this.SaveLoad(_stream, false, false, false);
	}

	public void SetFrom(World _world, EnumChunkProviderId _chunkProviderId)
	{
		this.waterLevel = WorldConstants.WaterLevel;
		this.chunkSizeX = 16;
		this.chunkSizeY = 16;
		this.chunkSizeZ = 256;
		this.chunkCount = 0;
		this.providerId = _chunkProviderId;
		this.seed = _world.Seed;
		this.worldTime = _world.worldTime;
		this.timeInTicks = GameTimer.Instance.ticks;
		this.sleeperVolumeState = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.sleeperVolumeState);
			_world.WriteSleeperVolumes(pooledBinaryWriter);
		}
		this.triggerVolumeState = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter2 = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter2.SetBaseStream(this.triggerVolumeState);
			_world.WriteTriggerVolumes(pooledBinaryWriter2);
		}
		this.wallVolumeState = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter3 = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter3.SetBaseStream(this.wallVolumeState);
			_world.WriteWallVolumes(pooledBinaryWriter3);
		}
		this.nextEntityID = EntityFactory.nextEntityID;
		this.activeGameMode = _world.GetGameMode();
		this.dynamicSpawnerState = new MemoryStream();
		if (_world.GetDynamiceSpawnManager() != null)
		{
			using (PooledBinaryWriter pooledBinaryWriter4 = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter4.SetBaseStream(this.dynamicSpawnerState);
				_world.GetDynamiceSpawnManager().Write(pooledBinaryWriter4);
			}
		}
		this.aiDirectorState = new MemoryStream();
		if (_world.aiDirector != null)
		{
			using (PooledBinaryWriter pooledBinaryWriter5 = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter5.SetBaseStream(this.aiDirectorState);
				_world.aiDirector.Save(pooledBinaryWriter5);
				return;
			}
		}
		using (PooledBinaryWriter pooledBinaryWriter6 = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter6.SetBaseStream(this.aiDirectorState);
			new AIDirector(_world).Save(pooledBinaryWriter6);
		}
	}

	public void ResetDynamicData()
	{
		this.worldTime = 0UL;
		this.timeInTicks = 0UL;
	}

	public void GenerateNewGuid()
	{
		this.Guid = Utils.GenerateGuid();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <Load>g__DoExtraBackup|29_0(string sourceFilename, ref WorldState.<>c__DisplayClass29_0 A_1)
	{
		if (!A_1._makeExtraBackupOnSuccess)
		{
			return;
		}
		string text = A_1._filename + ".ext.bak";
		try
		{
			SdFile.Copy(sourceFilename, text, true);
		}
		catch (Exception arg)
		{
			Log.Error(string.Format("Failed to make extra backup (due to successfully loading) by copying '{0}' to '{1}': {2}", sourceFilename, text, arg));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int CurrentSaveVersion = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint version;

	public string gameVersionString = "";

	public VersionInformation gameVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public float waterLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkSizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkSizeY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkSizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkCount;

	public MemoryStream dynamicSpawnerState;

	public MemoryStream aiDirectorState;

	public int activeGameMode;

	public EnumChunkProviderId providerId;

	public int seed;

	public ulong worldTime;

	public ulong timeInTicks;

	public int nextEntityID;

	public long saveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPointList playerSpawnPoints;

	public MemoryStream sleeperVolumeState;

	public MemoryStream triggerVolumeState;

	public MemoryStream wallVolumeState;
}
