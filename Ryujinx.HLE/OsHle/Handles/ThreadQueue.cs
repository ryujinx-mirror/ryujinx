namespace Ryujinx.HLE.OsHle.Handles
{
    class ThreadQueue
    {
        private const int LowestPriority = 0x3f;

        private SchedulerThread Head;

        private object ListLock;

        public ThreadQueue()
        {
            ListLock = new object();
        }

        public void Push(SchedulerThread Wait)
        {
            lock (ListLock)
            {
                //Ensure that we're not creating circular references
                //by adding a thread that is already on the list.
                if (HasThread(Wait))
                {
                    return;
                }

                if (Head == null || Head.Thread.ActualPriority >= Wait.Thread.ActualPriority)
                {
                    Wait.Next = Head;

                    Head = Wait;

                    return;
                }

                SchedulerThread Curr = Head;

                while (Curr.Next != null)
                {
                    if (Curr.Next.Thread.ActualPriority >= Wait.Thread.ActualPriority)
                    {
                        break;
                    }

                    Curr = Curr.Next;
                }

                Wait.Next = Curr.Next;
                Curr.Next = Wait;
            }
        }

        public SchedulerThread Pop(int Core, int MinPriority = LowestPriority)
        {
            lock (ListLock)
            {
                int CoreMask = 1 << Core;

                SchedulerThread Prev = null;
                SchedulerThread Curr = Head;

                while (Curr != null)
                {
                    KThread Thread = Curr.Thread;

                    if (Thread.ActualPriority <= MinPriority && (Thread.CoreMask & CoreMask) != 0)
                    {
                        if (Prev != null)
                        {
                            Prev.Next = Curr.Next;
                        }
                        else
                        {
                            Head = Head.Next;
                        }

                        break;
                    }

                    Prev = Curr;
                    Curr = Curr.Next;
                }

                return Curr;
            }
        }

        public bool Remove(SchedulerThread Thread)
        {
            lock (ListLock)
            {
                if (Head == null)
                {
                    return false;
                }
                else if (Head == Thread)
                {
                    Head = Head.Next;

                    return true;
                }

                SchedulerThread Prev = Head;
                SchedulerThread Curr = Head.Next;

                while (Curr != null)
                {
                    if (Curr == Thread)
                    {
                        Prev.Next = Curr.Next;

                        return true;
                    }

                    Prev = Curr;
                    Curr = Curr.Next;
                }

                return false;
            }
        }

        public bool Resort(SchedulerThread Thread)
        {
            lock (ListLock)
            {
                if (Remove(Thread))
                {
                    Push(Thread);

                    return true;
                }

                return false;
            }
        }

        public bool HasThread(SchedulerThread Thread)
        {
            lock (ListLock)
            {
                SchedulerThread Curr = Head;

                while (Curr != null)
                {
                    if (Curr == Thread)
                    {
                        return true;
                    }

                    Curr = Curr.Next;
                }

                return false;
            }
        }
    }
}