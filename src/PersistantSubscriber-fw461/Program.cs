using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace PersistantSubscriber_fw461
{
    class Program
    {
        const string StreamName = "domain-TestStream";
        const string GroupName = "test-subscribers";

        static void Main(string[] args)
        {
            var es = "localhost:1113";
            if (args.Length > 0)
                es = args[0];
            var uri = new Uri($"tcp://{es}");
            var conn = EventStoreConnection.Create(GetConnectionBuilder(), uri);
            try
            {
                conn.ConnectAsync().Wait();
                CreatePersistentSubscription(conn);
                conn.ConnectToPersistentSubscription(StreamName, GroupName,
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
                conn.CreatePersistentSubscriptionAsync(StreamName, GroupName, PersistentSubscriptionSettings.Create().StartFromBeginning(),
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
