using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.NotificationService;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator
{
    class INotificationService : IpcService, IDisposable
    {
        private readonly UserId                       _userId;
        private readonly FriendServicePermissionLevel _permissionLevel;

        private readonly object _lock = new object();

        private KEvent _notificationEvent;
        private int    _notificationEventHandle = 0;

        private LinkedList<NotificationInfo> _notifications;

        private bool _hasNewFriendRequest;
        private bool _hasFriendListUpdate;

        public INotificationService(ServiceCtx context, UserId userId, FriendServicePermissionLevel permissionLevel)
        {
            _userId            = userId;
            _permissionLevel   = permissionLevel;
            _notifications     = new LinkedList<NotificationInfo>();
            _notificationEvent = new KEvent(context.Device.System.KernelContext);

            _hasNewFriendRequest = false;
            _hasFriendListUpdate = false;

            NotificationEventHandler.Instance.RegisterNotificationService(this);
        }

        [Command(0)] //2.0.0+
        // nn::friends::detail::ipc::INotificationService::GetEvent() -> handle<copy>
        public ResultCode GetEvent(ServiceCtx context)
        {
            if (_notificationEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_notificationEvent.ReadableEvent, out _notificationEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_notificationEventHandle);

            return ResultCode.Success;
        }

        [Command(1)] //2.0.0+
        // nn::friends::detail::ipc::INotificationService::Clear()
        public ResultCode Clear(ServiceCtx context)
        {
            lock (_lock)
            {
                _hasNewFriendRequest = false;
                _hasFriendListUpdate = false;

                _notifications.Clear();
            }

            return ResultCode.Success;
        }

        [Command(2)] // 2.0.0+
        // nn::friends::detail::ipc::INotificationService::Pop() -> nn::friends::detail::ipc::SizedNotificationInfo
        public ResultCode Pop(ServiceCtx context)
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

                    return ResultCode.Success;
                }
            }

            return ResultCode.NotificationQueueEmpty;
        }

        public void SignalFriendListUpdate(UserId targetId)
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

        public void SignalNewFriendRequest(UserId targetId)
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