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
                byte spaceByte = (byte)' ';
                string secKey1 = "12998 5 Y3 1  .P00";//Encoding.UTF8.GetString(GenerateRandomSpaceAndNumber(GenerateRandomData(new byte[m_Random.Next(10, 20)]), spaceByte));
                string secKey2 = "4 @1  46546xW%0l 1 5";//Encoding.UTF8.GetString(GenerateRandomSpaceAndNumber(GenerateRandomData(new byte[m_Random.Next(10, 20)]), spaceByte));
                byte[] secKey3 = Encoding.UTF8.GetBytes("^n:ds[4U");//GenerateRandomData(new byte[8]);                

                m_Socket.Connect(m_RemoteEndPoint);

                var stream = new NetworkStream(m_Socket);

                var reader = new StreamReader(stream, Encoding.UTF8, false);
                var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10);

                writer.WriteLine("GET {0} HTTP/1.1", m_Path);
                writer.WriteLine("Upgrade: WebSocket");
                writer.WriteLine("Connection: Upgrade");
                writer.WriteLine("Sec-WebSocket-Key2: {0}", secKey2);
                writer.WriteLine("Host: {0}", m_Host);
                writer.WriteLine("Sec-WebSocket-Key1: {0}", secKey1);
                writer.WriteLine("Origin: {0}", m_Host);
                writer.WriteLine("WebSocket-Protocol: {0}", m_Protocol);
                writer.WriteLine("");

                writer.Write(Encoding.UTF8.GetString(secKey3));
                writer.Flush();

                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;
                }

                char[] buffer = new char[16];

                int totalRead = 0;

                while (totalRead < 16)
                {
                    int read = reader.Read(buffer, totalRead, buffer.Length - totalRead);

                    if (read <= 0)
                        return false;

                    totalRead += read;
                }

                string expectedResponse = Encoding.UTF8.GetString(GetResponseSecurityKey(secKey1, secKey2, secKey3));

                if (string.Compare(expectedResponse, new string(buffer)) == 0)
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
                int startPos = IndexOf(buffer, m_StartByte, offset, length);
                if (startPos < 0)
                    return;

                int endPos = IndexOf(buffer, m_EndByte, startPos, offset + length - startPos);
                if (endPos < 0)
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, startPos, offset + length - startPos));
                }
                else
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, startPos + 1, endPos - startPos - 1));
                    FireOnMessage(Encoding.UTF8.GetString(m_MessBuilder.ToArray()));
                    m_MessBuilder.Clear();

                    if (endPos >= (offset + length - 1))
                        return;

                    ProcessReceiveData(buffer, endPos + 1, offset + length - endPos - 1);
                }
            }
            else
            {
                int endPos = IndexOf(buffer, m_EndByte, offset, length);

                if (endPos < 0)
                {
                    m_MessBuilder.AddRange(CloneRange(buffer, offset, length));
                    return;
                }

                m_MessBuilder.AddRange(CloneRange(buffer, offset, endPos - offset + 1));
                FireOnMessage(Encoding.UTF8.GetString(m_MessBuilder.ToArray()));
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

        private int IndexOf(byte[] buffer, byte target, int offset, int length)
        {
            for (int i = offset; i < offset + length; i++)
            {
                if (buffer[i] == target)
                    return i;
            }

            return -1;
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

        private byte[] GenerateRandomData(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)m_Random.Next(0, 255);
            }

            return data;
        }

        private byte[] GenerateRandomSpaceAndNumber(byte[] data, byte spaceByte)
        {
            for (int i = 0; i < m_Random.Next(2, data.Length - 1); i++)
            {
                data[m_Random.Next(0, data.Length - 1)] = (byte)m_Random.Next(0, 9).ToString()[0];
            }

            for (int i = 0; i < m_Random.Next(1, data.Length / 2 + 1); i++)
            {
                data[m_Random.Next(0, data.Length - 1)] = spaceByte;
            }            

            return data;
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
