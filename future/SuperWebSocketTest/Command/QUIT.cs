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
    public class QUIT : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, StringRequestInfo commandInfo)
        {
            session.Close();
        }
    }
}
