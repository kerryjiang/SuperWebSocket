using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.SubProtocol
{
    public abstract class AsyncJsonSubCommand<TJsonCommandInfo> : AsyncJsonSubCommand<WebSocketSession, TJsonCommandInfo>
    {

    }

    public abstract class AsyncJsonSubCommand<TWebSocketSession, TJsonCommandInfo> : JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private Action<TWebSocketSession, string, TJsonCommandInfo> m_AsyncJsonCommandAction;

        public AsyncJsonSubCommand()
        {
            m_AsyncJsonCommandAction = ExecuteAsyncJsonCommand;
        }

        protected override void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo)
        {
            m_AsyncJsonCommandAction.BeginInvoke(session, session.CurrentToken, commandInfo, null, session);
        }

        protected abstract void ExecuteAsyncJsonCommand(TWebSocketSession session, string token, TJsonCommandInfo commandInfo);

        private void OnAsyncJsonCommandExecuted(IAsyncResult result)
        {
            var session = (TWebSocketSession)result.AsyncState;

            try
            {
                m_AsyncJsonCommandAction.EndInvoke(result);
            }
            catch (Exception e)
            {
                session.Logger.LogError(e);
            }
        }

        protected void SendJsonResponse(TWebSocketSession session, string token, object content)
        {
            session.SendResponse(GetJsonResponse(this.Name, token, content));
        }

        protected void SendJsonResponse(TWebSocketSession session, string name, string token, object content)
        {
            session.SendResponse(GetJsonResponse(name, token, content));
        }
    }
}
