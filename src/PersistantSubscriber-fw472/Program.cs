using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace PersistantSubscriber_fw472
{
    class Program
    {
        static void Main(string[] args)
        {
            var es = "localhost:1113";
            if (args.Length > 0)
                es = args[0];
            var uri = new Uri($"tcp://{es}");
            var subscriber = new EventStoreSubscriptionClient(
                EventStoreConnection.Create(ConnectionSettings.Default, uri, "test"),
                new UserCredentials("admin", "changeit"));
            subscriber.Connect();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
