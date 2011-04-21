using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using System.Collections.Specialized;

namespace SuperWebSocket
{
    public class WebSocketContext : SocketContext
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string HttpVersion { get; set; }
        public string Host { get { return m_Values[WebSocketConstant.Host]; } }
        public string Origin { get { return m_Values[WebSocketConstant.Origin]; } }
        public string Upgrade { get { return m_Values[WebSocketConstant.Upgrade]; } }
        public string Connection { get { return m_Values[WebSocketConstant.Connection]; } }
        public string SecWebSocketKey1 { get { return m_Values[WebSocketConstant.SecWebSocketKey1]; } }
        public string SecWebSocketKey2 { get { return m_Values[WebSocketConstant.SecWebSocketKey2]; } }
        public string SecWebSocketVersion { get { return m_Values[WebSocketConstant.SecWebSocketVersion]; } }
        public byte[] SecWebSocketKey3 { get; set; }

        public WebSocketContext()
        {
            this.Charset = Encoding.UTF8;
        }

        private readonly StringDictionary m_Values = new StringDictionary();

        public string this[string key]
        {
            get
            {
                return m_Values[key];
            }
            set
            {
                m_Values[key] = value;
            }
        }
    }
}
