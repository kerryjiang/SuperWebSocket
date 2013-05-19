using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using System.Threading;

namespace PushTestClient
{
    class Program
    {
        private static WebSocket[] m_WebSockets;

        static void Main(string[] args)
        {
            m_WebSockets = new WebSocket[1000];

            var autoEventReset = new AutoResetEvent(false);

            for(var i = 0; i < m_WebSockets.Length; i++)
            {
                var websocket = new WebSocket("ws://localhost:2011/");
                websocket.Opened += (s, e) =>
                    {
                        autoEventReset.Set();
                    };
                websocket.Error +=  (s, e) =>
                    {
                        Console.WriteLine(e.Exception.Message);
                        autoEventReset.Set();
                    };

                websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
                websocket.Open();
                autoEventReset.WaitOne();

                m_WebSockets[i] = websocket;

                Console.WriteLine(i);
            }

            Console.WriteLine("All connected");
            Console.ReadLine();
        }

        static void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            
        }
    }
}
