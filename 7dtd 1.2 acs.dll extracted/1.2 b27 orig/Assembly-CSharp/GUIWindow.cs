using System;
using Audio;
using UnityEngine;

public class GUIWindow
{
	public Rect windowRect
	{
		get
		{
			return this.internalWindowRect;
		}
		set
		{
			this.internalWindowRect = value;
			this.matrix.SetTRS(new Vector3(this.internalWindowRect.x, this.internalWindowRect.y, 0f), Quaternion.identity, Vector3.one);
		}
	}

	public GUIWindow(string _id, int _w, int _h, bool _bDrawBackground) : this(_id, _w, _h, _bDrawBackground, true)
	{
	}

	public GUIWindow(string _id, int _w, int _h, bool _bDrawBackground, bool _isDimBackground) : this(_id, new Rect((float)(Screen.width - _w) / 2f, (float)(Screen.height - _h) / 2f, (float)_w, (float)_h), _bDrawBackground, _isDimBackground)
	{
		this.bCenterWindow = true;
	}

	public GUIWindow(string _id, Rect _rect) : this(_id, _rect, false)
	{
	}

	public GUIWindow(string _id) : this(_id, default(Rect))
	{
	}

	public GUIWindow(string _id, Rect _rect, bool _bDrawBackground) : this(_id, _rect, _bDrawBackground, true)
	{
	}

	public GUIWindow(string _id, Rect _rect, bool _bDrawBackground, bool _isDimBackground)
	{
		this.windowRect = _rect;
		this.bDrawBackground = _bDrawBackground;
		this.isDimBackground = _isDimBackground;
		this.id = _id;
		this.bActionSetEnabled = false;
	}

	public string Id
	{
		get
		{
			return this.id;
		}
	}

	public bool GUIButton(Rect _rect, string _text)
	{
		if (GUI.Button(_rect, _text))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUIButton(Rect _rect, GUIContent _guiContent)
	{
		if (GUI.Button(_rect, _guiContent))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUIButton(Rect _rect, GUIContent _guiContent, GUIStyle _guiStyle)
	{
		if (GUI.Button(_rect, _guiContent, _guiStyle))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUILayoutButton(string _text)
	{
		return this.GUILayoutButton(_text, GUILayout.ExpandWidth(false));
	}

	public bool GUILayoutButton(string _text, GUILayoutOption options)
	{
		if (GUILayout.Button(_text, new GUILayoutOption[]
		{
			options
		}))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUIToggle(Rect _rect, bool _v, string _s)
	{
		bool flag = GUI.Toggle(_rect, _v, _s);
		if (flag != _v)
		{
			Manager.PlayButtonClick();
		}
		return flag;
	}

	public bool GUILayoutToggle(bool _v, string _s)
	{
		return this.GUILayoutToggle(_v, _s, null);
	}

	public bool GUILayoutToggle(bool _v, string _s, GUILayoutOption options)
	{
		bool flag = (options != null) ? GUILayout.Toggle(_v, _s, new GUILayoutOption[]
		{
			options
		}) : GUILayout.Toggle(_v, _s, Array.Empty<GUILayoutOption>());
		if (flag != _v)
		{
			Manager.PlayButtonClick();
		}
		return flag;
	}

	public virtual void OnGUI(bool _inputActive)
	{
		if (this.bDrawBackground)
		{
			GUI.Box(new Rect(0f, 0f, this.windowRect.width, this.windowRect.height), "");
		}
		if (this.bCenterWindow)
		{
			this.SetPosition(((float)Screen.width - this.windowRect.width) / 2f, ((float)Screen.height - this.windowRect.height) / 2f);
		}
	}

	public void SetPosition(float _x, float _y)
	{
		this.windowRect = new Rect(_x, _y, this.windowRect.width, this.windowRect.height);
	}

	public void SetSize(float _w, float _h)
	{
		this.windowRect = new Rect(((float)Screen.width - _w) / 2f, ((float)Screen.height - _h) / 2f, _w, _h);
	}

	public virtual void Update()
	{
	}

	public virtual void OnOpen()
	{
	}

	public virtual void OnClose()
	{
		Action onWindowClose = this.OnWindowClose;
		if (onWindowClose != null)
		{
			onWindowClose();
		}
		this.OnWindowClose = null;
	}

	public virtual void OnXPressed()
	{
		this.windowManager.Close(this, false);
	}

	public virtual PlayerActionsBase GetActionSet()
	{
		return this.playerUI.playerInput.GUIActions;
	}

	public virtual bool HasActionSet()
	{
		return true;
	}

	public override bool Equals(object obj)
	{
		return obj is GUIWindow && ((GUIWindow)obj).id.Equals(this.id);
	}

	public override int GetHashCode()
	{
		return this.id.GetHashCode();
	}

	public virtual void Cleanup()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string id;

	public bool bActionSetEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDrawBackground;

	public Rect internalWindowRect;

	public bool isShowing;

	public bool isModal;

	public bool alwaysUsesMouseCursor;

	public bool isEscClosable;

	public bool isInputActive;

	public bool isDimBackground;

	public GUIWindowManager windowManager;

	public NGUIWindowManager nguiWindowManager;

	public LocalPlayerUI playerUI;

	public Matrix4x4 matrix = Matrix4x4.identity;

	public bool bCenterWindow;

	public string openWindowOnEsc = string.Empty;

	public Action OnWindowClose;
}
