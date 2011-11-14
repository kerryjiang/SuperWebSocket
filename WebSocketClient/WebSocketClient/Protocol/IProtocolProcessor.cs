using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    public interface IProtocolProcessor
    {
        void Initialize(WebSocket websocket);

        void SendHandshake();

        ReaderBase CreateHandshakeReader();

        void SendMessage(string message);

        void SendCloseHandshake(string closeReason);

        void SendPing(string ping);
    }
}
