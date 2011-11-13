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

        public DateTime LastActiveTime { get; internal set; }

        internal IProtocolProcessor ProtocolProcessor { get; private set; }

        internal Uri TargetUri { get; private set; }

        internal string SubProtocol { get; private set; }

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
            : this(uri, subProtocol, GetProtocolProcessor(version))
        {
            Version = version;
        }

        public WebSocket(string uri, string subProtocol, IProtocolProcessor protocolProcessor)
            : base(protocolProcessor.CreateHandshakeReader(), new List<Assembly> { typeof(WebSocket).Assembly })
        {
            ProtocolProcessor = protocolProcessor;

            TargetUri = new Uri(uri);

            SubProtocol = subProtocol;

            if ("wss".Equals(TargetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("SuperWebSocket cannot support wss yet.", "uri");
            }

            if (!"ws".Equals(TargetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid websocket address's schema.", "uri");
            }

            IPAddress ipAddress;

            if (IPAddress.TryParse(TargetUri.Host, out ipAddress))
                RemoteEndPoint = new IPEndPoint(ipAddress, TargetUri.Port);
            else
                RemoteEndPoint = new DnsEndPoint(TargetUri.Host, TargetUri.Port);

            Connect();
        }

        private static IProtocolProcessor GetProtocolProcessor(WebSocketVersion version)
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

        protected override void OnConnected()
        {
            ProtocolProcessor.SendHandshake(this);
        }

        protected virtual void OnHandshaked()
        {

        }
    }
}
