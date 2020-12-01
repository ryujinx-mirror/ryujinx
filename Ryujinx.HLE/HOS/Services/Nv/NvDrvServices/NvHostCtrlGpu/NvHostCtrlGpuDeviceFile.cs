using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrlGpu.Types;
using Ryujinx.Memory;
using System;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrlGpu
{
    class NvHostCtrlGpuDeviceFile : NvDeviceFile
    {
        private static Stopwatch _pTimer    = new Stopwatch();
        private static double    _ticksToNs = (1.0 / Stopwatch.Frequency) * 1_000_000_000;

        private KEvent _errorEvent;
        private KEvent _unknownEvent;

        public NvHostCtrlGpuDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, long owner) : base(context, owner)
        {
            _errorEvent   = new KEvent(context.Device.System.KernelContext);
            _unknownEvent = new KEvent(context.Device.System.KernelContext);
        }

        static NvHostCtrlGpuDeviceFile()
        {
            _pTimer.Start();
        }

        public override NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvGpuMagic)
            {
                switch (command.Number)
                {
                    case 0x01:
                        result = CallIoctlMethod<ZcullGetCtxSizeArguments>(ZcullGetCtxSize, arguments);
                        break;
                    case 0x02:
                        result = CallIoctlMethod<ZcullGetInfoArguments>(ZcullGetInfo, arguments);
                        break;
                    case 0x03:
                        result = CallIoctlMethod<ZbcSetTableArguments>(ZbcSetTable, arguments);
                        break;
                    case 0x05:
                        result = CallIoctlMethod<GetCharacteristicsArguments>(GetCharacteristics, arguments);
                        break;
                    case 0x06:
                        result = CallIoctlMethod<GetTpcMasksArguments>(GetTpcMasks, arguments);
                        break;
                    case 0x14:
                        result = CallIoctlMethod<GetActiveSlotMaskArguments>(GetActiveSlotMask, arguments);
                        break;
                    case 0x1c:
                        result = CallIoctlMethod<GetGpuTimeArguments>(GetGpuTime, arguments);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult Ioctl3(NvIoctl command, Span<byte> arguments, Span<byte> inlineOutBuffer)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvGpuMagic)
            {
                switch (command.Number)
                {
                    case 0x05:
                        result = CallIoctlMethod<GetCharacteristicsArguments, GpuCharacteristics>(GetCharacteristics, arguments, inlineOutBuffer);
                        break;
                    case 0x06:
                        result = CallIoctlMethod<GetTpcMasksArguments, int>(GetTpcMasks, arguments, inlineOutBuffer);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult QueryEvent(out int eventHandle, uint eventId)
        {
            // TODO: accurately represent and implement those events.
            KEvent targetEvent = null;

            switch (eventId)
            {
                case 0x1:
                    targetEvent = _errorEvent;
                    break;
                case 0x2:
                    targetEvent = _unknownEvent;
                    break;
            }

            if (targetEvent != null)
            {
                if (Context.Process.HandleTable.GenerateHandle(targetEvent.ReadableEvent, out eventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }
            else
            {
                eventHandle = 0;

                return NvInternalResult.InvalidInput;
            }

            return NvInternalResult.Success;
        }

        public override void Close() { }

        private NvInternalResult ZcullGetCtxSize(ref ZcullGetCtxSizeArguments arguments)
        {
            arguments.Size = 1;

            return NvInternalResult.Success;
        }

        private NvInternalResult ZcullGetInfo(ref ZcullGetInfoArguments arguments)
        {
            arguments.WidthAlignPixels           = 0x20;
            arguments.HeightAlignPixels          = 0x20;
            arguments.PixelSquaresByAliquots     = 0x400;
            arguments.AliquotTotal               = 0x800;
            arguments.RegionByteMultiplier       = 0x20;
            arguments.RegionHeaderSize           = 0x20;
            arguments.SubregionHeaderSize        = 0xc0;
            arguments.SubregionWidthAlignPixels  = 0x20;
            arguments.SubregionHeightAlignPixels = 0x40;
            arguments.SubregionCount             = 0x10;

            return NvInternalResult.Success;
        }

        private NvInternalResult ZbcSetTable(ref ZbcSetTableArguments arguments)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult GetCharacteristics(ref GetCharacteristicsArguments arguments)
        {
            return GetCharacteristics(ref arguments, ref arguments.Characteristics);
        }

        private NvInternalResult GetCharacteristics(ref GetCharacteristicsArguments arguments, ref GpuCharacteristics characteristics)
        {
            arguments.Header.BufferSize = 0xa0;

            characteristics.Arch                   = 0x120;
            characteristics.Impl                   = 0xb;
            characteristics.Rev                    = 0xa1;
            characteristics.NumGpc                 = 0x1;
            characteristics.L2CacheSize            = 0x40000;
            characteristics.OnBoardVideoMemorySize = 0x0;
            characteristics.NumTpcPerGpc           = 0x2;
            characteristics.BusType                = 0x20;
            characteristics.BigPageSize            = 0x20000;
            characteristics.CompressionPageSize    = 0x20000;
            characteristics.PdeCoverageBitCount    = 0x1b;
            characteristics.AvailableBigPageSizes  = 0x30000;
            characteristics.GpcMask                = 0x1;
            characteristics.SmArchSmVersion        = 0x503;
            characteristics.SmArchSpaVersion       = 0x503;
            characteristics.SmArchWarpCount        = 0x80;
            characteristics.GpuVaBitCount          = 0x28;
            characteristics.Reserved               = 0x0;
            characteristics.Flags                  = 0x55;
            characteristics.TwodClass              = 0x902d;
            characteristics.ThreedClass            = 0xb197;
            characteristics.ComputeClass           = 0xb1c0;
            characteristics.GpfifoClass            = 0xb06f;
            characteristics.InlineToMemoryClass    = 0xa140;
            characteristics.DmaCopyClass           = 0xb0b5;
            characteristics.MaxFbpsCount           = 0x1;
            characteristics.FbpEnMask              = 0x0;
            characteristics.MaxLtcPerFbp           = 0x2;
            characteristics.MaxLtsPerLtc           = 0x1;
            characteristics.MaxTexPerTpc           = 0x0;
            characteristics.MaxGpcCount            = 0x1;
            characteristics.RopL2EnMask0           = 0x21d70;
            characteristics.RopL2EnMask1           = 0x0;
            characteristics.ChipName               = 0x6230326d67;
            characteristics.GrCompbitStoreBaseHw   = 0x0;

            arguments.Characteristics = characteristics;

            return NvInternalResult.Success;
        }

        private NvInternalResult GetTpcMasks(ref GetTpcMasksArguments arguments)
        {
            return GetTpcMasks(ref arguments, ref arguments.TpcMask);
        }

        private NvInternalResult GetTpcMasks(ref GetTpcMasksArguments arguments, ref int tpcMask)
        {
            if (arguments.MaskBufferSize != 0)
            {
                tpcMask           = 3;
                arguments.TpcMask = tpcMask;
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult GetActiveSlotMask(ref GetActiveSlotMaskArguments arguments)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            arguments.Slot = 0x07;
            arguments.Mask = 0x01;

            return NvInternalResult.Success;
        }

        private NvInternalResult GetGpuTime(ref GetGpuTimeArguments arguments)
        {
            arguments.Timestamp = GetPTimerNanoSeconds();

            return NvInternalResult.Success;
        }

        private static ulong GetPTimerNanoSeconds()
        {
            double ticks = _pTimer.ElapsedTicks;

            return (ulong)(ticks * _ticksToNs) & 0xff_ffff_ffff_ffff;
        }
    }
}
