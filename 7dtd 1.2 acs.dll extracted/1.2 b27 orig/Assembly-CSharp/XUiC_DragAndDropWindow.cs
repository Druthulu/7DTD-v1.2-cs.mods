using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DragAndDropWindow : XUiController
{
	public EntityPlayer entityPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUi xui = base.xui;
			if (xui == null)
			{
				return null;
			}
			LocalPlayerUI playerUI = xui.playerUI;
			if (playerUI == null)
			{
				return null;
			}
			return playerUI.entityPlayer;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem()
	{
		base.xui.PlayerInventory.DropItem(this.CurrentStack);
		this.CurrentStack = ItemStack.Empty.Clone();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem(int _count)
	{
		if (_count < this.CurrentStack.count)
		{
			ItemStack itemStack = this.CurrentStack.Clone();
			itemStack.count = _count;
			this.CurrentStack.count -= _count;
			base.xui.PlayerInventory.DropItem(itemStack);
			this.CurrentStack = this.itemStack;
			return;
		}
		this.DropCurrentItem();
	}

	public ItemStack CurrentStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			XUiC_ItemStack itemStackControl = this.ItemStackControl;
			bool flag = XUiC_ItemStack.IsStackLocationFromPlayer((itemStackControl != null) ? new XUiC_ItemStack.StackLocationTypes?(itemStackControl.StackLocation) : null);
			this.itemStack = value;
			this.ItemStackControl.ItemStack = value;
			if (flag)
			{
				EntityPlayerLocal entityPlayerLocal = this.entityPlayer as EntityPlayerLocal;
				if (entityPlayerLocal != null)
				{
					entityPlayerLocal.DragAndDropItem = value;
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.ItemStackControl = base.GetChildByType<XUiC_ItemStack>();
		this.ItemStackControl.IsDragAndDrop = true;
		this.ItemStackControl.ItemStack = ItemStack.Empty.Clone();
		base.ViewComponent.IsSnappable = false;
	}

	public override void Update(float _dt)
	{
		if (!this.InMenu)
		{
			this.PlaceItemBackInInventory();
		}
		if (this.itemStack != null && !this.itemStack.IsEmpty())
		{
			((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
			Vector2 screenPosition = base.xui.playerUI.CursorController.GetScreenPosition();
			Vector3 position = base.xui.playerUI.camera.ScreenToWorldPoint(screenPosition);
			Transform transform = base.xui.transform;
			position.z = transform.position.z - 3f * transform.lossyScale.z;
			base.ViewComponent.UiTransform.position = position;
		}
		else
		{
			((XUiV_Window)base.ViewComponent).Panel.alpha = 0f;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		EntityPlayerLocal entityPlayerLocal = this.entityPlayer as EntityPlayerLocal;
		if (entityPlayerLocal != null && entityPlayerLocal.DragAndDropItem != ItemStack.Empty)
		{
			this.CurrentStack = entityPlayerLocal.DragAndDropItem;
			this.PlaceItemBackInInventory();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.PlaceItemBackInInventory();
	}

	public void PlaceItemBackInInventory()
	{
		if (!this.CurrentStack.IsEmpty())
		{
			if (base.xui.PlayerInventory.AddItem(this.itemStack))
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
				this.CurrentStack = ItemStack.Empty.Clone();
				return;
			}
			base.xui.PlayerInventory.DropItem(this.itemStack);
			this.CurrentStack = ItemStack.Empty.Clone();
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "place_sound")
		{
			base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
			{
				this.placeSound = o;
			});
			return true;
		}
		return base.ParseAttribute(name, value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack ItemStackControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = new ItemStack(new ItemValue(0, false), 0);

	public bool InMenu;

	public XUiC_ItemStack.StackLocationTypes PickUpType;
}
