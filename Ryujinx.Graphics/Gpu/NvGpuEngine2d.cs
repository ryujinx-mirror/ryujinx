using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu
{
    public class NvGpuEngine2d : INvGpuEngine
    {
        private enum CopyOperation
        {
            SrcCopyAnd,
            RopAnd,
            Blend,
            SrcCopy,
            Rop,
            SrcCopyPremult,
            BlendPremult
        }

        public int[] Registers { get; private set; }

        private NsGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        public NvGpuEngine2d(NsGpu Gpu)
        {
            this.Gpu = Gpu;

            Registers = new int[0xe00];

            Methods = new Dictionary<int, NvGpuMethod>();

            void AddMethod(int Meth, int Count, int Stride, NvGpuMethod Method)
            {
                while (Count-- > 0)
                {
                    Methods.Add(Meth, Method);

                    Meth += Stride;
                }
            }

            AddMethod(0xb5, 1, 1, TextureCopy);
        }

        public void CallMethod(AMemory Memory, NsGpuPBEntry PBEntry)
        {
            if (Methods.TryGetValue(PBEntry.Method, out NvGpuMethod Method))
            {
                Method(Memory, PBEntry);
            }
            else
            {
                WriteRegister(PBEntry);
            }
        }

        private void TextureCopy(AMemory Memory, NsGpuPBEntry PBEntry)
        {
            CopyOperation Operation = (CopyOperation)ReadRegister(NvGpuEngine2dReg.CopyOperation);

            bool SrcLinear = ReadRegister(NvGpuEngine2dReg.SrcLinear) != 0;
            int  SrcWidth  = ReadRegister(NvGpuEngine2dReg.SrcWidth);
            int  SrcHeight = ReadRegister(NvGpuEngine2dReg.SrcHeight);

            bool DstLinear = ReadRegister(NvGpuEngine2dReg.DstLinear) != 0;
            int  DstWidth  = ReadRegister(NvGpuEngine2dReg.DstWidth);
            int  DstHeight = ReadRegister(NvGpuEngine2dReg.DstHeight);
            int  DstPitch  = ReadRegister(NvGpuEngine2dReg.DstPitch);
            int  DstBlkDim = ReadRegister(NvGpuEngine2dReg.DstBlockDimensions);

            TextureSwizzle DstSwizzle = DstLinear
                ? TextureSwizzle.Pitch
                : TextureSwizzle.BlockLinear;

            int DstBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            long Tag = MakeInt64From2xInt32(NvGpuEngine2dReg.SrcAddress);

            TryGetCpuAddr(NvGpuEngine2dReg.SrcAddress, out long SrcAddress);
            TryGetCpuAddr(NvGpuEngine2dReg.DstAddress, out long DstAddress);

            bool IsFbTexture = Gpu.Engine3d.IsFrameBufferPosition(Tag);

            if (IsFbTexture && DstLinear)
            {
                DstSwizzle = TextureSwizzle.BlockLinear;
            }

            Texture DstTexture = new Texture(
                DstAddress,
                DstWidth,
                DstHeight,
                DstBlockHeight,
                DstBlockHeight,
                DstSwizzle,
                GalTextureFormat.A8B8G8R8);

            if (IsFbTexture)
            {
                Gpu.Renderer.GetFrameBufferData(Tag, (byte[] Buffer) =>
                {
                    CopyTexture(Memory, DstTexture, Buffer);
                });
            }
            else
            {
                long Size = SrcWidth * SrcHeight * 4;

                byte[] Buffer = AMemoryHelper.ReadBytes(Memory, SrcAddress, Size);

                CopyTexture(Memory, DstTexture, Buffer);
            }
        }

        private void CopyTexture(AMemory Memory, Texture Texture, byte[] Buffer)
        {
            TextureWriter.Write(Memory, Texture, Buffer);
        }

        private bool TryGetCpuAddr(NvGpuEngine2dReg Reg, out long Position)
        {
            Position = MakeInt64From2xInt32(Reg);

            Position = Gpu.GetCpuAddr(Position);

            return Position != -1;
        }

        private long MakeInt64From2xInt32(NvGpuEngine2dReg Reg)
        {
            return
                (long)Registers[(int)Reg + 0] << 32 |
                (uint)Registers[(int)Reg + 1];
        }

        private void WriteRegister(NsGpuPBEntry PBEntry)
        {
            int ArgsCount = PBEntry.Arguments.Count;

            if (ArgsCount > 0)
            {
                Registers[PBEntry.Method] = PBEntry.Arguments[ArgsCount - 1];
            }
        }

        private int ReadRegister(NvGpuEngine2dReg Reg)
        {
            return Registers[(int)Reg];
        }

        private void WriteRegister(NvGpuEngine2dReg Reg, int Value)
        {
            Registers[(int)Reg] = Value;
        }
    }
}