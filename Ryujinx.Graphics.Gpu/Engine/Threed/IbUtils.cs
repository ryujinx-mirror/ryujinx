using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Index buffer utility methods.
    /// </summary>
    static class IbUtils
    {
        /// <summary>
        /// Minimum size that the vertex buffer must have, in bytes, to make the index counting profitable.
        /// </summary>
        private const ulong MinimumVbSizeThreshold = 0x200000; // 2 MB

        /// <summary>
        /// Maximum number of indices that the index buffer may have to make the index counting profitable.
        /// </summary>
        private const int MaximumIndexCountThreshold = 65536;

        /// <summary>
        /// Checks if getting the vertex buffer size from the maximum index buffer index is worth it.
        /// </summary>
        /// <param name="vbSizeMax">Maximum size that the vertex buffer may possibly have, in bytes</param>
        /// <param name="indexCount">Total number of indices on the index buffer</param>
        /// <returns>True if getting the vertex buffer size from the index buffer may yield performance improvements</returns>
        public static bool IsIbCountingProfitable(ulong vbSizeMax, int indexCount)
        {
            return vbSizeMax >= MinimumVbSizeThreshold && indexCount <= MaximumIndexCountThreshold;
        }

        /// <summary>
        /// Gets the vertex count of the vertex buffer accessed with the indices from the current index buffer.
        /// </summary>
        /// <param name="mm">GPU memory manager</param>
        /// <param name="type">Index buffer element integer type</param>
        /// <param name="gpuVa">GPU virtual address of the index buffer</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        /// <returns>Vertex count</returns>
        public static ulong GetVertexCount(MemoryManager mm, IndexType type, ulong gpuVa, int firstIndex, int indexCount)
        {
            return type switch
            {
                IndexType.UShort => CountU16(mm, gpuVa, firstIndex, indexCount),
                IndexType.UInt => CountU32(mm, gpuVa, firstIndex, indexCount),
                _ => CountU8(mm, gpuVa, firstIndex, indexCount)
            };
        }

        /// <summary>
        /// Gets the vertex count of the vertex buffer accessed with the indices from the current index buffer, with 8-bit indices.
        /// </summary>
        /// <param name="mm">GPU memory manager</param>
        /// <param name="gpuVa">GPU virtual address of the index buffer</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        /// <returns>Vertex count</returns>
        private unsafe static ulong CountU8(MemoryManager mm, ulong gpuVa, int firstIndex, int indexCount)
        {
            uint max = 0;
            ReadOnlySpan<byte> data = mm.GetSpan(gpuVa, firstIndex + indexCount);

            if (Avx2.IsSupported)
            {
                fixed (byte* pInput = data)
                {
                    int endAligned = firstIndex + ((data.Length - firstIndex) & ~127);

                    var result = Vector256<byte>.Zero;

                    for (int i = firstIndex; i < endAligned; i += 128)
                    {
                        var dataVec0 = Avx.LoadVector256(pInput + (nuint)(uint)i);
                        var dataVec1 = Avx.LoadVector256(pInput + (nuint)(uint)i + 32);
                        var dataVec2 = Avx.LoadVector256(pInput + (nuint)(uint)i + 64);
                        var dataVec3 = Avx.LoadVector256(pInput + (nuint)(uint)i + 96);

                        var max01 = Avx2.Max(dataVec0, dataVec1);
                        var max23 = Avx2.Max(dataVec2, dataVec3);
                        var max0123 = Avx2.Max(max01, max23);

                        result = Avx2.Max(result, max0123);
                    }

                    result = Avx2.Max(result, Avx2.Shuffle(result.AsInt32(), 0xee).AsByte());
                    result = Avx2.Max(result, Avx2.Shuffle(result.AsInt32(), 0x55).AsByte());
                    result = Avx2.Max(result, Avx2.ShuffleLow(result.AsUInt16(), 0x55).AsByte());
                    result = Avx2.Max(result, Avx2.ShiftRightLogical(result.AsUInt16(), 8).AsByte());

                    max = Math.Max(result.GetElement(0), result.GetElement(16));

                    firstIndex = endAligned;
                }
            }
            else if (Sse2.IsSupported)
            {
                fixed (byte* pInput = data)
                {
                    int endAligned = firstIndex + ((data.Length - firstIndex) & ~63);

                    var result = Vector128<byte>.Zero;

                    for (int i = firstIndex; i < endAligned; i += 64)
                    {
                        var dataVec0 = Sse2.LoadVector128(pInput + (nuint)(uint)i);
                        var dataVec1 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 16);
                        var dataVec2 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 32);
                        var dataVec3 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 48);

                        var max01 = Sse2.Max(dataVec0, dataVec1);
                        var max23 = Sse2.Max(dataVec2, dataVec3);
                        var max0123 = Sse2.Max(max01, max23);

                        result = Sse2.Max(result, max0123);
                    }

                    result = Sse2.Max(result, Sse2.Shuffle(result.AsInt32(), 0xee).AsByte());
                    result = Sse2.Max(result, Sse2.Shuffle(result.AsInt32(), 0x55).AsByte());
                    result = Sse2.Max(result, Sse2.ShuffleLow(result.AsUInt16(), 0x55).AsByte());
                    result = Sse2.Max(result, Sse2.ShiftRightLogical(result.AsUInt16(), 8).AsByte());

                    max = result.GetElement(0);

                    firstIndex = endAligned;
                }
            }

            for (int i = firstIndex; i < data.Length; i++)
            {
                if (max < data[i]) max = data[i];
            }

            return (ulong)max + 1;
        }

        /// <summary>
        /// Gets the vertex count of the vertex buffer accessed with the indices from the current index buffer, with 16-bit indices.
        /// </summary>
        /// <param name="mm">GPU memory manager</param>
        /// <param name="gpuVa">GPU virtual address of the index buffer</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        /// <returns>Vertex count</returns>
        private unsafe static ulong CountU16(MemoryManager mm, ulong gpuVa, int firstIndex, int indexCount)
        {
            uint max = 0;
            ReadOnlySpan<ushort> data = MemoryMarshal.Cast<byte, ushort>(mm.GetSpan(gpuVa, (firstIndex + indexCount) * 2));

            if (Avx2.IsSupported)
            {
                fixed (ushort* pInput = data)
                {
                    int endAligned = firstIndex + ((data.Length - firstIndex) & ~63);

                    var result = Vector256<ushort>.Zero;

                    for (int i = firstIndex; i < endAligned; i += 64)
                    {
                        var dataVec0 = Avx.LoadVector256(pInput + (nuint)(uint)i);
                        var dataVec1 = Avx.LoadVector256(pInput + (nuint)(uint)i + 16);
                        var dataVec2 = Avx.LoadVector256(pInput + (nuint)(uint)i + 32);
                        var dataVec3 = Avx.LoadVector256(pInput + (nuint)(uint)i + 48);

                        var max01 = Avx2.Max(dataVec0, dataVec1);
                        var max23 = Avx2.Max(dataVec2, dataVec3);
                        var max0123 = Avx2.Max(max01, max23);

                        result = Avx2.Max(result, max0123);
                    }

                    result = Avx2.Max(result, Avx2.Shuffle(result.AsInt32(), 0xee).AsUInt16());
                    result = Avx2.Max(result, Avx2.Shuffle(result.AsInt32(), 0x55).AsUInt16());
                    result = Avx2.Max(result, Avx2.ShuffleLow(result, 0x55));

                    max = Math.Max(result.GetElement(0), result.GetElement(8));

                    firstIndex = endAligned;
                }
            }
            else if (Sse41.IsSupported)
            {
                fixed (ushort* pInput = data)
                {
                    int endAligned = firstIndex + ((data.Length - firstIndex) & ~31);

                    var result = Vector128<ushort>.Zero;

                    for (int i = firstIndex; i < endAligned; i += 32)
                    {
                        var dataVec0 = Sse2.LoadVector128(pInput + (nuint)(uint)i);
                        var dataVec1 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 8);
                        var dataVec2 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 16);
                        var dataVec3 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 24);

                        var max01 = Sse41.Max(dataVec0, dataVec1);
                        var max23 = Sse41.Max(dataVec2, dataVec3);
                        var max0123 = Sse41.Max(max01, max23);

                        result = Sse41.Max(result, max0123);
                    }

                    result = Sse41.Max(result, Sse2.Shuffle(result.AsInt32(), 0xee).AsUInt16());
                    result = Sse41.Max(result, Sse2.Shuffle(result.AsInt32(), 0x55).AsUInt16());
                    result = Sse41.Max(result, Sse2.ShuffleLow(result, 0x55));

                    max = result.GetElement(0);

                    firstIndex = endAligned;
                }
            }

            for (int i = firstIndex; i < data.Length; i++)
            {
                if (max < data[i]) max = data[i];
            }

            return (ulong)max + 1;
        }

        /// <summary>
        /// Gets the vertex count of the vertex buffer accessed with the indices from the current index buffer, with 32-bit indices.
        /// </summary>
        /// <param name="mm">GPU memory manager</param>
        /// <param name="gpuVa">GPU virtual address of the index buffer</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        /// <returns>Vertex count</returns>
        private unsafe static ulong CountU32(MemoryManager mm, ulong gpuVa, int firstIndex, int indexCount)
        {
            uint max = 0;
            ReadOnlySpan<uint> data = MemoryMarshal.Cast<byte, uint>(mm.GetSpan(gpuVa, (firstIndex + indexCount) * 4));

            if (Avx2.IsSupported)
            {
                fixed (uint* pInput = data)
                {
                    int endAligned = firstIndex + ((data.Length - firstIndex) & ~31);

                    var result = Vector256<uint>.Zero;

                    for (int i = firstIndex; i < endAligned; i += 32)
                    {
                        var dataVec0 = Avx.LoadVector256(pInput + (nuint)(uint)i);
                        var dataVec1 = Avx.LoadVector256(pInput + (nuint)(uint)i + 8);
                        var dataVec2 = Avx.LoadVector256(pInput + (nuint)(uint)i + 16);
                        var dataVec3 = Avx.LoadVector256(pInput + (nuint)(uint)i + 24);

                        var max01 = Avx2.Max(dataVec0, dataVec1);
                        var max23 = Avx2.Max(dataVec2, dataVec3);
                        var max0123 = Avx2.Max(max01, max23);

                        result = Avx2.Max(result, max0123);
                    }

                    result = Avx2.Max(result, Avx2.Shuffle(result, 0xee));
                    result = Avx2.Max(result, Avx2.Shuffle(result, 0x55));

                    max = Math.Max(result.GetElement(0), result.GetElement(4));

                    firstIndex = endAligned;
                }
            }
            else if (Sse41.IsSupported)
            {
                fixed (uint* pInput = data)
                {
                    int endAligned = firstIndex + ((data.Length - firstIndex) & ~15);

                    var result = Vector128<uint>.Zero;

                    for (int i = firstIndex; i < endAligned; i += 16)
                    {
                        var dataVec0 = Sse2.LoadVector128(pInput + (nuint)(uint)i);
                        var dataVec1 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 4);
                        var dataVec2 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 8);
                        var dataVec3 = Sse2.LoadVector128(pInput + (nuint)(uint)i + 12);

                        var max01 = Sse41.Max(dataVec0, dataVec1);
                        var max23 = Sse41.Max(dataVec2, dataVec3);
                        var max0123 = Sse41.Max(max01, max23);

                        result = Sse41.Max(result, max0123);
                    }

                    result = Sse41.Max(result, Sse2.Shuffle(result, 0xee));
                    result = Sse41.Max(result, Sse2.Shuffle(result, 0x55));

                    max = result.GetElement(0);

                    firstIndex = endAligned;
                }
            }

            for (int i = firstIndex; i < data.Length; i++)
            {
                if (max < data[i]) max = data[i];
            }

            return (ulong)max + 1;
        }
    }
}
