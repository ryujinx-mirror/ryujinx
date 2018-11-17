using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics
{
    class NvGpuFifo
    {
        private const int MacrosCount    = 0x80;
        private const int MacroIndexMask = MacrosCount - 1;

        //Note: The size of the macro memory is unknown, we just make
        //a guess here and use 256kb as the size. Increase if needed.
        private const int MmeWords = 256 * 256;

        private NvGpu Gpu;

        private NvGpuEngine[] SubChannels;

        private struct CachedMacro
        {
            public int Position { get; private set; }

            private bool ExecutionPending;
            private int  Argument;

            private MacroInterpreter Interpreter;

            public CachedMacro(NvGpuFifo PFifo, INvGpuEngine Engine, int Position)
            {
                this.Position = Position;

                ExecutionPending = false;
                Argument         = 0;

                Interpreter = new MacroInterpreter(PFifo, Engine);
            }

            public void StartExecution(int Argument)
            {
                this.Argument = Argument;

                ExecutionPending = true;
            }

            public void Execute(NvGpuVmm Vmm, int[] Mme)
            {
                if (ExecutionPending)
                {
                    ExecutionPending = false;

                    Interpreter?.Execute(Vmm, Mme, Position, Argument);
                }
            }

            public void PushArgument(int Argument)
            {
                Interpreter?.Fifo.Enqueue(Argument);
            }
        }

        private int CurrMacroPosition;
        private int CurrMacroBindIndex;

        private CachedMacro[] Macros;

        private int[] Mme;

        public NvGpuFifo(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            SubChannels = new NvGpuEngine[8];

            Macros = new CachedMacro[MacrosCount];

            Mme = new int[MmeWords];
        }

        public void CallMethod(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            if ((NvGpuFifoMeth)MethCall.Method == NvGpuFifoMeth.BindChannel)
            {
                NvGpuEngine Engine = (NvGpuEngine)MethCall.Argument;

                SubChannels[MethCall.SubChannel] = Engine;
            }
            else
            {
                switch (SubChannels[MethCall.SubChannel])
                {
                    case NvGpuEngine._2d:  Call2dMethod  (Vmm, MethCall); break;
                    case NvGpuEngine._3d:  Call3dMethod  (Vmm, MethCall); break;
                    case NvGpuEngine.P2mf: CallP2mfMethod(Vmm, MethCall); break;
                    case NvGpuEngine.M2mf: CallM2mfMethod(Vmm, MethCall); break;
                }
            }
        }

        private void Call2dMethod(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            Gpu.Engine2d.CallMethod(Vmm, MethCall);
        }

        private void Call3dMethod(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            if (MethCall.Method < 0x80)
            {
                switch ((NvGpuFifoMeth)MethCall.Method)
                {
                    case NvGpuFifoMeth.SetMacroUploadAddress:
                    {
                        CurrMacroPosition = MethCall.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.SendMacroCodeData:
                    {
                        Mme[CurrMacroPosition++] = MethCall.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroBindingIndex:
                    {
                        CurrMacroBindIndex = MethCall.Argument;

                        break;
                    }

                    case NvGpuFifoMeth.BindMacro:
                    {
                        int Position = MethCall.Argument;

                        Macros[CurrMacroBindIndex] = new CachedMacro(this, Gpu.Engine3d, Position);

                        break;
                    }

                    default: CallP2mfMethod(Vmm, MethCall); break;
                }
            }
            else if (MethCall.Method < 0xe00)
            {
                Gpu.Engine3d.CallMethod(Vmm, MethCall);
            }
            else
            {
                int MacroIndex = (MethCall.Method >> 1) & MacroIndexMask;

                if ((MethCall.Method & 1) != 0)
                {
                    Macros[MacroIndex].PushArgument(MethCall.Argument);
                }
                else
                {
                    Macros[MacroIndex].StartExecution(MethCall.Argument);
                }

                if (MethCall.IsLastCall)
                {
                    Macros[MacroIndex].Execute(Vmm, Mme);
                }
            }
        }

        private void CallP2mfMethod(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            Gpu.EngineP2mf.CallMethod(Vmm, MethCall);
        }

        private void CallM2mfMethod(NvGpuVmm Vmm, GpuMethodCall MethCall)
        {
            Gpu.EngineM2mf.CallMethod(Vmm, MethCall);
        }
    }
}