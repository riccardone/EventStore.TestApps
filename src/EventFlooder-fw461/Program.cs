using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EventFlooder_fw461
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press f to send tons of test messages");
            try
            {
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.F)
                {
                    var port = 1113;
                    if (args.Length > 0 && int.TryParse(args[0], out port))
                        Console.WriteLine($"Connecting to localhost on port {port}");
                    var conn = EventStoreConnection.Create(GetConnectionBuilder(), new Uri($"tcp://localhost:{port}"));
                    conn.Reconnecting += Conn_Reconnecting;
                    conn.ConnectAsync().Wait();
                    var tasks = new Task<long>[100];
                    for (int i = 0; i < 100; i++)
                    {
                        tasks[i] = (WriteStreams(conn));
                    }

                    Task.WaitAll(tasks);
                    Console.WriteLine(tasks.Select(x => x.Result).Aggregate(0L, (cur, val) => cur + val));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.GetBaseException().Message}");
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        static async Task<long> WriteStreams(IEventStoreConnection cn)
        {
            long appended = 0;
            var data = Enumerable.Range(0, 1000).Select(x =>
                new EventData(Guid.NewGuid(), "test", false, new byte[] { 1, 2, 3 }, null)).ToArray();

            for (int i = 0; i < 99999; i++)
            {
                await cn.AppendToStreamAsync(Guid.NewGuid().ToString("N"), ExpectedVersion.NoStream, data);
                appended += data.Length;
                Console.Write(".");
            }

            return appended;
        }

        private static void Conn_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            Console.WriteLine("Conn_Reconnecting...");
        }

        private static ConnectionSettings GetConnectionBuilder()
        {
            var settings = ConnectionSettings.Create();
            return settings;
        }
    }
}
