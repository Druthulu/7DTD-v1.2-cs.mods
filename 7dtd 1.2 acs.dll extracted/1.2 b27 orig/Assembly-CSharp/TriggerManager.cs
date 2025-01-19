using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager
{
	public bool ShowNavObjects
	{
		get
		{
			return this.showNavObjects;
		}
		set
		{
			this.showNavObjects = value;
			this.HandleNavObjects(this.showNavObjects);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleNavObjects(bool enabled)
	{
		foreach (PrefabTriggerData prefabTriggerData in this.PrefabDataDict.Values)
		{
			if (enabled)
			{
				prefabTriggerData.SetupTriggerTestNavObjects();
			}
			else
			{
				prefabTriggerData.RemoveTriggerTestNavObjects();
			}
		}
	}

	public void AddPrefabData(PrefabInstance instance, int entityID)
	{
		PrefabTriggerData prefabTriggerData;
		if (!this.PrefabDataDict.ContainsKey(instance))
		{
			prefabTriggerData = new PrefabTriggerData(instance)
			{
				Owner = this
			};
			if (this.ShowNavObjects)
			{
				prefabTriggerData.SetupTriggerTestNavObjects();
			}
			this.PrefabDataDict.Add(instance, prefabTriggerData);
		}
		prefabTriggerData = this.PrefabDataDict[instance];
		prefabTriggerData.RefreshTriggers();
		prefabTriggerData.AddPlayerInArea(entityID);
	}

	public void RefreshTriggers(PrefabInstance instance, FastTags<TagGroup.Global> questTags)
	{
		PrefabTriggerData prefabTriggerData;
		if (!this.PrefabDataDict.ContainsKey(instance))
		{
			prefabTriggerData = new PrefabTriggerData(instance)
			{
				Owner = this
			};
			this.PrefabDataDict.Add(instance, prefabTriggerData);
		}
		else
		{
			prefabTriggerData = this.PrefabDataDict[instance];
			prefabTriggerData.RemoveTriggerTestNavObjects();
			prefabTriggerData.ResetData();
		}
		prefabTriggerData.ResetTriggers();
		prefabTriggerData.RefreshTriggersForQuest(questTags);
		prefabTriggerData.HandleNeedTriggers();
	}

	public void Trigger(EntityPlayer player, PrefabInstance instance, byte trigger)
	{
		PrefabTriggerData prefabTriggerData;
		if (this.PrefabDataDict.TryGetValue(instance, out prefabTriggerData))
		{
			prefabTriggerData.Trigger(player, trigger);
		}
	}

	public void TriggerBlocks(EntityPlayer player, PrefabInstance instance, BlockTrigger trigger)
	{
		if (!trigger.HasAnyTriggers())
		{
			return;
		}
		if (this.PrefabDataDict.ContainsKey(instance))
		{
			this.PrefabDataDict[instance].Trigger(player, trigger);
		}
	}

	public void TriggerBlocks(EntityPlayer player, PrefabInstance instance, TriggerVolume trigger)
	{
		if (!trigger.HasAnyTriggers())
		{
			return;
		}
		if (this.PrefabDataDict.ContainsKey(instance))
		{
			this.PrefabDataDict[instance].Trigger(player, trigger);
		}
	}

	public void RemovePlayer(PrefabInstance instance, int entityID)
	{
		if (this.PrefabDataDict.ContainsKey(instance))
		{
			this.PrefabDataDict[instance].RemovePlayerInArea(entityID);
		}
	}

	public void RemovePrefabData(PrefabInstance instance)
	{
		if (this.PrefabDataDict.ContainsKey(instance))
		{
			this.PrefabDataDict[instance].RemoveTriggerTestNavObjects();
			this.PrefabDataDict.Remove(instance);
		}
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		for (int i = this.UpdateList.Count - 1; i >= 0; i--)
		{
			this.UpdateList[i].Update(deltaTime);
		}
	}

	public List<byte> GetTriggerLayers()
	{
		List<byte> list = new List<byte>();
		foreach (PrefabTriggerData prefabTriggerData in this.PrefabDataDict.Values)
		{
			for (int i = 0; i < prefabTriggerData.TriggeredLayers.Count; i++)
			{
				if (!list.Contains(prefabTriggerData.TriggeredLayers[i]))
				{
					list.Add(prefabTriggerData.TriggeredLayers[i]);
				}
			}
			for (int j = 0; j < prefabTriggerData.TriggeredByLayers.Count; j++)
			{
				if (!list.Contains(prefabTriggerData.TriggeredByLayers[j]))
				{
					list.Add(prefabTriggerData.TriggeredByLayers[j]);
				}
			}
		}
		return list;
	}

	public void AddToUpdateList(PrefabTriggerData prefabTriggerData)
	{
		if (!this.UpdateList.Contains(prefabTriggerData))
		{
			this.UpdateList.Add(prefabTriggerData);
		}
	}

	public void RemoveFromUpdateList(PrefabTriggerData prefabTriggerData)
	{
		if (this.UpdateList.Contains(prefabTriggerData))
		{
			this.UpdateList.Remove(prefabTriggerData);
		}
	}

	public void RemoveFromUpdateList(PrefabInstance instance)
	{
		for (int i = this.UpdateList.Count - 1; i >= 0; i--)
		{
			if (this.UpdateList[i].PrefabInstance == instance)
			{
				this.UpdateList.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PrefabInstance, PrefabTriggerData> PrefabDataDict = new Dictionary<PrefabInstance, PrefabTriggerData>();

	public List<PrefabTriggerData> UpdateList = new List<PrefabTriggerData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showNavObjects;
}
