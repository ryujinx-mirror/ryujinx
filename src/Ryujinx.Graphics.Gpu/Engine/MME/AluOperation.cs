namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// GPU Macro Arithmetic and Logic unit operation.
    /// </summary>
    enum AluOperation
    {
        AluReg = 0,
        AddImmediate = 1,
        BitfieldReplace = 2,
        BitfieldExtractLslImm = 3,
        BitfieldExtractLslReg = 4,
        ReadImmediate = 5,
    }
}
