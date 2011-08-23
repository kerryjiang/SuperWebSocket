using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    public interface IProtocolProcessor
    {
        IProtocolProcessor NextProcessor { get; set; }

        bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<WebSocketCommandInfo> dataFrameReader);
    }
}
