using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerName : XUiController
{
	public Color Color
	{
		get
		{
			return this.lblName.Color;
		}
		set
		{
			this.lblName.Color = value;
			this.lblNameCrossplay.Color = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.rect = (XUiV_Rect)base.GetChildById("playerName").ViewComponent;
		this.lblName = (XUiV_Label)base.GetChildById("name").ViewComponent;
		this.lblNameCrossplay = (XUiV_Label)base.GetChildById("nameCrossplay").ViewComponent;
		this.sprIconCrossplay = (XUiV_Sprite)base.GetChildById("iconCrossplay").ViewComponent;
		this.rect.IsNavigatable = false;
		this.rect.IsSnappable = false;
		base.OnPress += this.PlayerName_OnPress;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnPress -= this.PlayerName_OnPress;
	}

	public void SetGenericName(string _name)
	{
		this.UpdatePlayerData(null, false, _name);
	}

	public void UpdatePlayerData(PlayerData _playerData, bool _showCrossplay, string _displayName = null)
	{
		this.PlayerData = _playerData;
		bool flag = false;
		if (_displayName != null)
		{
			XUiV_Label xuiV_Label = this.lblName;
			this.lblNameCrossplay.Text = _displayName;
			xuiV_Label.Text = _displayName;
			flag = true;
		}
		else if (this.PlayerData != null)
		{
			GeneratedTextManager.GetDisplayText(this.PlayerData.PlayerName, delegate(string name)
			{
				XUiV_Label xuiV_Label2 = this.lblName;
				this.lblNameCrossplay.Text = name;
				xuiV_Label2.Text = name;
			}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			flag = true;
		}
		this.rect.EventOnPress = (flag && this.CanShowProfile());
		this.rect.IsSnappable = flag;
		this.rect.IsNavigatable = flag;
		if (_showCrossplay && this.PlayerData != null && this.PlayerData.PlayGroup != EPlayGroup.Unknown)
		{
			this.sprIconCrossplay.SpriteName = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(this.PlayerData.PlayGroup, true, this.PlayerData.NativeId.PlatformIdentifier);
			this.sprIconCrossplay.UIAtlas = "SymbolAtlas";
			this.sprIconCrossplay.IsVisible = true;
			this.lblName.IsVisible = false;
			this.lblNameCrossplay.IsVisible = true;
		}
		else
		{
			this.sprIconCrossplay.IsVisible = false;
			this.lblName.IsVisible = true;
			this.lblNameCrossplay.IsVisible = false;
		}
		base.RefreshBindings(false);
	}

	public void ClearPlayerData()
	{
		this.PlayerData = null;
		this.lblName.Text = string.Empty;
		this.lblNameCrossplay.Text = string.Empty;
		this.sprIconCrossplay.IsVisible = false;
	}

	public bool CanShowProfile()
	{
		return this.PlayerData != null && ((this.PlayerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.PlayerData.NativeId)) || (this.PlayerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.PlayerData.PrimaryId)));
	}

	public void ShowProfile()
	{
		if (this.PlayerData == null)
		{
			return;
		}
		if (this.PlayerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.PlayerData.NativeId))
		{
			PlatformManager.MultiPlatform.User.ShowProfile(this.PlayerData.NativeId);
			return;
		}
		if (this.PlayerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.PlayerData.PrimaryId))
		{
			PlatformManager.MultiPlatform.User.ShowProfile(this.PlayerData.PrimaryId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerName_OnPress(XUiController _sender, int _mousebutton)
	{
		this.ShowProfile();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect rect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblNameCrossplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprIconCrossplay;

	public PlayerData PlayerData;
}
