using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ryujinx.Memory
{
    /// <summary>
    /// Represents a block of contiguous physical guest memory.
    /// </summary>
    public sealed class MemoryBlock : IDisposable
    {
        private IntPtr _pointer;

        /// <summary>
        /// Pointer to the memory block data.
        /// </summary>
        public IntPtr Pointer => _pointer;

        /// <summary>
        /// Size of the memory block.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// Initializes a new instance of the memory block class.
        /// </summary>
        /// <param name="size">Size of the memory block</param>
        /// <param name="flags">Flags that controls memory block memory allocation</param>
        /// <exception cref="OutOfMemoryException">Throw when there's no enough memory to allocate the requested size</exception>
        /// <exception cref="PlatformNotSupportedException">Throw when the current platform is not supported</exception>
        public MemoryBlock(ulong size, MemoryAllocationFlags flags = MemoryAllocationFlags.None)
        {
            if (flags.HasFlag(MemoryAllocationFlags.Reserve))
            {
                _pointer = MemoryManagement.Reserve(size);
            }
            else
            {
                _pointer = MemoryManagement.Allocate(size);
            }

            Size = size;
        }

        /// <summary>
        /// Commits a region of memory that has previously been reserved.
        /// This can be used to allocate memory on demand.
        /// </summary>
        /// <param name="offset">Starting offset of the range to be committed</param>
        /// <param name="size">Size of the range to be committed</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        public bool Commit(ulong offset, ulong size)
        {
            return MemoryManagement.Commit(GetPointerInternal(offset, size), size);
        }

        /// <summary>
        /// Reprotects a region of memory.
        /// </summary>
        /// <param name="offset">Starting offset of the range to be reprotected</param>
        /// <param name="size">Size of the range to be reprotected</param>
        /// <param name="permission">New memory permissions</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        /// <exception cref="MemoryProtectionException">Throw when <paramref name="permission"/> is invalid</exception>
        public void Reprotect(ulong offset, ulong size, MemoryPermission permission)
        {
            MemoryManagement.Reprotect(GetPointerInternal(offset, size), size, permission);
        }

        /// <summary>
        /// Reads bytes from the memory block.
        /// </summary>
        /// <param name="offset">Starting offset of the range being read</param>
        /// <param name="data">Span where the bytes being read will be copied to</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when the memory region specified for the the data is out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(ulong offset, Span<byte> data)
        {
            GetSpan(offset, data.Length).CopyTo(data);
        }

        /// <summary>
        /// Reads data from the memory block.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="offset">Offset where the data is located</param>
        /// <returns>Data at the specified address</returns>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when the memory region specified for the the data is out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ulong offset) where T : unmanaged
        {
            return GetRef<T>(offset);
        }

        /// <summary>
        /// Writes bytes to the memory block.
        /// </summary>
        /// <param name="offset">Starting offset of the range being written</param>
        /// <param name="data">Span where the bytes being written will be copied from</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when the memory region specified for the the data is out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong offset, ReadOnlySpan<byte> data)
        {
            data.CopyTo(GetSpan(offset, data.Length));
        }

        /// <summary>
        /// Writes data to the memory block.
        /// </summary>
        /// <typeparam name="T">Type of the data being written</typeparam>
        /// <param name="offset">Offset to write the data into</param>
        /// <param name="data">Data to be written</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when the memory region specified for the the data is out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ulong offset, T data) where T : unmanaged
        {
            GetRef<T>(offset) = data;
        }

        /// <summary>
        /// Copies data from one memory location to another.
        /// </summary>
        /// <param name="dstOffset">Destination offset to write the data into</param>
        /// <param name="srcOffset">Source offset to read the data from</param>
        /// <param name="size">Size of the copy in bytes</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when <paramref name="srcOffset"/>, <paramref name="dstOffset"/> or <paramref name="size"/> is out of range</exception>
        public void Copy(ulong dstOffset, ulong srcOffset, ulong size)
        {
            const int MaxChunkSize = 1 << 24;

            for (ulong offset = 0; offset < size; offset += MaxChunkSize)
            {
                int copySize = (int)Math.Min(MaxChunkSize, size - offset);

                Write(dstOffset + offset, GetSpan(srcOffset + offset, copySize));
            }
        }

        /// <summary>
        /// Fills a region of memory with zeros.
        /// </summary>
        /// <param name="offset">Offset of the region to fill with zeros</param>
        /// <param name="size">Size in bytes of the region to fill</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        public void ZeroFill(ulong offset, ulong size)
        {
            const int MaxChunkSize = 1 << 24;

            for (ulong subOffset = 0; subOffset < size; subOffset += MaxChunkSize)
            {
                int copySize = (int)Math.Min(MaxChunkSize, size - subOffset);

                GetSpan(offset + subOffset, copySize).Fill(0);
            }
        }

        /// <summary>
        /// Gets a reference of the data at a given memory block region.
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="offset">Offset of the memory region</param>
        /// <returns>A reference to the given memory region data</returns>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetRef<T>(ulong offset) where T : unmanaged
        {
            IntPtr ptr = _pointer;

            if (ptr == IntPtr.Zero)
            {
                ThrowObjectDisposed();
            }

            int size = Unsafe.SizeOf<T>();

            ulong endOffset = offset + (ulong)size;

            if (endOffset > Size || endOffset < offset)
            {
                ThrowInvalidMemoryRegionException();
            }

            return ref Unsafe.AsRef<T>((void*)PtrAddr(ptr, offset));
        }

        /// <summary>
        /// Gets the pointer of a given memory block region.
        /// </summary>
        /// <param name="offset">Start offset of the memory region</param>
        /// <param name="size">Size in bytes of the region</param>
        /// <returns>The pointer to the memory region</returns>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetPointer(ulong offset, int size) => GetPointerInternal(offset, (ulong)size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetPointerInternal(ulong offset, ulong size)
        {
            IntPtr ptr = _pointer;

            if (ptr == IntPtr.Zero)
            {
                ThrowObjectDisposed();
            }

            ulong endOffset = offset + size;

            if (endOffset > Size || endOffset < offset)
            {
                ThrowInvalidMemoryRegionException();
            }

            return PtrAddr(ptr, offset);
        }

        /// <summary>
        /// Gets the <see cref="Span{T}"/> of a given memory block region.
        /// </summary>
        /// <param name="offset">Start offset of the memory region</param>
        /// <param name="size">Size in bytes of the region</param>
        /// <returns>Span of the memory region</returns>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<byte> GetSpan(ulong offset, int size)
        {
            return new Span<byte>((void*)GetPointer(offset, size), size);
        }

        /// <summary>
        /// Gets the <see cref="Memory{T}"/> of a given memory block region.
        /// </summary>
        /// <param name="offset">Start offset of the memory region</param>
        /// <param name="size">Size in bytes of the region</param>
        /// <returns>Memory of the memory region</returns>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="InvalidMemoryRegionException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Memory<byte> GetMemory(ulong offset, int size)
        {
            return new NativeMemoryManager<byte>((byte*)GetPointer(offset, size), size).Memory;
        }

        /// <summary>
        /// Adds a 64-bits offset to a native pointer.
        /// </summary>
        /// <param name="pointer">Native pointer</param>
        /// <param name="offset">Offset to add</param>
        /// <returns>Native pointer with the added offset</returns>
        private IntPtr PtrAddr(IntPtr pointer, ulong offset)
        {
            return (IntPtr)(pointer.ToInt64() + (long)offset);
        }

        /// <summary>
        /// Frees the memory allocated for this memory block.
        /// </summary>
        /// <remarks>
        /// It's an error to use the memory block after disposal.
        /// </remarks>
        public void Dispose() => FreeMemory();

        ~MemoryBlock() => FreeMemory();

        private void FreeMemory()
        {
            IntPtr ptr = Interlocked.Exchange(ref _pointer, IntPtr.Zero);

            // If pointer is null, the memory was already freed or never allocated.
            if (ptr != IntPtr.Zero)
            {
                MemoryManagement.Free(ptr);
            }
        }

        private void ThrowObjectDisposed() => throw new ObjectDisposedException(nameof(MemoryBlock));
        private void ThrowInvalidMemoryRegionException() => throw new InvalidMemoryRegionException();
    }
}
