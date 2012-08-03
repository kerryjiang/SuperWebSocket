using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.Samples.JsonCommandAssembly.JsonObject;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.JsonCommandAssembly.Command
{
    /// <summary>
    /// When the client send "CHAT {'Sender':'kerry', 'Receiver': 'Linda', 'Content':'Where are you now?'}",
    /// the method of this class will be executed
    /// </summary>
    public class CHAT : JsonSubCommand<ChatMessage>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, ChatMessage commandInfo)
        {
            //Save to database or despacth to other session?
        }
    }
}
