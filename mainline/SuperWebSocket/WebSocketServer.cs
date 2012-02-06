using System;
using System.Collections.Concurrent;
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
using SuperWebSocket.Command;
using SuperWebSocket.Config;
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
            : base(new List<ISubProtocol<WebSocketSession>>())
        {

        }
    }

    public abstract class WebSocketServer<TWebSocketSession> : AppServer<TWebSocketSession, WebSocketCommandInfo>, IWebSocketServer
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public WebSocketServer(IEnumerable<ISubProtocol<TWebSocketSession>> subProtocols)
            : this()
        {
            if (!subProtocols.Any())
                return;

            foreach (var protocol in subProtocols)
            {
                if (!RegisterSubProtocol(protocol))
                    throw new Exception("Failed to register sub protocol!");
            }

            m_SubProtocolConfigured = true;
        }

        public WebSocketServer(ISubProtocol<TWebSocketSession> subProtocol)
            : this(new List<ISubProtocol<TWebSocketSession>> { subProtocol })
        {
            
        }

        public WebSocketServer()
            : base(new WebSocketProtocol())
        {

        }

        private Dictionary<string, ISubProtocol<TWebSocketSession>> m_SubProtocols = new Dictionary<string, ISubProtocol<TWebSocketSession>>(StringComparer.OrdinalIgnoreCase);

        internal ISubProtocol<TWebSocketSession> DefaultSubProtocol { get; private set; }

        private bool m_SubProtocolConfigured = false;

        private ConcurrentQueue<TWebSocketSession> m_HandshakePendingQueue = new ConcurrentQueue<TWebSocketSession>();

        /// <summary>
        /// The handshake timeout, in seconds
        /// </summary>
        private int m_HandshakeTimeOut;

        /// <summary>
        /// The interval of checking handshake pending queue, in seconds
        /// </summary>
        private int m_HandshakePendingQueueCheckingInterval;


        private Timer m_HandshakePendingQueueCheckingTimer;

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

        bool RegisterSubProtocol(ISubProtocol<TWebSocketSession> subProtocol)
        {
            if (m_SubProtocols.ContainsKey(subProtocol.Name))
            {
                Logger.LogError(string.Format("Cannot register duplicate name sub protocol! Duplicate name: {0}.", subProtocol.Name));
                return false;
            }

            m_SubProtocols.Add(subProtocol.Name, subProtocol);
            return true;
        }

        private bool SetupSubProtocols(IServerConfig config)
        {
            //Preparing sub protocols' configuration
            var subProtocolConfigSection = config.GetChildConfig<SubProtocolConfigCollection>("subProtocols");

            var subProtocolConfigDict = new Dictionary<string, SubProtocolConfig>(subProtocolConfigSection == null ? 0 : subProtocolConfigSection.Count, StringComparer.OrdinalIgnoreCase);

            if (subProtocolConfigSection != null)
            {
                foreach (var protocolConfig in subProtocolConfigSection)
                {
                    string originalProtocolName = protocolConfig.Name;
                    string protocolName;
                    
                    ISubProtocol<TWebSocketSession> subProtocolInstance;

                    if (!string.IsNullOrEmpty(originalProtocolName))
                    {
                        protocolName = originalProtocolName;

                        if (!string.IsNullOrEmpty(protocolConfig.Type))
                        {
                            Exception exception;
                            if (!AssemblyUtil.TryCreateInstance<ISubProtocol<TWebSocketSession>>(protocolConfig.Type, new object[] { originalProtocolName }, out subProtocolInstance, out exception))
                                return false;

                            if (!RegisterSubProtocol(subProtocolInstance))
                                return false;
                        }
                        else
                        {
                            if (!m_SubProtocols.ContainsKey(protocolName))
                            {
                                subProtocolInstance = new BasicSubProtocol<TWebSocketSession>(protocolName);

                                if (!RegisterSubProtocol(subProtocolInstance))
                                    return false;
                            }
                        }
                    }
                    else
                    {
                        protocolName = BasicSubProtocol<TWebSocketSession>.DefaultName;

                        if (!string.IsNullOrEmpty(protocolConfig.Type))
                        {
                            Logger.LogError("You needn't set Type attribute for SubProtocol, if you don't set Name attribute!");
                            return false;
                        }
                    }

                    subProtocolConfigDict[protocolName] = protocolConfig;
                }

                if(subProtocolConfigDict.Values.Any())
                    m_SubProtocolConfigured = true;
            }

            if (m_SubProtocols.Count <= 0 || (subProtocolConfigDict.ContainsKey(BasicSubProtocol<TWebSocketSession>.DefaultName) && !m_SubProtocols.ContainsKey(BasicSubProtocol<TWebSocketSession>.DefaultName)))
            {
                if (!RegisterSubProtocol(BasicSubProtocol<TWebSocketSession>.DefaultInstance))
                    return false;
            }

            //Initialize sub protocols
            foreach (var subProtocol in m_SubProtocols.Values)
            {
                SubProtocolConfig protocolConfig = null;

                subProtocolConfigDict.TryGetValue(subProtocol.Name, out protocolConfig);

                if (!subProtocol.Initialize(config, protocolConfig))
                {
                    Logger.LogError(string.Format("Failed to initialize the sub protocol '{0}'!", subProtocol.Name));
                    return false;
                }
            }
            
            return true;
        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<WebSocketCommandInfo> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            if (m_SubProtocols != null && m_SubProtocols.Count > 0)
                DefaultSubProtocol = m_SubProtocols.Values.FirstOrDefault();

            if (string.IsNullOrEmpty(config.Security) || "none".Equals(config.Security, StringComparison.OrdinalIgnoreCase))
                m_UriScheme = "ws";
            else
                m_UriScheme = "wss";

            m_WebSocketProtocolProcessor = new DraftHybi10Processor
            {
                NextProcessor = new Rfc6455Processor
                {
                    NextProcessor = new DraftHybi00Processor()
                }
            };

            SetupMultipleProtocolSwitch(m_WebSocketProtocolProcessor);

            if (!int.TryParse(config.Options.GetValue("handshakePendingQueueCheckingInterval"), out m_HandshakePendingQueueCheckingInterval))
                m_HandshakePendingQueueCheckingInterval = 60;// 1 minute default


            if (!int.TryParse(config.Options.GetValue("handshakeTimeOut"), out m_HandshakeTimeOut))
                m_HandshakeTimeOut = 120;// 2 minute default

            return true;
        }

        private void SetupMultipleProtocolSwitch(IProtocolProcessor rootProcessor)
        {
            var thisProcessor = rootProcessor;

            List<int> availableVersions = new List<int>();

            while (true)
            {
                availableVersions.Add(thisProcessor.Version);

                if (thisProcessor.NextProcessor == null)
                    break;

                thisProcessor = thisProcessor.NextProcessor;
            }

            thisProcessor.NextProcessor = new MultipleProtocolSwitchProcessor(availableVersions.ToArray());
        }

        protected override void OnStartup()
        {
            m_HandshakePendingQueueCheckingTimer = new Timer(HandshakePendingQueueCheckingCallback, null, m_HandshakePendingQueueCheckingInterval * 1000, m_HandshakePendingQueueCheckingInterval * 1000);
            base.OnStartup();
        }

        private void HandshakePendingQueueCheckingCallback(object state)
        {
            try
            {
                m_HandshakePendingQueueCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                while (true)
                {
                    TWebSocketSession session;

                    if (!m_HandshakePendingQueue.TryPeek(out session))
                        break;

                    if (session.Handshaked || session.Status != SessionStatus.Healthy)
                    {
                        //Handshaked or not connected
                        m_HandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.StartTime.AddSeconds(m_HandshakeTimeOut))
                        break;

                    //Timeout, dequeue and then close
                    m_HandshakePendingQueue.TryDequeue(out session);
                    session.Close(CloseReason.TimeOut);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            finally
            {
                m_HandshakePendingQueueCheckingTimer.Change(m_HandshakePendingQueueCheckingInterval * 1000, m_HandshakePendingQueueCheckingInterval * 1000);
            }
        }

        public override TWebSocketSession CreateAppSession(ISocketSession socketSession)
        {
            var session = base.CreateAppSession(socketSession);

            if (session != NullAppSession)
                m_HandshakePendingQueue.Enqueue(session);

            return session;
        }

        private SessionEventHandler<TWebSocketSession> m_NewSessionConnected;

        public event SessionEventHandler<TWebSocketSession> NewSessionConnected
        {
            add { m_NewSessionConnected += value; }
            remove { m_NewSessionConnected -= value; }
        }

        internal void OnNewSessionConnected(TWebSocketSession session)
        {
            if (m_NewSessionConnected != null)
                m_NewSessionConnected(session);
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
                if (m_SubProtocolConfigured)
                    throw new Exception("If you have defined any sub protocol, you cannot subscribe NewMessageReceived event!");

                m_NewMessageReceived += value;
            }
            remove
            {
                m_NewMessageReceived -= value;
            }
        }

        internal void OnNewMessageReceived(TWebSocketSession session, string message)
        {
            if (m_NewMessageReceived == null)
            {
                if (session.SubProtocol == null)
                {
                    Logger.LogError("No SubProtocol selected! This session cannot process any message!");
                    session.CloseWithHandshake(session.ProtocolProcessor.CloseStatusClode.ProtocolError, "No SubProtocol selected");
                    return;
                }

                ExecuteSubCommand(session, session.SubProtocol.SubCommandParser.ParseCommand(message));
            }
            else
            {
                m_NewMessageReceived(session, message);
            }
        }

        private SessionEventHandler<TWebSocketSession, byte[]> m_NewDataReceived;

        public event SessionEventHandler<TWebSocketSession, byte[]> NewDataReceived
        {
            add
            {
                m_NewDataReceived += value;
            }
            remove
            {
                m_NewDataReceived -= value;
            }
        }

        internal void OnNewDataReceived(TWebSocketSession session, byte[] data)
        {
            if (m_NewDataReceived == null)
                return;

            m_NewDataReceived(session, data);
        }

        private const string m_Tab = "\t";
        private const char m_Colon = ':';
        private const string m_Space = " ";
        private const char m_SpaceChar = ' ';
        private const string m_ValueSeparator = ", ";

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

                if (line.StartsWith(m_Tab) && !string.IsNullOrEmpty(prevKey))
                {
                    string currentValue = session.Items.GetValue<string>(prevKey, string.Empty);
                    session.Items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(m_Colon);

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = line.Substring(pos + 1);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(m_Space) && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                object oldValue;

                if (!session.Items.TryGetValue(key, out oldValue))
                {
                    session.Items.Add(key, value);
                }
                else
                {
                    session.Items[key] = oldValue + m_ValueSeparator + value;
                }

                prevKey = key;
            }

            var metaInfo = firstLine.Split(m_SpaceChar);

            session.Method = metaInfo[0];
            session.Path = metaInfo[1];
            session.HttpVersion = metaInfo[2];
        }

        protected override bool SetupCommands(Dictionary<string, ICommand<TWebSocketSession, WebSocketCommandInfo>> commandDict)
        {
            var commands = new List<ICommand<TWebSocketSession, WebSocketCommandInfo>>
                {
                    new HandShake<TWebSocketSession>(),
                    new Text<TWebSocketSession>(),  
                    new Binary<TWebSocketSession>(),
                    new Close<TWebSocketSession>(),
                    new Ping<TWebSocketSession>(),
                    new Pong<TWebSocketSession>()
                };

            commands.ForEach(c => commandDict.Add(c.Name, c));

            try
            {
                //Still require it because we need to ensure commandfilters dictionary is not null
                base.SetupCommands(new Dictionary<string, ICommand<TWebSocketSession, WebSocketCommandInfo>>());
            }
            catch
            {
            }

            if (!SetupSubProtocols(Config))
                return false;

            return true;
        }

        private void ExecuteSubCommand(TWebSocketSession session, StringCommandInfo subCommandInfo)
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
                session.HandleUnknownCommand(subCommandInfo);
            }

            session.LastActiveTime = DateTime.Now;
        }

        protected override void OnAppSessionClosed(object sender, AppSessionClosedEventArgs<TWebSocketSession> e)
        {
            if (m_SessionClosed != null)
                m_SessionClosed(e.Session, e.Reason);
        }
    }
}
