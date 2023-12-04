using Ryujinx.HLE.HOS.Tamper.Conditions;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 8 enters or skips a conditional block based on whether a key combination is pressed.
    /// </summary>
    class KeyPressConditional
    {
        private const int InputMaskIndex = 1;

        private const int InputMaskSize = 7;

        public static ICondition Emit(byte[] instruction, CompilationContext context)
        {
            // 8kkkkkkk
            // k: Keypad mask to check against, see below.
            // Note that for multiple button combinations, the bitmasks should be ORd together.
            // The Keypad Values are the direct output of hidKeysDown().

            ulong inputMask = InstructionHelper.GetImmediate(instruction, InputMaskIndex, InputMaskSize);

            return new InputMask((long)inputMask, context.PressedKeys);
        }
    }
}
