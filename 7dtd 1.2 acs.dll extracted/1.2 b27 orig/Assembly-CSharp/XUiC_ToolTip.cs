using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToolTip : XUiController
{
	public string ToolTip
	{
		get
		{
			return this.tooltip;
		}
		set
		{
			if (this.tooltip != value)
			{
				if (value == null)
				{
					this.tooltip = "";
				}
				else if (value.Length > 0 && value[value.Length - 1] == '\n')
				{
					this.tooltip = value.Substring(0, value.Length - 1);
				}
				else
				{
					this.tooltip = value;
				}
				if (this.label != null && this.label.Label != null)
				{
					this.label.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
					this.label.Text = this.tooltip;
					this.label.SetTextImmediately(this.tooltip);
					if (this.tooltip != "")
					{
						base.ViewComponent.Position = base.xui.GetMouseXUIPosition() + new Vector2i(0, -36);
						this.showDelay = Time.unscaledTime + XUiC_ToolTip.SHOW_DELAY_SEC;
					}
				}
			}
		}
	}

	public Vector2i ToolTipPosition
	{
		get
		{
			return base.ViewComponent.Position;
		}
		set
		{
			base.ViewComponent.Position = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.ID = base.WindowGroup.ID;
		base.xui.currentToolTip = this;
		this.label = (XUiV_Label)base.GetChildById("lblText").ViewComponent;
		this.background = (XUiV_Sprite)base.GetChildById("sprBackground").ViewComponent;
		this.border = (XUiV_Sprite)base.GetChildById("sprBackgroundBorder").ViewComponent;
		this.tooltip = "";
	}

	public override void Update(float _dt)
	{
		if (!GameManager.Instance.isAnyCursorWindowOpen(null))
		{
			this.tooltip = "";
		}
		if (this.tooltip != "")
		{
			if (Time.unscaledTime > this.showDelay)
			{
				((XUiV_Window)base.ViewComponent).TargetAlpha = 1f;
			}
			this.border.Size = new Vector2i(this.label.Label.width + 18, this.label.Label.height + 12);
			this.background.Size = new Vector2i(this.border.Size.x - 6, this.border.Size.y - 6);
			Vector2i xuiScreenSize = base.xui.GetXUiScreenSize();
			if (this.label.Label.width > xuiScreenSize.x / 4)
			{
				this.label.Label.overflowMethod = UILabel.Overflow.ResizeHeight;
				this.label.Label.width = xuiScreenSize.x / 4 - 10;
			}
			else
			{
				Vector2i vector2i = xuiScreenSize / 2;
				if ((base.ViewComponent.Position + this.border.Size).x > vector2i.x)
				{
					base.ViewComponent.Position -= new Vector2i(this.border.Size.x, 0);
				}
				if ((base.ViewComponent.Position - this.border.Size).y < -vector2i.y)
				{
					base.ViewComponent.Position += new Vector2i(20, 20 + this.border.Size.y);
				}
			}
		}
		else
		{
			((XUiV_Window)base.ViewComponent).TargetAlpha = 0.0015f;
		}
		base.Update(_dt);
	}

	public string ID = "";

	public static float SHOW_DELAY_SEC = 0.3f;

	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltip = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nextFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public float showDelay;
}
