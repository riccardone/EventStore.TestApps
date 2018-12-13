using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EventReader_fw461
{
    class Program
    {
        private static IEventStoreConnection _conn;
        private static int _port = 1113;

        static void Main(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out _port))
                Console.WriteLine($"Connecting to localhost on port {_port}");
            try
            {
                do
                {
                    Console.WriteLine("Insert a stream name and press Enter to read events from (Press ctrl+C to exit");
                    var stream = Console.ReadLine();
                    ReadEvents(stream, _conn);
                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static async void ReadEvents(string stream, IEventStoreConnection conn)
        {
            try
            {
                _conn = await GetOpenConnection(_port);
                var results = await _conn.ReadStreamEventsForwardAsync(stream, StreamPosition.Start, 10, false);
                foreach (var evt in results.Events)
                    Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task<IEventStoreConnection> GetOpenConnection(int port)
        {
            var conn = EventStoreConnection.Create(GetConnectionBuilder(), new Uri($"tcp://localhost:{port}"));
            conn.Reconnecting += Conn_Reconnecting;
            await conn.ConnectAsync();
            return conn;
        }

        private static ConnectionSettings GetConnectionBuilder()
        {
            var settings = ConnectionSettings.Create()
                .SetOperationTimeoutTo(TimeSpan.FromSeconds(2))
                .KeepReconnecting()
                .EnableVerboseLogging()
                .FailOnNoServerResponse();
            return settings;
        }

        private static void Conn_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            Console.WriteLine("Conn_Reconnecting...");
        }
    }
}
