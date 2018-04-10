using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ryujinx.Graphics.Gal.Shader
{
    class GlslDecompiler
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

        private static string[] ElemTypes = new string[] { "float", "vec2", "vec3", "vec4" };

        private GlslDecl Decl;

        private StringBuilder SB;

        public GlslDecompiler()
        {
            InstsExpr = new Dictionary<ShaderIrInst, GetInstExpr>()
            {
                { ShaderIrInst.And,   GetAndExpr   },
                { ShaderIrInst.Asr,   GetAsrExpr   },
                { ShaderIrInst.Band,  GetBandExpr  },
                { ShaderIrInst.Bnot,  GetBnotExpr  },
                { ShaderIrInst.Ceil,  GetCeilExpr  },
                { ShaderIrInst.Ceq,   GetCeqExpr   },
                { ShaderIrInst.Cge,   GetCgeExpr   },
                { ShaderIrInst.Cgt,   GetCgtExpr   },
                { ShaderIrInst.Clamp, GetClampExpr },
                { ShaderIrInst.Cle,   GetCleExpr   },
                { ShaderIrInst.Clt,   GetCltExpr   },
                { ShaderIrInst.Cne,   GetCneExpr   },
                { ShaderIrInst.Exit,  GetExitExpr  },
                { ShaderIrInst.Fabs,  GetFabsExpr  },
                { ShaderIrInst.Fadd,  GetFaddExpr  },
                { ShaderIrInst.Fceq,  GetCeqExpr   },
                { ShaderIrInst.Fcge,  GetCgeExpr   },
                { ShaderIrInst.Fcgt,  GetCgtExpr   },
                { ShaderIrInst.Fcle,  GetCleExpr   },
                { ShaderIrInst.Fclt,  GetCltExpr   },
                { ShaderIrInst.Fcne,  GetCneExpr   },
                { ShaderIrInst.Fcos,  GetFcosExpr  },
                { ShaderIrInst.Fex2,  GetFex2Expr  },
                { ShaderIrInst.Ffma,  GetFfmaExpr  },
                { ShaderIrInst.Flg2,  GetFlg2Expr  },
                { ShaderIrInst.Floor, GetFloorExpr },
                { ShaderIrInst.Fmul,  GetFmulExpr  },
                { ShaderIrInst.Fneg,  GetFnegExpr  },
                { ShaderIrInst.Frcp,  GetFrcpExpr  },
                { ShaderIrInst.Frsq,  GetFrsqExpr  },
                { ShaderIrInst.Fsin,  GetFsinExpr  },
                { ShaderIrInst.Ftos,  GetFtosExpr  },
                { ShaderIrInst.Ftou,  GetFtouExpr  },
                { ShaderIrInst.Ipa,   GetIpaExpr   },
                { ShaderIrInst.Kil,   GetKilExpr   },
                { ShaderIrInst.Lsr,   GetLsrExpr   },
                { ShaderIrInst.Not,   GetNotExpr   },
                { ShaderIrInst.Or,    GetOrExpr    },
                { ShaderIrInst.Stof,  GetStofExpr  },
                { ShaderIrInst.Texq,  GetTexqExpr  },
                { ShaderIrInst.Texs,  GetTexsExpr  },
                { ShaderIrInst.Trunc, GetTruncExpr },
                { ShaderIrInst.Txlf,  GetTxlfExpr  },
                { ShaderIrInst.Utof,  GetUtofExpr  },
                { ShaderIrInst.Xor,   GetXorExpr   }
            };
        }

        public GlslProgram Decompile(int[] Code, GalShaderType ShaderType)
        {
            ShaderIrBlock Block = ShaderDecoder.DecodeBasicBlock(Code, 0, ShaderType);

            ShaderIrNode[] Nodes = Block.GetNodes();

            Decl = new GlslDecl(Nodes, ShaderType);

            SB = new StringBuilder();

            SB.AppendLine("#version 330 core");

            PrintDeclTextures();
            PrintDeclUniforms();
            PrintDeclInAttributes();
            PrintDeclOutAttributes();
            PrintDeclGprs();
            PrintDeclPreds();

            PrintBlockScope("void main()", 1, Nodes);

            string GlslCode = SB.ToString();

            return new GlslProgram(
                GlslCode,
                Decl.Textures.Values,
                Decl.Uniforms.Values);
        }

        private void PrintDeclTextures()
        {
            PrintDecls(Decl.Textures, "uniform sampler2D");
        }

        private void PrintDeclUniforms()
        {
            foreach (ShaderDeclInfo DeclInfo in Decl.Uniforms.Values.OrderBy(DeclKeySelector))
            {
                SB.AppendLine($"uniform {GetDecl(DeclInfo)};");
            }

            if (Decl.Uniforms.Count > 0)
            {
                SB.AppendLine();
            }
        }

        private void PrintDeclInAttributes()
        {
            if (Decl.ShaderType == GalShaderType.Fragment)
            {
                SB.AppendLine("in vec4 " + GlslDecl.PositionOutAttrName + ";");
            }

            PrintDeclAttributes(Decl.InAttributes.Values, "in");
        }

        private void PrintDeclOutAttributes()
        {
            if (Decl.ShaderType == GalShaderType.Vertex)
            {
                SB.AppendLine("out vec4 " + GlslDecl.PositionOutAttrName + ";");
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
                    SB.AppendLine("layout (location = " + DeclInfo.Index + ") " + InOut + " " + GetDecl(DeclInfo) + ";");

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
        }

        private void PrintDeclPreds()
        {
            PrintDecls(Decl.Preds, "bool");
        }

        private void PrintDecls(IReadOnlyDictionary<int, ShaderDeclInfo> Dict, string CustomType = null)
        {
            foreach (ShaderDeclInfo DeclInfo in Dict.Values.OrderBy(DeclKeySelector))
            {
                string Name;

                if (CustomType != null)
                {
                    Name = CustomType + " " + DeclInfo.Name + ";";
                }
                else if (DeclInfo.Name == GlslDecl.FragmentOutputName)
                {
                    Name = "layout (location = 0) out " + GetDecl(DeclInfo) + ";";
                }
                else
                {
                    Name = GetDecl(DeclInfo) + ";";
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
            return ElemTypes[DeclInfo.Size - 1] + " " + DeclInfo.Name;
        }

        private void PrintBlockScope(string ScopeName, int IdentationLevel, params ShaderIrNode[] Nodes)
        {
            string Identation = string.Empty;

            for (int Index = 0; Index < IdentationLevel - 1; Index++)
            {
                Identation += IdentationStr;
            }

            if (ScopeName != string.Empty)
            {
                ScopeName += " ";
            }

            SB.AppendLine(Identation + ScopeName + "{");

            string LastLine = Identation + "}";

            if (IdentationLevel > 0)
            {
                Identation += IdentationStr;
            }

            for (int Index = 0; Index < Nodes.Length; Index++)
            {
                ShaderIrNode Node = Nodes[Index];

                if (Node is ShaderIrCond Cond)
                {
                    string IfExpr = GetSrcExpr(Cond.Pred, true);

                    if (Cond.Not)
                    {
                        IfExpr = "!(" + IfExpr + ")";
                    }

                    string SubScopeName = "if (" + IfExpr + ")";

                    PrintBlockScope(SubScopeName, IdentationLevel + 1, Cond.Child);
                }
                else if (Node is ShaderIrAsg Asg && IsValidOutOper(Asg.Dst))
                {
                    string Expr = GetSrcExpr(Asg.Src, true);

                    Expr = GetExprWithCast(Asg.Dst, Asg.Src, Expr);

                    SB.AppendLine(Identation + GetDstOperName(Asg.Dst) + " = " + Expr + ";");
                }
                else if (Node is ShaderIrOp Op)
                {
                    if (Op.Inst == ShaderIrInst.Exit)
                    {
                        //Do everything that needs to be done before
                        //the shader ends here.
                        if (Decl.ShaderType == GalShaderType.Vertex)
                        {
                            SB.AppendLine(Identation + GlslDecl.PositionOutAttrName + " = gl_Position;");
                        }
                    }

                    SB.AppendLine(Identation + GetSrcExpr(Op, true) + ";");
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            SB.AppendLine(LastLine);
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
                case ShaderIrInst.Frcp:
                    return true;

                case ShaderIrInst.Ipa:
                case ShaderIrInst.Texq:
                case ShaderIrInst.Texs:
                case ShaderIrInst.Txlf:
                    return false;
            }

            return Op.OperandB != null ||
                   Op.OperandC != null;
        }

        private string GetName(ShaderIrOperCbuf Cbuf)
        {
            if (!Decl.Uniforms.TryGetValue((Cbuf.Index, Cbuf.Offs), out ShaderDeclInfo DeclInfo))
            {
                throw new InvalidOperationException();
            }

            return DeclInfo.Name;
        }

        private string GetOutAbufName(ShaderIrOperAbuf Abuf)
        {
            return GetName(Decl.OutAttributes, Abuf);
        }

        private string GetName(ShaderIrOperAbuf Abuf)
        {
            if (Abuf.Offs == GlslDecl.VertexIdAttr)
            {
                return "gl_VertexID";
            }

            return GetName(Decl.InAttributes, Abuf);
        }

        private string GetName(IReadOnlyDictionary<int, ShaderDeclInfo> Dict, ShaderIrOperAbuf Abuf)
        {
            int Index =  Abuf.Offs >> 4;
            int Elem  = (Abuf.Offs >> 2) & 3;

            if (!Dict.TryGetValue(Index, out ShaderDeclInfo DeclInfo))
            {
                throw new InvalidOperationException();
            }

            return DeclInfo.Size > 1 ? DeclInfo.Name + "." + GetAttrSwizzle(Elem) : DeclInfo.Name;
        }

        private string GetName(ShaderIrOperGpr Gpr)
        {
            return Gpr.IsConst ? "0" : GetNameWithSwizzle(Decl.Gprs, Gpr.Index);
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
                return Imm.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private string GetValue(ShaderIrOperImmf Immf)
        {
            return Immf.Value.ToString(CultureInfo.InvariantCulture) + "f";
        }

        private string GetName(ShaderIrOperPred Pred)
        {
            return Pred.IsConst ? "true" : GetNameWithSwizzle(Decl.Preds, Pred.Index);
        }

        private string GetNameWithSwizzle(IReadOnlyDictionary<int, ShaderDeclInfo> Dict, int Index)
        {
            int VecIndex = Index >> 2;

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

        private string GetAndExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "&");

        private string GetAsrExpr(ShaderIrOp Op) => GetBinaryExpr(Op, ">>");

        private string GetBandExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "&&");

        private string GetBnotExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "!");

        private string GetCeilExpr(ShaderIrOp Op) => GetUnaryCall(Op, "ceil");

        private string GetClampExpr(ShaderIrOp Op) => GetTernaryCall(Op, "clamp");

        private string GetCltExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "<");
        private string GetCeqExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "==");
        private string GetCleExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "<=");
        private string GetCgtExpr(ShaderIrOp Op) => GetBinaryExpr(Op, ">");
        private string GetCneExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "!=");
        private string GetCgeExpr(ShaderIrOp Op) => GetBinaryExpr(Op, ">=");

        private string GetExitExpr(ShaderIrOp Op) => "return";

        private string GetFabsExpr(ShaderIrOp Op) => GetUnaryCall(Op, "abs");

        private string GetFaddExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "+");

        private string GetFcosExpr(ShaderIrOp Op) => GetUnaryCall(Op, "cos");

        private string GetFex2Expr(ShaderIrOp Op) => GetUnaryCall(Op, "exp2");

        private string GetFfmaExpr(ShaderIrOp Op) => GetTernaryExpr(Op, "*", "+");

        private string GetFlg2Expr(ShaderIrOp Op) => GetUnaryCall(Op, "log2");

        private string GetFloorExpr(ShaderIrOp Op) => GetUnaryCall(Op, "floor");

        private string GetFmulExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "*");

        private string GetFnegExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "-");

        private string GetFrcpExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "1f / ");

        private string GetFrsqExpr(ShaderIrOp Op) => GetUnaryCall(Op, "inversesqrt");

        private string GetFsinExpr(ShaderIrOp Op) => GetUnaryCall(Op, "sin");

        private string GetFtosExpr(ShaderIrOp Op)
        {
            return "int(" + GetOperExpr(Op, Op.OperandA) + ")";
        }

        private string GetFtouExpr(ShaderIrOp Op)
        {
            return "int(uint(" + GetOperExpr(Op, Op.OperandA) + "))";
        }

        private string GetIpaExpr(ShaderIrOp Op) => GetSrcExpr(Op.OperandA);

        private string GetKilExpr(ShaderIrOp Op) => "discard";

        private string GetLsrExpr(ShaderIrOp Op)
        {
            return "int(uint(" + GetOperExpr(Op, Op.OperandA) + ") >> " +
                                 GetOperExpr(Op, Op.OperandB) + ")";
        }

        private string GetNotExpr(ShaderIrOp Op) => GetUnaryExpr(Op, "~");

        private string GetOrExpr(ShaderIrOp Op) => GetBinaryExpr(Op, "|");

        private string GetStofExpr(ShaderIrOp Op)
        {
            return "float(" + GetOperExpr(Op, Op.OperandA) + ")";
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

                            return Value.ToString(CultureInfo.InvariantCulture) + "f";
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
                    return Abuf.Offs == GlslDecl.VertexIdAttr
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
    }
}