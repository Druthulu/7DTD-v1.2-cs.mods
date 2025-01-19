using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.openTimeLeft = 0f;
		this.lootWindow = base.GetChildByType<XUiC_LootWindow>();
		this.timerWindow = base.xui.GetChildByType<XUiC_Timer>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public void SetTileEntityChest(string _lootContainerName, ITileEntityLootable _te)
	{
		this.lootContainerName = _lootContainerName;
		this.te = _te;
		this.lootWindow.SetTileEntityChest(_lootContainerName, _te);
		this.lootingHeader = Localization.Get("xuiLooting", false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OpenContainer()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("backpack", false, false, true);
		this.lootWindow.ViewComponent.UiTransform.gameObject.SetActive(true);
		this.lootWindow.OpenContainer();
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(this.lootingHeader);
		}
		this.lootWindow.ViewComponent.IsVisible = true;
		base.xui.playerUI.windowManager.Close("timer");
		if (this.windowGroup.UseStackPanelAlignment)
		{
			base.xui.RecenterWindowGroup(this.windowGroup, false);
		}
		this.isOpening = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.hasBeenAttackedTime > 0 && this.isOpening)
		{
			GUIWindowManager windowManager = base.xui.playerUI.windowManager;
			windowManager.Close("timer");
			this.isOpening = false;
			this.isClosingFromDamage = true;
			windowManager.Close("looting");
			return;
		}
		if (this.isOpening)
		{
			if (this.te.bWasTouched || this.openTimeLeft <= 0f)
			{
				if (!this.te.bWasTouched && !this.te.bPlayerStorage && !this.te.bPlayerBackpack)
				{
					base.xui.playerUI.entityPlayer.Progression.AddLevelExp(base.xui.playerUI.entityPlayer.gameStage, "_xpFromLoot", Progression.XPTypes.Looting, true, true);
				}
				this.openTimeLeft = 0f;
				this.OpenContainer();
				return;
			}
			if (this.timerWindow != null)
			{
				float fillAmount = this.openTimeLeft / this.totalOpenTime;
				this.timerWindow.UpdateTimer(this.openTimeLeft, fillAmount);
			}
			this.openTimeLeft -= _dt;
		}
	}

	public override void OnOpen()
	{
		this.isClosingFromDamage = false;
		if (this.te.EntityId != -1)
		{
			Entity entity = GameManager.Instance.World.GetEntity(this.te.EntityId);
			if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, base.xui.playerUI.entityPlayer, null, entity.EntityClass.Tags, true, true, true, true, true, 1, true, false) > 0f)
			{
				Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
				GUIWindowManager windowManager = base.xui.playerUI.windowManager;
				this.ignoreCloseSound = true;
				windowManager.Close("timer");
				this.isOpening = false;
				this.isClosingFromDamage = true;
				windowManager.Close("looting");
				return;
			}
		}
		else if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, base.xui.playerUI.entityPlayer, null, this.te.blockValue.Block.Tags, true, true, true, true, true, 1, true, false) > 0f)
		{
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			GUIWindowManager windowManager2 = base.xui.playerUI.windowManager;
			this.ignoreCloseSound = true;
			windowManager2.Close("timer");
			this.isOpening = false;
			this.isClosingFromDamage = true;
			windowManager2.Close("looting");
			return;
		}
		this.ignoreCloseSound = false;
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		this.lootWindow.ViewComponent.UiTransform.gameObject.SetActive(false);
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		float openTime = LootContainer.GetLootContainer(this.te.lootListName, true).openTime;
		this.totalOpenTime = (this.openTimeLeft = EffectManager.GetValue(PassiveEffects.ScavengingTime, null, entityPlayer.IsCrouching ? (openTime * 1.5f) : openTime, entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", false, false, true);
		base.xui.playerUI.windowManager.Open("timer", false, false, true);
		this.timerWindow = base.xui.GetChildByType<XUiC_Timer>();
		this.timerWindow.currentOpenEventText = Localization.Get("xuiOpeningLoot", false);
		this.isOpening = true;
		LootContainer lootContainer = LootContainer.GetLootContainer(this.te.lootListName, true);
		if (lootContainer != null && lootContainer.soundClose != null)
		{
			Vector3 position = this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
			if (this.te.EntityId != -1 && GameManager.Instance.World != null)
			{
				Entity entity2 = GameManager.Instance.World.GetEntity(this.te.EntityId);
				if (entity2 != null)
				{
					position = entity2.GetPosition();
				}
			}
			Manager.BroadcastPlayByLocalPlayer(position, lootContainer.soundOpen);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		Vector3i blockPos = this.te.ToWorldPos();
		if (this.isOpening)
		{
			base.xui.playerUI.windowManager.Close("timer");
		}
		if (this.openTimeLeft > 0f && !this.te.bWasTouched && GameManager.Instance.World.GetTileEntity(this.te.GetClrIdx(), blockPos).GetSelfOrFeature<ITileEntityLootable>() == this.te)
		{
			this.te.bTouched = false;
			this.te.SetModified();
		}
		this.lootWindow.CloseContainer(this.ignoreCloseSound);
		this.lootWindow.ViewComponent.IsVisible = false;
		this.isOpening = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_LootWindowGroup GetInstance(XUi _xuiInstance = null)
	{
		if (_xuiInstance == null)
		{
			_xuiInstance = LocalPlayerUI.GetUIForPrimaryPlayer().xui;
		}
		return (XUiC_LootWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_LootWindowGroup.ID);
	}

	public static Vector3i GetTeBlockPos(XUi _xuiInstance = null)
	{
		ITileEntityLootable tileEntityLootable = XUiC_LootWindowGroup.GetInstance(_xuiInstance).te;
		if (tileEntityLootable == null)
		{
			return Vector3i.zero;
		}
		return tileEntityLootable.ToWorldPos();
	}

	public static void CloseIfOpenAtPos(Vector3i _blockPos, XUi _xuiInstance = null)
	{
		GUIWindowManager windowManager = XUiC_LootWindowGroup.GetInstance(_xuiInstance).xui.playerUI.windowManager;
		if (windowManager.IsWindowOpen(XUiC_LootWindowGroup.ID) && XUiC_LootWindowGroup.GetTeBlockPos(null) == _blockPos)
		{
			windowManager.Close(XUiC_LootWindowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LootWindow lootWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable te;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootContainerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpening;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Timer timerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public UISprite timerHourGlass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosingFromDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootingHeader;

	public static string ID = "looting";

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalOpenTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreCloseSound;
}
