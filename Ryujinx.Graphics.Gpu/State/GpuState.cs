using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.State
{
    class GpuState
    {
        private const int RegistersCount = 0xe00;

        public delegate void MethodCallback(GpuState state, int argument);

        private int[] _backingMemory;

        private struct Register
        {
            public MethodCallback Callback;

            public MethodOffset BaseOffset;

            public int Stride;
            public int Count;

            public bool Modified;
        }

        private Register[] _registers;

        public GpuState()
        {
            _backingMemory = new int[RegistersCount];

            _registers = new Register[RegistersCount];

            for (int index = 0; index < _registers.Length; index++)
            {
                _registers[index].BaseOffset = (MethodOffset)index;
                _registers[index].Stride     = 1;
                _registers[index].Count      = 1;
                _registers[index].Modified   = true;
            }

            foreach (var item in GpuStateTable.Table)
            {
                int totalRegs = item.Size * item.Count;

                for (int regOffset = 0; regOffset < totalRegs; regOffset++)
                {
                    int index = (int)item.Offset + regOffset;

                    _registers[index].BaseOffset = item.Offset;
                    _registers[index].Stride     = item.Size;
                    _registers[index].Count      = item.Count;
                }
            }

            InitializeDefaultState();
        }

        public void CallMethod(MethodParams meth)
        {
            Register register = _registers[meth.Method];

            if (_backingMemory[meth.Method] != meth.Argument)
            {
                _registers[(int)register.BaseOffset].Modified = true;
            }

            _backingMemory[meth.Method] = meth.Argument;

            MethodCallback callback = register.Callback;

            if (callback != null)
            {
                callback(this, meth.Argument);
            }
        }

        public int Read(int offset)
        {
            return _backingMemory[offset];
        }

        public void SetUniformBufferOffset(int offset)
        {
            _backingMemory[(int)MethodOffset.UniformBufferState + 3] = offset;
        }

        private void InitializeDefaultState()
        {
            // Depth ranges.
            for (int index = 0; index < 8; index++)
            {
                _backingMemory[(int)MethodOffset.ViewportExtents + index * 4 + 2] = 0;
                _backingMemory[(int)MethodOffset.ViewportExtents + index * 4 + 3] = 0x3F800000;
            }

            // Default front stencil mask.
            _backingMemory[0x4e7] = 0xff;

            // Default color mask.
            _backingMemory[(int)MethodOffset.RtColorMask] = 0x1111;
        }

        public void RegisterCallback(MethodOffset offset, int count, MethodCallback callback)
        {
            for (int index = 0; index < count; index++)
            {
                _registers[(int)offset + index].Callback = callback;
            }
        }

        public void RegisterCallback(MethodOffset offset, MethodCallback callback)
        {
            _registers[(int)offset].Callback = callback;
        }

        public bool QueryModified(MethodOffset offset)
        {
            bool modified = _registers[(int)offset].Modified;

            _registers[(int)offset].Modified = false;

            return modified;
        }

        public bool QueryModified(MethodOffset m1, MethodOffset m2)
        {
            bool modified = _registers[(int)m1].Modified ||
                            _registers[(int)m2].Modified;

            _registers[(int)m1].Modified = false;
            _registers[(int)m2].Modified = false;

            return modified;
        }

        public bool QueryModified(MethodOffset m1, MethodOffset m2, MethodOffset m3)
        {
            bool modified = _registers[(int)m1].Modified ||
                            _registers[(int)m2].Modified ||
                            _registers[(int)m3].Modified;

            _registers[(int)m1].Modified = false;
            _registers[(int)m2].Modified = false;
            _registers[(int)m3].Modified = false;

            return modified;
        }

        public bool QueryModified(MethodOffset m1, MethodOffset m2, MethodOffset m3, MethodOffset m4)
        {
            bool modified = _registers[(int)m1].Modified ||
                            _registers[(int)m2].Modified ||
                            _registers[(int)m3].Modified ||
                            _registers[(int)m4].Modified;

            _registers[(int)m1].Modified = false;
            _registers[(int)m2].Modified = false;
            _registers[(int)m3].Modified = false;
            _registers[(int)m4].Modified = false;

            return modified;
        }

        public T Get<T>(MethodOffset offset, int index) where T : struct
        {
            Register register = _registers[(int)offset];

            if ((uint)index >= register.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Get<T>(offset + index * register.Stride);
        }

        public T Get<T>(MethodOffset offset) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(_backingMemory.AsSpan().Slice((int)offset))[0];
        }
    }
}
