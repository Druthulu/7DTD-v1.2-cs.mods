using System;

namespace GameSparks.Platforms
{
	public interface IControlledWebSocket : IGameSparksWebSocket
	{
		void TriggerOnClose();

		void TriggerOnOpen();

		void TriggerOnError(string message);

		void TriggerOnMessage(string message);

		bool Update();

		int SocketId { get; }
	}
}
