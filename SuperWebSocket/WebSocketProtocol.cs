using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket
{
    /// <summary>
    /// WebSocket protocol
    /// </summary>
    public class WebSocketProtocol : IRequestFilterFactory<IWebSocketFragment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketProtocol"/> class.
        /// </summary>
        public WebSocketProtocol()
        {

        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        public IRequestFilter<IWebSocketFragment> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new WebSocketHeaderRequestFilter();
        }
    }
}
