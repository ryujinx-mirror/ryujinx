using System;
using System.Collections.Generic;

namespace ARMeilleure.Common
{
    public class SortedIntegerList
    {
        private List<int> _items;

        public int Count => _items.Count;

        public int this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }

        public SortedIntegerList()
        {
            _items = new List<int>();
        }

        public bool Add(int value)
        {
            if (_items.Count == 0 || value > Last())
            {
                _items.Add(value);
                return true;
            }
            else
            {
                int index = _items.BinarySearch(value);
                if (index >= 0)
                {
                    return false;
                }

                _items.Insert(-1 - index, value);
                return true;
            }
        }

        public int FindLessEqualIndex(int value)
        {
            int index = _items.BinarySearch(value);
            return (index < 0) ? (-2 - index) : index;
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                _items.RemoveRange(index, count);
            }
        }

        public int Last()
        {
            return _items[Count - 1];
        }

        public List<int> GetList()
        {
            return _items;
        }
    }
}
