using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// GPU state.
    /// </summary>
    class GpuState
    {
        private const int RegistersCount = 0xe00;

        public delegate void MethodCallback(GpuState state, int argument);

        private int[] _backingMemory;

        /// <summary>
        /// GPU register information.
        /// </summary>
        private struct Register
        {
            public MethodCallback Callback;

            public MethodOffset BaseOffset;

            public int Stride;
            public int Count;

            public bool Modified;
        }

        private Register[] _registers;

        /// <summary>
        /// Creates a new instance of the GPU state.
        /// </summary>
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

        /// <summary>
        /// Calls a GPU method, using this state.
        /// </summary>
        /// <param name="meth">The GPU method to be called</param>
        public void CallMethod(MethodParams meth)
        {
            Register register = _registers[meth.Method];

            if (_backingMemory[meth.Method] != meth.Argument)
            {
                _registers[(int)register.BaseOffset].Modified = true;
            }

            _backingMemory[meth.Method] = meth.Argument;

            register.Callback?.Invoke(this, meth.Argument);
        }

        /// <summary>
        /// Reads data from a GPU register at the given offset.
        /// </summary>
        /// <param name="offset">Offset to be read</param>
        /// <returns>Data at the register</returns>
        public int Read(int offset)
        {
            return _backingMemory[offset];
        }

        /// <summary>
        /// Writes data to the GPU register at the given offset.
        /// </summary>
        /// <param name="offset">Offset to be written</param>
        /// <param name="value">Value to be written</param>
        public void Write(int offset, int value)
        {
            _backingMemory[offset] = value;
        }

        /// <summary>
        /// Writes an offset value at the uniform buffer offset register.
        /// </summary>
        /// <param name="offset">The offset to be written</param>
        public void SetUniformBufferOffset(int offset)
        {
            _backingMemory[(int)MethodOffset.UniformBufferState + 3] = offset;
        }

        /// <summary>
        /// Initializes registers with the default state.
        /// </summary>
        private void InitializeDefaultState()
        {
            // Enable Rasterizer
            _backingMemory[(int)MethodOffset.RasterizeEnable] = 1;

            // Depth ranges.
            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                _backingMemory[(int)MethodOffset.ViewportExtents + index * 4 + 2] = 0;
                _backingMemory[(int)MethodOffset.ViewportExtents + index * 4 + 3] = 0x3F800000;
            }

            // Viewport transform enable.
            _backingMemory[(int)MethodOffset.ViewportTransformEnable] = 1;

            // Default front stencil mask.
            _backingMemory[0x4e7] = 0xff;

            // Default color mask.
            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                _backingMemory[(int)MethodOffset.RtColorMask + index] = 0x1111;
            }
        }

        /// <summary>
        /// Registers a callback that is called every time a GPU method, or methods are called.
        /// </summary>
        /// <param name="offset">Offset of the method</param>
        /// <param name="count">Word count of the methods region</param>
        /// <param name="callback">Calllback to be called</param>
        public void RegisterCallback(MethodOffset offset, int count, MethodCallback callback)
        {
            for (int index = 0; index < count; index++)
            {
                _registers[(int)offset + index].Callback = callback;
            }
        }

        /// <summary>
        /// Registers a callback that is called every time a GPU method is called.
        /// </summary>
        /// <param name="offset">Offset of the method</param>
        /// <param name="callback">Calllback to be called</param>
        public void RegisterCallback(MethodOffset offset, MethodCallback callback)
        {
            _registers[(int)offset].Callback = callback;
        }

        /// <summary>
        /// Checks if a given register has been modified since the last call to this method.
        /// </summary>
        /// <param name="offset">Register offset</param>
        /// <returns>True if modified, false otherwise</returns>
        public bool QueryModified(MethodOffset offset)
        {
            bool modified = _registers[(int)offset].Modified;

            _registers[(int)offset].Modified = false;

            return modified;
        }

        /// <summary>
        /// Checks if two registers have been modified since the last call to this method.
        /// </summary>
        /// <param name="m1">First register offset</param>
        /// <param name="m2">Second register offset</param>
        /// <returns>True if any register was modified, false otherwise</returns>
        public bool QueryModified(MethodOffset m1, MethodOffset m2)
        {
            bool modified = _registers[(int)m1].Modified ||
                            _registers[(int)m2].Modified;

            _registers[(int)m1].Modified = false;
            _registers[(int)m2].Modified = false;

            return modified;
        }

        /// <summary>
        /// Checks if two registers have been modified since the last call to this method.
        /// </summary>
        /// <param name="m1">First register offset</param>
        /// <param name="m2">Second register offset</param>
        /// <param name="m3">Third register offset</param>
        /// <returns>True if any register was modified, false otherwise</returns>
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

        /// <summary>
        /// Checks if two registers have been modified since the last call to this method.
        /// </summary>
        /// <param name="m1">First register offset</param>
        /// <param name="m2">Second register offset</param>
        /// <param name="m3">Third register offset</param>
        /// <param name="m4">Fourth register offset</param>
        /// <returns>True if any register was modified, false otherwise</returns>
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

        /// <summary>
        /// Checks if two registers have been modified since the last call to this method.
        /// </summary>
        /// <param name="m1">First register offset</param>
        /// <param name="m2">Second register offset</param>
        /// <param name="m3">Third register offset</param>
        /// <param name="m4">Fourth register offset</param>
        /// <param name="m5">Fifth register offset</param>
        /// <returns>True if any register was modified, false otherwise</returns>
        public bool QueryModified(
            MethodOffset m1,
            MethodOffset m2,
            MethodOffset m3,
            MethodOffset m4,
            MethodOffset m5)
        {
            bool modified = _registers[(int)m1].Modified ||
                            _registers[(int)m2].Modified ||
                            _registers[(int)m3].Modified ||
                            _registers[(int)m4].Modified ||
                            _registers[(int)m5].Modified;

            _registers[(int)m1].Modified = false;
            _registers[(int)m2].Modified = false;
            _registers[(int)m3].Modified = false;
            _registers[(int)m4].Modified = false;
            _registers[(int)m5].Modified = false;

            return modified;
        }

        /// <summary>
        /// Gets indexed data from a given register offset.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="offset">Register offset</param>
        /// <param name="index">Index for indexed data</param>
        /// <returns>The data at the specified location</returns>
        public T Get<T>(MethodOffset offset, int index) where T : struct
        {
            Register register = _registers[(int)offset];

            if ((uint)index >= register.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Get<T>(offset + index * register.Stride);
        }

        /// <summary>
        /// Gets data from a given register offset.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="offset">Register offset</param>
        /// <returns>The data at the specified location</returns>
        public T Get<T>(MethodOffset offset) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(_backingMemory.AsSpan().Slice((int)offset))[0];
        }
    }
}
