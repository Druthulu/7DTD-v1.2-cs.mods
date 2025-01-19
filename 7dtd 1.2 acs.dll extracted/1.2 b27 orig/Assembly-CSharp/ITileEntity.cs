using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITileEntity
{
	event XUiEvent_TileEntityDestroyed Destroyed;

	List<ITileEntityChangedListener> listeners { get; }

	void SetUserAccessing(bool _bUserAccessing);

	bool IsUserAccessing();

	void SetModified();

	Chunk GetChunk();

	Vector3i ToWorldPos();

	Vector3 ToWorldCenterPos();

	BlockValue blockValue { get; }

	int GetClrIdx();

	int EntityId { get; }
}
