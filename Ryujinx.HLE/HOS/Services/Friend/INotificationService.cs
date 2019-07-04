using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class INotificationService : IpcService, IDisposable
    {
        private readonly UInt128                      _userId;
        private readonly FriendServicePermissionLevel _permissionLevel;

        private readonly object _lock = new object();

        private KEvent _notificationEvent;
        private int    _notificationEventHandle = 0;


        private LinkedList<NotificationInfo> _notifications;

        private bool _hasNewFriendRequest;
        private bool _hasFriendListUpdate;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public INotificationService(ServiceCtx context, UInt128 userId, FriendServicePermissionLevel permissionLevel)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetEvent }, // 2.0.0+
                { 1, Clear    }, // 2.0.0+
                { 2, Pop      }, // 2.0.0+
            };

            _userId            = userId;
            _permissionLevel   = permissionLevel;
            _notifications     = new LinkedList<NotificationInfo>();
            _notificationEvent = new KEvent(context.Device.System);

            _hasNewFriendRequest = false;
            _hasFriendListUpdate = false;

            NotificationEventHandler.Instance.RegisterNotificationService(this);
        }

        // nn::friends::detail::ipc::INotificationService::GetEvent() -> handle<copy>
        public long GetEvent(ServiceCtx context)
        {
            if (_notificationEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_notificationEvent.ReadableEvent, out _notificationEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_notificationEventHandle);

            return 0;
        }

        // nn::friends::detail::ipc::INotificationService::Clear()
        public long Clear(ServiceCtx context)
        {
            lock (_lock)
            {
                _hasNewFriendRequest = false;
                _hasFriendListUpdate = false;

                _notifications.Clear();
            }

            return 0;
        }

        // nn::friends::detail::ipc::INotificationService::Pop() -> nn::friends::detail::ipc::SizedNotificationInfo
        public long Pop(ServiceCtx context)
        {
            lock (_lock)
            {
                if (_notifications.Count >= 1)
                {
                    NotificationInfo notificationInfo = _notifications.First.Value;
                    _notifications.RemoveFirst();

                    if (notificationInfo.Type == NotificationEventType.FriendListUpdate)
                    {
                        _hasFriendListUpdate = false;
                    }
                    else if (notificationInfo.Type == NotificationEventType.NewFriendRequest)
                    {
                        _hasNewFriendRequest = false;
                    }

                    context.ResponseData.WriteStruct(notificationInfo);

                    return 0;
                }
            }

            return MakeError(ErrorModule.Friends, FriendError.NotificationQueueEmpty);
        }

        public void SignalFriendListUpdate(UInt128 targetId)
        {
            lock (_lock)
            {
                if (_userId == targetId)
                {
                    if (!_hasFriendListUpdate)
                    {
                        NotificationInfo friendListNotification = new NotificationInfo();

                        if (_notifications.Count != 0)
                        {
                            friendListNotification = _notifications.First.Value;
                            _notifications.RemoveFirst();
                        }

                        friendListNotification.Type = NotificationEventType.FriendListUpdate;
                        _hasFriendListUpdate = true;

                        if (_hasNewFriendRequest)
                        {
                            NotificationInfo newFriendRequestNotification = new NotificationInfo();

                            if (_notifications.Count != 0)
                            {
                                newFriendRequestNotification = _notifications.First.Value;
                                _notifications.RemoveFirst();
                            }

                            newFriendRequestNotification.Type = NotificationEventType.NewFriendRequest;
                            _notifications.AddFirst(newFriendRequestNotification);
                        }

                        // We defer this to make sure we are on top of the queue.
                        _notifications.AddFirst(friendListNotification);
                    }

                    _notificationEvent.ReadableEvent.Signal();
                }
            }
        }

        public void SignalNewFriendRequest(UInt128 targetId)
        {
            lock (_lock)
            {
                if ((_permissionLevel & FriendServicePermissionLevel.OverlayMask) != 0 && _userId == targetId)
                {
                    if (!_hasNewFriendRequest)
                    {
                        if (_notifications.Count == 100)
                        {
                            SignalFriendListUpdate(targetId);
                        }

                        NotificationInfo newFriendRequestNotification = new NotificationInfo
                        {
                            Type = NotificationEventType.NewFriendRequest
                        };

                        _notifications.AddLast(newFriendRequestNotification);
                        _hasNewFriendRequest = true;
                    }

                    _notificationEvent.ReadableEvent.Signal();
                }
            }
        }

        public void Dispose()
        {
            NotificationEventHandler.Instance.UnregisterNotificationService(this);
        }
    }
}