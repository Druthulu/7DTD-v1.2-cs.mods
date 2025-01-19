using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childByType = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			this.nonPagingHeader = (XUiC_WindowNonPagingHeader)childByType;
		}
		childByType = base.GetChildByType<XUiC_PowerSourceStats>();
		if (childByType != null)
		{
			this.GeneratorStats = (XUiC_PowerSourceStats)childByType;
			this.GeneratorStats.Owner = this;
		}
		childByType = base.GetChildByType<XUiC_PowerSourceSlots>();
		if (childByType != null)
		{
			this.PowerSourceSlots = (XUiC_PowerSourceSlots)childByType;
			this.PowerSourceSlots.Owner = this;
		}
	}

	public TileEntityPowerSource TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.GeneratorStats.TileEntity = this.tileEntity;
			this.PowerSourceSlots.TileEntity = this.tileEntity;
		}
	}

	public void SetOn(bool isOn)
	{
		if (this.PowerSourceSlots != null && this.PowerSourceSlots.ViewComponent.IsVisible)
		{
			this.PowerSourceSlots.SetOn(isOn);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.windowGroup.isShowing)
		{
			if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				this.wasReleased = true;
			}
			if (this.wasReleased)
			{
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					this.activeKeyDown = true;
				}
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && this.activeKeyDown)
				{
					this.activeKeyDown = false;
					if (!base.xui.playerUI.windowManager.IsInputActive())
					{
						base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
					}
				}
			}
		}
	}

	public override bool AlwaysUpdate()
	{
		return false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (this.nonPagingHeader != null)
		{
			string header = "";
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			switch (this.TileEntity.PowerItemType)
			{
			case PowerItem.PowerItemTypes.Generator:
				header = Localization.Get("generatorbank", false);
				break;
			case PowerItem.PowerItemTypes.SolarPanel:
				header = Localization.Get("solarbank", false);
				break;
			case PowerItem.PowerItemTypes.BatteryBank:
				header = Localization.Get("batterybank", false);
				break;
			}
			this.nonPagingHeader.SetHeader(header);
		}
		base.xui.RecenterWindowGroup(this.windowGroup, false);
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		if (this.PowerSourceSlots != null && this.TileEntity != null)
		{
			this.PowerSourceSlots.OnOpen();
		}
		Manager.BroadcastPlayByLocalPlayer(this.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "open_vending");
		this.IsDirty = true;
		this.TileEntity.Destroyed += this.TileEntity_Destroyed;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.wasReleased = false;
		this.activeKeyDown = false;
		Manager.BroadcastPlayByLocalPlayer(this.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "close_vending");
		this.TileEntity.Destroyed -= this.TileEntity_Destroyed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (this.TileEntity == te)
		{
			if (GameManager.Instance != null)
			{
				base.xui.playerUI.windowManager.Close("powersource");
				return;
			}
		}
		else
		{
			te.Destroyed -= this.TileEntity_Destroyed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceStats GeneratorStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceSlots PowerSourceSlots;

	public static string ID = "powersource";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;
}
