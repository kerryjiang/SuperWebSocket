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
    class WebSocketDataRequestFilter : WebSocketRequestFilterBase
    {
        private byte? m_Type;
        private int m_TempLength;
        private int? m_Length;

        private const byte m_ClosingHandshakeType = 0xFF;

        public WebSocketDataRequestFilter(WebSocketRequestFilterBase prevFilter)
            : base(prevFilter)
        {

        }

        public override IWebSocketFragment Filter(IAppSession<IWebSocketFragment> session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            left = 0;

            var skipByteCount = 0;

            if (!m_Type.HasValue)
            {
                byte startByte = readBuffer[offset];
                skipByteCount = 1;
                m_Type = startByte;
            }

            //0xxxxxxx: Collect protocol data by end mark
            if ((m_Type.Value & 0x80) == 0x00)
            {
                byte lookForByte = 0xFF;

                int i;

                for (i = offset + skipByteCount; i < offset + length; i++)
                {
                    if (readBuffer[i] == lookForByte)
                    {
                        left = length - (i - offset + 1);

                        if (BufferSegments.Count <= 0)
                        {
                            var commandInfo = new PlainFragment(Encoding.UTF8.GetString(readBuffer, offset + skipByteCount, i - offset - skipByteCount));
                            Reset();
                            return commandInfo;
                        }
                        else
                        {
                            AddArraySegment(readBuffer, offset + skipByteCount, i - offset - skipByteCount, false);
                            var commandInfo = new PlainFragment(BufferSegments.Decode(Encoding.UTF8));
                            Reset();
                            return commandInfo;
                        }
                    }
                }

                AddArraySegment(readBuffer, offset + skipByteCount, length - skipByteCount, isReusableBuffer);
                return null;
            }
            else//10000000: Collect protocol data by length
            {
                while (!m_Length.HasValue)
                {
                    if (length <= skipByteCount)
                    {
                        //No data to read
                        return null;
                    }

                    byte lengthByte = readBuffer[skipByteCount];
                    //Closing handshake
                    if (lengthByte == 0x00 && m_Type.Value == m_ClosingHandshakeType)
                    {
                        session.Close(CloseReason.ClientClosing);
                        return null;
                    }

                    int thisLength = (int)(lengthByte & 0x7F);
                    m_TempLength = m_TempLength * 128 + thisLength;
                    skipByteCount++;

                    if ((lengthByte & 0x80) != 0x80)
                    {
                        m_Length = m_TempLength;
                        break;
                    }
                }

                int requiredSize = m_Length.Value - BufferSegments.Count;

                int leftSize = length - skipByteCount;

                if (leftSize < requiredSize)
                {
                    AddArraySegment(readBuffer, skipByteCount, length - skipByteCount, isReusableBuffer);
                    return null;
                }
                else
                {
                    left = leftSize - requiredSize;

                    if (BufferSegments.Count <= 0)
                    {
                        var commandInfo = new PlainFragment(Encoding.UTF8.GetString(readBuffer, offset + skipByteCount, requiredSize));
                        Reset();
                        return commandInfo;
                    }
                    else
                    {
                        AddArraySegment(readBuffer, offset + skipByteCount, requiredSize, false);
                        var commandInfo = new PlainFragment(BufferSegments.Decode(Encoding.UTF8));
                        Reset();
                        return commandInfo;
                    }
                }
            }
        }

        void Reset()
        {
            m_Type = null;
            m_Length = null;
            m_TempLength = 0;

            ClearBufferSegments();
        }
    }
}
