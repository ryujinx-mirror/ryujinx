using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Graphics3d
{
    class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        // Note: The size of the macro memory is unknown, we just make
        // a guess here and use 256kb as the size. Increase if needed.
        private const int MmeWords = 256 * 256;

        private NvGpu _gpu;

        private NvGpuEngine[] _subChannels;

        private struct CachedMacro
        {
            public int Position { get; private set; }

            private bool _executionPending;
            private int  _argument;

            private MacroInterpreter _interpreter;

            public CachedMacro(NvGpuFifo pFifo, INvGpuEngine engine, int position)
            {
                Position = position;

                _executionPending = false;
                _argument         = 0;

                _interpreter = new MacroInterpreter(pFifo, engine);
            }

            public void StartExecution(int argument)
            {
                _argument = argument;

                _executionPending = true;
            }

            public void Execute(NvGpuVmm vmm, int[] mme)
            {
                if (_executionPending)
                {
                    _executionPending = false;

                    _interpreter?.Execute(vmm, mme, Position, _argument);
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

        public NvGpuFifo(NvGpu gpu)
        {
            _gpu = gpu;

            _subChannels = new NvGpuEngine[8];

            _macros = new CachedMacro[MacrosCount];

            _mme = new int[MmeWords];
        }

        public void CallMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            if ((NvGpuFifoMeth)methCall.Method == NvGpuFifoMeth.BindChannel)
            {
                NvGpuEngine engine = (NvGpuEngine)methCall.Argument;

                _subChannels[methCall.SubChannel] = engine;
            }
            else
            {
                switch (_subChannels[methCall.SubChannel])
                {
                    case NvGpuEngine._2d:  Call2dMethod  (vmm, methCall); break;
                    case NvGpuEngine._3d:  Call3dMethod  (vmm, methCall); break;
                    case NvGpuEngine.P2mf: CallP2mfMethod(vmm, methCall); break;
                    case NvGpuEngine.M2mf: CallM2mfMethod(vmm, methCall); break;
                }
            }
        }

        private void Call2dMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            _gpu.Engine2d.CallMethod(vmm, methCall);
        }

        private void Call3dMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            if (methCall.Method < 0x80)
            {
                switch ((NvGpuFifoMeth)methCall.Method)
                {
                    case NvGpuFifoMeth.SetMacroUploadAddress:
                    {
                        _currMacroPosition = methCall.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.SendMacroCodeData:
                    {
                        _mme[_currMacroPosition++] = methCall.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroBindingIndex:
                    {
                        _currMacroBindIndex = methCall.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.BindMacro:
                    {
                        int position = methCall.Argument;

                        _macros[_currMacroBindIndex++] = new CachedMacro(this, _gpu.Engine3d, position);

                        break;
                    }

                    default: CallP2mfMethod(vmm, methCall); break;
                }
            }
            else if (methCall.Method < 0xe00)
            {
                _gpu.Engine3d.CallMethod(vmm, methCall);
            }
            else
            {
                int macroIndex = (methCall.Method >> 1) & MacroIndexMask;

                if ((methCall.Method & 1) != 0)
                {
                    _macros[macroIndex].PushArgument(methCall.Argument);
                }
                else
                {
                    _macros[macroIndex].StartExecution(methCall.Argument);
                }

                if (methCall.IsLastCall)
                {
                    _macros[macroIndex].Execute(vmm, _mme);
                }
            }
        }

        private void CallP2mfMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            _gpu.EngineP2mf.CallMethod(vmm, methCall);
        }

        private void CallM2mfMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            _gpu.EngineM2mf.CallMethod(vmm, methCall);
        }
    }
}