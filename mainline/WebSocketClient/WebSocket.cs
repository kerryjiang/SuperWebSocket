using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace SuperWebSocket.WebSocketClient
{
    public class WebSocket
    {
        private NameValueCollection m_Cookies;
        private EndPoint m_RemoteEndPoint;
        private string m_Path = string.Empty;
        private string m_Host = string.Empty;
        private string m_Protocol = string.Empty;
        private Socket m_Socket;

        private const byte m_StartByte = 0x00;
        private const byte m_EndByte = 0xFF;

        private SocketAsyncEventArgs m_SocketAsyncEventArgs;
        private byte[] m_Buffer = new byte[512];

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
            : this(uri, protocol, new NameValueCollection()) 
        {

        }

        public WebSocket(string uri, string protocol, NameValueCollection cookies)
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
            if(pos <= 0)
                throw new ArgumentException("Invalid websocket address!");

            m_Path = uri.Substring(pos);

            string host = uri.Substring(5, pos - 5);

            string[] hostInfo = host.Split(':');

            if (hostInfo.Length != 2)
                throw new ArgumentException("Invalid websocket address!");

            int port;
            if (!int.TryParse(hostInfo[1], out port))
                throw new ArgumentException("Invalid websocket address!");

            m_Host = hostInfo[0];

            IPAddress ipAddress;
            if (!IPAddress.TryParse(m_Host, out ipAddress))
                m_RemoteEndPoint = new DnsEndPoint(m_Host, port);
            else
                m_RemoteEndPoint = new IPEndPoint(ipAddress, port);          

            m_Cookies = cookies;
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

        public bool Connect()
        {
            m_Socket = new Socket(m_RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                string secKey1 = Encoding.UTF8.GetString(GenerateSecKey());
                Console.WriteLine(secKey1);
                string secKey2 = Encoding.UTF8.GetString(GenerateSecKey());
                Console.WriteLine(secKey2);
                byte[] secKey3 = GenerateSecKey(8);

                byte[] expectedResponse = GetResponseSecurityKey(secKey1, secKey2, secKey3);

                m_Socket.Connect(m_RemoteEndPoint);

                var stream = new NetworkStream(m_Socket);

                var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10);

                writer.WriteLine("GET {0} HTTP/1.1", m_Path);
                writer.WriteLine("Upgrade: WebSocket");
                writer.WriteLine("Connection: Upgrade");
                writer.WriteLine("Sec-WebSocket-Key2: {0}", secKey2);
                writer.WriteLine("Host: {0}", m_Host);
                writer.WriteLine("Sec-WebSocket-Key1: {0}", secKey1);
                writer.WriteLine("Origin: {0}", m_Host);
                writer.WriteLine("WebSocket-Protocol: {0}", m_Protocol);

                if (m_Cookies != null && m_Cookies.Count > 0)
                {
                    string[] cookiePairs = new string[m_Cookies.Count];
                    for (int i = 0; i < m_Cookies.Count; i++)
                    {
                        var key = m_Cookies.AllKeys[i];
                        var value = m_Cookies[key];
                        cookiePairs[i] = key + "=" + value;                  
                    }
                    writer.WriteLine("Cookie: {0}", string.Join("&", cookiePairs));
                }

                writer.WriteLine("");

                writer.Write(Encoding.UTF8.GetString(secKey3));
                writer.Flush();

                byte[] challengeResponse;

                string handshake = ParseServerHandshake(stream, out challengeResponse);

                bool matched = false;

                if (challengeResponse.Length == expectedResponse.Length)
                {
                    matched = true;

                    for (int i = 0; i < challengeResponse.Length; i++)
                    {
                        if (challengeResponse[i] != expectedResponse[i])
                        {
                            matched = false;
                            break;
                        }
                    }
                }

                if (matched)
                {                    
                    FireOnOpen();

                    m_SocketAsyncEventArgs = new SocketAsyncEventArgs();
                    m_SocketAsyncEventArgs.SetBuffer(m_Buffer, 0, m_Buffer.Length);
                    m_SocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_SocketAsyncEventArgs_Completed);

                    StartReceiveAsync(m_SocketAsyncEventArgs);
                    
                    return true;
                }

                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static readonly byte[] m_NewLineMark = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        private string ParseServerHandshake(Stream stream, out byte[] challengeResponse)
        {
            ArraySegmentList<byte> handshakeData = new ArraySegmentList<byte>();
            challengeResponse = new byte[16];
            int challengeOffset = 0;
            string handshake = string.Empty;
            int thisRead = 0;
            int lastTotalRead = 0;

            while (true)
            {
                lastTotalRead += thisRead;
                byte[] buffer = new byte[128];
                thisRead = stream.Read(buffer, 0, buffer.Length);

                if (thisRead <= 0)
                    break;

                if (string.IsNullOrEmpty(handshake))
                {
                    handshakeData.AddSegment(new ArraySegment<byte>(buffer, 0, thisRead));
                    var result = handshakeData.SearchMark(lastTotalRead, thisRead, m_NewLineMark);
                    if (result.HasValue && result.Value >= 0)
                    {
                        handshake = Encoding.UTF8.GetString(handshakeData.ToArrayData(0, result.Value));

                        if (result.Value + m_NewLineMark.Length < handshakeData.Count)
                        {
                            int leftLength = handshakeData.Count - result.Value - m_NewLineMark.Length;
                            int thisCopy = Math.Min(challengeResponse.Length, leftLength);
                            Array.Copy(buffer, result.Value - lastTotalRead + m_NewLineMark.Length, challengeResponse, 0, thisCopy);
                            challengeOffset = thisCopy;

                            if (thisCopy == challengeResponse.Length)
                                return handshake;
                        }
                    }

                    continue;
                }
                else
                {
                    int left = challengeResponse.Length - challengeOffset;
                    int thisCopy = Math.Min(left, thisRead);
                    Array.Copy(buffer, 0, challengeResponse, challengeOffset, thisCopy);
                    challengeOffset += thisCopy;

                    if (challengeOffset == challengeResponse.Length)
                        return handshake;

                    continue;
                }               
            }

            return handshake;
        }

        void m_SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
                ProcessReceive(e);
        }

        List<byte> m_MessBuilder = new List<byte>();

        void StartReceiveAsync(SocketAsyncEventArgs e)
        {
            var willRaiseEvent = m_Socket.ReceiveAsync(e);
            if (!willRaiseEvent)
                ProcessReceive(e);
        }

        void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                FireOnClose();
                return;
            }

            if (e.BytesTransferred == 0)
            {
                FireOnClose();
                return;
            }

            ProcessReceiveData(e.Buffer, e.Offset, e.BytesTransferred);
            StartReceiveAsync(e);
        }

        void ProcessReceiveData(byte[] buffer, int offset, int length)
        {
            //First data
            if (m_MessBuilder.Count <= 0)
            {
                int startPos = buffer.IndexOf(m_StartByte, offset, length);
                if (startPos < 0)
                    return;

                int endPos = buffer.IndexOf(m_EndByte, startPos, offset + length - startPos);
                if (endPos < 0)
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, startPos, offset + length - startPos));
                }
                else
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, startPos + 1, endPos - startPos - 2));
                    FireOnMessage(Encoding.UTF8.GetString(m_MessBuilder.Skip(1).ToArray()));
                    m_MessBuilder.Clear();

                    if (endPos >= (offset + length - 1))
                        return;

                    ProcessReceiveData(buffer, endPos + 1, offset + length - endPos - 1);
                }
            }
            else
            {
                int endPos = buffer.IndexOf(m_EndByte, offset, length);

                if (endPos < 0)
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, offset, length));
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

        public void Send(string message)
        {
            try
            {
                m_Socket.Send(new byte[] { m_StartByte });
                m_Socket.Send(Encoding.UTF8.GetBytes(message));
                m_Socket.Send(new byte[] { m_EndByte });
            }
            catch (Exception)
            {
                FireOnClose();
            }
        }

        public void Close()
        {
            try
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
