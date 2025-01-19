using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapInvitesList : XUiController
{
	public Waypoint SelectedInvite
	{
		get
		{
			return this.selectedInvite;
		}
		set
		{
			this.selectedInvite = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.waypointSetBtn = base.Parent.Parent.GetChildById("waypointSetBtn");
		this.waypointSetBtn.OnPress += this.onInviteAddToWaypoints;
		this.waypointShowOnMapBtn = base.Parent.Parent.GetChildById("showOnMapBtn");
		this.waypointShowOnMapBtn.OnPress += this.onInviteShowOnMapPressed;
		this.waypointRemoveBtn = base.Parent.Parent.GetChildById("waypointRemoveBtn");
		this.waypointRemoveBtn.OnPress += this.onInviteRemovePressed;
		this.waypointReportBtn = base.Parent.Parent.GetChildById("waypointReportBtn");
		this.waypointReportBtn.OnPress += this.onReportWaypointPressed;
		this.list = (XUiV_Grid)base.GetChildById("invitesList").ViewComponent;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.UpdateInvitesList();
	}

	public void UpdateInvitesList()
	{
		for (int i = 0; i < this.list.Rows; i++)
		{
			XUiC_MapInvitesListEntry xuiC_MapInvitesListEntry = (XUiC_MapInvitesListEntry)this.children[i];
			if (xuiC_MapInvitesListEntry != null)
			{
				xuiC_MapInvitesListEntry.Index = i;
				xuiC_MapInvitesListEntry.Sprite.SpriteName = string.Empty;
				xuiC_MapInvitesListEntry.Name.Text = string.Empty;
				xuiC_MapInvitesListEntry.Distance.Text = string.Empty;
				xuiC_MapInvitesListEntry.Selected = false;
				xuiC_MapInvitesListEntry.Waypoint = null;
				xuiC_MapInvitesListEntry.Background.SoundPlayOnClick = false;
			}
		}
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		List<Waypoint> waypointInvites = entityPlayer.WaypointInvites;
		int num = 0;
		for (int j = 0; j < 4; j++)
		{
			int num2 = j;
			if (num2 >= waypointInvites.Count)
			{
				break;
			}
			XUiC_MapInvitesListEntry inviteEntry = (XUiC_MapInvitesListEntry)this.children[num];
			if (inviteEntry != null)
			{
				inviteEntry.Index = j;
				inviteEntry.Sprite.SpriteName = waypointInvites[num2].icon;
				inviteEntry.Waypoint = waypointInvites[num2];
				if (inviteEntry.Waypoint.bIsAutoWaypoint)
				{
					inviteEntry.Name.Text = Localization.Get(inviteEntry.Waypoint.name.Text, false);
				}
				else
				{
					GeneratedTextManager.GetDisplayText(inviteEntry.Waypoint.name, delegate(string _filtered)
					{
						inviteEntry.Name.Text = _filtered;
					}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				}
				inviteEntry.Selected = false;
				inviteEntry.Background.SoundPlayOnClick = true;
				Vector3 a = waypointInvites[num2].pos.ToVector3();
				Vector3 position = entityPlayer.GetPosition();
				a.y = 0f;
				position.y = 0f;
				float num3 = (a - position).magnitude;
				string arg = "m";
				if (num3 >= 1000f)
				{
					num3 /= 1000f;
					arg = "km";
				}
				inviteEntry.Distance.Text = string.Format("{0} {1}", num3.ToCultureInvariantString("0.0"), arg);
				num++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteAddToWaypoints(XUiController _sender, int _mouseButton)
	{
		if (this.SelectedInvite != null)
		{
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			entityPlayer.WaypointInvites.Remove(this.SelectedInvite);
			entityPlayer.Waypoints.Collection.Add(this.SelectedInvite);
			this.SelectedInviteEntry.Selected = false;
			this.SelectedInvite.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", this.SelectedInvite.pos.ToVector3(), this.SelectedInvite.icon, false, null);
			this.SelectedInvite.navObject.IsActive = false;
			this.SelectedInvite.navObject.usingLocalizationId = this.SelectedInvite.bUsingLocalizationId;
			GeneratedTextManager.GetDisplayText(this.SelectedInvite.name, delegate(string _filtered)
			{
				this.SelectedInvite.navObject.name = _filtered;
			}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			XUiV_Window window = base.xui.GetWindow("mapTracking");
			if (window != null && window.IsVisible)
			{
				((XUiC_MapWaypointList)window.Controller.GetChildById("waypointList")).UpdateWaypointsList(null);
			}
			this.SelectedInvite = null;
			this.SelectedInviteEntry = null;
			this.UpdateInvitesList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteShowOnMapPressed(XUiController _sender, int _mouseButton)
	{
		if (this.SelectedInvite != null)
		{
			((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).PositionMapAt(this.SelectedInvite.pos.ToVector3());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteRemovePressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.SelectedInvite != null)
		{
			entityPlayer.WaypointInvites.Remove(this.SelectedInvite);
			this.SelectedInviteEntry.Selected = false;
			this.SelectedInvite = null;
			this.SelectedInviteEntry = null;
			this.UpdateInvitesList();
			Manager.PlayInsidePlayerHead("ui_waypoint_delete", -1, 0f, false, false);
			return;
		}
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onReportWaypointPressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.SelectedInvite != null && !this.SelectedInvite.bIsAutoWaypoint && PlatformManager.MultiPlatform.PlayerReporting != null)
		{
			PersistentPlayerData ppData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.SelectedInvite.InviterEntityId);
			if (ppData != null)
			{
				GeneratedTextManager.GetDisplayText(this.SelectedInvite.name, delegate(string _filtered)
				{
					ThreadManager.AddSingleTaskMainThread("OpenReportWindow", delegate(object _)
					{
						XUiC_ReportPlayer.Open(ppData.PlayerData, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage", false), _filtered), "");
					}, null);
				}, true, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			}
			return;
		}
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint selectedInvite;

	public XUiC_MapInvitesListEntry SelectedInviteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid list;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointSetBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointShowOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointReportBtn;
}
