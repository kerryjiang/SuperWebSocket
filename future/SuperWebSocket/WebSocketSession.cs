using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket
{
    /// <summary>
    /// WebSocketSession basic interface
    /// </summary>
    public interface IWebSocketSession : IAppSession
    {
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        string Method { get; set; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        /// <value>
        /// The HTTP version.
        /// </value>
        string HttpVersion { get; set; }

        /// <summary>
        /// Gets the sec web socket version.
        /// </summary>
        string SecWebSocketVersion { get; }

        /// <summary>
        /// Gets the origin.
        /// </summary>
        string Origin { get; }

        /// <summary>
        /// Gets the URI scheme.
        /// </summary>
        string UriScheme { get; }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        void SendResponse(string message);

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void SendResponse(byte[] data, int offset, int length);

        /// <summary>
        /// Gets the app server.
        /// </summary>
        new IWebSocketServer AppServer { get; }

        /// <summary>
        /// Gets or sets the protocol processor.
        /// </summary>
        /// <value>
        /// The protocol processor.
        /// </value>
        IProtocolProcessor ProtocolProcessor { get; set; }

        /// <summary>
        /// Gets the available sub protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns></returns>
        string GetAvailableSubProtocol(string protocol);

        /// <summary>
        /// Enqueues the data which should be sent.
        /// </summary>
        /// <param name="data">The data.</param>
        void EnqueueSend(IList<ArraySegment<byte>> data);

        /// <summary>
        /// Enqueues the data which should be sent.
        /// </summary>
        /// <param name="data">The data.</param>
        void EnqueueSend(ArraySegment<byte> data);
    }

    /// <summary>
    /// WebSocket AppSession
    /// </summary>
    public class WebSocketSession : WebSocketSession<WebSocketSession>
    {
        /// <summary>
        /// Gets the app server.
        /// </summary>
        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }
    }

    /// <summary>
    /// WebSocket AppSession class
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public class WebSocketSession<TWebSocketSession> : AppSession<TWebSocketSession, IWebSocketFragment>, IWebSocketSession
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        /// <value>
        /// The HTTP version.
        /// </value>
        public string HttpVersion { get; set; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        public string Host { get { return this.Items.GetValue<string>(WebSocketConstant.Host, string.Empty); } }

        /// <summary>
        /// Gets the origin.
        /// </summary>
        public string Origin { get { return this.Items.GetValue<string>(WebSocketConstant.Origin, string.Empty); } }

        /// <summary>
        /// Gets the upgrade.
        /// </summary>
        public string Upgrade { get { return this.Items.GetValue<string>(WebSocketConstant.Upgrade, string.Empty); } }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        public string Connection { get { return this.Items.GetValue<string>(WebSocketConstant.Connection, string.Empty); } }

        /// <summary>
        /// Gets the sec web socket version.
        /// </summary>
        public string SecWebSocketVersion { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketVersion, string.Empty); } }

        /// <summary>
        /// Gets the sec web socket protocol.
        /// </summary>
        public string SecWebSocketProtocol { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketProtocol, string.Empty); } }

        private Queue<ArraySegment<byte>> m_SendingQueue = new Queue<ArraySegment<byte>>();

        private volatile bool m_InSending = false;

        internal List<WebSocketDataFrame> Frames { get; private set; }

        internal DateTime StartClosingHandshakeTime { get; private set; }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public string CurrentToken { get; internal set; }

        /// <summary>
        /// Gets the app server.
        /// </summary>
        public new WebSocketServer<TWebSocketSession> AppServer
        {
            get { return (WebSocketServer<TWebSocketSession>)base.AppServer; }
        }

        IWebSocketServer IWebSocketSession.AppServer
        {
            get { return (IWebSocketServer)base.AppServer; }
        }

        string IWebSocketSession.GetAvailableSubProtocol(string protocol)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                SubProtocol = AppServer.DefaultSubProtocol;
                return string.Empty;
            }

            var arrNames = protocol.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var name in arrNames)
            {
                var subProtocol = AppServer.GetSubProtocol(name);

                if(subProtocol != null)
                {
                    SubProtocol = subProtocol;
                    return name;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the URI scheme, ws or wss
        /// </summary>
        public string UriScheme
        {
            get { return AppServer.UriScheme; }
        }

        /// <summary>
        /// Gets the sub protocol.
        /// </summary>
        public ISubProtocol<TWebSocketSession> SubProtocol { get; private set; }

        private bool m_Handshaked = false;

        internal bool Handshaked
        {
            get { return m_Handshaked; }
            set
            {
                m_Handshaked = value;
                if (m_Handshaked)
                {
                    SetCookie();
                    OnHandShaked();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the session [in closing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in closing]; otherwise, <c>false</c>.
        /// </value>
        public bool InClosing { get; private set; }

        /// <summary>
        /// Called when [init].
        /// </summary>
        protected override void OnInit()
        {
            Frames = new List<WebSocketDataFrame>();
            base.OnInit();
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        private void SetCookie()
        {
            string cookieValue = this.Items.GetValue<string>(WebSocketConstant.Cookie, string.Empty);

            var cookies = new StringDictionary();

            if (!string.IsNullOrEmpty(cookieValue))
            {
                string[] pairs = cookieValue.Split(';');

                int pos;
                string key, value;

                foreach (var p in pairs)
                {
                    pos = p.IndexOf('=');
                    if (pos > 0)
                    {
                        key = p.Substring(0, pos).Trim();
                        pos += 1;
                        if (pos < p.Length)
                            value = p.Substring(pos).Trim();
                        else
                            value = string.Empty;

                        cookies[key] = Uri.UnescapeDataString(value);
                    }
                }
            }

            this.Cookies = cookies;
        }

        /// <summary>
        /// Called when [hand shaked].
        /// </summary>
        protected virtual void OnHandShaked()
        {

        }

        /// <summary>
        /// Gets the cookies.
        /// </summary>
        public StringDictionary Cookies { get; private set; }

        void IWebSocketSession.EnqueueSend(IList<ArraySegment<byte>> data)
        {
            lock (m_SendingQueue)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    m_SendingQueue.Enqueue(data[i]);
                }
            }

            DequeueSend();
        }

        void IWebSocketSession.EnqueueSend(ArraySegment<byte> data)
        {
            lock (m_SendingQueue)
            {
                m_SendingQueue.Enqueue(data);
            }

            DequeueSend();
        }

        private void DequeueSend()
        {
            if (m_InSending)
                return;

            m_InSending = true;

            while (true)
            {
                if (!Connected)
                    break;

                ArraySegment<byte> segment;

                lock (m_SendingQueue)
                {
                    if (m_SendingQueue.Count <= 0)
                        break;

                    segment = m_SendingQueue.Dequeue();
                }

                SocketSession.SendResponse(segment.Array, segment.Offset, segment.Count);
            }

            m_InSending = false;
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendResponse(string message)
        {
            ProtocolProcessor.SendMessage(this, message);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="paramValues">The param values.</param>
        public override void SendResponse(string message, params object[] paramValues)
        {
            ProtocolProcessor.SendMessage(this, string.Format(message, paramValues));
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public new void SendResponse(byte[] data, int offset, int length)
        {
            if (!ProtocolProcessor.CanSendBinaryData)
            {
                if(Logger.IsErrorEnabled)
                    Logger.Error("The websocket of this version cannot used for sending binary data!");
                return;
            }

            ProtocolProcessor.SendData(this, data, offset, length);
        }

        /// <summary>
        /// Sends the response async.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendResponseAsync(string message)
        {
            Async.Run((s) => SendResponse((string)s), message);
        }

        /// <summary>
        /// Sends the response async.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="paramValues">The param values.</param>
        public void SendResponseAsync(string message, params object[] paramValues)
        {
            SendResponseAsync(string.Format(message, paramValues));
        }

        /// <summary>
        /// Closes the with handshake.
        /// </summary>
        /// <param name="reasonText">The reason text.</param>
        public void CloseWithHandshake(string reasonText)
        {
            this.CloseWithHandshake(ProtocolProcessor.CloseStatusClode.NormalClosure, reasonText);
        }

        /// <summary>
        /// Closes the with handshake.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="reasonText">The reason text.</param>
        public void CloseWithHandshake(int statusCode, string reasonText)
        {
            if (!InClosing)
                InClosing = true;

            ProtocolProcessor.SendCloseHandshake(this, statusCode, reasonText);

            StartClosingHandshakeTime = DateTime.Now;
            AppServer.PushToCloseHandshakeQueue(this);
        }

        /// <summary>
        /// Sends the close handshake response.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public void SendCloseHandshakeResponse(int statusCode)
        {
            if (!InClosing)
                InClosing = true;

            ProtocolProcessor.SendCloseHandshake(this, statusCode, string.Empty);
        }

        /// <summary>
        /// Closes the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public override void Close(CloseReason reason)
        {
            if (reason == CloseReason.TimeOut && ProtocolProcessor != null)
            {
                CloseWithHandshake(ProtocolProcessor.CloseStatusClode.NormalClosure, "Session timeOut");
                return;
            }

            base.Close(reason);
        }

        /// <summary>
        /// Gets or sets the protocol processor.
        /// </summary>
        /// <value>
        /// The protocol processor.
        /// </value>
        public IProtocolProcessor ProtocolProcessor { get; set; }

        /// <summary>
        /// Handles the unknown command.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        internal protected virtual void HandleUnknownCommand(SubRequestInfo requestInfo)
        {

        }

        /// <summary>
        /// Handles the unknown request.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        public override void HandleUnknownRequest(IWebSocketFragment requestInfo)
        {
            base.Close();
        }
    }
}
