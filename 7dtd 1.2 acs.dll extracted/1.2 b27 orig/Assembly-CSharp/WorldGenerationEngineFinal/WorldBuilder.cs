using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Platform;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace WorldGenerationEngineFinal
{
	public class WorldBuilder
	{
		public int HalfWorldSize
		{
			get
			{
				return this.WorldSize / 2;
			}
		}

		public Vector3i PrefabWorldOffset
		{
			get
			{
				return new Vector3i(-this.HalfWorldSize, 0, -this.HalfWorldSize);
			}
		}

		public long SerializedSize
		{
			get
			{
				if (!SaveInfoProvider.DataLimitEnabled)
				{
					return 0L;
				}
				return this.serializedTotalSize;
			}
		}

		public WorldBuilder(string _seed, int _worldSize)
		{
			this.WorldSeedName = _seed;
			this.WorldSize = _worldSize;
			this.WorldName = WorldBuilder.GetGeneratedWorldName(this.WorldSeedName, this.WorldSize);
			this.WorldPath = GameIO.GetUserGameDataDir() + "/GeneratedWorlds/" + this.WorldName + "/";
			this.WorldSizeDistDiv = ((this.WorldSize > 4500) ? 1 : ((this.WorldSize > 3500) ? 2 : ((this.WorldSize > 2500) ? 3 : 4)));
			this.DistrictPlanner = new DistrictPlanner(this);
			this.HighwayPlanner = new HighwayPlanner(this);
			this.PathingUtils = new PathingUtils(this);
			this.PathShared = new PathShared(this);
			this.POISmoother = new POISmoother(this);
			this.PrefabManager = new PrefabManager(this);
			this.StampManager = new StampManager(this);
			this.StreetTileShared = new StreetTileShared(this);
			this.TilePathingUtils = new TilePathingUtils(this);
			this.TownPlanner = new TownPlanner(this);
			this.TownshipShared = new TownshipShared(this);
			this.WildernessPathPlanner = new WildernessPathPlanner(this);
			this.WildernessPlanner = new WildernessPlanner(this);
			List<ValueTuple<string, string, Action<Stream>>> list = new List<ValueTuple<string, string, Action<Stream>>>();
			List<ValueTuple<string, string, Func<Stream, IEnumerator>>> list2 = new List<ValueTuple<string, string, Func<Stream, IEnumerator>>>();
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiBiomes", "biomes.png", delegate(Stream stream)
			{
				stream.Write(ImageConversion.EncodeArrayToPNG(this.biomeDest, GraphicsFormat.R8G8B8A8_UNorm, (uint)this.BiomeSize, (uint)this.BiomeSize, (uint)(this.BiomeSize * 4)));
			}));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiRadiation", "radiation.png", delegate(Stream stream)
			{
				stream.Write(ImageConversion.EncodeArrayToPNG(this.radDest, GraphicsFormat.R8G8B8A8_UNorm, (uint)this.WorldSize, (uint)this.WorldSize, (uint)(this.WorldSize * 4)));
			}));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiRoads", "splat3.png", delegate(Stream stream)
			{
				stream.Write(ImageConversion.EncodeArrayToPNG(this.dest, GraphicsFormat.R8G8B8A8_UNorm, (uint)this.WorldSize, (uint)this.WorldSize, (uint)(this.WorldSize * 4)));
			}));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiWater", "splat4.png", new Action<Stream>(this.serializeWater)));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiHeightmap", "dtm.raw", new Action<Stream>(this.serializeRawHeightmap)));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiPrefabs", "prefabs.xml", new Action<Stream>(this.serializePrefabs)));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiPlayerSpawns", "spawnpoints.xml", new Action<Stream>(this.serializePlayerSpawns)));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiLevelMetadata", "main.ttw", new Action<Stream>(this.serializeRWGTTW)));
			list.Add(new ValueTuple<string, string, Action<Stream>>("xuiMapInfo", "map_info.xml", new Action<Stream>(this.serializeDynamicProperties)));
			this.threadedSerializers = list.ToArray();
			this.mainThreadSerializers = list2.ToArray();
			this.threadedSerializerBuffers = new MemoryStream[this.threadedSerializers.Length];
			this.mainThreadSerializerBuffers = new MemoryStream[this.mainThreadSerializers.Length];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator Init()
		{
			if (PlatformOptimizations.RestartAfterRwg)
			{
				PlatformApplicationManager.SetRestartRequired();
			}
			this.dest = new Color32[this.WorldSize * this.WorldSize];
			yield return this.StampManager.LoadStamps();
			this.PrefabManager.PrefabInstanceId = 0;
			this.playerSpawns = new List<WorldBuilder.PlayerSpawn>();
			this.PathingGrid = new PathTile[this.WorldSize / 10, this.WorldSize / 10];
			foreach (KeyValuePair<string, Vector2i> keyValuePair in WorldBuilderStatic.WorldSizeMapper)
			{
				string text;
				Vector2i vector2i;
				keyValuePair.Deconstruct(out text, out vector2i);
				string text2 = text;
				Vector2i vector2i2 = vector2i;
				if (this.WorldSize >= vector2i2.x && this.WorldSize < vector2i2.y)
				{
					this.worldSizeName = text2;
				}
			}
			if (this.worldSizeName == null)
			{
				Log.Error(string.Format("There was an error finding rwgmixer world entry for the current world size! WorldSize: {0}/n Please make sure that the world size falls within the min/max ranges listed in xml.", this.WorldSize));
				yield break;
			}
			this.thisWorldProperties = WorldBuilderStatic.Properties[this.worldSizeName];
			this.Seed = this.WorldSeedName.GetHashCode() + this.WorldSize;
			Rand.Instance.SetSeed(this.Seed);
			this.biomeColors[BiomeType.forest] = WorldBuilderConstants.forestCol;
			this.biomeColors[BiomeType.burntForest] = WorldBuilderConstants.burntForestCol;
			this.biomeColors[BiomeType.desert] = WorldBuilderConstants.desertCol;
			this.biomeColors[BiomeType.snow] = WorldBuilderConstants.snowCol;
			this.biomeColors[BiomeType.wasteland] = WorldBuilderConstants.wastelandCol;
			yield break;
		}

		public void SetBiomeWeight(BiomeType _type, int _weight)
		{
			switch (_type)
			{
			case BiomeType.forest:
				this.ForestBiomeWeight = _weight;
				return;
			case BiomeType.burntForest:
				this.BurntForestBiomeWeight = _weight;
				return;
			case BiomeType.desert:
				this.DesertBiomeWeight = _weight;
				return;
			case BiomeType.snow:
				this.SnowBiomeWeight = _weight;
				return;
			case BiomeType.wasteland:
				this.WastelandBiomeWeight = _weight;
				return;
			default:
				return;
			}
		}

		public IEnumerator GenerateFromServer()
		{
			this.UsePreviewer = false;
			this.totalMS = new MicroStopwatch(true);
			yield return this.GenerateData();
			yield return this.SaveData(false, null, false, null, null, null);
			this.Cleanup();
			this.SetMessage(null, false, false);
			yield break;
		}

		public IEnumerator GenerateFromUI()
		{
			this.IsCanceled = false;
			this.IsFinished = false;
			this.totalMS = new MicroStopwatch(true);
			yield return this.SetMessage(Localization.Get("xuiStarting", false), false, false);
			yield return new WaitForSeconds(0.1f);
			yield return this.GenerateData();
			yield break;
		}

		public IEnumerator FinishForPreview()
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			yield return this.CreatePreviewTexture(this.dest);
			Log.Out("CreatePreviewTexture in {0}", new object[]
			{
				(float)ms.ElapsedMilliseconds * 0.001f
			});
			if (!this.IsCanceled)
			{
				yield return this.SetMessage(Localization.Get("xuiRwgGenerationComplete", false), true, false);
			}
			else
			{
				yield return this.SetMessage(Localization.Get("xuiRwgGenerationCanceled", false), true, true);
			}
			this.IsFinished = true;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GenerateData()
		{
			yield return this.Init();
			yield return this.SetMessage(string.Format(Localization.Get("xuiWorldGenerationGenerating", false), this.WorldName), true, false);
			yield return this.generateTerrain();
			if (this.IsCanceled)
			{
				yield break;
			}
			this.initStreetTiles();
			if (this.IsCanceled)
			{
				yield break;
			}
			if (this.Towns != WorldBuilder.GenerationSelections.None || this.Wilderness != WorldBuilder.GenerationSelections.None)
			{
				yield return this.PrefabManager.LoadPrefabs();
				this.PrefabManager.ShufflePrefabData(this.Seed);
				yield return null;
				this.PathingUtils.SetupPathingGrid();
			}
			else
			{
				this.PrefabManager.ClearDisplayed();
			}
			if (this.Towns != WorldBuilder.GenerationSelections.None)
			{
				yield return this.TownPlanner.Plan(this.thisWorldProperties, this.Seed);
			}
			yield return this.GenerateTerrainLast();
			if (this.IsCanceled)
			{
				yield break;
			}
			yield return this.POISmoother.SmoothStreetTiles();
			if (this.IsCanceled)
			{
				yield break;
			}
			if (this.Towns != WorldBuilder.GenerationSelections.None || this.Wilderness != WorldBuilder.GenerationSelections.None)
			{
				yield return this.HighwayPlanner.Plan(this.thisWorldProperties, this.Seed);
				yield return this.TownPlanner.SpawnPrefabs();
				if (this.IsCanceled)
				{
					yield break;
				}
			}
			if (this.Wilderness != WorldBuilder.GenerationSelections.None)
			{
				yield return this.WildernessPlanner.Plan(this.thisWorldProperties, this.Seed);
				yield return this.smoothWildernessTerrain();
				yield return this.WildernessPathPlanner.Plan(this.Seed);
			}
			int num = 12 - this.playerSpawns.Count;
			if (num > 0)
			{
				foreach (StreetTile streetTile in this.CalcPlayerSpawnTiles())
				{
					if (this.CreatePlayerSpawn(streetTile.WorldPositionCenter, true) && --num <= 0)
					{
						break;
					}
				}
			}
			GCUtils.Collect();
			yield return this.SetMessage(Localization.Get("xuiRwgDrawRoads", false), true, false);
			yield return this.DrawRoads(this.dest);
			if (this.Towns != WorldBuilder.GenerationSelections.None || this.Wilderness != WorldBuilder.GenerationSelections.None)
			{
				yield return this.SetMessage(Localization.Get("xuiRwgSmoothRoadTerrain", false), true, false);
				yield return this.smoothRoadTerrain(this.dest, this.HeightMap, this.WorldSize, this.Townships);
			}
			this.paths.Clear();
			this.wildernessPaths.Clear();
			yield return this.FinalizeWater();
			yield return this.SerializeData();
			GCUtils.Collect();
			Log.Out("RWG final in {0}:{1:00}, r={2:x}", new object[]
			{
				this.totalMS.Elapsed.Minutes,
				this.totalMS.Elapsed.Seconds,
				Rand.Instance.PeekSample()
			});
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator SerializeData()
		{
			if (!SaveInfoProvider.DataLimitEnabled)
			{
				yield break;
			}
			MicroStopwatch totalMs = new MicroStopwatch(true);
			Task[] threadedSerializerTasks = new Task[this.threadedSerializers.Length];
			for (int j = 0; j < this.threadedSerializers.Length; j++)
			{
				WorldBuilder.<>c__DisplayClass90_1 CS$<>8__locals2 = new WorldBuilder.<>c__DisplayClass90_1();
				ValueTuple<string, string, Action<Stream>> valueTuple = this.threadedSerializers[j];
				CS$<>8__locals2.fileName = valueTuple.Item2;
				CS$<>8__locals2.serializer = valueTuple.Item3;
				CS$<>8__locals2.buffer = new MemoryStream();
				this.threadedSerializerBuffers[j] = CS$<>8__locals2.buffer;
				Task task = new Task(new Action(CS$<>8__locals2.<SerializeData>g__SerializeToBuffer|1));
				threadedSerializerTasks[j] = task;
				task.Start();
			}
			int k;
			for (int l = 0; l < this.mainThreadSerializers.Length; l = k + 1)
			{
				WorldBuilder.<>c__DisplayClass90_2 CS$<>8__locals3 = new WorldBuilder.<>c__DisplayClass90_2();
				ValueTuple<string, string, Func<Stream, IEnumerator>> valueTuple2 = this.mainThreadSerializers[l];
				string item = valueTuple2.Item1;
				CS$<>8__locals3.fileName = valueTuple2.Item2;
				CS$<>8__locals3.serializer = valueTuple2.Item3;
				CS$<>8__locals3.buffer = new MemoryStream();
				this.mainThreadSerializerBuffers[l] = CS$<>8__locals3.buffer;
				yield return this.SetMessage(string.Format(Localization.Get("xuiRwgSerializing", false), Localization.Get(item, false)), false, false);
				yield return ThreadManager.CoroutineWrapperWithExceptionCallback(CS$<>8__locals3.<SerializeData>g__SerializeToBuffer|3(), delegate(Exception ex)
				{
					Log.Error(string.Format("Exception while serializing '{0}': {1}", CS$<>8__locals3.fileName, ex));
				});
				k = l;
			}
			object[] lastTaskNames = null;
			Func<ValueTuple<string, string, Action<Stream>>, int, bool> <>9__4;
			for (;;)
			{
				if (!threadedSerializerTasks.Any((Task x) => !x.IsCompleted))
				{
					break;
				}
				IEnumerable<ValueTuple<string, string, Action<Stream>>> source = this.threadedSerializers;
				Func<ValueTuple<string, string, Action<Stream>>, int, bool> predicate;
				if ((predicate = <>9__4) == null)
				{
					predicate = (<>9__4 = (([TupleElementNames(new string[]
					{
						"langKey",
						"fileName",
						"serializer"
					})] ValueTuple<string, string, Action<Stream>> _, int i) => !threadedSerializerTasks[i].IsCompleted));
				}
				object[] array = (from x in source.Where(predicate).Take(3)
				select Localization.Get(x.Item1, false)).Cast<object>().ToArray<object>();
				if (lastTaskNames != null && array.SequenceEqual(lastTaskNames))
				{
					yield return null;
				}
				else
				{
					lastTaskNames = array;
					yield return this.SetMessage(string.Format(Localization.Get("xuiRwgSerializing", false), Localization.FormatListAnd(array)), false, false);
				}
			}
			long num = 0L;
			foreach (MemoryStream memoryStream in this.threadedSerializerBuffers)
			{
				num += memoryStream.Length;
			}
			foreach (MemoryStream memoryStream2 in this.mainThreadSerializerBuffers)
			{
				num += memoryStream2.Length;
			}
			this.serializedTotalSize = num;
			Log.Out(string.Format("RWG SerializeData {0} in {1:F3} s", this.serializedTotalSize.FormatSize(true), totalMs.Elapsed.TotalSeconds));
			yield break;
		}

		public bool CanSaveData()
		{
			return !SdDirectory.Exists(this.WorldPath);
		}

		public IEnumerator SaveData(bool canPrompt, XUiController parentController = null, bool autoConfirm = false, Action onCancel = null, Action onDiscard = null, Action onConfirm = null)
		{
			WorldBuilder.<>c__DisplayClass92_0 CS$<>8__locals1 = new WorldBuilder.<>c__DisplayClass92_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.CanSaveData())
			{
				if (canPrompt)
				{
					XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(this.SerializedSize, this.WorldPath, parentController, autoConfirm, onCancel != null, onDiscard != null, null, "xuiRwgSaveWorld", null, null, "xuiSave", null);
					while (confirmationWindow.IsOpen)
					{
						yield return null;
					}
					switch (confirmationWindow.Result)
					{
					case XUiC_SaveSpaceNeeded.ConfirmationResult.Pending:
						Log.Error("Should not be pending.");
						yield break;
					case XUiC_SaveSpaceNeeded.ConfirmationResult.Cancelled:
						if (onCancel != null)
						{
							onCancel();
						}
						yield break;
					case XUiC_SaveSpaceNeeded.ConfirmationResult.Discarded:
						if (onDiscard != null)
						{
							onDiscard();
						}
						yield break;
					case XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed:
						if (onConfirm != null)
						{
							onConfirm();
						}
						confirmationWindow = null;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				this.totalMS.ResetAndRestart();
				SdDirectory.CreateDirectory(this.WorldPath);
				CS$<>8__locals1.threadedSaveTasks = new Task[this.threadedSerializers.Length];
				for (int j = 0; j < this.threadedSerializers.Length; j++)
				{
					WorldBuilder.<>c__DisplayClass92_1 CS$<>8__locals2 = new WorldBuilder.<>c__DisplayClass92_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.buffer = this.threadedSerializerBuffers[j];
					ValueTuple<string, string, Action<Stream>> valueTuple = this.threadedSerializers[j];
					CS$<>8__locals2.fileName = valueTuple.Item2;
					CS$<>8__locals2.serializer = valueTuple.Item3;
					Task task = new Task(new Action(CS$<>8__locals2.<SaveData>g__SaveToFile|1));
					CS$<>8__locals2.CS$<>8__locals1.threadedSaveTasks[j] = task;
					task.Start();
				}
				int num;
				for (int k = 0; k < this.mainThreadSerializers.Length; k = num + 1)
				{
					WorldBuilder.<>c__DisplayClass92_2 CS$<>8__locals3 = new WorldBuilder.<>c__DisplayClass92_2();
					CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals1;
					CS$<>8__locals3.buffer = this.mainThreadSerializerBuffers[k];
					ValueTuple<string, string, Func<Stream, IEnumerator>> valueTuple2 = this.mainThreadSerializers[k];
					string item = valueTuple2.Item1;
					CS$<>8__locals3.fileName = valueTuple2.Item2;
					CS$<>8__locals3.serializer = valueTuple2.Item3;
					yield return this.SetMessage(string.Format(Localization.Get("xuiRwgSaving", false), Localization.Get(item, false)), false, false);
					yield return ThreadManager.CoroutineWrapperWithExceptionCallback(CS$<>8__locals3.<SaveData>g__SerializeToFile|3(), delegate(Exception ex)
					{
						Log.Error(string.Format("Exception while saving '{0}': {1}", CS$<>8__locals3.fileName, ex));
					});
					num = k;
				}
				object[] lastTaskNames = null;
				for (;;)
				{
					if (!CS$<>8__locals1.threadedSaveTasks.Any((Task x) => !x.IsCompleted))
					{
						break;
					}
					IEnumerable<ValueTuple<string, string, Action<Stream>>> source = this.threadedSerializers;
					Func<ValueTuple<string, string, Action<Stream>>, int, bool> predicate;
					if ((predicate = CS$<>8__locals1.<>9__4) == null)
					{
						predicate = (CS$<>8__locals1.<>9__4 = (([TupleElementNames(new string[]
						{
							"langKey",
							"fileName",
							"serializer"
						})] ValueTuple<string, string, Action<Stream>> _, int i) => !CS$<>8__locals1.threadedSaveTasks[i].IsCompleted));
					}
					object[] array = (from x in source.Where(predicate).Take(3)
					select Localization.Get(x.Item1, false)).Cast<object>().ToArray<object>();
					if (lastTaskNames != null && array.SequenceEqual(lastTaskNames))
					{
						yield return null;
					}
					else
					{
						lastTaskNames = array;
						yield return this.SetMessage(string.Format(Localization.Get("xuiRwgSaving", false), Localization.FormatListAnd(array)), false, false);
					}
				}
				yield return this.SetMessage(Localization.Get("xuiDmCommitting", false), false, false);
				yield return SaveDataUtils.SaveDataManager.CommitCoroutine();
				SaveInfoProvider.Instance.ClearResources();
				yield return this.SetMessage(null, false, false);
				Log.Out(string.Format("RWG SaveData in {0:F3} s", this.totalMS.Elapsed.TotalSeconds));
				yield break;
			}
			if (!canPrompt)
			{
				yield break;
			}
			if (onCancel != null)
			{
				onCancel();
			}
			else if (onDiscard != null)
			{
				onDiscard();
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<StreetTile> CalcPlayerSpawnTiles()
		{
			List<StreetTile> list = (from StreetTile st in this.StreetTileMap
			where !st.OverlapsRadiation && !st.AllIsWater && st.Township == null && (st.District == null || st.District.name == "wilderness") && (this.ForestBiomeWeight == 0 || st.BiomeType == BiomeType.forest) && !st.Used
			select st).ToList<StreetTile>();
			list.Sort((StreetTile _t1, StreetTile _t2) => this.CalcClosestTraderDistance(_t1).CompareTo(this.CalcClosestTraderDistance(_t2)));
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float CalcClosestTraderDistance(StreetTile _st)
		{
			float num = float.MaxValue;
			foreach (Vector2i b in this.TraderCenterPositions)
			{
				float num2 = Vector2i.Distance(_st.WorldPositionCenter, b);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<StreetTile> getWildernessTilesToSmooth()
		{
			return (from StreetTile st in this.StreetTileMap
			where st.NeedsWildernessSmoothing
			select st).ToList<StreetTile>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initStreetTiles()
		{
			this.StreetTileMapSize = this.WorldSize / 150;
			this.StreetTileMap = new StreetTile[this.StreetTileMapSize, this.StreetTileMapSize];
			for (int i = 0; i < this.StreetTileMapSize; i++)
			{
				for (int j = 0; j < this.StreetTileMapSize; j++)
				{
					this.StreetTileMap[i, j] = new StreetTile(this, new Vector2i(i, j));
				}
			}
		}

		public void CleanupGeneratedData()
		{
			this.dest = null;
			this.WaterMap = null;
			this.terrainDest = null;
			this.terrainWaterDest = null;
			this.waterDest = null;
			this.biomeDest = null;
			this.radDest = null;
			this.Townships.Clear();
			if (this.PathingGrid != null)
			{
				Array.Clear(this.PathingGrid, 0, this.PathingGrid.Length);
				this.PathingGrid = null;
			}
			this.PrefabManager.Clear();
			this.PathingUtils.Cleanup();
			Rand.Instance.Cleanup();
		}

		public void Cleanup()
		{
			this.serializedTotalSize = 0L;
			Span<MemoryStream> span = this.mainThreadSerializerBuffers.AsSpan<MemoryStream>();
			for (int i = 0; i < span.Length; i++)
			{
				ref MemoryStream ptr = ref span[i];
				MemoryStream memoryStream = ptr;
				if (memoryStream != null)
				{
					memoryStream.Dispose();
				}
				ptr = null;
			}
			span = this.threadedSerializerBuffers.AsSpan<MemoryStream>();
			for (int i = 0; i < span.Length; i++)
			{
				ref MemoryStream ptr2 = ref span[i];
				MemoryStream memoryStream2 = ptr2;
				if (memoryStream2 != null)
				{
					memoryStream2.Dispose();
				}
				ptr2 = null;
			}
			this.CleanupGeneratedData();
			this.PrefabManager.Cleanup();
			this.StampManager.ClearStamps();
			this.HeightMap = null;
			if (this.PreviewImage)
			{
				UnityEngine.Object.Destroy(this.PreviewImage);
			}
			GCUtils.UnloadAndCollectStart();
			this.IsFinished = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator generateTerrain()
		{
			int num = this.WorldSize * this.WorldSize;
			this.HeightMap = new float[num];
			this.WaterMap = new byte[num];
			this.terrainDest = new float[num];
			this.terrainWaterDest = new float[num];
			this.BiomeSize = this.WorldSize / 8;
			this.biomeDest = new Color32[this.BiomeSize * this.BiomeSize];
			this.radDest = new Color32[num];
			this.waterDest = new float[num];
			this.GenWaterBorderN = (Rand.Instance.Float() > 0.5f);
			this.GenWaterBorderS = (Rand.Instance.Float() > 0.5f);
			this.GenWaterBorderW = (Rand.Instance.Float() > 0.5f);
			this.GenWaterBorderE = (Rand.Instance.Float() > 0.5f);
			Log.Out("generateBiomeTiles start at {0}, r={1:x}", new object[]
			{
				(float)this.totalMS.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			this.GenerateBiomeTiles();
			yield return null;
			if (this.IsCanceled)
			{
				yield break;
			}
			Log.Out("GenerateTerrainTiles start at {0}, r={1:x}", new object[]
			{
				(float)this.totalMS.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			this.GenerateTerrainTiles();
			yield return null;
			if (this.IsCanceled)
			{
				yield break;
			}
			Log.Out("generateBaseStamps start at {0}, r={1:x}", new object[]
			{
				(float)this.totalMS.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			yield return this.generateBaseStamps();
			if (this.IsCanceled)
			{
				yield break;
			}
			yield return this.GenerateTerrainFromTiles(TerrainType.plains, 1024);
			if (this.IsCanceled)
			{
				yield break;
			}
			yield return this.GenerateTerrainFromTiles(TerrainType.hills, 512);
			if (this.IsCanceled)
			{
				yield break;
			}
			yield return this.GenerateTerrainFromTiles(TerrainType.mountains, 256);
			if (this.IsCanceled)
			{
				yield break;
			}
			Log.Out("writeStampsToMaps start at {0}, r={1:x}", new object[]
			{
				(float)this.totalMS.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			yield return this.writeStampsToMaps();
			yield return this.SetMessage(Localization.Get("xuiRwgTerrainGenerationFinished", false), false, false);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator generateBaseStamps()
		{
			this.StampManager.GetStamp("ground", null);
			float num = 0.13333334f;
			for (int i = 0; i < this.terrainDest.Length; i++)
			{
				this.terrainDest[i] = num;
				this.terrainWaterDest[i] = num;
			}
			Vector2 sizeMinMax = new Vector2(1.5f, 3.5f);
			this.thisWorldProperties.ParseVec("border.scale", ref sizeMinMax);
			int borderStep = 512;
			this.thisWorldProperties.ParseInt("border_step_distance", ref borderStep);
			Task terrainBorderTask = new Task(delegate()
			{
				MicroStopwatch microStopwatch = new MicroStopwatch(true);
				new MicroStopwatch(false);
				new MicroStopwatch(false);
				Rand rand = new Rand(this.Seed + 1);
				int borderStep = borderStep;
				int num2 = borderStep / 2;
				int num3 = 0;
				while (num3 < this.WorldSize + borderStep && !this.IsCanceled)
				{
					if (!this.GenWaterBorderE || !this.GenWaterBorderW || !this.GenWaterBorderN || !this.GenWaterBorderS)
					{
						for (int k = 0; k < 4; k++)
						{
							TranslationData translationData = null;
							if (k == 0 && !this.GenWaterBorderS)
							{
								translationData = new TranslationData(num3 + rand.Range(0, num2), rand.Range(0, num2), rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							else if (k == 1 && !this.GenWaterBorderN)
							{
								translationData = new TranslationData(num3 + rand.Range(0, num2), this.WorldSize - rand.Range(0, num2), rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							else if (k == 2 && !this.GenWaterBorderW)
							{
								translationData = new TranslationData(rand.Range(0, num2), num3 + rand.Range(0, num2), rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							else if (k == 3 && !this.GenWaterBorderE)
							{
								translationData = new TranslationData(this.WorldSize - rand.Range(0, num2), num3 + rand.Range(0, num2), rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							if (translationData != null)
							{
								string str = this.biomeMap.data[Mathf.Clamp(translationData.x / 1024, 0, this.WorldSize / 1024 - 1), Mathf.Clamp(translationData.y / 1024, 0, this.WorldSize / 1024 - 1)].ToString();
								RawStamp stamp;
								if (this.StampManager.TryGetStamp(str + "_land_border", out stamp, rand) || this.StampManager.TryGetStamp("land_border", out stamp, rand))
								{
									this.StampManager.DrawStamp(this.terrainDest, this.terrainWaterDest, new Stamp(this, stamp, translationData, false, default(Color), 0.1f, false, ""));
								}
							}
						}
					}
					if (this.GenWaterBorderE || this.GenWaterBorderW || this.GenWaterBorderN || this.GenWaterBorderS)
					{
						for (int l = 0; l < 4; l++)
						{
							TranslationData translationData2 = null;
							if (l == 0 && this.GenWaterBorderS)
							{
								translationData2 = new TranslationData(num3, rand.Range(0, num2 / 2), rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							else if (l == 1 && this.GenWaterBorderN)
							{
								translationData2 = new TranslationData(num3, this.WorldSize - rand.Range(0, num2 / 2), rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							else if (l == 2 && this.GenWaterBorderW)
							{
								translationData2 = new TranslationData(rand.Range(0, num2 / 2), num3, rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							else if (l == 3 && this.GenWaterBorderE)
							{
								translationData2 = new TranslationData(this.WorldSize - rand.Range(0, num2 / 2), num3, rand.Range(sizeMinMax.x, sizeMinMax.y), rand.Angle());
							}
							if (translationData2 != null)
							{
								string str2 = this.biomeMap.data[Mathf.Clamp(translationData2.x / 1024, 0, this.WorldSize / 1024 - 1), Mathf.Clamp(translationData2.y / 1024, 0, this.WorldSize / 1024 - 1)].ToString();
								RawStamp stamp2;
								if (this.StampManager.TryGetStamp(str2 + "_water_border", out stamp2, rand) || this.StampManager.TryGetStamp("water_border", out stamp2, rand))
								{
									this.StampManager.DrawStamp(this.terrainDest, this.terrainWaterDest, new Stamp(this, stamp2, translationData2, false, default(Color), 0.1f, false, ""));
									Stamp stamp3 = new Stamp(this, stamp2, translationData2, true, new Color32(0, 0, (byte)this.WaterHeight, 0), 0.1f, true, "");
									this.waterLayer.Stamps.Add(stamp3);
									StampManager.DrawWaterStamp(stamp3, this.waterDest, this.WorldSize);
								}
							}
						}
					}
					num3 += borderStep;
				}
				rand.Free();
				Log.Out("generateBaseStamps terrainBorderThread in {0}", new object[]
				{
					(float)microStopwatch.ElapsedMilliseconds * 0.001f
				});
			});
			terrainBorderTask.Start();
			Task radTask = new Task(delegate()
			{
				MicroStopwatch microStopwatch = new MicroStopwatch(true);
				Color c = new Color(1f, 0f, 0f, 0f);
				int num2 = this.WorldSize - 1;
				for (int k = 0; k < this.WorldSize; k++)
				{
					this.radDest[k] = c;
					this.radDest[k + num2 * this.WorldSize] = c;
					this.radDest[k * this.WorldSize] = c;
					this.radDest[k * this.WorldSize + num2] = c;
				}
				Log.Out("generateBaseStamps radThread in {0}", new object[]
				{
					(float)microStopwatch.ElapsedMilliseconds * 0.001f
				});
			});
			radTask.Start();
			Task[] biomeTasks = new Task[]
			{
				new Task(delegate()
				{
					MicroStopwatch microStopwatch = new MicroStopwatch(true);
					Rand rand = new Rand(this.Seed + 3);
					Color32 color = this.biomeColors[BiomeType.forest];
					for (int k = 0; k < this.biomeDest.Length; k++)
					{
						this.biomeDest[k] = color;
					}
					RawStamp stamp = this.StampManager.GetStamp("filler_biome", rand);
					if (stamp != null)
					{
						int num2 = this.WorldSize / 256;
						int num3 = 32;
						int num4 = num3 / 2;
						float num5 = (float)num3 / (float)stamp.width * 1.5f;
						for (int l = 0; l < num2; l++)
						{
							int num6 = l * 256 / 8;
							for (int m = 0; m < num2; m++)
							{
								int num7 = m * 256 / 8;
								BiomeType biomeType = this.biomeMap.data[m, l];
								if (biomeType != BiomeType.none)
								{
									float scale = num5 + rand.Range(0f, 0.2f);
									float angle = (float)(rand.Range(0, 4) * 90 + rand.Range(-20, 20));
									StampManager.DrawBiomeStamp(this.biomeDest, stamp.alphaPixels, num7 + num4, num6 + num4, this.BiomeSize, this.BiomeSize, stamp.width, stamp.height, scale, this.biomeColors[biomeType], 0.1f, angle);
								}
							}
						}
					}
					rand.Free();
					Log.Out("generateBaseStamps biomeThreads in {0}", new object[]
					{
						(float)microStopwatch.ElapsedMilliseconds * 0.001f
					});
				})
			};
			Task[] array = biomeTasks;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Start();
			}
			bool isAnyAlive = true;
			while (isAnyAlive || !terrainBorderTask.IsCompleted || !radTask.IsCompleted)
			{
				isAnyAlive = false;
				foreach (Task task in biomeTasks)
				{
					isAnyAlive |= !task.IsCompleted;
				}
				if (!terrainBorderTask.IsCompleted && isAnyAlive)
				{
					yield return this.SetMessage(Localization.Get("xuiRwgCreatingTerrainAndBiomeStamps", false), false, false);
				}
				else if (!terrainBorderTask.IsCompleted && !isAnyAlive)
				{
					yield return this.SetMessage(Localization.Get("xuiRwgCreatingTerrainStamps", false), false, false);
				}
				else
				{
					yield return this.SetMessage(Localization.Get("xuiRwgCreatingBiomeStamps", false), false, false);
				}
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GenerateTerrainFromTiles(TerrainType _terrainType, int _tileSize)
		{
			Log.Out("GenerateTerrainFromTiles {0}, start at {1}, r={2:x}", new object[]
			{
				_terrainType,
				(float)this.totalMS.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			int widthInTiles = this.WorldSize / 256;
			int t = 0;
			string terrainTypeName = _terrainType.ToStringCached<TerrainType>();
			int step = _tileSize / 256;
			for (int tileX = 0; tileX < widthInTiles; tileX += step)
			{
				for (int tileY = 0; tileY < widthInTiles; tileY += step)
				{
					if (this.IsMessageElapsed())
					{
						yield return this.SetMessage(string.Format(Localization.Get("xuiRwgGeneratingTerrain", false), Mathf.FloorToInt(100f * ((float)t / (float)(widthInTiles * widthInTiles)))), false, false);
					}
					int num = t;
					t = num + 1;
					bool flag = true;
					for (int i = 0; i < step; i++)
					{
						for (int j = 0; j < step; j++)
						{
							if (this.terrainTypeMap.data[tileX + i, tileY + j] == _terrainType)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					if (!flag)
					{
						BiomeType biomeType = this.biomeMap.data[tileX, tileY];
						if (biomeType == BiomeType.none)
						{
							biomeType = BiomeType.forest;
						}
						if (_terrainType == TerrainType.mountains && biomeType == BiomeType.wasteland)
						{
							this.terrainTypeMap.data[tileX, tileY] = TerrainType.plains;
						}
						else
						{
							int num2 = tileX * 256 + _tileSize / 2;
							int num3 = tileY * 256 + _tileSize / 2;
							string text = biomeType.ToStringCached<BiomeType>();
							string comboTypeName = string.Format("{0}_{1}", text, terrainTypeName);
							Vector2 vector;
							int num4;
							int num5;
							float num6;
							bool flag2;
							float biomeAlphaCutoff;
							this.GetTerrainProperties(text, terrainTypeName, comboTypeName, out vector, out num4, out num5, out num6, out flag2, out biomeAlphaCutoff);
							vector *= (float)step;
							num5 *= step;
							int num7 = 0;
							float alpha = num6;
							bool additive = false;
							for (int k = 0; k < num4; k++)
							{
								RawStamp rawStamp;
								if (this.StampManager.TryGetStamp(terrainTypeName, comboTypeName, out rawStamp))
								{
									Vector2 vector2 = Rand.Instance.RandomOnUnitCircle() * (float)num7;
									TranslationData translationData = new TranslationData(num2 + Mathf.RoundToInt(vector2.x), num3 + Mathf.RoundToInt(vector2.y), vector.x, vector.y, -1);
									RawStamp stamp = rawStamp;
									TranslationData transData = translationData;
									bool isCustomColor = false;
									string name = rawStamp.name;
									Stamp stamp2 = new Stamp(this, stamp, transData, isCustomColor, default(Color), 0.1f, false, name);
									stamp2.alpha = alpha;
									stamp2.additive = additive;
									this.terrainLayer.Stamps.Add(stamp2);
									if (rawStamp.hasWater)
									{
										this.waterLayer.Stamps.Add(new Stamp(this, rawStamp, translationData, false, default(Color), 0.1f, true, ""));
									}
									if (flag2)
									{
										this.biomeLayer.Stamps.Add(new Stamp(this, rawStamp, translationData, true, this.biomeColors[biomeType], biomeAlphaCutoff, false, ""));
									}
									num7 = num5;
									alpha = num6 * 0.45f;
									additive = true;
								}
							}
						}
					}
				}
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator writeStampsToMaps()
		{
			Task terraTask = new Task(delegate()
			{
				MicroStopwatch microStopwatch = new MicroStopwatch(true);
				for (int i = 0; i < this.terrainDest.Length; i++)
				{
					this.SetHeight(i, this.terrainDest[i] * 255f);
				}
				Log.Out("writeStampsToMaps terrain in {0}", new object[]
				{
					(float)microStopwatch.ElapsedMilliseconds * 0.001f
				});
			});
			Task biomeTask = new Task(delegate()
			{
				MicroStopwatch microStopwatch = new MicroStopwatch(true);
				this.StampManager.DrawStampGroup(this.biomeLayer, this.biomeDest, this.BiomeSize, 0.125f);
				this.biomeLayer.Stamps.Clear();
				Log.Out("writeStampsToMaps biome in {0}", new object[]
				{
					(float)microStopwatch.ElapsedMilliseconds * 0.001f
				});
			});
			Task radnwatTask = new Task(delegate()
			{
				MicroStopwatch microStopwatch = new MicroStopwatch(true);
				this.StampManager.DrawStampGroup(this.radiationLayer, this.radDest, this.WorldSize, 1f);
				Log.Out("writeStampsToMaps rad in {0}", new object[]
				{
					(float)microStopwatch.ElapsedMilliseconds * 0.001f
				});
				microStopwatch.ResetAndRestart();
				for (int i = 0; i < this.waterLayer.Stamps.Count; i++)
				{
					Stamp stamp = this.waterLayer.Stamps[i];
					int num = Utils.FastMax(Mathf.FloorToInt(stamp.Area.min.x), 0);
					int num2 = Utils.FastMin(Mathf.FloorToInt(stamp.Area.max.x), this.WorldSize - 1);
					int num3 = Utils.FastMax(Mathf.FloorToInt(stamp.Area.min.y), 0);
					int num4 = Utils.FastMin(Mathf.FloorToInt(stamp.Area.max.y), this.WorldSize - 1);
					for (int j = num3; j <= num4; j++)
					{
						for (int k = num; k <= num2; k++)
						{
							int num5 = k + j * this.WorldSize;
							float num6 = this.waterDest[num5] * 255f;
							if (this.terrainWaterDest[num5] * 255f - 0.5f > num6)
							{
								this.waterDest[num5] = 0f;
								num6 = 0f;
							}
							this.WaterMap[num5] = (byte)num6;
						}
					}
				}
				this.waterLayer.Stamps.Clear();
				Log.Out("writeStampsToMaps WaterMap in {0}", new object[]
				{
					(float)microStopwatch.ElapsedMilliseconds * 0.001f
				});
			});
			terraTask.Start();
			biomeTask.Start();
			radnwatTask.Start();
			while (!terraTask.IsCompleted || !biomeTask.IsCompleted || !radnwatTask.IsCompleted)
			{
				yield return this.SetMessage(Localization.Get("xuiRwgWritingStampsToMap", false), false, false);
			}
			Log.Out("writeStampsToMaps end at {0}, r={1:x}", new object[]
			{
				(float)this.totalMS.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GenerateTerrainLast()
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			this.StampManager.GetStamp("base", null);
			if (this.Lakes > WorldBuilder.GenerationSelections.None)
			{
				this.generateTerrainFeature("lake", this.Lakes, true);
			}
			if (this.Rivers > WorldBuilder.GenerationSelections.None)
			{
				this.generateTerrainFeature("river", this.Rivers, true);
			}
			if (this.Canyons > WorldBuilder.GenerationSelections.None)
			{
				this.generateTerrainFeature("canyon", this.Canyons, false);
			}
			if (this.Craters > WorldBuilder.GenerationSelections.None)
			{
				this.generateTerrainFeature("crater", this.Craters, false);
			}
			MicroStopwatch ms2 = new MicroStopwatch(true);
			Task terraTask = new Task(delegate()
			{
				this.StampManager.DrawStampGroup(this.lowerLayer, this.terrainDest, this.terrainWaterDest, this.WorldSize);
				this.StampManager.DrawStampGroup(this.terrainLayer, this.terrainDest, this.terrainWaterDest, this.WorldSize);
				for (int i = 0; i < this.terrainDest.Length; i++)
				{
					this.SetHeight(i, Utils.FastMax(this.terrainDest[i] * 255f, 2f));
				}
			});
			Task waterTask = new Task(delegate()
			{
				this.StampManager.DrawWaterStampGroup(this.waterLayer, this.waterDest, this.WorldSize);
			});
			terraTask.Start();
			waterTask.Start();
			while (!terraTask.IsCompleted || !waterTask.IsCompleted)
			{
				if (!terraTask.IsCompleted && waterTask.IsCompleted)
				{
					yield return this.SetMessage(Localization.Get("xuiRwgWritingTerrainStampsToMap", false), false, false);
				}
				else if (terraTask.IsCompleted && !waterTask.IsCompleted)
				{
					yield return this.SetMessage(Localization.Get("xuiRwgWritingWaterStampsToMap", false), false, false);
				}
				else
				{
					yield return this.SetMessage(Localization.Get("xuiRwgWritingTerrainAndWaterStampsToMap", false), false, false);
				}
			}
			ms2.ResetAndRestart();
			Task waterMapTask = new Task(delegate()
			{
				for (int i = 0; i < this.waterLayer.Stamps.Count; i++)
				{
					Stamp stamp = this.waterLayer.Stamps[i];
					int num = Utils.FastMax(Mathf.FloorToInt(stamp.Area.min.x), 0);
					int num2 = Utils.FastMin(Mathf.FloorToInt(stamp.Area.max.x), this.WorldSize - 1);
					int num3 = Utils.FastMax(Mathf.FloorToInt(stamp.Area.min.y), 0);
					int num4 = Utils.FastMin(Mathf.FloorToInt(stamp.Area.max.y), this.WorldSize - 1);
					for (int j = num3; j <= num4; j++)
					{
						for (int k = num; k <= num2; k++)
						{
							int num5 = k + j * this.WorldSize;
							float num6 = this.waterDest[num5] * 255f;
							if (this.terrainDest[num5] * 255f - 0.5f > num6)
							{
								this.waterDest[num5] = 0f;
								num6 = 0f;
							}
							this.WaterMap[num5] = (byte)num6;
						}
					}
				}
			});
			waterMapTask.Start();
			while (!waterMapTask.IsCompleted)
			{
				yield return this.SetMessage(Localization.Get("xuiRwgCleaningUpWaterMapData", false), false, false);
			}
			Log.Out("GenerateTerrainLast done in {0}, r={1:x}", new object[]
			{
				(float)ms.ElapsedMilliseconds * 0.001f,
				Rand.Instance.PeekSample()
			});
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator FinalizeWater()
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			MicroStopwatch msReset = new MicroStopwatch(true);
			int num6;
			for (int i = 0; i < this.waterRects.Count; i = num6 + 1)
			{
				int num = Utils.FastMax(Mathf.FloorToInt(this.waterRects[i].min.x), 0);
				int num2 = Utils.FastMin(Mathf.FloorToInt(this.waterRects[i].max.x), this.WorldSize - 1);
				int num3 = Utils.FastMax(Mathf.FloorToInt(this.waterRects[i].min.y), 0);
				int num4 = Utils.FastMin(Mathf.FloorToInt(this.waterRects[i].max.y), this.WorldSize - 1);
				for (int j = num3; j <= num4; j++)
				{
					for (int k = num; k <= num2; k++)
					{
						int num5 = k + j * this.WorldSize;
						if (this.HeightMap[num5] - 0.5f > (float)this.WaterMap[num5])
						{
							this.waterDest[num5] = 0f;
							this.WaterMap[num5] = 0;
						}
						else
						{
							this.WaterMap[num5] = (byte)this.WaterHeight;
							this.waterDest[num5] = (float)this.WaterHeight / 255f;
						}
					}
				}
				if (msReset.ElapsedMilliseconds > 500L)
				{
					yield return null;
					msReset.ResetAndRestart();
				}
				num6 = i;
			}
			Log.Out("FinalizeWater in {0}", new object[]
			{
				(float)ms.ElapsedMilliseconds * 0.001f
			});
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serializeWater(Stream stream)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			Color32[] array = new Color32[this.WorldSize * this.WorldSize];
			for (int i = 0; i < this.WorldSize; i++)
			{
				for (int j = 0; j < this.WorldSize; j++)
				{
					array[i * this.WorldSize + j] = new Color32(0, 0, (byte)(this.waterDest[i * this.WorldSize + j] * 255f), 0);
				}
			}
			Log.Out(string.Format("Create water in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f));
			stream.Write(ImageConversion.EncodeArrayToPNG(array, GraphicsFormat.R8G8B8A8_UNorm, (uint)this.WorldSize, (uint)this.WorldSize, (uint)(this.WorldSize * 4)));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void generateTerrainFeature(string featureName, WorldBuilder.GenerationSelections selection, bool isWaterFeature = false)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			Vector2 vector = new Vector2(0.5f, 1.5f);
			Vector2i vector2i = Vector2i.zero;
			Vector2i vector2i2 = Vector2i.zero;
			Vector2i vector2i3 = Vector2i.zero;
			Vector2i vector2i4 = Vector2i.zero;
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(this.Seed + featureName.GetHashCode() + 1);
			GameRandom rnd2 = GameRandomManager.Instance.CreateGameRandom(this.Seed + featureName.GetHashCode() + 2);
			string @string;
			if ((@string = this.thisWorldProperties.GetString(featureName + "s.scale")) != string.Empty)
			{
				vector = StringParsers.ParseVector2(@string);
			}
			int count = this.GetCount(featureName + "s", selection);
			Func<StreetTile, int> <>9__1;
			for (int i = 0; i < count; i++)
			{
				RawStamp rawStamp;
				if (!this.StampManager.TryGetStamp(featureName, out rawStamp))
				{
					if (featureName.Contains("river"))
					{
						Log.Out("Could not find stamp {0}", new object[]
						{
							featureName
						});
					}
				}
				else
				{
					float num = gameRandom.RandomRange(vector.x, vector.y) * 1.4f;
					int num2 = gameRandom.RandomRange(0, 360);
					int num3 = (int)((float)rawStamp.width * num);
					int num4 = (int)((float)rawStamp.height * num);
					int num5 = -(num3 / 2);
					int num6 = -(num4 / 2);
					vector2i = this.getRotatedPoint(num5, num6, num5 + num3 / 2, num6 + num4 / 2, num2);
					vector2i2 = this.getRotatedPoint(num5 + num3, num6, num5 + num3 / 2, num6 + num4 / 2, num2);
					vector2i3 = this.getRotatedPoint(num5, num6 + num4, num5 + num3 / 2, num6 + num4 / 2, num2);
					vector2i4 = this.getRotatedPoint(num5 + num3, num6 + num4, num5 + num3 / 2, num6 + num4 / 2, num2);
					zero.x = (float)Mathf.Min(Mathf.Min(vector2i.x, vector2i2.x), Mathf.Min(vector2i3.x, vector2i4.x));
					zero.y = (float)Mathf.Min(Mathf.Min(vector2i.y, vector2i2.y), Mathf.Min(vector2i3.y, vector2i4.y));
					zero2.x = (float)Mathf.Max(Mathf.Max(vector2i.x, vector2i2.x), Mathf.Max(vector2i3.x, vector2i4.x));
					zero2.y = (float)Mathf.Max(Mathf.Max(vector2i.y, vector2i2.y), Mathf.Max(vector2i3.y, vector2i4.y));
					Rect rect = new Rect(zero, zero2 - zero);
					IEnumerable<StreetTile> source = from StreetTile st in this.StreetTileMap
					where (st.Township == null || st.District == null || st.District.name == "wilderness") && st.TerrainType != TerrainType.mountains && !st.HasFeature && st.GetNeighborCount() > 3
					select st;
					Func<StreetTile, int> keySelector;
					if ((keySelector = <>9__1) == null)
					{
						keySelector = (<>9__1 = ((StreetTile st) => rnd2.RandomInt));
					}
					using (List<StreetTile>.Enumerator enumerator = source.OrderBy(keySelector).ToList<StreetTile>().GetEnumerator())
					{
						IL_6A7:
						while (enumerator.MoveNext())
						{
							StreetTile streetTile = enumerator.Current;
							if (streetTile.GridPosition.x != 0 && streetTile.GridPosition.y != 0)
							{
								int num7 = streetTile.WorldPositionCenter.x - (int)rect.width / 2;
								while ((float)num7 < (float)streetTile.WorldPositionCenter.x + rect.width / 2f)
								{
									int num8 = streetTile.WorldPositionCenter.y - (int)rect.height / 2;
									while ((float)num8 < (float)streetTile.WorldPositionCenter.y + rect.height / 2f)
									{
										StreetTile streetTileWorld = this.GetStreetTileWorld(num7, num8);
										if (streetTileWorld == null || streetTileWorld.Township != null || streetTileWorld.District != null || streetTileWorld.Used || streetTileWorld.HasFeature)
										{
											goto IL_6A7;
										}
										num8 += 150;
									}
									num7 += 150;
								}
								int num9 = streetTile.WorldPositionCenter.x - (int)rect.width / 2;
								while ((float)num9 < (float)streetTile.WorldPositionCenter.x + rect.width / 2f)
								{
									int num10 = streetTile.WorldPositionCenter.y - (int)rect.height / 2;
									while ((float)num10 < (float)streetTile.WorldPositionCenter.y + rect.height / 2f)
									{
										this.GetStreetTileWorld(num9, num10).HasFeature = true;
										num10 += 150;
									}
									num9 += 150;
								}
								TranslationData transData = new TranslationData(streetTile.WorldPositionCenter.x, streetTile.WorldPositionCenter.y, num, num2);
								Stamp stamp = new Stamp(this, rawStamp, transData, false, default(Color), 0.1f, false, "");
								if (!isWaterFeature)
								{
									this.lowerLayer.Stamps.Add(stamp);
									bool flag = true;
									for (int j = 0; j < this.waterLayer.Stamps.Count; j++)
									{
										if (stamp.Area.Overlaps(this.waterLayer.Stamps[j].Area))
										{
											flag = false;
											break;
										}
									}
									if (flag)
									{
										for (int k = 0; k < this.waterRects.Count; k++)
										{
											if (stamp.Area.Overlaps(this.waterRects[k]))
											{
												flag = false;
												break;
											}
										}
									}
									if (!flag)
									{
										this.waterLayer.Stamps.Add(new Stamp(this, rawStamp, transData, true, new Color32(0, 0, (byte)this.WaterHeight, 0), 0.05f, true, ""));
									}
									break;
								}
								bool flag2 = false;
								for (int l = 0; l < this.terrainLayer.Stamps.Count; l++)
								{
									if (stamp.Name.Contains("mountain") && stamp.Area.Overlaps(this.terrainLayer.Stamps[l].Area))
									{
										flag2 = true;
										break;
									}
								}
								if (!flag2)
								{
									this.lowerLayer.Stamps.Add(stamp);
									this.waterLayer.Stamps.Add(new Stamp(this, rawStamp, transData, true, new Color32(0, 0, (byte)this.WaterHeight, 0), 0.1f, true, ""));
									break;
								}
								i--;
							}
						}
					}
				}
			}
			GameRandomManager.Instance.FreeGameRandom(rnd2);
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			Log.Out("generateTerrainFeature {0} in {1}", new object[]
			{
				featureName,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		public bool CreatePlayerSpawn(Vector2i worldPos, bool _isFallback = false)
		{
			Vector3 position = new Vector3((float)worldPos.x, this.GetHeight(worldPos), (float)worldPos.y);
			if (!_isFallback)
			{
				for (int i = 0; i < this.playerSpawns.Count; i++)
				{
					if (this.playerSpawns[i].IsTooClose(position))
					{
						return false;
					}
				}
				StreetTile streetTileWorld = this.GetStreetTileWorld(worldPos);
				if (streetTileWorld != null && streetTileWorld.HasPrefabs)
				{
					if (this.ForestBiomeWeight > 0 && streetTileWorld.BiomeType != BiomeType.forest)
					{
						return false;
					}
					using (List<PrefabDataInstance>.Enumerator enumerator = streetTileWorld.StreetTilePrefabDatas.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.prefab.DifficultyTier >= 2)
							{
								return false;
							}
						}
					}
				}
				List<Vector2i> list = (this.ForestBiomeWeight > 0) ? this.TraderForestCenterPositions : this.TraderCenterPositions;
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (Vector2i.DistanceSqr(list[j], worldPos) < 810000f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			WorldBuilder.PlayerSpawn item = new WorldBuilder.PlayerSpawn(position, (float)Rand.Instance.Range(0, 360));
			this.playerSpawns.Add(item);
			return true;
		}

		public IEnumerator smoothRoadTerrain(Color32[] roadMask, float[] HeightMap, int WorldSize, List<Township> _townships = null)
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			int len = WorldSize * WorldSize;
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < len; i++)
			{
				if (roadMask[i].r > 0)
				{
					num++;
					num2 += HeightMap[i];
				}
			}
			num2 /= (float)num + 0.0001f;
			byte[] mask = new byte[len];
			for (int j = 0; j < WorldSize; j++)
			{
				for (int k = 0; k < WorldSize; k++)
				{
					int num3 = k + j * WorldSize;
					int r = (int)roadMask[num3].r;
					if (r + (int)roadMask[num3].g > 0)
					{
						HeightMap[num3] += 0.0008f;
						mask[num3] = 200;
						int num4 = 80;
						int num5 = 3;
						int num6 = 30;
						if (r > 0)
						{
							mask[num3] = byte.MaxValue;
							num4 = 60;
							num5 = 6;
							num6 = 8;
						}
						for (int l = 1; l <= num5; l++)
						{
							for (int m = 0; m < 8; m++)
							{
								int num7 = k + this.directions8way[m].x * l;
								if ((ulong)num7 < (ulong)((long)WorldSize))
								{
									int num8 = j + this.directions8way[m].y * l;
									if ((ulong)num8 < (ulong)((long)WorldSize))
									{
										int num9 = num7 + num8 * WorldSize;
										if (num4 > (int)mask[num9])
										{
											mask[num9] = (byte)num4;
										}
									}
								}
							}
							num4 -= num6;
						}
					}
				}
			}
			yield return null;
			yield return null;
			int messageCnt = 0;
			int clampX = WorldSize - 1;
			int clampY = WorldSize - 1;
			float[] heights = new float[len];
			int highwayPasses = 6;
			for (;;)
			{
				int num10 = highwayPasses;
				highwayPasses = num10 - 1;
				if (num10 <= 0)
				{
					break;
				}
				Array.Copy(HeightMap, heights, len);
				for (int n = 1; n < clampY; n++)
				{
					int num11 = n * WorldSize;
					for (int num12 = 1; num12 < clampX; num12++)
					{
						int num13 = num12 + num11;
						if (roadMask[num13].r != 0)
						{
							float num15;
							float num14 = num15 = heights[num13];
							int num16 = num13 - WorldSize - 1;
							float num17 = num14;
							if (mask[num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.25f;
							num17 = num14;
							if (mask[++num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.5f;
							num17 = num14;
							if (mask[++num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.25f;
							num16 = num13 - 1;
							num17 = num14;
							if (mask[num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.5f;
							num16 += 2;
							num17 = num14;
							if (mask[num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.5f;
							num16 = num13 + WorldSize - 1;
							num17 = num14;
							if (mask[num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.25f;
							num17 = num14;
							if (mask[++num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.5f;
							num17 = num14;
							if (mask[++num16] >= 255)
							{
								num17 = heights[num16];
							}
							num15 += num17 * 0.25f;
							HeightMap[num13] = num15 / 4f;
						}
					}
				}
				if (this.IsMessageElapsed())
				{
					string format = Localization.Get("xuiRwgSmoothRoadTerrainCount", false);
					num10 = messageCnt + 1;
					messageCnt = num10;
					yield return this.SetMessage(string.Format(format, num10), false, false);
				}
			}
			messageCnt = 100;
			int roadAndAdjacentPasses = 30;
			for (;;)
			{
				int num10 = roadAndAdjacentPasses;
				roadAndAdjacentPasses = num10 - 1;
				if (num10 <= 0)
				{
					break;
				}
				Array.Copy(HeightMap, heights, len);
				for (int num18 = 1; num18 < clampY; num18++)
				{
					int num19 = num18 * WorldSize;
					for (int num20 = 1; num20 < clampX; num20++)
					{
						int num21 = num20 + num19;
						int num22 = (int)mask[num21];
						if (num22 != 0 && roadMask[num21].r <= 0)
						{
							int num23 = 0;
							float num24 = 0f;
							int num25 = num21 - WorldSize - 1;
							int num26 = (int)(mask[num25] / 2);
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num26 = (int)mask[++num25];
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num26 = (int)(mask[++num25] / 2);
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num25 = num21 - 1;
							num26 = (int)mask[num25];
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num25 = num21 + 1;
							num26 = (int)mask[num25];
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num25 = num21 + WorldSize - 1;
							num26 = (int)(mask[num25] / 2);
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num26 = (int)mask[++num25];
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							num26 = (int)(mask[++num25] / 2);
							num24 += heights[num25] * (float)num26;
							num23 += num26;
							if (num23 > 0)
							{
								if (num22 < 200)
								{
									float num27 = (float)num22 * 0.005f;
									HeightMap[num21] = HeightMap[num21] * (1f - num27) + num24 / (float)num23 * num27;
								}
								else
								{
									HeightMap[num21] = num24 / (float)num23;
								}
							}
						}
					}
				}
				if (this.IsMessageElapsed())
				{
					string format2 = Localization.Get("xuiRwgSmoothRoadTerrainCount", false);
					num10 = messageCnt + 1;
					messageCnt = num10;
					yield return this.SetMessage(string.Format(format2, num10), false, false);
				}
			}
			Log.Out("Smooth Road Terrain in {0}", new object[]
			{
				(float)ms.ElapsedMilliseconds * 0.001f
			});
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator smoothWildernessTerrain()
		{
			yield return null;
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			foreach (StreetTile streetTile in this.getWildernessTilesToSmooth())
			{
				streetTile.SmoothWildernessTerrain();
			}
			Log.Out(string.Format("Smooth Wilderness Terrain in {0}, r={1:x}", (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GenerateTerrainTiles()
		{
			int tileWidth = this.WorldSize / 256;
			this.terrainTypeMap = new DataMap<TerrainType>(tileWidth, TerrainType.none);
			Rand instance = Rand.Instance;
			List<TileGroup> list = new List<TileGroup>();
			for (int i = 0; i < 5; i++)
			{
				list.Add(new TileGroup
				{
					Biome = (BiomeType)i
				});
			}
			for (int j = 0; j < this.biomeMap.data.GetLength(0); j++)
			{
				for (int k = 0; k < this.biomeMap.data.GetLength(1); k++)
				{
					Vector2i item = new Vector2i(j, k);
					BiomeType index = this.biomeMap.data[j, k];
					list[(int)index].Positions.Add(item);
				}
			}
			float num = (float)(this.Plains + this.Hills + this.Mountains);
			if (num == 0f)
			{
				this.Plains = 1;
				num = 1f;
			}
			foreach (TileGroup tileGroup in list)
			{
				int num2 = Mathf.FloorToInt((float)this.Plains / num * (float)tileGroup.Positions.Count);
				int num3 = Mathf.FloorToInt((float)this.Hills / num * (float)tileGroup.Positions.Count);
				int num4 = Mathf.FloorToInt((float)this.Mountains / num * (float)tileGroup.Positions.Count);
				while (tileGroup.Positions.Count > num2 + num3 + num4)
				{
					int num5 = instance.Range(3);
					if (num5 == 0)
					{
						if (this.Plains > 0)
						{
							num2++;
						}
						else
						{
							num5++;
						}
					}
					if (num5 == 1)
					{
						if (this.Hills > 0)
						{
							num3++;
						}
						else
						{
							num5++;
						}
					}
					if (num5 == 2 && this.Mountains > 0)
					{
						num4++;
					}
				}
				int index2 = instance.Range(tileGroup.Positions.Count);
				while (tileGroup.Positions.Count > 0)
				{
					Vector2i vector2i = tileGroup.Positions[index2];
					tileGroup.Positions.RemoveAt(index2);
					index2 = instance.Range(tileGroup.Positions.Count);
					int num6 = vector2i.x / 1;
					int num7 = vector2i.y / 1;
					if (this.terrainTypeMap.data[num6, num7] == TerrainType.none)
					{
						if (num3 > 0)
						{
							if (num3 >= 2)
							{
								num6 &= -2;
								num7 &= -2;
								this.terrainTypeMap.data[num6, num7] = TerrainType.hills;
								this.terrainTypeMap.data[num6 + 1, num7] = TerrainType.hills;
								this.terrainTypeMap.data[num6, num7 + 1] = TerrainType.hills;
								this.terrainTypeMap.data[num6 + 1, num7 + 1] = TerrainType.hills;
							}
							num3 -= 4;
						}
						else if (num4 > 0)
						{
							num4--;
							this.terrainTypeMap.data[num6, num7] = TerrainType.mountains;
							if (num4 > 0 && instance.Float() < 0.8f)
							{
								int num8 = instance.Range(4);
								for (int l = 0; l < 4; l++)
								{
									num8 = (num8 + 1 & 3);
									Vector2i vector2i2;
									vector2i2.x = vector2i.x + this.directions4way[num8].x;
									vector2i2.y = vector2i.y + this.directions4way[num8].y;
									int num9 = tileGroup.Positions.IndexOf(vector2i2);
									if (num9 >= 0)
									{
										num6 = vector2i2.x / 1;
										num7 = vector2i2.y / 1;
										if (this.terrainTypeMap.data[num6, num7] == TerrainType.none)
										{
											index2 = num9;
											break;
										}
									}
								}
							}
						}
						else
						{
							this.terrainTypeMap.data[num6, num7] = TerrainType.plains;
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<WorldBuilder.BiomeTypeData> CalcBiomeTileBiomeData(int totalTiles)
		{
			float num = (float)(this.ForestBiomeWeight + this.BurntForestBiomeWeight + this.DesertBiomeWeight + this.SnowBiomeWeight + this.WastelandBiomeWeight);
			List<WorldBuilder.BiomeTypeData> list = new List<WorldBuilder.BiomeTypeData>
			{
				new WorldBuilder.BiomeTypeData(BiomeType.forest, (float)this.ForestBiomeWeight / num, totalTiles),
				new WorldBuilder.BiomeTypeData(BiomeType.burntForest, (float)this.BurntForestBiomeWeight / num, totalTiles),
				new WorldBuilder.BiomeTypeData(BiomeType.desert, (float)this.DesertBiomeWeight / num, totalTiles),
				new WorldBuilder.BiomeTypeData(BiomeType.snow, (float)this.SnowBiomeWeight / num, totalTiles),
				new WorldBuilder.BiomeTypeData(BiomeType.wasteland, (float)this.WastelandBiomeWeight / num, totalTiles)
			};
			list = (from b in list
			where b.Percent > 0f
			orderby -b.Percent
			select b).ToList<WorldBuilder.BiomeTypeData>();
			int num2 = 0;
			for (int i = 0; i < 5; i++)
			{
				num2 += list[i].TileCount;
			}
			int num3 = 0;
			for (int j = num2; j < totalTiles; j++)
			{
				list[num3].TileCount++;
				num3 = (num3 + 1) % 5;
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GenerateBiomeTiles()
		{
			int num = this.WorldSize / 256;
			float num2 = (float)num * 0.5f;
			int num3 = num * num;
			this.biomeMap = new DataMap<BiomeType>(num, BiomeType.none);
			List<WorldBuilder.BiomeTypeData> list = this.CalcBiomeTileBiomeData(num3);
			BiomeType biomeType = BiomeType.none;
			if (this.biomeLayout == WorldBuilder.BiomeLayout.CenterForest)
			{
				biomeType = BiomeType.forest;
			}
			if (this.biomeLayout == WorldBuilder.BiomeLayout.CenterWasteland)
			{
				biomeType = BiomeType.wasteland;
			}
			int num4 = 1;
			float num5 = num2;
			float num6 = num2;
			for (int i = 0; i < num4; i++)
			{
				if (this.biomeLayout == WorldBuilder.BiomeLayout.Line)
				{
					float num7 = (float)num * 0.4f;
					float f = (float)(Rand.Instance.Range(4) * 90) * 0.0174532924f;
					Vector2 a;
					a.x = num5 - Mathf.Cos(f) * num7;
					a.y = num6 - Mathf.Sin(f) * num7;
					Vector2 b;
					b.x = num5 + Mathf.Cos(f) * num7;
					b.y = num6 + Mathf.Sin(f) * num7;
					float num8 = 0f;
					float num9 = 1f / (float)(list.Count - 1);
					for (int j = 0; j < list.Count; j++)
					{
						WorldBuilder.BiomeTypeData biomeTypeData = list[j];
						Vector2 vector = Vector2.Lerp(a, b, num8);
						biomeTypeData.Center = new Vector2i((int)vector.x, (int)vector.y);
						biomeTypeData.TileCount--;
						this.biomeMap.data[biomeTypeData.Center.x, biomeTypeData.Center.y] = biomeTypeData.Type;
						num8 += num9;
					}
				}
				else
				{
					int num10 = list.Count - 1;
					if (this.biomeLayout == WorldBuilder.BiomeLayout.Circle)
					{
						num10 = list.Count;
					}
					float num11 = (float)Rand.Instance.Angle();
					float num12 = 360f / (float)num10;
					if (Rand.Instance.Float() < 0.5f)
					{
						num12 *= -1f;
					}
					for (int k = 0; k < list.Count; k++)
					{
						WorldBuilder.BiomeTypeData biomeTypeData2 = list[k];
						if (biomeTypeData2.Type == biomeType)
						{
							biomeTypeData2.Center = new Vector2i((int)num5, (int)num6);
						}
						else
						{
							float num13 = (float)num * 0.4f;
							float num14 = num5 + Mathf.Cos(num11 * 0.0174532924f) * num13;
							float num15 = num6 + Mathf.Sin(num11 * 0.0174532924f) * num13;
							num11 += num12;
							biomeTypeData2.Center = new Vector2i((int)num14, (int)num15);
						}
						biomeTypeData2.TileCount--;
						this.biomeMap.data[biomeTypeData2.Center.x, biomeTypeData2.Center.y] = biomeTypeData2.Type;
					}
				}
				num5 += 3f;
				num6 += 2f;
			}
			int num16 = num3 - list.Count;
			int num17 = 1 + this.WorldSize / 2048;
			int num18;
			do
			{
				num18 = num16;
				for (int l = 0; l < list.Count; l++)
				{
					WorldBuilder.BiomeTypeData biomeTypeData3 = list[l];
					if (biomeTypeData3.TileCount > 0)
					{
						int edge = 0;
						if (biomeTypeData3.Type == biomeType)
						{
							edge = num17;
						}
						int num19 = 1 + (int)(biomeTypeData3.Percent * 4f);
						int num20 = 0;
						while (num20 < num19 && this.FindBiomeEmptyAndSet(biomeTypeData3, edge))
						{
							biomeTypeData3.TileCount--;
							num16--;
							if (biomeTypeData3.TileCount <= 0)
							{
								break;
							}
							num20++;
						}
					}
				}
			}
			while (num16 != num18);
			do
			{
				num18 = num16;
				for (int m = 0; m < list.Count; m++)
				{
					WorldBuilder.BiomeTypeData biomeTypeData4 = list[m];
					if (biomeTypeData4.Type != BiomeType.wasteland && this.FindBiomeEmptyAndSet(biomeTypeData4, 0))
					{
						num16--;
					}
				}
			}
			while (num16 != num18);
			for (int n = 0; n < this.biomeMap.data.GetLength(0); n++)
			{
				for (int num21 = 0; num21 < this.biomeMap.data.GetLength(1); num21++)
				{
					if (this.biomeMap.data[n, num21] == BiomeType.none)
					{
						this.biomeMap.data[n, num21] = BiomeType.wasteland;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool FindBiomeEmptyAndSet(WorldBuilder.BiomeTypeData _b, int _edge)
		{
			int v = this.WorldSize / 256 - 1 - _edge;
			for (int i = 1; i <= 39; i++)
			{
				int num = Utils.FastMax(_edge, _b.Center.x - i);
				int num2 = Utils.FastMin(_b.Center.x + i, v);
				int num3 = Utils.FastMax(_edge, _b.Center.y - i);
				int num4 = Utils.FastMin(_b.Center.y + i, v);
				for (int j = 0; j <= i; j++)
				{
					int num5 = _b.Center.y - i;
					int num6;
					if (num5 >= num3)
					{
						num6 = _b.Center.x - j;
						if (num6 >= num && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
						num6 = _b.Center.x + j;
						if (num6 <= num2 && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
					}
					num5 = _b.Center.y + i;
					if (num5 <= num4)
					{
						num6 = _b.Center.x - j;
						if (num6 >= num && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
						num6 = _b.Center.x + j;
						if (num6 <= num2 && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
					}
					num6 = _b.Center.x - i;
					if (num6 >= num)
					{
						num5 = _b.Center.y - j;
						if (num5 >= num3 && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
						num5 = _b.Center.y + j;
						if (num5 <= num4 && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
					}
					num6 = _b.Center.x + i;
					if (num6 <= num2)
					{
						num5 = _b.Center.y - j;
						if (num5 >= num3 && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
						num5 = _b.Center.y + j;
						if (num5 <= num4 && this.biomeMap.data[num6, num5] == BiomeType.none && this.HasBiomeNeighbor(num6, num5, _b.Type))
						{
							this.biomeMap.data[num6, num5] = _b.Type;
							return true;
						}
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HasBiomeNeighbor(int _x, int _y, BiomeType _biomeType)
		{
			int num = this.WorldSize / 256;
			int num2 = _x - 1;
			if (num2 >= 0 && this.biomeMap.data[num2, _y] == _biomeType)
			{
				return true;
			}
			num2 = _x + 1;
			if (num2 < num && this.biomeMap.data[num2, _y] == _biomeType)
			{
				return true;
			}
			int num3 = _y - 1;
			if (num3 >= 0 && this.biomeMap.data[_x, num3] == _biomeType)
			{
				return true;
			}
			num3 = _y + 1;
			return num3 < num && this.biomeMap.data[_x, num3] == _biomeType;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BiomeType GetBiomeFromNeighbors(int _x, int _y)
		{
			int num = this.WorldSize / 256;
			int num2 = _x - 1;
			if (num2 >= 0)
			{
				BiomeType biomeType = this.biomeMap.data[num2, _y];
				if (biomeType != BiomeType.none && biomeType != BiomeType.wasteland)
				{
					return biomeType;
				}
			}
			num2 = _x + 1;
			if (num2 < num)
			{
				BiomeType biomeType2 = this.biomeMap.data[num2, _y];
				if (biomeType2 != BiomeType.none && biomeType2 != BiomeType.wasteland)
				{
					return biomeType2;
				}
			}
			int num3 = _y - 1;
			if (num3 >= 0)
			{
				BiomeType biomeType3 = this.biomeMap.data[_x, num3];
				if (biomeType3 != BiomeType.none && biomeType3 != BiomeType.wasteland)
				{
					return biomeType3;
				}
			}
			num3 = _y + 1;
			if (num3 < num)
			{
				BiomeType biomeType4 = this.biomeMap.data[_x, num3];
				if (biomeType4 != BiomeType.none && biomeType4 != BiomeType.wasteland)
				{
					return biomeType4;
				}
			}
			return BiomeType.none;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serializeRWGTTW(Stream stream)
		{
			World world = new World();
			WorldState worldState = new WorldState();
			worldState.SetFrom(world, EnumChunkProviderId.ChunkDataDriven);
			worldState.ResetDynamicData();
			worldState.Save(stream);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serializeDynamicProperties(Stream stream)
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			dynamicProperties.Values["Scale"] = "1";
			dynamicProperties.Values["HeightMapSize"] = string.Format("{0},{0}", this.WorldSize);
			dynamicProperties.Values["Modes"] = "Survival,SurvivalSP,SurvivalMP,Creative";
			dynamicProperties.Values["FixedWaterLevel"] = "false";
			dynamicProperties.Values["RandomGeneratedWorld"] = "true";
			dynamicProperties.Values["GameVersion"] = Constants.cVersionInformation.SerializableString;
			dynamicProperties.Values["Generation.Seed"] = this.WorldSeedName;
			dynamicProperties.Values["Seed"] = this.Seed.ToString();
			dynamicProperties.Values["Generation.Towns"] = this.Towns.ToString();
			dynamicProperties.Values["Generation.Wilderness"] = this.Wilderness.ToString();
			dynamicProperties.Values["Generation.Lakes"] = this.Lakes.ToString();
			dynamicProperties.Values["Generation.Rivers"] = this.Rivers.ToString();
			dynamicProperties.Values["Generation.Cracks"] = this.Canyons.ToString();
			dynamicProperties.Values["Generation.Craters"] = this.Craters.ToString();
			dynamicProperties.Values["Generation.Plains"] = this.Plains.ToString();
			dynamicProperties.Values["Generation.Hills"] = this.Hills.ToString();
			dynamicProperties.Values["Generation.Mountains"] = this.Mountains.ToString();
			dynamicProperties.Values["Generation.Forest"] = this.ForestBiomeWeight.ToString();
			dynamicProperties.Values["Generation.BurntForest"] = this.BurntForestBiomeWeight.ToString();
			dynamicProperties.Values["Generation.Desert"] = this.DesertBiomeWeight.ToString();
			dynamicProperties.Values["Generation.Snow"] = this.SnowBiomeWeight.ToString();
			dynamicProperties.Values["Generation.Wasteland"] = this.WastelandBiomeWeight.ToString();
			dynamicProperties.Save("MapInfo", stream);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator CreatePreviewTexture(Color32[] roadMask)
		{
			yield return this.SetMessage(Localization.Get("xuiRwgCreatingPreview", false), true, false);
			MicroStopwatch msReset = new MicroStopwatch(true);
			Color32[] dest = new Color32[roadMask.Length];
			Color32 color = new Color32(0, 0, 0, byte.MaxValue);
			int destOffsetY = 0;
			int biomeSteps = this.WorldSize / this.BiomeSize;
			int num3;
			for (int y = 0; y < this.BiomeSize; y = num3 + 1)
			{
				int num = destOffsetY;
				for (int i = 0; i < this.BiomeSize; i++)
				{
					Color32 color2 = this.biomeDest[i + y * this.BiomeSize];
					color.r = color2.r / 2;
					color.g = color2.g / 2;
					color.b = color2.b / 2;
					for (int j = 0; j < biomeSteps; j++)
					{
						int num2 = num + j * this.WorldSize;
						for (int k = 0; k < biomeSteps; k++)
						{
							dest[num2 + k] = color;
						}
					}
					num += biomeSteps;
				}
				destOffsetY += biomeSteps * this.WorldSize;
				if (msReset.ElapsedMilliseconds > 500L)
				{
					yield return null;
					msReset.ResetAndRestart();
				}
				num3 = y;
			}
			yield return null;
			msReset.ResetAndRestart();
			if (this.Townships != null)
			{
				StampGroup roadLayer = new StampGroup("Road Layer");
				foreach (Township township in this.Townships)
				{
					if (township.Streets.Count > 0)
					{
						foreach (Vector2i key in township.Streets.Keys)
						{
							if (township.Streets[key].Township != null)
							{
								roadLayer.Stamps.AddRange(township.Streets[key].GetStamps());
							}
						}
					}
					if (msReset.ElapsedMilliseconds > 500L)
					{
						yield return null;
						msReset.ResetAndRestart();
					}
				}
				List<Township>.Enumerator enumerator = default(List<Township>.Enumerator);
				this.StampManager.DrawStampGroup(roadLayer, dest, this.WorldSize, 1f);
				roadLayer = null;
			}
			yield return null;
			msReset.ResetAndRestart();
			Color32 waterColor = new Color32(0, 0, byte.MaxValue, byte.MaxValue);
			Color32 radColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
			for (int y = 0; y < roadMask.Length; y = num3 + 1)
			{
				int num4 = y % this.WorldSize;
				int num5 = y / this.WorldSize;
				if (roadMask[y].a > 0)
				{
					dest[y] = roadMask[y];
				}
				if (this.GetWater(y) > 0)
				{
					dest[y] = waterColor;
				}
				if (this.GetRad(y) > 0)
				{
					dest[y] = radColor;
				}
				if (y % 50000 == 0 && msReset.ElapsedMilliseconds > 500L)
				{
					yield return null;
					msReset.ResetAndRestart();
				}
				num3 = y;
			}
			Color32 color3 = new Color32(200, 200, byte.MaxValue, byte.MaxValue);
			Color32 color4 = new Color32(0, 0, 50, byte.MaxValue);
			for (int l = 0; l < this.playerSpawns.Count; l++)
			{
				WorldBuilder.PlayerSpawn playerSpawn = this.playerSpawns[l];
				int num6 = (int)playerSpawn.Position.x + (int)playerSpawn.Position.z * this.WorldSize;
				dest[num6 - this.WorldSize - 1] = color4;
				dest[num6 - this.WorldSize] = color3;
				dest[num6 - this.WorldSize + 1] = color4;
				dest[num6 - 1] = color3;
				dest[num6] = color4;
				dest[num6 + 1] = color3;
				dest[num6 + this.WorldSize - 1] = color4;
				dest[num6 + this.WorldSize] = color3;
				dest[num6 + this.WorldSize + 1] = color4;
			}
			yield return null;
			if (this.WorldSize >= 0)
			{
				XUiC_WorldGenerationWindowGroup.PreviewQuality previewQualityLevel = XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel;
				if (previewQualityLevel == XUiC_WorldGenerationWindowGroup.PreviewQuality.NoPreview)
				{
					UnityEngine.Object.Destroy(this.PreviewImage);
					this.PreviewImage = new Texture2D(1, 1);
				}
				else
				{
					UnityEngine.Object.Destroy(this.PreviewImage);
					this.PreviewImage = new Texture2D(this.WorldSize, this.WorldSize);
					this.PreviewImage.SetPixels32(dest);
					if (previewQualityLevel >= XUiC_WorldGenerationWindowGroup.PreviewQuality.Default)
					{
						this.PreviewImage.Apply(true, true);
						this.PreviewImage.filterMode = FilterMode.Point;
					}
					else
					{
						this.PreviewImage.Apply(false);
						float num7;
						if (previewQualityLevel != XUiC_WorldGenerationWindowGroup.PreviewQuality.Lowest)
						{
							if (previewQualityLevel != XUiC_WorldGenerationWindowGroup.PreviewQuality.Low)
							{
								num7 = 0.5f;
							}
							else
							{
								num7 = 0.5f;
							}
						}
						else
						{
							num7 = 0.25f;
						}
						int num8 = Mathf.CeilToInt(num7 * (float)this.WorldSize);
						Texture2D texture2D = new Texture2D(num8, num8);
						this.PreviewImage.PointScaleNoAlloc(texture2D);
						texture2D.Apply(true, true);
						UnityEngine.Object.Destroy(this.PreviewImage);
						this.PreviewImage = texture2D;
					}
				}
			}
			yield break;
			yield break;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StreetTile GetStreetTileGrid(Vector2i pos)
		{
			return this.GetStreetTileGrid(pos.x, pos.y);
		}

		public StreetTile GetStreetTileGrid(int x, int y)
		{
			if ((ulong)x >= (ulong)((long)this.StreetTileMapSize))
			{
				return null;
			}
			if ((ulong)y >= (ulong)((long)this.StreetTileMapSize))
			{
				return null;
			}
			return this.StreetTileMap[x, y];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StreetTile GetStreetTileWorld(Vector2i pos)
		{
			return this.GetStreetTileWorld(pos.x, pos.y);
		}

		public StreetTile GetStreetTileWorld(int x, int y)
		{
			x /= 150;
			if ((ulong)x >= (ulong)((long)this.StreetTileMapSize))
			{
				return null;
			}
			y /= 150;
			if ((ulong)y >= (ulong)((long)this.StreetTileMapSize))
			{
				return null;
			}
			return this.StreetTileMap[x, y];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(Vector2 pos)
		{
			return this.GetHeight((int)pos.x, (int)pos.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(Vector2i pos)
		{
			return this.GetHeight(pos.x, pos.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(float x, float y)
		{
			return this.GetHeight((int)x, (int)y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(int x, int y)
		{
			if ((ulong)x >= (ulong)((long)this.WorldSize) || (ulong)y >= (ulong)((long)this.WorldSize))
			{
				return 0f;
			}
			return this.HeightMap[x + y * this.WorldSize];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetHeight(int index, float height)
		{
			this.HeightMap[index] = height;
		}

		public void SetHeight(Vector2i pos, float height)
		{
			this.SetHeight(pos.x, pos.y, height);
		}

		public void SetHeight(int x, int y, float height)
		{
			if ((ulong)x >= (ulong)((long)this.WorldSize) || (ulong)y >= (ulong)((long)this.WorldSize))
			{
				return;
			}
			this.SetHeight(x + y * this.WorldSize, height);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetHeightTrusted(int x, int y, float height)
		{
			this.SetHeight(x + y * this.WorldSize, height);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TerrainType GetTerrainType(Vector2i pos)
		{
			return this.GetTerrainType(pos.x, pos.y);
		}

		public TerrainType GetTerrainType(int x, int y)
		{
			x /= 256;
			if ((ulong)x >= (ulong)((long)this.terrainTypeMap.data.GetLength(0)))
			{
				return TerrainType.none;
			}
			y /= 256;
			if ((ulong)y >= (ulong)((long)this.terrainTypeMap.data.GetLength(1)))
			{
				return TerrainType.none;
			}
			return this.terrainTypeMap.data[x, y];
		}

		public BiomeType GetBiome(Vector2i pos)
		{
			return this.GetBiome(pos.x, pos.y);
		}

		public BiomeType GetBiome(int x, int y)
		{
			int num = x / 8 + y / 8 * this.BiomeSize;
			if ((ulong)num >= (ulong)((long)(this.BiomeSize * this.BiomeSize)))
			{
				return BiomeType.forest;
			}
			Color32 color = this.biomeDest[num];
			BiomeType result = BiomeType.forest;
			if (color.g == WorldBuilderConstants.burntForestCol.g)
			{
				result = BiomeType.burntForest;
			}
			else if (color.g == WorldBuilderConstants.desertCol.g)
			{
				result = BiomeType.desert;
			}
			else if (color.g == WorldBuilderConstants.snowCol.g)
			{
				result = BiomeType.snow;
			}
			else if (color.g == WorldBuilderConstants.wastelandCol.g)
			{
				result = BiomeType.wasteland;
			}
			return result;
		}

		public void SetWater(int x, int y, byte height)
		{
			if ((ulong)x >= (ulong)((long)this.WorldSize) || (ulong)y >= (ulong)((long)this.WorldSize))
			{
				return;
			}
			this.WaterMap[x + y * this.WorldSize] = height;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte GetWater(int x, int y)
		{
			if ((ulong)x >= (ulong)((long)this.WorldSize) || (ulong)y >= (ulong)((long)this.WorldSize))
			{
				return 0;
			}
			return this.WaterMap[x + y * this.WorldSize];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte GetWater(int _index)
		{
			if ((ulong)_index >= (ulong)((long)(this.WorldSize * this.WorldSize)))
			{
				return 0;
			}
			return this.WaterMap[_index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte GetRad(int x, int y)
		{
			if ((ulong)x >= (ulong)((long)this.WorldSize) || (ulong)y >= (ulong)((long)this.WorldSize))
			{
				return 0;
			}
			return this.radDest[x + y * this.WorldSize].r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte GetRad(int _index)
		{
			if ((ulong)_index >= (ulong)((long)(this.WorldSize * this.WorldSize)))
			{
				return 0;
			}
			return this.radDest[_index].r;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serializePrefabs(Stream stream)
		{
			this.PrefabManager.SavePrefabData(stream);
			if (!this.UsePreviewer)
			{
				this.PrefabManager.UsedPrefabsWorld.Clear();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serializeRawHeightmap(Stream stream)
		{
			HeightMapUtils.SaveHeightMapRAW(stream, this.HeightMap, 65535);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator DrawRoads(Color32[] dest)
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			byte[] ids = new byte[this.WorldSize * this.WorldSize];
			int num;
			for (int i = 0; i < this.wildernessPaths.Count; i = num + 1)
			{
				this.wildernessPaths[i].DrawPathToRoadIds(ids);
				if (this.IsMessageElapsed())
				{
					yield return this.SetMessage(string.Format(Localization.Get("xuiRwgDrawRoadsWilderness", false), 100 * i / this.wildernessPaths.Count), false, false);
				}
				num = i;
			}
			for (int i = 0; i < this.paths.Count; i = num + 1)
			{
				this.paths[i].DrawPathToRoadIds(ids);
				if (this.IsMessageElapsed())
				{
					yield return this.SetMessage(string.Format(Localization.Get("xuiRwgDrawRoadsProgress", false), 100 * i / this.paths.Count), false, false);
				}
				num = i;
			}
			this.PathShared.ConvertIdsToColors(ids, dest);
			Log.Out(string.Format("DrawRoads in {0}", (float)ms.ElapsedMilliseconds * 0.001f));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void serializePlayerSpawns(Stream stream)
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, SdEncoding.UTF8NoBOM, 1024, true))
			{
				streamWriter.WriteLine("<spawnpoints>");
				if (this.playerSpawns != null)
				{
					for (int i = 0; i < this.playerSpawns.Count; i++)
					{
						WorldBuilder.PlayerSpawn playerSpawn = this.playerSpawns[i];
						streamWriter.WriteLine(string.Format("    <spawnpoint position=\"{0},{1},{2}\" rotation=\"0,{3},0\"/>", new object[]
						{
							(playerSpawn.Position.x - (float)this.WorldSize / 2f).ToCultureInvariantString(),
							playerSpawn.Position.y.ToCultureInvariantString(),
							(playerSpawn.Position.z - (float)this.WorldSize / 2f).ToCultureInvariantString(),
							playerSpawn.Rotation
						}));
					}
				}
				streamWriter.WriteLine("</spawnpoints>");
			}
		}

		public IEnumerator SetMessage(string _message, bool _logToConsole = false, bool _ignoreCancel = false)
		{
			if (_message != null)
			{
				_message += string.Format(" \n Time {0}:{1:00}", this.totalMS.Elapsed.Minutes, this.totalMS.Elapsed.Seconds);
			}
			if (!GameManager.IsDedicatedServer)
			{
				if (!_ignoreCancel)
				{
					this.IsCanceled |= this.CheckCancel();
				}
				if (_message != null)
				{
					if (!_ignoreCancel && this.IsCanceled)
					{
						_message = "Canceling...";
					}
					if (!XUiC_ProgressWindow.IsWindowOpen())
					{
						XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, _message, null, true, false, false, true);
					}
					else if (_message != this.setMessageLast)
					{
						this.setMessageLast = _message;
						XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, _message, true);
					}
				}
				else
				{
					this.setMessageLast = string.Empty;
					XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
				}
				yield return this.endOfFrameHandle;
			}
			if (_logToConsole && _message != null)
			{
				Log.Out("WorldGenerator:" + _message.Replace("\n", ": "));
			}
			yield return null;
			yield break;
		}

		public bool IsMessageElapsed()
		{
			if (this.messageMS.ElapsedMilliseconds > 600L)
			{
				this.messageMS.ResetAndRestart();
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckCancel()
		{
			return this.UsePreviewer && PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i getRotatedPoint(int x, int y, int cx, int cy, int angle)
		{
			return new Vector2i(Mathf.RoundToInt((float)((double)(x - cx) * Math.Cos((double)angle) - (double)(y - cy) * Math.Sin((double)angle) + (double)cx)), Mathf.RoundToInt((float)((double)(x - cx) * Math.Sin((double)angle) + (double)(y - cy) * Math.Cos((double)angle) + (double)cy)));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetTerrainProperties(string biomeTypeName, string terrainTypeName, string comboTypeName, out Vector2 _scaleMinMax, out int _clusterCount, out int _clusterRadius, out float _clusterStrength, out bool useBiomeMask, out float biomeCutoff)
		{
			_scaleMinMax = Vector2.one;
			string @string = this.thisWorldProperties.GetString(comboTypeName + ".scale");
			if (@string == string.Empty)
			{
				@string = this.thisWorldProperties.GetString(terrainTypeName + ".scale");
			}
			if (@string != string.Empty)
			{
				_scaleMinMax = StringParsers.ParseVector2(@string);
			}
			_scaleMinMax *= 0.5f;
			_clusterCount = 3;
			_clusterRadius = 85;
			_clusterStrength = 1f;
			@string = this.thisWorldProperties.GetString(comboTypeName + ".clusters");
			if (@string == string.Empty)
			{
				@string = this.thisWorldProperties.GetString(terrainTypeName + ".clusters");
			}
			if (@string != string.Empty)
			{
				Vector3 vector = StringParsers.ParseVector3(@string, 0, -1);
				_clusterCount = (int)vector.x;
				_clusterRadius = (int)(256f * vector.y);
				_clusterStrength = vector.z;
			}
			useBiomeMask = false;
			@string = this.thisWorldProperties.GetString(comboTypeName + ".use_biome_mask");
			if (@string == string.Empty)
			{
				@string = this.thisWorldProperties.GetString(terrainTypeName + ".use_biome_mask");
			}
			if (@string != string.Empty)
			{
				useBiomeMask = StringParsers.ParseBool(@string, 0, -1, true);
			}
			biomeCutoff = 0.1f;
			@string = this.thisWorldProperties.GetString(comboTypeName + ".biome_mask_min");
			if (@string == string.Empty)
			{
				@string = this.thisWorldProperties.GetString(terrainTypeName + ".biome_mask_min");
			}
			if (@string != string.Empty)
			{
				biomeCutoff = StringParsers.ParseFloat(@string, 0, -1, NumberStyles.Any);
			}
		}

		public static string GetGeneratedWorldName(string _worldSeedName, int _worldSize = 8192)
		{
			return RandomCountyNameGenerator.GetName(_worldSeedName.GetHashCode() + _worldSize);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int distanceSqr(Vector2i pointA, Vector2i pointB)
		{
			Vector2i vector2i = pointA - pointB;
			return vector2i.x * vector2i.x + vector2i.y * vector2i.y;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float distanceSqr(Vector2 pointA, Vector2 pointB)
		{
			Vector2 vector = pointA - pointB;
			return vector.x * vector.x + vector.y * vector.y;
		}

		public int GetCount(string _name, WorldBuilder.GenerationSelections _selection)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			this.thisWorldProperties.ParseVec(string.Format("{0}.count", _name), ref num, ref num2, ref num3);
			int result = (int)num;
			if (_selection == WorldBuilder.GenerationSelections.Default)
			{
				result = (int)num2;
			}
			else if (_selection == WorldBuilder.GenerationSelections.Many)
			{
				result = (int)num3;
			}
			return result;
		}

		public readonly DistrictPlanner DistrictPlanner;

		public readonly HighwayPlanner HighwayPlanner;

		public readonly PathingUtils PathingUtils;

		public readonly PathShared PathShared;

		public readonly POISmoother POISmoother;

		public readonly PrefabManager PrefabManager;

		public readonly StampManager StampManager;

		public readonly StreetTileShared StreetTileShared;

		public readonly TilePathingUtils TilePathingUtils;

		public readonly TownPlanner TownPlanner;

		public readonly TownshipShared TownshipShared;

		public readonly WildernessPathPlanner WildernessPathPlanner;

		public readonly WildernessPlanner WildernessPlanner;

		public string WorldName;

		public string WorldSeedName;

		[PublicizedFrom(EAccessModifier.Private)]
		public string WorldPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public MicroStopwatch totalMS;

		public bool IsCanceled;

		public bool IsFinished;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color32[] dest;

		public Texture2D PreviewImage;

		public readonly int WaterHeight = 29;

		public int WorldSize = 8192;

		public int WorldSizeDistDiv;

		public const int BiomeSizeDiv = 8;

		public int BiomeSize;

		public int Seed = 12345;

		public int Plains = 4;

		public int Hills = 4;

		public int Mountains = 2;

		public WorldBuilder.GenerationSelections Canyons = WorldBuilder.GenerationSelections.Default;

		public WorldBuilder.GenerationSelections Craters = WorldBuilder.GenerationSelections.Default;

		public WorldBuilder.GenerationSelections Lakes = WorldBuilder.GenerationSelections.Default;

		public WorldBuilder.GenerationSelections Rivers = WorldBuilder.GenerationSelections.Default;

		public WorldBuilder.GenerationSelections Towns = WorldBuilder.GenerationSelections.Default;

		public WorldBuilder.GenerationSelections Wilderness = WorldBuilder.GenerationSelections.Default;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] HeightMap;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte[] WaterMap;

		public byte[,] PathCreationGrid;

		public PathTile[,] PathingGrid;

		public int StreetTileMapSize;

		public StreetTile[,] StreetTileMap;

		public DataMap<BiomeType> biomeMap;

		public DataMap<TerrainType> terrainTypeMap;

		public List<Township> Townships = new List<Township>();

		public int WildernessPrefabCount;

		public DynamicPrefabDecorator PrefabDecorator;

		[PublicizedFrom(EAccessModifier.Private)]
		public string worldSizeName;

		[PublicizedFrom(EAccessModifier.Private)]
		public DynamicProperties thisWorldProperties;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int WorldTileSize = 1024;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int BiomeTileSize = 256;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TerrainTileSize = 256;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int terrainToBiomeTileScale = 1;

		public WorldBuilder.BiomeLayout biomeLayout;

		public int ForestBiomeWeight = 13;

		[PublicizedFrom(EAccessModifier.Private)]
		public int BurntForestBiomeWeight = 18;

		[PublicizedFrom(EAccessModifier.Private)]
		public int DesertBiomeWeight = 22;

		[PublicizedFrom(EAccessModifier.Private)]
		public int SnowBiomeWeight = 23;

		[PublicizedFrom(EAccessModifier.Private)]
		public int WastelandBiomeWeight = 24;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int BiomeTypes = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<BiomeType, Color32> biomeColors = new Dictionary<BiomeType, Color32>();

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cPlayerSpawnsNeeded = 12;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<WorldBuilder.PlayerSpawn> playerSpawns;

		public List<Path> paths = new List<Path>();

		public List<Path> wildernessPaths = new List<Path>();

		public List<Vector2i> TraderCenterPositions = new List<Vector2i>();

		public List<Vector2i> TraderForestCenterPositions = new List<Vector2i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public WaitForEndOfFrame endOfFrameHandle = new WaitForEndOfFrame();

		public bool UsePreviewer = true;

		[TupleElementNames(new string[]
		{
			"langKey",
			"fileName",
			"serializer"
		})]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ValueTuple<string, string, Action<Stream>>[] threadedSerializers;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MemoryStream[] threadedSerializerBuffers;

		[TupleElementNames(new string[]
		{
			"langKey",
			"fileName",
			"serializer"
		})]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ValueTuple<string, string, Func<Stream, IEnumerator>>[] mainThreadSerializers;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MemoryStream[] mainThreadSerializerBuffers;

		[PublicizedFrom(EAccessModifier.Private)]
		public long serializedTotalSize;

		public readonly int[] biomeTagBits = new int[]
		{
			FastTags<TagGroup.Poi>.GetBit("forest"),
			FastTags<TagGroup.Poi>.GetBit("burntforest"),
			FastTags<TagGroup.Poi>.GetBit("desert"),
			FastTags<TagGroup.Poi>.GetBit("snow"),
			FastTags<TagGroup.Poi>.GetBit("wasteland")
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] terrainDest;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] terrainWaterDest;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] waterDest;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color32[] biomeDest;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color32[] radDest;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StampGroup lowerLayer = new StampGroup("Lower Layer");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StampGroup terrainLayer = new StampGroup("Top Layer");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StampGroup radiationLayer = new StampGroup("Radiation Layer");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StampGroup biomeLayer = new StampGroup("Biome Layer");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StampGroup waterLayer = new StampGroup("Water Layer");

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GenWaterBorderN;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GenWaterBorderS;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GenWaterBorderW;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GenWaterBorderE;

		public List<Rect> waterRects = new List<Rect>();

		public List<WorldBuilder.WildernessPathInfo> wPathInfo = new List<WorldBuilder.WildernessPathInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public string setMessageLast = string.Empty;

		[PublicizedFrom(EAccessModifier.Private)]
		public MicroStopwatch messageMS = new MicroStopwatch(true);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] directions8way = new Vector2i[]
		{
			Vector2i.up,
			Vector2i.up + Vector2i.right,
			Vector2i.right,
			Vector2i.right + Vector2i.down,
			Vector2i.down,
			Vector2i.down + Vector2i.left,
			Vector2i.left,
			Vector2i.left + Vector2i.up
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] directions4way = new Vector2i[]
		{
			Vector2i.up,
			Vector2i.right,
			Vector2i.down,
			Vector2i.left
		};

		public enum BiomeLayout
		{
			CenterForest,
			CenterWasteland,
			Circle,
			Line
		}

		public class TownshipData
		{
			public string Name;

			public List<string> SpawnableTerrain = new List<string>();

			public bool SpawnCustomSizes;

			public bool SpawnTrader = true;

			public bool SpawnGateway = true;

			public string OutskirtDistrict;

			public float OutskirtDistrictPercent;

			public FastTags<TagGroup.Poi> Biomes;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class BiomeTypeData
		{
			public BiomeTypeData(BiomeType _type, float _percent, int _totalTiles)
			{
				this.Type = _type;
				this.Percent = _percent;
				this.TileCount = Mathf.FloorToInt(_percent * (float)_totalTiles);
				if (this.Percent > 0f && this.TileCount == 0)
				{
					this.TileCount = 1;
				}
			}

			public BiomeType Type;

			public float Percent;

			public int TileCount;

			public Vector2i Center;
		}

		public class WildernessPathInfo
		{
			public WildernessPathInfo(Vector2i _startPos, int _id, float _pathRadius, BiomeType _biome, int _connections = 0, Path _path = null)
			{
				this.Position = _startPos;
				this.PoiId = _id;
				this.PathRadius = _pathRadius;
				this.Biome = _biome;
				this.Connections = _connections;
				this.Path = _path;
			}

			public Vector2i Position;

			public int PoiId;

			public float PathRadius;

			public BiomeType Biome;

			public int Connections;

			public Path Path;
		}

		public enum GenerationSelections
		{
			None,
			Few,
			Default,
			Many
		}

		public struct PlayerSpawn
		{
			public PlayerSpawn(Vector3 _position, float _yRotation)
			{
				this.Position = _position;
				this.Rotation = _yRotation;
			}

			public bool IsTooClose(Vector3 _position)
			{
				float num = _position.x - this.Position.x;
				float num2 = _position.z - this.Position.z;
				return num * num + num2 * num2 < 3600f;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public const int cSafeDist = 60;

			public Vector3 Position;

			public float Rotation;
		}
	}
}
