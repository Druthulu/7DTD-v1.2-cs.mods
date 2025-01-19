using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPlaceObserver : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Place a chunk observer on a given position.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. chunkobserver add <x> <z> [size]\n  2. chunkobserver remove <x> <z>\n  3. chunkobserver list\n1. Place an observer on the chunk that contains the coordinate x/z.\n   Optionally specifying the box radius in chunks, defaulting to 1.\n2. Remove the observer from the chunk with the coordinate, if any.\n3. List all currently placed observers";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"chunkobserver",
			"co"
		};
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 1 && _params[0].EqualsCaseInsensitive("list"))
		{
			this.listObservers();
			return;
		}
		if ((!_params[0].EqualsCaseInsensitive("add") || (_params.Count != 3 && _params.Count != 4)) && (!_params[0].EqualsCaseInsensitive("remove") || _params.Count != 3))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal arguments");
			return;
		}
		int x;
		if (!int.TryParse(_params[1], out x))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x coordinate is not a valid integer");
			return;
		}
		int y;
		if (!int.TryParse(_params[2], out y))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z coordinate is not a valid integer");
			return;
		}
		int num;
		if (_params.Count == 4)
		{
			if (!int.TryParse(_params[3], out num) || num < 1 || num > 15)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given size is not a valid integer or exceeds the allowed range of 1-15");
				return;
			}
		}
		else
		{
			num = 1;
		}
		Vector2i pos = new Vector2i(x, y);
		if (_params[0].EqualsCaseInsensitive("remove"))
		{
			if (this.removeObserver(pos))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Observer removed from " + pos.ToString());
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No observer on " + pos.ToString());
			return;
		}
		else
		{
			if (this.addObserver(pos, num))
			{
				int num2 = 2 * (num - 1) + 1;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
				{
					"Observer added to ",
					pos.ToString(),
					" with radius ",
					num.ToString(),
					" (size ",
					num2.ToString(),
					"x",
					num2.ToString(),
					")"
				}));
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Already an observer on " + pos.ToString());
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addObserver(Vector2i _pos, int _radius)
	{
		if (this.observers.dict.ContainsKey(_pos))
		{
			return false;
		}
		Vector3 initialPosition = new Vector3((float)_pos.x, 0f, (float)_pos.y);
		ChunkManager.ChunkObserver value = GameManager.Instance.AddChunkObserver(initialPosition, false, _radius, -1);
		this.observers.Add(_pos, value);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeObserver(Vector2i _pos)
	{
		if (!this.observers.dict.ContainsKey(_pos))
		{
			return false;
		}
		ChunkManager.ChunkObserver observer = this.observers.dict[_pos];
		GameManager.Instance.RemoveChunkObserver(observer);
		this.observers.Remove(_pos);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void listObservers()
	{
		int num = 0;
		foreach (KeyValuePair<Vector2i, ChunkManager.ChunkObserver> keyValuePair in this.observers.dict)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format(" {0,3}: {1}", ++num, keyValuePair.Key.ToString()));
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(num.ToString() + " observers");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MapVisitor mapVisitor;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryList<Vector2i, ChunkManager.ChunkObserver> observers = new DictionaryList<Vector2i, ChunkManager.ChunkObserver>();
}
