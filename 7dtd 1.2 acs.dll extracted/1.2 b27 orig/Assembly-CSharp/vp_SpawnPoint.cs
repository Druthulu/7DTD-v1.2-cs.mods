using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class vp_SpawnPoint : MonoBehaviour
{
	public static List<vp_SpawnPoint> SpawnPoints
	{
		get
		{
			if (vp_SpawnPoint.m_SpawnPoints == null)
			{
				vp_SpawnPoint.m_SpawnPoints = new List<vp_SpawnPoint>(UnityEngine.Object.FindObjectsOfType(typeof(vp_SpawnPoint)) as vp_SpawnPoint[]);
			}
			return vp_SpawnPoint.m_SpawnPoints;
		}
	}

	public static vp_Placement GetRandomPlacement()
	{
		return vp_SpawnPoint.GetRandomPlacement(0f, null);
	}

	public static vp_Placement GetRandomPlacement(float physicsCheckRadius)
	{
		return vp_SpawnPoint.GetRandomPlacement(physicsCheckRadius, null);
	}

	public static vp_Placement GetRandomPlacement(string tag)
	{
		return vp_SpawnPoint.GetRandomPlacement(0f, tag);
	}

	public static vp_Placement GetRandomPlacement(float physicsCheckRadius, string tag)
	{
		if (vp_SpawnPoint.SpawnPoints == null || vp_SpawnPoint.SpawnPoints.Count < 1)
		{
			return null;
		}
		vp_SpawnPoint randomSpawnPoint;
		if (string.IsNullOrEmpty(tag))
		{
			randomSpawnPoint = vp_SpawnPoint.GetRandomSpawnPoint();
		}
		else
		{
			randomSpawnPoint = vp_SpawnPoint.GetRandomSpawnPoint(tag);
			if (randomSpawnPoint == null)
			{
				randomSpawnPoint = vp_SpawnPoint.GetRandomSpawnPoint();
				Debug.LogWarning("Warning (vp_SpawnPoint --> GetRandomPlacement) Could not find a spawnpoint tagged '" + tag + "'. Falling back to 'any random spawnpoint'.");
			}
		}
		if (randomSpawnPoint == null)
		{
			Debug.LogError("Error (vp_SpawnPoint --> GetRandomPlacement) Could not find a spawnpoint" + ((!string.IsNullOrEmpty(tag)) ? (" tagged '" + tag + "'") : ".") + " Reverting to world origin.");
			return null;
		}
		vp_Placement vp_Placement = new vp_Placement();
		vp_Placement.Position = randomSpawnPoint.transform.position;
		if (randomSpawnPoint.Radius > 0f)
		{
			Vector3 vector = UnityEngine.Random.insideUnitSphere * randomSpawnPoint.Radius;
			vp_Placement vp_Placement2 = vp_Placement;
			vp_Placement2.Position.x = vp_Placement2.Position.x + vector.x;
			vp_Placement vp_Placement3 = vp_Placement;
			vp_Placement3.Position.z = vp_Placement3.Position.z + vector.z;
		}
		if (physicsCheckRadius != 0f)
		{
			if (!vp_Placement.AdjustPosition(vp_Placement, physicsCheckRadius, 1000))
			{
				return null;
			}
			vp_Placement.SnapToGround(vp_Placement, physicsCheckRadius, randomSpawnPoint.GroundSnapThreshold);
		}
		if (randomSpawnPoint.RandomDirection)
		{
			vp_Placement.Rotation = Quaternion.Euler(Vector3.up * UnityEngine.Random.Range(0f, 360f));
		}
		else
		{
			vp_Placement.Rotation = randomSpawnPoint.transform.rotation;
		}
		return vp_Placement;
	}

	public static vp_SpawnPoint GetRandomSpawnPoint()
	{
		if (vp_SpawnPoint.SpawnPoints.Count < 1)
		{
			return null;
		}
		return vp_SpawnPoint.SpawnPoints[UnityEngine.Random.Range(0, vp_SpawnPoint.SpawnPoints.Count)];
	}

	public static vp_SpawnPoint GetRandomSpawnPoint(string tag)
	{
		vp_SpawnPoint.m_MatchingSpawnPoints.Clear();
		for (int i = 0; i < vp_SpawnPoint.SpawnPoints.Count; i++)
		{
			if (vp_SpawnPoint.m_SpawnPoints[i].tag == tag)
			{
				vp_SpawnPoint.m_MatchingSpawnPoints.Add(vp_SpawnPoint.m_SpawnPoints[i]);
			}
		}
		if (vp_SpawnPoint.m_MatchingSpawnPoints.Count < 1)
		{
			return null;
		}
		if (vp_SpawnPoint.m_MatchingSpawnPoints.Count == 1)
		{
			return vp_SpawnPoint.m_MatchingSpawnPoints[0];
		}
		return vp_SpawnPoint.m_MatchingSpawnPoints[UnityEngine.Random.Range(0, vp_SpawnPoint.m_MatchingSpawnPoints.Count)];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		SceneManager.sceneLoaded += this.NotifyLevelWasLoaded;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= this.NotifyLevelWasLoaded;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		vp_SpawnPoint.m_SpawnPoints = null;
	}

	public bool RandomDirection;

	public float Radius;

	public float GroundSnapThreshold = 2.5f;

	public bool LockGroundSnapToRadius = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static List<vp_SpawnPoint> m_MatchingSpawnPoints = new List<vp_SpawnPoint>(50);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static List<vp_SpawnPoint> m_SpawnPoints = null;
}
