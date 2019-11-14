using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletCreator
{
    class ILibraryAppletAccessor : IpcService
    {
        private IApplet _applet;

        private AppletFifo<byte[]> _inData;
        private AppletFifo<byte[]> _outData;

        private KEvent _stateChangedEvent;

        public ILibraryAppletAccessor(AppletId appletId, Horizon system)
        {
            _stateChangedEvent = new KEvent(system);

            _applet  = AppletManager.Create(appletId, system);
            _inData  = new AppletFifo<byte[]>();
            _outData = new AppletFifo<byte[]>();
            
            _applet.AppletStateChanged += OnAppletStateChanged;
            
            Logger.PrintInfo(LogClass.ServiceAm, $"Applet '{appletId}' created.");
        }

        private void OnAppletStateChanged(object sender, EventArgs e)
        {
            _stateChangedEvent.ReadableEvent.Signal();
        }

        [Command(0)]
        // GetAppletStateChangedEvent() -> handle<copy>
        public ResultCode GetAppletStateChangedEvent(ServiceCtx context)
        {
            _stateChangedEvent.ReadableEvent.Signal();

            if (context.Process.HandleTable.GenerateHandle(_stateChangedEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return ResultCode.Success;
        }

        [Command(10)]
        // Start()
        public ResultCode Start(ServiceCtx context)
        {
            return (ResultCode)_applet.Start(_inData, _outData);
        }

        [Command(30)]
        // GetResult()
        public ResultCode GetResult(ServiceCtx context)
        {
            return (ResultCode)_applet.GetResult();
        }

        [Command(100)]
        // PushInData(object<nn::am::service::IStorage>)
        public ResultCode PushInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            _inData.Push(data.Data);

            return ResultCode.Success;
        }

        [Command(101)]
        // PopOutData() -> object<nn::am::service::IStorage>
        public ResultCode PopOutData(ServiceCtx context)
        {
            byte[] data = _outData.Pop();

            MakeObject(context, new IStorage(data));
            
            return ResultCode.Success;
        }
    }
}
