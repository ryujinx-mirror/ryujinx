using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Services.Settings;
using Ryujinx.HLE.HOS.Services.Sockets.Nsd.Manager;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Sockets.Nsd
{
    [Service("nsd:a")] // Max sessions: 5
    [Service("nsd:u")] // Max sessions: 20
    class IManager : IpcService
    {
        private NsdSettings  _nsdSettings;
        private FqdnResolver _fqdnResolver;

        private bool _isInitialized = false;

        public IManager(ServiceCtx context)
        {
            // TODO: Load nsd settings through the savedata 0x80000000000000B0 (nsdsave:/).

            NxSettings.Settings.TryGetValue("nsd!test_mode", out object testMode);

            _nsdSettings = new NsdSettings
            {
                Initialized = true,
                TestMode    = (bool)testMode
            };

            _fqdnResolver = new FqdnResolver(_nsdSettings);

            _isInitialized = true;
        }

        [CommandHipc(10)]
        // GetSettingName() -> buffer<unknown<0x100>, 0x16>
        public ResultCode GetSettingName(ServiceCtx context)
        {
            (ulong outputPosition, ulong outputSize) = context.Request.GetBufferType0x22();

            ResultCode result = _fqdnResolver.GetSettingName(context, out string settingName);

            if (result == ResultCode.Success)
            {
                byte[] settingNameBuffer = Encoding.UTF8.GetBytes(settingName + '\0');

                context.Memory.Write(outputPosition, settingNameBuffer);
            }

            return result;
        }

        [CommandHipc(11)]
        // GetEnvironmentIdentifier() -> buffer<unknown<8>, 0x16>
        public ResultCode GetEnvironmentIdentifier(ServiceCtx context)
        {
            (ulong outputPosition, ulong outputSize) = context.Request.GetBufferType0x22();

            ResultCode result = _fqdnResolver.GetEnvironmentIdentifier(context, out string identifier);

            if (result == ResultCode.Success)
            {
                byte[] identifierBuffer = Encoding.UTF8.GetBytes(identifier + '\0');

                context.Memory.Write(outputPosition, identifierBuffer);
            }

            return result;
        }

        [CommandHipc(12)]
        // GetDeviceId() -> bytes<0x10, 1>
        public ResultCode GetDeviceId(ServiceCtx context)
        {
            // NOTE: Stubbed in system module.

            return ResultCode.Success;
        }

        [CommandHipc(13)]
        // DeleteSettings(u32)
        public ResultCode DeleteSettings(ServiceCtx context)
        {
            uint unknown = context.RequestData.ReadUInt32();

            if (!_isInitialized)
            {
                return ResultCode.ServiceNotInitialized;
            }

            if (unknown > 1)
            {
                return ResultCode.InvalidArgument;
            }

            if (unknown == 1)
            {
                NxSettings.Settings.TryGetValue("nsd!environment_identifier", out object environmentIdentifier);

                if ((string)environmentIdentifier == _nsdSettings.Environment)
                {
                    // TODO: Call nn::fs::DeleteSystemFile() to delete the savedata file and return ResultCode.
                }
                else
                {
                    // TODO: Mount the savedata file and return ResultCode.
                }
            }
            else
            {
                // TODO: Call nn::fs::DeleteSystemFile() to delete the savedata file and return ResultCode.
            }

            return ResultCode.Success;
        }

        [CommandHipc(14)]
        // ImportSettings(u32, buffer<unknown, 5>) -> buffer<unknown, 6>
        public ResultCode ImportSettings(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(15)]
        // Unknown(bytes<1>)
        public ResultCode Unknown(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(20)]
        // Resolve(buffer<unknown<0x100>, 0x15>) -> buffer<unknown<0x100>, 0x16>
        public ResultCode Resolve(ServiceCtx context)
        {
            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize     = context.Request.ReceiveBuff[0].Size;

            ResultCode result = _fqdnResolver.ResolveEx(context, out _, out string resolvedAddress);

            if ((ulong)resolvedAddress.Length > outputSize)
            {
                return ResultCode.InvalidArgument;
            }

            byte[] resolvedAddressBuffer = Encoding.UTF8.GetBytes(resolvedAddress);

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            context.Memory.Write(outputPosition, resolvedAddressBuffer);

            return result;
        }

        [CommandHipc(21)]
        // ResolveEx(buffer<unknown<0x100>, 0x15>) -> (u32, buffer<unknown<0x100>, 0x16>)
        public ResultCode ResolveEx(ServiceCtx context)
        {
            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize     = context.Request.ReceiveBuff[0].Size;

            ResultCode result = _fqdnResolver.ResolveEx(context, out ResultCode errorCode, out string resolvedAddress);

            if ((ulong)resolvedAddress.Length > outputSize)
            {
                return ResultCode.InvalidArgument;
            }

            byte[] resolvedAddressBuffer = Encoding.UTF8.GetBytes(resolvedAddress);

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            context.Memory.Write(outputPosition, resolvedAddressBuffer);

            context.ResponseData.Write((int)errorCode);

            return result;
        }

        [CommandHipc(30)]
        // GetNasServiceSetting(buffer<unknown<0x10>, 0x15>) -> buffer<unknown<0x108>, 0x16>
        public ResultCode GetNasServiceSetting(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(31)]
        // GetNasServiceSettingEx(buffer<unknown<0x10>, 0x15>) -> (u32, buffer<unknown<0x108>, 0x16>)
        public ResultCode GetNasServiceSettingEx(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(40)]
        // GetNasRequestFqdn() -> buffer<unknown<0x100>, 0x16>
        public ResultCode GetNasRequestFqdn(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(41)]
        // GetNasRequestFqdnEx() -> (u32, buffer<unknown<0x100>, 0x16>)
        public ResultCode GetNasRequestFqdnEx(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(42)]
        // GetNasApiFqdn() -> buffer<unknown<0x100>, 0x16>
        public ResultCode GetNasApiFqdn(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(43)]
        // GetNasApiFqdnEx() -> (u32, buffer<unknown<0x100>, 0x16>)
        public ResultCode GetNasApiFqdnEx(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(50)]
        // GetCurrentSetting() -> buffer<unknown<0x12bf0>, 0x16>
        public ResultCode GetCurrentSetting(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }

        [CommandHipc(60)]
        // ReadSaveDataFromFsForTest() -> buffer<unknown<0x12bf0>, 0x16>
        public ResultCode ReadSaveDataFromFsForTest(ServiceCtx context)
        {
            if (!_isInitialized)
            {
                return ResultCode.ServiceNotInitialized;
            }

            // TODO: Call nn::nsd::detail::fs::ReadSaveDataWithOffset() at offset 0 to write the 
            //       whole savedata inside the buffer.

            Logger.Stub?.PrintStub(LogClass.ServiceNsd);

            return ResultCode.Success;
        }

        [CommandHipc(61)]
        // WriteSaveDataToFsForTest(buffer<unknown<0x12bf0>, 0x15>)
        public ResultCode WriteSaveDataToFsForTest(ServiceCtx context)
        {
            // NOTE: Stubbed in system module.

            if (_isInitialized)
            {
                return ResultCode.NotImplemented;
            }
            else
            {
                return ResultCode.ServiceNotInitialized;
            }
        }

        [CommandHipc(62)]
        // DeleteSaveDataOfFsForTest()
        public ResultCode DeleteSaveDataOfFsForTest(ServiceCtx context)
        {
            // NOTE: Stubbed in system module.

            if (_isInitialized)
            {
                return ResultCode.NotImplemented;
            }
            else
            {
                return ResultCode.ServiceNotInitialized;
            }
        }

        [CommandHipc(63)]
        // IsChangeEnvironmentIdentifierDisabled() -> bytes<1>
        public ResultCode IsChangeEnvironmentIdentifierDisabled(ServiceCtx context)
        {
            throw new ServiceNotImplementedException(this, context, false);
        }
    }
}