using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Hid.HidServer;
using Ryujinx.HLE.HOS.Services.Nfc.Nfp.UserManager;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    class IUser : IpcService
    {
        private State _state = State.NonInitialized;

        private KEvent _availabilityChangeEvent;
        private int    _availabilityChangeEventHandle = 0;

        private List<Device> _devices = new List<Device>();

        public IUser() { }

        [Command(0)]
        // Initialize(u64, u64, pid, buffer<unknown, 5>)
        public ResultCode Initialize(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();
            long mcuVersionData       = context.RequestData.ReadInt64();

            long inputPosition = context.Request.SendBuff[0].Position;
            long inputSize     = context.Request.SendBuff[0].Size;

            byte[] unknownBuffer = new byte[inputSize];

            context.Memory.Read((ulong)inputPosition, unknownBuffer);

            // NOTE: appletResourceUserId, mcuVersionData and the buffer are stored inside an internal struct.
            //       The buffer seems to contains entries with a size of 0x40 bytes each.
            //       Sadly, this internal struct doesn't seems to be used in retail.

            // TODO: Add an instance of nn::nfc::server::Manager when it will be implemented.
            //       Add an instance of nn::nfc::server::SaveData when it will be implemented.

            // TODO: When we will be able to add multiple controllers add one entry by controller here.
            Device device1 = new Device
            {
                NpadIdType = NpadIdType.Player1,
                Handle     = HidUtils.GetIndexFromNpadIdType(NpadIdType.Player1),
                State      = DeviceState.Initialized
            };

            _devices.Add(device1);

            _state = State.Initialized;

            return ResultCode.Success;
        }

        [Command(1)]
        // Finalize()
        public ResultCode Finalize(ServiceCtx context)
        {
            // TODO: Call StopDetection() and Unmount() when they will be implemented.
            //       Remove the instance of nn::nfc::server::Manager when it will be implemented.
            //       Remove the instance of nn::nfc::server::SaveData when it will be implemented.

            _devices.Clear();

            _state = State.NonInitialized;

            return ResultCode.Success;
        }

        [Command(2)]
        // ListDevices() -> (u32, buffer<unknown, 0xa>)
        public ResultCode ListDevices(ServiceCtx context)
        {
            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.DevicesBufferIsNull;
            }

            long outputPosition = context.Request.RecvListBuff[0].Position;
            long outputSize     = context.Request.RecvListBuff[0].Size;

            if (_devices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            for (int i = 0; i < _devices.Count; i++)
            {
                context.Memory.Write((ulong)(outputPosition + (i * sizeof(long))), (uint)_devices[i].Handle);
            }

            context.ResponseData.Write(_devices.Count);

            return ResultCode.Success;
        }

        [Command(3)]
        // StartDetection(bytes<8, 4>)
        public ResultCode StartDetection(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(4)]
        // StopDetection(bytes<8, 4>)
        public ResultCode StopDetection(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(5)]
        // Mount(bytes<8, 4>, u32, u32)
        public ResultCode Mount(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(6)]
        // Unmount(bytes<8, 4>)
        public ResultCode Unmount(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(7)]
        // OpenApplicationArea(bytes<8, 4>, u32)
        public ResultCode OpenApplicationArea(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(8)]
        // GetApplicationArea(bytes<8, 4>) -> (u32, buffer<unknown, 6>)
        public ResultCode GetApplicationArea(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(9)]
        // SetApplicationArea(bytes<8, 4>, buffer<unknown, 5>)
        public ResultCode SetApplicationArea(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(10)]
        // Flush(bytes<8, 4>)
        public ResultCode Flush(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(11)]
        // Restore(bytes<8, 4>)
        public ResultCode Restore(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(12)]
        // CreateApplicationArea(bytes<8, 4>, u32, buffer<unknown, 5>)
        public ResultCode CreateApplicationArea(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(13)]
        // GetTagInfo(bytes<8, 4>) -> buffer<unknown<0x58>, 0x1a>
        public ResultCode GetTagInfo(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(14)]
        // GetRegisterInfo(bytes<8, 4>) -> buffer<unknown<0x100>, 0x1a>
        public ResultCode GetRegisterInfo(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(15)]
        // GetCommonInfo(bytes<8, 4>) -> buffer<unknown<0x40>, 0x1a>
        public ResultCode GetCommonInfo(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(16)]
        // GetModelInfo(bytes<8, 4>) -> buffer<unknown<0x40>, 0x1a>
        public ResultCode GetModelInfo(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(17)]
        // AttachActivateEvent(bytes<8, 4>) -> handle<copy>
        public ResultCode AttachActivateEvent(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < _devices.Count; i++)
            {
                if ((uint)_devices[i].Handle == deviceHandle)
                {
                    if (_devices[i].ActivateEventHandle == 0)
                    {
                        _devices[i].ActivateEvent = new KEvent(context.Device.System.KernelContext);

                        if (context.Process.HandleTable.GenerateHandle(_devices[i].ActivateEvent.ReadableEvent, out _devices[i].ActivateEventHandle) != KernelResult.Success)
                        {
                            throw new InvalidOperationException("Out of handles!");
                        }
                    }

                    context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_devices[i].ActivateEventHandle);

                    return ResultCode.Success;
                }
            }

            return ResultCode.DeviceNotFound;
        }

        [Command(18)]
        // AttachDeactivateEvent(bytes<8, 4>) -> handle<copy>
        public ResultCode AttachDeactivateEvent(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < _devices.Count; i++)
            {
                if ((uint)_devices[i].Handle == deviceHandle)
                {
                    if (_devices[i].DeactivateEventHandle == 0)
                    {
                        _devices[i].DeactivateEvent = new KEvent(context.Device.System.KernelContext);

                        if (context.Process.HandleTable.GenerateHandle(_devices[i].DeactivateEvent.ReadableEvent, out _devices[i].DeactivateEventHandle) != KernelResult.Success)
                        {
                            throw new InvalidOperationException("Out of handles!");
                        }
                    }

                    context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_devices[i].DeactivateEventHandle);

                    return ResultCode.Success;
                }
            }

            return ResultCode.DeviceNotFound;
        }

        [Command(19)]
        // GetState() -> u32
        public ResultCode GetState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_state);

            return ResultCode.Success;
        }

        [Command(20)]
        // GetDeviceState(bytes<8, 4>) -> u32
        public ResultCode GetDeviceState(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < _devices.Count; i++)
            {
                if ((uint)_devices[i].Handle == deviceHandle)
                {
                    context.ResponseData.Write((uint)_devices[i].State);

                    return ResultCode.Success;
                }
            }

            context.ResponseData.Write((uint)DeviceState.Unavailable);

            return ResultCode.DeviceNotFound;
        }

        [Command(21)]
        // GetNpadId(bytes<8, 4>) -> u32
        public ResultCode GetNpadId(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < _devices.Count; i++)
            {
                if ((uint)_devices[i].Handle == deviceHandle)
                {
                    context.ResponseData.Write((uint)HidUtils.GetNpadIdTypeFromIndex(_devices[i].Handle));

                    return ResultCode.Success;
                }
            }

            return ResultCode.DeviceNotFound;
        }

        [Command(22)]
        // GetApplicationAreaSize(bytes<8, 4>) -> u32
        public ResultCode GetApplicationAreaSize(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(23)] // 3.0.0+
        // AttachAvailabilityChangeEvent() -> handle<copy>
        public ResultCode AttachAvailabilityChangeEvent(ServiceCtx context)
        {
            if (_availabilityChangeEventHandle == 0)
            {
                _availabilityChangeEvent = new KEvent(context.Device.System.KernelContext);

                if (context.Process.HandleTable.GenerateHandle(_availabilityChangeEvent.ReadableEvent, out _availabilityChangeEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_availabilityChangeEventHandle);

            return ResultCode.Success;
        }

        [Command(24)] // 3.0.0+
        // RecreateApplicationArea(bytes<8, 4>, u32, buffer<unknown, 5>)
        public ResultCode RecreateApplicationArea(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context);
        }
    }
}