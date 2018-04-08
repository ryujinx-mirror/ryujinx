using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class GlslDecl
    {
        public const int VertexIdAttr    = 0x2fc;
        public const int GlPositionWAttr = 0x7c;

        private const int AttrStartIndex = 8;
        private const int TexStartIndex = 8;

        private const string InAttrName  = "in_attr";
        private const string OutAttrName = "out_attr";
        private const string UniformName = "c";

        private const string GprName     = "gpr";
        private const string PredName    = "pred";
        private const string TextureName = "tex";

        public const string FragmentOutputName = "FragColor";

        private string[] StagePrefixes = new string[] { "vp", "tcp", "tep", "gp", "fp" };

        private string StagePrefix;

        private Dictionary<int, ShaderDeclInfo> m_Textures;

        private Dictionary<(int, int), ShaderDeclInfo> m_Uniforms;

        private Dictionary<int, ShaderDeclInfo> m_InAttributes;
        private Dictionary<int, ShaderDeclInfo> m_OutAttributes;

        private Dictionary<int, ShaderDeclInfo> m_Gprs;
        private Dictionary<int, ShaderDeclInfo> m_Preds;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Textures => m_Textures;

        public IReadOnlyDictionary<(int, int), ShaderDeclInfo> Uniforms => m_Uniforms;

        public IReadOnlyDictionary<int, ShaderDeclInfo> InAttributes  => m_InAttributes;
        public IReadOnlyDictionary<int, ShaderDeclInfo> OutAttributes => m_OutAttributes;

        public IReadOnlyDictionary<int, ShaderDeclInfo> Gprs  => m_Gprs;
        public IReadOnlyDictionary<int, ShaderDeclInfo> Preds => m_Preds;

        public GalShaderType ShaderType { get; private set; }

        public GlslDecl(ShaderIrNode[] Nodes, GalShaderType ShaderType)
        {
            this.ShaderType = ShaderType;

            StagePrefix = StagePrefixes[(int)ShaderType] + "_";

            m_Uniforms = new Dictionary<(int, int), ShaderDeclInfo>();

            m_Textures = new Dictionary<int, ShaderDeclInfo>();

            m_InAttributes  = new Dictionary<int, ShaderDeclInfo>();
            m_OutAttributes = new Dictionary<int, ShaderDeclInfo>();

            m_Gprs  = new Dictionary<int, ShaderDeclInfo>();
            m_Preds = new Dictionary<int, ShaderDeclInfo>();

            //FIXME: Only valid for vertex shaders.
            if (ShaderType == GalShaderType.Fragment)
            {
                m_Gprs.Add(0, new ShaderDeclInfo(FragmentOutputName, 0, 0, 4));
            }
            else
            {
                m_OutAttributes.Add(7, new ShaderDeclInfo("gl_Position", -1, 0, 4));
            }

            foreach (ShaderIrNode Node in Nodes)
            {
                Traverse(null, Node);
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

                    if (Op.Inst == ShaderIrInst.Texr ||
                        Op.Inst == ShaderIrInst.Texg ||
                        Op.Inst == ShaderIrInst.Texb ||
                        Op.Inst == ShaderIrInst.Texa)
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
                    string Name = StagePrefix + UniformName + Cbuf.Index + "_" + Cbuf.Offs;

                    ShaderDeclInfo DeclInfo = new ShaderDeclInfo(Name, Cbuf.Offs, Cbuf.Index);

                    m_Uniforms.TryAdd((Cbuf.Index, Cbuf.Offs), DeclInfo);

                    break;
                }

                case ShaderIrOperAbuf Abuf:
                {
                    //This is a built-in input variable.
                    if (Abuf.Offs == VertexIdAttr)
                    {
                        break;
                    }

                    int Index =  Abuf.Offs >> 4;
                    int Elem  = (Abuf.Offs >> 2) & 3;

                    int GlslIndex = Index - AttrStartIndex;

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
            int VecIndex = Index >> 2;

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