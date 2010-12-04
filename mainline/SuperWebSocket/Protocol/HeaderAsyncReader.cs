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
    public class HeaderAsyncReader : AsyncReaderBase
    {
        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        public HeaderAsyncReader()
        {
            Segments = new ArraySegmentList<byte>();
        }

        #region ICommandAsyncReader Members

        public override WebSocketCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            int? result = Segments.SearchMark(m_HeaderTerminator);

            if (!result.HasValue || result.Value <= 0)
            {
                NextCommandReader = this;
                return null;
            }

            string header = Encoding.UTF8.GetString(Segments.ToArrayData(0, result.Value));

            var socketContext = context as WebSocketContext;

            WebSocketServer.ParseHandshake(socketContext, new StringReader(header));

            var secWebSocketKey1 = socketContext.SecWebSocketKey1;
            var secWebSocketKey2 = socketContext.SecWebSocketKey2;

            int left = Segments.Count - result.Value - m_HeaderTerminator.Length;

            Segments.ClearSegements();
            
            if (string.IsNullOrEmpty(secWebSocketKey1) && string.IsNullOrEmpty(secWebSocketKey2))
            {
                //v.75
                if (left > 0)
                    AddArraySegment(readBuffer, offset + length - left, left, isReusableBuffer);

                NextCommandReader = new DataAsyncReader(this);
                return CreateHeadCommandInfo();
            }
            else
            {
                //v.76
                //Read SecWebSocketKey3(8 bytes)
                if (left == 8)
                {
                    socketContext.SecWebSocketKey3 = readBuffer.Skip(offset + length - left).Take(left).ToArray();
                    NextCommandReader = new DataAsyncReader(this);
                    return CreateHeadCommandInfo();
                }
                else if (left > 8)
                {
                    socketContext.SecWebSocketKey3 = readBuffer.Skip(offset + length - left).Take(8).ToArray();
                    AddArraySegment(readBuffer, offset + length - left + 8, left - 8, isReusableBuffer);
                    NextCommandReader = new DataAsyncReader(this);
                    return CreateHeadCommandInfo();
                }
                else
                {
                    //left < 8
                    if(left > 0)
                        AddArraySegment(readBuffer, offset + length - left, left, isReusableBuffer);

                    NextCommandReader = new SecKey3AsyncReader(this);
                    return null;
                }
            }
        }

        #endregion        
    }
}
