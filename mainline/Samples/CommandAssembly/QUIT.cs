using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.CommandAssembly
{
    /// <summary>
    /// If the client send "QUIT" to server, the websocket connection will be closed
    /// </summary>
    public class QUIT : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            session.Close();
        }
    }
}
