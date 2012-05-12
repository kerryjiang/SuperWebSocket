using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using System.Threading.Tasks;
using System.Threading;

namespace SuperWebSocket.PerformanceTest.TestClient
{
    class Program
    {
        private static JsonWebSocket[] m_WebSockets;

        private static Semaphore m_Semaphore;

        private static volatile bool m_Stopped = false;

        private static long m_Sent = 0;

        private static long m_Received = 0;

        private static int m_ConnectedClients = 0;

        private static Timer m_PrintTimer;

        static void Main(string[] args)
        {
            int clientCount = 100;
            m_WebSockets = new JsonWebSocket[clientCount];
            m_Semaphore = new Semaphore(0, clientCount);

            m_PrintTimer = new Timer(OnPrintTimerCallback, null, 1000 * 5, 1000 * 5);

            int group = 100;

            for (var i = 0; i < clientCount; i += group)
            {
                int to = Math.Min(i + group, clientCount);

                Parallel.For(i, to, (j) =>
                {
                    m_WebSockets[j] = CreateWebSocket();
                });

                Thread.Sleep(1000);
            }

            Console.WriteLine("All clients have been created!");

            while (!Console.ReadLine().ToLower().Equals("q"))
                continue;

            m_Stopped = true;
            m_PrintTimer.Change(Timeout.Infinite, Timeout.Infinite);

            int closedNumber = 0;

            while (closedNumber < clientCount)
            {
                m_Semaphore.WaitOne();
                closedNumber++;
            }

            Console.WriteLine("All clients quit!");
            Console.ReadLine();
        }

        static JsonWebSocket CreateWebSocket()
        {
            var websocket = new JsonWebSocket("ws://127.0.0.1:2011/");
            
            websocket.On<string>("ECHO", HandleEchoResponse);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(websocket_Error);
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Open();
            return websocket;
        }

        static void websocket_Opened(object sender, EventArgs e)
        {
            Interlocked.Increment(ref m_ConnectedClients);
            RunTest((JsonWebSocket)sender);
        }

        static void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        }

        static void OnPrintTimerCallback(object state)
        {
            Console.WriteLine("Connected: {0}, Sent: {1}, Received: {2}", m_ConnectedClients, m_Sent, m_Received);
        }

        static void websocket_Closed(object sender, EventArgs e)
        {
            m_Semaphore.Release();
        }

        private static void HandleEchoResponse(JsonWebSocket websocket, string content)
        {
            Interlocked.Increment(ref m_Received);

            if (m_Stopped)
            {
                websocket.Close();
                return;
            }

            //Thread.Sleep(10);
            RunTest(websocket);
        }

        private static void RunTest(JsonWebSocket websocket)
        {
            websocket.Send("ECHO", Guid.NewGuid().ToString());
            Interlocked.Increment(ref m_Sent);
        }
    }
}
