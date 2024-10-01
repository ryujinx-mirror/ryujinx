using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 0xC1 performs saving or restoring of registers.
    /// NOTE: Registers are saved and restored to a different set of registers than the ones used
    /// for the other opcodes (Save Registers).
    /// </summary>
    class SaveOrRestoreRegister
    {
        private const int DestinationRegisterIndex = 3;
        private const int SourceRegisterIndex = 5;
        private const int OperationTypeIndex = 6;

        private const int RestoreRegister = 0;
        private const int SaveRegister = 1;
        private const int ClearSavedValue = 2;
        private const int ClearRegister = 3;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // C10D0Sx0
            // D: Destination index.
            // S: Source index.
            // x: Operand Type, see below.

            byte destinationRegIndex = instruction[DestinationRegisterIndex];
            byte sourceRegIndex = instruction[SourceRegisterIndex];
            byte operationType = instruction[OperationTypeIndex];
            Impl(operationType, destinationRegIndex, sourceRegIndex, context);
        }

        public static void Impl(byte operationType, byte destinationRegIndex, byte sourceRegIndex, CompilationContext context)
        {
            IOperand destinationOperand;
            IOperand sourceOperand;

            switch (operationType)
            {
                case RestoreRegister:
                    destinationOperand = context.GetRegister(destinationRegIndex);
                    sourceOperand = context.GetSavedRegister(sourceRegIndex);
                    break;
                case SaveRegister:
                    destinationOperand = context.GetSavedRegister(destinationRegIndex);
                    sourceOperand = context.GetRegister(sourceRegIndex);
                    break;
                case ClearSavedValue:
                    destinationOperand = new Value<ulong>(0);
                    sourceOperand = context.GetSavedRegister(sourceRegIndex);
                    break;
                case ClearRegister:
                    destinationOperand = new Value<ulong>(0);
                    sourceOperand = context.GetRegister(sourceRegIndex);
                    break;
                default:
                    throw new TamperCompilationException($"Invalid register operation type {operationType} in Atmosphere cheat");
            }

            context.CurrentOperations.Add(new OpMov<ulong>(destinationOperand, sourceOperand));
        }
    }
}
