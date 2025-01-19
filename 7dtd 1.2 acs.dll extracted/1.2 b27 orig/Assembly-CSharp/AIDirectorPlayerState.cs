using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorPlayerState : IMemoryPoolableObject
{
	public AIDirectorPlayerState Construct(EntityPlayer _player)
	{
		this.Player = _player;
		this.m_smellEmitTime = 1.0;
		this.m_dead = false;
		return this;
	}

	public void Reset()
	{
		this.Player = null;
	}

	public void Cleanup()
	{
	}

	public void EmitSmell(double dt)
	{
		this.m_smellEmitTime -= dt;
		if (this.m_smellEmitTime <= 0.0)
		{
			this.UpdateSmell();
			this.m_smellEmitTime += 1.0;
		}
	}

	public AIDirectorPlayerInventory Inventory
	{
		get
		{
			return this.m_inventory;
		}
		set
		{
			this.m_inventory = value;
		}
	}

	public bool Dead
	{
		get
		{
			return this.m_dead;
		}
		set
		{
			if (this.m_dead && !value)
			{
				this.m_smellEmitTime = 2.0;
			}
			this.m_dead = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSmell()
	{
		float num = 0f;
		if (this.m_inventory.bag != null)
		{
			for (int i = 0; i < this.m_inventory.bag.Count; i++)
			{
				ItemClass forId = ItemClass.GetForId(this.m_inventory.bag[i].id);
				if (forId != null && forId.Smell != null)
				{
					num = Math.Max(forId.Smell.range, num);
				}
			}
		}
		if (this.m_inventory.belt != null)
		{
			for (int j = 0; j < this.m_inventory.belt.Count; j++)
			{
				ItemClass forId2 = ItemClass.GetForId(this.m_inventory.belt[j].id);
				if (forId2 != null && forId2.Smell != null)
				{
					num = Math.Max(forId2.Smell.beltRange, num);
				}
			}
		}
		this.Player.Stealth.smell = Mathf.FloorToInt(num);
	}

	public const double kSmellEmitTime = 1.0;

	public const float kCheckUndergroundTime = 5f;

	public const int kNumBlocksUnderground = 10;

	public EntityPlayer Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory m_inventory;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_smellEmitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dead;
}
