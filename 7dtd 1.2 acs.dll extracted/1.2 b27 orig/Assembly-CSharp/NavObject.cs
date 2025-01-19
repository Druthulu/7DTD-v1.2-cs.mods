using System;
using System.Collections.Generic;
using UnityEngine;

public class NavObject
{
	public NavObject.TrackTypes TrackType { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool HasRequirements
	{
		get
		{
			return this.NavObjectClass != null;
		}
	}

	public string DisplayName
	{
		get
		{
			if (this.usingLocalizationId)
			{
				if (string.IsNullOrEmpty(this.localizedName))
				{
					this.localizedName = Localization.Get(this.name, false);
				}
				return this.localizedName;
			}
			return this.name;
		}
	}

	public Transform TrackedTransform
	{
		get
		{
			return this.trackedTransform;
		}
		set
		{
			this.trackedTransform = value;
			this.TrackType = NavObject.TrackTypes.Transform;
		}
	}

	public Vector3 TrackedPosition
	{
		get
		{
			return this.trackedPosition;
		}
		set
		{
			this.trackedPosition = value;
			this.TrackType = NavObject.TrackTypes.Position;
		}
	}

	public Entity TrackedEntity
	{
		get
		{
			return this.trackedEntity;
		}
		set
		{
			this.trackedEntity = value;
			this.TrackType = NavObject.TrackTypes.Entity;
			this.SetupEntityOptions();
		}
	}

	public NavObjectMapSettings CurrentMapSettings
	{
		get
		{
			return this.NavObjectClass.GetMapSettings(this.IsActive);
		}
	}

	public NavObjectCompassSettings CurrentCompassSettings
	{
		get
		{
			return this.NavObjectClass.GetCompassSettings(this.IsActive);
		}
	}

	public NavObjectScreenSettings CurrentScreenSettings
	{
		get
		{
			return this.NavObjectClass.GetOnScreenSettings(this.IsActive);
		}
	}

	public bool HasOnScreen
	{
		get
		{
			return this.hasOnScreen;
		}
	}

	public int Key { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public Vector3 Rotation
	{
		get
		{
			if (!this.CurrentMapSettings.UseRotation || this.TrackType != NavObject.TrackTypes.Entity)
			{
				return Vector3.zero;
			}
			if (this.trackedEntity.AttachedToEntity != null)
			{
				return this.trackedEntity.AttachedToEntity.rotation;
			}
			return this.trackedEntity.rotation;
		}
	}

	public bool IsTrackedTransform(Transform transform)
	{
		return this.TrackType == NavObject.TrackTypes.Transform && this.trackedTransform == transform;
	}

	public bool IsTrackedPosition(Vector3 position)
	{
		return this.TrackType == NavObject.TrackTypes.Position && this.trackedPosition == position;
	}

	public bool IsTrackedEntity(Entity entity)
	{
		return this.TrackType == NavObject.TrackTypes.Entity && (this.trackedEntity == entity || this.entityID == entity.entityId);
	}

	public bool IsValidPlayer(EntityPlayerLocal player, NavObjectClass navObjectClass)
	{
		if (this.ForceDisabled)
		{
			return false;
		}
		bool flag = true;
		if (this.TrackType == NavObject.TrackTypes.Entity)
		{
			flag = this.IsValidEntity(player, this.TrackedEntity, navObjectClass);
		}
		switch (navObjectClass.RequirementType)
		{
		case NavObjectClass.RequirementTypes.CVar:
			return player.GetCVar(navObjectClass.RequirementName) > 0f && flag;
		case NavObjectClass.RequirementTypes.QuestBounds:
		case NavObjectClass.RequirementTypes.Tracking:
			return flag;
		case NavObjectClass.RequirementTypes.NoTag:
			return !NavObjectManager.Instance.HasNavObjectTag(navObjectClass.RequirementName) && flag;
		case NavObjectClass.RequirementTypes.IsOwner:
			return this.OwnerEntity == player && flag;
		case NavObjectClass.RequirementTypes.MinimumTreasureRadius:
		{
			float num = this.ExtraData;
			num = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Mathf.Clamp(num, 0f, num);
			return num == 0f;
		}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidEntity(EntityPlayerLocal player, Entity entity, NavObjectClass navObjectClass)
	{
		if (entity == null)
		{
			return true;
		}
		if (player == null)
		{
			return true;
		}
		if (entity is EntityAlive)
		{
			EntityAlive entityAlive = entity as EntityAlive;
			if (navObjectClass.RequirementType == NavObjectClass.RequirementTypes.None)
			{
				return entityAlive.IsAlive() && !entityAlive.IsSleeperPassive;
			}
			if (!entityAlive.IsAlive() || entityAlive.IsSleeperPassive)
			{
				return false;
			}
			switch (navObjectClass.RequirementType)
			{
			case NavObjectClass.RequirementTypes.CVar:
				return entityAlive.GetCVar(navObjectClass.RequirementName) > 0f;
			case NavObjectClass.RequirementTypes.QuestBounds:
				if (player.QuestJournal.ActiveQuest != null && entityAlive.IsSleeper)
				{
					Vector3 position = entity.position;
					position.y = position.z;
					if (player.ZombieCompassBounds.Contains(position))
					{
						return true;
					}
				}
				return false;
			case NavObjectClass.RequirementTypes.Tracking:
				return EffectManager.GetValue(PassiveEffects.Tracking, null, 0f, player, null, entity.EntityTags, true, true, true, true, true, 1, true, false) > 0f;
			case NavObjectClass.RequirementTypes.InParty:
				return player.Party != null && player.Party.MemberList.Contains(entity as EntityPlayer) && entity != player && !(entity as EntityPlayer).IsSpectator && (player.AttachedToEntity == null || player.AttachedToEntity != entity.AttachedToEntity);
			case NavObjectClass.RequirementTypes.IsAlly:
				return entity as EntityPlayer != null && (entity as EntityPlayer).IsFriendOfLocalPlayer && entity != player && !(entity as EntityPlayer).IsSpectator;
			case NavObjectClass.RequirementTypes.IsPlayer:
				return entity == player;
			case NavObjectClass.RequirementTypes.IsVehicleOwner:
				return (entity as EntityVehicle != null && (entity as EntityVehicle).HasOwnedEntity(player.entityId)) || (entity as EntityTurret != null && (entity as EntityTurret).belongsPlayerId == player.entityId);
			case NavObjectClass.RequirementTypes.NoActiveQuests:
				return entity as EntityNPC == null || player.QuestJournal.FindReadyForTurnInQuestByGiver(entity.entityId) == null;
			case NavObjectClass.RequirementTypes.IsTwitchSpawnedSelf:
				return entity.spawnById == player.entityId && !string.IsNullOrEmpty(entity.spawnByName);
			case NavObjectClass.RequirementTypes.IsTwitchSpawnedOther:
				return entity.spawnById > 0 && entity.spawnById != player.entityId && !string.IsNullOrEmpty(entity.spawnByName);
			}
		}
		else
		{
			NavObjectClass.RequirementTypes requirementType = navObjectClass.RequirementType;
			if (requirementType == NavObjectClass.RequirementTypes.IsTwitchSpawnedSelf)
			{
				return entity.spawnById == player.entityId;
			}
			if (requirementType == NavObjectClass.RequirementTypes.IsTwitchSpawnedOther)
			{
				return entity.spawnById > 0 && entity.spawnById != player.entityId;
			}
		}
		return true;
	}

	public void AddNavObjectClass(NavObjectClass navClass)
	{
		if (!this.NavObjectClassList.Contains(navClass))
		{
			this.NavObjectClassList.Insert(0, navClass);
		}
	}

	public bool RemoveNavObjectClass(NavObjectClass navClass)
	{
		this.NavObjectClassList.Remove(navClass);
		if (this.NavObjectClassList.Count == 0)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupEntityOptions()
	{
		if (this.TrackType == NavObject.TrackTypes.Entity && this.NavObjectClass != null && this.NavObjectClass.RequirementType == NavObjectClass.RequirementTypes.Tracking)
		{
			this.OverrideSpriteName = ((this.TrackedEntity.GetTrackerIcon() == null) ? "" : this.TrackedEntity.GetTrackerIcon());
			return;
		}
		this.OverrideSpriteName = "";
	}

	public bool IsValid()
	{
		if (this.TrackType == NavObject.TrackTypes.Transform && this.TrackedTransform == null)
		{
			this.TrackType = NavObject.TrackTypes.None;
		}
		else if (this.TrackType == NavObject.TrackTypes.Entity && this.TrackedEntity == null && this.entityID == -1)
		{
			this.TrackType = NavObject.TrackTypes.None;
		}
		return this.TrackType > NavObject.TrackTypes.None;
	}

	public Vector3 GetPosition()
	{
		switch (this.TrackType)
		{
		case NavObject.TrackTypes.Transform:
			return this.trackedTransform.position;
		case NavObject.TrackTypes.Position:
			return this.trackedPosition - Origin.position;
		case NavObject.TrackTypes.Entity:
			if (this.entityID != -1)
			{
				return this.trackedPosition - Origin.position;
			}
			return this.trackedEntity.position - Origin.position;
		default:
			return NavObject.InvalidPos;
		}
	}

	public float GetMaxDistance(NavObjectSettings settings, EntityPlayer player)
	{
		if (this.TrackType == NavObject.TrackTypes.Entity && this.NavObjectClass.RequirementType == NavObjectClass.RequirementTypes.Tracking && settings.MaxDistance == -1f)
		{
			return EffectManager.GetValue(PassiveEffects.TrackDistance, null, 0f, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		return settings.MaxDistance;
	}

	public string GetSpriteName(NavObjectSettings settings)
	{
		if (!this.NavObjectClass.UseOverrideIcon)
		{
			return settings.SpriteName;
		}
		return this.OverrideSpriteName;
	}

	public int EntityID
	{
		get
		{
			return this.entityID;
		}
	}

	public void PauseEntityUpdate()
	{
		if (this.TrackType == NavObject.TrackTypes.Entity)
		{
			this.entityID = this.trackedEntity.entityId;
			this.trackedPosition = this.trackedEntity.position;
		}
	}

	public void RestoreEntityUpdate()
	{
		if (this.TrackType == NavObject.TrackTypes.Entity)
		{
			this.entityID = -1;
			this.trackedPosition = NavObject.InvalidPos;
		}
	}

	public NavObject(string className)
	{
		this.Key = NavObject.nextKey++;
		this.SetupNavObjectClass(className);
	}

	public void Reset(string className)
	{
		this.UseOverrideColor = false;
		this.OverrideColor = Color.white;
		this.SetupNavObjectClass(className);
		this.trackedPosition = NavObject.InvalidPos;
		this.trackedTransform = null;
		this.trackedEntity = null;
		this.OwnerEntity = null;
		this.name = "";
		this.ForceDisabled = false;
		this.usingLocalizationId = false;
		this.TrackType = NavObject.TrackTypes.None;
	}

	public void SetupNavObjectClass(string className)
	{
		this.NavObjectClassList.Clear();
		this.hasOnScreen = false;
		if (className.Contains(","))
		{
			string[] array = className.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(array[i]);
				if (navObjectClass != null)
				{
					if (navObjectClass.OnScreenSettings != null || navObjectClass.InactiveOnScreenSettings != null)
					{
						this.hasOnScreen = true;
					}
					this.NavObjectClassList.Add(navObjectClass);
				}
			}
		}
		else
		{
			NavObjectClass navObjectClass2 = NavObjectClass.GetNavObjectClass(className);
			if (navObjectClass2.OnScreenSettings != null || navObjectClass2.InactiveOnScreenSettings != null)
			{
				this.hasOnScreen = true;
			}
			this.NavObjectClassList.Add(navObjectClass2);
		}
		this.NavObjectClass = this.NavObjectClassList[0];
	}

	public void HandleActiveNavClass(EntityPlayerLocal localPlayer)
	{
		if (this.NavObjectClassList != null && this.NavObjectClassList.Count > 0)
		{
			for (int i = 0; i < this.NavObjectClassList.Count; i++)
			{
				if (this.IsValidPlayer(localPlayer, this.NavObjectClassList[i]))
				{
					if (this.NavObjectClass != this.NavObjectClassList[i])
					{
						this.NavObjectClass = this.NavObjectClassList[i];
						this.SetupEntityOptions();
					}
					return;
				}
			}
			this.NavObjectClass = null;
		}
	}

	public virtual float GetCompassIconScale(float _distance)
	{
		float t = 1f - _distance / this.CurrentCompassSettings.MaxScaleDistance;
		return Mathf.Lerp(this.CurrentCompassSettings.MinCompassIconScale, this.CurrentCompassSettings.MaxCompassIconScale, t);
	}

	public override string ToString()
	{
		string text = "";
		if (this.TrackType == NavObject.TrackTypes.Transform)
		{
			text = ((this.TrackedTransform != null) ? this.TrackedTransform.name : "none");
		}
		else if (this.TrackType == NavObject.TrackTypes.Entity)
		{
			text = ((this.TrackedEntity != null) ? this.TrackedEntity.GetDebugName() : "none");
		}
		string format = "{0} #{1}, {2}, {3}, {4}, {5}, {6}";
		object[] array = new object[7];
		array[0] = this.name;
		array[1] = this.NavObjectClassList.Count;
		array[2] = ((this.NavObjectClass != null) ? this.NavObjectClass.NavObjectClassName : "null");
		int num = 3;
		NavObjectClass navObjectClass = this.NavObjectClass;
		array[num] = ((navObjectClass != null) ? new NavObjectClass.RequirementTypes?(navObjectClass.RequirementType) : null);
		array[4] = this.TrackType;
		array[5] = text;
		array[6] = this.GetPosition();
		return string.Format(format, array);
	}

	public static Vector3 InvalidPos = new Vector3(-999f, -999f, -999f);

	public List<NavObjectClass> NavObjectClassList = new List<NavObjectClass>();

	public NavObjectClass NavObjectClass;

	public bool IsActive = true;

	public bool ForceDisabled;

	public Entity OwnerEntity;

	public string name;

	public bool usingLocalizationId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform trackedTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 trackedPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity trackedEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasOnScreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextKey = 0;

	public float ExtraData;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID = -1;

	public bool IsTracked;

	public bool hiddenOnCompass = true;

	public string HiddenDisplayName;

	public string OverrideSpriteName = "";

	public bool UseOverrideColor;

	public bool UseOverrideFontColor;

	public Color OverrideColor;

	public enum TrackTypes
	{
		None,
		Transform,
		Position,
		Entity
	}
}
