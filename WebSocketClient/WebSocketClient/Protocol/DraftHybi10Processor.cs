using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi10Processor : ProtocolProcessorBase
    {
        public override void SendHandshake()
        {
            throw new NotImplementedException();
        }

        public override ReaderBase CreateHandshakeReader()
        {
            return new DraftHybi10HandshakeReader(WebSocket);
        }

        public override void SendMessage(string message)
        {
            throw new NotImplementedException();
        }

        public override void SendCloseHandshake(string closeReason)
        {
            throw new NotImplementedException();
        }

        public override void SendPing(string ping)
        {
            throw new NotImplementedException();
        }
    }
}
