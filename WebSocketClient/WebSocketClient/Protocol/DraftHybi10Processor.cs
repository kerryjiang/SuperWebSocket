using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi10Processor : IProtocolProcessor
    {
        public void SendHandshake(WebSocket websocket)
        {
            throw new NotImplementedException();
        }

        public ReaderBase CreateHandshakeReader()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(WebSocket websocket, string message)
        {
            throw new NotImplementedException();
        }

        public void SendCloseHandshake(WebSocket websocket, string closeReason)
        {
            throw new NotImplementedException();
        }

        public void SendPing(WebSocket websocket, string ping)
        {
            throw new NotImplementedException();
        }
    }
}
