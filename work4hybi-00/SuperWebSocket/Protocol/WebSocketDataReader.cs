using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    public class WebSocketDataReader : WebSocketReaderBase
    {
        //-1 means that don't find start pos
        private int m_StartPos = -1;

        public WebSocketDataReader(WebSocketReaderBase prevReader, int startPos)
            : base(prevReader)
        {
            m_StartPos = startPos;
        }

        public WebSocketDataReader(WebSocketReaderBase prevReader)
            : base(prevReader)
        {

        }

        public override WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            if (m_StartPos < 0)
            {
                m_StartPos = BufferSegments.IndexOf(WebSocketConstant.StartByte);

                if (m_StartPos < 0)
                {
                    //Continue to read following bytes to seek start pos
                    NextCommandReader = this;
                    return null;
                }
            }

            int endPos = BufferSegments.IndexOf(WebSocketConstant.EndByte, m_StartPos, BufferSegments.Count - m_StartPos);

            if (endPos < 0)
            {
                //Continue to search end byte
                NextCommandReader = this;
                return null;
            }

            var commandInfo = new WebSocketCommandInfo(Encoding.UTF8.GetString(BufferSegments.ToArrayData(m_StartPos + 1, endPos - m_StartPos - 1)));

            BufferSegments.ClearSegements();

            int left = BufferSegments.Count - endPos - 1;

            if (left > 0)
                AddArraySegment(readBuffer, offset + length - left, left, isReusableBuffer);

            m_StartPos = -1;
            NextCommandReader = this;
            return commandInfo;
        }
    }
}
