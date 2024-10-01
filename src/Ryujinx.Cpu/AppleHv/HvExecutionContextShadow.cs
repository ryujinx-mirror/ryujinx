using ARMeilleure.State;

namespace Ryujinx.Cpu.AppleHv
{
    class HvExecutionContextShadow : IHvExecutionContext
    {
        public ulong Pc { get; set; }
        public ulong ElrEl1 { get; set; }
        public ulong EsrEl1 { get; set; }

        public long TpidrEl0 { get; set; }
        public long TpidrroEl0 { get; set; }

        public uint Pstate { get; set; }

        public uint Fpcr { get; set; }
        public uint Fpsr { get; set; }

        public bool IsAarch32 { get; set; }

        private readonly ulong[] _x;
        private readonly V128[] _v;

        public HvExecutionContextShadow()
        {
            _x = new ulong[32];
            _v = new V128[32];
        }

        public ulong GetX(int index)
        {
            return _x[index];
        }

        public void SetX(int index, ulong value)
        {
            _x[index] = value;
        }

        public V128 GetV(int index)
        {
            return _v[index];
        }

        public void SetV(int index, V128 value)
        {
            _v[index] = value;
        }
    }
}
