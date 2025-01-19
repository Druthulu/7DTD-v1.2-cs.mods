using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_NewsWindow : XUiController
{
	public int CurrentIndex
	{
		get
		{
			return this.currentIndex;
		}
		set
		{
			if (value >= this.entries.Count)
			{
				value = this.entries.Count - 1;
			}
			if (value >= this.maxNews)
			{
				value = this.maxNews - 1;
			}
			if (value < 0)
			{
				value = 0;
			}
			this.currentIndex = value;
			this.IsDirty = true;
		}
	}

	public NewsManager.NewsEntry CurrentEntry
	{
		get
		{
			if (this.CurrentIndex < 0)
			{
				return null;
			}
			if (this.CurrentIndex >= this.entries.Count)
			{
				return null;
			}
			object obj = this.newsLock;
			NewsManager.NewsEntry result;
			lock (obj)
			{
				result = this.entries[this.CurrentIndex];
			}
			return result;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("btnYounger");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.cycle(-1, false);
			};
		}
		XUiController childById2 = base.GetChildById("btnOlder");
		if (childById2 != null)
		{
			childById2.OnPress += delegate(XUiController _, int _)
			{
				this.cycle(1, false);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnLink") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnLink_OnPressed;
		}
		else
		{
			XUiController childById3 = base.GetChildById("btnLink");
			if (childById3 != null && childById3.ViewComponent is XUiV_Button)
			{
				childById3.OnPress += this.BtnLink_OnPressed;
				childById3.OnScroll += this.OnScrollEvent;
			}
		}
		base.OnScroll += this.OnScrollEvent;
		XUiController childById4 = base.GetChildById("newsImage");
		if (childById4 != null)
		{
			this.bannerTexture = (childById4.ViewComponent as XUiV_Texture);
		}
		this.selector = base.GetChildByType<XUiC_ComboBoxFloat>();
		if (this.selector != null)
		{
			this.selector.OnValueChanged += this.Selector_OnValueChanged;
			this.maxNews = Mathf.RoundToInt((float)this.selector.Max);
		}
		this.hasProviders = (this.newsProviders.Count > 0);
		if (!this.hasProviders)
		{
			Log.Warning(string.Concat(new string[]
			{
				"[XUi] News controller with no sources specified (window group '",
				base.WindowGroup.ID,
				"', window '",
				base.ViewComponent.ID,
				"')"
			}));
		}
		NewsManager.Instance.Updated += this.newsUpdated;
		this.newsUpdated(NewsManager.Instance);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollEvent(XUiController _sender, float _delta)
	{
		if (this.selector == null)
		{
			this.cycle(Math.Sign(_delta), true);
			return;
		}
		this.selector.ScrollEvent(_sender, _delta);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Selector_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		this.CurrentIndex = Mathf.CeilToInt((float)_newValue) - 1;
		this.selector.Value = (double)(this.CurrentIndex + 1);
		this.autoCycle = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newsUpdated(NewsManager _newsManager)
	{
		NewsManager.NewsEntry currentEntry = this.CurrentEntry;
		object obj = this.newsLock;
		lock (obj)
		{
			_newsManager.GetNewsData(this.newsProviders, this.entries);
		}
		int num = 0;
		while (num < this.maxNews && num < this.entries.Count)
		{
			this.entries[num].RequestImage();
			num++;
		}
		this.IsDirty = true;
		if (currentEntry == null || !currentEntry.Equals(this.CurrentEntry))
		{
			this.resetIndex();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.doAutoCycle(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			if (this.bannerTexture != null)
			{
				NewsManager.NewsEntry currentEntry = this.CurrentEntry;
				this.bannerTexture.Texture = ((currentEntry != null) ? currentEntry.Image : null);
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doAutoCycle(float _dt)
	{
		if (this.selector == null || !this.autoCycle)
		{
			return;
		}
		NewsManager.NewsEntry currentEntry = this.CurrentEntry;
		if (currentEntry == null || (currentEntry.HasImage && !currentEntry.ImageLoaded))
		{
			return;
		}
		float num = (float)this.selector.Value + _dt / this.autoCycleTimePerEntry;
		if ((double)num > this.selector.Max)
		{
			num = 0f;
		}
		this.selector.Value = (double)num;
		this.CurrentIndex = Mathf.CeilToInt((float)this.selector.Value) - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLink_OnPressed(XUiController _sender, int _mousebutton)
	{
		NewsManager.NewsEntry currentEntry = this.CurrentEntry;
		if (currentEntry == null)
		{
			return;
		}
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(base.xui, currentEntry.Url, false, null, null, null, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycle(int _increment, bool _sound)
	{
		object obj = this.newsLock;
		lock (obj)
		{
			int num = this.CurrentIndex;
			this.CurrentIndex += _increment;
			if (this.CurrentIndex != num && _sound && this.browseSound != null)
			{
				Manager.PlayXUiSound(this.browseSound, 1f);
			}
			this.autoCycle = false;
			if (this.selector != null)
			{
				this.selector.Value = (double)(this.CurrentIndex + 1);
			}
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetIndex()
	{
		this.CurrentIndex = 0;
		if (this.selector != null)
		{
			this.selector.Value = (double)(this.autoCycle ? 0 : 1);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.autoCycle = (this.autoCycleTimePerEntry > 0f);
		this.resetIndex();
		base.RefreshBindings(true);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "browse_sound")
		{
			base.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
			{
				this.browseSound = _o;
			});
			return true;
		}
		if (_name == "additional_sources" || _name == "sources")
		{
			string[] array = _value.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (text.Length != 0)
				{
					this.newsProviders.Add(text);
					NewsManager.Instance.RegisterNewsSource(text);
				}
			}
			return true;
		}
		if (_name == "auto_cycle_time_per_entry")
		{
			this.autoCycleTimePerEntry = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
			return true;
		}
		if (_name == "button_younger")
		{
			this.buttonYounger = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
			if (this.buttonYounger == null)
			{
				Log.Warning(string.Concat(new string[]
				{
					"[XUi] Could not find GUI action '",
					_value,
					"' for news window (window group '",
					base.WindowGroup.ID,
					"', window '",
					base.ViewComponent.ID,
					"')"
				}));
			}
			return true;
		}
		if (!(_name == "button_older"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.buttonOlder = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
		if (this.buttonOlder == null)
		{
			Log.Warning(string.Concat(new string[]
			{
				"[XUi] Could not find GUI action '",
				_value,
				"' for news window (window group '",
				base.WindowGroup.ID,
				"', window '",
				base.ViewComponent.ID,
				"')"
			}));
		}
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		NewsManager.NewsEntry newsEntry = this.CurrentEntry ?? NewsManager.EmptyEntry;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2179810835U)
		{
			if (num <= 742476188U)
			{
				if (num != 232457833U)
				{
					if (num != 466561496U)
					{
						if (num == 742476188U)
						{
							if (_bindingName == "age")
							{
								_value = ValueDisplayFormatters.DateAge(newsEntry.Date);
								return true;
							}
						}
					}
					else if (_bindingName == "source")
					{
						_value = (newsEntry.CustomListName ?? "");
						return true;
					}
				}
				else if (_bindingName == "link")
				{
					_value = (newsEntry.Url ?? "");
					return true;
				}
			}
			else if (num != 1208723273U)
			{
				if (num != 1280386353U)
				{
					if (num == 2179810835U)
					{
						if (_bindingName == "has_younger")
						{
							_value = (this.entries.Count > 0 && this.CurrentIndex > 0).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "has_news_provider")
				{
					_value = this.hasProviders.ToString();
					return true;
				}
			}
			else if (_bindingName == "headline")
			{
				_value = newsEntry.Headline;
				return true;
			}
		}
		else if (num <= 3165208740U)
		{
			if (num != 2851791573U)
			{
				if (num != 2874786163U)
				{
					if (num == 3165208740U)
					{
						if (_bindingName == "has_link")
						{
							_value = (!string.IsNullOrEmpty(newsEntry.Url)).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "has_news")
				{
					_value = (this.entries.Count > 0).ToString();
					return true;
				}
			}
			else if (_bindingName == "is_custom")
			{
				_value = newsEntry.IsCustom.ToString();
				return true;
			}
		}
		else if (num <= 3564297305U)
		{
			if (num != 3185987134U)
			{
				if (num == 3564297305U)
				{
					if (_bindingName == "date")
					{
						_value = newsEntry.Date.ToString("yyyy-MM-dd");
						return true;
					}
				}
			}
			else if (_bindingName == "text")
			{
				_value = newsEntry.Text;
				return true;
			}
		}
		else if (num != 3805401922U)
		{
			if (num == 3847792289U)
			{
				if (_bindingName == "headline2")
				{
					_value = newsEntry.Headline2;
					return true;
				}
			}
		}
		else if (_bindingName == "has_older")
		{
			_value = (this.entries.Count > 0 && this.CurrentIndex < this.entries.Count - 1).ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip browseSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat selector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture bannerTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object newsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NewsManager.NewsEntry> entries = new List<NewsManager.NewsEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasProviders;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> newsProviders = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxNews = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoCycleTimePerEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool autoCycle;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction buttonYounger;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction buttonOlder;
}
