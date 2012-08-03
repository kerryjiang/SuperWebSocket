using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocketTest.Command
{
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            var paramsArray = requestInfo.Data.Split(' ');
            for (var i = 0; i < paramsArray.Length; i++)
                session.SendResponse(paramsArray[i]);
        }
    }
}
