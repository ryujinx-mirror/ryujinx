using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public struct SoundIOChannelLayout
    {
        public static int BuiltInCount
        {
            get { return Natives.soundio_channel_layout_builtin_count(); }
        }

        public static SoundIOChannelLayout GetBuiltIn(int index)
        {
            return new SoundIOChannelLayout(Natives.soundio_channel_layout_get_builtin(index));
        }

        public static SoundIOChannelLayout GetDefault(int channelCount)
        {
            var handle = Natives.soundio_channel_layout_get_default(channelCount);

            return new SoundIOChannelLayout (handle);
        }

        public static SoundIOChannelId ParseChannelId(string name)
        {
            var ptr = Marshal.StringToHGlobalAnsi(name);

            try 
            {
                return (SoundIOChannelId)Natives.soundio_parse_channel_id(ptr, name.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        // instance members

        internal SoundIOChannelLayout(Pointer<SoundIoChannelLayout> handle)
        {
            this.handle = handle;
        }

        readonly Pointer<SoundIoChannelLayout> handle;

        public bool IsNull
        {
            get { return handle.Handle == IntPtr.Zero; }
        }

        internal IntPtr Handle
        {
            get { return handle; }
        }

        public int ChannelCount
        {
            get { return IsNull ? 0 : Marshal.ReadInt32((IntPtr)handle + channel_count_offset); }
        }

        static readonly int channel_count_offset = (int)Marshal.OffsetOf<SoundIoChannelLayout>("channel_count");

        public string Name
        {
            get { return IsNull ? null : Marshal.PtrToStringAnsi(Marshal.ReadIntPtr((IntPtr)handle + name_offset)); }
        }

        static readonly int name_offset = (int)Marshal.OffsetOf<SoundIoChannelLayout>("name");

        public IEnumerable<SoundIOChannelId> Channels
        {
            get
            {
                if (IsNull) yield break;

                for (int i = 0; i < 24; i++)
                {
                    yield return (SoundIOChannelId)Marshal.ReadInt32((IntPtr)handle + channels_offset + sizeof(SoundIoChannelId) * i);
                }
            }
        }

        static readonly int channels_offset = (int)Marshal.OffsetOf<SoundIoChannelLayout>("channels");

        public override bool Equals(object other)
        {
            if (!(other is SoundIOChannelLayout)) return false;

            var s = (SoundIOChannelLayout) other;

            return handle == s.handle || Natives.soundio_channel_layout_equal(handle, s.handle);
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public string DetectBuiltInName()
        {
            if (IsNull) throw new InvalidOperationException();

            return Natives.soundio_channel_layout_detect_builtin(handle) ? Name : null;
        }

        public int FindChannel(SoundIOChannelId channel)
        {
            if (IsNull) throw new InvalidOperationException();

            return Natives.soundio_channel_layout_find_channel(handle, (SoundIoChannelId)channel);
        }
    }
}