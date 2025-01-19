using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFactory
{
	public static void Init(Transform _entitiesTransform)
	{
		EntityFactory.ParentNameToTransform.Clear();
		EntityFactory.FindOrCreateTransform(null, "Players", 0);
		EntityFactory.FindOrCreateTransform(_entitiesTransform, "Items", 0);
		EntityFactory.FindOrCreateTransform(_entitiesTransform, "FallingBlocks", 0);
		EntityFactory.FindOrCreateTransform(_entitiesTransform, "FallingTrees", 0);
		EntityFactory.FindOrCreateTransform(_entitiesTransform, "Enemies", 1);
		EntityFactory.FindOrCreateTransform(_entitiesTransform, "Animals", 1);
		foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
		{
			EntityFactory.FindOrCreateTransform(_entitiesTransform, keyValuePair.Value.parentGameObjectName, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FindOrCreateTransform(Transform _parent, string _name, int _originLevel)
	{
		if (_name == null)
		{
			return;
		}
		if (!EntityFactory.ParentNameToTransform.ContainsKey(_name))
		{
			Transform transform;
			if (_parent != null)
			{
				transform = _parent.Find(_name);
			}
			else
			{
				GameObject gameObject = GameObject.Find("/" + _name);
				transform = ((gameObject != null) ? gameObject.transform : null);
			}
			if (transform == null)
			{
				transform = new GameObject(_name).transform;
				transform.name = _name;
				transform.parent = _parent;
				Origin.Add(transform, _originLevel);
			}
			EntityFactory.ParentNameToTransform[_name] = transform;
		}
	}

	public static void Cleanup()
	{
	}

	public static void CleanupStatic()
	{
		EntityClass.list.Clear();
	}

	public static Type GetEntityType(string _className)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_className);
		if (num <= 1891769587U)
		{
			if (num <= 680637634U)
			{
				if (num != 431260344U)
				{
					if (num == 680637634U)
					{
						if (_className == "EntityAnimalRabbit")
						{
							return typeof(EntityAnimalRabbit);
						}
					}
				}
				else if (_className == "EntityZombie")
				{
					return typeof(EntityZombie);
				}
			}
			else if (num != 1041995262U)
			{
				if (num == 1891769587U)
				{
					if (_className == "EntityPlayer")
					{
						return typeof(EntityPlayer);
					}
				}
			}
			else if (_className == "EntityAnimal")
			{
				return typeof(EntityAnimal);
			}
		}
		else if (num <= 2926901533U)
		{
			if (num != 2057157508U)
			{
				if (num == 2926901533U)
				{
					if (_className == "EntityAnimalStag")
					{
						return typeof(EntityAnimalStag);
					}
				}
			}
			else if (_className == "EntityBandit")
			{
				return typeof(EntityBandit);
			}
		}
		else if (num != 3350477675U)
		{
			if (num != 3522953666U)
			{
				if (num == 4192015845U)
				{
					if (_className == "EntityNPC")
					{
						return typeof(EntityNPC);
					}
				}
			}
			else if (_className == "EntityEnemyAnimal")
			{
				return typeof(EntityEnemyAnimal);
			}
		}
		else if (_className == "EntityHuman")
		{
			return typeof(EntityHuman);
		}
		Log.Warning("GetEntityType slow lookup for {0}", new object[]
		{
			_className
		});
		return Type.GetType(_className);
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos)
	{
		return EntityFactory.CreateEntity(_et, EntityFactory.nextEntityID++, _transformPos, Vector3.zero);
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos, Vector3 _rotation)
	{
		return EntityFactory.CreateEntity(_et, EntityFactory.nextEntityID++, _transformPos, _rotation);
	}

	public static Entity CreateEntity(int _et, int _id, Vector3 _transformPos, Vector3 _rotation)
	{
		return EntityFactory.CreateEntity(_et, _id, ItemValue.None.Clone(), 1, _transformPos, _rotation, float.MaxValue, -1, null, -1, "");
	}

	public static Entity CreateEntity(int _et, Vector3 _transformPos, Vector3 _rotation, int _spawnById, string _spawnByName)
	{
		return EntityFactory.CreateEntity(_et, EntityFactory.nextEntityID++, ItemValue.None.Clone(), 1, _transformPos, _rotation, float.MaxValue, -1, null, _spawnById, _spawnByName);
	}

	public static Entity CreateEntity(int _et, int _id, BlockValue _blockValue, long _textureFull, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, string _skinName, int _spawnById = -1, string _spawnByName = "")
	{
		return EntityFactory.CreateEntity(new EntityCreationData
		{
			entityClass = _et,
			id = _id,
			blockValue = _blockValue,
			textureFull = _textureFull,
			itemStack = 
			{
				count = _count
			},
			pos = _transformPos,
			rot = _transformRot,
			lifetime = _lifetime,
			belongsPlayerId = _playerId,
			spawnById = _spawnById,
			spawnByName = _spawnByName
		});
	}

	public static Entity CreateEntity(int _et, int _id, ItemValue _itemValue, int _count, Vector3 _transformPos, Vector3 _transformRot, float _lifetime, int _playerId, string _skinName, int _spawnById = -1, string _spawnByName = "")
	{
		return EntityFactory.CreateEntity(new EntityCreationData
		{
			entityClass = _et,
			id = _id,
			itemStack = new ItemStack(_itemValue, _count),
			pos = _transformPos,
			rot = _transformRot,
			lifetime = _lifetime,
			belongsPlayerId = _playerId,
			spawnById = _spawnById,
			spawnByName = _spawnByName
		});
	}

	public static Entity CreateEntity(EntityCreationData _ecd)
	{
		if (_ecd.id == -1)
		{
			_ecd.id = EntityFactory.nextEntityID++;
		}
		else
		{
			EntityFactory.nextEntityID = Math.Max(_ecd.id + 1, EntityFactory.nextEntityID);
		}
		EntityClass entityClass = EntityClass.GetEntityClass(_ecd.entityClass);
		if (entityClass == null)
		{
			Log.Error("EntityFactory CreateEntity: unknown type ({0}) {1}", new object[]
			{
				_ecd.entityClass,
				_ecd.entityName
			});
			return null;
		}
		bool flag = _ecd.entityClass == EntityClass.playerMaleClass || _ecd.entityClass == EntityClass.playerFemaleClass;
		bool flag2 = flag && _ecd.id == _ecd.belongsPlayerId;
		Transform transform;
		if (flag2)
		{
			transform = Resources.Load<Transform>("Prefabs/prefabEntityPlayerLocal");
		}
		else
		{
			EntityFactory.LoadPrefabs(entityClass);
			transform = entityClass.prefabT;
		}
		transform = UnityEngine.Object.Instantiate<Transform>(transform, Vector3.zero, Quaternion.identity);
		Transform transform2 = transform;
		Transform transform3 = transform.Find("GameObject");
		if (transform3)
		{
			transform2 = transform3;
		}
		transform2.position = _ecd.pos - Origin.position;
		GameObject gameObject = transform2.gameObject;
		Entity entity;
		if (flag)
		{
			EntityPlayer entityPlayer;
			if (flag2)
			{
				entity = EntityFactory.addEntityComponent(gameObject, entityClass.classname.FullName + "Local");
				entityPlayer = (EntityPlayer)entity;
				entity.RootTransform = transform;
				entity.ModelTransform = transform.Find("Graphics");
				entity.PhysicsTransform = transform;
				entityPlayer.playerProfile = _ecd.playerProfile;
				entity.Init(_ecd.entityClass);
				gameObject.AddComponent<LocalPlayer>();
			}
			else
			{
				entity = EntityFactory.addEntityComponent(gameObject, entityClass.classname);
				entityPlayer = (EntityPlayer)entity;
				entity.RootTransform = transform;
				entity.ModelTransform = transform2;
				entity.PhysicsTransform = transform.Find("Physics");
				entityPlayer.playerProfile = _ecd.playerProfile;
				entity.Init(_ecd.entityClass);
				gameObject.AddComponent<GUIHUDEntityName>();
			}
			if (!_ecd.holdingItem.IsEmpty())
			{
				entityPlayer.inventory.AddItem(new ItemStack(_ecd.holdingItem, 1));
				entityPlayer.inventory.SetHoldingItemIdx(0);
			}
			entityPlayer.TeamNumber = _ecd.teamNumber;
			entityPlayer.emodel.SetSkinTexture(_ecd.skinTexture);
			transform.SetParent(EntityFactory.ParentNameToTransform[entityClass.parentGameObjectName], false);
			transform.name = "Player_" + _ecd.id.ToString();
			Log.Out("Created player with id=" + _ecd.id.ToString());
		}
		else if (_ecd.entityClass == EntityClass.itemClass)
		{
			Entity entity2 = entity = gameObject.AddComponent<EntityItem>();
			entity.RootTransform = transform;
			entity.ModelTransform = transform2;
			entity.clientEntityId = _ecd.clientEntityId;
			entity2.OwnerId = _ecd.belongsPlayerId;
			entity.Init(_ecd.entityClass);
			transform.SetParent(EntityFactory.ParentNameToTransform["Items"], false);
			transform.name = "Item_" + _ecd.id.ToString();
			entity2.SetItemStack(_ecd.itemStack);
		}
		else if (_ecd.entityClass == EntityClass.fallingBlockClass)
		{
			Entity entity3 = entity = gameObject.AddComponent<EntityFallingBlock>();
			entity.RootTransform = transform;
			entity.ModelTransform = transform2;
			entity.Init(_ecd.entityClass);
			transform.SetParent(EntityFactory.ParentNameToTransform["FallingBlocks"], false);
			transform.name = "FallingBlock_" + _ecd.id.ToString();
			entity3.SetBlockValue(_ecd.blockValue);
			entity3.SetTextureFull(_ecd.textureFull);
		}
		else if (_ecd.entityClass == EntityClass.fallingTreeClass)
		{
			EntityFallingTree entityFallingTree = entity = gameObject.AddComponent<EntityFallingTree>();
			entity.RootTransform = transform;
			entity.ModelTransform = transform2;
			entity.Init(_ecd.entityClass);
			transform.SetParent(EntityFactory.ParentNameToTransform["FallingTrees"], false);
			transform.name = "FallingTree_" + _ecd.id.ToString();
			entityFallingTree.SetBlockPos(_ecd.blockPos, _ecd.fallTreeDir);
		}
		else
		{
			if (entityClass.classname == null)
			{
				Log.Error("Unknown entity " + _ecd.entityClass.ToString());
				return null;
			}
			entity = EntityFactory.addEntityComponent(gameObject, entityClass.classname);
			if (!entity)
			{
				return null;
			}
			transform2.eulerAngles = _ecd.rot;
			entity.entityId = _ecd.id;
			entity.RootTransform = transform;
			entity.ModelTransform = transform2;
			entity.Init(_ecd.entityClass);
			if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks) && entity is EntityAlive)
			{
				gameObject.AddComponent<GUIHUDEntityName>();
			}
			if (entityClass.parentGameObjectName != null)
			{
				transform.SetParent(EntityFactory.ParentNameToTransform[entityClass.parentGameObjectName], false);
			}
			transform.name = entityClass.entityClassName + "_" + _ecd.id.ToString();
			entity.SetEntityName(entityClass.entityClassName);
			entity.emodel.SetSkinTexture(entityClass.skinTexture);
			CapsuleCollider[] componentsInChildren = transform.GetComponentsInChildren<CapsuleCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameObject gameObject2 = componentsInChildren[i].gameObject;
				if (!gameObject2.CompareTag("LargeEntityBlocker") && !gameObject2.CompareTag("Physics"))
				{
					gameObject2.layer = 14;
				}
			}
			BoxCollider[] componentsInChildren2 = transform.GetComponentsInChildren<BoxCollider>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].gameObject.layer = 14;
			}
		}
		_ecd.ApplyToEntity(entity);
		if (entity.GetSpawnerSource() == EnumSpawnerSource.Delete)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
			return null;
		}
		entity.lifetime = _ecd.lifetime;
		entity.entityId = _ecd.id;
		entity.belongsPlayerId = _ecd.belongsPlayerId;
		entity.InitLocation(_ecd.pos, _ecd.rot);
		entity.onGround = _ecd.onGround;
		if (entityClass.SizeScale != 1f)
		{
			entity.SetScale(entityClass.SizeScale);
		}
		if (_ecd.overrideSize != 1f)
		{
			entity.SetScale(_ecd.overrideSize);
		}
		if (_ecd.overrideHeadSize != 1f)
		{
			EntityAlive entityAlive = entity as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.SetHeadSize(_ecd.overrideHeadSize);
			}
		}
		entity.PostInit();
		return entity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadPrefabs(EntityClass ec)
	{
		if (!ec.prefabT)
		{
			ec.prefabT = DataLoader.LoadAsset<Transform>(ec.prefabPath);
			if (!ec.prefabT)
			{
				Log.Error(string.Concat(new string[]
				{
					"Could not load file '",
					ec.prefabPath,
					"' for entity_class '",
					ec.entityClassName,
					"'"
				}));
				return;
			}
			MeshLodOptimization.Apply(ref ec.prefabT);
		}
		if (!ec.mesh && !string.IsNullOrEmpty(ec.meshPath))
		{
			ec.mesh = DataLoader.LoadAsset<Transform>(ec.meshPath);
			if (!ec.mesh)
			{
				Log.Error(string.Concat(new string[]
				{
					"Could not load file '",
					ec.meshPath,
					"' for entity_class '",
					ec.entityClassName,
					"'"
				}));
				return;
			}
			MeshLodOptimization.Apply(ref ec.mesh);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Entity addEntityComponent(GameObject _gameObject, string _className)
	{
		return EntityFactory.addEntityComponent(_gameObject, Type.GetType(_className));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Entity addEntityComponent(GameObject _gameObject, Type _classType)
	{
		if (_classType != null)
		{
			return (Entity)_gameObject.AddComponent(_classType);
		}
		return null;
	}

	public static int nextEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNumberOfCachedFallingBlocks = 150;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNumberOfCachedItems = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFirstEntityID = 1;

	public const int StartEntityID = 171;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int playerNewMaleClass = EntityClass.FromString("playerNewMale");

	public static Dictionary<string, Transform> ParentNameToTransform = new Dictionary<string, Transform>();
}
