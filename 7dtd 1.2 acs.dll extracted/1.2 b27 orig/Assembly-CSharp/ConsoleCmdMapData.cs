﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMapData : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"mapdata"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\nclear - reset player map data\nprefab - save prefabs to mapdata.png\nstart - save start points to mapdata.png";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Writes some map data to an image";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.getHelp());
			return;
		}
		string a = _params[0].ToLower();
		if (a == "clear")
		{
			GameManager.Instance.World.GetPrimaryPlayer().ChunkObserver.mapDatabase.Clear();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Map cleared");
			return;
		}
		ConsoleCmdMapData.EMode emode = ConsoleCmdMapData.EMode.Prefabs;
		if (a == "start")
		{
			emode = ConsoleCmdMapData.EMode.StartPoints;
		}
		IChunkProvider chunkProvider = GameManager.Instance.World.ChunkClusters[0].ChunkProvider;
		Vector2i worldSize = chunkProvider.GetWorldSize();
		Texture2D texture2D = new Texture2D(worldSize.x, worldSize.y);
		Color[] pixels = texture2D.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color(0f, 0f, 0f, 0f);
		}
		if (emode == ConsoleCmdMapData.EMode.Prefabs)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = chunkProvider.GetDynamicPrefabDecorator();
			List<PrefabInstance> list = (dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.GetDynamicPrefabs() : null;
			if (list == null)
			{
				return;
			}
			foreach (PrefabInstance prefabInstance in list)
			{
				this.setRect(worldSize, prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxSize, pixels, ConsoleCmdMapData.EColorChannel.Green);
			}
		}
		if (emode == ConsoleCmdMapData.EMode.StartPoints)
		{
			SpawnPointList spawnPointList = chunkProvider.GetSpawnPointList();
			if (spawnPointList == null)
			{
				return;
			}
			foreach (SpawnPoint spawnPoint in spawnPointList)
			{
				this.setRect(worldSize, new Vector3i(spawnPoint.spawnPosition.position) - new Vector3i(3, 0, 3), new Vector3i(7, 0, 7), pixels, ConsoleCmdMapData.EColorChannel.Blue);
			}
		}
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, "mapdata.png");
		UnityEngine.Object.Destroy(texture2D);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved mapdata.png and put " + emode.ToStringCached<ConsoleCmdMapData.EMode>() + " into it");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setRect(Vector2i _worldSize, Vector3i _pos, Vector3i _size, Color[] _cols, ConsoleCmdMapData.EColorChannel _channel)
	{
		int num = _worldSize.x / 2 + _pos.x;
		int num2 = _worldSize.y / 2 + _pos.z;
		for (int i = 0; i < _size.x; i++)
		{
			for (int j = 0; j < _size.z; j++)
			{
				int num3 = i + num + (j + num2) * _worldSize.x;
				if (num3 >= 0 && num3 < _cols.Length)
				{
					Color color = _cols[num3];
					switch (_channel)
					{
					case ConsoleCmdMapData.EColorChannel.Red:
						color.r = 1f;
						color.a = 1f;
						break;
					case ConsoleCmdMapData.EColorChannel.Green:
						color.g = 1f;
						color.a = 1f;
						break;
					case ConsoleCmdMapData.EColorChannel.Blue:
						color.b = 1f;
						color.a = 1f;
						break;
					}
					_cols[num3] = color;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EMode
	{
		Prefabs,
		StartPoints
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EColorChannel
	{
		Red,
		Green,
		Blue
	}
}
