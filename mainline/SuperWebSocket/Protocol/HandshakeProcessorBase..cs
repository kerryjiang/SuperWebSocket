using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    abstract class HandshakeProcessorBase<TWebSocketSession> : IHandshakeProcessor<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public abstract bool Handshake(TWebSocketSession session);

        public IHandshakeProcessor<TWebSocketSession> NextProcessor { get; set; }
    }
}
