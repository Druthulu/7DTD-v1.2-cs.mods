using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionVomit : ItemActionLauncher
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionVomit.ItemActionDataVomit(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		_props.ParseInt("AnimType", ref this.animType);
		this.warningDelay = 1.2f;
		_props.ParseFloat("WarningDelay", ref this.warningDelay);
		this.warningMax = 3;
		_props.ParseInt("WarningMax", ref this.warningMax);
		_props.ParseString("Sound_warning", ref this.soundWarning);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetAttack(ItemActionVomit.ItemActionDataVomit _actionData)
	{
		_actionData.numWarningsPlayed = 0;
		_actionData.warningTime = 0f;
		_actionData.bAttackStarted = false;
		_actionData.isDone = false;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		ItemActionVomit.ItemActionDataVomit itemActionDataVomit = (ItemActionVomit.ItemActionDataVomit)_actionData;
		if (itemActionDataVomit.muzzle == null)
		{
			itemActionDataVomit.muzzle = _actionData.invData.holdingEntity.emodel.GetRightHandTransform();
		}
		if (_firingState != 0)
		{
			itemActionDataVomit.numVomits++;
			Vector3 direction = itemActionDataVomit.invData.holdingEntity.GetLookRay().direction;
			int burstCount = this.GetBurstCount(_actionData);
			for (int i = 0; i < burstCount; i++)
			{
				Vector3 directionRandomOffset = this.getDirectionRandomOffset(itemActionDataVomit, direction);
				base.instantiateProjectile(_actionData, default(Vector3)).GetComponent<ProjectileMoveScript>().Fire(_startPos, directionRandomOffset, _actionData.invData.holdingEntity, this.hitmaskOverride, 0.2f);
			}
		}
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionVomit.ItemActionDataVomit itemActionDataVomit = (ItemActionVomit.ItemActionDataVomit)_actionData;
		if (_bReleased)
		{
			base.ExecuteAction(_actionData, _bReleased);
			this.resetAttack(itemActionDataVomit);
			return;
		}
		float time = Time.time;
		if (time - itemActionDataVomit.m_LastShotTime < this.Delay)
		{
			return;
		}
		if (itemActionDataVomit.warningTime > 0f && time < itemActionDataVomit.warningTime)
		{
			return;
		}
		if (!itemActionDataVomit.bAttackStarted)
		{
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			if (itemActionDataVomit.numWarningsPlayed < this.warningMax - 1 && holdingEntity.rand.RandomFloat < 0.5f)
			{
				itemActionDataVomit.numWarningsPlayed++;
				itemActionDataVomit.warningTime = time + this.warningDelay;
				holdingEntity.PlayOneShot(this.soundWarning, false, false, false);
				holdingEntity.Raging = true;
				return;
			}
			itemActionDataVomit.bAttackStarted = true;
			itemActionDataVomit.numVomits = 0;
			holdingEntity.StartSpecialAttack(this.animType);
			if (this.warningMax > 0)
			{
				holdingEntity.PlayOneShot(this.soundWarning, false, false, false);
				itemActionDataVomit.warningTime = time + this.warningDelay;
				return;
			}
		}
		if (itemActionDataVomit.numVomits >= this.GetMaxAmmoCount(itemActionDataVomit))
		{
			itemActionDataVomit.isDone = true;
			return;
		}
		itemActionDataVomit.curBurstCount = 0;
		base.ExecuteAction(_actionData, _bReleased);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadius = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int animType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float warningDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int warningMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundWarning;

	public class ItemActionDataVomit : ItemActionLauncher.ItemActionDataLauncher
	{
		public ItemActionDataVomit(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public float warningTime;

		public int numWarningsPlayed;

		public int numVomits;

		public bool bAttackStarted;

		public bool isDone;
	}
}
