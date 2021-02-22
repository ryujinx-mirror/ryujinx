using ARMeilleure.Translation.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using static ARMeilleure.Translation.PTC.PtcFormatter;

namespace ARMeilleure.Translation.PTC
{
    class PtcJumpTable
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 16*/)]
        public struct TableEntry<TAddress>
        {
            public int EntryIndex;
            public long GuestAddress;
            public TAddress HostAddress; // int

            public TableEntry(int entryIndex, long guestAddress, TAddress hostAddress)
            {
                EntryIndex = entryIndex;
                GuestAddress = guestAddress;
                HostAddress = hostAddress;
            }
        }

        public enum DirectHostAddress : int
        {
            CallStub = 0,
            TailCallStub = 1,
            Host = 2
        }

        public enum IndirectHostAddress : int
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

        public static PtcJumpTable Deserialize(Stream stream)
        {
            var jumpTable = DeserializeList<TableEntry<DirectHostAddress>>(stream);
            var dynamicTable = DeserializeList<TableEntry<IndirectHostAddress>>(stream);

            var targets = DeserializeList<ulong>(stream);
            var dependants = DeserializeDictionary<ulong, List<int>>(stream, (stream) => DeserializeList<int>(stream));
            var owners = DeserializeDictionary<ulong, List<int>>(stream, (stream) => DeserializeList<int>(stream));

            return new PtcJumpTable(jumpTable, dynamicTable, targets, dependants, owners);
        }

        public static int GetSerializeSize(PtcJumpTable ptcJumpTable)
        {
            int size = 0;

            size += GetSerializeSizeList(ptcJumpTable._jumpTable);
            size += GetSerializeSizeList(ptcJumpTable._dynamicTable);

            size += GetSerializeSizeList(ptcJumpTable.Targets);
            size += GetSerializeSizeDictionary(ptcJumpTable.Dependants, (list) => GetSerializeSizeList(list));
            size += GetSerializeSizeDictionary(ptcJumpTable.Owners, (list) => GetSerializeSizeList(list));

            return size;
        }

        public static void Serialize(Stream stream, PtcJumpTable ptcJumpTable)
        {
            SerializeList(stream, ptcJumpTable._jumpTable);
            SerializeList(stream, ptcJumpTable._dynamicTable);

            SerializeList(stream, ptcJumpTable.Targets);
            SerializeDictionary(stream, ptcJumpTable.Dependants, (stream, list) => SerializeList(stream, list));
            SerializeDictionary(stream, ptcJumpTable.Owners, (stream, list) => SerializeList(stream, list));
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

        public void ClearIfNeeded()
        {
            if (_jumpTable.Count == 0 && _dynamicTable.Count == 0 &&
                Targets.Count == 0 && Dependants.Count == 0 && Owners.Count == 0)
            {
                return;
            }

            _jumpTable.Clear();
            _jumpTable.TrimExcess();
            _dynamicTable.Clear();
            _dynamicTable.TrimExcess();

            Targets.Clear();
            Targets.TrimExcess();
            Dependants.Clear();
            Dependants.TrimExcess();
            Owners.Clear();
            Owners.TrimExcess();
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
                        if (!PtcProfiler.ProfiledFuncs.TryGetValue((ulong)guestAddress, out var value) || !value.HighCq)
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