using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid.Irs
{
    [Service("irs")]
    class IIrSensorServer : IpcService
    {
        private int _irsensorSharedMemoryHandle = 0;

        public IIrSensorServer(ServiceCtx context) { }

        [Command(302)]
        // ActivateIrsensor(nn::applet::AppletResourceUserId, pid)
        public long ActivateIrsensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceIrs, new { appletResourceUserId });

            return 0;
        }

        [Command(303)]
        // DeactivateIrsensor(nn::applet::AppletResourceUserId, pid)
        public long DeactivateIrsensor(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceIrs, new { appletResourceUserId });

            return 0;
        }

        [Command(304)]
        // GetIrsensorSharedMemoryHandle(nn::applet::AppletResourceUserId, pid) -> handle<copy>
        public long GetIrsensorSharedMemoryHandle(ServiceCtx context)
        {
            if (_irsensorSharedMemoryHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.System.IirsSharedMem, out _irsensorSharedMemoryHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_irsensorSharedMemoryHandle);

            return 0;
        }

        [Command(311)]
        // GetNpadIrCameraHandle(u32) -> nn::irsensor::IrCameraHandle
        public long GetNpadIrCameraHandle(ServiceCtx context)
        {
            NpadIdType npadIdType = (NpadIdType)context.RequestData.ReadUInt32();

            if (npadIdType >  NpadIdType.Player8 && 
                npadIdType != NpadIdType.Unknown && 
                npadIdType != NpadIdType.Handheld)
            {
                return ErrorCode.MakeError(ErrorModule.Irsensor, IrsError.NpadIdOutOfRange);
            }

            HidControllerId irCameraHandle = HidUtils.GetIndexFromNpadIdType(npadIdType);

            context.ResponseData.Write((int)irCameraHandle);

            // NOTE: If the irCameraHandle pointer is null this error is returned, Doesn't occur in our case. 
            //       return ErrorCode.MakeError(ErrorModule.Irsensor, IrsError.HandlePointerIsNull);

            return 0;
        }

        [Command(319)] // 4.0.0+
        // ActivateIrsensorWithFunctionLevel(nn::applet::AppletResourceUserId, nn::irsensor::PackedFunctionLevel, pid)
        public long ActivateIrsensorWithFunctionLevel(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            long packedFunctionLevel  = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceIrs, new { appletResourceUserId, packedFunctionLevel });

            return 0;
        }
    }
}