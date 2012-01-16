using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class HandShake<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Handshake.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            session.Handshaked = true;
            session.AppServer.OnNewSessionConnected(session);
        }
    }
}
