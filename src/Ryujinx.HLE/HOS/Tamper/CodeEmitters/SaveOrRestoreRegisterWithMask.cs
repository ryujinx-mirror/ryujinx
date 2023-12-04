namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 0xC2 performs saving or restoring of multiple registers using a bitmask.
    /// NOTE: Registers are saved and restored to a different set of registers than the ones used
    /// for the other opcodes (Save Registers).
    /// </summary>
    class SaveOrRestoreRegisterWithMask
    {
        private const int OperationTypeIndex = 2;
        private const int RegisterMaskIndex = 4;

        private const int RegisterMaskSize = 4;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // C2x0XXXX
            // x: Operand Type, see below.
            // X: 16-bit bitmask, bit i == save or restore register i.

            byte operationType = instruction[OperationTypeIndex];
            ulong mask = InstructionHelper.GetImmediate(instruction, RegisterMaskIndex, RegisterMaskSize);

            for (byte regIndex = 0; mask != 0; mask >>= 1, regIndex++)
            {
                if ((mask & 0x1) != 0)
                {
                    SaveOrRestoreRegister.Impl(operationType, regIndex, regIndex, context);
                }
            }
        }
    }
}
