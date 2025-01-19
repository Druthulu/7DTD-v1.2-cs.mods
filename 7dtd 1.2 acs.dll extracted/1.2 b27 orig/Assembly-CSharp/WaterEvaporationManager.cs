﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterEvaporationManager : MonoBehaviour
{
	public void Start()
	{
	}

	public void Update()
	{
	}

	public static void UpdateEvaporation()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		WaterEvaporationManager.vRemovalList.Clear();
		WaterEvaporationManager.uRemovalList.Clear();
		DictionaryList<ulong, WaterEvaporationManager.BlockData> obj = WaterEvaporationManager.evapWalkList;
		lock (obj)
		{
			int num = WaterEvaporationManager.evapWalkIndex;
			while (num < WaterEvaporationManager.evapWalkList.Count && num < WaterEvaporationManager.evapWalkIndex + 15)
			{
				if (GameManager.Instance == null)
				{
					return;
				}
				if (GameManager.Instance.World == null)
				{
					return;
				}
				if (GameManager.Instance.World.GetWorldTime() > WaterEvaporationManager.evapWalkList.list[num].time + 1000UL)
				{
					WaterEvaporationManager.vRemovalList.Add(WaterEvaporationManager.evapWalkList.list[num].pos);
					WaterEvaporationManager.uRemovalList.Add(WaterEvaporationManager.evapWalkList.list[num].ID);
					GameManager.Instance.World.SetBlockRPC(WaterEvaporationManager.evapWalkList.list[num].pos, BlockValue.Air);
					if (WaterEvaporationManager.waterBlockID < 0)
					{
						WaterEvaporationManager.waterBlockID = ItemClass.GetItem("Water", false).ToBlockValue().type;
					}
					GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, WaterEvaporationManager.evapWalkList.list[num].pos, WaterEvaporationManager.waterBlockID, 1UL);
				}
				num++;
			}
			num = 0;
			for (int i = 0; i < WaterEvaporationManager.vRemovalList.Count; i++)
			{
				Vector3i blockPos = WaterEvaporationManager.vRemovalList[i];
				WaterEvaporationManager.evapWalkList.Remove(WaterEvaporationManager.uRemovalList[num++]);
				WaterEvaporationManager.RemoveFromEvapList(blockPos);
			}
			WaterEvaporationManager.evapWalkIndex += 15;
			if (WaterEvaporationManager.evapWalkIndex >= WaterEvaporationManager.evapWalkList.Count)
			{
				WaterEvaporationManager.evapWalkIndex = 0;
			}
		}
		WaterEvaporationManager.vRemovalList.Clear();
		WaterEvaporationManager.uRemovalList.Clear();
		obj = WaterEvaporationManager.restWalkList;
		lock (obj)
		{
			int num2 = WaterEvaporationManager.restWalkIndex;
			while (num2 < WaterEvaporationManager.restWalkList.Count && num2 < WaterEvaporationManager.restWalkIndex + 15)
			{
				if (GameManager.Instance == null)
				{
					return;
				}
				if (GameManager.Instance.World == null)
				{
					return;
				}
				if (GameManager.Instance.World.GetWorldTime() > WaterEvaporationManager.restWalkList.list[num2].time + 1000UL)
				{
					WaterEvaporationManager.vRemovalList.Add(WaterEvaporationManager.restWalkList.list[num2].pos);
					WaterEvaporationManager.uRemovalList.Add(WaterEvaporationManager.restWalkList.list[num2].ID);
					BlockValue block = GameManager.Instance.World.GetBlock(WaterEvaporationManager.restWalkList.list[num2].pos.x, WaterEvaporationManager.restWalkList.list[num2].pos.y, WaterEvaporationManager.restWalkList.list[num2].pos.z);
					BlockLiquidv2.SetBlockState(ref block, BlockLiquidv2.UpdateID.Evaporate);
					GameManager.Instance.World.SetBlockRPC(WaterEvaporationManager.restWalkList.list[num2].pos, block);
					if (WaterEvaporationManager.waterBlockID < 0)
					{
						WaterEvaporationManager.waterBlockID = ItemClass.GetItem("Water", false).ToBlockValue().type;
					}
					GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, WaterEvaporationManager.restWalkList.list[num2].pos, WaterEvaporationManager.waterBlockID, 1UL);
				}
				num2++;
			}
			num2 = 0;
			for (int j = 0; j < WaterEvaporationManager.vRemovalList.Count; j++)
			{
				Vector3i blockPos2 = WaterEvaporationManager.vRemovalList[j];
				WaterEvaporationManager.restWalkList.Remove(WaterEvaporationManager.uRemovalList[num2++]);
				WaterEvaporationManager.RemoveFromRestList(blockPos2);
			}
			WaterEvaporationManager.restWalkIndex += 15;
			if (WaterEvaporationManager.restWalkIndex >= WaterEvaporationManager.restWalkList.Count)
			{
				WaterEvaporationManager.restWalkIndex = 0;
			}
		}
	}

	public static void ClearAll()
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.restingList;
		lock (obj)
		{
			WaterEvaporationManager.restingList.Clear();
		}
		obj = WaterEvaporationManager.evaporationList;
		lock (obj)
		{
			WaterEvaporationManager.evaporationList.Clear();
		}
		DictionaryList<ulong, WaterEvaporationManager.BlockData> obj2 = WaterEvaporationManager.evapWalkList;
		lock (obj2)
		{
			WaterEvaporationManager.evapWalkList.Clear();
		}
		obj2 = WaterEvaporationManager.restWalkList;
		lock (obj2)
		{
			WaterEvaporationManager.restWalkList.Clear();
		}
	}

	public static void AddToRestList(Vector3i _blockPos)
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.restingList;
		lock (obj)
		{
			DictionaryList<ulong, WaterEvaporationManager.BlockData> obj2 = WaterEvaporationManager.restWalkList;
			lock (obj2)
			{
				if (WaterEvaporationManager.restingList.dict.ContainsKey(_blockPos.x))
				{
					if (WaterEvaporationManager.restingList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y))
					{
						if (WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
						{
							WaterEvaporationManager.BlockData blockData = new WaterEvaporationManager.BlockData(_blockPos);
							WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z] = blockData;
							WaterEvaporationManager.restWalkList.Add(blockData.ID, blockData);
						}
						else
						{
							WaterEvaporationManager.BlockData blockData2 = new WaterEvaporationManager.BlockData(_blockPos);
							WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData2);
							WaterEvaporationManager.restWalkList.Add(blockData2.ID, blockData2);
						}
					}
					else
					{
						WaterEvaporationManager.BlockData blockData3 = new WaterEvaporationManager.BlockData(_blockPos);
						WaterEvaporationManager.restingList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, WaterEvaporationManager.BlockData>());
						WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData3);
						WaterEvaporationManager.restWalkList.Add(blockData3.ID, blockData3);
					}
				}
				else
				{
					WaterEvaporationManager.BlockData blockData4 = new WaterEvaporationManager.BlockData(_blockPos);
					WaterEvaporationManager.restingList.Add(_blockPos.x, new DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>());
					WaterEvaporationManager.restingList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, WaterEvaporationManager.BlockData>());
					WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData4);
					WaterEvaporationManager.restWalkList.Add(blockData4.ID, blockData4);
				}
			}
		}
	}

	public static void RemoveFromRestList(Vector3i _blockPos)
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.restingList;
		lock (obj)
		{
			if (WaterEvaporationManager.restingList.dict.ContainsKey(_blockPos.x) && WaterEvaporationManager.restingList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				DictionaryList<ulong, WaterEvaporationManager.BlockData> obj2 = WaterEvaporationManager.restWalkList;
				lock (obj2)
				{
					WaterEvaporationManager.restWalkList.Remove(WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].ID);
				}
				WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].Remove(_blockPos.z);
			}
		}
	}

	public static ulong GetRestTime(Vector3i _blockPos)
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.restingList;
		lock (obj)
		{
			if (WaterEvaporationManager.restingList.dict.ContainsKey(_blockPos.x) && WaterEvaporationManager.restingList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				return WaterEvaporationManager.restingList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].time;
			}
		}
		return 0UL;
	}

	public static void AddToEvapList(Vector3i _blockPos)
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.evaporationList;
		lock (obj)
		{
			DictionaryList<ulong, WaterEvaporationManager.BlockData> obj2 = WaterEvaporationManager.evapWalkList;
			lock (obj2)
			{
				if (WaterEvaporationManager.evaporationList.dict.ContainsKey(_blockPos.x))
				{
					if (WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y))
					{
						if (WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
						{
							WaterEvaporationManager.BlockData blockData = new WaterEvaporationManager.BlockData(_blockPos);
							WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z] = blockData;
							WaterEvaporationManager.evapWalkList.Add(blockData.ID, blockData);
						}
						else
						{
							WaterEvaporationManager.BlockData blockData2 = new WaterEvaporationManager.BlockData(_blockPos);
							WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData2);
							WaterEvaporationManager.evapWalkList.Add(blockData2.ID, blockData2);
						}
					}
					else
					{
						WaterEvaporationManager.BlockData blockData3 = new WaterEvaporationManager.BlockData(_blockPos);
						WaterEvaporationManager.evaporationList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, WaterEvaporationManager.BlockData>());
						WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData3);
						WaterEvaporationManager.evapWalkList.Add(blockData3.ID, blockData3);
					}
				}
				else
				{
					WaterEvaporationManager.BlockData blockData4 = new WaterEvaporationManager.BlockData(_blockPos);
					WaterEvaporationManager.evaporationList.Add(_blockPos.x, new DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>());
					WaterEvaporationManager.evaporationList.dict[_blockPos.x].Add(_blockPos.y, new DictionaryList<int, WaterEvaporationManager.BlockData>());
					WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].Add(_blockPos.z, blockData4);
					WaterEvaporationManager.evapWalkList.Add(blockData4.ID, blockData4);
				}
			}
		}
	}

	public static void RemoveFromEvapList(Vector3i _blockPos)
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.evaporationList;
		lock (obj)
		{
			if (WaterEvaporationManager.evaporationList.dict.ContainsKey(_blockPos.x) && WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				DictionaryList<ulong, WaterEvaporationManager.BlockData> obj2 = WaterEvaporationManager.evapWalkList;
				lock (obj2)
				{
					WaterEvaporationManager.evapWalkList.Remove(WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].ID);
				}
				WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].Remove(_blockPos.z);
			}
		}
	}

	public static ulong GetEvapTime(Vector3i _blockPos)
	{
		DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> obj = WaterEvaporationManager.evaporationList;
		lock (obj)
		{
			if (WaterEvaporationManager.evaporationList.dict.ContainsKey(_blockPos.x) && WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict.ContainsKey(_blockPos.y) && WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict.ContainsKey(_blockPos.z))
			{
				return WaterEvaporationManager.evaporationList.dict[_blockPos.x].dict[_blockPos.y].dict[_blockPos.z].time;
			}
		}
		return 0UL;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int evapWalkIndex = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static ulong uniqueIndex = 0UL;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int restWalkIndex = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static DictionaryList<ulong, WaterEvaporationManager.BlockData> evapWalkList = new DictionaryList<ulong, WaterEvaporationManager.BlockData>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> evaporationList = new DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static DictionaryList<ulong, WaterEvaporationManager.BlockData> restWalkList = new DictionaryList<ulong, WaterEvaporationManager.BlockData>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>> restingList = new DictionaryList<int, DictionaryList<int, DictionaryList<int, WaterEvaporationManager.BlockData>>>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int waterBlockID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Vector3i> vRemovalList = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<ulong> uRemovalList = new List<ulong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class BlockData
	{
		public BlockData(Vector3i _pos)
		{
			this.time = GameManager.Instance.World.GetWorldTime();
			ulong uniqueIndex = WaterEvaporationManager.uniqueIndex;
			WaterEvaporationManager.uniqueIndex = uniqueIndex + 1UL;
			this.ID = uniqueIndex;
			this.pos = _pos;
		}

		public ulong time;

		public ulong ID;

		public Vector3i pos;
	}
}
