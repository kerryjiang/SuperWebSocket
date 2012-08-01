using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperWebSocket.Samples.JsonCommandAssembly.JsonObject;

namespace SuperWebSocket.Samples.JsonCommandAssembly.Command
{
    public class ADD : JsonSubCommand<AddParameter>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, AddParameter commandInfo)
        {
            var result = new AddResult { Result = commandInfo.A + commandInfo.B };
            //Send calculating result to client
            SendJsonMessage(session, result);
        }
    }
}
