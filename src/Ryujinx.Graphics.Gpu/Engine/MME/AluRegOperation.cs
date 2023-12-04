namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// GPU Macro Arithmetic and Logic unit binary register-to-register operation.
    /// </summary>
    enum AluRegOperation
    {
        Add = 0,
        AddWithCarry = 1,
        Subtract = 2,
        SubtractWithBorrow = 3,
        BitwiseExclusiveOr = 8,
        BitwiseOr = 9,
        BitwiseAnd = 10,
        BitwiseAndNot = 11,
        BitwiseNotAnd = 12,
    }
}
