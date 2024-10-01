namespace Ryujinx.Cpu.LightningJit.Arm32
{
    readonly struct PendingBranch
    {
        public readonly BranchType BranchType;
        public readonly uint TargetAddress;
        public readonly uint NextAddress;
        public readonly InstName Name;
        public readonly int WriterPointer;

        public PendingBranch(BranchType branchType, uint targetAddress, uint nextAddress, InstName name, int writerPointer)
        {
            BranchType = branchType;
            TargetAddress = targetAddress;
            NextAddress = nextAddress;
            Name = name;
            WriterPointer = writerPointer;
        }
    }
}
