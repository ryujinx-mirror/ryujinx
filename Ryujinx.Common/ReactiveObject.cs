using System;
using System.Threading;

namespace Ryujinx.Common
{
    public class ReactiveObject<T>
    {
        private ReaderWriterLock _readerWriterLock = new ReaderWriterLock();
        private bool _isInitialized = false;
        private T _value;

        public event EventHandler<ReactiveEventArgs<T>> Event;

        public T Value
        {
            get
            {
                _readerWriterLock.AcquireReaderLock(Timeout.Infinite);
                T value = _value;
                _readerWriterLock.ReleaseReaderLock();

                return value;
            }
            set
            {
                _readerWriterLock.AcquireWriterLock(Timeout.Infinite);

                T oldValue = _value;

                bool oldIsInitialized = _isInitialized;

                _isInitialized = true;
                _value         = value;

                _readerWriterLock.ReleaseWriterLock();

                if (!oldIsInitialized || oldValue == null || !oldValue.Equals(_value))
                {
                    Event?.Invoke(this, new ReactiveEventArgs<T>(oldValue, value));
                }
            }
        }

        public static implicit operator T(ReactiveObject<T> obj)
        {
            return obj.Value;
        }
    }

    public class ReactiveEventArgs<T>
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ReactiveEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
