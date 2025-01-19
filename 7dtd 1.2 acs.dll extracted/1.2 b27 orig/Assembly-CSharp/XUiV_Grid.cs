using System;
using System.Collections.Generic;
using UnityEngine;

public class XUiV_Grid : XUiView
{
	public UIGrid Grid { get; set; }

	public UIGrid.Arrangement Arrangement { get; set; }

	public event UIGrid.OnSizeChanged OnSizeChanged;

	public event Action OnSizeChangedSimple;

	public int Columns
	{
		get
		{
			return this.columns;
		}
		set
		{
			if (this.initialized && this.Arrangement == UIGrid.Arrangement.Horizontal && this.Grid.maxPerLine != value)
			{
				this.Grid.maxPerLine = value;
				this.Grid.Reposition();
			}
			this.columns = value;
			this.isDirty = true;
		}
	}

	public int Rows
	{
		get
		{
			return this.rows;
		}
		set
		{
			if (this.initialized && this.Arrangement != UIGrid.Arrangement.Horizontal && this.Grid.maxPerLine != value)
			{
				this.Grid.maxPerLine = value;
				this.Grid.Reposition();
			}
			this.rows = value;
			this.isDirty = true;
		}
	}

	public override int RepeatCount
	{
		get
		{
			return this.Columns * this.Rows;
		}
		set
		{
		}
	}

	public int CellWidth { get; set; }

	public int CellHeight { get; set; }

	public bool HideInactive { get; set; }

	public XUiV_Grid(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
		_go.AddComponent<UIGrid>();
	}

	public override void InitView()
	{
		base.InitView();
		this.widget = this.uiTransform.gameObject.GetComponent<UIWidget>();
		this.widget.pivot = this.pivot;
		this.widget.depth = base.Depth + 2;
		this.Grid = this.uiTransform.gameObject.GetComponent<UIGrid>();
		this.Grid.hideInactive = this.HideInactive;
		this.Grid.arrangement = this.Arrangement;
		this.Grid.pivot = this.pivot;
		this.Grid.onSizeChanged = new UIGrid.OnSizeChanged(this.OnGridSizeChanged);
		if (this.Arrangement == UIGrid.Arrangement.Horizontal)
		{
			this.Grid.maxPerLine = this.Columns;
		}
		else
		{
			this.Grid.maxPerLine = this.Rows;
		}
		this.Grid.cellWidth = (float)this.CellWidth;
		this.Grid.cellHeight = (float)this.CellHeight;
		this.uiTransform.localScale = Vector3.one;
		this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
		this.initialized = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGridSizeChanged(Vector2Int _cells, Vector2 _size)
	{
		this.widget.width = Mathf.RoundToInt(_size.x);
		this.widget.height = Mathf.RoundToInt(_size.y);
		UIGrid.OnSizeChanged onSizeChanged = this.OnSizeChanged;
		if (onSizeChanged != null)
		{
			onSizeChanged(_cells, _size);
		}
		Action onSizeChangedSimple = this.OnSizeChangedSimple;
		if (onSizeChangedSimple == null)
		{
			return;
		}
		onSizeChangedSimple();
	}

	public override void Update(float _dt)
	{
		if (this.isDirty)
		{
			if (this.Arrangement == UIGrid.Arrangement.Horizontal)
			{
				this.Grid.maxPerLine = this.Columns;
			}
			else
			{
				this.Grid.maxPerLine = this.Rows;
			}
		}
		this.Grid.repositionNow = true;
		base.Update(_dt);
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.Columns = 0;
		this.Rows = 0;
		this.CellWidth = 0;
		this.CellHeight = 0;
		this.Arrangement = UIGrid.Arrangement.Horizontal;
		this.HideInactive = true;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			if (!(attribute == "cols"))
			{
				if (!(attribute == "rows"))
				{
					if (!(attribute == "cell_width"))
					{
						if (!(attribute == "cell_height"))
						{
							if (!(attribute == "arrangement"))
							{
								if (!(attribute == "hide_inactive"))
								{
									return false;
								}
								this.HideInactive = StringParsers.ParseBool(value, 0, -1, true);
							}
							else
							{
								this.Arrangement = EnumUtils.Parse<UIGrid.Arrangement>(value, true);
							}
						}
						else
						{
							this.CellHeight = int.Parse(value);
						}
					}
					else
					{
						this.CellWidth = int.Parse(value);
					}
				}
				else
				{
					this.Rows = int.Parse(value);
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

	public override void setRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
		base.setRepeatContentTemplateParams(_templateParams, _curRepeatNum);
		int num;
		int num2;
		if (this.Arrangement == UIGrid.Arrangement.Horizontal)
		{
			num = _curRepeatNum % this.Columns;
			num2 = _curRepeatNum / this.Columns;
		}
		else
		{
			num = _curRepeatNum / this.Rows;
			num2 = _curRepeatNum % this.Rows;
		}
		_templateParams["repeat_col"] = num;
		_templateParams["repeat_row"] = num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIWidget widget;
}
