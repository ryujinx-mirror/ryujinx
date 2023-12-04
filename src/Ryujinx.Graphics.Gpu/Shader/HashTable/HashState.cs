using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.HashTable
{
    /// <summary>
    /// State of a hash calculation.
    /// </summary>
    struct HashState
    {
        // This is using a slightly modified implementation of FastHash64.
        // Reference: https://github.com/ztanml/fast-hash/blob/master/fasthash.c
        private const ulong M = 0x880355f21e6d1965UL;
        private ulong _hash;
        private int _start;

        /// <summary>
        /// One shot hash calculation for a given data.
        /// </summary>
        /// <param name="data">Data to be hashed</param>
        /// <returns>Hash of the given data</returns>
        public static uint CalcHash(ReadOnlySpan<byte> data)
        {
            HashState state = new();

            state.Initialize();
            state.Continue(data);
            return state.Finalize(data);
        }

        /// <summary>
        /// Initializes the hash state.
        /// </summary>
        public void Initialize()
        {
            _hash = 23;
        }

        /// <summary>
        /// Calculates the hash of the given data.
        /// </summary>
        /// <remarks>
        /// The full data must be passed on <paramref name="data"/>.
        /// If this is not the first time the method is called, then <paramref name="data"/> must start with the data passed on the last call.
        /// If a smaller slice of the data was already hashed before, only the additional data will be hashed.
        /// This can be used for additive hashing of data in chuncks.
        /// </remarks>
        /// <param name="data">Data to be hashed</param>
        public void Continue(ReadOnlySpan<byte> data)
        {
            ulong h = _hash;

            ReadOnlySpan<ulong> dataAsUlong = MemoryMarshal.Cast<byte, ulong>(data[_start..]);

            for (int i = 0; i < dataAsUlong.Length; i++)
            {
                ulong value = dataAsUlong[i];

                h ^= Mix(value);
                h *= M;
            }

            _hash = h;
            _start = data.Length & ~7;
        }

        /// <summary>
        /// Performs the hash finalization step, and returns the calculated hash.
        /// </summary>
        /// <remarks>
        /// The full data must be passed on <paramref name="data"/>.
        /// <paramref name="data"/> must start with the data passed on the last call to <see cref="Continue"/>.
        /// No internal state is changed, so one can still continue hashing data with <see cref="Continue"/>
        /// after calling this method.
        /// </remarks>
        /// <param name="data">Data to be hashed</param>
        /// <returns>Hash of all the data hashed with this <see cref="HashState"/></returns>
        public readonly uint Finalize(ReadOnlySpan<byte> data)
        {
            ulong h = _hash;

            int remainder = data.Length & 7;
            if (remainder != 0)
            {
                ulong v = 0;

                for (int i = data.Length - remainder; i < data.Length; i++)
                {
                    v |= (ulong)data[i] << ((i - remainder) * 8);
                }

                h ^= Mix(v);
                h *= M;
            }

            h = Mix(h);
            return (uint)(h - (h >> 32));
        }

        /// <summary>
        /// Hash mix function.
        /// </summary>
        /// <param name="h">Hash to mix</param>
        /// <returns>Mixed hash</returns>
        private static ulong Mix(ulong h)
        {
            h ^= h >> 23;
            h *= 0x2127599bf4325c37UL;
            h ^= h >> 47;
            return h;
        }
    }
}
