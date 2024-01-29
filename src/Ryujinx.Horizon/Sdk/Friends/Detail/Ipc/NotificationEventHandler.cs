using Ryujinx.Horizon.Sdk.Account;

namespace Ryujinx.Horizon.Sdk.Friends.Detail.Ipc
{
    sealed class NotificationEventHandler
    {
        private readonly NotificationService[] _registry;

        public NotificationEventHandler()
        {
            _registry = new NotificationService[0x20];
        }

        public void RegisterNotificationService(NotificationService service)
        {
            // NOTE: When there's no enough space in the registry array, Nintendo doesn't return any errors.
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == null)
                {
                    _registry[i] = service;
                    break;
                }
            }
        }

        public void UnregisterNotificationService(NotificationService service)
        {
            // NOTE: When there's no enough space in the registry array, Nintendo doesn't return any errors.
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == service)
                {
                    _registry[i] = null;
                    break;
                }
            }
        }

        // TODO: Use this when we have enough things to go online.
        public void SignalFriendListUpdate(Uid targetId)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                _registry[i]?.SignalFriendListUpdate(targetId);
            }
        }

        // TODO: Use this when we have enough things to go online.
        public void SignalNewFriendRequest(Uid targetId)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                _registry[i]?.SignalNewFriendRequest(targetId);
            }
        }
    }
}
