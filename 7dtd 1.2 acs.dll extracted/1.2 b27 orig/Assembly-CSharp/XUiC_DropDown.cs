using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DropDown : XUiController
{
	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public int PageLength
	{
		get
		{
			XUiC_DropDown.Entry[] array = this.listEntryControllers;
			if (array == null)
			{
				return 1;
			}
			return array.Length;
		}
	}

	public bool DropdownOpen
	{
		get
		{
			return this.dropdownOpen;
		}
		set
		{
			if (value != this.dropdownOpen)
			{
				this.dropdownOpen = value;
				this.IsDirty = true;
			}
		}
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			int num = Mathf.Clamp(value, 0, this.LastPage);
			if (num != this.page)
			{
				this.page = num;
				this.IsDirty = true;
			}
		}
	}

	public int LastPage
	{
		get
		{
			return Math.Max(0, Mathf.CeilToInt((float)this.filteredEntries.Count / (float)this.PageLength) - 1);
		}
	}

	public int EntryCount
	{
		get
		{
			return this.filteredEntries.Count;
		}
	}

	public string Text
	{
		get
		{
			return this.input.Text;
		}
		set
		{
			if (value != this.input.Text)
			{
				this.input.Text = value;
				this.UpdateFilteredList();
			}
		}
	}

	public XUiC_TextInput TextInput
	{
		get
		{
			return this.input;
		}
	}

	public override void Init()
	{
		base.Init();
		base.OnScroll += this.HandleOnScroll;
		XUiController childById = base.GetChildById("pageUp");
		if (childById != null)
		{
			childById.OnPress += this.HandlePageUpPress;
		}
		XUiController childById2 = base.GetChildById("pageDown");
		if (childById2 != null)
		{
			childById2.OnPress += this.HandlePageDownPress;
		}
		this.handlePageDownAction = new Action(this.HandlePageDown);
		this.handlePageUpAction = new Action(this.HandlePageUp);
		XUiController childById3 = base.GetChildById("list");
		if (childById3 != null)
		{
			this.listEntryControllers = new XUiC_DropDown.Entry[childById3.Children.Count];
			for (int i = 0; i < childById3.Children.Count; i++)
			{
				this.listEntryControllers[i] = (childById3.Children[i] as XUiC_DropDown.Entry);
				if (this.listEntryControllers[i] != null)
				{
					this.listEntryControllers[i].OnScroll += this.HandleOnScroll;
					this.listEntryControllers[i].Owner = this;
				}
				else
				{
					Log.Warning("[XUi] DropDown elements do not have the correct controller set (should be \"XUiC_DropDown+Entry\")");
				}
			}
		}
		this.input = (base.GetChildById("input") as XUiC_TextInput);
		if (this.input != null)
		{
			this.input.OnChangeHandler += this.OnInputChanged;
			this.input.OnSubmitHandler += this.OnInputSubmit;
			this.input.OnSelect += this.OnInputSelected;
		}
		XUiController childById4 = base.GetChildById("btnDropdown");
		if (childById4 != null)
		{
			childById4.OnPress += this.BtnDropdown_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDropdown_OnPress(XUiController _sender, int _mouseButton)
	{
		this.DropdownOpen = !this.DropdownOpen;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			this.DropdownOpen = true;
			return;
		}
		ThreadManager.StartCoroutine(this.CloseDropdownLater());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CloseDropdownLater()
	{
		while (base.xui.playerUI.playerInput.GUIActions.LeftClick.IsPressed)
		{
			yield return null;
		}
		yield return null;
		if (!this.input.IsSelected)
		{
			this.DropdownOpen = false;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSubmit(XUiController _sender, string _text)
	{
		this.DropdownOpen = false;
		this.input.SetSelected(false, false);
		this.OnInputChanged(_sender, _text, false);
		this.SendSubmitEvent();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.UpdateFilteredList();
		this.SendChangedEvent(_changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendChangedEvent(bool _changeFromCode)
	{
		XUiEvent_InputOnChangedEventHandler onChangeHandler = this.OnChangeHandler;
		if (onChangeHandler == null)
		{
			return;
		}
		onChangeHandler(this, this.Text, _changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSubmitEvent()
	{
		XUiEvent_InputOnSubmitEventHandler onSubmitHandler = this.OnSubmitHandler;
		if (onSubmitHandler == null)
		{
			return;
		}
		onSubmitHandler(this, this.Text);
	}

	public void UpdateFilteredList()
	{
		XUiC_TextInput xuiC_TextInput = this.input;
		string text = (xuiC_TextInput != null) ? xuiC_TextInput.Text : null;
		this.filteredEntries.Clear();
		if (!string.IsNullOrEmpty(text))
		{
			using (List<string>.Enumerator enumerator = this.AllEntries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string text2 = enumerator.Current;
					if (text2.ContainsCaseInsensitive(text))
					{
						this.filteredEntries.Add(text2);
					}
				}
				goto IL_7B;
			}
		}
		this.filteredEntries.AddRange(this.AllEntries);
		IL_7B:
		if (this.sortEntries)
		{
			this.filteredEntries.Sort();
		}
		this.Page = 0;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			this.HandlePageDown();
			return;
		}
		this.HandlePageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageDownPress(XUiController _sender, int _mouseButton)
	{
		this.HandlePageDown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageUpPress(XUiController _sender, int _mouseButton)
	{
		this.HandlePageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageDown()
	{
		this.input.SetSelected(true, false);
		if (this.page > 0)
		{
			int num = this.Page;
			this.Page = num - 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageUp()
	{
		this.input.SetSelected(true, false);
		if ((this.page + 1) * this.PageLength < this.filteredEntries.Count)
		{
			int num = this.Page;
			this.Page = num + 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCurrentPageContents()
	{
		for (int i = 0; i < this.PageLength; i++)
		{
			int num = i + this.PageLength * this.page;
			this.listEntryControllers[i].Text = ((num < this.filteredEntries.Count) ? this.filteredEntries[num] : null);
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			if (this.page > this.LastPage)
			{
				this.Page = this.LastPage;
			}
			this.UpdateCurrentPageContents();
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
		base.Update(_dt);
		if (base.ViewComponent.IsVisible)
		{
			XUi.HandlePaging(base.xui, this.handlePageUpAction, this.handlePageDownAction, false);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "thumbareasize")
		{
			this.thumbAreaSize = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
			return true;
		}
		if (_name == "dropdown_textcolor")
		{
			this.dropdownTextcolor = _value;
			return true;
		}
		if (_name == "dropdown_hovercolor")
		{
			this.dropdownHovercolor = _value;
			return true;
		}
		if (_name == "sortentries")
		{
			this.sortEntries = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		if (!(_name == "clearonopen"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.clearOnOpen = StringParsers.ParseBool(_value, 0, -1, true);
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		int num = Mathf.RoundToInt(this.thumbAreaSize / (float)(this.LastPage + 1));
		if (_bindingName == "flip_dropdownbutton")
		{
			_value = (this.dropdownOpen ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing).ToStringCached<UIBasicSprite.Flip>();
			return true;
		}
		if (_bindingName == "dropdown_open")
		{
			_value = this.dropdownOpen.ToString();
			return true;
		}
		if (_bindingName == "thumb_size")
		{
			_value = num.ToString();
			return true;
		}
		if (!(_bindingName == "thumb_position"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = Mathf.RoundToInt((float)this.Page / (float)(this.LastPage + 1) * this.thumbAreaSize).ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.clearOnOpen)
		{
			this.Text = string.Empty;
		}
		this.DropdownOpen = false;
		this.IsDirty = true;
		base.RefreshBindings(false);
	}

	public readonly List<string> AllEntries = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> filteredEntries = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown.Entry[] listEntryControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput input;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public float thumbAreaSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string dropdownHovercolor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string dropdownTextcolor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool clearOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dropdownOpen;

	[Preserve]
	public class Entry : XUiController
	{
		public string Text
		{
			set
			{
				if (value != this.text)
				{
					this.text = value;
					this.IsDirty = true;
				}
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			this.hovered = false;
			this.IsDirty = true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnPressed(int _mouseButton)
		{
			base.OnPressed(_mouseButton);
			this.Owner.Text = this.text;
			this.Owner.DropdownOpen = false;
			this.Owner.SendChangedEvent(true);
			this.Owner.SendSubmitEvent();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			base.OnHovered(_isOver);
			this.hovered = _isOver;
			this.IsDirty = true;
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			if (this.IsDirty)
			{
				base.RefreshBindings(false);
				this.IsDirty = false;
			}
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.text;
				return true;
			}
			if (!(_bindingName == "textcolor"))
			{
				return base.GetBindingValue(ref _value, _bindingName);
			}
			_value = ((this.Owner == null) ? "100,100,100" : (this.hovered ? this.Owner.dropdownHovercolor : this.Owner.dropdownTextcolor));
			return true;
		}

		public XUiC_DropDown Owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public string text;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hovered;
	}
}
