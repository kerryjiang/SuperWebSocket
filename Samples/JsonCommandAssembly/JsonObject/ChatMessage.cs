using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Samples.JsonCommandAssembly.JsonObject
{
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
    }
}
