using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Command
{
    public abstract class WebSocketCommandBase : ICommand<WebSocketCommandInfo, WebSocketContext>
    {
        public abstract void ExecuteCommand(IClientSession<WebSocketCommandInfo, WebSocketContext> session, WebSocketCommandInfo commandInfo);

        public abstract string Name { get; }
    }
}
