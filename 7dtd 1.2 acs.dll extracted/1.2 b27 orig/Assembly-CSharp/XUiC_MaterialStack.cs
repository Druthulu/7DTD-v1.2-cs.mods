using System;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_MaterialStack : XUiC_SelectableEntry
{
	public bool IsLocked { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public BlockTextureData TextureData
	{
		get
		{
			return this.textureData;
		}
		set
		{
			this.textMaterial.IsVisible = false;
			base.ViewComponent.Enabled = (value != null);
			if (this.textureData != value)
			{
				this.textureData = value;
				this.isDirty = true;
				if (this.textureData == null)
				{
					this.SetItemNameText("");
					this.IsLocked = false;
				}
				else
				{
					this.textMaterial.IsVisible = true;
					MeshDescription meshDescription = MeshDescription.meshes[0];
					int textureID = (int)this.textureData.TextureID;
					Rect uvrect;
					if (textureID == 0)
					{
						uvrect = WorldConstants.uvRectZero;
					}
					else
					{
						uvrect = meshDescription.textureAtlas.uvMapping[textureID].uv;
					}
					this.textMaterial.Texture = meshDescription.textureAtlas.diffuseTexture;
					if (meshDescription.bTextureArray)
					{
						this.textMaterial.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
						this.textMaterial.Material.SetFloat("_Index", (float)meshDescription.textureAtlas.uvMapping[textureID].index);
						this.textMaterial.Material.SetFloat("_Size", (float)meshDescription.textureAtlas.uvMapping[textureID].blockW);
					}
					else
					{
						this.textMaterial.UVRect = uvrect;
					}
					this.SetItemNameText(string.Format("({0}) {1}", this.textureData.ID, this.textureData.LocalizedName));
				}
			}
			if (this.textureData != null)
			{
				if (!(this.textureData.LockedByPerk != ""))
				{
					this.IsLocked = false;
				}
				this.textMaterial.IsVisible = true;
			}
			base.RefreshBindings(false);
		}
	}

	public XUiC_MaterialInfoWindow InfoWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			this.SetColor(this.selectColor);
			if (base.xui.currentSelectedEntry == this)
			{
				this.InfoWindow.SetMaterial(this.textureData);
				return;
			}
		}
		else
		{
			this.SetColor(XUiC_MaterialStack.backgroundColor);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetColor(Color32 color)
	{
		this.background.Color = color;
	}

	public override void Init()
	{
		base.Init();
		this.tintedOverlay = base.GetChildById("tintedOverlay");
		this.highlightOverlay = (base.GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite);
		this.background = (base.GetChildById("background").ViewComponent as XUiV_Sprite);
		this.textMaterial = (base.GetChildById("textMaterial").ViewComponent as XUiV_Texture);
		this.textMaterial.CreateMaterial();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
			cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (this.isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (guiactions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = guiactions.Submit.WasReleased;
					bool wasReleased2 = guiactions.HalfStack.WasReleased;
					bool wasReleased3 = guiactions.Inspect.WasReleased;
					bool wasReleased4 = guiactions.RightStick.WasReleased;
					if (wasReleased && this.textureData != null)
					{
						this.SetSelectedTextureForItem();
					}
				}
				else if (mouseButtonUp && this.textureData != null)
				{
					this.SetSelectedTextureForItem();
				}
			}
			else
			{
				this.currentColor = XUiC_MaterialStack.backgroundColor;
				if (this.highlightOverlay != null)
				{
					this.highlightOverlay.Color = XUiC_MaterialStack.backgroundColor;
				}
				if (!base.Selected)
				{
					this.background.Color = this.currentColor;
				}
				this.lastClicked = false;
				if (this.isOver)
				{
					this.isOver = false;
				}
			}
		}
		if (this.isDirty)
		{
			this.isDirty = false;
		}
	}

	public void SetSelectedTextureForItem()
	{
		if (!this.IsLocked)
		{
			if (base.xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)
			{
				base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureAllSides = this.textureData.ID;
			}
			else
			{
				((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx = this.textureData.ID;
				base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.Meta = (int)((byte)this.textureData.ID);
				base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1].invData.itemValue = base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue;
			}
		}
		base.Selected = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleItemInspect()
	{
		if (this.textureData != null && this.InfoWindow != null)
		{
			base.Selected = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetItemNameText(string name)
	{
		this.viewComponent.ToolTip = ((this.textureData != null) ? name : string.Empty);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!base.Selected)
		{
			if (_isOver)
			{
				this.background.Color = XUiC_MaterialStack.highlightColor;
			}
			else
			{
				this.background.Color = XUiC_MaterialStack.backgroundColor;
			}
		}
		base.OnHovered(_isOver);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.RefreshBindings(false);
	}

	public void ClearSelectedInfoWindow()
	{
		if (base.Selected)
		{
			this.InfoWindow.SetMaterial(null);
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "islocked")
		{
			value = this.IsLocked.ToString();
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "select_color"))
			{
				if (!(name == "press_color"))
				{
					if (!(name == "background_color"))
					{
						if (!(name == "highlight_color"))
						{
							if (!(name == "select_sound"))
							{
								return false;
							}
							base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
							{
								this.selectSound = o;
							});
						}
						else
						{
							XUiC_MaterialStack.highlightColor = StringParsers.ParseColor32(value);
						}
					}
					else
					{
						XUiC_MaterialStack.backgroundColor = StringParsers.ParseColor32(value);
					}
				}
				else
				{
					this.pressColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.selectColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip selectSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTextureData textureData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;
}
