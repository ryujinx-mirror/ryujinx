using ARMeilleure.Translation.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation.PTC
{
    [Serializable]
    class PtcJumpTable
    {
        [Serializable]
        private struct TableEntry<TAddress>
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

        private enum DirectHostAddress
        {
            CallStub,
            TailCallStub,
            Host
        }

        private enum IndirectHostAddress
        {
            CallStub,
            TailCallStub
        }

        private readonly List<TableEntry<DirectHostAddress>> _jumpTable;
        private readonly List<TableEntry<IndirectHostAddress>> _dynamicTable;

        private readonly List<ulong> _targets;
        private readonly Dictionary<ulong, List<int>> _dependants;
        private readonly Dictionary<ulong, List<int>> _owners;

        public List<ulong> Targets => _targets;
        public Dictionary<ulong, List<int>> Dependants => _dependants;
        public Dictionary<ulong, List<int>> Owners => _owners;

        public PtcJumpTable()
        {
            _jumpTable = new List<TableEntry<DirectHostAddress>>();
            _dynamicTable = new List<TableEntry<IndirectHostAddress>>();

            _targets = new List<ulong>();
            _dependants = new Dictionary<ulong, List<int>>();
            _owners = new Dictionary<ulong, List<int>>();
        }

        public void Initialize(JumpTable jumpTable)
        {
            _targets.Clear();

            foreach (ulong guestAddress in jumpTable.Targets.Keys)
            {
                _targets.Add(guestAddress);
            }

            _dependants.Clear();

            foreach (var item in jumpTable.Dependants)
            {
                _dependants.Add(item.Key, new List<int>(item.Value));
            }

            _owners.Clear();

            foreach (var item in jumpTable.Owners)
            {
                _owners.Add(item.Key, new List<int>(item.Value));
            }
        }

        public void Clear()
        {
            _jumpTable.Clear();
            _dynamicTable.Clear();

            _targets.Clear();
            _dependants.Clear();
            _owners.Clear();
        }

        public void WriteJumpTable(JumpTable jumpTable, ConcurrentDictionary<ulong, TranslatedFunction> funcs)
        {
            // Writes internal state to jump table in-memory, after PTC was loaded.

            foreach (var item in _jumpTable)
            {
                long guestAddress = item.GuestAddress;
                DirectHostAddress directHostAddress = item.HostAddress;

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
                        throw new KeyNotFoundException($"({nameof(guestAddress)} = 0x{(ulong)guestAddress:X16})");
                    }
                }
                else
                {
                    throw new InvalidOperationException(nameof(directHostAddress));
                }

                int entry = item.EntryIndex;

                jumpTable.Table.SetEntry(entry);
                jumpTable.ExpandIfNeededJumpTable(entry);

                IntPtr addr = jumpTable.GetEntryAddressJumpTable(entry);

                Marshal.WriteInt64(addr, 0, guestAddress);
                Marshal.WriteInt64(addr, 8, hostAddress);
            }
        }

        public void WriteDynamicTable(JumpTable jumpTable)
        {
            // Writes internal state to jump table in-memory, after PTC was loaded.

            if (JumpTable.DynamicTableElems > 1)
            {
                throw new NotSupportedException();
            }

            foreach (var item in _dynamicTable)
            {
                long guestAddress = item.GuestAddress;
                IndirectHostAddress indirectHostAddress = item.HostAddress;

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

                int entry = item.EntryIndex;

                jumpTable.DynTable.SetEntry(entry);
                jumpTable.ExpandIfNeededDynamicTable(entry);

                IntPtr addr = jumpTable.GetEntryAddressDynamicTable(entry);

                Marshal.WriteInt64(addr, 0, guestAddress);
                Marshal.WriteInt64(addr, 8, hostAddress);
            }
        }

        public void ReadJumpTable(JumpTable jumpTable)
        {
            // Reads in-memory jump table state and store internally for PTC serialization.

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
            // Reads in-memory jump table state and store internally for PTC serialization.

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