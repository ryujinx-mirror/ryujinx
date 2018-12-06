using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ICommonStateGetter : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KEvent _displayResolutionChangeEvent;

        public ICommonStateGetter(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  GetEventHandle                          },
                { 1,  ReceiveMessage                          },
                { 5,  GetOperationMode                        },
                { 6,  GetPerformanceMode                      },
                { 8,  GetBootMode                             },
                { 9,  GetCurrentFocusState                    },
                { 60, GetDefaultDisplayResolution             },
                { 61, GetDefaultDisplayResolutionChangeEvent  }
            };

            _displayResolutionChangeEvent = new KEvent(system);
        }

        public long GetEventHandle(ServiceCtx context)
        {
            KEvent Event = context.Device.System.AppletState.MessageEvent;

            if (context.Process.HandleTable.GenerateHandle(Event.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        public long ReceiveMessage(ServiceCtx context)
        {
            if (!context.Device.System.AppletState.TryDequeueMessage(out MessageInfo message))
            {
                return MakeError(ErrorModule.Am, AmErr.NoMessages);
            }

            context.ResponseData.Write((int)message);

            return 0;
        }

        public long GetOperationMode(ServiceCtx context)
        {
            OperationMode mode = context.Device.System.State.DockedMode
                ? OperationMode.Docked
                : OperationMode.Handheld;

            context.ResponseData.Write((byte)mode);

            return 0;
        }

        public long GetPerformanceMode(ServiceCtx context)
        {
            Apm.PerformanceMode mode = context.Device.System.State.DockedMode
                ? Apm.PerformanceMode.Docked
                : Apm.PerformanceMode.Handheld;

            context.ResponseData.Write((int)mode);

            return 0;
        }

        public long GetBootMode(ServiceCtx context)
        {
            context.ResponseData.Write((byte)0); //Unknown value.

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetCurrentFocusState(ServiceCtx context)
        {
            context.ResponseData.Write((byte)context.Device.System.AppletState.FocusState);

            return 0;
        }

        public long GetDefaultDisplayResolution(ServiceCtx context)
        {
            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);

            return 0;
        }

        public long GetDefaultDisplayResolutionChangeEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_displayResolutionChangeEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
