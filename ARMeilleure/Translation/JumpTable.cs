using ARMeilleure.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace ARMeilleure.Translation
{
    class JumpTable
    {
        // The jump table is a block of (guestAddress, hostAddress) function mappings.
        // Each entry corresponds to one branch in a JIT compiled function. The entries are
        // reserved specifically for each call.
        // The _dependants dictionary can be used to update the hostAddress for any functions that change.

        public const int JumpTableStride = 16; // 8 byte guest address, 8 byte host address

        private const int JumpTableSize = 1048576;

        private const int JumpTableByteSize = JumpTableSize * JumpTableStride;

        // The dynamic table is also a block of (guestAddress, hostAddress) function mappings.
        // The main difference is that indirect calls and jumps reserve _multiple_ entries on the table.
        // These start out as all 0. When an indirect call is made, it tries to find the guest address on the table.

        // If we get to an empty address, the guestAddress is set to the call that we want.

        // If we get to a guestAddress that matches our own (or we just claimed it), the hostAddress is read.
        // If it is non-zero, we immediately branch or call the host function.
        // If it is 0, NativeInterface is called to find the rejited address of the call.
        // If none is found, the hostAddress entry stays at 0. Otherwise, the new address is placed in the entry.

        // If the table size is exhausted and we didn't find our desired address, we fall back to requesting 
        // the function from the JIT.

        private const int DynamicTableSize = 1048576;

        public const int DynamicTableElems = 1;

        public const int DynamicTableStride = DynamicTableElems * JumpTableStride;

        private const int DynamicTableByteSize = DynamicTableSize * JumpTableStride * DynamicTableElems;

        private int _tableEnd = 0;
        private int _dynTableEnd = 0;

        private ConcurrentDictionary<ulong, TranslatedFunction> _targets;
        private ConcurrentDictionary<ulong, LinkedList<int>> _dependants; // TODO: Attach to TranslatedFunction or a wrapper class.

        private ReservedRegion _jumpRegion;
        private ReservedRegion _dynamicRegion;
        public IntPtr JumpPointer => _jumpRegion.Pointer;
        public IntPtr DynamicPointer => _dynamicRegion.Pointer;

        public JumpTable(IJitMemoryAllocator allocator)
        {
            _jumpRegion = new ReservedRegion(allocator, JumpTableByteSize);
            _dynamicRegion = new ReservedRegion(allocator, DynamicTableByteSize);

            _targets = new ConcurrentDictionary<ulong, TranslatedFunction>();
            _dependants = new ConcurrentDictionary<ulong, LinkedList<int>>();
        }

        public void RegisterFunction(ulong address, TranslatedFunction func)
        {
            address &= ~3UL;
            _targets.AddOrUpdate(address, func, (key, oldFunc) => func);
            long funcPtr = func.GetPointer().ToInt64();

            // Update all jump table entries that target this address.
            if (_dependants.TryGetValue(address, out LinkedList<int> myDependants))
            {
                lock (myDependants)
                {
                    foreach (var entry in myDependants)
                    {
                        IntPtr addr = _jumpRegion.Pointer + entry * JumpTableStride;
                        Marshal.WriteInt64(addr, 8, funcPtr);
                    }
                }
            }
        }

        public int ReserveDynamicEntry(bool isJump)
        {
            int entry = Interlocked.Increment(ref _dynTableEnd);
            if (entry >= DynamicTableSize)
            {
                throw new OutOfMemoryException("JIT Dynamic Jump Table exhausted.");
            }

            _dynamicRegion.ExpandIfNeeded((ulong)((entry + 1) * DynamicTableStride));

            // Initialize all host function pointers to the indirect call stub.

            IntPtr addr = _dynamicRegion.Pointer + entry * DynamicTableStride;
            long stubPtr = (long)DirectCallStubs.IndirectCallStub(isJump);

            for (int i = 0; i < DynamicTableElems; i++)
            {
                Marshal.WriteInt64(addr, i * JumpTableStride + 8, stubPtr);
            }

            return entry;
        }

        public int ReserveTableEntry(long ownerAddress, long address, bool isJump)
        {
            int entry = Interlocked.Increment(ref _tableEnd);
            if (entry >= JumpTableSize)
            {
                throw new OutOfMemoryException("JIT Direct Jump Table exhausted.");
            }

            _jumpRegion.ExpandIfNeeded((ulong)((entry + 1) * JumpTableStride));

            // Is the address we have already registered? If so, put the function address in the jump table.
            // If not, it will point to the direct call stub.
            long value = (long)DirectCallStubs.DirectCallStub(isJump);
            if (_targets.TryGetValue((ulong)address, out TranslatedFunction func))
            {
                value = func.GetPointer().ToInt64();
            }

            // Make sure changes to the function at the target address update this jump table entry.
            LinkedList<int> targetDependants = _dependants.GetOrAdd((ulong)address, (addr) => new LinkedList<int>());
            lock (targetDependants)
            {
                targetDependants.AddLast(entry);
            }

            IntPtr addr = _jumpRegion.Pointer + entry * JumpTableStride;

            Marshal.WriteInt64(addr, 0, address);
            Marshal.WriteInt64(addr, 8, value);

            return entry;
        }
    }
}
