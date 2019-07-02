using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;
using System.Linq;

using static Ryujinx.Graphics.Shader.StructuredIr.AstHelper;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class StructuredProgramContext
    {
        private HashSet<BasicBlock> _loopTails;

        private Stack<(AstBlock Block, int EndIndex)> _blockStack;

        private Dictionary<Operand, AstOperand> _localsMap;

        private Dictionary<int, AstAssignment> _gotoTempAsgs;

        private List<GotoStatement> _gotos;

        private AstBlock _currBlock;

        private int _currEndIndex;

        public StructuredProgramInfo Info { get; }

        public StructuredProgramContext(int blocksCount)
        {
            _loopTails = new HashSet<BasicBlock>();

            _blockStack = new Stack<(AstBlock, int)>();

            _localsMap = new Dictionary<Operand, AstOperand>();

            _gotoTempAsgs = new Dictionary<int, AstAssignment>();

            _gotos = new List<GotoStatement>();

            _currBlock = new AstBlock(AstBlockType.Main);

            _currEndIndex = blocksCount;

            Info = new StructuredProgramInfo(_currBlock);
        }

        public void EnterBlock(BasicBlock block)
        {
            while (_currEndIndex == block.Index)
            {
                (_currBlock, _currEndIndex) = _blockStack.Pop();
            }

            if (_gotoTempAsgs.TryGetValue(block.Index, out AstAssignment gotoTempAsg))
            {
                AddGotoTempReset(block, gotoTempAsg);
            }

            LookForDoWhileStatements(block);
        }

        public void LeaveBlock(BasicBlock block, Operation branchOp)
        {
            LookForIfStatements(block, branchOp);
        }

        private void LookForDoWhileStatements(BasicBlock block)
        {
            // Check if we have any predecessor whose index is greater than the
            // current block, this indicates a loop.
            bool done = false;

            foreach (BasicBlock predecessor in block.Predecessors.OrderByDescending(x => x.Index))
            {
                if (predecessor.Index < block.Index)
                {
                    break;
                }

                if (predecessor.Index < _currEndIndex && !done)
                {
                    Operation branchOp = (Operation)predecessor.GetLastOp();

                    NewBlock(AstBlockType.DoWhile, branchOp, predecessor.Index + 1);

                    _loopTails.Add(predecessor);

                    done = true;
                }
                else
                {
                    AddGotoTempReset(block, GetGotoTempAsg(block.Index));

                    break;
                }
            }
        }

        private void LookForIfStatements(BasicBlock block, Operation branchOp)
        {
            if (block.Branch == null)
            {
                return;
            }

            bool isLoop = block.Branch.Index <= block.Index;

            if (block.Branch.Index <= _currEndIndex && !isLoop)
            {
                NewBlock(AstBlockType.If, branchOp, block.Branch.Index);
            }
            else if (!_loopTails.Contains(block))
            {
                AstAssignment gotoTempAsg = GetGotoTempAsg(block.Branch.Index);

                IAstNode cond = GetBranchCond(AstBlockType.DoWhile, branchOp);

                AddNode(Assign(gotoTempAsg.Destination, cond));

                AstOperation branch = new AstOperation(branchOp.Inst);

                AddNode(branch);

                GotoStatement gotoStmt = new GotoStatement(branch, gotoTempAsg, isLoop);

                _gotos.Add(gotoStmt);
            }
        }

        private AstAssignment GetGotoTempAsg(int index)
        {
            if (_gotoTempAsgs.TryGetValue(index, out AstAssignment gotoTempAsg))
            {
                return gotoTempAsg;
            }

            AstOperand gotoTemp = NewTemp(VariableType.Bool);

            gotoTempAsg = Assign(gotoTemp, Const(IrConsts.False));

            _gotoTempAsgs.Add(index, gotoTempAsg);

            return gotoTempAsg;
        }

        private void AddGotoTempReset(BasicBlock block, AstAssignment gotoTempAsg)
        {
            AddNode(gotoTempAsg);

            // For block 0, we don't need to add the extra "reset" at the beginning,
            // because it is already the first node to be executed on the shader,
            // so it is reset to false by the "local" assignment anyway.
            if (block.Index != 0)
            {
                Info.MainBlock.AddFirst(Assign(gotoTempAsg.Destination, Const(IrConsts.False)));
            }
        }

        private void NewBlock(AstBlockType type, Operation branchOp, int endIndex)
        {
            NewBlock(type, GetBranchCond(type, branchOp), endIndex);
        }

        private void NewBlock(AstBlockType type, IAstNode cond, int endIndex)
        {
            AstBlock childBlock = new AstBlock(type, cond);

            AddNode(childBlock);

            _blockStack.Push((_currBlock, _currEndIndex));

            _currBlock    = childBlock;
            _currEndIndex = endIndex;
        }

        private IAstNode GetBranchCond(AstBlockType type, Operation branchOp)
        {
            IAstNode cond;

            if (branchOp.Inst == Instruction.Branch)
            {
                cond = Const(type == AstBlockType.If ? IrConsts.False : IrConsts.True);
            }
            else
            {
                cond = GetOperandUse(branchOp.GetSource(0));

                Instruction invInst = type == AstBlockType.If
                    ? Instruction.BranchIfTrue
                    : Instruction.BranchIfFalse;

                if (branchOp.Inst == invInst)
                {
                    cond = new AstOperation(Instruction.LogicalNot, cond);
                }
            }

            return cond;
        }

        public void AddNode(IAstNode node)
        {
            _currBlock.Add(node);
        }

        public GotoStatement[] GetGotos()
        {
            return _gotos.ToArray();
        }

        private AstOperand NewTemp(VariableType type)
        {
            AstOperand newTemp = Local(type);

            Info.Locals.Add(newTemp);

            return newTemp;
        }

        public AstOperand GetOperandDef(Operand operand)
        {
            if (TryGetUserAttributeIndex(operand, out int attrIndex))
            {
                Info.OAttributes.Add(attrIndex);
            }

            return GetOperand(operand);
        }

        public AstOperand GetOperandUse(Operand operand)
        {
            if (TryGetUserAttributeIndex(operand, out int attrIndex))
            {
                Info.IAttributes.Add(attrIndex);
            }
            else if (operand.Type == OperandType.ConstantBuffer)
            {
                Info.CBuffers.Add(operand.GetCbufSlot());
            }

            return GetOperand(operand);
        }

        private AstOperand GetOperand(Operand operand)
        {
            if (operand == null)
            {
                return null;
            }

            if (operand.Type != OperandType.LocalVariable)
            {
                return new AstOperand(operand);
            }

            if (!_localsMap.TryGetValue(operand, out AstOperand astOperand))
            {
                astOperand = new AstOperand(operand);

                _localsMap.Add(operand, astOperand);

                Info.Locals.Add(astOperand);
            }

            return astOperand;
        }

        private static bool TryGetUserAttributeIndex(Operand operand, out int attrIndex)
        {
            if (operand.Type == OperandType.Attribute)
            {
                if (operand.Value >= AttributeConsts.UserAttributeBase &&
                    operand.Value <  AttributeConsts.UserAttributeEnd)
                {
                    attrIndex = (operand.Value - AttributeConsts.UserAttributeBase) >> 4;

                    return true;
                }
                else if (operand.Value >= AttributeConsts.FragmentOutputColorBase &&
                         operand.Value <  AttributeConsts.FragmentOutputColorEnd)
                {
                    attrIndex = (operand.Value - AttributeConsts.FragmentOutputColorBase) >> 4;

                    return true;
                }
            }

            attrIndex = 0;

            return false;
        }
    }
}