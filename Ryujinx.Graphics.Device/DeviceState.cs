using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Device
{
    public class DeviceState<TState> : IDeviceState where TState : unmanaged
    {
        private const int RegisterSize = sizeof(int);

        public TState State;

        private readonly BitArray _readableRegisters;
        private readonly BitArray _writableRegisters;

        private readonly Dictionary<int, Func<int>> _readCallbacks;
        private readonly Dictionary<int, Action<int>> _writeCallbacks;

        public DeviceState(IReadOnlyDictionary<string, RwCallback> callbacks = null)
        {
            int size = (Unsafe.SizeOf<TState>() + RegisterSize - 1) / RegisterSize;

            _readableRegisters = new BitArray(size);
            _writableRegisters = new BitArray(size);

            _readCallbacks = new Dictionary<int, Func<int>>();
            _writeCallbacks = new Dictionary<int, Action<int>>();

            var fields = typeof(TState).GetFields();
            int offset = 0;

            for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                var field = fields[fieldIndex];
                var regAttr = field.GetCustomAttributes<RegisterAttribute>(false).FirstOrDefault();

                int sizeOfField = SizeCalculator.SizeOf(field.FieldType);

                for (int i = 0; i < ((sizeOfField + 3) & ~3); i += 4)
                {
                    _readableRegisters[(offset + i) / RegisterSize] = regAttr?.AccessControl.HasFlag(AccessControl.ReadOnly)  ?? true;
                    _writableRegisters[(offset + i) / RegisterSize] = regAttr?.AccessControl.HasFlag(AccessControl.WriteOnly) ?? true;
                }

                if (callbacks != null && callbacks.TryGetValue(field.Name, out var cb))
                {
                    if (cb.Read != null)
                    {
                        _readCallbacks.Add(offset, cb.Read);
                    }

                    if (cb.Write != null)
                    {
                        _writeCallbacks.Add(offset, cb.Write);
                    }
                }

                offset += sizeOfField;
            }

            Debug.Assert(offset == Unsafe.SizeOf<TState>());
        }

        public virtual int Read(int offset)
        {
            if (Check(offset) && _readableRegisters[offset / RegisterSize])
            {
                int alignedOffset = Align(offset);

                if (_readCallbacks.TryGetValue(alignedOffset, out Func<int> read))
                {
                    return read();
                }
                else
                {
                    return GetRef<int>(alignedOffset);
                }
            }

            return 0;
        }

        public virtual void Write(int offset, int data)
        {
            if (Check(offset) && _writableRegisters[offset / RegisterSize])
            {
                int alignedOffset = Align(offset);

                GetRef<int>(alignedOffset) = data;

                if (_writeCallbacks.TryGetValue(alignedOffset, out Action<int> write))
                {
                    write(data);
                }
            }
        }

        private bool Check(int offset)
        {
            return (uint)Align(offset) < Unsafe.SizeOf<TState>();
        }

        public ref T GetRef<T>(int offset) where T : unmanaged
        {
            if ((uint)(offset + Unsafe.SizeOf<T>()) > Unsafe.SizeOf<TState>())
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return ref Unsafe.As<TState, T>(ref Unsafe.AddByteOffset(ref State, (IntPtr)offset));
        }

        private static int Align(int offset)
        {
            return offset & ~(RegisterSize - 1);
        }
    }
}
