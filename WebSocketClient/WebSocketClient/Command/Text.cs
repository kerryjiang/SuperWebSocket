using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Command
{
    public class Text : WebSocketCommandBase
    {
        public override void ExecuteCommand(IClientSession<WebSocketCommandInfo, WebSocketContext> session, WebSocketCommandInfo commandInfo)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return OpCode.Text.ToString(); }
        }
    }
}
