using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.SimpleSubProtocol.Command
{
    public class QUIT : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo)
        {
            session.Close();
        }
    }
}
