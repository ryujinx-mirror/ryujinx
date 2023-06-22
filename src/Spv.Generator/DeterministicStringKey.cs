using System;
using System.Diagnostics.CodeAnalysis;

namespace Spv.Generator
{
    internal class DeterministicStringKey : IEquatable<DeterministicStringKey>
    {
        private readonly string _value;

        public DeterministicStringKey(string value)
        {
            _value = value;
        }

        public override int GetHashCode()
        {
            return DeterministicHashCode.GetHashCode(_value);
        }

        public bool Equals(DeterministicStringKey other)
        {
            return _value == other._value;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is DeterministicStringKey && Equals((DeterministicStringKey)obj);
        }
    }
}
