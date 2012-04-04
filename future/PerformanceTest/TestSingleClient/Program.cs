using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using System.Threading;

namespace SuperWebSocket.PerformanceTest.TestSingleClient
{
    class Program
    {
        private static volatile bool m_Stopped = false;

        private static long m_Sent = 0;

        private static long m_Received = 0;

        private static Timer m_PrintTimer;

        private static ManualResetEvent m_ClosedEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var websocket = new JsonWebSocket("ws://127.0.0.1:2011/");

            websocket.On<string>("ECHO", HandleEchoResponse);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(websocket_Error);
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Open();

            m_PrintTimer = new Timer(OnPrintTimerCallback, null, 1000 * 5, 1000 * 5);

            while (!Console.ReadLine().ToLower().Equals("q"))
                continue;

            m_Stopped = true;
            m_PrintTimer.Change(Timeout.Infinite, Timeout.Infinite);

            m_ClosedEvent.WaitOne();

            Console.WriteLine("Quit");
            Console.ReadLine();
        }

        static void websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
            RunTest((JsonWebSocket)sender);
        }

        static void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        }

        static void OnPrintTimerCallback(object state)
        {
            Console.WriteLine("Sent: {0}, Received: {1}", m_Sent, m_Received);
        }

        static void websocket_Closed(object sender, EventArgs e)
        {
            m_ClosedEvent.Set();
        }

        private static void HandleEchoResponse(JsonWebSocket websocket, string content)
        {
            Interlocked.Increment(ref m_Received);

            if (m_Stopped)
            {
                websocket.Close();
                return;
            }

            RunTest(websocket);
        }

        private static void RunTest(JsonWebSocket websocket)
        {
            websocket.Send("ECHO", Guid.NewGuid().ToString());
            Interlocked.Increment(ref m_Sent);
        }
    }
}
