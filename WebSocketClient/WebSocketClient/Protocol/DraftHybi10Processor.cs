using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.WebSocketClient.Reader;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi10Processor : IProtocolProcessor
    {
        public HandshakeReader CreateHandshakeReader()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IClientSession session, string message)
        {
            throw new NotImplementedException();
        }

        public void SendCloseHandshake(IClientSession session, string closeReason)
        {
            throw new NotImplementedException();
        }

        public void SendPing(IClientSession session, string ping)
        {
            throw new NotImplementedException();
        }
    }
}
