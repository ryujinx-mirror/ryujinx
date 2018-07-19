using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.Gpu.Memory;
using Ryujinx.HLE.Gpu.Texture;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.Gpu.Engines
{
    class NvGpuEngine2d : INvGpuEngine
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

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        public NvGpuEngine2d(NvGpu Gpu)
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

        public void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            if (Methods.TryGetValue(PBEntry.Method, out NvGpuMethod Method))
            {
                Method(Vmm, PBEntry);
            }
            else
            {
                WriteRegister(PBEntry);
            }
        }

        private void TextureCopy(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            CopyOperation Operation = (CopyOperation)ReadRegister(NvGpuEngine2dReg.CopyOperation);

            bool SrcLinear = ReadRegister(NvGpuEngine2dReg.SrcLinear) != 0;
            int  SrcWidth  = ReadRegister(NvGpuEngine2dReg.SrcWidth);
            int  SrcHeight = ReadRegister(NvGpuEngine2dReg.SrcHeight);
            int  SrcPitch  = ReadRegister(NvGpuEngine2dReg.SrcPitch);
            int  SrcBlkDim = ReadRegister(NvGpuEngine2dReg.SrcBlockDimensions);

            bool DstLinear = ReadRegister(NvGpuEngine2dReg.DstLinear) != 0;
            int  DstWidth  = ReadRegister(NvGpuEngine2dReg.DstWidth);
            int  DstHeight = ReadRegister(NvGpuEngine2dReg.DstHeight);
            int  DstPitch  = ReadRegister(NvGpuEngine2dReg.DstPitch);
            int  DstBlkDim = ReadRegister(NvGpuEngine2dReg.DstBlockDimensions);

            TextureSwizzle SrcSwizzle = SrcLinear
                ? TextureSwizzle.Pitch
                : TextureSwizzle.BlockLinear;

            TextureSwizzle DstSwizzle = DstLinear
                ? TextureSwizzle.Pitch
                : TextureSwizzle.BlockLinear;

            int SrcBlockHeight = 1 << ((SrcBlkDim >> 4) & 0xf);
            int DstBlockHeight = 1 << ((DstBlkDim >> 4) & 0xf);

            long SrcAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.SrcAddress);
            long DstAddress = MakeInt64From2xInt32(NvGpuEngine2dReg.DstAddress);

            long SrcKey = Vmm.GetPhysicalAddress(SrcAddress);
            long DstKey = Vmm.GetPhysicalAddress(DstAddress);

            bool IsSrcFb = Gpu.Engine3d.IsFrameBufferPosition(SrcKey);
            bool IsDstFb = Gpu.Engine3d.IsFrameBufferPosition(DstKey);

            TextureInfo SrcTexture()
            {
                return new TextureInfo(
                    SrcAddress,
                    SrcWidth,
                    SrcHeight,
                    SrcPitch,
                    SrcBlockHeight, 1,
                    SrcSwizzle,
                    GalTextureFormat.A8B8G8R8);
            }

            TextureInfo DstTexture()
            {
                return new TextureInfo(
                    DstAddress,
                    DstWidth,
                    DstHeight,
                    DstPitch,
                    DstBlockHeight, 1,
                    DstSwizzle,
                    GalTextureFormat.A8B8G8R8);
            }

            //TODO: fb -> fb copies, tex -> fb copies, formats other than RGBA8,
            //make it throw for unimpl stuff (like the copy mode)...
            if (IsSrcFb && IsDstFb)
            {
                //Frame Buffer -> Frame Buffer copy.
                Gpu.Renderer.FrameBuffer.Copy(
                    SrcKey,
                    DstKey,
                    0,
                    0,
                    SrcWidth,
                    SrcHeight,
                    0,
                    0,
                    DstWidth,
                    DstHeight);
            }
            if (IsSrcFb)
            {
                //Frame Buffer -> Texture copy.
                Gpu.Renderer.FrameBuffer.GetBufferData(SrcKey, (byte[] Buffer) =>
                {
                    TextureInfo Src = SrcTexture();
                    TextureInfo Dst = DstTexture();

                    if (Src.Width  != Dst.Width ||
                        Src.Height != Dst.Height)
                    {
                        throw new NotImplementedException("Texture resizing is not supported");
                    }

                    TextureWriter.Write(Vmm, Dst, Buffer);
                });
            }
            else if (IsDstFb)
            {
                //Texture -> Frame Buffer copy.
                const GalTextureFormat Format = GalTextureFormat.A8B8G8R8;

                byte[] Buffer = TextureReader.Read(Vmm, SrcTexture());

                Gpu.Renderer.FrameBuffer.SetBufferData(
                    DstKey,
                    DstWidth,
                    DstHeight,
                    Format,
                    Buffer);
            }
            else
            {
                //Texture -> Texture copy.
                TextureInfo Src = SrcTexture();
                TextureInfo Dst = DstTexture();

                if (Src.Width  != Dst.Width ||
                    Src.Height != Dst.Height)
                {
                    throw new NotImplementedException("Texture resizing is not supported");
                }

                TextureWriter.Write(Vmm, Dst, TextureReader.Read(Vmm, Src));
            }
        }

        private long MakeInt64From2xInt32(NvGpuEngine2dReg Reg)
        {
            return
                (long)Registers[(int)Reg + 0] << 32 |
                (uint)Registers[(int)Reg + 1];
        }

        private void WriteRegister(NvGpuPBEntry PBEntry)
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