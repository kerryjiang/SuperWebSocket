using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    class DraftHixie75Processor<TWebSocketSession> : HandshakeProcessorBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override bool Handshake(TWebSocketSession session)
        {
            var responseBuilder = new StringBuilder();

            responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            responseBuilder.AppendLine("Upgrade: WebSocket");
            responseBuilder.AppendLine("Connection: Upgrade");

            if (!string.IsNullOrEmpty(session.Origin))
                responseBuilder.AppendLine(string.Format("WebSocket-Origin: {0}", session.Origin));
            responseBuilder.AppendLine(string.Format("WebSocket-Location: {0}://{1}{2}", session.AppServer.WebSocketUriSufix, session.Host, session.Path));
            responseBuilder.AppendLine();
            session.SendRawResponse(responseBuilder.ToString());

            return true;
        }
    }
}
