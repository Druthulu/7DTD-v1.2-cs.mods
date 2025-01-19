using System;
using UnityEngine;

public class XUiV_Table : XUiView
{
	public UITable Table { get; set; }

	public UITable.Sorting Sorting { get; set; }

	public int Columns { get; set; } = 1;

	public Vector2 Padding { get; set; }

	public bool HideInactive { get; set; } = true;

	public bool AlwaysReposition { get; set; }

	public bool RepositionTwice { get; set; }

	public XUiV_Table(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UITable>();
	}

	public override void InitView()
	{
		base.InitView();
		this.Table = this.uiTransform.gameObject.GetComponent<UITable>();
		this.Table.hideInactive = this.HideInactive;
		this.Table.sorting = this.Sorting;
		this.Table.direction = UITable.Direction.Down;
		this.Table.columns = this.Columns;
		this.Table.padding = this.Padding;
		this.uiTransform.localScale = Vector3.one;
		this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
		this.Table.Reposition();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.IsVisible = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Table.repositionNow = true;
		if (this.RepositionTwice)
		{
			this.repositionNextFrame = true;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.firstUpdate || this.AlwaysReposition)
		{
			this.firstUpdate = false;
			this.Table.Reposition();
		}
		if (this.repositionNextFrame && !this.Table.enabled)
		{
			this.Table.repositionNow = true;
			this.repositionNextFrame = false;
		}
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			if (!(attribute == "columns"))
			{
				if (!(attribute == "padding"))
				{
					if (!(attribute == "sorting"))
					{
						if (!(attribute == "hide_inactive"))
						{
							if (!(attribute == "always_reposition"))
							{
								if (!(attribute == "reposition_twice"))
								{
									return false;
								}
								this.RepositionTwice = StringParsers.ParseBool(value, 0, -1, true);
							}
							else
							{
								this.AlwaysReposition = StringParsers.ParseBool(value, 0, -1, true);
							}
						}
						else
						{
							this.HideInactive = StringParsers.ParseBool(value, 0, -1, true);
						}
					}
					else
					{
						this.Sorting = EnumUtils.Parse<UITable.Sorting>(value, true);
					}
				}
				else
				{
					this.Padding = StringParsers.ParseVector2(value);
				}
			}
			else
			{
				this.Columns = int.Parse(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdate = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool repositionNextFrame;
}
