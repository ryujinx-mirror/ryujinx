using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Pcv.Types;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Pcv.Clkrst.ClkrstManager
{
    class IClkrstSession : IpcService
    {
        private DeviceCode _deviceCode;
        private uint       _unknown;
        private uint       _clockRate;

        private DeviceCode[] allowedDeviceCodeTable = new DeviceCode[]
        {
            DeviceCode.Cpu,    DeviceCode.Gpu,      DeviceCode.Disp1,    DeviceCode.Disp2,
            DeviceCode.Tsec,   DeviceCode.Mselect,  DeviceCode.Sor1,     DeviceCode.Host1x,
            DeviceCode.Vic,    DeviceCode.Nvenc,    DeviceCode.Nvjpg,    DeviceCode.Nvdec,
            DeviceCode.Ape,    DeviceCode.AudioDsp, DeviceCode.Emc,      DeviceCode.Dsi,
            DeviceCode.SysBus, DeviceCode.XusbSs,   DeviceCode.XusbHost, DeviceCode.XusbDevice,
            DeviceCode.Gpuaux, DeviceCode.Pcie,     DeviceCode.Apbdma,   DeviceCode.Sdmmc1, 
            DeviceCode.Sdmmc2, DeviceCode.Sdmmc4
        };

        public IClkrstSession(DeviceCode deviceCode, uint unknown)
        {
            _deviceCode = deviceCode;
            _unknown    = unknown;
        }

        [CommandCmif(7)]
        // SetClockRate(u32 hz)
        public ResultCode SetClockRate(ServiceCtx context)
        {
            if (!allowedDeviceCodeTable.Contains(_deviceCode))
            {
                return ResultCode.InvalidArgument;
            }

            _clockRate = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServicePcv, new { _clockRate });

            return ResultCode.Success;
        }

        [CommandCmif(8)]
        // GetClockRate() -> u32 hz
        public ResultCode GetClockRate(ServiceCtx context)
        {
            if (!allowedDeviceCodeTable.Contains(_deviceCode))
            {
                return ResultCode.InvalidArgument;
            }

            context.ResponseData.Write(_clockRate);

            Logger.Stub?.PrintStub(LogClass.ServicePcv, new { _clockRate });

            return ResultCode.Success;
        }
    }
}