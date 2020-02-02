using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.NotificationService
{
    public sealed class NotificationEventHandler
    {
        private static NotificationEventHandler instance;
        private static object                   instanceLock = new object();

        private INotificationService[] _registry;

        public static NotificationEventHandler Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new NotificationEventHandler();
                    }

                    return instance;
                }
            }
        }

        NotificationEventHandler()
        {
            _registry = new INotificationService[0x20];
        }

        internal void RegisterNotificationService(INotificationService service)
        {
            // NOTE: in case there isn't space anymore in the registry array, Nintendo doesn't return any errors.
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == null)
                {
                    _registry[i] = service;
                    break;
                }
            }
        }

        internal void UnregisterNotificationService(INotificationService service)
        {
            // NOTE: in case there isn't the entry in the registry array, Nintendo doesn't return any errors.
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == service)
                {
                    _registry[i] = null;
                    break;
                }
            }
        }

        // TODO: Use this when we will have enough things to go online.
        public void SignalFriendListUpdate(UserId targetId)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] != null)
                {
                    _registry[i].SignalFriendListUpdate(targetId);
                }
            }
        }

        // TODO: Use this when we will have enough things to go online.
        public void SignalNewFriendRequest(UserId targetId)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] != null)
                {
                    _registry[i].SignalNewFriendRequest(targetId);
                }
            }
        }
    }
}
