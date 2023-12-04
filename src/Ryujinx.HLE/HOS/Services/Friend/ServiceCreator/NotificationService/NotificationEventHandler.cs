using Ryujinx.HLE.HOS.Services.Account.Acc;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.NotificationService
{
    public sealed class NotificationEventHandler
    {
        private static NotificationEventHandler _instance;
        private static readonly object _instanceLock = new();

        private readonly INotificationService[] _registry;

        public static NotificationEventHandler Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    _instance ??= new NotificationEventHandler();

                    return _instance;
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
                _registry[i]?.SignalFriendListUpdate(targetId);
            }
        }

        // TODO: Use this when we will have enough things to go online.
        public void SignalNewFriendRequest(UserId targetId)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                _registry[i]?.SignalNewFriendRequest(targetId);
            }
        }
    }
}
