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
    /// <summary>
    /// WebSocketRequestFilter basis
    /// </summary>
    public abstract class WebSocketRequestFilterBase : RequestFilterBase<IWebSocketFragment>
    {
        /// <summary>
        /// The length of Sec3Key
        /// </summary>
        protected const int SecKey3Len = 8;

        private readonly IWebSocketSession m_Session;

        internal IWebSocketSession Session
        {
            get { return m_Session; }
        }

        static WebSocketRequestFilterBase()
        {
            HandshakeRequestInfo = new HandshakeRequest();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketRequestFilterBase"/> class.
        /// </summary>
        protected WebSocketRequestFilterBase(IWebSocketSession session)
        {
            m_Session = session;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketRequestFilterBase"/> class.
        /// </summary>
        /// <param name="previousRequestFilter">The previous request filter.</param>
        protected WebSocketRequestFilterBase(WebSocketRequestFilterBase previousRequestFilter)
            : base(previousRequestFilter)
        {
            m_Session = previousRequestFilter.Session;
        }

        /// <summary>
        /// Handshakes the specified protocol processor.
        /// </summary>
        /// <param name="protocolProcessor">The protocol processor.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the handshake request info.
        /// </summary>
        protected static IWebSocketFragment HandshakeRequestInfo { get; private set; }
    }
}
