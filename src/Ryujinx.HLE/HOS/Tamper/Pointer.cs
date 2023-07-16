using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Tamper.Operations;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Tamper
{
    class Pointer : IOperand
    {
        private readonly IOperand _position;
        private readonly ITamperedProcess _process;

        public Pointer(IOperand position, ITamperedProcess process)
        {
            _position = position;
            _process = process;
        }

        public T Get<T>() where T : unmanaged
        {
            return _process.ReadMemory<T>(_position.Get<ulong>());
        }

        public void Set<T>(T value) where T : unmanaged
        {
            ulong position = _position.Get<ulong>();

            Logger.Debug?.Print(LogClass.TamperMachine, $"0x{position:X16}@{Unsafe.SizeOf<T>()}: {value:X}");

            _process.WriteMemory(position, value);
        }
    }
}
