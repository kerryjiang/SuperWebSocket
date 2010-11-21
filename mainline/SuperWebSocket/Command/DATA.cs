using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Command
{
    public class DATA : StringCommandBase<WebSocketSession>
    {
        public override void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo)
        {
            throw new NotImplementedException();
        }
    }
}
