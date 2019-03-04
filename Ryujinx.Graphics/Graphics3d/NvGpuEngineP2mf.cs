using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Graphics3d
{
    class NvGpuEngineP2mf : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu _gpu;

        private Dictionary<int, NvGpuMethod> _methods;

        private int _copyStartX;
        private int _copyStartY;

        private int _copyWidth;
        private int _copyHeight;
        private int _copyGobBlockHeight;

        private long _copyAddress;

        private int _copyOffset;
        private int _copySize;

        private bool _copyLinear;

        private byte[] _buffer;

        public NvGpuEngineP2mf(NvGpu gpu)
        {
            _gpu = gpu;

            Registers = new int[0x80];

            _methods = new Dictionary<int, NvGpuMethod>();

            void AddMethod(int meth, int count, int stride, NvGpuMethod method)
            {
                while (count-- > 0)
                {
                    _methods.Add(meth, method);

                    meth += stride;
                }
            }

            AddMethod(0x6c, 1, 1, Execute);
            AddMethod(0x6d, 1, 1, PushData);
        }

        public void CallMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            if (_methods.TryGetValue(methCall.Method, out NvGpuMethod method))
            {
                method(vmm, methCall);
            }
            else
            {
                WriteRegister(methCall);
            }
        }

        private void Execute(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            //TODO: Some registers and copy modes are still not implemented.
            int control = methCall.Argument;

            long dstAddress = MakeInt64From2xInt32(NvGpuEngineP2mfReg.DstAddress);

            int dstPitch  = ReadRegister(NvGpuEngineP2mfReg.DstPitch);
            int dstBlkDim = ReadRegister(NvGpuEngineP2mfReg.DstBlockDim);

            int dstX = ReadRegister(NvGpuEngineP2mfReg.DstX);
            int dstY = ReadRegister(NvGpuEngineP2mfReg.DstY);

            int dstWidth  = ReadRegister(NvGpuEngineP2mfReg.DstWidth);
            int dstHeight = ReadRegister(NvGpuEngineP2mfReg.DstHeight);

            int lineLengthIn = ReadRegister(NvGpuEngineP2mfReg.LineLengthIn);
            int lineCount    = ReadRegister(NvGpuEngineP2mfReg.LineCount);

            _copyLinear = (control & 1) != 0;

            _copyGobBlockHeight = 1 << ((dstBlkDim >> 4) & 0xf);

            _copyStartX = dstX;
            _copyStartY = dstY;

            _copyWidth  = dstWidth;
            _copyHeight = dstHeight;

            _copyAddress = dstAddress;

            _copyOffset = 0;
            _copySize   = lineLengthIn * lineCount;

            _buffer = new byte[_copySize];
        }

        private void PushData(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            if (_buffer == null)
            {
                return;
            }

            for (int shift = 0; shift < 32 && _copyOffset < _copySize; shift += 8, _copyOffset++)
            {
                _buffer[_copyOffset] = (byte)(methCall.Argument >> shift);
            }

            if (methCall.IsLastCall)
            {
                if (_copyLinear)
                {
                    vmm.WriteBytes(_copyAddress, _buffer);
                }
                else
                {
                    BlockLinearSwizzle swizzle = new BlockLinearSwizzle(
                        _copyWidth,
                        _copyHeight, 1,
                        _copyGobBlockHeight, 1, 1);

                    int srcOffset = 0;

                    for (int y = _copyStartY; y < _copyHeight && srcOffset < _copySize; y++)
                    for (int x = _copyStartX; x < _copyWidth  && srcOffset < _copySize; x++)
                    {
                        int dstOffset = swizzle.GetSwizzleOffset(x, y, 0);

                        vmm.WriteByte(_copyAddress + dstOffset, _buffer[srcOffset++]);
                    }
                }

                _buffer = null;
            }
        }

        private long MakeInt64From2xInt32(NvGpuEngineP2mfReg reg)
        {
            return
                (long)Registers[(int)reg + 0] << 32 |
                (uint)Registers[(int)reg + 1];
        }

        private void WriteRegister(GpuMethodCall methCall)
        {
            Registers[methCall.Method] = methCall.Argument;
        }

        private int ReadRegister(NvGpuEngineP2mfReg reg)
        {
            return Registers[(int)reg];
        }

        private void WriteRegister(NvGpuEngineP2mfReg reg, int value)
        {
            Registers[(int)reg] = value;
        }
    }
}