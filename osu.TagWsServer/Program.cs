using System;

namespace osu.TagWsServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketSharp.Server.WebSocketServer();

            server.AddWebSocketService<OsuTagWebSocketService>("/osutag");

            server.Start();

            Console.WriteLine("Started websocket server");
            Console.ReadKey(true);

            server.Stop();
        }
    }
}
