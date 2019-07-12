using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class ILibraryAppletAccessor : IpcService
    {
        private KEvent _stateChangedEvent;

        public ILibraryAppletAccessor(Horizon system)
        {
            _stateChangedEvent = new KEvent(system);
        }

        [Command(0)]
        // GetAppletStateChangedEvent() -> handle<copy>
        public long GetAppletStateChangedEvent(ServiceCtx context)
        {
            _stateChangedEvent.ReadableEvent.Signal();

            if (context.Process.HandleTable.GenerateHandle(_stateChangedEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(10)]
        // Start()
        public long Start(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(30)]
        // GetResult()
        public long GetResult(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(100)]
        // PushInData(object<nn::am::service::IStorage>)
        public long PushInData(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        [Command(101)]
        // PopOutData() -> object<nn::am::service::IStorage>
        public long PopOutData(ServiceCtx context)
        {
            MakeObject(context, new IStorage(StorageHelper.MakeLaunchParams()));

            return 0;
        }
    }
}