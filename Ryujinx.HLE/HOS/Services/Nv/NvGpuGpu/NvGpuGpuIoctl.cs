using ChocolArm64.Memory;
using Ryujinx.Common.Logging;
using System;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Services.Nv.NvGpuGpu
{
    class NvGpuGpuIoctl
    {
        private static Stopwatch PTimer;

        private static double TicksToNs;

        static NvGpuGpuIoctl()
        {
            PTimer = new Stopwatch();

            PTimer.Start();

            TicksToNs = (1.0 / Stopwatch.Frequency) * 1_000_000_000;
        }

        public static int ProcessIoctl(ServiceCtx Context, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x4701: return ZcullGetCtxSize   (Context);
                case 0x4702: return ZcullGetInfo      (Context);
                case 0x4703: return ZbcSetTable       (Context);
                case 0x4705: return GetCharacteristics(Context);
                case 0x4706: return GetTpcMasks       (Context);
                case 0x4714: return GetActiveSlotMask (Context);
                case 0x471c: return GetGpuTime        (Context);
            }

            throw new NotImplementedException(Cmd.ToString("x8"));
        }

        private static int ZcullGetCtxSize(ServiceCtx Context)
        {
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuGpuZcullGetCtxSize Args = new NvGpuGpuZcullGetCtxSize();

            Args.Size = 1;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZcullGetInfo(ServiceCtx Context)
        {
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuGpuZcullGetInfo Args = new NvGpuGpuZcullGetInfo();

            Args.WidthAlignPixels           = 0x20;
            Args.HeightAlignPixels          = 0x20;
            Args.PixelSquaresByAliquots     = 0x400;
            Args.AliquotTotal               = 0x800;
            Args.RegionByteMultiplier       = 0x20;
            Args.RegionHeaderSize           = 0x20;
            Args.SubregionHeaderSize        = 0xc0;
            Args.SubregionWidthAlignPixels  = 0x20;
            Args.SubregionHeightAlignPixels = 0x40;
            Args.SubregionCount             = 0x10;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZbcSetTable(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int GetCharacteristics(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetCharacteristics Args = MemoryHelper.Read<NvGpuGpuGetCharacteristics>(Context.Memory, InputPosition);

            Args.BufferSize = 0xa0;

            Args.Arch                   = 0x120;
            Args.Impl                   = 0xb;
            Args.Rev                    = 0xa1;
            Args.NumGpc                 = 0x1;
            Args.L2CacheSize            = 0x40000;
            Args.OnBoardVideoMemorySize = 0x0;
            Args.NumTpcPerGpc           = 0x2;
            Args.BusType                = 0x20;
            Args.BigPageSize            = 0x20000;
            Args.CompressionPageSize    = 0x20000;
            Args.PdeCoverageBitCount    = 0x1b;
            Args.AvailableBigPageSizes  = 0x30000;
            Args.GpcMask                = 0x1;
            Args.SmArchSmVersion        = 0x503;
            Args.SmArchSpaVersion       = 0x503;
            Args.SmArchWarpCount        = 0x80;
            Args.GpuVaBitCount          = 0x28;
            Args.Reserved               = 0x0;
            Args.Flags                  = 0x55;
            Args.TwodClass              = 0x902d;
            Args.ThreedClass            = 0xb197;
            Args.ComputeClass           = 0xb1c0;
            Args.GpfifoClass            = 0xb06f;
            Args.InlineToMemoryClass    = 0xa140;
            Args.DmaCopyClass           = 0xb0b5;
            Args.MaxFbpsCount           = 0x1;
            Args.FbpEnMask              = 0x0;
            Args.MaxLtcPerFbp           = 0x2;
            Args.MaxLtsPerLtc           = 0x1;
            Args.MaxTexPerTpc           = 0x0;
            Args.MaxGpcCount            = 0x1;
            Args.RopL2EnMask0           = 0x21d70;
            Args.RopL2EnMask1           = 0x0;
            Args.ChipName               = 0x6230326d67;
            Args.GrCompbitStoreBaseHw   = 0x0;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int GetTpcMasks(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetTpcMasks Args = MemoryHelper.Read<NvGpuGpuGetTpcMasks>(Context.Memory, InputPosition);

            if (Args.MaskBufferSize != 0)
            {
                Args.TpcMask = 3;
            }

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int GetActiveSlotMask(ServiceCtx Context)
        {
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetActiveSlotMask Args = new NvGpuGpuGetActiveSlotMask();

            Args.Slot = 0x07;
            Args.Mask = 0x01;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int GetGpuTime(ServiceCtx Context)
        {
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Memory.WriteInt64(OutputPosition, GetPTimerNanoSeconds());

            return NvResult.Success;
        }

        private static long GetPTimerNanoSeconds()
        {
            double Ticks = PTimer.ElapsedTicks;

            return (long)(Ticks * TicksToNs) & 0xff_ffff_ffff_ffff;
        }
    }
}