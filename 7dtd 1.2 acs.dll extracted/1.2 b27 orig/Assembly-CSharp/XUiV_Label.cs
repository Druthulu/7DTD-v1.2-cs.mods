using System;
using System.Globalization;
using Platform;
using UnityEngine;

public class XUiV_Label : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	static XUiV_Label()
	{
		Localization.LanguageSelected += XUiV_Label.OnLanguageSelected;
		XUiV_Label.OnLanguageSelected(Localization.RequestedLanguage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnLanguageSelected(string _lang)
	{
		string text = Localization.Get("cultureInfoName", false);
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("No culture info name given for selected language: " + _lang);
			return;
		}
		TextInfo textInfo;
		try
		{
			textInfo = CultureInfo.GetCultureInfo(text).TextInfo;
		}
		catch (Exception)
		{
			Log.Warning(string.Concat(new string[]
			{
				"No culture info found for given name: ",
				text,
				" (language: ",
				_lang,
				")"
			}));
			return;
		}
		if (textInfo.CultureName != XUiV_Label.textInfo.CultureName)
		{
			XUiV_Label.textInfo = textInfo;
			Log.Out("Updated culture for display texts");
		}
	}

	public UILabel Label
	{
		get
		{
			return this.label;
		}
		set
		{
			this.label = value;
			this.isDirty = true;
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

	public UILabel.Overflow Overflow
	{
		get
		{
			return this.overflow;
		}
		set
		{
			this.overflow = value;
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

	public string Text
	{
		get
		{
			return this.text;
		}
		set
		{
			if (this.text != value)
			{
				this.text = value;
				this.isDirty = true;
				this.bUpdateText = true;
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

	public int MaxLineCount
	{
		get
		{
			return this.maxLineCount;
		}
		set
		{
			if (value != this.maxLineCount)
			{
				this.maxLineCount = value;
				this.isDirty = true;
			}
		}
	}

	public int SpacingX
	{
		get
		{
			return this.spacingX;
		}
		set
		{
			if (value != this.spacingX)
			{
				this.spacingX = value;
				this.isDirty = true;
			}
		}
	}

	public int SpacingY
	{
		get
		{
			return this.spacingY;
		}
		set
		{
			if (value != this.spacingY)
			{
				this.spacingY = value;
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

	public Bounds LabelBounds
	{
		get
		{
			return this.label.CalculateBounds();
		}
	}

	public XUiV_Label(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UILabel>();
	}

	public override void InitView()
	{
		base.InitView();
		this.label = this.uiTransform.GetComponent<UILabel>();
		if (this.UIFont != null)
		{
			this.UpdateData();
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
		this.initialized = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		}
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
		this.label.symbolDepth = this.depth + 1;
		this.label.fontSize = this.fontSize;
		base.parseAnchors(this.label, this.label.overflowMethod != UILabel.Overflow.ResizeFreely);
		this.label.supportEncoding = this.supportBbCode;
		if (this.text != null && this.bUpdateText)
		{
			this.label.text = this.GetFormattedText(this.text);
			this.bUpdateText = false;
		}
		this.label.supportEncoding = this.supportBbCode;
		this.label.color = this.color;
		this.label.alignment = this.alignment;
		this.label.keepCrispWhenShrunk = this.crispness;
		this.label.effectStyle = this.effect;
		this.label.effectColor = this.effectColor;
		this.label.effectDistance = this.effectDistance;
		this.label.overflowMethod = this.overflow;
		this.label.spacingX = this.spacingX;
		this.label.spacingY = this.spacingY;
		this.label.maxLineCount = this.maxLineCount;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _obj)
	{
		if (this.parseActions && this.currentTextHasActions)
		{
			this.ForceTextUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetFormattedText(string _text)
	{
		if (this.upperCase)
		{
			_text = XUiV_Label.textInfo.ToUpper(_text);
		}
		else if (this.lowerCase)
		{
			_text = XUiV_Label.textInfo.ToLower(_text);
		}
		if (this.parseActions)
		{
			this.currentTextHasActions = XUiUtils.ParseActionsMarkup(base.xui, _text, out _text, this.actionsDefaultFormat, this.forceInputStyle);
		}
		return _text;
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.Alignment = NGUIText.Alignment.Left;
		this.FontSize = 16;
		this.overflow = UILabel.Overflow.ShrinkContent;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
			if (num <= 2396994324U)
			{
				if (num <= 1031692888U)
				{
					if (num <= 250128721U)
					{
						if (num != 233351102U)
						{
							if (num == 250128721U)
							{
								if (attribute == "spacing_x")
								{
									this.spacingX = StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer);
									return true;
								}
							}
						}
						else if (attribute == "spacing_y")
						{
							this.spacingY = StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer);
							return true;
						}
					}
					else if (num != 815142563U)
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
				else if (num <= 1852738900U)
				{
					if (num != 1519750509U)
					{
						if (num != 1788384520U)
						{
							if (num == 1852738900U)
							{
								if (attribute == "effect")
								{
									this.Effect = EnumUtils.Parse<UILabel.Effect>(value, true);
									return true;
								}
							}
						}
						else if (attribute == "parse_actions")
						{
							this.parseActions = StringParsers.ParseBool(value, 0, -1, true);
							return true;
						}
					}
					else if (attribute == "force_input_style")
					{
						this.forceInputStyle = EnumUtils.Parse<XUiUtils.ForceLabelInputStyle>(value, true);
						return true;
					}
				}
				else if (num != 2140393898U)
				{
					if (num != 2274577235U)
					{
						if (num == 2396994324U)
						{
							if (attribute == "font_size")
							{
								this.FontSize = int.Parse(value);
								return true;
							}
						}
					}
					else if (attribute == "crispness")
					{
						this.Crispness = EnumUtils.Parse<UILabel.Crispness>(value, true);
						return true;
					}
				}
				else if (attribute == "text_key")
				{
					if (!string.IsNullOrEmpty(value))
					{
						this.Text = Localization.Get(value, false);
						return true;
					}
					return true;
				}
			}
			else if (num <= 2887479055U)
			{
				if (num <= 2572986219U)
				{
					if (num != 2439222772U)
					{
						if (num == 2572986219U)
						{
							if (attribute == "overflow")
							{
								this.Overflow = EnumUtils.Parse<UILabel.Overflow>(value, true);
								return true;
							}
						}
					}
					else if (attribute == "font_face")
					{
						this.UIFont = base.xui.GetUIFontByName(value, false);
						if (this.UIFont == null)
						{
							Log.Warning(string.Concat(new string[]
							{
								"XUi Label: Font not found: ",
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
				else if (num != 2689418572U)
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
				else if (attribute == "max_line_count")
				{
					this.maxLineCount = StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer);
					return true;
				}
			}
			else if (num <= 3446912195U)
			{
				if (num != 3060355671U)
				{
					if (num != 3185987134U)
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
					else if (attribute == "text")
					{
						this.Text = value;
						return true;
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
			else if (num != 3458663872U)
			{
				if (num != 4144336821U)
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
			}
			else if (attribute == "actions_default_format")
			{
				this.actionsDefaultFormat = value;
				return true;
			}
			return false;
		}
		return flag;
	}

	public void SetTextImmediately(string _text)
	{
		if (this.label != null)
		{
			this.text = _text;
			this.label.text = this.GetFormattedText(this.text);
		}
	}

	public void ForceTextUpdate()
	{
		this.bUpdateText = true;
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static TextInfo textInfo = Utils.StandardCulture.TextInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel label;

	[PublicizedFrom(EAccessModifier.Protected)]
	public NGUIFont uiFont;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Overflow overflow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Effect effect;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color effectColor = new Color32(0, 0, 0, 80);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 effectDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Crispness crispness;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxLineCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacingX = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacingY;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new NGUIText.Alignment alignment;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool supportBbCode = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool upperCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lowerCase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parseActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string actionsDefaultFormat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool currentTextHasActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiUtils.ForceLabelInputStyle forceInputStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;
}
