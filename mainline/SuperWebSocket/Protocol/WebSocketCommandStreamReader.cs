using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using System.IO;
using SuperSocket.Common;

namespace SuperWebSocket.Protocol
{
    class WebSocketCommandStreamReader : ICommandStreamReader<WebSocketCommandInfo>
    {
        private Stream m_UnderlyingStream;
        private WebSocketContext m_SocketContext;
        private byte[] m_Buffer;
        private ArraySegmentList<byte> m_BufferSegement = new ArraySegmentList<byte>();
        private int m_BufferLength = 0;
        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);
        private static readonly byte[] m_StartMark = new byte[] { WebSocketConstant.StartByte };
        private static readonly byte[] m_EndMark = new byte[] { WebSocketConstant.EndByte };
        private bool m_GotHandshake = false;
        private bool m_GotStartMark = false;

        #region ICommandStreamReader<WebSocketCommandInfo> Members

        public void InitializeReader(SocketContext context, Stream stream, Encoding encoding, int bufferSize)
        {
            m_UnderlyingStream = stream;
            m_SocketContext = context as WebSocketContext;
            m_Buffer = new byte[bufferSize];
        }

        private void PrepareReceiveBuffer()
        {
            if (m_BufferLength > 0)
            {
                //No left buffer
                if (m_Buffer.Length - m_BufferLength <= 0)
                {
                    //Push buffer into buffer segement
                    m_BufferSegement.AddSegment(new ArraySegment<byte>(m_Buffer));
                    m_Buffer = new byte[m_Buffer.Length];
                    m_BufferLength = 0;
                }
            }
        }

        private WebSocketCommandInfo CreateHeadCommandInfo()
        {
            var commandInfo = new WebSocketCommandInfo(WebSocketConstant.CommandHead, string.Empty);
            
            var secWebSocketKey1 = m_SocketContext.SecWebSocketKey1;
            var secWebSocketKey2 = m_SocketContext.SecWebSocketKey2;

            if (!string.IsNullOrEmpty(secWebSocketKey1) || !string.IsNullOrEmpty(secWebSocketKey2))
            {
                List<byte> secKey3List = new List<byte>();

                if (m_BufferSegement.Count >= 8)
                {
                    m_SocketContext.SecWebSocketKey3 = m_BufferSegement.ToArrayData(0, 8);

                    if (m_BufferSegement.Count > 8)
                    {
                        var leftBuffer = m_BufferSegement.ToArrayData(8, m_BufferSegement.Count - 8);
                        m_BufferSegement.ClearSegements();
                        m_BufferSegement.AddSegment(new ArraySegment<byte>(leftBuffer));
                    }
                    else
                    {
                        m_BufferSegement.ClearSegements();
                    }

                    return commandInfo;
                }
                else
                {
                    secKey3List.AddRange(m_BufferSegement.ToArray());
                }

                int requireCount = 8 - secKey3List.Count;

                if (m_BufferLength > 0)
                {
                    if (m_BufferLength > requireCount)
                    {
                        secKey3List.AddRange(m_Buffer.Take(requireCount));
                        Buffer.BlockCopy(m_Buffer, requireCount, m_Buffer, 0, m_BufferLength - requireCount);
                        m_BufferLength = m_BufferLength - requireCount;
                    }
                    else if (m_BufferLength == requireCount)
                    {
                        secKey3List.AddRange(m_Buffer.Take(m_BufferLength));
                        m_BufferLength = 0;
                    }
                    else
                    {
                        secKey3List.AddRange(m_Buffer.Take(m_BufferLength));
                        m_BufferLength = 0;
                    }
                }

                requireCount = 8 - secKey3List.Count;

                if (requireCount == 0)
                {
                    m_SocketContext.SecWebSocketKey3 = secKey3List.ToArray();
                    return commandInfo;
                }

                int thisRead = 0;

                while (requireCount > 0)
                {
                    thisRead = m_UnderlyingStream.Read(m_Buffer, m_BufferLength, requireCount);
                    secKey3List.AddRange(m_Buffer.Skip(m_BufferLength).Take(thisRead));
                    requireCount -= thisRead;
                    m_BufferLength += thisRead;
                }

                m_SocketContext.SecWebSocketKey3 = secKey3List.ToArray();
                return commandInfo;
            }

            return commandInfo;
        }

        private int ProcessReadStream()
        {
            int thisRead = m_UnderlyingStream.Read(m_Buffer, m_BufferLength, m_Buffer.Length - m_BufferLength);
            m_BufferLength += thisRead;
            return thisRead;
        }

        private WebSocketCommandInfo ReadHandshake()
        {
            int thisRead = 0;

            while (true)
            {
                PrepareReceiveBuffer();

                thisRead = ProcessReadStream();

                //The connection will be closed
                if (thisRead == 0)
                    return null;

                if (thisRead >= m_HeaderTerminator.Length)
                {
                    var result = m_Buffer.SearchMark(0, m_BufferLength, m_HeaderTerminator);

                    //Found terminator
                    if (result.HasValue && result.Value >= 0)
                    {
                        int left = m_BufferLength - (result.Value + m_HeaderTerminator.Length);

                        //No buffer in segment
                        if (m_BufferSegement.Count <= 0)
                        {
                            string header = Encoding.UTF8.GetString(m_Buffer, 0, result.Value);
                            WebSocketServer.ParseHandshake(m_SocketContext, new StringReader(header));
                            m_GotHandshake = true;

                            //Save left buffer
                            if (left > 0)
                            {
                                Buffer.BlockCopy(m_Buffer, result.Value + m_HeaderTerminator.Length, m_Buffer, 0, left);
                                m_BufferLength = left;
                            }
                            else
                            {
                                m_BufferLength = 0;
                            }
                        }
                        else
                        {
                            m_BufferSegement.AddSegment(new ArraySegment<byte>(m_Buffer.Take(result.Value).ToArray()));

                            string header = Encoding.UTF8.GetString(m_BufferSegement.ToArrayData());
                            WebSocketServer.ParseHandshake(m_SocketContext, new StringReader(header));
                            m_GotHandshake = true;

                            m_BufferSegement.ClearSegements();

                            if (left > 0)
                            {
                                Buffer.BlockCopy(m_Buffer, result.Value + m_HeaderTerminator.Length, m_Buffer, 0, left);
                                m_BufferLength = left;
                            }
                            else
                            {
                                m_BufferLength = 0;//byte buffer has been cleared
                            }
                        }

                        return CreateHeadCommandInfo();
                    }
                }
                else
                {
                    //Push into segment and then search terminator
                    m_BufferSegement.AddSegment(new ArraySegment<byte>(m_Buffer.Take(m_BufferLength).ToArray()));
                    m_BufferLength = 0;

                    var result = m_BufferSegement.SearchMark(m_HeaderTerminator);

                    if (result.HasValue && result.Value > 0)
                    {
                        string header = Encoding.UTF8.GetString(m_BufferSegement.ToArrayData(0, result.Value));
                        WebSocketServer.ParseHandshake(m_SocketContext, new StringReader(header));
                        m_GotHandshake = true;
                        
                        int left = m_BufferSegement.Count - (result.Value + m_HeaderTerminator.Length);

                        if (left > 0)
                        {
                            byte[] leftBufferInSegment = m_BufferSegement.ToArrayData(result.Value + m_HeaderTerminator.Length, left);
                            m_BufferSegement.ClearSegements();
                            m_BufferSegement.AddSegment(new ArraySegment<byte>(leftBufferInSegment));
                        }
                        else
                        {
                            m_BufferSegement.ClearSegements();
                        }

                        return CreateHeadCommandInfo();
                    }
                }
            }
        }

        private WebSocketCommandInfo CreateDataCommandInfo(string data)
        {
            return new WebSocketCommandInfo(WebSocketConstant.CommandData, data);
        }

        private WebSocketCommandInfo ReadDataCommand()
        {
            int offset;
            int thisRead;

            while (true)
            {
                PrepareReceiveBuffer();
                offset = m_BufferLength;
                thisRead = ProcessReadStream();

                if (thisRead <= 0)
                    return null;

                if (!m_GotStartMark)
                {
                    //Search start mark
                    var startPos = m_Buffer.SearchMark(offset, thisRead, m_StartMark);

                    if (!startPos.HasValue || startPos.Value < 0)
                        continue;

                    m_GotStartMark = true;

                    var endPos = m_Buffer.SearchMark(startPos.Value + 1, m_BufferLength - startPos.Value - 1, m_EndMark);

                    if (!endPos.HasValue || endPos.Value < 0)
                    {
                        if (startPos.Value != offset)
                        {
                            int copyLength = thisRead - (startPos.Value - offset);
                            Buffer.BlockCopy(m_Buffer, startPos.Value, m_Buffer, 0, copyLength);
                            m_BufferLength = copyLength;
                        }
                        continue;
                    }

                    m_GotStartMark = true;

                    int currentLength = endPos.Value - startPos.Value - 1;
                    var dataCommandInfo = CreateDataCommandInfo(Encoding.UTF8.GetString(m_Buffer, startPos.Value + 1, currentLength));

                    int left = offset + thisRead - endPos.Value - 1;
                    Buffer.BlockCopy(m_Buffer, endPos.Value + 1, m_Buffer, 0, left);
                    m_BufferLength = left;

                    m_GotStartMark = false;

                    if (m_BufferSegement.Count > 0)
                        m_BufferSegement.ClearSegements();

                    return dataCommandInfo;
                }
                else
                {
                    var endPos = m_Buffer.SearchMark(offset, thisRead, m_EndMark);

                    if (!endPos.HasValue || endPos.Value < 0)
                        continue;

                    m_BufferSegement.AddSegment(new ArraySegment<byte>(m_Buffer.Take(endPos.Value + 1).ToArray()));
                    var dataCommandInfo = CreateDataCommandInfo(Encoding.UTF8.GetString(m_BufferSegement.ToArrayData(1, m_BufferSegement.Count - 2)));
                    m_BufferSegement.ClearSegements();
                    m_GotStartMark = false;

                    int left = m_BufferLength - endPos.Value - 1;
                    if (left > 0)
                    {
                        var startPos = m_Buffer.SearchMark(endPos.Value + 1, left, m_StartMark);

                        if (startPos.HasValue && startPos.Value >= 0)
                        {
                            m_GotStartMark = true;
                            left = m_BufferLength - startPos.Value;
                            Buffer.BlockCopy(m_Buffer, startPos.Value, m_Buffer, 0, left);
                            m_BufferLength = left;
                        }
                        else
                        {
                            m_BufferLength = 0;
                        }
                    }
                    else
                    {
                        m_BufferLength = 0;
                    }

                    return dataCommandInfo;
                }
            }
        }

        public WebSocketCommandInfo ReadCommand()
        {
            if (!m_GotHandshake)
            {
                var handshakeCmd = ReadHandshake();
                
                m_BufferLength = 0;
                m_BufferSegement.ClearSegements();

                return handshakeCmd;
            }

            return ReadDataCommand();
        }

        #endregion
    }
}
