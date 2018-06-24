using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.Gpu.Memory;
using Ryujinx.HLE.Gpu.Texture;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.Gpu.Engines
{
    class NvGpuEngine3d : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu Gpu;

        private Dictionary<int, NvGpuMethod> Methods;

        private struct ConstBuffer
        {
            public bool Enabled;
            public long Position;
            public int  Size;
        }

        private ConstBuffer[][] ConstBuffers;

        private HashSet<long> FrameBuffers;

        public NvGpuEngine3d(NvGpu Gpu)
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

            AddMethod(0x585,  1, 1, VertexEndGl);
            AddMethod(0x674,  1, 1, ClearBuffers);
            AddMethod(0x6c3,  1, 1, QueryControl);
            AddMethod(0x8e4, 16, 1, CbData);
            AddMethod(0x904,  5, 8, CbBind);

            ConstBuffers = new ConstBuffer[6][];

            for (int Index = 0; Index < ConstBuffers.Length; Index++)
            {
                ConstBuffers[Index] = new ConstBuffer[18];
            }

            FrameBuffers = new HashSet<long>();
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

        private void VertexEndGl(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            SetFrameBuffer(Vmm, 0);

            long[] Keys = UploadShaders(Vmm);

            Gpu.Renderer.Shader.BindProgram();

            SetAlphaBlending();

            UploadTextures(Vmm, Keys);
            UploadUniforms(Vmm);
            UploadVertexArrays(Vmm);
        }

        private void ClearBuffers(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            int Arg0 = PBEntry.Arguments[0];

            int FbIndex = (Arg0 >> 6) & 0xf;

            int Layer = (Arg0 >> 10) & 0x3ff;

            GalClearBufferFlags Flags = (GalClearBufferFlags)(Arg0 & 0x3f);

            SetFrameBuffer(Vmm, 0);

            //TODO: Enable this once the frame buffer problems are fixed.
            //Gpu.Renderer.ClearBuffers(Layer, Flags);
        }

        private void SetFrameBuffer(NvGpuVmm Vmm, int FbIndex)
        {
            long VA = MakeInt64From2xInt32(NvGpuEngine3dReg.FrameBufferNAddress + FbIndex * 0x10);

            long PA = Vmm.GetPhysicalAddress(VA);

            FrameBuffers.Add(PA);

            int Width  = ReadRegister(NvGpuEngine3dReg.FrameBufferNWidth  + FbIndex * 0x10);
            int Height = ReadRegister(NvGpuEngine3dReg.FrameBufferNHeight + FbIndex * 0x10);

            //Note: Using the Width/Height results seems to give incorrect results.
            //Maybe the size of all frame buffers is hardcoded to screen size? This seems unlikely.
            Gpu.Renderer.FrameBuffer.Create(PA, 1280, 720);
            Gpu.Renderer.FrameBuffer.Bind(PA);
        }

        private long[] UploadShaders(NvGpuVmm Vmm)
        {
            long[] Keys = new long[5];

            long BasePosition = MakeInt64From2xInt32(NvGpuEngine3dReg.ShaderAddress);

            for (int Index = 0; Index < 6; Index++)
            {
                int Control = ReadRegister(NvGpuEngine3dReg.ShaderNControl + Index * 0x10);
                int Offset  = ReadRegister(NvGpuEngine3dReg.ShaderNOffset  + Index * 0x10);

                //Note: Vertex Program (B) is always enabled.
                bool Enable = (Control & 1) != 0 || Index == 1;

                if (!Enable)
                {
                    continue;
                }

                long Key = BasePosition + (uint)Offset;

                GalShaderType ShaderType = GetTypeFromProgram(Index);

                Keys[(int)ShaderType] = Key;

                Gpu.Renderer.Shader.Create(Vmm, Key, ShaderType);
                Gpu.Renderer.Shader.Bind(Key);
            }

            int RawSX = ReadRegister(NvGpuEngine3dReg.ViewportScaleX);
            int RawSY = ReadRegister(NvGpuEngine3dReg.ViewportScaleY);

            float SX = BitConverter.Int32BitsToSingle(RawSX);
            float SY = BitConverter.Int32BitsToSingle(RawSY);

            float SignX = MathF.Sign(SX);
            float SignY = MathF.Sign(SY);

            Gpu.Renderer.Shader.SetFlip(SignX, SignY);

            return Keys;
        }

        private static GalShaderType GetTypeFromProgram(int Program)
        {
            switch (Program)
            {
                case 0:
                case 1: return GalShaderType.Vertex;
                case 2: return GalShaderType.TessControl;
                case 3: return GalShaderType.TessEvaluation;
                case 4: return GalShaderType.Geometry;
                case 5: return GalShaderType.Fragment;
            }

            throw new ArgumentOutOfRangeException(nameof(Program));
        }

        private void SetAlphaBlending()
        {
            //TODO: Support independent blend properly.
            bool Enable = (ReadRegister(NvGpuEngine3dReg.IBlendNEnable) & 1) != 0;

            if (Enable)
            {
                Gpu.Renderer.Blend.Enable();
            }
            else
            {
                Gpu.Renderer.Blend.Disable();
            }

            if (!Enable)
            {
                //If blend is not enabled, then the other values have no effect.
                //Note that if it is disabled, the register may contain invalid values.
                return;
            }

            bool BlendSeparateAlpha = (ReadRegister(NvGpuEngine3dReg.IBlendNSeparateAlpha) & 1) != 0;

            GalBlendEquation EquationRgb = (GalBlendEquation)ReadRegister(NvGpuEngine3dReg.IBlendNEquationRgb);

            GalBlendFactor FuncSrcRgb = (GalBlendFactor)ReadRegister(NvGpuEngine3dReg.IBlendNFuncSrcRgb);
            GalBlendFactor FuncDstRgb = (GalBlendFactor)ReadRegister(NvGpuEngine3dReg.IBlendNFuncDstRgb);

            if (BlendSeparateAlpha)
            {
                GalBlendEquation EquationAlpha = (GalBlendEquation)ReadRegister(NvGpuEngine3dReg.IBlendNEquationAlpha);

                GalBlendFactor FuncSrcAlpha = (GalBlendFactor)ReadRegister(NvGpuEngine3dReg.IBlendNFuncSrcAlpha);
                GalBlendFactor FuncDstAlpha = (GalBlendFactor)ReadRegister(NvGpuEngine3dReg.IBlendNFuncDstAlpha);

                Gpu.Renderer.Blend.SetSeparate(
                    EquationRgb,
                    EquationAlpha,
                    FuncSrcRgb,
                    FuncDstRgb,
                    FuncSrcAlpha,
                    FuncDstAlpha);
            }
            else
            {
                Gpu.Renderer.Blend.Set(EquationRgb, FuncSrcRgb, FuncDstRgb);
            }
        }

        private void UploadTextures(NvGpuVmm Vmm, long[] Keys)
        {
            long BaseShPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.ShaderAddress);

            int TextureCbIndex = ReadRegister(NvGpuEngine3dReg.TextureCbIndex);

            //Note: On the emulator renderer, Texture Unit 0 is
            //reserved for drawing the frame buffer.
            int TexIndex = 1;

            for (int Index = 0; Index < Keys.Length; Index++)
            {
                foreach (ShaderDeclInfo DeclInfo in Gpu.Renderer.Shader.GetTextureUsage(Keys[Index]))
                {
                    long Position = ConstBuffers[Index][TextureCbIndex].Position;

                    UploadTexture(Vmm, Position, TexIndex, DeclInfo.Index);

                    Gpu.Renderer.Shader.EnsureTextureBinding(DeclInfo.Name, TexIndex);

                    TexIndex++;
                }
            }
        }

        private void UploadTexture(NvGpuVmm Vmm, long BasePosition, int TexIndex, int HndIndex)
        {
            long Position = BasePosition + HndIndex * 4;

            int TextureHandle = Vmm.ReadInt32(Position);

            if (TextureHandle == 0)
            {
                //TODO: Is this correct?
                //Some games like puyo puyo will have 0 handles.
                //It may be just normal behaviour or a bug caused by sync issues.
                //The game does initialize the value properly after through.
                return;
            }

            int TicIndex = (TextureHandle >>  0) & 0xfffff;
            int TscIndex = (TextureHandle >> 20) & 0xfff;

            long TicPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.TexHeaderPoolOffset);
            long TscPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.TexSamplerPoolOffset);

            TicPosition += TicIndex * 0x20;
            TscPosition += TscIndex * 0x20;

            GalTextureSampler Sampler = TextureFactory.MakeSampler(Gpu, Vmm, TscPosition);

            long TextureAddress = Vmm.ReadInt64(TicPosition + 4) & 0xffffffffffff;

            long Key = TextureAddress;

            TextureAddress = Vmm.GetPhysicalAddress(TextureAddress);

            if (IsFrameBufferPosition(TextureAddress))
            {
                //This texture is a frame buffer texture,
                //we shouldn't read anything from memory and bind
                //the frame buffer texture instead, since we're not
                //really writing anything to memory.
                Gpu.Renderer.FrameBuffer.BindTexture(TextureAddress, TexIndex);
            }
            else
            {
                GalTexture NewTexture = TextureFactory.MakeTexture(Vmm, TicPosition);

                long Size = (uint)TextureHelper.GetTextureSize(NewTexture);

                bool HasCachedTexture = false;

                if (Gpu.Renderer.Texture.TryGetCachedTexture(Key, Size, out GalTexture Texture))
                {
                    if (NewTexture.Equals(Texture) && !Vmm.IsRegionModified(Key, Size, NvGpuBufferType.Texture))
                    {
                        Gpu.Renderer.Texture.Bind(Key, TexIndex);

                        HasCachedTexture = true;
                    }
                }

                if (!HasCachedTexture)
                {
                    byte[] Data = TextureFactory.GetTextureData(Vmm, TicPosition);

                    Gpu.Renderer.Texture.Create(Key, Data, NewTexture);
                }

                Gpu.Renderer.Texture.Bind(Key, TexIndex);
            }

            Gpu.Renderer.Texture.SetSampler(Sampler);
        }

        private void UploadUniforms(NvGpuVmm Vmm)
        {
            long BasePosition = MakeInt64From2xInt32(NvGpuEngine3dReg.ShaderAddress);

            for (int Index = 0; Index < 5; Index++)
            {
                int Control = ReadRegister(NvGpuEngine3dReg.ShaderNControl + (Index + 1) * 0x10);
                int Offset  = ReadRegister(NvGpuEngine3dReg.ShaderNOffset  + (Index + 1) * 0x10);

                //Note: Vertex Program (B) is always enabled.
                bool Enable = (Control & 1) != 0 || Index == 0;

                if (!Enable)
                {
                    continue;
                }

                for (int Cbuf = 0; Cbuf < ConstBuffers[Index].Length; Cbuf++)
                {
                    ConstBuffer Cb = ConstBuffers[Index][Cbuf];

                    if (Cb.Enabled)
                    {
                        byte[] Data = Vmm.ReadBytes(Cb.Position, (uint)Cb.Size);

                        Gpu.Renderer.Shader.SetConstBuffer(BasePosition + (uint)Offset, Cbuf, Data);
                    }
                }
            }
        }

        private void UploadVertexArrays(NvGpuVmm Vmm)
        {
            long IndexPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.IndexArrayAddress);

            int IndexEntryFmt = ReadRegister(NvGpuEngine3dReg.IndexArrayFormat);
            int IndexFirst    = ReadRegister(NvGpuEngine3dReg.IndexBatchFirst);
            int IndexCount    = ReadRegister(NvGpuEngine3dReg.IndexBatchCount);

            GalIndexFormat IndexFormat = (GalIndexFormat)IndexEntryFmt;

            int IndexEntrySize = 1 << IndexEntryFmt;

            if (IndexEntrySize > 4)
            {
                throw new InvalidOperationException();
            }

            if (IndexCount != 0)
            {
                int IbSize = IndexCount * IndexEntrySize;

                bool IboCached = Gpu.Renderer.Rasterizer.IsIboCached(IndexPosition, (uint)IbSize);

                if (!IboCached || Vmm.IsRegionModified(IndexPosition, (uint)IbSize, NvGpuBufferType.Index))
                {
                    byte[] Data = Vmm.ReadBytes(IndexPosition, (uint)IbSize);

                    Gpu.Renderer.Rasterizer.CreateIbo(IndexPosition, Data);
                }

                Gpu.Renderer.Rasterizer.SetIndexArray(IndexPosition, IbSize, IndexFormat);
            }

            List<GalVertexAttrib>[] Attribs = new List<GalVertexAttrib>[32];

            for (int Attr = 0; Attr < 16; Attr++)
            {
                int Packed = ReadRegister(NvGpuEngine3dReg.VertexAttribNFormat + Attr);

                int ArrayIndex = Packed & 0x1f;

                if (Attribs[ArrayIndex] == null)
                {
                    Attribs[ArrayIndex] = new List<GalVertexAttrib>();
                }

                Attribs[ArrayIndex].Add(new GalVertexAttrib(
                                           Attr,
                                         ((Packed >>  6) & 0x1) != 0,
                                          (Packed >>  7) & 0x3fff,
                    (GalVertexAttribSize)((Packed >> 21) & 0x3f),
                    (GalVertexAttribType)((Packed >> 27) & 0x7),
                                         ((Packed >> 31) & 0x1) != 0));
            }

            int VertexFirst = ReadRegister(NvGpuEngine3dReg.VertexArrayFirst);
            int VertexCount = ReadRegister(NvGpuEngine3dReg.VertexArrayCount);

            int PrimCtrl = ReadRegister(NvGpuEngine3dReg.VertexBeginGl);

            for (int Index = 0; Index < 32; Index++)
            {
                if (Attribs[Index] == null)
                {
                    continue;
                }

                int Control = ReadRegister(NvGpuEngine3dReg.VertexArrayNControl + Index * 4);

                bool Enable = (Control & 0x1000) != 0;

                long VertexPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.VertexArrayNAddress + Index * 4);
                long VertexEndPos   = MakeInt64From2xInt32(NvGpuEngine3dReg.VertexArrayNEndAddr + Index * 2);

                if (!Enable)
                {
                    continue;
                }

                int Stride = Control & 0xfff;

                long VbSize = 0;

                if (IndexCount != 0)
                {
                    VbSize = (VertexEndPos - VertexPosition) + 1;
                }
                else
                {
                    VbSize = VertexCount * Stride;
                }

                bool VboCached = Gpu.Renderer.Rasterizer.IsVboCached(VertexPosition, VbSize);

                if (!VboCached || Vmm.IsRegionModified(VertexPosition, VbSize, NvGpuBufferType.Vertex))
                {
                    byte[] Data = Vmm.ReadBytes(VertexPosition, VbSize);

                    Gpu.Renderer.Rasterizer.CreateVbo(VertexPosition, Data);
                }

                Gpu.Renderer.Rasterizer.SetVertexArray(Index, Stride, VertexPosition, Attribs[Index].ToArray());
            }

            GalPrimitiveType PrimType = (GalPrimitiveType)(PrimCtrl & 0xffff);

            if (IndexCount != 0)
            {
                Gpu.Renderer.Rasterizer.DrawElements(IndexPosition, IndexFirst, PrimType);
            }
            else
            {
                Gpu.Renderer.Rasterizer.DrawArrays(VertexFirst, VertexCount, PrimType);
            }
        }

        private void QueryControl(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            long Position = MakeInt64From2xInt32(NvGpuEngine3dReg.QueryAddress);

            int Seq  = Registers[(int)NvGpuEngine3dReg.QuerySequence];
            int Ctrl = Registers[(int)NvGpuEngine3dReg.QueryControl];

            int Mode = Ctrl & 3;

            if (Mode == 0)
            {
                //Write mode.
                Vmm.WriteInt32(Position, Seq);
            }

            WriteRegister(PBEntry);
        }

        private void CbData(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            long Position = MakeInt64From2xInt32(NvGpuEngine3dReg.ConstBufferAddress);

            int Offset = ReadRegister(NvGpuEngine3dReg.ConstBufferOffset);

            foreach (int Arg in PBEntry.Arguments)
            {
                Vmm.WriteInt32(Position + Offset, Arg);

                Offset += 4;
            }

            WriteRegister(NvGpuEngine3dReg.ConstBufferOffset, Offset);
        }

        private void CbBind(NvGpuVmm Vmm, NvGpuPBEntry PBEntry)
        {
            int Stage = (PBEntry.Method - 0x904) >> 3;

            int Index = PBEntry.Arguments[0];

            bool Enabled = (Index & 1) != 0;

            Index = (Index >> 4) & 0x1f;

            long Position = MakeInt64From2xInt32(NvGpuEngine3dReg.ConstBufferAddress);

            ConstBuffers[Stage][Index].Position = Position;
            ConstBuffers[Stage][Index].Enabled  = Enabled;

            ConstBuffers[Stage][Index].Size = ReadRegister(NvGpuEngine3dReg.ConstBufferSize);
        }

        private long MakeInt64From2xInt32(NvGpuEngine3dReg Reg)
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

        private int ReadRegister(NvGpuEngine3dReg Reg)
        {
            return Registers[(int)Reg];
        }

        private void WriteRegister(NvGpuEngine3dReg Reg, int Value)
        {
            Registers[(int)Reg] = Value;
        }

        public bool IsFrameBufferPosition(long Position)
        {
            return FrameBuffers.Contains(Position);
        }
    }
}