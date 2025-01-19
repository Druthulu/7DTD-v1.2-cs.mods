﻿using System;
using System.Collections.Generic;

public class MaterialBlock
{
	public float ExplosionResistance
	{
		get
		{
			return this.explosionResistance;
		}
		set
		{
			this.explosionResistance = Utils.FastClamp01(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static MaterialBlock()
	{
		MaterialBlock.air = new MaterialBlock();
	}

	public static void Cleanup()
	{
		MaterialBlock.materials = new Dictionary<string, MaterialBlock>();
	}

	public MaterialBlock()
	{
		this.Properties = new DynamicProperties();
	}

	public MaterialBlock(string _id) : this()
	{
		this.id = _id;
		this.IsCollidable = true;
		this.LightOpacity = 0;
		MaterialBlock.materials[_id] = this;
	}

	public static MaterialBlock fromString(string _name)
	{
		if (!MaterialBlock.materials.ContainsKey(_name))
		{
			return null;
		}
		return MaterialBlock.materials[_name];
	}

	public string GetLocalizedMaterialName()
	{
		return Localization.Get("material" + this.id, false);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool CheckDamageIgnore(FastTags<TagGroup.Global> _tags, EntityAlive _entity)
	{
		if (!this.IgnoreDamageFromTag.IsEmpty && _tags.Test_AnySet(this.IgnoreDamageFromTag))
		{
			if (_entity != null)
			{
				_entity.PlayOneShot("keystone_impact_overlay", false, false, false);
			}
			return true;
		}
		return false;
	}

	public static MaterialBlock air;

	public static Dictionary<string, MaterialBlock> materials = new Dictionary<string, MaterialBlock>();

	public DynamicProperties Properties;

	public bool StabilitySupport = true;

	public DataItem<float> Hardness;

	public StepSound stepSound;

	public bool IsCollidable;

	public int LightOpacity;

	public int StabilityGlue = 6;

	public bool IsLiquid;

	public string DamageCategory;

	public string SurfaceCategory;

	public string ParticleCategory;

	public string ParticleDestroyCategory;

	public string ForgeCategory;

	public int FertileLevel;

	public bool IsPlant;

	public float MovementFactor = 1f;

	public float Friction = 1f;

	public DataItem<int> Mass;

	public int MaxDamage;

	public int MaxIncomingDamage = int.MaxValue;

	public float Experience = 1f;

	public string id;

	public FastTags<TagGroup.Global> IgnoreDamageFromTag = FastTags<TagGroup.Global>.none;

	public bool IsGroundCover;

	public bool CanDestroy = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float explosionResistance;
}
