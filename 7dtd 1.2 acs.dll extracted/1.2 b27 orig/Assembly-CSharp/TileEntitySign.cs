using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class TileEntitySign : TileEntity, ILockable, ITileEntitySignable, ITileEntity
{
	public TileEntitySign(Chunk _chunk) : base(_chunk)
	{
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		this.isLocked = true;
		this.ownerID = null;
		this.password = "";
		this.signText = new AuthoredText();
		PlatformUserManager.BlockedStateChanged += this.UserBlockedStateChanged;
	}

	public override void OnUnload(World world)
	{
		base.OnUnload(world);
		PlatformUserManager.BlockedStateChanged -= this.UserBlockedStateChanged;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		PlatformUserManager.BlockedStateChanged -= this.UserBlockedStateChanged;
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
		this.setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerID)) || this.allowedUserIds.Contains(_userIdentifier);
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.password);
	}

	public string GetPassword()
	{
		return this.password;
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(this.ownerID))
		{
			if (Utils.HashString(_password) != this.password)
			{
				changed = true;
				this.password = Utils.HashString(_password);
				this.allowedUserIds.Clear();
				this.setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == this.password)
		{
			this.allowedUserIds.Add(_userIdentifier);
			this.setModified();
			return true;
		}
		return false;
	}

	public void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData != null && _blockEntityData.bHasTransform && !GameManager.IsDedicatedServer)
		{
			this.textMesh = _blockEntityData.transform.GetComponentInChildren<TextMesh>();
			this.smartTextMesh = this.textMesh.transform.gameObject.AddComponent<SmartTextMesh>();
			float num = (float)_blockEntityData.blockValue.Block.multiBlockPos.dim.x;
			this.smartTextMesh.MaxWidth = 0.48f * num;
			this.smartTextMesh.MaxLines = this.lineCount;
			this.smartTextMesh.ConvertNewLines = true;
			AuthoredText authoredText = this.signText;
			this.RefreshTextMesh((authoredText != null) ? authoredText.Text : null);
		}
	}

	public virtual void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		this.SetText((_authoredText != null) ? _authoredText.Text : null, _syncData, (_authoredText != null) ? _authoredText.Author : null);
	}

	public void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
	{
		if (_signingPlayer == null)
		{
			_signingPlayer = PlatformManager.MultiPlatform.User.PlatformUserId;
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		if (((persistentPlayers != null) ? persistentPlayers.GetPlayerData(_signingPlayer) : null) == null)
		{
			_signingPlayer = null;
		}
		if (_text == this.signText.Text)
		{
			return;
		}
		this.signText.Update(_text, _signingPlayer);
		GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		if (_syncData)
		{
			this.setModified();
		}
	}

	public AuthoredText GetAuthoredText()
	{
		return this.signText;
	}

	public bool CanRenderString(string _text)
	{
		return this.smartTextMesh.CanRenderString(_text);
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		this.isLocked = _br.ReadBoolean();
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		if (num > 1)
		{
			this.SetText(AuthoredText.FromStream(_br), false);
		}
		this.password = _br.ReadString();
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
		if (num <= 1)
		{
			this.SetText(_br.ReadString(), false, null);
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(2);
		_bw.Write(this.isLocked);
		this.ownerID.ToStream(_bw, false);
		AuthoredText.ToStream(this.signText, _bw);
		_bw.Write(this.password);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
	}

	public override TileEntity Clone()
	{
		return new TileEntitySign(this.chunk)
		{
			localChunkPos = base.localChunkPos,
			isLocked = this.isLocked,
			ownerID = this.ownerID,
			password = this.password,
			allowedUserIds = new List<PlatformUserIdentifierAbs>(this.allowedUserIds),
			signText = AuthoredText.Clone(this.signText)
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntitySign tileEntitySign = (TileEntitySign)_other;
		base.localChunkPos = tileEntitySign.localChunkPos;
		this.isLocked = tileEntitySign.isLocked;
		this.ownerID = tileEntitySign.ownerID;
		this.password = tileEntitySign.password;
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>(tileEntitySign.allowedUserIds);
		this.signText = AuthoredText.Clone(tileEntitySign.signText);
	}

	public new int EntityId
	{
		get
		{
			return this.entityId;
		}
		set
		{
			this.entityId = value;
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Sign;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserBlockedStateChanged(IPlatformUserData userData, EBlockType blockType, EUserBlockState blockState)
	{
		if (!userData.PrimaryId.Equals(this.signText.Author) || blockType != EBlockType.TextChat)
		{
			return;
		}
		GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshTextMesh(string _text)
	{
		if (this.smartTextMesh != null && !GameManager.IsDedicatedServer)
		{
			this.smartTextMesh.UnwrappedText = _text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 2;

	public int lineCharWidth = 19;

	public int lineCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextMesh textMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh smartTextMesh;
}
