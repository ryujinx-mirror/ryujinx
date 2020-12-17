using ARMeilleure.Diagnostics;
using ARMeilleure.Memory;
using ARMeilleure.Translation.PTC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation.Cache
{
    class JumpTable : IDisposable
    {
        // The jump table is a block of (guestAddress, hostAddress) function mappings.
        // Each entry corresponds to one branch in a JIT compiled function. The entries are
        // reserved specifically for each call.
        // The Dependants dictionary can be used to update the hostAddress for any functions that change.

        public const int JumpTableStride = 16; // 8 byte guest address, 8 byte host address.

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

        public const int DynamicTableElems = 1;

        public const int DynamicTableStride = DynamicTableElems * JumpTableStride;

        private const int DynamicTableSize = 1048576;
        private const int DynamicTableByteSize = DynamicTableSize * DynamicTableStride;

        public const int DynamicEntryTag = 1 << 31;

        private readonly ReservedRegion _jumpRegion;
        private readonly ReservedRegion _dynamicRegion;

        public IntPtr JumpPointer => _jumpRegion.Pointer;
        public IntPtr DynamicPointer => _dynamicRegion.Pointer;

        public JumpTableEntryAllocator Table { get; }
        public JumpTableEntryAllocator DynTable { get; }

        public ConcurrentDictionary<ulong, TranslatedFunction> Targets { get; }
        public ConcurrentDictionary<ulong, List<int>> Dependants { get; } // TODO: Attach to TranslatedFunction or a wrapper class.
        public ConcurrentDictionary<ulong, List<int>> Owners { get; }

        public JumpTable(IJitMemoryAllocator allocator)
        {
            _jumpRegion = new ReservedRegion(allocator, JumpTableByteSize);
            _dynamicRegion = new ReservedRegion(allocator, DynamicTableByteSize);

            Table = new JumpTableEntryAllocator();
            DynTable = new JumpTableEntryAllocator();

            Targets = new ConcurrentDictionary<ulong, TranslatedFunction>();
            Dependants = new ConcurrentDictionary<ulong, List<int>>();
            Owners = new ConcurrentDictionary<ulong, List<int>>();

            Symbols.Add((ulong)_jumpRegion.Pointer.ToInt64(), JumpTableByteSize, JumpTableStride, "JMP_TABLE");
            Symbols.Add((ulong)_dynamicRegion.Pointer.ToInt64(), DynamicTableByteSize, DynamicTableStride, "DYN_TABLE");
        }

        public void Initialize(PtcJumpTable ptcJumpTable, ConcurrentDictionary<ulong, TranslatedFunction> funcs)
        {
            foreach (ulong guestAddress in ptcJumpTable.Targets)
            {
                if (funcs.TryGetValue(guestAddress, out TranslatedFunction func))
                {
                    Targets.TryAdd(guestAddress, func);
                }
                else
                {
                    throw new KeyNotFoundException($"({nameof(guestAddress)} = 0x{guestAddress:X16})");
                }
            }

            foreach (var kv in ptcJumpTable.Dependants)
            {
                Dependants.TryAdd(kv.Key, new List<int>(kv.Value));
            }

            foreach (var kv in ptcJumpTable.Owners)
            {
                Owners.TryAdd(kv.Key, new List<int>(kv.Value));
            }
        }

        public void RegisterFunction(ulong address, TranslatedFunction func)
        {
            Targets.AddOrUpdate(address, func, (key, oldFunc) => func);
            long funcPtr = func.FuncPtr.ToInt64();

            // Update all jump table entries that target this address.
            if (Dependants.TryGetValue(address, out List<int> myDependants))
            {
                lock (myDependants)
                {
                    foreach (int entry in myDependants)
                    {
                        IntPtr addr = GetEntryAddressJumpTable(entry);

                        Marshal.WriteInt64(addr, 8, funcPtr);
                    }
                }
            }
        }

        public int ReserveTableEntry(ulong ownerGuestAddress, ulong address, bool isJump)
        {
            int entry = Table.AllocateEntry();

            ExpandIfNeededJumpTable(entry);

            // Is the address we have already registered? If so, put the function address in the jump table.
            // If not, it will point to the direct call stub.
            long value = DirectCallStubs.DirectCallStub(isJump).ToInt64();
            if (Targets.TryGetValue(address, out TranslatedFunction func))
            {
                value = func.FuncPtr.ToInt64();
            }

            // Make sure changes to the function at the target address update this jump table entry.
            List<int> targetDependants = Dependants.GetOrAdd(address, (addr) => new List<int>());
            lock (targetDependants)
            {
                targetDependants.Add(entry);
            }

            // Keep track of ownership for jump table entries.
            List<int> ownerEntries = Owners.GetOrAdd(ownerGuestAddress, (addr) => new List<int>());
            lock (ownerEntries)
            {
                ownerEntries.Add(entry);
            }

            IntPtr addr = GetEntryAddressJumpTable(entry);

            Marshal.WriteInt64(addr, 0, (long)address);
            Marshal.WriteInt64(addr, 8, value);

            return entry;
        }

        public int ReserveDynamicEntry(ulong ownerGuestAddress, bool isJump)
        {
            int entry = DynTable.AllocateEntry();

            ExpandIfNeededDynamicTable(entry);

            // Keep track of ownership for jump table entries.
            List<int> ownerEntries = Owners.GetOrAdd(ownerGuestAddress, (addr) => new List<int>());
            lock (ownerEntries)
            {
                ownerEntries.Add(entry | DynamicEntryTag);
            }

            // Initialize all host function pointers to the indirect call stub.
            IntPtr addr = GetEntryAddressDynamicTable(entry);
            long stubPtr = DirectCallStubs.IndirectCallStub(isJump).ToInt64();

            for (int i = 0; i < DynamicTableElems; i++)
            {
                Marshal.WriteInt64(addr, i * JumpTableStride + 8, stubPtr);
            }

            return entry;
        }

        // For future use.
        public void RemoveFunctionEntries(ulong guestAddress)
        {
            Targets.TryRemove(guestAddress, out _);
            Dependants.TryRemove(guestAddress, out _);

            if (Owners.TryRemove(guestAddress, out List<int> entries))
            {
                foreach (int entry in entries)
                {
                    if ((entry & DynamicEntryTag) == 0)
                    {
                        IntPtr addr = GetEntryAddressJumpTable(entry);

                        Marshal.WriteInt64(addr, 0, 0L);
                        Marshal.WriteInt64(addr, 8, 0L);

                        Table.FreeEntry(entry);
                    }
                    else
                    {
                        IntPtr addr = GetEntryAddressDynamicTable(entry & ~DynamicEntryTag);

                        for (int j = 0; j < DynamicTableElems; j++)
                        {
                            Marshal.WriteInt64(addr + j * JumpTableStride, 0, 0L);
                            Marshal.WriteInt64(addr + j * JumpTableStride, 8, 0L);
                        }

                        DynTable.FreeEntry(entry & ~DynamicEntryTag);
                    }
                }
            }
        }

        public void ExpandIfNeededJumpTable(int entry)
        {
            Debug.Assert(entry >= 0);

            if (entry < JumpTableSize)
            {
                _jumpRegion.ExpandIfNeeded((ulong)((entry + 1) * JumpTableStride));
            }
            else
            {
                throw new OutOfMemoryException("JIT Direct Jump Table exhausted.");
            }
        }

        public void ExpandIfNeededDynamicTable(int entry)
        {
            Debug.Assert(entry >= 0);

            if (entry < DynamicTableSize)
            {
                _dynamicRegion.ExpandIfNeeded((ulong)((entry + 1) * DynamicTableStride));
            }
            else
            {
                throw new OutOfMemoryException("JIT Dynamic Jump Table exhausted.");
            }
        }

        public IntPtr GetEntryAddressJumpTable(int entry)
        {
            Debug.Assert(Table.EntryIsValid(entry));

            return _jumpRegion.Pointer + entry * JumpTableStride;
        }

        public IntPtr GetEntryAddressDynamicTable(int entry)
        {
            Debug.Assert(DynTable.EntryIsValid(entry));

            return _dynamicRegion.Pointer + entry * DynamicTableStride;
        }

        public bool CheckEntryFromAddressJumpTable(IntPtr entryAddress)
        {
            int entry = Math.DivRem((int)((ulong)entryAddress - (ulong)_jumpRegion.Pointer), JumpTableStride, out int rem);

            return rem == 0 && Table.EntryIsValid(entry);
        }

        public bool CheckEntryFromAddressDynamicTable(IntPtr entryAddress)
        {
            int entry = Math.DivRem((int)((ulong)entryAddress - (ulong)_dynamicRegion.Pointer), DynamicTableStride, out int rem);

            return rem == 0 && DynTable.EntryIsValid(entry);
        }

        public void Dispose()
        {
            _jumpRegion.Dispose();
            _dynamicRegion.Dispose();
        }
    }
}
