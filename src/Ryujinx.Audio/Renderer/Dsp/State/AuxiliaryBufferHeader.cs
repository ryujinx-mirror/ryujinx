using Ryujinx.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x80)]
    public struct AuxiliaryBufferHeader
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
        public struct AuxiliaryBufferInfo
        {
            private const uint ReadOffsetPosition = 0x0;
            private const uint WriteOffsetPosition = 0x4;
            private const uint LostSampleCountPosition = 0x8;
            private const uint TotalSampleCountPosition = 0xC;

            public uint ReadOffset;
            public uint WriteOffset;
            public uint LostSampleCount;
            public uint TotalSampleCount;
            private unsafe fixed uint _unknown[12];

            public static uint GetReadOffset(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                return manager.Read<uint>(bufferAddress + ReadOffsetPosition);
            }

            public static uint GetWriteOffset(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                return manager.Read<uint>(bufferAddress + WriteOffsetPosition);
            }

            public static uint GetLostSampleCount(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                return manager.Read<uint>(bufferAddress + LostSampleCountPosition);
            }

            public static uint GetTotalSampleCount(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                return manager.Read<uint>(bufferAddress + TotalSampleCountPosition);
            }

            public static void SetReadOffset(IVirtualMemoryManager manager, ulong bufferAddress, uint value)
            {
                manager.Write(bufferAddress + ReadOffsetPosition, value);
            }

            public static void SetWriteOffset(IVirtualMemoryManager manager, ulong bufferAddress, uint value)
            {
                manager.Write(bufferAddress + WriteOffsetPosition, value);
            }

            public static void SetLostSampleCount(IVirtualMemoryManager manager, ulong bufferAddress, uint value)
            {
                manager.Write(bufferAddress + LostSampleCountPosition, value);
            }

            public static void SetTotalSampleCount(IVirtualMemoryManager manager, ulong bufferAddress, uint value)
            {
                manager.Write(bufferAddress + TotalSampleCountPosition, value);
            }

            public static void Reset(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                // NOTE: Lost sample count is never reset, since REV10.
                manager.Write(bufferAddress + ReadOffsetPosition, 0UL);
                manager.Write(bufferAddress + TotalSampleCountPosition, 0);
            }
        }

        public AuxiliaryBufferInfo CpuBufferInfo;
        public AuxiliaryBufferInfo DspBufferInfo;
    }
}
