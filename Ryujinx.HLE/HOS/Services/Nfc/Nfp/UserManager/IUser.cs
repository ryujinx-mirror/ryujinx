using Ryujinx.Common.Memory;
using Ryujinx.Cpu;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Hid.HidServer;
using Ryujinx.HLE.HOS.Services.Nfc.Nfp.UserManager;
using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    class IUser : IpcService
    {
        private ulong  _appletResourceUserId;
        private ulong  _mcuVersionData;
        private byte[] _mcuData;

        private State _state = State.NonInitialized;

        private KEvent _availabilityChangeEvent;

        private CancellationTokenSource _cancelTokenSource;

        public IUser() { }

        [CommandHipc(0)]
        // Initialize(u64, u64, pid, buffer<unknown, 5>)
        public ResultCode Initialize(ServiceCtx context)
        {
            _appletResourceUserId = context.RequestData.ReadUInt64();
            _mcuVersionData       = context.RequestData.ReadUInt64();

            ulong inputPosition = context.Request.SendBuff[0].Position;
            ulong inputSize     = context.Request.SendBuff[0].Size;

            _mcuData = new byte[inputSize];

            context.Memory.Read(inputPosition, _mcuData);

            // TODO: The mcuData buffer seems to contains entries with a size of 0x40 bytes each. Usage of the data needs to be determined.

            // TODO: Handle this in a controller class directly.
            //       Every functions which use the Handle call nn::hid::system::GetXcdHandleForNpadWithNfc().
            NfpDevice devicePlayer1 = new NfpDevice
            {
                NpadIdType = NpadIdType.Player1,
                Handle     = HidUtils.GetIndexFromNpadIdType(NpadIdType.Player1),
                State      = NfpDeviceState.Initialized
            };

            context.Device.System.NfpDevices.Add(devicePlayer1);

            // TODO: It mounts 0x8000000000000020 save data and stores a random generate value inside. Usage of the data needs to be determined.

            _state = State.Initialized;

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // Finalize()
        public ResultCode Finalize(ServiceCtx context)
        {
            if (_state == State.Initialized)
            {
                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Cancel();
                }

                // NOTE: All events are destroyed here.
                context.Device.System.NfpDevices.Clear();

                _state = State.NonInitialized;
            }

            return ResultCode.Success;
        }

        [CommandHipc(2)]
        // ListDevices() -> (u32, buffer<unknown, 0xa>)
        public ResultCode ListDevices(ServiceCtx context)
        {
            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.WrongArgument;
            }

            ulong outputPosition = context.Request.RecvListBuff[0].Position;
            ulong outputSize      = context.Request.RecvListBuff[0].Size;

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            if (CheckNfcIsEnabled() == ResultCode.Success)
            {
                for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
                {
                    context.Memory.Write(outputPosition + ((uint)i * sizeof(long)), (uint)context.Device.System.NfpDevices[i].Handle);
                }

                context.ResponseData.Write(context.Device.System.NfpDevices.Count);
            }
            else
            {
                context.ResponseData.Write(0);
            }

            return ResultCode.Success;
        }

        [CommandHipc(3)]
        // StartDetection(bytes<8, 4>)
        public ResultCode StartDetection(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    context.Device.System.NfpDevices[i].State = NfpDeviceState.SearchingForTag;

                    break;
                }
            }

            _cancelTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                {
                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagFound)
                        {
                            context.Device.System.NfpDevices[i].SignalActivate();
                            Thread.Sleep(50); // NOTE: Simulate amiibo scanning delay.
                            context.Device.System.NfpDevices[i].SignalDeactivate();

                            break;
                        }
                    }
                }
            }, _cancelTokenSource.Token);

            return ResultCode.Success;
        }

        [CommandHipc(4)]
        // StopDetection(bytes<8, 4>)
        public ResultCode StopDetection(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    context.Device.System.NfpDevices[i].State = NfpDeviceState.Initialized;

                    break;
                }
            }

            return ResultCode.Success;
        }

        [CommandHipc(5)]
        // Mount(bytes<8, 4>, u32, u32)
        public ResultCode Mount(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint                   deviceHandle = (uint)context.RequestData.ReadUInt64();
            UserManager.DeviceType deviceType   = (UserManager.DeviceType)context.RequestData.ReadUInt32();
            MountTarget            mountTarget  = (MountTarget)context.RequestData.ReadUInt32();

            if (deviceType != 0)
            {
                return ResultCode.WrongArgument;
            }

            if (((uint)mountTarget & 3) == 0)
            {
                return ResultCode.WrongArgument;
            }

            // TODO: Found how the MountTarget is handled.

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagFound)
                        {
                            // NOTE: This mount the amiibo data, which isn't needed in our case.

                            context.Device.System.NfpDevices[i].State = NfpDeviceState.TagMounted;

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(6)]
        // Unmount(bytes<8, 4>)
        public ResultCode Unmount(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        // NOTE: This mount the amiibo data, which isn't needed in our case.

                        context.Device.System.NfpDevices[i].State = NfpDeviceState.TagFound;

                        resultCode = ResultCode.Success;
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(7)]
        // OpenApplicationArea(bytes<8, 4>, u32)
        public ResultCode OpenApplicationArea(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            uint applicationAreaId = context.RequestData.ReadUInt32();

            bool isOpened = false;

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            isOpened = VirtualAmiibo.OpenApplicationArea(context.Device.System.NfpDevices[i].AmiiboId, applicationAreaId);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            if (!isOpened)
            {
                resultCode = ResultCode.ApplicationAreaIsNull;
            }

            return resultCode;
        }

        [CommandHipc(8)]
        // GetApplicationArea(bytes<8, 4>) -> (u32, buffer<unknown, 6>)
        public ResultCode GetApplicationArea(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize     = context.Request.ReceiveBuff[0].Size;

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            uint size = 0;

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            byte[] applicationArea = VirtualAmiibo.GetApplicationArea(context.Device.System.NfpDevices[i].AmiiboId);

                            context.Memory.Write(outputPosition, applicationArea);

                            size = (uint)applicationArea.Length;

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }
                }
            }

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            if (size == 0)
            {
                return ResultCode.ApplicationAreaIsNull;
            }

            context.ResponseData.Write(size);

            return ResultCode.Success;
        }

        [CommandHipc(9)]
        // SetApplicationArea(bytes<8, 4>, buffer<unknown, 5>)
        public ResultCode SetApplicationArea(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            ulong inputPosition = context.Request.SendBuff[0].Position;
            ulong inputSize     = context.Request.SendBuff[0].Size;

            byte[] applicationArea = new byte[inputSize];

            context.Memory.Read(inputPosition, applicationArea);

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            VirtualAmiibo.SetApplicationArea(context.Device.System.NfpDevices[i].AmiiboId, applicationArea);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(10)]
        // Flush(bytes<8, 4>)
        public ResultCode Flush(ServiceCtx context)
        {
            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            // NOTE: Since we handle amiibo through VirtualAmiibo, we don't have to flush anything in our case.

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // Restore(bytes<8, 4>)
        public ResultCode Restore(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(12)]
        // CreateApplicationArea(bytes<8, 4>, u32, buffer<unknown, 5>)
        public ResultCode CreateApplicationArea(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            uint applicationAreaId = context.RequestData.ReadUInt32();

            ulong inputPosition = context.Request.SendBuff[0].Position;
            ulong inputSize     = context.Request.SendBuff[0].Size;

            byte[] applicationArea = new byte[inputSize];

            context.Memory.Read(inputPosition, applicationArea);

            bool isCreated = false;

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            isCreated = VirtualAmiibo.CreateApplicationArea(context.Device.System.NfpDevices[i].AmiiboId, applicationAreaId, applicationArea);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            if (!isCreated)
            {
                resultCode = ResultCode.ApplicationAreaIsNull;
            }

            return resultCode;
        }

        [CommandHipc(13)]
        // GetTagInfo(bytes<8, 4>) -> buffer<unknown<0x58>, 0x1a>
        public ResultCode GetTagInfo(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.WrongArgument;
            }

            ulong outputPosition = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Marshal.SizeOf(typeof(TagInfo)));

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, Marshal.SizeOf(typeof(TagInfo)));

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted || context.Device.System.NfpDevices[i].State == NfpDeviceState.TagFound)
                        {
                            byte[] Uuid = VirtualAmiibo.GenerateUuid(context.Device.System.NfpDevices[i].AmiiboId, context.Device.System.NfpDevices[i].UseRandomUuid);

                            if (Uuid.Length > AmiiboConstants.UuidMaxLength)
                            {
                                throw new ArgumentOutOfRangeException();
                            }

                            TagInfo tagInfo = new TagInfo
                            {
                                UuidLength = (byte)Uuid.Length,
                                Reserved1  = new Array21<byte>(),
                                Protocol   = uint.MaxValue, // All Protocol
                                TagType    = uint.MaxValue, // All Type
                                Reserved2  = new Array6<byte>()
                            };

                            Uuid.CopyTo(tagInfo.Uuid.ToSpan());

                            context.Memory.Write(outputPosition, tagInfo);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(14)]
        // GetRegisterInfo(bytes<8, 4>) -> buffer<unknown<0x100>, 0x1a>
        public ResultCode GetRegisterInfo(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.WrongArgument;
            }

            ulong outputPosition = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Marshal.SizeOf(typeof(RegisterInfo)));

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, Marshal.SizeOf(typeof(RegisterInfo)));

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            RegisterInfo registerInfo = VirtualAmiibo.GetRegisterInfo(context.Device.System.NfpDevices[i].AmiiboId);

                            context.Memory.Write(outputPosition, registerInfo);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(15)]
        // GetCommonInfo(bytes<8, 4>) -> buffer<unknown<0x40>, 0x1a>
        public ResultCode GetCommonInfo(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.WrongArgument;
            }

            ulong outputPosition = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Marshal.SizeOf(typeof(CommonInfo)));

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, Marshal.SizeOf(typeof(CommonInfo)));

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            CommonInfo commonInfo = VirtualAmiibo.GetCommonInfo(context.Device.System.NfpDevices[i].AmiiboId);

                            context.Memory.Write(outputPosition, commonInfo);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(16)]
        // GetModelInfo(bytes<8, 4>) -> buffer<unknown<0x40>, 0x1a>
        public ResultCode GetModelInfo(ServiceCtx context)
        {
            ResultCode resultCode = CheckNfcIsEnabled();

            if (resultCode != ResultCode.Success)
            {
                return resultCode;
            }

            if (context.Request.RecvListBuff.Count == 0)
            {
                return ResultCode.WrongArgument;
            }

            ulong outputPosition = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Marshal.SizeOf(typeof(ModelInfo)));

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, Marshal.SizeOf(typeof(ModelInfo)));

            uint deviceHandle = (uint)context.RequestData.ReadUInt64();

            if (context.Device.System.NfpDevices.Count == 0)
            {
                return ResultCode.DeviceNotFound;
            }

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if (context.Device.System.NfpDevices[i].Handle == (PlayerIndex)deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagRemoved)
                    {
                        resultCode = ResultCode.TagNotFound;
                    }
                    else
                    {
                        if (context.Device.System.NfpDevices[i].State == NfpDeviceState.TagMounted)
                        {
                            ModelInfo modelInfo = new ModelInfo
                            {
                                Reserved = new Array57<byte>()
                            };

                            modelInfo.CharacterId      = BinaryPrimitives.ReverseEndianness(ushort.Parse(context.Device.System.NfpDevices[i].AmiiboId.Substring(0, 4), NumberStyles.HexNumber));
                            modelInfo.CharacterVariant = byte.Parse(context.Device.System.NfpDevices[i].AmiiboId.Substring(4, 2), NumberStyles.HexNumber);
                            modelInfo.Series           = byte.Parse(context.Device.System.NfpDevices[i].AmiiboId.Substring(12, 2), NumberStyles.HexNumber);
                            modelInfo.ModelNumber      = ushort.Parse(context.Device.System.NfpDevices[i].AmiiboId.Substring(8, 4), NumberStyles.HexNumber);
                            modelInfo.Type             = byte.Parse(context.Device.System.NfpDevices[i].AmiiboId.Substring(6, 2), NumberStyles.HexNumber);

                            context.Memory.Write(outputPosition, modelInfo);

                            resultCode = ResultCode.Success;
                        }
                        else
                        {
                            resultCode = ResultCode.WrongDeviceState;
                        }
                    }

                    break;
                }
            }

            return resultCode;
        }

        [CommandHipc(17)]
        // AttachActivateEvent(bytes<8, 4>) -> handle<copy>
        public ResultCode AttachActivateEvent(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if ((uint)context.Device.System.NfpDevices[i].Handle == deviceHandle)
                {
                    context.Device.System.NfpDevices[i].ActivateEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(context.Device.System.NfpDevices[i].ActivateEvent.ReadableEvent, out int activateEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }

                    context.Response.HandleDesc = IpcHandleDesc.MakeCopy(activateEventHandle);

                    return ResultCode.Success;
                }
            }

            return ResultCode.DeviceNotFound;
        }

        [CommandHipc(18)]
        // AttachDeactivateEvent(bytes<8, 4>) -> handle<copy>
        public ResultCode AttachDeactivateEvent(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if ((uint)context.Device.System.NfpDevices[i].Handle == deviceHandle)
                {
                    context.Device.System.NfpDevices[i].DeactivateEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(context.Device.System.NfpDevices[i].DeactivateEvent.ReadableEvent, out int deactivateEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }

                    context.Response.HandleDesc = IpcHandleDesc.MakeCopy(deactivateEventHandle);

                    return ResultCode.Success;
                }
            }

            return ResultCode.DeviceNotFound;
        }

        [CommandHipc(19)]
        // GetState() -> u32
        public ResultCode GetState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_state);

            return ResultCode.Success;
        }

        [CommandHipc(20)]
        // GetDeviceState(bytes<8, 4>) -> u32
        public ResultCode GetDeviceState(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if ((uint)context.Device.System.NfpDevices[i].Handle == deviceHandle)
                {
                    if (context.Device.System.NfpDevices[i].State > NfpDeviceState.Finalized)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    
                    context.ResponseData.Write((uint)context.Device.System.NfpDevices[i].State);

                    return ResultCode.Success;
                }
            }

            context.ResponseData.Write((uint)NfpDeviceState.Unavailable);

            return ResultCode.DeviceNotFound;
        }

        [CommandHipc(21)]
        // GetNpadId(bytes<8, 4>) -> u32
        public ResultCode GetNpadId(ServiceCtx context)
        {
            uint deviceHandle = context.RequestData.ReadUInt32();

            for (int i = 0; i < context.Device.System.NfpDevices.Count; i++)
            {
                if ((uint)context.Device.System.NfpDevices[i].Handle == deviceHandle)
                {
                    context.ResponseData.Write((uint)HidUtils.GetNpadIdTypeFromIndex(context.Device.System.NfpDevices[i].Handle));

                    return ResultCode.Success;
                }
            }

            return ResultCode.DeviceNotFound;
        }

        [CommandHipc(22)]
        // GetApplicationAreaSize() -> u32
        public ResultCode GetApplicationAreaSize(ServiceCtx context)
        {
            context.ResponseData.Write(AmiiboConstants.ApplicationAreaSize);

            return ResultCode.Success;
        }

        [CommandHipc(23)] // 3.0.0+
        // AttachAvailabilityChangeEvent() -> handle<copy>
        public ResultCode AttachAvailabilityChangeEvent(ServiceCtx context)
        {
            _availabilityChangeEvent = new KEvent(context.Device.System.KernelContext);

            if (context.Process.HandleTable.GenerateHandle(_availabilityChangeEvent.ReadableEvent, out int availabilityChangeEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(availabilityChangeEventHandle);

            return ResultCode.Success;
        }

        [CommandHipc(24)] // 3.0.0+
        // RecreateApplicationArea(bytes<8, 4>, u32, buffer<unknown, 5>)
        public ResultCode RecreateApplicationArea(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        private ResultCode CheckNfcIsEnabled()
        {
            // TODO: Call nn::settings::detail::GetNfcEnableFlag when it will be implemented.
            return true ? ResultCode.Success : ResultCode.NfcDisabled;
        }
    }
}