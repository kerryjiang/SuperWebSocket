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
        private List<WebSocketSession> m_SecureSessions = new List<WebSocketSession>();
        private object m_SessionSyncRoot = new object();
        private object m_SecureSessionSyncRoot = new object();
        private Timer m_SecureSocketPushTimer;

        void Application_Start(object sender, EventArgs e)
        {
            StartSuperWebSocketByConfig();
            //StartSuperWebSocketByProgramming();
            var ts = new TimeSpan(0, 0, 5);
            m_SecureSocketPushTimer = new Timer(OnSecureSocketPushTimerCallback, new object(), ts, ts);
        }

        void OnSecureSocketPushTimerCallback(object state)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.ForEach(s => s.SendResponseAsync("Push data from SecureWebSocket. Current Time: " + DateTime.Now));
            }
        }

        void StartSuperWebSocketByConfig()
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;
            var secureSocketServer = SocketServerManager.GetServerByName("SecureSuperWebSocket") as WebSocketServer;

            Application["WebSocketPort"] = socketServer.Config.Port;
            Application["SecureWebSocketPort"] = secureSocketServer.Config.Port;

            socketServer.CommandHandler += new CommandHandler<WebSocketSession, WebSocketCommandInfo>(socketServer_CommandHandler);
            socketServer.NewSessionConnected += new SessionEventHandler(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionClosedEventHandler(socketServer_SessionClosed);

            secureSocketServer.NewSessionConnected += new SessionEventHandler(secureSocketServer_NewSessionConnected);
            secureSocketServer.SessionClosed += new SessionClosedEventHandler(secureSocketServer_SessionClosed);

            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        void secureSocketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.Remove(session);
            }
        }

        void secureSocketServer_NewSessionConnected(WebSocketSession session)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.Add(session);
            }
        }

        void StartSuperWebSocketByProgramming()
        {
            var socketServer = new WebSocketServer();
            socketServer.Setup(new ServerConfig
                {
                    Ip = "Any",
                    Port = 2001,
                    Mode = SocketMode.Async
                }, SocketServerFactory.Instance);
            socketServer.CommandHandler += new CommandHandler<WebSocketSession, WebSocketCommandInfo>(socketServer_CommandHandler);
            socketServer.NewSessionConnected += new SessionEventHandler(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionClosedEventHandler(socketServer_SessionClosed);

            var secureSocketServer = new WebSocketServer();
            secureSocketServer.Setup(new ServerConfig
            {
                Ip = "Any",
                Port = 2001,
                Mode = SocketMode.Sync,
                Security = "tls",
                Certificate = new SuperSocket.SocketBase.Config.CertificateConfig
                {
                    FilePath = Server.MapPath("~/localhost.pfx"),
                    Password = "supersocket",
                    IsEnabled = true
                }
            }, SocketServerFactory.Instance);

            secureSocketServer.NewSessionConnected += new SessionEventHandler(secureSocketServer_NewSessionConnected);
            secureSocketServer.SessionClosed += new SessionClosedEventHandler(secureSocketServer_SessionClosed);

            Application["WebSocketPort"] = socketServer.Config.Port;
            Application["SecureWebSocketPort"] = secureSocketServer.Config.Port;
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Add(session);

            SendToAll("System: " + session.Cookies["name"] + " connected");
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Remove(session);

            if (reason == CloseReason.ServerShutdown)
                return;

            SendToAll("System: " + session.Cookies["name"] + " disconnected");
        }

        void socketServer_CommandHandler(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            SendToAll(session.Cookies["name"] + ": " + commandInfo.Data);
        }

        void SendToAll(string message)
        {
            lock (m_SessionSyncRoot)
            {
                foreach (var s in m_Sessions)
                {
                    //s.SendResponse(message);
                    s.SendResponseAsync(message);
                }
            }
        }

        void Application_End(object sender, EventArgs e)
        {
            m_SecureSocketPushTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_SecureSocketPushTimer.Dispose();
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
