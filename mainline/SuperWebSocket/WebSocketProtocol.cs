using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket
{
    public class WebSocketProtocol : SocketProtocolBase, IAsyncProtocol<WebSocketCommandInfo>, ISyncProtocol<WebSocketCommandInfo>
    {
        public WebSocketProtocol()
        {
        }

        #region IAsyncProtocol Members

        public ICommandAsyncReader<WebSocketCommandInfo> CreateAsyncCommandReader()
        {
            return new HeaderAsyncReader();
        }

        #endregion

        #region ISyncProtocol<WebSocketCommandInfo> Members

        public ICommandStreamReader<WebSocketCommandInfo> CreateSyncCommandReader()
        {
            return new WebSocketCommandStreamReader();
        }

        #endregion
    }
}
