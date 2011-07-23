using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol;

namespace SuperWebSocket
{
    public interface IWebSocketServer : IAppServer
    {
        IProtocolProcessor WebSocketProtocolProcessor { get; }
    }

    public delegate void SessionEventHandler<TWebSocketSession>(TWebSocketSession session)
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new();

    public delegate void SessionEventHandler<TWebSocketSession, TEventArgs>(TWebSocketSession session, TEventArgs e)
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new();

    public class WebSocketServer : WebSocketServer<WebSocketSession>
    {
        public WebSocketServer(ISubProtocol<WebSocketSession> subProtocol)
            : base(subProtocol)
        {

        }

        public WebSocketServer()
            : base(new BasicSubProtocol())
        {

        }
    }

    public abstract class WebSocketServer<TWebSocketSession> : AppServer<TWebSocketSession, WebSocketCommandInfo>, IWebSocketServer
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public WebSocketServer(ISubProtocol<TWebSocketSession> subProtocol)
            : this()
        {
            m_SubProtocol = subProtocol;
        }

        public WebSocketServer()
            : base(new WebSocketProtocol())
        {

        }

        private ISubProtocol<TWebSocketSession> m_SubProtocol;

        private string m_UriScheme;

        internal string UriScheme
        {
            get { return m_UriScheme; }
        }

        private IProtocolProcessor m_WebSocketProtocolProcessor;

        IProtocolProcessor IWebSocketServer.WebSocketProtocolProcessor
        {
            get { return m_WebSocketProtocolProcessor; }
        }

        public new WebSocketProtocol Protocol
        {
            get
            {
                return (WebSocketProtocol)base.Protocol;
            }
        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<WebSocketCommandInfo> protocol)
        {
            string subProtocolValue = config.Options.GetValue("subProtocol");
            if (!string.IsNullOrEmpty(subProtocolValue))
            {
                ISubProtocol<TWebSocketSession> subProtocol;
                if (AssemblyUtil.TryCreateInstance<ISubProtocol<TWebSocketSession>>(subProtocolValue, out subProtocol))
                    m_SubProtocol = subProtocol;
            }

            if (m_SubProtocol != null)
                m_SubProtocol.Initialize(config);

            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            if (string.IsNullOrEmpty(config.Security) || "none".Equals(config.Security, StringComparison.OrdinalIgnoreCase))
                m_UriScheme = "ws";
            else
                m_UriScheme = "wss";

            m_WebSocketProtocolProcessor = new DraftHybi00Processor
            {
                NextProcessor = new DraftHixie75Processor()
            };

            return true;
        }

        private Dictionary<string, ISubCommand<TWebSocketSession>> m_SubProtocolCommandDict = new Dictionary<string, ISubCommand<TWebSocketSession>>(StringComparer.OrdinalIgnoreCase);

        private bool m_SubCommandsLoaded = false;

        private SessionEventHandler<TWebSocketSession> m_NewSessionConnected;

        public event SessionEventHandler<TWebSocketSession> NewSessionConnected
        {
            add { m_NewSessionConnected += value; }
            remove { m_NewSessionConnected -= value; }
        }

        private SessionEventHandler<TWebSocketSession, CloseReason> m_SessionClosed;

        public event SessionEventHandler<TWebSocketSession, CloseReason> SessionClosed
        {
            add { m_SessionClosed += value; }
            remove { m_SessionClosed -= value; }
        }

        private SessionEventHandler<TWebSocketSession, string> m_NewMessageReceived;

        public event SessionEventHandler<TWebSocketSession, string> NewMessageReceived
        {
            add
            {
                if (m_SubCommandsLoaded)
                    throw new Exception("You cannot set the NewMessageReceived handler if you have defined the commands of your sub protocol!");

                m_NewMessageReceived += value;
                this.CommandHandler += new CommandHandler<TWebSocketSession, WebSocketCommandInfo>(WebSocketServer_CommandHandler);
            }
            remove
            {
                m_NewMessageReceived -= value;
                this.CommandHandler -= new CommandHandler<TWebSocketSession, WebSocketCommandInfo>(WebSocketServer_CommandHandler);
            }
        }

        void WebSocketServer_CommandHandler(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            if (m_NewMessageReceived == null)
                return;

            m_NewMessageReceived(session, commandInfo.Data);
        }

        internal static void ParseHandshake(IWebSocketSession session, TextReader reader)
        {
            string line;
            string firstLine = string.Empty;
            string prevKey = string.Empty;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = line;
                    continue;
                }

                if (line.StartsWith("\t") && !string.IsNullOrEmpty(prevKey))
                {
                    string currentValue = session.Items.GetValue<string>(prevKey, string.Empty);
                    session.Items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(':');

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = line.Substring(pos + 1);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(" ") && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                session.Items[key] = value;
                prevKey = key;
            }

            var metaInfo = firstLine.Split(' ');

            session.Method = metaInfo[0];
            session.Path = metaInfo[1];
            session.HttpVersion = metaInfo[2];
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            if (!session.Handshaked)
            {
                session.Handshaked = true;

                if (m_NewSessionConnected != null)
                    m_NewSessionConnected(session);
            }
            else
            {
                if (m_SubCommandsLoaded)
                {
                    ExecuteSubCommand(session, commandInfo, m_SubProtocol.SubCommandParser.ParseSubCommand(commandInfo));
                }
                else
                {
                    base.ExecuteCommand(session, commandInfo);
                }
            }
        }

        private void ExecuteSubCommand(TWebSocketSession session, WebSocketCommandInfo rawCommandInfo, StringCommandInfo subCommandInfo)
        {
            ISubCommand<TWebSocketSession> subCommand;

            if (m_SubProtocolCommandDict.TryGetValue(subCommandInfo.Key, out subCommand))
            {
                session.CurrentCommand = subCommandInfo.Key;
                subCommand.ExecuteCommand(session, subCommandInfo);
                session.PrevCommand = subCommandInfo.Key;

                if (Config.LogCommand)
                    Logger.LogError(session, string.Format("Command - {0} - {1}", session.IdentityKey, subCommandInfo.Key));
            }
            else
            {
                session.HandleUnknownCommand(rawCommandInfo);
            }

            session.LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// Setups the commands.
        /// </summary>
        /// <param name="commandDict">The command dict.</param>
        /// <returns></returns>
        protected override bool SetupCommands(Dictionary<string, ICommand<TWebSocketSession, WebSocketCommandInfo>> commandDict)
        {
            if (m_SubProtocol != null)
            {
                foreach (var command in m_SubProtocol.GetSubCommands())
                {
                    if (m_SubProtocolCommandDict.ContainsKey(command.Name))
                    {
                        Logger.LogError(string.Format("You have defined duplicated command {0} in your command assembly!", command.Name));
                        return false;
                    }

                    m_SubProtocolCommandDict.Add(command.Name, command);
                }

                //If doesn't load any commands, also don't set m_SubCommandsLoaded to true
                if (m_SubProtocolCommandDict.Count > 0)
                    m_SubCommandsLoaded = true;
            }
            else
            {
                base.SetupCommands(commandDict);
            }

            return true;
        }

        protected override void OnAppSessionClosed(object sender, AppSessionClosedEventArgs<TWebSocketSession> e)
        {
            if (m_SessionClosed != null)
                m_SessionClosed(e.Session, e.Reason);
        }
    }
}
