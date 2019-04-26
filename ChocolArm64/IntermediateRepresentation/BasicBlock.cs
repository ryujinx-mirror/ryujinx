using ChocolArm64.State;
using System;
using System.Collections.Generic;

using static ChocolArm64.State.RegisterConsts;

namespace ChocolArm64.IntermediateRepresentation
{
    class BasicBlock
    {
        public int Index { get; set; }

        public RegisterMask RegInputs  { get; private set; }
        public RegisterMask RegOutputs { get; private set; }

        public bool HasStateLoad { get; private set; }

        private List<Operation> _operations;

        public int Count => _operations.Count;

        private BasicBlock _next;
        private BasicBlock _branch;

        public BasicBlock Next
        {
            get => _next;
            set => _next = AddSuccessor(_next, value);
        }

        public BasicBlock Branch
        {
            get => _branch;
            set => _branch = AddSuccessor(_branch, value);
        }

        public List<BasicBlock> Predecessors { get; }

        public BasicBlock(int index = 0)
        {
            Index = index;

            _operations = new List<Operation>();

            Predecessors = new List<BasicBlock>();
        }

        private BasicBlock AddSuccessor(BasicBlock oldBlock, BasicBlock newBlock)
        {
            oldBlock?.Predecessors.Remove(this);
            newBlock?.Predecessors.Add(this);

            return newBlock;
        }

        public void Add(Operation operation)
        {
            if (operation.Type == OperationType.LoadLocal ||
                operation.Type == OperationType.StoreLocal)
            {
                int index = operation.GetArg<int>(0);

                if (IsRegIndex(index))
                {
                    long intMask = 0;
                    long vecMask = 0;

                    switch (operation.GetArg<RegisterType>(1))
                    {
                        case RegisterType.Flag:   intMask = (1L << RegsCount) << index; break;
                        case RegisterType.Int:    intMask =  1L               << index; break;
                        case RegisterType.Vector: vecMask =  1L               << index; break;
                    }

                    RegisterMask mask = new RegisterMask(intMask, vecMask);

                    if (operation.Type == OperationType.LoadLocal)
                    {
                        RegInputs |= mask & ~RegOutputs;
                    }
                    else
                    {
                        RegOutputs |= mask;
                    }
                }
            }
            else if (operation.Type == OperationType.LoadContext)
            {
                HasStateLoad = true;
            }

            operation.Parent = this;

            _operations.Add(operation);
        }

        public static bool IsRegIndex(int index)
        {
            return (uint)index < RegsCount;
        }

        public Operation GetOperation(int index)
        {
            if ((uint)index >= _operations.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _operations[index];
        }

        public Operation GetLastOp()
        {
            if (Count == 0)
            {
                return null;
            }

            return _operations[Count - 1];
        }
    }
}