using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Gpu.Engine.Compute;
using Ryujinx.Graphics.Gpu.Engine.Dma;
using Ryujinx.Graphics.Gpu.Engine.InlineToMemory;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Engine.Twod;
using Ryujinx.Graphics.Gpu.Memory;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Engine.GPFifo
{
    /// <summary>
    /// Represents a GPU General Purpose FIFO command processor.
    /// </summary>
    class GPFifoProcessor
    {
        private const int MacrosCount = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        private const int UniformBufferUpdateDataMethodOffset = 0x8e4;

        private readonly GpuChannel _channel;

        /// <summary>
        /// Channel memory manager.
        /// </summary>
        public MemoryManager MemoryManager => _channel.MemoryManager;

        /// <summary>
        /// Internal GPFIFO state.
        /// </summary>
        private struct DmaState
        {
            public int Method;
            public int SubChannel;
            public int MethodCount;
            public bool NonIncrementing;
            public bool IncrementOnce;
        }

        private DmaState _state;

        private readonly ThreedClass _3dClass;
        private readonly ComputeClass _computeClass;
        private readonly InlineToMemoryClass _i2mClass;
        private readonly TwodClass _2dClass;
        private readonly DmaClass _dmaClass;

        private readonly GPFifoClass _fifoClass;

        /// <summary>
        /// Creates a new instance of the GPU General Purpose FIFO command processor.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">Channel that the GPFIFO processor belongs to</param>
        public GPFifoProcessor(GpuContext context, GpuChannel channel)
        {
            _channel = channel;

            _fifoClass = new GPFifoClass(context, this);
            _3dClass = new ThreedClass(context, channel);
            _computeClass = new ComputeClass(context, channel, _3dClass);
            _i2mClass = new InlineToMemoryClass(context, channel);
            _2dClass = new TwodClass(channel);
            _dmaClass = new DmaClass(context, channel, _3dClass);
        }

        /// <summary>
        /// Processes a command buffer.
        /// </summary>
        /// <param name="commandBuffer">Command buffer</param>
        public void Process(ReadOnlySpan<int> commandBuffer)
        {
            for (int index = 0; index < commandBuffer.Length; index++)
            {
                int command = commandBuffer[index];

                if (_state.MethodCount != 0)
                {
                    Send(_state.Method, command, _state.SubChannel, _state.MethodCount <= 1);

                    if (!_state.NonIncrementing)
                    {
                        _state.Method++;
                    }

                    if (_state.IncrementOnce)
                    {
                        _state.NonIncrementing = true;
                    }

                    _state.MethodCount--;
                }
                else
                {
                    CompressedMethod meth = Unsafe.As<int, CompressedMethod>(ref command);

                    if (TryFastUniformBufferUpdate(meth, commandBuffer, index))
                    {
                        index += meth.MethodCount;
                        continue;
                    }

                    switch (meth.SecOp)
                    {
                        case SecOp.IncMethod:
                        case SecOp.NonIncMethod:
                        case SecOp.OneInc:
                            _state.Method = meth.MethodAddress;
                            _state.SubChannel = meth.MethodSubchannel;
                            _state.MethodCount = meth.MethodCount;
                            _state.IncrementOnce = meth.SecOp == SecOp.OneInc;
                            _state.NonIncrementing = meth.SecOp == SecOp.NonIncMethod;
                            break;
                        case SecOp.ImmdDataMethod:
                            Send(meth.MethodAddress, meth.ImmdData, meth.MethodSubchannel, true);
                            break;
                    }
                }
            }

            _3dClass.FlushUboDirty();
        }

        /// <summary>
        /// Tries to perform a fast constant buffer data update.
        /// If successful, all data will be copied at once, and <see cref="CompressedMethod.MethodCount"/> + 1
        /// command buffer entries will be consumed.
        /// </summary>
        /// <param name="meth">Compressed method to be checked</param>
        /// <param name="commandBuffer">Command buffer where <paramref name="meth"/> is contained</param>
        /// <param name="offset">Offset at <paramref name="commandBuffer"/> where <paramref name="meth"/> is located</param>
        /// <returns>True if the fast copy was successful, false otherwise</returns>
        private bool TryFastUniformBufferUpdate(CompressedMethod meth, ReadOnlySpan<int> commandBuffer, int offset)
        {
            int availableCount = commandBuffer.Length - offset;

            if (meth.MethodCount < availableCount &&
                meth.SecOp == SecOp.NonIncMethod &&
                meth.MethodAddress == UniformBufferUpdateDataMethodOffset)
            {
                _3dClass.ConstantBufferUpdate(commandBuffer.Slice(offset + 1, meth.MethodCount));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends a uncompressed method for processing by the graphics pipeline.
        /// </summary>
        /// <param name="meth">Method to be processed</param>
        private void Send(int offset, int argument, int subChannel, bool isLastCall)
        {
            if (offset < 0x60)
            {
                _fifoClass.Write(offset * 4, argument);
            }
            else if (offset < 0xe00)
            {
                offset *= 4;

                switch (subChannel)
                {
                    case 0:
                        _3dClass.Write(offset, argument);
                        break;
                    case 1:
                        _computeClass.Write(offset, argument);
                        break;
                    case 2:
                        _i2mClass.Write(offset, argument);
                        break;
                    case 3:
                        _2dClass.Write(offset, argument);
                        break;
                    case 4:
                        _dmaClass.Write(offset, argument);
                        break;
                }
            }
            else
            {
                IDeviceState state = subChannel switch
                {
                    0 => _3dClass,
                    3 => _2dClass,
                    _ => null
                };

                if (state != null)
                {
                    int macroIndex = (offset >> 1) & MacroIndexMask;

                    if ((offset & 1) != 0)
                    {
                        _fifoClass.MmePushArgument(macroIndex, argument);
                    }
                    else
                    {
                        _fifoClass.MmeStart(macroIndex, argument);
                    }

                    if (isLastCall)
                    {
                        _fifoClass.CallMme(macroIndex, state);

                        _3dClass.PerformDeferredDraws();
                    }
                }
            }
        }

        /// <summary>
        /// Writes data directly to the state of the specified class.
        /// </summary>
        /// <param name="classId">ID of the class to write the data into</param>
        /// <param name="offset">State offset in bytes</param>
        /// <param name="value">Value to be written</param>
        public void Write(ClassId classId, int offset, int value)
        {
            switch (classId)
            {
                case ClassId.Threed:
                    _3dClass.Write(offset, value);
                    break;
                case ClassId.Compute:
                    _computeClass.Write(offset, value);
                    break;
                case ClassId.InlineToMemory:
                    _i2mClass.Write(offset, value);
                    break;
                case ClassId.Twod:
                    _2dClass.Write(offset, value);
                    break;
                case ClassId.Dma:
                    _dmaClass.Write(offset, value);
                    break;
                case ClassId.GPFifo:
                    _fifoClass.Write(offset, value);
                    break;
            }
        }

        /// <summary>
        /// Sets the shadow ram control value of all sub-channels.
        /// </summary>
        /// <param name="control">New shadow ram control value</param>
        public void SetShadowRamControl(int control)
        {
            _3dClass.SetShadowRamControl(control);
        }

        /// <summary>
        /// Forces a full host state update by marking all state as modified,
        /// and also requests all GPU resources in use to be rebound.
        /// </summary>
        public void ForceAllDirty()
        {
            _3dClass.ForceStateDirty();
            _channel.BufferManager.Rebind();
            _channel.TextureManager.Rebind();
        }

        /// <summary>
        /// Perform any deferred draws.
        /// </summary>
        public void PerformDeferredDraws()
        {
            _3dClass.PerformDeferredDraws();
        }
    }
}
