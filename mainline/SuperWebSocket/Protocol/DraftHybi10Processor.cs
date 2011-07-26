using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    class DraftHybi10Processor : ProtocolProcessorBase
    {
        private const string m_SecWebSocketVersion = "8";
        private const string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public override bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<WebSocketCommandInfo> dataFrameReader)
        {
            dataFrameReader = null;

            if (!m_SecWebSocketVersion.Equals(session.SecWebSocketVersion) && NextProcessor != null)
            {
                return NextProcessor.Handshake(session, previousReader, out dataFrameReader);
            }

            var secWebSocketKey = session.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey, string.Empty);

            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                return false;
            }

            var responseBuilder = new StringBuilder();

            string secKeyAccept = string.Empty;

            try
            {
                secKeyAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey + m_Magic)));
            }
            catch (Exception)
            {
                return false;
            }

            responseBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
            responseBuilder.AppendLine("Upgrade: WebSocket");
            responseBuilder.AppendLine("Connection: Upgrade");
            responseBuilder.AppendLine(string.Format("Sec-WebSocket-Accept: {0}", secKeyAccept));
            responseBuilder.AppendLine();
            session.SendRawResponse(responseBuilder.ToString());

            dataFrameReader = new WebSocketDataFrameReader(previousReader);

            return true;
        }
    }
}
