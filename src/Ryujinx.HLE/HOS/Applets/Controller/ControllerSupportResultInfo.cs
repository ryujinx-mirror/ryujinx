using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets
{
#pragma warning disable CS0649 // Field is never assigned to
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ControllerSupportResultInfo
    {
        public sbyte PlayerCount;
        private Array3<byte> _padding;
        public uint SelectedId;
        public uint Result;
    }
#pragma warning restore CS0649
}
