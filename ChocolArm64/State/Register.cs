using System;
using System.Reflection;

namespace ChocolArm64.State
{
    struct Register : IEquatable<Register>
    {
        public int Index;

        public RegisterType Type;

        public Register(int index, RegisterType type)
        {
            Index = index;
            Type  = type;
        }

        public override int GetHashCode()
        {
            return (ushort)Index | ((ushort)Type << 16);
        }

        public override bool Equals(object obj)
        {
            return obj is Register reg && Equals(reg);
        }

        public bool Equals(Register other)
        {
            return Index == other.Index && Type == other.Type;
        }

        public FieldInfo GetField()
        {
            switch (Type)
            {
                case RegisterType.Flag:   return GetFieldFlag();
                case RegisterType.Int:    return GetFieldInt();
                case RegisterType.Vector: return GetFieldVector();
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldFlag()
        {
            switch ((PState)Index)
            {
                case PState.TBit: return GetField(nameof(CpuThreadState.Thumb));
                case PState.EBit: return GetField(nameof(CpuThreadState.BigEndian));

                case PState.VBit: return GetField(nameof(CpuThreadState.Overflow));
                case PState.CBit: return GetField(nameof(CpuThreadState.Carry));
                case PState.ZBit: return GetField(nameof(CpuThreadState.Zero));
                case PState.NBit: return GetField(nameof(CpuThreadState.Negative));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldInt()
        {
            switch (Index)
            {
                case 0:  return GetField(nameof(CpuThreadState.X0));
                case 1:  return GetField(nameof(CpuThreadState.X1));
                case 2:  return GetField(nameof(CpuThreadState.X2));
                case 3:  return GetField(nameof(CpuThreadState.X3));
                case 4:  return GetField(nameof(CpuThreadState.X4));
                case 5:  return GetField(nameof(CpuThreadState.X5));
                case 6:  return GetField(nameof(CpuThreadState.X6));
                case 7:  return GetField(nameof(CpuThreadState.X7));
                case 8:  return GetField(nameof(CpuThreadState.X8));
                case 9:  return GetField(nameof(CpuThreadState.X9));
                case 10: return GetField(nameof(CpuThreadState.X10));
                case 11: return GetField(nameof(CpuThreadState.X11));
                case 12: return GetField(nameof(CpuThreadState.X12));
                case 13: return GetField(nameof(CpuThreadState.X13));
                case 14: return GetField(nameof(CpuThreadState.X14));
                case 15: return GetField(nameof(CpuThreadState.X15));
                case 16: return GetField(nameof(CpuThreadState.X16));
                case 17: return GetField(nameof(CpuThreadState.X17));
                case 18: return GetField(nameof(CpuThreadState.X18));
                case 19: return GetField(nameof(CpuThreadState.X19));
                case 20: return GetField(nameof(CpuThreadState.X20));
                case 21: return GetField(nameof(CpuThreadState.X21));
                case 22: return GetField(nameof(CpuThreadState.X22));
                case 23: return GetField(nameof(CpuThreadState.X23));
                case 24: return GetField(nameof(CpuThreadState.X24));
                case 25: return GetField(nameof(CpuThreadState.X25));
                case 26: return GetField(nameof(CpuThreadState.X26));
                case 27: return GetField(nameof(CpuThreadState.X27));
                case 28: return GetField(nameof(CpuThreadState.X28));
                case 29: return GetField(nameof(CpuThreadState.X29));
                case 30: return GetField(nameof(CpuThreadState.X30));
                case 31: return GetField(nameof(CpuThreadState.X31));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldVector()
        {
            switch (Index)
            {
                case 0:  return GetField(nameof(CpuThreadState.V0));
                case 1:  return GetField(nameof(CpuThreadState.V1));
                case 2:  return GetField(nameof(CpuThreadState.V2));
                case 3:  return GetField(nameof(CpuThreadState.V3));
                case 4:  return GetField(nameof(CpuThreadState.V4));
                case 5:  return GetField(nameof(CpuThreadState.V5));
                case 6:  return GetField(nameof(CpuThreadState.V6));
                case 7:  return GetField(nameof(CpuThreadState.V7));
                case 8:  return GetField(nameof(CpuThreadState.V8));
                case 9:  return GetField(nameof(CpuThreadState.V9));
                case 10: return GetField(nameof(CpuThreadState.V10));
                case 11: return GetField(nameof(CpuThreadState.V11));
                case 12: return GetField(nameof(CpuThreadState.V12));
                case 13: return GetField(nameof(CpuThreadState.V13));
                case 14: return GetField(nameof(CpuThreadState.V14));
                case 15: return GetField(nameof(CpuThreadState.V15));
                case 16: return GetField(nameof(CpuThreadState.V16));
                case 17: return GetField(nameof(CpuThreadState.V17));
                case 18: return GetField(nameof(CpuThreadState.V18));
                case 19: return GetField(nameof(CpuThreadState.V19));
                case 20: return GetField(nameof(CpuThreadState.V20));
                case 21: return GetField(nameof(CpuThreadState.V21));
                case 22: return GetField(nameof(CpuThreadState.V22));
                case 23: return GetField(nameof(CpuThreadState.V23));
                case 24: return GetField(nameof(CpuThreadState.V24));
                case 25: return GetField(nameof(CpuThreadState.V25));
                case 26: return GetField(nameof(CpuThreadState.V26));
                case 27: return GetField(nameof(CpuThreadState.V27));
                case 28: return GetField(nameof(CpuThreadState.V28));
                case 29: return GetField(nameof(CpuThreadState.V29));
                case 30: return GetField(nameof(CpuThreadState.V30));
                case 31: return GetField(nameof(CpuThreadState.V31));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetField(string name)
        {
            return typeof(CpuThreadState).GetField(name);
        }
    }
}