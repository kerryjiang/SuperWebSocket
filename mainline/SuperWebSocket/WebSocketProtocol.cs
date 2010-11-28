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
    public class WebSocketProtocol : SocketProtocolBase, IAsyncProtocol<WebSocketCommandInfo>
    {
        public WebSocketProtocol()
        {
        }

        public WebSocketProtocol(ISubProtocol subProtocol)
            : this()
        {
            SubProtocol = subProtocol;
        }        

        public ISubProtocol SubProtocol { get; private set; }

        #region IAsyncProtocol Members

        public ICommandAsyncReader<WebSocketCommandInfo> CreateAsyncCommandReader()
        {
            return new HeaderAsyncReader();
        }

        #endregion
    }
}
