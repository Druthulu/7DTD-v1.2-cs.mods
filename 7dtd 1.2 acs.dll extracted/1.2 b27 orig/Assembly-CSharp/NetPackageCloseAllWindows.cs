﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageCloseAllWindows : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageCloseAllWindows Setup(int entityToClose)
	{
		this._playerIdToClose = entityToClose;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this._playerIdToClose = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this._playerIdToClose);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(this._playerIdToClose);
			if (localPlayerFromID != null)
			{
				localPlayerFromID.PlayerUI.windowManager.CloseAllOpenWindows(null, false);
			}
		}
	}

	public override int GetLength()
	{
		return 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int _playerIdToClose = -1;
}
