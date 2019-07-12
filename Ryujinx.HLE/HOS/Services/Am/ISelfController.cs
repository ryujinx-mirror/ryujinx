using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ISelfController : IpcService
    {
        private KEvent _libraryAppletLaunchableEvent;

        private KEvent _accumulatedSuspendedTickChangedEvent;
        private int    _accumulatedSuspendedTickChangedEventHandle = 0;

        private int _idleTimeDetectionExtension;

        public ISelfController(Horizon system)
        {
            _libraryAppletLaunchableEvent = new KEvent(system);
        }

        [Command(0)]
        // Exit()
        public long Exit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(1)]
        // LockExit()
        public long LockExit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(2)]
        // UnlockExit()
        public long UnlockExit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(9)]
        // GetLibraryAppletLaunchableEvent() -> handle<copy>
        public long GetLibraryAppletLaunchableEvent(ServiceCtx context)
        {
            _libraryAppletLaunchableEvent.ReadableEvent.Signal();

            if (context.Process.HandleTable.GenerateHandle(_libraryAppletLaunchableEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(10)]
        // SetScreenShotPermission(u32)
        public long SetScreenShotPermission(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(11)]
        // SetOperationModeChangedNotification(b8)
        public long SetOperationModeChangedNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(12)]
        // SetPerformanceModeChangedNotification(b8)
        public long SetPerformanceModeChangedNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(13)]
        // SetFocusHandlingMode(b8, b8, b8)
        public long SetFocusHandlingMode(ServiceCtx context)
        {
            bool flag1 = context.RequestData.ReadByte() != 0;
            bool flag2 = context.RequestData.ReadByte() != 0;
            bool flag3 = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(14)]
        // SetRestartMessageEnabled(b8)
        public long SetRestartMessageEnabled(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(16)] // 2.0.0+
        // SetOutOfFocusSuspendingEnabled(b8)
        public long SetOutOfFocusSuspendingEnabled(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(19)] // 3.0.0+
        public long SetScreenShotImageOrientation(ServiceCtx context)
        {
            int orientation = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(50)]
        // SetHandlesRequestToDisplay(b8)
        public long SetHandlesRequestToDisplay(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(62)]
        // SetIdleTimeDetectionExtension(u32)
        public long SetIdleTimeDetectionExtension(ServiceCtx context)
        {
            _idleTimeDetectionExtension = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm, new { _idleTimeDetectionExtension });

            return 0;
        }

        [Command(63)]
        // GetIdleTimeDetectionExtension() -> u32
        public long GetIdleTimeDetectionExtension(ServiceCtx context)
        {
            context.ResponseData.Write(_idleTimeDetectionExtension);

            Logger.PrintStub(LogClass.ServiceAm, new { _idleTimeDetectionExtension });

            return 0;
        }

        [Command(91)] // 6.0.0+
        // GetAccumulatedSuspendedTickChangedEvent() -> handle<copy>
        public long GetAccumulatedSuspendedTickChangedEvent(ServiceCtx context)
        {
            if (_accumulatedSuspendedTickChangedEventHandle == 0)
            {
                _accumulatedSuspendedTickChangedEvent = new KEvent(context.Device.System);

                _accumulatedSuspendedTickChangedEvent.ReadableEvent.Signal();

                if (context.Process.HandleTable.GenerateHandle(_accumulatedSuspendedTickChangedEvent.ReadableEvent, out _accumulatedSuspendedTickChangedEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_accumulatedSuspendedTickChangedEventHandle);

            return 0;
        }
    }
}