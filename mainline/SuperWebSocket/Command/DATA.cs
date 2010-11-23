using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Command
{
    public class DATA : CommandBase<WebSocketSession, WebSocketCommandInfo>
    {
        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            throw new NotImplementedException();
        }
    }
}
