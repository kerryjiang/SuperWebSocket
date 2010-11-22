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
