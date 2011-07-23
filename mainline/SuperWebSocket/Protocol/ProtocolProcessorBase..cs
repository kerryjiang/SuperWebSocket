using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    abstract class ProtocolProcessorBase : IProtocolProcessor
    {
        public abstract bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<WebSocketCommandInfo> dataFrameReader);

        public IProtocolProcessor NextProcessor { get; set; }
    }
}
