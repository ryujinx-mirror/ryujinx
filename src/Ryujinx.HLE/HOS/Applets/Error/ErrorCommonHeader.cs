using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.Error
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ErrorCommonHeader
    {
        public ErrorType Type;
        public byte      JumpFlag;
        public byte      ReservedFlag1;
        public byte      ReservedFlag2;
        public byte      ReservedFlag3;
        public byte      ContextFlag;
        public byte      MessageFlag;
        public byte      ContextFlag2;
    }
}