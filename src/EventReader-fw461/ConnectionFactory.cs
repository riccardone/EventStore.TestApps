using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EventReader_fw461
{
    partial class Program
    {
        public class ConnectionFactory
        {
            private readonly int _port;
            public event EventHandler ConnectionClosed;

            public ConnectionFactory(int port)
            {
                _port = port;
            }

            public async Task<IEventStoreConnection> GetOpenConnection()
            {
                try
                {
                    var conn = EventStoreConnection.Create(GetConnectionBuilder(), new Uri($"tcp://localhost:{_port}"));
                    conn.Reconnecting += Conn_Reconnecting1;
                    conn.Disconnected += Conn_Disconnected;
                    conn.ErrorOccurred += Conn_ErrorOccurred;
                    conn.Closed += Conn_Closed;
                    await conn.ConnectAsync();
                    return conn;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }

            private void Conn_Closed(object sender, ClientClosedEventArgs e)
            {
                Console.WriteLine(e.Reason);
                ConnectionClosed?.Invoke(sender, e);
            }

            private void Conn_ErrorOccurred(object sender, ClientErrorEventArgs e)
            {
                Console.WriteLine(e);
            }

            private void Conn_Disconnected(object sender, ClientConnectionEventArgs e)
            {
                Console.WriteLine(e);
            }

            private void Conn_Reconnecting1(object sender, ClientReconnectingEventArgs e)
            {
                Console.WriteLine("Conn_Reconnecting...");
            }

            private static ConnectionSettings GetConnectionBuilder()
            {
                var settings = ConnectionSettings.Create()
                    .SetOperationTimeoutTo(TimeSpan.FromSeconds(2))
                    .KeepReconnecting()
                    .EnableVerboseLogging()
                    .FailOnNoServerResponse();
                    //.LimitRetriesForOperationTo(100);
                //.LimitAttemptsForOperationTo(1)
                //.LimitReconnectionsTo(2)
                //.LimitRetriesForOperationTo(1);
                //.SetQueueTimeoutTo(TimeSpan.FromSeconds(2))
                return settings;
            }
        }

        
    }
}
