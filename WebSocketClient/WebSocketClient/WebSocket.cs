using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient
{
    public partial class WebSocket : TcpClientSession<WebSocketCommandInfo, WebSocketContext>
    {
        public WebSocket(string uri)
            : this(uri, string.Empty, null)
        {

        }

        public WebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, null)
        {

        }

        public WebSocket(string uri, string subProtocol, IEnumerable<Assembly> commandAssemblies)
            : base(new WebSocketCommandReader(), commandAssemblies)
        {
            var targetUri = new Uri(uri);

            if ("wss".Equals(targetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("SuperWebSocket cannot support wss yet.", "uri");
            }

            if (!"ws".Equals(targetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid websocket address's schema.", "uri");
            }
        }
    }
}
