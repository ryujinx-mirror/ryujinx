using ARMeilleure.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARMeilleure.Translation.Cache
{
    class JumpTableEntryAllocator
    {
        private readonly BitMap _bitmap;
        private int _freeHint;

        public JumpTableEntryAllocator()
        {
            _bitmap = new BitMap();
        }

        public bool EntryIsValid(int entryIndex)
        {
            lock (_bitmap)
            {
                return _bitmap.IsSet(entryIndex);
            }
        }

        public void SetEntry(int entryIndex)
        {
            lock (_bitmap)
            {
                _bitmap.Set(entryIndex);
            }
        }

        public int AllocateEntry()
        {
            lock (_bitmap)
            {
                int entryIndex;

                if (!_bitmap.IsSet(_freeHint))
                {
                    entryIndex = _freeHint;
                }
                else
                {
                    entryIndex = _bitmap.FindFirstUnset();
                }

                _freeHint = entryIndex + 1;

                bool wasSet = _bitmap.Set(entryIndex);
                Debug.Assert(wasSet);

                return entryIndex;
            }
        }

        public void FreeEntry(int entryIndex)
        {
            lock (_bitmap)
            {
                _bitmap.Clear(entryIndex);

                _freeHint = entryIndex;
            }
        }

        public IEnumerable<int> GetEntries()
        {
            return _bitmap;
        }
    }
}
