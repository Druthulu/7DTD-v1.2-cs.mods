using System;
using GameSparks.Core;
using UnityEngine;

[Serializable]
public class PlayerDetails
{
	public PlayerDetails(string _displayName, string _userId, GSData _responseData)
	{
		this.displayName = _displayName;
		this.userId = _userId;
	}

	public string displayName;

	[HideInInspector]
	public string userEmail;

	public string userId;
}
