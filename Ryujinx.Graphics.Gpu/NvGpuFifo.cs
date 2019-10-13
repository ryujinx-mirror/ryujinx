namespace Ryujinx.Graphics.Gpu
{
    class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        // Note: The size of the macro memory is unknown, we just make
        // a guess here and use 256kb as the size. Increase if needed.
        private const int MmeWords = 256 * 256;

        private GpuContext _context;

        private struct CachedMacro
        {
            public int Position { get; private set; }

            private bool _executionPending;
            private int  _argument;

            private MacroInterpreter _interpreter;

            public CachedMacro(GpuContext context, NvGpuFifo fifo, int position)
            {
                Position = position;

                _executionPending = false;
                _argument         = 0;

                _interpreter = new MacroInterpreter(context, fifo);
            }

            public void StartExecution(int argument)
            {
                _argument = argument;

                _executionPending = true;
            }

            public void Execute(int[] mme)
            {
                if (_executionPending)
                {
                    _executionPending = false;

                    _interpreter?.Execute(mme, Position, _argument);
                }
            }

            public void PushArgument(int argument)
            {
                _interpreter?.Fifo.Enqueue(argument);
            }
        }

        private int _currMacroPosition;
        private int _currMacroBindIndex;

        private CachedMacro[] _macros;

        private int[] _mme;

        private ClassId[] _subChannels;

        public NvGpuFifo(GpuContext context)
        {
            _context = context;

            _macros = new CachedMacro[MacrosCount];

            _mme = new int[MmeWords];

            _subChannels = new ClassId[8];
        }

        public void CallMethod(MethodParams meth)
        {
            if ((NvGpuFifoMeth)meth.Method == NvGpuFifoMeth.BindChannel)
            {
                _subChannels[meth.SubChannel] = (ClassId)meth.Argument;
            }
            else if (meth.Method < 0x60)
            {
                switch ((NvGpuFifoMeth)meth.Method)
                {
                    case NvGpuFifoMeth.WaitForIdle:
                    {
                        _context.Renderer.FlushPipelines();

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

                        _macros[_currMacroBindIndex++] = new CachedMacro(_context, this, position);

                        break;
                    }
                }
            }
            else if (meth.Method < 0xe00)
            {
                _context.State.CallMethod(meth);
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
                    _macros[macroIndex].Execute(_mme);

                    _context.Methods.PerformDeferredDraws();
                }
            }
        }
    }
}