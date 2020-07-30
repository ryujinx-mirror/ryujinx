namespace Ryujinx.Cpu
{
    /// <summary>
    /// Function that handles a invalid memory access from the emulated CPU.
    /// </summary>
    /// <param name="va">Virtual address of the invalid region that is being accessed</param>
    /// <returns>True if the invalid access should be ignored, false otherwise</returns>
    public delegate bool InvalidAccessHandler(ulong va);
}
