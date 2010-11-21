using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperWebSocket
{
    public class WebSocketContext : SocketContext
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string HttpVersion { get; set; }
        public string Host { get; set; }
        public string Origin { get; set; }
        public string Upgrade { get; set; }
        public string Connection { get; set; }
    }
}
