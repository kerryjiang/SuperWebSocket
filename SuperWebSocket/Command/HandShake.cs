using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class HandShake<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.HandshakeTag;
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment commandInfo)
        {
            session.Handshaked = true;
            session.AppServer.OnNewSessionConnected(session);
        }
    }
}
