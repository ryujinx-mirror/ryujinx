namespace Ryujinx.Cpu.LightningJit.Graph
{
    interface IBlockList
    {
        int Count { get; }

        IBlock this[int index] { get; }
    }
}
