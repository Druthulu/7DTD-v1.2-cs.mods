using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddWallVolume : NetPackageEditorAddSleeperVolume
{
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			PrefabVolumeManager.Instance.AddTeleportVolumeServer(this.hitPointBlockPos);
		}
	}
}
