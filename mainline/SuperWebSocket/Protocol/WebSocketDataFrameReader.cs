using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperWebSocket.Protocol
{
    class WebSocketDataFrameReader : WebSocketReaderBase
    {
        private WebSocketDataFrame m_Frame;
        private IDataFramePartReader m_PartReader;
        private int m_LastPartLength = 0;

        public WebSocketDataFrameReader(IAppServer appServer)
            : base(appServer)
        {
            m_Frame = new WebSocketDataFrame(BufferSegments);
            m_PartReader = DataFramePartReader.NewReader;
        }

        public WebSocketDataFrameReader(WebSocketReaderBase prevReader)
            : base(prevReader)
        {
            m_Frame = new WebSocketDataFrame(BufferSegments);
            m_PartReader = DataFramePartReader.NewReader;
        }

        public override WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            this.AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            IDataFramePartReader nextPartReader;

            int thisLength = m_PartReader.Process(m_LastPartLength, m_Frame, out nextPartReader);

            if (thisLength < 0)
            {
                left = 0;
                return null;
            }
            else
            {
                left = thisLength;

                //Means this part reader is the last one
                if (nextPartReader == null)
                {
                    var commandInfo = new WebSocketCommandInfo(Encoding.UTF8.GetString(BufferSegments.ToArrayData(m_LastPartLength, (int)m_Frame.ActualPayloadLength)));

                    BufferSegments.ClearSegements();
                    m_LastPartLength = 0;
                    m_PartReader = DataFramePartReader.NewReader;

                    return commandInfo;
                }
                else
                {
                    m_LastPartLength = BufferSegments.Count - thisLength;
                    m_PartReader = nextPartReader;

                    return null;
                }
            }
        }
    }
}
