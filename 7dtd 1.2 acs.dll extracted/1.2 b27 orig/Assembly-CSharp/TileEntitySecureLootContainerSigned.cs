using System;
using Platform;
using UnityEngine;

public class TileEntitySecureLootContainerSigned : TileEntitySecureLootContainer, ITileEntitySignable, ITileEntity
{
	public TileEntitySecureLootContainerSigned(Chunk _chunk) : base(_chunk)
	{
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

	public void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData != null && _blockEntityData.bHasTransform && !GameManager.IsDedicatedServer)
		{
			Block.MultiBlockArray multiBlockPos = _blockEntityData.blockValue.Block.multiBlockPos;
			float num = (float)((multiBlockPos != null) ? multiBlockPos.dim.x : 1);
			TextMesh[] componentsInChildren = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				return;
			}
			this.smartTextMesh = new SmartTextMesh[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				this.smartTextMesh[i] = componentsInChildren[i].gameObject.AddComponent<SmartTextMesh>();
				this.smartTextMesh[i].MaxWidth = 0.4f * num;
				this.smartTextMesh[i].MaxLines = this.lineCount;
				this.smartTextMesh[i].ConvertNewLines = true;
			}
			AuthoredText authoredText = this.signText;
			this.RefreshTextMesh((authoredText != null) ? authoredText.Text : null);
		}
	}

	public void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		this.SetText((_authoredText != null) ? _authoredText.Text : null, _syncData, (_authoredText != null) ? _authoredText.Author : null);
	}

	public void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
	{
		if (_signingPlayer == null)
		{
			_signingPlayer = PlatformManager.MultiPlatform.User.PlatformUserId;
		}
		if (GameManager.Instance.persistentPlayers.GetPlayerData(_signingPlayer) == null)
		{
			_signingPlayer = null;
			_text = "";
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
		return this.smartTextMesh.Length != 0 && this.smartTextMesh[0].CanRenderString(_text);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.SecureLootSigned;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		_br.ReadBoolean();
		_br.ReadBoolean();
		PlatformUserIdentifierAbs.FromStream(_br, false, false);
		if (num > 1)
		{
			this.SetText(AuthoredText.FromStream(_br), false);
		}
		_br.ReadString();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			PlatformUserIdentifierAbs.FromStream(_br, false, false);
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
		_bw.Write(this.bPlayerPlaced);
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
		if (this.smartTextMesh != null && _text != this.smartTextMesh[0].UnwrappedText && !GameManager.IsDedicatedServer)
		{
			for (int i = 0; i < this.smartTextMesh.Length; i++)
			{
				this.smartTextMesh[i].UnwrappedText = _text;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int get_EntityId()
	{
		return base.EntityId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int ver = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText;

	public int lineCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh[] smartTextMesh;
}
