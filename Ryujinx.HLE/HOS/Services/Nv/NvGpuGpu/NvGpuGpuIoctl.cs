using ChocolArm64.Memory;
using Ryujinx.Common.Logging;
using System;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Services.Nv.NvGpuGpu
{
    class NvGpuGpuIoctl
    {
        private static Stopwatch _pTimer;

        private static double _ticksToNs;

        static NvGpuGpuIoctl()
        {
            _pTimer = new Stopwatch();

            _pTimer.Start();

            _ticksToNs = (1.0 / Stopwatch.Frequency) * 1_000_000_000;
        }

        public static int ProcessIoctl(ServiceCtx context, int cmd)
        {
            switch (cmd & 0xffff)
            {
                case 0x4701: return ZcullGetCtxSize   (context);
                case 0x4702: return ZcullGetInfo      (context);
                case 0x4703: return ZbcSetTable       (context);
                case 0x4705: return GetCharacteristics(context);
                case 0x4706: return GetTpcMasks       (context);
                case 0x4714: return GetActiveSlotMask (context);
                case 0x471c: return GetGpuTime        (context);
            }

            throw new NotImplementedException(cmd.ToString("x8"));
        }

        private static int ZcullGetCtxSize(ServiceCtx context)
        {
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuZcullGetCtxSize args = new NvGpuGpuZcullGetCtxSize();

            args.Size = 1;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZcullGetInfo(ServiceCtx context)
        {
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuZcullGetInfo args = new NvGpuGpuZcullGetInfo();

            args.WidthAlignPixels           = 0x20;
            args.HeightAlignPixels          = 0x20;
            args.PixelSquaresByAliquots     = 0x400;
            args.AliquotTotal               = 0x800;
            args.RegionByteMultiplier       = 0x20;
            args.RegionHeaderSize           = 0x20;
            args.SubregionHeaderSize        = 0xc0;
            args.SubregionWidthAlignPixels  = 0x20;
            args.SubregionHeightAlignPixels = 0x40;
            args.SubregionCount             = 0x10;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZbcSetTable(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int GetCharacteristics(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetCharacteristics args = MemoryHelper.Read<NvGpuGpuGetCharacteristics>(context.Memory, inputPosition);

            args.BufferSize = 0xa0;

            args.Arch                   = 0x120;
            args.Impl                   = 0xb;
            args.Rev                    = 0xa1;
            args.NumGpc                 = 0x1;
            args.L2CacheSize            = 0x40000;
            args.OnBoardVideoMemorySize = 0x0;
            args.NumTpcPerGpc           = 0x2;
            args.BusType                = 0x20;
            args.BigPageSize            = 0x20000;
            args.CompressionPageSize    = 0x20000;
            args.PdeCoverageBitCount    = 0x1b;
            args.AvailableBigPageSizes  = 0x30000;
            args.GpcMask                = 0x1;
            args.SmArchSmVersion        = 0x503;
            args.SmArchSpaVersion       = 0x503;
            args.SmArchWarpCount        = 0x80;
            args.GpuVaBitCount          = 0x28;
            args.Reserved               = 0x0;
            args.Flags                  = 0x55;
            args.TwodClass              = 0x902d;
            args.ThreedClass            = 0xb197;
            args.ComputeClass           = 0xb1c0;
            args.GpfifoClass            = 0xb06f;
            args.InlineToMemoryClass    = 0xa140;
            args.DmaCopyClass           = 0xb0b5;
            args.MaxFbpsCount           = 0x1;
            args.FbpEnMask              = 0x0;
            args.MaxLtcPerFbp           = 0x2;
            args.MaxLtsPerLtc           = 0x1;
            args.MaxTexPerTpc           = 0x0;
            args.MaxGpcCount            = 0x1;
            args.RopL2EnMask0           = 0x21d70;
            args.RopL2EnMask1           = 0x0;
            args.ChipName               = 0x6230326d67;
            args.GrCompbitStoreBaseHw   = 0x0;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int GetTpcMasks(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetTpcMasks args = MemoryHelper.Read<NvGpuGpuGetTpcMasks>(context.Memory, inputPosition);

            if (args.MaskBufferSize != 0)
            {
                args.TpcMask = 3;
            }

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int GetActiveSlotMask(ServiceCtx context)
        {
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetActiveSlotMask args = new NvGpuGpuGetActiveSlotMask();

            args.Slot = 0x07;
            args.Mask = 0x01;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int GetGpuTime(ServiceCtx context)
        {
            long outputPosition = context.Request.GetBufferType0x22().Position;

            context.Memory.WriteInt64(outputPosition, GetPTimerNanoSeconds());

            return NvResult.Success;
        }

        private static long GetPTimerNanoSeconds()
        {
            double ticks = _pTimer.ElapsedTicks;

            return (long)(ticks * _ticksToNs) & 0xff_ffff_ffff_ffff;
        }
    }
}