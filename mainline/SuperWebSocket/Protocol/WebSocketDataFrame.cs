using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperWebSocket.Protocol
{
    public class WebSocketDataFrame
    {
        private ArraySegmentList<byte> m_InnerData;

        public ArraySegmentList<byte> InnerData
        {
            get { return m_InnerData; }
        }

        public WebSocketDataFrame(ArraySegmentList<byte> data)
        {
            m_InnerData = data;
            m_InnerData.ClearSegements();
        }

        public bool FIN
        {
            get { return ((m_InnerData[0] & 0x80) == 0x80); }
        }

        public bool RSV1
        {
            get { return ((m_InnerData[0] & 0x40) == 0x40); }
        }

        public bool RSV2
        {
            get { return ((m_InnerData[0] & 0x20) == 0x20); }
        }

        public bool RSV3
        {
            get { return ((m_InnerData[0] & 0x10) == 0x10); }
        }

        public sbyte OpCode
        {
            get { return (sbyte)(m_InnerData[0] & 0x0f); }
        }

        public bool HasMask
        {
            get { return ((m_InnerData[1] & 0x80) == 0x80); }
        }

        public sbyte PayloadLenght
        {
            get { return (sbyte)(m_InnerData[1] & 0x7f); }
        }

        private long m_ActualPayloadLength = -1;

        public long ActualPayloadLength
        {
            get
            {
                if (m_ActualPayloadLength >= 0)
                    return m_ActualPayloadLength;

                var payloadLength = PayloadLenght;

                if (payloadLength < 126)
                    m_ActualPayloadLength = payloadLength;
                else if (payloadLength == 126)
                    m_ActualPayloadLength = BitConverter.ToUInt16(m_InnerData.ToArrayData(2, 2), 0);
                else
                    m_ActualPayloadLength = BitConverter.ToInt64(m_InnerData.ToArrayData(2, 8), 0);

                return m_ActualPayloadLength;
            }
        }

        public byte[] MaskKeyOffset { get; set; }

        public byte[] ExtensionData { get; set; }

        public byte[] ApplicationData { get; set; }

        public int Length
        {
            get { return m_InnerData.Count; }
        }
    }
}
