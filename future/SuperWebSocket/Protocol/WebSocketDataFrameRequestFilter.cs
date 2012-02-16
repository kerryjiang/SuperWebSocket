using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol.FramePartReader;

namespace SuperWebSocket.Protocol
{
    class WebSocketDataFrameRequestFilter : IRequestFilter<WebSocketRequestInfo>
    {
        private List<WebSocketDataFrame> m_PreviousFrames;
        private WebSocketDataFrame m_Frame;
        private IDataFramePartReader m_PartReader;
        private int m_LastPartLength = 0;

        public int LeftBufferSize
        {
            get { return m_Frame.InnerData.Count; }
        }

        public IRequestFilter<WebSocketRequestInfo> NextRequestFilter
        {
            get { return this; }
        }

        public WebSocketDataFrameRequestFilter()
        {
            m_Frame = new WebSocketDataFrame(new ArraySegmentList());
            m_PartReader = DataFramePartReader.NewReader;
        }

        protected void AddArraySegment(ArraySegmentList segments, byte[] buffer, int offset, int length, bool isReusableBuffer)
        {
            segments.AddSegment(buffer, offset, length, isReusableBuffer);
        }

        public WebSocketRequestInfo Filter(IAppSession<WebSocketRequestInfo> session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
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
                    WebSocketRequestInfo commandInfo;

                    if (m_Frame.FIN)
                    {
                        if (!m_Frame.HasMask)
                        {
                            //Mask is required for client to server fragment
                            //http://tools.ietf.org/html/rfc6455#section-5.3
                            var websocketSession = session as WebSocketSession;
                            websocketSession.CloseWithHandshake(websocketSession.ProtocolProcessor.CloseStatusClode.ProtocolError, "Mask is required!");
                            return null;
                        }

                        if (m_PreviousFrames != null && m_PreviousFrames.Count > 0)
                        {
                            m_PreviousFrames.Add(m_Frame);
                            m_Frame = new WebSocketDataFrame(new ArraySegmentList());
                            commandInfo = new WebSocketRequestInfo(m_PreviousFrames);
                            m_PreviousFrames = null;
                        }
                        else
                        {
                            commandInfo = new WebSocketRequestInfo(m_Frame);
                            m_Frame.Clear();
                        }
                    }
                    else
                    {
                        if (m_PreviousFrames == null)
                            m_PreviousFrames = new List<WebSocketDataFrame>();

                        m_PreviousFrames.Add(m_Frame);
                        m_Frame = new WebSocketDataFrame(new ArraySegmentList());

                        commandInfo = null;
                    }
                    
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
