using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using SuperSocket.ClientEngine;
using SuperWebSocket.WebSocketClient.Protocol;

namespace SuperWebSocket.WebSocketClient
{
    public partial class WebSocket : TcpClientSession<WebSocketCommandInfo, WebSocketContext>
    {
        public WebSocketVersion Version { get; private set; }

        private IProtocolProcessor m_ProtocolProcessor;

        public WebSocket(string uri)
            : this(uri, string.Empty)
        {

        }

        public WebSocket(string uri, WebSocketVersion version)
            : this(uri, string.Empty, version)
        {

        }

        public WebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, WebSocketVersion.DraftHybi10)
        {

        }

        public WebSocket(string uri, string subProtocol, WebSocketVersion version)
            : base(new WebSocketCommandReader(), new List<Assembly> { typeof(WebSocket).Assembly })
        {
            Version = version;
            m_ProtocolProcessor = GetProtocolProcessor(version);

            var targetUri = new Uri(uri);

            if ("wss".Equals(targetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("SuperWebSocket cannot support wss yet.", "uri");
            }

            if (!"ws".Equals(targetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid websocket address's schema.", "uri");
            }

            IPAddress ipAddress;

            if (IPAddress.TryParse(targetUri.Host, out ipAddress))
                RemoteEndPoint = new IPEndPoint(ipAddress, targetUri.Port);
            else
                RemoteEndPoint = new DnsEndPoint(targetUri.Host, targetUri.Port);

            Connect();
        }

        private IProtocolProcessor GetProtocolProcessor(WebSocketVersion version)
        {
            switch (version)
            {
                case(WebSocketVersion.DraftHybi00):
                    return new DraftHybi00Processor();
                case(WebSocketVersion.DraftHybi10):
                    return new DraftHybi10Processor();
            }

            throw new ArgumentException("Invalid websocket version");
        }
    }
}
