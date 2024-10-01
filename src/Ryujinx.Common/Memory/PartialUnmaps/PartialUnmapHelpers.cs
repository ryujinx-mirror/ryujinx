using System.Runtime.CompilerServices;

namespace Ryujinx.Common.Memory.PartialUnmaps
{
    static class PartialUnmapHelpers
    {
        /// <summary>
        /// Calculates a byte offset of a given field within a struct.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <typeparam name="T2">Field type</typeparam>
        /// <param name="storage">Parent struct</param>
        /// <param name="target">Field</param>
        /// <returns>The byte offset of the given field in the given struct</returns>
        public static int OffsetOf<T, T2>(ref T2 storage, ref T target)
        {
            return (int)Unsafe.ByteOffset(ref Unsafe.As<T2, T>(ref storage), ref target);
        }
    }
}
