using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 0xC3 reads or writes a static register with a given register.
    /// NOTE: Registers are saved and restored to a different set of registers than the ones used
    /// for the other opcodes (Static Registers).
    /// </summary>
    class ReadOrWriteStaticRegister
    {
        private const int StaticRegisterIndex = 5;
        private const int RegisterIndex = 7;

        private const byte FirstWriteRegister = 0x80;

        private const int StaticRegisterSize = 2;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // C3000XXx
            // XX: Static register index, 0x00 to 0x7F for reading or 0x80 to 0xFF for writing.
            // x: Register index.

            ulong staticRegisterIndex = InstructionHelper.GetImmediate(instruction, StaticRegisterIndex, StaticRegisterSize);
            Register register = context.GetRegister(instruction[RegisterIndex]);

            IOperand sourceRegister;
            IOperand destinationRegister;

            if (staticRegisterIndex < FirstWriteRegister)
            {
                // Read from static register.
                sourceRegister = context.GetStaticRegister((byte)staticRegisterIndex);
                destinationRegister = register;
            }
            else
            {
                // Write to static register.
                sourceRegister = register;
                destinationRegister = context.GetStaticRegister((byte)(staticRegisterIndex - FirstWriteRegister));
            }

            context.CurrentOperations.Add(new OpMov<ulong>(destinationRegister, sourceRegister));
        }
    }
}
