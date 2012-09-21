using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.SubProtocol
{
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            session.Send(requestInfo.Body);
        }
    }
}
