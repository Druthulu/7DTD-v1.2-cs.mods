using System;
using System.Collections.Generic;
using UnityEngine;

public class GameGraphManager
{
	public static GameGraphManager Create(EntityPlayerLocal player)
	{
		GameGraphManager gameGraphManager = new GameGraphManager();
		gameGraphManager.player = player;
		gameGraphManager.Init();
		return gameGraphManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (!GameGraphManager.whiteTex)
		{
			GameGraphManager.whiteTex = new Texture2D(1, 1);
			GameGraphManager.whiteTex.FillTexture(Color.white, true, false);
		}
	}

	public void Destroy()
	{
		if (GameGraphManager.whiteTex)
		{
			UnityEngine.Object.Destroy(GameGraphManager.whiteTex);
		}
	}

	public void Add(string name, GameGraphManager.Graph.Callback callback, int sampleCount, float maxValue, float markerValue = 0f)
	{
		GameGraphManager.Graph graph = this.FindGraph(name);
		if (graph != null)
		{
			this.graphs.Remove(graph);
		}
		if (sampleCount > 0)
		{
			GameGraphManager.Graph graph2 = new GameGraphManager.Graph(this, name, sampleCount, maxValue, markerValue);
			this.graphs.Add(graph2);
			graph2.callback = callback;
		}
	}

	public void AddCVar(string name, int count, string cvarName, float maxValue, float markerValue = 0f)
	{
		GameGraphManager.Graph graph = this.FindGraph(name);
		if (graph != null)
		{
			this.graphs.Remove(graph);
		}
		if (count > 0)
		{
			GameGraphManager.Graph graph2 = new GameGraphManager.Graph(this, name, count, maxValue, markerValue);
			this.graphs.Add(graph2);
			graph2.cvarName = cvarName;
		}
	}

	public void AddPassiveEffect(string name, int count, PassiveEffects passiveEffect, float maxValue, float markerValue = 0f)
	{
		GameGraphManager.Graph graph = this.FindGraph(name);
		if (graph != null)
		{
			this.graphs.Remove(graph);
		}
		if (count > 0)
		{
			GameGraphManager.Graph graph2 = new GameGraphManager.Graph(this, name, count, maxValue, markerValue);
			this.graphs.Add(graph2);
			graph2.passiveEffect = passiveEffect;
		}
	}

	public void AddStat(string name, int count, string statName, float maxValue, float markerValue = 0f)
	{
		GameGraphManager.Graph graph = this.FindGraph(name);
		if (graph != null)
		{
			this.graphs.Remove(graph);
		}
		if (count > 0)
		{
			GameGraphManager.Graph graph2 = new GameGraphManager.Graph(this, name, count, maxValue, markerValue);
			this.graphs.Add(graph2);
			graph2.statName = statName.ToLower();
		}
	}

	public void RemoveAll()
	{
		this.graphs.Clear();
	}

	public GameGraphManager.Graph FindGraph(string name)
	{
		for (int i = 0; i < this.graphs.Count; i++)
		{
			GameGraphManager.Graph graph = this.graphs[i];
			if (graph.name == name)
			{
				return graph;
			}
		}
		return null;
	}

	public void Draw()
	{
		bool flag = Event.current.type == EventType.Repaint;
		float num = 1f;
		for (int i = 0; i < this.graphs.Count; i++)
		{
			GameGraphManager.Graph graph = this.graphs[i];
			if (flag)
			{
				graph.UpdateValues();
			}
			graph.pos.x = 2f;
			graph.pos.y = num;
			graph.Draw();
			num += (float)(this.graphHeight + 2);
		}
	}

	public void SetHeight(int _height)
	{
		this.graphHeight = _height;
		this.graphHeight = Mathf.Clamp(this.graphHeight, 1, 2100);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D whiteTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameGraphManager.Graph> graphs = new List<GameGraphManager.Graph>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int graphHeight = 100;

	public class Graph
	{
		public Graph(GameGraphManager _manager, string _name, int _count, float _maxValue, float _markerValue)
		{
			this.manager = _manager;
			this.name = _name;
			this.count = _count;
			this.count = Mathf.Clamp(this.count, 1, 4096);
			this.maxValue = _maxValue;
			this.markerValue = _markerValue;
			this.values = new float[this.count];
		}

		public void AddValue(float value)
		{
			this.index = (this.index + 1) % this.count;
			this.values[this.index] = value;
		}

		public void Draw()
		{
			Texture whiteTex = GameGraphManager.whiteTex;
			float width = (float)this.count * 2f + 2f;
			int graphHeight = this.manager.graphHeight;
			GUI.color = Color.white;
			GUI.DrawTexture(new Rect(this.pos.x, this.pos.y, width, (float)(graphHeight + 2)), whiteTex, ScaleMode.StretchToFill, false, 0f, new Color(0f, 0f, 0f, 0.9f), 0f, 0f);
			int num = this.index + 1;
			for (int i = 0; i < this.count; i++)
			{
				float num2 = this.values[num % this.count];
				num2 /= this.maxValue;
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				float num3 = (float)graphHeight * num2;
				num3 = (float)((int)(num3 + 0.5f));
				Color color = new Color(1f, num2, num2 * 0.6f + 0.4f);
				GUI.DrawTexture(new Rect(this.pos.x + 1f + (float)i * 2f, this.pos.y + 1f + (float)graphHeight - num3, 2f, num3), whiteTex, ScaleMode.StretchToFill, false, 0f, color, 0f, 0f);
				num++;
			}
			if (this.markerValue > 0f)
			{
				GUI.DrawTexture(new Rect(this.pos.x, this.pos.y + (float)graphHeight - this.markerValue / this.maxValue * (float)graphHeight, width, 1f), whiteTex, ScaleMode.StretchToFill, true, 0f, new Color(1f, 1f, 0f, 0.6f), 0f, 0f);
			}
			GUI.color = new Color(0.6f, 0.6f, 1f);
			GUI.Label(new Rect(this.pos.x + 1f, this.pos.y + 1f, 256f, 256f), string.Format("{0} {1}", this.name, this.values[this.index]));
		}

		public void UpdateValues()
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			EntityPlayerLocal player = this.manager.player;
			if (this.callback != null)
			{
				float value = this.values[this.index];
				if (this.callback(ref value))
				{
					this.AddValue(value);
					return;
				}
			}
			else if (!string.IsNullOrEmpty(this.cvarName))
			{
				float cvar = player.GetCVar(this.cvarName);
				if (cvar != this.values[this.index])
				{
					this.AddValue(cvar);
					return;
				}
			}
			else if (this.passiveEffect != PassiveEffects.None)
			{
				float value2 = EffectManager.GetValue(this.passiveEffect, null, 0f, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				if (value2 != this.values[this.index])
				{
					this.AddValue(value2);
					return;
				}
			}
			else if (!string.IsNullOrEmpty(this.statName))
			{
				float num = 0f;
				string a = this.statName;
				if (!(a == "health"))
				{
					if (!(a == "stamina"))
					{
						if (!(a == "coretemp"))
						{
							if (a == "water")
							{
								num = player.Stats.Water.Value;
							}
						}
						else
						{
							num = player.Stats.CoreTemp.Value;
						}
					}
					else
					{
						num = player.Stats.Stamina.Value;
					}
				}
				else
				{
					num = player.Stats.Health.Value;
				}
				if (num != this.values[this.index])
				{
					this.AddValue(num);
				}
			}
		}

		public string name;

		public GameGraphManager.Graph.Callback callback;

		public string cvarName;

		public PassiveEffects passiveEffect;

		public string statName;

		public Vector2 pos;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameGraphManager manager;

		[PublicizedFrom(EAccessModifier.Private)]
		public int count;

		[PublicizedFrom(EAccessModifier.Private)]
		public float maxValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public float markerValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] values;

		[PublicizedFrom(EAccessModifier.Private)]
		public int index;

		public delegate bool Callback(ref float value);
	}
}
