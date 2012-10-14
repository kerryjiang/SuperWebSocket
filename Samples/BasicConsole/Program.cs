using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperWebSocket.Samples.BasicConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start the WebSocketServer!");

            Console.ReadKey();
            Console.WriteLine();

            var appServer = new WebSocketServer();

            //Setup the appServer
            if (!appServer.Setup(2012)) //Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }

            appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);

            Console.WriteLine();

            //Try to start the appServer
            if (!appServer.Start())
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("The server started successfully, press key 'q' to stop it!");

            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            //Stop the appServer
            appServer.Stop();

            Console.WriteLine();
            Console.WriteLine("The server was stopped!");
            Console.ReadKey();
        }

        static void appServer_NewMessageReceived(WebSocketSession session, string message)
        {
            //Send the received message back
            session.Send("Server: " + message);
        }
    }
}
