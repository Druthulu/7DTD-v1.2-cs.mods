using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCategory
{
	public ISelectionBoxCallback callback { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public SelectionCategory(string _name, Transform _transform, Color _colActive, Color _colInactive, Color _colFaceSelected, bool _bCollider, string _tag, ISelectionBoxCallback _callback, int _layer = 0)
	{
		this.name = _name;
		this.transform = _transform;
		this.colActive = _colActive;
		this.colInactive = _colInactive;
		this.colFaceSelected = _colFaceSelected;
		this.bCollider = _bCollider;
		this.tag = _tag;
		this.callback = _callback;
		this.layer = _layer;
	}

	public void SetCallback(ISelectionBoxCallback _callback)
	{
		this.callback = _callback;
	}

	public bool IsVisible()
	{
		return this.transform.gameObject.activeSelf;
	}

	public void SetVisible(bool _bVisible)
	{
		this.transform.gameObject.SetActive(_bVisible);
		string a = this.name;
		if (!(a == "SleeperVolume"))
		{
			if (a == "POIMarker")
			{
				POIMarkerToolManager.UpdateAllColors();
				bool bShow;
				if (_bVisible)
				{
					ValueTuple<SelectionCategory, SelectionBox>? selection = SelectionBoxManager.Instance.Selection;
					bShow = (((selection != null) ? selection.GetValueOrDefault().Item1 : null) != null);
				}
				else
				{
					bShow = false;
				}
				POIMarkerToolManager.ShowPOIMarkers(bShow);
			}
		}
		else
		{
			SleeperVolumeToolManager.SetVisible(_bVisible);
		}
		if (!_bVisible)
		{
			ValueTuple<SelectionCategory, SelectionBox>? selection = SelectionBoxManager.Instance.Selection;
			if (((selection != null) ? selection.GetValueOrDefault().Item1 : null) == this)
			{
				SelectionBoxManager.Instance.Deactivate();
			}
		}
	}

	public void SetCaptionVisibility(bool _visible)
	{
		foreach (KeyValuePair<string, SelectionBox> keyValuePair in this.boxes)
		{
			keyValuePair.Value.SetCaptionVisibility(_visible);
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, SelectionBox> keyValuePair in this.boxes)
		{
			UnityEngine.Object.Destroy(keyValuePair.Value.gameObject);
		}
		this.boxes.Clear();
		if (this.name == "SleeperVolume")
		{
			SleeperVolumeToolManager.ClearSleeperVolumes();
		}
	}

	public SelectionBox AddBox(string _name, Vector3 _pos, Vector3i _size, bool _bDrawDirection = false, bool _bAlwaysDrawDirection = false)
	{
		SelectionBox selectionBox;
		if (this.boxes.TryGetValue(_name, out selectionBox))
		{
			this.RemoveBox(_name);
		}
		Transform transform = new GameObject(_name).transform;
		transform.parent = this.transform;
		SelectionBox selectionBox2 = transform.gameObject.AddComponent<SelectionBox>();
		selectionBox2.SetOwner(this);
		selectionBox2.SetAllFacesColor(this.colInactive, true);
		selectionBox2.bDrawDirection = _bDrawDirection;
		selectionBox2.bAlwaysDrawDirection = _bAlwaysDrawDirection;
		selectionBox2.SetPositionAndSize(_pos, _size);
		if (this.bCollider)
		{
			selectionBox2.EnableCollider(this.tag, this.layer);
		}
		this.boxes[_name] = selectionBox2;
		if (this.name == "SleeperVolume")
		{
			SleeperVolumeToolManager.RegisterSleeperVolume(selectionBox2);
		}
		return selectionBox2;
	}

	public SelectionBox GetBox(string _name)
	{
		SelectionBox result;
		this.boxes.TryGetValue(_name, out result);
		return result;
	}

	public void RenameBox(string _name, string _newName)
	{
		if (_name.Equals(_newName))
		{
			return;
		}
		SelectionBox selectionBox;
		if (!this.boxes.TryGetValue(_name, out selectionBox))
		{
			return;
		}
		selectionBox.name = _newName;
		this.boxes[_newName] = selectionBox;
		this.boxes.Remove(_name);
	}

	public void RemoveBox(string _name)
	{
		SelectionBox selectionBox;
		if (!this.boxes.TryGetValue(_name, out selectionBox))
		{
			return;
		}
		ValueTuple<SelectionCategory, SelectionBox>? valueTuple;
		if (((SelectionBoxManager.Instance.Selection != null) ? valueTuple.GetValueOrDefault().Item2 : null) == selectionBox)
		{
			SelectionBoxManager.Instance.Deactivate();
		}
		if (this.name == "SleeperVolume")
		{
			SleeperVolumeToolManager.UnRegisterSleeperVolume(selectionBox);
		}
		UnityEngine.Object.Destroy(selectionBox.gameObject);
		this.boxes.Remove(_name);
	}

	public readonly string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Transform transform;

	public readonly Color colActive;

	public readonly Color colInactive;

	public readonly Color colFaceSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool bCollider;

	public readonly string tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int layer;

	public readonly Dictionary<string, SelectionBox> boxes = new Dictionary<string, SelectionBox>();
}
