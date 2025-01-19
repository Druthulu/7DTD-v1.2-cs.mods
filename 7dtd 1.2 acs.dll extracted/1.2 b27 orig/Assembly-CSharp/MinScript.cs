﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class MinScript
{
	public static string ConvertFromUIText(string _text)
	{
		return _text.Replace("\n", "^");
	}

	public static string ConvertToUIText(string _text)
	{
		if (_text == null)
		{
			return string.Empty;
		}
		return _text.Replace("^", "\n");
	}

	public void SetText(string _text)
	{
		int num = 0;
		int length = _text.Length;
		int num2;
		for (int i = 0; i < length; i = num2 + 1)
		{
			num2 = _text.IndexOf('^', i, length - i);
			if (num2 < 0)
			{
				num2 = length;
			}
			while (i < length && _text[i] == ' ')
			{
				i++;
			}
			int num3 = num2 - i;
			if (num3 > 0 && _text[i] != '/')
			{
				int num4 = _text.IndexOf(' ', i, num3);
				if (num4 < 0)
				{
					num4 = num2;
				}
				string key = _text.Substring(i, num4 - i);
				MinScript.CmdLine item;
				if (MinScript.nameToCmds.TryGetValue(key, out item.command))
				{
					num4++;
					item.parameters = null;
					int num5 = num2 - num4;
					if (num5 > 0)
					{
						item.parameters = _text.Substring(num4, num5);
					}
					this.commandList.Add(item);
				}
			}
			num++;
		}
	}

	public void Reset()
	{
		this.curIndex = -1;
	}

	public void Restart()
	{
		this.curIndex = 0;
		this.sleep = 0f;
	}

	public void Run(SleeperVolume _sv, EntityPlayer _player, float _countScale)
	{
		if (this.commandList == null)
		{
			return;
		}
		this.player = _player;
		this.countScale = _countScale;
		this.curIndex = 0;
		this.sleep = 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsRunning()
	{
		return this.curIndex >= 0;
	}

	public void Tick(SleeperVolume _sv)
	{
		if (this.curIndex < 0)
		{
			return;
		}
		if (this.sleep > 0f)
		{
			this.sleep -= 0.05f;
			if (this.sleep > 0f)
			{
				return;
			}
		}
		for (;;)
		{
			MinScript.CmdLine cmdLine = this.commandList[this.curIndex];
			ushort command = cmdLine.command;
			if (command <= 4)
			{
				if (command != 1)
				{
					if (command == 4)
					{
						this.sleep = float.Parse(cmdLine.parameters ?? "1");
					}
				}
				else
				{
					Log.Out("MinScript " + cmdLine.parameters);
				}
			}
			else if (command != 40)
			{
				switch (command)
				{
				case 50:
					if (cmdLine.parameters != null)
					{
						string[] array = cmdLine.parameters.Split(' ', StringSplitOptions.None);
						if (array.Length >= 1)
						{
							float num = 1f;
							float num2 = 1f;
							if (array.Length >= 2)
							{
								num = float.Parse(array[1]);
								num2 = num;
								if (array.Length >= 3)
								{
									num2 = float.Parse(array[2]);
								}
							}
							_sv.AddSpawnCount(array[0], num * this.countScale, num2 * this.countScale);
						}
					}
					break;
				case 51:
				{
					int num3 = int.Parse(cmdLine.parameters ?? "0");
					if (_sv.GetAliveCount() > num3)
					{
						return;
					}
					break;
				}
				case 52:
					if (this.player)
					{
						if (cmdLine.parameters != null)
						{
							byte trigger = (byte)int.Parse(cmdLine.parameters);
							this.player.world.triggerManager.Trigger(this.player, _sv.PrefabInstance, trigger);
						}
						else
						{
							Log.Warning("MinScript trigger !param {0}", new object[]
							{
								_sv
							});
						}
					}
					break;
				}
			}
			else
			{
				GameManager.Instance.PlaySoundAtPositionServer(_sv.Center, cmdLine.parameters, AudioRolloffMode.Linear, 100, _sv.GetPlayerTouchedToUpdateId());
			}
			int num4 = this.curIndex + 1;
			this.curIndex = num4;
			if (num4 >= this.commandList.Count)
			{
				goto Block_18;
			}
			if (this.sleep > 0f)
			{
				return;
			}
		}
		return;
		Block_18:
		this.curIndex = -1;
	}

	public static MinScript Read(BinaryReader _br)
	{
		_br.ReadByte();
		MinScript minScript = new MinScript();
		minScript.curIndex = (int)_br.ReadInt16();
		if (minScript.curIndex >= 0)
		{
			minScript.sleep = _br.ReadSingle();
		}
		int num = (int)_br.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			MinScript.CmdLine item;
			item.command = _br.ReadUInt16();
			item.parameters = null;
			int num2 = (int)_br.ReadByte();
			if (num2 > 0)
			{
				_br.Read(MinScript.tempBytes, 0, num2);
				int chars = Encoding.UTF8.GetChars(MinScript.tempBytes, 0, num2, MinScript.tempChars, 0);
				item.parameters = new string(MinScript.tempChars, 0, chars);
			}
			minScript.commandList.Add(item);
		}
		return minScript;
	}

	public bool HasData()
	{
		return this.commandList.Count > 0;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(1);
		_bw.Write((short)this.curIndex);
		if (this.curIndex >= 0)
		{
			_bw.Write(this.sleep);
		}
		int count = this.commandList.Count;
		_bw.Write((ushort)count);
		for (int i = 0; i < count; i++)
		{
			MinScript.CmdLine cmdLine = this.commandList[i];
			_bw.Write(cmdLine.command);
			if (cmdLine.parameters != null && cmdLine.parameters.Length > 0)
			{
				for (int j = 0; j < cmdLine.parameters.Length; j++)
				{
					MinScript.tempChars[j] = cmdLine.parameters[j];
				}
				byte b = (byte)Encoding.UTF8.GetBytes(MinScript.tempChars, 0, cmdLine.parameters.Length, MinScript.tempBytes, 0);
				_bw.Write(b);
				_bw.Write(MinScript.tempBytes, 0, (int)b);
			}
			else
			{
				_bw.Write(0);
			}
		}
	}

	public override string ToString()
	{
		return string.Format("cmds {0}, index {1}, sleep {2}", this.commandList.Count, this.curIndex, this.sleep);
	}

	[Conditional("DEBUG_MINSCRIPTLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogMS(string format, params object[] args)
	{
		format = string.Format("{0} {1} MinScript {2}", GameManager.frameTime.ToCultureInvariantString(), GameManager.frameCount, format);
		Log.Warning(format, args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const char cLineSepChar = '^';

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cLineSepStr = "^";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<MinScript.CmdLine> commandList = new List<MinScript.CmdLine>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int curIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public float sleep;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdNop = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdLog = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdLabel = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdLoop = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdSleep = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdSound = 40;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdSpawn = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdWaitAlive = 51;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdTrigger = 52;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, ushort> nameToCmds = new Dictionary<string, ushort>
	{
		{
			"log",
			1
		},
		{
			"label",
			2
		},
		{
			"loop",
			3
		},
		{
			"sleep",
			4
		},
		{
			"sound",
			40
		},
		{
			"spawn",
			50
		},
		{
			"trigger",
			52
		},
		{
			"waitalive",
			51
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] tempBytes = new byte[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public static char[] tempChars = new char[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public float countScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CmdLine
	{
		public ushort command;

		public string parameters;
	}
}
