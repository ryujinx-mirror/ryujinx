using Ryujinx.Graphics.Gpu.State;
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

        private readonly GpuContext _context;

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

        private readonly GpuState[] _subChannels;
        private readonly GPFifoClass _fifoClass;

        /// <summary>
        /// Creates a new instance of the GPU General Purpose FIFO command processor.
        /// </summary>
        /// <param name="context">GPU context</param>
        public GPFifoProcessor(GpuContext context)
        {
            _context = context;

            _fifoClass = new GPFifoClass(context, this);
            _subChannels = new GpuState[8];

            for (int index = 0; index < _subChannels.Length; index++)
            {
                _subChannels[index] = new GpuState();

                _context.Methods.RegisterCallbacks(_subChannels[index]);
            }
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
                    Send(new MethodParams(_state.Method, command, _state.SubChannel, _state.MethodCount));

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
                            Send(new MethodParams(meth.MethodAddress, meth.ImmdData, meth.MethodSubchannel, 1));
                            break;
                    }
                }
            }
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
                meth.MethodAddress == (int)MethodOffset.UniformBufferUpdateData)
            {
                GpuState state = _subChannels[meth.MethodSubchannel];

                _context.Methods.UniformBufferUpdate(state, commandBuffer.Slice(offset + 1, meth.MethodCount));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends a uncompressed method for processing by the graphics pipeline.
        /// </summary>
        /// <param name="meth">Method to be processed</param>
        private void Send(MethodParams meth)
        {
            if ((MethodOffset)meth.Method == MethodOffset.BindChannel)
            {
                _subChannels[meth.SubChannel] = new GpuState();

                _context.Methods.RegisterCallbacks(_subChannels[meth.SubChannel]);
            }
            else if (meth.Method < 0x60)
            {
                // TODO: check if macros are shared between subchannels or not. For now let's assume they are.
                _fifoClass.Write(meth.Method * 4, meth.Argument);
            }
            else if (meth.Method < 0xe00)
            {
                _subChannels[meth.SubChannel].CallMethod(meth);
            }
            else
            {
                int macroIndex = (meth.Method >> 1) & MacroIndexMask;

                if ((meth.Method & 1) != 0)
                {
                    _fifoClass.MmePushArgument(macroIndex, meth.Argument);
                }
                else
                {
                    _fifoClass.MmeStart(macroIndex, meth.Argument);
                }

                if (meth.IsLastCall)
                {
                    _fifoClass.CallMme(macroIndex, _subChannels[meth.SubChannel]);

                    _context.Methods.PerformDeferredDraws();
                }
            }
        }

        /// <summary>
        /// Sets the shadow ram control value of all sub-channels.
        /// </summary>
        /// <param name="control">New shadow ram control value</param>
        public void SetShadowRamControl(ShadowRamControl control)
        {
            for (int i = 0; i < _subChannels.Length; i++)
            {
                _subChannels[i].ShadowRamControl = control;
            }
        }
    }
}
