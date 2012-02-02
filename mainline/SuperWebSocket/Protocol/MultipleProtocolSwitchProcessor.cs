using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455#section-4.4
    /// </summary>
    class MultipleProtocolSwitchProcessor : IProtocolProcessor
    {
        private int[] m_AvailableVersions;

        private string m_AvailableVersionLine;

        public MultipleProtocolSwitchProcessor(int[] availableVersions)
        {
            m_AvailableVersions = availableVersions;
            m_AvailableVersionLine = "Sec-WebSocket-Version: " + string.Join(", ", availableVersions.Select(i => i.ToString()).ToArray());
        }

        public bool CanSendBinaryData { get { return false; } }

        public ICloseStatusCode CloseStatusClode { get; set; }

        public IProtocolProcessor NextProcessor { get; set; }

        public bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<WebSocketCommandInfo> dataFrameReader)
        {
            dataFrameReader = null;

            var responseBuilder = new StringBuilder();

            responseBuilder.AppendWithCrCf("HTTP/1.1 400 Bad Request");
            responseBuilder.AppendWithCrCf("Upgrade: WebSocket");
            responseBuilder.AppendWithCrCf("Connection: Upgrade");
            responseBuilder.AppendWithCrCf(m_AvailableVersionLine);
            responseBuilder.AppendWithCrCf();

            session.SocketSession.SendResponse(responseBuilder.ToString());

            return true;
        }

        public void SendMessage(IWebSocketSession session, string message)
        {
            throw new NotImplementedException();
        }

        public void SendData(IWebSocketSession session, byte[] data, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason)
        {
            throw new NotImplementedException();
        }

        public void SendPong(IWebSocketSession session, string ping)
        {
            throw new NotImplementedException();
        }

        public int Version
        {
            get { return 0; }
        }
    }
}
