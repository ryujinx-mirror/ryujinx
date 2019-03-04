using Ryujinx.Graphics.Texture;
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

        private string[] _stagePrefixes = new string[] { "vp", "tcp", "tep", "gp", "fp" };

        private string _stagePrefix;

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

        private GlslDecl(GalShaderType shaderType)
        {
            ShaderType = shaderType;

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

        public GlslDecl(ShaderIrBlock[] blocks, GalShaderType shaderType, ShaderHeader header) : this(shaderType)
        {
            _stagePrefix = _stagePrefixes[(int)shaderType] + "_";

            if (shaderType == GalShaderType.Fragment)
            {
                int index = 0;

                for (int attachment = 0; attachment < 8; attachment++)
                {
                    for (int component = 0; component < 4; component++)
                    {
                        if (header.OmapTargets[attachment].ComponentEnabled(component))
                        {
                            m_Gprs.TryAdd(index, new ShaderDeclInfo(GetGprName(index), index));

                            index++;
                        }
                    }
                }

                if (header.OmapDepth)
                {
                    index = header.DepthRegister;

                    m_Gprs.TryAdd(index, new ShaderDeclInfo(GetGprName(index), index));
                }
            }

            foreach (ShaderIrBlock block in blocks)
            {
                ShaderIrNode[] nodes = block.GetNodes();

                foreach (ShaderIrNode node in nodes)
                {
                    Traverse(nodes, null, node);
                }
            }
        }

        public static GlslDecl Merge(GlslDecl vpA, GlslDecl vpB)
        {
            GlslDecl combined = new GlslDecl(GalShaderType.Vertex);

            Merge(combined.m_Textures, vpA.m_Textures, vpB.m_Textures);
            Merge(combined.m_Uniforms, vpA.m_Uniforms, vpB.m_Uniforms);

            Merge(combined.m_Attributes,    vpA.m_Attributes,    vpB.m_Attributes);
            Merge(combined.m_OutAttributes, vpA.m_OutAttributes, vpB.m_OutAttributes);

            Merge(combined.m_Gprs,     vpA.m_Gprs,     vpB.m_Gprs);
            Merge(combined.m_GprsHalf, vpA.m_GprsHalf, vpB.m_GprsHalf);
            Merge(combined.m_Preds,    vpA.m_Preds,    vpB.m_Preds);

            //Merge input attributes.
            foreach (KeyValuePair<int, ShaderDeclInfo> kv in vpA.m_InAttributes)
            {
                combined.m_InAttributes.TryAdd(kv.Key, kv.Value);
            }

            foreach (KeyValuePair<int, ShaderDeclInfo> kv in vpB.m_InAttributes)
            {
                //If Vertex Program A already writes to this attribute,
                //then we don't need to add it as an input attribute since
                //Vertex Program A will already have written to it anyway,
                //and there's no guarantee that there is an input attribute
                //for this slot.
                if (!vpA.m_OutAttributes.ContainsKey(kv.Key))
                {
                    combined.m_InAttributes.TryAdd(kv.Key, kv.Value);
                }
            }

            return combined;
        }

        public static string GetGprName(int index)
        {
            return GprName + index;
        }

        private static void Merge(
            Dictionary<int, ShaderDeclInfo> c,
            Dictionary<int, ShaderDeclInfo> a,
            Dictionary<int, ShaderDeclInfo> b)
        {
            foreach (KeyValuePair<int, ShaderDeclInfo> kv in a)
            {
                c.TryAdd(kv.Key, kv.Value);
            }

            foreach (KeyValuePair<int, ShaderDeclInfo> kv in b)
            {
                c.TryAdd(kv.Key, kv.Value);
            }
        }

        private void Traverse(ShaderIrNode[] nodes, ShaderIrNode parent, ShaderIrNode node)
        {
            switch (node)
            {
                case ShaderIrAsg asg:
                {
                    Traverse(nodes, asg, asg.Dst);
                    Traverse(nodes, asg, asg.Src);

                    break;
                }

                case ShaderIrCond cond:
                {
                    Traverse(nodes, cond, cond.Pred);
                    Traverse(nodes, cond, cond.Child);

                    break;
                }

                case ShaderIrOp op:
                {
                    Traverse(nodes, op, op.OperandA);
                    Traverse(nodes, op, op.OperandB);
                    Traverse(nodes, op, op.OperandC);

                    if (op.Inst == ShaderIrInst.Texq ||
                        op.Inst == ShaderIrInst.Texs ||
                        op.Inst == ShaderIrInst.Tld4 ||
                        op.Inst == ShaderIrInst.Txlf)
                    {
                        int handle = ((ShaderIrOperImm)op.OperandC).Value;

                        int index = handle - TexStartIndex;

                        string name = _stagePrefix + TextureName + index;

                        GalTextureTarget textureTarget;
                        
                        TextureInstructionSuffix textureInstructionSuffix;

                        // TODO: Non 2D texture type for TEXQ?
                        if (op.Inst == ShaderIrInst.Texq)
                        {
                            textureTarget            = GalTextureTarget.TwoD;
                            textureInstructionSuffix = TextureInstructionSuffix.None;
                        }
                        else
                        {
                            ShaderIrMetaTex meta = ((ShaderIrMetaTex)op.MetaData);

                            textureTarget            = meta.TextureTarget;
                            textureInstructionSuffix = meta.TextureInstructionSuffix;
                        }

                        m_Textures.TryAdd(handle, new ShaderDeclInfo(name, handle, false, 0, 1, textureTarget, textureInstructionSuffix));
                    }
                    else if (op.Inst == ShaderIrInst.Texb)
                    {
                        ShaderIrNode handleSrc = null;

                        int index = Array.IndexOf(nodes, parent) - 1;

                        for (; index >= 0; index--)
                        {
                            ShaderIrNode curr = nodes[index];

                            if (curr is ShaderIrAsg asg && asg.Dst is ShaderIrOperGpr gpr)
                            {
                                if (gpr.Index == ((ShaderIrOperGpr)op.OperandC).Index)
                                {
                                    handleSrc = asg.Src;

                                    break;
                                }
                            }
                        }

                        if (handleSrc != null && handleSrc is ShaderIrOperCbuf cbuf)
                        {
                            ShaderIrMetaTex meta = ((ShaderIrMetaTex)op.MetaData);
                            string name = _stagePrefix + TextureName + "_cb" + cbuf.Index + "_" + cbuf.Pos;

                            m_CbTextures.Add(op, new ShaderDeclInfo(name, cbuf.Pos, true, cbuf.Index, 1, meta.TextureTarget, meta.TextureInstructionSuffix));
                        }
                        else
                        {
                            throw new NotImplementedException("Shader TEX.B instruction is not fully supported!");
                        }
                    }
                    break;
                }

                case ShaderIrOperCbuf cbuf:
                {
                    if (!m_Uniforms.ContainsKey(cbuf.Index))
                    {
                        string name = _stagePrefix + UniformName + cbuf.Index;

                        ShaderDeclInfo declInfo = new ShaderDeclInfo(name, cbuf.Pos, true, cbuf.Index);

                        m_Uniforms.Add(cbuf.Index, declInfo);
                    }
                    break;
                }

                case ShaderIrOperAbuf abuf:
                {
                    //This is a built-in variable.
                    if (abuf.Offs == LayerAttr       ||
                        abuf.Offs == PointSizeAttr   ||
                        abuf.Offs == PointCoordAttrX ||
                        abuf.Offs == PointCoordAttrY ||
                        abuf.Offs == VertexIdAttr    ||
                        abuf.Offs == InstanceIdAttr  ||
                        abuf.Offs == FaceAttr)
                    {
                        break;
                    }

                    int index =  abuf.Offs >> 4;
                    int elem  = (abuf.Offs >> 2) & 3;

                    int glslIndex = index - AttrStartIndex;

                    if (glslIndex < 0)
                    {
                        return;
                    }

                    ShaderDeclInfo declInfo;

                    if (parent is ShaderIrAsg asg && asg.Dst == node)
                    {
                        if (!m_OutAttributes.TryGetValue(index, out declInfo))
                        {
                            declInfo = new ShaderDeclInfo(OutAttrName + glslIndex, glslIndex);

                            m_OutAttributes.Add(index, declInfo);
                        }
                    }
                    else
                    {
                        if (!m_InAttributes.TryGetValue(index, out declInfo))
                        {
                            declInfo = new ShaderDeclInfo(InAttrName + glslIndex, glslIndex);

                            m_InAttributes.Add(index, declInfo);
                        }
                    }

                    declInfo.Enlarge(elem + 1);

                    if (!m_Attributes.ContainsKey(index))
                    {
                        declInfo = new ShaderDeclInfo(AttrName + glslIndex, glslIndex, false, 0, 4);

                        m_Attributes.Add(index, declInfo);
                    }

                    Traverse(nodes, abuf, abuf.Vertex);

                    break;
                }

                case ShaderIrOperGpr gpr:
                {
                    if (!gpr.IsConst)
                    {
                        string name = GetGprName(gpr.Index);

                        if (gpr.RegisterSize == ShaderRegisterSize.Single)
                        {
                            m_Gprs.TryAdd(gpr.Index, new ShaderDeclInfo(name, gpr.Index));
                        }
                        else if (gpr.RegisterSize == ShaderRegisterSize.Half)
                        {
                            name += "_h" + gpr.HalfPart;

                            m_GprsHalf.TryAdd((gpr.Index << 1) | gpr.HalfPart, new ShaderDeclInfo(name, gpr.Index));
                        }
                        else /* if (Gpr.RegisterSize == ShaderRegisterSize.Double) */
                        {
                            throw new NotImplementedException("Double types are not supported.");
                        }
                    }
                    break;
                }

                case ShaderIrOperPred pred:
                {
                    if (!pred.IsConst && !HasName(m_Preds, pred.Index))
                    {
                        string name = PredName + pred.Index;

                        m_Preds.TryAdd(pred.Index, new ShaderDeclInfo(name, pred.Index));
                    }
                    break;
                }
            }
        }

        private bool HasName(Dictionary<int, ShaderDeclInfo> decls, int index)
        {
            //This is used to check if the dictionary already contains
            //a entry for a vector at a given index position.
            //Used to enable turning gprs into vectors.
            int vecIndex = index & ~3;

            if (decls.TryGetValue(vecIndex, out ShaderDeclInfo declInfo))
            {
                if (declInfo.Size > 1 && index < vecIndex + declInfo.Size)
                {
                    return true;
                }
            }

            return decls.ContainsKey(index);
        }
    }
}
