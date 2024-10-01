namespace Ryujinx.Common.Memory
{
    /// <summary>
    /// Array interface.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public interface IArray<T> where T : unmanaged
    {
        /// <summary>
        /// Used to index the array.
        /// </summary>
        /// <param name="index">Element index</param>
        /// <returns>Element at the specified index</returns>
        ref T this[int index] { get; }

        /// <summary>
        /// Number of elements on the array.
        /// </summary>
        int Length { get; }
    }
}
