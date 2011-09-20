using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    class WebSocketDataFrameReader : ICommandReader<WebSocketCommandInfo>
    {
        private List<WebSocketDataFrame> m_PreviousFrames;
        private WebSocketDataFrame m_Frame;
        private IDataFramePartReader m_PartReader;
        private int m_LastPartLength = 0;

        public IAppServer AppServer { get; private set; }

        public int LeftBufferSize
        {
            get { return m_Frame.InnerData.Count; }
        }

        public ICommandReader<WebSocketCommandInfo> NextCommandReader
        {
            get { return this; }
        }

        public WebSocketDataFrameReader(IAppServer appServer)
        {
            AppServer = appServer;
            m_Frame = new WebSocketDataFrame(new ArraySegmentList<byte>());
            m_PartReader = DataFramePartReader.NewReader;
        }

        protected void AddArraySegment(ArraySegmentList<byte> segments, byte[] buffer, int offset, int length, bool isReusableBuffer)
        {
            if (isReusableBuffer)
                segments.AddSegment(new ArraySegment<byte>(buffer.CloneRange(offset, length)));
            else
                segments.AddSegment(new ArraySegment<byte>(buffer, offset, length));
        }

        public WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            this.AddArraySegment(m_Frame.InnerData, readBuffer, offset, length, isReusableBuffer);

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

                if (left > 0)
                    m_Frame.InnerData.TrimEnd(left);

                //Means this part reader is the last one
                if (nextPartReader == null)
                {
                    WebSocketCommandInfo commandInfo;

                    if (m_Frame.FIN)
                    {
                        if (m_PreviousFrames != null && m_PreviousFrames.Count > 0)
                        {
                            m_PreviousFrames.Add(m_Frame);
                            m_Frame = new WebSocketDataFrame(new ArraySegmentList<byte>());
                            commandInfo = new WebSocketCommandInfo(m_PreviousFrames);
                            m_PreviousFrames = null;
                        }
                        else
                        {
                            commandInfo = new WebSocketCommandInfo(m_Frame);
                            m_Frame.Clear();
                        }
                    }
                    else
                    {
                        if (m_PreviousFrames == null)
                            m_PreviousFrames = new List<WebSocketDataFrame>();

                        m_PreviousFrames.Add(m_Frame);
                        m_Frame = new WebSocketDataFrame(new ArraySegmentList<byte>());

                        commandInfo = null;
                    }

                    //BufferSegments.ClearSegements();
                    m_LastPartLength = 0;
                    m_PartReader = DataFramePartReader.NewReader;

                    return commandInfo;
                }
                else
                {
                    m_LastPartLength = m_Frame.InnerData.Count - thisLength;
                    m_PartReader = nextPartReader;

                    return null;
                }
            }
        }
    }
}
