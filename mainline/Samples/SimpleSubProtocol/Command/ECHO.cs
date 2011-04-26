using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Samples.SimpleSubProtocol.Command
{
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo)
        {
            foreach (var p in commandInfo.Parameters)
            {
                session.SendResponse(p);
            }
        }
    }
}
