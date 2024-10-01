using Ryujinx.Cpu.LightningJit.CodeGen;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    readonly struct ScopedRegister : IDisposable
    {
        private readonly RegisterAllocator _registerAllocator;
        private readonly Operand _operand;
        private readonly bool _isAllocated;

        public readonly Operand Operand => _operand;
        public readonly bool IsAllocated => _isAllocated;

        public ScopedRegister(RegisterAllocator registerAllocator, Operand operand, bool isAllocated = true)
        {
            _registerAllocator = registerAllocator;
            _operand = operand;
            _isAllocated = isAllocated;
        }

        public readonly void Dispose()
        {
            if (!_isAllocated)
            {
                return;
            }

            if (_operand.Type.IsInteger())
            {
                _registerAllocator.FreeTempGprRegister(_operand.AsInt32());
            }
            else
            {
                _registerAllocator.FreeTempFpSimdRegister(_operand.AsInt32());
            }
        }
    }
}
