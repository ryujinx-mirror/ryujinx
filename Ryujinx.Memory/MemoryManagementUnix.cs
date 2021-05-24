using Mono.Unix.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory
{
    static class MemoryManagementUnix
    {
        private struct UnixSharedMemory
        {
            public IntPtr Pointer;
            public ulong Size;
            public IntPtr SourcePointer;
        }

        [DllImport("libc", SetLastError = true)]
        public static extern IntPtr mremap(IntPtr old_address, ulong old_size, ulong new_size, MremapFlags flags, IntPtr new_address);

        [DllImport("libc", SetLastError = true)]
        public static extern int madvise(IntPtr address, ulong size, int advice);

        private const int MADV_DONTNEED = 4;
        private const int MADV_REMOVE = 9;

        private static readonly List<UnixSharedMemory> _sharedMemory = new List<UnixSharedMemory>();
        private static readonly ConcurrentDictionary<IntPtr, ulong> _sharedMemorySource = new ConcurrentDictionary<IntPtr, ulong>();
        private static readonly ConcurrentDictionary<IntPtr, ulong> _allocations = new ConcurrentDictionary<IntPtr, ulong>();

        public static IntPtr Allocate(ulong size)
        {
            return AllocateInternal(size, MmapProts.PROT_READ | MmapProts.PROT_WRITE);
        }

        public static IntPtr Reserve(ulong size)
        {
            return AllocateInternal(size, MmapProts.PROT_NONE);
        }

        private static IntPtr AllocateInternal(ulong size, MmapProts prot, bool shared = false)
        {
            MmapFlags flags = MmapFlags.MAP_ANONYMOUS;

            if (shared)
            {
                flags |= MmapFlags.MAP_SHARED | (MmapFlags)0x80000;
            }
            else
            {
                flags |= MmapFlags.MAP_PRIVATE;
            }

            if (prot == MmapProts.PROT_NONE)
            {
                flags |= MmapFlags.MAP_NORESERVE;
            }

            IntPtr ptr = Syscall.mmap(IntPtr.Zero, size, prot, flags, -1, 0);

            if (ptr == new IntPtr(-1L))
            {
                throw new OutOfMemoryException();
            }

            if (!_allocations.TryAdd(ptr, size))
            {
                // This should be impossible, kernel shouldn't return an already mapped address.
                throw new InvalidOperationException();
            }

            return ptr;
        }

        public static bool Commit(IntPtr address, ulong size)
        {
            bool success = Syscall.mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE) == 0;

            if (success)
            {
                foreach (var shared in _sharedMemory)
                {
                    if ((ulong)address + size > (ulong)shared.SourcePointer && (ulong)address < (ulong)shared.SourcePointer + shared.Size)
                    {
                        ulong sharedAddress = ((ulong)address - (ulong)shared.SourcePointer) + (ulong)shared.Pointer;

                        if (Syscall.mprotect((IntPtr)sharedAddress, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE) != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return success;
        }

        public static bool Decommit(IntPtr address, ulong size)
        {
            bool isShared;

            lock (_sharedMemory)
            {
                isShared = _sharedMemory.Exists(x => (ulong)address >= (ulong)x.Pointer && (ulong)address + size <= (ulong)x.Pointer + x.Size);
            }

            // Must be writable for madvise to work properly.
            Syscall.mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE);

            madvise(address, size, isShared ? MADV_REMOVE : MADV_DONTNEED);

            return Syscall.mprotect(address, size, MmapProts.PROT_NONE) == 0;
        }

        public static bool Reprotect(IntPtr address, ulong size, MemoryPermission permission)
        {
            return Syscall.mprotect(address, size, GetProtection(permission)) == 0;
        }

        private static MmapProts GetProtection(MemoryPermission permission)
        {
            return permission switch
            {
                MemoryPermission.None => MmapProts.PROT_NONE,
                MemoryPermission.Read => MmapProts.PROT_READ,
                MemoryPermission.ReadAndWrite => MmapProts.PROT_READ | MmapProts.PROT_WRITE,
                MemoryPermission.ReadAndExecute => MmapProts.PROT_READ | MmapProts.PROT_EXEC,
                MemoryPermission.ReadWriteExecute => MmapProts.PROT_READ | MmapProts.PROT_WRITE | MmapProts.PROT_EXEC,
                MemoryPermission.Execute => MmapProts.PROT_EXEC,
                _ => throw new MemoryProtectionException(permission)
            };
        }

        public static bool Free(IntPtr address)
        {
            if (_allocations.TryRemove(address, out ulong size))
            {
                return Syscall.munmap(address, size) == 0;
            }

            return false;
        }

        public static IntPtr Remap(IntPtr target, IntPtr source, ulong size)
        {
            int flags = (int)MremapFlags.MREMAP_MAYMOVE;

            if (target != IntPtr.Zero)
            {
                flags |= 2;
            }

            IntPtr result = mremap(source, 0, size, (MremapFlags)(flags), target);

            if (result == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            return result;
        }

        public static IntPtr CreateSharedMemory(ulong size, bool reserve)
        {
            IntPtr result = AllocateInternal(
                size,
                reserve ? MmapProts.PROT_NONE : MmapProts.PROT_READ | MmapProts.PROT_WRITE,
                true);

            if (result == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            _sharedMemorySource[result] = (ulong)size;

            return result;
        }

        public static void DestroySharedMemory(IntPtr handle)
        {
            lock (_sharedMemory)
            {
                foreach (var memory in _sharedMemory)
                {
                    if (memory.SourcePointer == handle)
                    {
                        throw new InvalidOperationException("Shared memory cannot be destroyed unless fully unmapped.");
                    }
                }
            }

            _sharedMemorySource.Remove(handle, out ulong _);
        }

        public static IntPtr MapSharedMemory(IntPtr handle)
        {
            // Try find the handle for this shared memory. If it is mapped, then we want to map
            // it a second time in another location.
            // If it is not mapped, then its handle is the mapping.

            ulong size = _sharedMemorySource[handle];

            if (size == 0)
            {
                throw new InvalidOperationException("Shared memory cannot be mapped after its source is unmapped.");
            }

            lock (_sharedMemory)
            {
                foreach (var memory in _sharedMemory)
                {
                    if (memory.Pointer == handle)
                    {
                        IntPtr result = AllocateInternal(
                            memory.Size,
                            MmapProts.PROT_NONE
                            );

                        if (result == IntPtr.Zero)
                        {
                            throw new OutOfMemoryException();
                        }

                        Remap(result, handle, memory.Size);

                        _sharedMemory.Add(new UnixSharedMemory
                        {
                            Pointer = result,
                            Size = memory.Size,

                            SourcePointer = handle
                        });

                        return result;
                    }
                }

                _sharedMemory.Add(new UnixSharedMemory
                {
                    Pointer = handle,
                    Size = size,

                    SourcePointer = handle
                });
            }

            return handle;
        }

        public static void UnmapSharedMemory(IntPtr address)
        {
            lock (_sharedMemory)
            {
                int removed = _sharedMemory.RemoveAll(memory =>
                {
                    if (memory.Pointer == address)
                    {
                        if (memory.Pointer == memory.SourcePointer)
                        {
                            // After removing the original mapping, it cannot be mapped again.
                            _sharedMemorySource[memory.SourcePointer] = 0;
                        }

                        Free(address);
                        return true;
                    }

                    return false;
                });

                if (removed == 0)
                {
                    throw new InvalidOperationException("Shared memory mapping could not be found.");
                }
            }
        }
    }
}