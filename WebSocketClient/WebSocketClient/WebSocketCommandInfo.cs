using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.WebSocketClient
{
    public class WebSocketCommandInfo : ICommandInfo
    {
        public string Key { get; set; }

        public byte[] Data { get; set; }

        public string Text { get; set; }
    }
}
