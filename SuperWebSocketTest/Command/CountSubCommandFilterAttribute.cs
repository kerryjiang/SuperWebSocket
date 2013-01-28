using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocketTest.Command
{
    public class CountSubCommandFilterAttribute : SubCommandFilterAttribute
    {
        private static int m_ExecutingCount;

        public static int ExecutingCount
        {
            get { return m_ExecutingCount; }
        }

        private static int m_ExecutedCount;

        public static int ExecutedCount
        {
            get { return m_ExecutedCount; }
        }

        public override void OnCommandExecuted(CommandExecutingContext commandContext)
        {
            Interlocked.Increment(ref m_ExecutedCount);
        }

        public override void OnCommandExecuting(CommandExecutingContext commandContext)
        {
            Interlocked.Increment(ref m_ExecutingCount);
        }
    }
}
