using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;

namespace SuperWebSocket.Samples.LiveWebChat
{
    public class Global : System.Web.HttpApplication
    {
        private IBootstrap m_Bootstrap;
        private WebSocketServer m_WebSocketServer;

        void Application_Start(object sender, EventArgs e)
        {
            StartSuperWebSocketByConfig();
        }

        void StartSuperWebSocketByConfig()
        {
            m_Bootstrap = BootstrapFactory.CreateBootstrap();

            if (!m_Bootstrap.Initialize())
                return;

            var socketServer = m_Bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals("SuperWebSocket")) as WebSocketServer;

            socketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            socketServer.NewSessionConnected += socketServer_NewSessionConnected;
            socketServer.SessionClosed += socketServer_SessionClosed;

            m_WebSocketServer = socketServer;

            m_Bootstrap.Start();
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            SendToAll(session.Cookies["name"] + ": " + e);
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            SendToAll("System: " + session.Cookies["name"] + " connected");
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            if (reason == CloseReason.ServerShutdown)
                return;

            SendToAll("System: " + session.Cookies["name"] + " disconnected");
        }

        void SendToAll(string message)
        {
            foreach (var s in m_WebSocketServer.GetAllSessions())
            {
                s.Send(message);
            }
        }

        void Application_End(object sender, EventArgs e)
        {
            if (m_Bootstrap != null)
                m_Bootstrap.Stop();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
