using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAttachParticleEffectToEntity : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (_params.Self == null)
		{
			return;
		}
		Transform transform = null;
		float num = 0f;
		bool flag = this.setShapeMesh;
		if (_params.Tags.Test_AnySet(this.usePassedInTransformTag))
		{
			transform = _params.Transform;
		}
		else if (this.parent_transform_path == null)
		{
			transform = _params.Self.emodel.meshTransform;
		}
		else if (this.parent_transform_path == "LOD0")
		{
			transform = _params.Self.emodel.meshTransform;
		}
		else if (this.parent_transform_path == ".item")
		{
			Transform rightHandTransform = _params.Self.emodel.GetRightHandTransform();
			if (rightHandTransform && rightHandTransform.childCount > 0)
			{
				transform = rightHandTransform.GetChild(0);
			}
		}
		else if (this.parent_transform_path == ".body")
		{
			EModelSDCS emodelSDCS = _params.Self.emodel as EModelSDCS;
			if (emodelSDCS != null)
			{
				Transform parent = _params.Self.transform.Find("Graphics/Model");
				if (this.setShapeMesh && !emodelSDCS.IsFPV)
				{
					transform = GameUtils.FindDeepChildActive(parent, "body");
					if (transform == null)
					{
						transform = GameUtils.FindDeepChildActive(parent, "torso");
					}
					if (transform == null)
					{
						transform = GameUtils.FindDeepChild(parent, "body");
					}
				}
				else
				{
					transform = GameUtils.FindDeepChild(parent, "Spine1");
					flag = false;
				}
			}
			else
			{
				transform = _params.Self.emodel.GetPelvisTransform();
				if ((_params.Self.entityFlags & EntityFlags.Animal) > EntityFlags.None)
				{
					this.local_rotation.x = this.local_rotation.x + 90f;
					num = 1f;
				}
			}
		}
		else if (this.parent_transform_path == ".head")
		{
			transform = _params.Self.emodel.GetHeadTransform();
		}
		else
		{
			Transform transform2 = GameUtils.FindDeepChild(_params.Self.transform, this.parent_transform_path);
			if (transform2)
			{
				Transform parent2 = transform2.parent;
				if (!parent2 || !this.setShapeMesh || !parent2.gameObject.CompareTag("Item"))
				{
					transform = transform2;
				}
			}
		}
		if (!transform)
		{
			return;
		}
		string text = "Ptl_" + this.goToInstantiate.name;
		Transform transform3 = transform.Find(text);
		if (!transform3)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.goToInstantiate);
			if (!gameObject)
			{
				return;
			}
			transform3 = gameObject.transform;
			gameObject.name = text;
			Utils.SetLayerRecursively(gameObject, transform.gameObject.layer, null);
			transform3.SetParent(transform, false);
			transform3.SetLocalPositionAndRotation(this.local_offset, Quaternion.Euler(this.local_rotation.x, this.local_rotation.y, this.local_rotation.z));
			if (num > 0f)
			{
				transform3.localScale = Vector3.one * num;
			}
			_params.Self.AddParticle(text, transform3);
			AudioPlayer component = transform3.GetComponent<AudioPlayer>();
			if (component)
			{
				component.duration = 100000f;
			}
			ParticleSystem[] componentsInChildren = transform3.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren != null)
			{
				foreach (ParticleSystem particleSystem in componentsInChildren)
				{
					particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					ParticleSystem.ShapeModule shape = particleSystem.shape;
					ParticleSystemShapeType shapeType = shape.shapeType;
					if (shapeType == ParticleSystemShapeType.SkinnedMeshRenderer || shapeType == ParticleSystemShapeType.Mesh)
					{
						SkinnedMeshRenderer componentInChildren = transform.GetComponentInChildren<SkinnedMeshRenderer>();
						if (componentInChildren && flag)
						{
							shape.skinnedMeshRenderer = componentInChildren;
						}
						else
						{
							MeshRenderer componentInChildren2 = transform.GetComponentInChildren<MeshRenderer>();
							if (componentInChildren2 && flag)
							{
								shape.meshRenderer = componentInChildren2;
								shape.shapeType = ParticleSystemShapeType.MeshRenderer;
							}
							else
							{
								shape.shapeType = ParticleSystemShapeType.Sphere;
								if (flag)
								{
									Log.Warning("AttachParticleEffectToEntity {0}, {1} no renderer!", new object[]
									{
										_params.Self,
										text
									});
								}
							}
						}
					}
					if (flag)
					{
						EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
						if (entityPlayerLocal && entityPlayerLocal.bFirstPersonView)
						{
							shape.position += new Vector3(0f, 0f, 0.3f);
						}
					}
					particleSystem.main.duration = 100000f;
					particleSystem.Play();
				}
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && _params.Self != null && this.goToInstantiate != null;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "particle")
			{
				this.goToInstantiate = LoadManager.LoadAssetFromAddressables<GameObject>("ParticleEffects/" + _attribute.Value + ".prefab", null, null, false, true).Asset;
				return true;
			}
			if (localName == "parent_transform")
			{
				this.parent_transform_path = _attribute.Value;
				return true;
			}
			if (localName == "local_offset")
			{
				this.local_offset = StringParsers.ParseVector3(_attribute.Value, 0, -1);
				return true;
			}
			if (localName == "local_rotation")
			{
				this.local_rotation = StringParsers.ParseVector3(_attribute.Value, 0, -1);
				return true;
			}
			if (localName == "shape_mesh")
			{
				this.setShapeMesh = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
		}
		return flag;
	}

	public const string cParticlePrefix = "Ptl_";

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goToInstantiate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform_path;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_rotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> usePassedInTransformTag = FastTags<TagGroup.Global>.Parse("usePassedInTransform");

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setShapeMesh;
}
