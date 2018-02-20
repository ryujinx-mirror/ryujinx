using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu
{
    class NsGpuPGraph
    {
        private NsGpu Gpu;

        private int[] Registers;

        public NsGpuEngine[] SubChannels;

        private Dictionary<long, int> CurrentVertexBuffers;

        public NsGpuPGraph(NsGpu Gpu)
        {
            this.Gpu = Gpu;

            Registers = new int[0x1000];

            SubChannels = new NsGpuEngine[8];

            CurrentVertexBuffers = new Dictionary<long, int>();
        }

        public void ProcessPushBuffer(NsGpuPBEntry[] PushBuffer, AMemory Memory)
        {
            bool HasQuery = false;

            foreach (NsGpuPBEntry Entry in PushBuffer)
            {
                if (Entry.Arguments.Count == 1)
                {
                    SetRegister(Entry.Register, Entry.Arguments[0]);
                }

                switch (Entry.Register)
                {
                    case NsGpuRegister.BindChannel:
                        if (Entry.Arguments.Count > 0)
                        {
                            SubChannels[Entry.SubChannel] = (NsGpuEngine)Entry.Arguments[0];
                        }
                        break;

                    case NsGpuRegister._3dVertexArray0Fetch:
                        SendVertexBuffers(Memory);
                        break;
                    
                    case NsGpuRegister._3dCbData0:
                        if (GetRegister(NsGpuRegister._3dCbPos) == 0x20)
                        {
                            SendTexture(Memory);
                        }
                        break;

                    case NsGpuRegister._3dQueryAddressHigh:
                    case NsGpuRegister._3dQueryAddressLow:
                    case NsGpuRegister._3dQuerySequence:
                    case NsGpuRegister._3dQueryGet:
                        HasQuery = true;
                        break;
                }
            }

            if (HasQuery)
            {
                long Position =
                    (long)GetRegister(NsGpuRegister._3dQueryAddressHigh) << 32 |
                    (long)GetRegister(NsGpuRegister._3dQueryAddressLow)  << 0;

                int Seq = GetRegister(NsGpuRegister._3dQuerySequence);
                int Get = GetRegister(NsGpuRegister._3dQueryGet);

                int Mode = Get & 3;

                if (Mode == 0)
                {
                    //Write
                    Position = Gpu.MemoryMgr.GetCpuAddr(Position);

                    if (Position != -1)
                    {
                        Gpu.Renderer.QueueAction(delegate()
                        {
                            Memory.WriteInt32(Position, Seq);
                        });
                    }
                }
            }
        }

        private void SendVertexBuffers(AMemory Memory)
        {
            long Position =
                (long)GetRegister(NsGpuRegister._3dVertexArray0StartHigh) << 32 |
                (long)GetRegister(NsGpuRegister._3dVertexArray0StartLow)  << 0;

            long Limit =
                (long)GetRegister(NsGpuRegister._3dVertexArray0LimitHigh) << 32 |
                (long)GetRegister(NsGpuRegister._3dVertexArray0LimitLow)  << 0;

            int VbIndex = CurrentVertexBuffers.Count;

            if (!CurrentVertexBuffers.TryAdd(Position, VbIndex))
            {
                VbIndex = CurrentVertexBuffers[Position];
            }

            if (Limit != 0)
            {
                long Size = (Limit - Position) + 1;

                Position = Gpu.MemoryMgr.GetCpuAddr(Position);

                if (Position != -1)
                {
                    byte[] Buffer = AMemoryHelper.ReadBytes(Memory, Position, (int)Size);

                    int Stride = GetRegister(NsGpuRegister._3dVertexArray0Fetch) & 0xfff;

                    List<GalVertexAttrib> Attribs = new List<GalVertexAttrib>();

                    for (int Attr = 0; Attr < 16; Attr++)
                    {
                        int Packed = GetRegister(NsGpuRegister._3dVertexAttrib0Format + Attr * 4);

                        GalVertexAttrib Attrib = new GalVertexAttrib(Attr,
                                                  (Packed >>  0) & 0x1f,
                                                 ((Packed >>  6) & 0x1) != 0,
                                                  (Packed >>  7) & 0x3fff,
                            (GalVertexAttribSize)((Packed >> 21) & 0x3f),
                            (GalVertexAttribType)((Packed >> 27) & 0x7),
                                                 ((Packed >> 31) & 0x1) != 0);

                        if (Attrib.Offset < Stride)
                        {
                            Attribs.Add(Attrib);
                        }
                    }

                    Gpu.Renderer.QueueAction(delegate()
                    {
                        Gpu.Renderer.SendVertexBuffer(VbIndex, Buffer, Stride, Attribs.ToArray());
                    });
                }
            }
        }

        private void SendTexture(AMemory Memory)
        {
            long TicPos = (long)GetRegister(NsGpuRegister._3dTicAddressHigh) << 32 |
                          (long)GetRegister(NsGpuRegister._3dTicAddressLow)  << 0;

            int CbData = GetRegister(NsGpuRegister._3dCbData0);

            int TicIndex = (CbData >>  0) & 0xfffff;
            int TscIndex = (CbData >> 20) & 0xfff; //I guess?

            TicPos = Gpu.MemoryMgr.GetCpuAddr(TicPos + TicIndex * 0x20);

            if (TicPos != -1)
            {
                int Word0 = Memory.ReadInt32(TicPos + 0x0);
                int Word1 = Memory.ReadInt32(TicPos + 0x4);
                int Word2 = Memory.ReadInt32(TicPos + 0x8);
                int Word3 = Memory.ReadInt32(TicPos + 0xc);
                int Word4 = Memory.ReadInt32(TicPos + 0x10);
                int Word5 = Memory.ReadInt32(TicPos + 0x14);
                int Word6 = Memory.ReadInt32(TicPos + 0x18);
                int Word7 = Memory.ReadInt32(TicPos + 0x1c);

                long TexAddress = Word1;

                TexAddress |= (long)(Word2 & 0xff) << 32;

                TexAddress = Gpu.MemoryMgr.GetCpuAddr(TexAddress);

                if (TexAddress != -1)
                {
                    NsGpuTextureFormat Format = (NsGpuTextureFormat)(Word0 & 0x7f);

                    int Width  = (Word4 & 0xffff) + 1;
                    int Height = (Word5 & 0xffff) + 1;

                    byte[] Buffer = GetDecodedTexture(Memory, Format, TexAddress, Width, Height);

                    if (Buffer != null)
                    {
                        Gpu.Renderer.QueueAction(delegate()
                        {
                            Gpu.Renderer.SendR8G8B8A8Texture(0, Buffer, Width, Height);
                        });
                    }
                }
            }
        }

        private static byte[] GetDecodedTexture(
            AMemory            Memory,
            NsGpuTextureFormat Format,
            long               Position,
            int                Width,
            int                Height)
        {
            byte[] Data = null;

            switch (Format)
            {
                case NsGpuTextureFormat.BC1:
                {
                    int Size = (Width * Height) >> 1;

                    Data = AMemoryHelper.ReadBytes(Memory, Position, Size);

                    Data = BCn.DecodeBC1(new NsGpuTexture()
                    {
                        Width  = Width,
                        Height = Height,
                        Data   = Data
                    }, 0);

                    break;
                }

                case NsGpuTextureFormat.BC2:
                {
                    int Size = Width * Height;

                    Data = AMemoryHelper.ReadBytes(Memory, Position, Size);

                    Data = BCn.DecodeBC2(new NsGpuTexture()
                    {
                        Width  = Width,
                        Height = Height,
                        Data   = Data
                    }, 0);

                    break;
                }

                case NsGpuTextureFormat.BC3:
                {
                    int Size = Width * Height;

                    Data = AMemoryHelper.ReadBytes(Memory, Position, Size);

                    Data = BCn.DecodeBC3(new NsGpuTexture()
                    {
                        Width  = Width,
                        Height = Height,
                        Data   = Data
                    }, 0);

                    break;
                }

                //default: throw new NotImplementedException(Format.ToString());
            }

            return Data;
        }

        public int GetRegister(NsGpuRegister Register)
        {
            return Registers[((int)Register >> 2) & 0xfff];
        }

        public void SetRegister(NsGpuRegister Register, int Value)
        {
            Registers[((int)Register >> 2) & 0xfff] = Value;
        }
    }
}