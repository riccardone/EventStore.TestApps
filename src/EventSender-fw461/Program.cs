using System;
using System.Text;
using EventStore.ClientAPI;

namespace ConsoleApp3
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
                conn.Reconnecting += Conn_Reconnecting;
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
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
       
        private static void Conn_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            Console.WriteLine("Conn_Reconnecting...");
        }

        private static void PublishTestEvent(IEventStoreConnection conn)
        {
            conn.AppendToStreamAsync("ciccio", ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "CiccioCreated2", true, Encoding.UTF8.GetBytes("ciao ciao"), null)).Wait();
            Console.WriteLine("Event published");
        }

        private static ConnectionSettings GetConnectionBuilder()
        {
            var settings = ConnectionSettings.Create().SetQueueTimeoutTo(TimeSpan.FromSeconds(1));
                //.FailOnNoServerResponse();
                //.LimitRetriesForOperationTo(1);
                //.KeepRetrying()
                //.KeepReconnecting();
            return settings;
        }
    }
}
