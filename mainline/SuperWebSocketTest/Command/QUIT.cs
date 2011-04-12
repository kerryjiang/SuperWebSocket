using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperWebSocket;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocketTest.Command
{
    public class QUIT : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo)
        {
            session.Close();
        }
    }
}
