using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ryujinx.Graphics.Gal.Shader
{
    public class GlslDecompiler
    {
        private delegate string GetInstExpr(ShaderIrOp op);

        private Dictionary<ShaderIrInst, GetInstExpr> _instsExpr;

        private enum OperType
        {
            Bool,
            F32,
            I32
        }

        private const string IdentationStr = "    ";

        private const int MaxVertexInput = 3;

        private GlslDecl _decl;

        private ShaderHeader _header, _headerB;

        private ShaderIrBlock[] _blocks, _blocksB;

        private StringBuilder _sb;

        public int MaxUboSize { get; }

        private bool _isNvidiaDriver;

        public GlslDecompiler(int maxUboSize, bool isNvidiaDriver)
        {
            _instsExpr = new Dictionary<ShaderIrInst, GetInstExpr>()
            {
                { ShaderIrInst.Abs,    GetAbsExpr    },
                { ShaderIrInst.Add,    GetAddExpr    },
                { ShaderIrInst.And,    GetAndExpr    },
                { ShaderIrInst.Asr,    GetAsrExpr    },
                { ShaderIrInst.Band,   GetBandExpr   },
                { ShaderIrInst.Bnot,   GetBnotExpr   },
                { ShaderIrInst.Bor,    GetBorExpr    },
                { ShaderIrInst.Bxor,   GetBxorExpr   },
                { ShaderIrInst.Ceil,   GetCeilExpr   },
                { ShaderIrInst.Ceq,    GetCeqExpr    },
                { ShaderIrInst.Cge,    GetCgeExpr    },
                { ShaderIrInst.Cgt,    GetCgtExpr    },
                { ShaderIrInst.Clamps, GetClampsExpr },
                { ShaderIrInst.Clampu, GetClampuExpr },
                { ShaderIrInst.Cle,    GetCleExpr    },
                { ShaderIrInst.Clt,    GetCltExpr    },
                { ShaderIrInst.Cne,    GetCneExpr    },
                { ShaderIrInst.Cut,    GetCutExpr    },
                { ShaderIrInst.Exit,   GetExitExpr   },
                { ShaderIrInst.Fabs,   GetAbsExpr    },
                { ShaderIrInst.Fadd,   GetAddExpr    },
                { ShaderIrInst.Fceq,   GetCeqExpr    },
                { ShaderIrInst.Fcequ,  GetCequExpr   },
                { ShaderIrInst.Fcge,   GetCgeExpr    },
                { ShaderIrInst.Fcgeu,  GetCgeuExpr   },
                { ShaderIrInst.Fcgt,   GetCgtExpr    },
                { ShaderIrInst.Fcgtu,  GetCgtuExpr   },
                { ShaderIrInst.Fclamp, GetFclampExpr },
                { ShaderIrInst.Fcle,   GetCleExpr    },
                { ShaderIrInst.Fcleu,  GetCleuExpr   },
                { ShaderIrInst.Fclt,   GetCltExpr    },
                { ShaderIrInst.Fcltu,  GetCltuExpr   },
                { ShaderIrInst.Fcnan,  GetCnanExpr   },
                { ShaderIrInst.Fcne,   GetCneExpr    },
                { ShaderIrInst.Fcneu,  GetCneuExpr   },
                { ShaderIrInst.Fcnum,  GetCnumExpr   },
                { ShaderIrInst.Fcos,   GetFcosExpr   },
                { ShaderIrInst.Fex2,   GetFex2Expr   },
                { ShaderIrInst.Ffma,   GetFfmaExpr   },
                { ShaderIrInst.Flg2,   GetFlg2Expr   },
                { ShaderIrInst.Floor,  GetFloorExpr  },
                { ShaderIrInst.Fmax,   GetMaxExpr    },
                { ShaderIrInst.Fmin,   GetMinExpr    },
                { ShaderIrInst.Fmul,   GetMulExpr    },
                { ShaderIrInst.Fneg,   GetNegExpr    },
                { ShaderIrInst.Frcp,   GetFrcpExpr   },
                { ShaderIrInst.Frsq,   GetFrsqExpr   },
                { ShaderIrInst.Fsin,   GetFsinExpr   },
                { ShaderIrInst.Fsqrt,  GetFsqrtExpr  },
                { ShaderIrInst.Ftos,   GetFtosExpr   },
                { ShaderIrInst.Ftou,   GetFtouExpr   },
                { ShaderIrInst.Ipa,    GetIpaExpr    },
                { ShaderIrInst.Kil,    GetKilExpr    },
                { ShaderIrInst.Lsl,    GetLslExpr    },
                { ShaderIrInst.Lsr,    GetLsrExpr    },
                { ShaderIrInst.Max,    GetMaxExpr    },
                { ShaderIrInst.Min,    GetMinExpr    },
                { ShaderIrInst.Mul,    GetMulExpr    },
                { ShaderIrInst.Neg,    GetNegExpr    },
                { ShaderIrInst.Not,    GetNotExpr    },
                { ShaderIrInst.Or,     GetOrExpr     },
                { ShaderIrInst.Stof,   GetStofExpr   },
                { ShaderIrInst.Sub,    GetSubExpr    },
                { ShaderIrInst.Texb,   GetTexbExpr   },
                { ShaderIrInst.Texq,   GetTexqExpr   },
                { ShaderIrInst.Texs,   GetTexsExpr   },
                { ShaderIrInst.Tld4,   GetTld4Expr   },
                { ShaderIrInst.Trunc,  GetTruncExpr  },
                { ShaderIrInst.Txlf,   GetTxlfExpr   },
                { ShaderIrInst.Utof,   GetUtofExpr   },
                { ShaderIrInst.Xor,    GetXorExpr    }
            };

            MaxUboSize = maxUboSize / 16;
            _isNvidiaDriver = isNvidiaDriver;
        }

        public GlslProgram Decompile(
            IGalMemory    memory,
            long          vpAPosition,
            long          vpBPosition,
            GalShaderType shaderType)
        {
            _header  = new ShaderHeader(memory, vpAPosition);
            _headerB = new ShaderHeader(memory, vpBPosition);

            _blocks  = ShaderDecoder.Decode(memory, vpAPosition);
            _blocksB = ShaderDecoder.Decode(memory, vpBPosition);

            GlslDecl declVpA = new GlslDecl(_blocks,  shaderType, _header);
            GlslDecl declVpB = new GlslDecl(_blocksB, shaderType, _headerB);

            _decl = GlslDecl.Merge(declVpA, declVpB);

            return Decompile();
        }

        public GlslProgram Decompile(IGalMemory memory, long position, GalShaderType shaderType)
        {
            _header  = new ShaderHeader(memory, position);
            _headerB = null;

            _blocks  = ShaderDecoder.Decode(memory, position);
            _blocksB = null;

            _decl = new GlslDecl(_blocks, shaderType, _header);

            return Decompile();
        }

        private GlslProgram Decompile()
        {
            _sb = new StringBuilder();

            _sb.AppendLine("#version 410 core");

            PrintDeclHeader();
            PrintDeclTextures();
            PrintDeclUniforms();
            PrintDeclAttributes();
            PrintDeclInAttributes();
            PrintDeclOutAttributes();
            PrintDeclGprs();
            PrintDeclPreds();
            PrintDeclSsy();

            if (_blocksB != null)
            {
                PrintBlockScope(_blocks, GlslDecl.BasicBlockAName);

                _sb.AppendLine();

                PrintBlockScope(_blocksB, GlslDecl.BasicBlockBName);
            }
            else
            {
                PrintBlockScope(_blocks, GlslDecl.BasicBlockName);
            }

            _sb.AppendLine();

            PrintMain();

            string glslCode = _sb.ToString();

            List<ShaderDeclInfo> textureInfo = new List<ShaderDeclInfo>();

            textureInfo.AddRange(_decl.Textures.Values);
            textureInfo.AddRange(IterateCbTextures());

            return new GlslProgram(glslCode, textureInfo, _decl.Uniforms.Values);
        }

        private void PrintDeclHeader()
        {
            if (_decl.ShaderType == GalShaderType.Geometry)
            {
                int maxVertices = _header.MaxOutputVertexCount;

                string outputTopology;

                switch (_header.OutputTopology)
                {
                    case ShaderHeader.PointList:     outputTopology = "points";         break;
                    case ShaderHeader.LineStrip:     outputTopology = "line_strip";     break;
                    case ShaderHeader.TriangleStrip: outputTopology = "triangle_strip"; break;

                    default: throw new InvalidOperationException();
                }

                _sb.AppendLine("#extension GL_ARB_enhanced_layouts : require");

                _sb.AppendLine();

                _sb.AppendLine("// Stubbed. Maxwell geometry shaders don't inform input geometry type");

                _sb.AppendLine("layout(triangles) in;" + Environment.NewLine);

                _sb.AppendLine($"layout({outputTopology}, max_vertices = {maxVertices}) out;");

                _sb.AppendLine();
            }
        }

        private string GetSamplerType(TextureTarget textureTarget, bool hasShadow)
        {
            string result;

            switch (textureTarget)
            {
                case TextureTarget.Texture1D:
                    result = "sampler1D";
                    break;
                case TextureTarget.Texture2D:
                    result = "sampler2D";
                    break;
                case TextureTarget.Texture3D:
                    result = "sampler3D";
                    break;
                case TextureTarget.TextureCubeMap:
                    result = "samplerCube";
                    break;
                case TextureTarget.TextureRectangle:
                    result = "sampler2DRect";
                    break;
                case TextureTarget.Texture1DArray:
                    result = "sampler1DArray";
                    break;
                case TextureTarget.Texture2DArray:
                    result = "sampler2DArray";
                    break;
                case TextureTarget.TextureCubeMapArray:
                    result = "samplerCubeArray";
                    break;
                case TextureTarget.TextureBuffer:
                    result = "samplerBuffer";
                    break;
                case TextureTarget.Texture2DMultisample:
                    result = "sampler2DMS";
                    break;
                case TextureTarget.Texture2DMultisampleArray:
                    result = "sampler2DMSArray";
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (hasShadow)
                result += "Shadow";

            return result;
        }

        private void PrintDeclTextures()
        {
            foreach (ShaderDeclInfo declInfo in IterateCbTextures())
            {
                TextureTarget target = ImageUtils.GetTextureTarget(declInfo.TextureTarget);
                _sb.AppendLine($"// {declInfo.TextureSuffix}");
                _sb.AppendLine("uniform " + GetSamplerType(target, (declInfo.TextureSuffix & TextureInstructionSuffix.Dc) != 0) + " " + declInfo.Name + ";");
            }

            foreach (ShaderDeclInfo declInfo in _decl.Textures.Values.OrderBy(DeclKeySelector))
            {
                TextureTarget target = ImageUtils.GetTextureTarget(declInfo.TextureTarget);
                _sb.AppendLine($"// {declInfo.TextureSuffix}");
                _sb.AppendLine("uniform " + GetSamplerType(target, (declInfo.TextureSuffix & TextureInstructionSuffix.Dc) != 0) + " " + declInfo.Name + ";");
            }
        }

        private IEnumerable<ShaderDeclInfo> IterateCbTextures()
        {
            HashSet<string> names = new HashSet<string>();

            foreach (ShaderDeclInfo declInfo in _decl.CbTextures.Values.OrderBy(DeclKeySelector))
            {
                if (names.Add(declInfo.Name))
                {
                    yield return declInfo;
                }
            }
        }

        private void PrintDeclUniforms()
        {
            if (_decl.ShaderType == GalShaderType.Vertex)
            {
                //Memory layout here is [flip_x, flip_y, instance, unused]
                //It's using 4 bytes, not 8

                _sb.AppendLine("layout (std140) uniform " + GlslDecl.ExtraUniformBlockName + " {");

                _sb.AppendLine(IdentationStr + "vec2 " + GlslDecl.FlipUniformName + ";");

                _sb.AppendLine(IdentationStr + "int " + GlslDecl.InstanceUniformName + ";");

                _sb.AppendLine("};");
                _sb.AppendLine();
            }

            foreach (ShaderDeclInfo declInfo in _decl.Uniforms.Values.OrderBy(DeclKeySelector))
            {
                _sb.AppendLine($"layout (std140) uniform {declInfo.Name} {{");

                _sb.AppendLine($"{IdentationStr}vec4 {declInfo.Name}_data[{MaxUboSize}];");

                _sb.AppendLine("};");
            }

            if (_decl.Uniforms.Count > 0)
            {
                _sb.AppendLine();
            }
        }

        private void PrintDeclAttributes()
        {
            string geometryArray = (_decl.ShaderType == GalShaderType.Geometry) ? "[" + MaxVertexInput + "]" : "";

            PrintDecls(_decl.Attributes, suffix: geometryArray);
        }

        private void PrintDeclInAttributes()
        {
            if (_decl.ShaderType == GalShaderType.Fragment)
            {
                _sb.AppendLine("layout (location = " + GlslDecl.PositionOutAttrLocation + ") in vec4 " + GlslDecl.PositionOutAttrName + ";");
            }

            if (_decl.ShaderType == GalShaderType.Geometry)
            {
                if (_decl.InAttributes.Count > 0)
                {
                    _sb.AppendLine("in Vertex {");

                    foreach (ShaderDeclInfo declInfo in _decl.InAttributes.Values.OrderBy(DeclKeySelector))
                    {
                        if (declInfo.Index >= 0)
                        {
                            _sb.AppendLine(IdentationStr + "layout (location = " + declInfo.Index + ") vec4 " + declInfo.Name + "; ");
                        }
                    }

                    _sb.AppendLine("} block_in[];" + Environment.NewLine);
                }
            }
            else
            {
                PrintDeclAttributes(_decl.InAttributes.Values, "in");
            }
        }

        private void PrintDeclOutAttributes()
        {
            if (_decl.ShaderType == GalShaderType.Fragment)
            {
                int count = 0;

                for (int attachment = 0; attachment < 8; attachment++)
                {
                    if (_header.OmapTargets[attachment].Enabled)
                    {
                        _sb.AppendLine("layout (location = " + attachment + ") out vec4 " + GlslDecl.FragmentOutputName + attachment + ";");

                        count++;
                    }
                }

                if (count > 0)
                {
                    _sb.AppendLine();
                }
            }
            else
            {
                _sb.AppendLine("layout (location = " + GlslDecl.PositionOutAttrLocation + ") out vec4 " + GlslDecl.PositionOutAttrName + ";");
                _sb.AppendLine();
            }

            PrintDeclAttributes(_decl.OutAttributes.Values, "out");
        }

        private void PrintDeclAttributes(IEnumerable<ShaderDeclInfo> decls, string inOut)
        {
            int count = 0;

            foreach (ShaderDeclInfo declInfo in decls.OrderBy(DeclKeySelector))
            {
                if (declInfo.Index >= 0)
                {
                    _sb.AppendLine("layout (location = " + declInfo.Index + ") " + inOut + " vec4 " + declInfo.Name + ";");

                    count++;
                }
            }

            if (count > 0)
            {
                _sb.AppendLine();
            }
        }

        private void PrintDeclGprs()
        {
            PrintDecls(_decl.Gprs);
            PrintDecls(_decl.GprsHalf);
        }

        private void PrintDeclPreds()
        {
            PrintDecls(_decl.Preds, "bool");
        }

        private void PrintDeclSsy()
        {
            _sb.AppendLine("uint " + GlslDecl.SsyCursorName + " = 0;");

            _sb.AppendLine("uint " + GlslDecl.SsyStackName + "[" + GlslDecl.SsyStackSize + "];" + Environment.NewLine);
        }

        private void PrintDecls(IReadOnlyDictionary<int, ShaderDeclInfo> dict, string customType = null, string suffix = "")
        {
            foreach (ShaderDeclInfo declInfo in dict.Values.OrderBy(DeclKeySelector))
            {
                string name;

                if (customType != null)
                {
                    name = customType + " " + declInfo.Name + suffix + ";";
                }
                else if (declInfo.Name.Contains(GlslDecl.FragmentOutputName))
                {
                    name = "layout (location = " + declInfo.Index / 4 + ") out vec4 " + declInfo.Name + suffix + ";";
                }
                else
                {
                    name = GetDecl(declInfo) + suffix + ";";
                }

                _sb.AppendLine(name);
            }

            if (dict.Count > 0)
            {
                _sb.AppendLine();
            }
        }

        private int DeclKeySelector(ShaderDeclInfo declInfo)
        {
            return declInfo.Cbuf << 24 | declInfo.Index;
        }

        private string GetDecl(ShaderDeclInfo declInfo)
        {
            if (declInfo.Size == 4)
            {
                return "vec4 " + declInfo.Name;
            }
            else
            {
                return "float " + declInfo.Name;
            }
        }

        private void PrintMain()
        {
            _sb.AppendLine("void main() {");

            foreach (KeyValuePair<int, ShaderDeclInfo> kv in _decl.InAttributes)
            {
                if (!_decl.Attributes.TryGetValue(kv.Key, out ShaderDeclInfo attr))
                {
                    continue;
                }

                ShaderDeclInfo declInfo = kv.Value;

                if (_decl.ShaderType == GalShaderType.Geometry)
                {
                    for (int vertex = 0; vertex < MaxVertexInput; vertex++)
                    {
                        string dst = attr.Name + "[" + vertex + "]";

                        string src = "block_in[" + vertex + "]." + declInfo.Name;

                        _sb.AppendLine(IdentationStr + dst + " = " + src + ";");
                    }
                }
                else
                {
                    _sb.AppendLine(IdentationStr + attr.Name + " = " + declInfo.Name + ";");
                }
            }

            _sb.AppendLine(IdentationStr + "uint pc;");

            if (_blocksB != null)
            {
                PrintProgram(_blocks,  GlslDecl.BasicBlockAName);
                PrintProgram(_blocksB, GlslDecl.BasicBlockBName);
            }
            else
            {
                PrintProgram(_blocks, GlslDecl.BasicBlockName);
            }

            if (_decl.ShaderType != GalShaderType.Geometry)
            {
                PrintAttrToOutput();
            }

            if (_decl.ShaderType == GalShaderType.Fragment)
            {
                if (_header.OmapDepth)
                {
                    _sb.AppendLine(IdentationStr + "gl_FragDepth = " + GlslDecl.GetGprName(_header.DepthRegister) + ";");
                }

                int gprIndex = 0;

                for (int attachment = 0; attachment < 8; attachment++)
                {
                    string output = GlslDecl.FragmentOutputName + attachment;

                    OmapTarget target = _header.OmapTargets[attachment];

                    for (int component = 0; component < 4; component++)
                    {
                        if (target.ComponentEnabled(component))
                        {
                            _sb.AppendLine(IdentationStr + output + "[" + component + "] = " + GlslDecl.GetGprName(gprIndex) + ";");

                            gprIndex++;
                        }
                    }
                }
            }

            _sb.AppendLine("}");
        }

        private void PrintProgram(ShaderIrBlock[] blocks, string name)
        {
            const string ident1 = IdentationStr;
            const string ident2 = ident1 + IdentationStr;
            const string ident3 = ident2 + IdentationStr;
            const string ident4 = ident3 + IdentationStr;

            _sb.AppendLine(ident1 + "pc = " + GetBlockPosition(blocks[0]) + ";");
            _sb.AppendLine(ident1 + "do {");
            _sb.AppendLine(ident2 + "switch (pc) {");

            foreach (ShaderIrBlock block in blocks)
            {
                string functionName = block.Position.ToString("x8");

                _sb.AppendLine(ident3 + "case 0x" + functionName + ": pc = " + name + "_" + functionName + "(); break;");
            }

            _sb.AppendLine(ident3 + "default:");
            _sb.AppendLine(ident4 + "pc = 0;");
            _sb.AppendLine(ident4 + "break;");

            _sb.AppendLine(ident2 + "}");
            _sb.AppendLine(ident1 + "} while (pc != 0);");
        }

        private void PrintAttrToOutput(string identation = IdentationStr)
        {
            foreach (KeyValuePair<int, ShaderDeclInfo> kv in _decl.OutAttributes)
            {
                if (!_decl.Attributes.TryGetValue(kv.Key, out ShaderDeclInfo attr))
                {
                    continue;
                }

                ShaderDeclInfo declInfo = kv.Value;

                string name = attr.Name;

                if (_decl.ShaderType == GalShaderType.Geometry)
                {
                    name += "[0]";
                }

                _sb.AppendLine(identation + declInfo.Name + " = " + name + ";");
            }

            if (_decl.ShaderType == GalShaderType.Vertex)
            {
                _sb.AppendLine(identation + "gl_Position.xy *= " + GlslDecl.FlipUniformName + ";");
            }

            if (_decl.ShaderType != GalShaderType.Fragment)
            {
                _sb.AppendLine(identation + GlslDecl.PositionOutAttrName + " = gl_Position;");
                _sb.AppendLine(identation + GlslDecl.PositionOutAttrName + ".w = 1;");
            }
        }

        private void PrintBlockScope(ShaderIrBlock[] blocks, string name)
        {
            foreach (ShaderIrBlock block in blocks)
            {
                _sb.AppendLine("uint " + name + "_" + block.Position.ToString("x8") + "() {");

                PrintNodes(block, block.GetNodes());

                _sb.AppendLine("}" + Environment.NewLine);
            }
        }

        private void PrintNodes(ShaderIrBlock block, ShaderIrNode[] nodes)
        {
            foreach (ShaderIrNode node in nodes)
            {
                PrintNode(block, node, IdentationStr);
            }

            if (nodes.Length == 0)
            {
                _sb.AppendLine(IdentationStr + "return 0u;");

                return;
            }

            ShaderIrNode last = nodes[nodes.Length - 1];

            bool unconditionalFlowChange = false;

            if (last is ShaderIrOp op)
            {
                switch (op.Inst)
                {
                    case ShaderIrInst.Bra:
                    case ShaderIrInst.Exit:
                    case ShaderIrInst.Sync:
                        unconditionalFlowChange = true;
                        break;
                }
            }

            if (!unconditionalFlowChange)
            {
                if (block.Next != null)
                {
                    _sb.AppendLine(IdentationStr + "return " + GetBlockPosition(block.Next) + ";");
                }
                else
                {
                    _sb.AppendLine(IdentationStr + "return 0u;");
                }
            }
        }

        private void PrintNode(ShaderIrBlock block, ShaderIrNode node, string identation)
        {
            if (node is ShaderIrCond cond)
            {
                string ifExpr = GetSrcExpr(cond.Pred, true);

                if (cond.Not)
                {
                    ifExpr = "!(" + ifExpr + ")";
                }

                _sb.AppendLine(identation + "if (" + ifExpr + ") {");

                PrintNode(block, cond.Child, identation + IdentationStr);

                _sb.AppendLine(identation + "}");
            }
            else if (node is ShaderIrAsg asg)
            {
                if (IsValidOutOper(asg.Dst))
                {
                    string expr = GetSrcExpr(asg.Src, true);

                    expr = GetExprWithCast(asg.Dst, asg.Src, expr);

                    _sb.AppendLine(identation + GetDstOperName(asg.Dst) + " = " + expr + ";");
                }
            }
            else if (node is ShaderIrOp op)
            {
                switch (op.Inst)
                {
                    case ShaderIrInst.Bra:
                    {
                        _sb.AppendLine(identation + "return " + GetBlockPosition(block.Branch) + ";");

                        break;
                    }

                    case ShaderIrInst.Emit:
                    {
                        PrintAttrToOutput(identation);

                        _sb.AppendLine(identation + "EmitVertex();");

                        break;
                    }

                    case ShaderIrInst.Ssy:
                    {
                        string stackIndex = GlslDecl.SsyStackName + "[" + GlslDecl.SsyCursorName + "]";

                        int targetPosition = (op.OperandA as ShaderIrOperImm).Value;

                        string target = "0x" + targetPosition.ToString("x8") + "u";

                        _sb.AppendLine(identation + stackIndex + " = " + target + ";");

                        _sb.AppendLine(identation + GlslDecl.SsyCursorName + "++;");

                        break;
                    }

                    case ShaderIrInst.Sync:
                    {
                        _sb.AppendLine(identation + GlslDecl.SsyCursorName + "--;");

                        string target = GlslDecl.SsyStackName + "[" + GlslDecl.SsyCursorName + "]";

                        _sb.AppendLine(identation + "return " + target + ";");

                        break;
                    }

                    default:
                        _sb.AppendLine(identation + GetSrcExpr(op, true) + ";");

                        break;
                }
            }
            else if (node is ShaderIrCmnt cmnt)
            {
                _sb.AppendLine(identation + "// " + cmnt.Comment);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private bool IsValidOutOper(ShaderIrNode node)
        {
            if (node is ShaderIrOperGpr gpr && gpr.IsConst)
            {
                return false;
            }
            else if (node is ShaderIrOperPred pred && pred.IsConst)
            {
                return false;
            }

            return true;
        }

        private string GetDstOperName(ShaderIrNode node)
        {
            if (node is ShaderIrOperAbuf abuf)
            {
                return GetOutAbufName(abuf);
            }
            else if (node is ShaderIrOperGpr gpr)
            {
                return GetName(gpr);
            }
            else if (node is ShaderIrOperPred pred)
            {
                return GetName(pred);
            }

            throw new ArgumentException(nameof(node));
        }

        private string GetSrcExpr(ShaderIrNode node, bool entry = false)
        {
            switch (node)
            {
                case ShaderIrOperAbuf abuf: return GetName (abuf);
                case ShaderIrOperCbuf cbuf: return GetName (cbuf);
                case ShaderIrOperGpr  gpr:  return GetName (gpr);
                case ShaderIrOperImm  imm:  return GetValue(imm);
                case ShaderIrOperImmf immf: return GetValue(immf);
                case ShaderIrOperPred pred: return GetName (pred);

                case ShaderIrOp op:
                    string expr;

                    if (_instsExpr.TryGetValue(op.Inst, out GetInstExpr getExpr))
                    {
                        expr = getExpr(op);
                    }
                    else
                    {
                        throw new NotImplementedException(op.Inst.ToString());
                    }

                    if (!entry && NeedsParentheses(op))
                    {
                        expr = "(" + expr + ")";
                    }

                    return expr;

                default: throw new ArgumentException(nameof(node));
            }
        }

        private static bool NeedsParentheses(ShaderIrOp op)
        {
            switch (op.Inst)
            {
                case ShaderIrInst.Ipa:
                case ShaderIrInst.Texq:
                case ShaderIrInst.Texs:
                case ShaderIrInst.Tld4:
                case ShaderIrInst.Txlf:
                    return false;
            }

            return true;
        }

        private string GetName(ShaderIrOperCbuf cbuf)
        {
            if (!_decl.Uniforms.TryGetValue(cbuf.Index, out ShaderDeclInfo declInfo))
            {
                throw new InvalidOperationException();
            }

            if (cbuf.Offs != null)
            {
                string offset = "floatBitsToInt(" + GetSrcExpr(cbuf.Offs) + ")";

                string index = "(" + cbuf.Pos * 4 + " + " + offset + ")";

                return $"{declInfo.Name}_data[{index} / 16][({index} / 4) % 4]";
            }
            else
            {
                return $"{declInfo.Name}_data[{cbuf.Pos / 4}][{cbuf.Pos % 4}]";
            }
        }

        private string GetOutAbufName(ShaderIrOperAbuf abuf)
        {
            if (_decl.ShaderType == GalShaderType.Geometry)
            {
                switch (abuf.Offs)
                {
                    case GlslDecl.LayerAttr: return "gl_Layer";
                }
            }

            return GetAttrTempName(abuf);
        }

        private string GetName(ShaderIrOperAbuf abuf)
        {
            //Handle special scalar read-only attributes here.
            if (_decl.ShaderType == GalShaderType.Vertex)
            {
                switch (abuf.Offs)
                {
                    case GlslDecl.VertexIdAttr:   return "gl_VertexID";
                    case GlslDecl.InstanceIdAttr: return GlslDecl.InstanceUniformName;
                }
            }
            else if (_decl.ShaderType == GalShaderType.TessEvaluation)
            {
                switch (abuf.Offs)
                {
                    case GlslDecl.TessCoordAttrX: return "gl_TessCoord.x";
                    case GlslDecl.TessCoordAttrY: return "gl_TessCoord.y";
                    case GlslDecl.TessCoordAttrZ: return "gl_TessCoord.z";
                }
            }
            else if (_decl.ShaderType == GalShaderType.Fragment)
            {
                switch (abuf.Offs)
                {
                    case GlslDecl.PointCoordAttrX: return "gl_PointCoord.x";
                    case GlslDecl.PointCoordAttrY: return "gl_PointCoord.y";
                    case GlslDecl.FaceAttr:        return "(gl_FrontFacing ? -1 : 0)";
                }
            }

            return GetAttrTempName(abuf);
        }

        private string GetAttrTempName(ShaderIrOperAbuf abuf)
        {
            int index =  abuf.Offs >> 4;
            int elem  = (abuf.Offs >> 2) & 3;

            string swizzle = "." + GetAttrSwizzle(elem);

            if (!_decl.Attributes.TryGetValue(index, out ShaderDeclInfo declInfo))
            {
                //Handle special vec4 attributes here
                //(for example, index 7 is always gl_Position).
                if (index == GlslDecl.GlPositionVec4Index)
                {
                    string name =
                        _decl.ShaderType != GalShaderType.Vertex &&
                        _decl.ShaderType != GalShaderType.Geometry ? GlslDecl.PositionOutAttrName : "gl_Position";

                    return name + swizzle;
                }
                else if (abuf.Offs == GlslDecl.PointSizeAttr)
                {
                    return "gl_PointSize";
                }
            }

            if (declInfo.Index >= 32)
            {
                throw new InvalidOperationException($"Shader attribute offset {abuf.Offs} is invalid.");
            }

            if (_decl.ShaderType == GalShaderType.Geometry)
            {
                string vertex = "floatBitsToInt(" + GetSrcExpr(abuf.Vertex) + ")";

                return declInfo.Name + "[" + vertex + "]" + swizzle;
            }
            else
            {
                return declInfo.Name + swizzle;
            }
        }

        private string GetName(ShaderIrOperGpr gpr)
        {
            if (gpr.IsConst)
            {
                return "0";
            }

            if (gpr.RegisterSize == ShaderRegisterSize.Single)
            {
                return GetNameWithSwizzle(_decl.Gprs, gpr.Index);
            }
            else if (gpr.RegisterSize == ShaderRegisterSize.Half)
            {
                return GetNameWithSwizzle(_decl.GprsHalf, (gpr.Index << 1) | gpr.HalfPart);
            }
            else /* if (Gpr.RegisterSize == ShaderRegisterSize.Double) */
            {
                throw new NotImplementedException("Double types are not supported.");
            }
        }

        private string GetValue(ShaderIrOperImm imm)
        {
            //Only use hex is the value is too big and would likely be hard to read as int.
            if (imm.Value >  0xfff ||
                imm.Value < -0xfff)
            {
                return "0x" + imm.Value.ToString("x8", CultureInfo.InvariantCulture);
            }
            else
            {
                return GetIntConst(imm.Value);
            }
        }

        private string GetValue(ShaderIrOperImmf immf)
        {
            return GetFloatConst(immf.Value);
        }

        private string GetName(ShaderIrOperPred pred)
        {
            return pred.IsConst ? "true" : GetNameWithSwizzle(_decl.Preds, pred.Index);
        }

        private string GetNameWithSwizzle(IReadOnlyDictionary<int, ShaderDeclInfo> dict, int index)
        {
            int vecIndex = index & ~3;

            if (dict.TryGetValue(vecIndex, out ShaderDeclInfo declInfo))
            {
                if (declInfo.Size > 1 && index < vecIndex + declInfo.Size)
                {
                    return declInfo.Name + "." + GetAttrSwizzle(index & 3);
                }
            }

            if (!dict.TryGetValue(index, out declInfo))
            {
                throw new InvalidOperationException();
            }

            return declInfo.Name;
        }

        private string GetAttrSwizzle(int elem)
        {
            return "xyzw".Substring(elem, 1);
        }

        private string GetAbsExpr(ShaderIrOp op) => GetUnaryCall(op, "abs");

        private string GetAddExpr(ShaderIrOp op) => GetBinaryExpr(op, "+");

        private string GetAndExpr(ShaderIrOp op) => GetBinaryExpr(op, "&");

        private string GetAsrExpr(ShaderIrOp op) => GetBinaryExpr(op, ">>");

        private string GetBandExpr(ShaderIrOp op) => GetBinaryExpr(op, "&&");

        private string GetBnotExpr(ShaderIrOp op) => GetUnaryExpr(op, "!");

        private string GetBorExpr(ShaderIrOp op) => GetBinaryExpr(op, "||");

        private string GetBxorExpr(ShaderIrOp op) => GetBinaryExpr(op, "^^");

        private string GetCeilExpr(ShaderIrOp op) => GetUnaryCall(op, "ceil");

        private string GetClampsExpr(ShaderIrOp op)
        {
            return "clamp(" + GetOperExpr(op, op.OperandA) + ", " +
                              GetOperExpr(op, op.OperandB) + ", " +
                              GetOperExpr(op, op.OperandC) + ")";
        }

        private string GetClampuExpr(ShaderIrOp op)
        {
            return "int(clamp(uint(" + GetOperExpr(op, op.OperandA) + "), " +
                             "uint(" + GetOperExpr(op, op.OperandB) + "), " +
                             "uint(" + GetOperExpr(op, op.OperandC) + ")))";
        }

        private string GetCeqExpr(ShaderIrOp op) => GetBinaryExpr(op, "==");

        private string GetCequExpr(ShaderIrOp op) => GetBinaryExprWithNaN(op, "==");

        private string GetCgeExpr(ShaderIrOp op) => GetBinaryExpr(op, ">=");

        private string GetCgeuExpr(ShaderIrOp op) => GetBinaryExprWithNaN(op, ">=");

        private string GetCgtExpr(ShaderIrOp op) => GetBinaryExpr(op, ">");

        private string GetCgtuExpr(ShaderIrOp op) => GetBinaryExprWithNaN(op, ">");

        private string GetCleExpr(ShaderIrOp op) => GetBinaryExpr(op, "<=");

        private string GetCleuExpr(ShaderIrOp op) => GetBinaryExprWithNaN(op, "<=");

        private string GetCltExpr(ShaderIrOp op) => GetBinaryExpr(op, "<");

        private string GetCltuExpr(ShaderIrOp op) => GetBinaryExprWithNaN(op, "<");

        private string GetCnanExpr(ShaderIrOp op) => GetUnaryCall(op, "isnan");

        private string GetCneExpr(ShaderIrOp op) => GetBinaryExpr(op, "!=");

        private string GetCutExpr(ShaderIrOp op) => "EndPrimitive()";

        private string GetCneuExpr(ShaderIrOp op) => GetBinaryExprWithNaN(op, "!=");

        private string GetCnumExpr(ShaderIrOp op) => GetUnaryCall(op, "!isnan");

        private string GetExitExpr(ShaderIrOp op) => "return 0u";

        private string GetFcosExpr(ShaderIrOp op) => GetUnaryCall(op, "cos");

        private string GetFex2Expr(ShaderIrOp op) => GetUnaryCall(op, "exp2");

        private string GetFfmaExpr(ShaderIrOp op) => GetTernaryExpr(op, "*", "+");

        private string GetFclampExpr(ShaderIrOp op) => GetTernaryCall(op, "clamp");

        private string GetFlg2Expr(ShaderIrOp op) => GetUnaryCall(op, "log2");

        private string GetFloorExpr(ShaderIrOp op) => GetUnaryCall(op, "floor");

        private string GetFrcpExpr(ShaderIrOp op) => GetUnaryExpr(op, "1 / ");

        private string GetFrsqExpr(ShaderIrOp op) => GetUnaryCall(op, "inversesqrt");

        private string GetFsinExpr(ShaderIrOp op) => GetUnaryCall(op, "sin");

        private string GetFsqrtExpr(ShaderIrOp op) => GetUnaryCall(op, "sqrt");

        private string GetFtosExpr(ShaderIrOp op)
        {
            return "int(" + GetOperExpr(op, op.OperandA) + ")";
        }

        private string GetFtouExpr(ShaderIrOp op)
        {
            return "int(uint(" + GetOperExpr(op, op.OperandA) + "))";
        }

        private string GetIpaExpr(ShaderIrOp op)
        {
            ShaderIrMetaIpa meta = (ShaderIrMetaIpa)op.MetaData;

            ShaderIrOperAbuf abuf = (ShaderIrOperAbuf)op.OperandA;

            if (meta.Mode == ShaderIpaMode.Pass)
            {
                int index = abuf.Offs >> 4;
                int elem = (abuf.Offs >> 2) & 3;

                if (_decl.ShaderType == GalShaderType.Fragment && index == GlslDecl.GlPositionVec4Index)
                {
                    switch (elem)
                    {
                        case 0: return "gl_FragCoord.x";
                        case 1: return "gl_FragCoord.y";
                        case 2: return "gl_FragCoord.z";
                        case 3: return "1";
                    }
                }
            }

            return GetSrcExpr(op.OperandA);
        }

        private string GetKilExpr(ShaderIrOp op) => "discard";

        private string GetLslExpr(ShaderIrOp op) => GetBinaryExpr(op, "<<");
        private string GetLsrExpr(ShaderIrOp op)
        {
            return "int(uint(" + GetOperExpr(op, op.OperandA) + ") >> " +
                                 GetOperExpr(op, op.OperandB) + ")";
        }

        private string GetMaxExpr(ShaderIrOp op) => GetBinaryCall(op, "max");
        private string GetMinExpr(ShaderIrOp op) => GetBinaryCall(op, "min");

        private string GetMulExpr(ShaderIrOp op) => GetBinaryExpr(op, "*");

        private string GetNegExpr(ShaderIrOp op) => GetUnaryExpr(op, "-");

        private string GetNotExpr(ShaderIrOp op) => GetUnaryExpr(op, "~");

        private string GetOrExpr(ShaderIrOp op) => GetBinaryExpr(op, "|");

        private string GetStofExpr(ShaderIrOp op)
        {
            return "float(" + GetOperExpr(op, op.OperandA) + ")";
        }

        private string GetSubExpr(ShaderIrOp op) => GetBinaryExpr(op, "-");

        private string GetTexbExpr(ShaderIrOp op)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            if (!_decl.CbTextures.TryGetValue(op, out ShaderDeclInfo declInfo))
            {
                throw new InvalidOperationException();
            }

            string coords = GetTexSamplerCoords(op);

            string ch = "rgba".Substring(meta.Elem, 1);

            return GetTextureOperation(op, declInfo.Name, coords, ch);
        }

        private string GetTexqExpr(ShaderIrOp op)
        {
            ShaderIrMetaTexq meta = (ShaderIrMetaTexq)op.MetaData;

            string ch = "xyzw".Substring(meta.Elem, 1);

            if (meta.Info == ShaderTexqInfo.Dimension)
            {
                string sampler = GetTexSamplerName(op);

                string lod = GetOperExpr(op, op.OperandA); //???

                return "textureSize(" + sampler + ", " + lod + ")." + ch;
            }
            else
            {
                throw new NotImplementedException(meta.Info.ToString());
            }
        }

        private string GetTexsExpr(ShaderIrOp op)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            string sampler = GetTexSamplerName(op);

            string coords = GetTexSamplerCoords(op);

            string ch = "rgba".Substring(meta.Elem, 1);

            return GetTextureOperation(op, sampler, coords, ch);
        }

        private string GetTld4Expr(ShaderIrOp op)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            string sampler = GetTexSamplerName(op);

            string coords = GetTexSamplerCoords(op);

            string ch = "rgba".Substring(meta.Elem, 1);

            return GetTextureGatherOperation(op, sampler, coords, ch);
        }

        // TODO: support AOFFI on non nvidia drivers
        private string GetTxlfExpr(ShaderIrOp op)
        {
            // TODO: Support all suffixes
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            TextureInstructionSuffix suffix = meta.TextureInstructionSuffix;

            string sampler = GetTexSamplerName(op);

            string coords = GetITexSamplerCoords(op);

            string ch = "rgba".Substring(meta.Elem, 1);

            string lod = "0";

            if (meta.LevelOfDetail != null)
            {
                lod = GetOperExpr(op, meta.LevelOfDetail);
            }

            if ((suffix & TextureInstructionSuffix.AOffI) != 0 && _isNvidiaDriver)
            {
                string offset = GetTextureOffset(meta, GetOperExpr(op, meta.Offset));
                return "texelFetchOffset(" + sampler + ", " + coords + ", " + lod + ", " + offset + ")." + ch;
            }

            return "texelFetch(" + sampler + ", " + coords + ", " + lod + ")." + ch;
        }

        private string GetTruncExpr(ShaderIrOp op) => GetUnaryCall(op, "trunc");

        private string GetUtofExpr(ShaderIrOp op)
        {
            return "float(uint(" + GetOperExpr(op, op.OperandA) + "))";
        }

        private string GetXorExpr(ShaderIrOp op) => GetBinaryExpr(op, "^");

        private string GetUnaryCall(ShaderIrOp op, string funcName)
        {
            return funcName + "(" + GetOperExpr(op, op.OperandA) + ")";
        }

        private string GetBinaryCall(ShaderIrOp op, string funcName)
        {
            return funcName + "(" + GetOperExpr(op, op.OperandA) + ", " +
                                    GetOperExpr(op, op.OperandB) + ")";
        }

        private string GetTernaryCall(ShaderIrOp op, string funcName)
        {
            return funcName + "(" + GetOperExpr(op, op.OperandA) + ", " +
                                    GetOperExpr(op, op.OperandB) + ", " +
                                    GetOperExpr(op, op.OperandC) + ")";
        }

        private string GetUnaryExpr(ShaderIrOp op, string opr)
        {
            return opr + GetOperExpr(op, op.OperandA);
        }

        private string GetBinaryExpr(ShaderIrOp op, string opr)
        {
            return GetOperExpr(op, op.OperandA) + " " + opr + " " +
                   GetOperExpr(op, op.OperandB);
        }

        private string GetBinaryExprWithNaN(ShaderIrOp op, string opr)
        {
            string a = GetOperExpr(op, op.OperandA);
            string b = GetOperExpr(op, op.OperandB);

            string nanCheck =
                " || isnan(" + a + ")" +
                " || isnan(" + b + ")";

            return a + " " + opr + " " + b + nanCheck;
        }

        private string GetTernaryExpr(ShaderIrOp op, string opr1, string opr2)
        {
            return GetOperExpr(op, op.OperandA) + " " + opr1 + " " +
                   GetOperExpr(op, op.OperandB) + " " + opr2 + " " +
                   GetOperExpr(op, op.OperandC);
        }

        private string GetTexSamplerName(ShaderIrOp op)
        {
            ShaderIrOperImm node = (ShaderIrOperImm)op.OperandC;

            int handle = ((ShaderIrOperImm)op.OperandC).Value;

            if (!_decl.Textures.TryGetValue(handle, out ShaderDeclInfo declInfo))
            {
                throw new InvalidOperationException();
            }

            return declInfo.Name;
        }

        private string GetTexSamplerCoords(ShaderIrOp op)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            bool hasDepth = (meta.TextureInstructionSuffix & TextureInstructionSuffix.Dc) != 0;

            int coords = ImageUtils.GetCoordsCountTextureTarget(meta.TextureTarget);

            bool isArray = ImageUtils.IsArray(meta.TextureTarget);


            string GetLastArgument(ShaderIrNode node)
            {
                string result = GetOperExpr(op, node);

                // array index is actually an integer so we need to pass it correctly
                if (isArray)
                {
                    result = "float(floatBitsToInt(" + result + "))";
                }

                return result;
            }

            string lastArgument;
            string depthArgument = "";

            int vecSize = coords;
            if (hasDepth && op.Inst != ShaderIrInst.Tld4)
            {
                vecSize++;
                depthArgument = $", {GetOperExpr(op, meta.DepthCompare)}";
            }

            switch (coords)
            {
                case 1:
                    if (hasDepth)
                    {
                        return $"vec3({GetOperExpr(op, meta.Coordinates[0])}, 0.0{depthArgument})";
                    }

                    return GetOperExpr(op, meta.Coordinates[0]);
                case 2:
                    lastArgument = GetLastArgument(meta.Coordinates[1]);

                    return $"vec{vecSize}({GetOperExpr(op, meta.Coordinates[0])}, {lastArgument}{depthArgument})";
                case 3:
                    lastArgument = GetLastArgument(meta.Coordinates[2]);

                    return $"vec{vecSize}({GetOperExpr(op, meta.Coordinates[0])}, {GetOperExpr(op, meta.Coordinates[1])}, {lastArgument}{depthArgument})";
                case 4:
                    lastArgument = GetLastArgument(meta.Coordinates[3]);

                    return $"vec4({GetOperExpr(op, meta.Coordinates[0])}, {GetOperExpr(op, meta.Coordinates[1])}, {GetOperExpr(op, meta.Coordinates[2])}, {lastArgument}){depthArgument}";
                default:
                    throw new InvalidOperationException();
            }

        }

        private string GetTextureOffset(ShaderIrMetaTex meta, string oper, int shift = 4, int mask = 0xF)
        {
            string GetOffset(string operation, int index)
            {
                return $"({operation} >> {index * shift}) & 0x{mask:x}";
            }

            int coords = ImageUtils.GetCoordsCountTextureTarget(meta.TextureTarget);

            if (ImageUtils.IsArray(meta.TextureTarget))
                coords -= 1;

            switch (coords)
            {
                case 1:
                    return GetOffset(oper, 0);
                case 2:
                    return "ivec2(" + GetOffset(oper, 0) + ", " + GetOffset(oper, 1) + ")";
                case 3:
                    return "ivec3(" + GetOffset(oper, 0) + ", " + GetOffset(oper, 1) + ", " + GetOffset(oper, 2) + ")";
                case 4:
                    return "ivec4(" + GetOffset(oper, 0) + ", " + GetOffset(oper, 1) + ", " + GetOffset(oper, 2) + ", " + GetOffset(oper, 3) + ")";
                default:
                    throw new InvalidOperationException();
            }
        }

        // TODO: support AOFFI on non nvidia drivers
        private string GetTextureGatherOperation(ShaderIrOp op, string sampler, string coords, string ch)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            TextureInstructionSuffix suffix = meta.TextureInstructionSuffix;

            string chString = "." + ch;

            string comp = meta.Component.ToString();

            if ((suffix & TextureInstructionSuffix.Dc) != 0)
            {
                comp = GetOperExpr(op, meta.DepthCompare);
            }

            if ((suffix & TextureInstructionSuffix.AOffI) != 0 && _isNvidiaDriver)
            {
                string offset = GetTextureOffset(meta, "floatBitsToInt((" + GetOperExpr(op, meta.Offset) + "))", 8, 0x3F);

                if ((suffix & TextureInstructionSuffix.Dc) != 0)
                {
                    return "textureGatherOffset(" + sampler + ", " + coords + ", " + comp + ", " + offset + ")" + chString;
                }

                return "textureGatherOffset(" + sampler + ", " + coords + ", " + offset + ", " + comp + ")" + chString;
            }
            // TODO: Support PTP
            else if ((suffix & TextureInstructionSuffix.Ptp) != 0)
            {
                throw new NotImplementedException();
            }

            return "textureGather(" + sampler + ", " + coords + ", " + comp + ")" + chString;
        }

        // TODO: support AOFFI on non nvidia drivers
        private string GetTextureOperation(ShaderIrOp op, string sampler, string coords, string ch)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            TextureInstructionSuffix suffix = meta.TextureInstructionSuffix;

            string chString = "." + ch;

            if ((suffix & TextureInstructionSuffix.Dc) != 0)
            {
                chString = "";
            }

            // TODO: Support LBA and LLA
            if ((suffix & TextureInstructionSuffix.Lz) != 0)
            {
                if ((suffix & TextureInstructionSuffix.AOffI) != 0 && _isNvidiaDriver)
                {
                    string offset = GetTextureOffset(meta, "floatBitsToInt((" + GetOperExpr(op, meta.Offset) + "))");

                    return "textureLodOffset(" + sampler + ", " + coords + ", 0.0, " + offset + ")" + chString;
                }

                return "textureLod(" + sampler + ", " + coords + ", 0.0)" + chString;
            }
            else if ((suffix & TextureInstructionSuffix.Lb) != 0)
            {
                if ((suffix & TextureInstructionSuffix.AOffI) != 0 && _isNvidiaDriver)
                {
                    string offset = GetTextureOffset(meta, "floatBitsToInt((" + GetOperExpr(op, meta.Offset) + "))");

                    return "textureOffset(" + sampler + ", " + coords + ", " + offset + ", " + GetOperExpr(op, meta.LevelOfDetail) + ")" + chString;
                }

                return "texture(" + sampler + ", " + coords + ", " + GetOperExpr(op, meta.LevelOfDetail) + ")" + chString;
            }
            else if ((suffix & TextureInstructionSuffix.Ll) != 0)
            {
                if ((suffix & TextureInstructionSuffix.AOffI) != 0 && _isNvidiaDriver)
                {
                    string offset = GetTextureOffset(meta, "floatBitsToInt((" + GetOperExpr(op, meta.Offset) + "))");

                    return "textureLodOffset(" + sampler + ", " + coords + ", " + GetOperExpr(op, meta.LevelOfDetail) + ", " + offset + ")" + chString;
                }

                return "textureLod(" + sampler + ", " + coords + ", " + GetOperExpr(op, meta.LevelOfDetail) + ")" + chString;
            }
            else if ((suffix & TextureInstructionSuffix.AOffI) != 0 && _isNvidiaDriver)
            {
                string offset = GetTextureOffset(meta, "floatBitsToInt((" + GetOperExpr(op, meta.Offset) + "))");

                return "textureOffset(" + sampler + ", " + coords + ", " + offset + ")" + chString;
            }
            else
            {
                return "texture(" + sampler + ", " + coords + ")" + chString;
            }
            throw new NotImplementedException($"Texture Suffix {meta.TextureInstructionSuffix} is not implemented");

        }

        private string GetITexSamplerCoords(ShaderIrOp op)
        {
            ShaderIrMetaTex meta = (ShaderIrMetaTex)op.MetaData;

            switch (ImageUtils.GetCoordsCountTextureTarget(meta.TextureTarget))
            {
                case 1:
                    return GetOperExpr(op, meta.Coordinates[0]);
                case 2:
                    return "ivec2(" + GetOperExpr(op, meta.Coordinates[0]) + ", " + GetOperExpr(op, meta.Coordinates[1]) + ")";
                case 3:
                    return "ivec3(" + GetOperExpr(op, meta.Coordinates[0]) + ", " + GetOperExpr(op, meta.Coordinates[1]) + ", " + GetOperExpr(op, meta.Coordinates[2]) + ")";
                default:
                    throw new InvalidOperationException();
            }
        }

        private string GetOperExpr(ShaderIrOp op, ShaderIrNode oper)
        {
            return GetExprWithCast(op, oper, GetSrcExpr(oper));
        }

        private static string GetExprWithCast(ShaderIrNode dst, ShaderIrNode src, string expr)
        {
            //Note: The "DstType" (of the cast) is the type that the operation
            //uses on the source operands, while the "SrcType" is the destination
            //type of the operand result (if it is a operation) or just the type
            //of the variable for registers/uniforms/attributes.
            OperType dstType = GetSrcNodeType(dst);
            OperType srcType = GetDstNodeType(src);

            if (dstType != srcType)
            {
                //Check for invalid casts
                //(like bool to int/float and others).
                if (srcType != OperType.F32 &&
                    srcType != OperType.I32)
                {
                    throw new InvalidOperationException();
                }

                switch (src)
                {
                    case ShaderIrOperGpr gpr:
                    {
                        //When the Gpr is ZR, just return the 0 value directly,
                        //since the float encoding for 0 is 0.
                        if (gpr.IsConst)
                        {
                            return "0";
                        }
                        break;
                    }
                }

                switch (dstType)
                {
                    case OperType.F32: expr = "intBitsToFloat(" + expr + ")"; break;
                    case OperType.I32: expr = "floatBitsToInt(" + expr + ")"; break;
                }
            }

            return expr;
        }

        private static string GetIntConst(int value)
        {
            string expr = value.ToString(CultureInfo.InvariantCulture);

            return value < 0 ? "(" + expr + ")" : expr;
        }

        private static string GetFloatConst(float value)
        {
            string expr = value.ToString(CultureInfo.InvariantCulture);

            return value < 0 ? "(" + expr + ")" : expr;
        }

        private static OperType GetDstNodeType(ShaderIrNode node)
        {
            //Special case instructions with the result type different
            //from the input types (like integer <-> float conversion) here.
            if (node is ShaderIrOp op)
            {
                switch (op.Inst)
                {
                    case ShaderIrInst.Stof:
                    case ShaderIrInst.Txlf:
                    case ShaderIrInst.Utof:
                        return OperType.F32;

                    case ShaderIrInst.Ftos:
                    case ShaderIrInst.Ftou:
                        return OperType.I32;
                }
            }

            return GetSrcNodeType(node);
        }

        private static OperType GetSrcNodeType(ShaderIrNode node)
        {
            switch (node)
            {
                case ShaderIrOperAbuf abuf:
                    return abuf.Offs == GlslDecl.LayerAttr      ||
                           abuf.Offs == GlslDecl.InstanceIdAttr ||
                           abuf.Offs == GlslDecl.VertexIdAttr   ||
                           abuf.Offs == GlslDecl.FaceAttr
                        ? OperType.I32
                        : OperType.F32;

                case ShaderIrOperCbuf cbuf: return OperType.F32;
                case ShaderIrOperGpr  gpr:  return OperType.F32;
                case ShaderIrOperImm  imm:  return OperType.I32;
                case ShaderIrOperImmf immf: return OperType.F32;
                case ShaderIrOperPred pred: return OperType.Bool;

                case ShaderIrOp op:
                    if (op.Inst > ShaderIrInst.B_Start &&
                        op.Inst < ShaderIrInst.B_End)
                    {
                        return OperType.Bool;
                    }
                    else if (op.Inst > ShaderIrInst.F_Start &&
                             op.Inst < ShaderIrInst.F_End)
                    {
                        return OperType.F32;
                    }
                    else if (op.Inst > ShaderIrInst.I_Start &&
                             op.Inst < ShaderIrInst.I_End)
                    {
                        return OperType.I32;
                    }
                    break;
            }

            throw new ArgumentException(nameof(node));
        }

        private static string GetBlockPosition(ShaderIrBlock block)
        {
            if (block != null)
            {
                return "0x" + block.Position.ToString("x8") + "u";
            }
            else
            {
                return "0u";
            }
        }
    }
}
