using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAnimationData : NetPackage, IMemoryPoolableObject
{
	public NetPackageEntityAnimationData Setup(int _entityId, IList<AnimParamData> _animationParameterData)
	{
		this.entityId = _entityId;
		_animationParameterData.CopyTo(this.animationParameterData);
		return this;
	}

	public NetPackageEntityAnimationData Setup(int _entityId, Dictionary<int, AnimParamData> _animationParameterData)
	{
		this.entityId = _entityId;
		_animationParameterData.CopyValuesTo(this.animationParameterData);
		return this;
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.animationParameterData.Count);
		for (int i = 0; i < this.animationParameterData.Count; i++)
		{
			this.animationParameterData[i].Write(_writer);
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.animationParameterData.Add(AnimParamData.CreateFromBinary(_reader));
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive == null || !entityAlive.isEntityRemote)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(this.entityId, this.animationParameterData), false, -1, this.entityId, this.entityId, null, 192);
		}
		if (entityAlive.emodel == null)
		{
			return;
		}
		AvatarController avatarController = entityAlive.emodel.avatarController;
		if (avatarController == null)
		{
			return;
		}
		for (int i = 0; i < this.animationParameterData.Count; i++)
		{
			int nameHash = this.animationParameterData[i].NameHash;
			switch (this.animationParameterData[i].ValueType)
			{
			case AnimParamData.ValueTypes.Bool:
				avatarController.UpdateBool(nameHash, this.animationParameterData[i].IntValue != 0, true);
				break;
			case AnimParamData.ValueTypes.Trigger:
				if (this.animationParameterData[i].IntValue != 0)
				{
					avatarController.TriggerEvent(nameHash);
				}
				else
				{
					avatarController.CancelEvent(nameHash);
				}
				break;
			case AnimParamData.ValueTypes.Float:
				avatarController.UpdateFloat(nameHash, this.animationParameterData[i].FloatValue, true);
				break;
			case AnimParamData.ValueTypes.Int:
				avatarController.UpdateInt(nameHash, this.animationParameterData[i].IntValue, true);
				break;
			case AnimParamData.ValueTypes.DataFloat:
				avatarController.SetDataFloat((AvatarController.DataTypes)nameHash, this.animationParameterData[i].FloatValue, true);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	public void Reset()
	{
		this.entityId = 0;
		this.animationParameterData.Clear();
	}

	public void Cleanup()
	{
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IList<AnimParamData> animationParameterData = new List<AnimParamData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CustomSampler getSampler = CustomSampler.Create("NetPackageEntityAnimationData.read", false);
}
