using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.SubProtocol
{
    public class SubCommandInfo : WebSocketCommandInfo
    {
        public SubCommandInfo(string key, string data)
            : base(key, data)
        {

        }

        public string[] Parameters { get; set; }
    }
}
