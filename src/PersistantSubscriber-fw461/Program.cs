using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace PersistantSubscriber_fw461
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
                CreatePersistentSubscription(conn);
                conn.ConnectToPersistentSubscription("ciccio", "CiccioGroup",
                    (Action<EventStorePersistentSubscriptionBase, ResolvedEvent>)EventAppeared, SubscriptionDropped);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }

        private static void CreatePersistentSubscription(IEventStoreConnection conn)
        {
            try
            {
                conn.CreatePersistentSubscriptionAsync("ciccio", "CiccioGroup", PersistentSubscriptionSettings.Create().StartFromBeginning(),
                    new UserCredentials("admin", "changeit")).Wait();
            }
            catch (Exception e)
            {
                // Already exist
            }
        }

        private static void SubscriptionDropped(EventStorePersistentSubscriptionBase arg1, SubscriptionDropReason arg2, Exception arg3)
        {
            Console.WriteLine(arg2);
            Console.WriteLine(arg3.GetBaseException().Message);
        }

        private static void EventAppeared(EventStorePersistentSubscriptionBase arg1, ResolvedEvent arg2)
        {
            Console.WriteLine($"Message '{arg2.OriginalEvent.EventId}' handled");
            arg1.Acknowledge(arg2);
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
