using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Services.BluetoothManager.BtmUser
{
    class IBtmUserCore : IpcService
    {
        public KEvent _bleScanEvent;
        public int _bleScanEventHandle;

        public KEvent _bleConnectionEvent;
        public int _bleConnectionEventHandle;

        public KEvent _bleServiceDiscoveryEvent;
        public int _bleServiceDiscoveryEventHandle;

        public KEvent _bleMtuConfigEvent;
        public int _bleMtuConfigEventHandle;

        public IBtmUserCore() { }

        [CommandCmif(0)] // 5.0.0+
        // AcquireBleScanEvent() -> (byte<1>, handle<copy>)
        public ResultCode AcquireBleScanEvent(ServiceCtx context)
        {
            Result result = Result.Success;

            if (_bleScanEventHandle == 0)
            {
                _bleScanEvent = new KEvent(context.Device.System.KernelContext);

                result = context.Process.HandleTable.GenerateHandle(_bleScanEvent.ReadableEvent, out _bleScanEventHandle);

                if (result != Result.Success)
                {
                    // NOTE: We use a Logging instead of an exception because the call return a boolean if succeed or not.
                    Logger.Error?.Print(LogClass.ServiceBsd, "Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_bleScanEventHandle);

            context.ResponseData.Write(result == Result.Success ? 1 : 0);

            return ResultCode.Success;
        }

        [CommandCmif(17)] // 5.0.0+
        // AcquireBleConnectionEvent() -> (byte<1>, handle<copy>)
        public ResultCode AcquireBleConnectionEvent(ServiceCtx context)
        {
            Result result = Result.Success;

            if (_bleConnectionEventHandle == 0)
            {
                _bleConnectionEvent = new KEvent(context.Device.System.KernelContext);

                result = context.Process.HandleTable.GenerateHandle(_bleConnectionEvent.ReadableEvent, out _bleConnectionEventHandle);

                if (result != Result.Success)
                {
                    // NOTE: We use a Logging instead of an exception because the call return a boolean if succeed or not.
                    Logger.Error?.Print(LogClass.ServiceBsd, "Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_bleConnectionEventHandle);

            context.ResponseData.Write(result == Result.Success ? 1 : 0);

            return ResultCode.Success;
        }

        [CommandCmif(26)] // 5.0.0+
        // AcquireBleServiceDiscoveryEvent() -> (byte<1>, handle<copy>)
        public ResultCode AcquireBleServiceDiscoveryEvent(ServiceCtx context)
        {
            Result result = Result.Success;

            if (_bleServiceDiscoveryEventHandle == 0)
            {
                _bleServiceDiscoveryEvent = new KEvent(context.Device.System.KernelContext);

                result = context.Process.HandleTable.GenerateHandle(_bleServiceDiscoveryEvent.ReadableEvent, out _bleServiceDiscoveryEventHandle);

                if (result != Result.Success)
                {
                    // NOTE: We use a Logging instead of an exception because the call return a boolean if succeed or not.
                    Logger.Error?.Print(LogClass.ServiceBsd, "Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_bleServiceDiscoveryEventHandle);

            context.ResponseData.Write(result == Result.Success ? 1 : 0);

            return ResultCode.Success;
        }

        [CommandCmif(33)] // 5.0.0+
        // AcquireBleMtuConfigEvent() -> (byte<1>, handle<copy>)
        public ResultCode AcquireBleMtuConfigEvent(ServiceCtx context)
        {
            Result result = Result.Success;

            if (_bleMtuConfigEventHandle == 0)
            {
                _bleMtuConfigEvent = new KEvent(context.Device.System.KernelContext);

                result = context.Process.HandleTable.GenerateHandle(_bleMtuConfigEvent.ReadableEvent, out _bleMtuConfigEventHandle);

                if (result != Result.Success)
                {
                    // NOTE: We use a Logging instead of an exception because the call return a boolean if succeed or not.
                    Logger.Error?.Print(LogClass.ServiceBsd, "Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_bleMtuConfigEventHandle);

            context.ResponseData.Write(result == Result.Success ? 1 : 0);

            return ResultCode.Success;
        }
    }
}
