namespace Ryujinx.Cpu.LightningJit.Graph
{
    interface IBlock
    {
        int Index { get; }

        int PredecessorsCount { get; }
        int SuccessorsCount { get; }

        IBlock GetSuccessor(int index);
        IBlock GetPredecessor(int index);

        RegisterUse ComputeUseMasks();
        bool EndsWithContextLoad();
        bool EndsWithContextStore();
    }
}
