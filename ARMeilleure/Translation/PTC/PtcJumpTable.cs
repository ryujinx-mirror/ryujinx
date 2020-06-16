using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation.PTC
{
    [Serializable]
    class PtcJumpTable
    {
        private readonly List<KeyValuePair<long, DirectHostAddress>>   _jumpTable;
        private readonly List<KeyValuePair<long, IndirectHostAddress>> _dynamicTable;

        private readonly List<ulong> _targets;
        private readonly Dictionary<ulong, LinkedList<int>> _dependants;

        public int TableEnd    => _jumpTable.Count;
        public int DynTableEnd => _dynamicTable.Count;

        public List<ulong> Targets => _targets;
        public Dictionary<ulong, LinkedList<int>> Dependants => _dependants;

        public PtcJumpTable()
        {
            _jumpTable    = new List<KeyValuePair<long, DirectHostAddress>>();
            _dynamicTable = new List<KeyValuePair<long, IndirectHostAddress>>();

            _targets    = new List<ulong>();
            _dependants = new Dictionary<ulong, LinkedList<int>>();
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
                _dependants.Add(item.Key, new LinkedList<int>(item.Value));
            }
        }

        public void Clear()
        {
            _jumpTable.Clear();
            _dynamicTable.Clear();

            _targets.Clear();
            _dependants.Clear();
        }

        public void WriteJumpTable(JumpTable jumpTable, ConcurrentDictionary<ulong, TranslatedFunction> funcs)
        {
            jumpTable.ExpandIfNeededJumpTable(TableEnd);

            int entry = 0;

            foreach (var item in _jumpTable)
            {
                entry += 1;

                long guestAddress = item.Key;
                DirectHostAddress directHostAddress = item.Value;

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

                IntPtr addr = jumpTable.GetEntryAddressJumpTable(entry);

                Marshal.WriteInt64(addr, 0, guestAddress);
                Marshal.WriteInt64(addr, 8, hostAddress);
            }
        }

        public void WriteDynamicTable(JumpTable jumpTable)
        {
            if (JumpTable.DynamicTableElems > 1)
            {
                throw new NotSupportedException();
            }

            jumpTable.ExpandIfNeededDynamicTable(DynTableEnd);

            int entry = 0;

            foreach (var item in _dynamicTable)
            {
                entry += 1;

                long guestAddress = item.Key;
                IndirectHostAddress indirectHostAddress = item.Value;

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

                IntPtr addr = jumpTable.GetEntryAddressDynamicTable(entry);

                Marshal.WriteInt64(addr, 0, guestAddress);
                Marshal.WriteInt64(addr, 8, hostAddress);
            }
        }

        public void ReadJumpTable(JumpTable jumpTable)
        {
            _jumpTable.Clear();

            for (int entry = 1; entry <= jumpTable.TableEnd; entry++)
            {
                IntPtr addr = jumpTable.GetEntryAddressJumpTable(entry);

                long guestAddress = Marshal.ReadInt64(addr, 0);
                long hostAddress  = Marshal.ReadInt64(addr, 8);

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

                _jumpTable.Add(new KeyValuePair<long, DirectHostAddress>(guestAddress, directHostAddress));
            }
        }

        public void ReadDynamicTable(JumpTable jumpTable)
        {
            if (JumpTable.DynamicTableElems > 1)
            {
                throw new NotSupportedException();
            }

            _dynamicTable.Clear();

            for (int entry = 1; entry <= jumpTable.DynTableEnd; entry++)
            {
                IntPtr addr = jumpTable.GetEntryAddressDynamicTable(entry);

                long guestAddress = Marshal.ReadInt64(addr, 0);
                long hostAddress  = Marshal.ReadInt64(addr, 8);

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

                _dynamicTable.Add(new KeyValuePair<long, IndirectHostAddress>(guestAddress, indirectHostAddress));
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
    }
}