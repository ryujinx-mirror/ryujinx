using Ryujinx.Graphics.Memory;
using System.Collections.Generic;

namespace Ryujinx.Graphics
{
    public class CdmaProcessor
    {
        private const int MethSetMethod = 0x10;
        private const int MethSetData   = 0x11;

        private NvGpu Gpu;

        public CdmaProcessor(NvGpu Gpu)
        {
            this.Gpu = Gpu;
        }

        public void PushCommands(NvGpuVmm Vmm, int[] CmdBuffer)
        {
            List<ChCommand> Commands = new List<ChCommand>();

            ChClassId CurrentClass = 0;

            for (int Index = 0; Index < CmdBuffer.Length; Index++)
            {
                int Cmd = CmdBuffer[Index];

                int Value        = (Cmd >> 0)  & 0xffff;
                int MethodOffset = (Cmd >> 16) & 0xfff;

                ChSubmissionMode SubmissionMode = (ChSubmissionMode)((Cmd >> 28) & 0xf);

                switch (SubmissionMode)
                {
                    case ChSubmissionMode.SetClass: CurrentClass = (ChClassId)(Value >> 6); break;

                    case ChSubmissionMode.Incrementing:
                    {
                        int Count = Value;

                        for (int ArgIdx = 0; ArgIdx < Count; ArgIdx++)
                        {
                            int Argument = CmdBuffer[++Index];

                            Commands.Add(new ChCommand(CurrentClass, MethodOffset + ArgIdx, Argument));
                        }

                        break;
                    }

                    case ChSubmissionMode.NonIncrementing:
                    {
                        int Count = Value;

                        int[] Arguments = new int[Count];

                        for (int ArgIdx = 0; ArgIdx < Count; ArgIdx++)
                        {
                            Arguments[ArgIdx] = CmdBuffer[++Index];
                        }

                        Commands.Add(new ChCommand(CurrentClass, MethodOffset, Arguments));

                        break;
                    }
                }
            }

            ProcessCommands(Vmm, Commands.ToArray());
        }

        private void ProcessCommands(NvGpuVmm Vmm, ChCommand[] Commands)
        {
            int MethodOffset = 0;

            foreach (ChCommand Command in Commands)
            {
                switch (Command.MethodOffset)
                {
                    case MethSetMethod: MethodOffset = Command.Arguments[0]; break;

                    case MethSetData:
                    {
                        if (Command.ClassId == ChClassId.NvDec)
                        {
                            Gpu.VideoDecoder.Process(Vmm, MethodOffset, Command.Arguments);
                        }
                        else if (Command.ClassId == ChClassId.GraphicsVic)
                        {
                            Gpu.VideoImageComposer.Process(Vmm, MethodOffset, Command.Arguments);
                        }

                        break;
                    }
                }
            }
        }
    }
}