using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Samples.CommandAssembly
{
    /// <summary>
    /// If client send "ECHO A B C" to server, the A, B and C will be sent back
    /// A, B and C is processed as parameters list in commandInfo
    /// </summary>
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            session.SendResponse(requestInfo.Data);
        }
    }
}
