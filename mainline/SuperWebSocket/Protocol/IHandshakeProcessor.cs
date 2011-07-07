using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    interface IHandshakeProcessor<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        IHandshakeProcessor<TWebSocketSession> NextProcessor { get; set; }

        bool Handshake(TWebSocketSession session);
    }
}
