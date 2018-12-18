using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class KHandleTable
    {
        private const int SelfThreadHandle  = (0x1ffff << 15) | 0;
        private const int SelfProcessHandle = (0x1ffff << 15) | 1;

        private Horizon _system;

        private KHandleEntry[] _table;

        private KHandleEntry _tableHead;
        private KHandleEntry _nextFreeEntry;

        private int _activeSlotsCount;

        private int _size;

        private ushort _idCounter;

        public KHandleTable(Horizon system)
        {
            _system = system;
        }

        public KernelResult Initialize(int size)
        {
            if ((uint)size > 1024)
            {
                return KernelResult.OutOfMemory;
            }

            if (size < 1)
            {
                size = 1024;
            }

            _size = size;

            _idCounter = 1;

            _table = new KHandleEntry[size];

            _tableHead = new KHandleEntry(0);

            KHandleEntry entry = _tableHead;

            for (int index = 0; index < size; index++)
            {
                _table[index] = entry;

                entry.Next = new KHandleEntry(index + 1);

                entry = entry.Next;
            }

            _table[size - 1].Next = null;

            _nextFreeEntry = _tableHead;

            return KernelResult.Success;
        }

        public KernelResult GenerateHandle(object obj, out int handle)
        {
            handle = 0;

            lock (_table)
            {
                if (_activeSlotsCount >= _size)
                {
                    return KernelResult.HandleTableFull;
                }

                KHandleEntry entry = _nextFreeEntry;

                _nextFreeEntry = entry.Next;

                entry.Obj      = obj;
                entry.HandleId = _idCounter;

                _activeSlotsCount++;

                handle = (int)((_idCounter << 15) & 0xffff8000) | entry.Index;

                if ((short)(_idCounter + 1) >= 0)
                {
                    _idCounter++;
                }
                else
                {
                    _idCounter = 1;
                }
            }

            return KernelResult.Success;
        }

        public bool CloseHandle(int handle)
        {
            if ((handle >> 30) != 0 ||
                handle == SelfThreadHandle ||
                handle == SelfProcessHandle)
            {
                return false;
            }

            int index    = (handle >> 0) & 0x7fff;
            int handleId = (handle >> 15);

            bool result = false;

            lock (_table)
            {
                if (handleId != 0 && index < _size)
                {
                    KHandleEntry entry = _table[index];

                    if (entry.Obj != null && entry.HandleId == handleId)
                    {
                        entry.Obj  = null;
                        entry.Next = _nextFreeEntry;

                        _nextFreeEntry = entry;

                        _activeSlotsCount--;

                        result = true;
                    }
                }
            }

            return result;
        }

        public T GetObject<T>(int handle)
        {
            int index    = (handle >> 0) & 0x7fff;
            int handleId = (handle >> 15);

            lock (_table)
            {
                if ((handle >> 30) == 0 && handleId != 0)
                {
                    KHandleEntry entry = _table[index];

                    if (entry.HandleId == handleId && entry.Obj is T obj)
                    {
                        return obj;
                    }
                }
            }

            return default(T);
        }

        public KThread GetKThread(int handle)
        {
            if (handle == SelfThreadHandle)
            {
                return _system.Scheduler.GetCurrentThread();
            }
            else
            {
                return GetObject<KThread>(handle);
            }
        }

        public KProcess GetKProcess(int handle)
        {
            if (handle == SelfProcessHandle)
            {
                return _system.Scheduler.GetCurrentProcess();
            }
            else
            {
                return GetObject<KProcess>(handle);
            }
        }

        public void Destroy()
        {
            lock (_table)
            {
                for (int index = 0; index < _size; index++)
                {
                    KHandleEntry entry = _table[index];

                    if (entry.Obj != null)
                    {
                        if (entry.Obj is IDisposable disposableObj)
                        {
                            disposableObj.Dispose();
                        }

                        entry.Obj  = null;
                        entry.Next = _nextFreeEntry;

                        _nextFreeEntry = entry;
                    }
                }
            }
        }
    }
}