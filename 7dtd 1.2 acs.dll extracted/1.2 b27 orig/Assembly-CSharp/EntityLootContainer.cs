using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityLootContainer : EntityItem
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		foreach (Collider collider in base.transform.GetComponentsInChildren<Collider>())
		{
			collider.gameObject.tag = "E_BP_Body";
			collider.gameObject.layer = 0;
			collider.enabled = true;
			collider.gameObject.AddMissingComponent<RootTransformRefEntity>().RootTransform = base.transform;
		}
		this.SetDead();
		if (this.lootContainer != null)
		{
			this.lootContainer.entityId = this.entityId;
		}
	}

	public void SetContent(ItemStack[] _inventory)
	{
		this.isInventory = _inventory;
		this.lootContainer = null;
		this.forceInventoryCreate = true;
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropTimeStayAfterDeath))
		{
			this.timeStayAfterDeath = (int)(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropTimeStayAfterDeath], 0, -1, NumberStyles.Any) * 20f);
			return;
		}
		this.timeStayAfterDeath = 100;
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.lootContainer != null && this.deathUpdateTime > 0)
		{
			bool flag = GameManager.Instance.GetEntityIDForLockedTileEntity(this.lootContainer) != -1;
			if (!this.bRemoved && !this.lootContainer.IsUserAccessing() && !flag && ((this.lootContainer.bTouched && this.lootContainer.IsEmpty()) || this.deathUpdateTime >= this.timeStayAfterDeath - 1))
			{
				this.removeBackpack();
			}
		}
		this.deathUpdateTime++;
		if (!this.world.IsRemote() && (this.forceInventoryCreate || this.lootContainer == null))
		{
			this.lootContainer = new TileEntityLootContainer(null);
			this.lootContainer.bTouched = false;
			this.lootContainer.entityId = this.entityId;
			this.lootContainer.lootListName = this.GetLootList();
			this.lootContainer.SetContainerSize(LootContainer.GetLootContainer(this.lootContainer.lootListName, true).size, true);
			if (this.isInventory != null)
			{
				this.lootContainer.bTouched = true;
				for (int i = 0; i < this.isInventory.Length; i++)
				{
					if (!this.isInventory[i].IsEmpty())
					{
						this.lootContainer.AddItem(this.isInventory[i]);
					}
				}
			}
			this.lootContainer.SetModified();
			this.forceInventoryCreate = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBackpack()
	{
		this.deathUpdateTime = this.timeStayAfterDeath;
		this.bRemoved = true;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		if (_strength >= 99999)
		{
			this.removeBackpack();
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override bool IsMarkedForUnload()
	{
		return base.IsMarkedForUnload() && this.bRemoved;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createMesh()
	{
	}

	public override string GetLootList()
	{
		return this.lootListOnDeath;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write((ushort)((this.isInventory != null) ? this.isInventory.Length : 0));
		int num = 0;
		while (this.isInventory != null && num < this.isInventory.Length)
		{
			this.isInventory[num].Write(_bw);
			num++;
		}
		_bw.Write((ushort)((this.isBag != null) ? this.isBag.Length : 0));
		int num2 = 0;
		while (this.isBag != null && num2 < this.isBag.Length)
		{
			this.isBag[num2].Write(_bw);
			num2++;
		}
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		int num = (int)_br.ReadUInt16();
		this.isInventory = new ItemStack[num];
		for (int i = 0; i < num; i++)
		{
			this.isInventory[i] = new ItemStack();
			this.isInventory[i].Read(_br);
		}
		num = (int)_br.ReadUInt16();
		this.isBag = new ItemStack[num];
		for (int j = 0; j < num; j++)
		{
			this.isBag[j] = new ItemStack();
			this.isBag[j].Read(_br);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (EntityClass.list[this.entityClass].NavObject != "")
		{
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this, "", false);
		}
	}

	public override string ToString()
	{
		return string.Format("[type={0}, name={1}]", base.GetType().Name, (this.itemClass != null) ? this.itemClass.GetItemName() : "?");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemStack[] isInventory;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemStack[] isBag;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int deathUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int timeStayAfterDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bRemoved;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool forceInventoryCreate;
}
