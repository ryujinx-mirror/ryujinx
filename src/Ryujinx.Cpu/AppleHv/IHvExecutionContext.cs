using ARMeilleure.State;

namespace Ryujinx.Cpu.AppleHv
{
    interface IHvExecutionContext
    {
        ulong Pc { get; set; }
        ulong ElrEl1 { get; set; }
        ulong EsrEl1 { get; set; }

        long TpidrEl0 { get; set; }
        long TpidrroEl0 { get; set; }

        uint Pstate { get; set; }

        uint Fpcr { get; set; }
        uint Fpsr { get; set; }

        ulong GetX(int index);
        void SetX(int index, ulong value);

        V128 GetV(int index);
        void SetV(int index, V128 value);

        public void Load(IHvExecutionContext context)
        {
            Pc = context.Pc;
            ElrEl1 = context.ElrEl1;
            EsrEl1 = context.EsrEl1;
            TpidrEl0 = context.TpidrEl0;
            TpidrroEl0 = context.TpidrroEl0;
            Pstate = context.Pstate;
            Fpcr = context.Fpcr;
            Fpsr = context.Fpsr;

            for (int i = 0; i < 32; i++)
            {
                SetX(i, context.GetX(i));
                SetV(i, context.GetV(i));
            }
        }
    }
}
