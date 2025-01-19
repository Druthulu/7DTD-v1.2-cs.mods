using System;
using UnityEngine;

public class GUIWindowEditBlockSpawnEntity : GUIWindow
{
	public GUIWindowEditBlockSpawnEntity(GameManager _gm) : base(GUIWindowEditBlockSpawnEntity.ID, 580, 280, true)
	{
	}

	public void SetBlockValue(Vector3i _blockPos, BlockValue _bv)
	{
		this.blockPos = _blockPos;
		this.blockValue = _bv;
		this.compEntitiesToSpawn = new GUICompList(new Rect(0f, 0f, 350f, 200f));
		BlockSpawnEntity blockSpawnEntity = _bv.Block as BlockSpawnEntity;
		if (blockSpawnEntity == null)
		{
			return;
		}
		foreach (string line in blockSpawnEntity.spawnClasses)
		{
			this.compEntitiesToSpawn.AddLine(line);
		}
		this.selectedEntityClass = blockSpawnEntity.spawnClasses[(int)_bv.meta];
		this.compEntitiesToSpawn.SelectEntry(this.selectedEntityClass);
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		GUILayout.Space(20f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		GUILayout.Label("Select entity to spawn:", new GUILayoutOption[]
		{
			GUILayout.Width(180f)
		});
		GUILayout.Space(5f);
		this.compEntitiesToSpawn.OnGUILayout();
		this.selectedEntityClass = this.compEntitiesToSpawn.SelectedEntry;
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		if (base.GUILayoutButton("Ok"))
		{
			this.blockValue.meta = (byte)this.compEntitiesToSpawn.SelectedItemIndex;
			GameManager.Instance.World.SetBlockRPC(0, this.blockPos, this.blockValue);
			this.windowManager.Close(this, false);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.EndVertical();
	}

	public static string ID = typeof(GUIWindowEditBlockSpawnEntity).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUICompList compEntitiesToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dXZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dYm;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dYp;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedEntityClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;
}
