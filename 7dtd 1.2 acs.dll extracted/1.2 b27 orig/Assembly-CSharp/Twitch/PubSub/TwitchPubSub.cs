using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Twitch.PubSub
{
	public class TwitchPubSub
	{
		public void Connect(string userID)
		{
			if (this.cts != null)
			{
				this.cts.Cancel();
			}
			this.cts = new CancellationTokenSource();
			Task.Run(() => this.StartAsync(new TwitchTopic[]
			{
				TwitchTopic.ChannelPoints(userID),
				TwitchTopic.Bits(userID),
				TwitchTopic.Subscription(userID),
				TwitchTopic.HypeTrain(userID),
				TwitchTopic.CreatorGoal(userID)
			}, this.cts.Token));
		}

		public void Disconnect()
		{
			this.cts.Cancel();
			TwitchPubSub.reconnect = false;
		}

		public Task StartAsync(TwitchTopic[] newTopics, CancellationToken token)
		{
			TwitchPubSub.<StartAsync>d__12 <StartAsync>d__;
			<StartAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<StartAsync>d__.<>4__this = this;
			<StartAsync>d__.newTopics = newTopics;
			<StartAsync>d__.token = token;
			<StartAsync>d__.<>1__state = -1;
			<StartAsync>d__.<>t__builder.Start<TwitchPubSub.<StartAsync>d__12>(ref <StartAsync>d__);
			return <StartAsync>d__.<>t__builder.Task;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HandleMessage(string receivedMessage, TwitchPubSub.MessageTypes msgType)
		{
			if (msgType != TwitchPubSub.MessageTypes.Standard)
			{
				if (msgType == TwitchPubSub.MessageTypes.HypeStart)
				{
					TwitchManager.Current.StartHypeTrain();
				}
				return;
			}
			JObject jobject = JObject.Parse(receivedMessage);
			string a = jobject["type"].Value<string>();
			if (a == "RESPONSE" && jobject["error"].Value<string>() != "")
			{
				return;
			}
			if (a == "RESPONSE")
			{
				return;
			}
			if (this.HandlePongMessage(receivedMessage))
			{
				return;
			}
			if (this.HandleReconnectMessage(receivedMessage))
			{
				return;
			}
			this.HandleRedemptionsMessages(receivedMessage);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Task StartListening(IEnumerable<TwitchTopic> topics)
		{
			TwitchPubSub.<StartListening>d__14 <StartListening>d__;
			<StartListening>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<StartListening>d__.<>4__this = this;
			<StartListening>d__.topics = topics;
			<StartListening>d__.<>1__state = -1;
			<StartListening>d__.<>t__builder.Start<TwitchPubSub.<StartListening>d__14>(ref <StartListening>d__);
			return <StartListening>d__.<>t__builder.Task;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			string message = "{ \"type\": \"PING\" }";
			this.SendMessageOnSocket(message).GetAwaiter().GetResult();
			this.pongTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10.0).TotalMilliseconds);
			this.pongTimer.Elapsed += this.PongTimer_Elapsed;
			this.pongTimer.Start();
			this.pingAcknowledged = false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PongTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!this.pingAcknowledged)
			{
				TwitchPubSub.reconnect = true;
				this.pongTimer.Dispose();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Task SendMessageOnSocket(string message)
		{
			if (this.socket.State != WebSocketState.Open)
			{
				return Task.CompletedTask;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(message);
			return this.socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public event EventHandler<PubSubBitRedemptionMessage.BitRedemptionData> OnBitsRedeemed;

		public event EventHandler<PubSubSubscriptionRedemptionMessage> OnSubscriptionRedeemed;

		public event EventHandler<PubSubChannelPointMessage.ChannelRedemptionData> OnChannelPointsRedeemed;

		public event EventHandler<PubSubGoalMessage.Goal> OnGoalAchieved;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HandlePongMessage(string message)
		{
			if (message.Contains("\"PONG\""))
			{
				this.pingAcknowledged = true;
				this.pongTimer.Stop();
				this.pongTimer.Dispose();
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HandleReconnectMessage(string message)
		{
			if (message.Contains("\"RECONNECT\""))
			{
				TwitchPubSub.reconnect = true;
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HandleRedemptionsMessages(string message)
		{
			JObject jobject = JObject.Parse(message);
			if (jobject["type"].Value<string>() == "MESSAGE")
			{
				string text = jobject["data"]["topic"].Value<string>();
				if (text.StartsWith("channel-points-channel-v1"))
				{
					string message2 = jobject["data"]["message"].Value<string>();
					PubSubChannelPointMessage pubSubChannelPointMessage = null;
					try
					{
						pubSubChannelPointMessage = PubSubChannelPointMessage.Deserialize(message2);
					}
					catch (Exception ex)
					{
						Debug.LogError(ex.ToString());
						Debug.LogError(message2);
					}
					if (this.OnChannelPointsRedeemed != null && pubSubChannelPointMessage != null)
					{
						this.OnChannelPointsRedeemed(null, pubSubChannelPointMessage.data);
					}
					return true;
				}
				if (text.StartsWith("channel-bits-events"))
				{
					string message3 = jobject["data"]["message"].Value<string>();
					PubSubBitRedemptionMessage pubSubBitRedemptionMessage = null;
					try
					{
						pubSubBitRedemptionMessage = PubSubBitRedemptionMessage.Deserialize(message3);
					}
					catch (Exception ex2)
					{
						Debug.LogError(ex2.ToString());
						Debug.LogError(message3);
					}
					if (this.OnBitsRedeemed != null && pubSubBitRedemptionMessage != null)
					{
						this.OnBitsRedeemed(null, pubSubBitRedemptionMessage.data);
					}
					return true;
				}
				if (text.StartsWith("channel-subscribe-events"))
				{
					string message4 = jobject["data"]["message"].Value<string>();
					PubSubSubscriptionRedemptionMessage pubSubSubscriptionRedemptionMessage = null;
					try
					{
						pubSubSubscriptionRedemptionMessage = PubSubSubscriptionRedemptionMessage.Deserialize(message4);
					}
					catch (Exception ex3)
					{
						Debug.LogError(ex3.ToString());
						Debug.LogError(message4);
					}
					if (this.OnSubscriptionRedeemed != null && pubSubSubscriptionRedemptionMessage != null)
					{
						this.OnSubscriptionRedeemed(null, pubSubSubscriptionRedemptionMessage);
					}
					return true;
				}
				if (text.StartsWith("creator-goals-events"))
				{
					string message5 = jobject["data"]["message"].Value<string>();
					PubSubGoalMessage pubSubGoalMessage = null;
					try
					{
						pubSubGoalMessage = PubSubGoalMessage.Deserialize(message5);
					}
					catch (Exception ex4)
					{
						Debug.LogError(ex4.ToString());
						Debug.LogError(message5);
					}
					if (pubSubGoalMessage.type == "goal_achieved" && this.OnGoalAchieved != null && pubSubGoalMessage != null)
					{
						this.OnGoalAchieved(null, pubSubGoalMessage.data.goal);
					}
					return true;
				}
				if (text.StartsWith("hype-train-events-v1"))
				{
					try
					{
						string text2 = jobject["data"]["message"].ToString();
						Debug.LogWarning(text2);
						if (text2.Contains("hype-train-start"))
						{
							TwitchManager.Current.StartHypeTrain();
						}
						else if (text2.Contains("hype-train-level-up"))
						{
							TwitchManager.Current.IncrementHypeTrainLevel();
						}
						else if (text2.Contains("hype-train-end"))
						{
							TwitchManager.Current.EndHypeTrain();
						}
					}
					catch (Exception ex5)
					{
						Debug.LogWarning("Hype Train Error: " + message);
						Debug.LogWarning("Hype Train Exception: " + ex5.ToString());
					}
					return true;
				}
			}
			return false;
		}

		public void Cleanup()
		{
			if (this.pingTimer != null)
			{
				this.pingTimer.Dispose();
			}
			if (this.pongTimer != null)
			{
				this.pongTimer.Dispose();
			}
			if (this.socket != null)
			{
				this.socket.Dispose();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ClientWebSocket socket;

		[PublicizedFrom(EAccessModifier.Private)]
		public System.Timers.Timer pingTimer;

		[PublicizedFrom(EAccessModifier.Private)]
		public System.Timers.Timer pongTimer;

		[PublicizedFrom(EAccessModifier.Private)]
		public System.Timers.Timer reconnectTimer = new System.Timers.Timer();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool pingAcknowledged;

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool reconnect = false;

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchTopic[] topics;

		[PublicizedFrom(EAccessModifier.Private)]
		public CancellationTokenSource cts;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan[] _ReconnectTimeouts = new TimeSpan[]
		{
			TimeSpan.FromSeconds(1.0),
			TimeSpan.FromSeconds(5.0),
			TimeSpan.FromSeconds(10.0),
			TimeSpan.FromSeconds(30.0),
			TimeSpan.FromMinutes(1.0),
			TimeSpan.FromMinutes(5.0)
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public enum MessageTypes
		{
			Standard,
			HypeStart
		}
	}
}
