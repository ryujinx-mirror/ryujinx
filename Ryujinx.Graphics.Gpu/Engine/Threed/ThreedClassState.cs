using Ryujinx.Common.Memory;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.InlineToMemory;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Shader stage name.
    /// </summary>
    enum ShaderType
    {
        Vertex,
        TessellationControl,
        TessellationEvaluation,
        Geometry,
        Fragment
    }

    /// <summary>
    /// Tessellation mode.
    /// </summary>
    struct TessMode
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks the tessellation abstract patch type.
        /// </summary>
        /// <returns>Abtract patch type</returns>
        public TessPatchType UnpackPatchType()
        {
            return (TessPatchType)(Packed & 3);
        }

        /// <summary>
        /// Unpacks the spacing between tessellated vertices of the patch.
        /// </summary>
        /// <returns>Spacing between tessellated vertices</returns>
        public TessSpacing UnpackSpacing()
        {
            return (TessSpacing)((Packed >> 4) & 3);
        }

        /// <summary>
        /// Unpacks the primitive winding order.
        /// </summary>
        /// <returns>True if clockwise, false if counter-clockwise</returns>
        public bool UnpackCw()
        {
            return (Packed & (1 << 8)) != 0;
        }
    }

    /// <summary>
    /// Transform feedback buffer state.
    /// </summary>
    struct TfBufferState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public GpuVa Address;
        public int Size;
        public int Offset;
        public uint Padding0;
        public uint Padding1;
        public uint Padding2;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Transform feedback state.
    /// </summary>
    struct TfState
    {
#pragma warning disable CS0649
        public int BufferIndex;
        public int VaryingsCount;
        public int Stride;
        public uint Padding;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Render target color buffer state.
    /// </summary>
    struct RtColorState
    {
#pragma warning disable CS0649
        public GpuVa Address;
        public int WidthOrStride;
        public int Height;
        public ColorFormat Format;
        public MemoryLayout MemoryLayout;
        public int Depth;
        public int LayerSize;
        public int BaseLayer;
        public int Unknown0x24;
        public int Padding0;
        public int Padding1;
        public int Padding2;
        public int Padding3;
        public int Padding4;
        public int Padding5;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Viewport transform parameters, for viewport transformation.
    /// </summary>
    struct ViewportTransform
    {
#pragma warning disable CS0649
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float TranslateX;
        public float TranslateY;
        public float TranslateZ;
        public uint Swizzle;
        public uint SubpixelPrecisionBias;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks viewport swizzle of the position X component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleX()
        {
            return (ViewportSwizzle)(Swizzle & 7);
        }

        /// <summary>
        /// Unpacks viewport swizzle of the position Y component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleY()
        {
            return (ViewportSwizzle)((Swizzle >> 4) & 7);
        }

        /// <summary>
        /// Unpacks viewport swizzle of the position Z component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleZ()
        {
            return (ViewportSwizzle)((Swizzle >> 8) & 7);
        }

        /// <summary>
        /// Unpacks viewport swizzle of the position W component.
        /// </summary>
        /// <returns>Swizzle enum value</returns>
        public ViewportSwizzle UnpackSwizzleW()
        {
            return (ViewportSwizzle)((Swizzle >> 12) & 7);
        }
    }

    /// <summary>
    /// Viewport extents for viewport clipping, also includes depth range.
    /// </summary>
    struct ViewportExtents
    {
#pragma warning disable CS0649
        public ushort X;
        public ushort Width;
        public ushort Y;
        public ushort Height;
        public float DepthNear;
        public float DepthFar;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Draw state for non-indexed draws.
    /// </summary>
    struct VertexBufferDrawState
    {
#pragma warning disable CS0649
        public int First;
        public int Count;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Color buffer clear color.
    /// </summary>
    struct ClearColors
    {
#pragma warning disable CS0649
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Depth bias (also called polygon offset) parameters.
    /// </summary>
    struct DepthBiasState
    {
#pragma warning disable CS0649
        public Boolean32 PointEnable;
        public Boolean32 LineEnable;
        public Boolean32 FillEnable;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Scissor state.
    /// </summary>
    struct ScissorState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public ushort X1;
        public ushort X2;
        public ushort Y1;
        public ushort Y2;
        public uint Padding;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Stencil test masks for back tests.
    /// </summary>
    struct StencilBackMasks
    {
#pragma warning disable CS0649
        public int FuncRef;
        public int Mask;
        public int FuncMask;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Render target depth-stencil buffer state.
    /// </summary>
    struct RtDepthStencilState
    {
#pragma warning disable CS0649
        public GpuVa Address;
        public ZetaFormat Format;
        public MemoryLayout MemoryLayout;
        public int LayerSize;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Screen scissor state.
    /// </summary>
    struct ScreenScissorState
    {
#pragma warning disable CS0649
        public ushort X;
        public ushort Width;
        public ushort Y;
        public ushort Height;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Vertex buffer attribute state.
    /// </summary>
    struct VertexAttribState
    {
#pragma warning disable CS0649
        public uint Attribute;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks the index of the vertex buffer this attribute belongs to.
        /// </summary>
        /// <returns>Vertex buffer index</returns>
        public int UnpackBufferIndex()
        {
            return (int)(Attribute & 0x1f);
        }

        /// <summary>
        /// Unpacks the attribute constant flag.
        /// </summary>
        /// <returns>True if the attribute is constant, false otherwise</returns>
        public bool UnpackIsConstant()
        {
            return (Attribute & 0x40) != 0;
        }

        /// <summary>
        /// Unpacks the offset, in bytes, of the attribute on the vertex buffer.
        /// </summary>
        /// <returns>Attribute offset in bytes</returns>
        public int UnpackOffset()
        {
            return (int)((Attribute >> 7) & 0x3fff);
        }

        /// <summary>
        /// Unpacks the Maxwell attribute format integer.
        /// </summary>
        /// <returns>Attribute format integer</returns>
        public uint UnpackFormat()
        {
            return Attribute & 0x3fe00000;
        }
    }

    /// <summary>
    /// Render target draw buffers control.
    /// </summary>
    struct RtControl
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks the number of active draw buffers.
        /// </summary>
        /// <returns>Number of active draw buffers</returns>
        public int UnpackCount()
        {
            return (int)(Packed & 0xf);
        }

        /// <summary>
        /// Unpacks the color attachment index for a given draw buffer.
        /// </summary>
        /// <param name="index">Index of the draw buffer</param>
        /// <returns>Attachment index</returns>
        public int UnpackPermutationIndex(int index)
        {
            return (int)((Packed >> (4 + index * 3)) & 7);
        }
    }

    /// <summary>
    /// 3D, 2D or 1D texture size.
    /// </summary>
    struct Size3D
    {
#pragma warning disable CS0649
        public int Width;
        public int Height;
        public int Depth;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Stencil front test state and masks.
    /// </summary>
    struct StencilTestState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public StencilOp FrontSFail;
        public StencilOp FrontDpFail;
        public StencilOp FrontDpPass;
        public CompareOp FrontFunc;
        public int FrontFuncRef;
        public int FrontFuncMask;
        public int FrontMask;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Screen Y control register.
    /// </summary>
    [Flags]
    enum YControl
    {
        NegateY = 1 << 0,
        TriangleRastFlip = 1 << 4
    }

    /// <summary>
    /// Condition for conditional rendering.
    /// </summary>
    enum Condition
    {
        Never,
        Always,
        ResultNonZero,
        Equal,
        NotEqual
    }

    /// <summary>
    /// Texture or sampler pool state.
    /// </summary>
    struct PoolState
    {
#pragma warning disable CS0649
        public GpuVa Address;
        public int MaximumId;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Stencil back test state.
    /// </summary>
    struct StencilBackTestState
    {
#pragma warning disable CS0649
        public Boolean32 TwoSided;
        public StencilOp BackSFail;
        public StencilOp BackDpFail;
        public StencilOp BackDpPass;
        public CompareOp BackFunc;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Primitive restart state.
    /// </summary>
    struct PrimitiveRestartState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public int Index;
#pragma warning restore CS0649
    }

    /// <summary>
    /// GPU index buffer state.
    /// This is used on indexed draws.
    /// </summary>
    struct IndexBufferState
    {
#pragma warning disable CS0649
        public GpuVa Address;
        public GpuVa EndAddress;
        public IndexType Type;
        public int First;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Face culling and orientation parameters.
    /// </summary>
    struct FaceState
    {
#pragma warning disable CS0649
        public Boolean32 CullEnable;
        public FrontFace FrontFace;
        public Face CullFace;
#pragma warning restore CS0649
    }

    /// <summary>
    /// View volume clip control.
    /// </summary>
    [Flags]
    enum ViewVolumeClipControl
    {
        ForceDepthRangeZeroToOne = 1 << 0,
        DepthClampDisabled = 1 << 11
    }

    /// <summary>
    /// Logical operation state.
    /// </summary>
    struct LogicalOpState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public LogicalOp LogicalOp;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Render target color buffer mask.
    /// This defines which color channels are written to the color buffer.
    /// </summary>
    struct RtColorMask
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks red channel enable.
        /// </summary>
        /// <returns>True to write the new red channel color, false to keep the old value</returns>
        public bool UnpackRed()
        {
            return (Packed & 0x1) != 0;
        }

        /// <summary>
        /// Unpacks green channel enable.
        /// </summary>
        /// <returns>True to write the new green channel color, false to keep the old value</returns>
        public bool UnpackGreen()
        {
            return (Packed & 0x10) != 0;
        }

        /// <summary>
        /// Unpacks blue channel enable.
        /// </summary>
        /// <returns>True to write the new blue channel color, false to keep the old value</returns>
        public bool UnpackBlue()
        {
            return (Packed & 0x100) != 0;
        }

        /// <summary>
        /// Unpacks alpha channel enable.
        /// </summary>
        /// <returns>True to write the new alpha channel color, false to keep the old value</returns>
        public bool UnpackAlpha()
        {
            return (Packed & 0x1000) != 0;
        }
    }

    /// <summary>
    /// Vertex buffer state.
    /// </summary>
    struct VertexBufferState
    {
#pragma warning disable CS0649
        public uint Control;
        public GpuVa Address;
        public int Divisor;
#pragma warning restore CS0649

        /// <summary>
        /// Vertex buffer stride, defined as the number of bytes occupied by each vertex in memory.
        /// </summary>
        /// <returns>Vertex buffer stride</returns>
        public int UnpackStride()
        {
            return (int)(Control & 0xfff);
        }

        /// <summary>
        /// Vertex buffer enable.
        /// </summary>
        /// <returns>True if the vertex buffer is enabled, false otherwise</returns>
        public bool UnpackEnable()
        {
            return (Control & (1 << 12)) != 0;
        }
    }

    /// <summary>
    /// Color buffer blending parameters, shared by all color buffers.
    /// </summary>
    struct BlendStateCommon
    {
#pragma warning disable CS0649
        public Boolean32 SeparateAlpha;
        public BlendOp ColorOp;
        public BlendFactor ColorSrcFactor;
        public BlendFactor ColorDstFactor;
        public BlendOp AlphaOp;
        public BlendFactor AlphaSrcFactor;
        public uint Unknown0x1354;
        public BlendFactor AlphaDstFactor;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Color buffer blending parameters.
    /// </summary>
    struct BlendState
    {
#pragma warning disable CS0649
        public Boolean32 SeparateAlpha;
        public BlendOp ColorOp;
        public BlendFactor ColorSrcFactor;
        public BlendFactor ColorDstFactor;
        public BlendOp AlphaOp;
        public BlendFactor AlphaSrcFactor;
        public BlendFactor AlphaDstFactor;
        public uint Padding;
#pragma warning restore CS0649
    }

    /// <summary>
    /// Graphics shader stage state.
    /// </summary>
    struct ShaderState
    {
#pragma warning disable CS0649
        public uint Control;
        public uint Offset;
        public uint Unknown0x8;
        public int MaxRegisters;
        public ShaderType Type;
        public uint Unknown0x14;
        public uint Unknown0x18;
        public uint Unknown0x1c;
        public uint Unknown0x20;
        public uint Unknown0x24;
        public uint Unknown0x28;
        public uint Unknown0x2c;
        public uint Unknown0x30;
        public uint Unknown0x34;
        public uint Unknown0x38;
        public uint Unknown0x3c;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks shader enable information.
        /// Must be ignored for vertex shaders, those are always enabled.
        /// </summary>
        /// <returns>True if the stage is enabled, false otherwise</returns>
        public bool UnpackEnable()
        {
            return (Control & 1) != 0;
        }
    }

    /// <summary>
    /// Uniform buffer state for the uniform buffer currently being modified.
    /// </summary>
    struct UniformBufferState
    {
#pragma warning disable CS0649
        public int Size;
        public GpuVa Address;
        public int Offset;
#pragma warning restore CS0649
    }

    unsafe struct ThreedClassState : IShadowState
    {
#pragma warning disable CS0649
        public uint SetObject;
        public int SetObjectClassId => (int)((SetObject >> 0) & 0xFFFF);
        public int SetObjectEngineId => (int)((SetObject >> 16) & 0x1F);
        public fixed uint Reserved04[63];
        public uint NoOperation;
        public uint SetNotifyA;
        public int SetNotifyAAddressUpper => (int)((SetNotifyA >> 0) & 0xFF);
        public uint SetNotifyB;
        public uint Notify;
        public NotifyType NotifyType => (NotifyType)(Notify);
        public uint WaitForIdle;
        public uint LoadMmeInstructionRamPointer;
        public uint LoadMmeInstructionRam;
        public uint LoadMmeStartAddressRamPointer;
        public uint LoadMmeStartAddressRam;
        public uint SetMmeShadowRamControl;
        public SetMmeShadowRamControlMode SetMmeShadowRamControlMode => (SetMmeShadowRamControlMode)((SetMmeShadowRamControl >> 0) & 0x3);
        public fixed uint Reserved128[2];
        public uint SetGlobalRenderEnableA;
        public int SetGlobalRenderEnableAOffsetUpper => (int)((SetGlobalRenderEnableA >> 0) & 0xFF);
        public uint SetGlobalRenderEnableB;
        public uint SetGlobalRenderEnableC;
        public int SetGlobalRenderEnableCMode => (int)((SetGlobalRenderEnableC >> 0) & 0x7);
        public uint SendGoIdle;
        public uint PmTrigger;
        public uint PmTriggerWfi;
        public fixed uint Reserved148[2];
        public uint SetInstrumentationMethodHeader;
        public uint SetInstrumentationMethodData;
        public fixed uint Reserved158[10];
        public uint LineLengthIn;
        public uint LineCount;
        public uint OffsetOutUpper;
        public int OffsetOutUpperValue => (int)((OffsetOutUpper >> 0) & 0xFF);
        public uint OffsetOut;
        public uint PitchOut;
        public uint SetDstBlockSize;
        public SetDstBlockSizeWidth SetDstBlockSizeWidth => (SetDstBlockSizeWidth)((SetDstBlockSize >> 0) & 0xF);
        public SetDstBlockSizeHeight SetDstBlockSizeHeight => (SetDstBlockSizeHeight)((SetDstBlockSize >> 4) & 0xF);
        public SetDstBlockSizeDepth SetDstBlockSizeDepth => (SetDstBlockSizeDepth)((SetDstBlockSize >> 8) & 0xF);
        public uint SetDstWidth;
        public uint SetDstHeight;
        public uint SetDstDepth;
        public uint SetDstLayer;
        public uint SetDstOriginBytesX;
        public int SetDstOriginBytesXV => (int)((SetDstOriginBytesX >> 0) & 0xFFFFF);
        public uint SetDstOriginSamplesY;
        public int SetDstOriginSamplesYV => (int)((SetDstOriginSamplesY >> 0) & 0xFFFF);
        public uint LaunchDma;
        public LaunchDmaDstMemoryLayout LaunchDmaDstMemoryLayout => (LaunchDmaDstMemoryLayout)((LaunchDma >> 0) & 0x1);
        public LaunchDmaCompletionType LaunchDmaCompletionType => (LaunchDmaCompletionType)((LaunchDma >> 4) & 0x3);
        public LaunchDmaInterruptType LaunchDmaInterruptType => (LaunchDmaInterruptType)((LaunchDma >> 8) & 0x3);
        public LaunchDmaSemaphoreStructSize LaunchDmaSemaphoreStructSize => (LaunchDmaSemaphoreStructSize)((LaunchDma >> 12) & 0x1);
        public bool LaunchDmaReductionEnable => (LaunchDma & 0x2) != 0;
        public LaunchDmaReductionOp LaunchDmaReductionOp => (LaunchDmaReductionOp)((LaunchDma >> 13) & 0x7);
        public LaunchDmaReductionFormat LaunchDmaReductionFormat => (LaunchDmaReductionFormat)((LaunchDma >> 2) & 0x3);
        public bool LaunchDmaSysmembarDisable => (LaunchDma & 0x40) != 0;
        public uint LoadInlineData;
        public fixed uint Reserved1B8[22];
        public Boolean32 EarlyZForce;
        public fixed uint Reserved214[45];
        public uint SyncpointAction;
        public fixed uint Reserved2CC[21];
        public TessMode TessMode;
        public Array4<float> TessOuterLevel;
        public Array2<float> TessInnerLevel;
        public fixed uint Reserved33C[16];
        public Boolean32 RasterizeEnable;
        public Array4<TfBufferState> TfBufferState;
        public fixed uint Reserved400[192];
        public Array4<TfState> TfState;
        public fixed uint Reserved740[1];
        public Boolean32 TfEnable;
        public fixed uint Reserved748[46];
        public Array8<RtColorState> RtColorState;
        public Array16<ViewportTransform> ViewportTransform;
        public Array16<ViewportExtents> ViewportExtents;
        public fixed uint ReservedD00[29];
        public VertexBufferDrawState VertexBufferDrawState;
        public uint DepthMode;
        public ClearColors ClearColors;
        public float ClearDepthValue;
        public fixed uint ReservedD94[3];
        public uint ClearStencilValue;
        public fixed uint ReservedDA4[2];
        public PolygonMode PolygonModeFront;
        public PolygonMode PolygonModeBack;
        public Boolean32 PolygonSmoothEnable;
        public fixed uint ReservedDB8[2];
        public DepthBiasState DepthBiasState;
        public int PatchVertices;
        public fixed uint ReservedDD0[4];
        public uint TextureBarrier;
        public uint WatchdogTimer;
        public Boolean32 PrimitiveRestartDrawArrays;
        public fixed uint ReservedDEC[5];
        public Array16<ScissorState> ScissorState;
        public fixed uint ReservedF00[21];
        public StencilBackMasks StencilBackMasks;
        public fixed uint ReservedF60[5];
        public uint InvalidateTextures;
        public fixed uint ReservedF78[1];
        public uint TextureBarrierTiled;
        public fixed uint ReservedF80[4];
        public Boolean32 RtColorMaskShared;
        public fixed uint ReservedF94[19];
        public RtDepthStencilState RtDepthStencilState;
        public ScreenScissorState ScreenScissorState;
        public fixed uint ReservedFFC[33];
        public int DrawTextureDstX;
        public int DrawTextureDstY;
        public int DrawTextureDstWidth;
        public int DrawTextureDstHeight;
        public long DrawTextureDuDx;
        public long DrawTextureDvDy;
        public int DrawTextureSamplerId;
        public int DrawTextureTextureId;
        public int DrawTextureSrcX;
        public int DrawTextureSrcY;
        public fixed uint Reserved10B0[18];
        public uint ClearFlags;
        public fixed uint Reserved10FC[25];
        public Array16<VertexAttribState> VertexAttribState;
        public fixed uint Reserved11A0[31];
        public RtControl RtControl;
        public fixed uint Reserved1220[2];
        public Size3D RtDepthStencilSize;
        public SamplerIndex SamplerIndex;
        public fixed uint Reserved1238[37];
        public Boolean32 DepthTestEnable;
        public fixed uint Reserved12D0[5];
        public Boolean32 BlendIndependent;
        public Boolean32 DepthWriteEnable;
        public Boolean32 AlphaTestEnable;
        public fixed uint Reserved12F0[5];
        public uint VbElementU8;
        public uint Reserved1308;
        public CompareOp DepthTestFunc;
        public float AlphaTestRef;
        public CompareOp AlphaTestFunc;
        public uint Reserved1318;
        public ColorF BlendConstant;
        public fixed uint Reserved132C[4];
        public BlendStateCommon BlendStateCommon;
        public Boolean32 BlendEnableCommon;
        public Array8<Boolean32> BlendEnable;
        public StencilTestState StencilTestState;
        public fixed uint Reserved13A0[3];
        public YControl YControl;
        public float LineWidthSmooth;
        public float LineWidthAliased;
        public fixed uint Reserved13B8[27];
        public uint InvalidateSamplerCacheNoWfi;
        public uint InvalidateTextureHeaderCacheNoWfi;
        public fixed uint Reserved142C[2];
        public uint FirstVertex;
        public uint FirstInstance;
        public fixed uint Reserved143C[53];
        public uint ClipDistanceEnable;
        public uint Reserved1514;
        public float PointSize;
        public uint Reserved151C;
        public Boolean32 PointSpriteEnable;
        public fixed uint Reserved1524[3];
        public uint ResetCounter;
        public uint Reserved1534;
        public Boolean32 RtDepthStencilEnable;
        public fixed uint Reserved153C[5];
        public GpuVa RenderEnableAddress;
        public Condition RenderEnableCondition;
        public PoolState SamplerPoolState;
        public uint Reserved1568;
        public float DepthBiasFactor;
        public Boolean32 LineSmoothEnable;
        public PoolState TexturePoolState;
        public fixed uint Reserved1580[5];
        public StencilBackTestState StencilBackTestState;
        public fixed uint Reserved15A8[5];
        public float DepthBiasUnits;
        public fixed uint Reserved15C0[4];
        public TextureMsaaMode RtMsaaMode;
        public fixed uint Reserved15D4[5];
        public uint VbElementU32;
        public uint Reserved15EC;
        public uint VbElementU16;
        public fixed uint Reserved15F4[4];
        public uint PointCoordReplace;
        public GpuVa ShaderBaseAddress;
        public uint Reserved1610;
        public uint DrawEnd;
        public uint DrawBegin;
        public fixed uint Reserved161C[10];
        public PrimitiveRestartState PrimitiveRestartState;
        public fixed uint Reserved164C[95];
        public IndexBufferState IndexBufferState;
        public uint IndexBufferCount;
        public uint DrawIndexedSmall;
        public uint DrawIndexedSmall2;
        public uint Reserved17EC;
        public uint DrawIndexedSmallIncInstance;
        public uint DrawIndexedSmallIncInstance2;
        public fixed uint Reserved17F8[33];
        public float DepthBiasClamp;
        public Array16<Boolean32> VertexBufferInstanced;
        public fixed uint Reserved18C0[20];
        public Boolean32 VertexProgramPointSize;
        public uint Reserved1914;
        public FaceState FaceState;
        public fixed uint Reserved1924[2];
        public uint ViewportTransformEnable;
        public fixed uint Reserved1930[3];
        public ViewVolumeClipControl ViewVolumeClipControl;
        public fixed uint Reserved1940[2];
        public Boolean32 PrimitiveTypeOverrideEnable;
        public fixed uint Reserved194C[9];
        public PrimitiveTypeOverride PrimitiveTypeOverride;
        public fixed uint Reserved1974[20];
        public LogicalOpState LogicOpState;
        public uint Reserved19CC;
        public uint Clear;
        public fixed uint Reserved19D4[11];
        public Array8<RtColorMask> RtColorMask;
        public fixed uint Reserved1A20[56];
        public GpuVa SemaphoreAddress;
        public int SemaphorePayload;
        public uint SemaphoreControl;
        public fixed uint Reserved1B10[60];
        public Array16<VertexBufferState> VertexBufferState;
        public fixed uint Reserved1D00[64];
        public Array8<BlendState> BlendState;
        public Array16<GpuVa> VertexBufferEndAddress;
        public fixed uint Reserved1F80[32];
        public Array6<ShaderState> ShaderState;
        public fixed uint Reserved2180[96];
        public uint SetFalcon00;
        public uint SetFalcon01;
        public uint SetFalcon02;
        public uint SetFalcon03;
        public uint SetFalcon04;
        public uint SetFalcon05;
        public uint SetFalcon06;
        public uint SetFalcon07;
        public uint SetFalcon08;
        public uint SetFalcon09;
        public uint SetFalcon10;
        public uint SetFalcon11;
        public uint SetFalcon12;
        public uint SetFalcon13;
        public uint SetFalcon14;
        public uint SetFalcon15;
        public uint SetFalcon16;
        public uint SetFalcon17;
        public uint SetFalcon18;
        public uint SetFalcon19;
        public uint SetFalcon20;
        public uint SetFalcon21;
        public uint SetFalcon22;
        public uint SetFalcon23;
        public uint SetFalcon24;
        public uint SetFalcon25;
        public uint SetFalcon26;
        public uint SetFalcon27;
        public uint SetFalcon28;
        public uint SetFalcon29;
        public uint SetFalcon30;
        public uint SetFalcon31;
        public UniformBufferState UniformBufferState;
        public Array16<uint> UniformBufferUpdateData;
        public fixed uint Reserved23D0[16];
        public uint UniformBufferBindVertex;
        public fixed uint Reserved2414[7];
        public uint UniformBufferBindTessControl;
        public fixed uint Reserved2434[7];
        public uint UniformBufferBindTessEvaluation;
        public fixed uint Reserved2454[7];
        public uint UniformBufferBindGeometry;
        public fixed uint Reserved2474[7];
        public uint UniformBufferBindFragment;
        public fixed uint Reserved2494[93];
        public uint TextureBufferIndex;
        public fixed uint Reserved260C[125];
        public Array4<Array32<uint>> TfVaryingLocations;
        public fixed uint Reserved2A00[640];
        public MmeShadowScratch SetMmeShadowScratch;
#pragma warning restore CS0649
    }
}
