using ARMeilleure.State;
using Ryujinx.Cpu;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class ProcessExecutionContext : IExecutionContext
    {
        public ulong Pc => 0UL;

        public long TpidrEl0 { get; set; }
        public long TpidrroEl0 { get; set; }

        public uint Pstate { get; set; }

        public uint Fpcr { get; set; }
        public uint Fpsr { get; set; }

        public bool IsAarch32 { get => false; set { } }

        public bool Running { get; private set; } = true;

        private readonly ulong[] _x = new ulong[32];

        public ulong GetX(int index) => _x[index];
        public void SetX(int index, ulong value) => _x[index] = value;

        public V128 GetV(int index) => default;
        public void SetV(int index, V128 value) { }

        public void RequestInterrupt()
        {
        }

        public void StopRunning()
        {
            Running = false;
        }

        public void Dispose()
        {
        }
    }
}
