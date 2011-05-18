using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Reflection;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public class BasicSubProtocol : BasicSubProtocol<WebSocketSession>
    {
        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : base(commandAssemblies)
        {
        }

        public BasicSubProtocol()
            : base()
        {

        }
    }

    public class BasicSubProtocol<TWebSocketSession> : ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private List<Assembly> m_CommandAssemblies = new List<Assembly>();

        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
        {
            //The items in commandAssemblies may be null, so filter here
            m_CommandAssemblies.AddRange(commandAssemblies.Where(a => a != null));
            SubCommandParser = new BasicSubCommandParser();
        }

        public BasicSubProtocol()
            : this(new List<Assembly> { Assembly.GetEntryAssembly() })
        {

        }

        #region ISubProtocol Members

        public ISubProtocolCommandParser SubCommandParser { get; private set; }

        public IEnumerable<ISubCommand<TWebSocketSession>> GetSubCommands()
        {
            var subCommands = new List<ISubCommand<TWebSocketSession>>();

            foreach (var assembly in m_CommandAssemblies)
            {
                subCommands.AddRange(assembly.GetImplementedObjectsByInterface<ISubCommand<TWebSocketSession>>());
            }

            return subCommands;
        }

        public bool Initialize(IServerConfig config)
        {
            var commandAssembly = config.Options.GetValue("commandAssembly");

            if (string.IsNullOrEmpty(commandAssembly))
                return true;

            try
            {
                string[] assemblies = commandAssembly.Split(',', ';');

                foreach (var a in assemblies)
                {
                    m_CommandAssemblies.Add(Assembly.Load(a));
                }

                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                return false;
            }
        }

        #endregion
    }
}
