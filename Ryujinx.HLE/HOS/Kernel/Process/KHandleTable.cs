using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class KHandleTable
    {
        public const int SelfThreadHandle  = (0x1ffff << 15) | 0;
        public const int SelfProcessHandle = (0x1ffff << 15) | 1;

        private readonly KernelContext _context;

        private KHandleEntry[] _table;

        private KHandleEntry _tableHead;
        private KHandleEntry _nextFreeEntry;

        private int _activeSlotsCount;

        private int _size;

        private ushort _idCounter;

        public KHandleTable(KernelContext context)
        {
            _context = context;
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

        public KernelResult GenerateHandle(KAutoObject obj, out int handle)
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

                handle = (_idCounter << 15) | entry.Index;

                obj.IncrementReferenceCount();

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

        public KernelResult ReserveHandle(out int handle)
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

                _activeSlotsCount++;

                handle = (_idCounter << 15) | entry.Index;

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

        public void CancelHandleReservation(int handle)
        {
            int index = (handle >> 0) & 0x7fff;

            lock (_table)
            {
                KHandleEntry entry = _table[index];

                entry.Obj  = null;
                entry.Next = _nextFreeEntry;

                _nextFreeEntry = entry;

                _activeSlotsCount--;
            }
        }

        public void SetReservedHandleObj(int handle, KAutoObject obj)
        {
            int index    = (handle >> 0) & 0x7fff;
            int handleId = (handle >> 15);

            lock (_table)
            {
                KHandleEntry entry = _table[index];

                entry.Obj      = obj;
                entry.HandleId = (ushort)handleId;

                obj.IncrementReferenceCount();
            }
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

            KAutoObject obj = null;

            bool result = false;

            lock (_table)
            {
                if (handleId != 0 && index < _size)
                {
                    KHandleEntry entry = _table[index];

                    if ((obj = entry.Obj) != null && entry.HandleId == handleId)
                    {
                        entry.Obj  = null;
                        entry.Next = _nextFreeEntry;

                        _nextFreeEntry = entry;

                        _activeSlotsCount--;

                        result = true;
                    }
                }
            }

            if (result)
            {
                obj.DecrementReferenceCount();
            }

            return result;
        }

        public T GetObject<T>(int handle) where T : KAutoObject
        {
            int index    = (handle >> 0) & 0x7fff;
            int handleId = (handle >> 15);

            lock (_table)
            {
                if ((handle >> 30) == 0 && handleId != 0 && index < _size)
                {
                    KHandleEntry entry = _table[index];

                    if (entry.HandleId == handleId && entry.Obj is T obj)
                    {
                        return obj;
                    }
                }
            }

            return default;
        }

        public KThread GetKThread(int handle)
        {
            if (handle == SelfThreadHandle)
            {
                return KernelStatic.GetCurrentThread();
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
                return KernelStatic.GetCurrentProcess();
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

                        entry.Obj.DecrementReferenceCount();
                        entry.Obj  = null;
                        entry.Next = _nextFreeEntry;

                        _nextFreeEntry = entry;
                    }
                }
            }
        }
    }
}