using System;
using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Entity : MonoBehaviour
{
	public FastTags<TagGroup.Global> EntityTags
	{
		get
		{
			return this.cachedTags;
		}
	}

	public EntityClass EntityClass
	{
		get
		{
			if (!EntityClass.list.ContainsKey(this.entityClass))
			{
				return null;
			}
			return EntityClass.list[this.entityClass];
		}
	}

	public virtual Entity.EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Entity.EnumPositionUpdateMovementType.Lerp;
		}
	}

	public static bool CheckDistance(int entityID_A, int entityID_B)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_A);
		if (entity == null)
		{
			return false;
		}
		Entity entity2 = GameManager.Instance.World.GetEntity(entityID_B);
		return !(entity2 == null) && Entity.CheckDistance(entity, entity2);
	}

	public static bool CheckDistance(Entity entityB, int entityID_A)
	{
		return Entity.CheckDistance(entityID_A, entityB);
	}

	public static bool CheckDistance(int entityID_A, Entity entityB)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		if (entityB == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_A);
		return !(entity == null) && Entity.CheckDistance(entity, entityB);
	}

	public static bool CheckDistance(Entity A, Vector3 B)
	{
		return !(A == null) && Entity.CheckDistance(A.transform.position, B);
	}

	public static bool CheckDistance(Vector3 A, Entity B)
	{
		return !(B == null) && Entity.CheckDistance(A, B.transform.position);
	}

	public static bool CheckDistance(Vector3 A, int entityID_B)
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		Entity entity = GameManager.Instance.World.GetEntity(entityID_B);
		return !(entity == null) && Entity.CheckDistance(A - Origin.position, entity.transform.position);
	}

	public static bool CheckDistance(Vector3 A, Vector3 B)
	{
		return (A - B).magnitude < 256f;
	}

	public static bool CheckDistance(Entity listenerEntity, Entity sourceEntity)
	{
		return Entity.CheckDistance(sourceEntity.transform.position, listenerEntity.transform.position);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		Entity.InstanceCount++;
		this.world = GameManager.Instance.World;
		this.isEntityRemote = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		this.WorldTimeBorn = this.world.worldTime;
		this.rand = this.world.GetGameRandom();
		this.SetupBounds();
		if (OcclusionManager.Instance.cullEntities)
		{
			Occludee.Add(base.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~Entity()
	{
		Entity.InstanceCount--;
	}

	public virtual void OnXMLChanged()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupBounds()
	{
		BoxCollider boxCollider;
		if (base.TryGetComponent<BoxCollider>(out boxCollider))
		{
			this.nativeCollider = boxCollider;
			Vector3 localScale = base.transform.localScale;
			this.scaledExtent = Vector3.Scale(boxCollider.size, localScale) * 0.5f;
			Vector3 b = Vector3.Scale(boxCollider.center, localScale);
			this.boundingBox = BoundsUtils.BoundsForMinMax(-this.scaledExtent, this.scaledExtent);
			this.boundingBox.center = this.boundingBox.center + b;
			if (this.isDetailedHeadBodyColliders())
			{
				boxCollider.enabled = false;
				return;
			}
		}
		else
		{
			CharacterController characterController;
			if (base.TryGetComponent<CharacterController>(out characterController))
			{
				Vector3 localScale2 = base.transform.localScale;
				float radius = characterController.radius;
				this.scaledExtent = new Vector3(radius * localScale2.x, characterController.height * localScale2.y * 0.5f, radius * localScale2.z);
				this.boundingBox = BoundsUtils.BoundsForMinMax(-this.scaledExtent, this.scaledExtent);
				return;
			}
			this.boundingBox = BoundsUtils.BoundsForMinMax(Vector3.zero, Vector3.one);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.bWasDead = this.IsDead();
		this.animateYaw();
		if (this.physicsMasterTargetTime > 0f)
		{
			this.PhysicsMasterTargetFrameUpdate();
		}
		else
		{
			this.updateTransform();
		}
		if (this.bIsChunkObserver && !this.isEntityRemote)
		{
			if (this.movableChunkObserver == null)
			{
				this.movableChunkObserver = new MovableSharedChunkObserver(this.world.m_SharedChunkObserverCache);
			}
			this.movableChunkObserver.SetPosition(this.position);
			return;
		}
		if (!this.bIsChunkObserver && this.movableChunkObserver != null)
		{
			this.movableChunkObserver.Dispose();
			this.movableChunkObserver = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateTransform()
	{
		if (this.AttachedToEntity != null)
		{
			return;
		}
		this.ApplyFixedUpdate();
		if (!this.emodel || !this.emodel.IsRagdollOn)
		{
			float y;
			if (this.physicsRB)
			{
				Vector3 b = this.physicsRBT.position - this.physicsBasePos;
				Vector3 vector = Vector3.Lerp(base.transform.position, b, this.physicsPosMoveDistance * Time.deltaTime / Time.fixedDeltaTime);
				base.transform.position = vector;
				y = this.physicsRBT.eulerAngles.y;
			}
			else
			{
				Vector3 b2 = this.position - Origin.position;
				base.transform.position = Vector3.Lerp(base.transform.position, b2, Time.deltaTime * Entity.updatePositionLerpTimeScale);
				y = this.rotation.y;
			}
			if (this.isRotateToGround)
			{
				Vector3 vector2 = this.groundSurface.normal;
				float num = Vector3.Dot(vector2, Vector3.up);
				if (this.IsRotateToGroundFlat)
				{
					num = 1f;
				}
				if (num > 0.99f || num < 0.7f)
				{
					vector2 = Vector3.up;
				}
				Vector3 vector3 = Quaternion.AngleAxis(-y, Vector3.up) * vector2;
				float target = 90f - Mathf.Atan2(vector3.y, vector3.z) * 57.29578f;
				this.rotateToGroundPitchVel *= 0.86f;
				this.rotateToGroundPitchVel += Mathf.DeltaAngle(this.rotateToGroundPitch, target) * 0.8f * Time.deltaTime;
				this.rotateToGroundPitch += this.rotateToGroundPitchVel;
				base.transform.eulerAngles = new Vector3(this.rotateToGroundPitch, y, 0f);
			}
			else
			{
				base.transform.eulerAngles = new Vector3(0f, Mathf.LerpAngle(base.transform.eulerAngles.y, y, Time.deltaTime * Entity.updateRotationLerpTimeScale), 0f);
			}
		}
		if (this.isEntityRemote && this.PhysicsTransform != null)
		{
			this.PhysicsTransform.position = Vector3.Lerp(this.PhysicsTransform.position, this.position - Origin.position, Time.deltaTime * Entity.updateRotationLerpTimeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		this.ApplyFixedUpdate();
		this.wasFixedUpdate = true;
		if (this.physicsRB)
		{
			this.physicsRB.velocity *= 0.9f;
			this.physicsRB.angularVelocity *= 0.9f;
			Transform transform = this.physicsRBT;
			Vector3 b = this.physicsTargetPos + this.physicsBasePos;
			Vector3 vector = Vector3.Lerp(transform.position, b, 0.4f);
			this.physicsPos = vector;
			this.physicsRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, this.rotation.y, 0f), 0.3f);
			transform.SetPositionAndRotation(vector, this.physicsRot);
			if (this.physicsCapsuleCollider)
			{
				EntityAlive entityAlive = this as EntityAlive;
				if (entityAlive)
				{
					entityAlive.HeadHeightFixedUpdate();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyFixedUpdate()
	{
		if (!this.wasFixedUpdate)
		{
			return;
		}
		this.wasFixedUpdate = false;
		if (this.physicsRB)
		{
			Transform transform = this.physicsRBT;
			Vector3 a = transform.position;
			if ((a - this.physicsPos).sqrMagnitude > 0.0001f)
			{
				Vector3 vector = this.position;
				Vector3 a2 = a - this.physicsBasePos;
				this.physicsPos = a;
				this.SetPosition(a2 + Origin.position, false);
				this.PhysicsTransform.position = a2;
			}
			this.physicsPosMoveDistance = Vector3.Distance(this.physicsPos, base.transform.position);
			if (Mathf.Abs(Quaternion.Angle(transform.rotation, this.physicsRot)) > 0.1f)
			{
				Quaternion quaternion = transform.rotation;
				this.physicsRot = quaternion;
				this.rotation = quaternion.eulerAngles;
				this.qrotation = quaternion;
			}
		}
	}

	public virtual void OriginChanged(Vector3 _deltaPos)
	{
		this.physicsPos += _deltaPos;
		this.physicsTargetPos += _deltaPos;
		if (this.emodel)
		{
			this.emodel.OriginChanged(_deltaPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddCharacterController()
	{
		if (!this.PhysicsTransform)
		{
			return;
		}
		float num = 0.08f;
		Vector3 center = Vector3.zero;
		bool flag = false;
		bool flag2 = true;
		GameObject gameObject = this.PhysicsTransform.gameObject;
		float num2;
		float num3;
		if (this is EntityPlayer)
		{
			CharacterController component = gameObject.GetComponent<CharacterController>();
			if (!component)
			{
				Log.Error("Player !cc");
				return;
			}
			center = component.center;
			num2 = component.height;
			num3 = component.radius;
			this.m_characterController = new CharacterControllerUnity(component);
			if (!this.isEntityRemote)
			{
				gameObject.AddComponent<ColliderHitCallForward>().Entity = this;
			}
		}
		else
		{
			num = 0f;
			flag = true;
			flag2 = false;
			CapsuleCollider component2 = gameObject.GetComponent<CapsuleCollider>();
			if (component2)
			{
				center = component2.center;
				num2 = component2.height;
				num3 = component2.radius;
			}
			else
			{
				gameObject.AddComponent<CapsuleCollider>();
				center.y = 0.9f;
				num2 = 1.8f;
				num3 = 0.3f;
			}
			if (this.physicsCapsuleCollider)
			{
				num2 = this.physicsBaseHeight;
				center.y = num2 * 0.5f;
			}
			CharacterController characterController;
			if (gameObject.TryGetComponent<CharacterController>(out characterController))
			{
				center = characterController.center;
				num2 = characterController.height;
				num3 = characterController.radius;
				UnityEngine.Object.Destroy(characterController);
				Log.Warning("{0} has old CC", new object[]
				{
					this.ToString()
				});
			}
			this.m_characterController = new CharacterControllerKinematic(this);
		}
		if (num2 <= 0f)
		{
			return;
		}
		if (flag2)
		{
			BoxCollider boxCollider = this.nativeCollider as BoxCollider;
			if (boxCollider && this.linkCapsuleSizeToBoundingBox)
			{
				num2 = Utils.FastMax(boxCollider.size.y - num, this.stepHeight);
				center = boxCollider.center;
				center.y = num2 * 0.5f;
				if (boxCollider.size.x > boxCollider.size.y)
				{
					center.y += (boxCollider.size.x - boxCollider.size.y) * 0.5f;
				}
				num3 = boxCollider.size.x * 0.5f - num;
				flag = true;
			}
		}
		if (flag)
		{
			this.m_characterController.SetSize(center, num2 / this.physicsHeightScale, num3);
			if (this.physicsCapsuleCollider)
			{
				this.PhysicsSetHeight(num2);
			}
		}
		this.m_characterController.SetStepOffset(this.stepHeight);
		Vector3 localScale = base.transform.localScale;
		this.scaledExtent = new Vector3(num3 * localScale.x, num2 * localScale.y * 0.5f, num3 * localScale.z);
		this.boundingBox = BoundsUtils.BoundsForMinMax(-this.scaledExtent, this.scaledExtent);
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = false;
		}
	}

	public virtual bool linkCapsuleSizeToBoundingBox
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCCScale(float scale)
	{
		CharacterControllerAbstract characterController = this.m_characterController;
		if (characterController == null)
		{
			return;
		}
		this.PhysicsTransform.localScale = Vector3.one;
		Vector3 center = characterController.GetCenter() * scale;
		float num = characterController.GetHeight() * scale;
		if (num < 2.2f && num > 1.89f)
		{
			num = 1.89f;
			center.y = num * 0.5f;
		}
		float num2 = Utils.FastMax(scale, 1f);
		characterController.SetSize(center, num, characterController.GetRadius() * num2);
	}

	public virtual void Init(int _entityClass)
	{
		this.entityClass = _entityClass;
		this.InitCommon();
		this.InitEModel();
		this.PhysicsInit();
	}

	public virtual void InitFromPrefab(int _entityClass)
	{
		this.entityClass = _entityClass;
		this.InitCommon();
		this.InitEModelFromPrefab();
		this.PhysicsInit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitCommon()
	{
		EntityClass entityClass = EntityClass.list[this.entityClass];
		this.cachedTags = entityClass.Tags;
		this.bIsChunkObserver = entityClass.bIsChunkObserver;
		this.CopyPropertiesFromEntityClass();
		if (this.PhysicsTransform)
		{
			this.PhysicsTransform.gameObject.tag = "Physics";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEModel()
	{
		Type modelType = EntityClass.list[this.entityClass].modelType;
		this.emodel = (base.gameObject.AddComponent(modelType) as EModelBase);
		this.emodel.Init(this.world, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEModelFromPrefab()
	{
		Type modelType = EntityClass.list[this.entityClass].modelType;
		this.emodel = (base.gameObject.GetComponent(modelType) as EModelBase);
		this.emodel.InitFromPrefab(this.world, this);
	}

	public virtual void PostInit()
	{
		if (this.emodel != null)
		{
			this.emodel.PostInit();
			this.HandleNavObject();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsInit()
	{
		Transform transform = GameUtils.FindTagInChilds(this.ModelTransform, "Physics");
		if (transform)
		{
			this.PhysicsTransform = this.RootTransform.Find("Physics");
			if (this.PhysicsTransform)
			{
				UnityEngine.Object.Destroy(this.PhysicsTransform.gameObject);
				Log.Warning("{0} has old Physics", new object[]
				{
					this.ToString()
				});
			}
			this.PhysicsTransform = transform;
			transform.SetParent(this.RootTransform, false);
		}
		else if (!this.PhysicsTransform)
		{
			this.PhysicsTransform = this.RootTransform.Find("Physics");
		}
		this.physicsRBT = GameUtils.FindTagInChilds(this.RootTransform, "LargeEntityBlocker");
		if (this.physicsRBT)
		{
			Transform transform2 = base.transform;
			Transform parent = this.physicsRBT.parent;
			this.physicsPos = this.physicsRBT.position;
			this.physicsRot = transform2.rotation;
			if (parent != transform2.parent)
			{
				Vector3 vector = this.physicsRBT.localPosition;
				float x = parent.lossyScale.x;
				vector += parent.localPosition * (1f / x);
				Collider[] componentsInChildren = this.physicsRBT.GetComponentsInChildren<Collider>();
				for (int i = componentsInChildren.Length - 1; i >= 0; i--)
				{
					Collider collider = componentsInChildren[i];
					CapsuleCollider capsuleCollider;
					BoxCollider boxCollider;
					SphereCollider sphereCollider;
					if (capsuleCollider = (collider as CapsuleCollider))
					{
						capsuleCollider.center = (capsuleCollider.center + vector) * x;
						capsuleCollider.height *= x;
						capsuleCollider.radius *= x;
					}
					else if (boxCollider = (collider as BoxCollider))
					{
						boxCollider.center = (boxCollider.center + vector) * x;
						boxCollider.size *= x;
					}
					else if (sphereCollider = (collider as SphereCollider))
					{
						sphereCollider.center = (sphereCollider.center + vector) * x;
						sphereCollider.radius *= x;
					}
				}
				this.physicsBasePos = Vector3.zero;
				this.physicsRBT.SetParent(transform2.parent, true);
				this.physicsRBT.localScale = Vector3.one;
			}
			else
			{
				this.physicsBasePos = Vector3.Scale(this.physicsRBT.localPosition, parent.lossyScale);
			}
			this.physicsRB = this.physicsRBT.gameObject.AddComponent<Rigidbody>();
			this.physicsRB.useGravity = false;
			float v = EntityClass.list[this.entityClass].MassKg * 0.6f;
			this.physicsRB.mass = Utils.FastMax(30f, v);
			this.physicsRB.constraints = (RigidbodyConstraints)80;
			this.physicsRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			this.physicsTargetPos = this.physicsPos;
			CapsuleCollider component = this.physicsRBT.GetComponent<CapsuleCollider>();
			if (component && component.direction == 1)
			{
				this.physicsCapsuleCollider = component;
				this.physicsColliderRadius = component.radius;
				this.physicsHeightScale = 1.09f;
				float height = component.height;
				float y = component.center.y;
				float num = y + height * 0.5f;
				this.physicsBaseHeight = num * this.physicsHeightScale;
				this.physicsColliderLowerY = y - height * 0.5f;
				if ((double)this.physicsBaseHeight > 1.95)
				{
					this.physicsBaseHeight = 1.95f;
				}
			}
		}
	}

	public void PhysicsSetRB(Rigidbody rb)
	{
		this.physicsRB = rb;
	}

	public void PhysicsPause()
	{
		if (this.physicsRBT)
		{
			this.physicsRBT.gameObject.SetActive(false);
		}
	}

	public virtual void PhysicsResume(Vector3 pos, float rotY)
	{
		this.rotation = new Vector3(0f, rotY, 0f);
		if (this.physicsRBT)
		{
			this.physicsRBT.gameObject.SetActive(true);
			this.physicsRBT.eulerAngles = this.rotation;
			this.physicsPosMoveDistance = 0f;
		}
		this.SetPosition(pos, true);
		base.transform.SetPositionAndRotation(pos - Origin.position, Quaternion.Euler(this.rotation));
	}

	public virtual void PhysicsPush(Vector3 forceVec, Vector3 forceWorldPos, bool affectLocalPlayerController = false)
	{
		if (forceVec.sqrMagnitude > 0f)
		{
			Rigidbody rigidbody = this.physicsRB;
			if (rigidbody)
			{
				if (!this.emodel.IsRagdollActive)
				{
					forceVec *= 5f;
				}
				if (forceWorldPos.sqrMagnitude > 0f)
				{
					rigidbody.AddForceAtPosition(forceVec, forceWorldPos - Origin.position, ForceMode.Impulse);
					return;
				}
				rigidbody.AddForce(forceVec, ForceMode.Impulse);
			}
		}
	}

	public void PhysicsSetHeight(float _height)
	{
		this.physicsHeight = _height;
		float num = this.physicsColliderLowerY;
		if (_height - num < this.physicsColliderRadius)
		{
			num = _height - this.physicsColliderRadius;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		this.physicsCapsuleCollider.height = _height - num;
		Vector3 vector;
		vector.x = 0f;
		vector.z = 0f;
		vector.y = (_height + num) * 0.5f;
		if (vector.y < this.physicsColliderRadius)
		{
			vector.y = this.physicsColliderRadius;
		}
		this.physicsCapsuleCollider.center = vector;
	}

	public virtual void PhysicsMasterBecome()
	{
		this.isPhysicsMaster = true;
		this.physicsMasterTargetTime = 0f;
		this.SetPosition(this.physicsMasterTargetPos, false);
		this.qrotation = this.physicsMasterTargetRot;
		if (this.physicsRB)
		{
			this.physicsRB.position = this.position - Origin.position;
			this.physicsRB.rotation = this.qrotation;
			this.physicsRB.velocity = this.physicsVel;
			this.physicsRB.angularVelocity = this.physicsAngVel;
		}
	}

	public NetPackageEntityPhysics PhysicsMasterSetupBroadcast()
	{
		if ((this.position - this.physicsMasterSendPos).sqrMagnitude < 0.00250000018f && Quaternion.Angle(this.qrotation, this.physicsMasterSendRot) < 1f)
		{
			return null;
		}
		this.physicsMasterSendPos = this.position;
		this.physicsMasterSendRot = this.qrotation;
		return NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
	}

	public void PhysicsMasterSendToServer(Transform t)
	{
		if (this.clientEntityId != 0)
		{
			return;
		}
		this.position = t.position + Origin.position;
		this.qrotation = t.rotation;
		if (this.GetVelocityPerSecond().sqrMagnitude < 0.160000011f)
		{
			this.isPhysicsMaster = false;
		}
		NetPackageEntityPhysics package = NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
	}

	public Vector3 PhysicsMasterGetFinalPosition()
	{
		if (this.physicsMasterTargetTime > 0f)
		{
			return this.physicsMasterTargetPos;
		}
		return this.position;
	}

	public void PhysicsMasterSetTargetOrientation(Vector3 pos, Quaternion rot)
	{
		this.physicsMasterFromPos = this.position;
		this.physicsMasterFromRot = this.qrotation;
		this.physicsMasterTargetElapsed = 0f;
		this.physicsMasterTargetTime = 0.1f;
		this.physicsMasterTargetPos = pos;
		this.physicsMasterTargetRot = rot;
	}

	public void PhysicsMasterTargetFrameUpdate()
	{
		this.physicsMasterTargetElapsed += Time.deltaTime;
		float t = this.physicsMasterTargetElapsed / this.physicsMasterTargetTime;
		Vector3 vector = Vector3.Lerp(this.physicsMasterFromPos, this.physicsMasterTargetPos, t);
		this.SetPosition(vector, true);
		Quaternion quaternion = Quaternion.Lerp(this.physicsMasterFromRot, this.physicsMasterTargetRot, t);
		this.qrotation = quaternion;
		this.physicsRB.position = vector - Origin.position;
		this.physicsRB.rotation = quaternion;
		if (this.physicsMasterTargetElapsed >= this.physicsMasterTargetTime)
		{
			this.physicsMasterTargetTime = 0f;
		}
	}

	public void SetHeight(float _height)
	{
		this.m_characterController.SetHeight(_height / this.physicsHeightScale);
		this.PhysicsSetHeight(_height);
	}

	public void SetMaxHeight(float _maxHeight)
	{
		this.physicsBaseHeight = _maxHeight;
		if (this.m_characterController != null)
		{
			this.m_characterController.SetHeight(_maxHeight / this.physicsHeightScale);
		}
		if (this.physicsCapsuleCollider)
		{
			this.PhysicsSetHeight(_maxHeight);
			float y = this.physicsCapsuleCollider.center.y;
			float num = this.physicsCapsuleCollider.height * 0.5f;
			this.physicsBaseHeight = y + num;
			this.physicsColliderLowerY = y - num;
		}
	}

	public void SetScale(float scale)
	{
		Vector3 localScale = new Vector3(scale, scale, scale);
		this.ModelTransform.localScale = localScale;
		foreach (CharacterJoint characterJoint in this.ModelTransform.GetComponentsInChildren<CharacterJoint>())
		{
			if (characterJoint.autoConfigureConnectedAnchor)
			{
				characterJoint.autoConfigureConnectedAnchor = false;
				characterJoint.autoConfigureConnectedAnchor = true;
			}
		}
		if (this.physicsRBT)
		{
			this.physicsBaseHeight *= scale;
			this.physicsHeight *= scale;
			this.physicsColliderLowerY *= scale;
			Collider[] componentsInChildren2 = this.physicsRBT.GetComponentsInChildren<Collider>();
			for (int j = componentsInChildren2.Length - 1; j >= 0; j--)
			{
				Collider collider = componentsInChildren2[j];
				CapsuleCollider capsuleCollider;
				BoxCollider boxCollider;
				SphereCollider sphereCollider;
				if (capsuleCollider = (collider as CapsuleCollider))
				{
					capsuleCollider.center *= scale;
					capsuleCollider.height *= scale;
					capsuleCollider.radius *= scale;
				}
				else if (boxCollider = (collider as BoxCollider))
				{
					boxCollider.center *= scale;
					boxCollider.size *= scale;
				}
				else if (sphereCollider = (collider as SphereCollider))
				{
					sphereCollider.center *= scale;
					sphereCollider.radius *= scale;
				}
			}
		}
		this.SetCCScale(scale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ReplicateSpeeds()
	{
		int num = this.speedSentTicks - 1;
		this.speedSentTicks = num;
		if (num > 0)
		{
			return;
		}
		float num2 = this.speedForward - this.speedForwardSent;
		float num3 = this.speedStrafe - this.speedStrafeSent;
		if (num2 * num2 + num3 * num3 >= 4.00000044E-06f)
		{
			this.speedSentTicks = 3;
			this.speedForwardSent = this.speedForward;
			this.speedStrafeSent = this.speedStrafe;
			if (this.world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(this), false);
				return;
			}
			this.world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, this.entityId, NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(this), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetMovementState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void animateYaw()
	{
		if (this.yawSeekTimeMax > 0f)
		{
			this.yawSeekTime += Time.deltaTime;
			if (this.yawSeekTime < this.yawSeekTimeMax)
			{
				this.rotation.y = Mathf.Lerp(this.yawSeekAngle, this.yawSeekAngleEnd, this.yawSeekTime / this.yawSeekTimeMax);
				return;
			}
			this.yawSeekTimeMax = 0f;
			this.rotation.y = this.yawSeekAngleEnd;
		}
	}

	public void SeekYawToPos(Vector3 _pos, float _yawSlowAt)
	{
		float num = _pos.x - this.position.x;
		float num2 = _pos.z - this.position.z;
		if (num * num + num2 * num2 > 0.0001f)
		{
			float yaw = Mathf.Atan2(num, num2) * 57.29578f;
			this.SeekYaw(yaw, 0f, _yawSlowAt);
		}
	}

	public float SeekYaw(float yaw, float _, float yawSlowAt)
	{
		if (yaw < 0f)
		{
			yaw += 360f;
		}
		if (yaw > 360f)
		{
			yaw -= 360f;
		}
		if (this.rotation.y < 0f)
		{
			this.rotation.y = this.rotation.y + 360f;
		}
		if (this.rotation.y > 360f)
		{
			this.rotation.y = this.rotation.y - 360f;
		}
		float num = EntityClass.list[this.entityClass].MaxTurnSpeed;
		if (this.inWaterPercent > 0.3f)
		{
			num *= 1f - this.inWaterPercent * 0.5f;
		}
		if (num > 0f)
		{
			float num2 = yaw - this.rotation.y;
			if (num2 != 0f)
			{
				if (num2 < -180f)
				{
					num2 += 360f;
				}
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				float num3 = Utils.FastAbs(num2);
				if (num3 < yawSlowAt)
				{
					float num4 = num3 / yawSlowAt;
					num = num * num4 * num4;
					num = Utils.FastMax(num, 20f);
				}
				this.yawSeekTime = 0f;
				this.yawSeekTimeMax = num3 / num;
				this.yawSeekAngle = this.rotation.y;
				this.yawSeekAngleEnd = this.rotation.y + num2;
				return num2;
			}
		}
		this.rotation.y = yaw;
		this.yawSeekTimeMax = 0f;
		return 0f;
	}

	public virtual void KillLootContainer()
	{
		this.Kill(DamageResponse.New(true));
	}

	public virtual void Kill(DamageResponse _dmResponse)
	{
		this.SetDead();
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity != null)
				{
					entity.Kill(_dmResponse);
					entity.Detach();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickInWater()
	{
		this.inWaterLevel = this.CalcWaterLevel();
		this.inWaterPercent = this.inWaterLevel / (this.GetHeight() * 1.1f);
		this.isInWater = (this.inWaterPercent >= 0.25f);
		bool flag = this.isSwimming;
		this.isSwimming = this.CalcIfSwimming();
		if (this.isSwimming != flag)
		{
			this.SwimChanged();
		}
		bool flag2 = this.isHeadUnderwater;
		this.isHeadUnderwater = this.IsHeadUnderwater();
		if (this.isHeadUnderwater != flag2)
		{
			this.OnHeadUnderwaterStateChanged(this.isHeadUnderwater);
		}
	}

	public float CalcWaterLevel()
	{
		float num = this.GetHeight() * 1.1f;
		int num2 = Utils.Fastfloor(this.position.y + num);
		int num3 = Utils.Fastfloor(this.position.y);
		int num4 = num2 - num3 + 1;
		int num5 = Utils.Fastfloor(this.position.x);
		int num6 = Utils.Fastfloor(this.position.z);
		int i = -2;
		while (i < 6)
		{
			Vector3i vector3i;
			if (i < 0)
			{
				vector3i.x = num5;
				vector3i.z = num6;
				goto IL_E4;
			}
			vector3i.x = Utils.Fastfloor(this.position.x + Entity.waterLevelDirOffsets[i] * 0.28f);
			vector3i.z = Utils.Fastfloor(this.position.z + Entity.waterLevelDirOffsets[i + 1] * 0.28f);
			if (vector3i.x != num5 || vector3i.z != num6)
			{
				goto IL_E4;
			}
			IL_184:
			i += 2;
			continue;
			IL_E4:
			vector3i.y = num2;
			int num7 = num4;
			float num8;
			for (;;)
			{
				num8 = this.world.GetWaterPercent(vector3i);
				if (num8 > 0f)
				{
					break;
				}
				vector3i.y--;
				if (--num7 <= 0)
				{
					goto IL_184;
				}
			}
			if (num7 == num4)
			{
				vector3i.y++;
				if (this.world.GetWaterPercent(vector3i) == 0f)
				{
					num8 = 0.6f;
				}
				vector3i.y--;
			}
			else
			{
				num8 = 0.6f;
			}
			return Mathf.Clamp((float)vector3i.y + num8 - this.position.y, 0f, num);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CalcIfSwimming()
	{
		return this.inWaterPercent >= 0.5f;
	}

	public virtual void SwimChanged()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool IsHeadUnderwater()
	{
		return this.inWaterPercent >= 0.9f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		if (!_bUnderwater)
		{
			Manager.Play(this, "water_emerge", 1f, false);
		}
	}

	public virtual void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.normal.y > 0.707f && hit.normal.y > this.groundSurface.normal.y && hit.moveDirection.y < 0f)
		{
			if ((double)(hit.point - this.groundSurface.lastHitPoint).sqrMagnitude > 0.001 || this.groundSurface.lastNormal == Vector3.zero)
			{
				this.groundSurface.normal = hit.normal;
			}
			else
			{
				this.groundSurface.normal = this.groundSurface.lastNormal;
			}
			this.groundSurface.hitPoint = hit.point;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollision(Vector3 _vel)
	{
		this.canCCMove = true;
		this.ccEntityCollisionStart(_vel);
		if (!this.isCCDelayed)
		{
			this.ccEntityCollisionResults();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollisionStart(Vector3 _vel)
	{
		this.groundSurface.lastHitPoint = this.groundSurface.hitPoint;
		this.groundSurface.lastNormal = this.groundSurface.normal;
		this.groundSurface.normal = Vector3.up;
		this.ySize *= this.ConditionalScalePhysicsMulConstant(0.4f);
		if (this.isMotionSlowedDown)
		{
			this.isMotionSlowedDown = false;
			_vel.x *= this.motionMultiplier;
			if (!this.isCollidedVertically)
			{
				_vel.y *= this.motionMultiplier;
			}
			_vel.z *= this.motionMultiplier;
		}
		this.hitMove = _vel;
		this.collisionFlags = CollisionFlags.None;
		if (this.IsStuck)
		{
			this.PhysicsTransform.position += this.hitMove;
			return;
		}
		this.collisionFlags = this.m_characterController.Move(this.hitMove);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ccEntityCollisionResults()
	{
		Vector3 a = this.PhysicsTransform.position;
		this.physicsTargetPos = a;
		a += Origin.position;
		Vector3 vector = a - this.position;
		this.position = a;
		this.boundingBox.center = this.boundingBox.center + vector;
		Vector3 lhs = new Vector3(vector.x, 0f, vector.z);
		Vector3 vector2 = new Vector3(this.motion.x, 0f, this.motion.z);
		this.projectedMove = 0f;
		if (vector2 != Vector3.zero)
		{
			this.projectedMove = Utils.FastClamp01(Vector3.Dot(lhs, vector2) / vector2.sqrMagnitude);
			vector2 *= this.projectedMove;
		}
		if (this.motion.y > 0f)
		{
			if (vector.y >= 0f && vector.y < this.motion.y * 0.95f)
			{
				this.motion.y = 0f;
			}
			else
			{
				this.motion.y = Utils.FastClamp(vector.y, 0f, this.motion.y);
			}
		}
		else
		{
			this.motion.y = Utils.FastClamp(vector.y, this.motion.y, 0f);
		}
		this.motion.x = vector2.x;
		this.motion.z = vector2.z;
		this.isCollidedHorizontally = ((this.collisionFlags & CollisionFlags.Sides) > CollisionFlags.None);
		this.isCollidedVertically = ((this.collisionFlags & (CollisionFlags)6) > CollisionFlags.None);
		this.onGround = this.m_characterController.IsGrounded();
		if (this.onGround)
		{
			this.groundSurface.normal = this.m_characterController.GroundNormal;
		}
		this.world.CheckEntityCollisionWithBlocks(this);
		this.UpdateFall(this.hitMove.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aabbEntityCollision(Vector3 _vel)
	{
		this.ySize *= 0.4f;
		if (this.isMotionSlowedDown)
		{
			this.isMotionSlowedDown = false;
			_vel.x *= this.motionMultiplier;
			if (!this.isCollidedVertically)
			{
				_vel.y *= this.motionMultiplier;
			}
			_vel.z *= this.motionMultiplier;
			this.motion = Vector3.zero;
		}
		Vector3 vector = _vel;
		Bounds bounds = this.boundingBox;
		if (Math.Abs(_vel.x) <= 0.0001f)
		{
			Math.Abs(_vel.z);
		}
		this.collAABB.Clear();
		Bounds aabb = BoundsUtils.ExpandDirectional(this.boundingBox, vector);
		this.world.GetCollidingBounds(this, aabb, this.collAABB);
		Vector3 vector2 = BoundsUtils.ClipBoundsMove(this.boundingBox, vector, this.collAABB, this.collAABB.Count);
		this.boundingBox.center = this.boundingBox.center + vector2;
		bool flag = this.onGround || (vector.y != vector2.y && vector.y < 0f);
		if (this.stepHeight > 0f && flag && this.ySize < 0.05f && (vector.x != vector2.x || vector.z != vector2.z))
		{
			Vector3 vector3 = vector2;
			vector2 = vector;
			vector2.y = this.stepHeight;
			Bounds bounds2 = this.boundingBox;
			this.boundingBox = bounds;
			this.collAABB.Clear();
			aabb = BoundsUtils.ExpandDirectional(this.boundingBox, new Vector3(vector2.x, 0f, vector2.z));
			this.world.GetCollidingBounds(this, aabb, this.collAABB);
			vector2 = BoundsUtils.ClipBoundsMove(this.boundingBox, vector2, this.collAABB, this.collAABB.Count);
			this.boundingBox.center = this.boundingBox.center + vector2;
			float y = BoundsUtils.ClipBoundsMoveY(this.boundingBox.min, this.boundingBox.max, -this.stepHeight, this.collAABB, this.collAABB.Count);
			this.boundingBox.center = this.boundingBox.center + new Vector3(0f, y, 0f);
			vector2.y = y;
			if (vector3.x * vector3.x + vector3.z * vector3.z >= vector2.x * vector2.x + vector2.z * vector2.z)
			{
				vector2 = vector3;
				this.boundingBox = bounds2;
			}
			else if (this.boundingBox.min.y - (float)((int)this.boundingBox.min.y) > 0f)
			{
				this.ySize += this.boundingBox.min.y - bounds2.min.y;
			}
		}
		Vector3 center = this.boundingBox.center;
		this.position.x = center.x;
		this.position.y = this.boundingBox.min.y + this.yOffset - this.ySize;
		this.position.z = center.z;
		if (this.PhysicsTransform != null && (this.PhysicsTransform.position - (this.position - Origin.position)).sqrMagnitude > 0.0001f)
		{
			this.PhysicsTransform.position = this.position - Origin.position;
		}
		this.isCollidedHorizontally = (vector.x != vector2.x || vector.z != vector2.z);
		this.isCollidedVertically = (vector.y != vector2.y);
		this.onGround = (vector.y != vector2.y && vector.y < 0f);
		this.world.CheckEntityCollisionWithBlocks(this);
		this.UpdateFall(vector2.y);
		if (vector.x != vector2.x)
		{
			this.motion.x = 0f;
		}
		if (vector.y != vector2.y)
		{
			this.motion.y = 0f;
		}
		if (vector.z != vector2.z)
		{
			this.motion.z = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CalcFixedUpdateTimeScaleConstants()
	{
		this.kAddFixedUpdateTimeScale = Time.deltaTime / 0.05f;
	}

	public float ScalePhysicsMulConstant(float tickMulDelta)
	{
		return Mathf.Pow(tickMulDelta, this.kAddFixedUpdateTimeScale);
	}

	public float ScalePhysicsAddConstant(float tickAddDelta)
	{
		return this.kAddFixedUpdateTimeScale * tickAddDelta;
	}

	public float ConditionalScalePhysicsMulConstant(float tickMulDelta)
	{
		return tickMulDelta;
	}

	public float ConditionalScalePhysicsAddConstant(float tickAddDelta)
	{
		return tickAddDelta;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void entityCollision(Vector3 _motion)
	{
		if (this.emodel.IsRagdollMovement)
		{
			if (this.emodel.pelvisRB)
			{
				float num = this.emodel.bipedPelvisTransform.position.y + Origin.position.y;
				Vector3 velocity = this.emodel.pelvisRB.velocity;
				if (velocity.y < -1f)
				{
					this.fallVelY = Utils.FastMin(this.fallVelY, velocity.y);
					float num2 = this.fallLastY - num;
					if (num2 > 0f)
					{
						this.fallDistance += num2;
					}
				}
				else if (this.fallDistance > 0f)
				{
					this.fallLastMotion.y = this.fallVelY * 0.05f;
					this.onGround = true;
					this.UpdateFall(0f);
				}
				this.fallLastY = num;
			}
			return;
		}
		this.ApplyFixedUpdate();
		if (this.m_characterController != null)
		{
			this.ccEntityCollision(_motion);
			return;
		}
		this.aabbEntityCollision(_motion);
	}

	public virtual void SetMotionMultiplier(float _motionMultiplier)
	{
		this.isMotionSlowedDown = true;
		this.motionMultiplier = _motionMultiplier;
		if (this.motionMultiplier < 0.5f)
		{
			this.fallDistance = 0f;
		}
	}

	public float GetDistance(Entity _other)
	{
		return (this.position - _other.position).magnitude;
	}

	public float GetDistanceSq(Entity _other)
	{
		return (this.position - _other.position).sqrMagnitude;
	}

	public float GetDistanceSq(Vector3 _pos)
	{
		return (this.position - _pos).sqrMagnitude;
	}

	public float GetSoundTravelTime(Vector3 _otherPos)
	{
		return (this.position - _otherPos).magnitude / 343f;
	}

	public bool IsInWater()
	{
		return this.isInWater;
	}

	public bool IsSwimming()
	{
		return this.isSwimming;
	}

	public bool IsInElevator()
	{
		return this.bInElevator;
	}

	public void SetInElevator(bool _b)
	{
		this.bInElevator = _b;
	}

	public virtual bool IsAirBorne()
	{
		return this.bAirBorne || !this.onGround;
	}

	public void SetAirBorne(bool _b)
	{
		this.bAirBorne = _b;
	}

	public float width
	{
		get
		{
			return this.scaledExtent.x * 2f;
		}
	}

	public float depth
	{
		get
		{
			return this.scaledExtent.z * 2f;
		}
	}

	public float height
	{
		get
		{
			return this.scaledExtent.y * 2f;
		}
	}

	public virtual float GetEyeHeight()
	{
		return 0f;
	}

	public virtual float GetHeight()
	{
		if (this.m_characterController != null)
		{
			return this.m_characterController.GetHeight();
		}
		return this.height;
	}

	public virtual void Move(Vector3 _direction, bool _isDirAbsolute, float _velocity, float _maxVelocity)
	{
		if (!this.IsClientControlled() && (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving) || GameStats.GetInt(EnumGameStats.GameState) == 2))
		{
			return;
		}
		float y = _direction.y;
		_direction.y = 0f;
		_direction.Normalize();
		if (_isDirAbsolute)
		{
			float num = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(this.motion, _direction)), 0f, _velocity);
			this.motion.x = this.motion.x + this.ConditionalScalePhysicsAddConstant(_direction.x * num);
			this.motion.y = this.motion.y + this.ConditionalScalePhysicsAddConstant(_direction.y * _velocity);
			this.motion.z = this.motion.z + this.ConditionalScalePhysicsAddConstant(_direction.z * num);
			return;
		}
		Vector3 rhs = base.transform.forward * _direction.z + base.transform.right * _direction.x;
		rhs.Normalize();
		float num2 = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(this.motion, rhs)), 0f, _velocity);
		this.motion += base.transform.forward * this.ConditionalScalePhysicsAddConstant(_direction.z * num2) + base.transform.right * this.ConditionalScalePhysicsAddConstant(_direction.x * num2) + base.transform.up * this.ConditionalScalePhysicsAddConstant(y * _velocity);
	}

	public bool IsAlive()
	{
		return !this.IsDead();
	}

	public bool WasAlive()
	{
		return !this.WasDead();
	}

	public virtual bool IsDead()
	{
		return this.bDead;
	}

	public bool WasDead()
	{
		return this.bWasDead;
	}

	public virtual void SetDead()
	{
		this.bDead = true;
		Manager.DestroySoundsForEntity(this.entityId);
		if (this.m_marker != null)
		{
			this.m_marker.Release();
			this.m_marker = null;
		}
		if (this.PhysicsTransform != null)
		{
			if (this.emodel.HasRagdoll())
			{
				this.PhysicsTransform.gameObject.layer = 17;
			}
			else
			{
				this.PhysicsTransform.gameObject.layer = 14;
			}
		}
		if (this.physicsRBT)
		{
			this.physicsRBT.gameObject.SetActive(false);
		}
		if (this.emodel != null)
		{
			this.emodel.SetDead();
		}
	}

	public virtual void SetAlive()
	{
		this.bDead = false;
		if (this.PhysicsTransform != null)
		{
			if (this is EntityPlayerLocal)
			{
				this.PhysicsTransform.gameObject.layer = 20;
				return;
			}
			this.PhysicsTransform.gameObject.layer = 15;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFall(float mY)
	{
		if (this.onGround)
		{
			if (this.fallDistance > 0f)
			{
				this.fallHitGround(this.fallDistance, this.fallLastMotion);
				this.fallDistance = 0f;
				return;
			}
		}
		else if (mY < 0f)
		{
			this.fallLastMotion = this.motion;
			this.fallDistance -= mY;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void fallHitGround(float _v, Vector3 _fallMotion)
	{
	}

	public virtual void OnRagdoll(bool isActive)
	{
		if (isActive && this.emodel.bipedPelvisTransform)
		{
			this.fallLastY = this.emodel.bipedPelvisTransform.position.y + Origin.position.y;
			this.fallVelY = 0f;
		}
	}

	public virtual bool CanDamageEntity(int _sourceEntityId)
	{
		return true;
	}

	public virtual int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		this.setBeenAttacked();
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setBeenAttacked()
	{
	}

	public virtual void ProcessDamageResponse(DamageResponse _dmResponse)
	{
	}

	public Bounds getBoundingBox()
	{
		return this.boundingBox;
	}

	public virtual void OnDamagedByExplosion()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnPushEntity(Entity _entity)
	{
		Vector3 vector = _entity.position - this.position;
		float num = Utils.FastMax(Mathf.Abs(vector.x), Mathf.Abs(vector.z));
		if (num >= 0.01f)
		{
			num = Mathf.Sqrt(num);
			float num2 = 1f / num;
			vector.x *= num2;
			vector.z *= num2;
			if (num2 < 1f)
			{
				vector.x *= num2;
				vector.z *= num2;
			}
			float num3 = 0.05f * (1f - this.entityCollisionReduction);
			num3 *= Utils.FastMin(_entity.GetWeight(), this.GetWeight()) / Utils.FastMax(_entity.GetWeight(), this.GetWeight());
			vector.x *= num3;
			vector.z *= num3;
			this.AddVelocity(new Vector3(-vector.x, 0f, -vector.z));
			if (_entity.CanBePushed())
			{
				_entity.AddVelocity(new Vector3(vector.x, 0f, vector.z));
			}
		}
	}

	public virtual void AddVelocity(Vector3 _vel)
	{
		this.motion += _vel;
		this.SetAirBorne(true);
	}

	public virtual Vector3 GetVelocityPerSecond()
	{
		if (this.AttachedToEntity)
		{
			return this.AttachedToEntity.GetVelocityPerSecond();
		}
		if (this.physicsRB)
		{
			return this.physicsRB.velocity;
		}
		return this.motion * 20f;
	}

	public virtual Vector3 GetAngularVelocityPerSecond()
	{
		if (this.AttachedToEntity)
		{
			return this.AttachedToEntity.GetAngularVelocityPerSecond();
		}
		if (this.physicsRB)
		{
			return this.physicsRB.angularVelocity;
		}
		return Vector3.zero;
	}

	public virtual void SetVelocityPerSecond(Vector3 vel, Vector3 angularVel)
	{
		if (this.AttachedToEntity)
		{
			this.AttachedToEntity.SetVelocityPerSecond(vel, angularVel);
			return;
		}
		this.physicsVel = vel;
		this.physicsAngVel = angularVel;
		if (this.isPhysicsMaster && this.physicsRB)
		{
			this.physicsRB.velocity = vel;
			this.physicsRB.angularVelocity = angularVel;
		}
		this.motion = vel * 0.05f;
	}

	public virtual bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float GetPushBoundsVertical()
	{
		return 0f;
	}

	public virtual bool CanCollideWith(Entity _other)
	{
		return true;
	}

	public virtual void OnUpdatePosition(float _partialTicks)
	{
		this.ticksExisted++;
		this.prevPos = this.position;
		this.prevRotation = this.rotation;
		if (this.isUpdatePosition)
		{
			if (this.AttachedToEntity || (this.emodel && this.emodel.IsRagdollOn))
			{
				this.isUpdatePosition = false;
			}
			else
			{
				switch (this.positionUpdateMovementType)
				{
				case Entity.EnumPositionUpdateMovementType.Lerp:
					this.SetPosition(Vector3.Lerp(this.position, this.targetPos, Time.deltaTime / Time.fixedDeltaTime * Entity.tickPositionLerpMultiplier), false);
					goto IL_D6;
				case Entity.EnumPositionUpdateMovementType.MoveTowards:
					this.SetPosition(Vector3.MoveTowards(this.position, this.targetPos, Entity.tickPositionMoveTowardsMaxDistance), false);
					goto IL_D6;
				}
				this.SetPosition(this.targetPos, false);
				IL_D6:
				if (this.position == this.targetPos)
				{
					this.isUpdatePosition = false;
				}
				if (this.PhysicsTransform != null)
				{
					this.physicsTargetPos = this.position - Origin.position;
					this.PhysicsTransform.position = this.physicsTargetPos;
				}
			}
		}
		if (this.interpolateTargetQRot > 0)
		{
			this.qrotation = Quaternion.Lerp(this.qrotation, this.targetQRot, 1f / (float)this.interpolateTargetQRot);
			this.interpolateTargetQRot--;
		}
		if (this.interpolateTargetRot > 0)
		{
			float t = 1f / (float)this.interpolateTargetRot;
			this.SetRotation(new Vector3(Mathf.LerpAngle(this.rotation.x, this.targetRot.x, t), Mathf.LerpAngle(this.rotation.y, this.targetRot.y, t), Mathf.LerpAngle(this.rotation.z, this.targetRot.z, t)));
			this.interpolateTargetRot--;
		}
		if (!this.isEntityRemote && !this.IsDead() && !this.IsClientControlled() && this.position.y < 0f && this.IsDeadIfOutOfWorld())
		{
			EntityDrone entityDrone = this as EntityDrone;
			if (entityDrone)
			{
				entityDrone.NotifyOffTheWorld();
				return;
			}
			Log.Warning(string.Concat(new string[]
			{
				"Entity ",
				(this != null) ? this.ToString() : null,
				" fell off the world, id=",
				this.entityId.ToString(),
				" pos=",
				this.position.ToCultureInvariantString()
			}));
			this.MarkToUnload();
		}
	}

	public virtual void CheckPosition()
	{
		if (float.IsNaN(this.position.x) || float.IsInfinity(this.position.x))
		{
			this.position.x = this.lastTickPos[0].x;
		}
		if (float.IsNaN(this.position.y) || float.IsInfinity(this.position.y))
		{
			this.position.y = this.lastTickPos[0].y;
		}
		if (float.IsNaN(this.position.z) || float.IsInfinity(this.position.z))
		{
			this.position.z = this.lastTickPos[0].z;
		}
		if (float.IsNaN(this.rotation.x) || float.IsInfinity(this.rotation.x))
		{
			this.rotation.x = this.prevRotation.x;
		}
		if (float.IsNaN(this.rotation.y) || float.IsInfinity(this.rotation.y))
		{
			this.rotation.y = this.prevRotation.y;
		}
		if (float.IsNaN(this.rotation.z) || float.IsInfinity(this.rotation.z))
		{
			this.rotation.z = this.prevRotation.z;
		}
	}

	public virtual void OnUpdateEntity()
	{
		bool flag = this.isInWater;
		if (!this.isEntityStatic())
		{
			this.TickInWater();
		}
		if (this.isEntityRemote)
		{
			return;
		}
		if (this.isInWater)
		{
			if (!flag && !this.firstUpdate && this.fallDistance > 1f)
			{
				this.PlayOneShot("waterfallinginto", false, false, false);
			}
			this.fallDistance = 0f;
		}
		if (!this.RootMotion && !this.IsDead() && this.CanBePushed())
		{
			List<Entity> entitiesInBounds = this.world.GetEntitiesInBounds(this, BoundsUtils.ExpandBounds(this.boundingBox, 0.2f, this.GetPushBoundsVertical(), 0.2f));
			if (entitiesInBounds != null && entitiesInBounds.Count > 0)
			{
				for (int i = 0; i < entitiesInBounds.Count; i++)
				{
					Entity entity = entitiesInBounds[i];
					this.OnPushEntity(entity);
				}
			}
		}
		this.firstUpdate = false;
	}

	public virtual void OnAddedToWorld()
	{
	}

	public virtual void OnEntityUnload()
	{
		if (this.isUnloaded)
		{
			Log.Warning("OnEntityUnload already unloaded {0} ", new object[]
			{
				this.GetDebugName()
			});
			return;
		}
		this.isUnloaded = true;
		Manager.DestroySoundsForEntity(this.entityId);
		if (this.movableChunkObserver != null)
		{
			this.movableChunkObserver.Dispose();
			this.movableChunkObserver = null;
		}
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity != null)
				{
					entity.Detach();
				}
			}
		}
		if (this.AttachedToEntity != null)
		{
			this.Detach();
		}
		if (this.emodel != null)
		{
			this.emodel.OnUnload();
		}
		try
		{
			UnityEngine.Object.Destroy(this.RootTransform.gameObject);
		}
		catch (Exception e)
		{
			Log.Error("OnEntityUnload: {0}", new object[]
			{
				this.GetDebugName()
			});
			Log.Exception(e);
		}
	}

	public virtual float GetLightBrightness()
	{
		Vector3i blockPosition = this.GetBlockPosition();
		Vector3i blockPos = blockPosition;
		blockPos.y += Mathf.RoundToInt(this.height + 0.5f);
		return Utils.FastMax(this.world.GetLightBrightness(blockPosition), this.world.GetLightBrightness(blockPos));
	}

	public Vector3i GetBlockPosition()
	{
		return World.worldToBlockPos(this.position);
	}

	public virtual void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		this.serverPos = NetEntityDistributionEntry.EncodePos(_pos);
		this.SetPosition(_pos, true);
		this.SetRotation(_rot);
		this.SetPosAndRotFromNetwork(_pos, _rot, 0);
		this.ResetLastTickPos(_pos);
		base.transform.SetPositionAndRotation(this.position - Origin.position, Quaternion.Euler(this.rotation));
	}

	public Vector3 GetPosition()
	{
		return this.position;
	}

	public virtual void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		this.position = _pos;
		float num = this.width * 0.5f;
		float num2 = this.depth * 0.5f;
		float num3 = _pos.y - this.yOffset + this.ySize;
		this.boundingBox = BoundsUtils.BoundsForMinMax(_pos.x - num, num3, _pos.z - num2, _pos.x + num, num3 + this.height, _pos.z + num2);
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity != null)
				{
					entity.SetPosition(_pos, false);
				}
			}
		}
		if (_bUpdatePhysics && this.PhysicsTransform != null)
		{
			this.PhysicsTransform.position = _pos - Origin.position;
			if (this.physicsRBT)
			{
				this.physicsPos = _pos - Origin.position + this.physicsBasePos;
				this.physicsRBT.position = this.physicsPos;
				this.physicsTargetPos = this.PhysicsTransform.position;
			}
		}
	}

	public void SetRotationAndStopTurning(Vector3 _rot)
	{
		this.SetRotation(_rot);
		this.yawSeekTimeMax = 0f;
		this.interpolateTargetQRot = 0;
		this.interpolateTargetRot = 0;
	}

	public virtual void SetRotation(Vector3 _rot)
	{
		this.rotation = _rot;
		this.qrotation = Quaternion.Euler(_rot);
	}

	public void SetPosAndRotFromNetwork(Vector3 _pos, Vector3 _rot, int _steps)
	{
		this.targetPos = _pos;
		this.targetRot = _rot;
		this.isUpdatePosition = true;
		this.interpolateTargetRot = _steps;
	}

	public void SetPosAndQRotFromNetwork(Vector3 _pos, Quaternion _rot, int _steps)
	{
		this.targetPos = _pos;
		this.targetQRot = _rot;
		this.isUpdatePosition = true;
		this.interpolateTargetQRot = _steps;
	}

	public void SetRotFromNetwork(Vector3 _rot, int _steps)
	{
		this.targetRot = _rot;
		this.interpolateTargetRot = _steps;
	}

	public void SetQRotFromNetwork(Quaternion _qrot, int _steps)
	{
		this.targetQRot = _qrot;
		this.interpolateTargetQRot = _steps;
	}

	public float GetBrightness(float _t)
	{
		int num = Utils.Fastfloor(this.position.x);
		int num2 = Utils.Fastfloor(this.position.z);
		if (this.world.GetChunkSync(World.toChunkXZ(num), World.toChunkXZ(num2)) != null)
		{
			float num3 = (this.boundingBox.max.y - this.boundingBox.min.y) * 0.66f;
			int y = Utils.Fastfloor((double)this.position.y - (double)this.yOffset + (double)num3);
			return this.world.GetLightBrightness(new Vector3i(num, y, num2));
		}
		return 0f;
	}

	public virtual void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
	}

	public void SetIgnoredByAI(bool ignore)
	{
		this.isIgnoredByAI = ignore;
	}

	public virtual bool IsIgnoredByAI()
	{
		return this.isIgnoredByAI;
	}

	public virtual Vector3 getHeadPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight(), 0f);
		}
		return this.emodel.GetHeadPosition();
	}

	public virtual Vector3 getNavObjectPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight(), 0f);
		}
		return this.emodel.GetNavObjectPosition();
	}

	public virtual Vector3 getBellyPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight() / 2f, 0f);
		}
		return this.emodel.GetBellyPosition();
	}

	public virtual Vector3 getHipPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight() / 2f, 0f);
		}
		return this.emodel.GetHipPosition();
	}

	public virtual Vector3 getChestPosition()
	{
		if (this.emodel == null)
		{
			return this.position + new Vector3(0f, this.GetEyeHeight() / 2.4f, 0f);
		}
		return this.emodel.GetChestPosition();
	}

	public void SetVelocity(Vector3 _vel)
	{
		this.motion = _vel;
	}

	public virtual float GetWeight()
	{
		return 1f;
	}

	public virtual float GetPushFactor()
	{
		return 1f;
	}

	public virtual float GetSightDetectionScale()
	{
		return 1f;
	}

	public virtual void OnLoadedFromEntityCache(EntityCreationData _ed)
	{
		if (this.bIsChunkObserver && !this.isEntityRemote)
		{
			this.movableChunkObserver = new MovableSharedChunkObserver(this.world.m_SharedChunkObserverCache);
			this.movableChunkObserver.SetPosition(this.position);
		}
	}

	public virtual bool IsSavedToNetwork()
	{
		return true;
	}

	public virtual bool IsSavedToFile()
	{
		return !this.world.IsEditor() || !GameManager.Instance.GetDynamicPrefabDecorator().IsEntityInPrefab(this.entityId);
	}

	public virtual void SetEntityName(string _name)
	{
	}

	public virtual void CopyPropertiesFromEntityClass()
	{
		EntityClass entityClass = EntityClass.list[this.entityClass];
		this.RootMotion = entityClass.RootMotion;
		this.HasDeathAnim = entityClass.HasDeathAnim;
		this.entityFlags = entityClass.entityFlags;
		this.entityType = EntityType.Unknown;
		entityClass.Properties.ParseEnum<EntityType>(EntityClass.PropEntityType, ref this.entityType);
		entityClass.Properties.ParseFloat(EntityClass.PropLootDropProb, ref this.lootDropProb);
		this.lootListOnDeath = entityClass.Properties.GetString(EntityClass.PropLootListOnDeath);
		entityClass.Properties.ParseString(EntityClass.PropLootListAlive, ref this.lootListAlive);
		entityClass.Properties.ParseString(EntityClass.PropMapIcon, ref this.mapIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassIcon, ref this.compassIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassUpIcon, ref this.compassUpIcon);
		entityClass.Properties.ParseString(EntityClass.PropCompassDownIcon, ref this.compassDownIcon);
		entityClass.Properties.ParseString(EntityClass.PropTrackerIcon, ref this.trackerIcon);
		entityClass.Properties.ParseBool(EntityClass.PropRotateToGround, ref this.isRotateToGround);
	}

	public virtual string GetLootList()
	{
		return this.lootListAlive;
	}

	public virtual void MarkToUnload()
	{
		this.markedForUnload = true;
	}

	public virtual bool IsMarkedForUnload()
	{
		return this.markedForUnload || this.IsDead();
	}

	public virtual bool IsSpawned()
	{
		return true;
	}

	public void ResetLastTickPos(Vector3 _pos)
	{
		for (int i = 0; i < this.lastTickPos.Length; i++)
		{
			this.lastTickPos[i] = _pos;
		}
	}

	public void SetLastTickPos(Vector3 _pos)
	{
		for (int i = this.lastTickPos.Length - 1; i > 0; i--)
		{
			this.lastTickPos[i] = this.lastTickPos[i - 1];
		}
		this.lastTickPos[0] = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isDetailedHeadBodyColliders()
	{
		return false;
	}

	public virtual Transform GetModelTransform()
	{
		return null;
	}

	public virtual Vector3 GetMapIconScale()
	{
		return new Vector3(1f, 1f, 1f);
	}

	public virtual string GetMapIcon()
	{
		return this.mapIcon;
	}

	public virtual string GetCompassIcon()
	{
		if (this.compassIcon == null)
		{
			return this.mapIcon;
		}
		return this.compassIcon;
	}

	public virtual string GetCompassUpIcon()
	{
		return this.compassUpIcon;
	}

	public virtual string GetCompassDownIcon()
	{
		return this.compassDownIcon;
	}

	public virtual string GetTrackerIcon()
	{
		return this.trackerIcon;
	}

	public virtual bool HasUIIcon()
	{
		return this.mapIcon != null || this.trackerIcon != null || this.compassIcon != null;
	}

	public virtual EnumMapObjectType GetMapObjectType()
	{
		return EnumMapObjectType.Entity;
	}

	public virtual bool IsMapIconBlinking()
	{
		return false;
	}

	public virtual bool IsDrawMapIcon()
	{
		return this.IsSpawned();
	}

	public virtual Color GetMapIconColor()
	{
		return Color.white;
	}

	public virtual bool CanMapIconBeSelected()
	{
		return false;
	}

	public virtual int GetLayerForMapIcon()
	{
		return 2;
	}

	public virtual bool IsClientControlled()
	{
		return this.attachedEntities != null && this.attachedEntities.Length != 0 && this.attachedEntities[0] != null;
	}

	public virtual bool IsDeadIfOutOfWorld()
	{
		return true;
	}

	public virtual bool CanCollideWithBlocks()
	{
		return true;
	}

	public void SetSpawnerSource(EnumSpawnerSource _spawnerSource)
	{
		this.SetSpawnerSource(_spawnerSource, 0L, 0);
	}

	public void SetSpawnerSource(EnumSpawnerSource _spawnerSource, long _chunkKey, int _biomeIdHash)
	{
		this.spawnerSource = _spawnerSource;
		this.spawnerSourceChunkKey = _chunkKey;
		this.spawnerSourceBiomeIdHash = _biomeIdHash;
	}

	public EnumSpawnerSource GetSpawnerSource()
	{
		return this.spawnerSource;
	}

	public long GetSpawnerSourceChunkKey()
	{
		return this.spawnerSourceChunkKey;
	}

	public int GetSpawnerSourceBiomeIdHash()
	{
		return this.spawnerSourceBiomeIdHash;
	}

	public float CalculateAudioOcclusion()
	{
		return 0f;
	}

	public virtual void PlayOneShot(string clipName, bool sound_in_head = false, bool serverSignalOnly = false, bool isUnique = false)
	{
		if (sound_in_head)
		{
			Manager.PlayInsidePlayerHead(clipName, -1, 0f, false, isUnique);
			return;
		}
		if (serverSignalOnly)
		{
			Manager.Play(this, clipName, 1f, false);
			return;
		}
		Manager.BroadcastPlay(this, clipName, serverSignalOnly);
	}

	public void StopOneShot(string clipName)
	{
		Manager.BroadcastStop(this.entityId, clipName);
	}

	public virtual EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (this.lootContainer == null)
		{
			this.cmds[0].enabled = false;
		}
		return this.cmds;
	}

	public virtual bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (_indexInBlockActivationCommands == 0)
		{
			GameManager.Instance.TELockServer(0, _tePos, this.entityId, _entityFocusing.entityId, null);
			return true;
		}
		return false;
	}

	public void SetAttachMaxCount(int maxCount)
	{
		if (this.attachedEntities != null)
		{
			if (this.attachedEntities.Length == maxCount)
			{
				return;
			}
			for (int i = maxCount; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity)
				{
					entity.Detach();
				}
			}
		}
		Entity[] array = this.attachedEntities;
		this.attachedEntities = null;
		if (maxCount > 0)
		{
			this.attachedEntities = new Entity[maxCount];
			if (array != null)
			{
				int num = Utils.FastMin(array.Length, maxCount);
				for (int j = 0; j < num; j++)
				{
					this.attachedEntities[j] = array[j];
				}
			}
		}
	}

	public int GetAttachMaxCount()
	{
		if (this.attachedEntities != null)
		{
			return (int)((byte)this.attachedEntities.Length);
		}
		return 0;
	}

	public int GetAttachFreeCount()
	{
		int num = 0;
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				if (this.attachedEntities[i] == null)
				{
					num++;
				}
			}
		}
		return num;
	}

	public Entity GetAttached(int slot)
	{
		if (this.attachedEntities != null && slot < this.attachedEntities.Length)
		{
			return this.attachedEntities[slot];
		}
		return null;
	}

	public Entity AttachedMainEntity
	{
		get
		{
			if (this.attachedEntities == null)
			{
				return null;
			}
			return this.attachedEntities[0];
		}
	}

	public Entity GetFirstAttached()
	{
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				Entity entity = this.attachedEntities[i];
				if (entity)
				{
					return entity;
				}
			}
		}
		return null;
	}

	public EntityPlayerLocal GetAttachedPlayerLocal()
	{
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				EntityPlayerLocal entityPlayerLocal = this.attachedEntities[i] as EntityPlayerLocal;
				if (entityPlayerLocal)
				{
					return entityPlayerLocal;
				}
			}
		}
		return null;
	}

	public bool CanAttach(Entity _entity)
	{
		return this.FindAttachSlot(_entity) < 0 && this.FindAttachSlot(null) >= 0;
	}

	public int FindAttachSlot(Entity _entity)
	{
		if (this.attachedEntities != null)
		{
			for (int i = 0; i < this.attachedEntities.Length; i++)
			{
				if (this.attachedEntities[i] == _entity)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool IsAttached(Entity _entity)
	{
		return this.FindAttachSlot(_entity) >= 0;
	}

	public bool IsDriven()
	{
		return this.attachedEntities != null && this.attachedEntities[0];
	}

	public virtual int AttachEntityToSelf(Entity _other, int slot)
	{
		int num = this.FindAttachSlot(_other);
		if (num >= 0)
		{
			if (slot < 0 || slot == num)
			{
				return num;
			}
			this.DetachEntity(_other);
		}
		if (slot < 0)
		{
			slot = this.FindAttachSlot(null);
			if (slot < 0)
			{
				return -1;
			}
		}
		if (slot >= this.attachedEntities.Length)
		{
			return -1;
		}
		if (slot == 0)
		{
			this.serverPos = NetEntityDistributionEntry.EncodePos(this.position);
			this.isEntityRemote = _other.isEntityRemote;
		}
		this.attachedEntities[slot] = _other;
		return slot;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DetachEntity(Entity _other)
	{
		int num = this.FindAttachSlot(_other);
		if (num < 0)
		{
			return;
		}
		if (num == 0)
		{
			this.isEntityRemote = this.world.IsRemote();
		}
		this.attachedEntities[num] = null;
	}

	public virtual void StartAttachToEntity(Entity _other, int slot = -1)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachServer, this.entityId, _other.entityId, slot), false);
			return;
		}
		slot = this.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachClient, this.entityId, _other.entityId, slot), false, -1, -1, -1, null, 192);
		}
	}

	public virtual int AttachToEntity(Entity _other, int slot = -1)
	{
		if (_other.IsAttached(this))
		{
			return -1;
		}
		slot = _other.AttachEntityToSelf(this, slot);
		if (slot < 0)
		{
			return slot;
		}
		AttachedToEntitySlotInfo attachedToInfo = _other.GetAttachedToInfo(slot);
		this.RootTransform.SetParent(attachedToInfo.enterParentTransform, false);
		this.RootTransform.localPosition = Vector3.zero;
		this.RootTransform.localEulerAngles = Vector3.zero;
		this.ModelTransform.localPosition = attachedToInfo.enterPosition;
		this.ModelTransform.localEulerAngles = attachedToInfo.enterRotation;
		this.rotation = attachedToInfo.enterRotation;
		if (this.isEntityRemote && !attachedToInfo.bKeep3rdPersonModelVisible)
		{
			this.emodel.SetVisible(false, false);
		}
		this.AttachedToEntity = _other;
		return slot;
	}

	public void SendDetach()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachServer, this.entityId, -1, -1), false);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachClient, this.entityId, -1, -1), false, -1, -1, -1, null, 192);
		}
		this.Detach();
	}

	public virtual void Detach()
	{
		this.RootTransform.parent = EntityFactory.ParentNameToTransform[EntityClass.list[this.entityClass].parentGameObjectName];
		if (this.AttachedToEntity == null)
		{
			return;
		}
		int num = this.AttachedToEntity.FindAttachSlot(this);
		if (num < 0)
		{
			num = 0;
		}
		AttachedToEntitySlotInfo attachedToInfo = this.AttachedToEntity.GetAttachedToInfo(num);
		AttachedToEntitySlotExit attachedToEntitySlotExit = this.FindValidExitPosition(attachedToInfo.exits);
		Entity attachedToEntity = this.AttachedToEntity;
		this.AttachedToEntity = null;
		this.isUpdatePosition = false;
		if (attachedToEntitySlotExit.position != Vector3.zero)
		{
			this.SetPosition(attachedToEntitySlotExit.position, true);
			this.SetRotation(attachedToEntitySlotExit.rotation);
		}
		this.ResetLastTickPos(base.transform.position + Origin.position);
		attachedToEntity.DetachEntity(this);
		if (this.isEntityRemote && !attachedToInfo.bKeep3rdPersonModelVisible)
		{
			this.emodel.SetVisible(true, false);
		}
	}

	public virtual void MoveByAttachedEntity(EntityPlayerLocal _player)
	{
	}

	public virtual AttachedToEntitySlotExit FindValidExitPosition(List<AttachedToEntitySlotExit> candidatePositions)
	{
		AttachedToEntitySlotExit attachedToEntitySlotExit;
		attachedToEntitySlotExit.position = Vector3.zero;
		attachedToEntitySlotExit.rotation = Vector3.zero;
		if (this.m_characterController == null)
		{
			return attachedToEntitySlotExit;
		}
		this.AttachedToEntity.SetPhysicsCollidersLayer(14);
		float radius = this.m_characterController.GetRadius();
		float num = this.m_characterController.GetHeight() - radius * 2f;
		Vector3 vector = base.transform.position + this.m_characterController.GetCenter();
		vector.y -= num * 0.5f;
		for (int i = 0; i < candidatePositions.Count; i++)
		{
			for (float num2 = 0f; num2 < 0.75f; num2 += 0.24f)
			{
				Vector3 vector2 = vector;
				vector2.y += num2;
				attachedToEntitySlotExit = candidatePositions[i];
				attachedToEntitySlotExit.position.y = attachedToEntitySlotExit.position.y + num2;
				Vector3 vector3 = attachedToEntitySlotExit.position - Origin.position - vector2;
				vector3.y += radius;
				Vector3 normalized = vector3.normalized;
				float num3 = vector3.magnitude;
				if (normalized.y < 0f)
				{
					float num4 = normalized.y;
					if (num4 < -0.707f)
					{
						break;
					}
					num4 *= -1.6f;
					num3 += num4;
					attachedToEntitySlotExit.position += normalized * num4;
				}
				bool flag = false;
				Vector3 origin = vector2;
				for (float num5 = -radius * 0.5f; num5 < num; num5 += 0.2f)
				{
					origin.y = vector2.y + num5;
					flag = Physics.Raycast(origin, normalized, num3, 1084817408);
					if (flag)
					{
						break;
					}
				}
				Vector3 vector4 = vector2 - normalized * 0.1f;
				Vector3 point = vector4;
				point.y += num;
				if (!flag && !Physics.CapsuleCast(vector4, point, radius, normalized, num3, 1084817408))
				{
					this.AttachedToEntity.SetPhysicsCollidersLayer(21);
					return attachedToEntitySlotExit;
				}
			}
		}
		this.AttachedToEntity.SetPhysicsCollidersLayer(21);
		attachedToEntitySlotExit.position = Vector3.zero;
		return attachedToEntitySlotExit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPhysicsCollidersLayer(int layer)
	{
		Collider[] componentsInChildren = this.PhysicsTransform.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DebugCapsuleCast()
	{
	}

	public virtual AttachedToEntitySlotInfo GetAttachedToInfo(int _slotIdx)
	{
		return null;
	}

	public virtual bool CanUpdateEntity()
	{
		Vector3i vector3i = World.worldToBlockPos(this.position);
		IChunk chunkFromWorldPos = this.world.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
		if (chunkFromWorldPos == null || !chunkFromWorldPos.GetAvailable())
		{
			return false;
		}
		for (int i = 0; i < this.adjacentPositions.Length; i++)
		{
			int num = World.toChunkXZ(vector3i.x + this.adjacentPositions[i].x);
			int num2 = World.toChunkXZ(vector3i.z + this.adjacentPositions[i].z);
			if (num != chunkFromWorldPos.X || num2 != chunkFromWorldPos.Z)
			{
				IChunk chunkSync = this.world.GetChunkSync(num, num2);
				if (chunkSync == null || !chunkSync.GetAvailable())
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual Transform GetThirdPersonCameraTransform()
	{
		return this.emodel.GetThirdPersonCameraTransform();
	}

	public virtual void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		_bw.Write((byte)this.spawnerSource);
		if (this.spawnerSource == EnumSpawnerSource.Biome)
		{
			_bw.Write(this.spawnerSourceBiomeIdHash);
			_bw.Write(this.spawnerSourceChunkKey);
		}
		_bw.Write(this.WorldTimeBorn);
	}

	public virtual void Read(byte _version, BinaryReader _br)
	{
		if (_version >= 11)
		{
			this.spawnerSource = (EnumSpawnerSource)_br.ReadByte();
			if (this.spawnerSource == EnumSpawnerSource.Biome)
			{
				if (_version >= 28)
				{
					this.spawnerSourceBiomeIdHash = _br.ReadInt32();
				}
				else
				{
					_br.ReadString();
					this.spawnerSource = EnumSpawnerSource.Delete;
				}
				this.spawnerSourceChunkKey = _br.ReadInt64();
			}
		}
		if (_version >= 15)
		{
			this.WorldTimeBorn = _br.ReadUInt64();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isEntityStatic()
	{
		return false;
	}

	public virtual void AddUIHarvestingItem(ItemStack _is, bool _bAddOnlyIfNotExisting = false)
	{
	}

	public virtual bool IsQRotationUsed()
	{
		return false;
	}

	public bool HasAnyTags(FastTags<TagGroup.Global> tags)
	{
		return this.cachedTags.Test_AnySet(tags);
	}

	public bool HasAllTags(FastTags<TagGroup.Global> tags)
	{
		return this.cachedTags.Test_AllSet(tags);
	}

	public void SetTransformActive(string partName, bool active)
	{
		Transform transform = base.transform.FindInChilds(partName, false);
		if (transform != null)
		{
			transform.gameObject.SetActive(active);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleNavObject()
	{
		if (EntityClass.list[this.entityClass].NavObject != "")
		{
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this, "", false);
		}
	}

	public void AddNavObject(string navObjectName, string overrideSprite, string overrideText)
	{
		if (this.NavObject == null)
		{
			NavObjectManager.Instance.RegisterNavObject(navObjectName, this, overrideSprite, false).name = overrideText;
			return;
		}
		NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
		this.NavObject.name = overrideText;
		this.NavObject.AddNavObjectClass(navObjectClass);
	}

	public void RemoveNavObject(string navObjectName)
	{
		NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
		if (this.NavObject != null && this.NavObject.RemoveNavObjectClass(navObjectClass))
		{
			this.NavObject = null;
		}
	}

	public string GetDebugName()
	{
		EntityAlive entityAlive = this as EntityAlive;
		if (entityAlive != null)
		{
			return entityAlive.EntityName;
		}
		return base.GetType().ToString();
	}

	public const int EntityIdInvalid = -1;

	public const int cIdCreatorIsServer = -2;

	public const int cClientIdStart = -2;

	public const int cClientIdCreate = -1;

	public const int cClientIdNone = 0;

	public const int cKillAnythingDamage = 99999;

	public const int cIgnoreDamage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public FastTags<TagGroup.Global> cachedTags;

	public bool RootMotion;

	public bool HasDeathAnim;

	public World world;

	public Transform PhysicsTransform;

	public Transform RootTransform;

	public Transform ModelTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 scaledExtent;

	public Bounds boundingBox;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Collider nativeCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int interpolateTargetRot;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int interpolateTargetQRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUpdatePosition;

	public int entityId;

	public int clientEntityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float yOffset;

	public bool onGround;

	public bool isCollided;

	public bool isCollidedHorizontally;

	public bool isCollidedVertically;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isMotionSlowedDown;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float motionMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool firstUpdate = true;

	public Vector3 prevRotation;

	public Vector3 rotation;

	public Quaternion qrotation = Quaternion.identity;

	public Vector3 position;

	public Vector3 prevPos;

	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 targetRot;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion targetQRot = Quaternion.identity;

	public Vector3i chunkPosAddedEntityTo;

	public Vector3i serverPos;

	public Vector3i serverRot;

	public Vector3[] lastTickPos = new Vector3[5];

	public Vector3 motion;

	public bool IsMovementReplicated = true;

	public bool IsStuck;

	public bool IsEntityUpdatedInUnloadedChunk;

	public bool addedToChunk;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInWater;

	public bool isSwimming;

	public float inWaterLevel;

	public float inWaterPercent;

	public bool isHeadUnderwater;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bInElevator;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bAirBorne;

	public float stepHeight;

	public float ySize;

	public float distanceWalked;

	public float distanceSwam;

	public float distanceClimbed;

	public float fallDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallLastY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallVelY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 fallLastMotion;

	public float entityCollisionReduction = 0.9f;

	public bool isEntityRemote;

	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int ticksExisted;

	public static float updatePositionLerpTimeScale = 8f;

	public static float updateRotationLerpTimeScale = 8f;

	public static float tickPositionMoveTowardsMaxDistance = 3f;

	public static float tickPositionLerpMultiplier = 0.5f;

	public int entityClass;

	public float lifetime;

	public int count;

	public int belongsPlayerId;

	public bool bWillRespawn;

	public ulong WorldTimeBorn;

	public DataItem<bool> IsFlyMode = new DataItem<bool>();

	public DataItem<bool> IsGodMode = new DataItem<bool>();

	public DataItem<bool> IsNoCollisionMode = new DataItem<bool>();

	public EntityFlags entityFlags;

	public EntityType entityType;

	public float lootDropProb;

	public string lootListOnDeath;

	public string lootListAlive;

	public TileEntityLootContainer lootContainer;

	public float speedForward;

	public float speedStrafe;

	public float speedVertical;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int speedSentTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedForwardSent = float.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedStrafeSent = float.MaxValue;

	public int MovementState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekTimeMax;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float yawSeekAngleEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public IAIDirectorMarker m_marker;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string mapIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string compassIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string compassUpIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string compassDownIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string trackerIcon;

	public bool bDead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bWasDead;

	[Preserve]
	public EModelBase emodel;

	public CharacterControllerAbstract m_characterController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCCDelayed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canCCMove;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CollisionFlags collisionFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity.MoveHitSurface groundSurface;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 hitMove;

	public float projectedMove;

	public bool IsRotateToGroundFlat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isRotateToGround;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotateToGroundPitch;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotateToGroundPitchVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EnumSpawnerSource spawnerSource;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int spawnerSourceBiomeIdHash;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long spawnerSourceChunkKey;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityActivationCommand[] cmds = new EntityActivationCommand[]
	{
		new EntityActivationCommand("Search", "search", true)
	};

	public static int InstanceCount;

	public bool IsDespawned;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool markedForUnload;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public MovableSharedChunkObserver movableChunkObserver;

	public bool bIsChunkObserver;

	public NavObject NavObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isIgnoredByAI;

	public Entity AttachedToEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity[] attachedEntities;

	public const int cPhysicsMasterTickRate = 2;

	public bool usePhysicsMaster;

	public bool isPhysicsMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsMasterFromPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsMasterFromRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsMasterTargetElapsed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsMasterTargetTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsMasterTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsMasterTargetRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsMasterSendPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsMasterSendRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsHeightScale = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform physicsRBT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CapsuleCollider physicsCapsuleCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float physicsColliderRadius;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float physicsColliderLowerY;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float physicsBaseHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float physicsHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody physicsRB;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsBasePos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physicsPosMoveDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion physicsRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasFixedUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsAngVel;

	public bool spawnByAllowShare;

	public int spawnById = -1;

	public string spawnByName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWaterHeightScale = 1.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] waterLevelDirOffsets = new float[]
	{
		Mathf.Cos(0f),
		Mathf.Sin(0f),
		Mathf.Cos(2.09439516f),
		Mathf.Sin(2.09439516f),
		Mathf.Cos(4.18879032f),
		Mathf.Sin(4.18879032f)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Bounds> collAABB = new List<Bounds>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float kAddFixedUpdateTimeScale = 1f;

	public EnumRemoveEntityReason unloadReason;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUnloaded;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cAttachSlotNone = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i[] adjacentPositions = new Vector3i[]
	{
		Vector3i.forward,
		Vector3i.back,
		Vector3i.left,
		Vector3i.right
	};

	public struct MoveHitSurface
	{
		public Vector3 hitPoint;

		public Vector3 lastHitPoint;

		public Vector3 normal;

		public Vector3 lastNormal;
	}

	public enum EnumPositionUpdateMovementType
	{
		Lerp,
		MoveTowards,
		Instant
	}
}
