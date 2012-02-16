using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocketTest.Command
{
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, StringRequestInfo commandInfo)
        {
            foreach(var p in commandInfo.Parameters)
            {
                session.SendResponse(p);
            }
        }
    }
}
