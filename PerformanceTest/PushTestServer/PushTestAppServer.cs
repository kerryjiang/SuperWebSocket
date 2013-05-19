using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperWebSocket.PerformanceTest.PushTestServer
{
    public class PushTestAppServer : WebSocketServer
    {
        private Timer m_Timer;

        protected override void OnStartup()
        {
            m_Timer = new Timer(TimerCallback);
            m_Timer.Change(100, 100);

            base.OnStartup();
        }

        void TimerCallback(object state)
        {
            m_Timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                foreach (var s in this.GetAllSessions())
                {
                    s.Send("{\"message\": \"_ping\"}");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            m_Timer.Change(100, 100);
        }

        protected override void OnStopped()
        {
            m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
            m_Timer.Dispose();
            m_Timer = null;

            base.OnStopped();
        }
    }
}
