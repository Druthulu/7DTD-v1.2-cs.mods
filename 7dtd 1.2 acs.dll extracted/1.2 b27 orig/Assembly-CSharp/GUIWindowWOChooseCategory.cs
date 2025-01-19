using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIWindowWOChooseCategory : GUIWindow
{
	public GUIWindowWOChooseCategory() : base(GUIWindowWOChooseCategory.ID, 0, 0, true)
	{
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);
		GUILayout.Label(new GUIContent("Choose properties:"), Array.Empty<GUILayoutOption>());
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		foreach (KeyValuePair<string, SelectionCategory> keyValuePair in SelectionBoxManager.Instance.GetCategories())
		{
			SelectionCategory value = keyValuePair.Value;
			ISelectionBoxCallback callback = value.callback;
			if (callback != null && callback.OnSelectionBoxIsAvailable(value.name, EnumSelectionBoxAvailabilities.CanShowProperties))
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Space(20f);
				if (base.GUILayoutButton(value.name, GUILayout.Width(200f)))
				{
					value.callback.OnSelectionBoxShowProperties(true, this.windowManager);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}

	public override void OnOpen()
	{
		string value;
		string text;
		if (SelectionBoxManager.Instance.GetSelected(out value, out text))
		{
			foreach (KeyValuePair<string, SelectionCategory> keyValuePair in SelectionBoxManager.Instance.GetCategories())
			{
				SelectionCategory value2 = keyValuePair.Value;
				if (value2.name.Equals(value))
				{
					ISelectionBoxCallback callback = value2.callback;
					if (callback != null && callback.OnSelectionBoxIsAvailable(value2.name, EnumSelectionBoxAvailabilities.CanShowProperties))
					{
						this.windowManager.Close(this, false);
						value2.callback.OnSelectionBoxShowProperties(true, this.windowManager);
						break;
					}
					break;
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<string, SelectionCategory> keyValuePair2 in SelectionBoxManager.Instance.GetCategories())
		{
			SelectionCategory value3 = keyValuePair2.Value;
			ISelectionBoxCallback callback2 = value3.callback;
			if (callback2 != null && callback2.OnSelectionBoxIsAvailable(value3.name, EnumSelectionBoxAvailabilities.CanShowProperties))
			{
				num++;
			}
		}
		base.SetSize(240f, (float)(50 + num * 30));
	}

	public static string ID = "GUIWindowWOChooseCategory";
}
