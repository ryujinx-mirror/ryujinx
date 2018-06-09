using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.Gpu
{
    static class NvGpuPushBuffer
    {
        private enum SubmissionMode
        {
            Incrementing    = 1,
            NonIncrementing = 3,
            Immediate       = 4,
            IncrementOnce   = 5
        }

        public static NvGpuPBEntry[] Decode(byte[] Data)
        {
            using (MemoryStream MS = new MemoryStream(Data))
            {
                BinaryReader Reader = new BinaryReader(MS);

                List<NvGpuPBEntry> PushBuffer = new List<NvGpuPBEntry>();

                bool CanRead() => MS.Position + 4 <= MS.Length;

                while (CanRead())
                {
                    int Packed = Reader.ReadInt32();

                    int Meth = (Packed >> 0)  & 0x1fff;
                    int SubC = (Packed >> 13) & 7;
                    int Args = (Packed >> 16) & 0x1fff;
                    int Mode = (Packed >> 29) & 7;

                    switch ((SubmissionMode)Mode)
                    {
                        case SubmissionMode.Incrementing:
                        {
                            for (int Index = 0; Index < Args && CanRead(); Index++, Meth++)
                            {
                                PushBuffer.Add(new NvGpuPBEntry(Meth, SubC, Reader.ReadInt32()));
                            }

                            break;
                        }

                        case SubmissionMode.NonIncrementing:
                        {
                            int[] Arguments = new int[Args];

                            for (int Index = 0; Index < Arguments.Length; Index++)
                            {
                                if (!CanRead())
                                {
                                    break;
                                }

                                Arguments[Index] = Reader.ReadInt32();
                            }

                            PushBuffer.Add(new NvGpuPBEntry(Meth, SubC, Arguments));

                            break;
                        }

                        case SubmissionMode.Immediate:
                        {
                            PushBuffer.Add(new NvGpuPBEntry(Meth, SubC, Args));

                            break;
                        }

                        case SubmissionMode.IncrementOnce:
                        {
                            if (CanRead())
                            {
                                PushBuffer.Add(new NvGpuPBEntry(Meth, SubC, Reader.ReadInt32()));
                            }

                            if (CanRead() && Args > 1)
                            {
                                int[] Arguments = new int[Args - 1];

                                for (int Index = 0; Index < Arguments.Length && CanRead(); Index++)
                                {
                                    Arguments[Index] = Reader.ReadInt32();
                                }

                                PushBuffer.Add(new NvGpuPBEntry(Meth + 1, SubC, Arguments));
                            }

                            break;
                        }
                    }
                }

                return PushBuffer.ToArray();
            }
        }
    }
}