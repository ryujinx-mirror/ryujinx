using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContextFactory : IProcessContextFactory
    {
        public IProcessContext Create(MemoryBlock backingMemory, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler)
        {
            return new ArmProcessContext(new MemoryManager(backingMemory, addressSpaceSize, invalidAccessHandler));
        }
    }
}
