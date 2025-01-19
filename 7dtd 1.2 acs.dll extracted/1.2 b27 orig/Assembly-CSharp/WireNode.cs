using System;
using System.Collections.Generic;
using UnityEngine;

public class WireNode : MonoBehaviour, IWireNode
{
	public void Init()
	{
		if (WireNode.rootShockPrefab == null)
		{
			WireNode.rootShockPrefab = Resources.Load<GameObject>("Prefabs/ElectricShock");
		}
		this.prevParent = (this.parent = base.transform.parent);
		if (this.parent != null)
		{
			this.prevParentGO = (this.parentGO = this.parent.gameObject);
			this.usingLocalPosition = false;
		}
		this.shockSpots = new List<WireNode.ShockSpot>();
		base.transform.parent = null;
		base.transform.position = Vector3.zero;
		base.transform.localEulerAngles = Vector3.zero;
		base.transform.localScale = Vector3.one;
		this.meshFilter = base.GetComponent<MeshFilter>();
		this.meshRenderer = base.GetComponent<MeshRenderer>();
		this.meshCollider = base.GetComponent<MeshCollider>();
		this.meshCollider.convex = true;
		this.meshCollider.isTrigger = true;
		this.mesh = new Mesh();
		this.mesh.MarkDynamic();
		this.shockNodes = new List<bool>();
		this.points = new List<Vector3>();
		this.forces = new List<Vector3>();
		this.verts = new List<Vector3>();
		this.uvs = new List<Vector2>();
		this.uvs2 = new List<Vector2>();
		this.normals = new List<Vector3>();
		this.indices = new int[24 * (1 + this.numNodes) + 12];
		if (this.personalMaterial == null)
		{
			this.personalMaterial = UnityEngine.Object.Instantiate<Material>(this.material);
		}
		this.meshRenderer.material = this.personalMaterial;
	}

	public void OnDestroy()
	{
		if (this.personalMaterial != null)
		{
			UnityEngine.Object.Destroy(this.personalMaterial);
			this.personalMaterial = null;
		}
		for (int i = 0; i < this.shockSpots.Count; i++)
		{
			UnityEngine.Object.Destroy(this.shockSpots[i].shockPrefab);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.Init();
	}

	public void ToggleMeshCollider(bool _bOn)
	{
		this.meshCollider.enabled = _bOn;
	}

	public void SetPulseSpeed(float speed)
	{
		if (this.personalMaterial == null)
		{
			return;
		}
		this.personalMaterial.SetFloat("_PulseSpeed", speed);
	}

	public void TogglePulse(bool isOn)
	{
		if (this.personalMaterial == null)
		{
			return;
		}
		this.personalMaterial.SetColor("_PulseColor", isOn ? this.pulseColor : Color.black);
	}

	public void SetPulseColor(Color color)
	{
		this.pulseColor = color;
	}

	public void PlayShockAtPosition(Vector3 shockPosition)
	{
		if (shockPosition.x < this.min.x - 1f)
		{
			return;
		}
		if (shockPosition.y < this.min.y - 1f)
		{
			return;
		}
		if (shockPosition.z < this.min.z - 1f)
		{
			return;
		}
		if (shockPosition.x > this.max.x + 1f)
		{
			return;
		}
		if (shockPosition.y > this.max.y + 1f)
		{
			return;
		}
		if (shockPosition.z > this.max.z + 1f)
		{
			return;
		}
		int num = 99999;
		float num2 = 99999f;
		for (int i = 1; i < this.points.Count - 1; i++)
		{
			Vector3 vector = this.points[i] - shockPosition;
			if (vector.magnitude < num2)
			{
				num2 = vector.magnitude;
				num = i;
			}
		}
		if (num2 > 1f)
		{
			return;
		}
		if (num < this.shockNodes.Count && this.shockNodes[num])
		{
			return;
		}
		WireNode.ShockSpot shockSpot;
		shockSpot.timer = Time.time;
		shockSpot.vertex = num;
		shockSpot.shockDataIndex = 0;
		shockSpot.shockPrefab = UnityEngine.Object.Instantiate<GameObject>(WireNode.rootShockPrefab);
		shockSpot.shockPrefab.transform.position = this.points[num];
		shockSpot.shockPrefab.transform.parent = base.transform;
		this.shockSpots.Add(shockSpot);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePlayerIntersection()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		if (Time.time > this.playerShockTimer + 0.25f)
		{
			this.playerShockTimer = Time.time;
			this.PlayShockAtPosition(primaryPlayer.GetPosition());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RayCastHeightAt(ref Vector3 position)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(position + Vector3.up - Origin.position, Vector3.down), out raycastHit, 1.75f, 65537) && raycastHit.point.y > position.y)
		{
			position.y = raycastHit.point.y + 0.01f - Origin.position.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateForces()
	{
		if (this.points.Count < 3)
		{
			return;
		}
		float magnitude = (this.points[this.points.Count - 1] - this.points[0]).magnitude;
		if (magnitude == 0f)
		{
			return;
		}
		float num = magnitude / 10000f;
		num = Mathf.Clamp(num, 1E-05f, 10f);
		float d = (this.weightMod > 0f) ? (num * this.weightMod) : num;
		Vector3 a = Vector3.down * 9.81f * d;
		float num2 = 0.35f * ((this.weightMod > 0f) ? (1f - this.weightMod * num) : (1f - num)) * this.springMod;
		num2 = Mathf.Clamp(num2, 0.001f, 0.4999f);
		for (int i = 1; i < this.points.Count - 1; i++)
		{
			Vector3 a2 = this.points[i - 1] - this.points[i];
			a2 *= this.tensionMultiplier;
			a2 *= Mathf.Clamp01(a2.magnitude - this.minSegmentLength);
			Vector3 vector = this.points[i + 1] - this.points[i];
			vector *= this.tensionMultiplier;
			vector *= Mathf.Clamp01(vector.magnitude - this.minSegmentLength);
			Vector3 a3 = a2 + vector;
			float num3 = (i >= this.points.Count - 4) ? Mathf.Clamp01(1.1f - (float)(i - (this.points.Count - 5)) / 3f) : 1f;
			float d2 = num2 * num3;
			Vector3 b = a * (1f + (1f - num3));
			this.forces[i] = this.forces[i] * this.drag + (a3 + b) * d2;
		}
		this.currentTotalForces = 0f;
		for (int j = 1; j < this.points.Count - 1; j++)
		{
			RaycastHit raycastHit;
			if (this.forces[j].magnitude > 0f && this.forces[j].magnitude < this.snagThreshold && Physics.Raycast(new Ray(this.points[j] - Origin.position, this.forces[j].normalized), out raycastHit, this.forces[j].magnitude, 65537) && raycastHit.distance < this.forces[j].magnitude)
			{
				this.forces[j] = Vector3.zero;
			}
			List<Vector3> list = this.points;
			int index = j;
			list[index] += this.forces[j];
			this.currentTotalForces += this.forces[j].magnitude;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreatePoints()
	{
		Vector3 a = this.currSourcePosition - this.currLocalPosition;
		if (a.magnitude == 0f)
		{
			return;
		}
		Vector3 b = a / (float)(1 + this.numNodes);
		if (b.magnitude == 0f)
		{
			return;
		}
		Vector3 vector = this.currLocalPosition + b;
		if (this.points.Count == 0)
		{
			Dictionary<Vector3, bool> dictionary = new Dictionary<Vector3, bool>(Vector3EqualityComparer.Instance);
			dictionary.Add(this.currLocalPosition, true);
			this.points.Add(this.currLocalPosition);
			this.forces.Add(Vector3.zero);
			this.shockNodes.Add(false);
			for (int i = 0; i < this.numNodes; i++)
			{
				this.forces.Add(Vector3.zero);
				this.shockNodes.Add(false);
				if (dictionary.ContainsKey(vector))
				{
					this.points.Clear();
					this.forces.Clear();
					this.shockNodes.Clear();
					return;
				}
				this.points.Add(vector);
				dictionary.Add(vector, true);
				vector += b;
			}
		}
		if (this.points.Count > 1)
		{
			this.points[0] = this.currLocalPosition;
			this.points[this.points.Count - 1] = this.currSourcePosition;
		}
	}

	public void BuildMesh()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		this.usingSourcePosition = (this.Source == null);
		if (!this.usingLocalPosition && this.parentGO == null)
		{
			this.usingLocalPosition = true;
			return;
		}
		Vector3 b = this.currLocalPosition;
		Vector3 a = this.currSourcePosition;
		if (this.prevWireColor != this.wireColor)
		{
			this.prevWireColor = this.wireColor;
			this.personalMaterial.SetColor("_WireColor", this.wireColor);
		}
		for (int i = this.shockSpots.Count - 1; i >= 0; i--)
		{
			if (Time.time > this.shockSpots[i].timer + this.shocks[this.shockSpots[i].shockDataIndex].x)
			{
				WireNode.ShockSpot shockSpot = this.shockSpots[i];
				shockSpot.timer = Time.time;
				shockSpot.shockDataIndex++;
				if (shockSpot.shockDataIndex >= this.shocks.Length)
				{
					UnityEngine.Object.Destroy(shockSpot.shockPrefab);
					this.shockSpots.RemoveAt(i);
					this.shockNodes[i] = false;
				}
				else
				{
					this.shockSpots[i] = shockSpot;
				}
			}
		}
		this.currLocalPosition = this.localOffset + (this.usingLocalPosition ? this.LocalPosition : this.parent.position);
		Vector3 a2 = this.sourceOffset;
		if (!this.usingSourcePosition)
		{
			a2 = this.sourceOffset.x * Camera.main.transform.right;
			a2 += this.sourceOffset.y * Camera.main.transform.up;
			a2 += this.sourceOffset.z * Camera.main.transform.forward;
		}
		this.currSourcePosition = a2 + (this.usingSourcePosition ? this.SourcePosition : this.cachedSourcePosition);
		if (this.currSourcePosition == this.currLocalPosition)
		{
			return;
		}
		this.prevLocalPosition = this.currLocalPosition;
		this.prevSourcePosition = this.currSourcePosition;
		this.prevWeightMod = this.weightMod;
		this.prevSpringMod = this.springMod;
		this.prevSize = this.size;
		this.mesh.Clear(false);
		this.verts.Clear();
		this.uvs.Clear();
		this.uvs2.Clear();
		this.normals.Clear();
		this.CreatePoints();
		this.UpdateForces();
		this.uvs.Add(Vector2.zero);
		this.uvs.Add(Vector2.zero);
		this.uvs.Add(Vector2.zero);
		this.uvs.Add(Vector2.zero);
		this.uvs2.Add(Vector2.zero);
		this.uvs2.Add(Vector2.zero);
		this.uvs2.Add(Vector2.zero);
		this.uvs2.Add(Vector2.zero);
		for (int j = 1; j < this.points.Count; j++)
		{
			this.uvs.Add(Vector2.right * (float)j / (float)this.points.Count);
			this.uvs.Add(Vector2.right * (float)j / (float)this.points.Count + Vector2.up);
			this.uvs.Add(Vector2.right * (float)j / (float)this.points.Count);
			this.uvs.Add(Vector2.right * (float)j / (float)this.points.Count + Vector2.up);
			bool flag = false;
			for (int k = 0; k < this.shockSpots.Count; k++)
			{
				if (this.shockSpots[k].vertex == j)
				{
					this.uvs2.Add(Vector2.right * this.shocks[this.shockSpots[k].shockDataIndex].y);
					this.uvs2.Add(Vector2.right * this.shocks[this.shockSpots[k].shockDataIndex].y);
					this.uvs2.Add(Vector2.right * this.shocks[this.shockSpots[k].shockDataIndex].y);
					this.uvs2.Add(Vector2.right * this.shocks[this.shockSpots[k].shockDataIndex].y);
					flag = true;
					this.shockNodes[j] = true;
					break;
				}
			}
			if (!flag)
			{
				this.uvs2.Add(Vector2.zero);
				this.uvs2.Add(Vector2.zero);
				this.uvs2.Add(Vector2.zero);
				this.uvs2.Add(Vector2.zero);
			}
		}
		Vector3 vector = a - b;
		this.min = Vector3.one * float.PositiveInfinity;
		this.max = -Vector3.one * float.PositiveInfinity;
		for (int l = 0; l < this.points.Count; l++)
		{
			if (l > 0)
			{
				vector = this.points[l] - this.points[l - 1];
			}
			Vector3 normalized = Vector3.Cross(Vector3.up, vector.normalized).normalized;
			Vector3 normalized2 = Vector3.Cross(vector.normalized, normalized).normalized;
			this.verts.Add(normalized2 * this.size + this.points[l]);
			this.verts.Add(-normalized2 * this.size + this.points[l]);
			this.verts.Add(normalized * this.size + this.points[l]);
			this.verts.Add(-normalized * this.size + this.points[l]);
			if (l == 0)
			{
				normalized2 = Vector3.Lerp(normalized2, (a - b).normalized, 0.5f).normalized;
				normalized = Vector3.Lerp(normalized, (a - b).normalized, 0.5f).normalized;
			}
			else if (l == this.points.Count - 1)
			{
				normalized2 = Vector3.Lerp(normalized2, -(a - b).normalized, 0.5f).normalized;
				normalized = Vector3.Lerp(normalized, -(a - b).normalized, 0.5f).normalized;
			}
			this.normals.Add(normalized2);
			this.normals.Add(-normalized2);
			this.normals.Add(normalized);
			this.normals.Add(-normalized);
			if (this.points[l].x < this.min.x)
			{
				this.min.x = this.points[l].x;
			}
			if (this.points[l].x > this.max.x)
			{
				this.max.x = this.points[l].x;
			}
			if (this.points[l].y < this.min.y)
			{
				this.min.y = this.points[l].y;
			}
			if (this.points[l].y > this.max.y)
			{
				this.max.y = this.points[l].y;
			}
			if (this.points[l].z < this.min.z)
			{
				this.min.z = this.points[l].z;
			}
			if (this.points[l].z > this.max.z)
			{
				this.max.z = this.points[l].z;
			}
		}
		if (((this.min.x == this.max.x) ? 1 : ((0f + this.min.y == this.max.y) ? 1 : ((0f + this.min.z == this.max.z) ? 1 : 0))) >= 2)
		{
			return;
		}
		int num = 0;
		for (int m = 0; m < this.numNodes; m++)
		{
			this.indices[num++] = 4 * m;
			this.indices[num++] = 4 + 4 * m;
			this.indices[num++] = 7 + 4 * m;
			this.indices[num++] = 7 + 4 * m;
			this.indices[num++] = 3 + 4 * m;
			this.indices[num++] = 4 * m;
			this.indices[num++] = 4 + 4 * m;
			this.indices[num++] = 4 * m;
			this.indices[num++] = 2 + 4 * m;
			this.indices[num++] = 2 + 4 * m;
			this.indices[num++] = 6 + 4 * m;
			this.indices[num++] = 4 + 4 * m;
			this.indices[num++] = 3 + 4 * m;
			this.indices[num++] = 7 + 4 * m;
			this.indices[num++] = 5 + 4 * m;
			this.indices[num++] = 5 + 4 * m;
			this.indices[num++] = 1 + 4 * m;
			this.indices[num++] = 3 + 4 * m;
			this.indices[num++] = 6 + 4 * m;
			this.indices[num++] = 2 + 4 * m;
			this.indices[num++] = 1 + 4 * m;
			this.indices[num++] = 1 + 4 * m;
			this.indices[num++] = 5 + 4 * m;
			this.indices[num++] = 6 + 4 * m;
		}
		this.indices[num++] = 0;
		this.indices[num++] = 3;
		this.indices[num++] = 1;
		this.indices[num++] = 1;
		this.indices[num++] = 2;
		this.indices[num++] = 0;
		this.indices[num++] = 4 + 4 * (this.numNodes - 1);
		this.indices[num++] = 6 + 4 * (this.numNodes - 1);
		this.indices[num++] = 5 + 4 * (this.numNodes - 1);
		this.indices[num++] = 5 + 4 * (this.numNodes - 1);
		this.indices[num++] = 7 + 4 * (this.numNodes - 1);
		this.indices[num++] = 4 + 4 * (this.numNodes - 1);
		if (this.verts.Count < 3)
		{
			return;
		}
		if (this.uvs.Count < 3)
		{
			return;
		}
		if (this.uvs2.Count < 3)
		{
			return;
		}
		if (this.normals.Count < 3)
		{
			return;
		}
		if (this.indices.Length < 3)
		{
			return;
		}
		this.mesh.SetVertices(this.verts);
		this.mesh.uv = this.uvs.ToArray();
		this.mesh.uv2 = this.uvs2.ToArray();
		this.mesh.SetNormals(this.normals);
		this.mesh.SetIndices(this.indices, MeshTopology.Triangles, 0);
		this.meshFilter.mesh = this.mesh;
	}

	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MyUpdate()
	{
		if (this.Source != null)
		{
			if (this.cachedSourcePosition != this.Source.transform.position)
			{
				this.cachedSourcePosition = this.Source.transform.position;
				this.BuildMesh();
				return;
			}
		}
		else
		{
			this.BuildMesh();
		}
	}

	public void FixedUpdate()
	{
		this.MyUpdate();
	}

	public void LateUpdate()
	{
		this.MyUpdate();
	}

	public Vector3 GetStartPosition()
	{
		return this.SourcePosition;
	}

	public Vector3 GetStartPositionOffset()
	{
		return this.sourceOffset;
	}

	public void SetStartPosition(Vector3 pos)
	{
		this.SourcePosition = pos;
	}

	public void SetStartPositionOffset(Vector3 pos)
	{
		this.sourceOffset = pos;
	}

	public Vector3 GetEndPosition()
	{
		return this.LocalPosition;
	}

	public Vector3 GetEndPositionOffset()
	{
		return this.localOffset;
	}

	public void SetEndPosition(Vector3 pos)
	{
		this.LocalPosition = pos;
	}

	public void SetEndPositionOffset(Vector3 pos)
	{
		this.localOffset = pos;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public Bounds GetBounds()
	{
		return this.mesh.bounds;
	}

	public void SetWireDip(float _dist)
	{
	}

	public float GetWireDip()
	{
		return 0f;
	}

	public void SetWireRadius(float _radius)
	{
	}

	public void Reset()
	{
		this.pulseColor = new Color32(0, 97, byte.MaxValue, byte.MaxValue);
	}

	public void SetWireCanHide(bool _canHide)
	{
	}

	public void SetVisible(bool _visible)
	{
	}

	public const int cLayerMaskRayCast = 65537;

	public Vector2[] shocks;

	public Material material;

	public GameObject Source;

	public GameObject parentGO;

	public Color pulseColor = new Color32(0, 97, byte.MaxValue, byte.MaxValue);

	public Color wireColor = Color.black;

	public float size;

	public float weightMod;

	public float springMod;

	public float minSegmentLength = 0.25f;

	public float snagThreshold = 0.5f;

	public float tensionMultiplier = 2f;

	public float playerHeight = 1.8f;

	public Vector3 SourcePosition = Vector3.zero;

	public Vector3 LocalPosition = Vector3.one;

	public Vector3 localOffset = Vector3.zero;

	public Vector3 sourceOffset = Vector3.zero;

	public Vector3 cameraOffset;

	public bool attatchSoureToCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float playerShockTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentTotalForces = 1000f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 prevSourcePos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentSegmentLength;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameObject rootShockPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] shockTimers = new float[8];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] shockIndices = new int[8];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] currentShockingVertex = new int[8];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<WireNode.ShockSpot> shockSpots;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 min = Vector3.one * float.PositiveInfinity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 max = -Vector3.one * float.PositiveInfinity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 prevSourcePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 prevLocalPosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject prevParentGO;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float prevSize = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material personalMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numNodes = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color prevWireColor = Color.black;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float prevWeightMod = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float prevSpringMod = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform parent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform prevParent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Mesh mesh;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshFilter meshFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshRenderer meshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshCollider meshCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> points;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> forces;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> verts;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector2> uvs;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector2> uvs2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> normals;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<bool> shockNodes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] indices;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool usingLocalPosition = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool usingSourcePosition = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float checkPlayerConnectTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attachedNode = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int prevAttachedNode = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float slopeSpeed = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 currLocalPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 currSourcePosition;

	public float drag = 0.89f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 cachedSourcePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ShockSpot
	{
		public float timer;

		public int shockDataIndex;

		public int vertex;

		public GameObject shockPrefab;
	}
}
