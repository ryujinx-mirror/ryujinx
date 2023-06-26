using ARMeilleure.IntermediateRepresentation;
using System;

namespace ARMeilleure.CodeGen.Arm64
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

    static class ComparisonArm64Extensions
    {
        public static ArmCondition ToArmCondition(this Comparison comp)
        {
            return comp switch
            {
#pragma warning disable IDE0055 // Disable formatting
                Comparison.Equal            => ArmCondition.Eq,
                Comparison.NotEqual         => ArmCondition.Ne,
                Comparison.Greater          => ArmCondition.Gt,
                Comparison.LessOrEqual      => ArmCondition.Le,
                Comparison.GreaterUI        => ArmCondition.GtUn,
                Comparison.LessOrEqualUI    => ArmCondition.LeUn,
                Comparison.GreaterOrEqual   => ArmCondition.Ge,
                Comparison.Less             => ArmCondition.Lt,
                Comparison.GreaterOrEqualUI => ArmCondition.GeUn,
                Comparison.LessUI           => ArmCondition.LtUn,
#pragma warning restore IDE0055

                _ => throw new ArgumentException(null, nameof(comp)),
            };
        }
    }
}
