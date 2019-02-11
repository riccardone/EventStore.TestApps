using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace PersistantSubscriber_fw472
{
    public class EventStoreSubscriptionClient
    {
        private const string GroupName = "test-subscribers";
        private const string StreamName = "$ce-domain";

        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly UserCredentials _userCredentials;

        private EventStorePersistentSubscriptionBase EventStorePersistentSubscriptionBase { get; set; }

        public EventStoreSubscriptionClient(IEventStoreConnection eventStoreConnection, UserCredentials userCredentials)
        {
            _eventStoreConnection = eventStoreConnection;
            _userCredentials = userCredentials;
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

        public void Connect()
        {
            _eventStoreConnection.ConnectAsync().Wait();
            CreatePersistentSubscription(_eventStoreConnection);
            EventStorePersistentSubscriptionBase = _eventStoreConnection.ConnectToPersistentSubscription(StreamName,
                GroupName, EventAppeared, SubscriptionDropped, _userCredentials);
        }

        private void SubscriptionDropped(EventStorePersistentSubscriptionBase subscription, SubscriptionDropReason reason, Exception ex)
        {
            Connect();
        }

        private static Task EventAppeared(EventStorePersistentSubscriptionBase subscription, ResolvedEvent resolvedEvent)
        {
            Console.WriteLine("Event appeared: " + resolvedEvent.Event.EventId);
            return Task.CompletedTask;
        }
    }
}
