using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class GlslDecl
    {
        public const int LayerAttr       = 0x064;
        public const int PointSizeAttr   = 0x06c;
        public const int PointCoordAttrX = 0x2e0;
        public const int PointCoordAttrY = 0x2e4;
        public const int TessCoordAttrX  = 0x2f0;
        public const int TessCoordAttrY  = 0x2f4;
        public const int TessCoordAttrZ  = 0x2f8;
        public const int InstanceIdAttr  = 0x2f8;
        public const int VertexIdAttr    = 0x2fc;
        public const int FaceAttr        = 0x3fc;

        public const int GlPositionVec4Index = 7;

        public const int PositionOutAttrLocation = 15;

        private const int AttrStartIndex = 8;
        private const int TexStartIndex  = 8;

        public const string PositionOutAttrName = "position";

        private const string TextureName = "tex";
        private const string UniformName = "c";

        private const string AttrName    = "attr";
        private const string InAttrName  = "in_"  + AttrName;
        private const string OutAttrName = "out_" + AttrName;

        private const string GprName  = "gpr";
        private const string PredName = "pred";

        public const string FragmentOutputName = "FragColor";

        public const string ExtraUniformBlockName = "Extra";
        public const string FlipUniformName = "flip";
        public const string InstanceUniformName = "instance";

        public const string BasicBlockName  = "bb";
        public const string BasicBlockAName = BasicBlockName + "_a";
        public const string BasicBlockBName = BasicBlockName + "_b";

        public const int SsyStackSize = 16;
        public const string SsyStackName = "ssy_stack";
        public const string SsyCursorName = "ssy_cursor";

        private string[] StagePrefixes = new string[] { "vp", "tcp", "tep", "gp", "fp" };

        private string StagePrefix;

        private Dictionary<ShaderIrOp, ShaderDeclInfo> m_CbTextures;

        private Dictionary<int, ShaderDeclInfo> m_Textures;
        private Dictionary<int, ShaderDeclInfo> m_Uniforms;

        private Dictionary<int, ShaderDeclInfo> m_Attributes;
        private Dictionary<int, ShaderDeclInfo> m_InAttributes;
        private Dictionary<int, ShaderDeclInfo> m_OutAttributes;

        private Dictionary<int, ShaderDeclInfo> m_Gprs;
        private Dictionary<int, ShaderDeclInfo> m_GprsHalf;
        private Dictionary<int, ShaderDeclInfo> m_Preds;

        public IReadOnlyDictionary<ShaderIrOp, ShaderDeclInfo> CbTextures => m_CbTextures;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Textures => m_Textures;
        public IReadOnlyDictionary<int, ShaderDeclInfo> Uniforms => m_Uniforms;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Attributes    => m_Attributes;
        public IReadOnlyDictionary<int, ShaderDeclInfo> InAttributes  => m_InAttributes;
        public IReadOnlyDictionary<int, ShaderDeclInfo> OutAttributes => m_OutAttributes;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Gprs     => m_Gprs;
        public IReadOnlyDictionary<int, ShaderDeclInfo> GprsHalf => m_GprsHalf;
        public IReadOnlyDictionary<int, ShaderDeclInfo> Preds    => m_Preds;

        public GalShaderType ShaderType { get; private set; }

        private GlslDecl(GalShaderType ShaderType)
        {
            this.ShaderType = ShaderType;

            m_CbTextures = new Dictionary<ShaderIrOp, ShaderDeclInfo>();

            m_Textures = new Dictionary<int, ShaderDeclInfo>();
            m_Uniforms = new Dictionary<int, ShaderDeclInfo>();

            m_Attributes    = new Dictionary<int, ShaderDeclInfo>();
            m_InAttributes  = new Dictionary<int, ShaderDeclInfo>();
            m_OutAttributes = new Dictionary<int, ShaderDeclInfo>();

            m_Gprs     = new Dictionary<int, ShaderDeclInfo>();
            m_GprsHalf = new Dictionary<int, ShaderDeclInfo>();
            m_Preds    = new Dictionary<int, ShaderDeclInfo>();
        }

        public GlslDecl(ShaderIrBlock[] Blocks, GalShaderType ShaderType, ShaderHeader Header) : this(ShaderType)
        {
            StagePrefix = StagePrefixes[(int)ShaderType] + "_";

            if (ShaderType == GalShaderType.Fragment)
            {
                int Index = 0;

                for (int Attachment = 0; Attachment < 8; Attachment++)
                {
                    for (int Component = 0; Component < 4; Component++)
                    {
                        if (Header.OmapTargets[Attachment].ComponentEnabled(Component))
                        {
                            m_Gprs.TryAdd(Index, new ShaderDeclInfo(GetGprName(Index), Index));

                            Index++;
                        }
                    }
                }

                if (Header.OmapDepth)
                {
                    Index = Header.DepthRegister;

                    m_Gprs.TryAdd(Index, new ShaderDeclInfo(GetGprName(Index), Index));
                }
            }

            foreach (ShaderIrBlock Block in Blocks)
            {
                ShaderIrNode[] Nodes = Block.GetNodes();

                foreach (ShaderIrNode Node in Nodes)
                {
                    Traverse(Nodes, null, Node);
                }
            }
        }

        public static GlslDecl Merge(GlslDecl VpA, GlslDecl VpB)
        {
            GlslDecl Combined = new GlslDecl(GalShaderType.Vertex);

            Merge(Combined.m_Textures, VpA.m_Textures, VpB.m_Textures);
            Merge(Combined.m_Uniforms, VpA.m_Uniforms, VpB.m_Uniforms);

            Merge(Combined.m_Attributes,    VpA.m_Attributes,    VpB.m_Attributes);
            Merge(Combined.m_OutAttributes, VpA.m_OutAttributes, VpB.m_OutAttributes);

            Merge(Combined.m_Gprs,     VpA.m_Gprs,     VpB.m_Gprs);
            Merge(Combined.m_GprsHalf, VpA.m_GprsHalf, VpB.m_GprsHalf);
            Merge(Combined.m_Preds,    VpA.m_Preds,    VpB.m_Preds);

            //Merge input attributes.
            foreach (KeyValuePair<int, ShaderDeclInfo> KV in VpA.m_InAttributes)
            {
                Combined.m_InAttributes.TryAdd(KV.Key, KV.Value);
            }

            foreach (KeyValuePair<int, ShaderDeclInfo> KV in VpB.m_InAttributes)
            {
                //If Vertex Program A already writes to this attribute,
                //then we don't need to add it as an input attribute since
                //Vertex Program A will already have written to it anyway,
                //and there's no guarantee that there is an input attribute
                //for this slot.
                if (!VpA.m_OutAttributes.ContainsKey(KV.Key))
                {
                    Combined.m_InAttributes.TryAdd(KV.Key, KV.Value);
                }
            }

            return Combined;
        }

        public static string GetGprName(int Index)
        {
            return GprName + Index;
        }

        private static void Merge(
            Dictionary<int, ShaderDeclInfo> C,
            Dictionary<int, ShaderDeclInfo> A,
            Dictionary<int, ShaderDeclInfo> B)
        {
            foreach (KeyValuePair<int, ShaderDeclInfo> KV in A)
            {
                C.TryAdd(KV.Key, KV.Value);
            }

            foreach (KeyValuePair<int, ShaderDeclInfo> KV in B)
            {
                C.TryAdd(KV.Key, KV.Value);
            }
        }

        private void Traverse(ShaderIrNode[] Nodes, ShaderIrNode Parent, ShaderIrNode Node)
        {
            switch (Node)
            {
                case ShaderIrAsg Asg:
                {
                    Traverse(Nodes, Asg, Asg.Dst);
                    Traverse(Nodes, Asg, Asg.Src);

                    break;
                }

                case ShaderIrCond Cond:
                {
                    Traverse(Nodes, Cond, Cond.Pred);
                    Traverse(Nodes, Cond, Cond.Child);

                    break;
                }

                case ShaderIrOp Op:
                {
                    Traverse(Nodes, Op, Op.OperandA);
                    Traverse(Nodes, Op, Op.OperandB);
                    Traverse(Nodes, Op, Op.OperandC);

                    if (Op.Inst == ShaderIrInst.Texq ||
                        Op.Inst == ShaderIrInst.Texs ||
                        Op.Inst == ShaderIrInst.Txlf)
                    {
                        int Handle = ((ShaderIrOperImm)Op.OperandC).Value;

                        int Index = Handle - TexStartIndex;

                        string Name = StagePrefix + TextureName + Index;

                        m_Textures.TryAdd(Handle, new ShaderDeclInfo(Name, Handle));
                    }
                    else if (Op.Inst == ShaderIrInst.Texb)
                    {
                        ShaderIrNode HandleSrc = null;

                        int Index = Array.IndexOf(Nodes, Parent) - 1;

                        for (; Index >= 0; Index--)
                        {
                            ShaderIrNode Curr = Nodes[Index];

                            if (Curr is ShaderIrAsg Asg && Asg.Dst is ShaderIrOperGpr Gpr)
                            {
                                if (Gpr.Index == ((ShaderIrOperGpr)Op.OperandC).Index)
                                {
                                    HandleSrc = Asg.Src;

                                    break;
                                }
                            }
                        }

                        if (HandleSrc != null && HandleSrc is ShaderIrOperCbuf Cbuf)
                        {
                            string Name = StagePrefix + TextureName + "_cb" + Cbuf.Index + "_" + Cbuf.Pos;

                            m_CbTextures.Add(Op, new ShaderDeclInfo(Name, Cbuf.Pos, true, Cbuf.Index));
                        }
                        else
                        {
                            throw new NotImplementedException("Shader TEX.B instruction is not fully supported!");
                        }
                    }
                    break;
                }

                case ShaderIrOperCbuf Cbuf:
                {
                    if (!m_Uniforms.ContainsKey(Cbuf.Index))
                    {
                        string Name = StagePrefix + UniformName + Cbuf.Index;

                        ShaderDeclInfo DeclInfo = new ShaderDeclInfo(Name, Cbuf.Pos, true, Cbuf.Index);

                        m_Uniforms.Add(Cbuf.Index, DeclInfo);
                    }
                    break;
                }

                case ShaderIrOperAbuf Abuf:
                {
                    //This is a built-in variable.
                    if (Abuf.Offs == LayerAttr       ||
                        Abuf.Offs == PointSizeAttr   ||
                        Abuf.Offs == PointCoordAttrX ||
                        Abuf.Offs == PointCoordAttrY ||
                        Abuf.Offs == VertexIdAttr    ||
                        Abuf.Offs == InstanceIdAttr  ||
                        Abuf.Offs == FaceAttr)
                    {
                        break;
                    }

                    int Index =  Abuf.Offs >> 4;
                    int Elem  = (Abuf.Offs >> 2) & 3;

                    int GlslIndex = Index - AttrStartIndex;

                    if (GlslIndex < 0)
                    {
                        return;
                    }

                    ShaderDeclInfo DeclInfo;

                    if (Parent is ShaderIrAsg Asg && Asg.Dst == Node)
                    {
                        if (!m_OutAttributes.TryGetValue(Index, out DeclInfo))
                        {
                            DeclInfo = new ShaderDeclInfo(OutAttrName + GlslIndex, GlslIndex);

                            m_OutAttributes.Add(Index, DeclInfo);
                        }
                    }
                    else
                    {
                        if (!m_InAttributes.TryGetValue(Index, out DeclInfo))
                        {
                            DeclInfo = new ShaderDeclInfo(InAttrName + GlslIndex, GlslIndex);

                            m_InAttributes.Add(Index, DeclInfo);
                        }
                    }

                    DeclInfo.Enlarge(Elem + 1);

                    if (!m_Attributes.ContainsKey(Index))
                    {
                        DeclInfo = new ShaderDeclInfo(AttrName + GlslIndex, GlslIndex, false, 0, 4);

                        m_Attributes.Add(Index, DeclInfo);
                    }

                    Traverse(Nodes, Abuf, Abuf.Vertex);

                    break;
                }

                case ShaderIrOperGpr Gpr:
                {
                    if (!Gpr.IsConst)
                    {
                        string Name = GetGprName(Gpr.Index);

                        if (Gpr.RegisterSize == ShaderRegisterSize.Single)
                        {
                            m_Gprs.TryAdd(Gpr.Index, new ShaderDeclInfo(Name, Gpr.Index));
                        }
                        else if (Gpr.RegisterSize == ShaderRegisterSize.Half)
                        {
                            Name += "_h" + Gpr.HalfPart;

                            m_GprsHalf.TryAdd((Gpr.Index << 1) | Gpr.HalfPart, new ShaderDeclInfo(Name, Gpr.Index));
                        }
                        else /* if (Gpr.RegisterSize == ShaderRegisterSize.Double) */
                        {
                            throw new NotImplementedException("Double types are not supported.");
                        }
                    }
                    break;
                }

                case ShaderIrOperPred Pred:
                {
                    if (!Pred.IsConst && !HasName(m_Preds, Pred.Index))
                    {
                        string Name = PredName + Pred.Index;

                        m_Preds.TryAdd(Pred.Index, new ShaderDeclInfo(Name, Pred.Index));
                    }
                    break;
                }
            }
        }

        private bool HasName(Dictionary<int, ShaderDeclInfo> Decls, int Index)
        {
            //This is used to check if the dictionary already contains
            //a entry for a vector at a given index position.
            //Used to enable turning gprs into vectors.
            int VecIndex = Index & ~3;

            if (Decls.TryGetValue(VecIndex, out ShaderDeclInfo DeclInfo))
            {
                if (DeclInfo.Size > 1 && Index < VecIndex + DeclInfo.Size)
                {
                    return true;
                }
            }

            return Decls.ContainsKey(Index);
        }
    }
}
