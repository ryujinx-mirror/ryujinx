namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// GPU Macro assignment operation.
    /// </summary>
    enum AssignmentOperation
    {
        IgnoreAndFetch = 0,
        Move = 1,
        MoveAndSetMaddr = 2,
        FetchAndSend = 3,
        MoveAndSend = 4,
        FetchAndSetMaddr = 5,
        MoveAndSetMaddrThenFetchAndSend = 6,
        MoveAndSetMaddrThenSendHigh = 7
    }
}
