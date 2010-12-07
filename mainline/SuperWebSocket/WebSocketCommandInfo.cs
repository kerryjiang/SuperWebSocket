using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket
{
    public class WebSocketCommandInfo : CommandInfo<string>
    {
        public WebSocketCommandInfo(string key, string data)
            : base(key, data)
        {

        }

        public WebSocketCommandInfo(string data)
            : base(string.Empty, data)
        {

        }
    }
}
