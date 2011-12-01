using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Command
{
    public abstract class WebSocketCommandBase : ICommand<WebSocket, WebSocketCommandInfo, WebSocketContext>
    {
        public abstract void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo);

        public abstract string Name { get; }
    }
}
