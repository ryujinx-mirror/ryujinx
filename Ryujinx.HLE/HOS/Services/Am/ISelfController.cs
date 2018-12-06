using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ISelfController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KEvent _launchableEvent;

        private int _idleTimeDetectionExtension;

        public ISelfController(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  Exit                                  },
                { 1,  LockExit                              },
                { 2,  UnlockExit                            },
                { 9,  GetLibraryAppletLaunchableEvent       },
                { 10, SetScreenShotPermission               },
                { 11, SetOperationModeChangedNotification   },
                { 12, SetPerformanceModeChangedNotification },
                { 13, SetFocusHandlingMode                  },
                { 14, SetRestartMessageEnabled              },
                { 16, SetOutOfFocusSuspendingEnabled        },
                { 19, SetScreenShotImageOrientation         },
                { 50, SetHandlesRequestToDisplay            },
                { 62, SetIdleTimeDetectionExtension         },
                { 63, GetIdleTimeDetectionExtension         }
            };

            _launchableEvent = new KEvent(system);
        }

        public long Exit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long LockExit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long UnlockExit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetLibraryAppletLaunchableEvent(ServiceCtx context)
        {
            _launchableEvent.ReadableEvent.Signal();

            if (context.Process.HandleTable.GenerateHandle(_launchableEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetScreenShotPermission(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetOperationModeChangedNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetPerformanceModeChangedNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetFocusHandlingMode(ServiceCtx context)
        {
            bool flag1 = context.RequestData.ReadByte() != 0;
            bool flag2 = context.RequestData.ReadByte() != 0;
            bool flag3 = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetRestartMessageEnabled(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetOutOfFocusSuspendingEnabled(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetScreenShotImageOrientation(ServiceCtx context)
        {
            int orientation = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetHandlesRequestToDisplay(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadByte() != 0;

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        // SetIdleTimeDetectionExtension(u32)
        public long SetIdleTimeDetectionExtension(ServiceCtx context)
        {
            _idleTimeDetectionExtension = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm, $"Stubbed. IdleTimeDetectionExtension: {_idleTimeDetectionExtension}");

            return 0;
        }

        // GetIdleTimeDetectionExtension() -> u32
        public long GetIdleTimeDetectionExtension(ServiceCtx context)
        {
            context.ResponseData.Write(_idleTimeDetectionExtension);

            Logger.PrintStub(LogClass.ServiceAm, $"Stubbed. IdleTimeDetectionExtension: {_idleTimeDetectionExtension}");

            return 0;
        }
    }
}
