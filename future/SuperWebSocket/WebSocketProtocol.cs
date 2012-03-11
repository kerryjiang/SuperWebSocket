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
    public class WebSocketProtocol : IRequestFilterFactory<IWebSocketFragment>
    {
        public WebSocketProtocol()
        {

        }

        public IRequestFilter<IWebSocketFragment> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new WebSocketHeaderRequestFilter();
        }
    }
}
