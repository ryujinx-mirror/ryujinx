using System.Runtime.InteropServices;
using System.Threading;
using static Ryujinx.Common.Memory.PartialUnmaps.PartialUnmapHelpers;

namespace Ryujinx.Common.Memory.PartialUnmaps
{
    /// <summary>
    /// A simple fixed size thread safe map that can be used from native code.
    /// Integer thread IDs map to corresponding structs.
    /// </summary>
    /// <typeparam name="T">The value type for the map</typeparam>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ThreadLocalMap<T> where T : unmanaged
    {
        public const int MapSize = 20;

        public Array20<int> ThreadIds;
        public Array20<T> Structs;

        public static readonly int ThreadIdsOffset;
        public static readonly int StructsOffset;

        /// <summary>
        /// Populates the field offsets for use when emitting native code.
        /// </summary>
        static ThreadLocalMap()
        {
            ThreadLocalMap<T> instance = new();

            ThreadIdsOffset = OffsetOf(ref instance, ref instance.ThreadIds);
            StructsOffset = OffsetOf(ref instance, ref instance.Structs);
        }

        /// <summary>
        /// Gets the index of a given thread ID in the map, or reserves one.
        /// When reserving a struct, its value is set to the given initial value.
        /// Returns -1 when there is no space to reserve a new entry.
        /// </summary>
        /// <param name="threadId">Thread ID to use as a key</param>
        /// <param name="initial">Initial value of the associated struct.</param>
        /// <returns>The index of the entry, or -1 if none</returns>
        public int GetOrReserve(int threadId, T initial)
        {
            // Try get a match first.

            for (int i = 0; i < MapSize; i++)
            {
                int compare = Interlocked.CompareExchange(ref ThreadIds[i], threadId, threadId);

                if (compare == threadId)
                {
                    return i;
                }
            }

            // Try get a free entry. Since the id is assumed to be unique to this thread, we know it doesn't exist yet.

            for (int i = 0; i < MapSize; i++)
            {
                int compare = Interlocked.CompareExchange(ref ThreadIds[i], threadId, 0);

                if (compare == 0)
                {
                    Structs[i] = initial;
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the struct value for a given map entry.
        /// </summary>
        /// <param name="index">Index of the entry</param>
        /// <returns>A reference to the struct value</returns>
        public ref T GetValue(int index)
        {
            return ref Structs[index];
        }

        /// <summary>
        /// Releases an entry from the map.
        /// </summary>
        /// <param name="index">Index of the entry to release</param>
        public void Release(int index)
        {
            Interlocked.Exchange(ref ThreadIds[index], 0);
        }
    }
}
