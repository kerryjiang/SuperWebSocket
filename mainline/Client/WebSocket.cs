using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SuperWebSocket.Client
{
    public class WebSocket
    {
        private static byte[] m_ClosingHandshake = new byte[] { 0xFF, 0x00 };

        private List<KeyValuePair<string, string>> m_Cookies;
        private EndPoint m_RemoteEndPoint;
        private string m_Path = string.Empty;
        private string m_Host = string.Empty;
        private string m_Protocol = string.Empty;
        private Socket m_Socket;

        private const byte m_StartByte = 0x00;
        private const byte m_EndByte = 0xFF;

        private SocketAsyncEventArgs m_ReceiveAsyncEventArgs;
        private SocketAsyncEventArgs m_SendAsyncEventArgs;

        private byte[] m_SendBuffer;

        private ConcurrentQueue<string> m_MessagesBeingSent = new ConcurrentQueue<string>();

        /// <summary>
        /// It means whether hanshake successfully
        /// </summary>
        private bool m_Connected = false;

        private static List<char> m_CharLib = new List<char>();
        private static List<char> m_DigLib = new List<char>();

        static WebSocket()
        {
            for (int i = 33; i <= 126; i++)
            {
                char currentChar = (char)i;

                if (char.IsLetter(currentChar))
                    m_CharLib.Add(currentChar);
                else if (char.IsDigit(currentChar))
                    m_DigLib.Add(currentChar);
            }
        }

        public WebSocket(string uri, string protocol)
            : this(uri, protocol, new List<KeyValuePair<string, string>>())
        {

        }

        public WebSocket(string uri, string protocol, int sendBufferSize, int receiveBufferSize)
            : this(uri, protocol, new List<KeyValuePair<string, string>>(), sendBufferSize, receiveBufferSize)
        {

        }

        public WebSocket(string uri, string protocol, List<KeyValuePair<string, string>> cookies)
            : this(uri, protocol, cookies, 512, 512)
        {

        }

        public WebSocket(string uri, string protocol, List<KeyValuePair<string, string>> cookies, int sendBufferSize, int receiveBufferSize)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");

            m_Protocol = protocol;

            if (!uri.StartsWith("ws://"))
            {
                if (uri.StartsWith("wss://"))
                {
                    throw new ArgumentException("wss is not supported yet!");
                }
                else
                {
                    throw new ArgumentException("Invalid websocket address!");
                }
            }

            int pos = uri.IndexOf('/', 5);
            if (pos <= 0)
                throw new ArgumentException("Invalid websocket address!");

            m_Path = uri.Substring(pos);

            string host = uri.Substring(5, pos - 5);

            string[] hostInfo = host.Split(':');

            if (hostInfo.Length < 1 || hostInfo.Length > 2)
                throw new ArgumentException("Invalid websocket address!");

            int port;

            if (hostInfo.Length > 1)
            {
                if (!int.TryParse(hostInfo[1], out port))
                    throw new ArgumentException("Invalid websocket address!");
            }
            else
            {
                port = 80;
            }

            m_Host = hostInfo[0];

            IPAddress ipAddress;
            if (!IPAddress.TryParse(m_Host, out ipAddress))
                m_RemoteEndPoint = new DnsEndPoint(m_Host, port);
            else
                m_RemoteEndPoint = new IPEndPoint(ipAddress, port);

            m_Cookies = cookies;

            byte[] buffer = new byte[receiveBufferSize];
            m_ReceiveAsyncEventArgs = new SocketAsyncEventArgs();
            m_ReceiveAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
            m_ReceiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_ReceiveAsyncEventArgs_Completed);

            m_SendBuffer = new byte[sendBufferSize];
            m_SendAsyncEventArgs = new SocketAsyncEventArgs();
            m_SendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_SendAsyncEventArgs_Completed);
        }

        void m_SendAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        void m_ReceiveAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                ProcessConnect(e);
            }
            else if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(e);
            }
        }

        void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                FireOnError(new ErrorEventArgs(e.SocketError));
                return;
            }
#if NET35
            //Do nothing
#else
            m_Socket = e.ConnectSocket;
#endif
            SendHandShake();
        }

        void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                FireOnError(new ErrorEventArgs(e.SocketError));
                EnsureCloseSocket();
                return;
            }

            //If not connected, means it is a handshake sending
            if (!m_Connected)
            {
                //Now receive hanshake response
                StartReceive();
            }
            else
            {
                var sendContext = e.UserToken as SendMessageContext;
                if (sendContext == null)
                    return;

                if (!sendContext.Completed)
                {
                    StartSend(sendContext);
                }
                else
                {
                    StartSendFromQueue();
                }
            }
        }

        void StartReceive()
        {
            if (m_Socket != null && !m_Socket.ReceiveAsync(m_ReceiveAsyncEventArgs))
                ProcessReceive(m_ReceiveAsyncEventArgs);
        }

        void EnsureCloseSocket()
        {
            EnsureCloseSocket(true);
        }

        void EnsureCloseSocket(bool fireEvent)
        {
            if (m_Socket != null)
            {
                if (m_Socket.Connected)
                {
                    try
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                        m_Socket.Close();
                    }
                    catch
                    {

                    }
                }

                m_Socket = null;
            }

            if (m_Connected)
            {
                m_Connected = false;

                if (fireEvent)
                    FireOnClose();
            }
        }

        private EventHandler m_OnOpen;

        public event EventHandler OnOpen
        {
            add { m_OnOpen += value; }
            remove { m_OnOpen -= value; }
        }

        private void FireOnOpen()
        {
            var handler = m_OnOpen;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private EventHandler m_OnClose;

        public event EventHandler OnClose
        {
            add { m_OnClose += value; }
            remove { m_OnClose -= value; }
        }

        private void FireOnClose()
        {
            var handler = m_OnClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private EventHandler<MessageEventArgs> m_OnMessage;

        public event EventHandler<MessageEventArgs> OnMessage
        {
            add { m_OnMessage += value; }
            remove { m_OnMessage -= value; }
        }

        private void FireOnMessage(string message)
        {
            var handler = m_OnMessage;
            if (handler != null)
                handler(this, new MessageEventArgs(message));
        }

        private EventHandler<ErrorEventArgs> m_OnError;

        public event EventHandler<ErrorEventArgs> OnError
        {
            add { m_OnError += value; }
            remove { m_OnError -= value; }
        }

        private void FireOnError(ErrorEventArgs e)
        {
            if (m_OnError == null)
                return;

            m_OnError(this, e);
        }

        public void Connect()
        {
            m_ReceiveAsyncEventArgs.RemoteEndPoint = m_RemoteEndPoint;
#if NET35
            m_Socket = new Socket(m_RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.ConnectAsync(m_ReceiveAsyncEventArgs);
#else
            if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, m_ReceiveAsyncEventArgs))
                ProcessConnect(m_ReceiveAsyncEventArgs);
#endif
        }

        void SendHandShake()
        {
            string secKey1 = Encoding.UTF8.GetString(GenerateSecKey());

            string secKey2 = Encoding.UTF8.GetString(GenerateSecKey());

            byte[] secKey3 = GenerateSecKey(8);

            byte[] expectedResponse = GetResponseSecurityKey(secKey1, secKey2, secKey3);

            m_ReceiveAsyncEventArgs.UserToken = new HandshakeContext { ExpectedChallenge = expectedResponse };

            var handshakeBuilder = new StringBuilder();

            handshakeBuilder.AppendLine(string.Format("GET {0} HTTP/1.1", m_Path));
            handshakeBuilder.AppendLine("Upgrade: WebSocket");
            handshakeBuilder.AppendLine("Connection: Upgrade");
            handshakeBuilder.AppendLine(string.Format("Sec-WebSocket-Key2: {0}", secKey2));
            handshakeBuilder.AppendLine(string.Format("Host: {0}", m_Host));
            handshakeBuilder.AppendLine(string.Format("Sec-WebSocket-Key1: {0}", secKey1));
            handshakeBuilder.AppendLine(string.Format("Origin: {0}", m_Host));

            if (!string.IsNullOrEmpty(m_Protocol))
                handshakeBuilder.AppendLine(string.Format("Sec-WebSocket-Protocol: {0}", m_Protocol));

            if (m_Cookies != null && m_Cookies.Count > 0)
            {
                string[] cookiePairs = new string[m_Cookies.Count];
                for (int i = 0; i < m_Cookies.Count; i++)
                {
                    var item = m_Cookies[i];
                    cookiePairs[i] = item.Key + "=" + Uri.EscapeUriString(item.Value);
                }
                handshakeBuilder.AppendLine(string.Format("Cookie: {0}", string.Join(";", cookiePairs)));
            }

            handshakeBuilder.AppendLine();
            handshakeBuilder.Append(Encoding.UTF8.GetString(secKey3, 0, secKey3.Length));

            byte[] handshakeBuffer = Encoding.UTF8.GetBytes(handshakeBuilder.ToString());

            m_SendAsyncEventArgs.SetBuffer(handshakeBuffer, 0, handshakeBuffer.Length);

            if (!m_Socket.SendAsync(m_SendAsyncEventArgs))
                ProcessSend(m_SendAsyncEventArgs);
        }

        private static readonly byte[] m_NewLineMark = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        List<byte> m_MessBuilder = new List<byte>();

        void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                FireOnError(new ErrorEventArgs(e.SocketError));
                EnsureCloseSocket();
                return;
            }

            if (e.BytesTransferred == 0)
            {
                EnsureCloseSocket();
                return;
            }

            if (!m_Connected)
            {
                ProcessHanshake(e);
                return;
            }

            ProcessReceiveData(e.Buffer, e.Offset, e.BytesTransferred);
            StartReceive();
        }

        void ProcessHanshake(SocketAsyncEventArgs e)
        {
            var handshakeContext = e.UserToken as HandshakeContext;
            handshakeContext.HandshakeData.AddSegment(new ArraySegment<byte>(CloneRange(e.Buffer, e.Offset, e.BytesTransferred)));

            if (handshakeContext.ExpectedLength <= 0)
            {
                var result = handshakeContext.HandshakeData.SearchMark(m_NewLineMark);

                if (result.HasValue && result.Value >= 0)
                {
                    handshakeContext.ExpectedLength = result.Value + m_NewLineMark.Length + 16;
                }
            }

            if (handshakeContext.HandshakeData.Count < handshakeContext.ExpectedLength)
            {
                StartReceive();
                return;
            }

            byte[] challengeResponse = handshakeContext.HandshakeData.ToArrayData(handshakeContext.ExpectedLength - 16, 16);

            bool matched = false;

            if (challengeResponse.Length == handshakeContext.ExpectedChallenge.Length)
            {
                matched = true;

                for (int i = 0; i < challengeResponse.Length; i++)
                {
                    if (challengeResponse[i] != handshakeContext.ExpectedChallenge[i])
                    {
                        matched = false;
                        break;
                    }
                }
            }

            if (matched)
            {
                m_Connected = true;
                FireOnOpen();
                m_ReceiveAsyncEventArgs.UserToken = null;
                StartReceive();
            }
            else
            {
                EnsureCloseSocket();
            }
        }

        void ProcessReceiveData(byte[] buffer, int offset, int length)
        {
            //First data
            if (m_MessBuilder.Count <= 0)
            {
                if (buffer[offset] != m_StartByte && buffer[offset] != m_EndByte)
                    return;

                byte loofForByte = buffer[offset] == m_StartByte ? m_EndByte : m_StartByte;

                int endPos = buffer.IndexOf(loofForByte, offset + 1, length - 1);

                if (endPos < 0)
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, offset, length));
                }
                else
                {
                    if (loofForByte == m_StartByte)
                    {
                        EnsureCloseSocket(true);
                        return;
                    }

                    FireOnMessage(Encoding.UTF8.GetString(buffer, offset + 1, endPos - offset - 1));

                    if (endPos >= (offset + length - 1))
                        return;

                    ProcessReceiveData(buffer, endPos + 1, offset + length - endPos - 1);
                }
            }
            else
            {
                byte loofForByte = m_MessBuilder[0] == m_StartByte ? m_EndByte : m_StartByte;

                int endPos = buffer.IndexOf(loofForByte, offset, length);

                if (endPos < 0)
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, offset, length));
                    return;
                }

                //Closing handshake from server side
                if (loofForByte == m_StartByte)
                {
                    m_MessBuilder.Clear();
                    EnsureCloseSocket(true);
                    return;
                }

                m_MessBuilder.AddRange(CloneRange(buffer, offset, endPos - offset));
                FireOnMessage(Encoding.UTF8.GetString(m_MessBuilder.Skip(1).ToArray()));
                m_MessBuilder.Clear();

                if (endPos >= (offset + length - 1))
                    return;

                ProcessReceiveData(buffer, endPos + 1, offset + length - endPos - 1);
            }
        }

        private byte[] CloneRange(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];

            for (int i = 0; i < length; i++)
            {
                data[i] = buffer[offset + i];
            }

            return data;
        }

        private byte[] GetResponseSecurityKey(string secKey1, string secKey2, byte[] secKey3)
        {
            //Remove all symbols that are not numbers
            string k1 = Regex.Replace(secKey1, "[^0-9]", String.Empty);
            string k2 = Regex.Replace(secKey2, "[^0-9]", String.Empty);

            //Convert received string to 64 bit integer.
            Int64 intK1 = Int64.Parse(k1);
            Int64 intK2 = Int64.Parse(k2);

            //Dividing on number of spaces
            int k1Spaces = secKey1.Count(c => c == ' ');
            int k2Spaces = secKey2.Count(c => c == ' ');
            int k1FinalNum = (int)(intK1 / k1Spaces);
            int k2FinalNum = (int)(intK2 / k2Spaces);

            //Getting byte parts
            byte[] b1 = BitConverter.GetBytes(k1FinalNum).Reverse().ToArray();
            byte[] b2 = BitConverter.GetBytes(k2FinalNum).Reverse().ToArray();
            //byte[] b3 = Encoding.UTF8.GetBytes(secKey3);
            byte[] b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            List<byte> bChallenge = new List<byte>();
            bChallenge.AddRange(b1);
            bChallenge.AddRange(b2);
            bChallenge.AddRange(b3);

            //Hash and return
            byte[] hash = MD5.Create().ComputeHash(bChallenge.ToArray());
            return hash;
        }

        private Random m_Random = new Random();

        private byte[] GenerateSecKey()
        {
            int totalLen = m_Random.Next(10, 20);
            return GenerateSecKey(totalLen);
        }

        private byte[] GenerateSecKey(int totalLen)
        {
            int spaceLen = m_Random.Next(1, totalLen / 2 + 1);
            int charLen = m_Random.Next(3, totalLen - 1 - spaceLen);
            int digLen = totalLen - spaceLen - charLen;

            List<char> source = new List<char>(totalLen);

            for (int i = 0; i < spaceLen; i++)
                source.Add(' ');

            for (int i = 0; i < charLen; i++)
            {
                source.Add(m_CharLib[m_Random.Next(0, m_CharLib.Count - 1)]);
            }

            for (int i = 0; i < digLen; i++)
            {
                source.Add(m_DigLib[m_Random.Next(0, m_DigLib.Count - 1)]);
            }

            List<char> mixedChars = new List<char>();

            for (int i = 0; i < totalLen - 1; i++)
            {
                int pos = m_Random.Next(0, source.Count - 1);
                mixedChars.Add(source[pos]);
                source.RemoveAt(pos);
            }

            mixedChars.Add(source[0]);

            return mixedChars.Select(c => (byte)c).ToArray();
        }

        private bool m_InSending = false;

        public void Send(string message)
        {
            if (!m_Connected)
                throw new Exception("The websocket is not open, so you can not send message now!");

            m_MessagesBeingSent.Enqueue(message);

            if (!m_InSending)
                StartSendFromQueue();
        }

        private void StartSendFromQueue()
        {
            string message;

            if (!m_MessagesBeingSent.TryDequeue(out message))
            {
                m_InSending = false;
                return;
            }

            m_InSending = true;

            var sendContext = new SendMessageContext { Message = message.ToArray(), SentLength = 0, Encoder = Encoding.UTF8.GetEncoder() };

            m_SendAsyncEventArgs.UserToken = sendContext;

            m_SendBuffer[0] = m_StartByte;

            StartSend(sendContext);
        }

        void StartSend(SendMessageContext context)
        {
            int left = context.Message.Length - context.SentLength;
            int bufferOffset = context.SentLength == 0 ? 1 : 0;

            if (left == 0)
            {
                m_SendBuffer[bufferOffset] = m_EndByte;
                context.Completed = true;

                m_SendAsyncEventArgs.SetBuffer(m_SendBuffer, 0, bufferOffset + 1);

                if (!m_Socket.SendAsync(m_SendAsyncEventArgs))
                    ProcessSend(m_SendAsyncEventArgs);

                return;
            }

            int charsUsed, bytesUsed;
            bool completed;

            context.Encoder.Convert(context.Message, context.SentLength, left,
                m_SendBuffer, bufferOffset, m_SendBuffer.Length - bufferOffset,
                false, out charsUsed, out bytesUsed, out completed);

            context.SentLength += charsUsed;

            //Finished?
            if (context.Message.Length == context.SentLength)
            {
                //Has enought buffer to send end mark?
                if (m_SendBuffer.Length - bufferOffset - bytesUsed > 0)
                {
                    m_SendBuffer[bufferOffset + bytesUsed] = m_EndByte;
                    context.Completed = true;
                    bytesUsed++;
                }
            }

            bytesUsed += bufferOffset;

            m_SendAsyncEventArgs.SetBuffer(m_SendBuffer, 0, bytesUsed);

            if (!m_Socket.SendAsync(m_SendAsyncEventArgs))
                ProcessSend(m_SendAsyncEventArgs);
        }

        public void Close()
        {
            if (m_Socket != null && m_Socket.Connected)
            {
                var eventArgs = new SocketAsyncEventArgs();
                eventArgs.SetBuffer(m_ClosingHandshake, 0, m_ClosingHandshake.Length);
                m_Socket.SendAsync(eventArgs);
            }
        }
    }
}
