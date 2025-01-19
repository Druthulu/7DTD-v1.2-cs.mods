using System;
using System.Globalization;
using UnityEngine;

public class XUiV_Sprite : XUiView
{
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
			if (this.uiAtlas != value)
			{
				this.uiAtlas = value;
				this.uiAtlasChanged = true;
				this.isDirty = true;
			}
		}
	}

	public string SpriteName
	{
		get
		{
			return this.spriteName;
		}
		set
		{
			if (this.spriteName != value)
			{
				this.spriteName = value;
				this.isDirty = true;
			}
		}
	}

	public Color Color
	{
		get
		{
			return this.color;
		}
		set
		{
			if (this.color.r != value.r || this.color.g != value.g || this.color.b != value.b || this.color.a != value.a)
			{
				this.color = value;
				this.isDirty = true;
			}
		}
	}

	public virtual UIBasicSprite.Type Type
	{
		get
		{
			return this.type;
		}
		set
		{
			if (this.type != value)
			{
				this.type = value;
				this.isDirty = true;
			}
		}
	}

	public UIBasicSprite.FillDirection FillDirection
	{
		get
		{
			return this.fillDirection;
		}
		set
		{
			if (this.fillDirection != value)
			{
				this.fillDirection = value;
				this.isDirty = true;
			}
		}
	}

	public bool FillInvert
	{
		get
		{
			return this.fillInvert;
		}
		set
		{
			if (this.fillInvert != value)
			{
				this.fillInvert = value;
				this.isDirty = true;
			}
		}
	}

	public bool FillCenter
	{
		get
		{
			return this.fillCenter;
		}
		set
		{
			if (this.fillCenter != value)
			{
				this.fillCenter = value;
				this.isDirty = true;
			}
		}
	}

	public float Fill
	{
		get
		{
			return this.fillAmount;
		}
		set
		{
			if (this.fillAmount != value && (double)Math.Abs((value - this.fillAmount) / value) > 0.005)
			{
				this.fillAmount = Mathf.Clamp01(value);
				this.isDirty = true;
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

	public float GlobalOpacityModifier
	{
		get
		{
			return this.globalOpacityModifier;
		}
		set
		{
			if (this.globalOpacityModifier != value)
			{
				this.globalOpacityModifier = value;
				this.isDirty = true;
			}
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

	public XUiV_Sprite(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
	}

	public override void InitView()
	{
		base.InitView();
		this.sprite = this.uiTransform.GetComponent<UISprite>();
		this.UpdateData();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.GlobalOpacityChanged)
		{
			this.isDirty = true;
		}
		UISprite uisprite = this.sprite;
		if (uisprite != null)
		{
			bool isVisible = uisprite.isVisible;
			if (this.lastVisible != isVisible)
			{
				this.isDirty = true;
				this.lastVisible = isVisible;
			}
		}
	}

	public override void UpdateData()
	{
		bool initialized = this.initialized;
		this.applyAtlasAndSprite(false);
		this.sprite.keepAspectRatio = this.keepAspectRatio;
		this.sprite.aspectRatio = this.aspectRatio;
		if (this.sprite.color != this.color)
		{
			this.sprite.color = this.color;
		}
		if (this.globalOpacityModifier != 0f && (this.foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
		{
			float a = Mathf.Clamp01(this.color.a * (this.globalOpacityModifier * (this.foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
			this.sprite.color = new Color(this.color.r, this.color.g, this.color.b, a);
		}
		if (this.borderSize > 0 && this.sprite.border.x != (float)this.borderSize)
		{
			this.sprite.border = new Vector4((float)this.borderSize, (float)this.borderSize, (float)this.borderSize, (float)this.borderSize);
		}
		if (this.sprite.centerType != (this.fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible))
		{
			this.sprite.centerType = (this.fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
		}
		base.parseAnchors(this.sprite, true);
		if (this.sprite.fillDirection != this.fillDirection)
		{
			this.sprite.fillDirection = this.fillDirection;
		}
		if (this.sprite.invert != this.fillInvert)
		{
			this.sprite.invert = this.fillInvert;
		}
		if (this.sprite.fillAmount != this.fillAmount)
		{
			this.sprite.fillAmount = this.fillAmount;
		}
		if (this.sprite.type != this.type)
		{
			this.sprite.type = this.type;
		}
		if (this.sprite.flip != this.flip)
		{
			this.sprite.flip = this.flip;
		}
		if (!this.initialized)
		{
			this.initialized = true;
			this.sprite.pivot = this.pivot;
			this.sprite.depth = this.depth;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
			if (this.EventOnHover || this.EventOnPress)
			{
				BoxCollider collider = this.collider;
				collider.center = this.sprite.localCenter;
				collider.size = new Vector3(this.sprite.localSize.x * this.colliderScale, this.sprite.localSize.y * this.colliderScale, 0f);
			}
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

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.FillCenter = true;
		this.Type = UIBasicSprite.Type.Simple;
		this.FillDirection = UIBasicSprite.FillDirection.Horizontal;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
			if (num <= 3134659686U)
			{
				if (num <= 1993028337U)
				{
					if (num != 1031692888U)
					{
						if (num != 1361572173U)
						{
							if (num == 1993028337U)
							{
								if (attribute == "fillcenter")
								{
									this.FillCenter = StringParsers.ParseBool(value, 0, -1, true);
									return true;
								}
							}
						}
						else if (attribute == "type")
						{
							this.Type = EnumUtils.Parse<UIBasicSprite.Type>(value, true);
							return true;
						}
					}
					else if (attribute == "color")
					{
						this.Color = StringParsers.ParseColor32(value);
						return true;
					}
				}
				else if (num <= 2984927816U)
				{
					if (num != 2179094556U)
					{
						if (num == 2984927816U)
						{
							if (attribute == "fill")
							{
								this.Fill = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
								return true;
							}
						}
					}
					else if (attribute == "sprite")
					{
						this.SpriteName = value;
						return true;
					}
				}
				else if (num != 3060355671U)
				{
					if (num == 3134659686U)
					{
						if (attribute == "flip")
						{
							this.Flip = EnumUtils.Parse<UIBasicSprite.Flip>(value, true);
							return true;
						}
					}
				}
				else if (attribute == "globalopacity")
				{
					if (!StringParsers.ParseBool(value, 0, -1, true))
					{
						this.GlobalOpacityModifier = 0f;
						return true;
					}
					return true;
				}
			}
			else if (num <= 3435511540U)
			{
				if (num <= 3383065148U)
				{
					if (num != 3360407129U)
					{
						if (num == 3383065148U)
						{
							if (attribute == "fillinvert")
							{
								this.FillInvert = StringParsers.ParseBool(value, 0, -1, true);
								return true;
							}
						}
					}
					else if (attribute == "filldirection")
					{
						this.FillDirection = EnumUtils.Parse<UIBasicSprite.FillDirection>(value, true);
						return true;
					}
				}
				else if (num != 3407328204U)
				{
					if (num == 3435511540U)
					{
						if (attribute == "sprite_ps4")
						{
							this.spriteNamePS4 = value;
							return true;
						}
					}
				}
				else if (attribute == "atlas")
				{
					this.UIAtlas = value;
					return true;
				}
			}
			else if (num <= 3465140046U)
			{
				if (num != 3458780677U)
				{
					if (num == 3465140046U)
					{
						if (attribute == "bordersize")
						{
							this.borderSize = int.Parse(value);
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
			else if (num != 3607701014U)
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
			else if (attribute == "sprite_xb1")
			{
				this.spriteNameXB1 = value;
				return true;
			}
			return false;
		}
		return flag;
	}

	public void SetSpriteImmediately(string spriteName)
	{
		this.spriteName = spriteName;
		this.applyAtlasAndSprite(true);
	}

	public void SetColorImmediately(Color color)
	{
		if (this.sprite != null)
		{
			this.sprite.color = color;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool applyAtlasAndSprite(bool _force = false)
	{
		if (this.sprite == null)
		{
			return false;
		}
		if (!_force && this.sprite.spriteName != null && this.sprite.spriteName == this.spriteName && this.sprite.atlas != null && !this.uiAtlasChanged)
		{
			return false;
		}
		this.uiAtlasChanged = false;
		this.sprite.atlas = base.xui.GetAtlasByName(this.UIAtlas, this.spriteName);
		this.sprite.spriteName = this.spriteName;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uiAtlasChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillInvert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillCenter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float fillAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool foregroundLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteNameXB1 = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteNamePS4 = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float globalOpacityModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int borderSize = -1;
}
