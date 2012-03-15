using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Config;

namespace SuperWebSocket.SubProtocol
{
    /// <summary>
    /// Default basic sub protocol implementation
    /// </summary>
    public class BasicSubProtocol : BasicSubProtocol<WebSocketSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        public BasicSubProtocol()
            : base(Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        public BasicSubProtocol(string name)
            : base(name, Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(Assembly commandAssembly)
            : base(commandAssembly)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : base(commandAssemblies)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(string name, Assembly commandAssembly)
            : base(name, commandAssembly)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies)
            : base(name, commandAssemblies)
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        /// <param name="requestInfoParser">The request info parser.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies, IRequestInfoParser<StringRequestInfo> requestInfoParser)
            : base(name, commandAssemblies, requestInfoParser)
        {

        }
    }

    /// <summary>
    /// Default basic sub protocol implementation
    /// </summary>
    public class BasicSubProtocol<TWebSocketSession> : SubProtocolBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public const string DefaultName = "Basic";

        private List<Assembly> m_CommandAssemblies = new List<Assembly>();

        private Dictionary<string, ISubCommand<TWebSocketSession>> m_CommandDict;

        public static BasicSubProtocol<TWebSocketSession> DefaultInstance { get; private set; }

        private ILog m_Logger;

        static BasicSubProtocol()
        {
            DefaultInstance = new BasicSubProtocol<TWebSocketSession>(DefaultName, new List<Assembly>());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with the calling aseembly as command assembly
        /// </summary>
        public BasicSubProtocol()
            : this(DefaultName, Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with the calling aseembly as command assembly
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        public BasicSubProtocol(string name)
            : this(name, Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with command assemblies
        /// </summary>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : this(DefaultName, commandAssemblies, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with single command assembly.
        /// </summary>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(Assembly commandAssembly)
            : this(DefaultName, new List<Assembly> { commandAssembly }, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with name and single command assembly.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(string name, Assembly commandAssembly)
            : this(name, new List<Assembly> { commandAssembly }, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with name and command assemblies.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies)
            : this(name, commandAssemblies, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        /// <param name="commandParser">The command parser.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies, IRequestInfoParser<StringRequestInfo> requestInfoParser)
            : base(name)
        {
            //The items in commandAssemblies may be null, so filter here
            m_CommandAssemblies.AddRange(commandAssemblies.Where(a => a != null));
            SubCommandParser = requestInfoParser;
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


            m_Logger.Debug(cmdbuilder.ToString());
#endif

            m_CommandDict = new Dictionary<string, ISubCommand<TWebSocketSession>>(subCommands.Count, StringComparer.OrdinalIgnoreCase);
            subCommands.ForEach(c => m_CommandDict.Add(c.Name, c));
        }

        public override bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig, ILog logger)
        {
            m_Logger = logger;

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
                m_Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Tries get command from the sub protocol's command inventory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public override bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command)
        {
            return m_CommandDict.TryGetValue(name, out command);
        }

        #endregion
    }
}
