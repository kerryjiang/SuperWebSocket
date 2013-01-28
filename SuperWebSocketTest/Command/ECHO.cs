using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using System.Threading;

namespace SuperWebSocketTest.Command
{
    [CountSubCommandFilter]
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            var paramsArray = requestInfo.Body.Split(' ');
            for (var i = 0; i < paramsArray.Length; i++)
            {
                session.Send(paramsArray[i]);
            }
        }
    }
}
