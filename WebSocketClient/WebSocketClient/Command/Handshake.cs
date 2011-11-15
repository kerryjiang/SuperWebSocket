using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient.Command
{
    public class Handshake : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            if (!session.ProtocolProcessor.VerifyHandshake(commandInfo))
            {
                session.Close();
            }
        }

        public override string Name
        {
            get { return OpCode.Handshake.ToString(); }
        }
    }
}
