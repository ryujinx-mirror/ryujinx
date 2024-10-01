using System;

namespace ARMeilleure.Common
{
    /// <summary>
    /// Represents a numeric counter which can be used for instrumentation of compiled code.
    /// </summary>
    /// <typeparam name="T">Type of the counter</typeparam>
    class Counter<T> : IDisposable where T : unmanaged
    {
        private bool _disposed;
        /// <summary>
        /// Index in the <see cref="EntryTable{T}"/>
        /// </summary>
        private readonly int _index;
        private readonly EntryTable<T> _countTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Counter{T}"/> class from the specified
        /// <see cref="EntryTable{T}"/> instance and index.
        /// </summary>
        /// <param name="countTable"><see cref="EntryTable{T}"/> instance</param>
        /// <exception cref="ArgumentNullException"><paramref name="countTable"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is unsupported</exception>
        public Counter(EntryTable<T> countTable)
        {
            if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
                typeof(T) != typeof(short) && typeof(T) != typeof(ushort) &&
                typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
                typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
                typeof(T) != typeof(nint) && typeof(T) != typeof(nuint) &&
                typeof(T) != typeof(float) && typeof(T) != typeof(double))
            {
                throw new ArgumentException("Counter does not support the specified type.");
            }

            _countTable = countTable ?? throw new ArgumentNullException(nameof(countTable));
            _index = countTable.Allocate();
        }

        /// <summary>
        /// Gets a reference to the value of the counter.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="Counter{T}"/> instance was disposed</exception>
        /// <remarks>
        /// This can refer to freed memory if the owning <see cref="EntryTable{TEntry}"/> is disposed.
        /// </remarks>
        public ref T Value
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                return ref _countTable.GetValue(_index);
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Counter{T}"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged and optionally managed resources used by the <see cref="Counter{T}"/> instance.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to dispose managed resources also; otherwise just unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    // The index into the EntryTable is essentially an unmanaged resource since we allocate and free the
                    // resource ourselves.
                    _countTable.Free(_index);
                }
                catch (ObjectDisposedException)
                {
                    // Can happen because _countTable may be disposed before the Counter instance.
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Frees resources used by the <see cref="Counter{T}"/> instance.
        /// </summary>
        ~Counter()
        {
            Dispose(false);
        }
    }
}
