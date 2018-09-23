using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KProcessHandleTable
    {
        private const int SelfThreadHandle  = (0x1ffff << 15) | 0;
        private const int SelfProcessHandle = (0x1ffff << 15) | 1;

        private Horizon System;

        private KHandleEntry[] Table;

        private KHandleEntry TableHead;
        private KHandleEntry NextFreeEntry;

        private int ActiveSlotsCount;

        private int Size;

        private ushort IdCounter;

        private object LockObj;

        public KProcessHandleTable(Horizon System, int Size = 1024)
        {
            this.System = System;
            this.Size   = Size;

            IdCounter = 1;

            Table = new KHandleEntry[Size];

            TableHead = new KHandleEntry(0);

            KHandleEntry Entry = TableHead;

            for (int Index = 0; Index < Size; Index++)
            {
                Table[Index] = Entry;

                Entry.Next = new KHandleEntry(Index + 1);

                Entry = Entry.Next;
            }

            Table[Size - 1].Next = null;

            NextFreeEntry = TableHead;

            LockObj = new object();
        }

        public KernelResult GenerateHandle(object Obj, out int Handle)
        {
            Handle = 0;

            lock (LockObj)
            {
                if (ActiveSlotsCount >= Size)
                {
                    return KernelResult.HandleTableFull;
                }

                KHandleEntry Entry = NextFreeEntry;

                NextFreeEntry = Entry.Next;

                Entry.Obj      = Obj;
                Entry.HandleId = IdCounter;

                ActiveSlotsCount++;

                Handle = (int)((IdCounter << 15) & (uint)0xffff8000) | Entry.Index;

                if ((short)(IdCounter + 1) >= 0)
                {
                    IdCounter++;
                }
                else
                {
                    IdCounter = 1;
                }
            }

            return KernelResult.Success;
        }

        public bool CloseHandle(int Handle)
        {
            if ((Handle >> 30) != 0 ||
                Handle == SelfThreadHandle ||
                Handle == SelfProcessHandle)
            {
                return false;
            }

            int Index    = (Handle >>  0) & 0x7fff;
            int HandleId = (Handle >> 15);

            bool Result = false;

            lock (LockObj)
            {
                if (HandleId != 0 && Index < Size)
                {
                    KHandleEntry Entry = Table[Index];

                    if (Entry.Obj != null && Entry.HandleId == HandleId)
                    {
                        Entry.Obj  = null;
                        Entry.Next = NextFreeEntry;

                        NextFreeEntry = Entry;

                        ActiveSlotsCount--;

                        Result = true;
                    }
                }
            }

            return Result;
        }

        public T GetObject<T>(int Handle)
        {
            int Index    = (Handle >>  0) & 0x7fff;
            int HandleId = (Handle >> 15);

            lock (LockObj)
            {
                if ((Handle >> 30) == 0 && HandleId != 0)
                {
                    KHandleEntry Entry = Table[Index];

                    if (Entry.HandleId == HandleId && Entry.Obj is T Obj)
                    {
                        return Obj;
                    }
                }
            }

            return default(T);
        }

        public KThread GetKThread(int Handle)
        {
            if (Handle == SelfThreadHandle)
            {
                return System.Scheduler.GetCurrentThread();
            }
            else
            {
                return GetObject<KThread>(Handle);
            }
        }

        public void Destroy()
        {
            lock (LockObj)
            {
                for (int Index = 0; Index < Size; Index++)
                {
                    KHandleEntry Entry = Table[Index];

                    if (Entry.Obj != null)
                    {
                        if (Entry.Obj is IDisposable DisposableObj)
                        {
                            DisposableObj.Dispose();
                        }

                        Entry.Obj  = null;
                        Entry.Next = NextFreeEntry;

                        NextFreeEntry = Entry;
                    }
                }
            }
        }
    }
}