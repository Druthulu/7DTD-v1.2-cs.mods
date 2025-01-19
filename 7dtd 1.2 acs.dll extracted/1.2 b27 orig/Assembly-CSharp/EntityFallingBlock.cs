using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingBlock : Entity
{
	public override Entity.EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Entity.EnumPositionUpdateMovementType.Instant;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.yOffset = 0.15f;
		Transform transform = base.transform;
		this.rigidBody = transform.GetComponent<Rigidbody>();
		if (this.isEntityRemote)
		{
			UnityEngine.Object.Destroy(this.rigidBody);
		}
		transform.GetComponent<BoxCollider>().enabled = false;
		transform.GetComponent<SphereCollider>().enabled = false;
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		base.InitLocation(_pos, _rot);
		if (!this.isEntityRemote)
		{
			this.rigidBody.position = _pos - Origin.position;
			this.rigidBody.rotation = Quaternion.Euler(_rot);
		}
	}

	public BlockValue GetBlockValue()
	{
		return this.blockValue;
	}

	public void SetBlockValue(BlockValue _blockValue)
	{
		this.blockValue = _blockValue;
		this.isTerrain = this.blockValue.Block.shape.IsTerrain();
		if (this.isTerrain)
		{
			this.terrainScale = this.rand.RandomRange(0.3f, 0.98f);
			this.myCollider = base.transform.GetComponent<SphereCollider>();
			return;
		}
		this.myCollider = base.transform.GetComponent<BoxCollider>();
	}

	public long GetTextureFull()
	{
		return this.textureFull;
	}

	public void SetTextureFull(long _textureFull)
	{
		this.textureFull = _textureFull;
	}

	public void SetStartVelocity(Vector3 _vel, float _angularVel)
	{
		this.startVel = _vel;
		this.startAngularVel = _angularVel;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (!this.isMeshCreated && !GameManager.IsDedicatedServer)
		{
			this.CreateMesh();
			if (this.meshRenderer)
			{
				this.isMeshCreated = true;
				this.meshRenderer.enabled = true;
				if (this.isTerrain)
				{
					base.transform.localScale = new Vector3(this.terrainScale, this.terrainScale, this.terrainScale);
					TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
					int sideTextureId = this.blockValue.Block.GetSideTextureId(this.blockValue, BlockFace.Top);
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					materialPropertyBlock.SetTexture("_MainTex", textureAtlasTerrain.diffuse[sideTextureId]);
					materialPropertyBlock.SetTexture("_BumpMap", textureAtlasTerrain.normal[sideTextureId]);
					this.meshRenderer.SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
		if (this.myCollider && !this.myCollider.enabled)
		{
			this.myCollider.enabled = true;
			Block block = this.blockValue.Block;
			this.massKg = Utils.FastMin(block.blockMaterial.Hardness.Value * (float)block.blockMaterial.Mass.Value, 10f) * 8f;
			this.massKg *= ((!this.isTerrain) ? (block.isMultiBlock ? 2.2f : 1f) : (this.terrainScale * this.terrainScale * 1.5f));
			if (!this.isEntityRemote)
			{
				this.rigidBody.mass = Utils.FastMax(10f, this.massKg);
				this.rigidBody.velocity = this.startVel;
				this.rigidBody.angularVelocity = this.rand.RandomOnUnitSphere * this.startAngularVel;
			}
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.IsDead())
		{
			return;
		}
		this.fallTimeInTicks++;
		Block block = this.blockValue.Block;
		if (this.fallTimeInTicks == 1 && Time.time - EntityFallingBlock.lastTimeStartParticleSpawned > 0.2f)
		{
			EntityFallingBlock.lastTimeStartParticleSpawned = Time.time;
			if (!GameManager.IsDedicatedServer && this.prefabParticleOnFallT)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.prefabParticleOnFallT.gameObject, base.transform.position, Quaternion.identity).GetComponent<ParticleSystem>().Emit(10);
			}
		}
		if (this.isEntityRemote)
		{
			return;
		}
		Vector3 velocity = this.rigidBody.velocity;
		if ((this.fallTimeInTicks & 1) == 0)
		{
			List<Entity> entitiesInBounds = this.world.GetEntitiesInBounds(this, BoundsUtils.ExpandBounds(BoundsUtils.ExpandDirectional(this.boundingBox, this.motion), 0f, 0.2f, 0f));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				Entity entity = entitiesInBounds[i];
				int entityId = entity.entityId;
				int num;
				this.entityHits.TryGetValue(entityId, out num);
				if (num < 3 && entity.CanCollideWith(this) && this.position.y >= entity.getHeadPosition().y && velocity.y < -0.8f)
				{
					float num2 = (float)((int)Utils.FastMin(this.massKg * velocity.y * -0.05f, 40f));
					num2 = EffectManager.GetValue(PassiveEffects.FallingBlockDamage, null, num2, entity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					int num3 = (int)num2;
					entity.DamageEntity(DamageSource.fallingBlock, num3, false, 1f);
					if (num3 >= 1)
					{
						num++;
						this.entityHits[entityId] = num;
					}
					Log.Warning("{0} EntityFallingBlock {1} hit {2}, vel {3}, for {4}", new object[]
					{
						GameManager.frameCount,
						this,
						entity,
						velocity,
						num3
					});
				}
			}
		}
		bool flag = false;
		Transform transform = base.transform;
		if (this.fallTimeInTicks < 60 || velocity.sqrMagnitude > 0.0625f)
		{
			this.notMovingCount = 0;
		}
		else
		{
			int num4 = this.notMovingCount + 1;
			this.notMovingCount = num4;
			if (num4 > 3)
			{
				Vector3i pos = World.worldToBlockPos(this.position + Vector3.down);
				BlockValue block2 = this.world.GetBlock(pos);
				if (!block2.isair && this.world.GetStability(pos) > 0)
				{
					float time = Time.time;
					if (time - EntityFallingBlock.lastTimeEndParticleSpawned > 0.15f)
					{
						EntityFallingBlock.lastTimeEndParticleSpawned = time;
						Block block3 = block;
						string destroyParticle = block3.GetDestroyParticle(this.blockValue);
						if (destroyParticle != null && block3.blockMaterial.SurfaceCategory != null)
						{
							Vector3i blockPos = World.worldToBlockPos(transform.position + new Vector3(0f, 0.5f, 0f) + Origin.position);
							this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(blockPos), this.world.GetLightBrightness(blockPos), block3.GetColorForSide(this.blockValue, BlockFace.Top), block3.blockMaterial.SurfaceCategory + "destroy", null, false), this.entityId, false, false);
						}
					}
					if (GamePrefs.GetBool(EnumGamePrefs.OptionsStabSpawnBlocksOnGround) && (!this.isTerrain || block2.Block.shape.IsTerrain()))
					{
						if (block.HasItemsToDropForEvent(EnumDropEvent.Fall))
						{
							float overallProb = 1f;
							List<Block.SItemDropProb> list = block.itemsToDrop[EnumDropEvent.Fall];
							if (list.Count > 0)
							{
								overallProb = list[0].prob;
							}
							block.DropItemsOnEvent(this.world, this.blockValue, EnumDropEvent.Fall, overallProb, base.GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, false);
						}
						else if (this.fallTimeInTicks < 16)
						{
							block.DropItemsOnEvent(this.world, this.blockValue, EnumDropEvent.Destroy, 0.7f, base.GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, false);
						}
					}
				}
				flag = true;
			}
		}
		if (this.fallTimeInTicks > 300)
		{
			flag = true;
		}
		if (transform.position.y + Origin.position.y < 2f)
		{
			flag = true;
		}
		if (flag)
		{
			this.SetDead();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		int num = this.blockValue.ToItemType();
		ItemClass forId = ItemClass.GetForId(num);
		if (num == 0 || forId == null)
		{
			Log.Error("EntityFallingBlock failed id {0}, type", new object[]
			{
				num
			});
			this.SetDead();
			return;
		}
		Transform transform = base.transform;
		Transform transform2 = null;
		if (this.isTerrain)
		{
			GameObject gameObject = Resources.Load<GameObject>("Entities/Debris/Falling/Terrain1");
			if (gameObject)
			{
				transform2 = UnityEngine.Object.Instantiate<GameObject>(gameObject).transform;
				transform2.SetParent(transform, false);
				transform2.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
		}
		else
		{
			transform2 = forId.CloneModel(this.world, this.blockValue.ToItemValue(), this.position, transform, BlockShape.MeshPurpose.Local, this.textureFull);
		}
		if (!transform2)
		{
			Log.Warning("EntityFallingBlock failed id {0}, mesh", new object[]
			{
				num
			});
			this.SetDead();
			return;
		}
		transform2.rotation = this.blockValue.Block.shape.GetRotation(this.blockValue);
		this.meshRenderer = transform2.GetComponentInChildren<Renderer>();
		if (this.isTerrain)
		{
			return;
		}
		if (this.meshRenderer)
		{
			this.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}
		Collider[] componentsInChildren = transform2.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Animator[] componentsInChildren2 = transform2.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		Utils.SetColliderLayerRecursively(transform.gameObject, 13);
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if (this.meshRenderer)
		{
			this.meshRenderer.enabled = (_distanceSqr < (float)(_masterIsZooming ? 14400 : 10000));
		}
	}

	public override bool CanCollideWith(Entity _other)
	{
		return _other is EntityAlive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		Transform transform = base.transform;
		if (this.isEntityRemote)
		{
			if (this.targetPos.sqrMagnitude > 0f && (this.targetPos - this.interpEndPos).sqrMagnitude > 0.0001f)
			{
				this.interpStartPos = transform.position + Origin.position;
				this.interpEndPos = this.targetPos;
				this.interpTime = 0.1f;
			}
			Vector3 a = this.targetPos;
			float deltaTime = Time.deltaTime;
			if (this.interpTime > 0f)
			{
				this.interpTime -= deltaTime;
				float t = 1f - this.interpTime / 0.1f;
				a = Vector3.Lerp(this.interpStartPos, this.interpEndPos, t);
			}
			Quaternion rotation = Quaternion.Slerp(transform.rotation, this.qrotation, deltaTime * 20f);
			transform.SetPositionAndRotation(a - Origin.position, rotation);
			return;
		}
		this.SetPosition(transform.position + Origin.position, true);
		this.SetRotation(transform.eulerAngles);
		this.qrotation = transform.rotation;
	}

	public void OnContactEvent()
	{
		if (this.isGroundHit || this.isEntityRemote)
		{
			return;
		}
		Vector3i blockPosition = base.GetBlockPosition();
		blockPosition.y--;
		BlockValue block = this.world.GetBlock(blockPosition);
		if (block.isair)
		{
			return;
		}
		this.isGroundHit = true;
		float lightBrightness = this.world.GetLightBrightness(blockPosition);
		Color colorForSide = block.Block.GetColorForSide(block, BlockFace.Top);
		string name = "impact_stone_on_" + block.Block.blockMaterial.SurfaceCategory;
		this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(name, base.GetPosition(), Quaternion.identity, lightBrightness, colorForSide, this.blockValue.Block.blockMaterial.SurfaceCategory + "hit" + block.Block.blockMaterial.SurfaceCategory, null), this.entityId, false, false);
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		if (!this.isTerrain && this.meshRenderer)
		{
			UnityEngine.Object.Destroy(this.meshRenderer.material);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxHitsPerEntity = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMinDamageToCountHit = 1;

	public Transform prefabParticleOnFallT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Renderer meshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody rigidBody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isTerrain;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float terrainScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float massKg;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long textureFull;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMeshCreated;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int fallTimeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<int, int> entityHits = new Dictionary<int, int>(8);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lastTimeStartParticleSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lastTimeEndParticleSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int notMovingCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGroundHit;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Collider myCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 startVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startAngularVel = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 interpStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 interpEndPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float interpTime;
}
