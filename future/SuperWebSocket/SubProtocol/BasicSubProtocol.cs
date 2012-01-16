using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Reflection;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Config;

namespace SuperWebSocket.SubProtocol
{
    public class BasicSubProtocol : BasicSubProtocol<WebSocketSession>
    {
        public BasicSubProtocol()
            : base()
        {

        }

        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : base(commandAssemblies)
        {

        }

        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies)
            : base(name, commandAssemblies)
        {

        }

        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies, ICommandParser commandParser)
            : base(name, commandAssemblies, commandParser)
        {
            
        }
    }

    public class BasicSubProtocol<TWebSocketSession> : SubProtocolBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public const string DefaultName = "Basic";

        private List<Assembly> m_CommandAssemblies = new List<Assembly>();

        private Dictionary<string, ISubCommand<TWebSocketSession>> m_CommandDict;

        public static BasicSubProtocol<TWebSocketSession> DefaultInstance { get; private set; }

        static BasicSubProtocol()
        {
            DefaultInstance = new BasicSubProtocol<TWebSocketSession>();
        }

        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : this(DefaultName, commandAssemblies, new BasicSubCommandParser())
        {

        }

        public BasicSubProtocol()
            : this(DefaultName)
        {

        }

        public BasicSubProtocol(string name)
            : this(name, new List<Assembly>() )
        {

        }

        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies)
            : this(name, commandAssemblies, new BasicSubCommandParser())
        {

        }

        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies, ICommandParser commandParser)
            : base(name)
        {
            //The items in commandAssemblies may be null, so filter here
            m_CommandAssemblies.AddRange(commandAssemblies.Where(a => a != null));
            SubCommandParser = commandParser;
        }

        #region ISubProtocol Members

        private void DiscoverCommands()
        {
            var subCommands = new List<ISubCommand<TWebSocketSession>>();

            foreach (var assembly in m_CommandAssemblies)
            {
                subCommands.AddRange(assembly.GetImplementedObjectsByInterface<ISubCommand<TWebSocketSession>>());
            }

#if DEBUG
            var cmdbuilder = new StringBuilder();
            cmdbuilder.AppendLine(string.Format("SubProtocol {0} found the commands below:", this.Name));

            foreach (var c in subCommands)
            {
                cmdbuilder.AppendLine(c.Name);
            }

            LogUtil.LogDebug(cmdbuilder.ToString());
#endif

            m_CommandDict = new Dictionary<string, ISubCommand<TWebSocketSession>>(subCommands.Count, StringComparer.OrdinalIgnoreCase);
            subCommands.ForEach(c => m_CommandDict.Add(c.Name, c));
        }

        public override bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig)
        {
            if (Name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase))
            {
                var commandAssembly = config.Options.GetValue("commandAssembly");

                if (!string.IsNullOrEmpty(commandAssembly))
                {
                    if (!ResolveCommmandAssembly(commandAssembly))
                        return false;
                }
            }

            if (protocolConfig != null && protocolConfig.Commands != null)
            {
                foreach (var commandConfig in protocolConfig.Commands)
                {
                    var assembly = commandConfig.Options.GetValue("assembly");

                    if (!string.IsNullOrEmpty(assembly))
                    {
                        if (!ResolveCommmandAssembly(assembly))
                            return false;
                    }
                }
            }

            //Always discover commands
            DiscoverCommands();

            return true;
        }

        private bool ResolveCommmandAssembly(string definition)
        {
            try
            {
                var assemblies = AssemblyUtil.GetAssembliesFromString(definition);

                if (assemblies.Any())
                    m_CommandAssemblies.AddRange(assemblies);

                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                return false;
            }
        }

        public override bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command)
        {
            return m_CommandDict.TryGetValue(name, out command);
        }

        #endregion
    }
}
