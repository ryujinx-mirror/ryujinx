using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class GlslDecl
    {
        public const int LayerAttr       = 0x064;
        public const int TessCoordAttrX  = 0x2f0;
        public const int TessCoordAttrY  = 0x2f4;
        public const int TessCoordAttrZ  = 0x2f8;
        public const int InstanceIdAttr  = 0x2f8;
        public const int VertexIdAttr    = 0x2fc;
        public const int FaceAttr        = 0x3fc;

        public const int MaxUboSize = 1024;

        public const int GlPositionVec4Index = 7;

        public const int PositionOutAttrLocation = 15;

        private const int AttrStartIndex = 8;
        private const int TexStartIndex = 8;

        public const string PositionOutAttrName = "position";

        private const string TextureName = "tex";
        private const string UniformName = "c";

        private const string AttrName    = "attr";
        private const string InAttrName  = "in_"  + AttrName;
        private const string OutAttrName = "out_" + AttrName;

        private const string GprName  = "gpr";
        private const string PredName = "pred";

        public const string FragmentOutputName = "FragColor";

        public const string FlipUniformName = "flip";

        public const string ProgramName  = "program";
        public const string ProgramAName = ProgramName + "_a";
        public const string ProgramBName = ProgramName + "_b";

        private string[] StagePrefixes = new string[] { "vp", "tcp", "tep", "gp", "fp" };

        private string StagePrefix;

        private Dictionary<int, ShaderDeclInfo> m_Textures;
        private Dictionary<int, ShaderDeclInfo> m_Uniforms;

        private Dictionary<int, ShaderDeclInfo> m_Attributes;
        private Dictionary<int, ShaderDeclInfo> m_InAttributes;
        private Dictionary<int, ShaderDeclInfo> m_OutAttributes;

        private Dictionary<int, ShaderDeclInfo> m_Gprs;
        private Dictionary<int, ShaderDeclInfo> m_Preds;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Textures => m_Textures;
        public IReadOnlyDictionary<int, ShaderDeclInfo> Uniforms => m_Uniforms;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Attributes    => m_Attributes;
        public IReadOnlyDictionary<int, ShaderDeclInfo> InAttributes  => m_InAttributes;
        public IReadOnlyDictionary<int, ShaderDeclInfo> OutAttributes => m_OutAttributes;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Gprs  => m_Gprs;
        public IReadOnlyDictionary<int, ShaderDeclInfo> Preds => m_Preds;

        public GalShaderType ShaderType { get; private set; }

        private GlslDecl(GalShaderType ShaderType)
        {
            this.ShaderType = ShaderType;

            m_Uniforms = new Dictionary<int, ShaderDeclInfo>();
            m_Textures = new Dictionary<int, ShaderDeclInfo>();

            m_Attributes    = new Dictionary<int, ShaderDeclInfo>();
            m_InAttributes  = new Dictionary<int, ShaderDeclInfo>();
            m_OutAttributes = new Dictionary<int, ShaderDeclInfo>();

            m_Gprs  = new Dictionary<int, ShaderDeclInfo>();
            m_Preds = new Dictionary<int, ShaderDeclInfo>();
        }

        public GlslDecl(ShaderIrBlock[] Blocks, GalShaderType ShaderType) : this(ShaderType)
        {
            StagePrefix = StagePrefixes[(int)ShaderType] + "_";

            if (ShaderType == GalShaderType.Fragment)
            {
                m_Gprs.Add(0, new ShaderDeclInfo(FragmentOutputName, 0, 0, 4));
            }

            foreach (ShaderIrBlock Block in Blocks)
            {
                foreach (ShaderIrNode Node in Block.GetNodes())
                {
                    Traverse(null, Node);
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

            Merge(Combined.m_Gprs,  VpA.m_Gprs,  VpB.m_Gprs);
            Merge(Combined.m_Preds, VpA.m_Preds, VpB.m_Preds);

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

        private void Traverse(ShaderIrNode Parent, ShaderIrNode Node)
        {
            switch (Node)
            {
                case ShaderIrAsg Asg:
                {
                    Traverse(Asg, Asg.Dst);
                    Traverse(Asg, Asg.Src);

                    break;
                }

                case ShaderIrCond Cond:
                {
                    Traverse(Cond, Cond.Pred);
                    Traverse(Cond, Cond.Child);

                    break;
                }

                case ShaderIrOp Op:
                {
                    Traverse(Op, Op.OperandA);
                    Traverse(Op, Op.OperandB);
                    Traverse(Op, Op.OperandC);

                    if (Op.Inst == ShaderIrInst.Texq ||
                        Op.Inst == ShaderIrInst.Texs ||
                        Op.Inst == ShaderIrInst.Txlf)
                    {
                        int Handle = ((ShaderIrOperImm)Op.OperandC).Value;

                        int Index = Handle - TexStartIndex;

                        string Name = StagePrefix + TextureName + Index;

                        m_Textures.TryAdd(Handle, new ShaderDeclInfo(Name, Handle));
                    }
                    break;
                }

                case ShaderIrOperCbuf Cbuf:
                {
                    if (!m_Uniforms.ContainsKey(Cbuf.Index))
                    {
                        string Name = StagePrefix + UniformName + Cbuf.Index;

                        ShaderDeclInfo DeclInfo = new ShaderDeclInfo(Name, Cbuf.Pos, Cbuf.Index);

                        m_Uniforms.Add(Cbuf.Index, DeclInfo);
                    }
                    break;
                }

                case ShaderIrOperAbuf Abuf:
                {
                    //This is a built-in input variable.
                    if (Abuf.Offs == VertexIdAttr ||
                        Abuf.Offs == InstanceIdAttr ||
                        Abuf.Offs == FaceAttr ||
                        Abuf.Offs == LayerAttr)
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
                        DeclInfo = new ShaderDeclInfo(AttrName + GlslIndex, GlslIndex, 0, 4);

                        m_Attributes.Add(Index, DeclInfo);
                    }

                    Traverse(Abuf, Abuf.Vertex);
                    break;
                }

                case ShaderIrOperGpr Gpr:
                {
                    if (!Gpr.IsConst && !HasName(m_Gprs, Gpr.Index))
                    {
                        string Name = GprName + Gpr.Index;

                        m_Gprs.TryAdd(Gpr.Index, new ShaderDeclInfo(Name, Gpr.Index));
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