using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ryujinx.Graphics.Gal.Shader
{
    public class GlslDecompiler
    {
        private delegate string GetInstExpr(ShaderIrOp Op);

        private Dictionary<ShaderIrInst, GetInstExpr> InstsExpr;

        private enum OperType
        {
            Bool,
            F32,
            I32
        }

        private const string IdentationStr = "    ";

        private const int MaxVertexInput = 3;

        private GlslDecl Decl;

        private ShaderHeader Header, HeaderB;

        private ShaderIrBlock[] Blocks, BlocksB;

        private StringBuilder SB;

        public int MaxUboSize { get; }

        public GlslDecompiler(int MaxUboSize)
        {
            InstsExpr = new Dictionary<ShaderIrInst, GetInstExpr>()
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
                { ShaderIrInst.Trunc,  GetTruncExpr  },
                { ShaderIrInst.Txlf,   GetTxlfExpr   },
                { ShaderIrInst.Utof,   GetUtofExpr   },
                { ShaderIrInst.Xor,    GetXorExpr    }
            };

            this.MaxUboSize = MaxUboSize / 16;
        }

        public GlslProgram Decompile(
            IGalMemory    Memory,
            long          VpAPosition,
            long          VpBPosition,
            GalShaderType ShaderType)
        {
            Header  = new ShaderHeader(Memory, VpAPosition);
            HeaderB = new ShaderHeader(Memory, VpBPosition);

            Blocks  = ShaderDecoder.Decode(Memory, VpAPosition);
            BlocksB = ShaderDecoder.Decode(Memory, VpBPosition);

            GlslDecl DeclVpA = new GlslDecl(Blocks,  ShaderType, Header);
            GlslDecl DeclVpB = new GlslDecl(BlocksB, ShaderType, HeaderB);

            Decl = GlslDecl.Merge(DeclVpA, DeclVpB);

            return Decompile();
        }

        public GlslProgram Decompile(IGalMemory Memory, long Position, GalShaderType ShaderType)
        {
            Header  = new ShaderHeader(Memory, Position);
            HeaderB = null;

            Blocks  = ShaderDecoder.Decode(Memory, Position);
            BlocksB = null;

            Decl = new GlslDecl(Blocks, ShaderType, Header);

            return Decompile();
        }

        private GlslProgram Decompile()
        {
            SB = new StringBuilder();

            SB.AppendLine("#version 410 core");

            PrintDeclHeader();
            PrintDeclTextures();
            PrintDeclUniforms();
            PrintDeclAttributes();
            PrintDeclInAttributes();
            PrintDeclOutAttributes();
            PrintDeclGprs();
            PrintDeclPreds();
            PrintDeclSsy();

            if (BlocksB != null)
            {
                PrintBlockScope(Blocks, GlslDecl.BasicBlockAName);

                SB.AppendLine();

                PrintBlockScope(BlocksB, GlslDecl.BasicBlockBName);
            }
            else
            {
                PrintBlockScope(Blocks, GlslDecl.BasicBlockName);
            }

            SB.AppendLine();

            PrintMain();

            string GlslCode = SB.ToString();

            List<ShaderDeclInfo> TextureInfo = new List<ShaderDeclInfo>();

            TextureInfo.AddRange(Decl.Textures.Values);
            TextureInfo.AddRange(IterateCbTextures());

            return new GlslProgram(GlslCode, TextureInfo, Decl.Uniforms.Values);
        }

        private void PrintDeclHeader()
        {
            if (Decl.ShaderType == GalShaderType.Geometry)
            {
                int MaxVertices = Header.MaxOutputVertexCount;

                string OutputTopology;

                switch (Header.OutputTopology)
                {
                    case ShaderHeader.PointList:     OutputTopology = "points";         break;
                    case ShaderHeader.LineStrip:     OutputTopology = "line_strip";     break;
                    case ShaderHeader.TriangleStrip: OutputTopology = "triangle_strip"; break;

                    default: throw new InvalidOperationException();
                }

                SB.AppendLine("#extension GL_ARB_enhanced_layouts : require");

                SB.AppendLine();

                SB.AppendLine("// Stubbed. Maxwell geometry shaders don't inform input geometry type");

                SB.AppendLine("layout(triangles) in;" + Environment.NewLine);

                SB.AppendLine($"layout({OutputTopology}, max_vertices = {MaxVertices}) out;");

                SB.AppendLine();
            }
        }

        private void PrintDeclTextures()
        {
            foreach (ShaderDeclInfo DeclInfo in IterateCbTextures())
            {
                SB.AppendLine("uniform sampler2D " + DeclInfo.Name + ";");
            }

            PrintDecls(Decl.Textures, "uniform sampler2D");
        }

        private IEnumerable<ShaderDeclInfo> IterateCbTextures()
        {
            HashSet<string> Names = new HashSet<string>();

            foreach (ShaderDeclInfo DeclInfo in Decl.CbTextures.Values.OrderBy(DeclKeySelector))
            {
                if (Names.Add(DeclInfo.Name))
                {
                    yield return DeclInfo;
                }
            }
        }

        private void PrintDeclUniforms()
        {
            if (Decl.ShaderType == GalShaderType.Vertex)
            {
                //Memory layout here is [flip_x, flip_y, instance, unused]
                //It's using 4 bytes, not 8

                SB.AppendLine("layout (std140) uniform " + GlslDecl.ExtraUniformBlockName + " {");

                SB.AppendLine(IdentationStr + "vec2 " + GlslDecl.FlipUniformName + ";");

                SB.AppendLine(IdentationStr + "int " + GlslDecl.InstanceUniformName + ";");

                SB.AppendLine("};");
                SB.AppendLine();
            }

            foreach (ShaderDeclInfo DeclInfo in Decl.Uniforms.Values.OrderBy(DeclKeySelector))
            {
                SB.AppendLine($"layout (std140) uniform {DeclInfo.Name} {{");

                SB.AppendLine($"{IdentationStr}vec4 {DeclInfo.Name}_data[{MaxUboSize}];");

                SB.AppendLine("};");
            }

            if (Decl.Uniforms.Count > 0)
            {
                SB.AppendLine();
            }
        }

        private void PrintDeclAttributes()
        {
            string GeometryArray = (Decl.ShaderType == GalShaderType.Geometry) ? "[" + MaxVertexInput + "]" : "";

            PrintDecls(Decl.Attributes, Suffix: GeometryArray);
        }

        private void PrintDeclInAttributes()
        {
            if (Decl.ShaderType == GalShaderType.Fragment)
            {
                SB.AppendLine("layout (location = " + GlslDecl.PositionOutAttrLocation + ") in vec4 " + GlslDecl.PositionOutAttrName + ";");
            }

            if (Decl.ShaderType == GalShaderType.Geometry)
            {
                if (Decl.InAttributes.Count > 0)
                {
                    SB.AppendLine("in Vertex {");

                    foreach (ShaderDeclInfo DeclInfo in Decl.InAttributes.Values.OrderBy(DeclKeySelector))
                    {
                        if (DeclInfo.Index >= 0)
                        {
                            SB.AppendLine(IdentationStr + "layout (location = " + DeclInfo.Index + ") vec4 " + DeclInfo.Name + "; ");
                        }
                    }

                    SB.AppendLine("} block_in[];" + Environment.NewLine);
                }
            }
            else
            {
                PrintDeclAttributes(Decl.InAttributes.Values, "in");
            }
        }

        private void PrintDeclOutAttributes()
        {
            if (Decl.ShaderType == GalShaderType.Fragment)
            {
                int Count = 0;

                for (int Attachment = 0; Attachment < 8; Attachment++)
                {
                    if (Header.OmapTargets[Attachment].Enabled)
                    {
                        SB.AppendLine("layout (location = " + Attachment + ") out vec4 " + GlslDecl.FragmentOutputName + Attachment + ";");

                        Count++;
                    }
                }

                if (Count > 0)
                {
                    SB.AppendLine();
                }
            }
            else
            {
                SB.AppendLine("layout (location = " + GlslDecl.PositionOutAttrLocation + ") out vec4 " + GlslDecl.PositionOutAttrName + ";");
                SB.AppendLine();
            }

            PrintDeclAttributes(Decl.OutAttributes.Values, "out");
        }

        private void PrintDeclAttributes(IEnumerable<ShaderDeclInfo> Decls, string InOut)
        {
            int Count = 0;

            foreach (ShaderDeclInfo DeclInfo in Decls.OrderBy(DeclKeySelector))
            {
                if (DeclInfo.Index >= 0)
                {
                    SB.AppendLine("layout (location = " + DeclInfo.Index + ") " + InOut + " vec4 " + DeclInfo.Name + ";");

                    Count++;
                }
            }

            if (Count > 0)
            {
                SB.AppendLine();
            }
        }

        private void PrintDeclGprs()
        {
            PrintDecls(Decl.Gprs);
            PrintDecls(Decl.GprsHalf);
        }

        private void PrintDeclPreds()
        {
            PrintDecls(Decl.Preds, "bool");
        }

        private void PrintDeclSsy()
        {
            SB.AppendLine("uint " + GlslDecl.SsyCursorName + " = 0;");

            SB.AppendLine("uint " + GlslDecl.SsyStackName + "[" + GlslDecl.SsyStackSize + "];" + Environment.NewLine);
        }

        private void PrintDecls(IReadOnlyDictionary<int, ShaderDeclInfo> Dict, string CustomType = null, string Suffix = "")
        {
            foreach (ShaderDeclInfo DeclInfo in Dict.Values.OrderBy(DeclKeySelector))
            {
                string Name;

                if (CustomType != null)
                {
                    Name = CustomType + " " + DeclInfo.Name + Suffix + ";";
                }
                else if (DeclInfo.Name.Contains(GlslDecl.FragmentOutputName))
                {
                    Name = "layout (location = " + DeclInfo.Index / 4 + ") out vec4 " + DeclInfo.Name + Suffix + ";";
                }
                else
                {
                    Name = GetDecl(DeclInfo) + Suffix + ";";
                }

                SB.AppendLine(Name);
            }

            if (Dict.Count > 0)
            {
                SB.AppendLine();
            }
        }

        private int DeclKeySelector(ShaderDeclInfo DeclInfo)
        {
            return DeclInfo.Cbuf << 24 | DeclInfo.Index;
        }

        private string GetDecl(ShaderDeclInfo DeclInfo)
        {
            if (DeclInfo.Size == 4)
            {
                return "vec4 " + DeclInfo.Name;
            }
            else
            {
                return "float " + DeclInfo.Name;
            }
        }

        private void PrintMain()
        {
            SB.AppendLine("void main() {");

            foreach (KeyValuePair<int, ShaderDeclInfo> KV in Decl.InAttributes)
            {
                if (!Decl.Attributes.TryGetValue(KV.Key, out ShaderDeclInfo Attr))
                {
                    continue;
                }

                ShaderDeclInfo DeclInfo = KV.Value;

                if (Decl.ShaderType == GalShaderType.Geometry)
                {
                    for (int Vertex = 0; Vertex < MaxVertexInput; Vertex++)
                    {
                        string Dst = Attr.Name + "[" + Vertex + "]";

                        string Src = "block_in[" + Vertex + "]." + DeclInfo.Name;

                        SB.AppendLine(IdentationStr + Dst + " = " + Src + ";");
                    }
                }
                else
                {
                    SB.AppendLine(IdentationStr + Attr.Name + " = " + DeclInfo.Name + ";");
                }
            }

            SB.AppendLine(IdentationStr + "uint pc;");

            if (BlocksB != null)
            {
                PrintProgram(Blocks,  GlslDecl.BasicBlockAName);
                PrintProgram(BlocksB, GlslDecl.BasicBlockBName);
            }
            else
            {
                PrintProgram(Blocks, GlslDecl.BasicBlockName);
            }

            if (Decl.ShaderType != GalShaderType.Geometry)
            {
                PrintAttrToOutput();
            }

            if (Decl.ShaderType == GalShaderType.Fragment)
            {
                if (Header.OmapDepth)
                {
                    SB.AppendLine(IdentationStr + "gl_FragDepth = " + GlslDecl.GetGprName(Header.DepthRegister) + ";");
                }

                int GprIndex = 0;

                for (int Attachment = 0; Attachment < 8; Attachment++)
                {
                    string Output = GlslDecl.FragmentOutputName + Attachment;

                    OmapTarget Target = Header.OmapTargets[Attachment];

                    for (int Component = 0; Component < 4; Component++)
                    {
                        if (Target.ComponentEnabled(Component))
                        {
                            SB.AppendLine(IdentationStr + Output + "[" + Component + "] = " + GlslDecl.GetGprName(GprIndex) + ";");

                            GprIndex++;
                        }
                    }
                }
            }

            SB.AppendLine("}");
        }

        private void PrintProgram(ShaderIrBlock[] Blocks, string Name)
        {
            const string Ident1 = IdentationStr;
            const string Ident2 = Ident1 + IdentationStr;
            const string Ident3 = Ident2 + IdentationStr;
            const string Ident4 = Ident3 + IdentationStr;

            SB.AppendLine(Ident1 + "pc = " + GetBlockPosition(Blocks[0]) + ";");
            SB.AppendLine(Ident1 + "do {");
            SB.AppendLine(Ident2 + "switch (pc) {");

            foreach (ShaderIrBlock Block in Blocks)
            {
                string FunctionName = Block.Position.ToString("x8");

                SB.AppendLine(Ident3 + "case 0x" + FunctionName + ": pc = " + Name + "_" + FunctionName + "(); break;");
            }

            SB.AppendLine(Ident3 + "default:");
            SB.AppendLine(Ident4 + "pc = 0;");
            SB.AppendLine(Ident4 + "break;");

            SB.AppendLine(Ident2 + "}");
            SB.AppendLine(Ident1 + "} while (pc != 0);");
        }

        private void PrintAttrToOutput(string Identation = IdentationStr)
        {
            foreach (KeyValuePair<int, ShaderDeclInfo> KV in Decl.OutAttributes)
            {
                if (!Decl.Attributes.TryGetValue(KV.Key, out ShaderDeclInfo Attr))
                {
                    continue;
                }

                ShaderDeclInfo DeclInfo = KV.Value;

                string Name = Attr.Name;

                if (Decl.ShaderType == GalShaderType.Geometry)
                {
                    Name += "[0]";
                }

                SB.AppendLine(Identation + DeclInfo.Name + " = " + Name + ";");
            }

            if (Decl.ShaderType == GalShaderType.Vertex)
            {
                SB.AppendLine(Identation + "gl_Position.xy *= " + GlslDecl.FlipUniformName + ";");
            }

            if (Decl.ShaderType != GalShaderType.Fragment)
            {
                SB.AppendLine(Identation + GlslDecl.PositionOutAttrName + " = gl_Position;");
                SB.AppendLine(Identation + GlslDecl.PositionOutAttrName + ".w = 1;");
            }
        }

        private void PrintBlockScope(ShaderIrBlock[] Blocks, string Name)
        {
            foreach (ShaderIrBlock Block in Blocks)
            {
                SB.AppendLine("uint " + Name + "_" + Block.Position.ToString("x8") + "() {");

                PrintNodes(Block, Block.GetNodes());

                SB.AppendLine("}" + Environment.NewLine);
            }
        }

        private void PrintNodes(ShaderIrBlock Block, ShaderIrNode[] Nodes)
        {
            foreach (ShaderIrNode Node in Nodes)
            {
                PrintNode(Block, Node, IdentationStr);
            }

            if (Nodes.Length == 0)
            {
                SB.AppendLine(IdentationStr + "return 0u;");

                return;
            }

            ShaderIrNode Last = Nodes[Nodes.Length - 1];

            bool UnconditionalFlowChange = false;

            if (Last is ShaderIrOp Op)
            {
                switch (Op.Inst)
                {
                    case ShaderIrInst.Bra:
                    case ShaderIrInst.Exit:
                    case ShaderIrInst.Sync:
                        UnconditionalFlowChange = true;
                        break;
                }
            }

            if (!UnconditionalFlowChange)
            {
                if (Block.Next != null)
                {
                    SB.AppendLine(IdentationStr + "return " + GetBlockPosition(Block.Next) + ";");
                }
                else
                {
                    SB.AppendLine(IdentationStr + "return 0u;");
                }
            }
        }

        private void PrintNode(ShaderIrBlock Block, ShaderIrNode Node, string Identation)
        {
            if (Node is ShaderIrCond Cond)
            {
                string IfExpr = GetSrcExpr(Cond.Pred, true);

                if (Cond.Not)
                {
                    IfExpr = "!(" + IfExpr + ")";
                }

                SB.AppendLine(Identation + "if (" + IfExpr + ") {");

                PrintNode(Block, Cond.Child, Identation + IdentationStr);

                SB.AppendLine(Identation + "}");
            }
            else if (Node is ShaderIrAsg Asg)
            {
                if (IsValidOutOper(Asg.Dst))
                {
                    string Expr = GetSrcExpr(Asg.Src, true);

                    Expr = GetExprWithCast(Asg.Dst, Asg.Src, Expr);

                    SB.AppendLine(Identation + GetDstOperName(Asg.Dst) + " = " + Expr + ";");
                }
            }
            else if (Node is ShaderIrOp Op)
            {
                switch (Op.Inst)
                {
                    case ShaderIrInst.Bra:
                    {
                        SB.AppendLine(Identation + "return " + GetBlockPosition(Block.Branch) + ";");

                        break;
                    }

                    case ShaderIrInst.Emit:
                    {
                        PrintAttrToOutput(Identation);

                        SB.AppendLine(Identation + "EmitVertex();");

                        break;
                    }

                    case ShaderIrInst.Ssy:
                    {
                        string StackIndex = GlslDecl.SsyStackName + "[" + GlslDecl.SsyCursorName + "]";

                        int TargetPosition = (Op.OperandA as ShaderIrOperImm).Value;

                        string Target = "0x" + TargetPosition.ToString("x8") + "u";

                        SB.AppendLine(Identation + StackIndex + " = " + Target + ";");

                        SB.AppendLine(Identation + GlslDecl.SsyCursorName + "++;");

                        break;
                    }

                    case ShaderIrInst.Sync:
                    {
                        SB.AppendLine(Identation + GlslDecl.SsyCursorName + "--;");

                        string Target = GlslDecl.SsyStackName + "[" + GlslDecl.SsyCursorName + "]";

                        SB.AppendLine(Identation + "return " + Target + ";");

                        break;
                    }

                    default:
                        SB.AppendLine(Identation + GetSrcExpr(Op, true) + ";");

                        break;
                }
            }
            else if (Node is ShaderIrCmnt Cmnt)
            {
                SB.AppendLine(Identation + "// " + Cmnt.Comment);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private bool IsValidOutOper(ShaderIrNode Node)
        {
            if (Node is ShaderIrOperGpr Gpr && Gpr.IsConst)
            {
                return false;
            }
            else if (Node is ShaderIrOperPred Pred && Pred.IsConst)
            {
                return false;
            }

            return true;
        }

        private string GetDstOperName(ShaderIrNode Node)
        {
            if (Node is ShaderIrOperAbuf Abuf)
            {
                return GetOutAbufName(Abuf);
            }
            else if (Node is ShaderIrOperGpr Gpr)
            {
                return GetName(Gpr);
            }
            else if (Node is ShaderIrOperPred Pred)
            {
                return GetName(Pred);
            }

            throw new ArgumentException(nameof(Node));
        }

        private string GetSrcExpr(ShaderIrNode Node, bool Entry = false)
        {
            switch (Node)
            {
                case ShaderIrOperAbuf Abuf: return GetName (Abuf);
                case ShaderIrOperCbuf Cbuf: return GetName (Cbuf);
                case ShaderIrOperGpr  Gpr:  return GetName (Gpr);
                case ShaderIrOperImm  Imm:  return GetValue(Imm);
                case ShaderIrOperImmf Immf: return GetValue(Immf);
                case ShaderIrOperPred Pred: return GetName (Pred);

                case ShaderIrOp Op:
                    string Expr;

                    if (InstsExpr.TryGetValue(Op.Inst, out GetInstExpr GetExpr))
                    {
                        Expr = GetExpr(Op);
                    }
                    else
                    {
                        throw new NotImplementedException(Op.Inst.ToString());
                    }

                    if (!Entry && NeedsParentheses(Op))
                    {
                        Expr = "(" + Expr + ")";
                    }

                    return Expr;

                default: throw new ArgumentException(nameof(Node));
            }
        }

        private static bool NeedsParentheses(ShaderIrOp Op)
        {
            switch (Op.Inst)
            {
                case ShaderIrInst.Ipa:
                case ShaderIrInst.Texq:
                case ShaderIrInst.Texs:
                case ShaderIrInst.Txlf:
                    return false;
            }

            return true;
        }

        private string GetName(ShaderIrOperCbuf Cbuf)
        {
            if (!Decl.Uniforms.TryGetValue(Cbuf.Index, out ShaderDeclInfo DeclInfo))
            {
                throw new InvalidOperationException();
            }

            if (Cbuf.Offs != null)
            {
                string Offset = "floatBitsToInt(" + GetSrcExpr(Cbuf.Offs) + ")";

                string Index = "(" + Cbuf.Pos * 4 + " + " + Offset + ")";

                return $"{DeclInfo.Name}_data[{Index} / 16][({Index} / 4) % 4]";
            }
            else
            {
                return $"{DeclInfo.Name}_data[{Cbuf.Pos / 4}][{Cbuf.Pos % 4}]";
            }
        }

        private string GetOutAbufName(ShaderIrOperAbuf Abuf)
        {
            if (Decl.ShaderType == GalShaderType.Geometry)
            {
                switch (Abuf.Offs)
                {
                    case GlslDecl.LayerAttr: return "gl_Layer";
                }
            }

            return GetAttrTempName(Abuf);
        }

        private string GetName(ShaderIrOperAbuf Abuf)
        {
            //Handle special scalar read-only attributes here.
            if (Decl.ShaderType == GalShaderType.Vertex)
            {
                switch (Abuf.Offs)
                {
                    case GlslDecl.VertexIdAttr:   return "gl_VertexID";
                    case GlslDecl.InstanceIdAttr: return GlslDecl.InstanceUniformName;
                }
            }
            else if (Decl.ShaderType == GalShaderType.TessEvaluation)
            {
                switch (Abuf.Offs)
                {
                    case GlslDecl.TessCoordAttrX: return "gl_TessCoord.x";
                    case GlslDecl.TessCoordAttrY: return "gl_TessCoord.y";
                    case GlslDecl.TessCoordAttrZ: return "gl_TessCoord.z";
                }
            }
            else if (Decl.ShaderType == GalShaderType.Fragment)
            {
                switch (Abuf.Offs)
                {
                    case GlslDecl.PointCoordAttrX: return "gl_PointCoord.x";
                    case GlslDecl.PointCoordAttrY: return "gl_PointCoord.y";

                    //Note: It's a guess that Maxwell's face is 1 when gl_FrontFacing == true
                    case GlslDecl.FaceAttr: return "(gl_FrontFacing ? 1 : 0)";
                }
            }

            return GetAttrTempName(Abuf);
        }

        private string GetAttrTempName(ShaderIrOperAbuf Abuf)
        {
            int Index =  Abuf.Offs >> 4;
            int Elem  = (Abuf.Offs >> 2) & 3;

            string Swizzle = "." + GetAttrSwizzle(Elem);

            if (!Decl.Attributes.TryGetValue(Index, out ShaderDeclInfo DeclInfo))
            {
                //Handle special vec4 attributes here
                //(for example, index 7 is always gl_Position).
                if (Index == GlslDecl.GlPositionVec4Index)
                {
                    string Name =
                        Decl.ShaderType != GalShaderType.Vertex &&
                        Decl.ShaderType != GalShaderType.Geometry ? GlslDecl.PositionOutAttrName : "gl_Position";

                    return Name + Swizzle;
                }
                else if (Abuf.Offs == GlslDecl.PointSizeAttr)
                {
                    return "gl_PointSize";
                }
            }

            if (DeclInfo.Index >= 32)
            {
                throw new InvalidOperationException($"Shader attribute offset {Abuf.Offs} is invalid.");
            }

            if (Decl.ShaderType == GalShaderType.Geometry)
            {
                string Vertex = "floatBitsToInt(" + GetSrcExpr(Abuf.Vertex) + ")";

                return DeclInfo.Name + "[" + Vertex + "]" + Swizzle;
            }
            else
            {
                return DeclInfo.Name + Swizzle;
            }
        }

        private string GetName(ShaderIrOperGpr Gpr)
        {
            if (Gpr.IsConst)
            {
                return "0";
            }

            if (Gpr.RegisterSize == ShaderRegisterSize.Single)
            {
                return GetNameWithSwizzle(Decl.Gprs, Gpr.Index);
            }
            else if (Gpr.RegisterSize == ShaderRegisterSize.Half)
            {
                return GetNameWithSwizzle(Decl.GprsHalf, (Gpr.Index << 1) | Gpr.HalfPart);
            }
            else /* if (Gpr.RegisterSize == ShaderRegisterSize.Double) */
            {
                throw new NotImplementedException("Double types are not supported.");
            }
        }

        private string GetValue(ShaderIrOperImm Imm)
        {
            //Only use hex is the value is too big and would likely be hard to read as int.
            if (Imm.Value >  0xfff ||
                Imm.Value < -0xfff)
            {
                return "0x" + Imm.Value.ToString("x8", CultureInfo.InvariantCulture);
            }
            else
            {
                return GetIntConst(Imm.Value);
            }
        }

        private string GetValue(ShaderIrOperImmf Immf)
        {
            return GetFloatConst(Immf.Value);
        }

        private string GetName(ShaderIrOperPred Pred)
        {
            return Pred.IsConst ? "true" : GetNameWithSwizzle(Decl.Preds, Pred.Index);
        }

        private string GetNameWithSwizzle(IReadOnlyDictionary<int, ShaderDeclInfo> Dict, int Index)
        {
            int VecIndex = Index & ~3;

            if (Dict.TryGetValue(VecIndex, out ShaderDeclInfo DeclInfo))
            {
                if (DeclInfo.Size > 1 && Index < VecIndex + DeclInfo.Size)
                {
                    return DeclInfo.Name + "." + GetAttrSwizzle(Index & 3);
                }
            }

            if (!Dict.TryGetValue(Index, out DeclInfo))
            {
                throw new InvalidOperationException();
            }

            return DeclInfo.Name;
        }

        private string GetAttrSwizzle(int Elem)
        {
            return "xyzw".Substring(Elem, 1);
        }

        private string GetAbsExpr(ShaderIrOp Op) => GetUnaryCall(Op, "abs");

        private string GetAddExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "+");

        private string GetAndExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "&");

        private string GetAsrExpr(ShaderIrOp Op) => GetBinaryExpr(Op, ">>");

        private string GetBandExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "&&");

        private string GetBnotExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "!");

        private string GetBorExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "||");

        private string GetBxorExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "^^");

        private string GetCeilExpr(ShaderIrOp Op) => GetUnaryCall(Op, "ceil");

        private string GetClampsExpr(ShaderIrOp Op)
        {
            return "clamp(" + GetOperExpr(Op, Op.OperandA) + ", " +
                              GetOperExpr(Op, Op.OperandB) + ", " +
                              GetOperExpr(Op, Op.OperandC) + ")";
        }

        private string GetClampuExpr(ShaderIrOp Op)
        {
            return "int(clamp(uint(" + GetOperExpr(Op, Op.OperandA) + "), " +
                             "uint(" + GetOperExpr(Op, Op.OperandB) + "), " +
                             "uint(" + GetOperExpr(Op, Op.OperandC) + ")))";
        }

        private string GetCeqExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "==");

        private string GetCequExpr(ShaderIrOp Op) => GetBinaryExprWithNaN(Op, "==");

        private string GetCgeExpr(ShaderIrOp Op) => GetBinaryExpr(Op, ">=");

        private string GetCgeuExpr(ShaderIrOp Op) => GetBinaryExprWithNaN(Op, ">=");

        private string GetCgtExpr(ShaderIrOp Op) => GetBinaryExpr(Op, ">");

        private string GetCgtuExpr(ShaderIrOp Op) => GetBinaryExprWithNaN(Op, ">");

        private string GetCleExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "<=");

        private string GetCleuExpr(ShaderIrOp Op) => GetBinaryExprWithNaN(Op, "<=");

        private string GetCltExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "<");

        private string GetCltuExpr(ShaderIrOp Op) => GetBinaryExprWithNaN(Op, "<");

        private string GetCnanExpr(ShaderIrOp Op) => GetUnaryCall(Op, "isnan");

        private string GetCneExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "!=");

        private string GetCutExpr(ShaderIrOp Op) => "EndPrimitive()";

        private string GetCneuExpr(ShaderIrOp Op) => GetBinaryExprWithNaN(Op, "!=");

        private string GetCnumExpr(ShaderIrOp Op) => GetUnaryCall(Op, "!isnan");

        private string GetExitExpr(ShaderIrOp Op) => "return 0u";

        private string GetFcosExpr(ShaderIrOp Op) => GetUnaryCall(Op, "cos");

        private string GetFex2Expr(ShaderIrOp Op) => GetUnaryCall(Op, "exp2");

        private string GetFfmaExpr(ShaderIrOp Op) => GetTernaryExpr(Op, "*", "+");

        private string GetFclampExpr(ShaderIrOp Op) => GetTernaryCall(Op, "clamp");

        private string GetFlg2Expr(ShaderIrOp Op) => GetUnaryCall(Op, "log2");

        private string GetFloorExpr(ShaderIrOp Op) => GetUnaryCall(Op, "floor");

        private string GetFrcpExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "1 / ");

        private string GetFrsqExpr(ShaderIrOp Op) => GetUnaryCall(Op, "inversesqrt");

        private string GetFsinExpr(ShaderIrOp Op) => GetUnaryCall(Op, "sin");

        private string GetFsqrtExpr(ShaderIrOp Op) => GetUnaryCall(Op, "sqrt");

        private string GetFtosExpr(ShaderIrOp Op)
        {
            return "int(" + GetOperExpr(Op, Op.OperandA) + ")";
        }

        private string GetFtouExpr(ShaderIrOp Op)
        {
            return "int(uint(" + GetOperExpr(Op, Op.OperandA) + "))";
        }

        private string GetIpaExpr(ShaderIrOp Op)
        {
            ShaderIrMetaIpa Meta = (ShaderIrMetaIpa)Op.MetaData;

            ShaderIrOperAbuf Abuf = (ShaderIrOperAbuf)Op.OperandA;

            if (Meta.Mode == ShaderIpaMode.Pass)
            {
                int Index = Abuf.Offs >> 4;
                int Elem = (Abuf.Offs >> 2) & 3;

                if (Decl.ShaderType == GalShaderType.Fragment && Index == GlslDecl.GlPositionVec4Index)
                {
                    switch (Elem)
                    {
                        case 0: return "gl_FragCoord.x";
                        case 1: return "gl_FragCoord.y";
                        case 2: return "gl_FragCoord.z";
                        case 3: return "1";
                    }
                }
            }

            return GetSrcExpr(Op.OperandA);
        }

        private string GetKilExpr(ShaderIrOp Op) => "discard";

        private string GetLslExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "<<");
        private string GetLsrExpr(ShaderIrOp Op)
        {
            return "int(uint(" + GetOperExpr(Op, Op.OperandA) + ") >> " +
                                 GetOperExpr(Op, Op.OperandB) + ")";
        }

        private string GetMaxExpr(ShaderIrOp Op) => GetBinaryCall(Op, "max");
        private string GetMinExpr(ShaderIrOp Op) => GetBinaryCall(Op, "min");

        private string GetMulExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "*");

        private string GetNegExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "-");

        private string GetNotExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "~");

        private string GetOrExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "|");

        private string GetStofExpr(ShaderIrOp Op)
        {
            return "float(" + GetOperExpr(Op, Op.OperandA) + ")";
        }

        private string GetSubExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "-");

        private string GetTexbExpr(ShaderIrOp Op)
        {
            ShaderIrMetaTex Meta = (ShaderIrMetaTex)Op.MetaData;

            if (!Decl.CbTextures.TryGetValue(Op, out ShaderDeclInfo DeclInfo))
            {
                throw new InvalidOperationException();
            }

            string Coords = GetTexSamplerCoords(Op);

            string Ch = "rgba".Substring(Meta.Elem, 1);

            return "texture(" + DeclInfo.Name + ", " + Coords + ")." + Ch;
        }

        private string GetTexqExpr(ShaderIrOp Op)
        {
            ShaderIrMetaTexq Meta = (ShaderIrMetaTexq)Op.MetaData;

            string Ch = "xyzw".Substring(Meta.Elem, 1);

            if (Meta.Info == ShaderTexqInfo.Dimension)
            {
                string Sampler = GetTexSamplerName(Op);

                string Lod = GetOperExpr(Op, Op.OperandA); //???

                return "textureSize(" + Sampler + ", " + Lod + ")." + Ch;
            }
            else
            {
                throw new NotImplementedException(Meta.Info.ToString());
            }
        }

        private string GetTexsExpr(ShaderIrOp Op)
        {
            ShaderIrMetaTex Meta = (ShaderIrMetaTex)Op.MetaData;

            string Sampler = GetTexSamplerName(Op);

            string Coords = GetTexSamplerCoords(Op);

            string Ch = "rgba".Substring(Meta.Elem, 1);

            return "texture(" + Sampler + ", " + Coords + ")." + Ch;
        }

        private string GetTxlfExpr(ShaderIrOp Op)
        {
            ShaderIrMetaTex Meta = (ShaderIrMetaTex)Op.MetaData;

            string Sampler = GetTexSamplerName(Op);

            string Coords = GetITexSamplerCoords(Op);

            string Ch = "rgba".Substring(Meta.Elem, 1);

            return "texelFetch(" + Sampler + ", " + Coords + ", 0)." + Ch;
        }

        private string GetTruncExpr(ShaderIrOp Op) => GetUnaryCall(Op, "trunc");

        private string GetUtofExpr(ShaderIrOp Op)
        {
            return "float(uint(" + GetOperExpr(Op, Op.OperandA) + "))";
        }

        private string GetXorExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "^");

        private string GetUnaryCall(ShaderIrOp Op, string FuncName)
        {
            return FuncName + "(" + GetOperExpr(Op, Op.OperandA) + ")";
        }

        private string GetBinaryCall(ShaderIrOp Op, string FuncName)
        {
            return FuncName + "(" + GetOperExpr(Op, Op.OperandA) + ", " +
                                    GetOperExpr(Op, Op.OperandB) + ")";
        }

        private string GetTernaryCall(ShaderIrOp Op, string FuncName)
        {
            return FuncName + "(" + GetOperExpr(Op, Op.OperandA) + ", " +
                                    GetOperExpr(Op, Op.OperandB) + ", " +
                                    GetOperExpr(Op, Op.OperandC) + ")";
        }

        private string GetUnaryExpr(ShaderIrOp Op, string Opr)
        {
            return Opr + GetOperExpr(Op, Op.OperandA);
        }

        private string GetBinaryExpr(ShaderIrOp Op, string Opr)
        {
            return GetOperExpr(Op, Op.OperandA) + " " + Opr + " " +
                   GetOperExpr(Op, Op.OperandB);
        }

        private string GetBinaryExprWithNaN(ShaderIrOp Op, string Opr)
        {
            string A = GetOperExpr(Op, Op.OperandA);
            string B = GetOperExpr(Op, Op.OperandB);

            string NaNCheck =
                " || isnan(" + A + ")" +
                " || isnan(" + B + ")";

            return A + " " + Opr + " " + B + NaNCheck;
        }

        private string GetTernaryExpr(ShaderIrOp Op, string Opr1, string Opr2)
        {
            return GetOperExpr(Op, Op.OperandA) + " " + Opr1 + " " +
                   GetOperExpr(Op, Op.OperandB) + " " + Opr2 + " " +
                   GetOperExpr(Op, Op.OperandC);
        }

        private string GetTexSamplerName(ShaderIrOp Op)
        {
            ShaderIrOperImm Node = (ShaderIrOperImm)Op.OperandC;

            int Handle = ((ShaderIrOperImm)Op.OperandC).Value;

            if (!Decl.Textures.TryGetValue(Handle, out ShaderDeclInfo DeclInfo))
            {
                throw new InvalidOperationException();
            }

            return DeclInfo.Name;
        }

        private string GetTexSamplerCoords(ShaderIrOp Op)
        {
            return "vec2(" + GetOperExpr(Op, Op.OperandA) + ", " +
                             GetOperExpr(Op, Op.OperandB) + ")";
        }

        private string GetITexSamplerCoords(ShaderIrOp Op)
        {
            return "ivec2(" + GetOperExpr(Op, Op.OperandA) + ", " +
                              GetOperExpr(Op, Op.OperandB) + ")";
        }

        private string GetOperExpr(ShaderIrOp Op, ShaderIrNode Oper)
        {
            return GetExprWithCast(Op, Oper, GetSrcExpr(Oper));
        }

        private static string GetExprWithCast(ShaderIrNode Dst, ShaderIrNode Src, string Expr)
        {
            //Note: The "DstType" (of the cast) is the type that the operation
            //uses on the source operands, while the "SrcType" is the destination
            //type of the operand result (if it is a operation) or just the type
            //of the variable for registers/uniforms/attributes.
            OperType DstType = GetSrcNodeType(Dst);
            OperType SrcType = GetDstNodeType(Src);

            if (DstType != SrcType)
            {
                //Check for invalid casts
                //(like bool to int/float and others).
                if (SrcType != OperType.F32 &&
                    SrcType != OperType.I32)
                {
                    throw new InvalidOperationException();
                }

                switch (Src)
                {
                    case ShaderIrOperGpr Gpr:
                    {
                        //When the Gpr is ZR, just return the 0 value directly,
                        //since the float encoding for 0 is 0.
                        if (Gpr.IsConst)
                        {
                            return "0";
                        }
                        break;
                    }

                    case ShaderIrOperImm Imm:
                    {
                        //For integer immediates being used as float,
                        //it's better (for readability) to just return the float value.
                        if (DstType == OperType.F32)
                        {
                            float Value = BitConverter.Int32BitsToSingle(Imm.Value);

                            if (!float.IsNaN(Value) && !float.IsInfinity(Value))
                            {
                                return GetFloatConst(Value);
                            }
                        }
                        break;
                    }
                }

                switch (DstType)
                {
                    case OperType.F32: Expr = "intBitsToFloat(" + Expr + ")"; break;
                    case OperType.I32: Expr = "floatBitsToInt(" + Expr + ")"; break;
                }
            }

            return Expr;
        }

        private static string GetIntConst(int Value)
        {
            string Expr = Value.ToString(CultureInfo.InvariantCulture);

            return Value < 0 ? "(" + Expr + ")" : Expr;
        }

        private static string GetFloatConst(float Value)
        {
            string Expr = Value.ToString(CultureInfo.InvariantCulture);

            return Value < 0 ? "(" + Expr + ")" : Expr;
        }

        private static OperType GetDstNodeType(ShaderIrNode Node)
        {
            //Special case instructions with the result type different
            //from the input types (like integer <-> float conversion) here.
            if (Node is ShaderIrOp Op)
            {
                switch (Op.Inst)
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

            return GetSrcNodeType(Node);
        }

        private static OperType GetSrcNodeType(ShaderIrNode Node)
        {
            switch (Node)
            {
                case ShaderIrOperAbuf Abuf:
                    return Abuf.Offs == GlslDecl.LayerAttr      ||
                           Abuf.Offs == GlslDecl.InstanceIdAttr ||
                           Abuf.Offs == GlslDecl.VertexIdAttr   ||
                           Abuf.Offs == GlslDecl.FaceAttr
                        ? OperType.I32
                        : OperType.F32;

                case ShaderIrOperCbuf Cbuf: return OperType.F32;
                case ShaderIrOperGpr  Gpr:  return OperType.F32;
                case ShaderIrOperImm  Imm:  return OperType.I32;
                case ShaderIrOperImmf Immf: return OperType.F32;
                case ShaderIrOperPred Pred: return OperType.Bool;

                case ShaderIrOp Op:
                    if (Op.Inst > ShaderIrInst.B_Start &&
                        Op.Inst < ShaderIrInst.B_End)
                    {
                        return OperType.Bool;
                    }
                    else if (Op.Inst > ShaderIrInst.F_Start &&
                             Op.Inst < ShaderIrInst.F_End)
                    {
                        return OperType.F32;
                    }
                    else if (Op.Inst > ShaderIrInst.I_Start &&
                             Op.Inst < ShaderIrInst.I_End)
                    {
                        return OperType.I32;
                    }
                    break;
            }

            throw new ArgumentException(nameof(Node));
        }

        private static string GetBlockPosition(ShaderIrBlock Block)
        {
            if (Block != null)
            {
                return "0x" + Block.Position.ToString("x8") + "u";
            }
            else
            {
                return "0u";
            }
        }
    }
}
