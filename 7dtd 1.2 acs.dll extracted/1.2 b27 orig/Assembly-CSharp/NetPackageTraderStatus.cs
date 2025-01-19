using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTraderStatus : NetPackage
{
	public NetPackageTraderStatus Setup(int _traderId, bool _isOpen = false)
	{
		this.traderId = _traderId;
		this.isOpen = _isOpen;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.traderId = _br.ReadInt32();
		this.isOpen = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.traderId);
		_bw.Write(this.isOpen);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityTrader entityTrader = GameManager.Instance.World.GetEntity(this.traderId) as EntityTrader;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageTraderStatus package = NetPackageManager.GetPackage<NetPackageTraderStatus>();
			package.Setup(this.traderId, entityTrader.traderArea == null || !entityTrader.traderArea.IsClosed);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, base.Sender.entityId, -1, -1, null, 192);
			return;
		}
		entityTrader.ActivateTrader(this.isOpen);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int traderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;
}
