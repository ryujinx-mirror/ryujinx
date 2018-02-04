using System.Collections.Generic;

namespace Ryujinx.OsHle.Utilities
{
    class IdPool
    {
        private HashSet<int> Ids;

        private int CurrId;
        private int MinId;
        private int MaxId;

        public IdPool(int Min, int Max)
        {
            Ids = new HashSet<int>();

            CurrId = Min;
            MinId  = Min;
            MaxId  = Max;
        }

        public IdPool() : this(1, int.MaxValue) { }

        public int GenerateId()
        {
            lock (Ids)
            {
                for (int Cnt = MinId; Cnt < MaxId; Cnt++)
                {
                    if (Ids.Add(CurrId))
                    {
                        return CurrId;
                    }

                    if (CurrId++ == MaxId)
                    {
                        CurrId = MinId;
                    }
                }

                return -1;
            }
        }

        public bool DeleteId(int Id)
        {
            lock (Ids)
            {
                return Ids.Remove(Id);
            }
        }
    }
}