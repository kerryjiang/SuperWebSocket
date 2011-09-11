using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Protocol
{
    public class WebSocketSecKey3Reader : WebSocketReaderBase
    {
        private IProtocolProcessor m_ProtocolProcessor;

        public WebSocketSecKey3Reader(WebSocketReaderBase prevReader)
            : base(prevReader)
        {
            m_ProtocolProcessor = ((IWebSocketServer)this.AppServer).WebSocketProtocolProcessor;
        }

        public override WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            var webSocketSession = session as IWebSocketSession;

            int total = BufferSegments.Count + length;

            if (total == 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(BufferSegments.ToArrayData());
                key.AddRange(readBuffer.Skip(offset).Take(length));
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key.ToArray();
                BufferSegments.ClearSegements();
                left = 0;
                Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                return HandshakeCommandInfo;
            }
            else if (total > 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(BufferSegments.ToArrayData());
                key.AddRange(readBuffer.Skip(offset).Take(8 - BufferSegments.Count));
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key.ToArray();
                BufferSegments.ClearSegements();
                left = total - 8;
                Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                return HandshakeCommandInfo;
            }
            else
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                left = 0;
                NextCommandReader = this;
                return null;
            }
        }
    }
}
