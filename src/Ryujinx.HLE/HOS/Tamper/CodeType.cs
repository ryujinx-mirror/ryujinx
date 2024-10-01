namespace Ryujinx.HLE.HOS.Tamper
{
    /// <summary>
    /// The opcodes specified for the Atmosphere Cheat VM.
    /// </summary>
    enum CodeType
    {
        /// <summary>
        /// Code type 0 allows writing a static value to a memory address.
        /// </summary>
        StoreConstantToAddress = 0x0,

        /// <summary>
        /// Code type 1 performs a comparison of the contents of memory to a static value.
        /// If the condition is not met, all instructions until the appropriate conditional block terminator
        /// are skipped.
        /// </summary>
        BeginMemoryConditionalBlock = 0x1,

        /// <summary>
        /// Code type 2 marks the end of a conditional block (started by Code Type 1 or Code Type 8).
        /// </summary>
        EndConditionalBlock = 0x2,

        /// <summary>
        /// Code type 3 allows for iterating in a loop a fixed number of times.
        /// </summary>
        StartEndLoop = 0x3,

        /// <summary>
        /// Code type 4 allows setting a register to a constant value.
        /// </summary>
        LoadRegisterWithContant = 0x4,

        /// <summary>
        /// Code type 5 allows loading a value from memory into a register, either using a fixed address or by
        /// dereferencing the destination register.
        /// </summary>
        LoadRegisterWithMemory = 0x5,

        /// <summary>
        /// Code type 6 allows writing a fixed value to a memory address specified by a register.
        /// </summary>
        StoreConstantToMemory = 0x6,

        /// <summary>
        /// Code type 7 allows performing arithmetic on registers. However, it has been deprecated by Code
        /// type 9, and is only kept for backwards compatibility.
        /// </summary>
        LegacyArithmetic = 0x7,

        /// <summary>
        /// Code type 8 enters or skips a conditional block based on whether a key combination is pressed.
        /// </summary>
        BeginKeypressConditionalBlock = 0x8,

        /// <summary>
        /// Code type 9 allows performing arithmetic on registers.
        /// </summary>
        Arithmetic = 0x9,

        /// <summary>
        /// Code type 10 allows writing a register to memory.
        /// </summary>
        StoreRegisterToMemory = 0xA,

        /// <summary>
        /// Code type 0xC0 performs a comparison of the contents of a register and another value.
        /// This code support multiple operand types, see below. If the condition is not met,
        /// all instructions until the appropriate conditional block terminator are skipped.
        /// </summary>
        BeginRegisterConditionalBlock = 0xC0,

        /// <summary>
        /// Code type 0xC1 performs saving or restoring of registers.
        /// NOTE: Registers are saved and restored to a different set of registers than the ones used
        /// for the other opcodes (Save Registers).
        /// </summary>
        SaveOrRestoreRegister = 0xC1,

        /// <summary>
        /// Code type 0xC2 performs saving or restoring of multiple registers using a bitmask.
        /// NOTE: Registers are saved and restored to a different set of registers than the ones used
        /// for the other opcodes (Save Registers).
        /// </summary>
        SaveOrRestoreRegisterWithMask = 0xC2,

        /// <summary>
        /// Code type 0xC3 reads or writes a static register with a given register.
        /// NOTE: Registers are saved and restored to a different set of registers than the ones used
        /// for the other opcodes (Static Registers).
        /// </summary>
        ReadOrWriteStaticRegister = 0xC3,

        /// <summary>
        /// Code type 0xFF0 pauses the current process.
        /// </summary>
        PauseProcess = 0xFF0,

        /// <summary>
        /// Code type 0xFF1 resumes the current process.
        /// </summary>
        ResumeProcess = 0xFF1,

        /// <summary>
        /// Code type 0xFFF writes a debug log.
        /// </summary>
        DebugLog = 0xFFF,
    }
}
