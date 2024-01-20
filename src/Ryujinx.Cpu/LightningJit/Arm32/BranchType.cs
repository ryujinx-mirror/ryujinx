namespace Ryujinx.Cpu.LightningJit.Arm32
{
    enum BranchType
    {
        Branch,
        Call,
        IndirectBranch,
        TableBranchByte,
        TableBranchHalfword,
        IndirectCall,
        SyncPoint,
        SoftwareInterrupt,
        ReadCntpct,
    }
}
