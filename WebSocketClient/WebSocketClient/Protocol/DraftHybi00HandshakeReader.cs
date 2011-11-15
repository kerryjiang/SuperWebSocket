using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi00HandshakeReader : HandshakeReader
    {
        //-1 indicate response header has not been received
        private int m_ReceivedChallengeLength = -1;
        private int m_ExpectedChallengeLength = 16;

        public DraftHybi00HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            if (m_ReceivedChallengeLength < 0)
            {
                var commandInfo = base.GetCommandInfo(readBuffer, offset, length, out left);

                if (commandInfo == null)
                    return null;

                if (left < m_ExpectedChallengeLength)
                {
                    m_ReceivedChallengeLength = left;
                    this.AddArraySegment(readBuffer, offset + length - left, left);
                    return null;
                }
                else if (left == m_ExpectedChallengeLength)
                {
                    byte[] challenges = readBuffer.CloneRange(offset + length - left, left);
                    return new WebSocketCommandInfo
                        {
                            Key = OpCode.Handshake.ToString(),
                            Data = challenges
                        };
                }
                else
                {
                    byte[] challenges = readBuffer.CloneRange(offset + length - left, m_ExpectedChallengeLength);
                    left -= m_ExpectedChallengeLength;

                    return new WebSocketCommandInfo
                    {
                        Key = OpCode.Handshake.ToString(),
                        Data = challenges
                    };
                }
            }
            else
            {
                int receivedTotal = m_ReceivedChallengeLength + length;
                
                if (receivedTotal < m_ExpectedChallengeLength)
                {
                    left = 0;
                    m_ReceivedChallengeLength = receivedTotal;
                    return null;
                }
                else if (receivedTotal == m_ExpectedChallengeLength)
                {
                    left = 0;
                    this.AddArraySegment(readBuffer, offset, length);
                    byte[] challenges = BufferSegments.ToArrayData();
                    BufferSegments.ClearSegements();

                    return new WebSocketCommandInfo
                    {
                        Key = OpCode.Handshake.ToString(),
                        Data = challenges
                    };
                }
                else
                {
                    this.AddArraySegment(readBuffer, offset, m_ExpectedChallengeLength - m_ReceivedChallengeLength);
                    byte[] challenges = BufferSegments.ToArrayData();
                    BufferSegments.ClearSegements();
                    left = length - (m_ExpectedChallengeLength - m_ReceivedChallengeLength);

                    return new WebSocketCommandInfo
                    {
                        Key = OpCode.Handshake.ToString(),
                        Data = challenges
                    };
                }
            }
        }
    }
}
