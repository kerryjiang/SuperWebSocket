using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    public abstract class WebSocketReaderBase : CommandReaderBase<WebSocketCommandInfo>
    {
        static WebSocketReaderBase()
        {
            HandshakeCommandInfo = new WebSocketCommandInfo("HANDSHAKE", string.Empty);
        }

        public WebSocketReaderBase(IAppServer appServer)
            : base(appServer)
        {

        }

        public WebSocketReaderBase(WebSocketReaderBase previousCommandReader)
            : base(previousCommandReader)
        {

        }

        protected void Handshake(IProtocolProcessor protocolProcessor, IWebSocketSession session)
        {
            ICommandReader<WebSocketCommandInfo> dataFrameReader;

            if (!protocolProcessor.Handshake(session, this, out dataFrameReader))
            {
                session.Close(CloseReason.ServerClosing);
                return;
            }

            NextCommandReader = dataFrameReader;
            return;
        }

        protected static WebSocketCommandInfo HandshakeCommandInfo { get; private set; }
    }
}
