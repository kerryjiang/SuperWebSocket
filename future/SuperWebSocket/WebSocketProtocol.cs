using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase;

namespace SuperWebSocket
{
    public class WebSocketProtocol : IRequestFilterFactory<WebSocketRequestInfo>
    {
        public WebSocketProtocol()
        {

        }

        public IRequestFilter<WebSocketRequestInfo> CreateFilter(IAppServer appServer)
        {
            return new WebSocketHeaderRequestFilter();
        }
    }
}
