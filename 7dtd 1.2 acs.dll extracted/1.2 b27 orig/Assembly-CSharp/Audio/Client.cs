using System;
using UnityEngine;

namespace Audio
{
	public class Client : IDisposable
	{
		public Client(int _entityId)
		{
			this.entityId = _entityId;
		}

		public void Dispose()
		{
		}

		public void Play(int playOnEntityId, string soundGoupName, float _occlusion)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(playOnEntityId, soundGoupName, _occlusion, true, false);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, this.entityId, -1, -1, null, 192);
		}

		public void Play(Vector3 position, string soundGoupName, float _occlusion, int entityId = -1)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGoupName, _occlusion, true, entityId);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, this.entityId, -1, -1, null, 192);
		}

		public void Stop(int stopOnEntityId, string soundGroupName)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(stopOnEntityId, soundGroupName, 0f, false, false);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, this.entityId, -1, -1, null, 192);
		}

		public void Stop(Vector3 position, string soundGroupName)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, 0f, false, -1);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, this.entityId, -1, -1, null, 192);
		}

		public int entityId;
	}
}
