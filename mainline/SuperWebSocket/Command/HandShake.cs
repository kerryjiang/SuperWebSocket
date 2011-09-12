using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Command
{
    public class HandShake : CommandBase<WebSocketSession, WebSocketCommandInfo>
    {
        public override string Name
        {
            get
            {
                return "0";
            }
        }

        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            session.AppServer.OnNewSessionConnected(session);
        }
    }
}
