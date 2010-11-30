using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Command;
using System.Threading;

namespace SuperWebSocketWeb
{
    public class Global : System.Web.HttpApplication
    {
        private List<WebSocketSession> m_Sessions = new List<WebSocketSession>();
        private object m_SessionSyncRoot = new object();

        void Application_Start(object sender, EventArgs e)
        {
            StartSuperWebSocketByConfig();
        }

        void StartSuperWebSocketByConfig()
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;

            socketServer.CommandHandler += new CommandHandler<WebSocketSession, WebSocketCommandInfo>(socketServer_CommandHandler);
            socketServer.NewSessionConnected += new SessionEventHandler(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler(socketServer_SessionClosed);

            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        void StartSuperWebSocketByProgramming()
        {
            var socketServer = new WebSocketServer();
            socketServer.Setup(new ServerConfig
                {
                    Ip = "Any",
                    Port = 912,
                    Mode = SocketMode.Async
                }, SocketServerFactory.Instance);
            socketServer.CommandHandler += new CommandHandler<WebSocketSession, WebSocketCommandInfo>(socketServer_CommandHandler);
            socketServer.NewSessionConnected += new SessionEventHandler(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler(socketServer_SessionClosed);
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Add(session);

            SendToAll("System: " + session.Cookies["name"].Value + " connected");
        }

        void socketServer_SessionClosed(WebSocketSession session)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Remove(session);
        }

        void socketServer_CommandHandler(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            SendToAll(session.Cookies["name"].Value + ": " + commandInfo.CommandData);
        }

        void SendToAll(string message)
        {
            lock (m_SessionSyncRoot)
            {
                foreach (var s in m_Sessions)
                {
                    s.SendResponse(message);
                }
            }
        }

        void Application_End(object sender, EventArgs e)
        {
            SocketServerManager.Stop();
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
