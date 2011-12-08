using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Command
{
    public class Text : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.FireMessageReceived(commandInfo.Text);
        }

        public override string Name
        {
            get { return OpCode.Text.ToString(); }
        }
    }
}
