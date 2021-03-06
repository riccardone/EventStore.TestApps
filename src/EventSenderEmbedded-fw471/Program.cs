﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using EventStore.Common.Options;
using Newtonsoft.Json;

namespace EventSenderEmbedded_fw471
{
    class Program
    {
        static void Main(string[] args)
        {
            var ciccio = EmbeddedVNodeBuilder.AsSingleNode().RunProjections(ProjectionType.All).Build();
            ciccio.Start();

            var conn = EventStoreConnection.Create(GetConnectionBuilder(), new Uri($"tcp://localhost:1113"));
            var projectionsManager = new ProjectionsManager(new TestLogger(), new IPEndPoint(IPAddress.Loopback, 2113),
                TimeSpan.FromSeconds(5));
            var projectionName = "LockeshTest";
            var streamName = "ciccio1";
            try
            {
                conn.Reconnecting += Conn_Reconnecting;
                conn.ErrorOccurred += Conn_ErrorOccurred;
                conn.ConnectAsync();
                do
                {
                    Console.Clear();
                    Console.WriteLine($"Press P to create projection {projectionName}");
                    Console.WriteLine($"Press S to send 1000 events to stream {streamName}");
                    Console.WriteLine($"Press O to send 1 event to stream {streamName}");
                    Console.WriteLine($"Press D to  Disable projection '{projectionName}'");
                    Console.WriteLine($"Press E to  Enable projection '{projectionName}'");
                    Console.WriteLine($"Press A to set $maxAge=2 to stream {streamName}");
                    Console.WriteLine($"Press C to set $maxCount=2 to stream {streamName}");
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.O || key.Key == ConsoleKey.NumPad0)
                        AppendToStreamAsync(conn, streamName, 1);
                    if (key.Key == ConsoleKey.S)
                        AppendToStreamAsync(conn, streamName, 1000);
                    if (key.Key == ConsoleKey.P)
                        projectionsManager.CreateContinuousAsync(projectionName,
                            $"fromStream('{streamName}').when({{'$any': function(state, evnt) {{linkTo('OurTargetStreamName', evnt);  }}}});",
                            new UserCredentials("admin", "changeit"));
                    if (key.Key == ConsoleKey.D)
                        projectionsManager.DisableAsync(projectionName, new UserCredentials("admin", "changeit")).Wait();
                    if (key.Key == ConsoleKey.E)
                        projectionsManager.EnableAsync(projectionName, new UserCredentials("admin", "changeit")).Wait();
                    if (key.Key == ConsoleKey.A)
                        SetStreamMetadata(conn, streamName, new Dictionary<string, int> { { "$maxAge", 2 } });
                    if (key.Key == ConsoleKey.C)
                        SetStreamMetadata(conn, streamName, new Dictionary<string, int> { { "$maxCount", 2 } });
                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void Conn_ErrorOccurred(object sender, ClientErrorEventArgs e)
        {
            Console.WriteLine($"Conn_ErrorOccurred: {e.Exception.Message}");
        }

        private static void Conn_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            Console.WriteLine("Conn_Reconnecting...");
        }

        private static void SetStreamMetadata(IEventStoreConnection connection, string streamName, Dictionary<string, int> metadata)
        {
            connection.SetStreamMetadataAsync(streamName, ExpectedVersion.Any, SerializeObject(metadata)).Wait();
        }

        static async Task AppendToStreamAsync(IEventStoreConnection cn, string streamName, int howManyEvents)
        {
            for (int i = 0; i < howManyEvents; i++)
            {
                await cn.AppendToStreamAsync(streamName, ExpectedVersion.Any,
                    new EventData(Guid.NewGuid(), "test", false, new byte[] { 1, 2, 3 }, null));
            }
        }

        private static byte[] SerializeObject(object obj)
        {
            var jsonObj = JsonConvert.SerializeObject(obj);
            var data = Encoding.UTF8.GetBytes(jsonObj);
            return data;
        }

        private static ConnectionSettings GetConnectionBuilder()
        {
            var settings = ConnectionSettings.Create()
                .LimitRetriesForOperationTo(2)
                .FailOnNoServerResponse()
                .KeepReconnecting();
            return settings;
        }
    }

    public class TestLogger : ILogger
    {
        public void Error(string format, params object[] args) { }

        public void Error(Exception ex, string format, params object[] args) { }

        public void Info(string format, params object[] args) { }

        public void Info(Exception ex, string format, params object[] args) { }

        public void Debug(string format, params object[] args) { }

        public void Debug(Exception ex, string format, params object[] args) { }
    }
}
