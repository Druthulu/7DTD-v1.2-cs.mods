using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockable : TEFeatureAbs, ILockable
{
	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockpickFeature = base.Parent.GetFeature<ILockPickable>();
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ILockable feature = _other.GetFeature<ILockable>();
		if (feature != null)
		{
			this.locked = feature.IsLocked();
			this.allowedUserIds.AddRange(feature.GetUsers());
			this.passwordHash = feature.GetPassword();
		}
	}

	public bool IsLocked()
	{
		return this.locked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.locked = _isLocked;
		base.SetModified();
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return base.Parent.Owner;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		base.Parent.SetOwner(_userIdentifier);
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return this.IsOwner(_userIdentifier) || this.allowedUserIds.Contains(_userIdentifier) || (_userIdentifier.Equals(PlatformManager.InternalLocalUserIdentifier) && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(base.Parent.Owner);
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.passwordHash);
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool _changed)
	{
		_changed = false;
		string a = _password.GetStableHashCode().ToString("X8");
		if (this.IsOwner(_userIdentifier))
		{
			if (a != this.passwordHash)
			{
				_changed = true;
				this.passwordHash = a;
				this.allowedUserIds.Clear();
				base.SetModified();
			}
			return true;
		}
		if (a == this.passwordHash)
		{
			this.allowedUserIds.Add(_userIdentifier);
			base.SetModified();
			return true;
		}
		return false;
	}

	public string GetPassword()
	{
		return this.passwordHash;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (!this.IsLocked())
		{
			return string.Format(Localization.Get("tooltipUnlocked", false), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.lockpickFeature == null && !this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed", false), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return string.Format(Localization.Get("tooltipLocked", false), _activateHotkeyMarkup, _focusedTileEntityName);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("lock", "lock", false, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("unlock", "unlock", false, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("keypad", "keypad", false, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(base.Parent.Owner);
		HashSet<PlatformUserIdentifierAbs> hashSet = (playerData != null) ? playerData.ACL : null;
		bool flag = !this.LocalPlayerIsOwner() && hashSet != null && hashSet.Contains(internalLocalUserIdentifier);
		if (base.CommandIs(_commandName, "lock"))
		{
			_command.enabled = (!this.IsLocked() && (this.LocalPlayerIsOwner() || flag));
			return;
		}
		if (base.CommandIs(_commandName, "unlock"))
		{
			_command.enabled = (this.IsLocked() && this.LocalPlayerIsOwner());
			return;
		}
		if (base.CommandIs(_commandName, "keypad"))
		{
			_command.enabled = ((!this.IsUserAllowed(internalLocalUserIdentifier) && this.HasPassword() && this.IsLocked()) || this.LocalPlayerIsOwner());
			return;
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (base.CommandIs(_commandName, "lock"))
		{
			this.SetLocked(true);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
			GameManager.ShowTooltip(_player, "containerLocked", false);
			return true;
		}
		if (base.CommandIs(_commandName, "unlock"))
		{
			this.SetLocked(false);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
			GameManager.ShowTooltip(_player, "containerUnlocked", false);
			return true;
		}
		if (base.CommandIs(_commandName, "keypad"))
		{
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			if (uiforPlayer != null)
			{
				XUiC_KeypadWindow.Open(uiforPlayer, this);
			}
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
		base.Read(_br, _eStreamMode, _readVersion);
		this.locked = _br.ReadBoolean();
		int num = _br.ReadInt32();
		this.allowedUserIds.Clear();
		for (int i = 0; i < num; i++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
		this.passwordHash = _br.ReadString();
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		_bw.Write(this.locked);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
		_bw.Write(this.passwordHash);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlatformUserIdentifierAbs> allowedUserIds = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string passwordHash = "";
}
