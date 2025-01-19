using System;
using System.Globalization;
using UnityEngine;

public class XUiV_TextList : XUiView
{
	public UILabel Label
	{
		get
		{
			return this.label;
		}
	}

	public UITextList TextList
	{
		get
		{
			return this.textList;
		}
	}

	public NGUIFont UIFont
	{
		get
		{
			return this.uiFont;
		}
		set
		{
			this.uiFont = value;
			this.isDirty = true;
		}
	}

	public int FontSize
	{
		get
		{
			return this.fontSize;
		}
		set
		{
			this.fontSize = value;
			this.isDirty = true;
		}
	}

	public UILabel.Effect Effect
	{
		get
		{
			return this.effect;
		}
		set
		{
			this.effect = value;
			this.isDirty = true;
		}
	}

	public Color EffectColor
	{
		get
		{
			return this.effectColor;
		}
		set
		{
			this.effectColor = value;
			this.isDirty = true;
		}
	}

	public Vector2 EffectDistance
	{
		get
		{
			return this.effectDistance;
		}
		set
		{
			this.effectDistance = value;
			this.isDirty = true;
		}
	}

	public UILabel.Crispness Crispness
	{
		get
		{
			return this.crispness;
		}
		set
		{
			this.crispness = value;
			this.isDirty = true;
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
			if (this.color != value)
			{
				this.color = value;
				this.isDirty = true;
			}
		}
	}

	public NGUIText.Alignment Alignment
	{
		get
		{
			return this.alignment;
		}
		set
		{
			if (this.alignment != value)
			{
				this.alignment = value;
				this.isDirty = true;
			}
		}
	}

	public bool SupportBbCode
	{
		get
		{
			return this.supportBbCode;
		}
		set
		{
			if (this.supportBbCode != value)
			{
				this.supportBbCode = value;
				this.isDirty = true;
			}
		}
	}

	public int ParagraphHistory
	{
		get
		{
			return this.paragraphHistory;
		}
		set
		{
			if (value != this.paragraphHistory)
			{
				this.paragraphHistory = value;
				this.isDirty = true;
			}
		}
	}

	public UITextList.Style ListStyle
	{
		get
		{
			return this.listStyle;
		}
		set
		{
			if (value != this.listStyle)
			{
				this.listStyle = value;
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
			this.globalOpacityModifier = value;
			this.isDirty = true;
		}
	}

	public XUiV_TextList(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UILabel>();
		_go.AddComponent<UITextList>();
	}

	public override void InitView()
	{
		this.EventOnDrag = true;
		this.EventOnScroll = true;
		base.InitView();
		this.label = this.uiTransform.GetComponent<UILabel>();
		this.textList = this.uiTransform.GetComponent<UITextList>();
		if (this.UIFont != null)
		{
			this.UpdateData();
		}
		this.initialized = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.GlobalOpacityChanged)
		{
			this.isDirty = true;
		}
	}

	public override void UpdateData()
	{
		base.UpdateData();
		if (this.uiFont != null)
		{
			this.label.font = this.uiFont;
		}
		this.label.depth = this.depth;
		this.label.fontSize = this.fontSize;
		this.label.width = this.size.x;
		this.label.height = this.size.y;
		this.label.color = this.color;
		this.label.alignment = this.alignment;
		this.label.supportEncoding = this.supportBbCode;
		this.label.keepCrispWhenShrunk = this.crispness;
		this.label.effectStyle = this.effect;
		this.label.effectColor = this.effectColor;
		this.label.effectDistance = this.effectDistance;
		this.label.spacingX = 1;
		this.textList.paragraphHistory = this.paragraphHistory;
		this.textList.style = this.listStyle;
		if (!this.initialized)
		{
			this.label.pivot = this.pivot;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
			if (this.EventOnHover || this.EventOnPress)
			{
				BoxCollider collider = this.collider;
				collider.center = this.Label.localCenter;
				collider.size = new Vector3(this.label.localSize.x * this.colliderScale, this.label.localSize.y * this.colliderScale, 0f);
			}
		}
	}

	public void AddLine(string _line)
	{
		if (this.upperCase)
		{
			_line = _line.ToUpper();
		}
		else if (this.lowerCase)
		{
			_line = _line.ToLower();
		}
		this.textList.Add(_line);
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.Alignment = NGUIText.Alignment.Left;
		this.FontSize = 16;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
			if (num <= 2439222772U)
			{
				if (num <= 1031692888U)
				{
					if (num != 815142563U)
					{
						if (num != 1021845972U)
						{
							if (num == 1031692888U)
							{
								if (attribute == "color")
								{
									this.Color = StringParsers.ParseColor32(value);
									return true;
								}
							}
						}
						else if (attribute == "upper_case")
						{
							this.upperCase = StringParsers.ParseBool(value, 0, -1, true);
							return true;
						}
					}
					else if (attribute == "lower_case")
					{
						this.lowerCase = StringParsers.ParseBool(value, 0, -1, true);
						return true;
					}
				}
				else if (num <= 2274577235U)
				{
					if (num != 1852738900U)
					{
						if (num == 2274577235U)
						{
							if (attribute == "crispness")
							{
								this.Crispness = EnumUtils.Parse<UILabel.Crispness>(value, true);
								return true;
							}
						}
					}
					else if (attribute == "effect")
					{
						this.Effect = EnumUtils.Parse<UILabel.Effect>(value, true);
						return true;
					}
				}
				else if (num != 2396994324U)
				{
					if (num == 2439222772U)
					{
						if (attribute == "font_face")
						{
							this.UIFont = base.xui.GetUIFontByName(value, false);
							if (this.UIFont == null)
							{
								Log.Warning(string.Concat(new string[]
								{
									"XUi TextList: Font not found: ",
									value,
									", from: ",
									base.Controller.GetParentWindow().ID,
									".",
									base.ID
								}));
								return true;
							}
							return true;
						}
					}
				}
				else if (attribute == "font_size")
				{
					this.FontSize = int.Parse(value);
					return true;
				}
			}
			else if (num <= 3446912195U)
			{
				if (num <= 2887479055U)
				{
					if (num != 2863932660U)
					{
						if (num == 2887479055U)
						{
							if (attribute == "justify")
							{
								this.Alignment = EnumUtils.Parse<NGUIText.Alignment>(value, true);
								return true;
							}
						}
					}
					else if (attribute == "effect_color")
					{
						this.EffectColor = StringParsers.ParseColor32(value);
						return true;
					}
				}
				else if (num != 3060355671U)
				{
					if (num == 3446912195U)
					{
						if (attribute == "support_bb_code")
						{
							this.supportBbCode = StringParsers.ParseBool(value, 0, -1, true);
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
			else if (num <= 4072007535U)
			{
				if (num != 3879501881U)
				{
					if (num == 4072007535U)
					{
						if (attribute == "list_style")
						{
							this.listStyle = EnumUtils.Parse<UITextList.Style>(value, true);
							return true;
						}
					}
				}
				else if (attribute == "max_paragraphs")
				{
					this.paragraphHistory = StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer);
					return true;
				}
			}
			else if (num != 4144336821U)
			{
				if (num == 4255248174U)
				{
					if (attribute == "effect_distance")
					{
						this.EffectDistance = StringParsers.ParseVector2(value);
						return true;
					}
				}
			}
			else if (attribute == "globalopacitymod")
			{
				this.GlobalOpacityModifier = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
				return true;
			}
			return false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITextList textList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel label;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIFont uiFont;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Effect effect;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color effectColor = new Color32(0, 0, 0, 80);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 effectDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Crispness crispness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITextList.Style listStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int paragraphHistory = 50;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new NGUIText.Alignment alignment;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool supportBbCode = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool upperCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lowerCase;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;
}
