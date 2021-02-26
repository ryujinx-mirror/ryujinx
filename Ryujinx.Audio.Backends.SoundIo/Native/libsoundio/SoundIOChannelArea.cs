using System;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public struct SoundIOChannelArea
    {
        internal SoundIOChannelArea(Pointer<SoundIoChannelArea> handle)
        {
            this.handle = handle;
        }

        Pointer<SoundIoChannelArea> handle;

        public IntPtr Pointer
        {
            get { return Marshal.ReadIntPtr(handle, ptr_offset); }
            set { Marshal.WriteIntPtr(handle, ptr_offset, value); }
        }

        static readonly int ptr_offset = (int)Marshal.OffsetOf<SoundIoChannelArea>("ptr");

        public int Step
        {
            get { return Marshal.ReadInt32(handle, step_offset); }
        }

        static readonly int step_offset = (int)Marshal.OffsetOf<SoundIoChannelArea>("step");
    }
}