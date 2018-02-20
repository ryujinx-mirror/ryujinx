using ChocolArm64.Decoder;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class AILEmitter
    {
        public ALocalAlloc LocalAlloc { get; private set; }

        public ILGenerator Generator { get; private set; }

        private Dictionary<ARegister, int> Locals;

        private AILBlock[] ILBlocks;

        private AILBlock Root;

        private ATranslatedSub Subroutine;

        private string SubName;

        private int LocalsCount;

        public AILEmitter(ABlock[] Graph, ABlock Root, string SubName)
        {
            this.SubName = SubName;

            Locals = new Dictionary<ARegister, int>();

            ILBlocks = new AILBlock[Graph.Length];

            AILBlock GetBlock(int Index)
            {
                if (Index < 0 || Index >= ILBlocks.Length)
                {
                    return null;
                }

                if (ILBlocks[Index] == null)
                {
                    ILBlocks[Index] = new AILBlock();
                }

                return ILBlocks[Index];
            }

            for (int Index = 0; Index < ILBlocks.Length; Index++)
            {
                AILBlock Block = GetBlock(Index);

                Block.Next   = GetBlock(Array.IndexOf(Graph, Graph[Index].Next));
                Block.Branch = GetBlock(Array.IndexOf(Graph, Graph[Index].Branch));
            }

            this.Root = ILBlocks[Array.IndexOf(Graph, Root)];
        }

        public ATranslatedSub GetSubroutine()
        {
            LocalAlloc = new ALocalAlloc(ILBlocks, Root);

            InitSubroutine();
            InitLocals();

            foreach (AILBlock ILBlock in ILBlocks)
            {
                ILBlock.Emit(this);
            }

            return Subroutine;
        }

        public AILBlock GetILBlock(int Index) => ILBlocks[Index];

        private void InitLocals()
        {
            int ParamsStart = ATranslatedSub.FixedArgTypes.Length;

            Locals = new Dictionary<ARegister, int>();

            for (int Index = 0; Index < Subroutine.Params.Count; Index++)
            {
                ARegister Reg = Subroutine.Params[Index];

                Generator.EmitLdarg(Index + ParamsStart);
                Generator.EmitStloc(GetLocalIndex(Reg));
            }
        }

        private void InitSubroutine()
        {
            List<ARegister> Params = new List<ARegister>();

            void SetParams(long Inputs, ARegisterType BaseType)
            {
                for (int Bit = 0; Bit < 64; Bit++)
                {
                    long Mask = 1L << Bit;

                    if ((Inputs & Mask) != 0)
                    {
                        Params.Add(GetRegFromBit(Bit, BaseType));
                    }
                }
            }

            SetParams(LocalAlloc.GetIntInputs(Root), ARegisterType.Int);
            SetParams(LocalAlloc.GetVecInputs(Root), ARegisterType.Vector);

            DynamicMethod Mthd = new DynamicMethod(SubName, typeof(long), GetParamTypes(Params));

            Generator = Mthd.GetILGenerator();

            Subroutine = new ATranslatedSub(Mthd, Params);
        }

        private Type[] GetParamTypes(IList<ARegister> Params)
        {
            Type[] FixedArgs = ATranslatedSub.FixedArgTypes;

            Type[] Output = new Type[Params.Count + FixedArgs.Length];

            FixedArgs.CopyTo(Output, 0);

            int TypeIdx = FixedArgs.Length;

            for (int Index = 0; Index < Params.Count; Index++)
            {
                Output[TypeIdx++] = GetFieldType(Params[Index].Type);
            }

            return Output;
        }

        public int GetLocalIndex(ARegister Reg)
        {
            if (!Locals.TryGetValue(Reg, out int Index))
            {
                Generator.DeclareLocal(GetLocalType(Reg));

                Index = LocalsCount++;

                Locals.Add(Reg, Index);
            }

            return Index;
        }

        public Type GetLocalType(ARegister Reg) => GetFieldType(Reg.Type);

        public Type GetFieldType(ARegisterType RegType)
        {
            switch (RegType)
            {
                case ARegisterType.Flag:   return typeof(bool);
                case ARegisterType.Int:    return typeof(ulong);
                case ARegisterType.Vector: return typeof(AVec);
            }

            throw new ArgumentException(nameof(RegType));
        }

        public static ARegister GetRegFromBit(int Bit, ARegisterType BaseType)
        {
            if (Bit < 32)
            {
                return new ARegister(Bit, BaseType);
            }
            else if (BaseType == ARegisterType.Int)
            {
                return new ARegister(Bit & 0x1f, ARegisterType.Flag);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Bit));
            }
        }

        public static bool IsRegIndex(int Index)
        {
            return Index >= 0 && Index < 32;
        }
    }
}