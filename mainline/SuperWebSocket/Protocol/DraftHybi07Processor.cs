using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    class DraftHybi07Processor<TWebSocketSession> : HandshakeProcessorBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override bool Handshake(TWebSocketSession session)
        {
            if (!"7".Equals(session.SecWebSocketVersion) && NextProcessor != null)
            {
                return NextProcessor.Handshake(session);
            }

            return false;
        }
    }
}
