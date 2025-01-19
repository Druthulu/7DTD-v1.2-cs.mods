using System;
using GameSparks.Platforms;
using UnityEngine;

public class GameSparksUnity : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		base.gameObject.AddComponent<DefaultPlatform>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		if (GameSparksSettings.PreviewBuild)
		{
			GUILayout.BeginArea(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height));
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Space(10f);
			GUILayout.Label("GameSparks Preview mode", new GUILayoutOption[]
			{
				GUILayout.Width(200f),
				GUILayout.Height(25f)
			});
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}

	public GameSparksSettings settings;
}
