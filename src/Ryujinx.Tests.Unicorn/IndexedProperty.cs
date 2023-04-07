using System;

namespace Ryujinx.Tests.Unicorn
{
    public class IndexedProperty<TIndex, TValue>
    {
        private Func<TIndex, TValue>   _getFunc;
        private Action<TIndex, TValue> _setAction;

        public IndexedProperty(Func<TIndex, TValue> getFunc, Action<TIndex, TValue> setAction)
        {
            _getFunc   = getFunc;
            _setAction = setAction;
        }

        public TValue this[TIndex index]
        {
            get
            {
                return _getFunc(index);
            }
            set
            {
                _setAction(index, value);
            }
        }
    }
}
