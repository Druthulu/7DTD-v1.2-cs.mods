using System;
using System.Collections.Generic;
using GameSparks.Api;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UnityEngine;

public class GameSparksTestUI : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Application.logMessageReceivedThreaded += this.HandleLog;
		Screen.orientation = ScreenOrientation.AutoRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		GSMessageHandler._AllMessages = new Action<GSMessage>(this.HandleGameSparksMessageReceived);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleGameSparksMessageReceived(GSMessage message)
	{
		this.HandleLog("MSG:" + message.JSONString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLog(string logString)
	{
		GS.GSPlatform.ExecuteOnMainThread(delegate
		{
			this.HandleLog(logString, null, LogType.Log);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLog(string logString, string stackTrace, LogType logType)
	{
		if (this.myLogQueue.Count > 30)
		{
			this.myLogQueue.Dequeue();
		}
		this.myLogQueue.Enqueue(logString);
		this.myLog = "";
		foreach (string str in this.myLogQueue.ToArray())
		{
			this.myLog = this.myLog + "\n\n" + str;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
		GUI.skin.textArea.alignment = TextAnchor.LowerLeft;
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(GS.Available ? "AVAILABLE" : "NOT AVAILABLE", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		});
		GUILayout.Label("SDK Version: " + GS.Version.ToString(), new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		});
		GUILayout.EndHorizontal();
		GUILayout.Label(GS.Authenticated ? "AUTHENTICATED" : "NOT AUTHENTICATED", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		});
		if (GUILayout.Button("Clear Log", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			this.myLog = "";
			this.myLogQueue.Clear();
		}
		if (GUILayout.Button("Logout", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			GS.Reset();
		}
		if (GUILayout.Button("Disconnect", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			GS.Disconnect();
		}
		if (!GS.Available && GUILayout.Button("Reconnect", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			GS.Reconnect();
		}
		if (GUILayout.Button("DeviceAuthenticationRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new DeviceAuthenticationRequest().Send(delegate(AuthenticationResponse response)
			{
				this.HandleLog("DeviceAuthenticationRequest.JSON:" + response.JSONString);
				this.HandleLog("DeviceAuthenticationRequest.HasErrors:" + response.HasErrors.ToString());
				this.HandleLog("DeviceAuthenticationRequest.UserId:" + response.UserId);
			});
		}
		if (GUILayout.Button("durableAccountDetailsRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new AccountDetailsRequest().SetDurable(true).Send(null);
		}
		if (GUILayout.Button("accountDetailsRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new AccountDetailsRequest().Send(delegate(AccountDetailsResponse response)
			{
				this.HandleLog("AccountDetailsRequest.UserId:" + response.UserId);
			});
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("facebookConnectRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new FacebookConnectRequest().SetAccessToken(this.fbToken).Send(delegate(AuthenticationResponse response)
			{
				this.HandleLog("FacebookConnectRequest.HasErrors:" + response.HasErrors.ToString());
				this.HandleLog("FacebookConnectRequest.UserId:" + response.UserId);
			});
		}
		this.fbToken = GUILayout.TextField(this.fbToken, new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		});
		GUILayout.EndHorizontal();
		if (GUILayout.Button("listAchievementsRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new ListAchievementsRequest().Send(delegate(ListAchievementsResponse response)
			{
				foreach (ListAchievementsResponse._Achievement achievement in response.Achievements)
				{
					this.HandleLog("ListAchievementsRequest:shortCode:" + achievement.ShortCode);
				}
			});
		}
		if (GUILayout.Button("listGameFriendsRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new ListGameFriendsRequest().Send(delegate(ListGameFriendsResponse response)
			{
				foreach (ListGameFriendsResponse._Player player in response.Friends)
				{
					this.HandleLog("ListGameFriendsRequest.DisplayName:" + player.DisplayName);
				}
			});
		}
		if (GUILayout.Button("listVirtualGoodsRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new ListVirtualGoodsRequest().Send(delegate(ListVirtualGoodsResponse response)
			{
				foreach (ListVirtualGoodsResponse._VirtualGood virtualGood in response.VirtualGoods)
				{
					this.HandleLog("ListVirtualGoodsRequest.Description:" + virtualGood.Description);
				}
			});
		}
		if (GUILayout.Button("listChallengeTypeRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new ListChallengeTypeRequest().Send(delegate(ListChallengeTypeResponse response)
			{
				foreach (ListChallengeTypeResponse._ChallengeType challengeType in response.ChallengeTemplates)
				{
					this.HandleLog("ListAchievementsRequest.Challenge:" + challengeType.ChallengeShortCode);
				}
			});
		}
		if (GUILayout.Button("authenticationRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new AuthenticationRequest().SetUserName("gabs").SetPassword("gabs").Send(delegate(AuthenticationResponse AR)
			{
				if (AR.HasErrors)
				{
					Debug.Log("Didnt Work");
					return;
				}
				Debug.Log("Worked");
			});
		}
		if (GUILayout.Button("leaderboardData", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new LeaderboardDataRequest().SetLeaderboardShortCode("HSCORE").SetEntryCount(10L).Send(delegate(LeaderboardDataResponse leadResponse)
			{
				if (leadResponse.HasErrors)
				{
					Debug.Log("Leaderboard data retrieval failed ...");
					return;
				}
				Debug.Log("Leaderboard data retrieval succeeded ..." + ((leadResponse != null) ? leadResponse.ToString() : null));
				foreach (LeaderboardDataResponse._LeaderboardData leaderboardData in leadResponse.Data)
				{
					Debug.Log(string.Concat(new string[]
					{
						"Rank: ",
						leaderboardData.Rank.ToString(),
						"    UserName: ",
						leaderboardData.UserName,
						"    Score: ",
						leaderboardData.GetNumberValue("SCORE").ToString()
					}));
				}
			});
		}
		if (GUILayout.Button("listMessageRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new ListMessageRequest().Send(delegate(ListMessageResponse response)
			{
				foreach (GSData gsdata in response.MessageList)
				{
					this.HandleLog("ListMessageRequest.MessageList:" + gsdata.GetString("messageId"));
				}
			});
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("dismissMessageRequest", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			new DismissMessageRequest().SetMessageId(this.dismissMessageId).Send(delegate(DismissMessageResponse response)
			{
				this.HandleLog("DismissMessageRequest.HasErrors:" + response.HasErrors.ToString());
			});
		}
		this.dismissMessageId = GUILayout.TextField(this.dismissMessageId, new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		});
		GUILayout.EndHorizontal();
		if (GUILayout.Button("TRACE " + (GS.TraceMessages ? "ON" : "OFF"), new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(30f)
		}))
		{
			GS.TraceMessages = !GS.TraceMessages;
		}
		GUI.TextArea(new Rect(420f, 5f, (float)(Screen.width - 425), (float)(Screen.height - 10)), this.myLog);
	}

	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Queue<string> myLogQueue = new Queue<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string myLog = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string fbToken = "accessToken";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string dismissMessageId = "messageId";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int itemHeight = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int itemWidth = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool testing;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool working;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool result;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int counter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numTest;

	public Texture cursor;
}
