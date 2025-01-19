using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Audio
{
	[Preserve]
	public class NetPackageAudio : NetPackage
	{
		public NetPackageAudio Setup(int _playOnEntityId, string _soundGroupName, float _occlusion, bool _play, bool _signalOnly = false)
		{
			this.playOnEntity = true;
			this.playOnEntityId = _playOnEntityId;
			this.soundGroupName = _soundGroupName;
			this.play = _play;
			this.occlusion = _occlusion;
			this.signalOnly = _signalOnly;
			return this;
		}

		public NetPackageAudio Setup(Vector3 _position, string _soundGroupName, float _occlusion, bool _play, int entityId = -1)
		{
			this.playOnEntity = false;
			this.position = _position;
			this.playOnEntityId = entityId;
			this.soundGroupName = _soundGroupName;
			this.play = _play;
			this.occlusion = _occlusion;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			this.playOnEntityId = _reader.ReadInt32();
			this.soundGroupName = _reader.ReadString();
			this.play = _reader.ReadBoolean();
			float x = _reader.ReadSingle();
			float y = _reader.ReadSingle();
			float z = _reader.ReadSingle();
			this.position.x = x;
			this.position.y = y;
			this.position.z = z;
			this.playOnEntity = _reader.ReadBoolean();
			this.occlusion = _reader.ReadSingle();
			this.signalOnly = _reader.ReadBoolean();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(this.playOnEntityId);
			_writer.Write((this.soundGroupName != null) ? this.soundGroupName : "");
			_writer.Write(this.play);
			_writer.Write(this.position.x);
			_writer.Write(this.position.y);
			_writer.Write(this.position.z);
			_writer.Write(this.playOnEntity);
			_writer.Write(this.occlusion);
			_writer.Write(this.signalOnly);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(this.soundGroupName))
			{
				return;
			}
			if (this.playOnEntity && this.playOnEntityId >= 0)
			{
				Entity entity = _world.GetEntity(this.playOnEntityId);
				if (entity == null)
				{
					return;
				}
				if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
				{
					if (this.play)
					{
						Manager.ServerAudio.Play(entity, this.soundGroupName, this.occlusion, this.signalOnly);
						return;
					}
					Manager.ServerAudio.Stop(this.playOnEntityId, this.soundGroupName);
					return;
				}
				else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
				{
					if (this.play)
					{
						Manager.Play(entity, this.soundGroupName, 1f, false);
						Manager.ServerAudio.Play(entity, this.soundGroupName, this.occlusion, this.signalOnly);
						return;
					}
					Manager.Stop(this.playOnEntityId, this.soundGroupName);
					Manager.ServerAudio.Stop(this.playOnEntityId, this.soundGroupName);
					return;
				}
				else if (Manager.ServerAudio == null)
				{
					if (!this.play)
					{
						Manager.Stop(this.playOnEntityId, this.soundGroupName);
						return;
					}
					if (!this.signalOnly)
					{
						Manager.Play(entity, this.soundGroupName, 1f, false);
						return;
					}
				}
			}
			else if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (this.play)
				{
					Manager.ServerAudio.Play(this.position, this.soundGroupName, this.occlusion, this.playOnEntityId);
					return;
				}
				Manager.ServerAudio.Stop(this.position, this.soundGroupName);
				return;
			}
			else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (this.play)
				{
					Manager.Play(this.position, this.soundGroupName, this.playOnEntityId);
					Manager.ServerAudio.Play(this.position, this.soundGroupName, this.occlusion, this.playOnEntityId);
					return;
				}
				Manager.Stop(this.position, this.soundGroupName);
				Manager.ServerAudio.Stop(this.position, this.soundGroupName);
				return;
			}
			else if (Manager.ServerAudio == null)
			{
				if (this.play)
				{
					Manager.Play(this.position, this.soundGroupName, this.playOnEntityId);
					return;
				}
				Manager.Stop(this.position, this.soundGroupName);
			}
		}

		public override int GetLength()
		{
			return 10;
		}

		public int playOnEntityId;

		public string soundGroupName;

		public bool play;

		public Vector3 position;

		public bool playOnEntity;

		public float occlusion;

		public bool signalOnly;
	}
}
