using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.SubProtocol
{
    public class SUB : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            var paramArray = requestInfo.Body.Split(' ');

            session.Send((int.Parse(paramArray[0]) - int.Parse(paramArray[1])).ToString());
        }
    }
}
