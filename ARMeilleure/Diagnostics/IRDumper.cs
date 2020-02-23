using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARMeilleure.Diagnostics
{
    static class IRDumper
    {
        private const string Indentation = " ";

        public static string GetDump(ControlFlowGraph cfg)
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<Operand, string> localNames = new Dictionary<Operand, string>();

            string indentation = string.Empty;

            void IncreaseIndentation()
            {
                indentation += Indentation;
            }

            void DecreaseIndentation()
            {
                indentation = indentation.Substring(0, indentation.Length - Indentation.Length);
            }

            void AppendLine(string text)
            {
                sb.AppendLine(indentation + text);
            }

            IncreaseIndentation();

            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                string blockName = GetBlockName(block);

                if (block.Next != null)
                {
                    blockName += $" (next {GetBlockName(block.Next)})";
                }

                if (block.Branch != null)
                {
                    blockName += $" (branch {GetBlockName(block.Branch)})";
                }

                blockName += ":";

                AppendLine(blockName);

                IncreaseIndentation();

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    string[] sources = new string[node.SourcesCount];

                    string instName = string.Empty;

                    if (node is PhiNode phi)
                    {
                        for (int index = 0; index < sources.Length; index++)
                        {
                            string phiBlockName = GetBlockName(phi.GetBlock(index));

                            string operName = GetOperandName(phi.GetSource(index), localNames);

                            sources[index] = $"({phiBlockName}: {operName})";
                        }

                        instName = "Phi";
                    }
                    else if (node is Operation operation)
                    {
                        for (int index = 0; index < sources.Length; index++)
                        {
                            sources[index] = GetOperandName(operation.GetSource(index), localNames);
                        }

                        instName = operation.Instruction.ToString();
                    }

                    string allSources = string.Join(", ", sources);

                    string line = instName + " " + allSources;

                    if (node.Destination != null)
                    {
                        line = GetOperandName(node.Destination, localNames) + " = " + line;
                    }

                    AppendLine(line);
                }

                DecreaseIndentation();
            }

            return sb.ToString();
        }

        private static string GetBlockName(BasicBlock block)
        {
            return $"block{block.Index}";
        }

        private static string GetOperandName(Operand operand, Dictionary<Operand, string> localNames)
        {
            if (operand == null)
            {
                return "<NULL>";
            }

            string name = string.Empty;

            if (operand.Kind == OperandKind.LocalVariable)
            {
                if (!localNames.TryGetValue(operand, out string localName))
                {
                    localName = "%" + localNames.Count;

                    localNames.Add(operand, localName);
                }

                name = localName;
            }
            else if (operand.Kind == OperandKind.Register)
            {
                Register reg = operand.GetRegister();

                switch (reg.Type)
                {
                    case RegisterType.Flag:    name = "b" + reg.Index; break;
                    case RegisterType.FpFlag:  name = "f" + reg.Index; break;
                    case RegisterType.Integer: name = "r" + reg.Index; break;
                    case RegisterType.Vector:  name = "v" + reg.Index; break;
                }
            }
            else if (operand.Kind == OperandKind.Constant)
            {
                name = "0x" + operand.Value.ToString("X");
            }
            else
            {
                name = operand.Kind.ToString().ToLower();
            }

            return GetTypeName(operand.Type) + " " + name;
        }

        private static string GetTypeName(OperandType type)
        {
            switch (type)
            {
                case OperandType.FP32: return "f32";
                case OperandType.FP64: return "f64";
                case OperandType.I32:  return "i32";
                case OperandType.I64:  return "i64";
                case OperandType.None: return "none";
                case OperandType.V128: return "v128";
            }

            throw new ArgumentException($"Invalid operand type \"{type}\".");
        }
    }
}