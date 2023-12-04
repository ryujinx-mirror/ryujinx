using Ryujinx.HLE.Loaders.Executables;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    class NroInfo
    {
        public NroExecutable Executable { get; private set; }

        public byte[] Hash { get; private set; }
        public ulong NroAddress { get; private set; }
        public ulong NroSize { get; private set; }
        public ulong BssAddress { get; private set; }
        public ulong BssSize { get; private set; }
        public ulong TotalSize { get; private set; }
        public ulong NroMappedAddress { get; set; }

        public NroInfo(
            NroExecutable executable,
            byte[] hash,
            ulong nroAddress,
            ulong nroSize,
            ulong bssAddress,
            ulong bssSize,
            ulong totalSize)
        {
            Executable = executable;
            Hash = hash;
            NroAddress = nroAddress;
            NroSize = nroSize;
            BssAddress = bssAddress;
            BssSize = bssSize;
            TotalSize = totalSize;
        }
    }
}
