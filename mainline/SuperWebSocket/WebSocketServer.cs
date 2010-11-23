using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket
{
    public delegate void SessionEventHandler(WebSocketSession session);

    public class WebSocketServer : AppServer<WebSocketSession, WebSocketCommandInfo>
    {
        public WebSocketServer()
            : base()
        {
            Protocol = new WebSocketProtocol();
        }

        private SessionEventHandler m_NewSessionConnected;

        public event SessionEventHandler NewSessionConnected
        {
            add { m_NewSessionConnected += value; }
            remove { m_NewSessionConnected -= value; }
        }

        private SessionEventHandler m_SessionClosed;

        public event SessionEventHandler SessionClosed
        {
            add { m_SessionClosed += value; }
            remove { m_SessionClosed -= value; }
        }

        private void ProcessHeadRequest(WebSocketSession session)
        {

        }

        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            if (commandInfo.CommandKey.Equals(WebSocketConstant.CommandHead))
            {
                ProcessHeadRequest(session);

                if (m_NewSessionConnected != null)
                    m_NewSessionConnected(session);
            }
            else
            {
                base.ExecuteCommand(session, commandInfo);
            }
        }

        protected override void OnSocketSessionClosed(object sender, SocketSessionClosedEventArgs e)
        {
            var session = this.GetAppSessionByIndentityKey(e.IdentityKey);

            base.OnSocketSessionClosed(sender, e);

            if (m_SessionClosed == null)
                m_SessionClosed(session);
        }
    }
}
