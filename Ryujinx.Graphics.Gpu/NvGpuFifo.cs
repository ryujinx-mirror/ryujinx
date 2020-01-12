using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// GPU commands FIFO.
    /// </summary>
    class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        // Note: The size of the macro memory is unknown, we just make
        // a guess here and use 256kb as the size. Increase if needed.
        private const int MmeWords = 256 * 256;

        private GpuContext _context;

        /// <summary>
        /// Cached GPU macro program.
        /// </summary>
        private struct CachedMacro
        {
            /// <summary>
            /// Word offset of the code on the code memory.
            /// </summary>
            public int Position { get; }

            private bool _executionPending;
            private int  _argument;

            private MacroInterpreter _interpreter;

            /// <summary>
            /// Creates a new instance of the GPU cached macro program.
            /// </summary>
            /// <param name="position">Macro code start position</param>
            public CachedMacro(int position)
            {
                Position = position;

                _executionPending = false;
                _argument         = 0;

                _interpreter = new MacroInterpreter();
            }

            /// <summary>
            /// Sets the first argument for the macro call.
            /// </summary>
            /// <param name="argument">First argument</param>
            public void StartExecution(int argument)
            {
                _argument = argument;

                _executionPending = true;
            }

            /// <summary>
            /// Starts executing the macro program code.
            /// </summary>
            /// <param name="mme">Program code</param>
            /// <param name="state">Current GPU state</param>
            public void Execute(int[] mme, GpuState state)
            {
                if (_executionPending)
                {
                    _executionPending = false;

                    _interpreter?.Execute(mme, Position, _argument, state);
                }
            }

            /// <summary>
            /// Pushes an argument to the macro call argument FIFO.
            /// </summary>
            /// <param name="argument">Argument to be pushed</param>
            public void PushArgument(int argument)
            {
                _interpreter?.Fifo.Enqueue(argument);
            }
        }

        private int _currMacroPosition;
        private int _currMacroBindIndex;

        private CachedMacro[] _macros;

        private int[] _mme;

        /// <summary>
        /// GPU sub-channel information.
        /// </summary>
        private class SubChannel
        {
            /// <summary>
            /// Sub-channel GPU state.
            /// </summary>
            public GpuState State { get; }

            /// <summary>
            /// Engine bound to the sub-channel.
            /// </summary>
            public ClassId  Class { get; set; }

            /// <summary>
            /// Creates a new instance of the GPU sub-channel.
            /// </summary>
            public SubChannel()
            {
                State = new GpuState();
            }
        }

        private SubChannel[] _subChannels;

        /// <summary>
        /// Creates a new instance of the GPU commands FIFO.
        /// </summary>
        /// <param name="context">GPU emulation context</param>
        public NvGpuFifo(GpuContext context)
        {
            _context = context;

            _macros = new CachedMacro[MacrosCount];

            _mme = new int[MmeWords];

            _subChannels = new SubChannel[8];

            for (int index = 0; index < _subChannels.Length; index++)
            {
                _subChannels[index] = new SubChannel();

                context.Methods.RegisterCallbacks(_subChannels[index].State);
            }
        }

        /// <summary>
        /// Calls a GPU method.
        /// </summary>
        /// <param name="meth">GPU method call parameters</param>
        public void CallMethod(MethodParams meth)
        {
            if ((NvGpuFifoMeth)meth.Method == NvGpuFifoMeth.BindChannel)
            {
                _subChannels[meth.SubChannel].Class = (ClassId)meth.Argument;
            }
            else if (meth.Method < 0x60)
            {
                switch ((NvGpuFifoMeth)meth.Method)
                {
                    case NvGpuFifoMeth.WaitForIdle:
                    {
                        _context.Methods.PerformDeferredDraws();

                        _context.Renderer.Pipeline.Barrier();

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroUploadAddress:
                    {
                        _currMacroPosition = meth.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.SendMacroCodeData:
                    {
                        _mme[_currMacroPosition++] = meth.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroBindingIndex:
                    {
                        _currMacroBindIndex = meth.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.BindMacro:
                    {
                        int position = meth.Argument;

                        _macros[_currMacroBindIndex++] = new CachedMacro(position);

                        break;
                    }
                }
            }
            else if (meth.Method < 0xe00)
            {
                _subChannels[meth.SubChannel].State.CallMethod(meth);
            }
            else
            {
                int macroIndex = (meth.Method >> 1) & MacroIndexMask;

                if ((meth.Method & 1) != 0)
                {
                    _macros[macroIndex].PushArgument(meth.Argument);
                }
                else
                {
                    _macros[macroIndex].StartExecution(meth.Argument);
                }

                if (meth.IsLastCall)
                {
                    _macros[macroIndex].Execute(_mme, _subChannels[meth.SubChannel].State);

                    _context.Methods.PerformDeferredDraws();
                }
            }
        }
    }
}