using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    public interface IProtocolProcessor
    {
        void SendHandshake(WebSocket websocket);

        ReaderBase CreateHandshakeReader();

        void SendMessage(WebSocket websocket, string message);

        void SendCloseHandshake(WebSocket websocket, string closeReason);

        void SendPing(WebSocket websocket, string ping);
    }
}
