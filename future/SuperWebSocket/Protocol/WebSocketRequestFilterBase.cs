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
    public abstract class WebSocketRequestFilterBase : RequestFilterBase<IWebSocketFragment>
    {
        protected const int SecKey3Len = 8;

        static WebSocketRequestFilterBase()
        {
            HandshakeRequestInfo = new HandshakeRequest();
        }

        public WebSocketRequestFilterBase()
            : base()
        {

        }

        public WebSocketRequestFilterBase(WebSocketRequestFilterBase previousRequestFilter)
            : base(previousRequestFilter)
        {

        }

        protected bool Handshake(IProtocolProcessor protocolProcessor, IWebSocketSession session)
        {
            IRequestFilter<IWebSocketFragment> dataFrameReader;

            if (!protocolProcessor.Handshake(session, this, out dataFrameReader))
            {
                session.Close(CloseReason.ServerClosing);
                return false;
            }

            //Processor handshake sucessfully, but output datareader is null, so the multiple protocol switch handled the handshake
            //In this case, the handshake is not completed
            if (dataFrameReader == null)
            {
                NextRequestFilter = this;
                return false;
            }

            NextRequestFilter = dataFrameReader;
            return true;
        }

        protected static IWebSocketFragment HandshakeRequestInfo { get; private set; }
    }
}
