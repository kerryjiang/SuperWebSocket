using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.WebSocketClient.Reader;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    interface IProtocolProcessor
    {
        HandshakeReader CreateHandshakeReader();

        void SendMessage(IClientSession session, string message);

        void SendCloseHandshake(IClientSession session, string closeReason);

        void SendPing(IClientSession session, string ping);
    }
}
