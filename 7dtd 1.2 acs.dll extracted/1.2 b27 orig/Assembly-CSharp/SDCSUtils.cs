using System;
using System.Collections.Generic;
using System.Linq;
using ShinyScreenSpaceRaytracedReflections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public static class SDCSUtils
{
	public static void Stitch(GameObject sourceObj, GameObject parentObj, SDCSUtils.TransformCatalog boneCatalog, EModelSDCS emodel = null, bool isFPV = false, float cullDist = 0f, bool isUI = false, Material eyeMat = null, bool isGear = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(sourceObj, parentObj.transform);
		gameObject.name = sourceObj.name;
		gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(SDCSUtils.tempSMRs);
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in SDCSUtils.tempSMRs)
		{
			GameObject gameObject2 = skinnedMeshRenderer.gameObject;
			string name = gameObject2.name;
			skinnedMeshRenderer.bones = SDCSUtils.TranslateTransforms(skinnedMeshRenderer.bones, boneCatalog);
			skinnedMeshRenderer.rootBone = SDCSUtils.Find<string, Transform>(boneCatalog, skinnedMeshRenderer.rootBone.name);
			skinnedMeshRenderer.updateWhenOffscreen = true;
			Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material material = sharedMaterials[i];
				if (material)
				{
					string name2 = material.name;
					if (name2.Contains("_Body"))
					{
						Material material2 = DataLoader.LoadAsset<Material>(SDCSUtils.baseBodyMatLoc);
						sharedMaterials[i] = (material2 ? material2 : sharedMaterials[i]);
					}
					else if (name2.Contains("_Head"))
					{
						Material material3 = DataLoader.LoadAsset<Material>(SDCSUtils.baseHeadMatLoc);
						sharedMaterials[i] = (material3 ? material3 : sharedMaterials[i]);
					}
					else if (name2.Contains("_Hand"))
					{
						Material material4 = DataLoader.LoadAsset<Material>(SDCSUtils.baseHandsMatLoc);
						sharedMaterials[i] = (material4 ? material4 : sharedMaterials[i]);
					}
				}
			}
			if (name == "eyes" && eyeMat)
			{
				sharedMaterials[0] = eyeMat;
			}
			skinnedMeshRenderer.sharedMaterials = sharedMaterials;
			Material material5 = sharedMaterials[0];
			string text = material5 ? material5.shader.name : "";
			if (text.Equals("Game/SDCS/Skin") || text.Equals("Game/SDCS/Hair"))
			{
				gameObject2.AddComponent<ExcludeReflections>().enabled = !isFPV;
			}
			if (text.Equals("Game/Character") || text.Equals("Game/CharacterPlayerSkin") || text.Equals("Game/CharacterPlayerOutfit") || text.Equals("Game/CharacterCloth"))
			{
				Material material6 = skinnedMeshRenderer.material;
				if (name == "hands" || name == "gloves")
				{
					material6.SetFloat("_FirstPerson", 0f);
					material6.SetFloat("_ClipRadius", 0f);
				}
				else if (!isUI)
				{
					if (emodel && isFPV)
					{
						emodel.ClipMaterialsFP.Add(material6);
					}
					material6.SetFloat("_FirstPerson", (float)((emodel && isFPV) ? 1 : 0));
					material6.SetFloat("_ClipRadius", cullDist);
				}
				else
				{
					material6.SetFloat("_FirstPerson", 0f);
					material6.SetFloat("_ClipRadius", 0f);
				}
				material6.SetVector("_ClipCenter", boneCatalog["Head"].position);
				if (!isUI && emodel && isFPV && isGear)
				{
					SDCSUtils.RemoveFPViewObstructingGearPolygons(skinnedMeshRenderer);
				}
			}
		}
		SDCSUtils.tempSMRs.Clear();
		Transform transform = boneCatalog["Hips"];
		gameObject.GetComponentsInChildren<Cloth>(SDCSUtils.tempCloths);
		foreach (Cloth cloth in SDCSUtils.tempCloths)
		{
			cloth.capsuleColliders = transform.GetComponentsInChildren<CapsuleCollider>();
		}
		SDCSUtils.tempCloths.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RemoveFPViewObstructingGearPolygons(SkinnedMeshRenderer smr)
	{
		if (smr && smr.sharedMesh)
		{
			Mesh mesh = UnityEngine.Object.Instantiate<Mesh>(smr.sharedMesh);
			smr.sharedMesh = mesh;
			Color[] colors = mesh.colors;
			if (colors.Length != 0)
			{
				int[] triangles = mesh.triangles;
				int num = triangles.Length / 3;
				int vertexCount = mesh.vertexCount;
				int[] array = new int[num * 3];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					int num3 = triangles[i * 3];
					int num4 = triangles[i * 3 + 1];
					int num5 = triangles[i * 3 + 2];
					if (colors[num3].r == 0f && colors[num4].r == 0f && colors[num5].r == 0f)
					{
						array[num2 * 3] = num3;
						array[num2 * 3 + 1] = num4;
						array[num2 * 3 + 2] = num5;
						num2++;
					}
				}
				Array.Resize<int>(ref array, num2 * 3);
				mesh.triangles = array;
				if (num > num2)
				{
					Debug.Log("SDCSUtils::RemoveFPViewObstructingGearPolygons -> Removed " + (num - num2).ToString() + " obstructing polygons from " + mesh.name);
					return;
				}
				Debug.Log("SDCSUtils::RemoveFPViewObstructingGearPolygons -> " + mesh.name + " has no obstructing polygons");
			}
		}
	}

	public static void MatchRigs(SDCSUtils.SlotData wornItem, Transform source, Transform target, SDCSUtils.TransformCatalog transformCatalog)
	{
		Transform transform = source.Find("Origin");
		Transform transform2 = target.Find("Origin");
		if (!transform || !transform2)
		{
			return;
		}
		SDCSUtils.AddMissingChildren(wornItem, transform, transform2, transformCatalog);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddMissingChildren(SDCSUtils.SlotData wornItem, Transform sourceT, Transform targetT, SDCSUtils.TransformCatalog transformCatalog)
	{
		if (!sourceT || !targetT)
		{
			return;
		}
		foreach (object obj in sourceT)
		{
			Transform transform = (Transform)obj;
			string name = transform.name;
			Transform transform2 = null;
			bool flag = false;
			foreach (object obj2 in targetT)
			{
				Transform transform3 = (Transform)obj2;
				string name2 = transform3.name;
				if (name2 == name)
				{
					transform2 = transform3;
					flag = true;
					if (!transformCatalog.ContainsKey(name2))
					{
						transformCatalog.Add(name2, transform2);
						break;
					}
					break;
				}
			}
			if (!flag)
			{
				transform2 = UnityEngine.Object.Instantiate<GameObject>(transform.gameObject).transform;
				transform2.SetParent(targetT, false);
				transform2.name = name;
				transformCatalog[name] = transform2;
			}
			if (!flag)
			{
				transform2.SetLocalPositionAndRotation(transform.localPosition, transform.localRotation);
				transform2.localScale = transform.localScale;
			}
			SDCSUtils.TransferCharacterJoint(transform, transform2.gameObject, transformCatalog);
			SDCSUtils.AddMissingChildren(wornItem, transform, transform2, transformCatalog);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupRigConstraints(RigBuilder rigBuilder, Transform sourceRootT, Transform targetRootT, SDCSUtils.TransformCatalog transformCatalog)
	{
		if (!sourceRootT.GetComponent<RigBuilder>())
		{
			return;
		}
		Transform transform = sourceRootT.Find("RigConstraints");
		if (!transform)
		{
			return;
		}
		string text = transform.name + "_" + transform.parent.name;
		Transform transform2 = targetRootT.Find(text);
		if (!transform2)
		{
			transform2 = UnityEngine.Object.Instantiate<Transform>(transform, targetRootT);
			transform2.name = text;
			transform2.SetLocalPositionAndRotation(transform.localPosition, transform.localRotation);
			transform2.localScale = transform.localScale;
		}
		Rig component = transform2.GetComponent<Rig>();
		if (!component)
		{
			return;
		}
		rigBuilder.layers.Add(new RigLayer(component, true));
		BlendConstraint[] componentsInChildren = transform.GetComponentsInChildren<BlendConstraint>();
		foreach (BlendConstraint blendConstraint in transform2.GetComponentsInChildren<BlendConstraint>())
		{
			string name = blendConstraint.name;
			foreach (BlendConstraint blendConstraint2 in componentsInChildren)
			{
				if (blendConstraint2.name == name)
				{
					blendConstraint.data.constrainedObject = SDCSUtils.Find<string, Transform>(transformCatalog, blendConstraint2.data.constrainedObject.name);
					blendConstraint.data.sourceObjectA = SDCSUtils.Find<string, Transform>(transformCatalog, blendConstraint2.data.sourceObjectA.name);
					blendConstraint.data.sourceObjectB = SDCSUtils.Find<string, Transform>(transformCatalog, blendConstraint2.data.sourceObjectB.name);
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TransferCharacterJoint(Transform source, GameObject newBone, SDCSUtils.TransformCatalog transformCatalog)
	{
		CharacterJoint component;
		CharacterJoint characterJoint;
		if ((component = source.GetComponent<CharacterJoint>()) != null && (characterJoint = newBone.AddMissingComponent<CharacterJoint>()) != null)
		{
			Joint joint = characterJoint;
			Transform transform = SDCSUtils.Find<string, Transform>(transformCatalog, component.connectedBody.name);
			joint.connectedBody = ((transform != null) ? transform.GetComponent<Rigidbody>() : null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject AddChild(GameObject source, Transform parent)
	{
		source.transform.parent = parent;
		foreach (object obj in source.transform)
		{
			UnityEngine.Object.Destroy(((Transform)obj).gameObject);
		}
		return source;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SkinnedMeshRenderer AddSkinnedMeshRenderer(SkinnedMeshRenderer source, GameObject parent)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = new GameObject(source.name)
		{
			transform = 
			{
				parent = parent.transform
			}
		}.AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer.sharedMesh = source.sharedMesh;
		skinnedMeshRenderer.sharedMaterials = source.sharedMaterials;
		return skinnedMeshRenderer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform[] TranslateTransforms(Transform[] transforms, SDCSUtils.TransformCatalog transformCatalog)
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			Transform transform = transforms[i];
			if (transform)
			{
				transforms[i] = SDCSUtils.Find<string, Transform>(transformCatalog, transform.name);
			}
			else
			{
				Log.Error("Null transform in bone list");
			}
		}
		return transforms;
	}

	public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
	{
		TValue result;
		source.TryGetValue(key, out result);
		return result;
	}

	public static string baseBodyLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Entities/Player/" + SDCSUtils.tmpArchetype.Sex + "/Body/Meshes/player" + SDCSUtils.tmpArchetype.Sex;
		}
	}

	public static string baseHeadLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/Heads/Meshes/player",
				SDCSUtils.tmpArchetype.Sex,
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00")
			});
		}
	}

	public static string baseHairLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/HairMorphMatrix/Hair/",
				SDCSUtils.tmpArchetype.Hair,
				"/",
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00")
			});
		}
	}

	public static string baseMustacheLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/HairMorphMatrix/Mustache/",
				SDCSUtils.tmpArchetype.MustacheName,
				"/",
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00")
			});
		}
	}

	public static string baseChopsLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/HairMorphMatrix/Chops/",
				SDCSUtils.tmpArchetype.ChopsName,
				"/",
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00")
			});
		}
	}

	public static string baseBeardLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/HairMorphMatrix/Beard/",
				SDCSUtils.tmpArchetype.BeardName,
				"/",
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00")
			});
		}
	}

	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Entities/Player/HairColorSwatches";
		}
	}

	public static string baseEyeColorMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Entities/Player/Eyes/" + SDCSUtils.tmpArchetype.EyeColorName;
		}
	}

	public static string baseBodyMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/Body/Materials/player",
				SDCSUtils.tmpArchetype.Sex,
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00"),
				"_Body"
			});
		}
	}

	public static string baseHeadMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/Heads/Materials/player",
				SDCSUtils.tmpArchetype.Sex,
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00"),
				"_Head"
			});
		}
	}

	public static string baseHandsMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/Body/Materials/player",
				SDCSUtils.tmpArchetype.Sex,
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00"),
				"_Hand"
			});
		}
	}

	public static string baseRigPrefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Entities/Player/BaseRigs/baseRigPrefab";
		}
	}

	public static string baseRigFPPrefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Entities/Player/BaseRigs/baseRigFPPrefab";
		}
	}

	public static RuntimeAnimatorController UIAnimController
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DataLoader.LoadAsset<RuntimeAnimatorController>("Entities/Player/AnimControllers/MenuSDCSController");
		}
	}

	public static RuntimeAnimatorController TPAnimController
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DataLoader.LoadAsset<RuntimeAnimatorController>("Entities/Player/AnimControllers/3PPlayer" + SDCSUtils.tmpArchetype.Sex + "Controller");
		}
	}

	public static RuntimeAnimatorController FPAnimController
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DataLoader.LoadAsset<RuntimeAnimatorController>("Entities/Player/AnimControllers/FPPlayerController");
		}
	}

	public static void CreateVizTP(Archetype _archetype, ref GameObject baseRig, ref SDCSUtils.TransformCatalog boneCatalog, EntityAlive entity, bool isFPV)
	{
		SDCSUtils.DestroyViz(baseRig, true);
		SDCSUtils.tmpArchetype = _archetype;
		SDCSUtils.setupRig(ref baseRig, ref boneCatalog, SDCSUtils.baseRigPrefab, null, SDCSUtils.TPAnimController);
		if (!isFPV)
		{
			SDCSUtils.setupBase(baseRig, boneCatalog, SDCSUtils.baseParts, isFPV);
			SDCSUtils.setupEquipment(baseRig, boneCatalog, SDCSUtils.ignoredParts, entity, false);
			SDCSUtils.setupHairObjects(baseRig, boneCatalog, SDCSUtils.ignoredParts, entity, false);
		}
	}

	public static void CreateVizFP(Archetype _archetype, ref GameObject baseRigFP, ref SDCSUtils.TransformCatalog boneCatalogFP, EntityAlive entity, bool isFPV)
	{
		SDCSUtils.DestroyViz(baseRigFP, true);
		SDCSUtils.tmpArchetype = _archetype;
		Transform transform = entity.transform.FindInChildren("Camera");
		if (transform == null)
		{
			if (!(GameObject.Find("Camera") != null))
			{
				Log.Error("Unable to find first person camera!");
				return;
			}
			transform = GameObject.Find("Camera").transform;
		}
		Transform transform2 = transform.FindInChildren("Pivot");
		if (transform2 != null)
		{
			transform = transform2.parent;
		}
		SDCSUtils.setupRig(ref baseRigFP, ref boneCatalogFP, SDCSUtils.baseRigFPPrefab, transform, SDCSUtils.FPAnimController);
		SDCSUtils.setupBase(baseRigFP, boneCatalogFP, SDCSUtils.basePartsFP, isFPV);
		SDCSUtils.setupEquipment(baseRigFP, boneCatalogFP, SDCSUtils.ignoredPartsFP, entity, false);
		SDCSUtils.setupHairObjects(baseRigFP, boneCatalogFP, SDCSUtils.ignoredPartsFP, entity, false);
		Transform transform3 = baseRigFP.transform;
		transform3.SetParent(transform, false);
		transform3.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		baseRigFP.name = "baseRigFP";
		baseRigFP.AddMissingComponent<AnimationEventBridge>();
		SkinnedMeshRenderer[] componentsInChildren = baseRigFP.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("HoldingItem");
		}
		foreach (HingeJoint hingeJoint in baseRigFP.GetComponentsInChildren<HingeJoint>())
		{
			if (hingeJoint.connectedBody == null)
			{
				Log.Warning("SDCSUtils::CreateVizFP: No connected body for " + hingeJoint.transform.name + "'s HingeJoint! Disabling for FP as it is never seen.");
				hingeJoint.gameObject.SetActive(false);
			}
		}
		baseRigFP.GetComponentsInChildren<Cloth>(SDCSUtils.tempCloths);
		foreach (Cloth cloth in SDCSUtils.tempCloths)
		{
			cloth.enabled = false;
		}
		SDCSUtils.tempCloths.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupBodyColliders(GameObject baseRig)
	{
	}

	public static void CreateVizUI(Archetype _archetype, ref GameObject baseRigUI, ref SDCSUtils.TransformCatalog boneCatalogUI, EntityAlive entity)
	{
		SDCSUtils.DestroyViz(baseRigUI, true);
		SDCSUtils.tmpArchetype = _archetype;
		SDCSUtils.setupRig(ref baseRigUI, ref boneCatalogUI, SDCSUtils.baseRigPrefab, null, SDCSUtils.UIAnimController);
		SDCSUtils.SetupBodyColliders(baseRigUI);
		SDCSUtils.setupBase(baseRigUI, boneCatalogUI, SDCSUtils.baseParts, false);
		SDCSUtils.setupEquipment(baseRigUI, boneCatalogUI, SDCSUtils.ignoredParts, entity, true);
		SDCSUtils.setupHairObjects(baseRigUI, boneCatalogUI, SDCSUtils.ignoredParts, entity, true);
		Transform transform = baseRigUI.transform.Find("IKRig");
		if (transform != null)
		{
			transform.GetComponent<Rig>().weight = 0f;
		}
		foreach (HingeJoint hingeJoint in baseRigUI.GetComponentsInChildren<HingeJoint>())
		{
			if (hingeJoint.connectedBody == null)
			{
				Log.Warning("SDCSUtils::CreateVizUI: No connected body for " + hingeJoint.transform.name + "'s HingeJoint! Disabling for UI until this is solved.");
				hingeJoint.gameObject.SetActive(false);
			}
		}
	}

	public static void CreateVizUI(Archetype _archetype, ref GameObject baseRigUI, ref SDCSUtils.TransformCatalog boneCatalogUI)
	{
		SDCSUtils.DestroyViz(baseRigUI, false);
		SDCSUtils.tmpArchetype = _archetype;
		SDCSUtils.setupRig(ref baseRigUI, ref boneCatalogUI, SDCSUtils.baseRigPrefab, null, SDCSUtils.UIAnimController);
		SDCSUtils.setupBase(baseRigUI, boneCatalogUI, SDCSUtils.baseParts, false);
		SDCSUtils.setupHairObjects(baseRigUI, boneCatalogUI, null, false, SDCSUtils.ignoredParts, true, _archetype.Hair, _archetype.MustacheName, _archetype.ChopsName, _archetype.BeardName);
		SDCSUtils.setupEquipment(baseRigUI, boneCatalogUI, SDCSUtils.ignoredParts, true, _archetype.Equipment);
		Transform transform = baseRigUI.transform.Find("IKRig");
		if (transform != null)
		{
			transform.GetComponent<Rig>().weight = 0f;
		}
		foreach (HingeJoint hingeJoint in baseRigUI.GetComponentsInChildren<HingeJoint>())
		{
			if (hingeJoint.connectedBody == null)
			{
				Log.Warning("SDCSUtils::CreateVizUI: No connected body for " + hingeJoint.transform.name + "'s HingeJoint! Disabling for UI until this is solved.");
				hingeJoint.gameObject.SetActive(false);
			}
		}
	}

	public static void DestroyViz(GameObject _baseRigUI, bool _keepRig = false)
	{
		if (_baseRigUI)
		{
			Transform transform = _baseRigUI.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name != "Origin")
				{
					child.GetComponentsInChildren<SkinnedMeshRenderer>(true, SDCSUtils.tempSMRs);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in SDCSUtils.tempSMRs)
					{
						Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
						if (MeshMorph.IsInstance(sharedMesh))
						{
							UnityEngine.Object.Destroy(sharedMesh);
						}
						skinnedMeshRenderer.GetSharedMaterials(SDCSUtils.tempMats);
						Utils.CleanupMaterials<List<Material>>(SDCSUtils.tempMats);
						SDCSUtils.tempMats.Clear();
					}
				}
			}
			if (!_keepRig)
			{
				UnityEngine.Object.DestroyImmediate(_baseRigUI);
			}
		}
	}

	public static void SetVisible(GameObject _baseRigUI, bool _visible)
	{
		if (_baseRigUI)
		{
			Transform transform = _baseRigUI.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name != "Origin")
				{
					child.GetComponentsInChildren<SkinnedMeshRenderer>(true, SDCSUtils.tempSMRs);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in SDCSUtils.tempSMRs)
					{
						skinnedMeshRenderer.gameObject.SetActive(_visible);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupRig(ref GameObject _rigObj, ref SDCSUtils.TransformCatalog _boneCatalog, string prefabLocation, Transform parent, RuntimeAnimatorController animController)
	{
		if (!_rigObj)
		{
			_rigObj = UnityEngine.Object.Instantiate<GameObject>(DataLoader.LoadAsset<GameObject>(prefabLocation), parent);
			_boneCatalog = new SDCSUtils.TransformCatalog(_rigObj.transform);
			Animator component = _rigObj.GetComponent<Animator>();
			if (component && component.runtimeAnimatorController != animController)
			{
				component.runtimeAnimatorController = animController;
			}
			BoneRenderer[] componentsInChildren = _rigObj.GetComponentsInChildren<BoneRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			SDCSUtils.cleanupEquipment(_rigObj);
		}
		if (!SDCSUtils.tmpArchetype.IsMale)
		{
			CapsuleCollider orAddComponent = _boneCatalog["Hips"].gameObject.GetOrAddComponent<CapsuleCollider>();
			orAddComponent.center = new Vector3(0f, 0f, -0.03f);
			orAddComponent.radius = 0.15f;
			orAddComponent.height = 0.375f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void cleanupEquipment(GameObject _rigObj)
	{
		RigBuilder component = _rigObj.GetComponent<RigBuilder>();
		if (component)
		{
			List<RigLayer> layers = component.layers;
			for (int i = layers.Count - 1; i >= 0; i--)
			{
				if (layers[i].name != "IKRig")
				{
					layers.RemoveAt(i);
				}
			}
			component.Clear();
		}
		Animator component2 = _rigObj.GetComponent<Animator>();
		if (component2)
		{
			component2.UnbindAllStreamHandles();
		}
		GameUtils.DestroyAllChildrenBut(_rigObj.transform, new List<string>
		{
			"Origin",
			"IKRig"
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupBase(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] baseParts, bool isFPV)
	{
		foreach (string text in baseParts)
		{
			GameObject gameObject;
			if (text == "head")
			{
				gameObject = DataLoader.LoadAsset<GameObject>(SDCSUtils.baseHeadLoc);
			}
			else if (text == "hands")
			{
				gameObject = DataLoader.LoadAsset<GameObject>(SDCSUtils.baseBodyLoc);
			}
			else
			{
				gameObject = DataLoader.LoadAsset<GameObject>(SDCSUtils.baseBodyLoc);
			}
			if (gameObject == null)
			{
				return;
			}
			GameObject bodyPartContainingName;
			if (!((bodyPartContainingName = SDCSUtils.getBodyPartContainingName(gameObject.transform, text)) == null))
			{
				bodyPartContainingName.name = text;
				GameObject sourceObj = bodyPartContainingName;
				EModelSDCS emodel = null;
				Material eyeMat = DataLoader.LoadAsset<Material>(SDCSUtils.baseEyeColorMatLoc);
				SDCSUtils.Stitch(sourceObj, _rig, _boneCatalog, emodel, isFPV, 0f, false, eyeMat, false);
				if (text == "head")
				{
					Transform transform = _rig.transform;
					Transform transform2 = gameObject.transform;
					CharacterGazeController orAddComponent = transform.FindRecursive("Head").gameObject.GetOrAddComponent<CharacterGazeController>();
					orAddComponent.rootTransform = transform.FindRecursive("Origin");
					orAddComponent.neckTransform = _boneCatalog["Neck"];
					orAddComponent.headTransform = _boneCatalog["Head"];
					orAddComponent.leftEyeTransform = _boneCatalog["LeftEye"];
					orAddComponent.rightEyeTransform = _boneCatalog["RightEye"];
					orAddComponent.eyeMaterial = transform.FindRecursive("eyes").GetComponent<SkinnedMeshRenderer>().material;
					orAddComponent.leftEyeLocalPosition = transform2.FindInChildren("LeftEye").localPosition;
					orAddComponent.rightEyeLocalPosition = transform2.FindInChildren("RightEye").localPosition;
					orAddComponent.eyeLookAtTargetAngle = 35f;
					orAddComponent.eyeRotationSpeed = 30f;
					orAddComponent.twitchSpeed = 25f;
					orAddComponent.headLookAtTargetAngle = 45f;
					orAddComponent.headRotationSpeed = 7f;
					orAddComponent.maxLookAtDistance = 5f;
					EyeLidController orAddComponent2 = transform.FindRecursive("Head").gameObject.GetOrAddComponent<EyeLidController>();
					orAddComponent2.leftTopTransform = _boneCatalog["LeftEyelidTop"];
					orAddComponent2.leftBottomTransform = _boneCatalog["LeftEyelidBot"];
					orAddComponent2.rightTopTransform = _boneCatalog["RightEyelidTop"];
					orAddComponent2.rightBottomTransform = _boneCatalog["RightEyelidBot"];
					orAddComponent2.leftTopLocalPosition = transform2.FindInChildren("LeftEyelidTop").localPosition;
					orAddComponent2.leftBottomLocalPosition = transform2.FindInChildren("LeftEyelidBot").localPosition;
					orAddComponent2.leftTopRotation = transform2.FindInChildren("LeftEyelidTop").localRotation;
					orAddComponent2.leftBottomRotation = transform2.FindInChildren("LeftEyelidBot").localRotation;
					orAddComponent2.rightTopLocalPosition = transform2.FindInChildren("RightEyelidTop").localPosition;
					orAddComponent2.rightBottomLocalPosition = transform2.FindInChildren("RightEyelidBot").localPosition;
					orAddComponent2.rightTopRotation = transform2.FindInChildren("RightEyelidTop").localRotation;
					orAddComponent2.rightBottomRotation = transform2.FindInChildren("RightEyelidBot").localRotation;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipment(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] ignoredParts, bool isUI, List<SDCSUtils.SlotData> slotData)
	{
		if (slotData == null)
		{
			return;
		}
		List<Transform> allGears = new List<Transform>();
		Transform transform = _rig.transform.Find("Origin");
		if (transform)
		{
			List<Transform> list = SDCSUtils.findStartsWith(transform, "RigConstraints");
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				UnityEngine.Object.DestroyImmediate(list[i].gameObject);
			}
		}
		foreach (SDCSUtils.SlotData slotData2 in slotData)
		{
			if (string.IsNullOrEmpty(slotData2.HeadGearName))
			{
				Transform transform2 = SDCSUtils.setupEquipmentSlot(_rig, _boneCatalog, ignoredParts, slotData2, allGears);
				if (transform2)
				{
					float cullDistance = slotData2.CullDistance;
					SDCSUtils.Stitch(transform2.gameObject, _rig, _boneCatalog, null, false, cullDistance, isUI, null, false);
				}
			}
			else
			{
				SDCSUtils.setupHeadgear(_rig, _boneCatalog, null, false, ignoredParts, isUI, slotData2.HeadGearName);
			}
		}
		List<RigBuilder> rbs = new List<RigBuilder>();
		SDCSUtils.setupEquipmentCommon(_rig, _boneCatalog, allGears, rbs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipment(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] ignoredParts, EntityAlive entity, bool isUI)
	{
		if (!entity)
		{
			return;
		}
		EModelSDCS emodelSDCS = entity.emodel as EModelSDCS;
		if (!emodelSDCS)
		{
			return;
		}
		emodelSDCS.HairMaskType = SDCSUtils.SlotData.HairMaskTypes.Full;
		emodelSDCS.FacialHairMaskType = SDCSUtils.SlotData.HairMaskTypes.Full;
		if (!isUI && emodelSDCS.IsFPV)
		{
			emodelSDCS.ClipMaterialsFP.Clear();
		}
		List<Transform> allGears = new List<Transform>();
		Transform transform = _rig.transform.Find("Origin");
		if (transform)
		{
			List<Transform> list = SDCSUtils.findStartsWith(transform, "RigConstraints");
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				UnityEngine.Object.DestroyImmediate(list[i].gameObject);
			}
		}
		int slotCount = entity.equipment.GetSlotCount();
		for (int j = 0; j < slotCount; j++)
		{
			ItemValue slotItem = entity.equipment.GetSlotItem(j);
			if (slotItem != null && slotItem.ItemClass != null && slotItem.ItemClass.SDCSData != null)
			{
				SDCSUtils.SlotData sdcsdata = slotItem.ItemClass.SDCSData;
				ItemClassArmor itemClassArmor = slotItem.ItemClass as ItemClassArmor;
				if (itemClassArmor != null && itemClassArmor.EquipSlot == EquipmentSlots.Head)
				{
					emodelSDCS.HairMaskType = sdcsdata.HairMaskType;
					emodelSDCS.FacialHairMaskType = sdcsdata.FacialHairMaskType;
				}
				if (string.IsNullOrEmpty(sdcsdata.HeadGearName))
				{
					Transform transform2 = SDCSUtils.setupEquipmentSlot(_rig, _boneCatalog, ignoredParts, sdcsdata, allGears);
					if (transform2)
					{
						float cullDistance = sdcsdata.CullDistance;
						SDCSUtils.Stitch(transform2.gameObject, _rig, _boneCatalog, emodelSDCS, emodelSDCS.IsFPV, cullDistance, isUI, null, true);
					}
				}
				else
				{
					SDCSUtils.setupHeadgear(_rig, _boneCatalog, emodelSDCS, emodelSDCS.IsFPV, ignoredParts, isUI, sdcsdata.HeadGearName);
				}
			}
		}
		List<RigBuilder> rbs = new List<RigBuilder>();
		SDCSUtils.setupEquipmentCommon(_rig, _boneCatalog, allGears, rbs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform setupEquipmentSlot(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] ignoredParts, SDCSUtils.SlotData wornItem, List<Transform> allGears)
	{
		if (wornItem.PrefabName == null || wornItem.PrefabName.Length == 0)
		{
			return null;
		}
		if (wornItem.PartName == null || wornItem.PartName.Length == 0)
		{
			return null;
		}
		string text = wornItem.PartName.ToLower();
		foreach (string text2 in ignoredParts)
		{
			if (text.Contains(text2.ToLower()))
			{
				return null;
			}
		}
		string text3 = SDCSUtils.parseSexedLocation(wornItem.PrefabName, SDCSUtils.tmpArchetype.Sex);
		GameObject gameObject = DataLoader.LoadAsset<GameObject>(text3);
		if (!gameObject)
		{
			Log.Warning(string.Concat(new string[]
			{
				"SDCSUtils::",
				text3,
				" not found for item ",
				wornItem.PrefabName,
				"!"
			}));
			return null;
		}
		SDCSUtils.MatchRigs(wornItem, gameObject.transform, _rig.transform, _boneCatalog);
		if (!allGears.Contains(gameObject.transform))
		{
			allGears.Add(gameObject.transform);
		}
		Transform clothingPartWithName = SDCSUtils.getClothingPartWithName(gameObject, SDCSUtils.parseSexedLocation(wornItem.PartName, SDCSUtils.tmpArchetype.Sex));
		if (clothingPartWithName)
		{
			string baseToTurnOff = wornItem.BaseToTurnOff;
			if (baseToTurnOff != null && baseToTurnOff.Length > 0)
			{
				foreach (string name in wornItem.BaseToTurnOff.Split(',', StringSplitOptions.None))
				{
					Transform transform = _rig.transform.FindInChildren(name);
					if (transform)
					{
						UnityEngine.Object.Destroy(transform.gameObject);
					}
				}
			}
			if (!clothingPartWithName.gameObject.activeSelf)
			{
				clothingPartWithName.gameObject.SetActive(true);
			}
		}
		return clothingPartWithName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipmentCommon(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog, List<Transform> allGears, List<RigBuilder> rbs)
	{
		RigBuilder rigBuilder = _rigObj.GetComponent<RigBuilder>();
		if (!rigBuilder)
		{
			rigBuilder = _rigObj.AddComponent<RigBuilder>();
		}
		rigBuilder.enabled = false;
		rbs.Add(rigBuilder);
		foreach (Transform sourceRootT in allGears)
		{
			SDCSUtils.SetupRigConstraints(rigBuilder, sourceRootT, _rigObj.transform, _boneCatalog);
		}
		foreach (HingeJoint hingeJoint in _rigObj.GetComponentsInChildren<HingeJoint>())
		{
			if (hingeJoint.connectedBody != null && _boneCatalog.ContainsKey(hingeJoint.connectedBody.transform.name))
			{
				hingeJoint.connectedBody = _boneCatalog[hingeJoint.connectedBody.transform.name].GetComponent<Rigidbody>();
			}
			hingeJoint.autoConfigureConnectedAnchor = true;
		}
		rigBuilder.enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHairObjects(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] ignoredParts, EntityAlive entity, bool isUI)
	{
		if (!entity)
		{
			return;
		}
		EModelSDCS emodelSDCS = entity.emodel as EModelSDCS;
		if (!emodelSDCS)
		{
			return;
		}
		if (!isUI && emodelSDCS.IsFPV)
		{
			emodelSDCS.ClipMaterialsFP.Clear();
		}
		Transform transform = _rig.transform.Find("Origin");
		if (transform)
		{
			List<Transform> list = SDCSUtils.findStartsWith(transform, "RigConstraints");
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				UnityEngine.Object.DestroyImmediate(list[i].gameObject);
			}
		}
		if (!emodelSDCS.IsFPV || isUI)
		{
			SDCSUtils.setupHairObjects(_rig, _boneCatalog, emodelSDCS, emodelSDCS.IsFPV, ignoredParts, isUI, SDCSUtils.tmpArchetype.Hair, SDCSUtils.tmpArchetype.MustacheName, SDCSUtils.tmpArchetype.ChopsName, SDCSUtils.tmpArchetype.BeardName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHairObjects(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, EModelSDCS _emodel, bool _isFPV, string[] ignoredParts, bool isUI, string hairName, string mustacheName, string chopsName, string beardName)
	{
		HairColorSwatch hairColorSwatch = null;
		if (!string.IsNullOrEmpty(SDCSUtils.tmpArchetype.HairColor))
		{
			string text = SDCSUtils.baseHairColorLoc + "/" + SDCSUtils.tmpArchetype.HairColor;
			ScriptableObject scriptableObject = DataLoader.LoadAsset<ScriptableObject>(text);
			if (scriptableObject == null)
			{
				Log.Warning(string.Concat(new string[]
				{
					"SDCSUtils::",
					text,
					" not found for hair color ",
					SDCSUtils.tmpArchetype.HairColor,
					"!"
				}));
			}
			else
			{
				hairColorSwatch = (scriptableObject as HairColorSwatch);
			}
		}
		if (!string.IsNullOrEmpty(hairName))
		{
			if (_emodel != null)
			{
				if (_emodel.HairMaskType != SDCSUtils.SlotData.HairMaskTypes.None)
				{
					string str = "";
					if (_emodel.HairMaskType != SDCSUtils.SlotData.HairMaskTypes.Full)
					{
						str = "_" + _emodel.HairMaskType.ToString().ToLower();
					}
					SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseHairLoc + "/hair_" + hairName + str, hairName);
				}
			}
			else
			{
				SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseHairLoc + "/hair_" + hairName, hairName);
			}
		}
		if (!string.IsNullOrEmpty(mustacheName))
		{
			if (_emodel != null)
			{
				if (_emodel.FacialHairMaskType != SDCSUtils.SlotData.HairMaskTypes.None)
				{
					SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseMustacheLoc + "/hair_facial_mustache" + mustacheName, mustacheName);
				}
			}
			else
			{
				SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseMustacheLoc + "/hair_facial_mustache" + mustacheName, mustacheName);
			}
		}
		if (!string.IsNullOrEmpty(chopsName))
		{
			if (_emodel != null)
			{
				if (_emodel.FacialHairMaskType != SDCSUtils.SlotData.HairMaskTypes.None)
				{
					SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseChopsLoc + "/hair_facial_sideburns" + chopsName, chopsName);
				}
			}
			else
			{
				SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseChopsLoc + "/hair_facial_sideburns" + chopsName, chopsName);
			}
		}
		if (!string.IsNullOrEmpty(beardName))
		{
			if (_emodel != null)
			{
				if (_emodel.FacialHairMaskType != SDCSUtils.SlotData.HairMaskTypes.None)
				{
					SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseBeardLoc + "/hair_facial_beard" + beardName, beardName);
				}
			}
			else
			{
				SDCSUtils.setupHair(_rig, _boneCatalog, _emodel, _isFPV, ignoredParts, isUI, SDCSUtils.baseBeardLoc + "/hair_facial_beard" + beardName, beardName);
			}
		}
		if (hairColorSwatch != null)
		{
			SDCSUtils.ApplySwatchToGameObject(_rig, hairColorSwatch);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplySwatchToGameObject(GameObject targetGameObject, HairColorSwatch hairSwatch)
	{
		Shader y = Shader.Find("Game/SDCS/Hair");
		if (targetGameObject != null)
		{
			foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>(true))
			{
				Material[] array;
				if (Application.isPlaying)
				{
					array = renderer.materials;
				}
				else
				{
					array = renderer.sharedMaterials;
				}
				foreach (Material material in array)
				{
					if (material.shader == y && !material.name.Contains("lashes"))
					{
						hairSwatch.ApplyToMaterial(material);
					}
				}
			}
			return;
		}
		Debug.LogWarning("No target GameObject selected.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHair(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, EModelSDCS _emodel, bool _isFPV, string[] ignoredParts, bool isUI, string path, string hairName)
	{
		if (string.IsNullOrEmpty(hairName))
		{
			return;
		}
		MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(path);
		GameObject gameObject = (meshMorph != null) ? meshMorph.GetMorphedSkinnedMesh() : null;
		if (gameObject == null)
		{
			Log.Warning(string.Concat(new string[]
			{
				"SDCSUtils::",
				path,
				" not found for hair ",
				hairName,
				"!"
			}));
			return;
		}
		SDCSUtils.MatchRigs(null, gameObject.transform, _rig.transform, _boneCatalog);
		if (!gameObject.gameObject.activeSelf)
		{
			gameObject.gameObject.SetActive(true);
		}
		SDCSUtils.Stitch(gameObject.gameObject, _rig, _boneCatalog, _emodel, _isFPV, 0f, isUI, null, false);
		UnityEngine.Object.Destroy(gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHeadgear(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, EModelSDCS _emodel, bool _isFPV, string[] ignoredParts, bool isUI, string headgear)
	{
		if (string.IsNullOrEmpty(headgear) || ignoredParts.Contains("head"))
		{
			return;
		}
		string text = (string.IsNullOrEmpty(SDCSUtils.tmpArchetype.Hair) || SDCSUtils.shortHairNames.ContainsCaseInsensitive(SDCSUtils.tmpArchetype.Hair)) ? "Bald" : "";
		string text2 = string.Concat(new string[]
		{
			"Entities/Player/",
			SDCSUtils.tmpArchetype.Sex,
			"/HeadGearMorphMatrix/",
			headgear,
			"/",
			SDCSUtils.tmpArchetype.Race,
			SDCSUtils.tmpArchetype.Variant.ToString("00"),
			"/gear",
			headgear,
			text,
			"Head"
		});
		MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(text2);
		if (meshMorph == null)
		{
			text2 = string.Concat(new string[]
			{
				"Entities/Player/",
				SDCSUtils.tmpArchetype.Sex,
				"/HeadGearMorphMatrix/",
				headgear,
				"/",
				SDCSUtils.tmpArchetype.Race,
				SDCSUtils.tmpArchetype.Variant.ToString("00"),
				"/gear",
				headgear,
				"Head"
			});
			meshMorph = DataLoader.LoadAsset<MeshMorph>(text2);
		}
		GameObject gameObject = (meshMorph != null) ? meshMorph.GetMorphedSkinnedMesh() : null;
		if (gameObject == null)
		{
			Log.Warning(string.Concat(new string[]
			{
				"SDCSUtils::",
				text2,
				" not found for headgear ",
				headgear,
				"!"
			}));
			return;
		}
		SDCSUtils.MatchRigs(null, gameObject.transform, _rig.transform, _boneCatalog);
		if (!gameObject.gameObject.activeSelf)
		{
			gameObject.gameObject.SetActive(true);
		}
		DataLoader.LoadAsset<Material>(SDCSUtils.baseBodyMatLoc);
		SDCSUtils.Stitch(gameObject.gameObject, _rig, _boneCatalog, _emodel, _isFPV, 0f, isUI, null, false);
		UnityEngine.Object.Destroy(gameObject);
	}

	public static bool BasePartsExist(Archetype _archetype)
	{
		SDCSUtils.tmpArchetype = _archetype;
		if (!DataLoader.LoadAsset<GameObject>(SDCSUtils.baseBodyLoc))
		{
			Log.Error("base body not found at " + SDCSUtils.baseBodyLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<GameObject>(SDCSUtils.baseHeadLoc))
		{
			Log.Error("base head not found at " + SDCSUtils.baseHeadLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<Material>(SDCSUtils.baseBodyMatLoc))
		{
			Log.Error("body material not found at " + SDCSUtils.baseBodyMatLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<Material>(SDCSUtils.baseHeadMatLoc))
		{
			Log.Error("head material not found at " + SDCSUtils.baseHeadMatLoc);
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Transform> findStartsWith(Transform parent, string key)
	{
		List<Transform> list = new List<Transform>();
		foreach (object obj in parent)
		{
			Transform transform = (Transform)obj;
			if (transform.name.StartsWith(key))
			{
				list.Add(transform);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject getBodyPartContainingName(Transform parent, string name)
	{
		foreach (object obj in parent.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name.ToLower().Contains(name))
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform getClothingPartWithName(GameObject clothingPrefab, string partName)
	{
		foreach (object obj in clothingPrefab.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name.ToLower() == partName.ToLower())
			{
				return transform;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string parseSexedLocation(string sexedLocation, string sex)
	{
		return sexedLocation.Replace("*", sex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Cloth> tempCloths = new List<Cloth>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> tempMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SkinnedMeshRenderer> tempSMRs = new List<SkinnedMeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] shortHairNames = new string[]
	{
		"buzzcut",
		"comb_over",
		"cornrows",
		"flattop_fro",
		"mohawk",
		"pixie_cut",
		"small_fro"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ORIGIN = "Origin";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string RIGCON = "RigConstraints";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string IKRIG = "IKRig";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string HEAD = "head";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EYES = "eyes";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string BEARD = "beard";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string HAIR = "hair";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string BODY = "body";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string HANDS = "hands";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FEET = "feet";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string HELMET = "helmet";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string TORSO = "torso";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GLOVES = "gloves";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string BOOTS = "boots";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Archetype tmpArchetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] baseParts = new string[]
	{
		"head",
		"body",
		"hands",
		"feet"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ignoredParts = new string[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] basePartsFP = new string[]
	{
		"body",
		"hands"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ignoredPartsFP = new string[]
	{
		"head",
		"helmet",
		"feet",
		"boots"
	};

	public class SlotData
	{
		public string PrefabName;

		public string PartName;

		public string BaseToTurnOff;

		public float CullDistance = 0.32f;

		public string HeadGearName;

		public SDCSUtils.SlotData.HairMaskTypes HairMaskType;

		public SDCSUtils.SlotData.HairMaskTypes FacialHairMaskType;

		public enum HairMaskTypes
		{
			Full,
			Hat,
			Bald,
			None
		}
	}

	public class TransformCatalog : Dictionary<string, Transform>
	{
		public TransformCatalog(Transform _transform)
		{
			this.AddRecursive(_transform);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddRecursive(Transform _transform)
		{
			string name = _transform.name;
			if (base.ContainsKey(name))
			{
				base[name] = _transform;
			}
			else
			{
				base.Add(name, _transform);
			}
			foreach (object obj in _transform)
			{
				Transform transform = (Transform)obj;
				this.AddRecursive(transform);
			}
		}
	}
}
