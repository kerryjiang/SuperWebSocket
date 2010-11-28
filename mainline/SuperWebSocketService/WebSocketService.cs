using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;

namespace SuperWebSocket.Service
{
    partial class WebSocketService : ServiceBase
    {
        public WebSocketService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        protected override void OnStop()
        {
            SocketServerManager.Stop();
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            SocketServerManager.Stop();
            base.OnShutdown();
        }
    }
}
