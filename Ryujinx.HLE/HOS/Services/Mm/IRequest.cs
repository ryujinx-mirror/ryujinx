using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Mm
{
    class IRequest : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IRequest()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 1, InitializeOld },
                { 4, Initialize    },
                { 5, Finalize      },
                { 6, SetAndWait    },
                { 7, Get           }
            };
        }

        // InitializeOld(u32, u32, u32)
        public long InitializeOld(ServiceCtx context)
        {
            int unknown0 = context.RequestData.ReadInt32();
            int unknown1 = context.RequestData.ReadInt32();
            int unknown2 = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceMm, "Stubbed.");

            return 0;
        }

        public long Initialize(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceMm, "Stubbed.");

            return 0;
        }

        public long Finalize(ServiceCtx context)
        {
            context.Device.Gpu.UninitializeVideoDecoder();

            Logger.PrintStub(LogClass.ServiceMm, "Stubbed.");

            return 0;
        }

        public long SetAndWait(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceMm, "Stubbed.");

            return 0;
        }

        public long Get(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceMm, "Stubbed.");

            return 0;
        }
    }
}