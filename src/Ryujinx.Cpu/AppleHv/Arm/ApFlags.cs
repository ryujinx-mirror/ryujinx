namespace Ryujinx.Cpu.AppleHv.Arm
{
    enum ApFlags : ulong
    {
        ApShift = 6,
        PxnShift = 53,
        UxnShift = 54,

        UserExecuteKernelReadWriteExecute = (0UL << (int)ApShift),
        UserReadWriteExecuteKernelReadWrite = (1UL << (int)ApShift),
        UserExecuteKernelReadExecute = (2UL << (int)ApShift),
        UserReadExecuteKernelReadExecute = (3UL << (int)ApShift),

        UserExecuteKernelReadWrite = (1UL << (int)PxnShift) | (0UL << (int)ApShift),
        UserExecuteKernelRead = (1UL << (int)PxnShift) | (2UL << (int)ApShift),
        UserReadExecuteKernelRead = (1UL << (int)PxnShift) | (3UL << (int)ApShift),

        UserNoneKernelReadWriteExecute = (1UL << (int)UxnShift) | (0UL << (int)ApShift),
        UserReadWriteKernelReadWrite = (1UL << (int)UxnShift) | (1UL << (int)ApShift),
        UserNoneKernelReadExecute = (1UL << (int)UxnShift) | (2UL << (int)ApShift),
        UserReadKernelReadExecute = (1UL << (int)UxnShift) | (3UL << (int)ApShift),

        UserNoneKernelReadWrite = (1UL << (int)PxnShift) | (1UL << (int)UxnShift) | (0UL << (int)ApShift),
        UserNoneKernelRead = (1UL << (int)PxnShift) | (1UL << (int)UxnShift) | (2UL << (int)ApShift),
        UserReadKernelRead = (1UL << (int)PxnShift) | (1UL << (int)UxnShift) | (3UL << (int)ApShift),
    }
}
