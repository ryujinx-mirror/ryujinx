namespace Ryujinx.Cpu.LightningJit.CodeGen.Arm64
{
    enum ArmCondition
    {
        Eq = 0,
        Ne = 1,
        GeUn = 2,
        LtUn = 3,
        Mi = 4,
        Pl = 5,
        Vs = 6,
        Vc = 7,
        GtUn = 8,
        LeUn = 9,
        Ge = 10,
        Lt = 11,
        Gt = 12,
        Le = 13,
        Al = 14,
        Nv = 15,
    }

    static class ArmConditionExtensions
    {
        public static ArmCondition Invert(this ArmCondition condition)
        {
            return (ArmCondition)((int)condition ^ 1);
        }
    }
}
