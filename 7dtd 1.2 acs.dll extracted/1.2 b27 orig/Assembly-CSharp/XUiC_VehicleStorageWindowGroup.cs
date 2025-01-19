using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleStorageWindowGroup : XUiController
{
	public EntityVehicle CurrentVehicleEntity
	{
		get
		{
			return this.currentVehicleEntity;
		}
		set
		{
			base.xui.vehicle = value;
			this.currentVehicleEntity = value;
			this.containerWindow.SetSlots(value.bag.GetSlots());
		}
	}

	public override void Init()
	{
		base.Init();
		this.containerWindow = base.GetChildByType<XUiC_VehicleContainer>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public override void Update(float _dt)
	{
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
		if (this.currentVehicleEntity != null && !this.currentVehicleEntity.CheckUIInteraction())
		{
			base.xui.playerUI.windowManager.Close(XUiC_VehicleStorageWindowGroup.ID);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(Localization.Get("xuiStorage", false));
		}
		ITileEntityLootable lootContainer = this.CurrentVehicleEntity.lootContainer;
		if (lootContainer != null)
		{
			LootContainer lootContainer2 = LootContainer.GetLootContainer(lootContainer.lootListName, true);
			if (lootContainer2 != null && lootContainer2.soundClose != null)
			{
				Vector3 position = lootContainer.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
				if (lootContainer.EntityId != -1 && GameManager.Instance.World != null)
				{
					Entity entity = GameManager.Instance.World.GetEntity(lootContainer.EntityId);
					if (entity != null)
					{
						position = entity.GetPosition();
					}
				}
				Manager.BroadcastPlayByLocalPlayer(position, lootContainer2.soundOpen);
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.wasReleased = false;
		this.activeKeyDown = false;
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		this.CurrentVehicleEntity.StopUIInteraction();
		base.xui.vehicle = null;
		ITileEntityLootable lootContainer = this.CurrentVehicleEntity.lootContainer;
		if (lootContainer != null)
		{
			LootContainer lootContainer2 = LootContainer.GetLootContainer(lootContainer.lootListName, true);
			if (lootContainer2 != null && lootContainer2.soundClose != null)
			{
				Vector3 position = lootContainer.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
				if (lootContainer.EntityId != -1 && GameManager.Instance.World != null)
				{
					Entity entity = GameManager.Instance.World.GetEntity(lootContainer.EntityId);
					if (entity != null)
					{
						position = entity.GetPosition();
					}
				}
				Manager.BroadcastPlayByLocalPlayer(position, lootContainer2.soundClose);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VehicleContainer containerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "vehicleStorage";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentVehicleEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
