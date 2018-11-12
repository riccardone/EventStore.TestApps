using System;
using System.Text;
using EventStore.ClientAPI;

namespace EventSender_netcore21
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 1113;
            if (args.Length > 0 && int.TryParse(args[0], out port))
                Console.WriteLine($"Connecting to localhost on port {port}");
            var conn = EventStoreConnection.Create(GetConnectionBuilder(), new Uri($"tcp://localhost:{port}"));
            try
            {
                conn.ConnectAsync().Wait();
                ConsoleKeyInfo key;
                do
                {
                    Console.WriteLine("Press A to publish test event (any other key to leave)");
                    key = Console.ReadKey();
                    if (key.Key == ConsoleKey.A)
                    {
                        PublishTestEvent(conn);
                    }
                } while (key.Key == ConsoleKey.A);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void PublishTestEvent(IEventStoreConnection conn)
        {
            conn.AppendToStreamAsync("ciccio", ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "CiccioCreated2", true, Encoding.UTF8.GetBytes("ciao ciao"), null)).Wait();
            Console.WriteLine("Event published");
        }

        private static ConnectionSettings GetConnectionBuilder()
        {
            var settings = ConnectionSettings.Create()
                .KeepRetrying()
                .KeepReconnecting();
            return settings;
        }
    }
}
