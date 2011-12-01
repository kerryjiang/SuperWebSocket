using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Command
{
    public class Close : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.Close();
        }

        public override string Name
        {
            get { return OpCode.Close.ToString(); }
        }
    }
}
