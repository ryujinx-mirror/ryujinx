using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.VDec;
using Ryujinx.Graphics.Vic;
using System.Collections.Generic;

namespace Ryujinx.Graphics
{
    public class CdmaProcessor
    {
        private const int MethSetMethod = 0x10;
        private const int MethSetData   = 0x11;

        private readonly VideoDecoder _videoDecoder;
        private readonly VideoImageComposer _videoImageComposer;

        public CdmaProcessor()
        {
            _videoDecoder = new VideoDecoder();
            _videoImageComposer = new VideoImageComposer(_videoDecoder);
        }

        public void PushCommands(GpuContext gpu, int[] cmdBuffer)
        {
            List<ChCommand> commands = new List<ChCommand>();

            ChClassId currentClass = 0;

            for (int index = 0; index < cmdBuffer.Length; index++)
            {
                int cmd = cmdBuffer[index];

                int value        = (cmd >> 0)  & 0xffff;
                int methodOffset = (cmd >> 16) & 0xfff;

                ChSubmissionMode submissionMode = (ChSubmissionMode)((cmd >> 28) & 0xf);

                switch (submissionMode)
                {
                    case ChSubmissionMode.SetClass: currentClass = (ChClassId)(value >> 6); break;

                    case ChSubmissionMode.Incrementing:
                    {
                        int count = value;

                        for (int argIdx = 0; argIdx < count; argIdx++)
                        {
                            int argument = cmdBuffer[++index];

                            commands.Add(new ChCommand(currentClass, methodOffset + argIdx, argument));
                        }

                        break;
                    }

                    case ChSubmissionMode.NonIncrementing:
                    {
                        int count = value;

                        int[] arguments = new int[count];

                        for (int argIdx = 0; argIdx < count; argIdx++)
                        {
                            arguments[argIdx] = cmdBuffer[++index];
                        }

                        commands.Add(new ChCommand(currentClass, methodOffset, arguments));

                        break;
                    }
                }
            }

            ProcessCommands(gpu, commands.ToArray());
        }

        private void ProcessCommands(GpuContext gpu, ChCommand[] commands)
        {
            int methodOffset = 0;

            foreach (ChCommand command in commands)
            {
                switch (command.MethodOffset)
                {
                    case MethSetMethod: methodOffset = command.Arguments[0]; break;

                    case MethSetData:
                    {
                        if (command.ClassId == ChClassId.NvDec)
                        {
                            _videoDecoder.Process(gpu, methodOffset, command.Arguments);
                        }
                        else if (command.ClassId == ChClassId.GraphicsVic)
                        {
                            _videoImageComposer.Process(gpu, methodOffset, command.Arguments);
                        }

                        break;
                    }
                }
            }
        }
    }
}