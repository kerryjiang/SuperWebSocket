using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi10HandshakeReader : HandshakeReader
    {
        public DraftHybi10HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }
    }
}
