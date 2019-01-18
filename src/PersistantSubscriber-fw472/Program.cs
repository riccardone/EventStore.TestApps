using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace PersistantSubscriber_fw472
{
    class Program
    {
        static void Main(string[] args)
        {
            var subscriber = new EventStoreSubscriptionClient(
                EventStoreConnection.Create(ConnectionSettings.Default, new Uri("tcp://localhost:1113"), "test"),
                new UserCredentials("admin", "changeit"));
            subscriber.Connect();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
