using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Ryujinx.Gpu
{
    struct NsGpuPBEntry
    {
        public NsGpuRegister Register { get; private set; }

        public int SubChannel { get; private set; }

        private int[] m_Arguments;

        public ReadOnlyCollection<int> Arguments => Array.AsReadOnly(m_Arguments);

        public NsGpuPBEntry(NsGpuRegister Register, int SubChannel, params int[] Arguments)
        {
            this.Register    = Register;
            this.SubChannel  = SubChannel;
            this.m_Arguments = Arguments;
        }

        public static NsGpuPBEntry[] DecodePushBuffer(byte[] Data)
        {
            using (MemoryStream MS = new MemoryStream(Data))
            {
                BinaryReader Reader = new BinaryReader(MS);

                List<NsGpuPBEntry> GpFifos = new List<NsGpuPBEntry>();

                bool CanRead() => MS.Position + 4 <= MS.Length;

                while (CanRead())
                {
                    int Packed = Reader.ReadInt32();

                    int Reg  = (Packed << 2)  & 0x7ffc;
                    int SubC = (Packed >> 13) & 7;
                    int Args = (Packed >> 16) & 0x1fff;
                    int Mode = (Packed >> 29) & 7;

                    if (Mode == 4)
                    {
                        //Inline Mode.
                        GpFifos.Add(new NsGpuPBEntry((NsGpuRegister)Reg, SubC, Args));
                    }
                    else
                    {
                        //Word mode.
                        if (Mode == 1)
                        {
                            //Sequential Mode.
                            for (int Index = 0; Index < Args && CanRead(); Index++, Reg += 4)
                            {
                                GpFifos.Add(new NsGpuPBEntry((NsGpuRegister)Reg, SubC, Reader.ReadInt32()));
                            }
                        }
                        else
                        {
                            //Non-Sequential Mode.
                            int[] Arguments = new int[Args];

                            for (int Index = 0; Index < Args && CanRead(); Index++)
                            {
                                Arguments[Index] = Reader.ReadInt32();
                            }

                            GpFifos.Add(new NsGpuPBEntry((NsGpuRegister)Reg, SubC, Arguments));
                        }
                    }
                }

                return GpFifos.ToArray();
            }
        }
    }
}