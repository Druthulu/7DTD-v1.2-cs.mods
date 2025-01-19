using System;
using System.Globalization;
using UnityEngine;

public class XUiV_Button : XUiView
{
	public XUiV_Button(string _id) : base(_id)
	{
		this.UseSelectionBox = false;
	}

	public UISprite Sprite
	{
		get
		{
			return this.sprite;
		}
	}

	public string UIAtlas
	{
		get
		{
			return this.uiAtlas;
		}
		set
		{
			this.uiAtlas = value;
			this.isDirty = true;
		}
	}

	public UIBasicSprite.Type Type
	{
		get
		{
			return this.type;
		}
		set
		{
			this.type = value;
			this.isDirty = true;
		}
	}

	public string DefaultSpriteName
	{
		get
		{
			return this.defaultSpriteName;
		}
		set
		{
			this.defaultSpriteName = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public Color DefaultSpriteColor
	{
		get
		{
			return this.defaultSpriteColor;
		}
		set
		{
			this.defaultSpriteColor = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public string HoverSpriteName
	{
		get
		{
			if (this.hoverSpriteName == "")
			{
				return this.defaultSpriteName;
			}
			return this.hoverSpriteName;
		}
		set
		{
			this.hoverSpriteName = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public Color HoverSpriteColor
	{
		get
		{
			return this.hoverSpriteColor;
		}
		set
		{
			this.hoverSpriteColor = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public string SelectedSpriteName
	{
		get
		{
			if (this.selectedSpriteName == "")
			{
				return this.defaultSpriteName;
			}
			return this.selectedSpriteName;
		}
		set
		{
			this.selectedSpriteName = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public Color SelectedSpriteColor
	{
		get
		{
			return this.selectedSpriteColor;
		}
		set
		{
			this.selectedSpriteColor = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public string DisabledSpriteName
	{
		get
		{
			if (this.disabledSpriteName == "")
			{
				return this.defaultSpriteName;
			}
			return this.disabledSpriteName;
		}
		set
		{
			this.disabledSpriteName = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public Color DisabledSpriteColor
	{
		get
		{
			return this.disabledSpriteColor;
		}
		set
		{
			this.disabledSpriteColor = value;
			this.isDirty = true;
			this.updateCurrentSprite();
		}
	}

	public bool ManualColors
	{
		get
		{
			return this.manualColors;
		}
		set
		{
			if (value != this.manualColors)
			{
				this.manualColors = value;
				this.isDirty = true;
				this.updateCurrentSprite();
			}
		}
	}

	public UIBasicSprite.Flip Flip
	{
		get
		{
			return this.sprite.flip;
		}
		set
		{
			if (this.flip != value)
			{
				this.flip = value;
				this.isDirty = true;
			}
		}
	}

	public Color CurrentColor
	{
		get
		{
			return this.currentColor;
		}
		set
		{
			this.currentColor = value;
			this.isDirty = true;
			this.colorDirty = true;
		}
	}

	public string CurrentSpriteName
	{
		get
		{
			return this.currentSpriteName;
		}
		set
		{
			if (value != this.currentSpriteName)
			{
				this.currentSpriteName = value;
				this.isDirty = true;
				this.colorDirty = true;
			}
		}
	}

	public bool Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (this.selected != value)
			{
				this.selected = value;
				this.isDirty = true;
				this.updateCurrentSprite();
			}
		}
	}

	public float GlobalOpacityModifier
	{
		get
		{
			return this.globalOpacityModifier;
		}
		set
		{
			this.globalOpacityModifier = value;
			this.isDirty = true;
		}
	}

	public bool ForegroundLayer
	{
		get
		{
			return this.foregroundLayer;
		}
		set
		{
			if (this.foregroundLayer != value)
			{
				this.foregroundLayer = value;
				this.isDirty = true;
			}
		}
	}

	public float HoverScale
	{
		get
		{
			return this.hoverScale;
		}
		set
		{
			this.hoverScale = value;
			this.isDirty = true;
		}
	}

	public override bool Enabled
	{
		set
		{
			bool enabled = this.enabled;
			base.Enabled = value;
			if (value != enabled)
			{
				this.updateCurrentSprite();
				if (!value && this.hoverScale != 1f && this.tweenScale.value != Vector3.one)
				{
					this.tweenScale.SetStartToCurrentValue();
					this.tweenScale.to = Vector3.one;
					this.tweenScale.enabled = true;
					this.tweenScale.duration = 0.25f;
					this.tweenScale.ResetToBeginning();
				}
				if (!this.gamepadSelectableSetFromAttributes)
				{
					base.IsNavigatable = value;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentSprite()
	{
		if (!this.Enabled)
		{
			if (!this.manualColors)
			{
				this.CurrentColor = this.disabledSpriteColor;
			}
			this.CurrentSpriteName = this.DisabledSpriteName;
			return;
		}
		if (this.Selected)
		{
			if (!this.manualColors)
			{
				this.CurrentColor = this.selectedSpriteColor;
			}
			this.CurrentSpriteName = this.SelectedSpriteName;
			return;
		}
		if (!this.manualColors)
		{
			this.CurrentColor = (this.isOver ? this.hoverSpriteColor : this.defaultSpriteColor);
		}
		this.CurrentSpriteName = (this.isOver ? this.HoverSpriteName : this.DefaultSpriteName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
		_go.AddComponent<TweenScale>().enabled = false;
	}

	public override void InitView()
	{
		this.EventOnPress = true;
		this.EventOnHover = true;
		base.InitView();
		this.sprite = this.uiTransform.GetComponent<UISprite>();
		this.UpdateData();
		this.initialized = true;
		this.Enabled = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (null != this.sprite)
		{
			bool isVisible = this.sprite.isVisible;
			if (this.lastVisible != isVisible)
			{
				this.isDirty = true;
			}
			this.lastVisible = isVisible;
			if (this.isOver && UICamera.hoveredObject != this.uiTransform.gameObject)
			{
				this.OnHover(this.uiTransform.gameObject, false);
			}
		}
	}

	public override void UpdateData()
	{
		this.sprite.spriteName = this.currentSpriteName;
		this.sprite.atlas = base.xui.GetAtlasByName(this.uiAtlas, this.currentSpriteName);
		this.sprite.color = this.currentColor;
		if (this.globalOpacityModifier != 0f && (this.foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
		{
			float a = Mathf.Clamp01(this.currentColor.a * (this.globalOpacityModifier * (this.foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
			this.sprite.color = new Color(this.currentColor.r, this.currentColor.g, this.currentColor.b, a);
		}
		if (this.borderSize > 0)
		{
			this.sprite.border = new Vector4((float)this.borderSize, (float)this.borderSize, (float)this.borderSize, (float)this.borderSize);
		}
		this.sprite.centerType = UIBasicSprite.AdvancedType.Sliced;
		this.sprite.type = this.type;
		base.parseAnchors(this.sprite, true);
		if (this.sprite.flip != this.flip)
		{
			this.sprite.flip = this.flip;
		}
		if (this.hoverScale != 1f && this.tweenScale == null)
		{
			this.tweenScale = this.uiTransform.gameObject.GetComponent<TweenScale>();
		}
		if (!this.initialized)
		{
			this.sprite.pivot = this.pivot;
			this.sprite.depth = this.depth;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
			BoxCollider collider = this.collider;
			collider.center = this.sprite.localCenter;
			collider.size = new Vector3(this.sprite.localSize.x * this.colliderScale, this.sprite.localSize.y * this.colliderScale, 0f);
		}
		if (this.sprite.isAnchored)
		{
			this.sprite.autoResizeBoxCollider = true;
		}
		else
		{
			this.RefreshBoxCollider();
		}
		base.UpdateData();
	}

	public override void OnHover(GameObject _go, bool _isOver)
	{
		base.OnHover(_go, _isOver);
		this.updateCurrentSprite();
		if (this.Enabled && this.hoverScale != 1f)
		{
			this.tweenScale.to = (this.isOver ? (Vector3.one * this.hoverScale) : Vector3.one);
			this.tweenScale.SetStartToCurrentValue();
			this.tweenScale.duration = 0.25f;
			this.tweenScale.ResetToBeginning();
			this.tweenScale.enabled = true;
		}
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
			if (num <= 2179094556U)
			{
				if (num <= 358097113U)
				{
					if (num <= 45741760U)
					{
						if (num != 19667905U)
						{
							if (num == 45741760U)
							{
								if (attribute == "hovercolor")
								{
									this.HoverSpriteColor = StringParsers.ParseColor32(value);
									return true;
								}
							}
						}
						else if (attribute == "defaultcolor")
						{
							this.DefaultSpriteColor = StringParsers.ParseColor32(value);
							this.CurrentColor = this.defaultSpriteColor;
							return true;
						}
					}
					else if (num != 311159388U)
					{
						if (num == 358097113U)
						{
							if (attribute == "hoverscale")
							{
								this.HoverScale = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
								return true;
							}
						}
					}
					else if (attribute == "disabledsprite")
					{
						this.DisabledSpriteName = value;
						return true;
					}
				}
				else if (num <= 1361572173U)
				{
					if (num != 1309284212U)
					{
						if (num == 1361572173U)
						{
							if (attribute == "type")
							{
								this.Type = EnumUtils.Parse<UIBasicSprite.Type>(value, true);
								return true;
							}
						}
					}
					else if (attribute == "selected")
					{
						this.Selected = StringParsers.ParseBool(value, 0, -1, true);
						return true;
					}
				}
				else if (num != 2006771483U)
				{
					if (num == 2179094556U)
					{
						if (attribute == "sprite")
						{
							this.DefaultSpriteName = value;
							this.CurrentSpriteName = value;
							return true;
						}
					}
				}
				else if (attribute == "selectedcolor")
				{
					this.SelectedSpriteColor = StringParsers.ParseColor32(value);
					return true;
				}
			}
			else if (num <= 3060355671U)
			{
				if (num <= 2245939092U)
				{
					if (num != 2207167896U)
					{
						if (num == 2245939092U)
						{
							if (attribute == "hoversprite")
							{
								this.HoverSpriteName = value;
								return true;
							}
						}
					}
					else if (attribute == "disabledcolor")
					{
						this.DisabledSpriteColor = StringParsers.ParseColor32(value);
						return true;
					}
				}
				else if (num != 2629038627U)
				{
					if (num == 3060355671U)
					{
						if (attribute == "globalopacity")
						{
							if (!StringParsers.ParseBool(value, 0, -1, true))
							{
								this.GlobalOpacityModifier = 0f;
								return true;
							}
							return true;
						}
					}
				}
				else if (attribute == "manualcolors")
				{
					this.ManualColors = StringParsers.ParseBool(value, 0, -1, true);
					return true;
				}
			}
			else if (num <= 3194542381U)
			{
				if (num != 3134659686U)
				{
					if (num == 3194542381U)
					{
						if (attribute == "selectedsprite")
						{
							this.SelectedSpriteName = value;
							return true;
						}
					}
				}
				else if (attribute == "flip")
				{
					this.Flip = EnumUtils.Parse<UIBasicSprite.Flip>(value, true);
					return true;
				}
			}
			else if (num != 3407328204U)
			{
				if (num != 3458780677U)
				{
					if (num == 4144336821U)
					{
						if (attribute == "globalopacitymod")
						{
							this.GlobalOpacityModifier = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
							return true;
						}
					}
				}
				else if (attribute == "foregroundlayer")
				{
					this.foregroundLayer = StringParsers.ParseBool(value, 0, -1, true);
					return true;
				}
			}
			else if (attribute == "atlas")
			{
				this.UIAtlas = value;
				return true;
			}
			return false;
		}
		return flag;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.updateCurrentSprite();
		this.uiTransform.localScale = Vector3.one;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string defaultSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string hoverSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string selectedSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string disabledSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color defaultSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color hoverSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color selectedSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color disabledSpriteColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool manualColors;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color currentColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string currentSpriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool colorDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hoverScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool foregroundLayer = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int borderSize = -1;
}
