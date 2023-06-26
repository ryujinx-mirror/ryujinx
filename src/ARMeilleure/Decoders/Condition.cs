namespace ARMeilleure.Decoders
{
    enum Condition
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

    static class ConditionExtensions
    {
        public static Condition Invert(this Condition cond)
        {
            // Bit 0 of all conditions is basically a negation bit, so
            // inverting this bit has the effect of inverting the condition.
            return (Condition)((int)cond ^ 1);
        }
    }
}
