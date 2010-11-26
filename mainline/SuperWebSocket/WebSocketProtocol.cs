using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.Protocol;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;
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
