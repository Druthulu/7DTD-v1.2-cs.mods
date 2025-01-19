using System;
using System.Globalization;
using UnityEngine;

public class XUiV_Window : XUiView
{
	public string Anchor
	{
		get
		{
			return this.anchor;
		}
		set
		{
			this.anchor = value;
			this.isDirty = true;
		}
	}

	public XUiV_Window(string _id) : base(_id)
	{
	}

	public bool IsCursorArea
	{
		get
		{
			return this.cursorArea;
		}
	}

	public bool IsOpen
	{
		get
		{
			return this.isOpen;
		}
	}

	public bool IsInStackpanel
	{
		get
		{
			return this.isInStackpanel;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	public override void InitView()
	{
		base.InitView();
		base.Controller.OnVisiblity += this.UpdateVisibility;
		this.Panel = this.uiTransform.gameObject.GetComponent<UIPanel>();
		this.Panel.depth = base.Depth + 1;
		this.Panel.alpha = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRootNode()
	{
		if (this.rootNode == null)
		{
			Transform transform;
			if (this.Anchor == null)
			{
				transform = base.xui.transform;
			}
			else
			{
				transform = base.xui.transform.Find(this.Anchor);
				if (transform == null)
				{
					Log.Error(string.Concat(new string[]
					{
						"Specified window anchor \"",
						this.Anchor,
						"\" not found for window \"",
						base.ID,
						"\""
					}));
					throw new Exception();
				}
			}
			this.rootNode = transform;
			base.setRootNode();
			return;
		}
		if (this.uiTransform != null)
		{
			this.uiTransform.parent = this.rootNode;
			UITable component = this.rootNode.GetComponent<UITable>();
			if (component != null)
			{
				component.repositionNow = true;
			}
			this.IsVisible = true;
			this.uiTransform.gameObject.layer = 12;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
		}
	}

	public float TargetAlpha
	{
		get
		{
			return this.targetAlpha;
		}
		set
		{
			if (value != this.targetAlpha)
			{
				this.targetAlpha = value;
				this.fadeTimer = 0f;
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if ((double)Time.timeScale < 0.01 || !this.fade)
		{
			this.delayTimer = this.delayToFadeTime + 1f;
			this.fadeTimer = this.fadeInTime;
		}
		if (this.delayTimer < this.delayToFadeTime)
		{
			this.delayTimer += _dt;
		}
		if (this.delayTimer < this.delayToFadeTime)
		{
			return;
		}
		if (this.fadeTimer > this.fadeInTime)
		{
			this.fadeTimer = this.fadeInTime;
		}
		this.Panel.alpha = Mathf.Lerp(this.Panel.alpha, this.targetAlpha, this.fadeTimer / this.fadeInTime);
		this.fadeTimer += _dt;
		if (this.cursorArea && this.oldTransformPosition != base.UiTransform.position && this.IsVisible)
		{
			base.xui.UpdateWindowSoftCursorBounds(this);
			this.oldTransformPosition = base.UiTransform.position;
		}
	}

	public void ForceVisible(float _alpha = -1f)
	{
		this.delayTimer = 100f;
		this.fadeTimer = 100f;
		if (_alpha >= 0f)
		{
			this.targetAlpha = _alpha;
		}
		this.Panel.alpha = this.targetAlpha;
	}

	public override void UpdateData()
	{
		if (this.uiTransform != null)
		{
			this.setRootNode();
		}
		base.UpdateData();
		this.Panel.SetDirty();
		this.isDirty = false;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			if (!(attribute == "anchor"))
			{
				if (!(attribute == "panel"))
				{
					if (!(attribute == "cursor_area"))
					{
						if (!(attribute == "fade_delay"))
						{
							if (!(attribute == "fade_time"))
							{
								if (!(attribute == "fade_window"))
								{
									return false;
								}
								this.fade = StringParsers.ParseBool(value, 0, -1, true);
							}
							else
							{
								this.fadeInTime = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
							}
						}
						else
						{
							this.delayToFadeTime = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
						}
					}
					else
					{
						this.cursorArea = StringParsers.ParseBool(value, 0, -1, true);
					}
				}
				else
				{
					Transform transform = base.xui.transform.Find("StackPanels").transform;
					if (value != "")
					{
						this.rootNode = transform.FindInChilds(value, false);
						this.isInStackpanel = true;
					}
				}
			}
			else
			{
				this.Anchor = value;
			}
			return true;
		}
		return flag;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Panel.alpha = 0f;
		this.targetAlpha = 1f;
		this.fadeTimer = 0f;
		this.delayTimer = 0f;
		if (this.cursorArea)
		{
			this.oldTransformPosition = Vector3.zero;
		}
		this.isOpen = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.Panel.alpha = 0f;
		this.targetAlpha = 0f;
		this.fadeTimer = this.fadeInTime;
		this.delayTimer = this.delayToFadeTime;
		this.isOpen = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisibility(XUiController _sender, bool _visible)
	{
		if (this.cursorArea)
		{
			if (_visible)
			{
				base.xui.UpdateWindowSoftCursorBounds(this);
				return;
			}
			base.xui.RemoveWindowFromSoftCursorBounds(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new string anchor;

	public UIPanel Panel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInStackpanel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool cursorArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 oldTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fade = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeInTime = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayToFadeTime = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayTimer;
}
