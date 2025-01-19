using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureSignable : TEFeatureAbs, ITileEntitySignable, ITileEntity
{
	public TEFeatureSignable()
	{
		PlatformUserManager.BlockedStateChanged += this.UserBlockedStateChanged;
	}

	public override void OnUnload(World _world)
	{
		PlatformUserManager.BlockedStateChanged -= this.UserBlockedStateChanged;
	}

	public override void OnDestroy()
	{
		PlatformUserManager.BlockedStateChanged -= this.UserBlockedStateChanged;
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockFeature = base.Parent.GetFeature<ILockable>();
		DynamicProperties props = _featureData.Props;
		props.ParseInt("LineCount", ref this.lineCount);
		props.ParseFloat("LineWidth", ref this.lineWidth);
		props.ParseInt("FontSize", ref this.fontSize);
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform)
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		float num = 0.8f;
		Block.MultiBlockArray multiBlockPos = base.Parent.TeData.Block.multiBlockPos;
		float num2 = num * (float)((multiBlockPos != null) ? multiBlockPos.dim.x : 1);
		float maxWidthReal = (this.lineWidth > 0f) ? this.lineWidth : num2;
		TextMesh[] componentsInChildren = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		this.smartTextMesh = new SmartTextMesh[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].fontSize = this.fontSize;
			this.smartTextMesh[i] = componentsInChildren[i].gameObject.AddComponent<SmartTextMesh>();
			this.smartTextMesh[i].MaxWidthReal = maxWidthReal;
			this.smartTextMesh[i].MaxLines = this.lineCount;
			this.smartTextMesh[i].ConvertNewLines = true;
		}
		string text;
		if ((text = this.displayText) == null)
		{
			AuthoredText authoredText = this.signText;
			text = ((authoredText != null) ? authoredText.Text : null);
		}
		this.RefreshTextMesh(text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserBlockedStateChanged(IPlatformUserData _userData, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (!_userData.PrimaryId.Equals(this.signText.Author) || _blockType != EBlockType.TextChat)
		{
			return;
		}
		GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshTextMesh(string _text)
	{
		this.displayText = _text;
		if (GameManager.IsDedicatedServer || this.smartTextMesh == null || this.displayText == this.smartTextMesh[0].UnwrappedText)
		{
			return;
		}
		for (int i = 0; i < this.smartTextMesh.Length; i++)
		{
			this.smartTextMesh[i].UnwrappedText = this.displayText;
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ITileEntitySignable feature = _other.GetFeature<ITileEntitySignable>();
		if (feature != null)
		{
			this.signText = feature.GetAuthoredText();
			if (this.signText != null)
			{
				GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
			}
		}
	}

	public virtual void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		this.SetText((_authoredText != null) ? _authoredText.Text : null, _syncData, (_authoredText != null) ? _authoredText.Author : null);
	}

	public virtual void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
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
			base.SetModified();
		}
	}

	public virtual AuthoredText GetAuthoredText()
	{
		return this.signText;
	}

	public virtual bool CanRenderString(string _text)
	{
		return this.smartTextMesh != null && this.smartTextMesh.Length != 0 && this.smartTextMesh[0].CanRenderString(_text);
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		return Localization.Get("useWorkstation", false);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("edit", "pen", true, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("report", "report", true, false), TileEntityComposite.EBlockCommandOrder.Last, base.FeatureData);
	}

	public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
		if (base.CommandIs(_commandName, "edit"))
		{
			_command.enabled = (this.lockFeature == null || GameManager.Instance.IsEditMode() || !this.lockFeature.IsLocked() || this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier));
			return;
		}
		if (base.CommandIs(_commandName, "report"))
		{
			PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
			bool flag = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(this.signText.Text) && !internalLocalUserIdentifier.Equals(this.signText.Author);
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(this.signText.Author);
			bool flag2 = playerData != null && playerData.PlatformData.Blocked[EBlockType.TextChat].IsBlocked();
			_command.enabled = (flag && !flag2);
			return;
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (base.CommandIs(_commandName, "edit"))
		{
			_player.AimingGun = false;
			Vector3i blockPos = base.Parent.ToWorldPos();
			_world.GetGameManager().TELockServer(0, blockPos, base.Parent.EntityId, _player.entityId, "sign");
			return true;
		}
		if (base.CommandIs(_commandName, "report"))
		{
			GeneratedTextManager.GetDisplayText(this.signText, delegate(string _filtered)
			{
				ThreadManager.AddSingleTaskMainThread("OpenReportWindow", delegate(object _)
				{
					PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(this.signText.Author);
					XUiC_ReportPlayer.Open((playerData != null) ? playerData.PlayerData : null, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage", false), _filtered), "");
				}, null);
			}, true, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
		base.Read(_br, _eStreamMode, _readVersion);
		this.SetText(AuthoredText.FromStream(_br), false);
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		AuthoredText.ToStream(this.signText, _bw);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText = new AuthoredText();

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSize = 132;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lineWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayText;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh[] smartTextMesh;
}
