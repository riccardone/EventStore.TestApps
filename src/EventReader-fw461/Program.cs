using System;
using System.Text;
using EventStore.ClientAPI;

namespace EventReader_fw461
{
    partial class Program
    {
        private static IEventStoreConnection _conn;
        private static ConnectionFactory _connectionFactory;

        static void Main(string[] args)
        {
            var port = 1113;
            if (args.Length > 0 && int.TryParse(args[0], out port))
                Console.WriteLine($"Connecting to localhost on port {port}");
            
            try
            {
                _connectionFactory = new ConnectionFactory(port);
                _connectionFactory.ConnectionClosed += ConnFactory_ConnectionClosed;
                _conn = _connectionFactory.GetOpenConnection().Result;
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

        private static void ConnFactory_ConnectionClosed(object sender, EventArgs e)
        {
            _conn = _connectionFactory.GetOpenConnection().Result;
        }
        
        private static void ReadEvents(string stream, IEventStoreConnection conn)
        {
            try
            {
                var results = conn.ReadStreamEventsForwardAsync(stream, StreamPosition.Start, 10, false).GetAwaiter().GetResult();
                foreach (var evt in results.Events)
                    Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        
    }
}
