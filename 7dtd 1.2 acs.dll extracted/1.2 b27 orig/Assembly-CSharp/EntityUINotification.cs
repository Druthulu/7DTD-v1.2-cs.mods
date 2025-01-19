using System;
using UnityEngine;

public interface EntityUINotification
{
	void Tick(float dt);

	void SetBuff(BuffValue buff);

	void SetStats(EntityStats stats);

	void NotifyBuffRemoved();

	Color GetColor();

	float MinValue { get; }

	float MaxValue { get; }

	float MinWarningLevel { get; }

	float MaxWarningLevel { get; }

	float CurrentValue { get; }

	string Units { get; }

	string Icon { get; }

	string Description { get; }

	float FadeOutTime { get; }

	bool Expired { get; }

	bool Visible { get; }

	BuffValue Buff { get; }

	EnumEntityUINotificationDisplayMode DisplayMode { get; }

	EnumEntityUINotificationSubject Subject { get; }
}
