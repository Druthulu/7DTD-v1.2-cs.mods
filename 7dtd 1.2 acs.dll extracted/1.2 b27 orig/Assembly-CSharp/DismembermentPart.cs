using System;
using UnityEngine;

public class DismembermentPart
{
	public DismembermentPart(DismemberedPartData _data, uint _bodyDamageFlag, EnumDamageTypes _damageType)
	{
		this.data = _data;
		this.prefabPath = this.data.prefabPath;
		this.bodyDamageFlag = _bodyDamageFlag;
		this.damageType = _damageType;
	}

	public DismemberedPartData Data
	{
		get
		{
			return this.data;
		}
	}

	public void SetObj(Transform _t)
	{
		this.objT = _t;
		this.obj = _t.gameObject;
	}

	public void SetTarget(Transform _t)
	{
		this.targetT = _t;
	}

	public Transform objT { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public Transform targetT { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public void Hide()
	{
		this.obj.SetActive(false);
	}

	public uint bodyDamageFlag { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public EnumDamageTypes damageType { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public string prefabPath { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public bool useMask
	{
		get
		{
			return this.data.useMask;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DismemberedPartData data;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject obj;
}
