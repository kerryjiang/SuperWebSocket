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

namespace SuperWebSocketWeb
{
    public class Global : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            StartSuperWebSocketByConfig();
        }

        void StartSuperWebSocketByConfig()
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            if (!SocketServerManager.Start(serverConfig))
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
            
        }

        void socketServer_SessionClosed(WebSocketSession session)
        {
            
        }

        void socketServer_CommandHandler(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            
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
