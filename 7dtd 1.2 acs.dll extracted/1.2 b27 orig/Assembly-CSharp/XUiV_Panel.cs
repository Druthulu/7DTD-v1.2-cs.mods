using System;
using UnityEngine;

public class XUiV_Panel : XUiView
{
	public Color BackgroundColor
	{
		get
		{
			return this.backgroundColor;
		}
		set
		{
			this.backgroundColor = value;
			this.isDirty = true;
			this.useBackground = true;
		}
	}

	public string BackgroundSpriteName
	{
		get
		{
			return this.backgroundSpriteName;
		}
		set
		{
			this.backgroundSpriteName = value;
			if (value == "")
			{
				this.backgroundSpriteName = XUi.BlankTexture;
			}
		}
	}

	public Color BorderColor
	{
		get
		{
			return this.borderColor;
		}
		set
		{
			this.borderColor = value;
			this.isDirty = true;
			this.useBackground = true;
		}
	}

	public XUi_Thickness BorderThickness
	{
		get
		{
			return this.borderThickness;
		}
		set
		{
			this.borderThickness = value;
			this.isDirty = true;
			this.useBackground = true;
		}
	}

	public UIDrawCall.Clipping Clipping
	{
		get
		{
			return this.clipping;
		}
		set
		{
			if (value != this.clipping)
			{
				this.clipping = value;
				this.isDirty = true;
			}
		}
	}

	public Vector2 ClippingSize
	{
		get
		{
			return this.clippingSize;
		}
		set
		{
			if (value != this.clippingSize)
			{
				this.clippingSize = value;
				this.isDirty = true;
			}
		}
	}

	public Vector2 ClippingCenter
	{
		get
		{
			return this.clippingCenter;
		}
		set
		{
			if (value != this.clippingCenter)
			{
				this.clippingCenter = value;
				this.isDirty = true;
			}
		}
	}

	public Vector2 ClippingSoftness
	{
		get
		{
			return this.clippingSoftness;
		}
		set
		{
			if (value != this.clippingSoftness)
			{
				this.clippingSoftness = value;
				this.isDirty = true;
			}
		}
	}

	public override bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			this.enabled = value;
			this.RefreshEnabled();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshEnabled()
	{
		if (this.panel != null)
		{
			this.panel.gameObject.SetActive(this.enabled);
			this.panel.enabled = this.enabled;
			this.borderSprite.Enabled = this.enabled;
			this.backgroundSprite.Enabled = this.enabled;
		}
	}

	public bool UseGlobalBackgroundOpacity { get; set; }

	public XUiV_Panel(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	public override void InitView()
	{
		this.borderSprite = new XUiV_Sprite("_border");
		this.borderSprite.xui = base.xui;
		this.borderSprite.Controller = new XUiController(base.Controller);
		this.borderSprite.Controller.xui = base.xui;
		this.backgroundSprite = new XUiV_Sprite("_background");
		this.backgroundSprite.xui = base.xui;
		this.backgroundSprite.Controller = new XUiController(base.Controller);
		this.backgroundSprite.Controller.xui = base.xui;
		if (!this.disableAutoBackground && this.useBackground)
		{
			this.borderSprite.GlobalOpacityModifier = 0f;
			this.borderSprite.Position = new Vector2i(this.size.x / 2, -this.size.y / 2);
			this.borderSprite.SetDefaults(base.Controller);
			this.borderSprite.Size = this.size;
			this.borderSprite.UIAtlas = "UIAtlas";
			this.borderSprite.SpriteName = XUi.BlankTexture;
			this.borderSprite.Color = this.borderColor;
			this.borderSprite.Depth = this.depth;
			this.borderSprite.Pivot = UIWidget.Pivot.TopLeft;
			this.borderSprite.Type = UIBasicSprite.Type.Sliced;
			this.borderSprite.Controller.WindowGroup = base.Controller.WindowGroup;
			this.backgroundSprite.Position = new Vector2i(this.backgroundSprite.Size.x / 2 + this.borderThickness.left, -(this.backgroundSprite.Size.y / 2 + this.borderThickness.top));
			this.backgroundSprite.SetDefaults(base.Controller);
			this.backgroundSprite.Size = this.size;
			this.backgroundSprite.UIAtlas = "UIAtlas";
			this.backgroundSprite.SpriteName = this.backgroundSpriteName;
			this.backgroundSprite.Color = this.backgroundColor;
			this.backgroundSprite.Depth = this.depth;
			this.backgroundSprite.Pivot = UIWidget.Pivot.TopLeft;
			this.backgroundSprite.Type = UIBasicSprite.Type.Sliced;
			if (this.backgroundSpriteName != "")
			{
				this.backgroundSprite.GlobalOpacityModifier = 2f;
			}
			this.backgroundSprite.Controller.WindowGroup = base.Controller.WindowGroup;
			if (this.borderColor != new Color32(0, 0, 0, 0))
			{
				this.backgroundSprite.Size = new Vector2i(base.Size.x - (this.borderThickness.left + this.borderThickness.right), base.Size.y - (this.borderThickness.top + this.borderThickness.bottom));
			}
		}
		base.InitView();
		this.panel = this.uiTransform.gameObject.GetComponent<UIPanel>();
		if (!this.createUiPanel && this.clipping == UIDrawCall.Clipping.None)
		{
			UnityEngine.Object.Destroy(this.panel);
			this.panel = null;
		}
		else
		{
			if (this.createUiPanel)
			{
				this.panel.enabled = true;
				this.panel.depth = this.depth;
			}
			if (this.clipping != UIDrawCall.Clipping.None)
			{
				this.panel.enabled = true;
				if (this.clippingCenter == new Vector2(-10000f, -10000f))
				{
					this.clippingCenter = new Vector2((float)(this.size.x / 2), (float)(-(float)this.size.y / 2));
				}
				if (this.clippingSize == new Vector2(-10000f, -10000f))
				{
					this.clippingSize = new Vector2((float)this.size.x, (float)this.size.y);
				}
				this.updateClipping();
			}
		}
		BoxCollider collider = this.collider;
		if (collider != null)
		{
			float x = (float)this.size.x * 0.5f;
			float num = (float)this.size.y * 0.5f;
			collider.center = new Vector3(x, -num, 0f);
			collider.size = new Vector3((float)this.size.x * this.colliderScale, (float)this.size.y * this.colliderScale, 0f);
		}
		if (!this.disableAutoBackground && this.useBackground && this.backgroundSprite != null && this.borderSprite != null)
		{
			this.backgroundSprite.Color = this.backgroundColor;
			this.borderSprite.Color = this.borderColor;
		}
		this.RefreshEnabled();
		this.isDirty = true;
		this.UseGlobalBackgroundOpacity = true;
	}

	public override void UpdateData()
	{
		if (this.isDirty)
		{
			if (!this.disableAutoBackground && this.useBackground && this.backgroundSprite != null)
			{
				this.borderSprite.FillCenter = false;
				this.borderSprite.Size = this.size;
				this.borderSprite.Position = new Vector2i(0, 0);
				this.borderSprite.Color = this.borderColor;
				this.borderSprite.GlobalOpacityModifier = (float)(this.UseGlobalBackgroundOpacity ? 1 : 0);
				this.backgroundSprite.Size = this.size;
				this.backgroundSprite.Position = new Vector2i(0, 0);
				this.backgroundSprite.Color = this.backgroundColor;
				this.backgroundSprite.SpriteName = this.backgroundSpriteName;
				this.backgroundSprite.GlobalOpacityModifier = (float)(this.UseGlobalBackgroundOpacity ? 1 : 0);
				if (this.borderColor != new Color32(0, 0, 0, 0))
				{
					this.backgroundSprite.Size = new Vector2i(base.Size.x - (this.borderThickness.left + this.borderThickness.right), base.Size.y - (this.borderThickness.top + this.borderThickness.bottom));
					this.backgroundSprite.Position = new Vector2i(this.borderThickness.left, -this.borderThickness.top);
				}
			}
			if (this.panel != null)
			{
				this.panel.depth = this.depth;
			}
			this.updateClipping();
		}
		base.UpdateData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateClipping()
	{
		if (this.clipping != UIDrawCall.Clipping.None)
		{
			if (this.panel.clipping != this.clipping)
			{
				this.panel.clipping = this.clipping;
			}
			if (this.panel.clipSoftness != this.clippingSoftness)
			{
				this.panel.clipSoftness = this.clippingSoftness;
			}
			if (this.clippingSize.x < 0f)
			{
				this.clippingSize.x = 0f;
			}
			if (this.clippingSize.y < 0f)
			{
				this.clippingSize.y = 0f;
			}
			Vector4 vector = new Vector4(this.clippingCenter.x, this.clippingCenter.y, this.clippingSize.x, this.clippingSize.y);
			if (this.panel.baseClipRegion != vector)
			{
				this.panel.baseClipRegion = vector;
			}
		}
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.backgroundColor = new Color32(96, 96, 96, byte.MaxValue);
		this.borderColor = new Color32(0, 0, 0, 0);
		this.borderThickness = new XUi_Thickness(3);
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			flag = true;
			uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
			if (num <= 1092012428U)
			{
				if (num <= 625300902U)
				{
					if (num != 256789003U)
					{
						if (num == 625300902U)
						{
							if (attribute == "clippingsize")
							{
								this.clippingSize = StringParsers.ParseVector2(value);
								return flag;
							}
						}
					}
					else if (attribute == "createuipanel")
					{
						this.createUiPanel = StringParsers.ParseBool(value, 0, -1, true);
						return flag;
					}
				}
				else if (num != 727013168U)
				{
					if (num != 954579279U)
					{
						if (num == 1092012428U)
						{
							if (attribute == "disableautobackground")
							{
								this.disableAutoBackground = StringParsers.ParseBool(value, 0, -1, true);
								return flag;
							}
						}
					}
					else if (attribute == "clipping")
					{
						this.clipping = EnumUtils.Parse<UIDrawCall.Clipping>(value, true);
						return flag;
					}
				}
				else if (attribute == "backgroundcolor")
				{
					this.BackgroundColor = StringParsers.ParseColor32(value);
					return flag;
				}
			}
			else if (num <= 1694002274U)
			{
				if (num != 1650154374U)
				{
					if (num != 1657231807U)
					{
						if (num == 1694002274U)
						{
							if (attribute == "clippingcenter")
							{
								this.clippingCenter = StringParsers.ParseVector2(value);
								return flag;
							}
						}
					}
					else if (attribute == "backgroundspritename")
					{
						this.BackgroundSpriteName = value;
						return flag;
					}
				}
				else if (attribute == "bordercolor")
				{
					this.BorderColor = StringParsers.ParseColor32(value);
					return flag;
				}
			}
			else if (num != 2511885439U)
			{
				if (num != 2728752196U)
				{
					if (num == 3619465623U)
					{
						if (attribute == "snapcursor")
						{
							this.EventOnHover = true;
							return flag;
						}
					}
				}
				else if (attribute == "clippingsoftness")
				{
					this.clippingSoftness = StringParsers.ParseVector2(value);
					return flag;
				}
			}
			else if (attribute == "borderthickness")
			{
				this.BorderThickness = XUi_Thickness.Parse(value);
				return flag;
			}
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string backgroundSpriteName = XUi.BlankTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color borderColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUi_Thickness borderThickness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color backgroundColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useBackground = true;

	public bool createUiPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite borderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite backgroundSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIDrawCall.Clipping clipping;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 clippingSize = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 clippingCenter = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 clippingSoftness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool disableAutoBackground;
}
