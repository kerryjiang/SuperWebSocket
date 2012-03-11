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
    public abstract class WebSocketReaderBase : CommandReaderBase<IWebSocketFragment>
    {
        protected const int SecKey3Len = 8;

        static WebSocketReaderBase()
        {
            HandshakeCommandInfo = new HandshakeRequest();
        }

        public WebSocketReaderBase(IAppServer appServer)
            : base(appServer)
        {

        }

        public WebSocketReaderBase(WebSocketReaderBase previousCommandReader)
            : base(previousCommandReader)
        {

        }

        protected bool Handshake(IProtocolProcessor protocolProcessor, IWebSocketSession session)
        {
            ICommandReader<IWebSocketFragment> dataFrameReader;

            if (!protocolProcessor.Handshake(session, this, out dataFrameReader))
            {
                session.Close(CloseReason.ServerClosing);
                return false;
            }

            //Processor handshake sucessfully, but output datareader is null, so the multiple protocol switch handled the handshake
            //In this case, the handshake is not completed
            if (dataFrameReader == null)
            {
                NextCommandReader = this;
                return false;
            }

            NextCommandReader = dataFrameReader;
            return true;
        }

        protected static HandshakeRequest HandshakeCommandInfo { get; private set; }
    }
}
