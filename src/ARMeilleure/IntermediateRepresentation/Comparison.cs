namespace ARMeilleure.IntermediateRepresentation
{
    enum Comparison
    {
        Equal             = 0,
        NotEqual          = 1,
        Greater           = 2,
        LessOrEqual       = 3,
        GreaterUI         = 4,
        LessOrEqualUI     = 5,
        GreaterOrEqual    = 6,
        Less              = 7,
        GreaterOrEqualUI  = 8,
        LessUI            = 9
    }

    static class ComparisonExtensions
    {
        public static Comparison Invert(this Comparison comp)
        {
            return (Comparison)((int)comp ^ 1);
        }
    }
}
