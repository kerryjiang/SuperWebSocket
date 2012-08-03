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
    public class WebSocketProtocol : ICustomProtocol<IWebSocketFragment>
    {
        public WebSocketProtocol()
        {

        }

        #region ICustomProtocol<WebSocketCommandInfo> Members

        public ICommandReader<IWebSocketFragment> CreateCommandReader(IAppServer appServer)
        {
            return new WebSocketHeaderReader(appServer as IWebSocketServer);
        }

        #endregion
    }
}
