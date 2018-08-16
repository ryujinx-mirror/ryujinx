using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ICommonStateGetter : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent DisplayResolutionChangeEvent;

        public ICommonStateGetter()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
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

            DisplayResolutionChangeEvent = new KEvent();
        }

        public long GetEventHandle(ServiceCtx Context)
        {
            KEvent Event = Context.Process.AppletState.MessageEvent;

            int Handle = Context.Process.HandleTable.OpenHandle(Event);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long ReceiveMessage(ServiceCtx Context)
        {
            if (!Context.Process.AppletState.TryDequeueMessage(out MessageInfo Message))
            {
                return MakeError(ErrorModule.Am, AmErr.NoMessages);
            }

            Context.ResponseData.Write((int)Message);

            return 0;
        }

        public long GetOperationMode(ServiceCtx Context)
        {
            OperationMode Mode = Context.Device.System.State.DockedMode
                ? OperationMode.Docked
                : OperationMode.Handheld;

            Context.ResponseData.Write((byte)Mode);

            return 0;
        }

        public long GetPerformanceMode(ServiceCtx Context)
        {
            Apm.PerformanceMode Mode = Context.Device.System.State.DockedMode
                ? Apm.PerformanceMode.Docked
                : Apm.PerformanceMode.Handheld;

            Context.ResponseData.Write((int)Mode);

            return 0;
        }

        public long GetBootMode(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)0); //Unknown value.

            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetCurrentFocusState(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)Context.Process.AppletState.FocusState);

            return 0;
        }

        public long GetDefaultDisplayResolution(ServiceCtx Context)
        {
            Context.ResponseData.Write(1280);
            Context.ResponseData.Write(720);

            return 0;
        }

        public long GetDefaultDisplayResolutionChangeEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(DisplayResolutionChangeEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
