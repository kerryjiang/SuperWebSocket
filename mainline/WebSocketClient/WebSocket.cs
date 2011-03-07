using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SuperWebSocket.WebSocketClient
{
    public class WebSocket
    {
        private NameValueCollection m_Cookies;
        private EndPoint m_RemoteEndPoint;
        private string m_Path = string.Empty;
        private Socket m_Socket;

        public WebSocket(string uri)
            : this(uri, new NameValueCollection())
        {

        }

        public WebSocket(string uri, NameValueCollection cookies)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");

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

            int pos = uri.IndexOf('/', 6);
            if(pos <= 0)
                throw new ArgumentException("Invalid websocket address!");

            m_Path = uri.Substring(pos);

            string host = uri.Substring(6, pos - 6);

            string[] hostInfo = host.Split(':');

            if (hostInfo.Length != 2)
                throw new ArgumentException("Invalid websocket address!");

            int port;
            if (!int.TryParse(hostInfo[1], out port))
                throw new ArgumentException("Invalid websocket address!");

            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostInfo[0], out ipAddress))
                m_RemoteEndPoint = new DnsEndPoint(hostInfo[0], port);
            else
                m_RemoteEndPoint = new IPEndPoint(ipAddress, port);          

            m_Cookies = cookies;

            Connect();
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

        private void Connect()
        {
            m_Socket = new Socket(m_RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_Socket.Connect(m_RemoteEndPoint);

                var stream = new NetworkStream(m_Socket);

                var reader = new StreamReader(stream, Encoding.UTF8, false);
                var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10);

                writer.WriteLine("GET /websock HTTP/1.1");
                writer.WriteLine("Upgrade: WebSocket");
                writer.WriteLine("Connection: Upgrade");
                writer.WriteLine("Sec-WebSocket-Key2: 12998 5 Y3 1  .P00");
                writer.WriteLine("Host: example.com");
                writer.WriteLine("Sec-WebSocket-Key1: 4 @1  46546xW%0l 1 5");
                writer.WriteLine("Origin: http://example.com");
                writer.WriteLine("WebSocket-Protocol: sample");
                writer.WriteLine("");
                string secKey = "^n:ds[4U";
                writer.Write(secKey);
                writer.Flush();

                //secKey.ToList().ForEach(c => Console.WriteLine((int)c));

                for (var i = 0; i < 6; i++)
                    Console.WriteLine(reader.ReadLine());

                char[] buffer = new char[20];

                int read = reader.Read(buffer, 0, buffer.Length);

                //Assert.AreEqual("8jKS'y:G*Co,Wxa-", new string(buffer.Take(read).ToArray()));

                FireOnOpen();
            }
            catch (Exception)
            {

            }
        }

        public void Send(string message)
        {

        }        

        public void Close()
        {

        }
    }
}
