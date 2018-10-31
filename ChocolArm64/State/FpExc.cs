namespace ChocolArm64.State
{
    enum FpExc
    {
        InvalidOp    = 0,
        DivideByZero = 1,
        Overflow     = 2,
        Underflow    = 3,
        Inexact      = 4,
        InputDenorm  = 7
    }
}
