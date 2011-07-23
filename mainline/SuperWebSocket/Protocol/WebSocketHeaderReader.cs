using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    public class WebSocketHeaderReader : WebSocketReaderBase
    {
        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        public WebSocketHeaderReader(IAppServer server)
            : base(server)
        {

        }

        public override WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            int? result = BufferSegments.SearchMark(m_HeaderTerminator);

            if (!result.HasValue || result.Value <= 0)
            {
                NextCommandReader = this;
                return null;
            }

            string header = Encoding.UTF8.GetString(BufferSegments.ToArrayData(0, result.Value));

            var webSocketSession = session as IWebSocketSession;

            WebSocketServer.ParseHandshake(webSocketSession, new StringReader(header));

            var secWebSocketKey1 = webSocketSession.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey1, string.Empty);
            var secWebSocketKey2 = webSocketSession.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey2, string.Empty);
            var secWebSocketVersion = webSocketSession.SecWebSocketVersion;

            int left = BufferSegments.Count - result.Value - m_HeaderTerminator.Length;

            BufferSegments.ClearSegements();

            if (string.IsNullOrEmpty(secWebSocketKey1) && string.IsNullOrEmpty(secWebSocketKey2))
            {
                //draft-hixie-thewebsocketprotocol-75
                if (left > 0)
                    AddArraySegment(readBuffer, offset + length - left, left, isReusableBuffer);

                Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                return HandshakeCommandInfo;
            }
            else if ("6".Equals(secWebSocketVersion)) //draft-ietf-hybi-thewebsocketprotocol-06
            {
                if (left > 0)
                    AddArraySegment(readBuffer, offset + length - left, left, isReusableBuffer);

                Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                return HandshakeCommandInfo;
            }
            else
            {
                //draft-hixie-thewebsocketprotocol-76/draft-ietf-hybi-thewebsocketprotocol-00
                //Read SecWebSocketKey3(8 bytes)
                if (left == 8)
                {
                    webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = readBuffer.Skip(offset + length - left).Take(left).ToArray();

                    Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                    return HandshakeCommandInfo;
                }
                else if (left > 8)
                {
                    webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = readBuffer.Skip(offset + length - left).Take(8).ToArray();
                    AddArraySegment(readBuffer, offset + length - left + 8, left - 8, isReusableBuffer);

                    Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                    return HandshakeCommandInfo;
                }
                else
                {
                    //left < 8
                    if (left > 0)
                        AddArraySegment(readBuffer, offset + length - left, left, isReusableBuffer);

                    NextCommandReader = new WebSocketSecKey3Reader(this);
                    return null;
                }
            }
        }
    }
}
