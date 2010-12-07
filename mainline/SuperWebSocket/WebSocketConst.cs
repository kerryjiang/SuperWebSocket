using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket
{
    public class WebSocketConstant
    {
        public const string Host = "Host";
        public const string Connection = "Connection";
        public const string SecWebSocketKey1 = "Sec-WebSocket-Key1";
        public const string SecWebSocketKey2 = "Sec-WebSocket-Key2";
        public const string SecWebSocketKey3 = "Sec-WebSocket-Key3";
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
        public const string Cookie = "Cookie";
        public const string Upgrade = "Upgrade";
        public const string Origin = "Origin";
        public const byte StartByte = 0x00;
        public const byte EndByte = 0xFF;
    }
}
