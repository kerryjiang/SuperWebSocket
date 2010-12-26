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
    public class WebSocketProtocol : ICustomProtocol<WebSocketCommandInfo>
    {
        public WebSocketProtocol()
        {

        }

        #region ICustomProtocol<WebSocketCommandInfo> Members

        public ICommandReader<WebSocketCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new WebSocketHeaderReader(appServer);
        }

        #endregion
    }
}
