using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Command
{
    public class Close : CommandBase<WebSocketSession, WebSocketCommandInfo>
    {
        public override string Name
        {
            get
            {
                return "8";
            }
        }

        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            session.Close(CloseReason.ClientClosing);
        }
    }
}
