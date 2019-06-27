using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class INotificationService : IpcService
    {
        private UInt128 _userId;

        private KEvent _notificationEvent;
        private int    _notificationEventHandle = 0;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public INotificationService(UInt128 userId)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetEvent }, // 2.0.0+
              //{ 1, Clear    }, // 2.0.0+
              //{ 2, Pop      }, // 2.0.0+
            };

            _userId = userId;
        }

        public long GetEvent(ServiceCtx context)
        {
            if (_notificationEventHandle == 0)
            {
                _notificationEvent = new KEvent(context.Device.System);

                if (context.Process.HandleTable.GenerateHandle(_notificationEvent.ReadableEvent, out _notificationEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_notificationEventHandle);

            return 0;
        }
    }
}