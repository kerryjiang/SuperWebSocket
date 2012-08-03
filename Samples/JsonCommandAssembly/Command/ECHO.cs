using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperWebSocket.Samples.JsonCommandAssembly.JsonObject;

namespace SuperWebSocket.Samples.JsonCommandAssembly.Command
{
    public class ECHO : JsonSubCommand<ChatMessage>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, ChatMessage commandInfo)
        {
            //Send the received message back to client
            SendJsonMessage(session, commandInfo);
        }
    }
}
