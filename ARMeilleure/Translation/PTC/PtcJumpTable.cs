using ARMeilleure.Translation.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation.PTC
{
    class PtcJumpTable
    {
        public struct TableEntry<TAddress>
        {
            public int EntryIndex;
            public long GuestAddress;
            public TAddress HostAddress;

            public TableEntry(int entryIndex, long guestAddress, TAddress hostAddress)
            {
                EntryIndex = entryIndex;
                GuestAddress = guestAddress;
                HostAddress = hostAddress;
            }
        }

        public enum DirectHostAddress
        {
            CallStub = 0,
            TailCallStub = 1,
            Host = 2
        }

        public enum IndirectHostAddress
        {
            CallStub = 0,
            TailCallStub = 1
        }

        private readonly List<TableEntry<DirectHostAddress>> _jumpTable;
        private readonly List<TableEntry<IndirectHostAddress>> _dynamicTable;

        public List<ulong> Targets { get; }
        public Dictionary<ulong, List<int>> Dependants { get; }
        public Dictionary<ulong, List<int>> Owners { get; }

        public PtcJumpTable()
        {
            _jumpTable = new List<TableEntry<DirectHostAddress>>();
            _dynamicTable = new List<TableEntry<IndirectHostAddress>>();

            Targets = new List<ulong>();
            Dependants = new Dictionary<ulong, List<int>>();
            Owners = new Dictionary<ulong, List<int>>();
        }

        public PtcJumpTable(
            List<TableEntry<DirectHostAddress>> jumpTable, List<TableEntry<IndirectHostAddress>> dynamicTable,
            List<ulong> targets, Dictionary<ulong, List<int>> dependants, Dictionary<ulong, List<int>> owners)
        {
            _jumpTable = jumpTable;
            _dynamicTable = dynamicTable;

            Targets = targets;
            Dependants = dependants;
            Owners = owners;
        }

        public static PtcJumpTable Deserialize(MemoryStream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, EncodingCache.UTF8NoBOM, true))
            {
                var jumpTable = new List<TableEntry<DirectHostAddress>>();

                int jumpTableCount = reader.ReadInt32();

                for (int i = 0; i < jumpTableCount; i++)
                {
                    int entryIndex = reader.ReadInt32();
                    long guestAddress = reader.ReadInt64();
                    DirectHostAddress hostAddress = (DirectHostAddress)reader.ReadInt32();

                    jumpTable.Add(new TableEntry<DirectHostAddress>(entryIndex, guestAddress, hostAddress));
                }

                var dynamicTable = new List<TableEntry<IndirectHostAddress>>();

                int dynamicTableCount = reader.ReadInt32();

                for (int i = 0; i < dynamicTableCount; i++)
                {
                    int entryIndex = reader.ReadInt32();
                    long guestAddress = reader.ReadInt64();
                    IndirectHostAddress hostAddress = (IndirectHostAddress)reader.ReadInt32();

                    dynamicTable.Add(new TableEntry<IndirectHostAddress>(entryIndex, guestAddress, hostAddress));
                }

                var targets = new List<ulong>();

                int targetsCount = reader.ReadInt32();

                for (int i = 0; i < targetsCount; i++)
                {
                    ulong address = reader.ReadUInt64();

                    targets.Add(address);
                }

                var dependants = new Dictionary<ulong, List<int>>();

                int dependantsCount = reader.ReadInt32();

                for (int i = 0; i < dependantsCount; i++)
                {
                    ulong address = reader.ReadUInt64();

                    var entries = new List<int>();

                    int entriesCount = reader.ReadInt32();

                    for (int j = 0; j < entriesCount; j++)
                    {
                        int entry = reader.ReadInt32();

                        entries.Add(entry);
                    }

                    dependants.Add(address, entries);
                }

                var owners = new Dictionary<ulong, List<int>>();

                int ownersCount = reader.ReadInt32();

                for (int i = 0; i < ownersCount; i++)
                {
                    ulong address = reader.ReadUInt64();

                    var entries = new List<int>();

                    int entriesCount = reader.ReadInt32();

                    for (int j = 0; j < entriesCount; j++)
                    {
                        int entry = reader.ReadInt32();

                        entries.Add(entry);
                    }

                    owners.Add(address, entries);
                }

                return new PtcJumpTable(jumpTable, dynamicTable, targets, dependants, owners);
            }
        }

        public static void Serialize(MemoryStream stream, PtcJumpTable ptcJumpTable)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, EncodingCache.UTF8NoBOM, true))
            {
                writer.Write((int)ptcJumpTable._jumpTable.Count);

                foreach (var tableEntry in ptcJumpTable._jumpTable)
                {
                    writer.Write((int)tableEntry.EntryIndex);
                    writer.Write((long)tableEntry.GuestAddress);
                    writer.Write((int)tableEntry.HostAddress);
                }

                writer.Write((int)ptcJumpTable._dynamicTable.Count);

                foreach (var tableEntry in ptcJumpTable._dynamicTable)
                {
                    writer.Write((int)tableEntry.EntryIndex);
                    writer.Write((long)tableEntry.GuestAddress);
                    writer.Write((int)tableEntry.HostAddress);
                }

                writer.Write((int)ptcJumpTable.Targets.Count);

                foreach (ulong address in ptcJumpTable.Targets)
                {
                    writer.Write((ulong)address);
                }

                writer.Write((int)ptcJumpTable.Dependants.Count);

                foreach (var kv in ptcJumpTable.Dependants)
                {
                    writer.Write((ulong)kv.Key); // address

                    writer.Write((int)kv.Value.Count);

                    foreach (int entry in kv.Value)
                    {
                        writer.Write((int)entry);
                    }
                }

                writer.Write((int)ptcJumpTable.Owners.Count);

                foreach (var kv in ptcJumpTable.Owners)
                {
                    writer.Write((ulong)kv.Key); // address

                    writer.Write((int)kv.Value.Count);

                    foreach (int entry in kv.Value)
                    {
                        writer.Write((int)entry);
                    }
                }
            }
        }

        public void Initialize(JumpTable jumpTable)
        {
            Targets.Clear();

            foreach (ulong guestAddress in jumpTable.Targets.Keys)
            {
                Targets.Add(guestAddress);
            }

            Dependants.Clear();

            foreach (var kv in jumpTable.Dependants)
            {
                Dependants.Add(kv.Key, new List<int>(kv.Value));
            }

            Owners.Clear();

            foreach (var kv in jumpTable.Owners)
            {
                Owners.Add(kv.Key, new List<int>(kv.Value));
            }
        }

        // For future use.
        public void Clean(ulong guestAddress)
        {
            if (Owners.TryGetValue(guestAddress, out List<int> entries))
            {
                foreach (int entry in entries)
                {
                    if ((entry & JumpTable.DynamicEntryTag) == 0)
                    {
                        int removed = _jumpTable.RemoveAll(tableEntry => tableEntry.EntryIndex == entry);

                        Debug.Assert(removed == 1);
                    }
                    else
                    {
                        if (JumpTable.DynamicTableElems > 1)
                        {
                            throw new NotSupportedException();
                        }

                        int removed = _dynamicTable.RemoveAll(tableEntry => tableEntry.EntryIndex == (entry & ~JumpTable.DynamicEntryTag));

                        Debug.Assert(removed == 1);
                    }
                }
            }

            Targets.Remove(guestAddress);
            Dependants.Remove(guestAddress);
            Owners.Remove(guestAddress);
        }

        public void Clear()
        {
            _jumpTable.Clear();
            _dynamicTable.Clear();

            Targets.Clear();
            Dependants.Clear();
            Owners.Clear();
        }

        public void WriteJumpTable(JumpTable jumpTable, ConcurrentDictionary<ulong, TranslatedFunction> funcs)
        {
            // Writes internal state to jump table in-memory, after PtcJumpTable was deserialized.

            foreach (var tableEntry in _jumpTable)
            {
                long guestAddress = tableEntry.GuestAddress;
                DirectHostAddress directHostAddress = tableEntry.HostAddress;

                long hostAddress;

                if (directHostAddress == DirectHostAddress.CallStub)
                {
                    hostAddress = DirectCallStubs.DirectCallStub(false).ToInt64();
                }
                else if (directHostAddress == DirectHostAddress.TailCallStub)
                {
                    hostAddress = DirectCallStubs.DirectCallStub(true).ToInt64();
                }
                else if (directHostAddress == DirectHostAddress.Host)
                {
                    if (funcs.TryGetValue((ulong)guestAddress, out TranslatedFunction func))
                    {
                        hostAddress = func.FuncPtr.ToInt64();
                    }
                    else
                    {
                        if (!PtcProfiler.ProfiledFuncs.TryGetValue((ulong)guestAddress, out var value) || !value.highCq)
                        {
                            throw new KeyNotFoundException($"({nameof(guestAddress)} = 0x{(ulong)guestAddress:X16})");
                        }

                        hostAddress = 0L;
                    }
                }
                else
                {
                    throw new InvalidOperationException(nameof(directHostAddress));
                }

                int entry = tableEntry.EntryIndex;

                jumpTable.Table.SetEntry(entry);
                jumpTable.ExpandIfNeededJumpTable(entry);

                IntPtr addr = jumpTable.GetEntryAddressJumpTable(entry);

                Marshal.WriteInt64(addr, 0, guestAddress);
                Marshal.WriteInt64(addr, 8, hostAddress);
            }
        }

        public void WriteDynamicTable(JumpTable jumpTable)
        {
            // Writes internal state to jump table in-memory, after PtcJumpTable was deserialized.

            if (JumpTable.DynamicTableElems > 1)
            {
                throw new NotSupportedException();
            }

            foreach (var tableEntry in _dynamicTable)
            {
                long guestAddress = tableEntry.GuestAddress;
                IndirectHostAddress indirectHostAddress = tableEntry.HostAddress;

                long hostAddress;

                if (indirectHostAddress == IndirectHostAddress.CallStub)
                {
                    hostAddress = DirectCallStubs.IndirectCallStub(false).ToInt64();
                }
                else if (indirectHostAddress == IndirectHostAddress.TailCallStub)
                {
                    hostAddress = DirectCallStubs.IndirectCallStub(true).ToInt64();
                }
                else
                {
                    throw new InvalidOperationException(nameof(indirectHostAddress));
                }

                int entry = tableEntry.EntryIndex;

                jumpTable.DynTable.SetEntry(entry);
                jumpTable.ExpandIfNeededDynamicTable(entry);

                IntPtr addr = jumpTable.GetEntryAddressDynamicTable(entry);

                Marshal.WriteInt64(addr, 0, guestAddress);
                Marshal.WriteInt64(addr, 8, hostAddress);
            }
        }

        public void ReadJumpTable(JumpTable jumpTable)
        {
            // Reads in-memory jump table state and store internally for PtcJumpTable serialization.

            _jumpTable.Clear();

            IEnumerable<int> entries = jumpTable.Table.GetEntries();

            foreach (int entry in entries)
            {
                IntPtr addr = jumpTable.GetEntryAddressJumpTable(entry);

                long guestAddress = Marshal.ReadInt64(addr, 0);
                long hostAddress = Marshal.ReadInt64(addr, 8);

                DirectHostAddress directHostAddress;

                if (hostAddress == DirectCallStubs.DirectCallStub(false).ToInt64())
                {
                    directHostAddress = DirectHostAddress.CallStub;
                }
                else if (hostAddress == DirectCallStubs.DirectCallStub(true).ToInt64())
                {
                    directHostAddress = DirectHostAddress.TailCallStub;
                }
                else
                {
                    directHostAddress = DirectHostAddress.Host;
                }

                _jumpTable.Add(new TableEntry<DirectHostAddress>(entry, guestAddress, directHostAddress));
            }
        }

        public void ReadDynamicTable(JumpTable jumpTable)
        {
            // Reads in-memory jump table state and store internally for PtcJumpTable serialization.

            if (JumpTable.DynamicTableElems > 1)
            {
                throw new NotSupportedException();
            }

            _dynamicTable.Clear();

            IEnumerable<int> entries = jumpTable.DynTable.GetEntries();

            foreach (int entry in entries)
            {
                IntPtr addr = jumpTable.GetEntryAddressDynamicTable(entry);

                long guestAddress = Marshal.ReadInt64(addr, 0);
                long hostAddress = Marshal.ReadInt64(addr, 8);

                IndirectHostAddress indirectHostAddress;

                if (hostAddress == DirectCallStubs.IndirectCallStub(false).ToInt64())
                {
                    indirectHostAddress = IndirectHostAddress.CallStub;
                }
                else if (hostAddress == DirectCallStubs.IndirectCallStub(true).ToInt64())
                {
                    indirectHostAddress = IndirectHostAddress.TailCallStub;
                }
                else
                {
                    throw new InvalidOperationException($"({nameof(hostAddress)} = 0x{hostAddress:X16})");
                }

                _dynamicTable.Add(new TableEntry<IndirectHostAddress>(entry, guestAddress, indirectHostAddress));
            }
        }
    }
}