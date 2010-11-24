using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System.IO;

namespace SuperWebSocket.Protocol
{
    public class HeaderAsyncReader : AsyncReaderBase
    {
        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        public HeaderAsyncReader(HeaderAsyncReader prevHeaderReader)
        {
            Segments = prevHeaderReader.GetLeftBuffer();
        }

        public HeaderAsyncReader()
        {
            Segments = new ArraySegmentList<byte>();
        }

        #region ICommandAsyncReader Members

        public override WebSocketCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length)
        {
            Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset, length));

            int? result = Segments.SearchMark(m_HeaderTerminator);

            if (!result.HasValue || result.Value <= 0)
            {
                NextCommandReader = new HeaderAsyncReader(this);
                return null;
            }

            string header = Encoding.UTF8.GetString(Segments.ToArrayData(0, result.Value));

            var socketContext = context as WebSocketContext;

            ProcessHead(socketContext, header);

            var secWebSocketKey1 = socketContext.SecWebSocketKey1;
            var secWebSocketKey2 = socketContext.SecWebSocketKey2;

            int left = Segments.Count - result.Value - m_HeaderTerminator.Length;

            Segments.ClearSegements();
            
            if (string.IsNullOrEmpty(secWebSocketKey1) && string.IsNullOrEmpty(secWebSocketKey2))
            {
                //v.75
                if (left > 0)
                    Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left, left));

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
                    Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left + 8, left - 8));
                    NextCommandReader = new DataAsyncReader(this);
                    return CreateHeadCommandInfo();
                }
                else
                {
                    //left < 8
                    if(left > 0)
                        Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left, left));

                    NextCommandReader = new SecKey3AsyncReader(this);
                    return null;
                }
            }
        }

        #endregion

        private void ProcessHead(WebSocketContext context, string header)
        {
            StringReader reader = new StringReader(header);

            string line;
            string firstLine = string.Empty;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = line;
                    continue;
                }

                int pos = line.IndexOf(':');

                string key = line.Substring(0, pos);
                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = line.Substring(pos + 1);
                if (!string.IsNullOrEmpty(value))
                    value = value.TrimStart(' ');

                if (string.IsNullOrEmpty(key))
                    continue;

                context[key] = value;
            }

            var metaInfo = firstLine.Split(' ');

            context.Method = metaInfo[0];
            context.Path = metaInfo[1];
            context.HttpVersion = metaInfo[2];
        }
    }
}
