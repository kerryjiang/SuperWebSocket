using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WebSocket4Net;
using SuperWebSocket.PerformanceTest.TestServer;

namespace SuperWebSocket.PerformanceTest.TestClientOneVOne
{
    class Program
    {
        private static volatile bool m_Stopped = false;

        private static long m_Sent = 0;

        private static long m_PrevSend = 0;

        private static long m_Received = 0;

        private static long m_PrevReceived = 0;

        private static Timer m_PrintTimer;

        private static AutoResetEvent m_MessageEvent = new AutoResetEvent(false);

        private const int m_TimerSpan = 5;

        private static JsonWebSocket m_SendingWebSocket;

        private static JsonWebSocket m_ReceivingWebSocket;

        static void Main(string[] args)
        {
            var websocket = new JsonWebSocket("ws://127.0.0.1:2011/");

            websocket.On<string>("NEW", HandleNewCommingResponse);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(websocket_Error);
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Open();

            m_SendingWebSocket = websocket;

            m_ReceivingWebSocket = new JsonWebSocket("ws://127.0.0.1:2011/");
            m_ReceivingWebSocket.On<ClientInfo>("ECHOY", HandleEchoResponse);
            m_ReceivingWebSocket.Opened += new EventHandler(m_ReceivingWebSocket_Opened);
            m_ReceivingWebSocket.Closed += new EventHandler(m_ReceivingWebSocket_Closed);
            m_ReceivingWebSocket.Open();

            m_PrintTimer = new Timer(OnPrintTimerCallback, null, 1000 * m_TimerSpan, 1000 * m_TimerSpan);

            while (!Console.ReadLine().ToLower().Equals("q"))
                continue;

            m_Stopped = true;
            m_MessageEvent.Set();
            m_PrintTimer.Change(Timeout.Infinite, Timeout.Infinite);


            m_SendingWebSocket.Close();
            m_ReceivingWebSocket.Close();

            Console.WriteLine("Quit");
            Console.ReadLine();
        }

        static void m_ReceivingWebSocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Receiving websocket closed");
        }

        static void m_ReceivingWebSocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Receiving websocket connected");
        }

        static void websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Sending websocket connected");
        }

        static void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        }

        static void OnPrintTimerCallback(object state)
        {
            var received = m_Received;
            var sent = m_Sent;

            Console.WriteLine("Sent: {0}, {1}/s, Received: {2}, {3}/s", sent, (sent - m_PrevSend) / m_TimerSpan, received, (received - m_PrevReceived) / m_TimerSpan);
            m_PrevReceived = received;
            m_PrevSend = sent;
        }

        static void websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Sending websocket closed");
        }

        private static void HandleNewCommingResponse(JsonWebSocket websocket, string content)
        {
            while (!m_Stopped)
            {
                RunTest(websocket, content);
                m_MessageEvent.WaitOne();
            }
        }

        private static void HandleEchoResponse(JsonWebSocket websocket, ClientInfo content)
        {
            Interlocked.Increment(ref m_Received);
            m_MessageEvent.Set();
        }

        private static Random m_Random = new Random();

        private static void RunTest(JsonWebSocket websocket, string targetID)
        {
            websocket.Send("ECHOY", new ClientInfo
            {
                ID = m_Random.Next(1, 1000),
                Height = m_Random.Next(1, 1000),
                Width = m_Random.Next(1, 1000),
                LocationX = m_Random.Next(1, 1000),
                LocationY = m_Random.Next(1, 1000),
                TargetID = targetID
            });

            Interlocked.Increment(ref m_Sent);
        }
    }
}
