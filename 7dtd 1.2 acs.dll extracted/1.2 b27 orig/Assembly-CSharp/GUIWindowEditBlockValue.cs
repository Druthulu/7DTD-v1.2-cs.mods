using System;
using UnityEngine;

public class GUIWindowEditBlockValue : GUIWindow
{
	public GUIWindowEditBlockValue(GameManager _gm) : base(GUIWindowEditBlockValue.ID, 220, 140, true)
	{
		this.gameManager = _gm;
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		GUILayout.Space(20f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		this.blockValue.hasdecal = GUILayout.Toggle(this.blockValue.hasdecal, " Decal on", new GUILayoutOption[]
		{
			GUILayout.Width(80f)
		});
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		GUILayout.Label("Texture idx:", new GUILayoutOption[]
		{
			GUILayout.Width(80f)
		});
		this.blockValue.decaltex = (byte)int.Parse(GUILayout.TextField(this.blockValue.decaltex.ToString(), new GUILayoutOption[]
		{
			GUILayout.Width(40f)
		}));
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		if (base.GUILayoutButton("Ok"))
		{
			if (this.blockValue.hasdecal)
			{
				this.blockValue.decalface = this.blockFace;
			}
			else
			{
				this.blockValue.decalface = BlockFace.Top;
			}
			this.gameManager.World.SetBlockRPC(this.blockPos, this.blockValue);
			this.windowManager.Close(this, false);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.EndVertical();
	}

	public void SetBlock(Vector3i _blockPos, BlockFace _blockFace)
	{
		this.blockFace = _blockFace;
		this.blockPos = _blockPos;
		this.blockValue = this.gameManager.World.GetBlock(_blockPos);
	}

	public static string ID = typeof(GUIWindowEditBlockValue).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFace blockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameManager gameManager;
}
