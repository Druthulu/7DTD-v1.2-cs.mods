using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGazeController : MonoBehaviour
{
	public Transform LookAtTarget
	{
		get
		{
			if (this.lookAtTarget == null)
			{
				this.lookAtTarget = new GameObject("LookAtTarget").transform;
				this.lookAtTarget.parent = this.rootTransform;
				this.lookAtTarget.localPosition = this.lookAtTargetDefaultPosition;
				return this.lookAtTarget;
			}
			return this.lookAtTarget;
		}
	}

	public GameRandom Random
	{
		get
		{
			if (this.random != null)
			{
				return this.random;
			}
			return this.random = GameRandomManager.Instance.CreateGameRandom();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.entityAlive = base.GetComponentInParent<EntityAlive>();
		if (this.eyeMaterial == null || this.eyeMaterial.shader.name != "Game/SDCS/Eye")
		{
			Debug.LogError("Eye Material is not valid");
			base.enabled = false;
			return;
		}
		this.gazeTimer = Time.realtimeSinceStartup + this.Random.RandomRange(0.25f, 2f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		CharacterGazeController.instances.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (CharacterGazeController.instances.Contains(this))
		{
			CharacterGazeController.instances.Remove(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (CharacterGazeController.instances.Contains(this))
		{
			CharacterGazeController.instances.Remove(this);
		}
		if (this.lookAtTarget != null)
		{
			UnityEngine.Object.Destroy(this.lookAtTarget.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.entityAlive != null)
		{
			if (this.entityAlive.emodel.IsRagdollActive)
			{
				return;
			}
			this.isDead = this.entityAlive.IsDead();
		}
		this.UpdateLookAtTarget();
		this.UpdateHeadRotation();
		this.UpdateEyeGaze();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookAtTarget()
	{
		CharacterGazeController characterGazeController = null;
		float num = float.PositiveInfinity;
		for (int i = CharacterGazeController.instances.Count - 1; i >= 0; i--)
		{
			CharacterGazeController characterGazeController2 = CharacterGazeController.instances[i];
			if (characterGazeController2 == null)
			{
				CharacterGazeController.instances.RemoveAt(i);
			}
			else if (characterGazeController2.enabled && characterGazeController2 != this)
			{
				Vector3 to = characterGazeController2.headTransform.position - this.headTransform.position;
				float num2 = Vector3.Angle(this.headTransform.forward, to);
				float magnitude = to.magnitude;
				if (num2 < this.eyeLookAtTargetAngle && magnitude <= this.maxLookAtDistance && num2 < num)
				{
					characterGazeController = characterGazeController2;
					num = num2;
				}
			}
		}
		if (characterGazeController != null)
		{
			this.lookAtCamera = true;
			this.LookAtTarget.position = characterGazeController.headTransform.position;
		}
		else
		{
			EntityPlayerLocal entityPlayerLocal;
			if (GameManager.Instance != null && GameManager.Instance.World != null)
			{
				entityPlayerLocal = GameManager.Instance.World.GetPrimaryPlayer();
			}
			else
			{
				entityPlayerLocal = null;
			}
			Vector3 vector = this.rootTransform.TransformPoint(this.lookAtTargetDefaultPosition);
			if (entityPlayerLocal != null && entityPlayerLocal.cameraTransform != null)
			{
				vector = entityPlayerLocal.cameraTransform.position;
			}
			else if (Camera.main != null)
			{
				vector = Camera.main.transform.position;
			}
			Vector3 to2 = vector - this.headTransform.position;
			float num3 = Vector3.Angle(this.headTransform.forward, to2);
			float magnitude2 = to2.magnitude;
			if (num3 < this.eyeLookAtTargetAngle && magnitude2 <= this.maxLookAtDistance)
			{
				this.lookAtCamera = true;
				this.LookAtTarget.position = vector;
			}
			else
			{
				this.lookAtCamera = false;
				this.LookAtTarget.localPosition = this.lookAtTargetDefaultPosition;
			}
		}
		if (Time.realtimeSinceStartup > this.gazeTimer)
		{
			this.lookatOffsetIndex = this.Random.RandomRange(0, this.lookatOffsets.Count);
			this.gazeTimer = Time.realtimeSinceStartup + this.Random.RandomRange(0.25f, 2f);
		}
	}

	public void SnapNextUpdate()
	{
		this.shouldSnapHeadNextUpdate = true;
		this.shouldSnapEyesNextUpdate = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadRotation()
	{
		Vector3 normalized = (this.LookAtTarget.position - this.headTransform.position).normalized;
		Vector3 normalized2 = (normalized + this.headTransform.forward).normalized;
		if (Vector3.Angle(this.headTransform.forward, normalized) < this.headLookAtTargetAngle && !this.isDead)
		{
			if (this.shouldSnapHeadNextUpdate)
			{
				this.currentHeadRotation = Quaternion.FromToRotation(this.headTransform.forward, normalized2);
				this.shouldSnapHeadNextUpdate = false;
			}
			else
			{
				this.currentHeadRotation = Quaternion.Slerp(this.currentHeadRotation, Quaternion.FromToRotation(this.headTransform.forward, normalized2), this.headRotationSpeed * Time.deltaTime);
			}
		}
		else if (this.shouldSnapHeadNextUpdate)
		{
			this.currentHeadRotation = Quaternion.identity;
			this.shouldSnapHeadNextUpdate = false;
		}
		else
		{
			this.currentHeadRotation = Quaternion.Slerp(this.currentHeadRotation, Quaternion.identity, this.headRotationSpeed * Time.deltaTime);
		}
		this.headTransform.rotation = this.currentHeadRotation * this.headTransform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEyeGaze()
	{
		Vector3 localPosition = this.LookAtTarget.localPosition;
		if (!this.lookAtCamera)
		{
			this.LookAtTarget.position += this.lookatOffsets[this.lookatOffsetIndex];
		}
		Vector3 toDirection = this.LookAtTarget.position - this.headTransform.TransformPoint(this.leftEyeLocalPosition);
		Vector3 toDirection2 = this.LookAtTarget.position - this.headTransform.TransformPoint(this.rightEyeLocalPosition);
		Quaternion b = Quaternion.FromToRotation(this.leftEyeTransform.forward, toDirection);
		Quaternion b2 = Quaternion.FromToRotation(this.rightEyeTransform.forward, toDirection2);
		float num = Mathf.Sin(Time.time * this.twitchSpeed) * 0.5f + 0.5f;
		if (this.isDead)
		{
			b = Quaternion.identity;
			b2 = Quaternion.identity;
		}
		if (this.shouldSnapEyesNextUpdate)
		{
			this.currentLeftEyeRotation = b;
			this.currentRightEyeRotation = b2;
			this.shouldSnapEyesNextUpdate = false;
		}
		else
		{
			this.currentLeftEyeRotation = Quaternion.Slerp(this.currentLeftEyeRotation, b, this.eyeRotationSpeed * num * Time.deltaTime);
			this.currentRightEyeRotation = Quaternion.Slerp(this.currentRightEyeRotation, b2, this.eyeRotationSpeed * num * Time.deltaTime);
		}
		this.currentLeftEyeRotation.x = Utils.FastClamp(this.currentLeftEyeRotation.x, -0.2f, 0.2f);
		this.currentLeftEyeRotation.y = Utils.FastClamp(this.currentLeftEyeRotation.y, -0.4f, 0.4f);
		this.currentRightEyeRotation.x = Utils.FastClamp(this.currentRightEyeRotation.x, -0.2f, 0.2f);
		this.currentRightEyeRotation.y = Utils.FastClamp(this.currentRightEyeRotation.y, -0.4f, 0.4f);
		this.eyeMaterial.SetVector("_LeftEyeRotation", new Vector4(-this.currentLeftEyeRotation.x, this.currentLeftEyeRotation.y, this.currentLeftEyeRotation.z, this.currentLeftEyeRotation.w));
		this.eyeMaterial.SetVector("_RightEyeRotation", new Vector4(-this.currentRightEyeRotation.x, this.currentRightEyeRotation.y, this.currentRightEyeRotation.z, this.currentRightEyeRotation.w));
		this.eyeMaterial.SetVector("_LeftEyePosition", this.leftEyeLocalPosition);
		this.eyeMaterial.SetVector("_RightEyePosition", this.rightEyeLocalPosition);
		this.LookAtTarget.localPosition = localPosition;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void Cleanup()
	{
		CharacterGazeController.instances.Clear();
	}

	public static List<CharacterGazeController> instances = new List<CharacterGazeController>();

	public Transform leftEyeTransform;

	public Transform rightEyeTransform;

	public Vector3 leftEyeLocalPosition;

	public Vector3 rightEyeLocalPosition;

	public Transform rootTransform;

	public Transform neckTransform;

	public Transform headTransform;

	[Range(0f, 100f)]
	public float eyeRotationSpeed = 5f;

	[Range(0f, 100f)]
	public float headRotationSpeed = 5f;

	[Range(0f, 50f)]
	public float twitchSpeed = 10f;

	public float eyeLookAtTargetAngle = 5f;

	public float headLookAtTargetAngle = 5f;

	[Range(0f, 20f)]
	public float maxLookAtDistance = 10f;

	public Material eyeMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lookAtCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform lookAtTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> lookatOffsets = new List<Vector3>
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(-2f, 0f, 0f),
		new Vector3(2f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 0f, -2f),
		new Vector3(0f, 0f, 2f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive entityAlive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentLeftEyeRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentRightEyeRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentHeadRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float gazeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lookatOffsetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool shouldSnapHeadNextUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool shouldSnapEyesNextUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lookAtTargetDefaultPosition = new Vector3(0f, 1.7f, 10f);
}
