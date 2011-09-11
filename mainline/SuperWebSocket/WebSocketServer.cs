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
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Protocol;
using SuperWebSocket.SubProtocol;

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
        public WebSocketServer(IEnumerable<ISubProtocol<WebSocketSession>> subProtocols)
            : base(subProtocols)
        {

        }

        public WebSocketServer(ISubProtocol<WebSocketSession> subProtocol)
            : base(subProtocol)
        {

        }

        public WebSocketServer()
            : base(new List<ISubProtocol<WebSocketSession>> { new BasicSubProtocol() })
        {

        }
    }

    public abstract class WebSocketServer<TWebSocketSession> : AppServer<TWebSocketSession, WebSocketCommandInfo>, IWebSocketServer
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public WebSocketServer(IEnumerable<ISubProtocol<TWebSocketSession>> subProtocols)
            : this()
        {
            m_SubProtocols = subProtocols.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase); ;
        }

        public WebSocketServer(ISubProtocol<TWebSocketSession> subProtocol)
            : this(new List<ISubProtocol<TWebSocketSession>> { subProtocol })
        {
            
        }

        public WebSocketServer()
            : base(new WebSocketProtocol())
        {

        }

        private Dictionary<string, ISubProtocol<TWebSocketSession>> m_SubProtocols;

        /// <summary>
        /// Gets the sub protocol by sub protocol name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal ISubProtocol<TWebSocketSession> GetSubProtocol(string name)
        {
            ISubProtocol<TWebSocketSession> subProtocol;

            if (m_SubProtocols.TryGetValue(name, out subProtocol))
                return subProtocol;
            else
                return null;
        }

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

        private bool SetupSubProtocols(IServerConfig config)
        {
            string subProtocolValue = config.Options.GetValue("subProtocol");

            if (string.IsNullOrEmpty(subProtocolValue))
                return true;

            var subProtocolTypes = subProtocolValue.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            if (subProtocolTypes != null && subProtocolTypes.Length > 0)
            {
                m_SubProtocols = new Dictionary<string, ISubProtocol<TWebSocketSession>>(subProtocolTypes.Length, StringComparer.OrdinalIgnoreCase);
            }

            foreach (var t in subProtocolTypes)
            {
                ISubProtocol<TWebSocketSession> subProtocol;

                if (!AssemblyUtil.TryCreateInstance<ISubProtocol<TWebSocketSession>>(t, out subProtocol))
                    return false;

                if (m_SubProtocols.ContainsKey(subProtocol.Name))
                {
                    Logger.LogError(string.Format("This sub protocol '{0}' has been defined! You cannot define duplicated sub protocols!", subProtocol.Name));
                    return false;
                }

                if (!subProtocol.Initialize(config))
                {
                    Logger.LogError(string.Format("Failed to initialize the sub protocol '{0}'!", subProtocol.Name));
                    return false;
                }

                m_SubProtocols.Add(subProtocol.Name, subProtocol);
            }

            return true;
        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<WebSocketCommandInfo> protocol)
        {
            if (!SetupSubProtocols(config))
                return false;

            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            if (string.IsNullOrEmpty(config.Security) || "none".Equals(config.Security, StringComparison.OrdinalIgnoreCase))
                m_UriScheme = "ws";
            else
                m_UriScheme = "wss";

            m_WebSocketProtocolProcessor = new DraftHybi00Processor
            {
                NextProcessor = new DraftHybi10Processor()
            };

            return true;
        }

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

            m_NewMessageReceived(session, commandInfo.Text);
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
                if (m_NewMessageReceived == null)
                {
                    ExecuteSubCommand(session, commandInfo, session.SubProtocol.SubCommandParser.ParseSubCommand(commandInfo));
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

            if (session.SubProtocol.TryGetCommand(subCommandInfo.Key, out subCommand))
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
            if (m_SubProtocols == null || m_SubProtocols.Count <= 0)
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
