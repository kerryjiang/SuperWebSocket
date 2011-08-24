using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    class DraftHixie75Processor : ProtocolProcessorBase
    {
        public override bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<WebSocketCommandInfo> dataFrameReader)
        {
            var responseBuilder = new StringBuilder();

            responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            responseBuilder.AppendLine("Upgrade: WebSocket");
            responseBuilder.AppendLine("Connection: Upgrade");

            if (!string.IsNullOrEmpty(session.Origin))
                responseBuilder.AppendLine(string.Format("WebSocket-Origin: {0}", session.Origin));
            responseBuilder.AppendLine(string.Format("WebSocket-Location: {0}://{1}{2}", session.UriScheme, session.Host, session.Path));

            var subProtocol = session.GetAvailableSubProtocol(session.Items.GetValue<string>(WebSocketConstant.WebSocketProtocol, string.Empty));

            if(!string.IsNullOrEmpty(subProtocol))
                responseBuilder.AppendLine(string.Format("Sec-WebSocket-Protocol: {0}", subProtocol));
            
            responseBuilder.AppendLine();
            session.SendRawResponse(responseBuilder.ToString());

            dataFrameReader = new WebSocketDataReader(previousReader);

            return true;
        }
    }
}
