using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityClass
{
	public static void Add(string _entityClassname, EntityClass _entityClass)
	{
		_entityClass.entityClassName = _entityClassname;
		EntityClass.list[_entityClassname.GetHashCode()] = _entityClass;
	}

	public static EntityClass GetEntityClass(int entityClass)
	{
		EntityClass result;
		EntityClass.list.TryGetValue(entityClass, out result);
		return result;
	}

	public static string GetEntityClassName(int entityClass)
	{
		EntityClass entityClass2;
		if (EntityClass.list.TryGetValue(entityClass, out entityClass2))
		{
			return entityClass2.entityClassName;
		}
		return "null";
	}

	public static int FromString(string _s)
	{
		return _s.GetHashCode();
	}

	public EntityClass Init()
	{
		if (!this.Properties.Values.TryGetValue(EntityClass.PropPrefab, out this.prefabPath) || this.prefabPath.Length == 0)
		{
			throw new Exception("Mandatory property 'prefab' missing in entity_class '" + this.entityClassName + "'");
		}
		if (this.prefabPath[0] == '/')
		{
			this.prefabPath = this.prefabPath.Substring(1);
			this.IsPrefabCombined = true;
		}
		else if (DataLoader.IsInResources(this.prefabPath))
		{
			this.prefabPath = "Prefabs/prefabEntity" + this.prefabPath;
		}
		string text;
		if (this.Properties.Values.TryGetValue(EntityClass.PropMesh, out text) && text.Length > 0)
		{
			if (DataLoader.IsInResources(text))
			{
				text = "Entities/" + text;
			}
			this.meshPath = text;
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropMeshFP))
		{
			string text2 = this.Properties.Values[EntityClass.PropMeshFP];
			if (DataLoader.IsInResources(text2))
			{
				text2 = "Entities/" + text2;
			}
			this.meshFP = DataLoader.LoadAsset<Transform>(text2);
			if (this.meshFP == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"Could not load file '",
					text2,
					"' for entity_class '",
					this.entityClassName,
					"'"
				}));
			}
		}
		this.entityFlags = EntityFlags.None;
		EntityClass.ParseEntityFlags(this.Properties.GetString(EntityClass.PropEntityFlags), ref this.entityFlags);
		if (this.Properties.Values.ContainsKey(EntityClass.PropClass))
		{
			this.classname = Type.GetType(this.Properties.Values[EntityClass.PropClass]);
			if (this.classname == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"Could not instantiate class",
					this.Properties.Values[EntityClass.PropClass],
					"' for entity_class '",
					this.entityClassName,
					"'"
				}));
			}
		}
		this.modelType = typeof(EModelCustom);
		string @string = this.Properties.GetString(EntityClass.PropModelType);
		if (@string.Length > 0)
		{
			this.modelType = ReflectionHelpers.GetTypeWithPrefix("EModel", @string);
			if (this.modelType == null)
			{
				throw new Exception("Model class '" + @string + "' not found!");
			}
		}
		string string2 = this.Properties.GetString(EntityClass.PropAltMats);
		if (string2.Length > 0)
		{
			this.AltMatNames = string2.Split(',', StringSplitOptions.None);
		}
		string string3 = this.Properties.GetString(EntityClass.PropSwapMats);
		if (string3.Length > 0)
		{
			this.MatSwap = string3.Split(",", StringSplitOptions.None);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropParticleOnSpawn))
		{
			this.particleOnSpawn.fileName = this.Properties.Values[EntityClass.PropParticleOnSpawn];
			this.particleOnSpawn.shapeMesh = this.Properties.Params1[EntityClass.PropParticleOnSpawn];
			DataLoader.PreloadBundle(this.particleOnSpawn.fileName);
		}
		this.RagdollOnDeathChance = 0.5f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropRagdollOnDeathChance))
		{
			this.RagdollOnDeathChance = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropRagdollOnDeathChance], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropHasRagdoll))
		{
			this.HasRagdoll = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropHasRagdoll], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropColliders))
		{
			this.CollidersRagdollAsset = this.Properties.Values[EntityClass.PropColliders];
			DataLoader.PreloadBundle(this.CollidersRagdollAsset);
		}
		this.Properties.ParseFloat(EntityClass.PropLookAtAngle, ref this.LookAtAngle);
		if (this.Properties.Values.ContainsKey(EntityClass.PropCrouchYOffsetFP))
		{
			this.crouchYOffsetFP = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropCrouchYOffsetFP], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropParent))
		{
			this.parentGameObjectName = this.Properties.Values[EntityClass.PropParent];
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSkinTexture))
		{
			this.skinTexture = this.Properties.Values[EntityClass.PropSkinTexture];
			DataLoader.PreloadBundle(this.skinTexture);
		}
		this.bIsEnemyEntity = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsEnemyEntity))
		{
			this.bIsEnemyEntity = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsEnemyEntity], 0, -1, true);
		}
		this.bIsAnimalEntity = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsAnimalEntity))
		{
			this.bIsAnimalEntity = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsAnimalEntity], 0, -1, true);
		}
		this.CorpseBlockId = null;
		if (this.Properties.Values.ContainsKey(EntityClass.PropCorpseBlock))
		{
			this.CorpseBlockId = this.Properties.Values[EntityClass.PropCorpseBlock];
		}
		this.CorpseBlockChance = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropCorpseBlockChance))
		{
			this.CorpseBlockChance = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropCorpseBlockChance], 0, -1, NumberStyles.Any);
		}
		this.CorpseBlockDensity = (int)MarchingCubes.DensityTerrain;
		if (this.Properties.Values.ContainsKey(EntityClass.PropCorpseBlockDensity))
		{
			this.CorpseBlockDensity = int.Parse(this.Properties.Values[EntityClass.PropCorpseBlockDensity]);
			this.CorpseBlockDensity = Math.Max(-128, Math.Min(127, this.CorpseBlockDensity));
		}
		this.RootMotion = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropRootMotion))
		{
			this.RootMotion = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropRootMotion], 0, -1, true);
		}
		this.HasDeathAnim = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropHasDeathAnim))
		{
			this.HasDeathAnim = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropHasDeathAnim], 0, -1, true);
		}
		this.ExperienceValue = 100;
		if (this.Properties.Values.ContainsKey(EntityClass.PropExperienceGain))
		{
			this.ExperienceValue = (int)StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropExperienceGain], 0, -1, NumberStyles.Any);
		}
		string string4 = this.Properties.GetString(EntityClass.PropLootDropEntityClass);
		if (string4.Length > 0)
		{
			this.lootDropEntityClass = EntityClass.FromString(string4);
		}
		this.bIsMale = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsMale))
		{
			this.bIsMale = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsMale], 0, -1, true);
		}
		this.bIsChunkObserver = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsChunkObserver))
		{
			this.bIsChunkObserver = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsChunkObserver], 0, -1, true);
		}
		this.SightRange = Constants.cDefaultMonsterSeeDistance;
		if (this.Properties.Values.ContainsKey(EntityClass.PropSightRange))
		{
			this.SightRange = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSightRange], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperGroanSightDetectionMin))
		{
			this.GroanMin = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperGroanSightDetectionMin]);
		}
		else
		{
			this.GroanMin = new Vector2(25f, 25f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperGroanSightDetectionMax))
		{
			this.GroanMax = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperGroanSightDetectionMax]);
		}
		else
		{
			this.GroanMax = new Vector2(200f, 200f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperWakeupSightDetectionMin))
		{
			this.WakeMin = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperWakeupSightDetectionMin]);
		}
		else
		{
			this.WakeMin = new Vector2(15f, 15f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperWakeupSightDetectionMax))
		{
			this.WakeMax = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperWakeupSightDetectionMax]);
		}
		else
		{
			this.WakeMax = new Vector2(200f, 200f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSightLightThreshold))
		{
			this.sightLightThreshold = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSightLightThreshold]);
		}
		else
		{
			this.sightLightThreshold = new Vector2(30f, 100f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperNoiseGroanThreshold))
		{
			this.NoiseGroan = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperNoiseGroanThreshold]);
		}
		else
		{
			this.NoiseGroan = new Vector2(15f, 15f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperNoiseWakeThreshold))
		{
			this.NoiseWake = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperNoiseWakeThreshold]);
		}
		else
		{
			this.NoiseWake = new Vector2(15f, 15f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperSmellGroanThreshold))
		{
			this.SmellGroan = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperSmellGroanThreshold]);
		}
		else
		{
			this.SmellGroan = new Vector2(15f, 15f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSleeperSmellWakeThreshold))
		{
			this.SmellWake = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSleeperSmellWakeThreshold]);
		}
		else
		{
			this.SmellWake = new Vector2(15f, 15f);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSoundSleeperGroanChance))
		{
			this.GroanChance = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSoundSleeperGroanChance], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.GroanChance = 1f;
		}
		this.MassKg = 10f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropMass))
		{
			this.MassKg = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropMass], 0, -1, NumberStyles.Any);
		}
		this.MassKg *= 0.454f;
		this.SizeScale = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropSizeScale))
		{
			this.SizeScale = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSizeScale], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropPhysicsBody))
		{
			this.PhysicsBody = PhysicsBodyLayout.Find(this.Properties.Values[EntityClass.PropPhysicsBody]);
		}
		if (this.Properties.Values.ContainsKey("DeadBodyHitPoints"))
		{
			this.DeadBodyHitPoints = int.Parse(this.Properties.Values["DeadBodyHitPoints"]);
		}
		this.Properties.ParseFloat(EntityClass.PropLegCrippleScale, ref this.LegCrippleScale);
		this.Properties.ParseFloat(EntityClass.PropLegCrawlerThreshold, ref this.LegCrawlerThreshold);
		this.DismemberMultiplierHead = 1f;
		this.Properties.ParseFloat(EntityClass.PropDismemberMultiplierHead, ref this.DismemberMultiplierHead);
		this.DismemberMultiplierArms = 1f;
		this.Properties.ParseFloat(EntityClass.PropDismemberMultiplierArms, ref this.DismemberMultiplierArms);
		this.DismemberMultiplierLegs = 1f;
		this.Properties.ParseFloat(EntityClass.PropDismemberMultiplierLegs, ref this.DismemberMultiplierLegs);
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownKneelDamageThreshold))
		{
			this.KnockdownKneelDamageThreshold = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropKnockdownKneelDamageThreshold], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownKneelStunDuration))
		{
			this.KnockdownKneelStunDuration = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownKneelStunDuration]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownProneDamageThreshold))
		{
			this.KnockdownProneDamageThreshold = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropKnockdownProneDamageThreshold], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownProneStunDuration))
		{
			this.KnockdownProneStunDuration = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownProneStunDuration]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownKneelRefillRate))
		{
			this.KnockdownKneelRefillRate = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownKneelRefillRate]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownProneRefillRate))
		{
			this.KnockdownProneRefillRate = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownProneRefillRate]);
		}
		this.LegsExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropLegsExplosionDamageMultiplier))
		{
			this.LegsExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropLegsExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.ArmsExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropArmsExplosionDamageMultiplier))
		{
			this.ArmsExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropArmsExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.HeadExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropHeadExplosionDamageMultiplier))
		{
			this.HeadExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropHeadExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.ChestExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropChestExplosionDamageMultiplier))
		{
			this.ChestExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropChestExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.PainResistPerHit = 0f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropPainResistPerHit))
		{
			this.PainResistPerHit = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropPainResistPerHit], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropArchetype))
		{
			this.ArchetypeName = this.Properties.Values[EntityClass.PropArchetype];
		}
		this.SwimOffset = 0.9f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropSwimOffset))
		{
			this.SwimOffset = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSwimOffset], 0, -1, NumberStyles.Any);
		}
		this.SearchRadius = 6f;
		this.Properties.ParseFloat(EntityClass.PropSearchRadius, ref this.SearchRadius);
		if (this.Properties.Values.ContainsKey(EntityClass.PropUMARace))
		{
			this.UMARace = this.Properties.Values[EntityClass.PropUMARace];
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropUMAGeneratedModelName))
		{
			this.UMAGeneratedModelName = this.Properties.Values[EntityClass.PropUMAGeneratedModelName];
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropModelTransformAdjust))
		{
			this.ModelTransformAdjust = StringParsers.ParseVector3(this.Properties.Values[EntityClass.PropModelTransformAdjust], 0, -1);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropAIPackages))
		{
			this.AIPackages = this.Properties.Values[EntityClass.PropAIPackages].Split(',', StringSplitOptions.None);
			for (int i = 0; i < this.AIPackages.Length; i++)
			{
				this.AIPackages[i] = this.AIPackages[i].Trim();
			}
			this.UseAIPackages = true;
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropBuffs))
		{
			string[] array = this.Properties.Values[EntityClass.PropBuffs].Split(new char[]
			{
				';'
			}, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0)
			{
				this.Buffs = new List<string>(array);
			}
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropMaxTurnSpeed))
		{
			this.MaxTurnSpeed = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropMaxTurnSpeed], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropTags))
		{
			this.Tags = FastTags<TagGroup.Global>.Parse(this.Properties.Values[EntityClass.PropTags]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropNavObject))
		{
			this.NavObject = this.Properties.Values[EntityClass.PropNavObject];
		}
		this.Properties.ParseVec(EntityClass.PropNavObjectHeadOffset, ref this.NavObjectHeadOffset);
		this.explosionData = new ExplosionData(this.Properties, this.Effects);
		bool flag = false;
		this.Properties.ParseBool(EntityClass.PropHideInSpawnMenu, ref flag);
		if (flag)
		{
			this.userSpawnType = EntityClass.UserSpawnType.Console;
		}
		this.Properties.ParseEnum<EntityClass.UserSpawnType>(EntityClass.PropUserSpawnType, ref this.userSpawnType);
		this.Properties.ParseBool(EntityClass.PropCanBigHead, ref this.CanBigHead);
		this.Properties.ParseInt(EntityClass.PropDanceType, ref this.DanceTypeID);
		return this;
	}

	public void CopyFrom(EntityClass _other, HashSet<string> _exclude = null)
	{
		foreach (KeyValuePair<string, string> keyValuePair in _other.Properties.Values.Dict)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair.Key))
			{
				this.Properties.Values[keyValuePair.Key] = _other.Properties.Values[keyValuePair.Key];
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair2 in _other.Properties.Params1.Dict)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair2.Key))
			{
				this.Properties.Params1[keyValuePair2.Key] = keyValuePair2.Value;
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair3 in _other.Properties.Params2.Dict)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair3.Key))
			{
				this.Properties.Params2[keyValuePair3.Key] = keyValuePair3.Value;
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair4 in _other.Properties.Data.Dict)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair4.Key))
			{
				this.Properties.Data[keyValuePair4.Key] = keyValuePair4.Value;
			}
		}
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair5 in _other.Properties.Classes.Dict)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair5.Key))
			{
				DynamicProperties dynamicProperties = new DynamicProperties();
				dynamicProperties.CopyFrom(keyValuePair5.Value, null);
				this.Properties.Classes[keyValuePair5.Key] = dynamicProperties;
			}
		}
	}

	public static void ParseEntityFlags(string _names, ref EntityFlags optionalValue)
	{
		if (_names.Length > 0)
		{
			if (_names.IndexOf(',') >= 0)
			{
				string[] array = _names.Split(EntityClass.commaSeparator, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					EntityFlags entityFlags;
					if (EnumUtils.TryParse<EntityFlags>(array[i], out entityFlags, true))
					{
						optionalValue |= entityFlags;
					}
				}
				return;
			}
			EntityFlags entityFlags2;
			if (EnumUtils.TryParse<EntityFlags>(_names, out entityFlags2, true))
			{
				optionalValue = entityFlags2;
			}
		}
	}

	public static void Cleanup()
	{
		EntityClass.list.Clear();
	}

	public void AddDroppedId(EnumDropEvent _eEvent, string _name, int _minCount, int _maxCount, float _prob, float _stickChance, string _toolCategory, string _tag)
	{
		List<Block.SItemDropProb> list = this.itemsToDrop.ContainsKey(_eEvent) ? this.itemsToDrop[_eEvent] : null;
		if (list == null)
		{
			list = new List<Block.SItemDropProb>();
			this.itemsToDrop[_eEvent] = list;
		}
		list.Add(new Block.SItemDropProb(_name, _minCount, _maxCount, _prob, 1f, _stickChance, _toolCategory, _tag));
	}

	public static string PropEntityFlags = "EntityFlags";

	public static string PropEntityType = "EntityType";

	public static string PropPrefab = "Prefab";

	public static string PropMesh = "Mesh";

	public static string PropMeshFP = "MeshFP";

	public static string PropClass = "Class";

	public static string PropParent = "Parent";

	public static string PropAvatarController = "AvatarController";

	public static string PropLocalAvatarController = "LocalAvatarController";

	public static string PropSkinTexture = "SkinTexture";

	public static string PropAltMats = "AltMats";

	public static string PropSwapMats = "SwapMats";

	public static string PropRightHandJointName = "RightHandJointName";

	public static string PropHandItem = "HandItem";

	public static string PropHandItemCrawler = "HandItemCrawler";

	public static string PropMaxHealth = "MaxHealth";

	public static string PropMaxStamina = "MaxStamina";

	public static string PropSickness = "Sickness";

	public static string PropGassiness = "Gassiness";

	public static string PropWellness = "Wellness";

	public static string PropFood = "Food";

	public static string PropWater = "Water";

	public static string PropMaxViewAngle = "MaxViewAngle";

	public static string PropWeight = "Weight";

	public static string PropPushFactor = "PushFactor";

	public static string PropTimeStayAfterDeath = "TimeStayAfterDeath";

	public static string PropImmunity = "Immunity";

	public static string PropIsMale = "IsMale";

	public static string PropIsChunkObserver = "IsChunkObserver";

	public static string PropAIFeralSense = "AIFeralSense";

	public static string PropAIGroupCircle = "AIGroupCircle";

	public static string PropAINoiseSeekDist = "AINoiseSeekDist";

	public static string PropAISeeOffset = "AISeeOffset";

	public static string PropAIPathCostScale = "AIPathCostScale";

	public static string PropAITask = "AITask-";

	public static string PropAITargetTask = "AITarget-";

	public static string PropMoveSpeed = "MoveSpeed";

	public static string PropMoveSpeedNight = "MoveSpeedNight";

	public static string PropMoveSpeedAggro = "MoveSpeedAggro";

	public static string PropMoveSpeedRand = "MoveSpeedRand";

	public static string PropMoveSpeedPanic = "MoveSpeedPanic";

	public static string PropSwimSpeed = "SwimSpeed";

	public static string PropSwimStrokeRate = "SwimStrokeRate";

	public static string PropWalkType = "WalkType";

	public static string PropDeathType = "DeathType";

	public static string PropCanClimbVertical = "CanClimbVertical";

	public static string PropCanClimbLadders = "CanClimbLadders";

	public static string PropJumpDelay = "JumpDelay";

	public static string PropJumpMaxDistance = "JumpMaxDistance";

	public static string PropIsEnemyEntity = "IsEnemyEntity";

	public static string PropIsAnimalEntity = "IsAnimalEntity";

	public static string PropSoundRandomTime = "SoundRandomTime";

	public static string PropSoundAlertTime = "SoundAlertTime";

	public static string PropSoundRandom = "SoundRandom";

	public static string PropSoundHurt = "SoundHurt";

	public static string PropSoundJump = "SoundJump";

	public static string PropSoundHurtSmall = "SoundHurtSmall";

	public static string PropSoundDrownPain = "SoundDrownPain";

	public static string PropSoundDrownDeath = "SoundDrownDeath";

	public static string PropSoundWaterSurface = "SoundWaterSurface";

	public static string PropSoundDeath = "SoundDeath";

	public static string PropSoundAttack = "SoundAttack";

	public static string PropSoundAlert = "SoundAlert";

	public static string PropSoundSense = "SoundSense";

	public static string PropSoundStamina = "SoundStamina";

	public static string PropSoundLiving = "SoundLiving";

	public static string PropSoundSpawn = "SoundSpawn";

	public static string PropSoundLand = "SoundLanding";

	public static string PropSoundFootstepModifier = "SoundFootstepModifier";

	public static string PropSoundGiveUp = "SoundGiveUp";

	public static string PropSoundExplodeWarn = "SoundExplodeWarn";

	public static string PropSoundTick = "SoundTick";

	public static string PropExplodeDelay = "ExplodeDelay";

	public static string PropExplodeHealthThreshold = "ExplodeHealthThreshold";

	public static string PropLootListOnDeath = "LootListOnDeath";

	public static string PropLootListAlive = "LootListAlive";

	public static string PropLootDropProb = "LootDropProb";

	public static string PropLootDropEntityClass = "LootDropEntityClass";

	public static string PropAttackTimeoutDay = "AttackTimeoutDay";

	public static string PropAttackTimeoutNight = "AttackTimeoutNight";

	public static string PropMapIcon = "MapIcon";

	public static string PropCompassIcon = "CompassIcon";

	public static string PropTrackerIcon = "TrackerIcon";

	public static string PropCompassUpIcon = "CompassUpIcon";

	public static string PropCompassDownIcon = "CompassDownIcon";

	public static string PropParticleOnSpawn = "ParticleOnSpawn";

	public static string PropParticleOnDeath = "ParticleOnDeath";

	public static string PropParticleOnDestroy = "ParticleOnDestroy";

	public static string PropItemsOnEnterGame = "ItemsOnEnterGame";

	public static string PropFallLandBehavior = "FallLandBehavior";

	public static string PropDestroyBlockBehavior = "DestroyBlockBehavior";

	public static string PropDropInventoryBlock = "DropInventoryBlock";

	public static string PropModelType = "ModelType";

	public static string PropRagdollOnDeathChance = "RagdollOnDeathChance";

	public static string PropHasRagdoll = "HasRagdoll";

	public static string PropMass = "Mass";

	public static string PropSizeScale = "SizeScale";

	public static string PropPhysicsBody = "PhysicsBody";

	public static string PropColliders = "Colliders";

	public static string PropLookAtAngle = "LookAtAngle";

	public static string PropCrouchYOffsetFP = "CrouchYOffsetFP";

	public static string PropRotateToGround = "RotateToGround";

	public static string PropCorpseBlock = "CorpseBlock";

	public static string PropCorpseBlockChance = "CorpseBlockChance";

	public static string PropCorpseBlockDensity = "CorpseBlockDensity";

	public static string PropRootMotion = "RootMotion";

	public static string PropExperienceGain = "ExperienceGain";

	public static string PropHasDeathAnim = "HasDeathAnim";

	public static string PropLegCrippleScale = "LegCrippleScale";

	public static string PropLegCrawlerThreshold = "LegCrawlerThreshold";

	public static string PropDismemberMultiplierHead = "DismemberMultiplierHead";

	public static string PropDismemberMultiplierArms = "DismemberMultiplierArms";

	public static string PropDismemberMultiplierLegs = "DismemberMultiplierLegs";

	public static string PropKnockdownKneelDamageThreshold = "KnockdownKneelDamageThreshold";

	public static string PropKnockdownKneelStunDuration = "KnockdownKneelStunDuration";

	public static string PropKnockdownProneDamageThreshold = "KnockdownProneDamageThreshold";

	public static string PropKnockdownProneStunDuration = "KnockdownProneStunDuration";

	public static string PropKnockdownProneRefillRate = "KnockdownProneRefillRate";

	public static string PropKnockdownKneelRefillRate = "KnockdownKneelRefillRate";

	public static string PropArmsExplosionDamageMultiplier = "ArmsExplosionDamageMultiplier";

	public static string PropLegsExplosionDamageMultiplier = "LegsExplosionDamageMultiplier";

	public static string PropChestExplosionDamageMultiplier = "ChestExplosionDamageMultiplier";

	public static string PropHeadExplosionDamageMultiplier = "HeadExplosionDamageMultiplier";

	public static string PropPainResistPerHit = "PainResistPerHit";

	public static string PropArchetype = "Archetype";

	public static string PropSwimOffset = "SwimOffset";

	public static string PropUMARace = "UMARace";

	public static string PropUMAGeneratedModelName = "UMAGeneratedModelName";

	public static string PropNPCID = "NPCID";

	public static string PropModelTransformAdjust = "ModelTransformAdjust";

	public static string PropAIPackages = "AIPackages";

	public static string PropBuffs = "Buffs";

	public static string PropStealthSoundDecayRate = "StealthSoundDecayRate";

	public static string PropSightRange = "SightRange";

	public static string PropSleeperWakeupSightDetectionMin = "SleeperWakeupSightDetectionMin";

	public static string PropSleeperWakeupSightDetectionMax = "SleeperWakeupSightDetectionMax";

	public static string PropSleeperGroanSightDetectionMin = "SleeperSenseSightDetectionMin";

	public static string PropSleeperGroanSightDetectionMax = "SleeperSenseSightDetectionMax";

	public static string PropSoundSleeperGroan = "SoundSleeperSense";

	public static string PropSoundSleeperSnore = "SoundSleeperBackToSleep";

	public static string PropSightLightThreshold = "SightLightThreshold";

	public static string PropNoiseAlertThreshold = "NoiseAlertThreshold";

	public static string PropSleeperNoiseWakeThreshold = "SleeperNoiseWakeThreshold";

	public static string PropSleeperNoiseGroanThreshold = "SleeperNoiseSenseThreshold";

	public static string PropSmellAlertThreshold = "SmellAlertThreshold";

	public static string PropSleeperSmellWakeThreshold = "SleeperSmellWakeThreshold";

	public static string PropSleeperSmellGroanThreshold = "SleeperSmellSenseThreshold";

	public static string PropSoundSleeperGroanChance = "SoundSleeperSenseChance";

	public static string PropMaxTurnSpeed = "MaxTurnSpeed";

	public static string PropSearchRadius = "SearchRadius";

	public static string PropTags = "Tags";

	public static string PropNavObject = "NavObject";

	public static string PropNavObjectHeadOffset = "NavObjectHeadOffset";

	public static string PropStompsSpikes = "StompsSpikes";

	public static string PropUserSpawnType = "UserSpawnType";

	public static string PropHideInSpawnMenu = "HideInSpawnMenu";

	public static string PropCanBigHead = "CanBigHead";

	public static string PropDanceType = "DanceType";

	public static readonly int itemClass = EntityClass.FromString("item");

	public static readonly int fallingBlockClass = EntityClass.FromString("fallingBlock");

	public static readonly int fallingTreeClass = EntityClass.FromString("fallingTree");

	public static readonly int playerMaleClass = EntityClass.FromString("playerMale");

	public static readonly int playerFemaleClass = EntityClass.FromString("playerFemale");

	public static readonly int playerNewMaleClass = EntityClass.FromString("playerNewMale");

	public static DictionarySave<int, EntityClass> list = new DictionarySave<int, EntityClass>();

	public DynamicProperties Properties = new DynamicProperties();

	public string prefabPath;

	public Transform prefabT;

	public bool IsPrefabCombined;

	public string meshPath;

	public Transform mesh;

	public Transform meshFP;

	public EntityFlags entityFlags;

	public Type classname;

	public string skinTexture;

	public string parentGameObjectName;

	public string entityClassName;

	public EntityClass.UserSpawnType userSpawnType = EntityClass.UserSpawnType.Menu;

	public bool bIsEnemyEntity;

	public bool bIsAnimalEntity;

	public ExplosionData explosionData;

	public Type modelType;

	public float MassKg;

	public float SizeScale;

	public float RagdollOnDeathChance;

	public bool HasRagdoll;

	public string CollidersRagdollAsset;

	public float LookAtAngle;

	public float crouchYOffsetFP;

	public string CorpseBlockId;

	public float CorpseBlockChance;

	public int CorpseBlockDensity;

	public float MaxTurnSpeed;

	public bool RootMotion;

	public bool HasDeathAnim;

	public bool bIsMale;

	public bool bIsChunkObserver;

	public int ExperienceValue;

	public int lootDropEntityClass;

	public PhysicsBodyLayout PhysicsBody;

	public int DeadBodyHitPoints;

	public float LegCrippleScale;

	public float LegCrawlerThreshold;

	public float DismemberMultiplierHead;

	public float DismemberMultiplierArms;

	public float DismemberMultiplierLegs;

	public float LowerLegDismemberThreshold;

	public float LowerLegDismemberBonusChance;

	public float LowerLegDismemberBaseChance;

	public float UpperLegDismemberThreshold;

	public float UpperLegDismemberBonusChance;

	public float UpperLegDismemberBaseChance;

	public float LowerArmDismemberThreshold;

	public float LowerArmDismemberBonusChance;

	public float LowerArmDismemberBaseChance;

	public float UpperArmDismemberThreshold;

	public float UpperArmDismemberBonusChance;

	public float UpperArmDismemberBaseChance;

	public float KnockdownKneelDamageThreshold;

	public float LegsExplosionDamageMultiplier;

	public float ArmsExplosionDamageMultiplier;

	public float ChestExplosionDamageMultiplier;

	public float HeadExplosionDamageMultiplier;

	public float PainResistPerHit;

	public float SearchRadius;

	public float SwimOffset;

	public float SightRange;

	public Vector2 GroanMin;

	public Vector2 GroanMax;

	public Vector2 WakeMin;

	public Vector2 WakeMax;

	public Vector2 sightLightThreshold;

	public Vector2 SmellAlert;

	public Vector2 NoiseAlert;

	public Vector2 SmellWake;

	public Vector2 SmellGroan;

	public Vector2 NoiseWake;

	public Vector2 NoiseGroan;

	public float GroanChance;

	public string UMARace;

	public string UMAGeneratedModelName;

	public string[] AltMatNames;

	public string[] MatSwap;

	public EntityClass.ParticleData particleOnSpawn;

	public Vector2 KnockdownKneelStunDuration;

	public float KnockdownProneDamageThreshold;

	public Vector2 KnockdownProneStunDuration;

	public Vector2 KnockdownProneRefillRate;

	public Vector2 KnockdownKneelRefillRate;

	public Vector3 ModelTransformAdjust;

	public string ArchetypeName;

	public string[] AIPackages;

	public bool UseAIPackages;

	public Dictionary<EnumDropEvent, List<Block.SItemDropProb>> itemsToDrop = new EnumDictionary<EnumDropEvent, List<Block.SItemDropProb>>();

	public List<string> Buffs;

	public FastTags<TagGroup.Global> Tags;

	public string NavObject = "";

	public Vector3 NavObjectHeadOffset = Vector3.zero;

	public bool CanBigHead = true;

	public int DanceTypeID;

	public MinEffectController Effects;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] commaSeparator = new char[]
	{
		','
	};

	public enum UserSpawnType
	{
		None,
		Console,
		Menu
	}

	public struct ParticleData
	{
		public string fileName;

		public string shapeMesh;
	}
}
